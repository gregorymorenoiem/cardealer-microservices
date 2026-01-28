using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Interfaces;

/// <summary>
/// Registra y gestiona los proveedores de pago disponibles
/// Implementa el patrón Registry para administración centralizada
/// </summary>
public interface IPaymentGatewayRegistry
{
    /// <summary>
    /// Registra un nuevo proveedor de pago
    /// </summary>
    /// <param name="provider">Instancia del proveedor a registrar</param>
    void Register(IPaymentGatewayProvider provider);

    /// <summary>
    /// Des-registra un proveedor de pago
    /// </summary>
    /// <param name="gateway">Identificador de la pasarela a remover</param>
    /// <returns>True si se removió exitosamente</returns>
    bool Unregister(PaymentGateway gateway);

    /// <summary>
    /// Obtiene un proveedor registrado
    /// </summary>
    /// <param name="gateway">Identificador de la pasarela</param>
    /// <returns>Instancia del proveedor o null</returns>
    IPaymentGatewayProvider? Get(PaymentGateway gateway);

    /// <summary>
    /// Obtiene todos los proveedores registrados
    /// </summary>
    /// <returns>Colección de proveedores</returns>
    IReadOnlyCollection<IPaymentGatewayProvider> GetAll();

    /// <summary>
    /// Verifica si un proveedor está registrado
    /// </summary>
    /// <param name="gateway">Identificador de la pasarela</param>
    /// <returns>True si está registrado</returns>
    bool Contains(PaymentGateway gateway);

    /// <summary>
    /// Cuenta el número de proveedores registrados
    /// </summary>
    /// <returns>Número de proveedores</returns>
    int Count();

    /// <summary>
    /// Limpia todos los proveedores registrados
    /// </summary>
    void Clear();
}
