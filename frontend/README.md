# 🚗 CarDealer Frontend

Frontend moderno para el marketplace de vehículos CarDealer, construido con React + Vite + TypeScript.

## 🎯 Características

- ⚛️ **React 19.2** + **TypeScript 5.9**
- ⚡ **Vite 7.2** - Build tool ultra-rápido
- 🎨 **Tailwind CSS** - Utility-first CSS framework
- 🔄 **React Query** - Data fetching y caching
- 🏪 **Zustand** - State management simple y escalable
- 📱 **Responsive Design** - Mobile-first approach
- ♿ **Accessible** - WCAG AA compliant
- 🎭 **Framer Motion** - Animaciones suaves
- 📝 **React Hook Form + Zod** - Validación de formularios

## 📦 Estructura del Proyecto

```
frontend/
├── src/
│   ├── components/       # Componentes reutilizables (Atomic Design)
│   │   ├── atoms/        # Botones, inputs, badges
│   │   ├── molecules/    # FormField, SearchBar
│   │   ├── organisms/    # Navbar, VehicleCard
│   │   └── templates/    # Layouts
│   ├── features/         # Feature modules
│   │   ├── auth/         # Autenticación
│   │   ├── vehicles/     # Catálogo de vehículos
│   │   ├── user/         # Perfil de usuario
│   │   ├── admin/        # Panel admin
│   │   ├── messages/     # Sistema de mensajes
│   │   └── search/       # Búsqueda avanzada
│   ├── hooks/            # Custom hooks
│   ├── layouts/          # App layouts
│   ├── pages/            # Page components
│   ├── services/         # API clients
│   │   ├── api.ts        # Axios config
│   │   └── endpoints/    # Service endpoints
│   ├── store/            # Zustand stores
│   ├── types/            # TypeScript types
│   └── utils/            # Helper functions
├── public/               # Static assets
├── .env.example          # Environment variables template
├── tailwind.config.js    # Tailwind configuration
└── vite.config.ts        # Vite configuration
```

## 🚀 Quick Start

### Prerequisitos

- Node.js 20+
- npm 10+

### Instalación

```bash
# Instalar dependencias
npm install

# Copiar variables de entorno
cp .env.example .env

# Iniciar servidor de desarrollo
npm run dev
```

La aplicación estará disponible en `http://localhost:5173`

## 📜 Scripts Disponibles

```bash
# Desarrollo
npm run dev              # Inicia servidor de desarrollo

# Build
npm run build            # Build para producción
npm run preview          # Preview del build

# Calidad de código
npm run lint             # Ejecuta ESLint
npm run type-check       # Verifica tipos TypeScript
```

## 🔌 API Backend

El frontend se conecta al API Gateway en:
- **Development**: `http://localhost:15095`
- **Production**: `https://api.cardealer.com`

### Servicios Integrados

1. **AuthService** - Autenticación y autorización
2. **VehicleService** - Catálogo y búsqueda
3. **SearchService** - Búsqueda avanzada (Elasticsearch)
4. **MediaService** - Upload de imágenes
5. **UserService** - Gestión de usuarios
6. **ContactService** - Mensajería
7. **NotificationService** - Notificaciones
8. **AdminService** - Panel administrativo

## 🎨 Design System

### Colores

```typescript
Primary:   #00539F (Azul profesional)
Secondary: #0089FF (Azul brillante)
Accent:    #FF6B35 (Naranja llamativo)
```

### Tipografía

- **Heading**: Poppins (Bold, Semibold)
- **Body**: Inter (Regular, Medium)

### Componentes Base

- **Button**: `btn`, `btn-primary`, `btn-secondary`, `btn-outline`
- **Input**: `input`
- **Card**: `card`

## 📋 Roadmap - Sprints

| Sprint | Duración | Descripción |
|--------|----------|-------------|
| **Sprint 0** | 1 semana | Setup & Arquitectura ✅ |
| **Sprint 1** | 1 semana | Autenticación |
| **Sprint 2** | 1 semana | Home & Navigation |
| **Sprint 3** | 1.5 semanas | Vehicle Catalog |
| **Sprint 4** | 1.5 semanas | Vehicle Detail |
| **Sprint 5** | 1.5 semanas | Sell Vehicle |
| **Sprint 6** | 1 semana | User Dashboard |
| **Sprint 7** | 1 semana | Messages |
| **Sprint 8** | 1 semana | Admin Panel |
| **Sprint 9** | 1 semana | Testing & Polish |
| **Sprint 10** | 1 semana | Production Deploy |

**Total**: 11-13 semanas

## 🐳 Docker

### Build

```bash
docker build -t cardealer-frontend .
```

### Run

```bash
docker run -p 3000:80 cardealer-frontend
```

### Docker Compose

```bash
docker-compose up -d
```

## 🔐 Environment Variables

```bash
VITE_API_URL=http://localhost:15095
VITE_GATEWAY_URL=http://localhost:15095
VITE_CDN_URL=http://localhost:15095
VITE_ENVIRONMENT=development
VITE_ENABLE_ANALYTICS=false
VITE_ENABLE_ERROR_TRACKING=false
```

## 📚 Documentación

- [Sprint Plan](../FRONTEND_SPRINT_PLAN.md)
- [Design Analysis](../FRONTEND_DESIGN_ANALYSIS.md)
- [API Contracts](../FRONTEND_API_CONTRACTS.md)
- [Technical Specs](../FRONTEND_TECHNICAL_SPECS.md)

## 📊 Métricas de Éxito

- ✅ Lighthouse Performance > 90
- ✅ Lighthouse Accessibility > 90
- ✅ Bundle size < 500KB (gzip)
- ✅ Test coverage > 70%
- ✅ TTI < 3s
- ✅ FCP < 1.5s

## 🤝 Contribución

Ver el Sprint Plan para tareas pendientes y contribuir según la guía de sprints.

## 📄 Licencia

MIT

---

**Desarrollado con ❤️ por el equipo CarDealer**
