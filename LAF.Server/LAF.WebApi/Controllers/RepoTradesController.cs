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

        [HttpPost("submit")]
        public async Task<ActionResult<IEnumerable<RepoTradeDto>>> SubmitTrades([FromBody] int[] tradeIds)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                    throw new InvalidOperationException("User ID not found in claims"));
                    
                var submittedTrades = await _repoTradeService.SubmitTradesAsync(tradeIds, userId);
                return Ok(submittedTrades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting trades with IDs: {TradeIds}", string.Join(", ", tradeIds));
                return StatusCode(500, new { error = "An error occurred while submitting trades" });
            }
        }
    }
}