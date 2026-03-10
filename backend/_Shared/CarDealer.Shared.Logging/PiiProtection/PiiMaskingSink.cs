using Serilog.Core;
using Serilog.Events;

namespace CarDealer.Shared.Logging.PiiProtection;

/// <summary>
/// Serilog sink wrapper that intercepts log events and masks PII property values
/// before forwarding to the real sink. Defense-in-depth for Ley 172-13 compliance.
///
/// Wraps any ILogEventSink (Console, Seq, File, etc.) and ensures that even if
/// developers accidentally log PII in structured properties, the values are masked
/// before reaching any output sink.
///
/// Recognized PII properties: Email, Phone, Password, Token, DocumentNumber,
/// Content (chat), IpAddress, FullName, and variants.
/// </summary>
public sealed class PiiMaskingSink : ILogEventSink, IDisposable
{
    private readonly ILogEventSink _innerSink;

    public PiiMaskingSink(ILogEventSink innerSink)
    {
        _innerSink = innerSink ?? throw new ArgumentNullException(nameof(innerSink));
    }

    public void Emit(LogEvent logEvent)
    {
        // Fast path: check if any property needs masking
        bool needsMasking = false;
        foreach (var kvp in logEvent.Properties)
        {
            if (PiiMasking.IsPiiProperty(kvp.Key))
            {
                needsMasking = true;
                break;
            }
        }

        if (!needsMasking)
        {
            _innerSink.Emit(logEvent);
            return;
        }

        // Build masked property list
        var maskedProperties = new List<LogEventProperty>(logEvent.Properties.Count);
        foreach (var kvp in logEvent.Properties)
        {
            if (PiiMasking.IsPiiProperty(kvp.Key)
                && kvp.Value is ScalarValue sv
                && sv.Value is string strVal)
            {
                maskedProperties.Add(
                    new LogEventProperty(kvp.Key,
                        new ScalarValue(PiiMasking.Mask(kvp.Key, strVal))));
            }
            else
            {
                maskedProperties.Add(
                    new LogEventProperty(kvp.Key, kvp.Value));
            }
        }

        // Create masked event (same template, different property values)
        var maskedEvent = new LogEvent(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.Exception,
            logEvent.MessageTemplate,
            maskedProperties);

        _innerSink.Emit(maskedEvent);
    }

    public void Dispose()
    {
        (_innerSink as IDisposable)?.Dispose();
    }
}
