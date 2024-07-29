using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCFileCorrection.Entities
{
    public class DTRequestTable
    {
        [Key]
        public int Id { get; set; }
        public string? Tab { get; set; }
        public DateTime Start_Date { get; set; }
        public string? Period { get; set; }
        public bool IsAddedToDownloadManager { get; set; }
        public int FailureCount { get; set; }
        public DateTime RequestCreatedDateTime { get; set; }
        public string CountryCode { get; set; }

        public string? AzurePath { get; set; }
        public bool AddedToQueue { get; set; }

        public int RetailerId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
        public int? QueueID { get; set; }


        /*
        public DTVendorPortalConfig VendorPortalConfig { get; set; }*/
        //[ForeignKey("CountryId")]
        //public DTHCPOSVendorPortalConfig HCPOSVendorPortalConfig { get; set; }

        [ForeignKey("QueueID")]
        public DTDownloadQueue DownloadQueue { get; set; }
        // Navigation property
        public List<DTFilesTable>? Files { get; set; }
        [ForeignKey("RetailerId")]
        public DTRetailer Retailer { get; set; }
    }
}
