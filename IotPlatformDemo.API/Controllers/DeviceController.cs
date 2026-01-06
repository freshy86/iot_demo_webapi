using System.Security.Claims;
using IotPlatformDemo.Application.EventStore;
using IotPlatformDemo.Domain.Events.Device.V1;
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
    [HttpGet]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
    )]
    public async Task<IActionResult> GetDevice(string deviceId)
    {
        var viewId = DeviceView.IdPrefix + deviceId;
        try
        {
            DeviceView view = await readDataContainer.ReadItemAsync<DeviceView>(viewId, new PartitionKey(viewId));
            return Ok(view);
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
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
        var deviceId = Guid.NewGuid();
        var userId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return BadRequest("User not found");
        }

        var iotDeviceId = $"{userId}-{deviceId}";
        //Device newDevice = new(iotDeviceId);
        //var addedDevice = await registryManager.AddDeviceAsync(newDevice);

        var createDeviceEvent = new DeviceCreatedEvent(iotDeviceId, userId)
        {
            DeviceName = deviceName
        };
        
        Console.WriteLine($"Added new IoT device with ID: {iotDeviceId}");
        await eventStore.Append(createDeviceEvent);
        
        return Ok(createDeviceEvent.Id);
        //Console.WriteLine($"Device Key: {addedDevice.Authentication.SymmetricKey.PrimaryKey}");
    }
}