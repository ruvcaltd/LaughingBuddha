using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class VFundBalance
{
    public int FundId { get; set; }

    public string FundCode { get; set; } = null!;

    public string FundName { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public decimal AvailableBalance { get; set; }
}
