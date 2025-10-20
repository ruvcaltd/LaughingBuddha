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
    public class CollateralTypeRepository : ICollateralTypeRepository
    {
        private readonly LAFDbContext _context;

        public CollateralTypeRepository(LAFDbContext context)
        {
            _context = context;
        }

        public async Task<CollateralType> GetByIdAsync(int id)
        {
            return await _context.CollateralTypes
                .FirstOrDefaultAsync(ct => ct.Id == id);
        }

        public async Task<IEnumerable<CollateralType>> GetAllAsync()
        {
            return await _context.CollateralTypes
                .OrderBy(ct => ct.CollateralType1)
                .ToListAsync();
        }

        public async Task<IEnumerable<CollateralType>> FindAsync(Expression<Func<CollateralType, bool>> predicate)
        {
            return await _context.CollateralTypes
                .Where(predicate)
                .OrderBy(ct => ct.CollateralType1)
                .ToListAsync();
        }

        public async Task<CollateralType> AddAsync(CollateralType collateralType)
        {
            _context.CollateralTypes.Add(collateralType);
            await _context.SaveChangesAsync();
            return collateralType;
        }

        public async Task UpdateAsync(CollateralType collateralType)
        {
            _context.CollateralTypes.Update(collateralType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var collateralType = await _context.CollateralTypes.FindAsync(id);
            if (collateralType != null)
            {
                _context.CollateralTypes.Remove(collateralType);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CollateralType>> GetActiveCollateralTypesAsync()
        {
            // Assuming all collateral types are active by default since there's no IsActive field
            return await _context.CollateralTypes
                .OrderBy(ct => ct.AssetType)
                .ToListAsync();
        }

        public async Task<bool> CollateralTypeExistsAsync(string name)
        {
            return await _context.CollateralTypes.AnyAsync(ct => ct.AssetType == name);
        }
    }
}