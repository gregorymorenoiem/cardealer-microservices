using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

// ============================================================================
// REQUEST DTOs
// ============================================================================

/// <summary>
/// Solicitud para crear/guardar un nuevo método de pago
/// </summary>
public class AddPaymentMethodRequest
{
    /// <summary>
    /// Tipo de método: "card" o "bank_account"
    /// </summary>
    public string Type { get; set; } = "card";

    /// <summary>
    /// Pasarela a usar para tokenización (azul, pixelpay, paypal)
    /// Si no se especifica, se usa la predeterminada del sistema
    /// </summary>
    public string? Gateway { get; set; }

    /// <summary>
    /// Token pre-generado (si viene de Stripe.js, PayPal checkout, etc.)
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Datos de la tarjeta (si no se proporciona token)
    /// </summary>
    public CardDataRequest? Card { get; set; }

    /// <summary>
    /// Datos de cuenta bancaria (para ACH)
    /// </summary>
    public BankAccountDataRequest? BankAccount { get; set; }

    /// <summary>
    /// Nombre personalizado para el método de pago
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// Si debe ser el método de pago por defecto
    /// </summary>
    public bool SetAsDefault { get; set; }

    /// <summary>
    /// Dirección de facturación
    /// </summary>
    public BillingAddressRequest? BillingAddress { get; set; }
}

/// <summary>
/// Datos de tarjeta para tokenización
/// </summary>
public class CardDataRequest
{
    /// <summary>
    /// Número de tarjeta (sin espacios)
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Mes de expiración (1-12)
    /// </summary>
    public int ExpMonth { get; set; }

    /// <summary>
    /// Año de expiración (2 o 4 dígitos)
    /// </summary>
    public int ExpYear { get; set; }

    /// <summary>
    /// Código de seguridad (CVV/CVC)
    /// </summary>
    public string Cvv { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del titular
    /// </summary>
    public string? CardHolderName { get; set; }
}

/// <summary>
/// Datos de cuenta bancaria para ACH
/// </summary>
public class BankAccountDataRequest
{
    /// <summary>
    /// Número de ruta bancaria (routing number)
    /// </summary>
    public string RoutingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Número de cuenta
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cuenta: "checking" o "savings"
    /// </summary>
    public string AccountType { get; set; } = "checking";

    /// <summary>
    /// Nombre del titular de la cuenta
    /// </summary>
    public string AccountHolderName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string? BankName { get; set; }
}

/// <summary>
/// Dirección de facturación
/// </summary>
public class BillingAddressRequest
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "DO"; // República Dominicana por defecto
}

/// <summary>
/// Solicitud para actualizar un método de pago
/// </summary>
public class UpdatePaymentMethodRequest
{
    /// <summary>
    /// Nuevo nombre personalizado
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// Nueva dirección de facturación
    /// </summary>
    public BillingAddressRequest? BillingAddress { get; set; }
}

// ============================================================================
// RESPONSE DTOs
// ============================================================================

/// <summary>
/// Información de un método de pago guardado
/// </summary>
public class PaymentMethodDto
{
    /// <summary>
    /// ID único del método de pago
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de método de pago: "card" o "bank_account"
    /// </summary>
    public string Type { get; set; } = "card";

    /// <summary>
    /// Pasarela de pago asociada
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es el método por defecto
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Nombre personalizado del método
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// Información de la tarjeta (si es tipo card)
    /// </summary>
    public CardInfoDto? Card { get; set; }

    /// <summary>
    /// Información de cuenta bancaria (si es tipo bank_account)
    /// </summary>
    public BankAccountInfoDto? BankAccount { get; set; }

    /// <summary>
    /// Dirección de facturación
    /// </summary>
    public BillingAddressDto? BillingAddress { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última fecha de uso
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Número de veces usado
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Indica si la tarjeta está expirada
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Indica si la tarjeta expira pronto (próximos 2 meses)
    /// </summary>
    public bool ExpiresSoon { get; set; }
}

/// <summary>
/// Información enmascarada de la tarjeta
/// </summary>
public class CardInfoDto
{
    /// <summary>
    /// Marca de la tarjeta (Visa, Mastercard, Amex, etc.)
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Últimos 4 dígitos
    /// </summary>
    public string Last4 { get; set; } = string.Empty;

    /// <summary>
    /// Mes de expiración
    /// </summary>
    public int ExpMonth { get; set; }

    /// <summary>
    /// Año de expiración
    /// </summary>
    public int ExpYear { get; set; }

    /// <summary>
    /// Nombre del titular (si está disponible)
    /// </summary>
    public string? CardHolderName { get; set; }

    /// <summary>
    /// País del banco emisor
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Nombre del banco emisor
    /// </summary>
    public string? Bank { get; set; }

    /// <summary>
    /// Fecha de expiración formateada (MM/YYYY)
    /// </summary>
    public string Expiry => $"{ExpMonth:D2}/{ExpYear}";

    /// <summary>
    /// Número enmascarado (•••• •••• •••• 1234)
    /// </summary>
    public string MaskedNumber => $"•••• •••• •••• {Last4}";
}

/// <summary>
/// Información enmascarada de cuenta bancaria
/// </summary>
public class BankAccountInfoDto
{
    /// <summary>
    /// Nombre del banco
    /// </summary>
    public string BankName { get; set; } = string.Empty;

    /// <summary>
    /// Últimos 4 dígitos de la cuenta
    /// </summary>
    public string Last4 { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de cuenta (checking, savings)
    /// </summary>
    public string AccountType { get; set; } = string.Empty;
}

/// <summary>
/// Dirección de facturación
/// </summary>
public class BillingAddressDto
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// Lista de métodos de pago
/// </summary>
public class PaymentMethodsListDto
{
    /// <summary>
    /// Lista de métodos de pago
    /// </summary>
    public List<PaymentMethodDto> Methods { get; set; } = new();

    /// <summary>
    /// Método de pago por defecto (ID)
    /// </summary>
    public string? DefaultMethodId { get; set; }

    /// <summary>
    /// Total de métodos de pago
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Métodos expirados que requieren actualización
    /// </summary>
    public int ExpiredCount { get; set; }

    /// <summary>
    /// Métodos que expiran pronto
    /// </summary>
    public int ExpiringSoonCount { get; set; }
}

// ============================================================================
// MAPPING EXTENSIONS
// ============================================================================

public static class PaymentMethodMappingExtensions
{
    /// <summary>
    /// Convierte SavedPaymentMethod a PaymentMethodDto
    /// </summary>
    public static PaymentMethodDto ToDto(this SavedPaymentMethod entity)
    {
        var dto = new PaymentMethodDto
        {
            Id = entity.Id.ToString(),
            Type = entity.Type switch
            {
                SavedPaymentMethodType.Card => "card",
                SavedPaymentMethodType.BankAccount => "bank_account",
                SavedPaymentMethodType.PayPal => "paypal",
                _ => "other"
            },
            Gateway = entity.PaymentGateway.ToString().ToLower(),
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            NickName = entity.NickName,
            CreatedAt = entity.CreatedAt,
            LastUsedAt = entity.LastUsedAt,
            UsageCount = entity.UsageCount,
            IsExpired = entity.IsExpired(),
            ExpiresSoon = entity.ExpiresWithinMonths(2)
        };

        if (entity.Type == SavedPaymentMethodType.Card)
        {
            dto.Card = new CardInfoDto
            {
                Brand = entity.CardBrand,
                Last4 = entity.CardLast4,
                ExpMonth = entity.ExpirationMonth,
                ExpYear = entity.ExpirationYear,
                CardHolderName = entity.CardHolderName,
                Country = entity.BankCountry,
                Bank = entity.BankName
            };
        }
        else if (entity.Type == SavedPaymentMethodType.BankAccount)
        {
            dto.BankAccount = new BankAccountInfoDto
            {
                BankName = entity.AccountBankName ?? "Banco",
                Last4 = entity.AccountLast4 ?? "",
                AccountType = entity.AccountType ?? "checking"
            };
        }

        if (!string.IsNullOrEmpty(entity.BillingAddressJson))
        {
            try
            {
                dto.BillingAddress = System.Text.Json.JsonSerializer.Deserialize<BillingAddressDto>(entity.BillingAddressJson);
            }
            catch
            {
                // Ignore JSON parse errors
            }
        }

        return dto;
    }

    /// <summary>
    /// Convierte una lista de SavedPaymentMethod a PaymentMethodsListDto
    /// </summary>
    public static PaymentMethodsListDto ToListDto(this IEnumerable<SavedPaymentMethod> entities)
    {
        var list = entities.ToList();
        var dtos = list.Select(e => e.ToDto()).ToList();
        var defaultMethod = list.FirstOrDefault(e => e.IsDefault);

        return new PaymentMethodsListDto
        {
            Methods = dtos,
            DefaultMethodId = defaultMethod?.Id.ToString(),
            Total = dtos.Count,
            ExpiredCount = list.Count(e => e.IsExpired()),
            ExpiringSoonCount = list.Count(e => e.ExpiresWithinMonths(2) && !e.IsExpired())
        };
    }
}
