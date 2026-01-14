using MediatR;
using FluentValidation;
using Serilog;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;
using AzulPaymentService.Application.Validators;
using AzulPaymentService.Domain.Interfaces;
using AzulPaymentService.Infrastructure.Persistence;
using AzulPaymentService.Infrastructure.Repositories;
using AzulPaymentService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ============= LOGGING =============
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/azul-payment-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ============= SERVICES =============
// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AzulDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelaySeconds: 5);
    }));

// Repositories
builder.Services.AddScoped<IAzulTransactionRepository, AzulTransactionRepository>();
builder.Services.AddScoped<IAzulSubscriptionRepository, AzulSubscriptionRepository>();

// Services
builder.Services.AddScoped<AzulWebhookValidationService>();
builder.Services.AddHttpClient<AzulHttpClient>();

// MediatR
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ChargeRequestValidator>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:8080", "https://api.okla.com.do")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AZUL Payment Service",
        Version = "v1",
        Description = "Servicio de pagos integrado con AZUL (Banco Popular RD)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "OKLA Team",
            Email = "dev@okla.com.do"
        }
    });

    // Agregar seguridad JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AzulDbContext>("Database");

// ============= BUILD =============
var app = builder.Build();

// ============= MIDDLEWARE =============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AZUL Payment Service V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowApiGateway");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ============= DATABASE MIGRATION =============
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AzulDbContext>();
        dbContext.Database.Migrate();
        Log.Information("Database migration completed");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error during database migration");
}

// ============= RUN =============
try
{
    Log.Information("Starting AzulPaymentService");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
