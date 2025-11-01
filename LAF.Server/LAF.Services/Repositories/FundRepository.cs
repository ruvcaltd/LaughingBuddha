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
    public class FundRepository : IFundRepository
    {
        private readonly LAFDbContext _context;

        public FundRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<Fund> GetByIdAsync(int id)
        {
            return await _context.Funds
                  .Include(f => f.CashAccounts)
                .ThenInclude(c => c.Cashflows)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Fund> GetByFundCodeAsync(string fundCode)
        {
            return await _context.Funds
                  .Include(f => f.CashAccounts)
                .ThenInclude(c => c.Cashflows)
                .FirstOrDefaultAsync(f => f.FundCode == fundCode);
        }

        public async Task<IEnumerable<Fund>> GetAllAsync()
        {
            return await _context.Funds
                  .Include(f => f.CashAccounts)
                .ThenInclude(c => c.Cashflows)
                .OrderBy(f => f.FundCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fund>> FindAsync(Expression<Func<Fund, bool>> predicate)
        {
            return await _context.Funds
                  .Include(f => f.CashAccounts)
                .ThenInclude(c => c.Cashflows)
                .Where(predicate)
                .OrderBy(f => f.FundCode)
                .ToListAsync();
        }

        public async Task<Fund> AddAsync(Fund fund)
        {
            _context.Funds.Add(fund);
            await _context.SaveChangesAsync();
            return fund;
        }

        public async Task UpdateAsync(Fund fund)
        {
            _context.Funds.Update(fund);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var fund = await _context.Funds.FindAsync(id);
            if (fund != null)
            {
                _context.Funds.Remove(fund);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Fund>> GetActiveFundsAsync()
        {
            return await _context.Funds
                .Where(f => f.IsActive)
                .Include(f => f.CashAccounts)
                .ThenInclude(c => c.Cashflows)
                .OrderBy(f => f.FundCode)
                .ToListAsync();
        }

        public async Task<bool> FundExistsAsync(string fundCode)
        {
            return await _context.Funds.AnyAsync(f => f.FundCode == fundCode);
        }

        public async Task<IEnumerable<Fund>> GetFundsByCurrencyAsync(string currencyCode)
        {
            return await _context.Funds.Include(f => f.CashAccounts).ThenInclude(c => c.Cashflows)
                .Where(f => f.CurrencyCode == currencyCode)
                .OrderBy(f => f.FundCode)
                .ToListAsync();
        }
    }
}