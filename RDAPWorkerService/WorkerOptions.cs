namespace RDAPWorkerService
{
    public class WorkerOptions
    {
        public string GoogleCloudAPIKey { get; set; }

        public string QueueName { get; set; }

        public string ResponseQueueName { get; set; }
    }
}