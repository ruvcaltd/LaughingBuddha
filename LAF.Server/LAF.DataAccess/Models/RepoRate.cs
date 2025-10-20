using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class RepoRate
{
    public long Id { get; set; }

    public int CounterpartyId { get; set; }

    public DateTime EffectiveDate { get; set; }

    public decimal RepoRate1 { get; set; }

    public decimal TargetCircle { get; set; }

    public decimal FinalCircle { get; set; }

    public bool Active { get; set; }

    public short CollateralTypeId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public string? Tenor { get; set; }

    public virtual CollateralType CollateralType { get; set; } = null!;

    public virtual Counterparty Counterparty { get; set; } = null!;
}
