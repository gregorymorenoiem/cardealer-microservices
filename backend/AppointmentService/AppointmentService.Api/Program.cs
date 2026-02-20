using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using AppointmentService.Domain.Interfaces;
using AppointmentService.Infrastructure.Persistence;
using AppointmentService.Infrastructure.Repositories;
using FluentValidation;
using CarDealer.Shared.Audit.Extensions;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AppointmentService API", Version = "v1" });
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add DbContext
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

// Add Health Checks
builder.Services.AddHealthChecks();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssembly(
    typeof(AppointmentService.Application.Validators.SecurityValidators).Assembly);

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService-Dev";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OKLA-Dev";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

// Module Access (for paid feature gating) - disabled in development
// builder.Services.AddModuleAccessServices(builder.Configuration);

// ============= AUDIT (→ AuditService via RabbitMQ) =============
builder.Services.AddAuditPublisher(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Audit middleware
app.UseAuditMiddleware();

app.UseAuthentication();
app.UseAuthorization();

// Module access verification - disabled in development
// app.UseModuleAccess("appointments");

app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("external")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Ensure database tables are created on startup (all environments)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Log.Information("AppointmentService database schema created/verified");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "AppointmentService DB init failed — continuing startup");
    }
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
