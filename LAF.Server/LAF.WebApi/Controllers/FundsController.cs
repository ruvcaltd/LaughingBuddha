using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.Services.Mappers;

namespace LAF.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FundsController : ControllerBase
    {
        private readonly ICashManagementService _cashManagementService;
        private readonly IFundRepository _fundRepository;
        private readonly ILogger<FundsController> _logger;

        public FundsController(
            ICashManagementService cashManagementService,
            IFundRepository fundRepository,
            ILogger<FundsController> logger)
        {
            _cashManagementService = cashManagementService;
            _fundRepository = fundRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FundDto>>> GetAllFunds()
        {
            try
            {
                var funds = await _fundRepository.GetAllAsync();
                var fundDtos = FundMapper.ToDtoList(funds);
                return Ok(fundDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all funds");
                return StatusCode(500, new { error = "An error occurred while retrieving funds" });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FundDto>>> GetActiveFunds()
        {
            try
            {
                var funds = await _fundRepository.GetActiveFundsAsync();
                var fundDtos = FundMapper.ToDtoList(funds);
                return Ok(fundDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active funds");
                return StatusCode(500, new { error = "An error occurred while retrieving active funds" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FundDto>> GetFundById(int id)
        {
            try
            {
                var fund = await _fundRepository.GetByIdAsync(id);
                if (fund == null)
                {
                    return NotFound(new { error = $"Fund with ID {id} not found" });
                }

                var fundDto = FundMapper.ToDto(fund);
                return Ok(fundDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fund with ID {FundId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the fund" });
            }
        }

        [HttpGet("code/{fundCode}")]
        public async Task<ActionResult<FundDto>> GetFundByCode(string fundCode)
        {
            try
            {
                var fund = await _fundRepository.GetByFundCodeAsync(fundCode);
                if (fund == null)
                {
                    return NotFound(new { error = $"Fund with code {fundCode} not found" });
                }

                var fundDto = FundMapper.ToDto(fund);
                return Ok(fundDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fund with code {FundCode}", fundCode);
                return StatusCode(500, new { error = "An error occurred while retrieving the fund" });
            }
        }

        [HttpGet("{id}/balance/{asOfDate:datetime}")]
        public async Task<ActionResult<FundBalanceDto>> GetFundBalance(int id, DateTime asOfDate)
        {
            try
            {
                var fundBalance = await _cashManagementService.GetFundBalanceAsync(id, asOfDate);
                return Ok(fundBalance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for fund {FundId} as of {AsOfDate}", id, asOfDate);
                return StatusCode(500, new { error = "An error occurred while retrieving fund balance" });
            }
        }

        [HttpGet("balances/{asOfDate:datetime}")]
        public async Task<ActionResult<IEnumerable<FundBalanceDto>>> GetAllFundBalances(DateTime asOfDate)
        {
            try
            {
                var fundBalances = await _cashManagementService.GetAllFundBalancesAsync(asOfDate);
                return Ok(fundBalances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all fund balances as of {AsOfDate}", asOfDate);
                return StatusCode(500, new { error = "An error occurred while retrieving fund balances" });
            }
        }

        [HttpGet("{id}/flatness/{checkDate:datetime}")]
        public async Task<ActionResult<FundFlatnessCheckDto>> CheckFundFlatness(int id, DateTime checkDate)
        {
            try
            {
                var flatnessCheck = await _cashManagementService.CheckFundFlatnessAsync(id, checkDate);
                return Ok(flatnessCheck);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking flatness for fund {FundId} as of {CheckDate}", id, checkDate);
                return StatusCode(500, new { error = "An error occurred while checking fund flatness" });
            }
        }

        [HttpGet("flatness/{checkDate:datetime}")]
        public async Task<ActionResult<IEnumerable<FundFlatnessCheckDto>>> CheckAllFundsFlatness(DateTime checkDate)
        {
            try
            {
                var flatnessChecks = await _cashManagementService.CheckAllFundsFlatnessAsync(checkDate);
                return Ok(flatnessChecks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking all funds flatness as of {CheckDate}", checkDate);
                return StatusCode(500, new { error = "An error occurred while checking all funds flatness" });
            }
        }

        [HttpGet("{id}/cashflows")]
        public async Task<ActionResult<IEnumerable<CashflowDto>>> GetFundCashflows(
            int id, 
            [FromQuery] DateTime? fromDate, 
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var cashflows = await _cashManagementService.GetCashflowsByFundAsync(id, fromDate, toDate);
                return Ok(cashflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cashflows for fund {FundId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving fund cashflows" });
            }
        }

        [HttpGet("{id}/cashflow-summary")]
        public async Task<ActionResult<FundCashflowSummaryDto>> GetFundCashflowSummary(
            int id, 
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                var summary = await _cashManagementService.GetFundCashflowSummaryAsync(id, fromDate, toDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cashflow summary for fund {FundId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving fund cashflow summary" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,FundManager")]
        public async Task<ActionResult<FundDto>> CreateFund([FromBody] CreateFundDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get user ID from claims
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                createDto.CreatedByUserId = userId;

                // Check if fund code already exists
                if (await _fundRepository.FundExistsAsync(createDto.FundCode))
                {
                    return BadRequest(new { error = $"Fund with code {createDto.FundCode} already exists" });
                }

                var fund = FundMapper.ToEntity(createDto);
                fund = await _fundRepository.AddAsync(fund);

                var fundDto = FundMapper.ToDto(fund);
                return CreatedAtAction(nameof(GetFundById), new { id = fund.Id }, fundDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fund");
                return StatusCode(500, new { error = "An error occurred while creating the fund" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,FundManager")]
        public async Task<ActionResult<FundDto>> UpdateFund(int id, [FromBody] UpdateFundDto updateDto)
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
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                updateDto.ModifiedByUserId = userId;

                var existingFund = await _fundRepository.GetByIdAsync(id);
                if (existingFund == null)
                {
                    return NotFound(new { error = $"Fund with ID {id} not found" });
                }

                FundMapper.UpdateEntity(existingFund, updateDto);
                await _fundRepository.UpdateAsync(existingFund);

                var fundDto = FundMapper.ToDto(existingFund);
                return Ok(fundDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fund with ID {FundId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the fund" });
            }
        }

        [HttpPost("{id}/ensure-flatness/{date:datetime}")]
        [Authorize(Roles = "Admin,Trader")]
        public async Task<ActionResult> EnsureFundFlatness(int id, DateTime date)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var success = await _cashManagementService.EnsureFundFlatnessAsync(id, date, userId);
                
                if (success)
                {
                    return Ok(new { message = "Fund flatness ensured successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to ensure fund flatness" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring fund flatness for fund {FundId}", id);
                return StatusCode(500, new { error = "An error occurred while ensuring fund flatness" });
            }
        }
    }
}