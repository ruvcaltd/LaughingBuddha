using System;
using System.Collections.Generic;

namespace LAF.Dtos
{
    public class FundAccountCashflowsDto
    {
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public string FundName { get; set; }
        public List<AccountCashflowsDto> Accounts { get; set; } = new List<AccountCashflowsDto>();
    }

    public class AccountCashflowsDto
    {
        public int CashAccountId { get; set; }
        public string AccountName { get; set; }
        public string CurrencyCode { get; set; }
        public List<CashflowDto> Cashflows { get; set; } = new List<CashflowDto>();
    }
}