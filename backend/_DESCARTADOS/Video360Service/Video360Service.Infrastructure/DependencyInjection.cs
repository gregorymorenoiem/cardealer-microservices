using Amazon.S3;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Video360Service.Application.Features.Handlers;
using Video360Service.Application.Validators;
using Video360Service.Domain.Interfaces;
using Video360Service.Infrastructure.Persistence;
using Video360Service.Infrastructure.Persistence.Repositories;
using Video360Service.Infrastructure.Services;

namespace Video360Service.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVideo360Infrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration.GetConnectionString("Video360Db")
            ?? "Host=localhost;Database=video360db;Username=postgres;Password=postgres";

        services.AddDbContext<Video360DbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            }));

        // Repositories
        services.AddScoped<IVideo360JobRepository, Video360JobRepository>();
        services.AddScoped<IExtractedFrameRepository, ExtractedFrameRepository>();

        // Video360 Processor
        services.Configure<Video360ProcessorSettings>(
            configuration.GetSection(Video360ProcessorSettings.SectionName));
        
        services.AddHttpClient<IVideo360Processor, Video360Processor>(client =>
        {
            var settings = configuration.GetSection(Video360ProcessorSettings.SectionName)
                .Get<Video360ProcessorSettings>();
            
            if (!string.IsNullOrEmpty(settings?.PythonServiceUrl))
            {
                client.BaseAddress = new Uri(settings.PythonServiceUrl);
            }
            client.Timeout = TimeSpan.FromSeconds(settings?.TimeoutSeconds ?? 300);
        });

        // S3 Storage
        services.Configure<S3StorageSettings>(
            configuration.GetSection(S3StorageSettings.SectionName));
        
        var s3Settings = configuration.GetSection(S3StorageSettings.SectionName).Get<S3StorageSettings>();
        
        if (!string.IsNullOrEmpty(s3Settings?.ServiceUrl))
        {
            // MinIO o S3 compatible
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = s3Settings.ServiceUrl,
                    ForcePathStyle = true
                };
                return new AmazonS3Client(
                    s3Settings.AccessKey,
                    s3Settings.SecretKey,
                    config);
            });
        }
        else
        {
            // AWS S3
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
        }
        
        services.AddScoped<IStorageService, S3StorageService>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateVideo360JobHandler).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateVideo360JobRequestValidator>();

        return services;
    }
}
