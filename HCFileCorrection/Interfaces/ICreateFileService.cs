using HCFileCorrection.Entities;
using HCFileCorrection.Models;
using OpenQA.Selenium;
using System;

namespace HCFileCorrection.Interfaces
{
    public interface ICreateFileService
    {
        DTHCPOSVendorPortalConfig GetVendorPortalConfig(string countryCode);
        List<DTRequestTable> InsertRequests(string countryCode,int id);
        List<DTRequestTable> GetRequests(DownloadRequest requests);
        Task RunSeleniumJob1Async(List<DTRequestTable> data, IWebDriver drive, DTHCPOSVendorPortalConfig congfigDetails);
        List<DTFilesTable> GetFilesToDownload(string countryCode);
        List<DTFilesTable> GetFiles(string countryCode,string? tab,int? Retailerid);
        Task RunSeleniumJob2Async( IWebDriver drive, DTHCPOSVendorPortalConfig congfigDetails,string downloadDirectory);
        Task Login(IWebDriver driver, DTHCPOSVendorPortalConfig configDetails);
        int InsertDownloadQueueItem(DownloadRequest requests,int priority = 0);
        void UpdateQueueInProgress(int id, bool newStatus);
        void UpdateAddedToQueue(List<DTRequestTable> requests,int queueId);

    }
}
