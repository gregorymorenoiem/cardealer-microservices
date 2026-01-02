# 🎯 Menús de Portales - Estructura Implementada

Este documento describe la estructura de navegación optimizada para los portales de Dealer y Administrador.

---

## 🟦 PORTAL DEL DEALER – MENÚ OPTIMIZADO

```
PORTAL DEL DEALER
├─ 📊 Dashboard
│   ├─ Resumen del negocio
│   ├─ Ventas del mes
│   ├─ Inventario disponible (con botón "Promocionar")
│   ├─ Leads recientes
│   ├─ Publicaciones activas
│   └─ Alertas (NCF vencidos, fotos faltantes)
│
├─ 🧾 Facturación & NCF (DGII)
│   ├─ Nueva factura
│   ├─ Facturas emitidas
│   ├─ Notas de crédito/débito
│   ├─ Anulación de comprobantes
│   ├─ Reportes DGII (607, 608)
│   └─ Configuración fiscal (RNC, rangos NCF)
│
├─ 🚗 Inventario de Vehículos
│   ├─ Listado de vehículos (con opción "Promocionar" por cada uno)
│   ├─ Agregar vehículo (nuevo/usado)
│   ├─ Estados (Disponible, Reservado, Vendido)
│   └─ Costos & precios
│
├─ 🛒 Publicación en Marketplace
│   ├─ Publicaciones activas
│   ├─ Publicaciones pendientes
│   ├─ Nueva publicación
│   └─ Configuración de tienda (contactos, moneda)
│
├─ 👥 CRM / Leads
│   ├─ Todos los leads
│   ├─ Pipeline de ventas
│   ├─ Calendario de seguimiento
│   └─ Asignación a vendedores
│
├─ 📢 Publicidad y Promociones (💰 CORAZÓN DEL NEGOCIO)
│   ├─ Productos Disponibles
│   │   ├─ Destacado en Home (1, 3, 7 días)
│   │   ├─ Publicación Patrocinada en búsquedas
│   │   ├─ Banner promocional
│   │   ├─ Email marketing a usuarios interesados
│   │   └─ Paquete "Vendedor Premium"
│   ├─ Mis Campañas Activas
│   ├─ Historial de Promociones
│   └─ Resultados y ROI
│       ├─ Impresiones/Clics por campaña
│       ├─ Leads generados por campaña
│       └─ Costo por lead/venta
│
├─ 🏦 Financiamiento y Seguros
│   ├─ Simulador de financiamiento
│   │   ├─ Cálculo de cuota
│   │   ├─ Tabla de amortización
│   │   └─ Exportar/Guardar
│   ├─ Operaciones con financiamiento
│   └─ Comisiones generadas
│
└─ ⚙️ Configuración
    ├─ Perfil del dealer
    ├─ Usuarios y roles
    ├─ Preferencias del sistema
    └─ Canales de contacto
```

### Componente: `DealerSidebar.tsx`
- **Ubicación**: `frontend/web/src/components/navigation/DealerSidebar.tsx`
- **Características**:
  - Menús colapsables con animación
  - Badges de alertas/notificaciones
  - Control de acceso por plan (Basic/Pro/Enterprise)
  - Indicadores visuales de sección activa

---

## 🟥 PORTAL ADMINISTRADOR – MENÚ OPTIMIZADO

```
PORTAL ADMINISTRADOR
├─ 📊 Dashboard
│   ├─ Resumen general
│   ├─ Dealers activos
│   ├─ Publicaciones activas
│   ├─ Leads generados (plataforma)
│   ├─ Ingresos por suscripciones
│   ├─ Ingresos por publicidad (💰 ¡PRINCIPAL!)
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
│   ├─ Publicaciones destacadas (gestionar)
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
├─ 💎 Publicidad de la Plataforma (🏆 TU MINA DE ORO)
│   ├─ Productos Publicitarios
│   │   ├─ Destacados en Home (crear/editar)
│   │   ├─ Publicaciones Patrocinadas
│   │   ├─ Banners promocionales
│   │   └─ Email marketing masivo
│   ├─ Campañas Activas (de dealers)
│   ├─ Configuración de Precios
│   │   ├─ Precios por producto
│   │   ├─ Descuentos por volumen
│   │   └─ Ofertas especiales
│   └─ Reportes de Publicidad
│       ├─ Ingresos por tipo de producto
│       ├─ Dealers top (más gasto en publicidad)
│       ├─ Conversión por campaña tipo
│       └─ ROI promedio para dealers
│
├─ 📈 Analítica y Business Intelligence
│   ├─ Tráfico del marketplace
│   │   ├─ Vehículos más vistos
│   │   ├─ Búsquedas populares
│   │   └─ Conversión visitas→leads
│   ├─ Comportamiento de Dealers
│   │   ├─ Uso de herramientas
│   │   ├─ Frecuencia de publicación
│   │   └─ Tasa de adopción de publicidad
│   └─ Rentabilidad
│       ├─ Ingresos totales (SaaS + Publicidad)
│       ├─ CAC vs LTV por dealer
│       └─ Proyección de crecimiento
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

| Archivo | Descripción |
|---------|-------------|
| `components/navigation/DealerSidebar.tsx` | Sidebar colapsable del dealer |
| `components/navigation/AdminSidebar.tsx` | Sidebar colapsable del admin |
| `components/navigation/index.ts` | Exports de navegación |
| `layouts/DealerLayout.tsx` | Actualizado para usar DealerSidebar |
| `layouts/AdminLayout.tsx` | Actualizado para usar AdminSidebar |

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
