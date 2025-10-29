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
    }
}