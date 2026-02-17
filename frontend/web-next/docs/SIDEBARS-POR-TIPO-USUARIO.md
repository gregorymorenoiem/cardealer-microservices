# 🔍 SIDEBARS POR TIPO DE USUARIO - OKLA

**Fecha:** Febrero 2026  
**Estado:** ✅ IMPLEMENTADO - 4 SIDEBARS DISTINTOS

---

## 📋 TIPOS DE USUARIO Y SUS SIDEBARS

| Tipo          | AccountType                 | Sidebar            | Descripción                            |
| ------------- | --------------------------- | ------------------ | -------------------------------------- |
| **Comprador** | `individual` (sin listings) | `BUYER_NAV_ITEMS`  | Solo búsqueda y favoritos              |
| **Vendedor**  | `individual` (con listings) | `SELLER_NAV_ITEMS` | Gestión de publicaciones + facturación |
| **Dealer**    | `dealer`                    | `DEALER_NAV_ITEMS` | Portal comercial completo              |
| **Admin**     | `admin`                     | `ADMIN_NAV_ITEMS`  | Administración de plataforma           |

---

## 🛒 SIDEBAR: COMPRADOR PURO

Usuario que solo busca vehículos para comprar. **No paga nada.**

```
📊 Mi Cuenta
   ├── Dashboard
   └── Mi Perfil

🔍 Búsqueda
   ├── Favoritos
   ├── Búsquedas Guardadas
   └── Alertas de Precio

💬 Comunicación
   ├── Mensajes
   └── Notificaciones

⚙️ Configuración
   ├── Seguridad
   └── Preferencias
```

---

## 💰 SIDEBAR: VENDEDOR INDIVIDUAL

Usuario que ha publicado al menos 1 vehículo. **Paga $29/listing.**

```
📊 Mi Cuenta
   ├── Dashboard
   └── Mi Perfil

🚗 Mis Publicaciones
   ├── Mis Vehículos
   ├── Estadísticas
   └── Consultas Recibidas

🔍 Búsqueda
   ├── Favoritos
   └── Alertas de Precio

💳 Facturación
   ├── Pagos
   └── Historial

⚙️ Configuración
   ├── Seguridad
   ├── Notificaciones
   └── Preferencias
```

---

## 🏢 SIDEBAR: DEALER

Cuenta comercial con suscripción mensual. **Paga $49-$299/mes.**

```
🏢 Portal Dealer
   ├── Dashboard Dealer
   ├── Inventario
   ├── Publicar Vehículo
   └── Importar Masivo

📈 Ventas
   ├── Leads / Consultas
   ├── Analíticas
   └── Rendimiento

🏪 Mi Negocio
   ├── Perfil del Negocio
   ├── Sucursales
   └── Mi Equipo

💳 Facturación
   ├── Mi Suscripción
   ├── Facturación
   └── Historial de Pagos

👤 Cuenta Personal
   ├── Mi Perfil
   ├── Seguridad
   └── Configuración
```

---

## 🛡️ SIDEBAR: ADMIN

Administrador de la plataforma OKLA.

```
🛡️ Administración
   ├── Dashboard Admin
   ├── Métricas Plataforma
   └── Reportes

👥 Gestión de Usuarios
   ├── Usuarios
   ├── Dealers
   └── Verificaciones

📄 Contenido
   ├── Vehículos
   ├── Moderación
   └── Reportes de Usuarios

📣 Marketing
   ├── Promociones
   ├── Banners
   └── Early Bird

💵 Finanzas
   ├── Ingresos
   ├── Suscripciones
   └── Transacciones

⚙️ Sistema
   ├── Mantenimiento
   ├── Logs
   └── Configuración

👤 Mi Cuenta
   ├── Mi Perfil
   └── Seguridad
```

---

## 🏷️ BADGES DE USUARIO

Cada tipo de usuario muestra un badge distintivo en el header del sidebar:

| Tipo      | Badge      | Color                                    |
| --------- | ---------- | ---------------------------------------- |
| Admin     | `Admin`    | 🔴 Rojo (`bg-red-100 text-red-700`)      |
| Dealer    | `Dealer`   | 🔵 Azul (`bg-blue-100 text-blue-700`)    |
| Vendedor  | `Vendedor` | 🟢 Verde (`bg-green-100 text-green-700`) |
| Comprador | Sin badge  | -                                        |

---

## 📁 ARCHIVO IMPLEMENTADO

**`src/app/(main)/cuenta/layout.tsx`**

### Constantes de Menú:

- `BUYER_NAV_ITEMS` - Menú para compradores puros
- `SELLER_NAV_ITEMS` - Menú para vendedores individuales
- `DEALER_NAV_ITEMS` - Menú para dealers
- `ADMIN_NAV_ITEMS` - Menú para administradores

### Funciones:

- `getAccountNavItems(user)` - Retorna el menú correcto según el tipo de usuario
- `getUserBadge(user)` - Retorna el badge y color según el tipo

### Características del Sidebar:

- Ancho aumentado a `lg:w-72` para acomodar más opciones
- Scroll interno con `max-h-[calc(100vh-220px)] overflow-y-auto`
- Soporte para badges en items individuales
- Header con avatar, nombre, email y badge de tipo

---

## ✅ LÓGICA DE SELECCIÓN

```typescript
function getAccountNavItems(user: UserType): NavSection[] {
  // 1. Admin - Menú completo de administración
  if (user.accountType === 'admin') {
    return ADMIN_NAV_ITEMS;
  }

  // 2. Dealer - Menú de portal comercial
  if (user.accountType === 'dealer') {
    return DEALER_NAV_ITEMS;
  }

  // 3. Individual con listings - Menú de vendedor
  if ((user.listingsCount ?? 0) > 0) {
    return SELLER_NAV_ITEMS;
  }

  // 4. Individual sin listings - Menú de comprador
  return BUYER_NAV_ITEMS;
}
```

---

**Fecha de implementación:** Febrero 2026
