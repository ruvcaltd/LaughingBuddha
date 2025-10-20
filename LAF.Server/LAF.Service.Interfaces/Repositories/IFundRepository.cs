using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface IFundRepository
    {
        Task<Fund> GetByIdAsync(int id);
        Task<Fund> GetByFundCodeAsync(string fundCode);
        Task<IEnumerable<Fund>> GetAllAsync();
        Task<IEnumerable<Fund>> FindAsync(Expression<Func<Fund, bool>> predicate);
        Task<Fund> AddAsync(Fund fund);
        Task UpdateAsync(Fund fund);
        Task DeleteAsync(int id);
        Task<IEnumerable<Fund>> GetActiveFundsAsync();
        Task<bool> FundExistsAsync(string fundCode);
        Task<IEnumerable<Fund>> GetFundsByCurrencyAsync(string currencyCode);
    }
}