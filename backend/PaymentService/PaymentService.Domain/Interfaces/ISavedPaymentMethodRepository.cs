using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Repositorio para gestionar métodos de pago guardados
/// </summary>
public interface ISavedPaymentMethodRepository
{
    /// <summary>
    /// Obtiene un método de pago por su ID
    /// </summary>
    Task<SavedPaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un método de pago por su ID y usuario (verificación de propiedad)
    /// </summary>
    Task<SavedPaymentMethod?> GetByIdAndUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los métodos de pago de un usuario
    /// </summary>
    Task<List<SavedPaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene solo los métodos de pago activos de un usuario
    /// </summary>
    Task<List<SavedPaymentMethod>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el método de pago por defecto de un usuario
    /// </summary>
    Task<SavedPaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca un método de pago por su token en la pasarela
    /// </summary>
    Task<SavedPaymentMethod?> GetByTokenAsync(string token, PaymentGateway gateway, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un método de pago con los mismos últimos 4 dígitos
    /// para evitar duplicados
    /// </summary>
    Task<bool> ExistsSimilarAsync(Guid userId, string last4, string brand, int expMonth, int expYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta los métodos de pago de un usuario
    /// </summary>
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un nuevo método de pago guardado
    /// </summary>
    Task<SavedPaymentMethod> CreateAsync(SavedPaymentMethod paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un método de pago existente
    /// </summary>
    Task<SavedPaymentMethod> UpdateAsync(SavedPaymentMethod paymentMethod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Establece un método de pago como el predeterminado
    /// (desmarca los demás del usuario)
    /// </summary>
    Task SetAsDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desactiva un método de pago (soft delete)
    /// </summary>
    Task DeactivateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina permanentemente un método de pago
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un uso del método de pago
    /// </summary>
    Task RecordUsageAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métodos de pago que expiran pronto
    /// </summary>
    Task<List<SavedPaymentMethod>> GetExpiringSoonAsync(int withinMonths = 2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métodos de pago expirados
    /// </summary>
    Task<List<SavedPaymentMethod>> GetExpiredAsync(CancellationToken cancellationToken = default);
}
