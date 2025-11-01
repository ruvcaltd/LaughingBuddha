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
using System.Security.Claims;

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

        [HttpGet("flatten/{asOfDate:datetime}")]
        public async Task<ActionResult> FlattenAllFundBalances(DateTime asOfDate)
        {
            try
            {
                var previousDate = asOfDate.Date.AddDays(-1); //TODO: business date

                await _cashManagementService.Flatten(previousDate);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when trying to flatten fund balances as of {AsOfDate}", asOfDate);
                return StatusCode(500, new { error = "An error occurred while flattening fund balances" });
            }
        }
    }
}