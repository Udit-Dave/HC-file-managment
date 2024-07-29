using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Linq.Expressions;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;
using HCFileCorrection.Entities;
using Azure.Core;
namespace HCFileCorrection.Repository
{
    public class HCRepository : IHCRepository
    {
        private readonly FileDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public HCRepository(FileDbContext dbContext, ILogger logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        public DTHCPOSVendorPortalConfig GetVendorPortalConfig(string countryCode)
        {
            _logger.Information($"Retrieving credentials for country : {countryCode}");
            try
            {
                /*var credintials = _dbContext.HCPOSVendorPortalConfig.
                    Include.(c => c.Country).SingleOrDefault(r => r.CountryCode == countryCode);*/
                var credentials = _dbContext.HCPOSVendorPortalConfig
                                    .Include(c => c.Country)
                                    .SingleOrDefault(r => r.CountryCode == countryCode);
                if (credentials == null)
                {
                    _logger.Warning($"Credentials not found for country : {countryCode}");
                }
                else
                {
                    _logger.Information($"Credentials retrieved successfully for country : {countryCode}");
                }
                return credentials;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error retrieving credentials for country :{countryCode}");
                throw;
            }
        }
        public IEnumerable<DTRequestTable> GetCurrentDateRequests(DateTime startDate, string countryCode,int id)
        {
            _logger.Information($"Getting current  requests for the date: {startDate.Date}  country : {countryCode} and retailerid : {id}");
            try
            {
                startDate = startDate.Date;
                var request = _dbContext.HCPOSDownloadRequest
                    .Where(r => r.RequestCreatedDateTime.Date == startDate && r.CountryCode == countryCode && r.RetailerId == id)
                    .ToList();
                _logger.Information($"Current date requests retrieved successfully for the date: {startDate.Date} country : {countryCode} and retailerid : {id}");
                return request;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error retrieving current date requests for the date: {startDate.Date} country : {countryCode} and retailerid : {id}");
                throw;
            }
        }
        public IEnumerable<DTRequestTable> GetRequestsToQueue(DateTime startDate, string countryCode,int id)
        {
            _logger.Information($"Getting requests from queue for the date: {startDate.Date} country : {countryCode} and RetailerId : {id}");
            try
            {
                startDate = startDate.Date;
                var requests = _dbContext.HCPOSDownloadRequest.Where(r => r.CountryCode == countryCode && !r.IsAddedToDownloadManager && r.RequestCreatedDateTime.Date == startDate && r.RetailerId == id).ToList();
                
                var requestsfile = _dbContext.HCPOSDownloadFiles
                                           .Where(f => !f.IsDownloaded && f.Request.CountryCode == countryCode ||
                                                       !f.Request.IsAddedToDownloadManager && f.Request.CountryCode == countryCode || f.Request.RetailerId == id)
                                           .Select(f => f.Request)
                                           .Distinct()
                                           .ToList();

                _logger.Information($"Queue requests retrieved successfully for the date: {startDate.Date} country : {countryCode} and RetailerId : {id}");
                var combinedRequests = requests.Union(requestsfile).ToList();

                return requests;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error retrieving requests from queue for the date:{startDate.Date} country :  {countryCode} and RetailerId : {id}");
                throw;
            }
        }

        public IEnumerable<DTRequestTable> GetRequestsToDownload(DownloadRequest requests)
        {
            _logger.Information($"Getting requests to adding files in download manager for country : {requests.CountryCode}");
            try
            {
                List<DTRequestTable> result = new List<DTRequestTable>();
                var query = _dbContext.HCPOSDownloadRequest.Where(r => r.CountryCode == requests.CountryCode).ToList();
                if (requests.Request != null && requests.Request.Any())
                {
                    foreach (var request in requests.Request)
                    {
                        string formattedStartDate = request.StartDate.ToString("yyyy-MM-dd");
                        var filteredQuery = query.Where(r =>
                            r.Tab == request.Tab &&
                            r.Period == request.Period &&
                            r.Start_Date.ToString("yyyy-MM-dd") == formattedStartDate).ToList();
                        _logger.Information($"Request to add file in download manager retrieved successfully for the Tab : {request.Tab}, Date: {formattedStartDate} and Country :{requests.CountryCode}");
                        foreach (var filteredRequest in filteredQuery)
                        {
                            MarkRequestToBeDownloaded(filteredRequest.Id);
                        }
                        result.AddRange(filteredQuery);
                    }
                }
                else
                {
                    result.AddRange(query);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error retrieving requests to add files in download manager for Country : { requests.CountryCode}");
                throw;
            }
        }

        private void MarkRequestToBeDownloaded(int requestId)
        {
            _logger.Information("Mark request to be download in request table for Request ID: {RequestId}", requestId);
            var downloadRequest = _dbContext.HCPOSDownloadRequest.FirstOrDefault(r => r.Id == requestId);
            if (downloadRequest != null )
            {
                downloadRequest.IsAddedToDownloadManager = false;
               
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<DTFilesTable> GetFilesToDownload(string countryCode)
        {
            try
            {
                _logger.Information($"Getting request to download reports for coundtry code : {countryCode}");
                var data = _dbContext.HCPOSDownloadFiles.Where(r => r.IsDownloaded == false && r.Request.CountryCode == countryCode).ToList();
                if(data == null || data.Count == 0)
                {
                    _logger.Warning($"No files to download for country : {countryCode}");
                }
                _logger.Information($"Successfully get the request to download reports for coundtry : {countryCode}");
                return data;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error occurred while fetching request to download for country : {countryCode}");
                return null;
            }
        }

        public List<DTFilesTable> GetFiles(string countryCode, string? tab, int? Retailerid)
        {
            try
            {
                List<DTFilesTable> allFilesData = new List<DTFilesTable>();
                _logger.Information($"Fetching all files for country : {countryCode}");
                var query = _dbContext.HCPOSDownloadFiles
                    .Include(f => f.Request)
                        .ThenInclude(r => r.DownloadQueue)
                    .Where(f => f.Request.CountryCode == countryCode);
                if(query == null)
                {
                    _logger.Warning($"No files for country: {countryCode}");
                }
                if (tab != null)
                {
                    query = query.Where(f => f.Request.Tab == tab);
                }
                if (Retailerid != null)
                {
                    query = query.Where(f => f.Request.RetailerId == Retailerid);
                }

                allFilesData = query.ToList();
                _logger.Information($"Fetched all files for country : {countryCode}");
                return allFilesData;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error occurred while fetching files for country : {countryCode}");
                return null;
            }
        }
        public void InsertFile(DTFilesTable file)
        {
            _dbContext.HCPOSDownloadFiles.Add(file);
        }

        public int InsertDownloadQueueItem(DownloadRequest requests, int priority)
        {
            if (requests.Request == null || !requests.Request.Any())
            {
                throw new ArgumentException("Requests array cannot be null or empty.");
            }

            try
            {
                _logger.Information("Inserting request to the queue table");
                var queueItem = new DTDownloadQueue
                {
                    Request = JsonConvert.SerializeObject(requests.Request),
                    CountryCode = requests.CountryCode,
                    QueueCompleted = false,
                    CreatedDateTime = DateTime.Now,
                    Priority = priority,
                    ModifiedDateTime = DateTime.Now,
                    CreatedUser = requests.CreatedUser,
                    RetailerId = requests.RetailerId
                };

                _dbContext.HCPOSDownloadQueue.Add(queueItem);
                _dbContext.SaveChanges();
                _logger.Information("The request inserted to the queue table");
                
                return queueItem.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "Error occurred while inserting queue item");
                return -1; 
            }
        }


        public void UpdateQueueInProgress(int id, bool newStatus)
        {
            var dbRequest = _dbContext.HCPOSDownloadQueue.FirstOrDefault(r => r.Id == id);
            if (dbRequest != null)
            {
                dbRequest.QueueInProgress = newStatus;
                dbRequest.ModifiedDateTime = DateTime.Now;
                _logger.Information($"The status updated in queue table");
            }

            _dbContext.SaveChanges();
        }

        public DTDownloadQueue GetRequestsInQueue()
        {
            try
            {
                _logger.Information($"Retrieving request from the queue");
                var requests = _dbContext.HCPOSDownloadQueue.Where(r => r.QueueCompleted == false).OrderByDescending(r => r.Priority).FirstOrDefault();
                if (requests == null)
                {
                    _logger.Warning($"No request in queue");
                    return null;
                }
                else
                {
                    _logger.Information($"Successfully retrived request from the queue for the country : {requests.CountryCode}");
                    return requests;
                }
                
            }
            catch(Exception ex) 
            {
                _logger.Error(ex.Message, "An error occurred while retrieving requests from the queue.");
                throw;
            }
        }

        public int GetQueuesInProgress()
        {
            _logger.Information($"Checking if any queue in progress");
            var requests = _dbContext.HCPOSDownloadQueue.FirstOrDefault(r => r.QueueInProgress == true);
            if(requests == null)
            {
                return 0;
            }
            return requests.Id;
        }

        public DTDownloadQueue GetQueueById(int Id)
        {
            _logger.Information($"Checking if any queue in progress");
            var requests = _dbContext.HCPOSDownloadQueue.FirstOrDefault(r => r.Id == Id);
            return requests;
        }

        public void UpdateAddedToQueue(List<DTRequestTable> requests,int q)
        {
            try
            {
                _logger.Information("Updating request that is added in queue");
                foreach (var request in requests)
                {
                    request.QueueID = q;
                    request.ModifiedDateTime = DateTime.Now;
                    request.AddedToQueue = true;
                }

                _dbContext.SaveChanges();
                _logger.Information("The request is updated that is added in queue");
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, "An error occurred while updating the requests that is added in queue.");
                throw;
            }
        }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
        public void InsertDailyRequestDE(DateTime startDate, string countryCode)
        {
            try
            {
                _logger.Information("Inserting daily requests for Country : DE.");
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DailyRequestForDE.json");
                var json = File.ReadAllText(filePath);

                var requests = JsonConvert.DeserializeObject<List<DTRequestTable>>(json);

                foreach (var request in requests)
                {
                    request.Start_Date = startDate.AddDays(-2);
                    request.RequestCreatedDateTime = DateTime.Now;
                    request.CountryCode = countryCode;
                    request.AzurePath += countryCode + "/";
                }

                _dbContext.HCPOSDownloadRequest.AddRange(requests);               
                _dbContext.SaveChanges();
                _logger.Information("Inserted daily requests for Country : DE.");
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, "An error occurred while inserting daily requests for Country : DE.");
                throw;
            }
        }

        public void InsertRequestsDaily(string countryCode,int id)
        {
            var specifiedStartDateString = _configuration[$"SpecifiedDates:{countryCode}"];
            if (!DateTime.TryParse(specifiedStartDateString, out DateTime specifiedStartDate))
            {
                _logger.Error($"No valid specified start date found for country: {countryCode}");
                return;
            };
            var endDate = DateTime.Now.AddDays(-2); // End date is two days before today
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DailyRequests.json");
            //var filePathForDE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DailyRequestForDE.json");
            var json = File.ReadAllText(filePath);
            //var jsonForDE = File.ReadAllText(filePathForDE);
            
            var requestTemplates = JsonConvert.DeserializeObject<List<DTRequestTable>>(json);
            //var requestTemplatesForDE = JsonConvert.DeserializeObject<List<DTRequestTable>>(jsonForDE);
            var tabs = requestTemplates.Select(rt => rt.Tab).Distinct().ToList();
            //var tabsForDE = requestTemplatesForDE.Select(rt => rt.Tab).Distinct().ToList();
            List<string> tabsToUse;
            if (countryCode == "DE")
            {
               // tabsToUse = tabs.Union(tabsForDE).ToList();
            }
            else
            {
                tabsToUse = tabs;
            }
            for (var date = specifiedStartDate; date <= endDate; date = date.AddDays(1))
            {
                try
                {
                    _logger.Information("Getting missing request from start date to Current date");

                    var missingTabs = GetMissingTabsForDate(date, countryCode, tabs,id);

                    if (missingTabs.Any())
                    {
                        _logger.Information("Inserting request from start date to current date");

                        InsertRequestsForMissingTabs(date, countryCode, missingTabs, id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error( $"Error occurred while checking and inserting requests for date: {date} and country: {countryCode}. {ex.Message.Split('\n')[0]}");
                }
            }
        }

        private List<string> GetMissingTabsForDate(DateTime date, string countryCode, List<string> tabs,int id)
        {
            var existingTabs = _dbContext.HCPOSDownloadRequest
                .Where(r => r.Start_Date == date && r.Period== "Daily" && r.CountryCode == countryCode && r.RetailerId == id)
                .Select(r => r.Tab)
                .Distinct()
                .ToList();

            return tabs.Except(existingTabs).ToList();
        }

        private void InsertRequestsForMissingTabs(DateTime date, string countryCode, List<string> missingTabs,int id)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DailyRequests.json");
            var json = File.ReadAllText(filePath);
            var requestTemplates = JsonConvert.DeserializeObject<List<DTRequestTable>>(json);
            var retailerExists = _dbContext.HCPOSRetailer.Any(r => r.Id == id);
            if (!retailerExists)
            {
                throw new Exception($"Retailer with Id {id} does not exist.");
            }
            var requestsToInsert = requestTemplates
                .Where(rt => missingTabs.Contains(rt.Tab))
                .ToList();

            foreach (var request in requestsToInsert)
            {

                request.Start_Date = date;
                request.RequestCreatedDateTime = DateTime.Now;
                request.CountryCode = countryCode;
                request.AzurePath += countryCode + "/";
                request.RetailerId = id;

                switch (request.Tab)
                {
                    case "PublisherReportAlc":
                    case "PublisherReportSubscription":
                        // Assuming request has a VendorPortalConfig property
                        var countryConfig = _dbContext.HCPOSVendorPortalConfig
                                                .Where(r => r.CountryCode == countryCode)
                                                .Select(r => r.PublisherTabs)
                                                .FirstOrDefault();

                        if (countryConfig.HasValue && countryConfig.Value)
                        {
                            // Add the request to the database
                            _dbContext.HCPOSDownloadRequest.Add(request);
                        }
                        break;
                    default: 
                        // Add requests for other tabs without any condition
                        _dbContext.HCPOSDownloadRequest.Add(request);
                        break;
                }
            }

            int recordsAdded = _dbContext.SaveChanges();


            if (recordsAdded > 0)
            {
                _logger.Information($"Inserted missing daily requests for Country: {countryCode} and Date: {date}");
            }
            
        }


        public void InsertRequestsWeekly(DateTime mostRecentSaturday, string countryCode,int id)
        {
            try
            {
                var specifiedStartDateString = _configuration[$"SpecifiedDatesWeekly:{countryCode}"];
                if (!DateTime.TryParse(specifiedStartDateString, out DateTime specifiedStartDate))
                {
                    _logger.Error($"No valid specified start date found for weekly requests for country: {countryCode}");
                    return;
                }

                // Calculate all Saturdays from the specified start date to the most recent Saturday
                var saturdays = new List<DateTime>();
                for (var date = specifiedStartDate; date <= mostRecentSaturday; date = date.AddDays(7))
                {
                    saturdays.Add(date);
                }

                foreach (var saturday in saturdays)
                {
                    var missingTabs = GetMissingTabsForWeek(saturday, countryCode,id);

                    if (missingTabs.Any())
                    {
                        InsertRequestsForMissingTabsWeekly(saturday, countryCode, missingTabs,id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message,$"Error occurred while inserting weekly requests for country: {countryCode}");
            }
        }

        private List<string> GetMissingTabsForWeek(DateTime weekStartDate, string countryCode, int id)
        {
            var existingTabs = _dbContext.HCPOSDownloadRequest
                .Where(r => r.Start_Date == weekStartDate && r.Period == "Weekly" && r.CountryCode == countryCode && r.RetailerId == id)
                .Select(r => r.Tab)
                .Distinct()
                .ToList();

            // Read the request templates from the JSON file
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeekRequests.json");
            var json = File.ReadAllText(filePath);
            var requestTemplates = JsonConvert.DeserializeObject<List<DTRequestTable>>(json);
            var tabs = requestTemplates.Select(rt => rt.Tab).Distinct().ToList();

            return tabs.Except(existingTabs).ToList();
        }

        private void InsertRequestsForMissingTabsWeekly(DateTime weekStartDate, string countryCode, List<string> missingTabs,int id)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WeekRequests.json");
            var json = File.ReadAllText(filePath);
            var requestTemplates = JsonConvert.DeserializeObject<List<DTRequestTable>>(json);

            var requestsToInsert = requestTemplates
                .Where(rt => missingTabs.Contains(rt.Tab))
                .ToList();

            foreach (var request in requestsToInsert)
            {
                request.Start_Date = weekStartDate;
                request.RequestCreatedDateTime = DateTime.Now;
                request.CountryCode = countryCode;
                request.AzurePath += countryCode + "/";
                request.RetailerId = id;
            }

            _dbContext.HCPOSDownloadRequest.AddRange(requestsToInsert);
            int recordsAdded = _dbContext.SaveChanges();

            if (recordsAdded > 0)
            {
                _logger.Information($"Inserted missing weekly requests for Country: {countryCode} Date: {weekStartDate} and RetailerId : {id}");
            }
            else
            {
                _logger.Warning($"Failed to insert missing weekly requests for Country: {countryCode} Date: {weekStartDate} and RetailerId : {id}");
            }
        }
    }
}
