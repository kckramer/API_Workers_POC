using MaxMind.GeoIP2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeoIPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WebServiceClient _webServiceClient;
        private readonly WorkerOptions _options;

        public Worker(ILogger<Worker> logger, WebServiceClient webServiceClient, WorkerOptions options)
        {
            _logger = logger;
            _webServiceClient = webServiceClient;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var client = new WebServiceClient(_options.AccountId, _options.LicenseKey, host: "geolite.info");

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
