using IotPlatformDemo.Domain.AggregateRoots.Device;
using IotPlatformDemo.Domain.Events.Base.V1;
using IotPlatformDemo.Domain.Events.Device.V1;
using IotPlatformDemo.Functions.General;
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
        [OrchestrationTrigger] TaskOrchestrationContext context, (string, string) userIdEventStringTuple)
    {
        var userId = userIdEventStringTuple.Item1;
        var eventAsString = userIdEventStringTuple.Item2;
        try
        {
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), new OrchestrationStatus
            {
                Status = OrchestrationStatus.StatusCode.Start,
                UserId = userId,
                OrchestrationId = context.InstanceId
            });
        
            var aggregateRoot = await context.CallActivityAsync<DeviceAggregateRoot>(nameof(Device_UpdateAggregateRoot), 
                eventAsString);
            await context.CallActivityAsync(nameof(Device_UpdateMaterializedViews), aggregateRoot);
        
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), new OrchestrationStatus
            {
                Status = OrchestrationStatus.StatusCode.Success,
                UserId = userId,
                OrchestrationId = context.InstanceId
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Orchestration failed.");
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), new OrchestrationStatus
            {
                Status = OrchestrationStatus.StatusCode.Fail,
                UserId = userId,
                OrchestrationId = context.InstanceId
            });
            throw;
        }
    }

    [Function(nameof(Device_UpdateAggregateRoot))]
    public DeviceAggregateRoot Device_UpdateAggregateRoot([ActivityTrigger] string eventAsString,
        FunctionContext executionContext)
    {
        var eventsAssembly = typeof(Event).Assembly;
        var jsonObject = (JsonConvert.DeserializeObject(eventAsString) as JObject)!;
        var eventType = eventsAssembly.GetType($"{jsonObject.GetValue("version")}", true)!;
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