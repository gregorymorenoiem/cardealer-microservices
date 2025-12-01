# POLÍTICA 02: CONFIGURACIÓN Y GESTIÓN DE ENTORNOS

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben soportar múltiples entornos (Development, Staging, Production) con configuración segregada y segura.

**Objetivo**: Garantizar configuración consistente, segura y fácilmente configurable en todos los ambientes, sin hardcodear valores sensibles.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 JERARQUÍA DE CONFIGURACIÓN OBLIGATORIA

### Estructura de Archivos appsettings

**REGLA**: Cada microservicio debe tener mínimo 3 archivos de configuración:

```
{ServiceName}.Api/
├── appsettings.json                    # ✅ Configuración BASE (común a todos los ambientes)
├── appsettings.Development.json        # ✅ Configuración DESARROLLO (override)
├── appsettings.Staging.json            # ⚠️ Configuración STAGING (opcional)
└── appsettings.Production.json         # ✅ Configuración PRODUCCIÓN (override)
```

### Orden de Precedencia (Mayor a Menor)

```
1. Variables de Entorno (ASPNETCORE_*, ConnectionStrings__)
2. User Secrets (solo Development)
3. appsettings.{Environment}.json
4. appsettings.json
```

---

## 📄 ESTRUCTURA OBLIGATORIA DE appsettings.json

### appsettings.json (BASE)

**REGLA**: Solo valores comunes y NO SENSIBLES. Valores por defecto seguros.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  
  "AllowedHosts": "*",
  
  "Jwt": {
    "Issuer": "cardealer-auth",
    "Audience": "cardealer-services",
    "ExpirationMinutes": 60
  },
  
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionStrings": {
      "PostgreSQL": "Host=localhost;Port=25432;Database={servicename};Username=postgres;Password=PLACEHOLDER",
      "SqlServer": "Server=localhost;Database={servicename};User Id=sa;Password=PLACEHOLDER",
      "MySQL": "Server=localhost;Port=3306;Database={servicename};Uid=root;Pwd=PLACEHOLDER"
    },
    "CommandTimeout": 30,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3
  },
  
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "PLACEHOLDER",
    "VirtualHost": "/",
    "Exchange": "cardealer.events",
    "QueuePrefix": "{servicename}",
    "DeadLetterExchange": "cardealer.dlx",
    "RetryCount": 3,
    "RetryDelaySeconds": 5
  },
  
  "RateLimiting": {
    "EnableRateLimiting": true,
    "DefaultMaxRequests": 100,
    "DefaultWindowSeconds": 60,
    "WhitelistedIPs": [],
    "BypassTokens": []
  },
  
  "CircuitBreaker": {
    "FailureRatio": 0.5,
    "SamplingDurationSeconds": 30,
    "MinimumThroughput": 3,
    "BreakDurationSeconds": 30
  },
  
  "OpenTelemetry": {
    "ServiceName": "{ServiceName}",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://localhost:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "SamplingRatio": 1.0
  },
  
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": false,
    "TimeoutSeconds": 10
  }
}
```

---

### appsettings.Development.json (DESARROLLO)

**REGLA**: Valores para desarrollo local, logging detallado, conexiones locales.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  
  "Database": {
    "ConnectionStrings": {
      "PostgreSQL": "Host=localhost;Port=25432;Database=errorservice;Username=postgres;Password=password"
    },
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  
  "Jwt": {
    "Key": "cardealer-development-secret-key-min-32-chars-long-for-jwt-signing-hmac-sha256!"
  },
  
  "RateLimiting": {
    "DefaultMaxRequests": 1000,
    "DefaultWindowSeconds": 60,
    "WhitelistedIPs": ["127.0.0.1", "::1"]
  },
  
  "OpenTelemetry": {
    "SamplingRatio": 1.0,
    "OtlpEndpoint": "http://localhost:4317"
  },
  
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:4200",
      "https://localhost:5001"
    ]
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": true
  }
}
```

---

### appsettings.Production.json (PRODUCCIÓN)

**REGLA**: Configuración mínima, valores sensibles desde variables de entorno o Azure Key Vault.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  
  "Database": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 60,
    "MaxRetryCount": 5
  },
  
  "RateLimiting": {
    "DefaultMaxRequests": 50,
    "DefaultWindowSeconds": 60,
    "WhitelistedIPs": []
  },
  
  "OpenTelemetry": {
    "SamplingRatio": 0.1,
    "OtlpEndpoint": "https://otel-collector.cardealer.com:4317"
  },
  
  "Cors": {
    "AllowedOrigins": [
      "https://cardealer.com",
      "https://app.cardealer.com"
    ]
  },
  
  "HealthChecks": {
    "EnableDetailedErrors": false,
    "TimeoutSeconds": 5
  },
  
  "CircuitBreaker": {
    "FailureRatio": 0.3,
    "BreakDurationSeconds": 60
  }
}
```

---

## 🔒 GESTIÓN DE SECRETOS

### ❌ PROHIBIDO: Hardcodear Secretos

```json
// ❌ NUNCA HACER ESTO
{
  "Database": {
    "ConnectionStrings": {
      "PostgreSQL": "Host=prod-db.aws.com;Password=MySecretPassword123!"
    }
  },
  "Jwt": {
    "Key": "super-secret-production-key-12345"
  }
}
```

### ✅ CORRECTO: Variables de Entorno

**Desarrollo (User Secrets)**:
```bash
# Inicializar User Secrets
dotnet user-secrets init --project ErrorService.Api

# Agregar secretos
dotnet user-secrets set "Jwt:Key" "dev-secret-key-32-chars-minimum!" --project ErrorService.Api
dotnet user-secrets set "Database:ConnectionStrings:PostgreSQL" "Host=localhost;Port=25432;Database=errorservice;Username=postgres;Password=password" --project ErrorService.Api
dotnet user-secrets set "RabbitMQ:Password" "guest" --project ErrorService.Api
```

**Producción (Variables de Entorno)**:
```bash
# Azure App Service - Application Settings
export ConnectionStrings__PostgreSQL="Host=prod-db.postgres.database.azure.com;Port=5432;Database=errorservice;Username=admin@prodserver;Password=$DB_PASSWORD"
export Jwt__Key="$JWT_SECRET_FROM_KEYVAULT"
export RabbitMQ__Password="$RABBITMQ_PASSWORD"

# Docker
docker run -e "ConnectionStrings__PostgreSQL=Host=..." \
           -e "Jwt__Key=..." \
           errorservice:latest

# Kubernetes Secret
apiVersion: v1
kind: Secret
metadata:
  name: errorservice-secrets
type: Opaque
stringData:
  JWT_KEY: "production-jwt-secret-key-from-vault"
  DB_PASSWORD: "production-db-password"
```

**Azure Key Vault (Recomendado para Producción)**:
```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Endpoint"]!);
    builder.Configuration.AddAzureKeyVault(
        keyVaultEndpoint,
        new DefaultAzureCredential());
}
```

---

## 🌍 CONFIGURACIÓN POR AMBIENTE

### Development (Local)

**Características**:
- ✅ Logging detallado (Debug/Information)
- ✅ Sensitive data logging habilitado
- ✅ Detailed errors habilitado
- ✅ Rate limiting relajado (1000 req/60s)
- ✅ OpenTelemetry sampling 100%
- ✅ CORS permisivo (localhost)
- ✅ Health checks con detalles completos

**Configuración de Ambiente**:
```bash
# PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# launchSettings.json
{
  "profiles": {
    "Development": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

---

### Staging (Pre-Producción)

**Características**:
- ⚠️ Logging moderado (Information/Warning)
- ❌ Sensitive data logging deshabilitado
- ⚠️ Rate limiting intermedio (200 req/60s)
- ⚠️ OpenTelemetry sampling 50%
- ⚠️ CORS restrictivo (dominios staging)
- ⚠️ Health checks sin detalles sensibles

**appsettings.Staging.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "Database": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": true
  },
  
  "RateLimiting": {
    "DefaultMaxRequests": 200,
    "DefaultWindowSeconds": 60
  },
  
  "OpenTelemetry": {
    "SamplingRatio": 0.5,
    "OtlpEndpoint": "https://otel-staging.cardealer.com:4317"
  },
  
  "Cors": {
    "AllowedOrigins": [
      "https://staging.cardealer.com",
      "https://app-staging.cardealer.com"
    ]
  }
}
```

---

### Production (Producción)

**Características**:
- 🔴 Logging mínimo (Warning/Error)
- ❌ Sensitive data logging DESHABILITADO
- ❌ Detailed errors DESHABILITADO
- 🔴 Rate limiting estricto (50 req/60s)
- 🔴 OpenTelemetry sampling 10%
- 🔴 CORS restrictivo (solo dominios oficiales)
- ❌ Health checks sin detalles

**Configuración**:
```bash
# Azure App Service
az webapp config appsettings set \
  --name errorservice-prod \
  --resource-group cardealer-rg \
  --settings ASPNETCORE_ENVIRONMENT=Production \
             ConnectionStrings__PostgreSQL="@Microsoft.KeyVault(SecretUri=https://cardealer-kv.vault.azure.net/secrets/db-connection)" \
             Jwt__Key="@Microsoft.KeyVault(SecretUri=https://cardealer-kv.vault.azure.net/secrets/jwt-key)"
```

---

## 🔧 CONFIGURACIÓN DE SERVICIOS EN Program.cs

### Multi-Provider Database Configuration

```csharp
// Program.cs
var databaseProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";

switch (databaseProvider.ToUpperInvariant())
{
    case "POSTGRESQL":
        var postgresConnection = builder.Configuration["Database:ConnectionStrings:PostgreSQL"];
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(postgresConnection, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: builder.Configuration.GetValue<int>("Database:MaxRetryCount"),
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(builder.Configuration.GetValue<int>("Database:CommandTimeout"));
            });
            
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });
        break;
    
    case "SQLSERVER":
        var sqlServerConnection = builder.Configuration["Database:ConnectionStrings:SqlServer"];
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(sqlServerConnection, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: builder.Configuration.GetValue<int>("Database:MaxRetryCount"));
            });
        });
        break;
    
    case "MYSQL":
        var mysqlConnection = builder.Configuration["Database:ConnectionStrings:MySQL"];
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(mysqlConnection, ServerVersion.AutoDetect(mysqlConnection));
        });
        break;
    
    default:
        throw new InvalidOperationException($"Unsupported database provider: {databaseProvider}");
}
```

---

## 📊 CONFIGURACIÓN DE LOGGING POR AMBIENTE

### Serilog Configuration

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: 
        builder.Environment.IsDevelopment() 
            ? "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j} TraceId={TraceId}{NewLine}{Exception}"
            : "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} TraceId={TraceId}{NewLine}{Exception}")
    .WriteTo.File(
        path: $"logs/{serviceName}-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: builder.Environment.IsProduction() ? 30 : 7,
        fileSizeLimitBytes: 100_000_000,
        rollOnFileSizeLimit: true)
    .CreateLogger();
```

---

## 🔍 VALIDACIÓN DE CONFIGURACIÓN

### Startup Configuration Validator

```csharp
// Extensions/ConfigurationValidator.cs
public static class ConfigurationValidator
{
    public static void ValidateConfiguration(this WebApplicationBuilder builder)
    {
        var errors = new List<string>();
        
        // Validar JWT
        if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]))
            errors.Add("Jwt:Key is not configured");
        
        if (builder.Configuration["Jwt:Key"]?.Length < 32)
            errors.Add("Jwt:Key must be at least 32 characters");
        
        // Validar Database
        var provider = builder.Configuration["Database:Provider"];
        var connectionString = builder.Configuration[$"Database:ConnectionStrings:{provider}"];
        
        if (string.IsNullOrEmpty(connectionString))
            errors.Add($"Database connection string for provider '{provider}' is not configured");
        
        if (connectionString?.Contains("PLACEHOLDER") == true)
            errors.Add("Database connection string contains PLACEHOLDER - update with real values");
        
        // Validar RabbitMQ
        if (string.IsNullOrEmpty(builder.Configuration["RabbitMQ:Host"]))
            errors.Add("RabbitMQ:Host is not configured");
        
        if (builder.Configuration["RabbitMQ:Password"] == "PLACEHOLDER")
            errors.Add("RabbitMQ password is PLACEHOLDER - update with real password");
        
        // Validar OpenTelemetry en producción
        if (builder.Environment.IsProduction())
        {
            var samplingRatio = builder.Configuration.GetValue<double>("OpenTelemetry:SamplingRatio");
            if (samplingRatio > 0.2)
                errors.Add("OpenTelemetry:SamplingRatio should be <= 0.2 in production (performance)");
        }
        
        if (errors.Any())
        {
            var errorMessage = "Configuration validation failed:\n" + string.Join("\n", errors);
            throw new InvalidOperationException(errorMessage);
        }
        
        Log.Information("✅ Configuration validation passed");
    }
}

// Program.cs
builder.ValidateConfiguration();
```

---

## 🎯 MEJORES PRÁCTICAS

### ✅ DO's (Hacer)

1. **Usar User Secrets en Development**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "development-key"
   ```

2. **Usar Variables de Entorno en Producción**
   ```bash
   export Jwt__Key="$PRODUCTION_JWT_KEY"
   export ConnectionStrings__PostgreSQL="$DB_CONNECTION"
   ```

3. **Validar configuración al startup**
   ```csharp
   builder.ValidateConfiguration();
   ```

4. **Usar Configuration Binding**
   ```csharp
   builder.Services.Configure<JwtSettings>(
       builder.Configuration.GetSection("Jwt"));
   ```

5. **Documentar configuración requerida en README**
   ```markdown
   ## Required Configuration
   
   ### Development
   - User Secrets: `Jwt:Key`, `Database:ConnectionStrings:PostgreSQL`
   
   ### Production
   - Environment Variables: `Jwt__Key`, `ConnectionStrings__PostgreSQL`
   ```

---

### ❌ DON'Ts (No Hacer)

1. **NO hardcodear secretos**
   ```csharp
   // ❌ PROHIBIDO
   var jwtKey = "my-hardcoded-secret-key";
   ```

2. **NO commitear secretos a Git**
   ```
   # ❌ PROHIBIDO - NO commitear
   appsettings.Production.json con passwords
   ```

3. **NO usar misma configuración en todos los ambientes**
   ```json
   // ❌ PROHIBIDO - Sampling 100% en producción
   "OpenTelemetry": { "SamplingRatio": 1.0 }
   ```

4. **NO exponer detalles de errores en producción**
   ```json
   // ❌ PROHIBIDO en producción
   "Database": { "EnableDetailedErrors": true }
   ```

---

## 📋 CHECKLIST DE CUMPLIMIENTO

- [ ] appsettings.json (base) creado con valores por defecto
- [ ] appsettings.Development.json con configuración local
- [ ] appsettings.Production.json con configuración segura
- [ ] User Secrets configurado para desarrollo
- [ ] Variables de entorno documentadas en README
- [ ] Validación de configuración implementada
- [ ] Sin secretos hardcodeados en código
- [ ] Sin secretos commiteados a Git
- [ ] .gitignore incluye appsettings.*.json (excepto Development)
- [ ] Logging configurado por ambiente
- [ ] Rate limiting ajustado por ambiente
- [ ] OpenTelemetry sampling optimizado (10% prod, 100% dev)
- [ ] CORS configurado restrictivamente en producción
- [ ] Database provider multi-soporte configurado
- [ ] Circuit Breaker configurado con valores apropiados

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService.Api/appsettings.json`
- **User Secrets**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- **Azure Key Vault**: [Azure Key Vault Configuration Provider](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- **Configuration**: [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Esta política es OBLIGATORIA. Secretos en código o configuración insegura bloquean deployment a producción.
