using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using KYCService.Infrastructure.Persistence;
using KYCService.Infrastructure.Repositories;
using KYCService.Infrastructure;
using KYCService.Domain.Interfaces;
using KYCService.Application.Validators;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "KYC Service API", 
        Version = "v1",
        Description = "API para gestión de KYC según Ley 155-17 de Prevención de Lavado de Activos"
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

// Database
builder.Services.AddDbContext<KYCDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(KYCService.Application.Handlers.CreateKYCProfileHandler).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateKYCProfileValidator>();

// Repositories
builder.Services.AddScoped<IKYCProfileRepository, KYCProfileRepository>();
builder.Services.AddScoped<IKYCDocumentRepository, KYCDocumentRepository>();
builder.Services.AddScoped<IKYCVerificationRepository, KYCVerificationRepository>();
builder.Services.AddScoped<IKYCRiskAssessmentRepository, KYCRiskAssessmentRepository>();
builder.Services.AddScoped<ISuspiciousTransactionReportRepository, SuspiciousTransactionReportRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();

// External Services (JCE, OCR, Face Comparison)
if (builder.Environment.IsDevelopment())
{
    // En desarrollo, usar simulación
    builder.Services.AddKYCInfrastructureDevelopment(builder.Configuration);
}
else
{
    // En producción, usar servicios reales
    builder.Services.AddKYCInfrastructureProduction(builder.Configuration);
}

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-development-key-change-in-production-min-32-chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CarDealer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CarDealer";

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

// Authorization policies based on account_type claim
// AccountType enum: Guest=0, Individual=1, Dealer=2, DealerEmployee=3, Admin=4, PlatformEmployee=5
builder.Services.AddAuthorization(options =>
{
    // Admin policy: account_type = 4 (Admin)
    options.AddPolicy("Admin", policy => 
        policy.RequireClaim("account_type", "4"));
    
    // Compliance policy: account_type = 4 (Admin) or 5 (PlatformEmployee)
    options.AddPolicy("Compliance", policy => 
        policy.RequireClaim("account_type", "4", "5"));
    
    // AdminOrCompliance: Either Admin or Compliance (used in controllers)
    options.AddPolicy("AdminOrCompliance", policy => 
        policy.RequireClaim("account_type", "4", "5"));
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddDbContextCheck<KYCDbContext>();

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<KYCDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
