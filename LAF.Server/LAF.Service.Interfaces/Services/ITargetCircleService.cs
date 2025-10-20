using System;
using System.Threading.Tasks;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface ITargetCircleService
    {
        Task<TargetCircleValidationDto> ValidateTradeAgainstTargetCircleAsync(int counterpartyId, DateTime tradeDate, decimal proposedNotional);
        Task<bool> IsTradeWithinTargetCircleAsync(int counterpartyId, DateTime tradeDate, decimal proposedNotional);
        Task<decimal> GetCurrentExposureAsync(int counterpartyId, DateTime tradeDate);
        Task<decimal> GetTargetCircleAsync(int counterpartyId, DateTime tradeDate);
        Task<CounterpartyExposureDto> GetCounterpartyExposureAsync(int counterpartyId, DateTime tradeDate);
        Task<IEnumerable<CounterpartyExposureDto>> GetAllCounterpartyExposuresAsync(DateTime tradeDate);
        Task<IEnumerable<CounterpartyExposureDto>> GetLimitBreachesAsync(DateTime tradeDate);
    }
}