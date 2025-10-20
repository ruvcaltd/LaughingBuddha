using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class RepoTrade
{
    public int Id { get; set; }

    public DateTime TradeDate { get; set; }

    public long SecurityId { get; set; }

    public decimal Notional { get; set; }

    public decimal Rate { get; set; }

    public DateTime MaturityDate { get; set; }

    public int? CounterpartyId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public string? Direction { get; set; }

    public short? CollateralTypeId { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public string? Status { get; set; }

    public int? FundId { get; set; }

    public virtual ICollection<Cashflow> Cashflows { get; set; } = new List<Cashflow>();

    public virtual CollateralType? CollateralType { get; set; }

    public virtual Counterparty? Counterparty { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Fund? Fund { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Security Security { get; set; } = null!;
}
