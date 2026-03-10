namespace ReviewService.Application.Interfaces;

/// <summary>
/// Client para verificar si un comprador tuvo contacto real con un vendedor.
/// Llama a ContactService vía HTTP (endpoint interno) para validar
/// que el buyer hizo al menos 1 ContactRequest al seller antes de permitir una reseña.
/// Prevención de reseñas falsas — OKLA Trust Architecture.
/// </summary>
public interface IContactVerificationService
{
    /// <summary>
    /// Verifica si el comprador contactó al vendedor al menos una vez.
    /// </summary>
    /// <param name="buyerId">ID del comprador que quiere dejar la reseña</param>
    /// <param name="sellerId">ID del vendedor/dealer que recibirá la reseña</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>true si existe al menos un ContactRequest del buyer al seller</returns>
    Task<bool> HasBuyerContactedSellerAsync(Guid buyerId, Guid sellerId, CancellationToken ct = default);
}
