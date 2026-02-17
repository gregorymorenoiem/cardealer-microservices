using CarDealer.Shared.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using TaxComplianceService.Infrastructure.Persistence;
using TaxComplianceService.Infrastructure.Repositories;
using TaxComplianceService.Domain.Interfaces;
using TaxComplianceService.Application.Validators;
using TaxComplianceService.Application.Handlers;
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
        Title = "Tax Compliance Service API", 
        Version = "v1",
        Description = "API para cumplimiento tributario según Ley 11-92 Código Tributario de República Dominicana - DGII"
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
builder.Services.AddDbContext<TaxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => 

// SecurityValidation — ensures FluentValidation validators (NoSqlInjection, NoXss) run in MediatR pipeline
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(TaxComplianceService.Application.Behaviors.ValidationBehavior<,>));
    cfg.RegisterServicesFromAssembly(typeof(CreateTaxpayerHandler).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaxpayerValidator>();

// Repositories
builder.Services.AddScoped<ITaxpayerRepository, TaxpayerRepository>();
builder.Services.AddScoped<ITaxDeclarationRepository, TaxDeclarationRepository>();
builder.Services.AddScoped<ITaxPaymentRepository, TaxPaymentRepository>();
builder.Services.AddScoped<INcfSequenceRepository, NcfSequenceRepository>();
builder.Services.AddScoped<IReporte606Repository, Reporte606Repository>();
builder.Services.AddScoped<IReporte607Repository, Reporte607Repository>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key must be configured via environment/settings. Do NOT use hardcoded keys.");
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

builder.Services.AddAuthorization();

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddDbContextCheck<TaxDbContext>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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

var app = builder.Build();

// Configure the HTTP request pipeline
// OWASP Security Headers
app.UseApiSecurityHeaders(isProduction: !app.Environment.IsDevelopment());

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

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaxDbContext>();
    db.Database.Migrate();
}

app.Run();
