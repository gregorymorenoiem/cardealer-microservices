// DisputeService - Program.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using MediatR;
using System.Text;
using DisputeService.Infrastructure.Persistence;
using DisputeService.Infrastructure.Persistence.Repositories;
using DisputeService.Domain.Interfaces;
using DisputeService.Application.Handlers;
using DisputeService.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<DisputeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<FileDisputeHandler>());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<FileDisputeValidator>();

// Repositories
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IDisputeEvidenceRepository, DisputeEvidenceRepository>();
builder.Services.AddScoped<IDisputeCommentRepository, DisputeCommentRepository>();
builder.Services.AddScoped<IDisputeTimelineRepository, DisputeTimelineRepository>();
builder.Services.AddScoped<IMediationSessionRepository, MediationSessionRepository>();
builder.Services.AddScoped<IDisputeParticipantRepository, DisputeParticipantRepository>();
builder.Services.AddScoped<IResolutionTemplateRepository, ResolutionTemplateRepository>();
builder.Services.AddScoped<IDisputeSlaConfigurationRepository, DisputeSlaConfigurationRepository>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultSecretKey12345678901234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DisputeService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DisputeService";

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DisputeService API",
        Version = "v1",
        Description = "Gestión de Disputas y Mediación - Ley 358-05 Pro-Consumidor"
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

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migrate in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DisputeDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
