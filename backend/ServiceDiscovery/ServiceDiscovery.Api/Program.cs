using CarDealer.Shared.Middleware;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;

namespace ServiceDiscovery.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure Consul
        var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
        builder.Services.AddSingleton<IConsulClient>(provider =>
            new ConsulClient(config => config.Address = new Uri(consulAddress)));

        // Register application services
        builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
        builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();
        builder.Services.AddScoped<IHealthChecker, HttpHealthChecker>();

        // Add HttpClient for health checks
        builder.Services.AddHttpClient("HealthCheck", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Add MediatR
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.Handlers.RegisterServiceHandler).Assembly));

        // SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
        builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ServiceDiscovery.Application.Behaviors.ValidationBehavior<,>));

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDev)
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://okla.com.do",
                    "https://www.okla.com.do",
                    "https://api.okla.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        // OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseAuthorization();
        app.MapControllers();
        
        // Add health check endpoint
        app.MapGet("/health", () => Results.Ok(new 
        { 
            status = "healthy", 
            service = "ServiceDiscovery",
            timestamp = DateTime.UtcNow 
        }));

        app.Run();
    }
}

// Make Program accessible for testing
public partial class Program { }
