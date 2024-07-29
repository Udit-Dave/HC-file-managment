using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace HCFileCorrection.BackgroundJob
{
    public class InsertRequestsBackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly string[] countryCodes;
        private readonly int[] retailerid;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public InsertRequestsBackgroundService(ILogger logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            countryCodes = configuration.GetSection("CountryCodes").Get<string[]>();
            retailerid = configuration.GetSection("RetailerId").Get<int[]>();
            if (countryCodes == null || countryCodes.Length == 0)
            {
                throw new ArgumentNullException(nameof(countryCodes), "Country codes cannot be null or empty.");
            }
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.Information("Inserting the requests");
                await ExecuteLogic();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while processing Insert background job.");
            }
        }

        private async Task ExecuteLogic()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                // Resolve scoped services within the scope
                var _createFileService = serviceProvider.GetRequiredService<ICreateFileService>();
                var _hCRepository = serviceProvider.GetRequiredService<IHCRepository>();

                foreach (var countryCode in countryCodes)
                {
                    foreach (var Id in retailerid)
                    {
                        _createFileService.InsertRequests(countryCode,Id);

                        var data = _hCRepository.GetRequestsToQueue(DateTime.Now, countryCode,Id);
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
                                    Request = requests,
                                    RetailerId = Id
                                    
                                };
                                int queueId = _createFileService.InsertDownloadQueueItem(req, 1);
                                _logger.Information("Inserted the Queue's");
                                if (queueId != 0)
                                {
                                    _hCRepository.UpdateAddedToQueue(data.ToList(), queueId);
                                }
                            }
                            else
                            {
                                _logger.Warning("No valid requests found for country code: {CountryCode}", countryCode);
                            }
                        }
                        else
                        {
                            _logger.Warning("No data found for country code: {CountryCode}", countryCode);
                        }
                    }
                }
            }
        }
    }
}
