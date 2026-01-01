using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using ContactService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for HealthChecker
builder.Services.AddHttpClient();

// Configure Consul Client
var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri(consulAddress);
}));

// Configure Service Discovery
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Service Discovery Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "ContactService" }));

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program class accessible for integration tests
public partial class Program { }
 