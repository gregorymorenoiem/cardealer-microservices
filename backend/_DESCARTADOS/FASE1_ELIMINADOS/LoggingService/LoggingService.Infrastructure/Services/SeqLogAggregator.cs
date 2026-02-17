using LoggingService.Application.Interfaces;
using LoggingService.Domain;
using Serilog.Events;
using System.Net.Http.Json;

namespace LoggingService.Infrastructure.Services;

public class SeqLogAggregator : ILogAggregator
{
    private readonly HttpClient _httpClient;
    private readonly string _seqUrl;

    public SeqLogAggregator(IHttpClientFactory httpClientFactory, string seqUrl)
    {
        _httpClient = httpClientFactory.CreateClient();
        _seqUrl = seqUrl.TrimEnd('/');
    }

    public async Task<IEnumerable<LogEntry>> QueryLogsAsync(LogFilter filter, CancellationToken cancellationToken = default)
    {
        var queryParams = BuildSeqQuery(filter);
        var url = $"{_seqUrl}/api/events?{queryParams}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<SeqEventsResponse>(url, cancellationToken);

            if (response?.Events == null)
                return Enumerable.Empty<LogEntry>();

            return response.Events.Select(MapToLogEntry);
        }
        catch
        {
            // Si Seq no está disponible, retornar lista vacía
            return Enumerable.Empty<LogEntry>();
        }
    }

    public async Task<LogEntry?> GetLogByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var url = $"{_seqUrl}/api/events/{id}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<SeqEvent>(url, cancellationToken);
            return response != null ? MapToLogEntry(response) : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<LogStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var filter = new LogFilter
        {
            StartDate = startDate ?? DateTime.UtcNow.AddHours(-24),
            EndDate = endDate ?? DateTime.UtcNow,
            PageSize = 1000 // Get more data for statistics
        };

        var logs = await QueryLogsAsync(filter, cancellationToken);
        var logsList = logs.ToList();

        return new LogStatistics
        {
            TotalLogs = logsList.Count,
            TraceCount = logsList.Count(l => l.Level == Domain.LogLevel.Trace),
            DebugCount = logsList.Count(l => l.Level == Domain.LogLevel.Debug),
            InformationCount = logsList.Count(l => l.Level == Domain.LogLevel.Information),
            WarningCount = logsList.Count(l => l.Level == Domain.LogLevel.Warning),
            ErrorCount = logsList.Count(l => l.Level == Domain.LogLevel.Error),
            CriticalCount = logsList.Count(l => l.Level == Domain.LogLevel.Critical),
            LogsByService = logsList.GroupBy(l => l.ServiceName)
                                   .ToDictionary(g => g.Key, g => g.Count()),
            OldestLog = logsList.Any() ? logsList.Min(l => l.Timestamp) : null,
            NewestLog = logsList.Any() ? logsList.Max(l => l.Timestamp) : null
        };
    }

    private string BuildSeqQuery(LogFilter filter)
    {
        var parameters = new List<string>
        {
            $"count={filter.PageSize}",
            $"skip={filter.GetSkip()}"
        };

        if (filter.StartDate.HasValue)
            parameters.Add($"fromDateUtc={filter.StartDate.Value:O}");

        if (filter.EndDate.HasValue)
            parameters.Add($"toDateUtc={filter.EndDate.Value:O}");

        if (filter.MinLevel.HasValue)
            parameters.Add($"level={MapToSeqLevel(filter.MinLevel.Value)}");

        if (!string.IsNullOrEmpty(filter.ServiceName))
            parameters.Add($"filter=ServiceName='{filter.ServiceName}'");

        if (!string.IsNullOrEmpty(filter.RequestId))
            parameters.Add($"filter=RequestId='{filter.RequestId}'");

        if (!string.IsNullOrEmpty(filter.SearchText))
            parameters.Add($"filter=@Message like '%{filter.SearchText}%'");

        return string.Join("&", parameters);
    }

    private LogEntry MapToLogEntry(SeqEvent seqEvent)
    {
        return new LogEntry
        {
            Id = seqEvent.Id ?? Guid.NewGuid().ToString(),
            Timestamp = seqEvent.Timestamp,
            Level = MapFromSeqLevel(seqEvent.Level),
            Message = seqEvent.MessageTemplate ?? string.Empty,
            ServiceName = seqEvent.Properties?.GetValueOrDefault("ServiceName")?.ToString() ?? "Unknown",
            RequestId = seqEvent.Properties?.GetValueOrDefault("RequestId")?.ToString(),
            TraceId = seqEvent.Properties?.GetValueOrDefault("TraceId")?.ToString(),
            SpanId = seqEvent.Properties?.GetValueOrDefault("SpanId")?.ToString(),
            UserId = seqEvent.Properties?.GetValueOrDefault("UserId")?.ToString(),
            Exception = seqEvent.Exception,
            Properties = seqEvent.Properties ?? new Dictionary<string, object>()
        };
    }

    private string MapToSeqLevel(Domain.LogLevel level) => level switch
    {
        Domain.LogLevel.Trace => "Verbose",
        Domain.LogLevel.Debug => "Debug",
        Domain.LogLevel.Information => "Information",
        Domain.LogLevel.Warning => "Warning",
        Domain.LogLevel.Error => "Error",
        Domain.LogLevel.Critical => "Fatal",
        _ => "Information"
    };

    private Domain.LogLevel MapFromSeqLevel(string? level) => level?.ToLowerInvariant() switch
    {
        "verbose" => Domain.LogLevel.Trace,
        "debug" => Domain.LogLevel.Debug,
        "information" => Domain.LogLevel.Information,
        "warning" => Domain.LogLevel.Warning,
        "error" => Domain.LogLevel.Error,
        "fatal" => Domain.LogLevel.Critical,
        _ => Domain.LogLevel.Information
    };

    // DTOs para deserialización de respuestas de Seq
    private class SeqEventsResponse
    {
        public List<SeqEvent>? Events { get; set; }
    }

    private class SeqEvent
    {
        public string? Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Level { get; set; }
        public string? MessageTemplate { get; set; }
        public string? Exception { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }
}
