using System.Net;
using System.Security.Claims;
using IotPlatformDemo.Domain.Events;
using IotPlatformDemo.Domain.Events.Device;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Identity.Web.Resource;

[Authorize]
[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerBase
{
    private readonly RegistryManager _registryManager;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IEventStore _eventStore;
    
    public DeviceController(RegistryManager registryManager,
                            IHttpContextAccessor contextAccessor,
                            IEventStore eventStore)
    {
        _registryManager = registryManager;
        _contextAccessor = contextAccessor;
        _eventStore = eventStore;
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Write",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Write"
    )]
    public async Task<IActionResult> AddNewDevice()
    {
        var deviceId = Guid.NewGuid();
        var userId = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return BadRequest("User not found");
        }

        Device newDevice = new($"{userId}-{deviceId}");
        var addedDevice = await _registryManager.AddDeviceAsync(newDevice);

        Console.WriteLine($"Added new IoT device with ID: {addedDevice.Id}");
        _eventStore.Append(new DeviceCreatedEvent(addedDevice.Id, userId, "myNewDevice"));
        
        return Ok();
        //Console.WriteLine($"Device Key: {addedDevice.Authentication.SymmetricKey.PrimaryKey}");
    }
}