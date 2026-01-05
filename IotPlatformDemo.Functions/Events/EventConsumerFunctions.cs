using System.Text;
using Azure.Messaging.ServiceBus;
using IotPlatformDemo.Domain.Events.Base.V1;
using IotPlatformDemo.Domain.Events.Device.V1;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IotPlatformDemo.Functions.Events;

public class EventConsumerFunctions(ILogger<EventConsumerFunctions> logger)
{
    [Function(nameof(Event_StartOrchestration))]
    public async Task Event_StartOrchestration(
        [ServiceBusTrigger("events", "events-subscription", Connection = "ServiceBus", 
            IsBatched = false, IsSessionsEnabled = true)] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient client, FunctionContext executionContext)
    {
        try
        {
            var eventType = Enum.Parse<EventType>(message.ApplicationProperties[nameof(Event.Type)].ToString()!);
            var eventAsString = Encoding.UTF8.GetString(message.Body);
            var userId = message.ApplicationProperties[nameof(Event.UserId)].ToString()!;
            var eventId = message.ApplicationProperties[nameof(Event.Id)].ToString()!;
            
            logger.LogInformation("Event received: {eventType}", eventType);

            switch (eventType)
            {
                case EventType.DeviceEvent:
                    await client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(EventHandler.Device.DeviceEventHandlerFunctions.Device_RunEventOrchestrator),
                        Tuple.Create(userId, eventAsString),
                        new StartOrchestrationOptions(eventId)
                        );
                    break;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Orchestration by event could not be started.");
            throw;
        }
    }
}