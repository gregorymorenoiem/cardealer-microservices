namespace StripePaymentService.Domain.Enums;

/// <summary>
/// Tipo de método de pago en Stripe
/// </summary>
public enum PaymentMethodType
{
    Card = 0,
    BankAccount = 1,
    ApplePay = 2,
    GooglePay = 3,
    AffirmPayment = 4,
    Alipay = 5,
    AuBecsDebit = 6,
    BanContact = 7,
    Boleto = 8,
    EPS = 9,
    FPX = 10,
    Giropay = 11,
    IDeal = 12,
    Klarna = 13,
    KonbiniPayment = 14,
    MercadoPago = 15,
    Multibanco = 16,
    OXXOPayment = 17,
    PayPal = 18,
    PromptPay = 19,
    SepaDebit = 20,
    Sofort = 21,
    WeChatPay = 22
}
