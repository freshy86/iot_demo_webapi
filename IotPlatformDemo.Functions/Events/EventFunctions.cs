using IotPlatformDemo.Application;
using IotPlatformDemo.Domain.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions.Events;

public class EventFunctions(ILogger<EventFunctions> logger, IServiceHubContext serviceHubContext)
{
    [Function(nameof(EventToServiceBusForwarding))]
    public async Task EventToServiceBusForwarding([CosmosDBTrigger(
            databaseName: "iot_demo_write",
            containerName: "events",
            Connection = "CosmosDb",
            LeaseContainerName = "leases",
            LeaseContainerPrefix = $"events{LeaseContainerPrefixConstants.Extension}",
            CreateLeaseContainerIfNotExists = true)] List<DataObject<Event>> dataObjects,
        FunctionContext context)
    {
        logger.LogInformation("C# Cosmos DB trigger function processed {count} documents.", dataObjects?.Count ?? 0);
        await serviceHubContext.Clients.All.SendAsync("BroadcastMessage", "system", "hallo");
        //await serviceHubContext.Clients.Clients(["O-wvhueJ4B-SpU3q3KhhcQx7A16wd02"]).SendAsync("echo", "system", "y0l0");
        if (dataObjects is not null && dataObjects.Any())
        {
            foreach (var obj in dataObjects)
            {
                logger.LogInformation("Data: {desc}", obj.Data);
            }
        }
    }
}