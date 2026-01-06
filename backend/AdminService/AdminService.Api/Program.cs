using AdminService.Application.Interfaces;
using AdminService.Infrastructure.External;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using AdminService.Api.Middleware;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add secret provider for secure configuration
builder.Services.AddSecretProvider();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(AdminService.Application.UseCases.Vehicles.ApproveVehicle.ApproveVehicleCommand).Assembly));

// Service Discovery Configuration
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => config.Address = new Uri(consulAddress));
});

builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
builder.Services.AddHttpClient("HealthCheck");
builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

// Configure HttpClients for external services
builder.Services.AddHttpClient<IAuditServiceClient, AuditServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:AuditService"] ?? "https://localhost:7287";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:NotificationService"] ?? "https://localhost:45954";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IErrorServiceClient, ErrorServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:ErrorService"] ?? "https://localhost:45952";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

app.MapControllers();

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "AdminService" }));

app.Run();
