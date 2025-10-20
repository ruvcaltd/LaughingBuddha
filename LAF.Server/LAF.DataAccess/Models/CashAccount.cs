using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class CashAccount
{
    public int Id { get; set; }

    public string? AccountName { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string? OwnerType { get; set; }

    public int OwnerId { get; set; }

    public decimal? Balance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public int? FundId { get; set; }

    public virtual ICollection<Cashflow> Cashflows { get; set; } = new List<Cashflow>();

    public virtual Fund? Fund { get; set; }
}
