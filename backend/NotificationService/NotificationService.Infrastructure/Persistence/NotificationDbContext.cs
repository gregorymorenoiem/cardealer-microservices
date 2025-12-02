using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

// Shim wrapper so tests referencing NotificationDbContext compile.
public class NotificationDbContext : ApplicationDbContext
{
    public NotificationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}
