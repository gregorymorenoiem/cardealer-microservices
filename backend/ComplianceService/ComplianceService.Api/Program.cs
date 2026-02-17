using CarDealer.Shared.Middleware;
// ComplianceService - Program.cs
// Punto de entrada del servicio de cumplimiento regulatorio

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using ComplianceService.Application.Commands;
using ComplianceService.Application.Validators;
using ComplianceService.Domain.Interfaces;
using ComplianceService.Infrastructure.Persistence;
using ComplianceService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============ Logging Configuration ============
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ============ Database Configuration ============
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=postgres;Database=complianceservice;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(30);
    }));

// ============ Repository Registration ============
builder.Services.AddScoped<IRegulatoryFrameworkRepository, RegulatoryFrameworkRepository>();
builder.Services.AddScoped<IComplianceRequirementRepository, ComplianceRequirementRepository>();
builder.Services.AddScoped<IComplianceControlRepository, ComplianceControlRepository>();
builder.Services.AddScoped<IControlTestRepository, ControlTestRepository>();
builder.Services.AddScoped<IComplianceAssessmentRepository, ComplianceAssessmentRepository>();
builder.Services.AddScoped<IComplianceFindingRepository, ComplianceFindingRepository>();
builder.Services.AddScoped<IRemediationActionRepository, RemediationActionRepository>();
builder.Services.AddScoped<IRegulatoryReportRepository, RegulatoryReportRepository>();
builder.Services.AddScoped<IComplianceCalendarRepository, ComplianceCalendarRepository>();
builder.Services.AddScoped<IComplianceTrainingRepository, ComplianceTrainingRepository>();
builder.Services.AddScoped<ITrainingCompletionRepository, TrainingCompletionRepository>();
builder.Services.AddScoped<IComplianceMetricRepository, ComplianceMetricRepository>();

// ============ MediatR Configuration ============
builder.Services.AddMediatR(cfg => 

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ComplianceService.Application.Behaviors.ValidationBehavior<,>));
{
    cfg.RegisterServicesFromAssembly(typeof(CreateFrameworkCommand).Assembly);
});

// ============ FluentValidation Configuration ============
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFrameworkValidator>();

// ============ JWT Authentication ============
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "CarDealerSuperSecretKeyForJWTAuthentication2024!@#$%";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "CarDealer",
        ValidAudience = jwtSettings["Audience"] ?? "CarDealerAPI",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ComplianceOnly", policy => 
        policy.RequireRole("Admin", "Compliance"));
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("SystemAccess", policy => 
        policy.RequireRole("Admin", "Compliance", "System"));
});

// ============ CORS Configuration ============
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

// ============ Controllers & API Configuration ============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ============ Swagger Configuration ============
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ComplianceService API",
        Version = "v1",
        Description = @"
API de Cumplimiento Regulatorio para OKLA.
Gestiona marcos regulatorios, evaluaciones, hallazgos y reportes según:
- Ley 155-17: Prevención de Lavado de Activos (PLD/KYC/AML)
- Ley 172-13: Protección de Datos Personales (ARCO)
- Ley 126-02: Comercio Electrónico

Funcionalidades:
- Marcos regulatorios y requisitos
- Controles y evaluaciones de cumplimiento
- Hallazgos y acciones correctivas
- Reportes regulatorios (UAF, DGII, ProConsumidor, SIB)
- Calendario de obligaciones
- Capacitaciones y certificaciones
- Dashboard consolidado y métricas
",
        Contact = new OpenApiContact
        {
            Name = "OKLA Compliance Team",
            Email = "compliance@okla.com.do"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ingrese 'Bearer' [espacio] y luego su token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// ============ Health Checks ============
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", timeout: TimeSpan.FromSeconds(5))
    .AddDbContextCheck<ComplianceDbContext>("ef-core");

var app = builder.Build();

// ============ Middleware Pipeline ============

// Swagger (all environments for API documentation)

// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ComplianceService API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ============ Health Check Endpoints ============
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// ============ API Info Endpoint ============
app.MapGet("/", () => Results.Ok(new
{
    Service = "ComplianceService",
    Version = "1.0.0",
    Description = "Servicio de Cumplimiento Regulatorio - OKLA",
    Documentation = "/swagger",
    HealthCheck = "/health",
    Regulations = new[]
    {
        "Ley 155-17 (PLD/KYC/AML)",
        "Ley 172-13 (Protección de Datos/ARCO)",
        "Ley 126-02 (Comercio Electrónico)"
    },
    Endpoints = new
    {
        Frameworks = "/api/frameworks",
        Requirements = "/api/requirements",
        Controls = "/api/controls",
        Assessments = "/api/assessments",
        Findings = "/api/findings",
        Remediations = "/api/remediations",
        Reports = "/api/reports",
        Calendar = "/api/calendar",
        Training = "/api/training",
        Dashboard = "/api/dashboard"
    }
}));

app.MapControllers();

// ============ Database Migration ============
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Aplicando migraciones de base de datos...");
        db.Database.Migrate();
        logger.LogInformation("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "No se pudieron aplicar las migraciones. Intentando EnsureCreated...");
        try
        {
            db.Database.EnsureCreated();
            logger.LogInformation("Base de datos creada con EnsureCreated.");
        }
        catch (Exception ex2)
        {
            logger.LogError(ex2, "Error al crear la base de datos. El servicio continuará pero puede fallar operaciones de BD.");
        }
    }
}

app.Run();
