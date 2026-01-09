using EventTrackingService.Domain.Entities;
using EventTrackingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;

namespace EventTrackingService.Infrastructure.Persistence;

/// <summary>
/// Implementación del repositorio de eventos usando ClickHouse
/// ClickHouse es óptimo para analytics de alto volumen
/// </summary>
public class ClickHouseEventRepository : IEventRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ClickHouseEventRepository> _logger;

    public ClickHouseEventRepository(string connectionString, ILogger<ClickHouseEventRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    #region CREATE Operations

    public async Task IngestEventAsync(TrackedEvent trackedEvent, CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            INSERT INTO tracked_events 
            (id, event_type, timestamp, user_id, session_id, ip_address, user_agent, 
             referrer, current_url, device_type, browser, operating_system, country, city, 
             event_data, source, campaign, medium, content)
            VALUES 
            (@id, @event_type, @timestamp, @user_id, @session_id, @ip_address, @user_agent, 
             @referrer, @current_url, @device_type, @browser, @operating_system, @country, @city, 
             @event_data, @source, @campaign, @medium, @content)";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddEventParameters(command, trackedEvent);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Event {EventType} ingested for session {SessionId}", trackedEvent.EventType, trackedEvent.SessionId);
    }

    public async Task IngestBatchAsync(IEnumerable<TrackedEvent> events, CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var bulkCopy = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = "tracked_events"
        };

        var eventsList = events.ToList();
        var dataTable = CreateDataTable(eventsList);

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        _logger.LogInformation("Batch of {Count} events ingested successfully", eventsList.Count);
    }

    #endregion

    #region READ Operations

    public async Task<List<TrackedEvent>> GetEventsByTypeAsync(
        string eventType, 
        DateTime startDate, 
        DateTime endDate, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT * FROM tracked_events
            WHERE event_type = @event_type
              AND timestamp >= @start_date
              AND timestamp <= @end_date
            ORDER BY timestamp DESC
            LIMIT @limit";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@event_type", eventType);
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);
        command.Parameters.AddWithValue("@limit", limit);

        var events = new List<TrackedEvent>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToTrackedEvent(reader));
        }

        return events;
    }

    public async Task<List<TrackedEvent>> GetEventsByUserAsync(
        Guid userId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT * FROM tracked_events
            WHERE user_id = @user_id
              AND timestamp >= @start_date
              AND timestamp <= @end_date
            ORDER BY timestamp DESC";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);

        var events = new List<TrackedEvent>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToTrackedEvent(reader));
        }

        return events;
    }

    public async Task<List<TrackedEvent>> GetEventsBySessionAsync(
        string sessionId, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT * FROM tracked_events
            WHERE session_id = @session_id
            ORDER BY timestamp ASC";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@session_id", sessionId);

        var events = new List<TrackedEvent>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapToTrackedEvent(reader));
        }

        return events;
    }

    #endregion

    #region AGGREGATIONS

    public async Task<Dictionary<string, long>> CountEventsByTypeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT event_type, count(*) as count
            FROM tracked_events
            WHERE timestamp >= @start_date AND timestamp <= @end_date
            GROUP BY event_type";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);

        var result = new Dictionary<string, long>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetInt64(1);
        }

        return result;
    }

    public async Task<Dictionary<string, (long Count, double AvgResults, double CTR)>> GetTopSearchQueriesAsync(
        DateTime startDate, 
        DateTime endDate, 
        int topN = 20, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // ClickHouse optimizado para agregaciones
        var sql = @"
            SELECT 
                JSONExtractString(event_data, 'SearchQuery') as query,
                count(*) as search_count,
                avg(JSONExtractInt(event_data, 'ResultsCount')) as avg_results,
                countIf(JSONExtractInt(event_data, 'ClickedPosition') > 0) / count(*) as ctr
            FROM tracked_events
            WHERE event_type = 'Search'
              AND timestamp >= @start_date 
              AND timestamp <= @end_date
            GROUP BY query
            ORDER BY search_count DESC
            LIMIT @top_n";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);
        command.Parameters.AddWithValue("@top_n", topN);

        var result = new Dictionary<string, (long Count, double AvgResults, double CTR)>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var query = reader.GetString(0);
            var count = reader.GetInt64(1);
            var avgResults = reader.GetDouble(2);
            var ctr = reader.GetDouble(3);
            result[query] = (count, avgResults, ctr);
        }

        return result;
    }

    public async Task<Dictionary<Guid, (string Title, long Views, double AvgTime, long Contacts, long Favorites, double ConversionRate)>> 
        GetMostViewedVehiclesAsync(
            DateTime startDate, 
            DateTime endDate, 
            int topN = 20, 
            CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT 
                JSONExtractString(event_data, 'VehicleId') as vehicle_id,
                JSONExtractString(event_data, 'VehicleTitle') as title,
                count(*) as views,
                avg(JSONExtractInt(event_data, 'TimeSpentSeconds')) as avg_time,
                countIf(JSONExtractInt(event_data, 'ClickedContact') = 1) as contacts,
                countIf(JSONExtractInt(event_data, 'AddedToFavorites') = 1) as favorites,
                (countIf(JSONExtractInt(event_data, 'ClickedContact') = 1) / count(*)) as conversion_rate
            FROM tracked_events
            WHERE event_type = 'VehicleView'
              AND timestamp >= @start_date 
              AND timestamp <= @end_date
            GROUP BY vehicle_id, title
            ORDER BY views DESC
            LIMIT @top_n";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);
        command.Parameters.AddWithValue("@top_n", topN);

        var result = new Dictionary<Guid, (string Title, long Views, double AvgTime, long Contacts, long Favorites, double ConversionRate)>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var vehicleId = Guid.Parse(reader.GetString(0));
            var title = reader.GetString(1);
            var views = reader.GetInt64(2);
            var avgTime = reader.GetDouble(3);
            var contacts = reader.GetInt64(4);
            var favorites = reader.GetInt64(5);
            var conversionRate = reader.GetDouble(6);
            result[vehicleId] = (title, views, avgTime, contacts, favorites, conversionRate);
        }

        return result;
    }

    public async Task<long> GetUniqueVisitorsCountAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT countDistinct(session_id)
            FROM tracked_events
            WHERE timestamp >= @start_date AND timestamp <= @end_date";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);

        var count = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(count);
    }

    public async Task<double> GetConversionRateAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Conversion = (VehicleView con ClickedContact) / TotalVehicleViews
        var sql = @"
            SELECT 
                countIf(event_type = 'VehicleView' AND JSONExtractInt(event_data, 'ClickedContact') = 1) as conversions,
                countIf(event_type = 'VehicleView') as total_views
            FROM tracked_events
            WHERE timestamp >= @start_date AND timestamp <= @end_date";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var conversions = reader.GetInt64(0);
            var totalViews = reader.GetInt64(1);
            return totalViews > 0 ? (double)conversions / totalViews : 0;
        }

        return 0;
    }

    #endregion

    #region RETENTION

    public async Task DeleteOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "ALTER TABLE tracked_events DELETE WHERE timestamp < @older_than";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@older_than", olderThan);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogWarning("Deleted events older than {OlderThan}", olderThan);
    }

    public async Task ArchiveOldEventsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Move to archive table
        var sql = @"
            INSERT INTO tracked_events_archive
            SELECT * FROM tracked_events
            WHERE timestamp < @older_than";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@older_than", olderThan);

        await command.ExecuteNonQueryAsync(cancellationToken);

        // Delete from main table
        await DeleteOldEventsAsync(olderThan, cancellationToken);

        _logger.LogInformation("Archived events older than {OlderThan}", olderThan);
    }

    #endregion

    #region Specialized Queries

    public async Task<List<PageViewEvent>> GetPageViewsAsync(
        DateTime startDate, 
        DateTime endDate, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        var events = await GetEventsByTypeAsync("PageView", startDate, endDate, limit, cancellationToken);
        return events.OfType<PageViewEvent>().ToList();
    }

    public async Task<List<SearchEvent>> GetSearchesAsync(
        DateTime startDate, 
        DateTime endDate, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        var events = await GetEventsByTypeAsync("Search", startDate, endDate, limit, cancellationToken);
        return events.OfType<SearchEvent>().ToList();
    }

    public async Task<List<VehicleViewEvent>> GetVehicleViewsAsync(
        DateTime startDate, 
        DateTime endDate, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        var events = await GetEventsByTypeAsync("VehicleView", startDate, endDate, limit, cancellationToken);
        return events.OfType<VehicleViewEvent>().ToList();
    }

    public async Task<List<VehicleViewEvent>> GetVehicleViewsByVehicleIdAsync(
        Guid vehicleId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT * FROM tracked_events
            WHERE event_type = 'VehicleView'
              AND JSONExtractString(event_data, 'VehicleId') = @vehicle_id
              AND timestamp >= @start_date
              AND timestamp <= @end_date
            ORDER BY timestamp DESC";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@vehicle_id", vehicleId.ToString());
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);

        var events = new List<VehicleViewEvent>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add((VehicleViewEvent)MapToTrackedEvent(reader));
        }

        return events;
    }

    public async Task<Dictionary<string, long>> CountPageViewsByUrlAsync(
        DateTime startDate, 
        DateTime endDate, 
        int topN = 20, 
        CancellationToken cancellationToken = default)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT 
                JSONExtractString(event_data, 'PageUrl') as url,
                count(*) as views
            FROM tracked_events
            WHERE event_type = 'PageView'
              AND timestamp >= @start_date 
              AND timestamp <= @end_date
            GROUP BY url
            ORDER BY views DESC
            LIMIT @top_n";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@start_date", startDate);
        command.Parameters.AddWithValue("@end_date", endDate);
        command.Parameters.AddWithValue("@top_n", topN);

        var result = new Dictionary<string, long>();
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[reader.GetString(0)] = reader.GetInt64(1);
        }

        return result;
    }

    #endregion

    #region Helper Methods

    private static void AddEventParameters(ClickHouseCommand command, TrackedEvent evt)
    {
        command.Parameters.AddWithValue("@id", evt.Id);
        command.Parameters.AddWithValue("@event_type", evt.EventType);
        command.Parameters.AddWithValue("@timestamp", evt.Timestamp);
        command.Parameters.AddWithValue("@user_id", evt.UserId.HasValue ? evt.UserId.Value.ToString() : DBNull.Value);
        command.Parameters.AddWithValue("@session_id", evt.SessionId);
        command.Parameters.AddWithValue("@ip_address", evt.IpAddress);
        command.Parameters.AddWithValue("@user_agent", evt.UserAgent);
        command.Parameters.AddWithValue("@referrer", (object?)evt.Referrer ?? DBNull.Value);
        command.Parameters.AddWithValue("@current_url", evt.CurrentUrl);
        command.Parameters.AddWithValue("@device_type", evt.DeviceType);
        command.Parameters.AddWithValue("@browser", evt.Browser);
        command.Parameters.AddWithValue("@operating_system", evt.OperatingSystem);
        command.Parameters.AddWithValue("@country", (object?)evt.Country ?? DBNull.Value);
        command.Parameters.AddWithValue("@city", (object?)evt.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@event_data", (object?)evt.EventData ?? DBNull.Value);
        command.Parameters.AddWithValue("@source", (object?)evt.Source ?? DBNull.Value);
        command.Parameters.AddWithValue("@campaign", (object?)evt.Campaign ?? DBNull.Value);
        command.Parameters.AddWithValue("@medium", (object?)evt.Medium ?? DBNull.Value);
        command.Parameters.AddWithValue("@content", (object?)evt.Content ?? DBNull.Value);
    }

    private static DataTable CreateDataTable(List<TrackedEvent> events)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(Guid));
        table.Columns.Add("event_type", typeof(string));
        table.Columns.Add("timestamp", typeof(DateTime));
        table.Columns.Add("user_id", typeof(string));
        table.Columns.Add("session_id", typeof(string));
        table.Columns.Add("ip_address", typeof(string));
        table.Columns.Add("user_agent", typeof(string));
        table.Columns.Add("referrer", typeof(string));
        table.Columns.Add("current_url", typeof(string));
        table.Columns.Add("device_type", typeof(string));
        table.Columns.Add("browser", typeof(string));
        table.Columns.Add("operating_system", typeof(string));
        table.Columns.Add("country", typeof(string));
        table.Columns.Add("city", typeof(string));
        table.Columns.Add("event_data", typeof(string));
        table.Columns.Add("source", typeof(string));
        table.Columns.Add("campaign", typeof(string));
        table.Columns.Add("medium", typeof(string));
        table.Columns.Add("content", typeof(string));

        foreach (var evt in events)
        {
            table.Rows.Add(
                evt.Id,
                evt.EventType,
                evt.Timestamp,
                evt.UserId?.ToString(),
                evt.SessionId,
                evt.IpAddress,
                evt.UserAgent,
                evt.Referrer,
                evt.CurrentUrl,
                evt.DeviceType,
                evt.Browser,
                evt.OperatingSystem,
                evt.Country,
                evt.City,
                evt.EventData,
                evt.Source,
                evt.Campaign,
                evt.Medium,
                evt.Content
            );
        }

        return table;
    }

    private static TrackedEvent MapToTrackedEvent(IDataReader reader)
    {
        return new TrackedEvent
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            EventType = reader.GetString(reader.GetOrdinal("event_type")),
            Timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp")),
            UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) 
                ? null 
                : Guid.Parse(reader.GetString(reader.GetOrdinal("user_id"))),
            SessionId = reader.GetString(reader.GetOrdinal("session_id")),
            IpAddress = reader.GetString(reader.GetOrdinal("ip_address")),
            UserAgent = reader.GetString(reader.GetOrdinal("user_agent")),
            Referrer = reader.IsDBNull(reader.GetOrdinal("referrer")) ? null : reader.GetString(reader.GetOrdinal("referrer")),
            CurrentUrl = reader.GetString(reader.GetOrdinal("current_url")),
            DeviceType = reader.GetString(reader.GetOrdinal("device_type")),
            Browser = reader.GetString(reader.GetOrdinal("browser")),
            OperatingSystem = reader.GetString(reader.GetOrdinal("operating_system")),
            Country = reader.IsDBNull(reader.GetOrdinal("country")) ? null : reader.GetString(reader.GetOrdinal("country")),
            City = reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city")),
            EventData = reader.IsDBNull(reader.GetOrdinal("event_data")) ? null : reader.GetString(reader.GetOrdinal("event_data")),
            Source = reader.IsDBNull(reader.GetOrdinal("source")) ? null : reader.GetString(reader.GetOrdinal("source")),
            Campaign = reader.IsDBNull(reader.GetOrdinal("campaign")) ? null : reader.GetString(reader.GetOrdinal("campaign")),
            Medium = reader.IsDBNull(reader.GetOrdinal("medium")) ? null : reader.GetString(reader.GetOrdinal("medium")),
            Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString(reader.GetOrdinal("content"))
        };
    }

    #endregion
}
