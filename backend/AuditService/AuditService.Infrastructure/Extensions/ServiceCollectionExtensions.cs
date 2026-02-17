using AuditService.Domain.Interfaces;
using AuditService.Domain.Interfaces.Repositories;
using AuditService.Infrastructure.Messaging;
using AuditService.Infrastructure.Persistence;
using AuditService.Infrastructure.Persistence.Repositories;
using AuditService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarDealer.Shared.Database;
using System.Reflection;


namespace AuditService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context - Intentar primero ConnectionStrings:DefaultConnection como fallback
        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(defaultConnectionString))
        {
            // Si existe ConnectionStrings:DefaultConnection, usarlo directamente
            services.AddDbContext<AuditDbContext>(options =>
                options.UseNpgsql(defaultConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30);
                }));
        }
        else
        {
            // Si no, usar multi-provider configuration
            services.AddDatabaseProvider<AuditDbContext>(configuration);
        }

        // Repositories
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // MediatR - Register handlers from Application assembly
        // Use a known type from the Application assembly to ensure it's loaded
        var applicationAssembly = typeof(AuditService.Application.Features.Audit.Commands.CreateAudit.CreateAuditCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // Background Services - RabbitMQ Event Consumer
        services.AddHostedService<RabbitMqEventConsumer>();

        return services;
    }
}