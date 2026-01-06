using System.Dynamic;
using System.Text;
using Azure.Messaging.ServiceBus;
using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IotPlatformDemo.Functions.Events;

public class EventProducerFunctions(ILogger<EventProducerFunctions> logger, 
    IServiceHubContext signalrServiceHubContext,
    ServiceBusSender serviceBusSender)
{
    [Function(nameof(Event_ForwardToServiceBus))]
    [FixedDelayRetry(10, "00:00:03")]
    public async Task Event_ForwardToServiceBus([CosmosDBTrigger(
            databaseName: "iot_demo_write",
            containerName: "events",
            Connection = "CosmosDb",
            LeaseContainerName = "leases",
            LeaseContainerPrefix = "%LeaseContainerPrefix%",
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
                    string partitionKey = e.partitionKey;
                    var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e)))
                    {
                        ContentType = "application/json;charset=utf-8",
                        Subject = e.type.ToString(),
                        MessageId = e.id,
                        SessionId = partitionKey
                    };
                    serviceBusMessage.ApplicationProperties.Add(nameof(Event.Version), e.version);
                    serviceBusMessage.ApplicationProperties.Add(nameof(Event.Type), e.type);
                    serviceBusMessage.ApplicationProperties.Add(nameof(Event.Id), e.id);
                    serviceBusMessage.ApplicationProperties.Add(nameof(Event.UserId), e.userId);

                    if (serviceBusMessages.TryGetValue(partitionKey, out var value))
                    {
                        value.Add(serviceBusMessage);
                    }
                    else
                    {
                        serviceBusMessages[partitionKey] = [serviceBusMessage];
                    }

                    eventsCount += 1;
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