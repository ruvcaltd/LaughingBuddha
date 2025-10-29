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

namespace LAF.Services.Services
{
    public class CashManagementService : ICashManagementService
    {
        private readonly ICashAccountRepository _cashAccountRepository;
        private readonly ICashflowRepository _cashflowRepository;
        private readonly IFundRepository _fundRepository;
        private readonly IRepoTradeRepository _repoTradeRepository;
        private readonly LAFDbContext _context;
        private readonly ILogger<CashManagementService> _logger;

        public CashManagementService(
            ICashAccountRepository cashAccountRepository,
            ICashflowRepository cashflowRepository,
            IFundRepository fundRepository,
            IRepoTradeRepository repoTradeRepository,
            LAFDbContext context,
            ILogger<CashManagementService> logger)
        {
            _cashAccountRepository = cashAccountRepository;
            _cashflowRepository = cashflowRepository;
            _fundRepository = fundRepository;
            _repoTradeRepository = repoTradeRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<CashAccountBalanceDto> GetCashAccountBalanceAsync(int cashAccountId, DateTime asOfDate)
        {
            try
            {
                var account = await _cashAccountRepository.GetByIdAsync(cashAccountId);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Cash account with ID {cashAccountId} not found");
                }

                var netCashflow = await GetNetCashflowForAccountAsync(cashAccountId, asOfDate);

                return new CashAccountBalanceDto
                {
                    CashAccountId = cashAccountId,
                    AccountName = account.AccountName ?? string.Empty,
                    FundId = account.FundId ?? 0,
                    FundCode = account.Fund?.FundCode,
                    FundName = account.Fund?.FundName,
                    CurrencyCode = account.CurrencyCode,
                    CurrentBalance = netCashflow,
                    AsOfDate = asOfDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cash account balance for account {CashAccountId}", cashAccountId);
                throw;
            }
        }

        public async Task<IEnumerable<CashAccountBalanceDto>> GetFundCashBalancesAsync(int fundId, DateTime asOfDate)
        {
            try
            {
                var cashAccounts = await _cashAccountRepository.FindAsync(ca => ca.FundId == fundId);
                var balances = new List<CashAccountBalanceDto>();

                foreach (var account in cashAccounts)
                {
                    var balance = await GetCashAccountBalanceAsync(account.Id, asOfDate);
                    balances.Add(balance);
                }

                return balances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cash balances for fund {FundId}", fundId);
                throw;
            }
        }

        public async Task<FundBalanceDto> GetFundBalanceAsync(int fundId, DateTime asOfDate)
        {
            try
            {
                var fund = await _fundRepository.GetByIdAsync(fundId);
                if (fund == null)
                {
                    throw new KeyNotFoundException($"Fund with ID {fundId} not found");
                }

                var cashBalances = await GetFundCashBalancesAsync(fundId, asOfDate);
                var totalBalance = cashBalances.Sum(b => b.CurrentBalance);

                return new FundBalanceDto
                {
                    FundId = fundId,
                    FundCode = fund.FundCode,
                    FundName = fund.FundName,
                    CurrencyCode = fund.CurrencyCode,
                    AvailableCash = totalBalance,
                    AsOfDate = asOfDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fund balance for fund {FundId}", fundId);
                throw;
            }
        }

        public async Task<IEnumerable<FundBalanceDto>> GetAllFundBalancesAsync(DateTime asOfDate)
        {
            try
            {
                var activeFunds = await _fundRepository.GetActiveFundsAsync();
                var balances = new List<FundBalanceDto>();

                foreach (var fund in activeFunds)
                {
                    var balance = await GetFundBalanceAsync(fund.Id, asOfDate);
                    balances.Add(balance);
                }

                return balances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fund balances");
                throw;
            }
        }

        public async Task<FundFlatnessCheckDto> CheckFundFlatnessAsync(int fundId, DateTime checkDate)
        {
            try
            {
                var fund = await _fundRepository.GetByIdAsync(fundId);
                if (fund == null)
                {
                    throw new KeyNotFoundException($"Fund with ID {fundId} not found");
                }

                var fundBalance = await GetFundBalanceAsync(fundId, checkDate);
                var isFlat = Math.Abs(fundBalance.AvailableCash) < 0.01m; // Consider flat if within 1 cent
                var requiredAdjustment = isFlat ? 0 : -fundBalance.AvailableCash;

                string adjustmentType = null;
                if (!isFlat)
                {
                    adjustmentType = fundBalance.AvailableCash > 0 ? "Repo" : "ReverseRepo";
                }

                return new FundFlatnessCheckDto
                {
                    FundId = fundId,
                    FundCode = fund.FundCode,
                    FundName = fund.FundName,
                    CurrentBalance = fundBalance.AvailableCash,
                    Currency = fund.CurrencyCode,
                    IsFlat = isFlat,
                    RequiredAdjustment = requiredAdjustment,
                    AdjustmentType = adjustmentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking fund flatness for fund {FundId}", fundId);
                throw;
            }
        }

        public async Task<IEnumerable<FundFlatnessCheckDto>> CheckAllFundsFlatnessAsync(DateTime checkDate)
        {
            try
            {
                var activeFunds = await _fundRepository.GetActiveFundsAsync();
                var flatnessChecks = new List<FundFlatnessCheckDto>();

                foreach (var fund in activeFunds)
                {
                    var check = await CheckFundFlatnessAsync(fund.Id, checkDate);
                    flatnessChecks.Add(check);
                }

                return flatnessChecks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking all funds flatness");
                throw;
            }
        }

        public async Task<CashflowDto> CreateCashflowAsync(CreateCashflowDto createDto, bool useTransaction)
        {
            // Handle null context (for testing)
            if (_context != null)
            {
                // if useTransaction is true, begin a transaction
                if (useTransaction)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var result = await CreateCashflowInternalAsync(createDto, transaction);
                        await transaction.CommitAsync();
                        return result;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    return await CreateCashflowInternalAsync(createDto, null);
                }
            }
            else
            {
                // For testing without database context
                return await CreateCashflowInternalAsync(createDto, null);
            }
        }

        private async Task<CashflowDto> CreateCashflowInternalAsync(CreateCashflowDto createDto, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction)
        {
            // Validate cash account exists
            var cashAccount = await _cashAccountRepository.GetByIdAsync(createDto.CashAccountId);
            if (cashAccount == null)
            {
                throw new KeyNotFoundException($"Cash account with ID {createDto.CashAccountId} not found");
            }

            // Validate fund exists and matches cash account
            var fund = await _fundRepository.GetByIdAsync(createDto.FundId);
            if (fund == null)
            {
                throw new KeyNotFoundException($"Fund with ID {createDto.FundId} not found");
            }

            if (cashAccount.FundId != createDto.FundId)
            {
                throw new InvalidOperationException("Cash account does not belong to specified fund");
            }

            // Validate currency matches fund currency
            if (createDto.CurrencyCode != fund.CurrencyCode)
            {
                throw new InvalidOperationException($"Cashflow currency {createDto.CurrencyCode} does not match fund currency {fund.CurrencyCode}");
            }

            // Create the cashflow
            var cashflow = new Cashflow
            {
                CashAccountId = createDto.CashAccountId,
                FundId = createDto.FundId,
                TradeId = createDto.RepoTradeId,
                Amount = createDto.Amount,
                CurrencyCode = createDto.CurrencyCode,
                CashflowDate = createDto.CashflowDate,
                Description = createDto.Description,
                CashflowType = createDto.Source,
                CreatedBy = createDto.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = createDto.CreatedByUserId,
                ModifiedAt = DateTimeOffset.UtcNow,
                SettlementDate = new DateTimeOffset(createDto.CashflowDate) // Map effective date to settlement date
            };

            cashflow = await _cashflowRepository.AddAsync(cashflow);

            // Commit transaction if we have one
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            _logger.LogInformation($"Cashflow created successfully: ID {cashflow.Id}");
            return ToCashflowDto(cashflow);
        }

        public async Task<IEnumerable<CashflowDto>> GetCashflowsByAccountAsync(int cashAccountId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var cashflows = await _cashflowRepository.GetCashflowsByAccountAsync(cashAccountId);

                if (fromDate.HasValue)
                {
                    cashflows = cashflows.Where(cf => cf.CashflowDate.DateTime >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    cashflows = cashflows.Where(cf => cf.CashflowDate.DateTime <= toDate.Value);
                }

                return ToCashflowDtoList(cashflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cashflows for account {CashAccountId}", cashAccountId);
                throw;
            }
        }

        public async Task<IEnumerable<CashflowDto>> GetCashflowsByFundAsync(int fundId, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var cashflows = await _cashflowRepository.GetCashflowsByFundAsync(fundId);

                if (fromDate.HasValue)
                {
                    cashflows = cashflows.Where(cf => cf.CashflowDate.DateTime >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    cashflows = cashflows.Where(cf => cf.CashflowDate.DateTime <= toDate.Value);
                }

                return ToCashflowDtoList(cashflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cashflows for fund {FundId}", fundId);
                throw;
            }
        }

        public async Task<FundCashflowSummaryDto> GetFundCashflowSummaryAsync(int fundId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fund = await _fundRepository.GetByIdAsync(fundId);
                if (fund == null)
                {
                    throw new KeyNotFoundException($"Fund with ID {fundId} not found");
                }

                var cashflows = await GetCashflowsByFundAsync(fundId, fromDate, toDate);

                var totalInflows = cashflows.Where(cf => cf.Amount > 0).Sum(cf => cf.Amount);
                var totalOutflows = cashflows.Where(cf => cf.Amount < 0).Sum(cf => cf.Amount);

                return new FundCashflowSummaryDto
                {
                    FundId = fundId,
                    FundCode = fund.FundCode,
                    FundName = fund.FundName,
                    CurrencyCode = fund.CurrencyCode,
                    TotalInflows = totalInflows,
                    TotalOutflows = Math.Abs(totalOutflows),
                    NetCashflow = totalInflows + totalOutflows,
                    DateFrom = fromDate,
                    DateTo = toDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fund cashflow summary for fund {FundId}", fundId);
                throw;
            }
        }

        public async Task<bool> ProcessTradeSettlementAsync(int tradeId, int processedByUserId)
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
                    throw new InvalidOperationException($"Trade is not in Pending status: {trade.Status}");
                }

                // Get the fund's cash account
                var cashAccount = await _cashAccountRepository.GetByFundIdAsync(trade.FundId ?? 0);
                if (cashAccount == null)
                {
                    throw new KeyNotFoundException($"No cash account found for fund {trade.FundId}");
                }

                // Create settlement cashflow
                var settlementAmount = trade.Direction == "Lend" ? -trade.Notional : trade.Notional;

                var cashflowDto = new CreateCashflowDto
                {
                    CashAccountId = cashAccount.Id,
                    FundId = trade.FundId ?? 0,
                    RepoTradeId = trade.Id,
                    Amount = settlementAmount,
                    CurrencyCode = cashAccount.CurrencyCode,
                    CashflowDate = trade.TradeDate,
                    Description = $"Repo trade settlement: Trade {trade.Id}",
                    Source = "RepoTrade",
                    CreatedByUserId = processedByUserId
                };

                await CreateCashflowAsync(cashflowDto, false);

                // Update trade status to Settled
                trade.Status = "Settled";
                trade.ModifiedBy = processedByUserId;
                trade.ModifiedAt = DateTimeOffset.UtcNow;
                await _repoTradeRepository.UpdateAsync(trade);

                await transaction.CommitAsync();

                _logger.LogInformation($"Trade settlement processed successfully: Trade {trade.Id}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing trade settlement for trade {TradeId}", tradeId);
                throw;
            }
        }

        public async Task<bool> ProcessTradeMaturityAsync(int tradeId, int processedByUserId)
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
                    throw new InvalidOperationException($"Trade is not in Settled status: {trade.Status}");
                }

                // Get the fund's cash account
                var cashAccount = await _cashAccountRepository.GetByFundIdAsync(trade.FundId ?? 0);
                if (cashAccount == null)
                {
                    throw new KeyNotFoundException($"No cash account found for fund {trade.FundId}");
                }

                // Calculate maturity amount (principal + interest)
                var days = (trade.MaturityDate.Date - (trade.StartDate?.Date ?? trade.TradeDate.Date)).Days;
                var interest = trade.Notional * trade.Rate / 100 * days / 365;
                var maturityAmount = trade.Direction == "Lend"
                    ? trade.Notional + interest
                    : -(trade.Notional + interest);

                // Create maturity cashflow
                var cashflowDto = new CreateCashflowDto
                {
                    CashAccountId = cashAccount.Id,
                    FundId = trade.FundId ?? 0,
                    RepoTradeId = trade.Id,
                    Amount = maturityAmount,
                    CurrencyCode = cashAccount.CurrencyCode,
                    CashflowDate = trade.MaturityDate.Date,
                    Description = $"Repo trade maturity: Trade {trade.Id}",
                    Source = "RepoTrade",
                    CreatedByUserId = processedByUserId
                };

                await CreateCashflowAsync(cashflowDto, false);

                // Update trade status to Matured
                trade.Status = "Matured";
                trade.ModifiedBy = processedByUserId;
                trade.ModifiedAt = DateTimeOffset.UtcNow;
                await _repoTradeRepository.UpdateAsync(trade);

                await transaction.CommitAsync();

                _logger.LogInformation($"Trade maturity processed successfully: Trade {trade.Id}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing trade maturity for trade {TradeId}", tradeId);
                throw;
            }
        }

        public async Task<bool> EnsureFundFlatnessAsync(int fundId, DateTime date, int processedByUserId)
        {
            try
            {
                var flatnessCheck = await CheckFundFlatnessAsync(fundId, date);

                if (flatnessCheck.IsFlat)
                {
                    _logger.LogInformation($"Fund {fundId} is already flat");
                    return true;
                }

                // Get the fund's cash account
                var cashAccount = await _cashAccountRepository.GetByFundIdAsync(fundId);
                if (cashAccount == null)
                {
                    throw new KeyNotFoundException($"No cash account found for fund {fundId}");
                }

                // Create adjustment cashflow to make fund flat
                var adjustmentCashflow = new CreateCashflowDto
                {
                    CashAccountId = cashAccount.Id,
                    FundId = fundId,
                    RepoTradeId = null,
                    Amount = flatnessCheck.RequiredAdjustment,
                    CurrencyCode = cashAccount.CurrencyCode,
                    CashflowDate = date,
                    Description = $"End-of-day flatness adjustment for fund {flatnessCheck.FundCode}",
                    Source = "Adjustment",
                    CreatedByUserId = processedByUserId
                };

                await CreateCashflowAsync(adjustmentCashflow, false);

                _logger.LogInformation($"Fund {fundId} flatness ensured with adjustment of {flatnessCheck.RequiredAdjustment:C}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring fund flatness for fund {FundId}", fundId);
                throw;
            }
        }

        // Private helper methods
        private async Task<decimal> GetNetCashflowForAccountAsync(int cashAccountId, DateTime asOfDate)
        {
            return await _cashflowRepository.GetNetCashflowByAccountAsync(cashAccountId, asOfDate);
        }

        public async Task<IEnumerable<CashflowDto>> GetCashflowsByTradeAsync(int repoTradeId)
        {
            var cashflows = await _cashflowRepository.FindAsync(cf => cf.TradeId == repoTradeId);
            return ToCashflowDtoList(cashflows);
        }

        private CashflowDto ToCashflowDto(Cashflow entity)
        {
            if (entity == null) return null;

            return new CashflowDto
            {
                Id = entity.Id,
                CashAccountId = entity.CashAccountId,
                AccountNumber = entity.CashAccount?.AccountName ?? string.Empty,
                FundId = entity.FundId ?? 0,
                FundCode = entity.Fund?.FundCode,
                FundName = entity.Fund?.FundName,
                RepoTradeId = entity.TradeId,
                TradeReference = entity.Trade?.Id.ToString() ?? string.Empty,
                Amount = entity.Amount,
                CurrencyCode = entity.CurrencyCode,
                EffectiveDate = entity.CashflowDate.DateTime,
                Description = entity.Description ?? string.Empty,
                Source = entity.CashflowType ?? string.Empty,
                CreatedDate = entity.CreatedAt ?? DateTime.MinValue,
                CreatedBy = entity.CreatedBy?.ToString(),
                ModifiedDate = entity.ModifiedAt?.DateTime,
                ModifiedBy = entity.ModifiedBy?.ToString()
            };
        }

        private List<CashflowDto> ToCashflowDtoList(IEnumerable<Cashflow> entities)
        {
            return entities?.Select(ToCashflowDto).ToList() ?? new List<CashflowDto>();
        }
    }
}