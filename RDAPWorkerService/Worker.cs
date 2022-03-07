using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.DomainsRDAP.v1;
using Google.Apis.Services;
using Azure.Messaging.ServiceBus;

namespace RDAPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerOptions _options;
        private DomainsRDAPService _domainsRDAPService;
        private ServiceBusClient _serviceBusClient;

        public Worker(ILogger<Worker> logger, DomainsRDAPService domainsRDAPService, WorkerOptions options, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _domainsRDAPService = domainsRDAPService;
            _options = options;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // create a processor that we can use to process the messages
                var processor = _serviceBusClient.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions());

                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation($"Received: {body}");

            await _domainsRDAPService.Domain.Get(body).ExecuteAsync();

            _logger.LogInformation(string.Empty);

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
