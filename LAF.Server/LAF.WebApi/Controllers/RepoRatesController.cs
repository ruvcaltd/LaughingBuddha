using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Services.Mappers;
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
    public class RepoRatesController : ControllerBase
    {
        private readonly IRepoRateRepository _repoRateRepository;
        private readonly ILogger<RepoRatesController> _logger;

        public RepoRatesController(
            IRepoRateRepository repoRateRepository,
            ILogger<RepoRatesController> logger)
        {
            _repoRateRepository = repoRateRepository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RepoRateDto>> GetRepoRateById(long id)
        {
            try
            {
                var repoRate = await _repoRateRepository.GetByIdAsync((int)id);
                if (repoRate == null)
                {
                    return NotFound(new { error = $"Repo rate with ID {id} not found" });
                }

                var repoRateDto = RepoRateMapper.ToDto(repoRate);
                return Ok(repoRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repo rate with ID {RepoRateId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the repo rate" });
            }
        }

        [HttpGet("date/{repoDate:datetime}")]
        public async Task<ActionResult<IEnumerable<RepoRateDto>>> GetRepoRatesByDate(DateTime repoDate)
        {
            try
            {
                var repoRates = await _repoRateRepository.GetRepoRatesByDateAsync(repoDate.Date, false);
                var repoRateDtos = RepoRateMapper.ToDtoList(repoRates);
                return Ok(repoRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repo rates for date {RepoDate}", repoDate);
                return StatusCode(500, new { error = "An error occurred while retrieving repo rates for the date" });
            }
        }

        [HttpGet("counterparty/{counterpartyId}")]
        public async Task<ActionResult<IEnumerable<RepoRateDto>>> GetRepoRatesByCounterparty(int counterpartyId)
        {
            try
            {
                var repoRates = await _repoRateRepository.GetRepoRatesByCounterpartyAsync(counterpartyId);
                var repoRateDtos = RepoRateMapper.ToDtoList(repoRates);
                return Ok(repoRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repo rates for counterparty {CounterpartyId}", counterpartyId);
                return StatusCode(500, new { error = "An error occurred while retrieving repo rates for the counterparty" });
            }
        }

        [HttpGet("previous-day")]
        public async Task<ActionResult<IEnumerable<PreviousDayRepoRateDto>>> GetPreviousDayRepoRate(
            [FromQuery] DateTime currentDate)
        {
            try
            {
                var previousDate = currentDate.Date.AddDays(-1);

                var previousDayRate = await _repoRateRepository.GetRepoRatesByDateAsync(previousDate, true);

                var result = previousDayRate.Select(x => new PreviousDayRepoRateDto
                {
                    Id = x.Id,
                    CounterpartyId = x.CounterpartyId,
                    CollateralTypeId = x.CollateralTypeId,
                    RepoDate = x.EffectiveDate,
                    RepoRate = x.RepoRate1,
                    TargetCircle = x.TargetCircle,
                    FinalCircle = x.FinalCircle,
                    Active = x.Active
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving previous day repo rate for counterparty current date {CurrentDate}", currentDate);
                return StatusCode(500, new { error = "An error occurred while retrieving the previous day repo rate" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RepoRateDto), StatusCodes.Status201Created)] // important for nswag code gen
        public async Task<ActionResult<RepoRateDto>> CreateRepoRate([FromBody] CreateRepoRateDto createDto)
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

                createDto.CreatedByUserId = userId;

                // Check if repo rate already exists for the same counterparty, collateral type and date
                if (await _repoRateRepository.RepoRateExistsAsync(
                    createDto.CounterpartyId,
                    createDto.CollateralTypeId,
                    createDto.RepoDate))
                {
                    return BadRequest(new
                    {
                        error = $"Repo rate already exists for counterparty {createDto.CounterpartyId}, collateral type {createDto.CollateralTypeId} on {createDto.RepoDate:yyyy-MM-dd}"
                    });
                }

                var repoRate = RepoRateMapper.ToEntity(createDto);
                repoRate = await _repoRateRepository.AddAsync(repoRate);

                var repoRateDto = RepoRateMapper.ToDto(repoRate);
                return CreatedAtAction(nameof(GetRepoRateById), new { id = repoRate.Id }, repoRateDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating repo rate");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating repo rate");
                return StatusCode(500, new { error = "An error occurred while creating the repo rate" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RepoRateDto>> UpdateRepoRate(long id, [FromBody] UpdateRepoRateDto updateDto)
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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }


                updateDto.ModifiedByUserId = userId;

                var existingRepoRate = await _repoRateRepository.GetByIdAsync((int)id);
                if (existingRepoRate == null)
                {
                    return NotFound(new { error = $"Repo rate with ID {id} not found" });
                }

                RepoRateMapper.UpdateEntity(existingRepoRate, updateDto);
                await _repoRateRepository.UpdateAsync(existingRepoRate);

                var repoRateDto = RepoRateMapper.ToDto(existingRepoRate);
                return Ok(repoRateDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating repo rate");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating repo rate with ID {RepoRateId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the repo rate" });
            }
        }

        [HttpPatch("{id}/set-inactive")]
        public async Task<ActionResult<RepoRateDto>> SetRepoRateInactive(long id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var repoRate = await _repoRateRepository.GetByIdAsync((int)id);
                if (repoRate == null)
                {
                    return NotFound(new { error = $"Repo rate with ID {id} not found" });
                }

                // Set the repo rate as inactive
                repoRate.Active = false;
                repoRate.ModifiedAt = DateTimeOffset.UtcNow;

                await _repoRateRepository.UpdateAsync(repoRate);

                var repoRateDto = RepoRateMapper.ToDto(repoRate);
                return Ok(repoRateDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting repo rate {RepoRateId} as inactive", id);
                return StatusCode(500, new { error = "An error occurred while setting the repo rate as inactive" });
            }
        }

        [HttpPatch("{id}/set-active")]
        public async Task<ActionResult<RepoRateDto>> SetRepoRateActive(long id)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { error = "Invalid user authentication" });
                }

                var repoRate = await _repoRateRepository.GetByIdAsync((int)id);
                if (repoRate == null)
                {
                    return NotFound(new { error = $"Repo rate with ID {id} not found" });
                }

                // Set the repo rate as active
                repoRate.Active = true;
                repoRate.ModifiedAt = DateTimeOffset.UtcNow;

                await _repoRateRepository.UpdateAsync(repoRate);

                var repoRateDto = RepoRateMapper.ToDto(repoRate);
                return Ok(repoRateDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting repo rate {RepoRateId} as active", id);
                return StatusCode(500, new { error = "An error occurred while setting the repo rate as active" });
            }
        }

        [HttpGet("active/{asOfDate:datetime}")]
        public async Task<ActionResult<IEnumerable<RepoRateDto>>> GetActiveRepoRates(DateTime asOfDate)
        {
            try
            {
                var repoRates = await _repoRateRepository.FindAsync(rr =>
                    rr.EffectiveDate == asOfDate.Date && rr.Active);

                var repoRateDtos = RepoRateMapper.ToDtoList(repoRates);
                return Ok(repoRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active repo rates as of {AsOfDate}", asOfDate);
                return StatusCode(500, new { error = "An error occurred while retrieving active repo rates" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RepoRateDto>>> SearchRepoRates(
            [FromQuery] int? counterpartyId = null,
            [FromQuery] int? collateralTypeId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? activeOnly = null)
        {
            try
            {
                var query = await _repoRateRepository.FindAsync(rr => true); // Start with all records

                if (counterpartyId.HasValue)
                {
                    query = query.Where(rr => rr.CounterpartyId == counterpartyId.Value);
                }

                if (collateralTypeId.HasValue)
                {
                    query = query.Where(rr => rr.CollateralTypeId == collateralTypeId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(rr => rr.EffectiveDate >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(rr => rr.EffectiveDate <= toDate.Value.Date);
                }

                if (activeOnly.HasValue && activeOnly.Value)
                {
                    query = query.Where(rr => rr.Active);
                }

                var repoRates = query.ToList();
                var repoRateDtos = RepoRateMapper.ToDtoList(repoRates);
                return Ok(repoRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching repo rates with parameters");
                return StatusCode(500, new { error = "An error occurred while searching repo rates" });
            }
        }
    }
}