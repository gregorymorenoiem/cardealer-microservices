using MediatR;
using UserService.Domain.Interfaces;

namespace UserService.Application.UseCases.Dealers.GetDealer;

/// <summary>
/// DTO for sitemap entries — minimal data (slug + updatedAt) for XML sitemap generation.
/// </summary>
public record DealerSitemapEntry(string Slug, DateTime? UpdatedAt);

/// <summary>
/// Query to get all active dealers with slugs for sitemap generation.
/// Used by: frontend/web-next/src/app/sitemap.ts → GET /api/dealers/sitemap
/// </summary>
public record GetDealersSitemapQuery : IRequest<IEnumerable<DealerSitemapEntry>>;

public class GetDealersSitemapQueryHandler : IRequestHandler<GetDealersSitemapQuery, IEnumerable<DealerSitemapEntry>>
{
    private readonly IDealerRepository _dealerRepository;

    public GetDealersSitemapQueryHandler(IDealerRepository dealerRepository)
    {
        _dealerRepository = dealerRepository;
    }

    public async Task<IEnumerable<DealerSitemapEntry>> Handle(GetDealersSitemapQuery request, CancellationToken cancellationToken)
    {
        var dealers = await _dealerRepository.GetForSitemapAsync();

        return dealers.Select(d => new DealerSitemapEntry(
            Slug: d.Slug!,
            UpdatedAt: d.UpdatedAt ?? d.CreatedAt
        ));
    }
}
