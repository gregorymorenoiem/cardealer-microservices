# 🏢 ARQUITECTURA: PLATAFORMA SaaS ERP PARA DEALERS

**Fecha**: Diciembre 5, 2025  
**Visión**: Plataforma all-in-one para que dealers gestionen TODO su negocio  
**Modelo**: SaaS Multi-Tenant con módulos vendibles  
**Comparables**: Shopify + Salesforce + QuickBooks para dealers

---

## 🎯 VISIÓN COMPLETA DEL NEGOCIO

### Tu plataforma NO es solo un marketplace, es:

```
🏪 Marketplace (Frontend público)
   ├── Venta de vehículos
   ├── Búsqueda y filtros
   └── Contacto con dealers

📊 ERP/CRM para Dealers (Portal administrativo)
   ├── Gestión de inventario
   ├── CRM y leads
   ├── Facturación y contabilidad
   ├── Módulos adicionales (compra por suscripción)
   └── Todos los procesos del dealer desde TU plataforma
```

### Modelo de Negocio (Similar a Shopify):

| Concepto | Shopify (E-commerce) | Tu Plataforma (Dealers) |
|----------|---------------------|-------------------------|
| **Plan Base** | $29-299/mes | FREE, BASIC, PRO, ENTERPRISE |
| **Core Features** | Tienda online | Marketplace + Inventario |
| **Módulos Extras** | Apps ($5-50/mes c/u) | CRM, Facturación, WhatsApp |
| **Transacciones** | 2.9% + 30¢ | Comisión por venta (5-10%) |
| **Customización** | Themes | Branding personalizado |

---

## 🏗️ ARQUITECTURA DE MICROSERVICIOS COMPLETA

### Servicios CORE (Ya tienes muchos ✅):

```
✅ UserService/              # Usuarios, dealers, empleados, suscripciones
✅ AuthService/              # Login, JWT, OAuth
✅ RoleService/              # RBAC, permisos
✅ MediaService/             # Imágenes, videos, documentos
✅ NotificationService/      # Emails, SMS, push notifications
✅ SearchService/            # Elasticsearch para búsquedas
✅ AuditService/             # Logs de auditoría
✅ ConfigurationService/     # Feature flags, configs
✅ FileStorageService/       # S3, Azure Blob
✅ Gateway/                  # API Gateway (Ocelot o YARP)
✅ CacheService/             # Redis
✅ MessageBusService/        # RabbitMQ/Kafka
✅ SchedulerService/         # Hangfire/Quartz
✅ HealthCheckService/       # Monitoring
✅ RateLimitingService/      # Rate limiting
✅ TracingService/           # OpenTelemetry
```

### Servicios de NEGOCIO (Faltan algunos 🆕):

```
🔄 VehicleService/           # → Migrar a ProductService (genérico)
   └── Inventario de productos (vehículos ahora, cualquier cosa después)

✅ ContactService/           # Leads, consultas, mensajes
   └── Base de tu CRM

🆕 CRMService/               # ← NUEVO (Módulo vendible)
   ├── Leads management
   ├── Pipeline de ventas
   ├── Follow-ups automáticos
   ├── Customer journey
   └── Integración con ContactService

🆕 InvoicingService/         # ← NUEVO (Módulo vendible)
   ├── Facturas (CFDI México, etc.)
   ├── Cotizaciones
   ├── Pagos
   ├── Reportes fiscales
   └── Integración con contabilidad

🆕 FinanceService/           # ← NUEVO (Módulo vendible)
   ├── Contabilidad básica
   ├── Gastos e ingresos
   ├── Balance general
   ├── Reportes financieros
   └── Integración con InvoicingService

🆕 InventoryService/         # ← NUEVO o expandir VehicleService
   ├── Stock management
   ├── Alertas de inventario bajo
   ├── Movimientos (entradas/salidas)
   ├── Valuación de inventario
   └── Órdenes de compra

🆕 ReportsService/           # ← NUEVO (Módulo vendible)
   ├── Reportes de ventas
   ├── Analytics avanzados
   ├── Dashboards personalizados
   ├── Export a Excel/PDF
   └── Scheduled reports

🆕 IntegrationService/       # ← NUEVO (Módulo vendible)
   ├── WhatsApp Business API
   ├── Facebook Marketplace
   ├── Instagram Shopping
   ├── Google My Business
   ├── Webhooks personalizados
   └── API pública para integraciones

🆕 MarketingService/         # ← NUEVO (Módulo vendible)
   ├── Email campaigns
   ├── SMS marketing
   ├── Landing pages
   ├── Lead magnets
   ├── A/B testing
   └── Marketing automation

🆕 CustomerPortalService/    # ← NUEVO
   ├── Portal para clientes finales
   ├── Seguimiento de órdenes
   ├── Historial de compras
   ├── Documentos (contratos, facturas)
   └── Citas y servicio post-venta

🆕 AppointmentService/       # ← NUEVO
   ├── Agendamiento de citas
   ├── Test drives
   ├── Servicio post-venta
   ├── Recordatorios automáticos
   └── Sincronización con calendarios
```

---

## 📦 SISTEMA DE MÓDULOS (Add-ons)

### Tabla: `ModuleAddons` (en UserService o nuevo ModuleService)

```csharp
// UserService/UserService.Domain/Entities/ModuleAddon.cs
public class ModuleAddon
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // "CRM Avanzado", "Facturación"
    public string Code { get; set; } = string.Empty; // "crm-advanced", "invoicing"
    public string Description { get; set; } = string.Empty;
    public ModuleCategory Category { get; set; } // Sales, Finance, Marketing, Integration
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; } // Descuento anual
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    // Features del módulo
    public string Features { get; set; } = "[]"; // JSON array
    
    // Dependencias (algunos módulos requieren otros)
    public List<string> RequiredModules { get; set; } = new(); // ["crm-basic"]
    
    // Planes que incluyen este módulo gratis
    public List<DealerPlan> IncludedInPlans { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
}

public enum ModuleCategory
{
    Core,           // Incluido en plan base
    Sales,          // CRM, leads
    Finance,        // Facturación, contabilidad
    Marketing,      // Email, SMS, campañas
    Integration,    // WhatsApp, Facebook, APIs
    Analytics,      // Reportes avanzados
    Automation      // Workflows, reglas
}
```

### Tabla: `DealerModuleSubscription` (módulos activos por dealer)

```csharp
// UserService/UserService.Domain/Entities/DealerModuleSubscription.cs
public class DealerModuleSubscription
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public Guid ModuleAddonId { get; set; }
    public SubscriptionStatus Status { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    
    // Billing
    public decimal MonthlyPrice { get; set; } // Precio en el momento de suscribirse
    public string? StripeSubscriptionItemId { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    
    // Navigation
    public ModuleAddon ModuleAddon { get; set; } = null!;
    public User Dealer { get; set; } = null!;
}
```

---

## 💰 MODELO DE PRICING (Ejemplo)

### Planes Base:

| Plan | Precio | Listings | Módulos Incluidos | Add-ons |
|------|--------|----------|-------------------|---------|
| **FREE** | $0/mes | 3 | Marketplace básico | ❌ |
| **BASIC** | $49/mes | 50 | + Inventario + CRM básico | ✅ |
| **PRO** | $149/mes | 200 | + Facturación + Marketing básico | ✅ |
| **ENTERPRISE** | $499/mes | Ilimitado | + Todos los módulos | ✅ |

### Módulos Add-ons (solo BASIC y PRO):

| Módulo | Precio/mes | Incluido en Plan |
|--------|------------|------------------|
| **CRM Avanzado** | $29 | ENTERPRISE |
| **Facturación Electrónica (CFDI)** | $39 | PRO, ENTERPRISE |
| **Contabilidad** | $49 | ENTERPRISE |
| **WhatsApp Business** | $19 | PRO, ENTERPRISE |
| **Marketing Automation** | $59 | ENTERPRISE |
| **Reportes Avanzados** | $29 | ENTERPRISE |
| **Integraciones** | $39 | ENTERPRISE |
| **API Pública** | $99 | ENTERPRISE |

### Revenue Ejemplo (1 dealer PRO):

```
Plan PRO:                      $149/mes
+ CRM Avanzado:                $29/mes
+ WhatsApp Business:           $19/mes
+ Reportes Avanzados:          $29/mes
─────────────────────────────────────
Total:                         $226/mes
MRR por dealer:                $226
Con 100 dealers:               $22,600/mes
Con 1000 dealers:              $226,000/mes
```

---

## 🔐 CONTROL DE ACCESO A MÓDULOS

### Middleware en cada servicio:

```csharp
// CRMService/CRMService.Api/Middleware/ModuleAccessMiddleware.cs
public class ModuleAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IModuleAccessService _moduleAccess;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var dealerId = context.User.FindFirst("dealerId")?.Value;
        
        if (string.IsNullOrEmpty(dealerId))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Dealer ID required" });
            return;
        }
        
        // Verificar si el dealer tiene acceso al módulo CRM
        var hasAccess = await _moduleAccess.HasModuleAccessAsync(
            dealerId: Guid.Parse(dealerId),
            moduleCode: "crm-advanced"
        );
        
        if (!hasAccess)
        {
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "CRM module not available in your plan",
                upgradeUrl = "/billing/modules/crm-advanced"
            });
            return;
        }
        
        await _next(context);
    }
}

// Uso en Program.cs
app.UseModuleAccess("crm-advanced");
```

### Service: ModuleAccessService

```csharp
// Shared/Services/ModuleAccessService.cs
public interface IModuleAccessService
{
    Task<bool> HasModuleAccessAsync(Guid dealerId, string moduleCode);
    Task<List<string>> GetActiveModulesAsync(Guid dealerId);
}

public class ModuleAccessService : IModuleAccessService
{
    private readonly IDistributedCache _cache;
    private readonly HttpClient _userServiceClient;
    
    public async Task<bool> HasModuleAccessAsync(Guid dealerId, string moduleCode)
    {
        // 1. Check cache (Redis)
        var cacheKey = $"dealer:{dealerId}:modules";
        var cachedModules = await _cache.GetStringAsync(cacheKey);
        
        if (cachedModules != null)
        {
            var modules = JsonSerializer.Deserialize<List<string>>(cachedModules);
            return modules.Contains(moduleCode);
        }
        
        // 2. Query UserService
        var response = await _userServiceClient.GetAsync(
            $"/api/dealers/{dealerId}/active-modules"
        );
        
        if (!response.IsSuccessStatusCode)
            return false;
        
        var activeModules = await response.Content.ReadFromJsonAsync<List<string>>();
        
        // 3. Cache por 5 minutos
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(activeModules),
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
            }
        );
        
        return activeModules.Contains(moduleCode);
    }
}
```

---

## 📱 FRONTEND: ARQUITECTURA DE MÓDULOS

### Portal del Dealer con Sidebar Dinámico:

```typescript
// frontend/web/src/layouts/DealerPortalLayout.tsx
const DealerPortalLayout: React.FC = () => {
  const { activeModules } = useDealerModules();
  const { subscription } = useDealerSubscription();
  
  const menuItems = [
    // Core (siempre visible)
    { icon: Home, label: 'Dashboard', path: '/dealer/dashboard', module: null },
    { icon: Package, label: 'Inventory', path: '/dealer/inventory', module: null },
    { icon: Eye, label: 'My Listings', path: '/dealer/listings', module: null },
    
    // CRM (requiere módulo)
    { 
      icon: Users, 
      label: 'CRM', 
      path: '/dealer/crm', 
      module: 'crm-advanced',
      badge: subscription.plan === 'FREE' ? 'PRO' : null 
    },
    
    // Facturación (requiere módulo)
    { 
      icon: FileText, 
      label: 'Invoicing', 
      path: '/dealer/invoicing', 
      module: 'invoicing',
      badge: !activeModules.includes('invoicing') ? 'Add-on' : null
    },
    
    // Contabilidad (requiere módulo)
    { 
      icon: DollarSign, 
      label: 'Finance', 
      path: '/dealer/finance', 
      module: 'finance',
      badge: subscription.plan !== 'ENTERPRISE' ? 'Enterprise' : null
    },
    
    // Marketing (requiere módulo)
    { 
      icon: Mail, 
      label: 'Marketing', 
      path: '/dealer/marketing', 
      module: 'marketing-automation',
      badge: !activeModules.includes('marketing-automation') ? '$59/mo' : null
    },
    
    // Reportes (requiere módulo)
    { 
      icon: BarChart, 
      label: 'Reports', 
      path: '/dealer/reports', 
      module: 'reports-advanced',
      badge: !activeModules.includes('reports-advanced') ? '$29/mo' : null
    },
    
    // Integraciones (requiere módulo)
    { 
      icon: Zap, 
      label: 'Integrations', 
      path: '/dealer/integrations', 
      module: 'integrations',
      badge: subscription.plan !== 'ENTERPRISE' ? 'Enterprise' : null
    },
    
    // Configuración (siempre visible)
    { icon: Settings, label: 'Settings', path: '/dealer/settings', module: null },
  ];
  
  const handleLockedModuleClick = (module: string) => {
    // Redirect a página de upgrade
    navigate(`/dealer/billing/modules/${module}`);
  };
  
  return (
    <div className="flex h-screen">
      <Sidebar>
        {menuItems.map(item => (
          <SidebarItem
            key={item.path}
            {...item}
            isLocked={item.module && !activeModules.includes(item.module)}
            onLockedClick={() => handleLockedModuleClick(item.module)}
          />
        ))}
      </Sidebar>
      
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
};
```

### Página de Módulos (Marketplace interno):

```typescript
// frontend/web/src/pages/dealer/billing/ModulesMarketplace.tsx
const ModulesMarketplace: React.FC = () => {
  const { modules, loading } = useAvailableModules();
  const { activeModules } = useDealerModules();
  const { subscription } = useDealerSubscription();
  
  return (
    <div className="p-6">
      <h1>Modules Marketplace</h1>
      <p>Extend your dealership with powerful add-ons</p>
      
      <div className="grid grid-cols-3 gap-6 mt-6">
        {modules.map(module => (
          <ModuleCard
            key={module.code}
            module={module}
            isActive={activeModules.includes(module.code)}
            isIncludedInPlan={module.includedInPlans.includes(subscription.plan)}
            onSubscribe={() => subscribeToModule(module.code)}
            onUnsubscribe={() => unsubscribeFromModule(module.code)}
          />
        ))}
      </div>
    </div>
  );
};

const ModuleCard: React.FC<ModuleCardProps> = ({
  module,
  isActive,
  isIncludedInPlan,
  onSubscribe,
  onUnsubscribe
}) => {
  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-xl font-bold">{module.name}</h3>
        {isActive && <Badge variant="success">Active</Badge>}
        {isIncludedInPlan && <Badge variant="info">Included</Badge>}
      </div>
      
      <p className="text-gray-600 mb-4">{module.description}</p>
      
      <div className="mb-4">
        <span className="text-3xl font-bold">${module.monthlyPrice}</span>
        <span className="text-gray-500">/month</span>
      </div>
      
      <ul className="space-y-2 mb-6">
        {module.features.map((feature, i) => (
          <li key={i} className="flex items-start">
            <Check className="w-5 h-5 text-green-500 mr-2 flex-shrink-0" />
            <span>{feature}</span>
          </li>
        ))}
      </ul>
      
      {isIncludedInPlan ? (
        <Button variant="secondary" disabled>
          Included in your plan
        </Button>
      ) : isActive ? (
        <Button variant="outline" onClick={onUnsubscribe}>
          Unsubscribe
        </Button>
      ) : (
        <Button variant="primary" onClick={onSubscribe}>
          Subscribe for ${module.monthlyPrice}/mo
        </Button>
      )}
    </Card>
  );
};
```

---

## 🎨 ACTUALIZAR DEALERPLANFEATURES

Agregar campos para módulos:

```csharp
// UserService/UserService.Domain/Entities/DealerSubscription.cs
public class DealerPlanFeatures
{
    // ✅ Marketplace features (ya existen)
    public int MaxListings { get; set; }
    public int MaxImages { get; set; }
    public int FeaturedListings { get; set; }
    public bool AnalyticsAccess { get; set; }
    public bool MarketPriceAnalysis { get; set; }
    public bool BulkUpload { get; set; }
    
    // 🆕 CRM features
    public bool CRMBasic { get; set; }                    // FREE=false, BASIC=true
    public bool CRMAdvanced { get; set; }                 // PRO+=true o add-on
    public int MaxLeads { get; set; }                     // FREE=10, BASIC=100, PRO=500
    public bool LeadAutomation { get; set; }              // PRO+=true
    
    // 🆕 Invoicing features
    public bool InvoicingBasic { get; set; }              // BASIC+=true
    public bool InvoicingCFDI { get; set; }               // PRO+=true o add-on
    public int MaxInvoicesPerMonth { get; set; }          // FREE=5, BASIC=50, PRO=200
    
    // 🆕 Finance features
    public bool FinanceBasic { get; set; }                // PRO+=true
    public bool FinanceAdvanced { get; set; }             // ENTERPRISE o add-on
    public bool TaxReports { get; set; }                  // ENTERPRISE
    
    // 🆕 Marketing features
    public bool EmailMarketing { get; set; }              // BASIC+=true
    public int MaxEmailsPerMonth { get; set; }            // BASIC=500, PRO=2000
    public bool SMSMarketing { get; set; }                // PRO+=true o add-on
    public bool MarketingAutomation { get; set; }         // ENTERPRISE o add-on
    
    // 🆕 Integration features
    public bool WhatsappIntegration { get; set; }         // PRO+=true o add-on
    public bool FacebookMarketplace { get; set; }         // PRO+=true
    public bool WebhooksAPI { get; set; }                 // ENTERPRISE o add-on
    public bool PublicAPI { get; set; }                   // ENTERPRISE o add-on ($99/mo)
    
    // 🆕 Reports features
    public bool ReportsBasic { get; set; }                // BASIC+=true
    public bool ReportsAdvanced { get; set; }             // PRO+=true o add-on
    public bool CustomDashboards { get; set; }            // ENTERPRISE o add-on
    public bool ScheduledReports { get; set; }            // ENTERPRISE
    
    // ✅ Support features (ya existen)
    public bool PrioritySupport { get; set; }
    public bool CustomBranding { get; set; }
    public bool ApiAccess { get; set; }
}
```

---

## 🚀 PLAN DE IMPLEMENTACIÓN

### FASE 1: MVP Marketplace + Empleados (Actual - 3 meses)
- ✅ Marketplace de vehículos funcionando
- ✅ Sistema de empleados (21 endpoints)
- ✅ Planes FREE/BASIC/PRO/ENTERPRISE básicos
- ✅ Inventario básico
- ✅ CRM básico (ContactService actual)

### FASE 2: Módulos Core (6-9 meses después de MVP)
- 🆕 **CRMService** completo
  - Pipeline de ventas
  - Seguimiento de leads
  - Automatizaciones básicas
- 🆕 **InvoicingService**
  - Cotizaciones
  - Facturas simples
  - CFDI México (módulo add-on)
- 🆕 **FinanceService**
  - Balance básico
  - Gastos/ingresos
  - Reportes simples

### FASE 3: Módulos Avanzados (12-18 meses)
- 🆕 **MarketingService**
  - Email campaigns
  - SMS (add-on)
  - Landing pages
- 🆕 **IntegrationService**
  - WhatsApp Business (add-on)
  - Facebook/Instagram
  - Webhooks
- 🆕 **ReportsService**
  - Dashboards personalizados
  - Export avanzado
  - Scheduled reports

### FASE 4: Enterprise Features (18-24 meses)
- 🆕 **API Pública** ($99/mo)
- 🆕 **White-label** (branding completo)
- 🆕 **Multi-location** (dealerships con múltiples sucursales)
- 🆕 **Advanced analytics** con ML

---

## 💡 RECOMENDACIONES ESTRATÉGICAS

### 1. Pricing Strategy (Land and Expand):
```
1. FREE plan → Engancha dealers
2. Upgrade a BASIC → $49/mo (bajo punto de entrada)
3. Add-ons → $19-59/mo cada uno (fácil de justificar)
4. ENTERPRISE → $499/mo (cuando son grandes)
```

### 2. Módulos más rentables (priorizar):
1. **WhatsApp Business** ($19/mo) - Alta demanda, fácil implementación
2. **Facturación CFDI** ($39/mo) - Obligatorio en México, sticky
3. **CRM Avanzado** ($29/mo) - High value, retención
4. **API Pública** ($99/mo) - Pocas ventas pero alto margen

### 3. Features que aumentan retención:
- 📊 Analytics (dealers ven ROI)
- 📧 Email automation (ahorra tiempo)
- 💬 WhatsApp (canal principal de ventas)
- 🧾 Facturación (critical workflow)

### 4. Go-to-Market:
- Mes 1-6: Solo marketplace (FREE + BASIC)
- Mes 7-12: Agregar CRM + Facturación (PRO plan)
- Mes 13-18: Add-ons marketplace
- Mes 19+: Enterprise features

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### Backend:
- [ ] Crear tabla `ModuleAddons`
- [ ] Crear tabla `DealerModuleSubscription`
- [ ] Endpoint `GET /api/dealers/{id}/active-modules`
- [ ] Endpoint `POST /api/dealers/{id}/modules/{code}/subscribe`
- [ ] Endpoint `DELETE /api/dealers/{id}/modules/{code}/unsubscribe`
- [ ] ModuleAccessMiddleware en cada servicio
- [ ] ModuleAccessService compartido
- [ ] Integración con Stripe (subscriptions items)
- [ ] Webhooks para activación/desactivación

### Servicios Nuevos (por prioridad):
- [ ] CRMService (alta prioridad)
- [ ] InvoicingService (alta prioridad - sticky)
- [ ] FinanceService (media prioridad)
- [ ] MarketingService (media prioridad)
- [ ] IntegrationService (alta prioridad - WhatsApp)
- [ ] ReportsService (media prioridad)

### Frontend:
- [ ] Sidebar dinámico con módulos
- [ ] Página ModulesMarketplace
- [ ] Badges de upgrade en sidebar
- [ ] Modal de paywall cuando no tiene acceso
- [ ] Billing page con módulos activos
- [ ] UI de cada módulo (CRM, Invoicing, etc.)

---

**Conclusión**: Tu visión de **Shopify para Dealers** es brillante. El marketplace es solo el hook para capturar dealers, pero el verdadero revenue viene de los módulos SaaS (CRM, Facturación, WhatsApp, etc.). Implementa el MVP primero, valida tracción, y luego agrega módulos basado en demanda real. 🚀
