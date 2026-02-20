---
version: 3.0
lastUpdated: 2026-02-20
author: Engineering Lead
category: infrastructure + qa
scope: backend-infra, full-platform-audit, user-flows, qa-automation
---

# 🚀 OKLA — Phase 3: Infrastructure Fixes + Full Platform QA Audit

> **INSTRUCCIÓN CRÍTICA:** Ejecuta CADA tarea hasta completarla. Cuando encuentres un error, corrígelo INMEDIATAMENTE y continúa. NO te detengas. NO reportes un error y esperes — corrígelo y avanza. Solo termina cuando TODOS los ítems de esta lista estén marcados como completados y verificados.

---

## 📋 CONTEXTO DEL PROYECTO

**OKLA** es un marketplace de compra/venta de vehículos en República Dominicana.

| Capa        | Tecnología                                                                             |
| ----------- | -------------------------------------------------------------------------------------- |
| Backend     | .NET 8, Clean Architecture, CQRS + MediatR                                             |
| Frontend    | Next.js 16.1.6, TypeScript, App Router, pnpm                                           |
| DB          | PostgreSQL 16 (DO Managed: `okla-db-do-user-31493168-0.g.db.ondigitalocean.com:25060`) |
| Cache       | Redis 7 (in-cluster K8s)                                                               |
| Messaging   | RabbitMQ 3.12 (in-cluster K8s)                                                         |
| Gateway     | Ocelot 22.0.1                                                                          |
| K8s         | DigitalOcean DOKS — namespace `okla`, cluster `okla-cluster`                           |
| CI/CD       | GitHub Actions → `ghcr.io/gregorymorenoiem/{service}:latest`                           |
| Prod URL    | `https://okla.com.do` (BFF: Next.js → gateway:8080)                                    |
| Dev Gateway | `http://localhost:18443`                                                               |

**⚠️ Package manager: SIEMPRE `pnpm`. NUNCA `npm` o `yarn`.**

### 🔑 Cuentas de Test

| Tipo                                          | Email                              | Password            | IDs                                                                                              |
| --------------------------------------------- | ---------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------ |
| Seller                                        | `seller-test@okla.com.do`          | `Test2026Seller!@#` | userId: `cd93c047-2185-47d5-9578-25b7f4bd31c8`                                                   |
| Dealer                                        | `dealer-test@okla.com.do`          | `Test2026Dealer!@#` | userId: `428c82f6-66d0-4294-868e-e01c3971fb3c`, dealerId: `9710694a-fb35-44cf-85c2-afb0bc0c4706` |
| Buyer (registrar uno nuevo)                   | `buyer-qa-{timestamp}@okla.com.do` | `Test2026Buyer!@#`  | —                                                                                                |
| Admin (verificar credenciales en K8s secrets) | —                                  | —                   | accountType: `admin`                                                                             |

### Vehículos de Test Activos

- **Seller** — Toyota Corolla 2024: vehicleId `616a181b-005d-45d8-8e79-b86b30971256`
- **Dealer** — Honda CR-V 2023: vehicleId `4b3186dc-3adf-4f59-9ad6-eb6df0b1686b`

---

## 🔧 BLOQUE 1: CORRECCIONES DE INFRAESTRUCTURA

### 1.1 — DealerManagementService: RabbitMQ AUTH_REFUSED

**Diagnóstico confirmado:**

- `RabbitMqAuditPublisher` (en `_Shared/CarDealer.Shared.Audit/Services/`) lee su config de la sección `Audit:RabbitMq:Username` y `Audit:RabbitMq:Password`
- El K8s secret `rabbitmq-secrets` expone `RabbitMQ__UserName = okla_admin` y `RabbitMQ__Password = ...`
- La sección `Audit:RabbitMq` se configura por separado y su default es `guest/guest` → `ACCESS_REFUSED`
- `AuditOptions` → `RabbitMqConfig.Username` defaults a `"guest"`, `Password` defaults a `"guest"`

**Archivos a revisar:**

- `backend/_Shared/CarDealer.Shared.Audit/Configuration/AuditOptions.cs`
- `backend/DealerManagementService/DealerManagementService.Api/Program.cs`
- `k8s/deployments.yaml` (env vars del deployment `dealermanagementservice`)
- `k8s/secrets.yaml` (si existe sección de dealer-management)

**Acciones requeridas:**

1. En `k8s/deployments.yaml`, en el deployment `dealermanagementservice`, agregar bajo `envFrom` o bajo `env` los mapeos:
   ```yaml
   - name: Audit__RabbitMq__Username
     valueFrom:
       secretKeyRef:
         name: rabbitmq-secrets
         key: RabbitMQ__UserName
   - name: Audit__RabbitMq__Password
     valueFrom:
       secretKeyRef:
         name: rabbitmq-secrets
         key: RabbitMQ__Password
   - name: Audit__RabbitMq__Host
     value: "rabbitmq"
   - name: Audit__RabbitMq__Port
     value: "5672"
   ```
2. Verificar si OTROS servicios activos también usan `AddAuditPublisher` sin estos env vars. Si sí, aplicar el mismo fix a todos.
3. Aplicar: `kubectl apply -f k8s/deployments.yaml -n okla`
4. Reiniciar: `kubectl rollout restart deployment/dealermanagementservice -n okla`
5. Verificar logs limpios: `kubectl logs deployment/dealermanagementservice -n okla --tail=30 | grep -i rabbit`
6. Verificar que no hay `ACCESS_REFUSED` ni `BrokerUnreachableException`
7. Hacer `kubectl rollout status deployment/dealermanagementservice -n okla`

**Criterio de éxito:** Log debe mostrar `"Audit publisher connected to RabbitMQ at rabbitmq:5672"` o equivalente. Cero errores de auth en RabbitMQ.

---

### 1.2 — ChatbotService: Segundo Pod en CrashLoopBackOff

**Diagnóstico confirmado:**

- `kubectl describe pod chatbotservice-78b89b4d96-8p79s -n okla` muestra `Startup probe failed: HTTP probe failed with statuscode: 503`
- Logs del pod fallando: Redis health check retorna `WRONGPASS invalid username-password pair or user is disabled`
- El pod ANTIGUO (`chatbotservice-845ff575db`) funciona (0 restarts, 12h uptime) porque su imagen es anterior a algún cambio en configuración de Redis
- Resultado: rolling update nunca completa → 2 pods, 1 failing

**Causa raíz**: El `/health` endpoint de ChatbotService incluye un `RedisHealthCheck` que falla con auth inválida, y como no está excluido por tags, hace que `/health` → 503. La startup probe falla → pod nunca Ready → rolling update bloqueado.

**Archivos a revisar:**

- `backend/ChatbotService/ChatbotService.Api/Program.cs` — cómo se configura `/health` endpoint y el health check de Redis
- `k8s/deployments.yaml` — deployment `chatbotservice`, sección de `redis-secrets` y probes
- K8s secret `redis-secrets` — qué password tiene vs qué usa ChatbotService

**Acciones requeridas — Opción A (fix de config Redis):**

1. Comparar el password en `redis-secrets`:
   ```bash
   kubectl get secret redis-secrets -n okla -o jsonpath='{.data}' | python3 -c "import sys,json,base64; d=json.load(sys.stdin); [print(k,'=',base64.b64decode(v).decode()) for k,v in sorted(d.items())]"
   ```
2. Comparar con la config actual que lee ChatbotService en su `Program.cs`
3. Si hay mismatch de keys (ej: ChatbotService busca `Redis__Password` pero el secret tiene `REDIS_PASSWORD`), corregir el mapeo en `k8s/deployments.yaml`

**Acciones requeridas — Opción B (fix health check — aplicar independientemente de A):**
En `backend/ChatbotService/ChatbotService.Api/Program.cs`, el health check de Redis DEBE tener el tag `"ready"` y NO estar expuesto en el endpoint `/health` sin filtro. Aplicar el patrón canónico:

```csharp
// En registro de health checks:
builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready", "external" })
    // ... otros checks
    ;

// En mapping de endpoints:
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("external") // ← excluir Redis/externos
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")     // ← Redis sí aquí
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false                                  // ← solo proceso vivo
});
```

**Acciones post-fix:**

1. Hacer commit y push → CI/CD build → nueva imagen
2. `kubectl rollout restart deployment/chatbotservice -n okla`
3. Verificar que solo 1 pod resulta en `1/1 Running, 0 restarts`
4. `kubectl rollout status deployment/chatbotservice -n okla` → debe reportar "successfully rolled out"
5. `kubectl get pods -n okla | grep chatbot` → exactamente 1 pod, 1/1 Running

**Criterio de éxito:** Un único pod de ChatbotService en `1/1 Running` con 0 restarts. Rolling update completado.

---

### 1.3 — DealerAnalyticsService: Build y Deploy de Imagen Docker

**Estado actual:** `replicas: 0 # DISABLED: no image in GHCR`. El código fuente existe en `backend/DealerAnalyticsService/` pero nunca se ha construido la imagen Docker.

**Archivos a revisar:**

- `backend/DealerAnalyticsService/` — estructura completa del servicio
- `backend/DealerAnalyticsService/Dockerfile` — verificar que existe y es correcto
- `.github/workflows/smart-cicd.yml` — variable `SERVICES` — verificar si `dealeranalyticsservice` está incluida
- `k8s/deployments.yaml` — línea con `replicas: 0 # DISABLED: no image in GHCR` para dealeranalyticsservice

**Acciones requeridas:**

1. Verificar estructura del servicio: `ls backend/DealerAnalyticsService/`
2. Verificar que `DealerAnalyticsService.sln` existe. Si no, crear:
   ```bash
   cd backend/DealerAnalyticsService
   dotnet new sln -n DealerAnalyticsService
   # Agregar todos los proyectos del servicio
   dotnet sln add DealerAnalyticsService.Api/DealerAnalyticsService.Api.csproj
   dotnet sln add DealerAnalyticsService.Application/DealerAnalyticsService.Application.csproj
   dotnet sln add DealerAnalyticsService.Domain/DealerAnalyticsService.Domain.csproj
   dotnet sln add DealerAnalyticsService.Infrastructure/DealerAnalyticsService.Infrastructure.csproj
   # Agregar shared libs
   dotnet sln add ../_Shared/CarDealer.Shared/CarDealer.Shared.csproj
   dotnet sln add ../_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj
   ```
3. Hacer `dotnet build` local para verificar que compila sin errores:
   ```bash
   cd backend/DealerAnalyticsService
   dotnet build DealerAnalyticsService.sln
   ```
4. Corregir TODOS los errores de compilación encontrados (CS errors, missing refs, etc.)
5. Verificar que `Dockerfile` existe y usa el patrón multi-stage correcto:

   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
   WORKDIR /src
   COPY . .
   RUN dotnet publish "DealerAnalyticsService.Api/DealerAnalyticsService.Api.csproj" -c Release -o /app/publish

   FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
   WORKDIR /app
   COPY --from=build /app/publish .
   EXPOSE 8080
   ENV ASPNETCORE_URLS=http://+:8080
   ENTRYPOINT ["dotnet", "DealerAnalyticsService.Api.dll"]
   ```

6. Verificar que `Program.cs` tiene los 3 health check endpoints (`/health`, `/health/ready`, `/health/live`) con el filtro de tags correcto
7. Agregar `dealeranalyticsservice` a la lista `SERVICES` en `.github/workflows/smart-cicd.yml` si no está
8. Cambiar `replicas: 0` a `replicas: 1` en `k8s/deployments.yaml` para dealeranalyticsservice
9. Crear K8s secret para dealeranalyticsservice si no existe:
   ```bash
   kubectl get secret dealeranalyticsservice-db-secret -n okla 2>/dev/null || \
   kubectl create secret generic dealeranalyticsservice-db-secret \
     --namespace=okla \
     --from-literal=ConnectionStrings__DefaultConnection="Host=okla-db-do-user-31493168-0.g.db.ondigitalocean.com;Port=25060;Database=dealeranalyticsservice;Username=doadmin;Password=REDACTED_AIVEN_PASSWORD;SslMode=Require" \
     --from-literal=Database__AutoMigrate=true
   ```
10. Hacer commit y push:
    ```
    fix(ci): add DealerAnalyticsService .sln, fix build, enable deployment
    ```
11. Esperar que CI/CD construya la imagen
12. Aplicar: `kubectl apply -f k8s/deployments.yaml -n okla`
13. Verificar: `kubectl get pods -n okla | grep dealeranalytics` → `1/1 Running`

**Criterio de éxito:** Imagen `ghcr.io/gregorymorenoiem/dealeranalyticsservice:latest` existe en GHCR. Pod corriendo `1/1 Running`. DB migrations aplicadas.

---

### 1.4 — Verificación Post-Fix de Infraestructura

Después de completar 1.1, 1.2, 1.3:

```bash
# Todos los pods activos deben estar 1/1 Running
kubectl get pods -n okla | grep -v "0/1\|0/0\|Pending\|Error"

# Health checks de los servicios afectados
kubectl port-forward svc/gateway 18443:8080 -n okla &
sleep 3

TOKEN=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@okla.com.do","password":"Test2026Dealer!@#"}' | \
  python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('accessToken','ERROR'))")

echo "Token: ${TOKEN:0:30}..."

# Health checks
curl -s http://localhost:18443/api/dealers/health | python3 -m json.tool
curl -s http://localhost:18443/api/chatbot/health | python3 -m json.tool

# Logs limpios
kubectl logs deployment/dealermanagementservice -n okla --tail=5 | grep -v "INF"
kubectl logs deployment/chatbotservice -n okla --tail=5 | grep -v "INF"
```

---

## 🎭 BLOQUE 2: AUDITORÍA COMPLETA DE FLUJOS DE USUARIO

> Para cada tipo de usuario, auditar Y corregir TODOS los problemas encontrados en UI, API, navegación, guards de autenticación, y flujos end-to-end. Si falta una página, créala. Si un componente está roto, corrígelo. Si una API no responde correctamente, depura. NO documentes los problemas — CORRÍGELOS.

### Archivos de referencia para la auditoría:

- `frontend/web-next/src/middleware.ts` — protección de rutas (server-side)
- `frontend/web-next/src/components/auth/auth-guard.tsx` — guard client-side
- `frontend/web-next/src/config/navigation.ts` — menús por tipo de usuario
- `frontend/web-next/src/hooks/use-auth.tsx` — contexto de autenticación

---

### 2.1 — Usuario No Autenticado (Guest)

**Rutas públicas que deben funcionar SIN login:**

| Ruta                    | Componente/Página    | Verificar                                                                    |
| ----------------------- | -------------------- | ---------------------------------------------------------------------------- |
| `/`                     | Homepage             | Vehículos destacados, buscador, CTA de registro                              |
| `/vehiculos`            | Listado              | Paginación, filtros, cards de vehículo                                       |
| `/vehiculos/[slug]`     | Detalle vehículo     | Galería, specs, precio, ReviewsSection, ChatWidget (Ana), mapa, seller-card  |
| `/buscar`               | Búsqueda avanzada    | Filtros completos (marca, modelo, año, precio, tipo, combustible)            |
| `/dealers`              | Lista de dealers     | Cards de dealers con rating y conteo de inventario                           |
| `/dealers/[slug]`       | Perfil dealer        | Info dealer, inventario, ReviewsSection, AppointmentCalendar, botón Chat Ana |
| `/comparar`             | Comparador           | Hasta 3 vehículos lado a lado                                                |
| `/ayuda`                | Centro de ayuda      | FAQs, categorías                                                             |
| `/contacto`             | Contacto             | Formulario funcional                                                         |
| `/nosotros`             | Sobre OKLA           | —                                                                            |
| `/terminos`             | Términos             | —                                                                            |
| `/privacidad`           | Privacidad           | —                                                                            |
| `/vender`               | Landing vendedor     | CTA, features, testimonios                                                   |
| `/login`                | Login                | Form, OAuth (Google), "Olvidé contraseña", link a registro                   |
| `/registro`             | Registro             | Form completo (firstName, lastName, email, phone, password, acceptTerms)     |
| `/recuperar-contrasena` | Recuperar contraseña | Form de email                                                                |
| `/verificar-email`      | Verificar email      | Input de código                                                              |

**Flujos a probar para Guest:**

**F-G1: Flujo Registro → Verificación → Login**

```
GET / → Clic en "Registrarse"
→ GET /registro → Completar form → POST /api/auth/register
→ Redirect a /verificar-email
→ Ingresar código de email → POST /api/auth/verify-email
→ Redirect a /cuenta (authenticated)
```

**F-G2: Flujo de Búsqueda y Detalle**

```
GET /buscar?marca=toyota&tipo=sedan&precioMax=2000000
→ Resultados paginados visibles
→ Clic en vehículo → GET /vehiculos/[slug]
→ Ver galería de fotos (carousel)
→ Ver ReviewsSection (carga lazy, empty state si no hay reviews)
→ ChatWidget Ana disponible (sin login = debe redirigir a login al intentar chatear)
→ Clic en "Contactar vendedor/dealer" → Redirect a /login?callbackUrl=/vehiculos/[slug]
```

**F-G3: Flujo Comparar**

```
GET /vehiculos → Agregar vehículo A al comparador (botón "Comparar")
→ Agregar vehículo B al comparador
→ GET /comparar → Ver tabla comparativa con specs side-by-side
```

**F-G4: Intentar acceder a rutas protegidas**

```
GET /cuenta → Redirect a /login?callbackUrl=/cuenta ✓
GET /dealer → Redirect a /login ✓
GET /admin → Redirect a /login ✓
GET /publicar → Redirect a /login ✓
```

**Validaciones de UI para Guest:**

- [ ] Navbar muestra "Iniciar sesión" y "Registrarse" (NO perfil de usuario)
- [ ] ChatWidget en /vehiculos/[slug] muestra prompt de login para chatear
- [ ] Botón "Agendar cita" en /dealers/[slug] muestra modal de login o redirect
- [ ] Filtros de búsqueda funcionan sin login
- [ ] Comparador persiste selección en localStorage (no requiere login)
- [ ] Formulario de contacto (ContactService) funciona sin login

---

### 2.2 — Comprador (Buyer) — accountType: 'buyer'

**Rutas de cuenta (requieren auth, cualquier tipo):**

| Ruta                        | Verificar                                                  | Dependencia API                          |
| --------------------------- | ---------------------------------------------------------- | ---------------------------------------- |
| `/cuenta`                   | Dashboard: favoritos recientes, alertas activas, historial | UserService                              |
| `/cuenta/perfil`            | Editar nombre, foto, teléfono                              | UserService                              |
| `/cuenta/verificacion`      | Flujo KYC (cédula + liveness)                              | KYCService                               |
| `/cuenta/favoritos`         | Lista de vehículos guardados, opción de eliminar           | VehiclesSaleService                      |
| `/cuenta/busquedas`         | Búsquedas guardadas, re-ejecutar                           | VehiclesSaleService                      |
| `/cuenta/alertas`           | Alertas de precio activas, crear, eliminar                 | AlertService (0/0) o VehiclesSaleService |
| `/cuenta/mensajes`          | Redirect a /mensajes                                       | —                                        |
| `/cuenta/notificaciones`    | Notificaciones recibidas                                   | NotificationService                      |
| `/cuenta/historial`         | Historial de pagos/transacciones                           | BillingService                           |
| `/cuenta/seguridad`         | Cambiar contraseña, 2FA, sesiones activas                  | AuthService                              |
| `/cuenta/configuracion`     | Preferencias: idioma, moneda, notificaciones               | UserService                              |
| `/cuenta/convert-to-seller` | Formulario upgrade a vendedor ($29/listing)                | BillingService                           |

**Flujos a probar para Buyer:**

**F-B1: Flujo KYC Completo**

```
Login como buyer → GET /cuenta → "Verificar identidad" → GET /cuenta/verificacion
→ Step 1: Upload frente de cédula (camera/file)
→ Step 2: Upload reverso de cédula
→ Step 3: Prueba de vida (blink, smile, turn)
→ POST /api/kyc/profiles + POST /api/kyc/profiles/{id}/documents + POST /api/kyc/profiles/{id}/liveness
→ POST /api/kyc/profiles/{id}/submit
→ Estado "Pendiente de revisión"
→ Verificar estado: GET /api/kyc/profiles/user/{userId}
```

**F-B2: Flujo Favoritos**

```
GET /vehiculos → Clic en ♡ de un vehículo (star/heart icon)
→ POST /api/vehicles/{id}/favorite
→ Ir a /cuenta/favoritos → Vehículo aparece en lista
→ Clic en ♡ para quitar → DELETE /api/vehicles/{id}/favorite
→ Vehículo desaparece de /cuenta/favoritos
```

**F-B3: Flujo Mensajes**

```
GET /vehiculos/[slug] → Clic en "Contactar" (seller individual)
→ Redirect o modal de mensajes
→ Enviar mensaje
→ GET /mensajes → Conversación visible
→ Respuesta del vendedor visible
```

**F-B4: Flujo Convert to Seller**

```
GET /cuenta/convert-to-seller
→ Información del plan ($29/listing)
→ Formulario de datos adicionales (si aplica)
→ Redirect a checkout / pago
→ Después del pago: accountType cambia a 'seller'
→ Menú de navegación actualiza a SELLER_NAVIGATION
```

**Validaciones de UI para Buyer:**

- [ ] Sidebar en /cuenta muestra BUYER_NAVIGATION (no dealer ni seller items)
- [ ] Badge en sidebar: "Comprador" (púrpura)
- [ ] `/dealer` redirige a /403 o /login (no accessible)
- [ ] `/admin` redirige a /403 o /login (no accessible)
- [ ] Empty states correctos en /cuenta/favoritos, /cuenta/alertas si no hay items

---

### 2.3 — Vendedor Individual (Seller) — accountType: 'seller'

**Rutas adicionales del seller (además de todas las del buyer):**

| Ruta                    | Verificar                                               | Dependencia API     |
| ----------------------- | ------------------------------------------------------- | ------------------- |
| `/cuenta/mis-vehiculos` | Lista de publicaciones, estado (Activo/Draft/Archivado) | VehiclesSaleService |
| `/cuenta/estadisticas`  | Vistas, consultas, favoritos por vehículo               | VehiclesSaleService |
| `/cuenta/consultas`     | Inquiries/mensajes recibidos sobre sus vehículos        | —                   |
| `/cuenta/pagos`         | Pagos realizados (listings, upgrades)                   | BillingService      |
| `/publicar`             | Step 1: Datos del vehículo                              | VehiclesSaleService |
| `/publicar/fotos`       | Step 2: Upload fotos (drag & drop, reorder)             | MediaService        |
| `/publicar/preview`     | Step 3: Vista previa + publicar                         | VehiclesSaleService |
| `/vender`               | Landing con CTA para publicar                           | —                   |
| `/vender/dashboard`     | Mini-dashboard del vendedor                             | VehiclesSaleService |
| `/vender/publicar`      | Redirect a /publicar                                    | —                   |
| `/vender/leads`         | Consultas recibidas                                     | —                   |

**Flujos a probar para Seller:**

**F-S1: Flujo Publicar Vehículo Completo**

```
Login como seller-test → GET /publicar
→ Step 1: Marca=Toyota, Modelo=Camry, Año=2023, Precio=1800000, Km=25000,
           Combustible=Gasolina, Transmisión=Automática, Tipo=Sedán,
           Descripción=... → Guardar borrador
→ GET /publicar/fotos → Upload mínimo 3 fotos
→ GET /publicar/preview → Verificar vista previa correcta
→ "Publicar" → POST /api/vehicles → vehicleId creado
→ Redirect a /cuenta/mis-vehiculos → Nuevo vehículo aparece como "Activo"
→ GET /vehiculos/[nuevo-slug] → Página de detalle visible públicamente
```

**F-S2: Flujo Gestión de Publicación**

```
GET /cuenta/mis-vehiculos → Ver vehículo existente (seller vehicle)
→ Clic "Editar" → Formulario con datos pre-cargados
→ Modificar precio → PUT /api/vehicles/{id}
→ Guardar → Precio actualizado en listado público
→ Clic "Archivar" → Estado cambia a Archivado
→ Vehículo NO aparece en /vehiculos (búsqueda pública)
→ Clic "Reactivar" → Estado vuelve a Activo
```

**F-S3: Flujo Recibir y Responder Review**

```
(Como buyer-qa) POST /api/reviews → Crear review del vehículo de seller-test
→ Login como seller-test → /cuenta o /vehiculos/[slug]
→ Review visible en ReviewsSection del vehículo
→ Clic "Responder" → POST /api/reviews/{id}/responses
→ Respuesta visible bajo la review
```

**Validaciones de UI para Seller:**

- [ ] Sidebar en /cuenta muestra SELLER_NAVIGATION (mis-vehiculos, estadísticas, consultas, pagos)
- [ ] Badge: "Vendedor" (verde)
- [ ] Botón "+ Publicar" siempre visible en header/sidebar
- [ ] `/dealer` redirige a /403 (seller no puede acceder al portal dealer)
- [ ] `/publicar` accesible y completo (3 steps)

---

### 2.4 — Dealer — accountType: 'dealer' / 'dealer_employee'

**Portal Dealer — todas las rutas bajo `/dealer/`:**

| Ruta                          | Verificar                                                           | Dependencia API                                      |
| ----------------------------- | ------------------------------------------------------------------- | ---------------------------------------------------- |
| `/dealer`                     | Dashboard: KPIs (inventario, leads, visitas, revenue)               | DealerManagementService, DealerAnalyticsService      |
| `/dealer/inventario`          | Lista de vehículos del dealer, filtros, búsqueda                    | VehiclesSaleService                                  |
| `/dealer/inventario/nuevo`    | Formulario agregar vehículo (mismo que /publicar pero con dealerId) | VehiclesSaleService                                  |
| `/dealer/inventario/[id]`     | Editar vehículo específico                                          | VehiclesSaleService                                  |
| `/dealer/inventario/importar` | CSV/Excel bulk import                                               | InventoryManagementService (0/0) → fallback graceful |
| `/dealer/leads`               | Lista de leads/inquiries recibidos                                  | —                                                    |
| `/dealer/leads/[id]`          | Detalle de lead con historial de comunicación                       | —                                                    |
| `/dealer/analytics`           | Gráficas: vistas, conversiones, leads por vehículo/período          | DealerAnalyticsService                               |
| `/dealer/citas`               | Lista de citas/appointments                                         | AppointmentService ✓                                 |
| `/dealer/citas/calendario`    | Vista calendario de citas                                           | AppointmentService ✓                                 |
| `/dealer/mensajes`            | Bandeja de mensajes del dealer                                      | —                                                    |
| `/dealer/empleados`           | Gestión de empleados, invitaciones                                  | DealerManagementService ✓                            |
| `/dealer/ubicaciones`         | Sucursales del dealer                                               | DealerManagementService ✓                            |
| `/dealer/pricing`             | Pricing IA (VehicleIntelligenceService — 0/0) → fallback graceful   | —                                                    |
| `/dealer/reportes`            | Reportes exportables (PDF/CSV)                                      | —                                                    |
| `/dealer/perfil`              | Perfil público del dealer (nombre, descripción, logo)               | DealerManagementService ✓                            |
| `/dealer/documentos`          | Documentos de verificación (RNC, etc.)                              | DealerManagementService ✓                            |
| `/dealer/facturacion`         | Facturas y cobros                                                   | BillingService ✓                                     |
| `/dealer/suscripcion`         | Plan actual, upgrade/downgrade                                      | BillingService ✓                                     |
| `/dealer/configuracion`       | Ajustes del portal dealer                                           | —                                                    |

**Flujos a probar para Dealer:**

**F-D1: Flujo Gestión de Inventario**

```
Login como dealer-test → GET /dealer → Dashboard visible
→ GET /dealer/inventario → Lista de vehículos (debe incluir Honda CR-V 2023)
→ Clic "+ Nuevo vehículo" → GET /dealer/inventario/nuevo
→ Rellenar: Marca=Nissan, Modelo=Rogue, Año=2024, Precio=2500000
→ POST /api/vehicles (con dealerId incluido) → Creado con estado Draft
→ Activar vehículo → GET /dealer/inventario → Nissan Rogue aparece como Activo
→ GET /vehiculos (público) → Nissan Rogue visible
```

**F-D2: Flujo Citas/Appointments**

```
GET /dealer/citas → Lista de citas (empty state si no hay)
GET /dealer/citas/calendario → Vista de calendario
→ (Como guest/buyer en otro browser) GET /dealers/[dealer-slug]
→ AppointmentCalendar → Seleccionar fecha → Seleccionar hora → Datos → Confirmar
→ POST /api/appointments → Cita creada
→ Volver al browser del dealer → /dealer/citas → Nueva cita visible
```

**F-D3: Flujo Perfil Dealer (DealerManagementService)**

```
GET /dealer/perfil → Formulario de perfil
→ Si dealer no registrado en DealerManagementService: POST /api/dealers (crear perfil)
→ Si dealer ya existe: PUT /api/dealers/{dealerId}
→ Actualizar nombre, descripción, logo, horario
→ GET /dealers/[dealer-slug] (público) → Cambios reflejados
```

**F-D4: Flujo Reviews del Dealer**

```
GET /dealers/[dealer-slug] (público) → ReviewsSection del dealer
→ Stars summary bar visible
→ (Como buyer) Clic "Escribir reseña" → POST /api/reviews (targetType: dealer)
→ (Como dealer) Review visible en /dealers/[dealer-slug]
```

**F-D5: Flujo DealerAnalyticsService (después de fix 1.3)**

```
GET /dealer/analytics → Gráficas cargando desde DealerAnalyticsService
→ Verificar: vistas de inventario, leads por período, conversión
→ GET /api/dealer-analytics/dashboard/{dealerId} → 200 OK con datos
```

**Validaciones de UI para Dealer:**

- [ ] `/dealer` accesible para `dealer`, `dealer_employee`, `admin` — redirect /403 para otros
- [ ] Sidebar del portal dealer muestra todas las secciones
- [ ] Middleware verifica `dealerId` en token para rutas de inventario/analytics/leads
- [ ] Si dealer no tiene perfil en DealerManagementService, redirect a `/dealer/perfil` para completar registro
- [ ] Graceful fallback en `/dealer/inventario/importar` si InventoryManagementService está 0/0
- [ ] Graceful fallback en `/dealer/pricing` si VehicleIntelligenceService está 0/0

---

### 2.5 — Administrador de Plataforma — accountType: 'admin' / 'platform_employee'

**Panel Admin — todas las rutas bajo `/admin/`:**

| Ruta                   | Verificar                                                        | Dependencia API                    |
| ---------------------- | ---------------------------------------------------------------- | ---------------------------------- |
| `/admin`               | Dashboard: KPIs globales (usuarios, vehículos, dealers, revenue) | Multiple services                  |
| `/admin/usuarios`      | Lista de usuarios, filtros, buscar por email/nombre              | UserService                        |
| `/admin/usuarios/[id]` | Perfil usuario, historial, cambiar accountType, suspender        | UserService, AuthService           |
| `/admin/vehiculos`     | Lista de vehículos, moderación (aprobar/rechazar/suspend)        | VehiclesSaleService                |
| `/admin/dealers`       | Lista de dealers, estado de verificación                         | DealerManagementService            |
| `/admin/dealers/[id]`  | Perfil dealer admin view, documentos, aprobar/rechazar           | DealerManagementService            |
| `/admin/reviews`       | Listado de reviews, aprobar/rechazar/marcar spam                 | ReviewService ✓                    |
| `/admin/reportes`      | Reportes de usuarios reportando contenido                        | —                                  |
| `/admin/kyc`           | Queue de verificaciones KYC pendientes                           | KYCService ✓                       |
| `/admin/kyc/[id]`      | Revisar documentos + liveness de un usuario                      | KYCService ✓                       |
| `/admin/facturacion`   | Transacciones, reembolsos, disputas                              | BillingService                     |
| `/admin/analytics`     | Métricas de plataforma (DAU, MAU, conversión)                    | —                                  |
| `/admin/contenido`     | Moderación de contenido, banners, SEO                            | —                                  |
| `/admin/mensajes`      | Mensajes de soporte                                              | —                                  |
| `/admin/equipo`        | Gestión del equipo de plataforma                                 | —                                  |
| `/admin/roles`         | RBAC — roles y permisos                                          | RoleService                        |
| `/admin/configuracion` | Config de plataforma: maintenance, features flags                | MaintenanceService                 |
| `/admin/logs`          | Logs del sistema, audit trail                                    | AuditService ✓                     |
| `/admin/mantenimiento` | Toggle maintenance mode, mensaje customizado                     | MaintenanceService                 |
| `/admin/promociones`   | Crear/editar promociones activas                                 | —                                  |
| `/admin/banners`       | Banners del homepage                                             | —                                  |
| `/admin/early-bird`    | Gestión del programa early bird                                  | —                                  |
| `/admin/compliance`    | Reportes de compliance, alertas regulatorias                     | ComplianceService (0/0) → fallback |
| `/admin/suscripciones` | Planes de suscripción, configuración                             | BillingService                     |
| `/admin/transacciones` | Historial completo de transacciones                              | BillingService                     |

**Flujos a probar para Admin:**

**F-A1: Flujo Moderación de Vehículos**

```
Login como admin → GET /admin/vehiculos
→ Ver vehículo con estado "Pendiente aprobación"
→ Clic "Revisar" → Ver detalles, fotos, descripción
→ Clic "Aprobar" → PUT /api/vehicles/{id}/approve
→ Vehículo aparece en búsqueda pública
→ Clic "Rechazar" → Modal con razón → PUT /api/vehicles/{id}/reject
→ Notificación enviada al vendedor (NotificationService)
```

**F-A2: Flujo KYC Review**

```
GET /admin/kyc → Lista de verificaciones pendientes
→ Clic en una verificación → GET /admin/kyc/[id]
→ Ver foto de cédula (frente + reverso), selfie liveness
→ POST /api/kyc/profiles/{id}/approve
→ Estado del usuario actualizado a KYC verified
→ Notificación enviada al usuario
```

**F-A3: Flujo Moderación Reviews**

```
GET /admin/reviews → Lista de reviews (aprobadas, pendientes, reportadas)
→ Clic en review reportada → Ver contenido completo
→ "Aprobar" → PUT /api/reviews/{id}/approve
→ "Rechazar como spam" → DELETE /api/reviews/{id} o PUT status=rejected
→ Review desaparece del frontend público
```

**F-A4: Flujo Gestión de Usuarios**

```
GET /admin/usuarios → Buscar "dealer-test@okla.com.do"
→ Ver perfil: accountType=dealer, KYC status, listados, historial
→ Clic "Suspender cuenta" → POST /api/auth/users/{id}/suspend
→ Usuario intenta login → 401/403 "Cuenta suspendida"
→ Admin "Reactivar cuenta" → Usuario puede volver a loguearse
```

**F-A5: Flujo Mantenimiento**

```
GET /admin/configuracion o /admin/mantenimiento
→ Toggle "Activar modo mantenimiento" + mensaje personalizado
→ PUT /api/maintenance/enable
→ GET / (cualquier ruta no-admin) → Redirect a /mantenimiento con mensaje
→ Admin sigue teniendo acceso normal
→ Toggle "Desactivar" → Plataforma vuelve a normal
```

**F-A6: Flujo Audit Logs**

```
GET /admin/logs
→ Filtrar por usuario, por tipo de acción, por fecha
→ GET /api/audit/logs?userId={id}&from={date}&to={date}
→ Logs de las acciones F-A1, F-A2, F-A3 aparecen con timestamp y detalles
```

**Validaciones de UI para Admin:**

- [ ] `/admin` solo accesible con accountType `admin` o `platform_employee`
- [ ] `/cuenta` y `/dealer` también accesibles para admins (cross-navigation)
- [ ] Todas las acciones destructivas (suspender, rechazar) tienen confirmación modal
- [ ] Loading states en todas las tablas
- [ ] Paginación funcional en todas las listas
- [ ] Empty states correctos

---

## 🧪 BLOQUE 3: QA AUTOMATIZADO — PRUEBAS DE TODOS LOS FLUJOS

> Ejecutar estas pruebas en ORDEN. Para cada prueba: si falla → diagnosticar → corregir → re-ejecutar hasta pasar. NO avanzar si hay fallos críticos.

### 3.1 — Setup del Entorno de Pruebas

```bash
# 1. Port-forward activo
kubectl port-forward svc/gateway 18443:8080 -n okla &
GATEWAY_PID=$!
sleep 3
echo "Gateway PID: $GATEWAY_PID"

# 2. Obtener tokens de prueba
export TOKEN_BUYER=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"buyer-qa@okla.com.do","password":"Test2026Buyer!@#"}' | \
  python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('accessToken','FAILED'))" 2>/dev/null)

export TOKEN_SELLER=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"Test2026Seller!@#"}' | \
  python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('accessToken','FAILED'))")

export TOKEN_DEALER=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@okla.com.do","password":"Test2026Dealer!@#"}' | \
  python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('accessToken','FAILED'))")

echo "Seller: ${TOKEN_SELLER:0:20}..."
echo "Dealer: ${TOKEN_DEALER:0:20}..."
```

### 3.2 — Suite de Pruebas de API

#### 3.2.1 — Auth y Usuarios

```bash
BASE="http://localhost:18443"

# T-001: Login válido retorna accessToken
R=$(curl -s -o /dev/null -w "%{http_code}" -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"Test2026Seller!@#"}')
[ "$R" == "200" ] && echo "✅ T-001: Login" || echo "❌ T-001: Login ($R)"

# T-002: Login inválido retorna 401
R=$(curl -s -o /dev/null -w "%{http_code}" -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"seller-test@okla.com.do","password":"wrongpassword"}')
[ "$R" == "401" ] && echo "✅ T-002: Login inválido" || echo "❌ T-002: Login inválido ($R)"

# T-003: GET /api/auth/me con token válido retorna usuario
R=$(curl -s -o /dev/null -w "%{http_code}" $BASE/api/auth/me \
  -H "Authorization: Bearer $TOKEN_SELLER")
[ "$R" == "200" ] && echo "✅ T-003: GET /me" || echo "❌ T-003: GET /me ($R)"

# T-004: GET /api/auth/me sin token retorna 401
R=$(curl -s -o /dev/null -w "%{http_code}" $BASE/api/auth/me)
[ "$R" == "401" ] && echo "✅ T-004: GET /me sin auth" || echo "❌ T-004: GET /me sin auth ($R)"
```

#### 3.2.2 — Vehículos (VehiclesSaleService)

```bash
# T-010: GET /api/vehicles retorna lista pública (sin auth)
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/vehicles?pageSize=5")
[ "$R" == "200" ] && echo "✅ T-010: GET /api/vehicles público" || echo "❌ T-010 ($R)"

# T-011: GET /api/vehicles/{id} retorna detalle
VEHICLE_ID="4b3186dc-3adf-4f59-9ad6-eb6df0b1686b"  # Honda CR-V dealer
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/vehicles/$VEHICLE_ID")
[ "$R" == "200" ] && echo "✅ T-011: GET /api/vehicles/{id}" || echo "❌ T-011 ($R)"

# T-012: POST /api/vehicles requiere auth
R=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/vehicles" \
  -H "Content-Type: application/json" -d '{}')
[ "$R" == "401" ] && echo "✅ T-012: POST vehiculo sin auth" || echo "❌ T-012 ($R)"

# T-013: POST /api/vehicles con auth seller crea vehículo (borrador)
R=$(curl -s -w "\n%{http_code}" -X POST "$BASE/api/vehicles" \
  -H "Authorization: Bearer $TOKEN_SELLER" \
  -H "Content-Type: application/json" \
  -d '{
    "make": "Honda", "model": "Civic", "year": 2023,
    "price": 1500000, "currency": "DOP", "mileage": 15000,
    "fuelType": "Gasoline", "transmission": "Automatic",
    "bodyType": "Sedan", "condition": "Used",
    "description": "QA Test vehicle - safe to delete",
    "status": "Draft"
  }')
CODE=$(echo "$R" | tail -1)
BODY=$(echo "$R" | head -1)
[ "$CODE" == "201" ] || [ "$CODE" == "200" ] && echo "✅ T-013: POST vehiculo" || echo "❌ T-013 ($CODE): $BODY"
```

#### 3.2.3 — Reviews (ReviewService)

```bash
SELLER_ID="cd93c047-2185-47d5-9578-25b7f4bd31c8"
VEHICLE_ID="616a181b-005d-45d8-8e79-b86b30971256"

# T-020: GET reviews de seller (público)
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/reviews/seller/$SELLER_ID")
[ "$R" == "200" ] && echo "✅ T-020: GET reviews seller" || echo "❌ T-020 ($R)"

# T-021: POST review requiere auth
R=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/reviews" \
  -H "Content-Type: application/json" -d '{}')
[ "$R" == "401" ] && echo "✅ T-021: POST review sin auth" || echo "❌ T-021 ($R)"

# T-022: POST review con auth buyer
R=$(curl -s -w "\n%{http_code}" -X POST "$BASE/api/reviews" \
  -H "Authorization: Bearer $TOKEN_BUYER" \
  -H "Content-Type: application/json" \
  -d "{
    \"targetId\": \"$SELLER_ID\",
    \"targetType\": \"seller\",
    \"vehicleId\": \"$VEHICLE_ID\",
    \"overallRating\": 5,
    \"title\": \"QA Test Review\",
    \"comment\": \"Excellent service. This is a QA test review.\",
    \"wouldRecommend\": true
  }")
CODE=$(echo "$R" | tail -1)
[ "$CODE" == "201" ] || [ "$CODE" == "200" ] && echo "✅ T-022: POST review" || echo "❌ T-022 ($CODE)"

# T-023: GET review summary
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/reviews/summary/$SELLER_ID")
[ "$R" == "200" ] || [ "$R" == "404" ] && echo "✅ T-023: GET review summary ($R)" || echo "❌ T-023 ($R)"
```

#### 3.2.4 — Appointments (AppointmentService)

```bash
DEALER_ID="9710694a-fb35-44cf-85c2-afb0bc0c4706"

# T-030: GET appointments del dealer (requiere auth)
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/appointments/dealer/$DEALER_ID" \
  -H "Authorization: Bearer $TOKEN_DEALER")
[ "$R" == "200" ] && echo "✅ T-030: GET appointments dealer" || echo "❌ T-030 ($R)"

# T-031: GET timeslots activos
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/timeslots/active" \
  -H "Authorization: Bearer $TOKEN_DEALER")
[ "$R" == "200" ] && echo "✅ T-031: GET timeslots activos" || echo "❌ T-031 ($R)"

# T-032: POST appointment (crear cita como buyer)
TOMORROW=$(python3 -c "from datetime import date, timedelta; d=date.today()+timedelta(1); print(d.isoformat())")
R=$(curl -s -w "\n%{http_code}" -X POST "$BASE/api/appointments" \
  -H "Authorization: Bearer $TOKEN_BUYER" \
  -H "Content-Type: application/json" \
  -d "{
    \"dealerId\": \"$DEALER_ID\",
    \"vehicleId\": \"4b3186dc-3adf-4f59-9ad6-eb6df0b1686b\",
    \"appointmentDate\": \"${TOMORROW}T10:00:00\",
    \"type\": \"TestDrive\",
    \"notes\": \"QA Test appointment\"
  }")
CODE=$(echo "$R" | tail -1)
[ "$CODE" == "201" ] || [ "$CODE" == "200" ] && echo "✅ T-032: POST appointment" || echo "❌ T-032 ($CODE)"
```

#### 3.2.5 — Dealer Management (DealerManagementService)

```bash
DEALER_ID="9710694a-fb35-44cf-85c2-afb0bc0c4706"

# T-040: GET dealer profile (público)
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/dealers/$DEALER_ID")
[ "$R" == "200" ] || [ "$R" == "404" ] && echo "✅ T-040: GET dealer profile ($R)" || echo "❌ T-040 ($R)"

# T-041: GET dealer analytics (requiere auth dealer)
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/dealers/$DEALER_ID/analytics" \
  -H "Authorization: Bearer $TOKEN_DEALER")
[ "$R" == "200" ] || [ "$R" == "404" ] && echo "✅ T-041: GET dealer analytics ($R)" || echo "❌ T-041 ($R)"

# T-042: POST dealer (crear perfil si no existe)
R=$(curl -s -w "\n%{http_code}" -X POST "$BASE/api/dealers" \
  -H "Authorization: Bearer $TOKEN_DEALER" \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"QA Motors Test Dealer\",
    \"rnc\": \"123456789\",
    \"phone\": \"8091234567\",
    \"email\": \"dealer-test@okla.com.do\",
    \"address\": \"Av. Principal 123, Santo Domingo\"
  }")
CODE=$(echo "$R" | tail -1)
[ "$CODE" == "201" ] || [ "$CODE" == "200" ] || [ "$CODE" == "409" ] && \
  echo "✅ T-042: POST dealer profile ($CODE)" || echo "❌ T-042 ($CODE)"
```

#### 3.2.6 — KYC (KYCService)

```bash
# T-050: GET KYC status del buyer
BUYER_ID="" # Obtener del token
BUYER_ID=$(curl -s $BASE/api/auth/me -H "Authorization: Bearer $TOKEN_BUYER" | \
  python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('data',{}).get('id',''))")

R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/kyc/profiles/user/$BUYER_ID" \
  -H "Authorization: Bearer $TOKEN_BUYER")
[ "$R" == "200" ] || [ "$R" == "404" ] && echo "✅ T-050: GET KYC status ($R)" || echo "❌ T-050 ($R)"

# T-051: POST KYC profile (iniciar verificación)
R=$(curl -s -w "\n%{http_code}" -X POST "$BASE/api/kyc/profiles" \
  -H "Authorization: Bearer $TOKEN_BUYER" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$BUYER_ID\", \"documentType\": \"NationalId\"}")
CODE=$(echo "$R" | tail -1)
[ "$CODE" == "201" ] || [ "$CODE" == "200" ] || [ "$CODE" == "409" ] && \
  echo "✅ T-051: POST KYC profile ($CODE)" || echo "❌ T-051 ($CODE)"
```

#### 3.2.7 — Audit Trail

```bash
# T-060: Verificar que acciones previas generaron audit logs
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/audit/logs?pageSize=10" \
  -H "Authorization: Bearer $TOKEN_DEALER")
[ "$R" == "200" ] || [ "$R" == "403" ] && echo "✅ T-060: GET audit logs ($R)" || echo "❌ T-060 ($R)"
```

#### 3.2.8 — Health Checks de Todos los Servicios Activos

```bash
SERVICES="authservice userservice roleservice vehiclessaleservice mediaservice \
          notificationservice billingservice errorservice kycservice auditservice \
          chatbotservice reviewservice appointmentservice dealermanagementservice \
          dealeranalyticsservice"

for SVC in $SERVICES; do
  R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/$SVC/health" 2>/dev/null || \
      curl -s -o /dev/null -w "%{http_code}" "http://localhost:18443/health" 2>/dev/null)
  # Health checks vía K8s port-forward al servicio directamente
  R=$(kubectl exec deployment/$SVC -n okla -- \
      wget -qO- http://localhost:8080/health 2>/dev/null | \
      python3 -c "import sys,json; d=json.load(sys.stdin); print('OK' if d.get('status')=='Healthy' else 'DEGRADED')" 2>/dev/null || echo "SKIP")
  [ "$R" == "OK" ] && echo "✅ HEALTH $SVC" || \
  [ "$R" == "SKIP" ] && echo "⚪ HEALTH $SVC (no exec)" || \
  echo "⚠️  HEALTH $SVC: $R"
done
```

### 3.3 — Pruebas de Permisos y Control de Acceso

```bash
# T-100: Buyer NO puede acceder a rutas de dealer
R=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/dealers/$DEALER_ID/employees" \
  -H "Authorization: Bearer $TOKEN_BUYER")
[ "$R" == "403" ] || [ "$R" == "401" ] && echo "✅ T-100: Buyer bloqueado de dealer APIs ($R)" || echo "❌ T-100 ($R)"

# T-101: Seller NO puede modificar vehículo de otro usuario
OTHER_VEHICLE="4b3186dc-3adf-4f59-9ad6-eb6df0b1686b"  # dealer's vehicle
R=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE/api/vehicles/$OTHER_VEHICLE" \
  -H "Authorization: Bearer $TOKEN_SELLER" \
  -H "Content-Type: application/json" \
  -d '{"price": 1}')
[ "$R" == "403" ] || [ "$R" == "401" ] && echo "✅ T-101: Seller bloqueado de vehículo ajeno ($R)" || echo "❌ T-101 ($R)"

# T-102: Dealer puede modificar SU vehículo
OWN_VEHICLE="4b3186dc-3adf-4f59-9ad6-eb6df0b1686b"  # dealer's own vehicle
R=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE/api/vehicles/$OWN_VEHICLE" \
  -H "Authorization: Bearer $TOKEN_DEALER" \
  -H "Content-Type: application/json" \
  -d '{"description": "QA updated description"}')
[ "$R" == "200" ] || [ "$R" == "204" ] && echo "✅ T-102: Dealer modifica su vehículo ($R)" || echo "❌ T-102 ($R)"
```

### 3.4 — Pruebas de UI Críticas (Manual + Verificación Visual)

Ejecutar en el browser de desarrollo (`http://localhost:3000`) o en producción (`https://okla.com.do`):

```
CHECKLIST DE UI:

□ Guest
  □ Homepage carga con vehículos destacados en menos de 3 segundos
  □ Búsqueda con filtros retorna resultados correctos
  □ /vehiculos/[slug] carga ReviewsSection (puede ser empty state)
  □ ChatWidget Ana visible y abre con clic (pide login si es guest)
  □ /dealers/[slug] muestra AppointmentCalendar (pide login al intentar agendar)
  □ Comparador funciona con 2-3 vehículos
  □ Registro con datos válidos → redirección a /verificar-email
  □ Login con credenciales válidas → redirección a /cuenta

□ Buyer (login con buyer-qa@okla.com.do)
  □ Dashboard /cuenta muestra sección correcta (BUYER_NAVIGATION)
  □ Badge "Comprador" visible en sidebar
  □ /cuenta/favoritos funciona (add/remove desde /vehiculos)
  □ /cuenta/verificacion inicia flujo KYC
  □ /dealer → /403 (no acceso)
  □ /admin → /403 o /login

□ Seller (login con seller-test@okla.com.do)
  □ Dashboard /cuenta muestra SELLER_NAVIGATION
  □ /cuenta/mis-vehiculos lista los vehículos publicados
  □ /publicar → 3 pasos completos, sin errores de TypeScript/React
  □ /publicar/fotos → upload de imágenes funciona (MediaService)
  □ Nuevo vehículo aparece en /vehiculos después de publicar

□ Dealer (login con dealer-test@okla.com.do)
  □ /dealer carga con KPIs del dashboard
  □ /dealer/inventario lista vehículos del dealer
  □ /dealer/citas muestra AppointmentCalendar del dealer
  □ /dealer/mensajes accesible
  □ /dealer/analytics carga (con DealerAnalyticsService corriendo)
  □ /cuenta/perfil también accesible para dealer (cross-navigation)

□ Admin (login con cuenta admin)
  □ /admin carga con métricas globales
  □ /admin/reviews muestra lista de reviews con opciones de moderación
  □ /admin/kyc muestra queue de verificaciones
  □ /admin/dealers muestra lista de dealers
  □ /admin/logs muestra audit trail con los eventos de las pruebas anteriores
  □ /cuenta también accesible para admin
  □ /dealer también accesible para admin (cross-navigation total)
```

---

## 🔍 BLOQUE 4: AUDIT DE INCONSISTENCIAS CONOCIDAS

> Para cada ítem: analizar → diagnosticar → corregir → verificar.

### 4.1 — Middleware vs Navegación — Verificar Alineación

Comparar `frontend/web-next/src/middleware.ts` (rutas protegidas) con `frontend/web-next/src/config/navigation.ts` (menús).

**Verificar que TODA ruta en navigation.ts tiene su guard correcto en middleware.ts:**

| Ruta nav                    | ¿En middleware authenticatedRoutes?     | ¿En roleProtectedRoutes?                                     |
| --------------------------- | --------------------------------------- | ------------------------------------------------------------ |
| `/cuenta/alertas`           | ✓ (verificar)                           | No role                                                      |
| `/cuenta/estadisticas`      | ✓ (verificar)                           | No role (seller only por nav, pero middleware no diferencia) |
| `/cuenta/consultas`         | ✓ (verificar)                           | No role                                                      |
| `/cuenta/pagos`             | ✓ (verificar)                           | No role                                                      |
| `/cuenta/convert-to-seller` | ✓ (verificar)                           | No role                                                      |
| `/dealer/citas`             | ✓ verificar está en roleProtectedRoutes | ['dealer', 'dealer_employee', 'admin']                       |
| `/dealer/rendimiento`       | verificar                               | dealer roles                                                 |
| `/dealer/configuracion`     | verificar                               | dealer roles                                                 |
| `/dealer/historial-pagos`   | verificar                               | dealer roles                                                 |

**Acción:** Revisar `middleware.ts` y agregar a `authenticatedRoutes` cualquier ruta que falte. Agregar a `roleProtectedRoutes` cualquier ruta bajo `/dealer/` que no esté.

### 4.2 — Empty States y Loading States

Verificar que TODAS las páginas con listas tienen:

- `loading.tsx` en el directorio de la ruta (ya existe en muchos)
- Empty state con mensaje descriptivo (NO pantalla en blanco)
- Error state con opción de retry

Rutas con mayor riesgo de falta de empty/error states:

- `/dealer/analytics` — si DealerAnalyticsService está offline
- `/dealer/leads` — si no hay leads
- `/admin/reviews` — si ReviewService responde 503
- `/cuenta/alertas` — AlertService (0/0 replicas)

**Acción:** Para servicios con `replicas: 0`, implementar graceful degradation en la UI:

```tsx
// Pattern de fallback
const { data, error, isLoading } = useQuery(...)

if (isLoading) return <LoadingSkeleton />
if (error) return (
  <EmptyState
    icon={AlertCircle}
    title="Servicio no disponible temporalmente"
    description="Esta función estará disponible próximamente."
  />
)
```

### 4.3 — Tipos TypeScript de Componentes Recientes

Los siguientes componentes fueron creados recientemente y posiblemente tengan cambios desde la creación. Verificar errores TypeScript en todos:

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/frontend/web-next
pnpm tsc --noEmit 2>&1 | head -50
```

Si hay errores TypeScript en:

- `src/components/reviews/star-rating.tsx`
- `src/components/reviews/review-card.tsx`
- `src/components/reviews/write-review-dialog.tsx`
- `src/components/reviews/reviews-section.tsx`
- `src/components/appointments/appointment-calendar.tsx`
- `src/components/vehicle-detail/seller-card.tsx`
- `src/app/(main)/dealers/[slug]/dealer-profile-client.tsx`

→ Corregirlos TODOS.

### 4.4 — API Routes Faltantes en Ocelot Gateway

Verificar que estas rutas existen en `ocelot.prod.json`:

```bash
python3 -c "
import json
with open('backend/Gateway/Gateway.Api/ocelot.prod.json') as f:
    config = json.load(f)

upstreams = [r['UpstreamPathTemplate'] for r in config.get('Routes', [])]

required = [
    '/api/reviews',
    '/api/appointments',
    '/api/timeslots',
    '/api/dealers',
    '/api/kyc',
    '/api/audit',
    '/api/auth',
    '/api/vehicles',
    '/api/users',
    '/api/roles',
    '/api/media',
    '/api/notifications',
    '/api/billing',
]

for route in required:
    found = any(route in u for u in upstreams)
    print(f'{'✅' if found else '❌'} {route}')
"
```

Para cada ruta ❌: agregar bloque de rutas en `ocelot.prod.json`, aplicar ConfigMap, reiniciar gateway.

### 4.5 — Secrets de K8s para Servicios Nuevos

Verificar que TODOS los servicios activos (replicas ≥ 1) tienen sus secrets:

```bash
for SVC in reviewservice appointmentservice dealermanagementservice dealeranalyticsservice; do
  kubectl get secret ${SVC}-db-secret -n okla 2>/dev/null && echo "✅ $SVC secret" || echo "❌ $SVC secret MISSING"
done
```

Para secrets faltantes, crear con la connection string correcta usando la DB managed de DO.

---

## 📦 BLOQUE 5: COMMIT Y DEPLOY FINAL

### 5.1 — Compilar y Verificar

```bash
# Backend — verificar que todos los servicios modificados compilan
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Verificar DealerAnalyticsService
cd backend/DealerAnalyticsService && dotnet build && cd ../..

# Frontend — verificar TypeScript
cd frontend/web-next && pnpm tsc --noEmit && pnpm lint && cd ../..
```

### 5.2 — Commit Atómico por Bloque

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Commit 1: Infrastructure fixes
git add k8s/deployments.yaml k8s/secrets.yaml
git add backend/ChatbotService/
git add backend/DealerAnalyticsService/
git commit -m "fix(infra): rabbit auth env vars, chatbot health probe, dealer analytics build"

# Commit 2: Frontend fixes (si hay cambios en UI/TypeScript)
git add frontend/web-next/src/
git commit -m "fix(frontend): resolve TypeScript errors, add empty states, align middleware guards"

# Commit 3: Gateway routes (si se agregaron)
git add backend/Gateway/
git commit -m "feat(gateway): add missing routes for all active services"

# Push
git push origin main
```

### 5.3 — Deploy y Verificación Final

```bash
# Aplicar todos los cambios de K8s
kubectl apply -f k8s/deployments.yaml -n okla
kubectl apply -f k8s/secrets.yaml -n okla 2>/dev/null || true

# Esperar a que todos los pods se estabilicen
kubectl rollout status deployment/chatbotservice -n okla
kubectl rollout status deployment/dealermanagementservice -n okla
kubectl rollout status deployment/dealeranalyticsservice -n okla

# Estado final de pods
kubectl get pods -n okla --sort-by='.metadata.name' | \
  awk 'NR==1 || $3!="Running" || $2!~/^[0-9]+\/[0-9]+/ {print} $2~/^[0-9]+\/[0-9]+/ && $3=="Running" {split($2,a,"/"); if(a[1]==a[2]) print "✅ " $0; else print "⚠️  " $0}'
```

### 5.4 — Reporte de Estado Final

Al terminar, reportar:

```
═══════════════════════════════════════════════════════════════
 OKLA PHASE 3 — ESTADO FINAL
═══════════════════════════════════════════════════════════════

INFRAESTRUCTURA:
  ✅/❌ DealerManagementService RabbitMQ auth — RESUELTO/PENDIENTE
  ✅/❌ ChatbotService — 1/1 Running, 0 restarts
  ✅/❌ DealerAnalyticsService — 1/1 Running, imagen en GHCR

PODS ACTIVOS (debe ser 17+ servicios 1/1 Running):
  [output de kubectl get pods -n okla]

TESTS DE API:
  T-001 a T-102: X/Y passed

FLUJOS DE USUARIO:
  ✅/❌ Guest: F-G1 a F-G4
  ✅/❌ Buyer: F-B1 a F-B4
  ✅/❌ Seller: F-S1 a F-S3
  ✅/❌ Dealer: F-D1 a F-D5
  ✅/❌ Admin: F-A1 a F-A6

TYPESCRIPT ERRORS: 0
GATEWAY ROUTES: Todos presentes ✅
K8s SECRETS: Todos presentes ✅

URL PRODUCCIÓN: https://okla.com.do ✅
```

---

## ⚠️ REGLAS DE EJECUCIÓN

1. **NO te detengas ante errores.** Cada error es un ítem de trabajo, no un bloqueante.
2. **Corrige y continúa.** Si un test falla, diagnostica, corrige, y re-ejecuta antes de avanzar.
3. **Sin documentación de errores.** No crees archivos `.md` que documenten los errores — corrígelos directamente.
4. **Verifica cada fix.** Después de cada corrección, ejecuta el test correspondiente para confirmar.
5. **Orden de prioridad:** Bloque 1 (infraestructura) > Bloque 2+3 (flows + QA) > Bloque 4 (polish).
6. **Secrets y credenciales nunca en código.** Usar K8s Secrets o env vars siempre.
7. **OpenTelemetry: versión máxima 1.9.0.** (1.10.0 requiere .NET 9 y rompe el build)
8. **pnpm siempre.** Nunca npm ni yarn.
9. **Puerto 8080 en K8s.** Todos los servicios escuchan en 8080 internamente.
10. **Commits atómicos.** Un commit por bloque lógico de cambios, con mensaje descriptivo.

---

_Versión 3.0 — Feb 20, 2026 — OKLA Microservices Platform_
