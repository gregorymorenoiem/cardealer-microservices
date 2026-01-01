using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using IntegrationService.Domain.Interfaces;
using IntegrationService.Infrastructure.Persistence;
using IntegrationService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "IntegrationService API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<IntegrationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Repositories
builder.Services.AddScoped<IIntegrationRepository, IntegrationRepository>();
builder.Services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
builder.Services.AddScoped<ISyncJobRepository, SyncJobRepository>();

// Health checks
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

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Module access verification - disabled in development
// app.UseModuleAccess("integrations");

app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not migrate database. This may be expected in development.");
    }
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
 