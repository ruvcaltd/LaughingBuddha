namespace LAF.Dtos
{
    /// <summary>
    /// Data transfer object for position cell editing status
    /// </summary>
    public class PositionLockDto
    {
        /// <summary>
        /// The ID of the counterparty
        /// </summary>
        public int CounterpartyId { get; set; }

        /// <summary>
        /// The ID of the collateral type
        /// </summary>
        public int CollateralTypeId { get; set; }

        /// <summary>
        /// The ID of the fund
        /// </summary>
        public int FundId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is locked.
        /// </summary>
        public bool Locked { get; set; } = true;

        /// <summary>
        /// User who has locked the position
        /// </summary>
        public string? UserDisplay { get; set; }
    }
}