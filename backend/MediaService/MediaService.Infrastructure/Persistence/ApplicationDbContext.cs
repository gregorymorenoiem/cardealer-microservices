using MediaService.Domain.Entities;
using MediaService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaService.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets para todas las entidades
        public DbSet<MediaAsset> MediaAssets { get; set; }
        public DbSet<MediaVariant> MediaVariants { get; set; }
        public DbSet<ImageMedia> ImageMedia { get; set; }
        public DbSet<VideoMedia> VideoMedia { get; set; }
        public DbSet<DocumentMedia> DocumentMedia { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones desde el assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configuraciones específicas
            modelBuilder.Entity<MediaAsset>(entity =>
            {
                entity.ToTable("media_assets");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ContentType);

                // Relación con variantes
                entity.HasMany(e => e.Variants)
                      .WithOne(v => v.MediaAsset)
                      .HasForeignKey(v => v.MediaAssetId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Discriminador para herencia TPH
                entity.HasDiscriminator<string>("MediaType")
                      .HasValue<ImageMedia>("Image")
                      .HasValue<VideoMedia>("Video")
                      .HasValue<DocumentMedia>("Document")
                      .HasValue<MediaAsset>("Other");
            });

            modelBuilder.Entity<MediaVariant>(entity =>
            {
                entity.ToTable("media_variants");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.MediaAssetId, e.Name }).IsUnique();
                entity.HasIndex(e => e.Width);
                entity.HasIndex(e => e.Height);
                entity.HasIndex(e => e.Quality);
            });

            modelBuilder.Entity<ImageMedia>(entity =>
            {
                entity.HasBaseType<MediaAsset>();
                entity.ToTable("image_media");
            });

            modelBuilder.Entity<VideoMedia>(entity =>
            {
                entity.HasBaseType<MediaAsset>();
                entity.ToTable("video_media");
            });

            modelBuilder.Entity<DocumentMedia>(entity =>
            {
                entity.HasBaseType<MediaAsset>();
                entity.ToTable("document_media");
            });
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Domain.Common.EntityBase && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Domain.Common.EntityBase)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    var createdAtProperty = entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProperty != null && createdAtProperty.CanWrite)
                    {
                        createdAtProperty.SetValue(entity, DateTime.UtcNow);
                    }
                }

                entity.MarkAsUpdated();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}