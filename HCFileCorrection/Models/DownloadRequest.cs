
namespace HCFileCorrection.Models
{
    public class DownloadRequest
    {
        public string CountryCode { get; set; }
        public List<Request> Request { get; set; }
        public string? CreatedUser { get; set; }

        public int RetailerId { get; set; }

    }
}
