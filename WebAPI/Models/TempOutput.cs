using Azure.Messaging.ServiceBus;

namespace WebAPI.Models
{
    public class TempOutput
    {
        public ServiceBusReceivedMessage message { get; set; }

        public Output Output { get; set; }

        public int WorkerIndex { get; set; }
    }
}