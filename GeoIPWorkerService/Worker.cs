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

                using (_webServiceClient = new WebServiceClient(_options.AccountId, _options.LicenseKey, host: "geolite.info"))
                {
                    var response = await _webServiceClient.CityAsync("192.168.1.1");

                    if (response != null)
                    {
                        var geoIPResult = new GeoIPResult(response);
                    }
                    
                    Console.WriteLine(response.Country.IsoCode);        // 'US'
                    Console.WriteLine(response.Country.Name);           // 'United States'

                    Console.WriteLine(response.MostSpecificSubdivision.Name);    // 'Minnesota'
                    Console.WriteLine(response.MostSpecificSubdivision.IsoCode); // 'MN'

                    Console.WriteLine(response.City.Name); // 'Minneapolis'

                    Console.WriteLine(response.Postal.Code); // '55455'

                    Console.WriteLine(response.Location.Latitude);  // 44.9733
                    Console.WriteLine(response.Location.Longitude); // -93.2323
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
