using System;

namespace RDAPWorkerService.Models
{
    public class InputMessage
    {
        public Guid CorrelationId { get; set; }
        public string Data { get; set; }

        public InputMessage(Guid correlationId, string data)
        {
            CorrelationId = correlationId;
            Data = data;
        }
    }
}