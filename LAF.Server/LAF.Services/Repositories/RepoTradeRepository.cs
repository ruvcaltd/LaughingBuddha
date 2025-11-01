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
    public class RepoTradeRepository : IRepoTradeRepository
    {
        private readonly LAFDbContext _context;

        public RepoTradeRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<RepoTrade> GetByIdAsync(int id)
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task<IEnumerable<RepoTrade>> GetAllAsync()
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepoTrade>> FindAsync(Expression<Func<RepoTrade, bool>> predicate)
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .Where(predicate)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<RepoTrade> AddAsync(RepoTrade repoTrade)
        {
            _context.RepoTrades.Add(repoTrade);
            await _context.SaveChangesAsync();
            return repoTrade;
        }

        public async Task UpdateAsync(RepoTrade repoTrade)
        {
            _context.RepoTrades.Update(repoTrade);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var repoTrade = await _context.RepoTrades.FindAsync(id);
            if (repoTrade != null)
            {
                _context.RepoTrades.Remove(repoTrade);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<RepoTrade>> GetTradesByFundAndDateAsync(int fundId, DateTime tradeDate)
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .Where(rt => rt.FundId == fundId && rt.StartDate <= tradeDate && rt.MaturityDate >= tradeDate)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalNotionalByCounterpartyCollateralTypeAndDateAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate)
        {
            return await _context.RepoTrades
                .Where(rt => rt.CounterpartyId == counterpartyId && rt.CollateralTypeId == collateralTypeId && rt.StartDate <= tradeDate && rt.MaturityDate >= tradeDate)
                .SumAsync(rt => rt.Notional);
        }

        public async Task<IEnumerable<RepoTrade>> GetActiveTradesAsync(DateTime asOfDate)
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .Where(rt => rt.StartDate <= asOfDate && rt.MaturityDate >= asOfDate)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepoTrade>> GetTradesBySettlementDateAsync(DateTime settlementDate)
        {
            return await _context.RepoTrades
                .Include(rt => rt.Security)
                .Include(rt => rt.Counterparty)
                .Include(rt => rt.Fund)
                .Include(rt => rt.CollateralType)
                .Where(rt => rt.TradeDate == settlementDate)
                .OrderByDescending(rt => rt.CreatedDate)
                .ToListAsync();
        }
    }
}