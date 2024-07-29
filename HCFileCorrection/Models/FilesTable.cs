namespace HCFileCorrection.Models
{
    public class FilesTable
    {
        public int File_Id { get; set; }
        public string? File_Name { get; set; }
        public string? File_Path { get; set; }
        public int Request_id { get; set; }
        public bool IsDownloaded { get; set; }
    }
}
