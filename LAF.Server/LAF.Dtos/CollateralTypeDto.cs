using System;

namespace LAF.Dtos
{
    public class CollateralTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? StandardHaircut { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateCollateralTypeDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? StandardHaircut { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateCollateralTypeDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal? StandardHaircut { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}