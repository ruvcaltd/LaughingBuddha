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
    public class RepoRateRepository : IRepoRateRepository
    {
        private readonly LAFDbContext _context;

        public RepoRateRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<RepoRate> GetByIdAsync(int id)
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .FirstOrDefaultAsync(rr => rr.Id == id);
        }

        public async Task<IEnumerable<RepoRate>> GetAllAsync()
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .OrderByDescending(rr => rr.EffectiveDate)
                .ThenBy(rr => rr.Counterparty.CounterpartyName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepoRate>> FindAsync(Expression<Func<RepoRate, bool>> predicate)
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .Where(predicate)
                .OrderByDescending(rr => rr.EffectiveDate)
                .ThenBy(rr => rr.Counterparty.CounterpartyName)
                .ToListAsync();
        }

        public async Task<RepoRate> AddAsync(RepoRate repoRate)
        {
            _context.RepoRates.Add(repoRate);
            await _context.SaveChangesAsync();
            return repoRate;
        }

        public async Task UpdateAsync(RepoRate repoRate)
        {
            _context.RepoRates.Update(repoRate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var repoRate = await _context.RepoRates.FindAsync(id);
            if (repoRate != null)
            {
                _context.RepoRates.Remove(repoRate);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RepoRate> GetRepoRateAsync(int counterpartyId, int collateralTypeId, DateTime repoDate)
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .FirstOrDefaultAsync(rr => 
                    rr.CounterpartyId == counterpartyId && 
                    rr.CollateralTypeId == collateralTypeId && 
                    rr.EffectiveDate == repoDate);
        }

        public async Task<IEnumerable<RepoRate>> GetRepoRatesByDateAsync(DateTime repoDate)
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .Where(rr => rr.EffectiveDate == repoDate)
                .OrderBy(rr => rr.Counterparty.CounterpartyName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RepoRate>> GetRepoRatesByCounterpartyAsync(int counterpartyId)
        {
            return await _context.RepoRates
                .Include(rr => rr.Counterparty)
                .Include(rr => rr.CollateralType)
                .Where(rr => rr.CounterpartyId == counterpartyId)
                .OrderByDescending(rr => rr.EffectiveDate)
                .ToListAsync();
        }

        public async Task<bool> RepoRateExistsAsync(int counterpartyId, int collateralTypeId, DateTime repoDate)
        {
            return await _context.RepoRates.AnyAsync(rr => 
                rr.CounterpartyId == counterpartyId && 
                rr.CollateralTypeId == collateralTypeId && 
                rr.EffectiveDate == repoDate);
        }
    }
}