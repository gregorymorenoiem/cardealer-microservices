using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReviewService.Domain.Interfaces;
using ReviewService.Domain.Services;
using ReviewService.Infrastructure.Persistence;
using ReviewService.Infrastructure.Persistence.Repositories;

namespace ReviewService.Infrastructure;

/// <summary>
/// Configuración de DI para ReviewService.Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ReviewDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ReviewDbContext).Assembly.FullName)));

        // Sprint 14 - Repositories básicos
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewResponseRepository, ReviewResponseRepository>();
        services.AddScoped<IReviewSummaryRepository, ReviewSummaryRepository>();

        // Sprint 15 - Nuevos repositories
        services.AddScoped<IReviewHelpfulVoteRepository, ReviewHelpfulVoteRepository>();
        services.AddScoped<ISellerBadgeRepository, SellerBadgeRepository>();
        services.AddScoped<IReviewRequestRepository, ReviewRequestRepository>();
        services.AddScoped<IFraudDetectionLogRepository, FraudDetectionLogRepository>();

        // Sprint 15 - Domain Services
        services.AddScoped<IBadgeCalculationService, BadgeCalculationService>();

        return services;
    }
}