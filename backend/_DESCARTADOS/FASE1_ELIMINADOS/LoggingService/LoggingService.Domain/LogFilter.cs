namespace LoggingService.Domain;

public class LogFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public LogLevel? MinLevel { get; set; }
    public string? ServiceName { get; set; }
    public string? RequestId { get; set; }
    public string? TraceId { get; set; }
    public string? UserId { get; set; }
    public string? SearchText { get; set; }
    public bool? HasException { get; set; }
    public int PageSize { get; set; } = 100;
    public int PageNumber { get; set; } = 1;

    public bool IsValid()
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            return false;

        if (PageSize <= 0 || PageSize > 1000)
            return false;

        if (PageNumber <= 0)
            return false;

        return true;
    }

    public int GetSkip() => (PageNumber - 1) * PageSize;
}
