using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface ISecurityRepository
    {
        Task<Security> GetByIdAsync(int id);
        Task<Security> GetByIsinAsync(string isin);
        Task<IEnumerable<Security>> GetAllAsync();
        Task<IEnumerable<Security>> FindAsync(Expression<Func<Security, bool>> predicate);
        Task<Security> AddAsync(Security security);
        Task UpdateAsync(Security security);
        Task DeleteAsync(int id);
        Task<IEnumerable<Security>> GetSecuritiesByTypeAsync(string assetType);
        Task<IEnumerable<Security>> GetSecuritiesByIssuerAsync(string issuer);
        Task<bool> SecurityExistsAsync(string isin);
    }
}