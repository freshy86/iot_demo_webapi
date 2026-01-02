using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<WorkerOptions>(workerOptions =>
{
    var settings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();
    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(settings);
});

var serviceManager = new ServiceManagerBuilder()
    .WithOptions(option =>
    {
        option.ConnectionString = builder.Configuration.GetSection("ConnectionStrings").GetSection("SignalR").Value;
    })
    .BuildServiceManager();

var serviceHubContext = await serviceManager.CreateHubContextAsync("ChatSampleHub", CancellationToken.None);

builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddSingleton<IServiceHubContext>(serviceHubContext);

builder.Build().Run();