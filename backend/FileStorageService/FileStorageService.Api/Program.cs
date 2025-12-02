using FileStorageService.Core.Interfaces;
using FileStorageService.Core.Models;
using FileStorageService.Core.Services;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Service", "FileStorageService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container

// Configuration
builder.Services.Configure<StorageProviderConfig>(
    builder.Configuration.GetSection("Storage"));

// Core services - register as singletons for in-memory state
builder.Services.AddSingleton<IStorageProvider, LocalStorageProvider>();
builder.Services.AddSingleton<IImageProcessingService, ImageProcessingService>();
builder.Services.AddSingleton<IMetadataExtractorService, MetadataExtractorService>();
builder.Services.AddSingleton<IVirusScanService, VirusScanService>();
builder.Services.AddSingleton<IPresignedUrlService, PresignedUrlService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageServiceImpl>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FileStorage Service API",
        Version = "v1",
        Description = "File storage service with virus scanning, metadata extraction, and image processing",
        Contact = new OpenApiContact
        {
            Name = "CarDealer Team",
            Email = "dev@cardealer.com"
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStorage Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthorization();

// Serve static files from uploads directory
var storageConfig = builder.Configuration.GetSection("Storage").Get<StorageProviderConfig>();
var uploadsPath = Path.GetFullPath(storageConfig?.BasePath ?? "./uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/files"
});

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("FileStorage Service starting on {Environment}", app.Environment.EnvironmentName);

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
