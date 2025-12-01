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
        // Database Context (multi-provider configuration)
        services.AddDatabaseProvider<AuditDbContext>(configuration);

        // Repositories
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // MediatR - Register handlers from Application assembly
        var applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "AuditService.Application");

        if (applicationAssembly != null)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        }

        // Background Services - RabbitMQ Event Consumer
        services.AddHostedService<RabbitMqEventConsumer>();

        return services;
    }
}