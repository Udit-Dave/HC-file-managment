using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class DTDownloadQueue
    {
        [Key]
        public int Id { get; set; }
        public string Request { get; set; }
        public string CountryCode { get; set; }
        public bool QueueCompleted { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int Priority { get; set; }
        public bool QueueInProgress { get; set; }
    }
}
