using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.DataAccess.Models;
using LAF.Services.Mappers;

namespace LAF.Services.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ISecurityRepository _securityRepository;
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(
            ISecurityRepository securityRepository,
            ILogger<SecurityService> logger)
        {
            _securityRepository = securityRepository;
            _logger = logger;
        }

        public async Task<SecurityDto> GetByIdAsync(int id)
        {
            try
            {
                var security = await _securityRepository.GetByIdAsync(id);
                return SecurityMapper.ToDto(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security with ID {SecurityId}", id);
                throw;
            }
        }

        public async Task<SecurityDto> GetByIsinAsync(string isin)
        {
            try
            {
                var security = await _securityRepository.GetByIsinAsync(isin);
                return SecurityMapper.ToDto(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security with ISIN {Isin}", isin);
                throw;
            }
        }

        public async Task<IEnumerable<SecurityDto>> GetAllAsync()
        {
            try
            {
                var securities = await _securityRepository.GetAllAsync();
                return SecurityMapper.ToDtoList(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all securities");
                throw;
            }
        }

        public async Task<IEnumerable<SecurityDto>> GetSecuritiesByTypeAsync(string assetType)
        {
            try
            {
                var securities = await _securityRepository.GetSecuritiesByTypeAsync(assetType);
                return SecurityMapper.ToDtoList(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving securities of type {AssetType}", assetType);
                throw;
            }
        }

        public async Task<IEnumerable<SecurityDto>> GetSecuritiesByIssuerAsync(string issuer)
        {
            try
            {
                var securities = await _securityRepository.GetSecuritiesByIssuerAsync(issuer);
                return SecurityMapper.ToDtoList(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving securities for issuer {Issuer}", issuer);
                throw;
            }
        }

        public async Task<SecurityDto> CreateAsync(CreateSecurityDto createDto)
        {
            try
            {
                if (await SecurityExistsAsync(createDto.Isin))
                {
                    throw new InvalidOperationException($"Security with ISIN {createDto.Isin} already exists");
                }

                var security = SecurityMapper.ToEntity(createDto);
                security = await _securityRepository.AddAsync(security);
                return SecurityMapper.ToDto(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating security with ISIN {Isin}", createDto.Isin);
                throw;
            }
        }

        public async Task<SecurityDto> UpdateAsync(UpdateSecurityDto updateDto)
        {
            try
            {
                var existingSecurity = await _securityRepository.GetByIdAsync(updateDto.Id);
                if (existingSecurity == null)
                {
                    throw new KeyNotFoundException($"Security with ID {updateDto.Id} not found");
                }

                SecurityMapper.UpdateEntity(existingSecurity, updateDto);
                await _securityRepository.UpdateAsync(existingSecurity);
                return SecurityMapper.ToDto(existingSecurity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security with ID {SecurityId}", updateDto.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _securityRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting security with ID {SecurityId}", id);
                throw;
            }
        }

        public async Task<bool> SecurityExistsAsync(string isin)
        {
            return await _securityRepository.SecurityExistsAsync(isin);
        }

        public async Task<SecurityDto> GetByIssuerAssetTypeAndMaturity(string issuer, string assetType, DateTime securityMaturityDate)
        {
            try
            {
                var security = await _securityRepository.GetByIssuerAssetTypeAndMaturity(issuer, assetType, securityMaturityDate);
                return SecurityMapper.ToDto(security);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security with Issuer {Issuer}, AssetType {AssetType}, and MaturityDate {MaturityDate}", 
                    issuer, assetType, securityMaturityDate);
                throw;
            }
        }

        public async Task<IEnumerable<SecurityDto>> FindAsync(Expression<Func<Security, bool>> predicate)
        {
            try
            {
                var securities = await _securityRepository.FindAsync(predicate);
                return SecurityMapper.ToDtoList(securities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding securities with predicate");
                throw;
            }
        }
    }
}