using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class Security
{
    public long Id { get; set; }

    public string Isin { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string AssetType { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public DateTimeOffset? MaturityDate { get; set; }

    public decimal? Coupon { get; set; }

    public string? IssuerType { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<RepoTrade> RepoTrades { get; set; } = new List<RepoTrade>();
}
