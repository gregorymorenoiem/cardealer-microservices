using MediaService.Domain.Interfaces.Repositories;
using MediaService.Domain.Interfaces.Services;
using MediaService.Infrastructure.HealthChecks;
using MediaService.Infrastructure.Middleware;
using MediaService.Infrastructure.Persistence;
using MediaService.Infrastructure.Repositories;
using MediaService.Infrastructure.Services.Processing;
using MediaService.Infrastructure.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MediaService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=MediaServiceDb;Trusted_Connection=true;MultipleActiveResultSets=true";
            }
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Repositories
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IMediaVariantRepository, MediaVariantRepository>();

        // Storage Services
        services.Configure<LocalStorageOptions>(configuration.GetSection("Storage:Local"));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection("Storage:Azure"));
        services.Configure<S3StorageOptions>(configuration.GetSection("Storage:S3"));

        // Storage Services - Registro dinámico
        var storageProvider = configuration["Storage:Provider"]?.ToLowerInvariant() ?? "local";
        switch (storageProvider)
        {
            case "azure":
                services.AddScoped<IMediaStorageService, AzureBlobStorageService>();
                break;
            case "s3":
            case "aws":
                services.AddScoped<IMediaStorageService, S3StorageService>();
                break;
            case "local":
            default:
                services.AddScoped<IMediaStorageService>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<LocalStorageService>>();
                    var basePath = configuration["Storage:Local:BasePath"] ?? "wwwroot/uploads";
                    var baseUrl = configuration["Storage:Local:BaseUrl"] ?? "https://localhost:7000/uploads";

                    return new LocalStorageService(basePath, baseUrl, logger);
                });
                break;
        }

        // Processing Services
        services.AddScoped<IImageProcessor, ImageSharpProcessor>();
        services.AddScoped<IVideoProcessor, FfmpegVideoProcessor>();

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" })
            .AddCheck<StorageHealthCheck>(
                "storage",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready" })
            .AddDbContextCheck<ApplicationDbContext>(
                "db-context",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" });

        // Middleware
        services.AddTransient<ErrorHandlingMiddleware>();

        // Servicios de infraestructura adicionales
        services.AddSingleton<IFileValidator, FileValidator>();
        services.AddScoped<IStorageHealthChecker, StorageHealthChecker>();

        return services;
    }

    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: connectionString,
                    healthQuery: "SELECT 1;",
                    name: "sqlserver",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "sql" });
        }

        return services;
    }
}

// Interfaces y clases adicionales necesarias
public interface IFileValidator
{
    bool ValidateFile(Stream fileStream, string contentType, long maxFileSize);
    bool IsImage(string contentType);
    bool IsVideo(string contentType);
    bool IsDocument(string contentType);
}

public class FileValidator : IFileValidator
{
    private readonly HashSet<string> _imageTypes = new()
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/svg+xml"
    };

    private readonly HashSet<string> _videoTypes = new()
    {
        "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm"
    };

    private readonly HashSet<string> _documentTypes = new()
    {
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    public bool ValidateFile(Stream fileStream, string contentType, long maxFileSize)
    {
        if (fileStream.Length > maxFileSize)
            return false;

        if (!IsImage(contentType) && !IsVideo(contentType) && !IsDocument(contentType))
            return false;

        return true;
    }

    public bool IsImage(string contentType) => _imageTypes.Contains(contentType?.ToLowerInvariant() ?? "");
    public bool IsVideo(string contentType) => _videoTypes.Contains(contentType?.ToLowerInvariant() ?? "");
    public bool IsDocument(string contentType) => _documentTypes.Contains(contentType?.ToLowerInvariant() ?? "");
}

public interface IStorageHealthChecker
{
    Task<bool> CheckStorageHealthAsync();
}

public class StorageHealthChecker : IStorageHealthChecker
{
    private readonly IMediaStorageService _storageService;
    private readonly ILogger<StorageHealthChecker> _logger;

    public StorageHealthChecker(IMediaStorageService storageService, ILogger<StorageHealthChecker> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<bool> CheckStorageHealthAsync()
    {
        try
        {
            return await _storageService.IsHealthyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking storage health");
            return false;
        }
    }
}

// Health Check para S3 (implementación básica)
public class S3HealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("S3 storage is accessible"));
    }
}

// Clases de opciones
public class LocalStorageOptions
{
    public string BasePath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "https://localhost:7000/uploads";
}

public class AzureBlobStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "media";
    public string CdnBaseUrl { get; set; } = string.Empty;
    public long MaxUploadSizeBytes { get; set; } = 104857600;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public int SasTokenExpirationMinutes { get; set; } = 60;
}

public class S3StorageOptions
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = "media-service";
    public string CdnBaseUrl { get; set; } = string.Empty;
    public long MaxUploadSizeBytes { get; set; } = 104857600;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public int PreSignedUrlExpirationMinutes { get; set; } = 60;
}