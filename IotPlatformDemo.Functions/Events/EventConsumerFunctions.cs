using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions.Events;

public class EventConsumerFunctions(ILogger<EventConsumerFunctions> logger)
{
    [Function(nameof(EventConsumerFunctions))]
    public async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var outputs = new List<string>();

        // Replace name and input with values relevant for your Durable Functions Activity
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
        outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

        // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
        return outputs;
    }

    [Function(nameof(SayHello))]
    public string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
    {
        logger.LogInformation("Saying hello to {name}.", name);
        return $"Hello {name}!";
    }

    [Function(nameof(StartOrchestrationByEvent))]
    public async Task StartOrchestrationByEvent(
        [ServiceBusTrigger("events", "events-subscription", Connection = "ServiceBus", 
            IsBatched = false, IsSessionsEnabled = true)] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient client, FunctionContext executionContext)
    {
        
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(EventConsumerFunctions));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        //await serviceHubContext.Clients.User(e.UserId).SendAsync("notification", "system", $"Event received: {e.Type}, {e.Action} for user: {e.UserId}");
    }
    
    /*[Function("HelloOrchestration_HttpStart")]
    public async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(EventConsumerFunctions));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }*/
}