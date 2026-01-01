using CRMService.Domain.Interfaces;
using CRMService.Infrastructure.Persistence;
using CRMService.Infrastructure.Persistence.Repositories;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using CarDealer.Shared.Database;
using CarDealer.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add logging services
builder.Services.AddLogging();

// Add secret provider for Docker secrets
builder.Services.AddSecretProvider();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CRM Service API", Version = "v1" });
});

// Configure DbContext with multi-provider support
builder.Services.AddDatabaseProvider<CRMDbContext>(builder.Configuration);

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Register repositories
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IDealRepository, DealRepository>();
builder.Services.AddScoped<IPipelineRepository, PipelineRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();

// Configure MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CRMService.Application.DTOs.LeadDto).Assembly));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// Module Access (for paid feature gating) - disabled in development
// builder.Services.AddModuleAccessServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Module access verification - disabled in development
// app.UseModuleAccess("crm-advanced");

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup (optional, for development)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CRMDbContext>();
    try
    {
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "An error occurred while applying database migrations. The database may not exist yet.");
    }
}

Log.Information("CRM Service starting...");
app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
 