using Microsoft.AspNetCore.Mvc;

namespace HCFileCorrection.Entities
{
    public class DTCountry
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string CountryDescription { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<DTHCPOSVendorPortalConfig> HCPOSVendorPortalConfigs { get; set; }
        public virtual ICollection<DTUserMapping> UserMappings { get; set; }
    }
}
