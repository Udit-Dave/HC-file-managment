using HCFileCorrection.Entities;
using HCFileCorrection.Models;

namespace HCFileCorrection.Interfaces
{
    public interface IHCRepository
    {
        DTHCPOSVendorPortalConfig GetVendorPortalConfig(string countryCode);
        IEnumerable<DTRequestTable> GetCurrentDateRequests(DateTime startDate,string countryCode, int id);
        IEnumerable<DTRequestTable> GetRequestsToQueue(DateTime startDate, string countryCode,int id);
        IEnumerable<DTRequestTable> GetRequestsToDownload(DownloadRequest requests);
        IEnumerable<DTFilesTable> GetFilesToDownload(string countryCode);
        void InsertRequestsDaily(string countryCode,int id);
        void InsertRequestsWeekly(DateTime startDate, string countryCode,int id);
        void InsertFile(DTFilesTable file);
        List<DTFilesTable> GetFiles(string countryCode,string? tab, int? Retailerid);
        int InsertDownloadQueueItem(DownloadRequest requests,int prority);
        void UpdateQueueInProgress(int id, bool newStatus);
        DTDownloadQueue GetRequestsInQueue();
        int GetQueuesInProgress();
        DTDownloadQueue GetQueueById(int Id);
        void UpdateAddedToQueue(List<DTRequestTable> requests,int q);
        void SaveChanges();
    }
}
