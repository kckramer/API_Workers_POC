using System;
using System.Collections.Generic;
using System.Text;

namespace RDAPWorkerService.Models
{
    public class Output
    {
        public Guid CorrelationId { get; set; }

        public string Data { get; set; }
    }
}
