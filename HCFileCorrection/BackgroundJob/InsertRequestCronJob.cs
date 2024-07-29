/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HCFileCorrection.BackgroundJob
{
    

    public class InsertRequestCronJob : IJob
    {
        private readonly ILogger<InsertRequestCronJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly string[] _countryCodes;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _dailyRunTime;

        public InsertRequestCronJob(ILogger<InsertRequestCronJob> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _countryCodes = configuration.GetSection("CountryCodes").Get<string[]>();
            var dailyRunTime = configuration.GetValue<TimeSpan>("DailyRunTime");
            _dailyRunTime = new TimeSpan(dailyRunTime.Hours, dailyRunTime.Minutes, dailyRunTime.Seconds);

            if (_countryCodes == null || _countryCodes.Length == 0)
            {
                throw new ArgumentNullException(nameof(_countryCodes), "Country codes cannot be null or empty.");
            }
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("Insertion Job has been started");

                await ExecuteLogic();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing Insert background job.");
            }
        }

        private async Task ExecuteLogic()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                // Resolve scoped services within the scope
                var createFileService = serviceProvider.GetRequiredService<ICreateFileService>();
                var hCRepository = serviceProvider.GetRequiredService<IHCRepository>();

                foreach (var countryCode in _countryCodes)
                {
                    createFileService.InsertRequests(countryCode);

                    var data = hCRepository.GetRequestsToQueue(DateTime.Now, countryCode);
                    if (data != null)
                    {
                        List<Request> requests = data
                            .Where(dtRequest => dtRequest.Tab != null && dtRequest.Period != null && dtRequest.Start_Date != null)
                            .Select(dtRequest => new Request
                            {
                                Tab = dtRequest.Tab,
                                Period = dtRequest.Period,
                                StartDate = dtRequest.Start_Date
                            }).ToList();

                        if (requests.Any())
                        {
                            DownloadRequest req = new DownloadRequest
                            {
                                CountryCode = countryCode,
                                Request = requests
                            };
                            int queueId = createFileService.InsertDownloadQueueItem(req, 1);
                            _logger.LogInformation("Inserted the Queue's");
                            if (queueId != 0)
                            {
                                hCRepository.UpdateAddedToQueue(data.ToList(), queueId);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No valid requests found for country code: {CountryCode}", countryCode);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No data found for country code: {CountryCode}", countryCode);
                    }
                }
            }
        }
    }
}
*/