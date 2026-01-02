# 🔐 EJEMPLO: Implementar Verificación de Módulos en Microservicios

Este documento muestra cómo agregar verificación de acceso a módulos en cualquier microservicio.

---

## 📁 Estructura de Archivos

```
CRMService/
├── CRMService.Api/
│   ├── Program.cs                      # ← Configurar middleware aquí
│   ├── appsettings.json                # ← URL de UserService
│   ├── Controllers/
│   │   ├── LeadsController.cs         # Endpoints de CRM
│   │   └── PipelineController.cs
│   └── CRMService.Api.csproj
└── ...
```

---

## 🔧 PASO 1: Configurar `appsettings.json`

Agregar URL del UserService:

```json
{
  "Services": {
    "UserService": {
      "BaseUrl": "http://localhost:5001",
      "Timeout": 30
    }
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

---

## 🔧 PASO 2: Registrar Servicios en `Program.cs`

```csharp
using CarDealer.Shared.Services;
using CarDealer.Shared.Middleware;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// ====================================
// 1. REDIS CACHE (para módulos activos)
// ====================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "CRMService_";
});

// ====================================
// 2. HTTP CLIENT para UserService
// ====================================
builder.Services.AddHttpClient("UserService", client =>
{
    var baseUrl = builder.Configuration["Services:UserService:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ====================================
// 3. MODULE ACCESS SERVICE
// ====================================
builder.Services.AddScoped<IModuleAccessService, ModuleAccessService>();

// Otros servicios...
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ====================================
// 4. MIDDLEWARE DE VERIFICACIÓN (GLOBAL)
// ====================================
// Todas las requests a este servicio requieren "crm-advanced"
app.UseModuleAccess("crm-advanced");

// Otros middlewares...
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

---

## ✅ PASO 3: Endpoints Protegidos Automáticamente

Con el middleware global, **TODOS** los endpoints están protegidos:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CRMService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Solo usuarios autenticados
    public class LeadsController : ControllerBase
    {
        private readonly ILeadService _leadService;
        
        public LeadsController(ILeadService leadService)
        {
            _leadService = leadService;
        }
        
        // ✅ Protegido automáticamente por middleware
        // Requiere: JWT válido + dealerId claim + módulo "crm-advanced"
        [HttpGet]
        public async Task<IActionResult> GetLeads()
        {
            var dealerId = User.FindFirst("dealerId")?.Value;
            var leads = await _leadService.GetLeadsAsync(Guid.Parse(dealerId));
            return Ok(leads);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request)
        {
            var dealerId = User.FindFirst("dealerId")?.Value;
            var lead = await _leadService.CreateLeadAsync(Guid.Parse(dealerId), request);
            return CreatedAtAction(nameof(GetLead), new { id = lead.Id }, lead);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLead(Guid id)
        {
            var lead = await _leadService.GetLeadByIdAsync(id);
            if (lead == null) return NotFound();
            return Ok(lead);
        }
    }
}
```

---

## 🛡️ PASO 4: Proteger Endpoints Específicos (Alternativa)

Si NO quieres middleware global, puedes proteger endpoints individuales con atributo:

```csharp
using CarDealer.Shared.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CRMService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadsController : ControllerBase
    {
        // ✅ Este endpoint requiere módulo "crm-advanced"
        [HttpGet]
        [RequireModule("crm-advanced")]
        public async Task<IActionResult> GetLeads()
        {
            // ...
        }
        
        // ✅ Este endpoint requiere módulo "crm-advanced"
        [HttpPost]
        [RequireModule("crm-advanced")]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request)
        {
            // ...
        }
        
        // ❌ Este endpoint NO requiere módulo (público)
        [HttpGet("pricing")]
        [AllowAnonymous]
        public IActionResult GetPricing()
        {
            return Ok(new { monthly = 29, yearly = 290 });
        }
    }
}
```

**Nota**: Para que `[RequireModule]` funcione, necesitas un `ActionFilter`:

```csharp
using CarDealer.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequireModuleFilter : IAsyncActionFilter
{
    private readonly IModuleAccessService _moduleAccessService;
    
    public RequireModuleFilter(IModuleAccessService moduleAccessService)
    {
        _moduleAccessService = moduleAccessService;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireModuleAttribute>()
            .FirstOrDefault();
        
        if (attribute != null)
        {
            var dealerIdClaim = context.HttpContext.User.FindFirst("dealerId")?.Value;
            
            if (string.IsNullOrEmpty(dealerIdClaim))
            {
                context.Result = new ForbidResult();
                return;
            }
            
            var hasAccess = await _moduleAccessService.HasModuleAccessAsync(
                Guid.Parse(dealerIdClaim), 
                attribute.ModuleCode
            );
            
            if (!hasAccess)
            {
                context.Result = new ObjectResult(new 
                { 
                    error = "Payment Required",
                    message = $"This feature requires the '{attribute.ModuleCode}' module",
                    moduleCode = attribute.ModuleCode,
                    upgradeUrl = $"/dealer/billing/modules/{attribute.ModuleCode}"
                })
                {
                    StatusCode = 402
                };
                return;
            }
        }
        
        await next();
    }
}

// Registrar en Program.cs:
builder.Services.AddScoped<RequireModuleFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequireModuleFilter>();
});
```

---

## 📦 PASO 5: Verificación Programática (Opcional)

Si necesitas verificar acceso dentro de un método:

```csharp
public class LeadService : ILeadService
{
    private readonly IModuleAccessService _moduleAccessService;
    private readonly ILogger<LeadService> _logger;
    
    public LeadService(
        IModuleAccessService moduleAccessService,
        ILogger<LeadService> logger)
    {
        _moduleAccessService = moduleAccessService;
        _logger = logger;
    }
    
    public async Task<List<Lead>> GetLeadsAsync(Guid dealerId)
    {
        // Verificación programática
        var hasAccess = await _moduleAccessService.HasModuleAccessAsync(dealerId, "crm-advanced");
        
        if (!hasAccess)
        {
            _logger.LogWarning("Dealer {DealerId} attempted to access CRM without subscription", dealerId);
            throw new UnauthorizedAccessException("CRM Advanced module required");
        }
        
        // Lógica normal...
        return await _repository.GetLeadsAsync(dealerId);
    }
    
    // Verificación de múltiples módulos
    public async Task<bool> CanExportReports(Guid dealerId)
    {
        var access = await _moduleAccessService.HasModulesAccessAsync(
            dealerId, 
            "crm-advanced",      // Requiere CRM
            "analytics-reports"  // Y reportes avanzados
        );
        
        return access["crm-advanced"] && access["analytics-reports"];
    }
}
```

---

## 🧪 PASO 6: Testing

```csharp
using Xunit;
using Moq;
using CarDealer.Shared.Services;

public class LeadsControllerTests
{
    [Fact]
    public async Task GetLeads_WithoutModuleAccess_Returns402()
    {
        // Arrange
        var mockModuleService = new Mock<IModuleAccessService>();
        mockModuleService
            .Setup(x => x.HasModuleAccessAsync(It.IsAny<Guid>(), "crm-advanced"))
            .ReturnsAsync(false); // Sin acceso
        
        var controller = new LeadsController(mockModuleService.Object, Mock.Of<ILeadService>());
        
        // Act
        var result = await controller.GetLeads();
        
        // Assert
        Assert.IsType<ObjectResult>(result);
        var objectResult = result as ObjectResult;
        Assert.Equal(402, objectResult.StatusCode);
    }
    
    [Fact]
    public async Task GetLeads_WithModuleAccess_ReturnsOk()
    {
        // Arrange
        var mockModuleService = new Mock<IModuleAccessService>();
        mockModuleService
            .Setup(x => x.HasModuleAccessAsync(It.IsAny<Guid>(), "crm-advanced"))
            .ReturnsAsync(true); // Con acceso
        
        var mockLeadService = new Mock<ILeadService>();
        mockLeadService
            .Setup(x => x.GetLeadsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Lead>());
        
        var controller = new LeadsController(mockModuleService.Object, mockLeadService.Object);
        
        // Act
        var result = await controller.GetLeads();
        
        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
```

---

## ⚡ PASO 7: Invalidar Cache al Cambiar Suscripción

En `UserService`, cuando un dealer se suscribe/desuscribe de un módulo:

```csharp
// UserService/Application/UseCases/SubscribeToModuleUseCase.cs
public class SubscribeToModuleUseCase
{
    private readonly IModuleAccessService _moduleAccessService;
    private readonly IUserRepository _userRepository;
    
    public async Task<Result> ExecuteAsync(Guid dealerId, string moduleCode)
    {
        // 1. Activar suscripción en BD
        var subscription = await _userRepository.SubscribeToModuleAsync(dealerId, moduleCode);
        
        // 2. Invalidar cache (IMPORTANTE)
        await _moduleAccessService.InvalidateCacheAsync(dealerId);
        
        // 3. Enviar notificación
        await _notificationService.SendModuleActivatedEmailAsync(dealerId, moduleCode);
        
        return Result.Success();
    }
}
```

---

## 📊 Respuestas HTTP

### ✅ Con Acceso (200 OK):
```json
{
  "leads": [
    { "id": "...", "name": "John Doe", "email": "john@example.com" }
  ]
}
```

### ❌ Sin Acceso (402 Payment Required):
```json
{
  "error": "Payment Required",
  "message": "This feature requires the 'crm-advanced' module",
  "moduleCode": "crm-advanced",
  "upgradeUrl": "/dealer/billing/modules/crm-advanced"
}
```

### ❌ Sin DealerId (403 Forbidden):
```json
{
  "error": "Forbidden",
  "message": "Dealer ID required"
}
```

---

## 🚀 Microservicios que Necesitan Este Patrón

| Servicio | Módulo Requerido | Prioridad |
|----------|-----------------|-----------|
| **CRMService** | `crm-advanced` | 🔥 Alta |
| **InvoicingService** | `invoicing-cfdi` | 🔥 Alta |
| **FinanceService** | `finance-accounting` | ⚡ Media |
| **MarketingService** | `marketing-automation` | ⚡ Media |
| **IntegrationService** (WhatsApp) | `integration-whatsapp` | 🔥 Alta |
| **ReportsService** | `analytics-reports` | ⚡ Media |

---

## ✅ Checklist de Implementación

- [ ] Agregar `ModuleAccessService` a `_Shared/Services/`
- [ ] Agregar `ModuleAccessMiddleware` a `_Shared/Middleware/`
- [ ] Configurar Redis en cada microservicio
- [ ] Registrar HttpClient para UserService
- [ ] Agregar middleware en `Program.cs`
- [ ] Endpoint `/api/dealers/{id}/active-modules` en UserService
- [ ] Invalidar cache al cambiar suscripciones
- [ ] Tests unitarios de verificación
- [ ] Documentación en Swagger (mostrar 402 como posible respuesta)

---

**Listo**: Con esto, cualquier microservicio puede verificar acceso a módulos de forma consistente. 🚀
