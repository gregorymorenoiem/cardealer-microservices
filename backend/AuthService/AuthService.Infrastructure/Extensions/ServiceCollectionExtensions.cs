using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.Interfaces.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.External;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Persistence.Repositories;
using AuthService.Infrastructure.Services.Identity;
using AuthService.Infrastructure.Services.Notification;
using AuthService.Infrastructure.Services.Security;
using AuthService.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthService.Infrastructure.Services.ExternalAuth;
using AuthService.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using AuthService.Application.Features.ExternalAuth.Commands.ExternalAuth;
using Polly;
using Polly.Extensions.Http;
using AuthService.Application.Common.Interfaces;
using AuthService.Infrastructure.Services;

namespace AuthService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Redis Configuration
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));
        }

        // Identity Configuration
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings from configuration
            var passwordPolicy = configuration.GetSection("Security:PasswordPolicy").Get<PasswordPolicySettings>();
            options.Password.RequiredLength = passwordPolicy?.RequiredLength ?? 8;
            options.Password.RequireDigit = passwordPolicy?.RequireDigit ?? true;
            options.Password.RequireLowercase = passwordPolicy?.RequireLowercase ?? true;
            options.Password.RequireUppercase = passwordPolicy?.RequireUppercase ?? true;
            options.Password.RequireNonAlphanumeric = passwordPolicy?.RequireNonAlphanumeric ?? false;

            // Lockout settings from configuration
            var lockoutPolicy = configuration.GetSection("Security:LockoutPolicy").Get<LockoutPolicySettings>();
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockoutPolicy?.DefaultLockoutMinutes ?? 30);
            options.Lockout.MaxFailedAccessAttempts = lockoutPolicy?.MaxFailedAccessAttempts ?? 5;
            options.Lockout.AllowedForNewUsers = lockoutPolicy?.Enabled ?? true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null)
            throw new InvalidOperationException("JWT settings are not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization Policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireVerifiedEmail", policy =>
                policy.RequireClaim("email_verified", "true"));
        });

        // Health Checks
        services.AddHealthChecks(configuration);

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();
        services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        services.AddScoped<IAuthNotificationService, AuthNotificationService>();
        services.AddScoped<ITokenService, TokenService>();

        // Request Context (for IP/UserAgent tracking)
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestContext, HttpRequestContext>();

        // Servicios 2FA
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IQRCodeService, QRCodeService>();

        // External Services
        services.AddHttpClient<NotificationServiceClient>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        services.Configure<NotificationServiceSettings>(configuration.GetSection("NotificationService"));

        // External Authentication Services
        services.AddHttpClient<ExternalTokenValidator>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        services.AddScoped<IExternalTokenValidator, ExternalTokenValidator>();
        services.AddScoped<IExternalAuthService, ExternalAuthService>();

        // Configuration
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));
        services.Configure<RateLimitSettings>(configuration.GetSection("Security:RateLimit"));

        // Caching
        var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>();
        if (cacheSettings?.EnableDistributedCache == true && !string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "AuthService_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ExternalAuthCommandHandler).Assembly));

        return services;
    }

    // NUEVO: Método para configurar Health Checks
    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks()
            .AddCheck<ApplicationHealthCheck>("application", HealthStatus.Unhealthy, new[] { "app", "live", "ready" });

        // Redis Health Check
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            healthChecksBuilder.AddCheck<RedisHealthCheck>("redis", HealthStatus.Unhealthy, new[] { "cache", "redis", "ready" });
        }

        // External Services Health Check
        var errorServiceUrl = configuration["ErrorService:BaseUrl"];
        var notificationServiceUrl = configuration["NotificationService:BaseUrl"];
        if (!string.IsNullOrEmpty(errorServiceUrl) || !string.IsNullOrEmpty(notificationServiceUrl))
        {
            services.AddHttpClient<ExternalServiceHealthCheck>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            healthChecksBuilder.AddCheck<ExternalServiceHealthCheck>("external-services", HealthStatus.Degraded, new[] { "external", "ready" });
        }

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
