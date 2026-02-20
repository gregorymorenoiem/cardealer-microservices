using CarDealer.Shared.Middleware;
using DataProtectionService.Application.Handlers;
using DataProtectionService.Application.Validators;
using DataProtectionService.Domain.Interfaces;
using DataProtectionService.Infrastructure.Persistence;
using DataProtectionService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DataProtection Service API",
        Version = "v1",
        Description = "API para gestión de protección de datos personales - Cumplimiento Ley 172-13 RD",
        Contact = new OpenApiContact
        {
            Name = "OKLA Team",
            Email = "legal@okla.com.do"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=dataprotectionservice;Username=postgres;Password=postgres";

builder.Services.AddDbContext<DataProtectionDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(30);
    }));

// MediatR Configuration
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateConsentCommandHandler).Assembly));

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(DataProtectionService.Application.Behaviors.ValidationBehavior<,>));

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateConsentCommandValidator>();

// Repository Registration
builder.Services.AddScoped<IUserConsentRepository, UserConsentRepository>();
builder.Services.AddScoped<IARCORequestRepository, ARCORequestRepository>();
builder.Services.AddScoped<IDataChangeLogRepository, DataChangeLogRepository>();
builder.Services.AddScoped<IPrivacyPolicyRepository, PrivacyPolicyRepository>();
builder.Services.AddScoped<IAnonymizationRecordRepository, AnonymizationRecordRepository>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(, policy =>
    {
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (isDev)
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(
                    "https://okla.com.do",
                    "https://www.okla.com.do",
                    "https://api.okla.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataProtectionDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DataProtection Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Database Migration on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataProtectionDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        // In development, create the database if it doesn't exist
        if (app.Environment.IsDevelopment())
        {
            db.Database.EnsureCreated();
            Console.WriteLine("Database created using EnsureCreated.");
        }
    }
}

Console.WriteLine("DataProtectionService started on port 8080");
Console.WriteLine("Ley 172-13 Compliance: ARCO Rights Management Active");

app.Run();
