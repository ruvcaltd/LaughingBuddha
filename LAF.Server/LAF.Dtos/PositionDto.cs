namespace LAF.Dtos
{
    public class PositionDto
    {
        public int CollateralTypeId { get; set; }
        public string CollateralTypeName { get; set; } = string.Empty;
        public int CounterpartyId { get; set; }
        public string CounterpartyName { get; set; } = string.Empty;
        public int SecurityId { get; set; }
        public decimal Variance { get; set; }
        public decimal Rate { get; set; }
        public string SecurityName { get; set; } = string.Empty;
        public DateTime SecurityMaturityDate { get; set; }
        public Dictionary<int, decimal> FundNotionals { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> ExposurePercentages { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, string> Statuses { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> CommitStatus { get; set; } = new Dictionary<int, string>();
    }
}
