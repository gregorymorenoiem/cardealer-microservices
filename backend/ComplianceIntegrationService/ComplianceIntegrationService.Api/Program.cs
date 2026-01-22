// =====================================================
// C12: ComplianceIntegrationService - Program.cs
// Configuración de la aplicación ASP.NET Core
// =====================================================

using System.Text;
using ComplianceIntegrationService.Application.Features.Integrations.Commands;
using ComplianceIntegrationService.Application.Validators;
using ComplianceIntegrationService.Domain.Interfaces;
using ComplianceIntegrationService.Infrastructure.Persistence;
using ComplianceIntegrationService.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Configuración de Servicios
// =====================================================

// DbContext - PostgreSQL
builder.Services.AddDbContext<ComplianceIntegrationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)));

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateIntegrationConfigCommand).Assembly));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateIntegrationConfigValidator>();

// Repositorios
builder.Services.AddScoped<IIntegrationConfigRepository, IntegrationConfigRepository>();
builder.Services.AddScoped<IIntegrationCredentialRepository, IntegrationCredentialRepository>();
builder.Services.AddScoped<IDataTransmissionRepository, DataTransmissionRepository>();
builder.Services.AddScoped<IFieldMappingRepository, FieldMappingRepository>();
builder.Services.AddScoped<IIntegrationLogRepository, IntegrationLogRepository>();
builder.Services.AddScoped<IWebhookConfigRepository, WebhookConfigRepository>();
builder.Services.AddScoped<IIntegrationStatusHistoryRepository, IntegrationStatusHistoryRepository>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ComplianceIntegrationSuperSecretKey2024!@#$%^&*()";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ComplianceIntegrationService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ComplianceIntegrationService";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ComplianceAccess", policy => policy.RequireRole("Admin", "Compliance"));
    options.AddPolicy("SystemAccess", policy => policy.RequireRole("Admin", "System"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OKLA Compliance Integration Service API",
        Version = "v1",
        Description = "API para integración con entes reguladores de República Dominicana (DGII, UAF, ProConsumidor, etc.)",
        Contact = new OpenApiContact
        {
            Name = "OKLA Compliance Team",
            Email = "compliance@okla.com.do"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ComplianceIntegrationDbContext>("database");

// =====================================================
// Configuración del Pipeline HTTP
// =====================================================

var app = builder.Build();

// Desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance Integration API v1");
        options.RoutePrefix = "swagger";
    });
}

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoint
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Información de inicio
app.Logger.LogInformation("==============================================");
app.Logger.LogInformation("OKLA Compliance Integration Service");
app.Logger.LogInformation("Versión: 1.0.0");
app.Logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Puerto: {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "5032");
app.Logger.LogInformation("==============================================");
app.Logger.LogInformation("Integraciones soportadas:");
app.Logger.LogInformation("  - DGII (Impuestos Internos)");
app.Logger.LogInformation("  - UAF (Análisis Financiero)");
app.Logger.LogInformation("  - ProConsumidor");
app.Logger.LogInformation("  - SuperintendenciaBancos");
app.Logger.LogInformation("  - INDOTEL");
app.Logger.LogInformation("  - ProCompetencia");
app.Logger.LogInformation("  - OGTIC");
app.Logger.LogInformation("  - DGA (Aduanas)");
app.Logger.LogInformation("  - TSS (Seguridad Social)");
app.Logger.LogInformation("==============================================");

app.Run();
