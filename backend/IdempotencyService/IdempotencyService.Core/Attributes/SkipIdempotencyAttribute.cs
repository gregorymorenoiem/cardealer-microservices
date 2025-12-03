namespace IdempotencyService.Core.Attributes;

/// <summary>
/// Marks an endpoint to skip idempotency checking even if the controller has IdempotentAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class SkipIdempotencyAttribute : Attribute
{
}
