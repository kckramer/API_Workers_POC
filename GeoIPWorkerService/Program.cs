using MaxMind.GeoIP2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GeoIPWorkerService
{
    public partial class Program
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
                logging.AddDebug();
                logging.AddEventLog();
            })
            .UseWindowsService()
            .ConfigureAppConfiguration((hostingContext, configBuilder) =>
            {
                configBuilder.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configBuilder
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);




            })
            .ConfigureServices((hostingContext, services) =>
            {
                services.AddHostedService<Worker>();

                IConfiguration configuration = hostingContext.Configuration;
                WorkerOptions options = configuration.GetSection("MaxMind").Get<WorkerOptions>();

                services.AddSingleton(options);
            });
    }
}
