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
        Task<IEnumerable<RepoRate>> FindAsync(Expression<Func<RepoRate, bool>> predicate);
        Task<RepoRate> AddAsync(RepoRate repoRate);
        Task UpdateAsync(RepoRate repoRate);
        Task DeleteAsync(int id);
        Task<RepoRate> GetRepoRateAsync(int counterpartyId, int collateralTypeId, DateTime repoDate);
        Task<IEnumerable<RepoRate>> GetRepoRatesByDateAsync(DateTime repoDate, bool returnPreviousDayIfNotAvailable);
        Task<IEnumerable<RepoRate>> GetRepoRatesByCounterpartyAsync(int counterpartyId);
        Task<IEnumerable<RepoRate>> GetRepoRatesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<bool> RepoRateExistsAsync(int counterpartyId, int collateralTypeId, DateTime repoDate);
        Task UpdateFinalCircle(int counterpartyId, int collateralTypeId, DateTime startDate, decimal proposedNotional);
    }
}