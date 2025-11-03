using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAF.Dtos
{
    public class PositionChangeDto
    {
        public int CollateralTypeId { get; set; }
        public int CounterpartyId { get; set; }
        public int SecurityId { get; set; }
        public DateTime SecurityMaturityDate { get; set; }
        public int FundId { get; set; }
        public decimal NewNotionalAmount { get; set; }
        public decimal NewVariance { get; set; }
        public string Status { get; set; } = "Pending"; // or "Success" or "Failed"

        public string? ErrorMessage { get; set; }
    }
}
