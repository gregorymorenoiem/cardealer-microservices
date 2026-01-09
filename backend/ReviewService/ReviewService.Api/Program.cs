using Microsoft.EntityFrameworkCore;
using ReviewService.Infrastructure.Persistence;
using ReviewService.Domain.Interfaces;
using ReviewService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext&lt;ReviewDbContext&gt;(options =&gt;
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b =&gt; b.MigrationsAssembly("ReviewService.Infrastructure"));
});

// Repositories
builder.Services.AddScoped&lt;IReviewRepository, ReviewRepository&gt;();
builder.Services.AddScoped&lt;IReviewSummaryRepository, ReviewSummaryRepository&gt;();
builder.Services.AddScoped&lt;IReviewResponseRepository, ReviewResponseRepository&gt;();

// MediatR
builder.Services.AddMediatR(cfg =&gt; {
    cfg.RegisterServicesFromAssembly(Assembly.Load("ReviewService.Application"));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("ReviewService.Application"));

// AutoMapper (si se usa)
// builder.Services.AddAutoMapper(Assembly.Load("ReviewService.Application"));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"];

if (!string.IsNullOrEmpty(secretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =&gt;
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =&gt;
{
    options.AddPolicy("AllowAll", policy =&gt;
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddEntityFrameworkCoreCheck&lt;ReviewDbContext&gt;();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =&gt;
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ReviewService API",
        Version = "v1",
        Description = "API para gestión de reviews de vendedores/dealers en OKLA",
        Contact = new OpenApiContact
        {
            Name = "OKLA Development Team",
            Email = "dev@okla.com.do"
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Bearer configuration
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
            Array.Empty&lt;string&gt;()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =&gt;
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReviewService API v1");
        c.RoutePrefix = string.Empty; // Swagger UI en la raíz
    });
}

// Middlewares
app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

// Root endpoint
app.MapGet("/", () =&gt; new
{
    service = "ReviewService",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow,
    endpoints = new[]
    {
        "GET /health - Health check",
        "GET /swagger - API documentation",
        "GET /api/reviews/seller/{sellerId} - Get seller reviews",
        "GET /api/reviews/seller/{sellerId}/summary - Get review statistics",
        "POST /api/reviews - Create review (auth required)",
        "PUT /api/reviews/{reviewId} - Update review (auth required)",
        "DELETE /api/reviews/{reviewId} - Delete review (auth required)"
    }
});

// Aplicar migraciones automáticamente en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService&lt;ReviewDbContext&gt;();
    
    try
    {
        await context.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error applying database migrations");
    }
}

Log.Information("ReviewService starting...");

app.Run();