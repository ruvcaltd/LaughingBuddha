using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Services;
using System.Security.Claims;

namespace LAF.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RepoTradesController : ControllerBase
    {
        private readonly IRepoTradeService _repoTradeService;
        private readonly ITargetCircleService _targetCircleService;
        private readonly ILogger<RepoTradesController> _logger;

        public RepoTradesController(
            IRepoTradeService repoTradeService,
            ITargetCircleService targetCircleService,
            ILogger<RepoTradesController> logger)
        {
            _repoTradeService = repoTradeService;
            _targetCircleService = targetCircleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> GetAllTrades()
        {
            try
            {
                var trades = await _repoTradeService.GetAllAsync();
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all repo trades");
                return StatusCode(500, new { error = "An error occurred while retrieving trades" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RepoTradeDto>> GetTradeById(int id)
        {
            try
            {
                var trade = await _repoTradeService.GetByIdAsync(id);
                if (trade == null)
                {
                    return NotFound(new { error = $"Trade with ID {id} not found" });
                }
                return Ok(trade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the trade" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> SearchTrades([FromQuery] RepoTradeQueryDto query)
        {
            try

            {
                var trades = await _repoTradeService.FindAsync(query);
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching trades with query: {Query}", query);
                return StatusCode(500, new { error = "An error occurred while searching trades" });
            }
        }

        [HttpGet("fund/{fundId}")]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> GetTradesByFund(int fundId)
        {
            try
            {
                var trades = await _repoTradeService.GetTradesByFundAsync(fundId);
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trades for fund {FundId}", fundId);
                return StatusCode(500, new { error = "An error occurred while retrieving trades for the fund" });
            }
        }

        [HttpGet("active/{asOfDate:datetime}")]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> GetActiveTrades(DateTime asOfDate)
        {
            try
            {
                var trades = await _repoTradeService.GetActiveTradesAsync(asOfDate);
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active trades as of {AsOfDate}", asOfDate);
                return StatusCode(500, new { error = "An error occurred while retrieving active trades" });
            }
        }

        [HttpGet("settlement/{settlementDate:datetime}")]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> GetTradesBySettlementDate(DateTime settlementDate)
        {
            try
            {
                var trades = await _repoTradeService.GetTradesBySettlementDateAsync(settlementDate);
                return Ok(trades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trades with settlement date {SettlementDate}", settlementDate);
                return StatusCode(500, new { error = "An error occurred while retrieving trades by settlement date" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RepoRateDto), StatusCodes.Status201Created)] // important for nswag code gen
        public async Task<ActionResult<RepoTradeDto>> CreateTrade([FromBody] CreateRepoTradeDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get user ID from claims (assuming it's stored in the JWT token)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                createDto.CreatedByUserId = userId;

                var trade = await _repoTradeService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetTradeById), new { id = trade.Id }, trade);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating repo trade");
                return StatusCode(500, new { error = "An error occurred while creating the trade" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RepoTradeDto>> UpdateTrade(int id, [FromBody] UpdateRepoTradeDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != updateDto.Id)
                {
                    return BadRequest(new { error = "ID mismatch" });
                }

                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                updateDto.ModifiedByUserId = userId;

                var trade = await _repoTradeService.UpdateAsync(updateDto);
                return Ok(trade);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating repo trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the trade" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrade(int id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                await _repoTradeService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting repo trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the trade" });
            }
        }

        [HttpPost("{id}/settle")]
        public async Task<ActionResult<RepoTradeDto>> SettleTrade(int id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var trade = await _repoTradeService.SettleTradeAsync(id, userId);
                return Ok(trade);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while settling trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error settling trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while settling the trade" });
            }
        }

        [HttpPost("{id}/mature")]
        public async Task<ActionResult<RepoTradeDto>> MatureTrade(int id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var trade = await _repoTradeService.MatureTradeAsync(id, userId);
                return Ok(trade);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while maturing trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error maturing trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while maturing the trade" });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<RepoTradeDto>> CancelTrade(int id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var trade = await _repoTradeService.CancelTradeAsync(id, userId);
                return Ok(trade);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while cancelling trade");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling trade with ID {TradeId}", id);
                return StatusCode(500, new { error = "An error occurred while cancelling the trade" });
            }
        }

        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateTrade([FromBody] CreateRepoTradeDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isValid = await _repoTradeService.ValidateTradeAsync(createDto);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating trade");
                return StatusCode(500, new { error = "An error occurred while validating the trade" });
            }
        }

        [HttpPost("validate-target-circle")]
        public async Task<ActionResult<TargetCircleValidationDto>> ValidateTargetCircle(
            [FromQuery] int counterpartyId,
            [FromQuery] DateTime tradeDate,
            [FromQuery] decimal proposedNotional)
        {
            try
            {
                var validation = await _targetCircleService.ValidateTradeAgainstTargetCircleAsync(
                    counterpartyId, tradeDate, proposedNotional);
                return Ok(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating TargetCircle");
                return StatusCode(500, new { error = "An error occurred while validating TargetCircle" });
            }
        }
    }
}