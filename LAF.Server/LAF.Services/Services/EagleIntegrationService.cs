using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LAF.DataAccess.Data;
using LAF.DataAccess.Models;
using LAF.Dtos;
using LAF.Service.Interfaces.Services;
using LAF.Service.Interfaces.Repositories;

namespace LAF.Services.Services
{
    public class EagleIntegrationService : IEagleIntegrationService
    {
        private readonly ICashManagementService _cashManagementService;
        private readonly IFundRepository _fundRepository;
        private readonly ICashflowRepository _cashflowRepository;
        private readonly ICashAccountRepository _cashAccountRepository;
        private readonly LAFDbContext _context;
        private readonly ILogger<EagleIntegrationService> _logger;
        private readonly IConfiguration _configuration;

        public EagleIntegrationService(
            ICashManagementService cashManagementService,
            IFundRepository fundRepository,
            ICashflowRepository cashflowRepository,
            ICashAccountRepository cashAccountRepository,
            LAFDbContext context,
            ILogger<EagleIntegrationService> logger,
            IConfiguration configuration)
        {
            _cashManagementService = cashManagementService;
            _fundRepository = fundRepository;
            _cashflowRepository = cashflowRepository;
            _cashAccountRepository = cashAccountRepository;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<EagleImportResponseDto> ImportCashBalancesAsync(EagleImportRequestDto importRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Starting Eagle cash balance import for {BalanceDate}", importRequest.BalanceDate);

                // Validate import data
                var validationResult = await ValidateEagleImportDataAsync(importRequest);
                if (!validationResult)
                {
                    throw new InvalidOperationException("Eagle import data validation failed");
                }

                var response = new EagleImportResponseDto
                {
                    Success = true,
                    Message = "Import completed successfully",
                    ImportDate = DateTime.UtcNow,
                    RecordsProcessed = 0,
                    RecordsImported = 0,
                    Errors = new List<string>()
                };

                foreach (var balance in importRequest.CashBalances)
                {
                    response.RecordsProcessed++;

                    try
                    {
                        var success = await ProcessEagleCashBalanceAsync(
                            balance.FundCode,
                            balance.OpeningBalance,
                            balance.Currency,
                            importRequest.BalanceDate,
                            importRequest.ImportedByUserId);

                        if (success)
                        {
                            response.RecordsImported++;
                        }
                        else
                        {
                            response.Errors.Add($"Failed to process balance for fund {balance.FundCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing Eagle balance for fund {FundCode}", balance.FundCode);
                        response.Errors.Add($"Error processing fund {balance.FundCode}: {ex.Message}");
                    }
                }

                response.Success = response.Errors.Count == 0;
                response.Message = response.Success
                    ? $"Successfully imported {response.RecordsImported} cash balances"
                    : $"Imported {response.RecordsImported} of {response.RecordsProcessed} cash balances with {response.Errors.Count} errors";

                await transaction.CommitAsync();

                _logger.LogInformation("Eagle cash balance import completed: {Message}", response.Message);
                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during Eagle cash balance import");
                throw;
            }
        }

        public async Task<EagleExportResponseDto> ExportEndOfDayBalancesAsync(EagleExportRequestDto exportRequest)
        {
            try
            {
                _logger.LogInformation("Starting Eagle end-of-day balance export for {ExportDate}", exportRequest.ExportDate);

                var response = new EagleExportResponseDto
                {
                    ExportDate = exportRequest.ExportDate,
                    Success = true,
                    FundBalances = new List<FundBalanceExportDto>()
                };

                // Prepare end-of-day balances
                var fundBalances = await PrepareEndOfDayBalancesAsync(exportRequest.ExportDate);
                response.FundBalances = fundBalances.ToList();

                // Generate export file
                var exportFilePath = await GenerateEagleExportFileAsync(response);
                response.ExportFilePath = exportFilePath;

                response.Message = $"Successfully exported {response.FundBalances.Count} fund balances to {exportFilePath}";

                _logger.LogInformation("Eagle end-of-day balance export completed: {Message}", response.Message);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Eagle end-of-day balance export");
                throw;
            }
        }

        public async Task<bool> ProcessEagleCashBalanceAsync(string fundCode, decimal openingBalance, string currency, DateTime balanceDate, int processedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the fund by fund code
                var fund = await _fundRepository.GetByFundCodeAsync(fundCode);
                if (fund == null)
                {
                    _logger.LogWarning("Fund with code {FundCode} not found", fundCode);
                    return false;
                }

                // Validate currency matches fund currency
                if (fund.CurrencyCode != currency)
                {
                    _logger.LogWarning("Currency mismatch for fund {FundCode}: expected {ExpectedCurrency}, got {ActualCurrency}",
                        fundCode, fund.CurrencyCode, currency);
                    return false;
                }

                // Get or create cash account for the fund
                var cashAccount = await _cashAccountRepository.GetByFundIdAsync(fund.Id);
                if (cashAccount == null)
                {
                    // Create a new cash account for the fund
                    var createAccountDto = new CreateCashAccountDto
                    {
                        FundId = fund.Id,
                        AccountNumber = $"{fund.FundCode}_CASH",
                        CurrencyCode = fund.CurrencyCode,
                        AccountType = "Operating",
                        IsActive = true,
                        CreatedByUserId = processedByUserId
                    };

                    // Note: In a real implementation, we would have a service method to create cash accounts
                    // For now, we'll assume the cash account exists
                    _logger.LogWarning("Cash account not found for fund {FundCode}, needs to be created", fundCode);
                    return false;
                }

                // Check if an opening balance cashflow already exists for this date
                var existingCashflows = await _cashflowRepository.FindAsync(cf =>
                    cf.FundId == fund.Id &&
                    cf.CashflowDate.DateTime == balanceDate &&
                    cf.CashflowType == "Eagle");

                if (existingCashflows.Any())
                {
                    _logger.LogInformation("Opening balance already exists for fund {FundCode} on {BalanceDate}", fundCode, balanceDate);
                    return true;
                }

                // Create opening balance cashflow
                var openingBalanceCashflow = new CreateCashflowDto
                {
                    CashAccountId = cashAccount.Id,
                    FundId = fund.Id,
                    RepoTradeId = null,
                    Amount = openingBalance,
                    CurrencyCode = currency,
                    CashflowDate = balanceDate,
                    Description = $"Opening balance from Eagle: {fundCode}",
                    Source = "Eagle",
                    CreatedByUserId = processedByUserId
                };

                await _cashManagementService.CreateCashflowAsync(openingBalanceCashflow, false);

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully processed Eagle opening balance for fund {FundCode}: {OpeningBalance:C}",
                    fundCode, openingBalance);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing Eagle cash balance for fund {FundCode}", fundCode);
                throw;
            }
        }

        public async Task<IEnumerable<FundBalanceExportDto>> PrepareEndOfDayBalancesAsync(DateTime exportDate)
        {
            try
            {
                var fundBalances = new List<FundBalanceExportDto>();
                var activeFunds = await _fundRepository.GetActiveFundsAsync();

                foreach (var fund in activeFunds)
                {
                    try
                    {
                        var fundBalance = await _cashManagementService.GetFundBalanceAsync(fund.Id, exportDate);
                        var isFlat = Math.Abs(fundBalance.AvailableCash) < 0.01m; // Consider flat if within 1 cent

                        fundBalances.Add(new FundBalanceExportDto
                        {
                            FundCode = fund.FundCode,
                            FundName = fund.FundName,
                            ClosingBalance = fundBalance.AvailableCash,
                            Currency = fund.CurrencyCode,
                            BalanceDate = exportDate,
                            IsFlat = isFlat
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error preparing balance for fund {FundCode}", fund.FundCode);
                        // Continue with other funds
                    }
                }

                return fundBalances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing end-of-day balances");
                throw;
            }
        }

        public async Task<EagleExportResponseDto> GenerateFlatFundBalancesAsync(DateTime exportDate, int generatedByUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // First, ensure all funds are flat
                var activeFunds = await _fundRepository.GetActiveFundsAsync();
                foreach (var fund in activeFunds)
                {
                    await _cashManagementService.EnsureFundFlatnessAsync(fund.Id, exportDate, generatedByUserId);
                }

                // Then prepare the export data
                var exportResponse = await ExportEndOfDayBalancesAsync(new EagleExportRequestDto
                {
                    ExportDate = exportDate,
                    ExportedByUserId = generatedByUserId
                });

                await transaction.CommitAsync();

                _logger.LogInformation("Successfully generated flat fund balances for export");
                return exportResponse;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error generating flat fund balances");
                throw;
            }
        }

        public async Task<bool> ValidateEagleImportDataAsync(EagleImportRequestDto importRequest)
        {
            try
            {
                if (importRequest.CashBalances == null || !importRequest.CashBalances.Any())
                {
                    _logger.LogWarning("No cash balances provided in Eagle import request");
                    return false;
                }

                if (importRequest.BalanceDate == default)
                {
                    _logger.LogWarning("Invalid balance date in Eagle import request");
                    return false;
                }

                foreach (var balance in importRequest.CashBalances)
                {
                    if (string.IsNullOrWhiteSpace(balance.FundCode))
                    {
                        _logger.LogWarning("Empty fund code in Eagle import data");
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(balance.Currency))
                    {
                        _logger.LogWarning("Empty currency for fund {FundCode} in Eagle import data", balance.FundCode);
                        return false;
                    }

                    if (balance.OpeningBalance < 0)
                    {
                        _logger.LogWarning("Negative opening balance for fund {FundCode} in Eagle import data", balance.FundCode);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Eagle import data");
                return false;
            }
        }

        public async Task<string> GenerateEagleExportFileAsync(EagleExportResponseDto exportData)
        {
            try
            {
                var exportFolder = _configuration["Eagle:ExportFolder"] ?? "EagleExports";
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"LAF_EndOfDay_Balances_{timestamp}.json";
                var filePath = Path.Combine(exportFolder, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(exportFolder);

                // Serialize the export data
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(exportData, jsonOptions);
                await File.WriteAllTextAsync(filePath, jsonContent);

                _logger.LogInformation("Eagle export file generated: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Eagle export file");
                throw;
            }
        }
    }
}