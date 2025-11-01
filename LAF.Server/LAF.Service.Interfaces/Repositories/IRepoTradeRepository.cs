using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LAF.DataAccess.Models;

namespace LAF.Service.Interfaces.Repositories
{
    public interface IRepoTradeRepository
    {
        Task<RepoTrade> GetByIdAsync(int id);
        Task<IEnumerable<RepoTrade>> GetAllAsync();
        Task<IEnumerable<RepoTrade>> FindAsync(Expression<Func<RepoTrade, bool>> predicate);
        Task<RepoTrade> AddAsync(RepoTrade repoTrade);
        Task UpdateAsync(RepoTrade repoTrade);
        Task DeleteAsync(int id);
        Task<IEnumerable<RepoTrade>> GetTradesByFundAndDateAsync(int fundId, DateTime tradeDate);
        Task<decimal> GetTotalNotionalByCounterpartyCollateralTypeAndDateAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate);
        Task<IEnumerable<RepoTrade>> GetActiveTradesAsync(DateTime asOfDate);
        Task<IEnumerable<RepoTrade>> GetTradesBySettlementDateAsync(DateTime settlementDate);
    }
}