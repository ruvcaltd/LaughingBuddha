using System;

namespace LAF.Dtos
{
    public class CashflowDto
    {
        public int Id { get; set; }
        public int CashAccountId { get; set; }
        public string AccountNumber { get; set; }
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public int? RepoTradeId { get; set; }
        public string TradeReference { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Description { get; set; }
        public string Source { get; set; } // "Eagle", "RepoTrade", "Manual", "Adjustment"
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateCashflowDto
    {
        public int CashAccountId { get; set; }
        public int FundId { get; set; }
        public int? RepoTradeId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime CashflowDate { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateCashflowDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class FundCashflowSummaryDto
    {
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetCashflow { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}