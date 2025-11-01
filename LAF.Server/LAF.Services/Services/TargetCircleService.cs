using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.Services.Mappers;

namespace LAF.Services.Services
{
    public class TargetCircleService : ITargetCircleService
    {
        private readonly IRepoTradeRepository _repoTradeRepository;
        private readonly IRepoRateRepository _repoRateRepository;
        private readonly ICounterpartyRepository _counterpartyRepository;
        private readonly ILogger<TargetCircleService> _logger;

        public TargetCircleService(
            IRepoTradeRepository repoTradeRepository,
            IRepoRateRepository repoRateRepository,
            ICounterpartyRepository counterpartyRepository,
            ILogger<TargetCircleService> logger)
        {
            _repoTradeRepository = repoTradeRepository;
            _repoRateRepository = repoRateRepository;
            _counterpartyRepository = counterpartyRepository;
            _logger = logger;
        }


        public async Task<TargetCircleValidationDto> ValidateTradeAgainstTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate, decimal proposedNotional)
        {
            try
            {
                // Get current exposure for the counterparty on the trade date
                var currentExposure = await GetCurrentExposureAsync(counterpartyId, collateralTypeId, tradeDate);

                // Get TargetCircle for the counterparty on the trade date
                var targetCircle = await GetTargetCircleAsync(counterpartyId, collateralTypeId, tradeDate);

                // Get counterparty name
                var counterparty = await _counterpartyRepository.GetByIdAsync(counterpartyId);
                var counterpartyName = counterparty?.CounterpartyName ?? "Unknown";

                return RepoRateMapper.ToValidationDto(counterpartyId, counterpartyName, tradeDate,
                    currentExposure, proposedNotional, targetCircle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating trade against TargetCircle for counterparty {CounterpartyId}", counterpartyId);
                throw;
            }
        }

        public async Task<bool> IsTradeWithinTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate, decimal proposedNotional)
        {
            var validation = await ValidateTradeAgainstTargetCircleAsync(counterpartyId, collateralTypeId, tradeDate, proposedNotional);
            return validation.IsWithinLimit;
        }

        public async Task<decimal> GetCurrentExposureAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate)
        {
            try
            {
                // Get total notional of all active trades for this counterparty on the trade date
                var totalNotional = await _repoTradeRepository.GetTotalNotionalByCounterpartyCollateralTypeAndDateAsync(counterpartyId, collateralTypeId, tradeDate);
                return totalNotional;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current exposure for counterparty {CounterpartyId}", counterpartyId);
                throw;
            }
        }

        public async Task<decimal> GetTargetCircleAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate)
        {
            try
            {
                // Get the repo rate for the counterparty on the trade date
                // We need to specify a collateral type, so we'll get the most restrictive one
                var repoRates = await _repoRateRepository.FindAsync(rr =>
                    rr.CounterpartyId == counterpartyId && rr.CollateralTypeId == collateralTypeId && rr.EffectiveDate == tradeDate);

                var repoRate = repoRates.OrderBy(rr => rr.TargetCircle).FirstOrDefault();

                if (repoRate == null)
                {
                    _logger.LogWarning("No TargetCircle found for counterparty {CounterpartyId}, {CollateralTypeId} on date {TradeDate}", counterpartyId, collateralTypeId, tradeDate);
                    return 0;
                }

                return repoRate.TargetCircle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TargetCircle for counterparty {CounterpartyId} and {CollateralTypeId}", counterpartyId, collateralTypeId);
                throw;
            }
        }

        public async Task<CounterpartyExposureDto> GetCounterpartyExposureAsync(int counterpartyId, int collateralTypeId, DateTime tradeDate)
        {
            try
            {
                // Get current exposure
                var currentExposure = await GetCurrentExposureAsync(counterpartyId, collateralTypeId, tradeDate);

                // Get TargetCircle
                var targetCircle = await GetTargetCircleAsync(counterpartyId, collateralTypeId, tradeDate);

                // Get counterparty details
                var counterparty = await _counterpartyRepository.GetByIdAsync(counterpartyId);
                if (counterparty == null)
                {
                    throw new KeyNotFoundException($"Counterparty with ID {counterpartyId} not found");
                }

                var availableLimit = Math.Max(0, (targetCircle) - currentExposure);
                var utilizationPercentage = targetCircle > 0 ? (currentExposure / (targetCircle)) * 100 : 0;
                var isLimitBreached = currentExposure > (targetCircle);

                return new CounterpartyExposureDto
                {
                    CounterpartyId = counterpartyId,
                    CounterpartyName = counterparty.CounterpartyName,
                    TradeDate = tradeDate,
                    CurrentExposure = currentExposure,
                    TargetCircle = targetCircle,
                    AvailableLimit = availableLimit,
                    UtilizationPercentage = utilizationPercentage,
                    IsLimitBreached = isLimitBreached
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counterparty exposure for counterparty {CounterpartyId}", counterpartyId);
                throw;
            }
        }


    }
}