using IotPlatformDemo.Application;
using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions.Events;

public class EventFunctions(ILogger<EventFunctions> logger)
{
    [Function(nameof(EventToServiceBusForwarding))]
    public void EventToServiceBusForwarding([CosmosDBTrigger(
            databaseName: "iot_demo_write",
            containerName: "events",
            Connection = "CosmosDb",
            LeaseContainerName = "leases",
            LeaseContainerPrefix = $"events{ChangeFeedContainerPrefixConstants.Extension}",
            CreateLeaseContainerIfNotExists = true)] List<DataObject<Event>> dataObjects,
        FunctionContext context)
    {
        logger.LogInformation("C# Cosmos DB trigger function processed {count} documents.", dataObjects?.Count ?? 0);
        if (dataObjects is not null && dataObjects.Any())
        {
            foreach (var obj in dataObjects)
            {
                logger.LogInformation("Data: {desc}", obj.Data);
            }
        }
    }
}