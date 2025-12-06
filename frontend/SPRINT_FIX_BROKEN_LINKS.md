# 🔧 Sprint Fix: Broken Links & Missing Pages

**Fecha**: Diciembre 5, 2025  
**Objetivo**: Corregir todos los enlaces rotos y crear páginas faltantes  
**Duración Estimada**: 2-3 días  
**Prioridad**: ALTA 🔴

---

## 📋 ANÁLISIS DE PROBLEMAS ENCONTRADOS

### 1. ❌ PÁGINAS NO EXISTENTES - Footer Links

El Footer tiene múltiples links que apuntan a páginas que no existen:

```tsx
// Footer.tsx - Links rotos:
<Link to="/about">About Us</Link>           // ❌ No existe
<Link to="/how-it-works">How It Works</Link> // ❌ No existe
<Link to="/pricing">Pricing</Link>          // ❌ No existe
<Link to="/faq">FAQ</Link>                  // ❌ No existe
<Link to="/contact">Contact Us</Link>       // ❌ No existe
<Link to="/help">Help Center</Link>         // ❌ No existe
<Link to="/terms">Terms of Service</Link>   // ❌ No existe
<Link to="/privacy">Privacy Policy</Link>   // ❌ No existe
<Link to="/cookies">Cookie Policy</Link>    // ❌ No existe
```

**Impacto**: ⚠️ Usuarios hacen clic y llegan a página 404

---

### 2. ⚠️ AUTENTICACIÓN MOCK vs API REAL

**Problema actual**:
```typescript
// authService.ts usa datos MOCK hardcodeados
const MOCK_USERS = [
  { email: 'demo@cardealer.com', password: 'demo123' },
  { email: 'admin@cardealer.com', password: 'admin123' }
];
```

**Necesita**:
- Integración con backend real (AuthService API)
- Endpoints: `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh-token`
- Manejo de errores de API real
- Refresh token automático

---

### 3. ⚠️ DATOS MOCK en Vistas

**Archivos usando datos mock**:
```
- HomePage.tsx → mockVehicles (debería llamar API)
- BrowsePage.tsx → mockVehicles (debería llamar API)
- VehicleDetailPage.tsx → mockVehicles.find() (debería llamar API)
- AdminDashboardPage.tsx → mockAdminStats, mockActivityLogs
- PendingApprovalsPage.tsx → mockPendingVehicles
- UsersManagementPage.tsx → mockUsers
- MessagesPage.tsx → mockConversations
```

**Impacto**: 📊 Los datos no son reales, no persisten

---

### 4. ⚠️ FUNCIONALIDADES SIN IMPLEMENTAR

#### a) HomePage - Search Functionality
```tsx
const handleSearch = (filters: unknown) => {
  console.log('Search filters:', filters);
  // TODO: Navigate to browse page with filters  ❌
};
```

#### b) VehicleDetailPage - Seller Phone Link
```tsx
<Link to={`tel:${vehicle.seller.phone}`}>  // ❌ Usar <a href>
```

#### c) Notificaciones
```tsx
<NotificationDropdown /> // Componente existe pero no funciona con API real
```

#### d) Mensajería
- MessagesPage usa datos mock
- No hay WebSocket para mensajes en tiempo real
- No integra con backend real

---

### 5. 🔴 API ENDPOINTS NO CONFIGURADOS

**API Base**: `http://localhost:15095`

**Endpoints necesarios pero no implementados en servicios**:
```typescript
// Faltantes en vehicleService:
- POST /api/vehicles (crear vehículo)
- PUT /api/vehicles/:id (editar vehículo)
- DELETE /api/vehicles/:id (eliminar vehículo)
- GET /api/vehicles/my-listings (mis publicaciones)
- POST /api/vehicles/:id/favorite (agregar favorito)
- DELETE /api/vehicles/:id/favorite (quitar favorito)
- GET /api/vehicles/favorites (obtener favoritos)

// Faltantes en authService:
- POST /api/auth/logout
- GET /api/auth/me (obtener usuario actual)
- PUT /api/auth/profile (actualizar perfil)
- POST /api/auth/change-password

// Faltantes completamente:
- messageService (mensajes)
- adminService (estadísticas admin)
- notificationService (notificaciones)
- uploadService (subir imágenes)
```

---

### 6. ⚠️ RUTAS NO DEFINIDAS EN App.tsx

Rutas que existen en links pero no en router:
```tsx
// Faltantes en App.tsx:
<Route path="/about" />
<Route path="/how-it-works" />
<Route path="/pricing" />
<Route path="/faq" />
<Route path="/contact" />
<Route path="/help" />
<Route path="/terms" />
<Route path="/privacy" />
<Route path="/cookies" />
<Route path="/wishlist/shared" />  // Para compartir wishlist
```

---

### 7. ⚠️ PROTECTED ROUTES - Problemas Potenciales

```tsx
<ProtectedRoute requireAdmin>
```

**Verificar**:
- ¿Redirige correctamente cuando no autenticado?
- ¿Valida rol de admin correctamente?
- ¿Maneja loading states?
- ¿Persiste autenticación en refresh?

---

## 🎯 PLAN DE CORRECCIÓN

### **FASE 1: Páginas Estáticas (Día 1 - 4 horas)**

#### Crear páginas informativas:

1. **AboutPage.tsx**
   - Historia de la empresa
   - Misión y visión
   - Equipo

2. **HowItWorksPage.tsx**
   - Pasos para comprar
   - Pasos para vender
   - Proceso de verificación

3. **PricingPage.tsx**
   - Planes de publicación
   - Features por plan
   - Comparación

4. **FAQPage.tsx**
   - Preguntas frecuentes
   - Búsqueda de preguntas
   - Categorías

5. **ContactPage.tsx**
   - Formulario de contacto
   - Información de contacto
   - Mapa (opcional)

6. **HelpCenterPage.tsx**
   - Centro de ayuda
   - Artículos por categorías
   - Búsqueda

7. **TermsPage.tsx**
   - Términos de servicio
   - Condiciones de uso

8. **PrivacyPage.tsx**
   - Política de privacidad
   - Uso de datos
   - GDPR compliance

9. **CookiesPage.tsx**
   - Política de cookies
   - Configuración de cookies

**Checklist**:
- [ ] Crear 9 páginas estáticas
- [ ] Agregar rutas en App.tsx
- [ ] Diseño consistente con MainLayout
- [ ] Responsive design
- [ ] SEO meta tags

---

### **FASE 2: Integración API Real (Día 1-2 - 8 horas)**

#### 2.1 Configurar Variables de Entorno
```bash
# .env.development
VITE_API_URL=http://localhost:15095
VITE_GATEWAY_URL=http://localhost:5000  # Si tienes API Gateway
VITE_AUTH_SERVICE_URL=http://localhost:15090
VITE_VEHICLE_SERVICE_URL=http://localhost:15091
```

#### 2.2 Actualizar authService.ts
```typescript
// Eliminar MOCK_USERS
// Conectar con endpoints reales:
- POST /api/auth/login
- POST /api/auth/register
- POST /api/auth/refresh-token
- POST /api/auth/logout
- GET /api/auth/me
```

#### 2.3 Crear vehicleService.ts completo
```typescript
export const vehicleService = {
  // CRUD completo
  getAll(filters),
  getById(id),
  create(data),
  update(id, data),
  delete(id),
  
  // Favoritos
  addFavorite(id),
  removeFavorite(id),
  getFavorites(),
  
  // Búsqueda
  search(query, filters),
  
  // Admin
  approve(id),
  reject(id),
  getPending()
};
```

#### 2.4 Crear messageService.ts
```typescript
export const messageService = {
  getConversations(),
  getMessages(conversationId),
  sendMessage(conversationId, message),
  markAsRead(conversationId),
  createConversation(vehicleId, sellerId)
};
```

#### 2.5 Crear adminService.ts
```typescript
export const adminService = {
  getStats(),
  getActivityLogs(),
  getUsers(),
  banUser(id),
  unbanUser(id),
  deleteUser(id)
};
```

#### 2.6 Crear notificationService.ts
```typescript
export const notificationService = {
  getNotifications(),
  markAsRead(id),
  markAllAsRead(),
  deleteNotification(id)
};
```

#### 2.7 Crear uploadService.ts
```typescript
export const uploadService = {
  uploadImage(file): Promise<string>,  // Devuelve URL
  uploadMultipleImages(files): Promise<string[]>,
  deleteImage(url)
};
```

**Checklist**:
- [ ] Configurar variables de entorno
- [ ] Actualizar authService con API real
- [ ] Crear todos los servicios faltantes
- [ ] Probar autenticación completa
- [ ] Probar CRUD de vehículos
- [ ] Probar upload de imágenes
- [ ] Manejo de errores en todos los servicios

---

### **FASE 3: Actualizar Componentes con API Real (Día 2 - 6 horas)**

#### 3.1 HomePage
```tsx
// Eliminar: const featuredVehicles = mockVehicles.filter()
// Reemplazar con:
const { data: featuredVehicles } = useQuery({
  queryKey: ['vehicles', 'featured'],
  queryFn: () => vehicleService.getAll({ isFeatured: true, limit: 8 })
});
```

#### 3.2 BrowsePage
```tsx
// Usar react-query para paginación
const { data, isLoading } = useQuery({
  queryKey: ['vehicles', filters, page],
  queryFn: () => vehicleService.getAll({ ...filters, page })
});
```

#### 3.3 VehicleDetailPage
```tsx
// Usar useParams + useQuery
const { id } = useParams();
const { data: vehicle } = useQuery({
  queryKey: ['vehicle', id],
  queryFn: () => vehicleService.getById(id!)
});
```

#### 3.4 MessagesPage
```tsx
// Usar API real + WebSocket (opcional)
const { data: conversations } = useQuery({
  queryKey: ['conversations'],
  queryFn: messageService.getConversations
});
```

#### 3.5 Admin Pages
```tsx
// AdminDashboardPage
const { data: stats } = useQuery(['admin', 'stats'], adminService.getStats);

// PendingApprovalsPage
const { data: pending } = useQuery(['vehicles', 'pending'], vehicleService.getPending);

// UsersManagementPage
const { data: users } = useQuery(['admin', 'users'], adminService.getUsers);
```

#### 3.6 Fix Phone Link
```tsx
// VehicleDetailPage.tsx
// Cambiar:
<Link to={`tel:${vehicle.seller.phone}`}>

// Por:
<a href={`tel:${vehicle.seller.phone}`}>
```

#### 3.7 Implementar Search Navigation
```tsx
// HomePage.tsx
const navigate = useNavigate();

const handleSearch = (filters: SearchFilters) => {
  const queryParams = new URLSearchParams(filters).toString();
  navigate(`/browse?${queryParams}`);
};
```

**Checklist**:
- [ ] HomePage con API real
- [ ] BrowsePage con API real
- [ ] VehicleDetailPage con API real
- [ ] MessagesPage con API real
- [ ] Admin pages con API real
- [ ] Fix phone link a `<a href>`
- [ ] Implementar search navigation
- [ ] Loading states en todos los componentes
- [ ] Error boundaries

---

### **FASE 4: Notificaciones y Mensajería (Día 3 - 4 horas)**

#### 4.1 NotificationDropdown
```tsx
// Conectar con API real
const { data: notifications } = useQuery({
  queryKey: ['notifications'],
  queryFn: notificationService.getNotifications,
  refetchInterval: 30000  // Cada 30 segundos
});
```

#### 4.2 MessagesPage - Real-time (Opcional)
```typescript
// Usar WebSocket o polling
useEffect(() => {
  const ws = new WebSocket('ws://localhost:15095/ws/messages');
  
  ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    queryClient.invalidateQueries(['conversations']);
  };
  
  return () => ws.close();
}, []);
```

**Checklist**:
- [ ] NotificationDropdown con API
- [ ] Refresh automático de notificaciones
- [ ] Badge de notificaciones no leídas
- [ ] Mensajería con API
- [ ] WebSocket o polling (opcional)

---

### **FASE 5: Testing & Validación (Día 3 - 2 horas)**

#### 5.1 Verificar Rutas
- [ ] Todas las rutas del Footer funcionan
- [ ] Todas las rutas del Navbar funcionan
- [ ] Links internos funcionan
- [ ] Redirect a login cuando no autenticado
- [ ] Redirect a 404 cuando ruta no existe

#### 5.2 Verificar Autenticación
- [ ] Login funciona con API
- [ ] Register funciona con API
- [ ] Logout funciona
- [ ] Refresh token automático
- [ ] Persiste sesión en refresh
- [ ] Protected routes funcionan
- [ ] Admin routes verifican rol

#### 5.3 Verificar Funcionalidades
- [ ] CRUD vehículos completo
- [ ] Upload de imágenes funciona
- [ ] Favoritos funcionan
- [ ] Búsqueda funciona
- [ ] Filtros funcionan
- [ ] Paginación funciona
- [ ] Mensajería funciona (básica)
- [ ] Notificaciones funcionan
- [ ] Admin panel funciona

#### 5.4 Testing Manual
```
1. Usuario no autenticado:
   - ✓ Puede ver home
   - ✓ Puede ver browse
   - ✓ Puede ver detalle de vehículo
   - ✗ No puede acceder a dashboard
   - ✗ No puede acceder a messages
   - ✗ No puede acceder a sell

2. Usuario autenticado:
   - ✓ Puede hacer todo lo anterior
   - ✓ Puede crear listing
   - ✓ Puede editar su listing
   - ✓ Puede eliminar su listing
   - ✓ Puede enviar mensajes
   - ✓ Puede agregar favoritos
   - ✗ No puede acceder a admin

3. Usuario admin:
   - ✓ Puede hacer todo lo anterior
   - ✓ Puede acceder a admin panel
   - ✓ Puede aprobar vehículos
   - ✓ Puede rechazar vehículos
   - ✓ Puede banear usuarios
```

---

## 📦 ENTREGABLES FINALES

### Archivos Nuevos a Crear:
```
src/pages/
  ├── AboutPage.tsx
  ├── HowItWorksPage.tsx
  ├── PricingPage.tsx
  ├── FAQPage.tsx
  ├── ContactPage.tsx
  ├── HelpCenterPage.tsx
  ├── TermsPage.tsx
  ├── PrivacyPage.tsx
  └── CookiesPage.tsx

src/services/
  ├── endpoints/
  │   ├── vehicleService.ts (actualizar)
  │   ├── messageService.ts (crear)
  │   ├── adminService.ts (crear)
  │   ├── notificationService.ts (crear)
  │   └── uploadService.ts (crear)
  └── authService.ts (actualizar)
```

### Archivos a Modificar:
```
src/
  ├── App.tsx (agregar 9 rutas)
  ├── pages/
  │   ├── HomePage.tsx (usar API)
  │   ├── BrowsePage.tsx (usar API)
  │   ├── VehicleDetailPage.tsx (usar API, fix phone)
  │   ├── MessagesPage.tsx (usar API)
  │   ├── admin/
  │   │   ├── AdminDashboardPage.tsx (usar API)
  │   │   ├── PendingApprovalsPage.tsx (usar API)
  │   │   └── UsersManagementPage.tsx (usar API)
  └── components/
      └── organisms/
          └── NotificationDropdown.tsx (usar API)
```

---

## 🎯 ORDEN DE IMPLEMENTACIÓN RECOMENDADO

### Día 1 (Morning):
1. ✅ Crear 9 páginas estáticas básicas
2. ✅ Agregar rutas en App.tsx
3. ✅ Verificar navegación funciona

### Día 1 (Afternoon):
4. ✅ Configurar variables de entorno
5. ✅ Actualizar authService.ts con API real
6. ✅ Probar login/register/logout

### Día 2 (Morning):
7. ✅ Crear vehicleService.ts completo
8. ✅ Crear uploadService.ts
9. ✅ Actualizar HomePage, BrowsePage, VehicleDetailPage

### Día 2 (Afternoon):
10. ✅ Crear messageService.ts
11. ✅ Crear adminService.ts
12. ✅ Actualizar MessagesPage y Admin pages

### Día 3 (Morning):
13. ✅ Crear notificationService.ts
14. ✅ Actualizar NotificationDropdown
15. ✅ Fix phone link y search navigation

### Día 3 (Afternoon):
16. ✅ Testing completo
17. ✅ Fix bugs encontrados
18. ✅ Documentación final

---

## 🔥 ISSUES CRÍTICOS A RESOLVER PRIMERO

**Prioridad 1** (Bloquea funcionalidad):
1. 🔴 Autenticación con API real
2. 🔴 CRUD de vehículos con API
3. 🔴 Upload de imágenes

**Prioridad 2** (Afecta UX):
4. 🟠 Páginas 404 en Footer
5. 🟠 Search navigation
6. 🟠 Phone link roto

**Prioridad 3** (Features secundarias):
7. 🟡 Mensajería real-time
8. 🟡 Notificaciones automáticas
9. 🟡 Contenido de páginas estáticas

---

## ✅ CHECKLIST FINAL

### Backend Ready:
- [ ] AuthService API está corriendo
- [ ] VehicleService API está corriendo
- [ ] API Gateway configurado (si aplica)
- [ ] Base de datos con datos de prueba
- [ ] CORS configurado correctamente

### Frontend Complete:
- [ ] Todas las rutas definidas
- [ ] Todas las páginas existen
- [ ] Todos los servicios implementados
- [ ] Autenticación funciona end-to-end
- [ ] CRUD vehículos funciona
- [ ] Upload imágenes funciona
- [ ] No hay links rotos
- [ ] No hay console.errors
- [ ] Loading states implementados
- [ ] Error handling implementado

### Testing:
- [ ] Flujo completo de registro
- [ ] Flujo completo de login
- [ ] Flujo completo de crear vehículo
- [ ] Flujo completo de editar vehículo
- [ ] Flujo completo de favoritos
- [ ] Flujo completo de mensajes
- [ ] Flujo completo admin (aprobar/rechazar)
- [ ] Navegación completa sin errores

---

## 📝 NOTAS IMPORTANTES

1. **API URLs**: Asegúrate que los servicios del backend estén corriendo antes de probar
2. **CORS**: Verifica que el backend permita requests desde `localhost:5173`
3. **Token Storage**: Los tokens se guardan en `localStorage`
4. **Refresh Token**: Se maneja automáticamente en `api.ts` interceptor
5. **Protected Routes**: Verifican `isAuthenticated` del store
6. **Admin Routes**: Verifican `user.role === 'admin'`

---

## 🚀 COMANDOS ÚTILES

```bash
# Verificar backend está corriendo
curl http://localhost:15095/health

# Verificar auth service
curl http://localhost:15090/health

# Verificar vehicle service
curl http://localhost:15091/health

# Ver logs del frontend
npm run dev

# Build de producción
npm run build
npm run preview
```

---

**FIN DEL PLAN DE CORRECCIÓN** 🎯

**Próximo paso**: ¿Empezamos con la FASE 1 (páginas estáticas) o prefieres comenzar con la FASE 2 (integración API)?
