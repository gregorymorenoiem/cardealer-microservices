using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchedulerService.Application.Interfaces;
using SchedulerService.Domain.Interfaces;
using SchedulerService.Infrastructure.Data;
using SchedulerService.Infrastructure.Executors;
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

        // Execution Engine
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JobExecutionEngine>>();
            var jobRepo = sp.GetRequiredService<IJobRepository>();
            var execRepo = sp.GetRequiredService<IJobExecutionRepository>();
            var executors = sp.GetServices<IJobExecutor>();
            var maxConcurrent = configuration.GetValue<int>("ExecutionEngine:MaxConcurrentJobs", 10);

            return new JobExecutionEngine(logger, jobRepo, execRepo, executors, maxConcurrent);
        });

        // Executors
        services.AddScoped<IJobExecutor, InternalJobExecutor>();
        services.AddScoped<IJobExecutor, HttpJobExecutor>();

        // HTTP Client for HttpJobExecutor
        services.AddHttpClient();

        // Jobs
        services.AddScoped<CleanupOldExecutionsJob>();
        services.AddScoped<DailyStatsReportJob>();
        services.AddScoped<HealthCheckJob>();

        return services;
    }
}
