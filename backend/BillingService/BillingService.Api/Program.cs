using Microsoft.EntityFrameworkCore;
using BillingService.Application.Services;
using BillingService.Domain.Interfaces;
using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Repositories;
using BillingService.Infrastructure.Services;
using BillingService.Infrastructure.External;
using BillingService.Infrastructure.Messaging;
using CarDealer.Shared.Secrets;
using CarDealer.Shared.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Secret Provider for Docker secrets
builder.Services.AddSecretProvider();

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BillingService API", Version = "v1" });
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure Stripe
builder.Services.Configure<StripeSettings>(
    builder.Configuration.GetSection("Stripe"));

// Add DbContext with secrets support
var connectionString = MicroserviceSecretsConfiguration.GetDatabaseConnectionString(builder.Configuration, "BillingService");
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Repositories
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IStripeCustomerRepository, StripeCustomerRepository>();
builder.Services.AddScoped<IEarlyBirdRepository, EarlyBirdRepository>();

// Add Stripe Services
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<BillingApplicationService>();

// Add RabbitMQ Event Publisher
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

// Add UserService Client for syncing subscriptions
var userServiceUrl = builder.Configuration["Services:UserService"] ?? "http://localhost:5020";
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.Migrate();
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
