using System;
using System.Collections.Generic;

namespace LAF.Dtos
{
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