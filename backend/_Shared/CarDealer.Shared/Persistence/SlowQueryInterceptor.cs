using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// EF Core command interceptor that detects and logs slow database queries.
/// Queries exceeding the configured threshold are logged as warnings.
/// 
/// Register via: options.AddInterceptors(new SlowQueryInterceptor(logger, thresholdMs));
/// </summary>
public class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor>? _logger;
    private readonly int _slowQueryThresholdMs;

    /// <summary>
    /// Creates a new SlowQueryInterceptor.
    /// </summary>
    /// <param name="logger">Logger for slow query warnings.</param>
    /// <param name="slowQueryThresholdMs">Threshold in milliseconds. Queries slower than this are logged. Default: 500ms.</param>
    public SlowQueryInterceptor(
        ILogger<SlowQueryInterceptor>? logger = null,
        int slowQueryThresholdMs = 500)
    {
        _logger = logger;
        _slowQueryThresholdMs = slowQueryThresholdMs;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogIfSlow(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogIfSlow(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogIfSlow(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
    {
        var durationMs = eventData.Duration.TotalMilliseconds;

        if (durationMs >= _slowQueryThresholdMs)
        {
            _logger?.LogWarning(
                "⚠️ SLOW QUERY detected ({DurationMs:F0}ms, threshold: {ThresholdMs}ms):\n" +
                "Command: {CommandText}\n" +
                "Parameters: {Parameters}",
                durationMs,
                _slowQueryThresholdMs,
                command.CommandText,
                FormatParameters(command));
        }
    }

    private static string FormatParameters(DbCommand command)
    {
        if (command.Parameters.Count == 0)
            return "(none)";

        var parts = new List<string>();
        foreach (DbParameter param in command.Parameters)
        {
            parts.Add($"{param.ParameterName}={param.Value}");
        }
        return string.Join(", ", parts);
    }
}
