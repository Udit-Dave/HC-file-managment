//using HCFileCorrection.Interfaces;
//using HCFileCorrection.Models;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;

//namespace HCFileCorrectionExecutor.BackgroundJob
//{
//    public class InsertRequestsBackgroundService : BackgroundService
//    {
//        private readonly ILogger<InsertRequestsBackgroundService> _logger;
//        private readonly ICreateFileService _createFileService;
//        private readonly IHCRepository _hCRepository;
//        private readonly IConfiguration _configuration;
//        private readonly string[] countryCodes; // Array of country codes
//        private readonly TimeSpan _dailyRunTime = new TimeSpan(17, 00, 0);

//        public InsertRequestsBackgroundService(ILogger<InsertRequestsBackgroundService> logger, ICreateFileService createFileService, IHCRepository hCRepository, IConfiguration configuration)
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            _createFileService = createFileService ?? throw new ArgumentNullException(nameof(createFileService));
//            _hCRepository = hCRepository ?? throw new ArgumentNullException(nameof(hCRepository));
//            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//            countryCodes = configuration.GetSection("CountryCodes").Get<string[]>();
//            if (countryCodes == null || countryCodes.Length == 0)
//            {
//                throw new ArgumentNullException(nameof(countryCodes), "Country codes cannot be null or empty.");
//            }
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    // Get the current date and time
//                    var currentDateTime = DateTime.Now;

//                    // Calculate the next run time
//                    var nextRunDateTime = CalculateNextRunTime(currentDateTime);

//                    // Calculate the delay until the next run time
//                    var delay = nextRunDateTime - currentDateTime;

//                    // Wait until the next run time
//                    await Task.Delay(delay, stoppingToken);

//                    // Execute the logic at 4 PM
//                    await ExecuteLogic();

                    

//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "An error occurred while processing background job.");
//                }
//            }
//        }

//        private DateTime CalculateNextRunTime(DateTime currentDateTime)
//        {
//            // Calculate the next run time as tomorrow at 4 PM
//            var nextRunDate = currentDateTime.Date.AddDays(1); // Next day
//            var nextRunDateTime = nextRunDate.Add(_dailyRunTime);
//            return nextRunDateTime;
//        }

//        private async Task ExecuteLogic()
//        {
//            foreach (var countryCode in countryCodes)
//            {
//                // Execute the insertion logic for each country code
//                _createFileService.InsertRequests(countryCode);

//                var data = _hCRepository.GetRequestsToQueue(DateTime.Now, countryCode);
//                if (data != null)
//                {
//                    List<Request> requests = data
//                        .Where(dtRequest => dtRequest.Tab != null && dtRequest.Period != null && dtRequest.Start_Date != null)
//                        .Select(dtRequest => new Request
//                        {
//                            Tab = dtRequest.Tab,
//                            Period = dtRequest.Period,
//                            StartDate = dtRequest.Start_Date
//                        }).ToList();

//                    if (requests.Any())
//                    {
//                        DownloadRequest req = new DownloadRequest
//                        {
//                            CountryCode = countryCode,
//                            Request = requests
//                        };
//                        int queueId = _createFileService.InsertDownloadQueueItem(req, 1);
//                        if (queueId != 0)
//                        {
//                            _hCRepository.UpdateAddedToQueue(data.ToList(), queueId);
//                        }

//                    }
//                    else
//                    {
//                        _logger.LogWarning("No valid requests found for country code: {CountryCode}", countryCode);
//                    }
//                }
//                else
//                {
//                    _logger.LogWarning("No data found for country code: {CountryCode}", countryCode);
//                }
//            }
//        }
//    }
//}
