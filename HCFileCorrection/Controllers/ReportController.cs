using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Selenium;
using HCFileCorrection.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HCFileCorrection.Repository;
using ILogger = Serilog.ILogger;
using HCFileCorrection.BackgroundJob;
using Microsoft.AspNetCore.Authorization;

namespace HCFileCorrection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        
        private readonly ICreateFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly EmailSender _emailSender;


        public ReportController(ICreateFileService fileService,IConfiguration configuration,ILogger logger, EmailSender email)
        {           
            _fileService = fileService;
            _configuration = configuration;       
            _logger = logger;
            _emailSender = email;
            
        }

        [HttpPost("DownloadFiles")]
        public async Task<IActionResult> RunSeleniumJobs(DownloadRequest request)
        {
            ChromeDriver driver = null;
            try
            {
                DTHCPOSVendorPortalConfig configDetails = _fileService.GetVendorPortalConfig(request.CountryCode);
                if (configDetails == null)
                {
                    return Ok("Country Code is invalid");
                }

                var data = _fileService.GetRequests(request);

                if (data == null || data.Count == 0)
                {
                    return Ok("No files to download");
                }

                string downloadDirectory = Path.Combine(_configuration.GetValue<string>("LocalStoragePath"), request.CountryCode);
                if (!Directory.Exists(downloadDirectory))
                {
                    Directory.CreateDirectory(downloadDirectory);
                }

                ChromeOptions options = new ChromeOptions();
                options.AddUserProfilePreference("download.default_directory", downloadDirectory);
                options.AddUserProfilePreference("download.prompt_for_download", false);
                options.AddUserProfilePreference("download.directory_upgrade", true);

                driver = new ChromeDriver(options);

                driver.Manage().Window.Maximize();
                Thread.Sleep(2000); 

                _fileService.Login(driver, configDetails);

                await _fileService.RunSeleniumJob1Async(data, driver, configDetails);
                await _fileService.RunSeleniumJob2Async(driver, configDetails, downloadDirectory);

                return Ok("Selenium job completed");

            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while running Selenium jobs:{ex.Message}");
                return BadRequest(ex.Message);
            }
            finally
            {
                if (driver != null)
                {
                    driver.Quit();
                    driver.Dispose();
                }
            }
        }

        [Authorize]
        [HttpGet("GetReportData")]
        public async Task<IActionResult> GetReportData(string countryCode, string? tab,int? Retailerid)            
        {
            try
            {
                DTHCPOSVendorPortalConfig configDetails = _fileService.GetVendorPortalConfig(countryCode);
                if (configDetails == null)
                {
                    return Ok("Country Code is invalid");
                }
                var filesData = _fileService.GetFiles(countryCode, tab,Retailerid);

                var responseModels = filesData.Select(file =>
                {
                    string? status;
                    
                    if (file.IsDownloaded && file.Request.IsAddedToDownloadManager)
                    {
                        status = "Downloaded";
                    }
                    else if (!file.Request.IsAddedToDownloadManager && file.Request.DownloadQueue?.QueueInProgress == true && file.Request.DownloadQueue?.QueueCompleted != true)
                    {
                        status = "In progress";
                    }
                    else if (!file.IsDownloaded || !file.Request.IsAddedToDownloadManager && file.Request.DownloadQueue?.QueueInProgress == false && file.Request.DownloadQueue?.QueueCompleted == true)
                    {
                        status = "Download Failed";
                    }
                    else if (!file.IsDownloaded || !file.Request.IsAddedToDownloadManager && file.Request.DownloadQueue?.QueueInProgress != true && file.Request.DownloadQueue?.QueueCompleted != true)
                    {
                        status = "In Queue";
                    }
                    else
                    {
                        status = "Reset";
                    }

                    return new ResponseModel
                    {
                        Id = file.Id,
                        FileName = file.FileName,
                        Tab = file.Request?.Tab,
                        Period = file.Request?.Period,
                        Date = file.Request?.Start_Date,
                        Status = status,
                        LastDownloaded = file.Request?.ModifiedDateTime
                    };
                }).ToList();

                return Ok(responseModels);
            }
            catch(Exception ex) 
            {
                throw new Exception(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AddRequestsToQueue")]
        public IActionResult AddRequests(DownloadRequest request)
        {
            try
            {
                int queueId = _fileService.InsertDownloadQueueItem(request);
                if(queueId != 0)
                {
                    var data = _fileService.GetRequests(request);
                    _fileService.UpdateAddedToQueue(data.ToList(),queueId);
                }

                return Ok("Requests added to the queue successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding requests to the queue.");
            }
        }
    }
}
