using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LAF.DataAccess.Data;
using LAF.DataAccess.Models;
using LAF.Dtos;
using LAF.Service.Interfaces.Repositories;
using LAF.Service.Interfaces.Services;
using LAF.Services.Mappers;

namespace LAF.Services.Services
{
    public class RepoTradeService : IRepoTradeService
    {
        private readonly IRepoTradeRepository _repoTradeRepository;
        private readonly IRepoRateRepository _repoRateRepository;
        private readonly IFundRepository _fundRepository;
        private readonly ICounterpartyRepository _counterpartyRepository;
        private readonly ISecurityRepository _securityRepository;
        private readonly ICashManagementService _cashManagementService;
        private readonly ITargetCircleService _targetCircleService;
        private readonly LAFDbContext _context;
        private readonly ILogger<RepoTradeService> _logger;

        public RepoTradeService(
            IRepoTradeRepository repoTradeRepository,
            IRepoRateRepository repoRateRepository,
            IFundRepository fundRepository,
            ICounterpartyRepository counterpartyRepository,
            ISecurityRepository securityRepository,
            ICashManagementService cashManagementService,
            ITargetCircleService targetCircleService,
            LAFDbContext context,
            ILogger<RepoTradeService> logger)
        {
            _repoTradeRepository = repoTradeRepository;
            _repoRateRepository = repoRateRepository;
            _fundRepository = fundRepository;
            _counterpartyRepository = counterpartyRepository;
            _securityRepository = securityRepository;
            _cashManagementService = cashManagementService;
            _targetCircleService = targetCircleService;
            _context = context;
            _logger = logger;
        }

        public async Task<RepoTradeDto> GetByIdAsync(int id)
        {
            var trade = await _repoTradeRepository.GetByIdAsync(id);
            return RepoTradeMapper.ToDto(trade);
        }

        public async Task<IEnumerable<RepoTradeDto>> GetAllAsync()
        {
            var trades = await _repoTradeRepository.GetAllAsync();
            return RepoTradeMapper.ToDtoList(trades);
        }

        public async Task<IEnumerable<RepoTradeDto>> FindAsync(RepoTradeQueryDto query)
        {
            var trades = await _repoTradeRepository.FindAsync(trade =>
                 (!query.FundId.HasValue || trade.FundId == query.FundId.Value) &&
                 (!query.CounterpartyId.HasValue || trade.CounterpartyId == query.CounterpartyId.Value) &&
                 (!query.StartDateFrom.HasValue || trade.StartDate >= query.StartDateFrom.Value.Date) &&
                 (!query.StartDateTo.HasValue || trade.StartDate <= query.StartDateTo.Value.Date) &&
                 (!query.SettlementDate.HasValue || trade.TradeDate == query.SettlementDate.Value.Date) &&
                 (string.IsNullOrEmpty(query.Status) || trade.Status == query.Status) &&
                 (string.IsNullOrEmpty(query.Direction) || trade.Direction == query.Direction)
             );


            return RepoTradeMapper.ToDtoList(trades);
        }

        public async Task<RepoTradeDto> CreateAsync(CreateRepoTradeDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate the trade
                var validationResult = await ValidateTradeAsync(createDto);
                if (!validationResult)
                {
                    throw new InvalidOperationException("Trade validation failed");
                }

                // Check TargetCircle limits
                var targetCircleValidation = await _targetCircleService.ValidateTradeAgainstTargetCircleAsync(
                    createDto.CounterpartyId, createDto.StartDate, createDto.Notional);

                if (!targetCircleValidation.IsWithinLimit)
                {
                    throw new InvalidOperationException(targetCircleValidation.ValidationMessage);
                }

                // Create the trade
                var trade = RepoTradeMapper.ToEntity(createDto);
                trade = await _repoTradeRepository.AddAsync(trade);
                trade.Status = "Draft";

                // Create cashflow for trade settlement
                await CreateTradeSettlementCashflow(trade);

                await transaction.CommitAsync();

                _logger.LogInformation($"Repo trade created successfully: TRD{trade.Id:D6}");
                return RepoTradeMapper.ToDto(trade);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating repo trade");
                throw;
            }
        }

        public async Task<RepoTradeDto> UpdateAsync(UpdateRepoTradeDto updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTrade = await _repoTradeRepository.GetByIdAsync(updateDto.Id);
                if (existingTrade == null)
                {
                    throw new KeyNotFoundException($"Trade with ID {updateDto.Id} not found");
                }

                // Validate the update
                var validationResult = await ValidateTradeUpdateAsync(existingTrade, updateDto);
                if (!validationResult)
                {
                    throw new InvalidOperationException("Trade update validation failed");
                }

                // Update the trade
                RepoTradeMapper.UpdateEntity(existingTrade, updateDto);
                await _repoTradeRepository.UpdateAsync(existingTrade);

                await transaction.CommitAsync();

                _logger.LogInformation($"Repo trade updated successfully: TRD{existingTrade.Id:D6}");
                return RepoTradeMapper.ToDto(existingTrade);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating repo trade");
                throw;
            }
        }

        public async Task DeleteAsync(int id, int deletedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trade = await _repoTradeRepository.GetByIdAsync(id);
                if (trade == null)
                {
                    throw new KeyNotFoundException($"Trade with ID {id} not found");
                }

                if (trade.Status == "Settled")
                {
                    throw new InvalidOperationException("Cannot delete settled trades");
                }

                // Reverse any cashflows
                await ReverseTradeCashflows(trade.Id, deletedByUserId);

                await _repoTradeRepository.DeleteAsync(id);
                await transaction.CommitAsync();

                _logger.LogInformation($"Repo trade deleted successfully: TRD{trade.Id:D6}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting repo trade");
                throw;
            }
        }

        public async Task<IEnumerable<RepoTradeDto>> GetTradesByFundAsync(int fundId)
        {
            var trades = await _repoTradeRepository.FindAsync(trade => trade.FundId == fundId);
            return RepoTradeMapper.ToDtoList(trades);
        }

        public async Task<IEnumerable<RepoTradeDto>> GetActiveTradesAsync(DateTime asOfDate)
        {
            var trades = await _repoTradeRepository.GetActiveTradesAsync(asOfDate);
            return RepoTradeMapper.ToDtoList(trades);
        }

        public async Task<IEnumerable<RepoTradeDto>> GetTradesBySettlementDateAsync(DateTime settlementDate)
        {
            var trades = await _repoTradeRepository.GetTradesBySettlementDateAsync(settlementDate);
            return RepoTradeMapper.ToDtoList(trades);
        }

        public async Task<bool> ValidateTradeAsync(CreateRepoTradeDto tradeDto)
        {
            // Validate fund exists and is active
            var fund = await _fundRepository.GetByIdAsync(tradeDto.FundId);
            if (fund == null || !fund.IsActive)
            {
                _logger.LogWarning($"Invalid fund ID: {tradeDto.FundId}");
                return false;
            }

            // Validate counterparty exists and is active
            var counterparty = await _counterpartyRepository.GetByIdAsync(tradeDto.CounterpartyId);
            if (counterparty == null || !counterparty.IsActive)
            {
                _logger.LogWarning($"Invalid counterparty ID: {tradeDto.CounterpartyId}");
                return false;
            }

            // Validate security exists or collateral type and counterparty is provided
            var security = await _securityRepository.GetByIdAsync(tradeDto.SecurityId);
            if (security == null && (counterparty == null && tradeDto.CollateralTypeId == default))
            {
                _logger.LogWarning($"Invalid security ID: {tradeDto.SecurityId}");
                return false;
            }

            // Validate dates
            //if (tradeDto.StartDate > tradeDto.EndDate)
            //{
            //    _logger.LogWarning("Start date cannot be after end date");
            //    return false;
            //}

            //if (tradeDto.SettlementDate < DateTime.Today)
            //{
            //    _logger.LogWarning("Settlement date cannot be in the past");
            //    return false;
            //}

            // Validate notional amount
            if (tradeDto.Notional <= 0)
            {
                _logger.LogWarning("Notional amount must be positive");
                return false;
            }

            // Validate rate
            if (tradeDto.Rate < 0)
            {
                _logger.LogWarning("Rate cannot be negative");
                return false;
            }

            // Validate direction
            if (tradeDto.Direction != "Borrow" && tradeDto.Direction != "Lend")
            {
                _logger.LogWarning($"Invalid direction: {tradeDto.Direction}");
                return false;
            }

            return true;
        }

        public async Task<RepoTradeDto> SettleTradeAsync(int tradeId, int settledByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trade = await _repoTradeRepository.GetByIdAsync(tradeId);
                if (trade == null)
                {
                    throw new KeyNotFoundException($"Trade with ID {tradeId} not found");
                }

                if (trade.Status != "Pending")
                {
                    throw new InvalidOperationException($"Trade cannot be settled from status: {trade.Status}");
                }

                trade.Status = "Settled";
                trade.ModifiedBy = settledByUserId;
                trade.ModifiedDate = DateTime.UtcNow;

                await _repoTradeRepository.UpdateAsync(trade);
                await transaction.CommitAsync();

                _logger.LogInformation($"Trade settled successfully: TRD{trade.Id:D6}");
                return RepoTradeMapper.ToDto(trade);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error settling trade");
                throw;
            }
        }

        public async Task<RepoTradeDto> MatureTradeAsync(int tradeId, int maturedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trade = await _repoTradeRepository.GetByIdAsync(tradeId);
                if (trade == null)
                {
                    throw new KeyNotFoundException($"Trade with ID {tradeId} not found");
                }

                if (trade.Status != "Settled")
                {
                    throw new InvalidOperationException($"Trade cannot mature from status: {trade.Status}");
                }

                trade.Status = "Matured";
                trade.ModifiedBy = maturedByUserId;
                trade.ModifiedDate = DateTime.UtcNow;

                await _repoTradeRepository.UpdateAsync(trade);

                // Process maturity cashflow
                await ProcessTradeMaturity(trade, maturedByUserId);

                await transaction.CommitAsync();

                _logger.LogInformation($"Trade matured successfully: TRD{trade.Id:D6}");
                return RepoTradeMapper.ToDto(trade);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error maturing trade");
                throw;
            }
        }

        public async Task<RepoTradeDto> CancelTradeAsync(int tradeId, int cancelledByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trade = await _repoTradeRepository.GetByIdAsync(tradeId);
                if (trade == null)
                {
                    throw new KeyNotFoundException($"Trade with ID {tradeId} not found");
                }

                if (trade.Status == "Matured")
                {
                    throw new InvalidOperationException("Cannot cancel matured trades");
                }

                trade.Status = "Cancelled";
                trade.ModifiedBy = cancelledByUserId;
                trade.ModifiedDate = DateTime.UtcNow;

                await _repoTradeRepository.UpdateAsync(trade);

                // Reverse settlement cashflow if trade was settled
                if (trade.Status == "Settled")
                {
                    await ReverseTradeCashflows(trade.Id, cancelledByUserId);
                }

                await transaction.CommitAsync();

                _logger.LogInformation($"Trade cancelled successfully: TRD{trade.Id:D6}");
                return RepoTradeMapper.ToDto(trade);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling trade");
                throw;
            }
        }

        private async Task CreateTradeSettlementCashflow(RepoTrade trade)
        {
            var cashflowAmount = trade.Direction == "Lend" ? -trade.Notional : trade.Notional;

            var cashflowDto = new CreateCashflowDto
            {
                CashAccountId = trade.FundId ?? 0,
                FundId = trade.FundId ?? 0,
                RepoTradeId = trade.Id,
                Amount = cashflowAmount,
                CurrencyCode = trade.Fund?.CurrencyCode ?? "USD",
                CashflowDate = trade.TradeDate,
                Description = $"Repo trade settlement: TRD{trade.Id:D6}",
                Source = "RepoTrade",
                CreatedByUserId = trade.CreatedBy ?? 0
            };

            await _cashManagementService.CreateCashflowAsync(cashflowDto, false);
        }

        private async Task ProcessTradeMaturity(RepoTrade trade, int processedByUserId)
        {
            var startDate = trade.StartDate?.DateTime ?? trade.TradeDate;
            var endDate = trade.MaturityDate;
            var days = (endDate - startDate).Days;
            var maturityAmount = trade.Direction == "Lend"
                ? trade.Notional + (trade.Notional * trade.Rate / 100 * days / 365)
                : -(trade.Notional + (trade.Notional * trade.Rate / 100 * days / 365));

            var cashflowDto = new CreateCashflowDto
            {
                CashAccountId = trade.FundId ?? 0,
                FundId = trade.FundId ?? 0,
                RepoTradeId = trade.Id,
                Amount = maturityAmount,
                CurrencyCode = trade.Fund?.CurrencyCode ?? "USD",
                CashflowDate = endDate,
                Description = $"Repo trade maturity: TRD{trade.Id:D6}",
                Source = "RepoTrade",
                CreatedByUserId = processedByUserId
            };

            await _cashManagementService.CreateCashflowAsync(cashflowDto, false);
        }

        private async Task ReverseTradeCashflows(int tradeId, int reversedByUserId)
        {
            var cashflows = await _cashManagementService.GetCashflowsByTradeAsync(tradeId);
            foreach (var cashflow in cashflows)
            {
                var reversalDto = new CreateCashflowDto
                {
                    CashAccountId = cashflow.CashAccountId,
                    FundId = cashflow.FundId,
                    RepoTradeId = tradeId,
                    Amount = -cashflow.Amount, // Reverse the amount
                    CurrencyCode = cashflow.CurrencyCode,
                    CashflowDate = DateTime.Today,
                    Description = $"Reversal of cashflow: {cashflow.Description}",
                    Source = "Adjustment",
                    CreatedByUserId = reversedByUserId
                };

                await _cashManagementService.CreateCashflowAsync(reversalDto, false);
            }
        }

        private async Task<bool> ValidateTradeUpdateAsync(RepoTrade existingTrade, UpdateRepoTradeDto updateDto)
        {
            // Validate notional amount
            if (updateDto.Notional <= 0)
            {
                _logger.LogWarning("Notional amount must be positive");
                return false;
            }

            // Validate rate
            if (updateDto.Rate < 0)
            {
                _logger.LogWarning("Rate cannot be negative");
                return false;
            }

            // Validate dates
            if (updateDto.StartDate > updateDto.EndDate)
            {
                _logger.LogWarning("Start date cannot be after end date");
                return false;
            }

            if (updateDto.SettlementDate < DateTime.Today)
            {
                _logger.LogWarning("Settlement date cannot be in the past");
                return false;
            }

            return true;
        }
    }
}