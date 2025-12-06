using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using MarketingService.Domain.Interfaces;
using MarketingService.Infrastructure.Persistence;
using MarketingService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MarketingService API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<MarketingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Repositories
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
builder.Services.AddScoped<IAudienceRepository, AudienceRepository>();

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

// Module access verification - requires "marketing-automation" module
app.UseModuleAccess("marketing-automation");

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
