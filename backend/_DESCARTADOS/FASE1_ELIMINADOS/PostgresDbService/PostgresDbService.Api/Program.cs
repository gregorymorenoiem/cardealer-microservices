using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PostgresDbService.Infrastructure.Persistence;
using PostgresDbService.Domain.Interfaces;
using PostgresDbService.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext&lt;CentralizedDbContext&gt;(options =&gt;
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Repository registration
builder.Services.AddScoped&lt;IGenericRepository, GenericRepository&gt;();
builder.Services.AddScoped&lt;IUserRepository, UserRepository&gt;();
builder.Services.AddScoped&lt;IVehicleRepository, VehicleRepository&gt;();
builder.Services.AddScoped&lt;IContactRepository, ContactRepository&gt;();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =&gt;
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey not found"))),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck&lt;CentralizedDbContext&gt;();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =&gt;
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PostgresDbService API", 
        Version = "v1",
        Description = "Centralized database service for all CarDealer microservices",
        Contact = new OpenApiContact
        {
            Name = "OKLA Development Team",
            Email = "dev@okla.com.do"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

// CORS
builder.Services.AddCors(options =&gt;
{
    options.AddDefaultPolicy(policy =&gt;
    {
        policy.WithOrigins(
                "https://okla.com.do",
                "https://www.okla.com.do",
                "http://localhost:5173",
                "http://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Logging
builder.Services.AddLogging(logging =&gt;
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =&gt;
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PostgresDbService API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "PostgresDbService API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Database migration on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService&lt;CentralizedDbContext&gt;();
    
    try
    {
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error occurred during database migration");
        throw;
    }
}

app.Logger.LogInformation("PostgresDbService started successfully");

app.Run();