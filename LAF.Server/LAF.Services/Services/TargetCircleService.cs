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

        public async Task<TargetCircleValidationDto> ValidateTradeAgainstTargetCircleAsync(int counterpartyId, DateTime tradeDate, decimal proposedNotional)
        {
            try
            {
                // Get current exposure for the counterparty on the trade date
                var currentExposure = await GetCurrentExposureAsync(counterpartyId, tradeDate);

                // Get TargetCircle for the counterparty on the trade date
                var targetCircle = await GetTargetCircleAsync(counterpartyId, tradeDate);

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

        public async Task<bool> IsTradeWithinTargetCircleAsync(int counterpartyId, DateTime tradeDate, decimal proposedNotional)
        {
            var validation = await ValidateTradeAgainstTargetCircleAsync(counterpartyId, tradeDate, proposedNotional);
            return validation.IsWithinLimit;
        }

        public async Task<decimal> GetCurrentExposureAsync(int counterpartyId, DateTime tradeDate)
        {
            try
            {
                // Get total notional of all active trades for this counterparty on the trade date
                var totalNotional = await _repoTradeRepository.GetTotalNotionalByCounterpartyAndDateAsync(counterpartyId, tradeDate);
                return totalNotional;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current exposure for counterparty {CounterpartyId}", counterpartyId);
                throw;
            }
        }

        public async Task<decimal> GetTargetCircleAsync(int counterpartyId, DateTime tradeDate)
        {
            try
            {
                // Get the repo rate for the counterparty on the trade date
                // We need to specify a collateral type, so we'll get the most restrictive one
                var repoRates = await _repoRateRepository.FindAsync(rr =>
                    rr.CounterpartyId == counterpartyId && rr.EffectiveDate == tradeDate);

                var repoRate = repoRates.OrderBy(rr => rr.TargetCircle).FirstOrDefault();

                if (repoRate == null)
                {
                    _logger.LogWarning("No TargetCircle found for counterparty {CounterpartyId} on date {TradeDate}", counterpartyId, tradeDate);
                    return 0;
                }

                return repoRate.TargetCircle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TargetCircle for counterparty {CounterpartyId}", counterpartyId);
                throw;
            }
        }

        public async Task<CounterpartyExposureDto> GetCounterpartyExposureAsync(int counterpartyId, DateTime tradeDate)
        {
            try
            {
                // Get current exposure
                var currentExposure = await GetCurrentExposureAsync(counterpartyId, tradeDate);

                // Get TargetCircle
                var targetCircle = await GetTargetCircleAsync(counterpartyId, tradeDate);

                // Get counterparty details
                var counterparty = await _counterpartyRepository.GetByIdAsync(counterpartyId);
                if (counterparty == null)
                {
                    throw new KeyNotFoundException($"Counterparty with ID {counterpartyId} not found");
                }

                var availableLimit = Math.Max(0, (targetCircle * 1000000) - currentExposure);
                var utilizationPercentage = targetCircle > 0 ? (currentExposure / (targetCircle * 1000000)) * 100 : 0;
                var isLimitBreached = currentExposure > (targetCircle * 1000000);

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

        public async Task<IEnumerable<CounterpartyExposureDto>> GetAllCounterpartyExposuresAsync(DateTime tradeDate)
        {
            try
            {
                var activeCounterparties = await _counterpartyRepository.GetActiveCounterpartiesAsync();
                var exposures = new List<CounterpartyExposureDto>();

                foreach (var counterparty in activeCounterparties)
                {
                    try
                    {
                        var exposure = await GetCounterpartyExposureAsync(counterparty.Id, tradeDate);
                        exposures.Add(exposure);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting exposure for counterparty {CounterpartyId}", counterparty.Id);
                        // Continue with other counterparties
                    }
                }

                return exposures.OrderByDescending(e => e.UtilizationPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all counterparty exposures");
                throw;
            }
        }

        public async Task<IEnumerable<CounterpartyExposureDto>> GetLimitBreachesAsync(DateTime tradeDate)
        {
            try
            {
                var allExposures = await GetAllCounterpartyExposuresAsync(tradeDate);
                return allExposures.Where(e => e.IsLimitBreached).OrderByDescending(e => e.UtilizationPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting limit breaches");
                throw;
            }
        }

        public async Task<IEnumerable<CounterpartyExposureDto>> GetHighUtilizationCounterpartiesAsync(DateTime tradeDate, decimal thresholdPercentage = 80m)
        {
            try
            {
                var allExposures = await GetAllCounterpartyExposuresAsync(tradeDate);
                return allExposures.Where(e => e.UtilizationPercentage >= thresholdPercentage)
                                  .OrderByDescending(e => e.UtilizationPercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting high utilization counterparties");
                throw;
            }
        }

        public async Task<bool> ValidateRepoRateExistsAsync(int counterpartyId, int collateralTypeId, DateTime repoDate)
        {
            try
            {
                return await _repoRateRepository.RepoRateExistsAsync(counterpartyId, collateralTypeId, repoDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating repo rate existence");
                throw;
            }
        }

        public async Task<IEnumerable<TargetCircleValidationDto>> ValidateMultipleTradesAsync(
            int counterpartyId, DateTime tradeDate, IEnumerable<decimal> proposedNotionals)
        {
            var validations = new List<TargetCircleValidationDto>();
            var counterparty = await _counterpartyRepository.GetByIdAsync(counterpartyId);
            var counterpartyName = counterparty?.CounterpartyName ?? "Unknown";

            var currentExposure = await GetCurrentExposureAsync(counterpartyId, tradeDate);
            var targetCircle = await GetTargetCircleAsync(counterpartyId, tradeDate);
            var runningTotal = currentExposure;

            foreach (var notional in proposedNotionals)
            {
                runningTotal += notional;
                var validation = RepoRateMapper.ToValidationDto(counterpartyId, counterpartyName, tradeDate,
                    currentExposure, notional, targetCircle);

                // Update with running total instead of individual notional
                validation.NewTotalExposure = runningTotal;
                validation.IsWithinLimit = runningTotal <= targetCircle * 1000000;
                validation.LimitUtilizationPercentage = targetCircle > 0 ? (runningTotal / (targetCircle * 1000000)) * 100 : 0;
                validation.ValidationMessage = validation.IsWithinLimit
                    ? "Trade is within TargetCircle limit"
                    : $"Cumulative trades exceed TargetCircle limit of {targetCircle}M by {runningTotal - (targetCircle * 1000000):C}";

                validations.Add(validation);
            }

            return validations;
        }
    }
}