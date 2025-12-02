using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Interfaces;
using SchedulerService.Infrastructure.Data;
using SchedulerService.Infrastructure.Jobs;
using SchedulerService.Infrastructure.Repositories;
using SchedulerService.Infrastructure.Services;

namespace SchedulerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<SchedulerDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => 
                c.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.ServerName = $"SchedulerService-{Environment.MachineName}";
        });

        // Repositories
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();

        // Services
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        // Jobs
        services.AddScoped<CleanupOldExecutionsJob>();
        services.AddScoped<DailyStatsReportJob>();
        services.AddScoped<HealthCheckJob>();

        return services;
    }
}
