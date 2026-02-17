using FeatureToggleService.Application.Interfaces;
using FeatureToggleService.Domain.Interfaces;
using FeatureToggleService.Infrastructure.Data;
using FeatureToggleService.Infrastructure.Repositories;
using FeatureToggleService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureToggleService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<FeatureToggleDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Memory Cache
        services.AddMemoryCache();

        // Repositories
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<IFeatureFlagHistoryRepository, FeatureFlagHistoryRepository>();
        services.AddScoped<IABExperimentRepository, ABExperimentRepository>();
        services.AddScoped<IExperimentAssignmentRepository, ExperimentAssignmentRepository>();
        services.AddScoped<IExperimentMetricRepository, ExperimentMetricRepository>();

        // Services
        services.AddScoped<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
        services.AddScoped<IABTestingService, ABTestingService>();

        return services;
    }
}
