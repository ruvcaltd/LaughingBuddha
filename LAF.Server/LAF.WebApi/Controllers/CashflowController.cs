using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.WebApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class CashflowController : ControllerBase
    {
        private readonly ICashManagementService _cashManagementService;
        private readonly IFundRepository _fundRepository;
        private readonly ISignalRBroker _signalRBroker;
        private readonly ILogger<CashflowController> _logger;

        public CashflowController(
            ICashManagementService cashManagementService,
            IFundRepository fundRepository,
            ISignalRBroker signalRBroker,
            ILogger<CashflowController> logger)
        {
            _cashManagementService = cashManagementService;
            _fundRepository = fundRepository;
            this._signalRBroker = signalRBroker;
            _logger = logger;
        }

        [HttpGet("daily/{asOfDate:datetime}")]
        public async Task<ActionResult<IEnumerable<FundAccountCashflowsDto>>> GetDailyCashflows(DateTime asOfDate)
        {
            try
            {
                var activeFunds = await _fundRepository.GetActiveFundsAsync();
                var result = new List<FundAccountCashflowsDto>();

                foreach (var fund in activeFunds)
                {
                    var fundCashflows = new FundAccountCashflowsDto
                    {
                        FundId = fund.Id,
                        FundCode = fund.FundCode,
                        FundName = fund.FundName
                    };

                    // Get all accounts for the fund
                    foreach (var account in fund.CashAccounts)
                    {
                        var accountCashflows = new AccountCashflowsDto
                        {
                            CashAccountId = account.Id,
                            AccountName = account.AccountName,
                            CurrencyCode = account.CurrencyCode
                        };

                        // Get cashflows for this account on the specified date
                        var cashflows = await _cashManagementService.GetCashflowsByAccountAsync(
                            account.Id,
                            asOfDate,
                            asOfDate);

                        accountCashflows.Cashflows = cashflows.ToList();

                        fundCashflows.Accounts.Add(accountCashflows);
                    }

                    // Only add funds that have accounts with cashflows
                    if (fundCashflows.Accounts.Any())
                    {
                        result.Add(fundCashflows);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily cashflows for {AsOfDate}", asOfDate);
                return StatusCode(500, new { error = "An error occurred while retrieving cashflows" });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<CashflowDto>> CreateCashflow(CreateCashflowDto createDto)
        {
            try
            {
                // Validate fund exists
                var fund = await _fundRepository.GetByIdAsync(createDto.FundId);
                if (fund == null)
                {
                    return NotFound($"Fund with ID {createDto.FundId} not found");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                // Validate cash account belongs to fund
                var cashAccount = fund.CashAccounts.FirstOrDefault(ca => ca.Id == createDto.CashAccountId);
                if (cashAccount == null)
                {
                    return BadRequest($"Cash account {createDto.CashAccountId} does not belong to fund {createDto.FundId}");
                }

                createDto.CreatedByUserId = userId;

                // Create the cashflow
                var createdCashflow = await _cashManagementService.CreateCashflowAsync(createDto, true);

                await _signalRBroker.SendToAll(SignalRBrokerMessages.CashflowCreated, new SignalRBrokerMessages<CashflowDto>(userId, createdCashflow));

                return CreatedAtAction(
                    nameof(GetDailyCashflows),
                    new { asOfDate = createDto.CashflowDate },
                    createdCashflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cashflow");
                return StatusCode(500, new { error = "An error occurred while creating the cashflow" });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCashflow(int id)
        {
            try
            {
                var result = await _cashManagementService.DeleteCashflowAsync(id);

                if (!string.IsNullOrEmpty(result))
                {
                    return BadRequest(new { error = result });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    await _signalRBroker.SendToAll(SignalRBrokerMessages.CashflowDeleted, new SignalRBrokerMessages<CashflowDto>(userId, new CashflowDto { Id = id }));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cashflow {CashflowId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the cashflow" });
            }
        }
    }
}