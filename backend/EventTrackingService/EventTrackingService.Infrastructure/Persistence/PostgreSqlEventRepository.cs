using EventTrackingService.Domain.Entities;
using EventTrackingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace EventTrackingService.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL-backed event repository for production use.
/// Replaces InMemoryEventRepository to persist events across pod restarts
/// and share data across replicas during auto-scaling.
/// 
/// Uses raw Npgsql for high-performance batch inserts.
/// Optimized with JSONB for event-specific data and proper indexing.
/// </summary>
public class PostgreSqlEventRepository : IEventRepository
{
    private readonly string _connectionString;
    private readonly ILogger<PostgreSqlEventRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _tableCreated;

    public PostgreSqlEventRepository(
        string connectionString,
        ILogger<PostgreSqlEventRepository> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private async Task EnsureTablesAsync(CancellationToken ct)
    {
        if (_tableCreated) return;

        const string sql = """
            CREATE TABLE IF NOT EXISTS tracked_events (
                id UUID PRIMARY KEY,
                event_type VARCHAR(100) NOT NULL,
                timestamp TIMESTAMPTZ NOT NULL,
                user_id UUID,
                session_id VARCHAR(100),
                ip_address VARCHAR(45),
                user_agent TEXT,
                referrer TEXT,
                current_url TEXT,
                device_type VARCHAR(20),
                browser VARCHAR(50),
                operating_system VARCHAR(50),
                country VARCHAR(10),
                city VARCHAR(100),
                event_data JSONB NOT NULL DEFAULT '{}',
                source VARCHAR(20),
                campaign VARCHAR(255),
                medium VARCHAR(100),
                content VARCHAR(255),
                -- Specialized fields (denormalized for query performance)
                page_url TEXT,
                page_title TEXT,
                search_query TEXT,
                results_count INT,
                clicked_position INT,
                vehicle_id UUID,
                vehicle_title TEXT,
                vehicle_price DECIMAL(18,2),
                time_spent_seconds INT,
                clicked_contact BOOLEAN DEFAULT FALSE,
                added_to_favorites BOOLEAN DEFAULT FALSE
            );

            -- Indexes for common query patterns
            CREATE INDEX IF NOT EXISTS idx_te_event_type_ts ON tracked_events (event_type, timestamp DESC);
            CREATE INDEX IF NOT EXISTS idx_te_user_id_ts ON tracked_events (user_id, timestamp DESC) WHERE user_id IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_te_session ON tracked_events (session_id, timestamp DESC) WHERE session_id IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_te_timestamp ON tracked_events (timestamp DESC);
            CREATE INDEX IF NOT EXISTS idx_te_vehicle_id ON tracked_events (vehicle_id, timestamp DESC) WHERE vehicle_id IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_te_page_url ON tracked_events (page_url, timestamp DESC) WHERE page_url IS NOT NULL;
            CREATE INDEX IF NOT EXISTS idx_te_search ON tracked_events (search_query, timestamp DESC) WHERE search_query IS NOT NULL;
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);

        _tableCreated = true;
        _logger.LogInformation("✅ EventTracking PostgreSQL tables ensured");
    }

    #region CREATE Operations

    public async Task IngestEventAsync(TrackedEvent trackedEvent, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        await InsertEventAsync(trackedEvent, cancellationToken);
        _logger.LogDebug("Event {EventType} ingested for session {SessionId}", trackedEvent.EventType, trackedEvent.SessionId);
    }

    public async Task IngestBatchAsync(IEnumerable<TrackedEvent> events, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        var list = events.ToList();
        if (list.Count == 0) return;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Use COPY for high-performance batch insert
        using var writer = conn.BeginBinaryImport("""
            COPY tracked_events (id, event_type, timestamp, user_id, session_id, ip_address, user_agent,
                referrer, current_url, device_type, browser, operating_system, country, city, event_data,
                source, campaign, medium, content, page_url, page_title, search_query, results_count,
                clicked_position, vehicle_id, vehicle_title, vehicle_price, time_spent_seconds,
                clicked_contact, added_to_favorites)
            FROM STDIN (FORMAT BINARY)
            """);

        foreach (var e in list)
        {
            writer.StartRow();
            WriteEventRow(writer, e);
        }

        await writer.CompleteAsync(cancellationToken);
        _logger.LogInformation("Batch of {Count} events ingested successfully (PostgreSQL)", list.Count);
    }

    private async Task InsertEventAsync(TrackedEvent e, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO tracked_events (id, event_type, timestamp, user_id, session_id, ip_address, user_agent,
                referrer, current_url, device_type, browser, operating_system, country, city, event_data,
                source, campaign, medium, content, page_url, page_title, search_query, results_count,
                clicked_position, vehicle_id, vehicle_title, vehicle_price, time_spent_seconds,
                clicked_contact, added_to_favorites)
            VALUES (@id, @eventType, @ts, @userId, @sessionId, @ip, @ua, @ref, @url, @device, @browser,
                @os, @country, @city, @eventData::jsonb, @source, @campaign, @medium, @content,
                @pageUrl, @pageTitle, @searchQuery, @resultsCount, @clickedPos, @vehicleId, @vehicleTitle,
                @vehiclePrice, @timeSpent, @clickedContact, @addedFav)
            ON CONFLICT (id) DO NOTHING
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("id", e.Id);
        cmd.Parameters.AddWithValue("eventType", e.EventType);
        cmd.Parameters.AddWithValue("ts", e.Timestamp);
        cmd.Parameters.AddWithValue("userId", (object?)e.UserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("sessionId", (object?)e.SessionId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ip", (object?)e.IpAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ua", (object?)e.UserAgent ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ref", (object?)e.Referrer ?? DBNull.Value);
        cmd.Parameters.AddWithValue("url", (object?)e.CurrentUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("device", (object?)e.DeviceType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("browser", (object?)e.Browser ?? DBNull.Value);
        cmd.Parameters.AddWithValue("os", (object?)e.OperatingSystem ?? DBNull.Value);
        cmd.Parameters.AddWithValue("country", (object?)e.Country ?? DBNull.Value);
        cmd.Parameters.AddWithValue("city", (object?)e.City ?? DBNull.Value);
        cmd.Parameters.AddWithValue("eventData", e.EventData ?? "{}");
        cmd.Parameters.AddWithValue("source", (object?)e.Source ?? DBNull.Value);
        cmd.Parameters.AddWithValue("campaign", (object?)e.Campaign ?? DBNull.Value);
        cmd.Parameters.AddWithValue("medium", (object?)e.Medium ?? DBNull.Value);
        cmd.Parameters.AddWithValue("content", (object?)e.Content ?? DBNull.Value);

        // Specialized fields from subclass types
        var (pageUrl, pageTitle, searchQuery, resultsCount, clickedPos, vehicleId, vehicleTitle, vehiclePrice, timeSpent, clickedContact, addedFav) = ExtractSpecializedFields(e);
        cmd.Parameters.AddWithValue("pageUrl", (object?)pageUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("pageTitle", (object?)pageTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("searchQuery", (object?)searchQuery ?? DBNull.Value);
        cmd.Parameters.AddWithValue("resultsCount", (object?)resultsCount ?? DBNull.Value);
        cmd.Parameters.AddWithValue("clickedPos", (object?)clickedPos ?? DBNull.Value);
        cmd.Parameters.AddWithValue("vehicleId", (object?)vehicleId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("vehicleTitle", (object?)vehicleTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("vehiclePrice", (object?)vehiclePrice ?? DBNull.Value);
        cmd.Parameters.AddWithValue("timeSpent", (object?)timeSpent ?? DBNull.Value);
        cmd.Parameters.AddWithValue("clickedContact", clickedContact);
        cmd.Parameters.AddWithValue("addedFav", addedFav);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private void WriteEventRow(NpgsqlBinaryImporter writer, TrackedEvent e)
    {
        var (pageUrl, pageTitle, searchQuery, resultsCount, clickedPos, vehicleId, vehicleTitle, vehiclePrice, timeSpent, clickedContact, addedFav) = ExtractSpecializedFields(e);

        writer.Write(e.Id, NpgsqlDbType.Uuid);
        writer.Write(e.EventType, NpgsqlDbType.Varchar);
        writer.Write(e.Timestamp, NpgsqlDbType.TimestampTz);
        WriteNullable(writer, e.UserId, NpgsqlDbType.Uuid);
        WriteNullable(writer, e.SessionId, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.IpAddress, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.UserAgent, NpgsqlDbType.Text);
        WriteNullable(writer, e.Referrer, NpgsqlDbType.Text);
        WriteNullable(writer, e.CurrentUrl, NpgsqlDbType.Text);
        WriteNullable(writer, e.DeviceType, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.Browser, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.OperatingSystem, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.Country, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.City, NpgsqlDbType.Varchar);
        writer.Write(e.EventData ?? "{}", NpgsqlDbType.Jsonb);
        WriteNullable(writer, e.Source, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.Campaign, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.Medium, NpgsqlDbType.Varchar);
        WriteNullable(writer, e.Content, NpgsqlDbType.Varchar);
        WriteNullable(writer, pageUrl, NpgsqlDbType.Text);
        WriteNullable(writer, pageTitle, NpgsqlDbType.Text);
        WriteNullable(writer, searchQuery, NpgsqlDbType.Text);
        WriteNullable(writer, resultsCount, NpgsqlDbType.Integer);
        WriteNullable(writer, clickedPos, NpgsqlDbType.Integer);
        WriteNullable(writer, vehicleId, NpgsqlDbType.Uuid);
        WriteNullable(writer, vehicleTitle, NpgsqlDbType.Text);
        WriteNullable(writer, vehiclePrice, NpgsqlDbType.Numeric);
        WriteNullable(writer, timeSpent, NpgsqlDbType.Integer);
        writer.Write(clickedContact, NpgsqlDbType.Boolean);
        writer.Write(addedFav, NpgsqlDbType.Boolean);
    }

    private static void WriteNullable<T>(NpgsqlBinaryImporter writer, T? value, NpgsqlDbType type)
    {
        if (value is null)
            writer.WriteNull();
        else
            writer.Write(value, type);
    }

    private static (string? pageUrl, string? pageTitle, string? searchQuery, int? resultsCount,
        int? clickedPos, Guid? vehicleId, string? vehicleTitle, decimal? vehiclePrice,
        int? timeSpent, bool clickedContact, bool addedFav) ExtractSpecializedFields(TrackedEvent e)
    {
        return e switch
        {
            PageViewEvent pv => (pv.PageUrl, pv.PageTitle, null, null, null, null, null, null, pv.TimeOnPage, false, false),
            SearchEvent se => (null, null, se.SearchQuery, se.ResultsCount, se.ClickedPosition, se.ClickedVehicleId, null, null, null, false, false),
            VehicleViewEvent vv => (null, null, null, null, null, vv.VehicleId, vv.VehicleTitle, vv.VehiclePrice, vv.TimeSpentSeconds, vv.ClickedContact, vv.AddedToFavorites),
            _ => (null, null, null, null, null, null, null, null, null, false, false)
        };
    }

    #endregion

    #region READ Operations

    public async Task<List<TrackedEvent>> GetEventsByTypeAsync(string eventType, DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE event_type = @eventType AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC LIMIT @limit
            """;

        return await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["eventType"] = eventType, ["start"] = startDate, ["end"] = endDate, ["limit"] = limit
        }, cancellationToken);
    }

    public async Task<List<TrackedEvent>> GetEventsByUserAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE user_id = @userId AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC
            """;

        return await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["userId"] = userId, ["start"] = startDate, ["end"] = endDate
        }, cancellationToken);
    }

    public async Task<List<TrackedEvent>> GetEventsBySessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = "SELECT * FROM tracked_events WHERE session_id = @sessionId ORDER BY timestamp DESC";

        return await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["sessionId"] = sessionId
        }, cancellationToken);
    }

    public async Task<List<PageViewEvent>> GetPageViewsAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE event_type = 'page_view' AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC LIMIT @limit
            """;

        var events = await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["start"] = startDate, ["end"] = endDate, ["limit"] = limit
        }, cancellationToken);

        return events.Select(MapToPageView).ToList();
    }

    public async Task<List<SearchEvent>> GetSearchesAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE event_type = 'search' AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC LIMIT @limit
            """;

        var events = await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["start"] = startDate, ["end"] = endDate, ["limit"] = limit
        }, cancellationToken);

        return events.Select(MapToSearch).ToList();
    }

    public async Task<List<VehicleViewEvent>> GetVehicleViewsAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE event_type = 'vehicle_view' AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC LIMIT @limit
            """;

        var events = await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["start"] = startDate, ["end"] = endDate, ["limit"] = limit
        }, cancellationToken);

        return events.Select(MapToVehicleView).ToList();
    }

    public async Task<List<VehicleViewEvent>> GetVehicleViewsByVehicleIdAsync(Guid vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT * FROM tracked_events
            WHERE event_type = 'vehicle_view' AND vehicle_id = @vehicleId AND timestamp >= @start AND timestamp <= @end
            ORDER BY timestamp DESC
            """;

        var events = await QueryEventsAsync(sql, new Dictionary<string, object>
        {
            ["vehicleId"] = vehicleId, ["start"] = startDate, ["end"] = endDate
        }, cancellationToken);

        return events.Select(MapToVehicleView).ToList();
    }

    #endregion

    #region Analytics Operations

    public async Task<Dictionary<string, long>> CountEventsByTypeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT event_type, COUNT(*) as cnt
            FROM tracked_events WHERE timestamp >= @start AND timestamp <= @end
            GROUP BY event_type
            """;

        var result = new Dictionary<string, long>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result[reader.GetString(0)] = reader.GetInt64(1);
        return result;
    }

    public async Task<Dictionary<string, long>> CountPageViewsByUrlAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT page_url, COUNT(*) as cnt
            FROM tracked_events
            WHERE event_type = 'page_view' AND timestamp >= @start AND timestamp <= @end AND page_url IS NOT NULL
            GROUP BY page_url ORDER BY cnt DESC LIMIT @topN
            """;

        var result = new Dictionary<string, long>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        cmd.Parameters.AddWithValue("topN", topN);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result[reader.GetString(0)] = reader.GetInt64(1);
        return result;
    }

    public async Task<Dictionary<string, (long Count, double AvgResults, double CTR)>> GetTopSearchQueriesAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT search_query, COUNT(*) as cnt,
                   COALESCE(AVG(results_count), 0) as avg_results,
                   COALESCE(COUNT(clicked_position) FILTER (WHERE clicked_position IS NOT NULL)::FLOAT / NULLIF(COUNT(*), 0) * 100, 0) as ctr
            FROM tracked_events
            WHERE event_type = 'search' AND timestamp >= @start AND timestamp <= @end AND search_query IS NOT NULL
            GROUP BY search_query ORDER BY cnt DESC LIMIT @topN
            """;

        var result = new Dictionary<string, (long Count, double AvgResults, double CTR)>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        cmd.Parameters.AddWithValue("topN", topN);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result[reader.GetString(0)] = (reader.GetInt64(1), reader.GetDouble(2), reader.GetDouble(3));
        return result;
    }

    public async Task<Dictionary<Guid, (string Title, long Views, double AvgTime, long Contacts, long Favorites, double ConversionRate)>> GetMostViewedVehiclesAsync(DateTime startDate, DateTime endDate, int topN = 100, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT vehicle_id, 
                   MAX(vehicle_title) as title,
                   COUNT(*) as views,
                   COALESCE(AVG(time_spent_seconds) FILTER (WHERE time_spent_seconds IS NOT NULL), 0) as avg_time,
                   COUNT(*) FILTER (WHERE clicked_contact = TRUE) as contacts,
                   COUNT(*) FILTER (WHERE added_to_favorites = TRUE) as favorites,
                   COALESCE(COUNT(*) FILTER (WHERE clicked_contact = TRUE OR added_to_favorites = TRUE)::FLOAT / NULLIF(COUNT(*), 0) * 100, 0) as conv_rate
            FROM tracked_events
            WHERE event_type = 'vehicle_view' AND vehicle_id IS NOT NULL AND timestamp >= @start AND timestamp <= @end
            GROUP BY vehicle_id ORDER BY views DESC LIMIT @topN
            """;

        var result = new Dictionary<Guid, (string Title, long Views, double AvgTime, long Contacts, long Favorites, double ConversionRate)>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        cmd.Parameters.AddWithValue("topN", topN);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetGuid(0)] = (
                reader.GetString(1), reader.GetInt64(2), reader.GetDouble(3),
                reader.GetInt64(4), reader.GetInt64(5), reader.GetDouble(6)
            );
        }
        return result;
    }

    public async Task<long> GetUniqueVisitorsCountAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT COUNT(DISTINCT session_id)
            FROM tracked_events
            WHERE timestamp >= @start AND timestamp <= @end AND session_id IS NOT NULL
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        return (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0L);
    }

    public async Task<double> GetConversionRateAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = """
            SELECT COALESCE(
                COUNT(*) FILTER (WHERE clicked_contact = TRUE OR added_to_favorites = TRUE)::FLOAT 
                / NULLIF(COUNT(*), 0) * 100, 0)
            FROM tracked_events
            WHERE event_type = 'vehicle_view' AND timestamp >= @start AND timestamp <= @end
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", startDate);
        cmd.Parameters.AddWithValue("end", endDate);
        return (double)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0.0);
    }

    #endregion

    #region Cleanup Operations

    public async Task DeleteOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        await EnsureTablesAsync(cancellationToken);
        const string sql = "DELETE FROM tracked_events WHERE timestamp < @olderThan";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("olderThan", olderThan);
        var deleted = await cmd.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Deleted {Count} events older than {OlderThan}", deleted, olderThan);
    }

    public async Task ArchiveOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        // In PostgreSQL, archiving can be done by moving to a partitioned archive table
        // For now, log the action — ClickHouse migration will handle long-term archiving
        _logger.LogInformation("Archive requested for events older than {OlderThan}. Use pg_dump or ClickHouse migration.", olderThan);
        await Task.CompletedTask;
    }

    #endregion

    #region Helpers

    private async Task<List<TrackedEvent>> QueryEventsAsync(string sql, Dictionary<string, object> parameters, CancellationToken ct)
    {
        var events = new List<TrackedEvent>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (key, value) in parameters)
            cmd.Parameters.AddWithValue(key, value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            events.Add(MapFromReader(reader));
        }
        return events;
    }

    private static TrackedEvent MapFromReader(NpgsqlDataReader reader)
    {
        var eventType = reader.GetString(reader.GetOrdinal("event_type"));
        var baseEvent = new TrackedEvent
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            EventType = eventType,
            Timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp")),
            UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? null : reader.GetGuid(reader.GetOrdinal("user_id")),
            SessionId = reader.IsDBNull(reader.GetOrdinal("session_id")) ? null : reader.GetString(reader.GetOrdinal("session_id")),
            IpAddress = reader.IsDBNull(reader.GetOrdinal("ip_address")) ? null : reader.GetString(reader.GetOrdinal("ip_address")),
            UserAgent = reader.IsDBNull(reader.GetOrdinal("user_agent")) ? null : reader.GetString(reader.GetOrdinal("user_agent")),
            Referrer = reader.IsDBNull(reader.GetOrdinal("referrer")) ? null : reader.GetString(reader.GetOrdinal("referrer")),
            CurrentUrl = reader.IsDBNull(reader.GetOrdinal("current_url")) ? null : reader.GetString(reader.GetOrdinal("current_url")),
            DeviceType = reader.IsDBNull(reader.GetOrdinal("device_type")) ? null : reader.GetString(reader.GetOrdinal("device_type")),
            Browser = reader.IsDBNull(reader.GetOrdinal("browser")) ? null : reader.GetString(reader.GetOrdinal("browser")),
            OperatingSystem = reader.IsDBNull(reader.GetOrdinal("operating_system")) ? null : reader.GetString(reader.GetOrdinal("operating_system")),
            Country = reader.IsDBNull(reader.GetOrdinal("country")) ? null : reader.GetString(reader.GetOrdinal("country")),
            City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city")),
            EventData = reader.IsDBNull(reader.GetOrdinal("event_data")) ? "{}" : reader.GetString(reader.GetOrdinal("event_data")),
            Source = reader.IsDBNull(reader.GetOrdinal("source")) ? null : reader.GetString(reader.GetOrdinal("source")),
            Campaign = reader.IsDBNull(reader.GetOrdinal("campaign")) ? null : reader.GetString(reader.GetOrdinal("campaign")),
            Medium = reader.IsDBNull(reader.GetOrdinal("medium")) ? null : reader.GetString(reader.GetOrdinal("medium")),
            Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString(reader.GetOrdinal("content")),
        };

        return baseEvent;
    }

    private static PageViewEvent MapToPageView(TrackedEvent e)
    {
        return new PageViewEvent
        {
            Id = e.Id, EventType = e.EventType, Timestamp = e.Timestamp, UserId = e.UserId,
            SessionId = e.SessionId, IpAddress = e.IpAddress, UserAgent = e.UserAgent,
            Referrer = e.Referrer, CurrentUrl = e.CurrentUrl, DeviceType = e.DeviceType,
            Browser = e.Browser, OperatingSystem = e.OperatingSystem, Country = e.Country,
            City = e.City, EventData = e.EventData, Source = e.Source, Campaign = e.Campaign,
            Medium = e.Medium, Content = e.Content,
            PageUrl = e.CurrentUrl ?? string.Empty,
            PageTitle = string.Empty
        };
    }

    private static SearchEvent MapToSearch(TrackedEvent e)
    {
        return new SearchEvent
        {
            Id = e.Id, EventType = e.EventType, Timestamp = e.Timestamp, UserId = e.UserId,
            SessionId = e.SessionId, IpAddress = e.IpAddress, UserAgent = e.UserAgent,
            Referrer = e.Referrer, CurrentUrl = e.CurrentUrl, DeviceType = e.DeviceType,
            Browser = e.Browser, OperatingSystem = e.OperatingSystem, Country = e.Country,
            City = e.City, EventData = e.EventData, Source = e.Source, Campaign = e.Campaign,
            Medium = e.Medium, Content = e.Content,
            SearchQuery = string.Empty,
            ResultsCount = 0
        };
    }

    private static VehicleViewEvent MapToVehicleView(TrackedEvent e)
    {
        return new VehicleViewEvent
        {
            Id = e.Id, EventType = e.EventType, Timestamp = e.Timestamp, UserId = e.UserId,
            SessionId = e.SessionId, IpAddress = e.IpAddress, UserAgent = e.UserAgent,
            Referrer = e.Referrer, CurrentUrl = e.CurrentUrl, DeviceType = e.DeviceType,
            Browser = e.Browser, OperatingSystem = e.OperatingSystem, Country = e.Country,
            City = e.City, EventData = e.EventData, Source = e.Source, Campaign = e.Campaign,
            Medium = e.Medium, Content = e.Content,
            VehicleId = Guid.Empty,
            VehicleTitle = string.Empty
        };
    }

    #endregion
}
