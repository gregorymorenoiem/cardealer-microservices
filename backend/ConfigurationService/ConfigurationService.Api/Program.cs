using ConfigurationService.Application.Interfaces;
using ConfigurationService.Infrastructure.Data;
using ConfigurationService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ConfigurationService.Application.Commands.CreateConfigurationCommand).Assembly);
});

// Application Services
var encryptionKey = builder.Configuration["Encryption:Key"] ?? "DefaultKey123!ChangeMe";
builder.Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
builder.Services.AddScoped<ConfigurationService.Application.Interfaces.IConfigurationManager, ConfigurationService.Infrastructure.Services.ConfigurationManager>();
builder.Services.AddScoped<ISecretManager, SecretManager>();
builder.Services.AddScoped<IFeatureFlagManager, FeatureFlagManager>();

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
