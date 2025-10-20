using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class Fund
{
    public int Id { get; set; }

    public string FundCode { get; set; } = null!;

    public string FundName { get; set; } = null!;

    public string CurrencyCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual ICollection<CashAccount> CashAccounts { get; set; } = new List<CashAccount>();

    public virtual ICollection<Cashflow> Cashflows { get; set; } = new List<Cashflow>();

    public virtual ICollection<RepoTrade> RepoTrades { get; set; } = new List<RepoTrade>();
}
