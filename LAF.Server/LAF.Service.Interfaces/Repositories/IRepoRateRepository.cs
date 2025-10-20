using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface IRepoRateRepository
    {
        Task<RepoRate> GetByIdAsync(int id);
        Task<IEnumerable<RepoRate>> GetAllAsync();
        Task<IEnumerable<RepoRate>> FindAsync(Expression<Func<RepoRate, bool>> predicate);
        Task<RepoRate> AddAsync(RepoRate repoRate);
        Task UpdateAsync(RepoRate repoRate);
        Task DeleteAsync(int id);
        Task<RepoRate> GetRepoRateAsync(int counterpartyId, int collateralTypeId, DateTime repoDate);
        Task<IEnumerable<RepoRate>> GetRepoRatesByDateAsync(DateTime repoDate);
        Task<IEnumerable<RepoRate>> GetRepoRatesByCounterpartyAsync(int counterpartyId);
        Task<bool> RepoRateExistsAsync(int counterpartyId, int collateralTypeId, DateTime repoDate);
    }
}