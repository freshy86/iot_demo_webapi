using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions.General;

public class GeneralActivityFunctions(ILogger<GeneralActivityFunctions> logger, IServiceHubContext signalrHubContext)
{
    [Function(nameof(General_SignalOrchestrationStatusToFrontends))]
    public async Task General_SignalOrchestrationStatusToFrontends([ActivityTrigger] ClientNotification notification,
        FunctionContext executionContext)
    {
        logger.LogInformation("Signal status to frontend: {OrchestrationStatus}", notification);
        if (notification.Status == ClientNotification.NotificationStatus.Success && notification.Result == null)
        {
            throw new InvalidOperationException("Returning successful notification without a result.");
        }
        await signalrHubContext.Clients.User(notification.UserId).SendAsync("notification", 
            notification);
    }
}