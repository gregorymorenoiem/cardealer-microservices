namespace CarDealer.Shared.Secrets;

/// <summary>
/// Interface para abstracción de obtención de secretos.
/// Permite cambiar entre diferentes fuentes de secretos (ENV, Docker Secrets, Vault, etc.)
/// sin modificar el código de la aplicación.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Obtiene un secreto por su nombre.
    /// </summary>
    /// <param name="key">Nombre del secreto (ej: "JWT_SECRET_KEY", "DB_PASSWORD")</param>
    /// <returns>Valor del secreto o null si no existe</returns>
    string? GetSecret(string key);

    /// <summary>
    /// Obtiene un secreto requerido. Lanza excepción si no existe.
    /// </summary>
    /// <param name="key">Nombre del secreto</param>
    /// <returns>Valor del secreto</returns>
    /// <exception cref="SecretNotFoundException">Si el secreto no existe</exception>
    string GetRequiredSecret(string key);

    /// <summary>
    /// Obtiene un secreto con valor por defecto si no existe.
    /// </summary>
    /// <param name="key">Nombre del secreto</param>
    /// <param name="defaultValue">Valor por defecto</param>
    /// <returns>Valor del secreto o el valor por defecto</returns>
    string GetSecretOrDefault(string key, string defaultValue);

    /// <summary>
    /// Verifica si un secreto existe.
    /// </summary>
    /// <param name="key">Nombre del secreto</param>
    /// <returns>True si el secreto existe</returns>
    bool HasSecret(string key);

    /// <summary>
    /// Obtiene múltiples secretos que comienzan con un prefijo.
    /// Útil para obtener todas las configuraciones de un servicio.
    /// </summary>
    /// <param name="prefix">Prefijo a buscar (ej: "SENDGRID_")</param>
    /// <returns>Diccionario con los secretos encontrados</returns>
    IDictionary<string, string> GetSecretsWithPrefix(string prefix);
}

/// <summary>
/// Excepción lanzada cuando un secreto requerido no se encuentra.
/// </summary>
public class SecretNotFoundException : Exception
{
    public string SecretKey { get; }

    public SecretNotFoundException(string key)
        : base($"Required secret '{key}' was not found. " +
               $"Ensure the secret is configured via environment variable, Docker secret, or secret provider.")
    {
        SecretKey = key;
    }

    public SecretNotFoundException(string key, string message)
        : base(message)
    {
        SecretKey = key;
    }
}
