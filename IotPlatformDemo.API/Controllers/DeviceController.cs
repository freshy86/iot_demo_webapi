using System.Security.Claims;
using IotPlatformDemo.Application.EventStore;
using IotPlatformDemo.Domain.Events.Device.V1;
using IotPlatformDemo.Domain.Helpers;
using IotPlatformDemo.Domain.MaterializedViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Devices;
using Microsoft.Identity.Web.Resource;

namespace IotPlatformDemo.API.Controllers;

[Authorize]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("[controller]")]
public class DeviceController(
    RegistryManager registryManager,
    IHttpContextAccessor contextAccessor,
    IEventStore eventStore, 
    Container readDataContainer)
    : ControllerBase
{
    [Route("{deviceId}")]
    [HttpGet]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
    )]
    public async Task<IActionResult> GetDevice(string deviceId)
    {
        var userId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            DeviceView view = await readDataContainer.ReadItemAsync<DeviceView>(deviceId, new PartitionKey(userId));
            return Ok(view);
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
    )]
    public async Task<IActionResult> GetDevices(int maxItems = 0, string? continuationToken = null)
    {
        var userId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            var query = new QueryDefinition("SELECT * FROM data d WHERE d.type = @type")
                .WithParameter("@type", nameof(DeviceView));

            var result = await Helpers.QueryHelpers.GetMultipleItemsQuery<DeviceView>(
                readDataContainer,
                query, 
                new PartitionKey(userId),
                maxItems,
                continuationToken
            );
            
            return Ok(result);
        }
        catch (CosmosException e)
        {
            return BadRequest(e);
        }
    }
    
    [HttpPut]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Write",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Write"
    )]
    public async Task<IActionResult> RenameDevice(string deviceId, string newDeviceName)
    {
        var userId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("User not found");
        }
        var renameDeviceEvent = new DeviceRenameEvent(deviceId, userId, newDeviceName);
        await eventStore.Append(renameDeviceEvent);
        return Ok(renameDeviceEvent.Id);
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Write",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Write"
    )]
    public async Task<IActionResult> AddNewDevice([FromBody] string deviceName)
    {
        var deviceId = GuidHelpers.NewSimpleGuidString();
        var userId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return BadRequest("User not found");
        }

        //Device newDevice = new(iotDeviceId);
        //var addedDevice = await registryManager.AddDeviceAsync(newDevice);

        var createDeviceEvent = new DeviceCreatedEvent(deviceId, userId)
        {
            DeviceName = deviceName
        };
        
        Console.WriteLine($"Added new IoT device with ID: {deviceId}");
        await eventStore.Append(createDeviceEvent);
        
        return Ok(createDeviceEvent.Id);
        //Console.WriteLine($"Device Key: {addedDevice.Authentication.SymmetricKey.PrimaryKey}");
    }
}