using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Services.Mappers;
using LAF.DataAccess.Models;

namespace LAF.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CollateralTypesController : ControllerBase
    {
        private readonly ICollateralTypeRepository _collateralTypeRepository;
        private readonly ILogger<CollateralTypesController> _logger;

        public CollateralTypesController(
            ICollateralTypeRepository collateralTypeRepository,
            ILogger<CollateralTypesController> logger)
        {
            _collateralTypeRepository = collateralTypeRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollateralTypeDto>>> GetAllCollateralTypes()
        {
            try
            {
                var collateralTypes = await _collateralTypeRepository.GetAllAsync();
                var collateralTypeDtos = CollateralTypeMapper.ToDtoList(collateralTypes);
                return Ok(collateralTypeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all collateral types");
                return StatusCode(500, new { error = "An error occurred while retrieving collateral types" });
            }
        }
    }
}