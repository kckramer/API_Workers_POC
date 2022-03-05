using System;

namespace WebAPI
{
    public class Input
    {
        public string DomainAddress { get; set; }

        public string IPAddress { get; set; }

        public WorkerService[] Services { get; set; }
    }
}
