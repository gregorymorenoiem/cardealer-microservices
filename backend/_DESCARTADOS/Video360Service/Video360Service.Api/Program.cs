using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Video360Service.Infrastructure;
using Video360Service.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Video360 Service API",
        Version = "v1",
        Description = "API para procesar videos 360 de vehículos y extraer imágenes para viewer interactivo",
        Contact = new OpenApiContact
        {
            Name = "OKLA Team",
            Email = "dev@okla.com.do"
        }
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

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-here-min-32-chars!!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "okla",
            ValidAudience = jwtSettings["Audience"] ?? "okla-api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "http://localhost:5173", "https://okla.com.do" };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Infrastructure services (DB, Repositories, S3, Processor)
builder.Services.AddVideo360Infrastructure(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Video360DbContext>("database")
    .AddCheck("python-worker", () =>
    {
        // TODO: Check Python worker health
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Video360 Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup (in development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Video360DbContext>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not apply migrations. Database may not be available.");
    }
}

app.Logger.LogInformation("Video360Service started on port {Port}", 
    builder.Configuration["ASPNETCORE_URLS"] ?? "8080");

app.Run();
