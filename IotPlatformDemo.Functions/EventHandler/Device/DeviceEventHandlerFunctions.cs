using IotPlatformDemo.Domain.AggregateRoots.Device;
using IotPlatformDemo.Domain.Events.Base.V1;
using IotPlatformDemo.Domain.Events.Device.V1;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IotPlatformDemo.Functions.EventHandler.Device;

public class DeviceEventHandlerFunctions(ILogger<DeviceEventHandlerFunctions> logger)
{
    [Function(nameof(Device_RunEventOrchestrator))]
    public async Task Device_RunEventOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context, string eventAsString)
    {
        var aggregateRoot = await context.CallActivityAsync<DeviceAggregateRoot>(nameof(Device_UpdateAggregateRoot), 
            eventAsString);
        await context.CallActivityAsync(nameof(Device_UpdateMaterializedViews), aggregateRoot);
    }

    [Function(nameof(Device_UpdateAggregateRoot))]
    public DeviceAggregateRoot Device_UpdateAggregateRoot([ActivityTrigger] string eventAsString,
        FunctionContext executionContext)
    {
        var eventsAssembly = typeof(Event).Assembly;
        var jsonObject = JsonConvert.DeserializeObject(eventAsString) as JObject;
        var eventType = eventsAssembly.GetType($"{jsonObject!.GetValue("version")}", true)!;
        var receivedEvent = (jsonObject.ToObject(eventType) as DeviceEvent)!;

        //TODO: Get it
        var aggregateRoot = new DeviceAggregateRoot(receivedEvent.PartitionKey);
        logger.LogInformation("Updating device aggregate root");
        receivedEvent.Apply(aggregateRoot);

        return aggregateRoot;
    }
    
    [Function(nameof(Device_UpdateMaterializedViews))]
    public void Device_UpdateMaterializedViews([ActivityTrigger] DeviceAggregateRoot aggregateRoot,
        FunctionContext executionContext)
    {
        logger.LogInformation("Updating device materialized views");
    }
}