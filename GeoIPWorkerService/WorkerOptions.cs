namespace GeoIPWorkerService
{
    public class WorkerOptions
    {
        public int AccountId { get; set; }

        public string LicenseKey { get; set; }

        public string QueueName { get; set; }

        public string WorkerResponseQueueName { get; set; }
    }
}
