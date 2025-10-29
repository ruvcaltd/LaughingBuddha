using System;

namespace LAF.Dtos
{
    public class SecurityDto
    {
        public long Id { get; set; }
        public string Isin { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset? MaturityDate { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedByUserId { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public class CreateSecurityDto
    {
        public string Isin { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public string Currency { get; set; }
        public DateTime? MaturityDate { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateSecurityDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        public string Issuer { get; set; }
        public string Currency { get; set; }
        public DateTime? MaturityDate { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedByUserId { get; set; }
    }
}