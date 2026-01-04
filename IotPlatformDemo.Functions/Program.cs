using Azure.Core.Serialization;
using Azure.Messaging.ServiceBus;
using IotPlatformDemo.Application.Notifications;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
        option.ConnectionString = builder.Configuration.GetSection("ConnectionStrings").GetSection("SignalR").Value;
    })
    .BuildServiceManager();

var signalrServiceHubContext = await signalrServiceManager.CreateHubContextAsync(nameof(ClientNotificationHub), CancellationToken.None);

ServiceBusClient serviceBusClient = new (builder.Configuration.GetSection("ConnectionStrings")["ServiceBus"]);
var serviceBusSender = serviceBusClient.CreateSender(builder.Configuration.GetSection("ServiceBusTopicName").Value);

builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton<IServiceHubContext>(signalrServiceHubContext)
    .AddSingleton(serviceBusSender);

builder.Build().Run();