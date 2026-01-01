using BackupDRService.Core.BackgroundServices;
using BackupDRService.Core.Data;
using BackupDRService.Core.Interfaces;
using BackupDRService.Core.Models;
using BackupDRService.Core.Repositories;
using BackupDRService.Core.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "BackupDRService")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Backup & DR Service API",
        Version = "v1",
        Description = "Servicio de backup y disaster recovery automatizado para CarDealer Microservices"
    });
});

// Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BackupDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IBackupHistoryRepository, BackupHistoryRepository>();
builder.Services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
builder.Services.AddScoped<IRetentionPolicyRepository, RetentionPolicyRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register domain services
builder.Services.AddScoped<BackupHistoryService>();
builder.Services.AddScoped<SchedulerService>();
builder.Services.AddScoped<RetentionService>();
builder.Services.AddScoped<SchedulerMonitoringService>();

// Configure BackupOptions
builder.Services.Configure<BackupOptions>(
    builder.Configuration.GetSection(BackupOptions.SectionName));

// Register backup services
builder.Services.AddSingleton<IStorageProvider, LocalStorageProvider>();
builder.Services.AddSingleton<IDatabaseBackupProvider, PostgreSqlBackupProvider>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IRestoreService, RestoreService>();

// Register background services
builder.Services.AddHostedService<BackupSchedulerHostedService>();
builder.Services.AddHostedService<RetentionCleanupHostedService>();

// Add health checks
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres")
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
}
else
{
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backup & DR Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Log startup
Log.Information("BackupDRService starting on {Urls}", string.Join(", ", app.Urls));

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "BackupDRService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
