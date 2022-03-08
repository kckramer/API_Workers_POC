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
using System.Runtime.Serialization;
using System.Xml;
using RDAPWorkerService.Models;
using System.Text.Json;
using System.IO;

namespace RDAPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerOptions _options;
        private DomainsRDAPService _domainsRDAPService;
        private ServiceBusClient _serviceBusClient;
        private ServiceBusSender _serviceBusSender;

        public Worker(ILogger<Worker> logger, DomainsRDAPService domainsRDAPService, WorkerOptions options, ServiceBusClient serviceBusClient, ServiceBusSender serviceBusSender)
        {
            _logger = logger;
            _domainsRDAPService = domainsRDAPService;
            _options = options;
            _serviceBusClient = serviceBusClient;
            _serviceBusSender = serviceBusSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
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
            var body = args.Message.Body;
            _logger.LogInformation($"Received: {body}");

            var output = JsonSerializer.Deserialize<Output>(body);

            output.Data = "Service unavailable";

            await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(output)));

            await args.CompleteMessageAsync(args.Message);
        }
    }
}
