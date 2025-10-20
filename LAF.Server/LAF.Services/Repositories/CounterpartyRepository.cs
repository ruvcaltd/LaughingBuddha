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
    public class CounterpartyRepository : ICounterpartyRepository
    {
        private readonly LAFDbContext _context;

        public CounterpartyRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<Counterparty> GetByIdAsync(int id)
        {
            return await _context.Counterparties
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Counterparty> GetByNameAsync(string name)
        {
            return await _context.Counterparties
                .FirstOrDefaultAsync(c => c.CounterpartyName == name);
        }

        public async Task<IEnumerable<Counterparty>> GetAllAsync()
        {
            return await _context.Counterparties
                .OrderBy(c => c.CounterpartyName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Counterparty>> FindAsync(Expression<Func<Counterparty, bool>> predicate)
        {
            return await _context.Counterparties
                .Where(predicate)
                .OrderBy(c => c.CounterpartyName)
                .ToListAsync();
        }

        public async Task<Counterparty> AddAsync(Counterparty counterparty)
        {
            _context.Counterparties.Add(counterparty);
            await _context.SaveChangesAsync();
            return counterparty;
        }

        public async Task UpdateAsync(Counterparty counterparty)
        {
            _context.Counterparties.Update(counterparty);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var counterparty = await _context.Counterparties.FindAsync(id);
            if (counterparty != null)
            {
                _context.Counterparties.Remove(counterparty);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Counterparty>> GetActiveCounterpartiesAsync()
        {
            return await _context.Counterparties
                .Where(c => c.IsActive)
                .OrderBy(c => c.CounterpartyName)
                .ToListAsync();
        }

        public async Task<bool> CounterpartyExistsAsync(string name)
        {
            return await _context.Counterparties.AnyAsync(c => c.CounterpartyName == name);
        }

        public async Task<IEnumerable<Counterparty>> GetCounterpartiesByCreditRatingAsync(string creditRating)
        {
            return await _context.Counterparties
                .Where(c => c.CreditRating == creditRating)
                .OrderBy(c => c.CounterpartyName)
                .ToListAsync();
        }
    }
}