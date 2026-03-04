using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// In-memory banner repository.
/// Data is pre-seeded with demo banners so /vehiculos has content out of the box.
/// TODO: Migrate to EF Core when ApplicationDbContext is wired to PostgreSQL.
/// </summary>
public class InMemoryBannerRepository : IBannerRepository
{
    private readonly ILogger<InMemoryBannerRepository> _logger;

    // Static so all requests in the same pod share data
    private static readonly ConcurrentDictionary<string, BannerEntity> s_banners;

    static InMemoryBannerRepository()
    {
        s_banners = new ConcurrentDictionary<string, BannerEntity>(StringComparer.Ordinal);
        SeedDemoBanners();
    }

    public InMemoryBannerRepository(ILogger<InMemoryBannerRepository> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<BannerEntity>> GetAllAsync()
    {
        var result = s_banners.Values
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.CreatedAt)
            .AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<IEnumerable<BannerEntity>> GetByPlacementAsync(string placement, bool activeOnly = true)
    {
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var result = s_banners.Values
            .Where(b => b.Placement == placement)
            .Where(b => !activeOnly || b.Status == "active")
            .Where(b => !activeOnly || (string.Compare(b.StartDate, now) <= 0 && string.Compare(b.EndDate, now) >= 0))
            .OrderBy(b => b.DisplayOrder)
            .AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<BannerEntity?> GetByIdAsync(string id)
    {
        s_banners.TryGetValue(id, out var banner);
        return Task.FromResult(banner);
    }

    public Task<BannerEntity> CreateAsync(BannerEntity banner)
    {
        s_banners[banner.Id] = banner;
        _logger.LogInformation("Banner created: {BannerId} — {Title}", banner.Id, banner.Title);
        return Task.FromResult(banner);
    }

    public Task<BannerEntity?> UpdateAsync(string id, BannerEntity banner)
    {
        if (!s_banners.ContainsKey(id))
            return Task.FromResult<BannerEntity?>(null);

        s_banners[id] = banner;
        _logger.LogInformation("Banner updated: {BannerId}", id);
        return Task.FromResult<BannerEntity?>(banner);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = s_banners.TryRemove(id, out _);
        if (removed) _logger.LogInformation("Banner deleted: {BannerId}", id);
        return Task.FromResult(removed);
    }

    public Task RecordViewAsync(string id)
    {
        if (s_banners.TryGetValue(id, out var banner))
            banner.RecordView();
        return Task.CompletedTask;
    }

    public Task RecordClickAsync(string id)
    {
        if (s_banners.TryGetValue(id, out var banner))
            banner.RecordClick();
        return Task.CompletedTask;
    }

    // ────────────────────────────────────────────────────────────
    // Demo seed — provides out-of-the-box banners on /vehiculos
    // ────────────────────────────────────────────────────────────
    private static void SeedDemoBanners()
    {
        var today = DateTime.UtcNow;
        var demos = new[]
        {
            BannerEntity.Create(
                title: "Financiamiento para tu vehículo",
                image: "/images/banners/financing.jpg",
                link: "/financiamiento",
                placement: "search_leaderboard",
                status: "active",
                startDate: today.AddDays(-30).ToString("yyyy-MM-dd"),
                endDate: today.AddDays(60).ToString("yyyy-MM-dd"),
                subtitle: "Obtén tu préstamo con la tasa más baja del mercado dominicano",
                ctaText: "Solicitar ahora",
                displayOrder: 1),

            BannerEntity.Create(
                title: "Seguros de auto desde RD$2,500/mes",
                image: "/images/banners/insurance.jpg",
                link: "/seguros",
                placement: "search_leaderboard",
                status: "active",
                startDate: today.AddDays(-15).ToString("yyyy-MM-dd"),
                endDate: today.AddDays(45).ToString("yyyy-MM-dd"),
                subtitle: "Protege tu vehículo con la cobertura completa",
                ctaText: "Ver planes",
                displayOrder: 2),

            BannerEntity.Create(
                title: "¿Quieres publicar aquí?",
                image: "",
                link: "/publicidad",
                placement: "search_leaderboard",
                status: "inactive",
                startDate: today.ToString("yyyy-MM-dd"),
                endDate: today.AddDays(365).ToString("yyyy-MM-dd"),
                subtitle: "Llega a miles de compradores activos en RD",
                ctaText: "Anunciar",
                displayOrder: 99),

            BannerEntity.Create(
                title: "Banner Homepage Principal",
                image: "/images/banners/hero.jpg",
                link: "/vehiculos",
                placement: "homepage-hero",
                status: "active",
                startDate: today.AddDays(-60).ToString("yyyy-MM-dd"),
                endDate: today.AddDays(30).ToString("yyyy-MM-dd"),
                displayOrder: 1),
        };

        foreach (var b in demos)
            s_banners[b.Id] = b;
    }
}
