using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface ICollateralTypeRepository
    {
        Task<CollateralType> GetByIdAsync(int id);
        Task<IEnumerable<CollateralType>> GetAllAsync();
        Task<IEnumerable<CollateralType>> FindAsync(Expression<Func<CollateralType, bool>> predicate);
        Task<CollateralType> AddAsync(CollateralType collateralType);
        Task UpdateAsync(CollateralType collateralType);
        Task DeleteAsync(int id);
        Task<IEnumerable<CollateralType>> GetActiveCollateralTypesAsync();
        Task<bool> CollateralTypeExistsAsync(string name);
    }
}