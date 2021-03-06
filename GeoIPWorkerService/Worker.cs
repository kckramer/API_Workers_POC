using Azure.Messaging.ServiceBus;
using MaxMind.GeoIP2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GeoIPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private WebServiceClient _webServiceClient;
        private readonly WorkerOptions _options;
        private ServiceBusClient _serviceBusClient;
        private ServiceBusSender _serviceBusSender;

        public Worker(ILogger<Worker> logger, WebServiceClient webServiceClient, WorkerOptions options, ServiceBusClient serviceBusClient, ServiceBusSender serviceBusSender)
        {
            _logger = logger;
            _webServiceClient = webServiceClient;
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

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation($"Received: {body}");

            var inputMessage = JsonSerializer.Deserialize<InputMessage>(args.Message.Body); 

            var response = await _webServiceClient.CityAsync(inputMessage.Data);

            if (response != null)
            {
                var geoIPResult = new GeoIPResult(response);
                geoIPResult.CorrelationId = inputMessage.CorrelationId;

                await _serviceBusSender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(geoIPResult)));
            }   

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
