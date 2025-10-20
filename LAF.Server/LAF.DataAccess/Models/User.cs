using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class User
{
    public int Id { get; set; }

    public string DisplayName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Cashflow> CashflowCreatedByNavigations { get; set; } = new List<Cashflow>();

    public virtual ICollection<Cashflow> CashflowModifiedByNavigations { get; set; } = new List<Cashflow>();

    public virtual ICollection<CollateralType> CollateralTypeCreatedByNavigations { get; set; } = new List<CollateralType>();

    public virtual ICollection<CollateralType> CollateralTypeModifiedByNavigations { get; set; } = new List<CollateralType>();

    public virtual ICollection<RepoTrade> RepoTradeCreatedByNavigations { get; set; } = new List<RepoTrade>();

    public virtual ICollection<RepoTrade> RepoTradeModifiedByNavigations { get; set; } = new List<RepoTrade>();

    public virtual ICollection<Security> SecurityCreatedByNavigations { get; set; } = new List<Security>();

    public virtual ICollection<Security> SecurityModifiedByNavigations { get; set; } = new List<Security>();
}
