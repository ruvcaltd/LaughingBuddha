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

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CounterpartyDto>>> GetActiveCounterparties()
        {
            try
            {
                var counterparties = await _counterpartyRepository.GetActiveCounterpartiesAsync();
                var counterpartyDtos = CounterpartyMapper.ToDtoList(counterparties);
                return Ok(counterpartyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active counterparties");
                return StatusCode(500, new { error = "An error occurred while retrieving active counterparties" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CounterpartyDto>> GetCounterpartyById(int id)
        {
            try
            {
                var counterparty = await _counterpartyRepository.GetByIdAsync(id);
                if (counterparty == null)
                {
                    return NotFound(new { error = $"Counterparty with ID {id} not found" });
                }

                var counterpartyDto = CounterpartyMapper.ToDto(counterparty);
                return Ok(counterpartyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving counterparty with ID {CounterpartyId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the counterparty" });
            }
        }

        [HttpGet("{id}/exposure/{tradeDate:datetime}")]
        public async Task<ActionResult<CounterpartyExposureDto>> GetCounterpartyExposure(int id, DateTime tradeDate)
        {
            try
            {
                var exposure = await _targetCircleService.GetCounterpartyExposureAsync(id, tradeDate);
                return Ok(exposure);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exposure for counterparty {CounterpartyId} as of {TradeDate}", id, tradeDate);
                return StatusCode(500, new { error = "An error occurred while retrieving counterparty exposure" });
            }
        }

        [HttpGet("exposures/{tradeDate:datetime}")]
        public async Task<ActionResult<IEnumerable<CounterpartyExposureDto>>> GetAllCounterpartyExposures(DateTime tradeDate)
        {
            try
            {
                var exposures = await _targetCircleService.GetAllCounterpartyExposuresAsync(tradeDate);
                return Ok(exposures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all counterparty exposures as of {TradeDate}", tradeDate);
                return StatusCode(500, new { error = "An error occurred while retrieving counterparty exposures" });
            }
        }

        [HttpGet("limit-breaches/{tradeDate:datetime}")]
        public async Task<ActionResult<IEnumerable<CounterpartyExposureDto>>> GetLimitBreaches(DateTime tradeDate)
        {
            try
            {
                var breaches = await _targetCircleService.GetLimitBreachesAsync(tradeDate);
                return Ok(breaches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving limit breaches as of {TradeDate}", tradeDate);
                return StatusCode(500, new { error = "An error occurred while retrieving limit breaches" });
            }
        }

        //[HttpGet("high-utilization/{tradeDate:datetime}")]
        //public async Task<ActionResult<IEnumerable<CounterpartyExposureDto>>> GetHighUtilizationCounterparties(
        //    DateTime tradeDate, 
        //    [FromQuery] decimal threshold = 80m)
        //{
        //    try
        //    {
        //        var highUtilization = await _targetCircleService.GetHighUtilizationCounterpartiesAsync(tradeDate, threshold);
        //        return Ok(highUtilization);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving high utilization counterparties as of {TradeDate}", tradeDate);
        //        return StatusCode(500, new { error = "An error occurred while retrieving high utilization counterparties" });
        //    }
        //}

        [HttpPost]
        [Authorize(Roles = "Admin,CounterpartyManager")]
        public async Task<ActionResult<CounterpartyDto>> CreateCounterparty([FromBody] CreateCounterpartyDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                createDto.CreatedByUserId = userId;

                // Check if counterparty name already exists
                if (await _counterpartyRepository.CounterpartyExistsAsync(createDto.Name))
                {
                    return BadRequest(new { error = $"Counterparty with name {createDto.Name} already exists" });
                }

                var counterparty = CounterpartyMapper.ToEntity(createDto);
                counterparty = await _counterpartyRepository.AddAsync(counterparty);

                var counterpartyDto = CounterpartyMapper.ToDto(counterparty);
                return CreatedAtAction(nameof(GetCounterpartyById), new { id = counterparty.Id }, counterpartyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating counterparty");
                return StatusCode(500, new { error = "An error occurred while creating the counterparty" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,CounterpartyManager")]
        public async Task<ActionResult<CounterpartyDto>> UpdateCounterparty(int id, [FromBody] UpdateCounterpartyDto updateDto)
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

                var existingCounterparty = await _counterpartyRepository.GetByIdAsync(id);
                if (existingCounterparty == null)
                {
                    return NotFound(new { error = $"Counterparty with ID {id} not found" });
                }

                CounterpartyMapper.UpdateEntity(existingCounterparty, updateDto);
                await _counterpartyRepository.UpdateAsync(existingCounterparty);

                var counterpartyDto = CounterpartyMapper.ToDto(existingCounterparty);
                return Ok(counterpartyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating counterparty with ID {CounterpartyId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the counterparty" });
            }
        }
    }
}