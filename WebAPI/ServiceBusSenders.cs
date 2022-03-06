using Azure.Messaging.ServiceBus;

namespace WebAPI
{
    public class ServiceBusSenders
    {
        public ServiceBusSenders(ServiceBusSender geoIPSender, ServiceBusSender rdapSender)
        {
            GeoIPSender = geoIPSender;
            RDAPSender = rdapSender;
        }

        public ServiceBusSender GeoIPSender { get; set; }

        public ServiceBusSender RDAPSender { get; set; }
    }
}
