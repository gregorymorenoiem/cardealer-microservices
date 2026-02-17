namespace TracingService.Domain.Enums;

/// <summary>
/// Defines the kind of span (client, server, producer, consumer, internal)
/// Following OpenTelemetry SpanKind specification
/// </summary>
public enum SpanKind
{
    /// <summary>
    /// Indicates that the span describes a synchronous request to some remote service.
    /// </summary>
    Client = 1,

    /// <summary>
    /// Indicates that the span covers server-side handling of a synchronous RPC or other remote request.
    /// </summary>
    Server = 2,

    /// <summary>
    /// Indicates that the span describes the initiator of an asynchronous request.
    /// </summary>
    Producer = 3,

    /// <summary>
    /// Indicates that the span describes a child of an asynchronous Producer request.
    /// </summary>
    Consumer = 4,

    /// <summary>
    /// Indicates that the span represents an internal operation within an application.
    /// </summary>
    Internal = 5
}
