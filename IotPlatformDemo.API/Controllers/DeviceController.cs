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

    public DeviceController(RegistryManager registryManager)
    {
        _registryManager = registryManager;
    }

    [HttpPost]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAD:Scopes:Write",
        RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Write"
    )]
    public void AddNewDevice()
    {
        var configId = Guid.NewGuid().ToString();
        
        Device newDevice = new Device(configId);
        var addedDevice = _registryManager.AddDeviceAsync(newDevice).GetAwaiter().GetResult();

        Console.WriteLine($"Added new IoT device with ID: {addedDevice.Id}");
        //Console.WriteLine($"Device Key: {addedDevice.Authentication.SymmetricKey.PrimaryKey}");
    }
}