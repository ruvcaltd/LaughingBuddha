using System;

namespace LAF.Dtos
{
    public class RepoTradeDto
    {
        public int Id { get; set; }
        public string TradeReference { get; set; }
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public int SecurityId { get; set; }
        public string SecurityIsin { get; set; }
        public string SecurityName { get; set; }
        public int CollateralTypeId { get; set; }
        public string CollateralTypeName { get; set; }
        public string Direction { get; set; } // "Borrow" or "Lend"
        public decimal Notional { get; set; }
        public decimal Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public string Status { get; set; } // "Pending", "Settled", "Matured", "Cancelled"
        public string Currency { get; set; }
        public decimal? Haircut { get; set; }
        public decimal? CollateralValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public SecurityDto? Security { get; set; }
    }

    public class CreateRepoTradeDto
    {
        public int FundId { get; set; }
        public int CounterpartyId { get; set; }
        public long SecurityId { get; set; }
        public int CollateralTypeId { get; set; }
        public string Direction { get; set; }
        public decimal Notional { get; set; }
        public decimal Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public string Currency { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateRepoTradeDto
    {
        public int Id { get; set; }
        public decimal Notional { get; set; }
        public decimal Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SettlementDate { get; set; }
        public decimal? Haircut { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class RepoTradeQueryDto
    {
        public int? FundId { get; set; }
        public int? CounterpartyId { get; set; }
        public int? CollateralTypeId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string? Status { get; set; }
        public string? Direction { get; set; }
    }
}