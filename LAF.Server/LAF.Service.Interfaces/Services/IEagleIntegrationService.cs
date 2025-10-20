using System;
using System.Threading.Tasks;
using LAF.Dtos;

namespace LAF.Service.Interfaces.Services
{
    public interface IEagleIntegrationService
    {
        Task<EagleImportResponseDto> ImportCashBalancesAsync(EagleImportRequestDto importRequest);
        Task<EagleExportResponseDto> ExportEndOfDayBalancesAsync(EagleExportRequestDto exportRequest);
        Task<bool> ProcessEagleCashBalanceAsync(string fundCode, decimal openingBalance, string currency, DateTime balanceDate, int processedByUserId);
        Task<IEnumerable<FundBalanceExportDto>> PrepareEndOfDayBalancesAsync(DateTime exportDate);
        Task<EagleExportResponseDto> GenerateFlatFundBalancesAsync(DateTime exportDate, int generatedByUserId);
        Task<bool> ValidateEagleImportDataAsync(EagleImportRequestDto importRequest);
        Task<string> GenerateEagleExportFileAsync(EagleExportResponseDto exportData);
    }
}