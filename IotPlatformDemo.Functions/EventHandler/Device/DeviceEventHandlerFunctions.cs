using IotPlatformDemo.Domain.AggregateRoots.Device;
using IotPlatformDemo.Domain.Events.Device.V1;
using IotPlatformDemo.Domain.MaterializedViews;
using IotPlatformDemo.Functions.Events;
using IotPlatformDemo.Functions.General;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Action = IotPlatformDemo.Domain.Events.Action;

namespace IotPlatformDemo.Functions.EventHandler.Device;

public class DeviceEventHandlerFunctions(ILogger<DeviceEventHandlerFunctions> logger, 
    [FromKeyedServices(ContainerType.Write)] Container writeContainer,
    [FromKeyedServices(ContainerType.Read)] Container readContainer)
{
    [Function(nameof(Device_RunEventOrchestrator))]
    public async Task Device_RunEventOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context, (string, string) userIdEventStringTuple)
    {
        var userId = userIdEventStringTuple.Item1;
        var eventString = userIdEventStringTuple.Item2;

        var clientNotification = new ClientNotification
        {
            Context = ClientNotification.NotificationContext.DeviceCreate,
            UserId = userId,
            OrchestrationId = context.InstanceId
        };
        
        try
        {
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), clientNotification);
        
            var options = TaskOptions.FromRetryPolicy(new RetryPolicy(
                maxNumberOfAttempts: 5,
                firstRetryInterval: TimeSpan.FromSeconds(1)));
            
            var aggregateRoot = await context.CallActivityAsync<DeviceAggregateRoot>(nameof(Device_UpdateAggregateRoot), 
                eventString, options);
            await context.CallActivityAsync(nameof(Device_UpdateMaterializedViews), (aggregateRoot, eventString),
                options);

            clientNotification.Result = aggregateRoot.Id;
            clientNotification.Status = ClientNotification.NotificationStatus.Success;
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), clientNotification);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Orchestration failed.");
            clientNotification.Status = ClientNotification.NotificationStatus.Fail;
            await context.CallActivityAsync(nameof(GeneralActivityFunctions.General_SignalOrchestrationStatusToFrontends), clientNotification);
            throw;
        }
    }
    
    [Function(nameof(Device_UpdateAggregateRoot))]
    public async Task<DeviceAggregateRoot> Device_UpdateAggregateRoot([ActivityTrigger] string eventString,
        FunctionContext executionContext)
    {
        var receivedEvent = eventString.DeserializeEvent<DeviceEvent>();
        
        DeviceAggregateRoot? aggregateRoot;
        if (receivedEvent.Action == Action.Create)
        {
            aggregateRoot = new DeviceAggregateRoot(receivedEvent.PartitionKey);
        }
        else
        {
            aggregateRoot = await writeContainer.ReadItemAsync<DeviceAggregateRoot>(receivedEvent.DeviceId,
                new PartitionKey(receivedEvent.DeviceId), null, executionContext.CancellationToken);
        }

        if (aggregateRoot == null)
        {
            throw new Exception("Could not retrieve or create aggregate root");
        }
        
        logger.LogInformation("Updating device aggregate root");
        receivedEvent.Apply(aggregateRoot);

        var requestOptions = new ItemRequestOptions
        {
            IfMatchEtag = aggregateRoot.ETag
        };

        aggregateRoot = await writeContainer.UpsertItemAsync(aggregateRoot, new PartitionKey(receivedEvent.DeviceId), requestOptions,
            executionContext.CancellationToken);

        return aggregateRoot;
    }
    
    [Function(nameof(Device_UpdateMaterializedViews))]
    public async Task Device_UpdateMaterializedViews([ActivityTrigger] (DeviceAggregateRoot, string) aggregateRootEventAsStringTuple,
        FunctionContext executionContext)
    {
        var aggregateRoot = aggregateRootEventAsStringTuple.Item1;
        var eventString = aggregateRootEventAsStringTuple.Item2;
        var receivedEvent = eventString.DeserializeEvent<DeviceEvent>();

        var viewId = aggregateRoot.Id;
        var partitionKeyValue = receivedEvent.UserId;
        var partitionKey = new PartitionKey(partitionKeyValue);
        
        DeviceView? deviceView;
        try
        {
            deviceView = await readContainer.ReadItemAsync<DeviceView>(viewId,
                partitionKey);
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            deviceView = new DeviceView(viewId, partitionKeyValue);
        }

        var requestOptions = new ItemRequestOptions
        {
            EnableContentResponseOnWrite = false
        };
        
        logger.LogInformation("Updating device materialized views");
        aggregateRoot.ApplyTo(deviceView);
        
        await readContainer.UpsertItemAsync(deviceView, partitionKey, requestOptions);
    }
}