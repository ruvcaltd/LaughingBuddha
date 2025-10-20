using System;

namespace LAF.DataAccess.Models;

public partial class VCounterpartyExposure
{
    public int CounterpartyId { get; set; }

    public string? CounterpartyName { get; set; }

    public DateTime TradeDate { get; set; }

    public decimal? CurrentExposure { get; set; }

    public decimal? TargetCircle { get; set; }

    public decimal? AvailableLimit { get; set; }

    public decimal? UtilizationPercentage { get; set; }

    public bool? IsLimitBreached { get; set; }
}