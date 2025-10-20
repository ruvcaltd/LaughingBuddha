using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class CollateralType
{
    public short Id { get; set; }

    public string AssetType { get; set; } = null!;

    public string? CollateralType1 { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual ICollection<RepoRate> RepoRates { get; set; } = new List<RepoRate>();

    public virtual ICollection<RepoTrade> RepoTrades { get; set; } = new List<RepoTrade>();
}
