using MediatR;

namespace AdminService.Application.UseCases.Content;

public record GetContentOverviewQuery : IRequest<ContentOverviewResponse>;
public record GetBannersQuery : IRequest<List<Banner>>;
public record GetPublicBannersQuery(string Placement) : IRequest<List<Banner>>;
public record GetStaticPagesQuery : IRequest<List<StaticPage>>;
public record GetBlogPostsQuery : IRequest<List<BlogPost>>;
public record DeleteBannerCommand(string BannerId) : IRequest;
public record CreateBannerCommand(CreateBannerRequest Data) : IRequest<Banner>;
public record UpdateBannerCommand(string BannerId, UpdateBannerRequest Data) : IRequest<Banner?>;
public record RecordBannerViewCommand(string BannerId) : IRequest;
public record RecordBannerClickCommand(string BannerId) : IRequest;
