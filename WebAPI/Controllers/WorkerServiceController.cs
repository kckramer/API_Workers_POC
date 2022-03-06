using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public WorkerServiceController(ILogger<WorkerServiceController> logger, ServiceBusClient serviceBusClient, ServiceBusSenders serviceBusSenders)
        {
            _logger = logger;
            _client = serviceBusClient;
            _senders = serviceBusSenders;
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

            foreach(var service in input.Services)
            {
                if (service.Name == "GeoIP Worker Service" && !string.IsNullOrEmpty(input.IPAddress))
                {
                    var task = _senders.GeoIPSender.SendMessageAsync(new ServiceBusMessage(input.IPAddress));

                    tasks.Add(task);
                }
                else if (service.Name == "RDAP Worker Service" && !string.IsNullOrEmpty(input.DomainAddress))
                {
                    var task = _senders.RDAPSender.SendMessageAsync(new ServiceBusMessage(input.DomainAddress));
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);    

            return Ok(input);
        }
    }
}
