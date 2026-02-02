using IotPlatformDemo.Application.EventStore;
using IotPlatformDemo.Application.Notifications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Devices;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;

var apiTitle = "IoT Demo API";
const string apiVersion = "v1";

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var configuration = builder.Configuration;
var isDevelopment = env.IsDevelopment();

var cOpts = new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions()
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
        IgnoreNullValues = true
    }
};

const string dataContainerName = "data";
const string eventsContainerName = "events";

var containers = new List<(string, string)>
{
    (configuration.GetValue<string>("CosmosDb:DbWrite")!, dataContainerName),
    (configuration.GetValue<string>("CosmosDb:DbWrite")!, eventsContainerName),
    (configuration.GetValue<string>("CosmosDb:DbRead")!, dataContainerName)
};

var cosmosClient = CosmosClient.CreateAndInitializeAsync(configuration.GetValue<string>("CosmosDb:ConnectionString"),
    containers, cOpts).Result;
var writeEventsContainer= cosmosClient.GetContainer(configuration.GetValue<string>("CosmosDb:DbWrite"),
    eventsContainerName);
var readDataContainer= cosmosClient.GetContainer(configuration.GetValue<string>("CosmosDb:DbRead"),
    dataContainerName);

builder.Services.AddSingleton(RegistryManager.CreateFromConnectionString(configuration.GetValue<string>("IotHub:ConnectionString")))
    .AddSingleton<IEventStore>(new CosmosDbEventStore(writeEventsContainer!))
    .AddSingleton(readDataContainer)
    .AddHttpContextAccessor()
    .AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration);
builder.Services.AddSignalR().AddAzureSignalR(configuration.GetValue<string>("SignalR:ConnectionString"));

const string allowSpecificOriginsDevelopment = "allowSpecificOriginsDevelopment";
if (isDevelopment)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: allowSpecificOriginsDevelopment,
            policy  =>
            {
                policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
    });
}

var baseUrl = $"{configuration["AzureAd:Instance"]}/{configuration["AzureAd:TenantId"]}";

builder.Services.AddSwaggerGen(options =>
{
    var scopes = new Dictionary<string, string>
    {
        { configuration["SwaggerAuth:Scope"] ?? "ReadWrite", "ReadWrite" },
        { "openid", "openid" },
        { "profile", "profile" }
    };
    var authFlow = new OpenApiOAuthFlow
    {
        AuthorizationUrl = new Uri($"{baseUrl}/oauth2/v2.0/authorize"),
        TokenUrl = new Uri($"{baseUrl}/oauth2/v2.0/token"),
        Scopes = scopes,
    };
    var openApiSecurityScheme = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            //AuthorizationCode = authFlow,
            Implicit = authFlow,
        },
        In = ParameterLocation.Header,
        Scheme = "oauth2",
        Name = "Authorization",
        Description = "Use OAuth2 [Auth Code] authorization",
    };

    options.AddSecurityDefinition("bearer", openApiSecurityScheme);
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });

    options.SwaggerDoc(apiVersion, new ()
    {
        Title = apiTitle,
        Version = apiVersion,
        Description = ""
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiTitle} {apiVersion}");
    options.RoutePrefix = string.Empty; // serve UI at application root
    options.OAuthClientId(configuration["SwaggerAuth:ClientId"]);
    options.OAuthClientSecret(configuration["SwaggerAuth:ClientSecret"]);
    options.OAuthScopes(["openid", "profile", configuration["SwaggerAuth:Scope"]]);
    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    options.EnablePersistAuthorization();
});

app.UseDefaultFiles();
app.UseRouting();
if (isDevelopment)
{
    app.UseCors(allowSpecificOriginsDevelopment);
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ClientNotificationHub>("/notifications");
app.Run();