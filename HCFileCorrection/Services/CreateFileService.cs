using Azure.Storage.Blobs;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Selenium;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;

using OpenQA.Selenium.DevTools.V120.Database;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
using HCFileCorrection.Entities;
using Microsoft.Extensions.Azure;

namespace HCFileCorrection.Services
{
    public class CreateFileService : ICreateFileService
    {
        private readonly FileDownloadJob _filedownloadjob;
        private readonly Downloadinlocalstorage _downloadinlclstorage;
        private readonly IHCRepository _repository;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly FileDbContext _FileContext;
        private readonly ILogger _logger;

        public CreateFileService(FileDownloadJob fileDownloadJob, Downloadinlocalstorage dwnldinlclstrg, IHCRepository repository, IConfiguration configuration, FileDbContext fileContext,EmailSender email, ILogger logger)
        {           
            _filedownloadjob = fileDownloadJob;
            _downloadinlclstorage = dwnldinlclstrg;
            _repository = repository;
            _configuration = configuration;
            _FileContext = fileContext;
            _emailSender = email;
            _logger = logger;

        }

        public DTHCPOSVendorPortalConfig GetVendorPortalConfig(string countryCode)
        {
            try
            {
                var configDetails = _repository.GetVendorPortalConfig(countryCode);
                return configDetails;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public List<DTRequestTable> InsertRequests(string countryCode,int id)
        {
            try
            {
                var requests = _repository.GetCurrentDateRequests(DateTime.Now, countryCode,id);
                if (requests.Count() == 0 || requests == null)
                {
                    _repository.InsertRequestsDaily(countryCode,id);
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Tuesday || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                    {
                        DateTime mostRecentSaturday = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1);
                        _repository.InsertRequestsWeekly(mostRecentSaturday,countryCode,id);
                    }
                    requests = _repository.GetCurrentDateRequests(DateTime.Now, countryCode,id);
                    return requests.ToList();
                }
                else
                {
                    return requests.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public List<DTRequestTable> GetRequests(DownloadRequest requests)
        {
            try
            {                
                var requestsList = _repository.GetRequestsToDownload(requests);
                return requestsList.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }


        public async Task Login(IWebDriver driver, DTHCPOSVendorPortalConfig configDetails)
        {
            try
            {
                _filedownloadjob.NavigateToVendorCentral(driver,configDetails);
                
            }
            catch(Exception ex) 
            {
                throw new Exception();
            }
        }
        public async Task RunSeleniumJob1Async(List<DTRequestTable> data, IWebDriver driver, DTHCPOSVendorPortalConfig configDetails)
        {
            foreach (var row in data)
            {
                if (row.IsAddedToDownloadManager == false)
                {
                    
                    bool flag = _filedownloadjob.NavigateToTab(driver, row.Tab.ToLower(), row.Period, row.Start_Date, configDetails);
                    _logger.Information($"Changed the flag to {flag} in Request table for Id : {row.Id}");
                    row.IsAddedToDownloadManager = flag;
                    
                    if(flag == false)
                    {
                        row.FailureCount++;
                        if (row.FailureCount >= 3)
                        {
                            _logger.Information($"Sending the email about download failure for the Country : {configDetails.Country} Tab : {row.Tab}, Period : {row.Period} and Date : {row.Start_Date}  ");
                            await _emailSender.SendEmail( "Requested file failed to download",
                                $"Requested file for country : {configDetails.Country}, Tab : {row.Tab}, Period : {row.Period} and Date : {row.Start_Date.Date} is failed to download");
                        }
                    }
                    _FileContext.Entry(row).State = EntityState.Modified;
                    await _FileContext.SaveChangesAsync();

                    string fileName = "";


                    DTFilesTable file = new DTFilesTable();

                    switch (row.Period)
                    {
                        case "Daily":
                            switch (row.Tab)
                            {
                                case "Sales":
                                    fileName = "Sales_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                    break;
                                case "Traffic":
                                    fileName = "Traffic_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                    break;
                                case "PublisherReportAlc":
                                    fileName = "Publisher_Report_Alc_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                    break;
                                case "PublisherReportSubscription":
                                    fileName = "Publisher_Report_Subscription_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "Weekly":
                            switch (row.Tab)
                            {
                                case "Sales":
                                    fileName = "Sales_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + ".csv";

                                    break;
                                case "Inventory":
                                    fileName = "Inventory_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + ".csv";

                                    break;
                                case "Traffic":
                                    fileName = "Traffic_" + configDetails.Country.CountryName + "_" + row.Period + "_" + row.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + ".csv";

                                    break;
                                case "Catalogue":
                                    fileName = "Catalogue_Manufacturing_" + configDetails.Country.CountryName + ".csv";
                                    break;

                                case "Forecast - Mean":
                                    fileName = "Forecasting_Retail_Meanforecast_" + configDetails.Country.CountryName + ".csv";

                                    break;
                                case "Forecast - P70":
                                    fileName = "Forecasting_Retail_P70Forecast_" + configDetails.Country.CountryName + ".csv";

                                    break;
                                case "Forecast - P80":
                                    fileName = "Forecasting_Retail_P80Forecast_" + configDetails.Country.CountryName + ".csv";

                                    break;
                                case "Forecast - P90":
                                    fileName = "Forecasting_Retail_P90Forecast_" + configDetails.Country.CountryName + ".csv";

                                    break;
                                case "RepeatPurchaseBehaviour-Asin":
                                    fileName = configDetails.CountryCode + "_Repeat_Purchase_Behaviour_ASIN_view_Simple_Week" + "_" + row.Start_Date.ToString("yyyy_MM_dd") + ".csv";

                                    break;
                                case "TopSearchTerms":
                                    fileName = configDetails.CountryCode + "_Top_Search_Terms_Simple_Week" + "_" + row.Start_Date.ToString("yyyy_MM_dd") + ".csv";

                                    break;
                                case "MarketBasketAnalysis":
                                    fileName = configDetails.CountryCode + "_Market_Basket_Analysis_Simple_Week" + "_" + row.Start_Date.ToString("yyyy_MM_dd") + ".csv";

                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                    var existingFile = _FileContext.HCPOSDownloadFiles.FirstOrDefault(f => f.RequestId == row.Id);
                    if (existingFile != null)
                    {
                        existingFile.IsDownloaded = false;
                    }
                    else
                    {
                        var files = new DTFilesTable
                        {
                            FileName = fileName,
                            FilePath = "Downloads",
                            RequestId = row.Id,
                            IsDownloaded = false
                        };
                        _logger.Information($"Adding the request to Files table for download report");
                        _FileContext.HCPOSDownloadFiles.Add(files);

                    }
                }
            }
            _repository.SaveChanges();
            _logger.Information("Request added to Files table");
        }

        private async Task UploadFileToAzureBlobStorage(string sasUrl, string localFilePath, string blobPath, string newFileName)
        {
            
            try
            {
                var blobServiceClient = new BlobServiceClient(new Uri(sasUrl));

                string containerName = "<Your_Container_Name>"; 
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                string newBlobName = Path.Combine(blobPath, newFileName);

                var blobClient = containerClient.GetBlobClient(newBlobName);
                await blobClient.UploadAsync(localFilePath, true);
                _logger.Information($"Upload the file on Azure storage, File Name = {newFileName}");
                File.Delete(localFilePath);
                _logger.Information("Delete the file from local storage");
                Console.WriteLine("Added to Azure");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error uploading file to Azure Blob Storage: {ex.Message}");
                Console.WriteLine($"Error uploading file to Azure Blob Storage: {ex.Message}");
            }
        }

        
        public  List<DTFilesTable> GetFilesToDownload(string countryCode)
        {
            try
            {
                var data = _repository.GetFilesToDownload(countryCode);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public List<DTFilesTable> GetFiles(string countryCode,string? tab,int? id)
        {
            try
            {
                var data = _repository.GetFiles(countryCode,tab,id);
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task RunSeleniumJob2Async(IWebDriver driver, DTHCPOSVendorPortalConfig configDetails,string downloadDirectory)
        {
            try
            {
                var data = GetFilesToDownload(configDetails.CountryCode);
                foreach (var row in data)
                {
                    bool flag = false;
                    if (row.IsDownloaded == false)
                    {
                        
                        flag = _downloadinlclstorage.localdirectory(row.Request.Tab.ToLower(), row.Request.Period.ToLower(), row.Request.Start_Date, driver, configDetails);
                        _logger.Information($"Changed the flag to {flag} in Files table for Id : {row.Id}");
                    }
                    Thread.Sleep(2000);
                    if (flag == true)
                    {
                        string country = configDetails.CountryCode.Substring(0, 2).ToUpper(); 
                        string fileName = "";
                        string blobPath = "";
                        string newFileName = "";
                        string Amazon = "Amazon";

                        switch (row.Request.Period)
                        {
                            case "Daily":
                                switch (row.Request.Tab)
                                {
                                    case "Sales":
                                        fileName = "Sales_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Request.Period + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Traffic":
                                        fileName = "Traffic_" + configDetails.Country.CountryName + "_" + row.Request.Period + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "PublisherReportAlc":
                                        fileName = "E+PReport_ALC" + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "-to-" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "_by_day" + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Publisher Report ALC" + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "PublisherReportSubscription":
                                        fileName = "EBookReport_SUBS" + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "-to-" + row.Request.Start_Date.ToString("dd-MM-yyyy") + "_by_day" + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Publisher Report Subscription" + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;


                                    default:
                                        break;
                                }
                                break;
                            case "Weekly":
                                switch (row.Request.Tab)
                                {
                                    case "Sales":
                                        fileName = "Sales_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Request.Period + "_" + row.Request.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Inventory":
                                        fileName = "Inventory_Manufacturing_Retail_" + configDetails.Country.CountryName + "_" + row.Request.Period + "_" + row.Request.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Traffic":
                                        fileName = "Traffic_" + configDetails.Country.CountryName + "_" + row.Request.Period + "_" + row.Request.Start_Date.AddDays(-6).ToString("dd-MM-yyyy") + "_" + row.Request.Start_Date.ToString("dd-MM-yyyy") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Catalogue":
                                        fileName = "Catalogue_Manufacturing_" + configDetails.Country.CountryName + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Item Catalog " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Forecast - Mean":
                                        fileName = "Forecasting_Retail_Meanforecast_" + configDetails.Country.CountryName + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Forecast - P70":
                                        fileName = "Forecasting_Retail_P70Forecast_" + configDetails.Country.CountryName + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Forecast - P80":
                                        fileName = "Forecasting_Retail_P80Forecast_" + configDetails.Country.CountryName + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "Forecast - P90":
                                        fileName = "Forecasting_Retail_P90Forecast_" + configDetails.Country.CountryName + ".csv";                                     
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + row.Request.Tab + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "RepeatPurchaseBehaviour-Asin":
                                        fileName = country + "_Repeat_Purchase_Behaviour_ASIN_view_Simple_Week" + "_" + row.Request.Start_Date.ToString("yyyy_MM_dd") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Repeat Purchase Behaviour" + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "TopSearchTerms":
                                        fileName = country + "_Top_Search_Terms_Simple_Week" + "_" + row.Request.Start_Date.ToString("yyyy_MM_dd") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Top Search Terms" + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    case "MarketBasketAnalysis":
                                        fileName = country + "_Market_Basket_Analysis_Simple_Week" + "_" + row.Request.Start_Date.ToString("yyyy_MM_dd") + ".csv";
                                        blobPath = row.Request.AzurePath;
                                        newFileName = Amazon + " " + configDetails.CountryCode + " " + "Market Basket Analysis" + " " + row.Request.Period + " " + row.Request.Start_Date.ToString("yyyy-MM-dd") + ".csv";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                        }
                        string localFilePath = Path.Combine(downloadDirectory, fileName);

                        if (File.Exists(localFilePath))
                        {
                            string sasUrl = _configuration.GetValue<string>("StorageAccountConnectionString");
                            await UploadFileToAzureBlobStorage(sasUrl, localFilePath, blobPath, newFileName);
                            row.IsDownloaded = true;
                            _FileContext.Entry(row).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        string[] files = Directory.GetFiles(downloadDirectory);
                        foreach (string file in files)
                        {
                            File.Delete(file);
                        }
                        Console.WriteLine("Failed to download file");
                    }
                }
                _repository.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                string[] files = Directory.GetFiles(downloadDirectory);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                _repository.SaveChanges();
                await RunSeleniumJob2Async(driver, configDetails, downloadDirectory);
            }
        }
        
        public int InsertDownloadQueueItem(DownloadRequest requests,int priority = 0)
        {
            try
            {
                if (requests == null)
                {

                    throw new ArgumentException("Requests array cannot be null or empty.");
                }
                int queueId = _repository.InsertDownloadQueueItem(requests,priority);
                return queueId;
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }

        public void UpdateQueueInProgress(int id, bool newStatus)
        {
            try
            {
                _repository.UpdateQueueInProgress(id,newStatus);
            }
            catch (Exception e)
            {
                throw new Exception();
            }
        }
        public void UpdateAddedToQueue(List<DTRequestTable> requests,int q)
        {
            try
            {
                _repository.UpdateAddedToQueue(requests,q);
            }
            catch(Exception e) 
            {
                throw new Exception();
            }
        }
    }
}