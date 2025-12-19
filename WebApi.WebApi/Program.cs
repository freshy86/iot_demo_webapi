const string apiTitle = "IoT Fullstack Demo WebApi";
const string apiVersion = "v1";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
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
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
