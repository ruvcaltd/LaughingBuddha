using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface ISecurityService
    {
        Task<SecurityDto> GetByIdAsync(int id);
        Task<SecurityDto> GetByIsinAsync(string isin);
        Task<IEnumerable<SecurityDto>> GetAllAsync();
        Task<IEnumerable<SecurityDto>> GetSecuritiesByTypeAsync(string assetType);
        Task<IEnumerable<SecurityDto>> GetSecuritiesByIssuerAsync(string issuer);
        Task<SecurityDto> CreateAsync(CreateSecurityDto createDto);
        Task<SecurityDto> UpdateAsync(UpdateSecurityDto updateDto);
        Task DeleteAsync(int id);
        Task<bool> SecurityExistsAsync(string isin);
        Task<SecurityDto> GetByIssuerAssetTypeAndMaturity(string issuer, string assetType, DateTime securityMaturityDate);
        Task<IEnumerable<SecurityDto>> FindAsync(Expression<Func<Security, bool>> predicate);
    }
}