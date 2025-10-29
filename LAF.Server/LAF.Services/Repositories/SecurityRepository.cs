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
    public class SecurityRepository : ISecurityRepository
    {
        private readonly LAFDbContext _context;

        public SecurityRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<Security> GetByIdAsync(long id)
        {
            return await _context.Securities
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Security> GetByIsinAsync(string isin)
        {
            return await _context.Securities
                .FirstOrDefaultAsync(s => s.Isin == isin);
        }

        public async Task<IEnumerable<Security>> GetAllAsync()
        {
            return await _context.Securities
                .OrderBy(s => s.Isin)
                .ToListAsync();
        }

        public async Task<IEnumerable<Security>> FindAsync(Expression<Func<Security, bool>> predicate)
        {
            return await _context.Securities
                .Where(predicate)
                .OrderBy(s => s.Isin)
                .ToListAsync();
        }

        public async Task<Security> AddAsync(Security security)
        {
            _context.Securities.Add(security);
            await _context.SaveChangesAsync();
            return security;
        }

        public async Task UpdateAsync(Security security)
        {
            _context.Securities.Update(security);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var security = await _context.Securities.FindAsync(id);
            if (security != null)
            {
                _context.Securities.Remove(security);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Security>> GetSecuritiesByTypeAsync(string assetType)
        {
            return await _context.Securities
                .Where(s => s.AssetType == assetType)
                .OrderBy(s => s.Isin)
                .ToListAsync();
        }

        public async Task<IEnumerable<Security>> GetSecuritiesByIssuerAsync(string issuer)
        {
            return await _context.Securities
                .Where(s => s.Issuer == issuer)
                .OrderBy(s => s.Isin)
                .ToListAsync();
        }

        public async Task<Security> GetByIssuerAssetTypeAndMaturity(string issuer, string assetType, DateTime securityMaturityDate)
        {
            return await _context.Securities
               .Where(s => s.AssetType == assetType && s.Issuer == issuer && s.MaturityDate == securityMaturityDate)
               .FirstOrDefaultAsync();
        }

        public async Task<bool> SecurityExistsAsync(string isin)
        {
            return await _context.Securities.AnyAsync(s => s.Isin == isin);
        }


    }
}