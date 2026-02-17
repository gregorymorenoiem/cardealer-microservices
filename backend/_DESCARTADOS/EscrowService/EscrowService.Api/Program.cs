// EscrowService - Program.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using MediatR;
using EscrowService.Infrastructure.Persistence;
using EscrowService.Infrastructure.Persistence.Repositories;
using EscrowService.Domain.Interfaces;
using EscrowService.Application.Validators;
using EscrowService.Application.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// DbContext
builder.Services.AddDbContext<EscrowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IEscrowAccountRepository, EscrowAccountRepository>();
builder.Services.AddScoped<IReleaseConditionRepository, ReleaseConditionRepository>();
builder.Services.AddScoped<IFundMovementRepository, FundMovementRepository>();
builder.Services.AddScoped<IEscrowDocumentRepository, EscrowDocumentRepository>();
builder.Services.AddScoped<IEscrowDisputeRepository, EscrowDisputeRepository>();
builder.Services.AddScoped<IEscrowAuditLogRepository, EscrowAuditLogRepository>();
builder.Services.AddScoped<IEscrowFeeConfigurationRepository, EscrowFeeConfigurationRepository>();

// MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateEscrowAccountHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetEscrowAccountByIdHandler).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateEscrowAccountValidator>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TempDevKey123456789012345678901234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EscrowService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EscrowService";

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

// Controllers
builder.Services.AddControllers();

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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OKLA EscrowService API",
        Version = "v1",
        Description = "Servicio de cuentas escrow y fondos en garantía para transacciones seguras"
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
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "postgresql");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EscrowService API v1");
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EscrowDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
