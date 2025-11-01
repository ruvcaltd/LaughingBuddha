using System;
using System.Threading.Tasks;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface ITargetCircleService
    {
        Task<TargetCircleValidationDto> ValidateTradeAgainstTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate, decimal proposedNotional);
        Task<bool> IsTradeWithinTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate, decimal proposedNotional);
        Task<decimal> GetCurrentExposureAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate);
        Task<decimal> GetTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate);
        Task<CounterpartyExposureDto> GetCounterpartyExposureAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate);
    }
}