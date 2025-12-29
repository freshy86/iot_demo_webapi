using IotPlatformDemo.Application;
using IotPlatformDemo.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace IotPlatformDemo.Functions;

public class DeviceAggregateRoot
{
    private readonly ILogger<DeviceAggregateRoot> _logger;

    public DeviceAggregateRoot(ILogger<DeviceAggregateRoot> logger)
    {
        _logger = logger;
    }

    [Function("MyDeviceAggregateRootFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        DataObject<Device> dataObject = new("id", "partitionKey", new Device { Name = "MyDevice" });
        return new OkObjectResult($"Welcome to Azure Functions! MyDeviceAggregateRootFunction { dataObject }");
    }
}