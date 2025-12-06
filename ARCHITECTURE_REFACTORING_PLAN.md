# 🏗️ PLAN DE REFACTORING ARQUITECTÓNICO - CARDEALER PLATFORM

**Fecha**: Diciembre 5, 2025  
**Tipo**: Refactorización Estratégica Multi-Rol y Multi-Plataforma  
**Alcance**: Backend (Microservicios) + Frontend (Web + Mobile)

---

## 📊 ANÁLISIS DE ESTADO ACTUAL

### Backend - Microservicios Existentes ✅
```
✅ AuthService          - Autenticación JWT
✅ UserService          - Gestión de usuarios
✅ RoleService          - RBAC (Role-Based Access Control)
✅ VehicleService       - Gestión de vehículos
✅ MessageBusService    - Comunicación asíncrona
✅ NotificationService  - Notificaciones push/email
✅ AdminService         - Operaciones administrativas
✅ UploadService        - Gestión de archivos/imágenes
✅ CacheService         - Redis caching
✅ Gateway              - API Gateway (Ocelot)
✅ SearchService        - Elasticsearch
... + 20 servicios más
```

### Frontend - Estado Actual 🔄
```
✅ Web Application (React + Vite + TypeScript)
   - HomePage, BrowsePage, VehicleDetailPage
   - UserDashboard, AdminDashboard
   - Messaging, Notifications
   - 9 Static pages (About, FAQ, Terms, etc.)
   
❌ Mobile Application (NO EXISTE)
❌ Dealer Panel (NO EXISTE)
❌ Multi-Platform Architecture (NO IMPLEMENTADA)
```

### Sistema de Roles Actual 🔄
```
Backend (RoleService):
✅ SuperAdmin, Admin, Manager, User, ReadOnly
✅ RBAC con Permissions granulares
✅ UserRoles (relación many-to-many)

Frontend:
⚠️ Solo valida "role" como string
⚠️ No hay distinción entre User, Dealer, Admin
⚠️ ProtectedRoute solo valida requireAdmin (boolean)
```

---

## 🎯 REQUERIMIENTOS DEL NUEVO SISTEMA

### Tipos de Usuario Identificados

#### 1. **Guest User** (No autenticado)
- **Acceso**: Búsqueda y visualización de vehículos
- **Limitaciones**: No puede contactar vendedores, guardar favoritos
- **Conversión**: Registro obligatorio para interactuar

#### 2. **Individual Seller** (Usuario registrado no-dealer)
- **Capacidades**:
  - Publicar hasta N vehículos (según plan)
  - Gestionar listados propios
  - Recibir mensajes de compradores
  - Panel básico de estadísticas
- **Limitaciones**: Sin herramientas profesionales
- **Plan**: Gratuito con límites

#### 3. **Dealer** (Usuario con membresía pagada)
- **Capacidades Exclusivas**:
  1. **📊 Analytics Dashboard**
     - Análisis de precios de mercado
     - Comparativa con competencia
     - Tendencias de venta por marca/modelo
     - ROI de publicaciones
  
  2. **🏭 Inventory Management**
     - Gestión masiva de inventario
     - Importación CSV/Excel
     - Integración con sistemas externos (DMS)
     - Alertas de stock bajo
  
  3. **💰 Billing & Invoicing**
     - Generación de facturas
     - Historial de transacciones
     - Reportes fiscales
     - Integración con contabilidad
  
  4. **🚀 Bulk Publishing**
     - Publicación masiva de vehículos
     - Templates pre-configurados
     - Auto-fill con VIN decoder
     - Programación de publicaciones
  
  5. **📢 Advertising Boost**
     - Destacar publicaciones (Featured)
     - Placement premium en búsquedas
     - Campañas publicitarias
     - Retargeting de visitantes
  
  6. **📈 Lead Management**
     - CRM integrado
     - Seguimiento de leads
     - Email automation
     - WhatsApp Business integration
  
  7. **🎨 Custom Branding**
     - Logo personalizado en listings
     - Página de dealer personalizada
     - Dominio personalizado (opcional)
  
  8. **📊 Advanced Reporting**
     - Reportes personalizables
     - Exportación a Excel/PDF
     - Métricas de rendimiento
     - A/B testing de anuncios

- **Planes Sugeridos**:
  - Basic Dealer: $99/mes (50 listings)
  - Pro Dealer: $199/mes (200 listings + analytics)
  - Enterprise: $499/mes (ilimitado + API access)

#### 4. **Platform Admin** (Administrador del sistema)
- **Acceso**: Panel admin existente + nuevas funciones
- **Capacidades**:
  - Gestión de usuarios/dealers
  - Aprobación de publicaciones
  - Moderación de contenido
  - Configuración del sistema
  - Gestión de planes y precios
  - Auditoría y reportes

---

## 🏗️ ARQUITECTURA PROPUESTA

### 1. Backend - Nuevos Microservicios Requeridos

#### **DealerService** (NUEVO) 🆕
```yaml
Responsabilidad: Gestión completa de dealers
Endpoints:
  - POST   /api/dealers/register           # Registro como dealer
  - GET    /api/dealers/{id}               # Info del dealer
  - PUT    /api/dealers/{id}               # Actualizar perfil
  - GET    /api/dealers/{id}/inventory     # Inventario completo
  - GET    /api/dealers/{id}/analytics     # Analytics dashboard
  - GET    /api/dealers/{id}/leads         # Lead management
  - POST   /api/dealers/{id}/bulk-upload   # Carga masiva
  - GET    /api/dealers/{id}/branding      # Configuración de marca

Base de Datos:
  - Dealers (id, userId, businessName, licenseNumber, verificationStatus)
  - DealerPlans (id, dealerId, planType, startDate, endDate, features)
  - DealerInventory (id, dealerId, vehicleId, acquisitionCost, margin)
  - DealerLeads (id, dealerId, leadSource, status, followUpDate)
```

#### **AnalyticsService** (NUEVO) 🆕
```yaml
Responsabilidad: Análisis de mercado y precios
Endpoints:
  - GET    /api/analytics/market-prices    # Precios de mercado
  - GET    /api/analytics/trends           # Tendencias por segmento
  - GET    /api/analytics/competitor       # Análisis competitivo
  - POST   /api/analytics/price-suggestion # Sugerencia de precio
  - GET    /api/analytics/roi/{vehicleId}  # ROI de publicación

Integraciones:
  - ElasticSearch para agregaciones
  - Machine Learning para predicción de precios
  - Web scraping de competidores (opcional)
```

#### **BillingService** (NUEVO) 🆕
```yaml
Responsabilidad: Facturación y pagos
Endpoints:
  - POST   /api/billing/subscriptions      # Crear suscripción
  - GET    /api/billing/invoices           # Listado de facturas
  - POST   /api/billing/invoices/{id}/pay  # Pagar factura
  - GET    /api/billing/reports            # Reportes fiscales
  - POST   /api/billing/credits/purchase   # Comprar créditos para ads

Integraciones:
  - Stripe/PayPal para pagos
  - Sistema de facturación electrónica
  - Webhook para renovaciones automáticas
```

#### **CampaignService** (NUEVO) 🆕
```yaml
Responsabilidad: Gestión de campañas publicitarias
Endpoints:
  - POST   /api/campaigns                  # Crear campaña
  - GET    /api/campaigns/{id}/performance # Rendimiento
  - POST   /api/campaigns/{id}/boost       # Impulsar publicación
  - GET    /api/campaigns/budget           # Gestión de presupuesto

Features:
  - Featured listings (destacados)
  - Premium placement (posición premium)
  - Banner ads (opcional)
  - Retargeting pixels
```

#### **LeadService** (NUEVO) 🆕
```yaml
Responsabilidad: CRM y gestión de leads
Endpoints:
  - POST   /api/leads                      # Crear lead
  - GET    /api/leads/{dealerId}           # Leads del dealer
  - PUT    /api/leads/{id}/status          # Actualizar estado
  - POST   /api/leads/{id}/notes           # Agregar notas
  - POST   /api/leads/automation           # Email automation

Features:
  - Lead scoring
  - Email sequences
  - SMS notifications
  - WhatsApp integration
```

#### **InventoryService** (NUEVO) 🆕
```yaml
Responsabilidad: Gestión de inventario para dealers
Endpoints:
  - POST   /api/inventory/bulk-import      # Importación masiva
  - GET    /api/inventory/{dealerId}       # Inventario del dealer
  - POST   /api/inventory/templates        # Templates personalizados
  - POST   /api/inventory/vin-decode       # Decodificar VIN
  - GET    /api/inventory/alerts           # Alertas de stock

Features:
  - Importación CSV/Excel
  - Integración con DMS (Dealer Management System)
  - Auto-fill con VIN decoder API
  - Alertas automáticas
```

### 2. Backend - Modificaciones a Servicios Existentes

#### **AuthService** (MODIFICAR) 🔧
```yaml
Cambios:
  - Agregar campo "accountType" en JWT claims
    * guest, individual, dealer, admin
  - Middleware para validar nivel de acceso
  - Refresh token con claims actualizados

Claims JWT actuales:
  - userId, email, role (string)
  
Claims JWT nuevos:
  - userId, email, roles[] (array)
  - accountType (guest/individual/dealer/admin)
  - dealerId (si es dealer)
  - permissions[] (array de permisos granulares)
  - subscription { plan, expiresAt, features[] }
```

#### **UserService** (MODIFICAR) 🔧
```yaml
Cambios:
  - Agregar tabla "UserSubscriptions"
  - Endpoint para upgrade a dealer
  - Validación de límites por plan
  
Nuevos Endpoints:
  - POST   /api/users/upgrade-to-dealer
  - GET    /api/users/{id}/subscription
  - GET    /api/users/{id}/usage-limits
```

#### **VehicleService** (MODIFICAR) 🔧
```yaml
Cambios:
  - Agregar campo "dealerId" opcional
  - Bulk operations para dealers
  - Featured/Boosted listings
  
Nuevos Endpoints:
  - POST   /api/vehicles/bulk              # Publicación masiva
  - POST   /api/vehicles/{id}/boost        # Destacar vehículo
  - GET    /api/vehicles/featured          # Obtener destacados
```

#### **AdminService** (MODIFICAR) 🔧
```yaml
Cambios:
  - Dashboard con métricas de dealers
  - Gestión de planes y precios
  - Aprobación de dealers
  
Nuevos Endpoints:
  - GET    /api/admin/dealers              # Listado de dealers
  - POST   /api/admin/dealers/{id}/verify  # Verificar dealer
  - GET    /api/admin/revenue              # Dashboard de ingresos
  - POST   /api/admin/plans                # Gestionar planes
```

---

## 📱 ARQUITECTURA FRONTEND - MULTI-PLATAFORMA

### Estructura Propuesta

```
frontend/
├── packages/                    # Monorepo con pnpm/yarn workspaces
│   ├── shared/                  # Código compartido entre plataformas
│   │   ├── types/              # TypeScript interfaces/types
│   │   ├── utils/              # Funciones utilitarias
│   │   ├── hooks/              # React hooks reutilizables
│   │   ├── api/                # Servicios API (axios)
│   │   ├── constants/          # Constantes compartidas
│   │   └── validation/         # Schemas de validación (Zod)
│   │
│   ├── web/                    # Aplicación Web (actual)
│   │   ├── src/
│   │   │   ├── pages/
│   │   │   │   ├── public/    # Páginas públicas
│   │   │   │   ├── user/      # Panel de usuario
│   │   │   │   ├── dealer/    # Panel de dealer (NUEVO)
│   │   │   │   └── admin/     # Panel de admin
│   │   │   ├── components/
│   │   │   ├── layouts/
│   │   │   └── styles/
│   │   └── vite.config.ts
│   │
│   ├── mobile/                 # Aplicación Mobile (NUEVO)
│   │   ├── src/
│   │   │   ├── screens/       # Pantallas de React Native
│   │   │   ├── components/    # Componentes mobile
│   │   │   ├── navigation/    # React Navigation
│   │   │   └── styles/        # Styles mobile
│   │   ├── android/
│   │   ├── ios/
│   │   └── app.json
│   │
│   └── ui-components/         # Componentes UI compartidos
│       ├── Button/
│       ├── Input/
│       ├── Card/
│       └── ... (más componentes)
│
└── package.json               # Root package.json (workspaces)
```

### Tecnologías Frontend

#### Web (Mantener y mejorar)
```yaml
Stack Actual:
  - React 18
  - Vite
  - TypeScript
  - Tailwind CSS
  - Zustand (state)
  - React Router v6
  - Axios
  - React Hook Form + Zod

Agregar:
  - TanStack Query (react-query) para data fetching
  - Recharts para gráficos (analytics)
  - React Table para tablas complejas
  - React Dropzone para uploads
  - Socket.io client para real-time
```

#### Mobile (NUEVO)
```yaml
Stack Propuesto:
  - React Native (no Expo, bare workflow)
  - TypeScript
  - React Navigation 6
  - NativeWind (Tailwind para RN)
  - Zustand (state compartido con web)
  - Axios (compartido con web)
  - React Hook Form + Zod (compartido)
  
Librerías Nativas:
  - react-native-camera (fotos de vehículos)
  - react-native-image-picker
  - react-native-maps (ubicación)
  - react-native-push-notification
  - react-native-biometrics (autenticación)
```

### Componentes Compartidos

#### Shared Package (`packages/shared`)
```typescript
// packages/shared/types/user.types.ts
export enum AccountType {
  GUEST = 'guest',
  INDIVIDUAL = 'individual',
  DEALER = 'dealer',
  ADMIN = 'admin'
}

export enum DealerPlan {
  BASIC = 'basic',
  PRO = 'pro',
  ENTERPRISE = 'enterprise'
}

export interface User {
  id: string;
  email: string;
  accountType: AccountType;
  roles: string[];
  dealer?: DealerInfo;
  subscription?: Subscription;
}

export interface DealerInfo {
  id: string;
  businessName: string;
  licenseNumber: string;
  verificationStatus: 'pending' | 'verified' | 'rejected';
  plan: DealerPlan;
}

export interface Subscription {
  plan: DealerPlan;
  startDate: string;
  endDate: string;
  features: string[];
  limits: {
    maxListings: number;
    maxImages: number;
    analyticsAccess: boolean;
    bulkUpload: boolean;
    featuredListings: number;
  };
}

// packages/shared/api/dealerService.ts
import { apiClient } from './client';
import type { DealerInfo, DealerAnalytics } from '../types';

export const dealerService = {
  async getProfile(dealerId: string): Promise<DealerInfo> {
    const response = await apiClient.get(`/api/dealers/${dealerId}`);
    return response.data;
  },
  
  async getAnalytics(dealerId: string): Promise<DealerAnalytics> {
    const response = await apiClient.get(`/api/dealers/${dealerId}/analytics`);
    return response.data;
  },
  
  async bulkUpload(dealerId: string, vehicles: any[]): Promise<void> {
    await apiClient.post(`/api/dealers/${dealerId}/bulk-upload`, { vehicles });
  },
  
  // ... más funciones
};
```

---

## 🚀 PLAN DE IMPLEMENTACIÓN POR FASES

### **FASE 1: Foundation & Architecture** (2 semanas)

#### Backend
- [ ] Crear `DealerService` (estructura básica)
- [ ] Modificar `AuthService` para multi-rol
- [ ] Actualizar JWT claims (accountType, dealerId)
- [ ] Crear migrations para tablas de Dealers
- [ ] Seed data con planes básicos

#### Frontend
- [ ] Restructurar proyecto a monorepo
- [ ] Crear `packages/shared` con types y API
- [ ] Refactorizar authStore con AccountType
- [ ] Actualizar ProtectedRoute para multi-rol
- [ ] Crear DealerLayout (similar a AdminLayout)

**Entregables**:
- Microservicios base creados
- Monorepo funcionando
- Autenticación multi-rol operativa

---

### **FASE 2: Dealer Panel - Core Features** (3 semanas)

#### Backend
- [ ] Implementar `DealerService` completo
- [ ] Crear `InventoryService` con bulk upload
- [ ] Modificar `VehicleService` para dealers
- [ ] Implementar sistema de planes (BillingService básico)

#### Frontend Web
- [ ] Crear `/dealer` routes
- [ ] DealerDashboard (overview con stats)
- [ ] DealerInventoryPage (gestión de inventario)
- [ ] DealerListingsPage (publicaciones)
- [ ] BulkUploadPage (importación CSV)
- [ ] DealerSettingsPage (configuración)

**Entregables**:
- Panel dealer funcional
- Publicación masiva operativa
- Gestión de inventario básica

---

### **FASE 3: Analytics & Advanced Features** (2 semanas)

#### Backend
- [ ] Crear `AnalyticsService`
- [ ] Implementar price analysis (ML opcional)
- [ ] Crear `CampaignService` (featured listings)
- [ ] Integrar Elasticsearch para analytics

#### Frontend Web
- [ ] DealerAnalyticsPage (gráficos con Recharts)
- [ ] Price Analysis Tool
- [ ] Market Trends Dashboard
- [ ] Competitor Comparison
- [ ] ROI Calculator

**Entregables**:
- Analytics dashboard funcional
- Herramienta de análisis de precios
- Featured listings operativo

---

### **FASE 4: Billing & Subscriptions** (2 semanas)

#### Backend
- [ ] Completar `BillingService`
- [ ] Integración con Stripe/PayPal
- [ ] Sistema de renovación automática
- [ ] Facturación electrónica
- [ ] Webhooks de pagos

#### Frontend Web
- [ ] DealerBillingPage (facturas)
- [ ] SubscriptionPage (planes y upgrades)
- [ ] PaymentMethodsPage (métodos de pago)
- [ ] InvoicesPage (historial)

**Entregables**:
- Sistema de suscripciones completo
- Pagos funcionando
- Facturación automática

---

### **FASE 5: Lead Management & CRM** (2 semanas)

#### Backend
- [ ] Crear `LeadService`
- [ ] Email automation
- [ ] SMS integration
- [ ] WhatsApp Business API

#### Frontend Web
- [ ] DealerLeadsPage (CRM básico)
- [ ] Lead detail modal
- [ ] Email templates
- [ ] Automation workflows

**Entregables**:
- CRM integrado
- Lead scoring
- Email automation

---

### **FASE 6: Mobile Application** (4 semanas)

#### Setup
- [ ] Inicializar React Native project
- [ ] Configurar navegación (React Navigation)
- [ ] Setup NativeWind (Tailwind)
- [ ] Compartir API services con web

#### Features Core
- [ ] Login/Register screens
- [ ] Vehicle browse con scroll infinito
- [ ] Vehicle detail screen
- [ ] Search con filtros
- [ ] Favorites
- [ ] Messaging

#### Features Dealer (Mobile)
- [ ] Dealer dashboard (stats)
- [ ] Add vehicle con cámara
- [ ] Inventory list
- [ ] Leads management
- [ ] Push notifications

**Entregables**:
- App Android funcional
- App iOS funcional
- Feature parity con web (80%)

---

### **FASE 7: Advanced Dealer Tools** (2 semanas)

#### Backend
- [ ] VIN decoder API integration
- [ ] DMS integration endpoints
- [ ] Custom branding system
- [ ] White-label options

#### Frontend
- [ ] Custom branding page
- [ ] VIN decoder tool
- [ ] Template builder
- [ ] Advanced reporting

**Entregables**:
- VIN decoder funcionando
- Custom branding
- Templates avanzados

---

### **FASE 8: Testing, Optimization & Launch** (2 semanas)

#### Backend
- [ ] Unit tests (>80% coverage)
- [ ] Integration tests
- [ ] Load testing (k6)
- [ ] Security audit

#### Frontend
- [ ] E2E tests (Playwright)
- [ ] Performance optimization
- [ ] Accessibility audit (WCAG)
- [ ] Mobile testing (physical devices)

#### DevOps
- [ ] CI/CD pipelines
- [ ] Monitoring (Grafana)
- [ ] Logging (ELK)
- [ ] Documentation

**Entregables**:
- Sistema completo testeado
- Documentación completa
- Listo para producción

---

## 📊 ESTIMACIONES Y RECURSOS

### Timeline Total: **19 semanas (≈4.5 meses)**

### Equipo Sugerido
```
Backend:
  - 2 Senior .NET Developers
  - 1 DevOps Engineer

Frontend:
  - 2 Senior React Developers
  - 1 React Native Developer
  - 1 UI/UX Designer

QA:
  - 1 QA Engineer

Total: 8 personas
```

### Costos Estimados (Ejemplo)
```
Desarrollo: $120,000 - $180,000
Infraestructura (AWS/Azure): $2,000/mes
Third-party APIs:
  - Stripe: 2.9% + $0.30 por transacción
  - VIN Decoder API: $0.01 por VIN
  - SMS Gateway: $0.05 por SMS
  - WhatsApp Business: $0.005 por mensaje
```

---

## 🎯 MÉTRICAS DE ÉXITO

### KPIs Técnicos
- [ ] 99.9% uptime
- [ ] <200ms response time (p95)
- [ ] >80% test coverage
- [ ] 0 critical security vulnerabilities

### KPIs de Negocio
- [ ] 30% conversión guest → individual
- [ ] 10% conversión individual → dealer
- [ ] $150 MRR promedio por dealer
- [ ] <5% churn rate mensual

---

## 🔐 CONSIDERACIONES DE SEGURIDAD

### Autenticación Multi-Rol
```typescript
// Middleware de autorización por accountType
export const requireAccountType = (...types: AccountType[]) => {
  return (req, res, next) => {
    const { accountType } = req.user;
    if (!types.includes(accountType)) {
      return res.status(403).json({ error: 'Insufficient permissions' });
    }
    next();
  };
};

// Uso:
app.get('/api/dealer/analytics', 
  authenticate,
  requireAccountType(AccountType.DEALER, AccountType.ADMIN),
  getDealerAnalytics
);
```

### Rate Limiting por Tipo de Usuario
```yaml
Guest: 100 requests/hour
Individual: 500 requests/hour
Dealer: 2000 requests/hour
Admin: unlimited
```

---

## 📝 PRÓXIMOS PASOS INMEDIATOS

1. **Decisión Arquitectónica**: Aprobar estructura de monorepo
2. **Priorización**: Definir qué features de dealer son MVP
3. **Diseño UI/UX**: Mockups del dealer panel
4. **Setup Inicial**: Crear estructura del monorepo
5. **Backend**: Comenzar DealerService + modificar AuthService

---

## 🤔 PREGUNTAS PENDIENTES

1. ¿Qué features de dealer son **obligatorias** para el MVP?
2. ¿Mobile debe lanzarse junto con dealer panel o después?
3. ¿Cuál es el precio objetivo para cada plan de dealer?
4. ¿Necesitamos white-label para enterprise dealers?
5. ¿Integramos con DMS existentes (CDK, Reynolds)?

---

**Documento preparado por**: GitHub Copilot  
**Fecha**: Diciembre 5, 2025  
**Versión**: 1.0
