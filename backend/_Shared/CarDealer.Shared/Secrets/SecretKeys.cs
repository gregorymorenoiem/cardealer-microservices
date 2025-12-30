namespace CarDealer.Shared.Secrets;

/// <summary>
/// Nombres estándar de secretos usados en CarDealer Microservices.
/// Centraliza las claves para evitar errores de tipeo.
/// </summary>
public static class SecretKeys
{
    // ===== DATABASE =====
    /// <summary>Connection string principal de la base de datos</summary>
    public const string DatabaseConnectionString = "DATABASE_CONNECTION_STRING";
    
    /// <summary>Host de la base de datos PostgreSQL</summary>
    public const string DatabaseHost = "DATABASE_HOST";
    
    /// <summary>Puerto de la base de datos</summary>
    public const string DatabasePort = "DATABASE_PORT";
    
    /// <summary>Nombre de la base de datos</summary>
    public const string DatabaseName = "DATABASE_NAME";
    
    /// <summary>Usuario de la base de datos</summary>
    public const string DatabaseUser = "DATABASE_USER";
    
    /// <summary>Contraseña de la base de datos</summary>
    public const string DatabasePassword = "DATABASE_PASSWORD";

    // ===== JWT / AUTH =====
    /// <summary>Clave secreta para firmar JWT tokens</summary>
    public const string JwtSecretKey = "JWT_SECRET_KEY";
    
    /// <summary>Issuer del JWT token</summary>
    public const string JwtIssuer = "JWT_ISSUER";
    
    /// <summary>Audience del JWT token</summary>
    public const string JwtAudience = "JWT_AUDIENCE";
    
    /// <summary>Minutos de expiración del token de acceso</summary>
    public const string JwtExpiresMinutes = "JWT_EXPIRES_MINUTES";
    
    /// <summary>Días de expiración del refresh token</summary>
    public const string JwtRefreshTokenExpiresDays = "JWT_REFRESH_TOKEN_EXPIRES_DAYS";

    // ===== REDIS =====
    /// <summary>Connection string de Redis</summary>
    public const string RedisConnectionString = "REDIS_CONNECTION_STRING";
    
    /// <summary>Host de Redis</summary>
    public const string RedisHost = "REDIS_HOST";
    
    /// <summary>Puerto de Redis</summary>
    public const string RedisPort = "REDIS_PORT";
    
    /// <summary>Contraseña de Redis (opcional)</summary>
    public const string RedisPassword = "REDIS_PASSWORD";

    // ===== RABBITMQ =====
    /// <summary>Connection string de RabbitMQ</summary>
    public const string RabbitMqConnectionString = "RABBITMQ_CONNECTION_STRING";
    
    /// <summary>Host de RabbitMQ</summary>
    public const string RabbitMqHost = "RABBITMQ_HOST";
    
    /// <summary>Puerto de RabbitMQ</summary>
    public const string RabbitMqPort = "RABBITMQ_PORT";
    
    /// <summary>Usuario de RabbitMQ</summary>
    public const string RabbitMqUser = "RABBITMQ_USER";
    
    /// <summary>Contraseña de RabbitMQ</summary>
    public const string RabbitMqPassword = "RABBITMQ_PASSWORD";
    
    /// <summary>Virtual host de RabbitMQ</summary>
    public const string RabbitMqVirtualHost = "RABBITMQ_VIRTUAL_HOST";

    // ===== EMAIL (SendGrid) =====
    /// <summary>API Key de SendGrid</summary>
    public const string SendGridApiKey = "SENDGRID_API_KEY";
    
    /// <summary>Email del remitente</summary>
    public const string SendGridFromEmail = "SENDGRID_FROM_EMAIL";
    
    /// <summary>Nombre del remitente</summary>
    public const string SendGridFromName = "SENDGRID_FROM_NAME";

    // ===== SMS (Twilio) =====
    /// <summary>Account SID de Twilio</summary>
    public const string TwilioAccountSid = "TWILIO_ACCOUNT_SID";
    
    /// <summary>Auth Token de Twilio</summary>
    public const string TwilioAuthToken = "TWILIO_AUTH_TOKEN";
    
    /// <summary>Número de teléfono de Twilio</summary>
    public const string TwilioFromNumber = "TWILIO_FROM_NUMBER";

    // ===== PUSH NOTIFICATIONS (Firebase) =====
    /// <summary>Project ID de Firebase</summary>
    public const string FirebaseProjectId = "FIREBASE_PROJECT_ID";
    
    /// <summary>Private Key de Firebase (base64 encoded)</summary>
    public const string FirebasePrivateKey = "FIREBASE_PRIVATE_KEY";
    
    /// <summary>Client Email de Firebase</summary>
    public const string FirebaseClientEmail = "FIREBASE_CLIENT_EMAIL";
    
    /// <summary>Contenido completo del service account JSON (base64 encoded)</summary>
    public const string FirebaseServiceAccountJson = "FIREBASE_SERVICE_ACCOUNT_JSON";

    // ===== STORAGE (S3/Azure) =====
    /// <summary>Proveedor de storage (S3, Azure, Local)</summary>
    public const string StorageProvider = "STORAGE_PROVIDER";
    
    /// <summary>AWS Access Key ID</summary>
    public const string AwsAccessKeyId = "AWS_ACCESS_KEY_ID";
    
    /// <summary>AWS Secret Access Key</summary>
    public const string AwsSecretAccessKey = "AWS_SECRET_ACCESS_KEY";
    
    /// <summary>AWS Region</summary>
    public const string AwsRegion = "AWS_REGION";
    
    /// <summary>AWS S3 Bucket Name</summary>
    public const string AwsS3BucketName = "AWS_S3_BUCKET_NAME";
    
    /// <summary>Azure Storage Connection String</summary>
    public const string AzureStorageConnectionString = "AZURE_STORAGE_CONNECTION_STRING";
    
    /// <summary>Azure Blob Container Name</summary>
    public const string AzureBlobContainerName = "AZURE_BLOB_CONTAINER_NAME";

    // ===== ELASTICSEARCH =====
    /// <summary>URL de Elasticsearch</summary>
    public const string ElasticsearchUrl = "ELASTICSEARCH_URL";
    
    /// <summary>Usuario de Elasticsearch</summary>
    public const string ElasticsearchUsername = "ELASTICSEARCH_USERNAME";
    
    /// <summary>Contraseña de Elasticsearch</summary>
    public const string ElasticsearchPassword = "ELASTICSEARCH_PASSWORD";
    
    /// <summary>Índice por defecto de Elasticsearch</summary>
    public const string ElasticsearchDefaultIndex = "ELASTICSEARCH_DEFAULT_INDEX";

    // ===== PAYMENT GATEWAYS =====
    /// <summary>Stripe Secret Key</summary>
    public const string StripeSecretKey = "STRIPE_SECRET_KEY";
    
    /// <summary>Stripe Publishable Key</summary>
    public const string StripePublishableKey = "STRIPE_PUBLISHABLE_KEY";
    
    /// <summary>Stripe Webhook Secret</summary>
    public const string StripeWebhookSecret = "STRIPE_WEBHOOK_SECRET";
    
    /// <summary>PayPal Client ID</summary>
    public const string PaypalClientId = "PAYPAL_CLIENT_ID";
    
    /// <summary>PayPal Client Secret</summary>
    public const string PaypalClientSecret = "PAYPAL_CLIENT_SECRET";

    // ===== CONSUL / SERVICE DISCOVERY =====
    /// <summary>Dirección de Consul</summary>
    public const string ConsulAddress = "CONSUL_ADDRESS";
    
    /// <summary>Token de Consul (si está habilitado ACL)</summary>
    public const string ConsulToken = "CONSUL_TOKEN";

    // ===== OBSERVABILITY =====
    /// <summary>Endpoint de OTLP (OpenTelemetry)</summary>
    public const string OtlpEndpoint = "OTLP_ENDPOINT";
    
    /// <summary>Endpoint de Jaeger</summary>
    public const string JaegerEndpoint = "JAEGER_ENDPOINT";

    // ===== APPLICATION =====
    /// <summary>Entorno de la aplicación (Development, Staging, Production)</summary>
    public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
    
    /// <summary>URLs de la aplicación</summary>
    public const string AspNetCoreUrls = "ASPNETCORE_URLS";

    /// <summary>
    /// Obtiene los secretos requeridos para un servicio dado.
    /// </summary>
    public static string[] GetRequiredSecretsForService(string serviceName)
    {
        return serviceName.ToLowerInvariant() switch
        {
            "authservice" => new[]
            {
                DatabaseConnectionString,
                JwtSecretKey
            },
            "notificationservice" => new[]
            {
                DatabaseConnectionString,
                RabbitMqHost
                // SendGrid, Twilio, Firebase son opcionales con graceful degradation
            },
            "errorservice" => new[]
            {
                DatabaseConnectionString
            },
            "gateway" => Array.Empty<string>(), // Gateway no requiere secretos
            _ => new[]
            {
                DatabaseConnectionString
            }
        };
    }
}
