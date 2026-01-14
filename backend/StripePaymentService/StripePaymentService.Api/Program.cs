using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StripePaymentService.Application.Features.PaymentIntent.Commands;
using StripePaymentService.Application.Features.Subscription.Commands;
using StripePaymentService.Application.Validators;
using StripePaymentService.Domain.Interfaces;
using StripePaymentService.Infrastructure.Persistence;
using StripePaymentService.Infrastructure.Repositories;
using StripePaymentService.Infrastructure.Services;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/stripe-payment-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=StripePaymentService;User Id=postgres;Password=postgres";

builder.Services.AddDbContext<StripeDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
    })
);

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreatePaymentIntentCommand).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentIntentValidator>();

// Repositories
builder.Services.AddScoped<IStripePaymentIntentRepository, StripePaymentIntentRepository>();
builder.Services.AddScoped<IStripeCustomerRepository, StripeCustomerRepository>();
builder.Services.AddScoped<IStripeSubscriptionRepository, StripeSubscriptionRepository>();

// Services
var stripeApiKey = builder.Configuration["Stripe:ApiKey"] ?? throw new InvalidOperationException("Stripe:ApiKey not configured");
var stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"] ?? throw new InvalidOperationException("Stripe:WebhookSecret not configured");

builder.Services.AddHttpClient<StripeHttpClient>((provider, client) =>
{
    var logger = provider.GetRequiredService<ILogger<StripeHttpClient>>();
    return new StripeHttpClient(client, stripeApiKey, logger);
});

builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetRequiredService<ILogger<StripeWebhookValidationService>>();
    return new StripeWebhookValidationService(stripeWebhookSecret, logger);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stripe Payment Service API",
        Version = "v1",
        Description = "Payment processing service for Stripe integration"
    });

    // Security scheme for JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
    .AddDbContextCheck<StripeDbContext>();

var app = builder.Build();

// Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StripeDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
