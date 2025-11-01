using LAF.DataAccess.Models;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.Services.Services;
using LAF.WebApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LAF.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PositionsController : ControllerBase
    {
        private readonly IRepoTradeService _repoTradeService;
        private readonly IRepoRateRepository _repoRateRepository;
        private readonly ISecurityService _securityService;
        private readonly ISignalRBroker hub;
        private readonly ILogger<PositionsController> _logger;

        public PositionsController(
            IRepoTradeService repoTradeService,
            IRepoRateRepository repoRateRepository,
            ISecurityService securityService,
            ISignalRBroker _hub,
            ILogger<PositionsController> logger)
        {
            this._repoTradeService = repoTradeService;
            this._repoRateRepository = repoRateRepository;
            this._securityService = securityService;
            this.hub = _hub;
            _logger = logger;
        }

        /// <summary>
        /// Updates position information for a given fund and counterparty combination
        /// </summary>
        /// <param name="positionChange">The position change details</param>
        /// <returns>The updated position information</returns>
        /// <response code="200">Returns the updated position information</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("update")]
        [ProducesResponseType(typeof(PositionChangeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PositionChangeDto>> UpdatePositions([FromBody] PositionChangeDto positionChange)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get user ID from claims                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var rates = await _repoRateRepository.GetRepoRatesByDateAsync(DateTime.Today, false);
                var rate = rates.FirstOrDefault(r => r.CounterpartyId == positionChange.CounterpartyId && r.CollateralTypeId == positionChange.CollateralTypeId);
                if (rate == null)
                {
                    return BadRequest(new { error = "Repo Rates have not been defined for today." });
                }

                var security = await _securityService.GetByIssuerAssetTypeAndMaturity(rate.Counterparty.CounterpartyCode, rate.CollateralType.AssetType, positionChange.SecurityMaturityDate.Date);

                if (security == null)
                {
                    var createDto = new CreateSecurityDto
                    {
                        Description = $"{rate.Counterparty.CounterpartyCode}{rate.CollateralType.AssetType}_{positionChange.SecurityMaturityDate:yyyyMMdd}",
                        AssetType = rate.CollateralType.AssetType,
                        Currency = "USD",
                        CreatedByUserId = userId,
                        MaturityDate = positionChange.SecurityMaturityDate.Date,
                        Issuer = rate.Counterparty.CounterpartyCode,
                        Isin = $"{rate.Counterparty.CounterpartyCode}{rate.CollateralType.AssetType}_{positionChange.SecurityMaturityDate:yyyyMMdd}",
                    };
                    security = await _securityService.CreateAsync(createDto);
                }

                // user _repoTradeService to find all trades for today for the given fund, collateralTypeId and counterpartyId
                var query = new RepoTradeQueryDto
                {
                    FundId = positionChange.FundId,
                    CollateralTypeId = positionChange.CollateralTypeId,
                    CounterpartyId = positionChange.CounterpartyId,
                    StartDateFrom = DateTime.UtcNow.Date,
                    StartDateTo = DateTime.UtcNow.Date
                };

                var trades = await _repoTradeService.FindAsync(query);

                // calculate the new position based on the trades and the new notional amount
                var tradeNotional = positionChange.NewNotionalAmount - trades.Sum(t => t.Notional);

                if (tradeNotional == 0) return Ok(positionChange); // no change needed

                // place a draft trade with the new position
                var newTrade = new CreateRepoTradeDto
                {
                    FundId = positionChange.FundId,
                    CounterpartyId = positionChange.CounterpartyId,
                    CollateralTypeId = positionChange.CollateralTypeId,
                    SecurityId = security.Id,
                    Notional = Math.Abs(tradeNotional),
                    Rate = rate.RepoRate1,
                    StartDate = DateTime.UtcNow.Date,
                    EndDate = DateTime.UtcNow.Date.AddDays(1), // TODO: use business days
                    SettlementDate = DateTime.UtcNow.Date.AddDays(1),
                    Direction = tradeNotional >= 0 ? "Lend" : "Borrow",
                    CreatedByUserId = userId
                };

                try
                {
                    var trade = await _repoTradeService.CreateAsync(newTrade);

                    var allPositions = await GetPositionsForDay(DateTime.Today);
                    var position = allPositions.Value?.FirstOrDefault(p => p.SecurityMaturityDate.Date == positionChange.SecurityMaturityDate.Date
                        && p.CounterpartyId == positionChange.CounterpartyId
                        && p.CollateralTypeId == positionChange.CollateralTypeId);

                    //broadcast position change event here if needed
                    if (position != null)
                        await hub.SendToAll(SignalRBrokerMessages.PositionChanged, new SignalRBrokerMessages<PositionDto>(userId, position));
                    await hub.SendToAll(SignalRBrokerMessages.NewTrade, new SignalRBrokerMessages<RepoTradeDto>(userId, trade));

                    positionChange.Status = "Success";
                }
                catch (InvalidOperationException e)
                {
                    positionChange.Status = "Failed";
                    positionChange.ErrorMessage = e.Message;
                }
                catch (Exception)
                {
                    positionChange.Status = "Failed";
                }

                return Ok(positionChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating position for fund {FundId}, counterparty {CounterpartyId}, collateral type {CollateralTypeId}",
                    positionChange.FundId, positionChange.CounterpartyId, positionChange.CollateralTypeId);

                return StatusCode(500, new { error = "An error occurred while updating the position" });
            }
        }

        /// <summary>
        /// Gets all positions for a specific date by aggregating repo trades
        /// </summary>
        /// <param name="date">The date to get positions for. If not provided, uses today's date</param>
        /// <returns>A list of positions aggregated by collateral type, counterparty and security</returns>
        /// <response code="200">Returns the list of positions</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("day")]
        [ProducesResponseType(typeof(List<PositionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PositionDto>>> GetPositionsForDay([FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                var query = new RepoTradeQueryDto
                {
                    StartDateFrom = targetDate,
                    StartDateTo = targetDate
                };

                var rates = await _repoRateRepository.GetRepoRatesByDateAsync(targetDate, false);

                var trades = await _repoTradeService.FindAsync(query);

                // Group trades by collateral type, counterparty and security
                var positions = trades
                    .GroupBy(t => new { t.CollateralTypeId, t.CounterpartyId, t.SecurityId })
                    .Select(g => new PositionDto
                    {
                        CollateralTypeId = g.Key.CollateralTypeId,
                        CollateralTypeName = g.First().CollateralTypeName,
                        CounterpartyId = g.Key.CounterpartyId,
                        CounterpartyName = g.First().CounterpartyName,
                        SecurityId = g.Key.SecurityId,
                        SecurityName = g.First().SecurityName,
                        SecurityMaturityDate = g.First().Security?.MaturityDate?.Date ?? DateTime.Today.AddDays(1), //TODO: adjust as needed
                        Variance = rates.Where(r => r.CollateralTypeId == g.Key.CollateralTypeId && r.CounterpartyId == g.Key.CounterpartyId).Sum(x => x.TargetCircle) - g.Sum(t => t.Notional),
                        // Group by fund to get fund-specific notionals and statuses
                        FundNotionals = g.GroupBy(t => t.FundId)
                            .ToDictionary(
                                fg => fg.Key,
                                fg => fg.Sum(t => t.Notional)
                            ),
                        // Calculate exposure percentages (you may need to adjust this calculation based on your business logic)
                        ExposurePercentages = g.GroupBy(t => t.FundId)
                            .ToDictionary(
                                fg => fg.Key,
                                fg => fg.Sum(t => t.Notional) / (decimal)100.0 // Placeholder calculation
                            ),
                        // Aggregate statuses per fund
                        Statuses = g.GroupBy(t => t.FundId)
                            .ToDictionary(
                                fg => fg.Key,
                                fg => fg.All(t => t.Status == "Active") ? "Active" : "Partial"
                            )
                    })
                    .ToList();

                return Ok(positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions for date {Date}", date);
                return StatusCode(500, new { error = "An error occurred while getting positions" });
            }
        }

        /// <summary>
        /// Broadcasts a position cell editing status to all clients except the sender
        /// </summary>
        /// <param name="lockInfo">The lock information containing CounterpartyId, CollateralTypeId and FundId</param>
        /// <returns>OK if the broadcast was successful</returns>
        /// <response code="200">Returns if the broadcast was successful</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpPost("broadcast-lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BroadcastLock([FromBody] PositionLockDto lockInfo)
        {
            try
            {
                var username = User.FindFirst("UserName")?.Value ?? "Unknown";
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }
                lockInfo.UserDisplay = username;
                await hub.SendToAll(SignalRBrokerMessages.PositionCellEditing, new SignalRBrokerMessages<PositionLockDto>(userId, lockInfo));
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting position lock for CounterpartyId {CounterpartyId}, CollateralTypeId {CollateralTypeId}, FundId {FundId}",
                    lockInfo.CounterpartyId, lockInfo.CollateralTypeId, lockInfo.FundId);
                return StatusCode(500, new { error = "An error occurred while broadcasting position lock" });
            }
        }
    }
}