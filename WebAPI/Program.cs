using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.Sources.Clear();

                    configBuilder
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                })
                .ConfigureServices((hostingContext, services) =>
                {
                    IConfiguration configuration = hostingContext.Configuration;
                    var connString = configuration.GetValue<string>("AzureServiceBus");

                    if (connString != null)
                    {
                        var serviceBusClient = new ServiceBusClient(connString);
                        services.AddSingleton(serviceBusClient);

                        var queue1 = configuration.GetValue<string>("WorkerQueue1");
                        var queue2 = configuration.GetValue<string>("WorkerQueue2");

                        var geoIPQueueSender = serviceBusClient.CreateSender(queue1);
                        var rdapQueueSender = serviceBusClient.CreateSender(queue2);

                        var senders = new ServiceBusSenders(geoIPQueueSender, rdapQueueSender);

                        services.AddSingleton(senders);

                        var responseQueue1 = configuration.GetValue<string>("WorkerResponseQueue1");
                        var responseQueue2 = configuration.GetValue<string>("WorkerResponseQueue2");

                        var collectorQueues = new CollectorQueues(responseQueue1, responseQueue2);
                        services.AddSingleton(collectorQueues);
                    }
                });
    }

    public class CollectorQueues
    {
        public string responseQueue1 { get; set; }
        public string responseQueue2 { get; set; }

        public CollectorQueues(string responseQueue1, string responseQueue2)
        {
            this.responseQueue1 = responseQueue1;
            this.responseQueue2 = responseQueue2;
        }
    }
}
