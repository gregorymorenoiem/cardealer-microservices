using MediaService.Application.DTOs;
using MediaService.Shared;
using MediatR;

namespace MediaService.Application.Features.Media.Queries.ListMedia;

public class ListMediaQuery : IRequest<ApiResponse<ListMediaResponse>>
{
    public string? OwnerId { get; set; }
    public string? Context { get; set; }
    public string? MediaType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public bool IncludeVariants { get; set; } = false;

    public ListMediaQuery() { }

    public ListMediaQuery(
        string? ownerId = null,
        string? context = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 50)
    {
        OwnerId = ownerId;
        Context = context;
        FromDate = fromDate;
        ToDate = toDate;
        Page = page;
        PageSize = pageSize;
    }
}
