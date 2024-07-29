using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using HCFileCorrection.Repository;
using HCFileCorrection.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DataAccess;
using HCFileCorrection.Utility;
using HCFileCorrection.Selenium;
using HCFileCorrectionExecutor.BackgroundJob;
using HCFileCorrection.Controllers; // Import your DataAccess namespace here

namespace HCFileCorrectionExecutor
{
    public class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddControllers();
                    services.AddScoped<ReportController>();
                    services.AddSingleton<IConfiguration>(hostContext.Configuration);
                    services.AddSingleton<ILoggerFactory, LoggerFactory>();

                    // Register dependencies for CreateFileService
                    services.AddSingleton<GetOtp>();
                    services.AddSingleton<EmailSetting>();
                    services.AddSingleton<FileDownloadJob>();
                    services.AddSingleton<Downloadinlocalstorage>();
                    services.AddSingleton<ICreateFileService, CreateFileService>();

                    // Register FileDbContext
                    services.AddTransient<FileDbContext>(); // Register FileDbContext here

                    // Register HCRepository with FileDbContext dependency
                    services.AddScoped<IHCRepository, HCRepository>();

                    // Register background services
                    services.AddHostedService<InsertRequestsBackgroundService>();
                    services.AddHostedService<QueueRequestsExecutor>();
                });
    }
}
