using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdvertisingService.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AdvertisingDbContext>
{
    public AdvertisingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdvertisingDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=advertisingservice_db;Username=postgres;Password=postgres");

        return new AdvertisingDbContext(optionsBuilder.Options);
    }
}
