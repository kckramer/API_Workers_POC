# Summary
Web API and workers proof of concept. Pass in domain and/or ip address and find detailed information on it using geo ip and rdap.

# Debug Steps
1. Pull down code.
2. Run web api project and worker projects.
3. Fill in POST request via Swagger UI for domain and ip address (Example: "google.com" "74.125.21.138").

# Dependencies
1. Geo IP <a href="https://github.com/maxmind/GeoIP2-dotnet">GeoIP2<a>
2. Google API <a href="https://github.com/googleapis/google-api-dotnet-client">Google API<a>
3. Azure Service Bus <a href="https://github.com/Azure/azure-sdk-for-net/blob/Azure.Messaging.ServiceBus_7.6.0/sdk/servicebus/Azure.Messaging.ServiceBus/README.md">Azure Service Bus<a>
4. Swagger/Swashbuckle <a href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore">Swagger/Swashbuckle<a>

# Difficulties Encountered
1. Google API for domain details always fails with the same message regardless of passed in domain.  Possible resolution steps include changing the way I call that to a more typical URL request instead of using internal google service.
2. Deploy to Azure failed with docker containers.  Think this is due to container and port and possible SSL issues.  Could not resolve to deploy.  Went through the motions though and was a fun learning experience. :)

# Possible Improvements
1. Unit testing around workers and process of gathering results from all workers.
2. Improve Google API worker to get it to function properly.
3. Add Ping worker.
4. Refactor opportunities around workers and gathering data and service bus messages (pub/sub, topics).  Reference some existing patterns to not re-invent the wheel.
5. Troubleshoot azure deployment so the containers can deploy and work properly.