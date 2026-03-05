using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdminService.Application.UseCases.Content;

// ────────────────────────────────────────────────────────────────────
// GetContentOverviewQueryHandler
// ────────────────────────────────────────────────────────────────────
public class GetContentOverviewQueryHandler : IRequestHandler<GetContentOverviewQuery, ContentOverviewResponse>
{
    private readonly IBannerRepository _banners;
    private readonly ILogger<GetContentOverviewQueryHandler> _logger;

    public GetContentOverviewQueryHandler(IBannerRepository banners, ILogger<GetContentOverviewQueryHandler> logger)
    {
        _banners = banners;
        _logger = logger;
    }

    public async Task<ContentOverviewResponse> Handle(GetContentOverviewQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting content overview");
        var bannerEntities = await _banners.GetAllAsync();
        var banners = bannerEntities.Select(MapBanner).ToList();
        var pages = GetSamplePages();
        var posts = GetSampleBlogPosts();
        return new ContentOverviewResponse(banners, pages, posts, "24,580");
    }

    private static List<StaticPage> GetSamplePages() => new()
    {
        new("p1", "Términos y Condiciones", "terminos-condiciones", "published",
            DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd"), "Admin", 3200),
        new("p2", "Política de Privacidad", "politica-privacidad", "published",
            DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd"), "Admin", 1800),
        new("p3", "Guía para Vendedores", "guia-vendedores", "published",
            DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"), "Admin", 5600),
        new("p4", "Preguntas Frecuentes", "faq", "published",
            DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-dd"), "Admin", 9800),
    };

    private static List<BlogPost> GetSampleBlogPosts() => new()
    {
        new("bp1", "5 consejos para vender tu carro más rápido", "5-consejos-vender-carro",
            "published", "OKLA Team", DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd"), 4200, "Consejos"),
        new("bp2", "Los carros más buscados en República Dominicana 2025", "carros-mas-buscados-2025",
            "published", "OKLA Team", DateTime.UtcNow.AddDays(-12).ToString("yyyy-MM-dd"), 8900, "Tendencias"),
        new("bp3", "Cómo evitar estafas al comprar un vehículo", "evitar-estafas-vehiculo",
            "draft", "OKLA Team", null, 0, "Seguridad"),
    };

    internal static Banner MapBanner(BannerEntity e) => new(
        e.Id, e.Title, e.Subtitle, e.Image, e.MobileImage, e.Link,
        e.CtaText, e.Placement, e.Status, e.StartDate, e.EndDate,
        e.Views, e.Clicks, e.DisplayOrder);
}

// ────────────────────────────────────────────────────────────────────
// GetBannersQueryHandler  (admin — all)
// ────────────────────────────────────────────────────────────────────
public class GetBannersQueryHandler : IRequestHandler<GetBannersQuery, List<Banner>>
{
    private readonly IBannerRepository _banners;

    public GetBannersQueryHandler(IBannerRepository banners) => _banners = banners;

    public async Task<List<Banner>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
    {
        var entities = await _banners.GetAllAsync();
        return entities.Select(GetContentOverviewQueryHandler.MapBanner).ToList();
    }
}

// ────────────────────────────────────────────────────────────────────
// GetPublicBannersQueryHandler  (public — active only, by placement)
// ────────────────────────────────────────────────────────────────────
public class GetPublicBannersQueryHandler : IRequestHandler<GetPublicBannersQuery, List<Banner>>
{
    private readonly IBannerRepository _banners;

    public GetPublicBannersQueryHandler(IBannerRepository banners) => _banners = banners;

    public async Task<List<Banner>> Handle(GetPublicBannersQuery request, CancellationToken cancellationToken)
    {
        var entities = await _banners.GetByPlacementAsync(request.Placement, activeOnly: true);
        return entities.Select(GetContentOverviewQueryHandler.MapBanner).ToList();
    }
}

// ────────────────────────────────────────────────────────────────────
// CreateBannerCommandHandler
// ────────────────────────────────────────────────────────────────────
public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, Banner>
{
    private readonly IBannerRepository _banners;
    private readonly ILogger<CreateBannerCommandHandler> _logger;

    public CreateBannerCommandHandler(IBannerRepository banners, ILogger<CreateBannerCommandHandler> logger)
    {
        _banners = banners;
        _logger = logger;
    }

    public async Task<Banner> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var d = request.Data;
        var entity = BannerEntity.Create(
            d.Title, d.Image, d.Link, d.Placement, d.Status,
            d.StartDate, d.EndDate, d.Subtitle, d.MobileImage, d.CtaText, d.DisplayOrder);

        await _banners.CreateAsync(entity);
        _logger.LogInformation("Banner created: {BannerId} placement={Placement}", entity.Id, entity.Placement);
        return GetContentOverviewQueryHandler.MapBanner(entity);
    }
}

// ────────────────────────────────────────────────────────────────────
// UpdateBannerCommandHandler
// ────────────────────────────────────────────────────────────────────
public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, Banner?>
{
    private readonly IBannerRepository _banners;
    private readonly ILogger<UpdateBannerCommandHandler> _logger;

    public UpdateBannerCommandHandler(IBannerRepository banners, ILogger<UpdateBannerCommandHandler> logger)
    {
        _banners = banners;
        _logger = logger;
    }

    public async Task<Banner?> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _banners.GetByIdAsync(request.BannerId);
        if (entity is null) return null;

        var d = request.Data;
        entity.Update(d.Title, d.Image, d.Link, d.Placement, d.Status,
            d.StartDate, d.EndDate, d.Subtitle, d.MobileImage, d.CtaText, d.DisplayOrder);

        await _banners.UpdateAsync(request.BannerId, entity);
        _logger.LogInformation("Banner updated: {BannerId}", request.BannerId);
        return GetContentOverviewQueryHandler.MapBanner(entity);
    }
}

// ────────────────────────────────────────────────────────────────────
// DeleteBannerCommandHandler
// ────────────────────────────────────────────────────────────────────
public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand>
{
    private readonly IBannerRepository _banners;
    private readonly ILogger<DeleteBannerCommandHandler> _logger;

    public DeleteBannerCommandHandler(IBannerRepository banners, ILogger<DeleteBannerCommandHandler> logger)
    {
        _banners = banners;
        _logger = logger;
    }

    public async Task Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        await _banners.DeleteAsync(request.BannerId);
        _logger.LogInformation("Banner deleted: {BannerId}", request.BannerId);
    }
}

// ────────────────────────────────────────────────────────────────────
// RecordBannerViewCommandHandler / RecordBannerClickCommandHandler
// ────────────────────────────────────────────────────────────────────
public class RecordBannerViewCommandHandler : IRequestHandler<RecordBannerViewCommand>
{
    private readonly IBannerRepository _banners;

    public RecordBannerViewCommandHandler(IBannerRepository banners) => _banners = banners;

    public Task Handle(RecordBannerViewCommand request, CancellationToken cancellationToken)
        => _banners.RecordViewAsync(request.BannerId);
}

public class RecordBannerClickCommandHandler : IRequestHandler<RecordBannerClickCommand>
{
    private readonly IBannerRepository _banners;

    public RecordBannerClickCommandHandler(IBannerRepository banners) => _banners = banners;

    public Task Handle(RecordBannerClickCommand request, CancellationToken cancellationToken)
        => _banners.RecordClickAsync(request.BannerId);
}

// ────────────────────────────────────────────────────────────────────
// GetStaticPagesQueryHandler / GetBlogPostsQueryHandler
// ────────────────────────────────────────────────────────────────────
public class GetStaticPagesQueryHandler : IRequestHandler<GetStaticPagesQuery, List<StaticPage>>
{
    public Task<List<StaticPage>> Handle(GetStaticPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = new List<StaticPage>
        {
            new("p1", "Términos y Condiciones", "terminos-condiciones", "published",
                DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd"), "Admin", 3200),
            new("p2", "Política de Privacidad", "politica-privacidad", "published",
                DateTime.UtcNow.AddDays(-60).ToString("yyyy-MM-dd"), "Admin", 1800),
            new("p3", "Guía para Vendedores", "guia-vendedores", "published",
                DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"), "Admin", 5600),
            new("p4", "Preguntas Frecuentes", "faq", "published",
                DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-dd"), "Admin", 9800),
        };
        return Task.FromResult(pages);
    }
}

public class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, List<BlogPost>>
{
    public Task<List<BlogPost>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = new List<BlogPost>
        {
            new("bp1", "5 consejos para vender tu carro más rápido", "5-consejos-vender-carro",
                "published", "OKLA Team", DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd"), 4200, "Consejos"),
            new("bp2", "Los carros más buscados en República Dominicana 2025", "carros-mas-buscados-2025",
                "published", "OKLA Team", DateTime.UtcNow.AddDays(-12).ToString("yyyy-MM-dd"), 8900, "Tendencias"),
            new("bp3", "Cómo evitar estafas al comprar un vehículo", "evitar-estafas-vehiculo",
                "draft", "OKLA Team", null, 0, "Seguridad"),
        };
        return Task.FromResult(posts);
    }
}
