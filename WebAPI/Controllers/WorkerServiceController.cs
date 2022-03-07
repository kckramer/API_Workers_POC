using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkerServiceController : ControllerBase
    {
        private static readonly WorkerService[] WorkerServices = new[]
        {
            new WorkerService("GeoIP Worker Service", "Description of GeoIP Worker Service"),
            new WorkerService("RDAP Worker Service", "Description of RDAP Worker Service")
        };

        private readonly ILogger<WorkerServiceController> _logger;
        private ServiceBusClient _client;
        private ServiceBusSenders _senders;
        private CollectorQueues _collectorQueues;

        public WorkerServiceController(ILogger<WorkerServiceController> logger,
            ServiceBusClient serviceBusClient,
            ServiceBusSenders serviceBusSenders,
            CollectorQueues collectorQueues)
        {
            _logger = logger;
            _client = serviceBusClient;
            _senders = serviceBusSenders;
            _collectorQueues = collectorQueues;
        }

        [HttpGet]
        public IEnumerable<WorkerService> Get()
        {
            return WorkerServices;
        }

        [HttpPost]
        public async Task<ActionResult<WorkerService>> Post(Input input)
        {
            if (string.IsNullOrEmpty(input.IPAddress) && string.IsNullOrEmpty(input.DomainAddress))
            {
                return BadRequest();
            }

            if (!input.Services.Any())
            {
                input.Services = WorkerServices;
            }

            var tasks = new List<Task>();
            var workerServicesMessagesSent = new List<string>();
            var correlationId = Guid.NewGuid();

            foreach(var service in input.Services)
            {
                if (service.Name == "GeoIP Worker Service" && !string.IsNullOrEmpty(input.IPAddress))
                {
                    var messageBody = new InputMessage(correlationId, input.IPAddress);
                    var task = _senders.GeoIPSender.SendMessageAsync(new ServiceBusMessage(messageBody.ToString()));

                    tasks.Add(task);
                    workerServicesMessagesSent.Add(service.Name);
                }
                else if (service.Name == "RDAP Worker Service" && !string.IsNullOrEmpty(input.DomainAddress))
                {
                    var messageBody = new InputMessage(correlationId, input.DomainAddress);
                    var task = _senders.RDAPSender.SendMessageAsync(new ServiceBusMessage(messageBody.ToString()));

                    tasks.Add(task);
                    workerServicesMessagesSent.Add(service.Name);
                }
            }

            await Task.WhenAll(tasks);

            var result = await CollectResponsesAsync(workerServicesMessagesSent);

            return Ok(result);
        }

        private async Task<Output> CollectResponsesAsync(List<string> workerServicesMessagesSent)
        {
            var result = false;

            while (!result)
            {
                await Task.Delay(1000);

                foreach (var message in workerServicesMessagesSent)
                {
                    
                }
            }

            return new Output();
        }
    }
}
