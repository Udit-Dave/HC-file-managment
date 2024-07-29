using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCFileCorrection.Entities
{
    public class DTFilesTable
    {
        [Key]
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public int RequestId { get; set; } // Renamed to FileRequest_Id
        public bool IsDownloaded { get; set; }

        //public string CountryCode { get; set; }
       

        // Navigation property
        [ForeignKey("RequestId")]
        public DTRequestTable? Request { get; set; }
        
    }
}
