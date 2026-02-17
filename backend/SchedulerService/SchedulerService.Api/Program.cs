using CarDealer.Shared.Middleware;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using SchedulerService.Application.Handlers;
using SchedulerService.Infrastructure;
using SchedulerService.Infrastructure.Data;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add secret provider for Docker secrets
builder.Services.AddSecretProvider();

// Get connection string with secrets support
var connectionString = MicroserviceSecretsConfiguration.GetDatabaseConnectionString(
    builder.Configuration, "SchedulerService");

// Add services with JSON cycle handling
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SchedulerService API",
        Version = "v1",
        Description = "Distributed job scheduling service with Hangfire"
    });
});

// Add MediatR
builder.Services.AddMediatR(cfg =>

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(SchedulerService.Application.Behaviors.ValidationBehavior<,>));
    cfg.RegisterServicesFromAssembly(typeof(CreateJobCommandHandler).Assembly));

// Add Infrastructure layer (includes Hangfire, EF Core, repositories)
builder.Services.AddInfrastructure(builder.Configuration, connectionString);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "SchedulerService - Job Dashboard"
});

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Simple authorization filter for Hangfire Dashboard (allow all in development)
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // In production, implement proper authorization
        return true;
    }
}
