namespace HCFileCorrection.Entities
{
    public class DTRetailer
    {
        public int Id { get; set; }
        public string RetailerName { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<DTHCPOSVendorPortalConfig> HCPOSVendorPortalConfigs { get; set; }
        public virtual ICollection<DTUserMapping> UserMappings { get; set; }
    }
}
