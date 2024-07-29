namespace HCFileCorrection.Entities
{
    public class DTHCPOSVendorPortalConfig
    {
        public int Id { get; set; }
        public int? CountryId { get; set; }
        public string? CountryCode { get; set; }
        public int? RetailerId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OtpString { get; set; }
        public string VendorPortalLink { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
        public bool? PublisherTabs { get; set; }

        public virtual DTCountry Country { get; set; }
        public  virtual DTRetailer Retailer { get; set; }
    }
}
