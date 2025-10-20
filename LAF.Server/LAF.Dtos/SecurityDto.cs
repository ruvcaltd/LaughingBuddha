using System;

namespace LAF.Dtos
{
    public class SecurityDto
    {
        public int Id { get; set; }
        public string Isin { get; set; }
        public string SecurityName { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public string Currency { get; set; }
        public decimal? CouponRate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class CreateSecurityDto
    {
        public string Isin { get; set; }
        public string SecurityName { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public string Currency { get; set; }
        public decimal? CouponRate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateSecurityDto
    {
        public int Id { get; set; }
        public string SecurityName { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public decimal? CouponRate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}