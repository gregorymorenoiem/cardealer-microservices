using Microsoft.EntityFrameworkCore;

namespace MediaService.Infrastructure.Persistence;

// Shim wrapper so tests referencing MediaDbContext compile.
public class MediaDbContext : ApplicationDbContext
{
    public MediaDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
}
