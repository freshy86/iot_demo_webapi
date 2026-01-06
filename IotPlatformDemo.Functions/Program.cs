using Azure.Core.Serialization;
using Azure.Messaging.ServiceBus;
using IotPlatformDemo.Application.Notifications;
using IotPlatformDemo.Functions.General;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

//Due to bugs in HTTP worker and/or JSON.Net this is needed
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.Configure<WorkerOptions>(workerOptions =>
{
    var settings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();
    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(settings);
});

var signalrServiceManager = new ServiceManagerBuilder()
    .WithOptions(option =>
    {
        option.ConnectionString = builder.Configuration.GetSection("SignalR").Value;
    })
    .BuildServiceManager();

var signalrServiceHubContext = await signalrServiceManager.CreateHubContextAsync(nameof(ClientNotificationHub), CancellationToken.None);

ServiceBusClient serviceBusClient = new (builder.Configuration.GetSection("ServiceBus").Value);
var serviceBusSender = serviceBusClient.CreateSender(builder.Configuration.GetSection("ServiceBusTopicName").Value);

var cOpts = new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions()
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
        IgnoreNullValues = true
    }
};

var containers = new List<(string, string)>
{
    ("iot_demo_write", "data"),
    ("iot_demo_read", "data")
};

var cosmosClient = CosmosClient.CreateAndInitializeAsync(builder.Configuration.GetSection("CosmosDb").Value,
    containers, cOpts).Result;
var writeDataContainer = cosmosClient.GetContainer("iot_demo_write", "data");
var readDataContainer = cosmosClient.GetContainer("iot_demo_read", "data");

builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddKeyedSingleton(ContainerType.Write, writeDataContainer)
    .AddKeyedSingleton(ContainerType.Read, readDataContainer)
    .AddSingleton<IServiceHubContext>(signalrServiceHubContext)
    .AddSingleton(serviceBusSender);

builder.Build().Run();