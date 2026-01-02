using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web.Resource;

namespace IotPlatformDemo.API;

[Authorize]
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
    RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
)]
public class ChatSampleHub : Hub
{
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnected: {Context.UserIdentifier}, exception: {exception?.Message}");
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connected: {Context.UserIdentifier} with connection: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    /*public Task BroadcastMessage(string name, string message) =>
        //user id from token is in Context.UserIdentifier here
        Clients.All.SendAsync("broadcastMessage", name, message);

    public Task Echo(string name, string message) =>
        Clients.Client(Context.ConnectionId)
            .SendAsync("echo", name, $"{message} (echo from server)");*/
}