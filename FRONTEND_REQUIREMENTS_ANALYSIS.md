# 🎨 Análisis de Microservicios - Requerimientos Frontend

> **Proyecto**: CarDealer - Marketplace de Vehículos  
> **Fecha**: 4 de Diciembre 2025  
> **Objetivo**: Identificar qué microservicios necesitan interfaces de usuario

---

## 📊 RESUMEN EJECUTIVO

De los **29 microservicios/módulos**, identificamos:

| Categoría | Cantidad | Tipo |
|-----------|----------|------|
| **🎯 Requieren Frontend** | **8 servicios** | Interfaces de usuario necesarias |
| **🔧 Backend Only** | 17 servicios | Solo APIs internas |
| **📦 Infraestructura** | 4 módulos | Soporte/configuración |

---

## 🎯 MICROSERVICIOS QUE REQUIEREN FRONTEND

### 1. 🛍️ **ProductService** ⭐ CRÍTICO
**Prioridad**: ALTA - Es el core del marketplace

**Interfaces Necesarias**:
```
👥 Usuario Público (Comprador):
├── 🏠 Home/Landing Page
│   └── Catálogo de productos destacados
├── 🔍 Búsqueda de Vehículos
│   ├── Filtros (marca, modelo, año, precio, combustible)
│   ├── Ordenamiento (precio, fecha, kilometraje)
│   └── Búsqueda avanzada
├── 📄 Detalle de Vehículo
│   ├── Galería de imágenes (carrusel)
│   ├── Especificaciones técnicas
│   ├── Precio y financiamiento
│   ├── Descripción completa
│   └── Botón "Contactar Vendedor"
├── ❤️ Favoritos
│   └── Lista de vehículos guardados
└── 📊 Comparador
    └── Comparar hasta 3 vehículos

👤 Usuario Vendedor:
├── 📝 Publicar Vehículo
│   ├── Formulario multi-step
│   ├── Upload de fotos (drag & drop)
│   ├── Especificaciones técnicas
│   └── Pricing
├── 🚙 Mis Vehículos
│   ├── Lista de publicaciones
│   ├── Estado (borrador, publicado, vendido)
│   ├── Editar/eliminar
│   └── Estadísticas de vistas
└── 📈 Dashboard Vendedor
    ├── Vistas totales
    ├── Contactos recibidos
    └── Vehículos activos
```

**Endpoints API Clave**:
- `GET /api/vehicles` - Listar vehículos
- `GET /api/vehicles/{id}` - Detalle
- `GET /api/vehicles/search` - Búsqueda con filtros
- `POST /api/vehicles` - Crear publicación
- `PUT /api/vehicles/{id}` - Actualizar
- `POST /api/vehicles/{id}/images` - Subir fotos
- `GET /api/vehicles/user/{userId}` - Mis vehículos

---

### 2. 🔐 **AuthService** ⭐ CRÍTICO
**Prioridad**: ALTA - Acceso y seguridad

**Interfaces Necesarias**:
```
├── 🔑 Login
│   ├── Email/password
│   ├── OAuth (Google, Facebook)
│   └── "Recordarme"
├── ✍️ Registro
│   ├── Formulario de usuario
│   ├── Verificación email
│   └── Términos y condiciones
├── 🔒 Recuperar Contraseña
│   ├── Email de recuperación
│   └── Reset password
├── 👤 Mi Perfil
│   ├── Datos personales
│   ├── Cambiar contraseña
│   ├── Foto de perfil
│   └── Preferencias
└── 🔐 2FA (Opcional)
    └── Configurar autenticación de dos factores
```

**Endpoints API Clave**:
- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `GET /api/auth/profile`
- `PUT /api/auth/profile`

---

### 3. 💬 **ContactService**
**Prioridad**: MEDIA - Comunicación comprador-vendedor

**Interfaces Necesarias**:
```
├── 📧 Formulario de Contacto
│   ├── Contactar sobre un vehículo
│   ├── Mensaje predefinido
│   └── Envío de consulta
├── 💬 Chat/Mensajes (Opcional)
│   ├── Inbox de mensajes
│   ├── Historial de conversaciones
│   └── Notificaciones en tiempo real
└── 📱 Mi Bandeja
    ├── Mensajes recibidos
    ├── Mensajes enviados
    └── Estado (leído/no leído)
```

**Endpoints API Clave**:
- `POST /api/contacts/send` - Enviar mensaje
- `GET /api/contacts/inbox` - Bandeja entrada
- `GET /api/contacts/sent` - Mensajes enviados
- `GET /api/contacts/{id}` - Ver conversación

---

### 4. 👤 **UserService**
**Prioridad**: MEDIA - Perfiles y gestión de usuarios

**Interfaces Necesarias**:
```
├── 👤 Perfil Público
│   ├── Información del vendedor
│   ├── Vehículos publicados
│   ├── Rating/reviews
│   └── Insignias
├── ⚙️ Configuración de Cuenta
│   ├── Datos personales
│   ├── Privacidad
│   ├── Notificaciones
│   └── Eliminar cuenta
└── ⭐ Mis Favoritos
    └── Vehículos guardados
```

**Endpoints API Clave**:
- `GET /api/users/{id}` - Perfil público
- `GET /api/users/me` - Mi perfil
- `PUT /api/users/me` - Actualizar perfil
- `GET /api/users/me/favorites` - Favoritos
- `POST /api/users/me/favorites/{vehicleId}` - Agregar favorito

---

### 5. 🔍 **SearchService**
**Prioridad**: ALTA - Búsqueda avanzada

**Interfaces Necesarias**:
```
├── 🔍 Barra de Búsqueda Global
│   ├── Autocompletado
│   ├── Sugerencias
│   └── Búsqueda inteligente
├── 🎛️ Filtros Avanzados
│   ├── Panel lateral de filtros
│   ├── Rango de precio
│   ├── Año min/max
│   ├── Kilometraje
│   ├── Múltiples marcas
│   └── Tags/características
└── 📊 Resultados de Búsqueda
    ├── Grid/lista de vehículos
    ├── Ordenamiento
    ├── Paginación
    └── Contador de resultados
```

**Endpoints API Clave**:
- `GET /api/search/vehicles` - Búsqueda con filtros
- `GET /api/search/suggestions` - Autocompletado
- `GET /api/search/filters` - Opciones de filtros disponibles

---

### 6. 📸 **MediaService**
**Prioridad**: MEDIA - Gestión de imágenes

**Interfaces Necesarias**:
```
├── 📤 Upload de Imágenes
│   ├── Drag & drop múltiple
│   ├── Preview antes de subir
│   ├── Progress bar
│   └── Crop/resize opcional
├── 🖼️ Galería de Imágenes
│   ├── Carrusel en detalle
│   ├── Lightbox/modal
│   ├── Zoom
│   └── Thumbnails
└── 🗑️ Gestión de Fotos
    ├── Reordenar imágenes
    ├── Establecer foto principal
    └── Eliminar fotos
```

**Endpoints API Clave**:
- `POST /api/media/upload` - Subir imagen
- `GET /api/media/{id}` - Obtener imagen
- `DELETE /api/media/{id}` - Eliminar
- `GET /api/media/vehicle/{vehicleId}` - Todas las fotos

---

### 7. 🛡️ **AdminService**
**Prioridad**: MEDIA - Panel de administración

**Interfaces Necesarias**:
```
├── 🏠 Dashboard Admin
│   ├── Estadísticas generales
│   ├── Gráficos (ventas, usuarios, vehículos)
│   └── Actividad reciente
├── ✅ Moderación de Vehículos
│   ├── Lista de vehículos pendientes
│   ├── Aprobar/rechazar publicaciones
│   └── Razones de rechazo
├── 👥 Gestión de Usuarios
│   ├── Lista de usuarios
│   ├── Bloquear/desbloquear
│   ├── Ver actividad
│   └── Editar roles
├── 🚨 Reportes
│   ├── Lista de reportes
│   ├── Resolver/cerrar
│   └── Acciones tomadas
└── 📊 Estadísticas
    ├── Vehículos por categoría
    ├── Usuarios activos
    └── Reportes generados
```

**Endpoints API Clave**:
- `GET /api/admin/dashboard` - Estadísticas
- `GET /api/admin/vehicles/pending` - Pendientes
- `POST /api/admin/vehicles/{id}/approve` - Aprobar
- `POST /api/admin/vehicles/{id}/reject` - Rechazar
- `GET /api/admin/users` - Listar usuarios
- `GET /api/admin/reports` - Reportes

---

### 8. 🔔 **NotificationService** (UI Mínima)
**Prioridad**: BAJA - Principalmente backend, UI básica

**Interfaces Necesarias**:
```
├── 🔔 Centro de Notificaciones
│   ├── Dropdown/modal
│   ├── Lista de notificaciones
│   ├── Marcar como leído
│   └── Badge contador
└── ⚙️ Preferencias
    ├── Email on/off
    ├── Push on/off
    └── Tipos de notificaciones
```

**Endpoints API Clave**:
- `GET /api/notifications` - Obtener notificaciones
- `PUT /api/notifications/{id}/read` - Marcar leído
- `GET /api/notifications/settings` - Preferencias
- `PUT /api/notifications/settings` - Actualizar preferencias

---

## 🔧 MICROSERVICIOS BACKEND ONLY (Sin Frontend)

Estos servicios **NO necesitan interfaz de usuario** directa:

| # | Servicio | Propósito | Por qué no necesita UI |
|---|----------|-----------|------------------------|
| 1 | **ErrorService** | Gestión de errores | Log interno, solo APIs |
| 2 | **LoggingService** | Logs centralizados | Usar Seq/Kibana externo |
| 3 | **AuditService** | Auditoría de eventos | Solo para admins/devs |
| 4 | **CacheService** | Cache Redis | Infraestructura |
| 5 | **MessageBusService** | RabbitMQ wrapper | Mensajería interna |
| 6 | **HealthCheckService** | Health checks | Monitoreo interno |
| 7 | **ConfigurationService** | Configuración | Env vars/admin solo |
| 8 | **FeatureToggleService** | Feature flags | Admin panel (opcional) |
| 9 | **IdempotencyService** | Prevención duplicados | Middleware transparente |
| 10 | **RateLimitingService** | Rate limiting | Middleware transparente |
| 11 | **SchedulerService** | Jobs programados | Hangfire dashboard |
| 12 | **TracingService** | Distributed tracing | Jaeger UI externo |
| 13 | **BackupDRService** | Backups | Automatizado |
| 14 | **RoleService** | Roles/permisos | API para admin |
| 15 | **Gateway** | API Gateway | Proxy transparente |
| 16 | **ServiceDiscovery** | Consul | Consul UI |
| 17 | **ApiDocsService** | Swagger aggregator | Swagger UI ya incluido |

---

## 📦 MÓDULOS DE INFRAESTRUCTURA

No son servicios propiamente, son carpetas de soporte:

| Módulo | Descripción |
|--------|-------------|
| **_Shared** | Contratos y código compartido |
| **_Tests** | Proyectos de tests |
| **monitoring** | Configuración Prometheus/Grafana |
| **observability** | OpenTelemetry configs |
| **postgresql** | Scripts de base de datos |

---

## 🎯 PRIORIZACIÓN PARA DESARROLLO FRONTEND

### Fase 1 - MVP (Mínimo Viable Product)
**Objetivo**: Marketplace funcional básico

| Orden | Servicio | Estimación | Por qué |
|-------|----------|------------|---------|
| 1️⃣ | **AuthService** | 1 semana | Login/registro necesario primero |
| 2️⃣ | **VehicleService** | 2-3 semanas | Core del marketplace |
| 3️⃣ | **SearchService** | 1 semana | Búsqueda esencial |
| 4️⃣ | **MediaService** | 1 semana | Fotos de vehículos |
| 5️⃣ | **ContactService** | 3-4 días | Comunicación básica |

**Total Fase 1**: ~6-7 semanas

### Fase 2 - Funcionalidades Intermedias

| Orden | Servicio | Estimación | Por qué |
|-------|----------|------------|---------|
| 6️⃣ | **UserService** | 1 semana | Perfiles y favoritos |
| 7️⃣ | **NotificationService** | 3-4 días | Centro notificaciones |

**Total Fase 2**: ~2 semanas

### Fase 3 - Panel de Administración

| Orden | Servicio | Estimación | Por qué |
|-------|----------|------------|---------|
| 8️⃣ | **AdminService** | 2 semanas | Moderación y gestión |

**Total Fase 3**: ~2 semanas

---

## 🖼️ WIREFRAMES BÁSICOS RECOMENDADOS

### Para ProductService:

```
┌─────────────────────────────────────────┐
│  🚗 CarDealer - Home                   │
├─────────────────────────────────────────┤
│  [Logo]  🔍 Buscar productos...  [Login]│
├─────────────────────────────────────────┤
│                                         │
│  🎯 Filtros Rápidos:                    │
│  [Marca▼] [Modelo▼] [Año▼] [Precio▼]  │
│                                         │
│  📊 Vehículos Destacados:              │
│  ┌────────┐ ┌────────┐ ┌────────┐      │
│  │ [Foto] │ │ [Foto] │ │ [Foto] │      │
│  │ BMW 320│ │ Audi A4│ │ Toyota │      │
│  │ $25,000│ │ $30,000│ │ $18,000│      │
│  └────────┘ └────────┘ └────────┘      │
│                                         │
└─────────────────────────────────────────┘
```

---

## 📱 STACK TECNOLÓGICO FRONTEND RECOMENDADO

### Opción 1: React (Recomendado)
```javascript
Frontend Stack:
├── React 18 + TypeScript
├── Vite (build tool)
├── React Router (navegación)
├── TanStack Query (data fetching)
├── Zustand o Redux Toolkit (state)
├── Tailwind CSS (estilos)
├── shadcn/ui (componentes)
└── Axios (HTTP client)
```

### Opción 2: Next.js (Para SEO)
```javascript
Frontend Stack:
├── Next.js 14 (App Router)
├── TypeScript
├── Server Components
├── TanStack Query
├── Zustand
├── Tailwind CSS
└── shadcn/ui
```

### Opción 3: Vue.js (Alternativa)
```javascript
Frontend Stack:
├── Vue 3 + TypeScript
├── Vite
├── Vue Router
├── Pinia (state)
├── TanStack Query
├── Tailwind CSS
└── PrimeVue (componentes)
```

---

## 🔌 INTEGRACIÓN FRONTEND-BACKEND

### Comunicación:
```
Frontend (React/Next.js)
     │
     ├── HTTP REST
     │   └── Gateway (puerto 18443)
     │       └── Routing a microservicios
     │
     └── WebSocket (Opcional)
         └── SignalR para notificaciones real-time
```

### Autenticación:
```
1. Login → AuthService
2. Recibe JWT token
3. Almacena en localStorage/sessionStorage
4. Envía token en header: Authorization: Bearer <token>
5. Gateway valida token
6. Redirige a microservicio correspondiente
```

---

## 📝 PRÓXIMOS PASOS

### Para el Frontend:

1. **Decidir Stack** (React, Next.js, o Vue)
2. **Crear proyecto base**
   ```bash
   npm create vite@latest cardealer-frontend -- --template react-ts
   ```
3. **Configurar routing**
4. **Implementar auth flow**
5. **Conectar con Gateway (http://localhost:18443)**
6. **Desarrollar vistas por prioridad** (ver Fase 1)

### Para el Backend:

1. **Verificar que Gateway está configurado** para recibir requests del frontend
2. **Configurar CORS** en Gateway y servicios
3. **Documentar APIs** en Swagger
4. **Probar endpoints** desde frontend

---

## 🎨 RESUMEN VISUAL

```
┌─────────────────────────────────────────────────────┐
│           MICROSERVICIOS CON FRONTEND                │
├─────────────────────────────────────────────────────┤
│                                                      │
│  ⭐ CRÍTICOS (MVP):                                  │
│  1. AuthService      → Login/Registro               │
│  2. ProductService   → Catálogo/Búsqueda/Detalle   │
│  3. SearchService    → Filtros avanzados            │
│                                                      │
│  🟡 INTERMEDIOS:                                     │
│  4. MediaService     → Upload/galería fotos         │
│  5. ContactService   → Mensajes                     │
│  6. UserService      → Perfiles/favoritos           │
│                                                      │
│  🟢 OPCIONALES:                                      │
│  7. AdminService     → Panel admin                  │
│  8. NotificationService → Centro notificaciones     │
│                                                      │
└─────────────────────────────────────────────────────┘

Total: 8 servicios con UI de 29 totales (27.5%)
```

---

*Documento generado para planificación del frontend de CarDealer*
