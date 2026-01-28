# 🎯 Menús de Portales - Estructura Implementada

> **Última Actualización:** Enero 27, 2026  
> **Importante:** OKLA es una plataforma de **anuncios clasificados** de vehículos.  
> Los usuarios **PUBLICAN** vehículos, NO los venden directamente en la plataforma.

Este documento describe la estructura de navegación optimizada para todos los tipos de usuario de OKLA.

---

## 📋 TIPOS DE USUARIO Y SUS MENÚS

| Tipo de Usuario      | AccountType         | Portal                      | Sidebar                    |
| -------------------- | ------------------- | --------------------------- | -------------------------- |
| Visitante            | `null`              | Público                     | ❌ No aplica               |
| Comprador Registrado | `individual`        | Público + Dashboard         | UserSidebar (futuro)       |
| Vendedor Individual  | `individual`        | Público + Mis Publicaciones | UserSidebar (futuro)       |
| Dealer (Propietario) | `dealer`            | DealerPortal                | DealerSidebar ✅           |
| Empleado Dealer      | `dealer_employee`   | DealerPortal (filtrado)     | DealerSidebar (filtrado)   |
| Moderador            | `platform_employee` | AdminPortal                 | AdminSidebar (parcial)     |
| Contabilidad         | `platform_employee` | AdminPortal                 | AdminSidebar (fiscal)      |
| Soporte              | `platform_employee` | AdminPortal                 | AdminSidebar (support)     |
| Super Admin          | `admin`             | AdminPortal                 | AdminSidebar (completo) ✅ |

---

## 📱 NAVBAR - Estructura por Usuario

### Todos los Usuarios

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  🚗 OKLA   │ Vehículos │ [Context Link] │  🔍 Buscar...  │ [Actions] │ 👤    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Links según tipo:

| Usuario           | Context Link   | CTA Principal  | Dropdown Links                          |
| ----------------- | -------------- | -------------- | --------------------------------------- |
| **Visitante**     | "Para Dealers" | Login/Registro | -                                       |
| **Comprador**     | "Para Dealers" | -              | Dashboard, Favoritos, Mensajes, Perfil  |
| **Vendedor Ind.** | "Para Dealers" | ➕ Publicar    | Mis Vehículos, Consultas, Favoritos     |
| **Dealer**        | "Mi Dashboard" | ➕ Publicar    | Dashboard, Inventario, Leads, Analytics |
| **Admin**         | "Admin Panel"  | -              | Dashboard Admin, Dealers, Pendientes    |

---

## 🟦 PORTAL DEL DEALER – MENÚ CORREGIDO (Enero 27, 2026)

> **⚠️ IMPORTANTE:** La sección "Facturación & NCF (DGII)" fue **ELIMINADA** del DealerSidebar.  
> Esa funcionalidad es para **Administradores de OKLA** (contabilidad), NO para dealers.  
> Los dealers tienen acceso a **"Mi Suscripción"** para gestionar sus pagos como CLIENTES de OKLA.

```
PORTAL DEL DEALER (DealerSidebar.tsx)
├─ 📊 Dashboard
│   ├─ Resumen del negocio
│   ├─ Ventas del mes
│   ├─ Analytics
│   ├─ Publicaciones activas
│   └─ Alertas (fotos faltantes, leads nuevos)
│
├─ 🚗 Inventario de Vehículos
│   ├─ Listado de vehículos
│   ├─ Agregar vehículo
│   ├─ Estados (Disponible, Reservado, Vendido)
│   └─ Costos & precios
│
├─ 🛒 Publicación en Marketplace
│   ├─ Publicaciones activas
│   ├─ Publicaciones pendientes
│   ├─ Nueva publicación
│   └─ Configuración de tienda
│
├─ 👥 CRM / Leads (🔒 según plan)
│   ├─ Todos los leads
│   ├─ Pipeline de ventas
│   ├─ Calendario de seguimiento
│   └─ Asignación a vendedores
│
├─ 📢 Publicidad y Promociones 💰
│   ├─ Productos Disponibles
│   ├─ Destacado en Home
│   ├─ Publicación Patrocinada
│   ├─ Mis Campañas Activas
│   └─ Resultados y ROI
│
├─ 🏦 Financiamiento (🔒 según plan)
│   ├─ Simulador de financiamiento
│   ├─ Tabla de amortización
│   └─ Operaciones activas
│
├─ 💳 Mi Suscripción ⭐ NUEVO
│   ├─ Mi Plan Actual
│   ├─ Mis Facturas (recibidas de OKLA)
│   ├─ Método de Pago
│   └─ Cambiar Plan / Upgrade
│
└─ ⚙️ Configuración
    ├─ Perfil del dealer
    ├─ Empleados y roles
    ├─ Preferencias del sistema
    └─ Canales de contacto
```

### Componente: `DealerSidebar.tsx`

- **Ubicación**: `frontend/web/src/components/navigation/DealerSidebar.tsx`
- **Características**:
  - Menús colapsables con animación
  - Badges de alertas/notificaciones
  - Control de acceso por plan (Free/Basic/Pro/Enterprise)
  - Indicadores visuales de sección activa
  - **"Mi Suscripción"** para gestión de pagos del dealer como cliente

---

## 🟥 PORTAL ADMINISTRADOR – MENÚ OPTIMIZADO (Enero 27, 2026)

> **NOTA:** El AdminSidebar ahora incluye "Contabilidad & NCF (DGII)" que fue removido del DealerSidebar.

```
PORTAL ADMINISTRADOR (AdminSidebar.tsx)
├─ 📊 Dashboard
│   ├─ Resumen general
│   ├─ Dealers activos
│   ├─ Publicaciones activas
│   ├─ Leads generados (plataforma)
│   ├─ Ingresos por suscripciones
│   ├─ Ingresos por publicidad 💰
│   └─ Alertas críticas
│
├─ 🏢 Dealers (Clientes)
│   ├─ Listado de dealers
│   ├─ Crear/Editar dealer
│   ├─ Activar/Desactivar
│   └─ Plan de suscripción
│
├─ 🛒 Marketplace Público
│   ├─ Publicaciones pendientes
│   ├─ Publicaciones reportadas
│   ├─ Publicaciones destacadas
│   └─ Reglas de publicación
│
├─ 🛡️ Moderación y Seguridad
│   ├─ Reportes de usuarios
│   ├─ Bloqueo de publicaciones/dealers
│   └─ Lista negra (teléfonos, emails)
│
├─ 💳 Facturación SaaS
│   ├─ Suscripciones activas
│   ├─ Facturas a dealers
│   ├─ Pagos recibidos
│   └─ Planes y precios
│
├─ 🧾 Contabilidad & NCF (DGII) ⭐ MOVIDO DE DEALER
│   ├─ Nueva factura
│   ├─ Facturas emitidas
│   ├─ Notas de crédito/débito
│   ├─ Anulación de comprobantes
│   ├─ Secuencias NCF
│   ├─ Reporte 607 (Ventas)
│   ├─ Reporte 608 (Compras)
│   └─ Configuración fiscal
│
├─ 💎 Publicidad de la Plataforma 🏆
│   ├─ Productos Publicitarios
│   ├─ Destacados en Home
│   ├─ Publicaciones Patrocinadas
│   ├─ Banners promocionales
│   ├─ Email marketing masivo
│   ├─ Campañas Activas
│   ├─ Configuración de Precios
│   ├─ Descuentos por volumen
│   ├─ Ofertas especiales
│   └─ Reportes de Publicidad
│
├─ 📈 Analítica y Business Intelligence
│   ├─ Tráfico del marketplace
│   ├─ Vehículos más vistos
│   ├─ Búsquedas populares
│   ├─ Conversión visitas→leads
│   ├─ Comportamiento Dealers
│   ├─ Uso de herramientas
│   ├─ Rentabilidad
│   └─ CAC vs LTV
│
├─ 🔐 Roles y Permisos (RBAC)
│   ├─ Gestión de Roles
│   └─ Gestión de Permisos
│
└─ ⚙️ Sistema
    ├─ Configuración general
    ├─ Auditoría de acciones
    └─ Notificaciones globales
```

### Componente: `AdminSidebar.tsx`

- **Ubicación**: `frontend/web/src/components/navigation/AdminSidebar.tsx`
- **Características**:
  - Sección de Publicidad destacada visualmente (gradiente dorado)
  - Badges con contadores de items pendientes
  - Sección de Analytics expandida
  - Color scheme: Indigo para admin (diferente al azul de dealers)

---

## 📁 Archivos Creados/Modificados

| Archivo                                   | Descripción                         |
| ----------------------------------------- | ----------------------------------- |
| `components/navigation/DealerSidebar.tsx` | Sidebar colapsable del dealer       |
| `components/navigation/AdminSidebar.tsx`  | Sidebar colapsable del admin        |
| `components/navigation/index.ts`          | Exports de navegación               |
| `layouts/DealerLayout.tsx`                | Actualizado para usar DealerSidebar |
| `layouts/AdminLayout.tsx`                 | Actualizado para usar AdminSidebar  |

---

## 🎨 Diseño y UX

### Dealer Portal

- **Ancho sidebar**: 288px (w-72)
- **Color primario**: Azul (#2563eb)
- **Sección destacada**: Publicidad con emoji 💰

### Admin Portal

- **Ancho sidebar**: 320px (w-80)
- **Color primario**: Indigo (#4f46e5)
- **Sección destacada**: Publicidad con gradiente dorado y badge 🏆

### Características Comunes

- ✅ Menús colapsables (expand/collapse)
- ✅ Indicador visual de sección activa
- ✅ Badges de notificaciones
- ✅ Scroll interno en sidebar largo
- ✅ Iconos descriptivos por cada item
- ✅ Control de acceso por permisos/plan
