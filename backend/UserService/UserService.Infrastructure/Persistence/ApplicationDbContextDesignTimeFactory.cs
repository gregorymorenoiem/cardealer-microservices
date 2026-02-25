using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace UserService.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory for creating ApplicationDbContext instances.
    /// Used by EF Core tools (migrations, scaffolding, etc.).
    /// </summary>
    public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try to get connection string from environment or use development default
            var connectionString = Environment.GetEnvironmentVariable("EF_CONNECTION_STRING")
                ?? "Host=localhost;Database=userservice;Username=postgres;Password=devpassword;Port=5432";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
