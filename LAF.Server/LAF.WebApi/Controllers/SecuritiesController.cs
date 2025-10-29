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
    public class SecuritiesController : ControllerBase
    {
        private readonly ISecurityService _securityService;
        private readonly ILogger<SecuritiesController> _logger;

        public SecuritiesController(
            ISecurityService securityService,
            ILogger<SecuritiesController> logger)
        {
            _securityService = securityService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all securities
        /// </summary>
        /// <returns>List of securities</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SecurityDto>>> GetAllSecurities()
        {
            try
            {
                var securities = await _securityService.GetAllAsync();
                return Ok(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all securities");
                return StatusCode(500, new { error = "An error occurred while retrieving securities" });
            }
        }

        /// <summary>
        /// Gets a security by ID
        /// </summary>
        /// <param name="id">Security ID</param>
        /// <returns>Security details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SecurityDto>> GetSecurityById(int id)
        {
            try
            {
                var security = await _securityService.GetByIdAsync(id);
                if (security == null)
                {
                    return NotFound(new { error = $"Security with ID {id} not found" });
                }
                return Ok(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security with ID {SecurityId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the security" });
            }
        }

        /// <summary>
        /// Gets a security by ISIN
        /// </summary>
        /// <param name="isin">Security ISIN</param>
        /// <returns>Security details</returns>
        [HttpGet("isin/{isin}")]
        public async Task<ActionResult<SecurityDto>> GetSecurityByIsin(string isin)
        {
            try
            {
                var security = await _securityService.GetByIsinAsync(isin);
                if (security == null)
                {
                    return NotFound(new { error = $"Security with ISIN {isin} not found" });
                }
                return Ok(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security with ISIN {Isin}", isin);
                return StatusCode(500, new { error = "An error occurred while retrieving the security" });
            }
        }

        /// <summary>
        /// Gets securities by asset type
        /// </summary>
        /// <param name="assetType">Asset type</param>
        /// <returns>List of securities of the specified type</returns>
        [HttpGet("type/{assetType}")]
        public async Task<ActionResult<IEnumerable<SecurityDto>>> GetSecuritiesByType(string assetType)
        {
            try
            {
                var securities = await _securityService.GetSecuritiesByTypeAsync(assetType);
                return Ok(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving securities of type {AssetType}", assetType);
                return StatusCode(500, new { error = "An error occurred while retrieving securities" });
            }
        }

        /// <summary>
        /// Creates a new security
        /// </summary>
        /// <param name="createDto">Security creation details</param>
        /// <returns>Created security</returns>
        [HttpPost]
        public async Task<ActionResult<SecurityDto>> CreateSecurity([FromBody] CreateSecurityDto createDto)
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

                var security = await _securityService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetSecurityById), new { id = security.Id }, security);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security");
                return StatusCode(500, new { error = "An error occurred while creating the security" });
            }
        }

        /// <summary>
        /// Updates an existing security
        /// </summary>
        /// <param name="id">Security ID</param>
        /// <param name="updateDto">Security update details</param>
        /// <returns>Updated security</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<SecurityDto>> UpdateSecurity(int id, [FromBody] UpdateSecurityDto updateDto)
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

                var security = await _securityService.UpdateAsync(updateDto);
                return Ok(security);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security with ID {SecurityId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the security" });
            }
        }

        /// <summary>
        /// Deletes a security
        /// </summary>
        /// <param name="id">Security ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSecurity(int id)
        {
            try
            {
                await _securityService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting security with ID {SecurityId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the security" });
            }
        }
    }
}