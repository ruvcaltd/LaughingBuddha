using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface ICashManagementService
    {
        Task<CashAccountBalanceDto> GetCashAccountBalanceAsync(int cashAccountId, DateTime asOfDate);
        Task<IEnumerable<CashAccountBalanceDto>> GetFundCashBalancesAsync(int fundId, DateTime asOfDate);
        Task<FundBalanceDto> GetFundBalanceAsync(int fundId, DateTime asOfDate);
        Task<IEnumerable<FundBalanceDto>> GetAllFundBalancesAsync(DateTime asOfDate);
        Task<FundFlatnessCheckDto> CheckFundFlatnessAsync(int fundId, DateTime checkDate);
        Task<IEnumerable<FundFlatnessCheckDto>> CheckAllFundsFlatnessAsync(DateTime checkDate);
        Task<CashflowDto> CreateCashflowAsync(CreateCashflowDto createDto, bool useTransaction);
        Task<IEnumerable<CashflowDto>> GetCashflowsByAccountAsync(int cashAccountId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<CashflowDto>> GetCashflowsByFundAsync(int fundId, DateTime? fromDate, DateTime? toDate);
        Task<FundCashflowSummaryDto> GetFundCashflowSummaryAsync(int fundId, DateTime fromDate, DateTime toDate);
        Task<bool> ProcessTradeSettlementAsync(int tradeId, int processedByUserId);
        Task<bool> ProcessTradeMaturityAsync(int tradeId, int processedByUserId);
        Task<bool> EnsureFundFlatnessAsync(int fundId, DateTime date, int processedByUserId);
        Task<IEnumerable<CashflowDto>> GetCashflowsByTradeAsync(int repoTradeId);
    }
}