using System.Text;
using Azure.Messaging.ServiceBus;
using IotPlatformDemo.Domain.Events;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

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
            var eventString = Encoding.UTF8.GetString(message.Body);
            var receivedEvent = eventString.DeserializeEvent<Event>();
            
            logger.LogInformation("Event received: {eventType}", receivedEvent.Type);

            switch (receivedEvent.Type)
            {
                case EventType.DeviceEvent:
                    await client.ScheduleNewOrchestrationInstanceAsync(
                        nameof(EventHandler.Device.DeviceEventHandlerFunctions.Device_RunEventOrchestrator),
                        (receivedEvent.UserId, eventString),
                        new StartOrchestrationOptions(receivedEvent.Id)
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