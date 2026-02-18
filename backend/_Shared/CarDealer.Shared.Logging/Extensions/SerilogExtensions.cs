using CarDealer.Shared.Logging.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Span;

namespace CarDealer.Shared.Logging.Extensions;

/// <summary>
/// Extension methods for configuring centralized Serilog logging
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Adds standardized Serilog logging with Seq, Console, and optional RabbitMQ
    /// </summary>
    public static IHostBuilder UseStandardSerilog(
        this IHostBuilder hostBuilder,
        string serviceName,
        Action<LoggingOptions>? configureOptions = null)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfig) =>
        {
            var options = new LoggingOptions { ServiceName = serviceName };

            // Bind from configuration
            context.Configuration.GetSection(LoggingOptions.SectionName).Bind(options);

            // Allow programmatic overrides
            configureOptions?.Invoke(options);

            ConfigureLogger(loggerConfig, options, context.Configuration);
        });
    }

    /// <summary>
    /// Adds standardized Serilog logging with Seq, Console, and optional RabbitMQ (WebApplicationBuilder overload)
    /// </summary>
    public static WebApplicationBuilder UseStandardSerilog(
        this WebApplicationBuilder builder,
        string serviceName,
        Action<LoggingOptions>? configureOptions = null)
    {
        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            var options = new LoggingOptions { ServiceName = serviceName };

            // Bind from configuration
            context.Configuration.GetSection(LoggingOptions.SectionName).Bind(options);

            // Allow programmatic overrides
            configureOptions?.Invoke(options);

            ConfigureLogger(loggerConfig, options, context.Configuration);
        });

        return builder;
    }

    /// <summary>
    /// Creates a bootstrap logger for startup logging before DI is available
    /// </summary>
    public static ILogger CreateBootstrapLogger(string serviceName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures the Serilog logger with all sinks
    /// </summary>
    private static void ConfigureLogger(
        LoggerConfiguration loggerConfig,
        LoggingOptions options,
        IConfiguration configuration)
    {
        var minimumLevel = ParseLogLevel(options.MinimumLevel);

        loggerConfig
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", options.ServiceName)
            .Enrich.WithProperty("Environment", options.Environment)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithSpan(); // OpenTelemetry TraceId/SpanId

        // Console sink
        if (options.EnableConsole)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: minimumLevel);
        }

        // Seq sink
        if (options.Seq.Enabled)
        {
            var seqLevel = ParseLogLevel(options.Seq.MinimumLevel);
            loggerConfig.WriteTo.Seq(
                serverUrl: options.Seq.ServerUrl,
                apiKey: options.Seq.ApiKey,
                restrictedToMinimumLevel: seqLevel);
        }

        // RabbitMQ sink for critical logs (optional - requires proper RabbitMQ setup)
        // Note: RabbitMQ sink is disabled for now. Error events are sent via ErrorHandling middleware.
        // To enable, uncomment the following and ensure Serilog.Sinks.RabbitMQ is properly configured.
        /*
        if (options.RabbitMQ.Enabled)
        {
            var rabbitLevel = ParseLogLevel(options.RabbitMQ.MinimumLevel);
            loggerConfig.WriteTo.RabbitMQ((config, sinkConfig) =>
            {
                config.Port = options.RabbitMQ.Port;
                config.Username = options.RabbitMQ.Username;
                config.Password = options.RabbitMQ.Password;
                config.Exchange = options.RabbitMQ.Exchange;
                config.ExchangeType = options.RabbitMQ.ExchangeType;
                config.RouteKey = options.RabbitMQ.RouteKey;
                foreach (var hostname in options.RabbitMQ.Hostnames)
                {
                    config.Hostnames.Add(hostname);
                }
                sinkConfig.RestrictedToMinimumLevel = rabbitLevel;
            });
        }
        */

        // File sink (optional, mainly for local debugging)
        if (options.File.Enabled)
        {
            var rollingInterval = Enum.Parse<Serilog.RollingInterval>(options.File.RollingInterval, true);
            loggerConfig.WriteTo.File(
                path: options.File.Path,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: options.File.RetainedFileCountLimit,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");
        }
    }

    /// <summary>
    /// Parses a string log level to LogEventLevel
    /// </summary>
    private static LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" or "info" => LogEventLevel.Information,
            "warning" or "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
