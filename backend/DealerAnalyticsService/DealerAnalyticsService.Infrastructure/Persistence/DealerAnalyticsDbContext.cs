using Microsoft.EntityFrameworkCore;
using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Infrastructure.Persistence.Configurations;

namespace DealerAnalyticsService.Infrastructure.Persistence;

public class DealerAnalyticsDbContext : DbContext
{
    public DealerAnalyticsDbContext(DbContextOptions<DealerAnalyticsDbContext> options) : base(options)
    {
    }
    
    public DbSet<DealerAnalytic> DealerAnalytics { get; set; }
    public DbSet<ConversionFunnel> ConversionFunnels { get; set; }
    public DbSet<MarketBenchmark> MarketBenchmarks { get; set; }
    public DbSet<DealerInsight> DealerInsights { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new DealerAnalyticConfiguration());
        modelBuilder.ApplyConfiguration(new ConversionFunnelConfiguration());
        modelBuilder.ApplyConfiguration(new MarketBenchmarkConfiguration());
        modelBuilder.ApplyConfiguration(new DealerInsightConfiguration());
    }
}
