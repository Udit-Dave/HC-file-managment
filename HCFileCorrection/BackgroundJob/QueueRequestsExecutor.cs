using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HCFileCorrection;
using HCFileCorrection.Controllers;
using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using ILogger = Serilog.ILogger;

namespace HCFileCorrection.BackgroundJob
{
    public class QueueRequestsExecutor 
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;


        public QueueRequestsExecutor(ILogger logger, IServiceScopeFactory serviceScopeFactory, EmailSender emailSender, IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public  async Task ExecuteAsync()
        {
            
            
            try
            {
                _logger.Information("Getting the queue from queue checker Job");


                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var serviceProvider = scope.ServiceProvider;

                    // Resolve scoped services within the scope
                    var _repository = serviceProvider.GetRequiredService<IHCRepository>();
                    var _dbContext = serviceProvider.GetRequiredService<FileDbContext>();


                    int JobIdInProgress = _repository.GetQueuesInProgress();
                    

                    if (JobIdInProgress == 0)
                    {
                        _logger.Information("Checking for pending queues");

                        DTDownloadQueue pendingRequest = _repository.GetRequestsInQueue();
                        //null check or empty
                        if (pendingRequest != null)
                        {
                            _logger.Information("Executing the queue");
                            _repository.UpdateQueueInProgress(pendingRequest.Id, true);
                            List<Request> request = DeserializeRequestTableList(pendingRequest.Request);

                            DownloadRequest download = new DownloadRequest
                            {
                                CountryCode = pendingRequest.CountryCode,
                                Request = request
                            };
                            var controller = scope.ServiceProvider.GetRequiredService<ReportController>();
                            
                            var result = await controller.RunSeleniumJobs(download);
                            if (result is OkObjectResult okResult)
                            {
                                pendingRequest.QueueCompleted = true;
                                _logger.Information("Queue execution completed successfully");
                            }
                            else
                            {
                                pendingRequest.QueueCompleted = false;
                                _logger.Warning("Queue execution failed");
                            }

                            //pendingRequest.QueueCompleted = true;
                            _dbContext.Update(pendingRequest);
                            await _dbContext.SaveChangesAsync();
                            _repository.UpdateQueueInProgress(pendingRequest.Id, false);
                            _logger.Information("Queue execution completed successfully");

                        }
                        else
                        {                         
                            _logger.Information("There are no pending queue's");

                        }


                    }
                    else
                    {

                        DTDownloadQueue queue = _repository.GetQueueById(JobIdInProgress);
                        if (queue != null)
                        {
                            TimeSpan timeSinceModified = DateTime.Now - queue.ModifiedDateTime;
                            int thresholdHours = _configuration.GetValue<int>("ThresholdHours");
                            if (timeSinceModified.TotalHours >= thresholdHours)
                            {
                                _repository.UpdateQueueInProgress(JobIdInProgress, false);
                                await _emailSender.SendEmail("Queue execution canceled", "Queue execution exceeded the 2-hour limit and was canceled.");

                                _logger.Information($"Queue execution exceeded the {thresholdHours}-hour limit and was canceled. QueueId :  {JobIdInProgress}");
                            }
                        }
                        _logger.Information("Previous job still in progress");

                    }

                }
                _logger.Information("Waiting for 5 minutes");

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while processing queue executoion background job.");
            }
           
        }

        private List<Request> DeserializeRequestTableList(string requestArrayString)
        {
            List<Request> requestList = JsonConvert.DeserializeObject<List<Request>>(requestArrayString);
            return requestList;
        }
    }
}
