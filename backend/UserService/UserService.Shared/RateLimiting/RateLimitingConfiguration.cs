namespace UserService.Shared.RateLimiting
{
    /// <summary>
    /// Configuración global de Rate Limiting
    /// </summary>
    public class RateLimitingConfiguration
    {
        /// <summary>
        /// Número máximo de requests permitidos en la ventana de tiempo
        /// </summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>
        /// Duración de la ventana de tiempo en segundos
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Indica si el rate limiting está habilitado
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Indica si se permite registrar los eventos de rate limiting excedido
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Lista de IPs que están exentas del rate limiting
        /// </summary>
        public List<string> WhitelistedIps { get; set; } = new List<string> { "127.0.0.1", "::1" };
    }

    /// <summary>
    /// Configuración de Rate Limiting por endpoint
    /// </summary>
    public class EndpointRateLimitPolicy
    {
        /// <summary>
        /// Nombre del endpoint (ej: "POST:/api/errors")
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Número máximo de requests para este endpoint
        /// </summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>
        /// Duración de la ventana de tiempo en segundos
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Indica si está habilitado para este endpoint
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Configuración de Rate Limiting por cliente
    /// </summary>
    public class ClientRateLimitPolicy
    {
        /// <summary>
        /// Identificador del cliente (puede ser API key, user ID, etc)
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Número máximo de requests para este cliente
        /// </summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>
        /// Duración de la ventana de tiempo en segundos
        /// </summary>
        public int WindowSeconds { get; set; } = 60;
    }
}
