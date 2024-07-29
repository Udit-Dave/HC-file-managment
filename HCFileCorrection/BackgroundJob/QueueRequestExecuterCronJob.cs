//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using HCFileCorrection.Controllers;
//using HCFileCorrection.Entities;
//using HCFileCorrection.Interfaces;
//using HCFileCorrection.Models;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Quartz;

//namespace HCFileCorrection.BackgroundJob
//{
//    [DisallowConcurrentExecution]
//    public class QueueRequestsCronJob : IJob
//    {
//        private readonly ILogger<QueueRequestsCronJob> _logger;
//        private readonly IServiceScopeFactory _serviceScopeFactory;

//        public QueueRequestsCronJob(ILogger<QueueRequestsCronJob> logger, IServiceScopeFactory serviceScopeFactory)
//        {
//            _logger = logger;
//            _serviceScopeFactory = serviceScopeFactory;
//        }

//        public async Task Execute(IJobExecutionContext context)
//        {
//            try
//            {
//                _logger.LogInformation("Getting the queue from queue checker Job");

//                using (var scope = _serviceScopeFactory.CreateScope())
//                {
//                    var serviceProvider = scope.ServiceProvider;

//                    // Resolve scoped services within the scope
//                    var _repository = serviceProvider.GetRequiredService<IHCRepository>();
//                    var _dbContext = serviceProvider.GetRequiredService<FileDbContext>();

//                    bool anyJobsInProgress = _repository.GetQueuesInProgress();

//                    if (!anyJobsInProgress)
//                    {
//                        _logger.LogInformation("Checking for pending queues");

//                        DTDownloadQueue pendingRequest = _repository.GetRequestsInQueue();

//                        if (pendingRequest != null)
//                        {
//                            _logger.LogInformation("Executing the queue");
//                            _repository.UpdateQueueInProgress(pendingRequest.Id, true);
//                            List<Request> request = DeserializeRequestTableList(pendingRequest.Request);

//                            DownloadRequest download = new DownloadRequest
//                            {
//                                CountryCode = pendingRequest.CountryCode,
//                                Request = request
//                            };
//                            var controller = scope.ServiceProvider.GetRequiredService<ReportController>();
//                            var result = await controller.RunSeleniumJobs(download);

//                            pendingRequest.QueueCompleted = true;
//                            _dbContext.Update(pendingRequest);
//                            await _dbContext.SaveChangesAsync();
//                            _repository.UpdateQueueInProgress(pendingRequest.Id, false);
//                            _logger.LogInformation("Queue execution completed successfully");
//                        }
//                        else
//                        {
//                            _logger.LogInformation("There are no pending queues");
//                        }
//                    }
//                    else
//                    {
//                        _logger.LogInformation("Previous job still in progress");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred while processing queue execution background job.");
//            }
//        }

//        private List<Request> DeserializeRequestTableList(string requestArrayString)
//        {
//            List<Request> requestList = JsonConvert.DeserializeObject<List<Request>>(requestArrayString);
//            return requestList;
//        }
//    }
//}
