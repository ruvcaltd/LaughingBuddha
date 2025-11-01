using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface IRepoTradeService
    {
        Task<RepoTradeDto> GetByIdAsync(int id);
        Task<IEnumerable<RepoTradeDto>> GetAllAsync();
        Task<IEnumerable<RepoTradeDto>> FindAsync(RepoTradeQueryDto query);
        Task<RepoTradeDto> CreateAsync(CreateRepoTradeDto createDto);
        Task<RepoTradeDto> UpdateAsync(UpdateRepoTradeDto updateDto);
        Task DeleteAsync(int id, int deletedByUserId);
        Task<IEnumerable<RepoTradeDto>> GetTradesByFundAsync(int fundId);
        Task<IEnumerable<RepoTradeDto>> GetActiveTradesAsync(DateTime asOfDate);
        Task<IEnumerable<RepoTradeDto>> GetTradesBySettlementDateAsync(DateTime settlementDate);
        Task<bool> ValidateTradeAsync(CreateRepoTradeDto tradeDto);
        Task<RepoTradeDto> SettleTradeAsync(int tradeId, int settledByUserId);
        Task<RepoTradeDto> MatureTradeAsync(int tradeId, int maturedByUserId);
        Task<RepoTradeDto> CancelTradeAsync(int tradeId, int cancelledByUserId);
        Task<IEnumerable<RepoTradeDto>> SubmitTradesAsync(int[] tradeIds, int submittedByUserId);
    }
}