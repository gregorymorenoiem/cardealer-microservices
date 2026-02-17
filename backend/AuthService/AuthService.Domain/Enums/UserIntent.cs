namespace AuthService.Domain.Enums;

/// <summary>
/// Representa la intención del usuario en la plataforma OKLA.
/// A diferencia de AccountType (que define el tipo de cuenta),
/// UserIntent define QUÉ quiere hacer el usuario y puede cambiar.
/// </summary>
public enum UserIntent
{
    /// <summary>
    /// Solo navega la plataforma, aún no ha definido intención.
    /// Es el estado default al registrarse.
    /// </summary>
    Browse = 0,

    /// <summary>
    /// Quiere comprar un vehículo (gratis).
    /// </summary>
    Buy = 1,

    /// <summary>
    /// Quiere vender un vehículo ($29/listing).
    /// </summary>
    Sell = 2,

    /// <summary>
    /// Quiere comprar y vender (ambos).
    /// </summary>
    BuyAndSell = 3
}

/// <summary>
/// Extensiones para el enum UserIntent.
/// </summary>
public static class UserIntentExtensions
{
    /// <summary>
    /// Indica si el usuario tiene intención de compra.
    /// </summary>
    public static bool WantsToBuy(this UserIntent intent) =>
        intent == UserIntent.Buy || intent == UserIntent.BuyAndSell;

    /// <summary>
    /// Indica si el usuario tiene intención de venta.
    /// </summary>
    public static bool WantsToSell(this UserIntent intent) =>
        intent == UserIntent.Sell || intent == UserIntent.BuyAndSell;

    /// <summary>
    /// Obtiene el nombre descriptivo en español.
    /// </summary>
    public static string GetDisplayName(this UserIntent intent) => intent switch
    {
        UserIntent.Browse => "Navegando",
        UserIntent.Buy => "Comprador",
        UserIntent.Sell => "Vendedor",
        UserIntent.BuyAndSell => "Comprador y Vendedor",
        _ => "Desconocido"
    };

    /// <summary>
    /// Convierte a string para JWT/frontend.
    /// </summary>
    public static string ToClaimValue(this UserIntent intent) => intent switch
    {
        UserIntent.Browse => "browse",
        UserIntent.Buy => "buy",
        UserIntent.Sell => "sell",
        UserIntent.BuyAndSell => "buy_and_sell",
        _ => "browse"
    };

    /// <summary>
    /// Parsea desde string (JWT/frontend).
    /// </summary>
    public static UserIntent FromClaimValue(string? value) => value?.ToLowerInvariant() switch
    {
        "browse" => UserIntent.Browse,
        "buy" => UserIntent.Buy,
        "sell" => UserIntent.Sell,
        "buy_and_sell" => UserIntent.BuyAndSell,
        _ => UserIntent.Browse
    };
}
