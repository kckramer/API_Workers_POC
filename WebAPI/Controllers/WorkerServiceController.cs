using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
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

            foreach (var service in input.Services)
            {
                if (service.Name == "GeoIP Worker Service" && !string.IsNullOrEmpty(input.IPAddress))
                {
                    var messageBody = new InputMessage(correlationId, input.IPAddress);
                    var json = JsonSerializer.Serialize(messageBody);

                    var task = _senders.GeoIPSender.SendMessageAsync(new ServiceBusMessage(json));

                    tasks.Add(task);
                    workerServicesMessagesSent.Add(_collectorQueues.responseQueue1);
                }
                else if (service.Name == "RDAP Worker Service" && !string.IsNullOrEmpty(input.DomainAddress))
                {
                    var messageBody = new InputMessage(correlationId, input.DomainAddress);
                    string json = JsonSerializer.Serialize(messageBody);

                    var task = _senders.RDAPSender.SendMessageAsync(new ServiceBusMessage(json));

                    tasks.Add(task);
                    workerServicesMessagesSent.Add(_collectorQueues.responseQueue2);
                }
            }

            await Task.WhenAll(tasks);

            var result = await CollectResponsesAsync(correlationId, workerServicesMessagesSent);

            return Ok(result);
        }

        private async Task<Output> CollectResponsesAsync(Guid correlationId, List<string> workerServicesMessagesSent)
        {
            var doneProcessing = false;

            var responseReceiver1 = _client.CreateReceiver(_collectorQueues.responseQueue1);
            var responseReceiver2 = _client.CreateReceiver(_collectorQueues.responseQueue2);

            var resultOutput = new Output();
            var count = 0;

            while (!doneProcessing && count <= 30)
            {
                await Task.Delay(1000);

                var worker1Messages = new List<ServiceBusReceivedMessage>();
                var worker2Messages = new List<ServiceBusReceivedMessage>();

                if (workerServicesMessagesSent.Contains(_collectorQueues.responseQueue1))
                {
                    worker1Messages.AddRange(await responseReceiver1.ReceiveMessagesAsync(10));
                }

                if (workerServicesMessagesSent.Contains(_collectorQueues.responseQueue2))
                {
                    worker2Messages.AddRange(await responseReceiver2.ReceiveMessagesAsync(10));
                }

                var messagesToProcess = new List<TempOutput>();

                foreach (var message in worker1Messages)
                {

                    try
                    {
                        // deserialize the JSON string into TestModel
                        var output = JsonSerializer.Deserialize<Output>(message.Body);

                        if (output.CorrelationId == correlationId)
                        {
                            messagesToProcess.Add(new TempOutput
                            {
                                message = message,
                                Output = output,
                                WorkerIndex = 0
                            });

                            break;
                        }
                        else
                        {
                            await responseReceiver1.AbandonMessageAsync(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        await responseReceiver1.CompleteMessageAsync(message);
                    }

                    
                }

                foreach (var message in worker2Messages)
                {
                    try
                    {
                        var output = JsonSerializer.Deserialize<Output>(message.Body);

                        if (output.CorrelationId == correlationId)
                        {
                            messagesToProcess.Add(new TempOutput
                            {
                                message = message,
                                Output = output,
                                WorkerIndex = 1
                            });
                            
                            break;
                        }
                        else
                        {
                            await responseReceiver2.AbandonMessageAsync(message);
                        }
                    }
                    catch
                    {
                        await responseReceiver2.CompleteMessageAsync(message);
                    }
                    
                }

                if (messagesToProcess.Count == workerServicesMessagesSent.Count)
                {
                    foreach (var message in messagesToProcess)
                    {
                        Output.CopyValues(resultOutput, message.Output);

                        if (message.WorkerIndex == 0)
                        {
                            await responseReceiver1.CompleteMessageAsync(message.message);
                        }
                        else if (message.WorkerIndex == 1)
                        {
                            await responseReceiver2.CompleteMessageAsync(message.message);
                        }
                    }

                    doneProcessing = true;
                }

                count++;
            }

            return resultOutput;
        }
    }
}
