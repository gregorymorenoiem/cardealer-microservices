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

// Module Access (for paid feature gating)
builder.Services.AddModuleAccessServices(builder.Configuration);

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

// Module access verification - requires "integrations" module
app.UseModuleAccess("integrations");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
