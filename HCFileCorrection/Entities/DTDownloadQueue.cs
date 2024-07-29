using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCFileCorrection.Entities
{
    public class DTDownloadQueue
    {
        [Key]
        public int Id { get; set; }
        public string Request { get; set; }
        public string CountryCode { get; set; }
        public bool QueueCompleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public int Priority { get; set; }
        public bool QueueInProgress { get; set; }

        public string? CreatedUser { get; set; }
        public int RetailerId { get; set; }

        public ICollection<DTRequestTable> Requests { get; set; }
    }
}
