using IotPlatformDemo.Application;
using IotPlatformDemo.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IotPlatformDemo.Functions;

public class MyHttpTrigger
{
    private readonly ILogger<MyHttpTrigger> _logger;

    public MyHttpTrigger(ILogger<MyHttpTrigger> logger)
    {
        _logger = logger;
    }

    [Function("MyHttpTrigger")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        DataObject<Device> dataObject = new("id", "partitionKey", new Device { Name = "MyDevice" });
        return new OkObjectResult($"Welcome to Azure Functions! { dataObject }");
    }
}
