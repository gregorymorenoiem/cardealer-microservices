namespace StripePaymentService.Domain.Enums;

/// <summary>
/// Estado de Payment Intent en Stripe
/// </summary>
public enum PaymentIntentStatus
{
    RequiresPaymentMethod = 0,
    RequiresConfirmation = 1,
    RequiresAction = 2,
    Processing = 3,
    RequiresCapture = 4,
    Canceled = 5,
    Succeeded = 6
}
