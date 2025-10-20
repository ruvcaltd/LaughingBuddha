using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Services;

namespace LAF.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EagleIntegrationController : ControllerBase
    {
        private readonly IEagleIntegrationService _eagleIntegrationService;
        private readonly ILogger<EagleIntegrationController> _logger;

        public EagleIntegrationController(
            IEagleIntegrationService eagleIntegrationService,
            ILogger<EagleIntegrationController> logger)
        {
            _eagleIntegrationService = eagleIntegrationService;
            _logger = logger;
        }

        [HttpPost("import-cash-balances")]
        [Authorize(Roles = "Admin,EagleImporter")]
        public async Task<ActionResult<EagleImportResponseDto>> ImportCashBalances([FromBody] EagleImportRequestDto importRequest)
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

                importRequest.ImportedByUserId = userId;

                var result = await _eagleIntegrationService.ImportCashBalancesAsync(importRequest);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation during Eagle import");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing cash balances from Eagle");
                return StatusCode(500, new { error = "An error occurred while importing cash balances" });
            }
        }

        [HttpPost("export-end-of-day-balances")]
        [Authorize(Roles = "Admin,EagleExporter")]
        public async Task<ActionResult<EagleExportResponseDto>> ExportEndOfDayBalances([FromBody] EagleExportRequestDto exportRequest)
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

                exportRequest.ExportedByUserId = userId;

                var result = await _eagleIntegrationService.ExportEndOfDayBalancesAsync(exportRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting end-of-day balances to Eagle");
                return StatusCode(500, new { error = "An error occurred while exporting end-of-day balances" });
            }
        }

        [HttpPost("generate-flat-fund-balances")]
        [Authorize(Roles = "Admin,EagleExporter")]
        public async Task<ActionResult<EagleExportResponseDto>> GenerateFlatFundBalances([FromBody] EagleExportRequestDto exportRequest)
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

                exportRequest.ExportedByUserId = userId;

                var result = await _eagleIntegrationService.GenerateFlatFundBalancesAsync(
                    exportRequest.ExportDate, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flat fund balances for Eagle export");
                return StatusCode(500, new { error = "An error occurred while generating flat fund balances" });
            }
        }

        [HttpGet("prepare-end-of-day-balances/{exportDate:datetime}")]
        [Authorize(Roles = "Admin,EagleExporter")]
        public async Task<ActionResult<IEnumerable<FundBalanceExportDto>>> PrepareEndOfDayBalances(DateTime exportDate)
        {
            try
            {
                var balances = await _eagleIntegrationService.PrepareEndOfDayBalancesAsync(exportDate);
                return Ok(balances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing end-of-day balances for {ExportDate}", exportDate);
                return StatusCode(500, new { error = "An error occurred while preparing end-of-day balances" });
            }
        }

        [HttpPost("validate-import-data")]
        [Authorize(Roles = "Admin,EagleImporter")]
        public async Task<ActionResult<bool>> ValidateImportData([FromBody] EagleImportRequestDto importRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isValid = await _eagleIntegrationService.ValidateEagleImportDataAsync(importRequest);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Eagle import data");
                return StatusCode(500, new { error = "An error occurred while validating import data" });
            }
        }

        [HttpPost("process-cash-balance")]
        [Authorize(Roles = "Admin,EagleImporter")]
        public async Task<ActionResult<bool>> ProcessCashBalance(
            [FromQuery] string fundCode,
            [FromQuery] decimal openingBalance,
            [FromQuery] string currency,
            [FromQuery] DateTime balanceDate)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var success = await _eagleIntegrationService.ProcessEagleCashBalanceAsync(
                    fundCode, openingBalance, currency, balanceDate, userId);

                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Eagle cash balance for fund {FundCode}", fundCode);
                return StatusCode(500, new { error = "An error occurred while processing the cash balance" });
            }
        }
    }
}