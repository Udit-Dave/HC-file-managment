using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HCFileCorrection.Controllers;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Repository;
using HCFileCorrection.Services;
using HCFileCorrection.Utility;
using HCFileCorrection.Models;
using HCFileCorrection.Selenium;
using Serilog;
using Serilog.Sinks.File;
using Microsoft.EntityFrameworkCore;
using HCFileCorrection.BackgroundJob;
using Quartz;
using Quartz.Spi;
using Serilog.Core;
using Serilog.Events;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;

namespace HCFileCorrection
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = LogEventLevel.Information // Set the default minimum level
        };

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Serilog with daily rolling file and custom file naming
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            // Apply the filter switch
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Override logging level for Microsoft namespace
            .MinimumLevel.Override("System", LogEventLevel.Warning) // Override logging level for System namespace
            .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "C:\\Logs\\log.txt", // Base path for the log file
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day, // Create a new log file every day
                    fileSizeLimitBytes: null, // No file size limit
                    retainedFileCountLimit: null, // Keep all log files
                    rollOnFileSizeLimit: true, // Roll over based on file size limit
                    shared: true // Allow sharing the log file
                )
                .CreateLogger();
            services.AddScoped<JwtValidator>();
            services.AddScoped<GenerateJwtToken>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true); // Add Serilog
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
            var jwtBearerOptions = new JwtBearerOptions();


                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "your_issuer",
                    ValidAudience = "your_audience_value",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RjA2R0FWeUZmTWZrTDQ3dWRhNjJNSFNyM2lPUWJrYWtFamw3Y3g0eGg0djE0NUVXZz0=\r\n")), // Ensure this matches the key used in token generation
                };
                  options.SecurityTokenValidators.Add(services.BuildServiceProvider().GetService<JwtValidator>());

            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("User", policy => policy.RequireRole("User"));
                options.AddPolicy("AdminOrUser", policy =>
                {
                    policy.RequireRole("Admin", "User");
                });
            });
            



            services.AddSingleton<IConfiguration>(Configuration);
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("*")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddDbContext<FileDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Scoped);

            services.AddScoped<FileDownloadJob>();
            services.AddScoped<Downloadinlocalstorage>();
            services.AddScoped<GetOtp>();
            services.AddScoped<EmailSetting>();
            services.AddScoped<EmailSender>();
            services.AddScoped<IHCRepository, HCRepository>();
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<ICreateFileService, CreateFileService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IConfigService, ConfigService>();
            services.AddSingleton<Serilog.ILogger>(Log.Logger);
            services.AddScoped<QueueRequestsExecutor>();
            services.AddSingleton<InsertRequestsBackgroundService>();

            services.AddControllers();
            services.AddScoped<ReportController>();
            services.AddScoped<TaskExecuterController>();
            //services.AddSwaggerGen();
            services.AddSwaggerGen(options =>
            {
                // Swagger configuration

                // Define security scheme for JWT
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lowercase
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);

                // Optionally, specify the Swagger UI to include the security scheme
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                   {
                      securityScheme, new string[] { }
                   }
                });

                
            });


        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors("AllowSpecificOrigin");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
