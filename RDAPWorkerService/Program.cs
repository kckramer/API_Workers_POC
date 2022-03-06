using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Google.Apis.Services;
using Google.Apis.DomainsRDAP.v1;
using Azure.Messaging.ServiceBus;

namespace RDAPWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().RunAsync().Wait();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .UseWindowsService()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configBuilder
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    IConfiguration configuration = hostingContext.Configuration;
                    var apiKey = configuration.GetValue<string>("GoogleCloudAPIKey");

                    var workerOptions = new WorkerOptions();

                    workerOptions.GoogleCloudAPIKey = apiKey;

                    var domainsRDAPService = new DomainsRDAPService(new BaseClientService.Initializer
                    {
                        ApplicationName = "domainsrdap",
                        ApiKey = workerOptions.GoogleCloudAPIKey,
                    });

                    services.AddSingleton(domainsRDAPService);

                    var connString = configuration.GetValue<string>("AzureServiceBus");

                    if (connString != null)
                    {
                        var serviceBusClient = new ServiceBusClient(connString);
                        services.AddSingleton(serviceBusClient);

                        var rdapQueue = configuration.GetValue<string>("WorkerQueue");
                        workerOptions.QueueName = rdapQueue;
                    }

                    services.AddSingleton(workerOptions);
                });
    }
}
