using AuditService.Infrastructure.Extensions;
using AuditService.Infrastructure.Persistence;
using AuditService.Shared;
using AuditService.Shared.Settings;
using Microsoft.EntityFrameworkCore;
using Serilog;
// REMOVER esta línea problemática
// using AspNetCore.HealthChecks.UI.Client; 
// EN SU LUGAR usar:
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client; // Este es el namespace correcto

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Audit Service API",
        Version = "v1",
        Description = "Microservice for handling audit logs and security events",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Audit Service Team",
            Email = "audit-service@cargurus.com"
        }
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "https://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Infrastructure services (Database, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuditDbContext>(
        name: "database",
        tags: new[] { "ready", "liveness" });

// Add Health Checks UI
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetHeaderText("Audit Service - Health Status");
    setup.AddHealthCheckEndpoint("Audit Service", "/health");
    setup.SetEvaluationTimeInSeconds(60);
    setup.SetApiMaxActiveRequests(3);
    setup.MaximumHistoryEntriesPerEndpoint(50);
})
.AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit Service API v1");
        c.RoutePrefix = "swagger";
    });

    // Apply migrations in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();

    // Apply migrations in production if enabled
    var databaseSettings = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseSettings>>().Value;
    if (databaseSettings.AutoMigrate)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

app.UseCors("CorsPolicy");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    Predicate = check => check.Tags.Contains("liveness")
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui";
    // setup.AddCustomStylesheet("healthchecks-ui.css"); // Comentar si no existe el archivo
});

// Global exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An unhandled exception occurred");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An internal server error occurred",
            requestId = context.TraceIdentifier
        });
    }
});

Log.Information("Audit Service starting up...");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();