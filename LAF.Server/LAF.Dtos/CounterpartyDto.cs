using System;

namespace LAF.Dtos
{
    public class CounterpartyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Country { get; set; }
        public string CreditRating { get; set; }
        public string Sector { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateCounterpartyDto
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Country { get; set; }
        public string CreditRating { get; set; }
        public string Sector { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateCounterpartyDto
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Country { get; set; }
        public string CreditRating { get; set; }
        public string Sector { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class CounterpartyExposureDto
    {
        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public DateTime TradeDate { get; set; }
        public decimal CurrentExposure { get; set; }
        public decimal TargetCircle { get; set; }
        public decimal AvailableLimit { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public bool IsLimitBreached { get; set; }
    }
}