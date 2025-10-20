using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface ICashflowRepository
    {
        Task<Cashflow> GetByIdAsync(int id);
        Task<IEnumerable<Cashflow>> GetAllAsync();
        Task<IEnumerable<Cashflow>> FindAsync(Expression<Func<Cashflow, bool>> predicate);
        Task<Cashflow> AddAsync(Cashflow cashflow);
        Task UpdateAsync(Cashflow cashflow);
        Task DeleteAsync(int id);
        Task<IEnumerable<Cashflow>> GetCashflowsByAccountAsync(int cashAccountId);
        Task<IEnumerable<Cashflow>> GetCashflowsByFundAsync(int fundId);
        Task<IEnumerable<Cashflow>> GetCashflowsByDateRangeAsync(int cashAccountId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Cashflow>> GetCashflowsByTradeAsync(int repoTradeId);
        Task<decimal> GetNetCashflowByAccountAsync(int cashAccountId, DateTime asOfDate);
    }
}