using System;

namespace LAF.Dtos
{
    public class RepoRateDto
    {
        public int Id { get; set; }
        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public int CollateralTypeId { get; set; }
        public string CollateralTypeName { get; set; }
        public DateTime RepoDate { get; set; }
        public decimal RepoRate { get; set; }
        public decimal TargetCircle { get; set; }
        public decimal? FinalCircle { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateRepoRateDto
    {
        public int CounterpartyId { get; set; }
        public int CollateralTypeId { get; set; }
        public DateTime RepoDate { get; set; }
        public decimal RepoRate { get; set; }
        public decimal TargetCircle { get; set; }
        public decimal? FinalCircle { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateRepoRateDto
    {
        public int Id { get; set; }
        public decimal RepoRate { get; set; }
        public decimal TargetCircle { get; set; }
        public decimal? FinalCircle { get; set; }
        public int ModifiedByUserId { get; set; }
    }

    public class TargetCircleValidationDto
    {
        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; }
        public DateTime TradeDate { get; set; }
        public decimal CurrentExposure { get; set; }
        public decimal ProposedNotional { get; set; }
        public decimal TargetCircle { get; set; }
        public decimal NewTotalExposure { get; set; }
        public bool IsWithinLimit { get; set; }
        public decimal LimitUtilizationPercentage { get; set; }
        public string ValidationMessage { get; set; }
    }
}