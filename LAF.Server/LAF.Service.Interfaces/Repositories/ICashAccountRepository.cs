using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface ICashAccountRepository
    {
        Task<CashAccount> GetByIdAsync(int id);
        Task<CashAccount> GetByFundIdAsync(int fundId);
        Task<IEnumerable<CashAccount>> GetAllAsync();
        Task<IEnumerable<CashAccount>> FindAsync(Expression<Func<CashAccount, bool>> predicate);
        Task<CashAccount> AddAsync(CashAccount cashAccount);
        Task UpdateAsync(CashAccount cashAccount);
        Task DeleteAsync(int id);
        Task<IEnumerable<CashAccount>> GetCashAccountsByFundAsync(int fundId);
        Task<bool> CashAccountExistsForFundAsync(int fundId, string currencyCode);
        Task<decimal> GetCurrentBalanceAsync(int cashAccountId);
    }
}