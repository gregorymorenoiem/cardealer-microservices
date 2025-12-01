namespace UserService.Application.DTOs
{
    public record PaginationDto(int Page, int PageSize, int TotalCount, int TotalPages);
}
