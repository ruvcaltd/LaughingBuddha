using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class Cashflow
{
    public int Id { get; set; }

    public int CashAccountId { get; set; }

    public int? TradeId { get; set; }

    public DateTimeOffset CashflowDate { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string? CashflowType { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public DateTimeOffset? SettlementDate { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public int? FundId { get; set; }

    public virtual CashAccount CashAccount { get; set; } = null!;

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Fund? Fund { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual RepoTrade? Trade { get; set; }
}
