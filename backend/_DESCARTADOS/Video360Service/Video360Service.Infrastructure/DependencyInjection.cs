using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Video360Service.Application.Interfaces;
using Video360Service.Domain.Interfaces;
using Video360Service.Infrastructure.Configuration;
using Video360Service.Infrastructure.Persistence;
using Video360Service.Infrastructure.Persistence.Repositories;
using Video360Service.Infrastructure.Providers;
using Video360Service.Infrastructure.Services;

namespace Video360Service.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<SecretsSettings>(configuration.GetSection("Secrets"));
        services.Configure<FfmpegApiSettings>(configuration.GetSection("Providers:FfmpegApi"));
        services.Configure<ApyHubSettings>(configuration.GetSection("Providers:ApyHub"));
        services.Configure<CloudinarySettings>(configuration.GetSection("Providers:Cloudinary"));
        services.Configure<ImgixSettings>(configuration.GetSection("Providers:Imgix"));
        services.Configure<ShotstackSettings>(configuration.GetSection("Providers:Shotstack"));
        services.Configure<S3StorageSettings>(configuration.GetSection("Storage:S3"));
        
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<Video360DbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(Video360DbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(3);
            }));
        
        // Repositories
        services.AddScoped<IVideo360JobRepository, Video360JobRepository>();
        services.AddScoped<IProviderConfigurationRepository, ProviderConfigurationRepository>();
        services.AddScoped<IUsageRecordRepository, UsageRecordRepository>();
        
        // Providers (HttpClient configured)
        services.AddHttpClient<IVideo360Provider, FfmpegApiProvider>();
        services.AddHttpClient<IVideo360Provider, ApyHubProvider>();
        services.AddHttpClient<IVideo360Provider, CloudinaryProvider>();
        services.AddHttpClient<IVideo360Provider, ImgixProvider>();
        services.AddHttpClient<IVideo360Provider, ShotstackProvider>();
        
        // Register all providers explicitly for DI resolution
        services.AddScoped<FfmpegApiProvider>();
        services.AddScoped<ApyHubProvider>();
        services.AddScoped<CloudinaryProvider>();
        services.AddScoped<ImgixProvider>();
        services.AddScoped<ShotstackProvider>();
        
        // Provider collection for factory
        services.AddScoped<IEnumerable<IVideo360Provider>>(sp => new List<IVideo360Provider>
        {
            sp.GetRequiredService<FfmpegApiProvider>(),
            sp.GetRequiredService<ApyHubProvider>(),
            sp.GetRequiredService<CloudinaryProvider>(),
            sp.GetRequiredService<ImgixProvider>(),
            sp.GetRequiredService<ShotstackProvider>()
        });
        
        // Services
        services.AddScoped<IVideo360ProviderFactory, Video360ProviderFactory>();
        services.AddHttpClient<IVideoStorageService, VideoStorageService>();
        services.AddScoped<IVideo360Orchestrator, Video360Orchestrator>();
        
        return services;
    }
}
