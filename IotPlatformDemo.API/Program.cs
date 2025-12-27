using IotPlatformDemo.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Devices;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;

var apiTitle = "IoT Demo API";
const string apiVersion = "v1";

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var configuration = builder.Configuration;
var isDevelopment = env.IsDevelopment();

if (isDevelopment)
    apiTitle += " (Development)";

builder.Services.AddSingleton(RegistryManager.CreateFromConnectionString(configuration.GetValue<string>("iothub:connectionString")));

builder.Services.AddControllers();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddSwaggerGen(options =>
{
    var scopes = new Dictionary<string, string>
    {
        { configuration["SwaggerAuth:Scope"], "ReadWrite" },
        // add any custom scopes necessary, in place of or in addition to Default, e.g.
        // { $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user", "Access as User" },
        { "openid", "openid" },
    };
    var authFlow = new OpenApiOAuthFlow
    {
        AuthorizationUrl = new Uri($"https://svfriotdemo.ciamlogin.com/0484e3c3-e93f-4d52-bbb0-4af88d1c1b44/oauth2/v2.0/authorize"),
        TokenUrl = new Uri($"https://svfriotdemo.ciamlogin.com/0484e3c3-e93f-4d52-bbb0-4af88d1c1b44/oauth2/v2.0/token"),
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

// Configure the HTTP request pipeline.
// Serve the Swagger UI at the app root ("/") by setting RoutePrefix to empty.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiTitle} {apiVersion}");
    options.RoutePrefix = string.Empty; // serve UI at application root
    options.OAuthClientId(configuration["SwaggerAuth:ClientId"]);
    options.OAuthClientSecret(configuration["SwaggerAuth:ClientSecret"]);
    options.OAuthScopes(["openid", configuration["SwaggerAuth:Scope"]]);
    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
});

app.UseHttpsRedirection();

app.UseAuthorization();

//if (isDevelopment)
//    app.MapControllers().WithMetadata(new AllowAnonymousAttribute());
//else
    app.MapControllers();

app.Run();
