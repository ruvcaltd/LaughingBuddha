using System;

namespace LAF.Dtos
{
    public class CashAccountDto
    {
        public int Id { get; set; }
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateCashAccountDto
    {
        public int FundId { get; set; }
        public string AccountNumber { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountType { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateCashAccountDto
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class CashAccountBalanceDto
    {
        public int CashAccountId { get; set; }
        public string AccountName { get; set; }
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime AsOfDate { get; set; }
    }
}