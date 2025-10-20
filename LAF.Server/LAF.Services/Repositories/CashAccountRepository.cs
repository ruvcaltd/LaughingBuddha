using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LAF.DataAccess.Data;
using LAF.DataAccess.Models;
using LAF.Service.Interfaces.Repositories;

namespace LAF.Services.Repositories
{
    public class CashAccountRepository : ICashAccountRepository
    {
        private readonly LAFDbContext _context;

        public CashAccountRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<CashAccount> GetByIdAsync(int id)
        {
            return await _context.CashAccounts
                .Include(ca => ca.Fund)
                .FirstOrDefaultAsync(ca => ca.Id == id);
        }

        public async Task<CashAccount> GetByFundIdAsync(int fundId)
        {
            return await _context.CashAccounts
                .Include(ca => ca.Fund)
                .FirstOrDefaultAsync(ca => ca.FundId == fundId);
        }

        public async Task<IEnumerable<CashAccount>> GetAllAsync()
        {
            return await _context.CashAccounts
                .Include(ca => ca.Fund)
                .OrderBy(ca => ca.AccountName)
                .ToListAsync();
        }

        public async Task<IEnumerable<CashAccount>> FindAsync(Expression<Func<CashAccount, bool>> predicate)
        {
            return await _context.CashAccounts
                .Include(ca => ca.Fund)
                .Where(predicate)
                .OrderBy(ca => ca.AccountName)
                .ToListAsync();
        }

        public async Task<CashAccount> AddAsync(CashAccount cashAccount)
        {
            _context.CashAccounts.Add(cashAccount);
            await _context.SaveChangesAsync();
            return cashAccount;
        }

        public async Task UpdateAsync(CashAccount cashAccount)
        {
            _context.CashAccounts.Update(cashAccount);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cashAccount = await _context.CashAccounts.FindAsync(id);
            if (cashAccount != null)
            {
                _context.CashAccounts.Remove(cashAccount);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CashAccount>> GetCashAccountsByFundAsync(int fundId)
        {
            return await _context.CashAccounts
                .Include(ca => ca.Fund)
                .Where(ca => ca.FundId == fundId)
                .OrderBy(ca => ca.AccountName)
                .ToListAsync();
        }

        public async Task<bool> CashAccountExistsForFundAsync(int fundId, string currencyCode)
        {
            return await _context.CashAccounts.AnyAsync(ca => ca.FundId == fundId && ca.CurrencyCode == currencyCode);
        }

        public async Task<decimal> GetCurrentBalanceAsync(int cashAccountId)
        {
            var account = await _context.CashAccounts
                .Include(ca => ca.Cashflows)
                .FirstOrDefaultAsync(ca => ca.Id == cashAccountId);

            if (account == null)
                return 0;

            return account.Cashflows.Sum(cf => cf.Amount);
        }
    }
}