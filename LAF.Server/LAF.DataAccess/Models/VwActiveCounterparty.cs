using System;
using System.Collections.Generic;

namespace LAF.DataAccess.Models;

public partial class VwActiveCounterparty
{
    public int Id { get; set; }

    public string CounterpartyCode { get; set; } = null!;

    public string CounterpartyName { get; set; } = null!;

    public string CounterpartyType { get; set; } = null!;

    public string? LegalEntityIdentifier { get; set; }

    public string CountryCode { get; set; } = null!;

    public string? Region { get; set; }

    public string? CreditRating { get; set; }

    public decimal? CreditLimit { get; set; }
}
