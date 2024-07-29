using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HCFileCorrection.Controllers;
using HCFileCorrection.Entities;
using HCFileCorrection.Interfaces;
using HCFileCorrection.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HCFileCorrection.BackgroundJob;
using Quartz;
using ILogger = Serilog.ILogger;
using OpenQA.Selenium.Chrome;

namespace HCFileCorrection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskExecuterController : ControllerBase
    {
        private readonly QueueRequestsExecutor _queueRequestsExecutor;
        private readonly InsertRequestsBackgroundService _insertRequestsBackgroundService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public TaskExecuterController(QueueRequestsExecutor queueRequestsExecutor, InsertRequestsBackgroundService insertRequestsBackgroundService, IConfiguration configuration, ILogger logger)
        {
            _queueRequestsExecutor = queueRequestsExecutor;
            _insertRequestsBackgroundService = insertRequestsBackgroundService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("execute")]
        public async Task<IActionResult> ExecuteQueue()
        {
            await _queueRequestsExecutor.ExecuteAsync();
            return Ok("Queue execution job Completed.");

        }
        [HttpGet("insert")]
        public async Task<IActionResult> InsertRequests()
        {
            await _insertRequestsBackgroundService.ExecuteAsync();
            return Ok("Insertion job Completed.");
        }
    }
}
        

        

    

