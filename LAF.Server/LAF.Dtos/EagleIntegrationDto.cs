using System;
using System.Collections.Generic;

namespace LAF.Dtos
{
    public class EagleCashBalanceDto
    {
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public decimal OpeningBalance { get; set; }
        public string Currency { get; set; }
        public DateTime BalanceDate { get; set; }
        public string Source { get; set; }
    }

    public class EagleImportRequestDto
    {
        public DateTime BalanceDate { get; set; }
        public List<EagleCashBalanceDto> CashBalances { get; set; }
        public int ImportedByUserId { get; set; }
    }

    public class EagleImportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsImported { get; set; }
        public List<string> Errors { get; set; }
        public DateTime ImportDate { get; set; }
    }

    public class EagleExportRequestDto
    {
        public DateTime ExportDate { get; set; }
        public int ExportedByUserId { get; set; }
    }

    public class EagleExportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<FundBalanceExportDto> FundBalances { get; set; }
        public DateTime ExportDate { get; set; }
        public string ExportFilePath { get; set; }
    }

    public class FundBalanceExportDto
    {
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public decimal ClosingBalance { get; set; }
        public string Currency { get; set; }
        public DateTime BalanceDate { get; set; }
        public bool IsFlat { get; set; }
    }

    public class FundFlatnessCheckDto
    {
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; }
        public bool IsFlat { get; set; }
        public decimal RequiredAdjustment { get; set; }
        public string AdjustmentType { get; set; } // "Repo", "ReverseRepo", "Deposit"
    }
}