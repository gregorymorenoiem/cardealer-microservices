using System.Net.Http.Json;
using System.Text.Json;
using TracingService.Application.Interfaces;
using TracingService.Domain.Entities;
using TracingService.Domain.Enums;

namespace TracingService.Infrastructure.Services;

/// <summary>
/// Implementation of ITraceQueryService that queries Jaeger HTTP API
/// </summary>
public class JaegerTraceQueryService : ITraceQueryService
{
    private readonly HttpClient _httpClient;
    private readonly string _jaegerQueryUrl;

    public JaegerTraceQueryService(HttpClient httpClient, string jaegerQueryUrl)
    {
        _httpClient = httpClient;
        _jaegerQueryUrl = jaegerQueryUrl;
    }

    public async Task<Trace?> GetTraceByIdAsync(string traceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_jaegerQueryUrl}/api/traces/{traceId}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var jaegerResponse = await response.Content.ReadFromJsonAsync<JaegerTraceResponse>(cancellationToken: cancellationToken);
            
            if (jaegerResponse?.Data == null || jaegerResponse.Data.Count == 0)
                return null;

            return ConvertJaegerTraceToTrace(jaegerResponse.Data[0]);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Trace>> SearchTracesAsync(
        string? serviceName = null,
        string? operationName = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? minDurationMs = null,
        int? maxDurationMs = null,
        bool? hasError = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(serviceName))
                queryParams.Add($"service={Uri.EscapeDataString(serviceName)}");
            
            if (!string.IsNullOrEmpty(operationName))
                queryParams.Add($"operation={Uri.EscapeDataString(operationName)}");
            
            if (startTime.HasValue)
                queryParams.Add($"start={new DateTimeOffset(startTime.Value).ToUnixTimeMilliseconds()}000");
            
            if (endTime.HasValue)
                queryParams.Add($"end={new DateTimeOffset(endTime.Value).ToUnixTimeMilliseconds()}000");
            
            if (minDurationMs.HasValue)
                queryParams.Add($"minDuration={minDurationMs.Value}ms");
            
            if (maxDurationMs.HasValue)
                queryParams.Add($"maxDuration={maxDurationMs.Value}ms");
            
            queryParams.Add($"limit={limit}");
            
            if (hasError.HasValue && hasError.Value)
                queryParams.Add("tags={\"error\":\"true\"}");

            var url = $"{_jaegerQueryUrl}/api/traces?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return new List<Trace>();

            var jaegerResponse = await response.Content.ReadFromJsonAsync<JaegerTraceResponse>(cancellationToken: cancellationToken);
            
            if (jaegerResponse?.Data == null)
                return new List<Trace>();

            return jaegerResponse.Data.Select(ConvertJaegerTraceToTrace).ToList();
        }
        catch
        {
            return new List<Trace>();
        }
    }

    public async Task<List<Span>> GetSpansByTraceIdAsync(string traceId, CancellationToken cancellationToken = default)
    {
        var trace = await GetTraceByIdAsync(traceId, cancellationToken);
        return trace?.Spans ?? new List<Span>();
    }

    public async Task<TraceStatistics> GetTraceStatisticsAsync(
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? serviceName = null,
        CancellationToken cancellationToken = default)
    {
        var traces = await SearchTracesAsync(
            serviceName: serviceName,
            startTime: startTime,
            endTime: endTime,
            limit: 1000,
            cancellationToken: cancellationToken);

        if (traces.Count == 0)
        {
            return new TraceStatistics
            {
                StartTime = startTime ?? DateTime.UtcNow.AddHours(-1),
                EndTime = endTime ?? DateTime.UtcNow
            };
        }

        var durations = traces.Where(t => t.DurationMs.HasValue)
                              .Select(t => t.DurationMs!.Value)
                              .OrderBy(d => d)
                              .ToList();

        var statistics = new TraceStatistics
        {
            TotalTraces = traces.Count,
            TotalSpans = traces.Sum(t => t.SpanCount),
            TracesWithErrors = traces.Count(t => t.HasError),
            AverageDurationMs = durations.Any() ? durations.Average() : 0,
            MedianDurationMs = durations.Any() ? durations[durations.Count / 2] : 0,
            P95DurationMs = durations.Any() ? durations[(int)(durations.Count * 0.95)] : 0,
            P99DurationMs = durations.Any() ? durations[(int)(durations.Count * 0.99)] : 0,
            StartTime = startTime ?? traces.Min(t => t.StartTime),
            EndTime = endTime ?? traces.Max(t => t.EndTime ?? DateTime.UtcNow)
        };

        var slowest = traces.OrderByDescending(t => t.DurationMs).FirstOrDefault();
        if (slowest != null)
        {
            statistics.SlowestTraceId = slowest.TraceId;
            statistics.SlowestTraceDurationMs = slowest.DurationMs;
        }

        var spansByService = traces.SelectMany(t => t.Spans)
                                   .GroupBy(s => s.ServiceName)
                                   .ToDictionary(g => g.Key, g => g.Count());
        
        statistics.SpansByService = spansByService;

        if (spansByService.Any())
        {
            var mostActive = spansByService.OrderByDescending(kvp => kvp.Value).First();
            statistics.MostActiveService = mostActive.Key;
            statistics.MostActiveServiceSpanCount = mostActive.Value;
        }

        var errorsByService = traces.SelectMany(t => t.Spans)
                                    .Where(s => s.HasError)
                                    .GroupBy(s => s.ServiceName)
                                    .ToDictionary(g => g.Key, g => g.Count());
        
        statistics.ErrorsByService = errorsByService;

        return statistics;
    }

    public async Task<List<string>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_jaegerQueryUrl}/api/services";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var jaegerResponse = await response.Content.ReadFromJsonAsync<JaegerServicesResponse>(cancellationToken: cancellationToken);
            return jaegerResponse?.Data ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<List<string>> GetOperationsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_jaegerQueryUrl}/api/services/{Uri.EscapeDataString(serviceName)}/operations";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var jaegerResponse = await response.Content.ReadFromJsonAsync<JaegerOperationsResponse>(cancellationToken: cancellationToken);
            return jaegerResponse?.Data?.Select(op => op.Name).ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private Trace ConvertJaegerTraceToTrace(JaegerTrace jaegerTrace)
    {
        var spans = jaegerTrace.Spans.Select(ConvertJaegerSpanToSpan).ToList();
        var rootSpan = spans.FirstOrDefault(s => string.IsNullOrEmpty(s.ParentSpanId));

        return new Trace
        {
            TraceId = jaegerTrace.TraceID,
            RootSpan = rootSpan,
            Spans = spans,
            StartTime = spans.Min(s => s.StartTime),
            EndTime = spans.Max(s => s.EndTime)
        };
    }

    private Span ConvertJaegerSpanToSpan(JaegerSpan jaegerSpan)
    {
        var span = new Span
        {
            SpanId = jaegerSpan.SpanID,
            TraceId = jaegerSpan.TraceID,
            Name = jaegerSpan.OperationName,
            StartTime = DateTimeOffset.FromUnixTimeMilliseconds(jaegerSpan.StartTime / 1000).UtcDateTime,
            ServiceName = jaegerSpan.Process?.ServiceName ?? "unknown",
            Tags = jaegerSpan.Tags?.ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "") ?? new Dictionary<string, string>(),
            Events = jaegerSpan.Logs?.Select(log => new SpanEvent
            {
                Name = log.Fields?.FirstOrDefault(f => f.Key == "event")?.Value?.ToString() ?? "log",
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(log.Timestamp / 1000).UtcDateTime,
                Attributes = log.Fields?.ToDictionary(f => f.Key, f => f.Value?.ToString() ?? "") ?? new Dictionary<string, string>()
            }).ToList() ?? new List<SpanEvent>()
        };

        if (jaegerSpan.References?.Any() == true)
        {
            var parentRef = jaegerSpan.References.FirstOrDefault(r => r.RefType == "CHILD_OF");
            if (parentRef != null)
            {
                span.ParentSpanId = parentRef.SpanID;
            }
        }

        if (jaegerSpan.Duration > 0)
        {
            span.EndTime = span.StartTime.AddMilliseconds(jaegerSpan.Duration / 1000.0);
        }

        // Determine span kind
        var kindTag = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "span.kind");
        span.Kind = kindTag?.Value?.ToString()?.ToLower() switch
        {
            "client" => SpanKind.Client,
            "server" => SpanKind.Server,
            "producer" => SpanKind.Producer,
            "consumer" => SpanKind.Consumer,
            _ => SpanKind.Internal
        };

        // Check for errors
        var errorTag = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "error");
        if (errorTag != null && bool.TryParse(errorTag.Value?.ToString(), out var hasError) && hasError)
        {
            span.Status = SpanStatus.Error;
            var errorMsg = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "error.message");
            span.StatusMessage = errorMsg?.Value?.ToString();
        }
        else
        {
            span.Status = SpanStatus.Ok;
        }

        // Extract HTTP info if available
        var httpMethod = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "http.method");
        if (httpMethod != null)
        {
            span.HttpMethod = httpMethod.Value?.ToString();
        }

        var httpUrl = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "http.url");
        if (httpUrl != null)
        {
            span.HttpUrl = httpUrl.Value?.ToString();
        }

        var httpStatusCode = jaegerSpan.Tags?.FirstOrDefault(t => t.Key == "http.status_code");
        if (httpStatusCode != null && int.TryParse(httpStatusCode.Value?.ToString(), out var statusCode))
        {
            span.HttpStatusCode = statusCode;
        }

        return span;
    }

    // Jaeger API response models
    private class JaegerTraceResponse
    {
        public List<JaegerTrace>? Data { get; set; }
    }

    private class JaegerTrace
    {
        public string TraceID { get; set; } = string.Empty;
        public List<JaegerSpan> Spans { get; set; } = new();
    }

    private class JaegerSpan
    {
        public string TraceID { get; set; } = string.Empty;
        public string SpanID { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;
        public List<JaegerReference>? References { get; set; }
        public long StartTime { get; set; }
        public long Duration { get; set; }
        public List<JaegerTag>? Tags { get; set; }
        public List<JaegerLog>? Logs { get; set; }
        public JaegerProcess? Process { get; set; }
    }

    private class JaegerReference
    {
        public string RefType { get; set; } = string.Empty;
        public string TraceID { get; set; } = string.Empty;
        public string SpanID { get; set; } = string.Empty;
    }

    private class JaegerTag
    {
        public string Key { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    private class JaegerLog
    {
        public long Timestamp { get; set; }
        public List<JaegerTag>? Fields { get; set; }
    }

    private class JaegerProcess
    {
        public string ServiceName { get; set; } = string.Empty;
    }

    private class JaegerServicesResponse
    {
        public List<string>? Data { get; set; }
    }

    private class JaegerOperationsResponse
    {
        public List<JaegerOperation>? Data { get; set; }
    }

    private class JaegerOperation
    {
        public string Name { get; set; } = string.Empty;
    }
}
