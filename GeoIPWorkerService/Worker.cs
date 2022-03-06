using Azure.Messaging.ServiceBus;
using MaxMind.GeoIP2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeoIPWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private WebServiceClient _webServiceClient;
        private readonly WorkerOptions _options;
        private ServiceBusClient _serviceBusClient;

        public Worker(ILogger<Worker> logger, WebServiceClient webServiceClient, WorkerOptions options, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _webServiceClient = webServiceClient;
            _options = options;
            _serviceBusClient = serviceBusClient;
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

            var response = await _webServiceClient.CityAsync(body);

            if (response != null)
            {
                var geoIPResult = new GeoIPResult(response);
            }

            _logger.LogInformation(response.Country.IsoCode);        // 'US'
            _logger.LogInformation(response.Country.Name);           // 'United States'

            _logger.LogInformation(response.MostSpecificSubdivision.Name);    // 'Minnesota'
            _logger.LogInformation(response.MostSpecificSubdivision.IsoCode); // 'MN'

            _logger.LogInformation(response.City.Name); // 'Minneapolis'

            _logger.LogInformation(response.Postal.Code); // '55455'

            _logger.LogInformation(response.Location.Latitude.ToString());  // 44.9733
            _logger.LogInformation(response.Location.Longitude.ToString()); // -93.2323
            

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
