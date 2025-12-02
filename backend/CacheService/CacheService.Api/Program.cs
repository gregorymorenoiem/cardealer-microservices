using StackExchange.Redis;
using CacheService.Application.Interfaces;
using CacheService.Infrastructure;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using CacheService.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Cache Service API", Version = "v1" });
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CacheService.Application.Commands.SetCacheCommand).Assembly));

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

// Add Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});

// Add Application Services
builder.Services.AddSingleton<IStatisticsManager, RedisStatisticsManager>();
builder.Services.AddSingleton<ICacheManager, RedisCacheManager>();
builder.Services.AddSingleton<IDistributedLockManager, RedisLockManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();

// Service Discovery Auto-Registration
app.UseMiddleware<ServiceRegistrationMiddleware>();

// Add Health Check endpoint
app.MapGet("/health", async (IConnectionMultiplexer redis) =>
{
    try
    {
        var db = redis.GetDatabase();
        await db.PingAsync();
        return Results.Ok(new
        {
            status = "healthy",
            service = "CacheService",
            timestamp = DateTime.UtcNow,
            redis = "connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 503,
            title: "Service Unhealthy"
        );
    }
});

app.MapControllers();

app.Run();

// Make Program accessible for testing
public partial class Program { }
