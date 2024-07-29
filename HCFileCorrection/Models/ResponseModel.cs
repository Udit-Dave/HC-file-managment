namespace HCFileCorrection.Models
{
    public class ResponseModel
    {
        public int Id { get; set; }
        public string? FileName {  get; set; }
        public string? Tab { get; set; }
        public string? Period { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public DateTime? LastDownloaded { get; set; }
    }
}
