# CarDealer Design - Classic Car Dealership

## 🚗 Descripción

Diseño clásico especializado **exclusivamente en venta de vehículos**. Interfaz profesional y directa enfocada en la compra y venta de automóviles nuevos y usados.

## 🎯 Características Clave

- ✅ **SOLO Vehículos** - Sin categorías adicionales
- ❌ **Sin Properties** - Eliminado completamente
- ❌ **Sin Vehicle Rental** - Eliminado completamente  
- ❌ **Sin Hospedaje** - Eliminado completamente
- ❌ **Sin Marketplace Legacy** - Eliminado completamente

## 📦 Funcionalidades Incluidas

### Públicas
- **Home**: Página principal enfocada en vehículos
- **Browse**: Explorar catálogo de vehículos
- **Detail**: Detalles completos del vehículo
- **Compare**: Comparar hasta 3 vehículos
- **Map View**: Vista de mapa con ubicaciones
- **Sell Your Car**: Publicar vehículo para venta

### Usuario
- Dashboard personal
- Mensajes y notificaciones
- Lista de favoritos (wishlist)
- Perfil y configuración

### Dealer
- Dashboard de dealer
- Gestión de inventario
- CRM y clientes
- Analytics y reportes
- Facturación

### Admin
- Panel de administración
- Gestión de usuarios
- Aprobaciones pendientes
- Reportes y estadísticas
- Configuración del sistema

## 🚀 Comandos

```bash
# Instalar dependencias
npm install

# Desarrollo (puerto 5175)
npm run dev

# Build para producción
npm run build

# Preview del build
npm run preview

# Tests
npm test
npm run test:ui
npm run test:coverage

# Linting
npm run lint
```

## 🌐 Acceso

- **Desarrollo**: http://localhost:5175/
- **Puerto**: 5175
- **Nombre**: cardealer-cardealer

## 📂 Estructura

```
cardealer/
├── src/
│   ├── pages/
│   │   ├── HomePage.tsx          ← Home enfocado solo en vehículos
│   │   ├── vehicles/             ← Módulo principal de vehículos
│   │   ├── user/                 ← Páginas de usuario
│   │   ├── dealer/               ← Dashboard de dealers
│   │   ├── admin/                ← Panel administrativo
│   │   ├── auth/                 ← Login/Register
│   │   ├── billing/              ← Facturación
│   │   └── common/               ← Páginas comunes (legal, help)
│   ├── components/               ← Componentes reutilizables
│   ├── layouts/                  ← Layouts (Main, Auth, Admin, Dealer)
│   ├── services/                 ← API services
│   ├── hooks/                    ← Custom hooks
│   ├── stores/                   ← Estado global (Zustand)
│   └── utils/                    ← Utilidades
├── public/                       ← Assets estáticos
└── package.json
```

## 🎨 Diseño

- **Estilo**: Clásico y profesional
- **Colores**: Azul como color principal
- **Tipografía**: Clean y legible
- **Enfoque**: Compra y venta de vehículos exclusivamente

## 🔗 Rutas Principales

### Públicas
- `/` - Home
- `/browse` - Catálogo
- `/listing/:id` - Detalle de vehículo
- `/compare` - Comparador
- `/sell-your-car` - Vender vehículo
- `/map` - Vista de mapa

### Autenticación
- `/login` - Inicio de sesión
- `/register` - Registro

### Usuario (Protegidas)
- `/dashboard` - Dashboard del usuario
- `/profile` - Perfil
- `/messages` - Mensajes
- `/wishlist` - Favoritos

### Dealer (Protegidas)
- `/dealer` - Dashboard
- `/dealer/listings` - Mis vehículos
- `/dealer/crm` - CRM
- `/dealer/analytics` - Analytics
- `/dealer/billing` - Facturación

### Admin (Protegidas)
- `/admin` - Panel principal
- `/admin/users` - Usuarios
- `/admin/listings` - Vehículos
- `/admin/pending` - Aprobaciones
- `/admin/reports` - Reportes

## 🔑 Diferencias con otros diseños

| Característica | Okla | Original | **CarDealer** |
|---------------|------|----------|---------------|
| Vehículos | ✅ | ✅ | ✅ |
| Properties | ✅ | ✅ | ❌ |
| Vehicle Rental | ✅ | ✅ | ❌ |
| Hospedaje | ✅ | ✅ | ❌ |
| Multi-categoría | ✅ | ✅ | ❌ |
| Enfoque | Premium | Marketplace | **Venta de Autos** |

## 🔧 Tecnologías

- React 19
- TypeScript
- Vite 7
- React Router 7
- TanStack Query
- Zustand
- Tailwind CSS
- Framer Motion
- React Hook Form
- Zod

## 📝 Notas Importantes

1. **100% Independiente**: No comparte código con otros diseños
2. **Especializado**: Solo venta de vehículos
3. **Sin categorías**: Todo el flujo orientado a cars
4. **Limpio**: Sin código legacy de marketplace o properties
5. **Profesional**: Interfaz clásica para dealers establecidos

## 🚀 Deploy

Este diseño puede desplegarse de forma completamente independiente en cualquier plataforma:

- Vercel
- Netlify
- AWS Amplify
- Azure Static Web Apps
- DigitalOcean App Platform

Cada diseño (okla, original, cardealer) es una aplicación React independiente.
