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

        [HttpGet("previous-day")]
        public async Task<ActionResult<IEnumerable<PreviousDayRepoRateDto>>> GetPreviousDayRepoRate(
            [FromQuery] DateTime currentDate)
        {
            try
            {
                var previousDate = currentDate.Date.AddDays(-1); //TODO: business date

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

        [HttpPost("new-day")]
        public async Task<ActionResult> NewDay([FromBody] DateTime currentDate)
        {
            try
            {
                var previousDate = currentDate.Date.AddDays(-1); // TODO: business day
                var previousDayRate = await _repoRateRepository.GetRepoRatesByDateAsync(previousDate.Date, true);
                var newDayData = previousDayRate.Select(x => new CreateRepoRateDto
                {
                    CounterpartyId = x.CounterpartyId,
                    CollateralTypeId = x.CollateralTypeId,
                    RepoDate = currentDate.Date,
                    RepoRate = x.RepoRate1,
                    TargetCircle = x.TargetCircle,
                    FinalCircle = 0,
                    Active = x.Active
                });

                foreach (var newDay in newDayData)
                    try
                    {
                        await this.CreateRepoRate(newDay);
                    }
                    catch
                    {
                        _logger.LogWarning("Skipping duplicate repo rate for counterparty {CounterpartyId}, collateral type {CollateralTypeId} on date {RepoDate}",
                            newDay.CounterpartyId, newDay.CollateralTypeId, newDay.RepoDate);
                        continue;
                    }

                return Ok();
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
                return CreatedAtAction(nameof(GetRepoRatesByDate), new { repoDate = repoRate.EffectiveDate }, repoRateDto);
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
        public async Task<ActionResult<bool>> UpdateRepoRate(long id, [FromBody] UpdateRepoRateDto updateDto)
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
                var changed = existingRepoRate.TargetCircle != updateDto.TargetCircle || existingRepoRate.RepoRate1 != updateDto.RepoRate;

                RepoRateMapper.UpdateEntity(existingRepoRate, updateDto);
                await _repoRateRepository.UpdateAsync(existingRepoRate);

                return Ok(changed);
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
    }
}