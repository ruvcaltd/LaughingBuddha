using System;

namespace LAF.Dtos
{
    public class FundDto
    {
        public int Id { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateFundDto
    {
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateFundDto
    {
        public int Id { get; set; }
        public string FundName { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class FundBalanceDto
    {
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal AvailableCash { get; set; }
        public decimal? OpeningBalance { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}