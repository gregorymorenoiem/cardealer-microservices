namespace CarDealer.Contracts.DTOs.Common;

/// <summary>
/// Standard pagination DTO used across all microservices.
/// </summary>
public class PaginationDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
