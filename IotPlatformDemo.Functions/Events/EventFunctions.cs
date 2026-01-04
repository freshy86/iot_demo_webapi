using System.Dynamic;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IotPlatformDemo.Functions.Events;

public class EventFunctions(ILogger<EventFunctions> logger, 
    IServiceHubContext signalrServiceHubContext,
    ServiceBusSender serviceBusSender)
{
    [Function(nameof(EventToServiceBusForwarding))]
    [FixedDelayRetry(10, "00:00:03")]
    public async Task EventToServiceBusForwarding([CosmosDBTrigger(
            databaseName: "iot_demo_write",
            containerName: "events",
            Connection = "CosmosDb",
            LeaseContainerName = "leases",
            LeaseContainerPrefix = $"events{LeaseContainerPrefixConstants.Extension}",
            CreateLeaseContainerIfNotExists = true)] List<ExpandoObject> events,
        FunctionContext context, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("C# Cosmos DB trigger function processed {count} documents.", events?.Count ?? 0);
        
            Dictionary<string, List<ServiceBusMessage>> serviceBusMessages = new();
            var eventsCount = 0;
            
            if (events is not null && events.Count != 0)
            {
                foreach (var e in events as dynamic)
                {
                    var partitionKey = e.partitionKey;
                    var serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(e))
                    {
                        ContentType = "application/json;charset=utf-8",
                        Subject = e.type.ToString(),
                        MessageId = e.id.ToString(),
                        SessionId = partitionKey
                    };

                    // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
                    if (serviceBusMessages.ContainsKey(partitionKey))
                    {
                        serviceBusMessages[partitionKey].Add(serviceBusMessage);
                    }
                    else
                    {
                        serviceBusMessages[partitionKey] = new List<ServiceBusMessage> { serviceBusMessage };
                    }

                    eventsCount += 1;
                    //await serviceHubContext.Clients.User(e.UserId).SendAsync("notification", "system", $"Event received: {e.Type}, {e.Action} for user: {e.UserId}");
                }

                if (serviceBusMessages.Count > 0)
                {
                    logger.LogInformation("Sending events {EventsCount} to service bus.", eventsCount);

                    foreach (var partitionKey in serviceBusMessages.Keys)
                    {
                        using var messageBatch = await serviceBusSender.CreateMessageBatchAsync(cancellationToken);
                        if (serviceBusMessages[partitionKey].Any(serviceBusMessage => !messageBatch.TryAddMessage(serviceBusMessage)))
                        {
                            throw new Exception("Could not add message to batch");
                        }

                        await serviceBusSender.SendMessagesAsync(messageBatch, cancellationToken);
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occured during processing of events");
            throw;
        }
    }
}