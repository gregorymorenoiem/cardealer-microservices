# POLÍTICA 08: SEGURIDAD Y AUTENTICACIÓN

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los microservicios deben implementar JWT Authentication + Authorization Policies + Input Validation (SQL Injection, XSS, CSRF). Endpoints públicos sin autenticación deben ser EXPLÍCITAMENTE aprobados.

**Objetivo**: Proteger los microservicios contra ataques comunes (OWASP Top 10) y garantizar que solo usuarios autorizados accedan a recursos protegidos.

**Alcance**: Aplica a TODOS los microservicios del ecosistema CarDealer.

---

## 🎯 CAPAS DE SEGURIDAD OBLIGATORIAS

### Matriz de Seguridad

| Capa | Tecnología | Propósito | Obligatorio |
|------|------------|-----------|-------------|
| **1. Authentication** | JWT Bearer Tokens | Identificar usuario | ✅ SÍ |
| **2. Authorization** | ASP.NET Core Policies | Controlar acceso | ✅ SÍ |
| **3. Input Validation** | FluentValidation | SQL Injection, XSS | ✅ SÍ |
| **4. Rate Limiting** | ASP.NET Core Rate Limiter | DDoS protection | ✅ SÍ |
| **5. HTTPS** | TLS 1.2+ | Cifrado en tránsito | ✅ SÍ |
| **6. CORS** | Configuración restrictiva | Cross-origin control | ✅ SÍ |
| **7. Secrets Management** | Azure Key Vault / User Secrets | Proteger credenciales | ✅ SÍ |

---

## 🔐 CAPA 1: JWT AUTHENTICATION

### Configuración JWT (appsettings.json)

```json
{
  "Jwt": {
    "SecretKey": "NEVER-STORE-HERE-USE-SECRETS!",
    "Issuer": "cardealer-auth",
    "Audience": "cardealer-services",
    "ExpirationMinutes": 60,
    "ClockSkew": 5,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true
  }
}
```

**REGLA**: `SecretKey` NUNCA en appsettings.json. Usar User Secrets (dev) o Azure Key Vault (prod).

---

### Configuración JWT en Program.cs

```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT AUTHENTICATION
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    
    // IMPORTANTE: SecretKey desde User Secrets o Key Vault
    var secretKey = builder.Configuration["Jwt:SecretKey"];
    
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException(
            "JWT SecretKey not found. Configure User Secrets or Azure Key Vault.");
    }
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtSettings.GetValue<bool>("ValidateIssuer"),
        ValidateAudience = jwtSettings.GetValue<bool>("ValidateAudience"),
        ValidateLifetime = jwtSettings.GetValue<bool>("ValidateLifetime"),
        ValidateIssuerSigningKey = jwtSettings.GetValue<bool>("ValidateIssuerSigningKey"),
        
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)),
        
        ClockSkew = TimeSpan.FromMinutes(
            jwtSettings.GetValue<int>("ClockSkew"))
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            
            return Task.CompletedTask;
        },
        
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.Principal?.Identity?.Name;
            
            logger.LogInformation(
                "JWT token validated successfully for user {UserId} ({UserName})",
                userId,
                userName);
            
            return Task.CompletedTask;
        },
        
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();
            
            logger.LogWarning(
                "JWT authentication challenge. Path={Path}, Error={Error}",
                context.Request.Path,
                context.Error);
            
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// MIDDLEWARES EN ORDEN CORRECTO
app.UseHttpsRedirection();  // 1. HTTPS
app.UseAuthentication();     // 2. Authentication
app.UseAuthorization();      // 3. Authorization

app.Run();
```

---

### Generar JWT Token (AuthService)

```csharp
// JwtTokenGenerator.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Security
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        
        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GenerateToken(string userId, string userName, IEnumerable<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = _configuration["Jwt:SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey not configured");
            }
            
            // Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName)
            };
            
            // Agregar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                Expires = DateTime.UtcNow.AddMinutes(
                    jwtSettings.GetValue<int>("ExpirationMinutes")),
                SigningCredentials = credentials
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
        
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = _configuration["Jwt:SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
            {
                return null;
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
```

---

## 🛡️ CAPA 2: AUTHORIZATION POLICIES

### Definir Policies

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    // Policy 1: Requiere autenticación básica
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
    
    // Policy 2: Requiere rol específico
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    // Policy 3: Requiere claim específico
    options.AddPolicy("ErrorServiceAccess", policy =>
        policy.RequireClaim("service", "errorservice"));
    
    // Policy 4: Requiere múltiples claims
    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Manager") ||
            context.User.IsInRole("Admin")));
    
    // Policy 5: Custom requirement
    options.AddPolicy("MinimumAge18", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

// Registrar custom authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
```

---

### Custom Authorization Requirement

```csharp
// MinimumAgeRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace ErrorService.Api.Authorization
{
    public class MinimumAgeRequirement : IAuthorizationRequirement
    {
        public int MinimumAge { get; }
        
        public MinimumAgeRequirement(int minimumAge)
        {
            MinimumAge = minimumAge;
        }
    }
    
    public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MinimumAgeRequirement requirement)
        {
            var dateOfBirthClaim = context.User.FindFirst(
                c => c.Type == "date_of_birth");
            
            if (dateOfBirthClaim == null)
            {
                return Task.CompletedTask;
            }
            
            if (DateTime.TryParse(dateOfBirthClaim.Value, out var dateOfBirth))
            {
                var age = DateTime.Today.Year - dateOfBirth.Year;
                
                if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                {
                    age--;
                }
                
                if (age >= requirement.MinimumAge)
                {
                    context.Succeed(requirement);
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
```

---

### Aplicar Policies en Controllers

```csharp
// ErrorsController.cs
using Microsoft.AspNetCore.Authorization;

namespace ErrorService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // ✅ OBLIGATORIO - Requiere autenticación por defecto
    public class ErrorsController : ControllerBase
    {
        // Endpoint público (DEBE ser explícitamente aprobado)
        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy" });
        }
        
        // Requiere autenticación básica
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // ...
        }
        
        // Requiere policy específico
        [Authorize(Policy = "ErrorServiceAccess")]
        [HttpPost]
        public async Task<IActionResult> LogError([FromBody] LogErrorCommand command)
        {
            // ...
        }
        
        // Requiere rol Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // ...
        }
        
        // Múltiples policies
        [Authorize(Policy = "ErrorServiceAccess")]
        [Authorize(Policy = "MinimumAge18")]
        [HttpGet("sensitive/{id}")]
        public async Task<IActionResult> GetSensitive(Guid id)
        {
            // ...
        }
    }
}
```

---

## 🔒 CAPA 3: INPUT VALIDATION (SQL INJECTION, XSS, CSRF)

### FluentValidation con Seguridad

```csharp
// LogErrorCommandValidator.cs
using FluentValidation;
using System.Text.RegularExpressions;

namespace ErrorService.Application.Commands.LogError
{
    public class LogErrorCommandValidator : AbstractValidator<LogErrorCommand>
    {
        // Patterns para detectar ataques
        private static readonly Regex SqlInjectionPattern = new(
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)|" +
            @"(--|;|\/\*|\*\/|xp_|sp_)|" +
            @"('(''|[^'])*')|" +
            @"(\bOR\b.*=.*)|" +
            @"(\bAND\b.*=.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        private static readonly Regex XssPattern = new(
            @"(<script|<iframe|<object|<embed|<img|javascript:|onerror=|onload=|onclick=|onmouseover=)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public LogErrorCommandValidator()
        {
            // ServiceName
            RuleFor(x => x.ServiceName)
                .NotEmpty()
                .WithMessage("ServiceName is required")
                .MaximumLength(100)
                .WithMessage("ServiceName cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\-_\.]+$")
                .WithMessage("ServiceName can only contain alphanumeric characters, hyphens, underscores, and dots")
                .Must(NotContainSqlInjection)
                .WithMessage("ServiceName contains potentially dangerous SQL injection patterns")
                .Must(NotContainXss)
                .WithMessage("ServiceName contains potentially dangerous XSS patterns");
            
            // ExceptionType
            RuleFor(x => x.ExceptionType)
                .NotEmpty()
                .WithMessage("ExceptionType is required")
                .MaximumLength(200)
                .WithMessage("ExceptionType cannot exceed 200 characters")
                .Must(NotContainSqlInjection)
                .WithMessage("ExceptionType contains potentially dangerous SQL injection patterns")
                .Must(NotContainXss)
                .WithMessage("ExceptionType contains potentially dangerous XSS patterns");
            
            // Message
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required")
                .MaximumLength(5000)
                .WithMessage("Message cannot exceed 5000 characters")
                .Must(NotContainSqlInjection)
                .WithMessage("Message contains potentially dangerous SQL injection patterns")
                .Must(NotContainXss)
                .WithMessage("Message contains potentially dangerous XSS patterns");
            
            // StackTrace
            RuleFor(x => x.StackTrace)
                .MaximumLength(50000)
                .WithMessage("StackTrace cannot exceed 50000 characters")
                .When(x => !string.IsNullOrEmpty(x.StackTrace));
            
            // StatusCode
            RuleFor(x => x.StatusCode)
                .InclusiveBetween(100, 599)
                .WithMessage("StatusCode must be between 100 and 599");
        }
        
        private bool NotContainSqlInjection(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            
            return !SqlInjectionPattern.IsMatch(value);
        }
        
        private bool NotContainXss(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            
            return !XssPattern.IsMatch(value);
        }
    }
}
```

---

### Anti-CSRF Tokens

```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// Endpoint para obtener token CSRF
app.MapGet("/api/csrf-token", (IAntiforgery antiforgery, HttpContext context) =>
{
    var tokens = antiforgery.GetAndStoreTokens(context);
    return Results.Ok(new { token = tokens.RequestToken });
});
```

```csharp
// Uso en controller
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([FromBody] CreateCommand command)
{
    // ...
}
```

---

## 🚦 CAPA 4: RATE LIMITING

### Configuración de Rate Limiting

```csharp
// Program.cs
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    // Fixed Window Limiter
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    // Sliding Window Limiter
    options.AddSlidingWindowLimiter("sliding", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 4;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    // Token Bucket Limiter
    options.AddTokenBucketLimiter("token", limiterOptions =>
    {
        limiterOptions.TokenLimit = 100;
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        limiterOptions.TokensPerPeriod = 100;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    // Concurrency Limiter
    options.AddConcurrencyLimiter("concurrency", limiterOptions =>
    {
        limiterOptions.PermitLimit = 50;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 20;
    });
    
    // Policy por IP
    options.AddPolicy("ByIpAddress", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
    
    // Rejection handler
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : null
        }, cancellationToken: cancellationToken);
    };
});

var app = builder.Build();

app.UseRateLimiter();  // ANTES de UseAuthentication
```

---

### Aplicar Rate Limiting

```csharp
// Global
app.MapControllers()
    .RequireRateLimiting("fixed");

// Por endpoint
[EnableRateLimiting("sliding")]
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateCommand command)
{
    // ...
}

// Deshabilitar rate limiting
[DisableRateLimiting]
[HttpGet("health")]
public IActionResult HealthCheck()
{
    // ...
}
```

---

## 🔐 CAPA 5: HTTPS Y TLS

### Configuración HTTPS

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// HTTPS Redirection
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort = 443;
});

// HSTS (HTTP Strict Transport Security)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
```

---

### Configuración de Certificados (Production)

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://+:443",
        "Certificate": {
          "Path": "/app/certs/certificate.pfx",
          "Password": "STORE_IN_KEY_VAULT"
        }
      }
    }
  }
}
```

---

## 🌐 CAPA 6: CORS CONFIGURATION

### CORS Restrictivo

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    // Development - Permisivo
    options.AddPolicy("DevelopmentPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    
    // Production - Restrictivo
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder
            .WithOrigins(
                "https://cardealer.com",
                "https://www.cardealer.com",
                "https://admin.cardealer.com")
            .WithHeaders("Content-Type", "Authorization", "X-CSRF-TOKEN")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
    
    // Default - Más restrictivo
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("https://cardealer.com")
            .WithHeaders("Content-Type", "Authorization")
            .WithMethods("GET", "POST")
            .AllowCredentials();
    });
});

var app = builder.Build();

// Aplicar política según ambiente
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
}
```

---

## 🔑 CAPA 7: SECRETS MANAGEMENT

### User Secrets (Development)

```powershell
# Inicializar User Secrets
dotnet user-secrets init --project ErrorService.Api

# Agregar secrets
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-min-32-chars-long" --project ErrorService.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=errorservice;..." --project ErrorService.Api
dotnet user-secrets set "RabbitMQ:Password" "your-rabbitmq-password" --project ErrorService.Api

# Listar secrets
dotnet user-secrets list --project ErrorService.Api

# Remover secret
dotnet user-secrets remove "Jwt:SecretKey" --project ErrorService.Api

# Limpiar todos
dotnet user-secrets clear --project ErrorService.Api
```

---

### Azure Key Vault (Production)

```csharp
// Program.cs
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Configurar Azure Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential());
    }
}
```

```json
{
  "KeyVault": {
    "Url": "https://cardealer-keyvault.vault.azure.net/"
  }
}
```

---

### Environment Variables (Staging/Production)

```csharp
// Program.cs - Leer desde variables de entorno
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
    ?? builder.Configuration["Jwt:SecretKey"];

var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
```

---

## 🛡️ SECURITY HEADERS

### Configuración de Security Headers

```csharp
// Program.cs
var app = builder.Build();

app.Use(async (context, next) =>
{
    // X-Content-Type-Options
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    
    // X-Frame-Options
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    
    // X-XSS-Protection
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    // Referrer-Policy
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Content-Security-Policy
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';");
    
    // Permissions-Policy
    context.Response.Headers.Add("Permissions-Policy",
        "geolocation=(), microphone=(), camera=()");
    
    // Remover headers que revelan información
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("X-AspNet-Version");
    
    await next();
});
```

---

## 🔐 PASSWORD HASHING (AuthService)

### Configuración de Password Hasher

```csharp
// PasswordHasher.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthService.Infrastructure.Security
{
    public class PasswordHasher
    {
        private const int SaltSize = 128 / 8;  // 16 bytes
        private const int HashSize = 256 / 8;  // 32 bytes
        private const int Iterations = 100000;
        
        public string HashPassword(string password)
        {
            // Generar salt aleatorio
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            
            // Hash del password
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);
            
            // Combinar salt + hash
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            
            return Convert.ToBase64String(hashBytes);
        }
        
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Extraer salt y hash
            var hashBytes = Convert.FromBase64String(hashedPassword);
            
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            var storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);
            
            // Hash del password ingresado
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize);
            
            // Comparación constante en tiempo (evita timing attacks)
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
```

---

## ✅ CHECKLIST DE SEGURIDAD

### Authentication & Authorization
- [ ] JWT Authentication configurado
- [ ] Token expiration configurado (60 min)
- [ ] Clock skew configurado (5 min)
- [ ] Authorization policies definidas
- [ ] `[Authorize]` en todos los controllers
- [ ] `[AllowAnonymous]` solo en endpoints aprobados
- [ ] Custom authorization handlers si es necesario
- [ ] Claims correctamente configurados
- [ ] Roles implementados correctamente

### Input Validation
- [ ] FluentValidation en todos los commands/queries
- [ ] SQL Injection detection implementado
- [ ] XSS detection implementado
- [ ] Max length validation en todos los strings
- [ ] Regex validation para formatos específicos
- [ ] Anti-CSRF tokens configurados (si aplica)

### Rate Limiting
- [ ] Rate limiting configurado
- [ ] Policy apropiada seleccionada (fixed/sliding/token/concurrency)
- [ ] Límites apropiados según carga esperada
- [ ] Rejection handler personalizado
- [ ] Health checks excluidos de rate limiting

### HTTPS & TLS
- [ ] HTTPS redirection habilitado
- [ ] HSTS configurado (production)
- [ ] Certificados válidos (production)
- [ ] TLS 1.2+ únicamente

### CORS
- [ ] CORS policy restrictiva (production)
- [ ] Origins explícitamente listados
- [ ] Headers permitidos mínimos
- [ ] Methods permitidos mínimos
- [ ] Credentials correctamente configurados

### Secrets Management
- [ ] User Secrets configurado (development)
- [ ] Azure Key Vault configurado (production)
- [ ] NO secrets en appsettings.json
- [ ] NO secrets en código fuente
- [ ] .gitignore incluye secrets.json

### Security Headers
- [ ] X-Content-Type-Options: nosniff
- [ ] X-Frame-Options: DENY
- [ ] X-XSS-Protection: 1; mode=block
- [ ] Content-Security-Policy configurado
- [ ] Referrer-Policy configurado
- [ ] Server headers removidos

### Password Security (AuthService)
- [ ] PBKDF2 con SHA256
- [ ] 100,000+ iterations
- [ ] Salt aleatorio por password
- [ ] Timing-safe comparison

---

## 📚 RECURSOS Y REFERENCIAS

- **Microservicio de Referencia**: `ErrorService/Program.cs`
- **OWASP Top 10**: [https://owasp.org/www-project-top-ten/](https://owasp.org/www-project-top-ten/)
- **JWT.io**: [https://jwt.io/](https://jwt.io/)
- **ASP.NET Core Security**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/)
- **FluentValidation**: [https://docs.fluentvalidation.net/](https://docs.fluentvalidation.net/)
- **Azure Key Vault**: [Microsoft Docs](https://docs.microsoft.com/en-us/azure/key-vault/)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Seguridad es NO NEGOCIABLE. PRs sin validación de SQL Injection/XSS son automáticamente RECHAZADOS.
