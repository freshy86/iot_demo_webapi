using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions.General;

public class GeneralActivityFunctions(ILogger<GeneralActivityFunctions> logger, IServiceHubContext signalrHubContext)
{
    [Function(nameof(General_SignalOrchestrationStatusToFrontends))]
    public async Task General_SignalOrchestrationStatusToFrontends([ActivityTrigger] OrchestrationStatus status,
        FunctionContext executionContext)
    {
        logger.LogInformation("Signal status to frontend: {OrchestrationStatus}", status);
        await signalrHubContext.Clients.User(status.UserId).SendAsync("notification", 
            status);
    }
}