using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Entities;
using HCFileCorrection.Controllers;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HCFileCorrectionExecutor.BackgroundJob
{
    public class QueueRequestsExecutor : BackgroundService
    {
        private readonly ILogger<QueueRequestsExecutor> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHCRepository _repository;
        private readonly FileDbContext _dbContext;

        public QueueRequestsExecutor(ILogger<QueueRequestsExecutor> logger, IServiceScopeFactory serviceScopeFactory,IHCRepository repository,FileDbContext filecontext)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _repository = repository;
            _dbContext = filecontext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool anyJobsInProgress = _repository.GetQueuesInProgress();

                    if (!anyJobsInProgress)
                    {
                        DTDownloadQueue pendingRequest = _repository.GetRequestsInQueue();
                        //null check or empty
                        if(pendingRequest == null)
                        {
                            return;
                        }
                        _repository.UpdateQueueInProgress(pendingRequest.Id, true);
                        List<Request> request = DeserializeRequestTableList(pendingRequest.Request);

                        DownloadRequest download = new DownloadRequest
                        {
                            CountryCode = pendingRequest.CountryCode,
                            Request = request
                        };
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var controller = scope.ServiceProvider.GetRequiredService<ReportController>();
                            await controller.RunSeleniumJobs(download);
                            pendingRequest.QueueCompleted = true;
                            _dbContext.Update(pendingRequest);
                            await _dbContext.SaveChangesAsync();
                        }
                        _repository.UpdateQueueInProgress(pendingRequest.Id, false);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing background job.");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        private List<Request> DeserializeRequestTableList(string requestArrayString)
        {
            List<Request> requestList = JsonConvert.DeserializeObject<List<Request>>(requestArrayString);
            return requestList;
        }
    }
}
