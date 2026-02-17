using CarDealer.Shared.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Vehicle360ProcessingService.Application.Features.Handlers;
using Vehicle360ProcessingService.Application.Validators;
using Vehicle360ProcessingService.Infrastructure;
using Vehicle360ProcessingService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Vehicle 360 Processing Service", 
        Version = "v1",
        Description = "Orquestador para procesamiento completo de vistas 360° de vehículos. " +
                      "Coordina la extracción de frames, remoción de fondos y almacenamiento final."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-256-bit-secret-key-for-jwt-auth-minimum-32-chars!";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "cardealer",
        ValidAudience = jwtSettings["Audience"] ?? "cardealer",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// MediatR
builder.Services.AddMediatR(cfg => 

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(Vehicle360ProcessingService.Application.Behaviors.ValidationBehavior<,>));
    cfg.RegisterServicesFromAssemblyContaining<StartVehicle360ProcessingHandler>());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<StartVehicle360ProcessingCommandValidator>();

// Infrastructure (DB, HTTP clients with Polly)
builder.Services.AddInfrastructure(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "database",
        tags: new[] { "db", "postgres" })
    .AddUrlGroup(
        new Uri($"{builder.Configuration["Services:MediaService:BaseUrl"]}/health"),
        name: "mediaservice",
        tags: new[] { "service", "media" })
    .AddUrlGroup(
        new Uri($"{builder.Configuration["Services:Video360Service:BaseUrl"]}/health"),
        name: "video360service",
        tags: new[] { "service", "video" })
    .AddUrlGroup(
        new Uri($"{builder.Configuration["Services:BackgroundRemovalService:BaseUrl"]}/health"),
        name: "backgroundremovalservice",
        tags: new[] { "service", "bgremoval" });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000", "http://localhost:5173" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapControllers();

// Apply migrations on startup (development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Vehicle360ProcessingDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
