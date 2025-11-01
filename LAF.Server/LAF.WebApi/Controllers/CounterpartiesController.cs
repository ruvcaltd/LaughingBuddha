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
    public class CounterpartiesController : ControllerBase
    {
        private readonly ICounterpartyRepository _counterpartyRepository;
        private readonly ITargetCircleService _targetCircleService;
        private readonly ILogger<CounterpartiesController> _logger;

        public CounterpartiesController(
            ICounterpartyRepository counterpartyRepository,
            ITargetCircleService targetCircleService,
            ILogger<CounterpartiesController> logger)
        {
            _counterpartyRepository = counterpartyRepository;
            _targetCircleService = targetCircleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CounterpartyDto>>> GetAllCounterparties()
        {
            try
            {
                var counterparties = await _counterpartyRepository.GetAllAsync();
                var counterpartyDtos = CounterpartyMapper.ToDtoList(counterparties);
                return Ok(counterpartyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all counterparties");
                return StatusCode(500, new { error = "An error occurred while retrieving counterparties" });
            }
        }
    }
}