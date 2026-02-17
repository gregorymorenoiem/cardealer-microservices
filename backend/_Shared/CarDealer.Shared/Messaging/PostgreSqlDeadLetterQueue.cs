using Microsoft.Extensions.Logging;
using Npgsql;

namespace CarDealer.Shared.Messaging;

/// <summary>
/// PostgreSQL-backed Dead Letter Queue that persists failed events across pod restarts.
/// Replaces InMemoryDeadLetterQueue to ensure data survives auto-scaling events.
/// 
/// Uses raw Npgsql (no EF Core dependency) — auto-creates the dead_letter_events table.
/// All services share the same PostgreSQL instance, each with its own service_name filter.
/// </summary>
public class PostgreSqlDeadLetterQueue : ISharedDeadLetterQueue
{
    private readonly string _connectionString;
    private readonly string _serviceName;
    private readonly int _maxRetries;
    private readonly ILogger<PostgreSqlDeadLetterQueue> _logger;
    private bool _tableCreated;

    public PostgreSqlDeadLetterQueue(
        string connectionString,
        string serviceName,
        ILogger<PostgreSqlDeadLetterQueue> logger,
        int maxRetries = 5)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _logger = logger;
        _maxRetries = maxRetries;
    }

    /// <summary>
    /// Ensures the dead_letter_events table exists (idempotent).
    /// </summary>
    private async Task EnsureTableAsync(CancellationToken ct)
    {
        if (_tableCreated) return;

        const string sql = """
            CREATE TABLE IF NOT EXISTS dead_letter_events (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                service_name VARCHAR(100) NOT NULL,
                event_type VARCHAR(255) NOT NULL,
                event_json TEXT NOT NULL,
                failed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                retry_count INT NOT NULL DEFAULT 0,
                max_retries INT NOT NULL DEFAULT 5,
                next_retry_at TIMESTAMPTZ,
                last_error TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            
            CREATE INDEX IF NOT EXISTS idx_dle_service_retry 
                ON dead_letter_events (service_name, next_retry_at) 
                WHERE retry_count < max_retries;
            
            CREATE INDEX IF NOT EXISTS idx_dle_service_name 
                ON dead_letter_events (service_name);
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);

        _tableCreated = true;
        _logger.LogInformation("✅ Dead letter queue table ensured for {ServiceName}", _serviceName);
    }

    public async Task EnqueueAsync(DeadLetterEvent failedEvent, CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);

        const string sql = """
            INSERT INTO dead_letter_events (id, service_name, event_type, event_json, failed_at, retry_count, max_retries, next_retry_at, last_error)
            VALUES (@id, @serviceName, @eventType, @eventJson, @failedAt, @retryCount, @maxRetries, @nextRetryAt, @lastError)
            ON CONFLICT (id) DO NOTHING
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", failedEvent.Id);
        cmd.Parameters.AddWithValue("serviceName", _serviceName);
        cmd.Parameters.AddWithValue("eventType", failedEvent.EventType);
        cmd.Parameters.AddWithValue("eventJson", failedEvent.EventJson);
        cmd.Parameters.AddWithValue("failedAt", failedEvent.FailedAt);
        cmd.Parameters.AddWithValue("retryCount", failedEvent.RetryCount);
        cmd.Parameters.AddWithValue("maxRetries", failedEvent.MaxRetries);
        cmd.Parameters.AddWithValue("nextRetryAt", (object?)failedEvent.NextRetryAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("lastError", (object?)failedEvent.LastError ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(ct);

        _logger.LogInformation(
            "📮 Event {EventId} ({EventType}) enqueued to PostgreSQL DLQ for {ServiceName}",
            failedEvent.Id, failedEvent.EventType, _serviceName);
    }

    public async Task<IReadOnlyList<DeadLetterEvent>> GetEventsReadyForRetryAsync(CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);

        const string sql = """
            SELECT id, service_name, event_type, event_json, failed_at, retry_count, max_retries, next_retry_at, last_error
            FROM dead_letter_events
            WHERE service_name = @serviceName
              AND retry_count < max_retries
              AND (next_retry_at IS NULL OR next_retry_at <= @now)
            ORDER BY failed_at ASC
            LIMIT 100
            """;

        var events = new List<DeadLetterEvent>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("serviceName", _serviceName);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            events.Add(new DeadLetterEvent
            {
                Id = reader.GetGuid(0),
                ServiceName = reader.GetString(1),
                EventType = reader.GetString(2),
                EventJson = reader.GetString(3),
                FailedAt = reader.GetDateTime(4),
                RetryCount = reader.GetInt32(5),
                MaxRetries = reader.GetInt32(6),
                NextRetryAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                LastError = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }

        return events;
    }

    public async Task RemoveAsync(Guid eventId, CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);

        const string sql = "DELETE FROM dead_letter_events WHERE id = @id AND service_name = @serviceName";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", eventId);
        cmd.Parameters.AddWithValue("serviceName", _serviceName);
        var deleted = await cmd.ExecuteNonQueryAsync(ct);

        if (deleted > 0)
        {
            _logger.LogInformation(
                "✅ Event {EventId} removed from PostgreSQL DLQ for {ServiceName}",
                eventId, _serviceName);
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string error, CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);

        // Calculate next retry with exponential backoff in SQL
        const string sql = """
            UPDATE dead_letter_events 
            SET retry_count = retry_count + 1,
                last_error = @error,
                next_retry_at = CASE 
                    WHEN retry_count + 1 >= max_retries THEN NULL
                    ELSE NOW() + (POWER(2, retry_count) || ' minutes')::INTERVAL
                END
            WHERE id = @id AND service_name = @serviceName
            RETURNING retry_count, max_retries
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", eventId);
        cmd.Parameters.AddWithValue("serviceName", _serviceName);
        cmd.Parameters.AddWithValue("error", error);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var retryCount = reader.GetInt32(0);
            var maxRetries = reader.GetInt32(1);

            if (retryCount >= maxRetries)
            {
                _logger.LogError(
                    "❌ Event {EventId} exceeded max retries ({MaxRetries}) in {ServiceName}. Last error: {Error}",
                    eventId, maxRetries, _serviceName, error);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Event {EventId} failed retry {RetryCount}/{MaxRetries} in {ServiceName}. Error: {Error}",
                    eventId, retryCount, maxRetries, _serviceName, error);
            }
        }
    }

    public async Task<(int TotalEvents, int ReadyForRetry, int MaxRetriesReached)> GetStatsAsync(CancellationToken ct = default)
    {
        await EnsureTableAsync(ct);

        const string sql = """
            SELECT 
                COUNT(*) AS total,
                COUNT(*) FILTER (WHERE retry_count < max_retries AND (next_retry_at IS NULL OR next_retry_at <= @now)) AS ready,
                COUNT(*) FILTER (WHERE retry_count >= max_retries) AS exhausted
            FROM dead_letter_events
            WHERE service_name = @serviceName
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("serviceName", _serviceName);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return (
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetInt32(2)
            );
        }

        return (0, 0, 0);
    }
}
