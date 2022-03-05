using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.DomainsRDAP.v1;
using Google.Apis.Services;

namespace RDAPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerOptions _options;
        private DomainsRDAPService _domainsRDAPService;

        public Worker(ILogger<Worker> logger, DomainsRDAPService domainsRDAPService, WorkerOptions options)
        {
            _logger = logger;
            _domainsRDAPService = domainsRDAPService;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var domainsService = new DomainsRDAPService(new BaseClientService.Initializer
                {
                    ApplicationName = "domainsrdap",
                    ApiKey = _options.GoogleCloudAPIKey,
                }))
                {
                    var result = domainsService.Domain.Get("google.com");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
