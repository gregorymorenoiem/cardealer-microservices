// ContractService - Program.cs Entry Point

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using MediatR;
using ContractService.Infrastructure.Persistence;
using ContractService.Infrastructure.Repositories;
using ContractService.Domain.Interfaces;
using ContractService.Application.Handlers;
using ContractService.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OKLA ContractService API",
        Version = "v1",
        Description = "Servicio de Contratos Electrónicos - Ley 126-02 de Comercio Electrónico de República Dominicana. Gestión de contratos, firmas digitales y certificación.",
        Contact = new OpenApiContact
        {
            Name = "OKLA Compliance Team",
            Email = "compliance@okla.com.do"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
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

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=contractservice;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ContractDbContext>(options =>
    options.UseNpgsql(connectionString));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateContractHandler).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateContractValidator>();

// Repositories
builder.Services.AddScoped<IContractTemplateRepository, ContractTemplateRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IContractPartyRepository, ContractPartyRepository>();
builder.Services.AddScoped<IContractSignatureRepository, ContractSignatureRepository>();
builder.Services.AddScoped<IContractClauseRepository, ContractClauseRepository>();
builder.Services.AddScoped<IContractVersionRepository, ContractVersionRepository>();
builder.Services.AddScoped<IContractDocumentRepository, ContractDocumentRepository>();
builder.Services.AddScoped<IContractAuditLogRepository, ContractAuditLogRepository>();
builder.Services.AddScoped<ICertificationAuthorityRepository, CertificationAuthorityRepository>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "okla-super-secret-key-for-jwt-token-validation-2024";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OKLA";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "OKLA";

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
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "sql", "postgresql" });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OKLA ContractService API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Database migration
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ContractDbContext>();
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrated successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database migration failed, will try EnsureCreated");
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ContractDbContext>();
            db.Database.EnsureCreated();
            app.Logger.LogInformation("Database created successfully");
        }
        catch (Exception ex2)
        {
            app.Logger.LogError(ex2, "Database creation failed");
        }
    }
}

app.Logger.LogInformation("ContractService started - Ley 126-02 Compliance");
app.Run();
