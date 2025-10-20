using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class VAvailableCash
{
    public int CashAccountId { get; set; }

    public string? AccountName { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public int? FundId { get; set; }

    public string? FundCode { get; set; }

    public string? FundName { get; set; }

    public decimal? AvailableBalance { get; set; }
}
