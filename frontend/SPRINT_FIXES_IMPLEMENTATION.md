# 🚀 Sprint de Correcciones Frontend - Plan de Implementación

**Sprint**: Frontend Fixes & Missing Features  
**Fecha Inicio**: Diciembre 5, 2025  
**Duración**: 3 días  
**Objetivo**: Corregir todos los enlaces rotos, integrar APIs reales y completar funcionalidades faltantes

---

## 📊 OVERVIEW DEL SPRINT

### Métricas del Sprint:
- **Tasks**: 15 principales
- **Story Points**: 21 puntos
- **Archivos a crear**: 14 nuevos
- **Archivos a modificar**: 12 existentes
- **Prioridad**: 🔴 CRÍTICA

### Distribución de Tareas:
- **Día 1**: 7 tasks (Setup + Páginas)
- **Día 2**: 5 tasks (APIs + Servicios)
- **Día 3**: 3 tasks (Integration + Testing)

---

## 🎯 DÍA 1: FUNDAMENTOS Y PÁGINAS ESTÁTICAS

### Task 1.1: Variables de Entorno y Configuración ⭐ (1 SP)
**Duración**: 30 minutos  
**Prioridad**: 🔴 CRÍTICA

**Acción**:
```bash
# Actualizar .env.development
VITE_API_URL=http://localhost:15095
VITE_AUTH_SERVICE_URL=http://localhost:15090
VITE_VEHICLE_SERVICE_URL=http://localhost:15091
VITE_MESSAGE_SERVICE_URL=http://localhost:15092
VITE_ADMIN_SERVICE_URL=http://localhost:15093
VITE_UPLOAD_SERVICE_URL=http://localhost:15094
VITE_GATEWAY_URL=http://localhost:5000
```

**Archivos**:
- `.env.development` (actualizar)
- `.env.example` (actualizar)

**Verificación**:
- [ ] Variables están en .env.development
- [ ] .env.example tiene todas las variables
- [ ] Vite reconoce las variables (import.meta.env.VITE_*)

---

### Task 1.2: Crear Página About ⭐ (1 SP)
**Duración**: 45 minutos  
**Prioridad**: 🟠 ALTA

**Archivo**: `src/pages/AboutPage.tsx`

**Estructura**:
```tsx
import MainLayout from '@/layouts/MainLayout';

export default function AboutPage() {
  return (
    <MainLayout>
      {/* Hero Section */}
      <div className="bg-gradient-to-br from-primary to-secondary text-white py-20">
        <div className="max-w-7xl mx-auto px-4">
          <h1>About CarDealer</h1>
          <p>Your trusted automotive marketplace</p>
        </div>
      </div>

      {/* Mission Section */}
      <section className="py-16">
        <div className="max-w-7xl mx-auto px-4">
          <h2>Our Mission</h2>
          <p>Connect buyers and sellers...</p>
        </div>
      </section>

      {/* Values Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4">
          <h2>Our Values</h2>
          <div className="grid md:grid-cols-3 gap-8">
            {/* Trust, Quality, Service */}
          </div>
        </div>
      </section>

      {/* Team Section */}
      <section className="py-16">
        <h2>Meet Our Team</h2>
        {/* Team cards */}
      </section>

      {/* CTA Section */}
      <section className="bg-primary text-white py-16">
        <h2>Ready to Get Started?</h2>
        <Button>Browse Cars</Button>
      </section>
    </MainLayout>
  );
}
```

**Verificación**:
- [ ] Página renderiza correctamente
- [ ] Responsive design funciona
- [ ] Links internos funcionan
- [ ] SEO meta tags incluidos

---

### Task 1.3: Crear Página How It Works ⭐ (1 SP)
**Duración**: 45 minutos  
**Archivo**: `src/pages/HowItWorksPage.tsx`

**Secciones**:
1. Hero con título
2. Para Compradores (3 pasos)
3. Para Vendedores (3 pasos)
4. Proceso de Verificación
5. FAQs Rápidas
6. CTA

---

### Task 1.4: Crear Página Pricing ⭐ (1 SP)
**Duración**: 1 hora  
**Archivo**: `src/pages/PricingPage.tsx`

**Componentes**:
```tsx
// Pricing Cards
const plans = [
  {
    name: 'Basic',
    price: 0,
    features: ['1 listing', '30 days', 'Basic support'],
  },
  {
    name: 'Premium',
    price: 49,
    features: ['5 listings', '60 days', 'Featured badge', 'Priority support'],
    popular: true,
  },
  {
    name: 'Dealer',
    price: 199,
    features: ['Unlimited listings', '90 days', 'All features', 'Dedicated support'],
  },
];
```

---

### Task 1.5: Crear Página FAQ ⭐ (1 SP)
**Duración**: 1 hora  
**Archivo**: `src/pages/FAQPage.tsx`

**Features**:
- Accordion component para preguntas
- Búsqueda de FAQs
- Categorías (Buying, Selling, Account, Payment)
- 20+ preguntas predefinidas

**Componente Accordion**:
```tsx
const FAQItem = ({ question, answer }) => {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <div className="border-b">
      <button onClick={() => setIsOpen(!isOpen)}>
        {question}
        <FiChevronDown />
      </button>
      {isOpen && <div>{answer}</div>}
    </div>
  );
};
```

---

### Task 1.6: Crear Página Contact ⭐ (1 SP)
**Duración**: 1 hora  
**Archivo**: `src/pages/ContactPage.tsx`

**Formulario**:
```tsx
interface ContactForm {
  name: string;
  email: string;
  subject: string;
  message: string;
}

// Validación con Zod
const contactSchema = z.object({
  name: z.string().min(2),
  email: z.string().email(),
  subject: z.string().min(5),
  message: z.string().min(20),
});
```

**Secciones**:
1. Formulario de contacto
2. Información de contacto (teléfono, email, dirección)
3. Horarios de atención
4. Enlaces a redes sociales

---

### Task 1.7: Crear Páginas Legales (Terms, Privacy, Cookies) ⭐ (2 SP)
**Duración**: 2 horas  
**Archivos**: 
- `src/pages/TermsPage.tsx`
- `src/pages/PrivacyPage.tsx`
- `src/pages/CookiesPage.tsx`

**Template Compartido**:
```tsx
// src/components/templates/LegalPageTemplate.tsx
export const LegalPageTemplate = ({ title, lastUpdated, children }) => {
  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto px-4 py-16">
        <h1 className="text-4xl font-bold mb-4">{title}</h1>
        <p className="text-gray-600 mb-8">Last updated: {lastUpdated}</p>
        <div className="prose max-w-none">{children}</div>
      </div>
    </MainLayout>
  );
};
```

**Contenido**:
- Terms: 10+ secciones (uso, cuenta, contenido, responsabilidad)
- Privacy: GDPR compliance, datos, cookies, derechos
- Cookies: Tipos, uso, configuración, opt-out

---

### Task 1.8: Actualizar App.tsx con Nuevas Rutas ⭐ (0.5 SP)
**Duración**: 15 minutos  
**Archivo**: `src/App.tsx`

**Agregar**:
```tsx
import AboutPage from './pages/AboutPage';
import HowItWorksPage from './pages/HowItWorksPage';
import PricingPage from './pages/PricingPage';
import FAQPage from './pages/FAQPage';
import ContactPage from './pages/ContactPage';
import HelpCenterPage from './pages/HelpCenterPage';
import TermsPage from './pages/TermsPage';
import PrivacyPage from './pages/PrivacyPage';
import CookiesPage from './pages/CookiesPage';

// En Routes:
<Route path="/about" element={<AboutPage />} />
<Route path="/how-it-works" element={<HowItWorksPage />} />
<Route path="/pricing" element={<PricingPage />} />
<Route path="/faq" element={<FAQPage />} />
<Route path="/contact" element={<ContactPage />} />
<Route path="/help" element={<HelpCenterPage />} />
<Route path="/terms" element={<TermsPage />} />
<Route path="/privacy" element={<PrivacyPage />} />
<Route path="/cookies" element={<CookiesPage />} />
```

**Verificación Día 1**:
- [ ] Todas las páginas accesibles desde Footer
- [ ] No hay errores 404 en navegación
- [ ] Todas las páginas son responsive
- [ ] Build sin errores: `npm run build`

---

## ⚡ DÍA 2: INTEGRACIÓN DE APIs Y SERVICIOS

### Task 2.1: Actualizar authService con API Real ⭐⭐⭐ (3 SP)
**Duración**: 2 horas  
**Prioridad**: 🔴 CRÍTICA  
**Archivo**: `src/services/authService.ts`

**Implementación**:
```typescript
import api from './api';
import type { User } from '@/types';

const AUTH_BASE = import.meta.env.VITE_AUTH_SERVICE_URL || '/api/auth';

export const authService = {
  async login(credentials: { email: string; password: string }) {
    const response = await api.post(`${AUTH_BASE}/login`, credentials);
    const { accessToken, refreshToken, user } = response.data.data;
    
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    
    return { user, accessToken, refreshToken };
  },

  async register(data: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phone?: string;
  }) {
    const response = await api.post(`${AUTH_BASE}/register`, data);
    const { accessToken, refreshToken, user } = response.data.data;
    
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    
    return { user, accessToken, refreshToken };
  },

  async logout() {
    try {
      await api.post(`${AUTH_BASE}/logout`);
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    }
  },

  async refreshToken(refreshToken: string) {
    const response = await api.post(`${AUTH_BASE}/refresh-token`, {
      refreshToken,
    });
    return response.data.data;
  },

  async getCurrentUser(): Promise<User> {
    const response = await api.get(`${AUTH_BASE}/me`);
    return response.data.data;
  },

  async updateProfile(data: Partial<User>) {
    const response = await api.put(`${AUTH_BASE}/profile`, data);
    return response.data.data;
  },

  async changePassword(oldPassword: string, newPassword: string) {
    await api.post(`${AUTH_BASE}/change-password`, {
      oldPassword,
      newPassword,
    });
  },
};
```

**Verificación**:
- [ ] Login funciona con backend
- [ ] Register funciona con backend
- [ ] Logout limpia tokens
- [ ] Refresh token automático funciona
- [ ] getCurrentUser funciona
- [ ] Tokens se guardan correctamente

---

### Task 2.2: Crear vehicleService Completo ⭐⭐⭐ (3 SP)
**Duración**: 2 horas  
**Prioridad**: 🔴 CRÍTICA  
**Archivo**: `src/services/endpoints/vehicleService.ts`

```typescript
import api from '../api';
import type { Vehicle, VehicleFilters } from '@/types';

const VEHICLE_BASE = import.meta.env.VITE_VEHICLE_SERVICE_URL || '/api/vehicles';

export const vehicleService = {
  // CRUD Operations
  async getAll(filters?: VehicleFilters & { page?: number; limit?: number }) {
    const params = new URLSearchParams();
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, String(value));
        }
      });
    }
    const response = await api.get(`${VEHICLE_BASE}?${params}`);
    return response.data.data;
  },

  async getById(id: string): Promise<Vehicle> {
    const response = await api.get(`${VEHICLE_BASE}/${id}`);
    return response.data.data;
  },

  async create(data: Omit<Vehicle, 'id'>) {
    const response = await api.post(VEHICLE_BASE, data);
    return response.data.data;
  },

  async update(id: string, data: Partial<Vehicle>) {
    const response = await api.put(`${VEHICLE_BASE}/${id}`, data);
    return response.data.data;
  },

  async delete(id: string) {
    await api.delete(`${VEHICLE_BASE}/${id}`);
  },

  // Search & Filter
  async search(query: string, filters?: VehicleFilters) {
    const params = new URLSearchParams({ q: query });
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value) params.append(key, String(value));
      });
    }
    const response = await api.get(`${VEHICLE_BASE}/search?${params}`);
    return response.data.data;
  },

  // Favorites
  async addFavorite(vehicleId: string) {
    const response = await api.post(`${VEHICLE_BASE}/${vehicleId}/favorite`);
    return response.data.data;
  },

  async removeFavorite(vehicleId: string) {
    await api.delete(`${VEHICLE_BASE}/${vehicleId}/favorite`);
  },

  async getFavorites() {
    const response = await api.get(`${VEHICLE_BASE}/favorites`);
    return response.data.data;
  },

  // User Listings
  async getMyListings() {
    const response = await api.get(`${VEHICLE_BASE}/my-listings`);
    return response.data.data;
  },

  // Admin Operations
  async getPending() {
    const response = await api.get(`${VEHICLE_BASE}/pending`);
    return response.data.data;
  },

  async approve(id: string, notes?: string) {
    const response = await api.post(`${VEHICLE_BASE}/${id}/approve`, { notes });
    return response.data.data;
  },

  async reject(id: string, reason: string) {
    const response = await api.post(`${VEHICLE_BASE}/${id}/reject`, { reason });
    return response.data.data;
  },

  // Similar Vehicles
  async getSimilar(id: string, limit = 4) {
    const response = await api.get(`${VEHICLE_BASE}/${id}/similar?limit=${limit}`);
    return response.data.data;
  },
};
```

---

### Task 2.3: Crear uploadService ⭐⭐ (2 SP)
**Duración**: 1.5 horas  
**Archivo**: `src/services/endpoints/uploadService.ts`

```typescript
import api from '../api';

const UPLOAD_BASE = import.meta.env.VITE_UPLOAD_SERVICE_URL || '/api/upload';

export const uploadService = {
  async uploadImage(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('image', file);

    const response = await api.post(`${UPLOAD_BASE}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });

    return response.data.data.url;
  },

  async uploadMultipleImages(files: File[]): Promise<string[]> {
    const formData = new FormData();
    files.forEach((file) => formData.append('images', file));

    const response = await api.post(`${UPLOAD_BASE}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });

    return response.data.data.urls;
  },

  async deleteImage(url: string) {
    await api.delete(`${UPLOAD_BASE}/image`, { data: { url } });
  },

  // Image compression antes de subir
  async compressAndUpload(file: File): Promise<string> {
    // Usar browser-image-compression
    const imageCompression = (await import('browser-image-compression')).default;
    
    const options = {
      maxSizeMB: 1,
      maxWidthOrHeight: 1920,
      useWebWorker: true,
    };

    const compressedFile = await imageCompression(file, options);
    return this.uploadImage(compressedFile);
  },
};
```

---

### Task 2.4: Crear messageService ⭐⭐ (2 SP)
**Duración**: 1.5 horas  
**Archivo**: `src/services/endpoints/messageService.ts`

```typescript
import api from '../api';
import type { Conversation, Message } from '@/types';

const MESSAGE_BASE = import.meta.env.VITE_MESSAGE_SERVICE_URL || '/api/messages';

export const messageService = {
  async getConversations(): Promise<Conversation[]> {
    const response = await api.get(`${MESSAGE_BASE}/conversations`);
    return response.data.data;
  },

  async getConversation(id: string): Promise<Conversation> {
    const response = await api.get(`${MESSAGE_BASE}/conversations/${id}`);
    return response.data.data;
  },

  async getMessages(conversationId: string): Promise<Message[]> {
    const response = await api.get(`${MESSAGE_BASE}/conversations/${conversationId}/messages`);
    return response.data.data;
  },

  async sendMessage(conversationId: string, content: string): Promise<Message> {
    const response = await api.post(`${MESSAGE_BASE}/conversations/${conversationId}/messages`, {
      content,
    });
    return response.data.data;
  },

  async createConversation(vehicleId: string, recipientId: string): Promise<Conversation> {
    const response = await api.post(`${MESSAGE_BASE}/conversations`, {
      vehicleId,
      recipientId,
    });
    return response.data.data;
  },

  async markAsRead(conversationId: string) {
    await api.post(`${MESSAGE_BASE}/conversations/${conversationId}/read`);
  },

  async getUnreadCount(): Promise<number> {
    const response = await api.get(`${MESSAGE_BASE}/unread-count`);
    return response.data.data.count;
  },
};
```

---

### Task 2.5: Crear adminService y notificationService ⭐⭐ (2 SP)
**Duración**: 1.5 horas  

**Archivo 1**: `src/services/endpoints/adminService.ts`
```typescript
import api from '../api';
import type { AdminStats, ActivityLog, User } from '@/types';

const ADMIN_BASE = import.meta.env.VITE_ADMIN_SERVICE_URL || '/api/admin';

export const adminService = {
  async getStats(): Promise<AdminStats> {
    const response = await api.get(`${ADMIN_BASE}/stats`);
    return response.data.data;
  },

  async getActivityLogs(limit = 50): Promise<ActivityLog[]> {
    const response = await api.get(`${ADMIN_BASE}/activity-logs?limit=${limit}`);
    return response.data.data;
  },

  async getUsers(filters?: { search?: string; role?: string; status?: string }) {
    const params = new URLSearchParams();
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value) params.append(key, value);
      });
    }
    const response = await api.get(`${ADMIN_BASE}/users?${params}`);
    return response.data.data;
  },

  async banUser(userId: string, reason?: string) {
    await api.post(`${ADMIN_BASE}/users/${userId}/ban`, { reason });
  },

  async unbanUser(userId: string) {
    await api.post(`${ADMIN_BASE}/users/${userId}/unban`);
  },

  async deleteUser(userId: string) {
    await api.delete(`${ADMIN_BASE}/users/${userId}`);
  },
};
```

**Archivo 2**: `src/services/endpoints/notificationService.ts`
```typescript
import api from '../api';
import type { Notification } from '@/types';

const NOTIFICATION_BASE = '/api/notifications';

export const notificationService = {
  async getNotifications(): Promise<Notification[]> {
    const response = await api.get(NOTIFICATION_BASE);
    return response.data.data;
  },

  async markAsRead(id: string) {
    await api.put(`${NOTIFICATION_BASE}/${id}/read`);
  },

  async markAllAsRead() {
    await api.put(`${NOTIFICATION_BASE}/read-all`);
  },

  async deleteNotification(id: string) {
    await api.delete(`${NOTIFICATION_BASE}/${id}`);
  },

  async getUnreadCount(): Promise<number> {
    const response = await api.get(`${NOTIFICATION_BASE}/unread-count`);
    return response.data.data.count;
  },
};
```

**Verificación Día 2**:
- [ ] Todos los servicios compilan sin errores
- [ ] authService funciona con backend
- [ ] vehicleService CRUD funciona
- [ ] uploadService sube imágenes
- [ ] messageService envía mensajes
- [ ] adminService obtiene stats
- [ ] notificationService funciona

---

## 🔗 DÍA 3: INTEGRACIÓN Y TESTING

### Task 3.1: Actualizar Componentes con API Real ⭐⭐⭐ (3 SP)
**Duración**: 3 horas  
**Prioridad**: 🔴 CRÍTICA

**HomePage.tsx**:
```tsx
import { useQuery } from '@tanstack/react-query';
import { vehicleService } from '@/services/endpoints/vehicleService';

// Reemplazar mockVehicles con:
const { data: featuredVehicles, isLoading } = useQuery({
  queryKey: ['vehicles', 'featured'],
  queryFn: () => vehicleService.getAll({ isFeatured: true, limit: 8 }),
});

// Implementar handleSearch:
const navigate = useNavigate();
const handleSearch = (filters: SearchFilters) => {
  const params = new URLSearchParams();
  if (filters.make) params.append('make', filters.make);
  if (filters.model) params.append('model', filters.model);
  if (filters.minPrice) params.append('minPrice', String(filters.minPrice));
  if (filters.maxPrice) params.append('maxPrice', String(filters.maxPrice));
  navigate(`/browse?${params.toString()}`);
};
```

**BrowsePage.tsx**:
```tsx
const [page, setPage] = useState(1);
const [filters, setFilters] = useState<VehicleFilters>({});

// Parse URL params
useEffect(() => {
  const searchParams = new URLSearchParams(location.search);
  const newFilters: VehicleFilters = {};
  searchParams.forEach((value, key) => {
    newFilters[key] = value;
  });
  setFilters(newFilters);
}, [location.search]);

// Usar API
const { data, isLoading } = useQuery({
  queryKey: ['vehicles', filters, page],
  queryFn: () => vehicleService.getAll({ ...filters, page, limit: 12 }),
});
```

**VehicleDetailPage.tsx**:
```tsx
const { id } = useParams();
const { data: vehicle, isLoading } = useQuery({
  queryKey: ['vehicle', id],
  queryFn: () => vehicleService.getById(id!),
  enabled: !!id,
});

// Similar vehicles
const { data: similarVehicles } = useQuery({
  queryKey: ['vehicles', 'similar', id],
  queryFn: () => vehicleService.getSimilar(id!),
  enabled: !!id,
});

// Fix phone link:
<a 
  href={`tel:${vehicle.seller.phone}`}
  className="btn btn-outline"
>
  <FiPhone /> Call Seller
</a>
```

**SellYourCarPage.tsx**:
```tsx
const { mutate: createVehicle, isPending } = useMutation({
  mutationFn: vehicleService.create,
  onSuccess: () => {
    toast.success('Vehicle listed successfully!');
    navigate('/dashboard');
  },
  onError: (error) => {
    toast.error(error.message);
  },
});

// Handle image upload
const handleImageUpload = async (files: File[]) => {
  setUploading(true);
  try {
    const urls = await uploadService.uploadMultipleImages(files);
    setImages((prev) => [...prev, ...urls]);
  } catch (error) {
    toast.error('Failed to upload images');
  } finally {
    setUploading(false);
  }
};
```

**MessagesPage.tsx**:
```tsx
const { data: conversations } = useQuery({
  queryKey: ['conversations'],
  queryFn: messageService.getConversations,
  refetchInterval: 30000, // Poll every 30s
});

const { data: messages } = useQuery({
  queryKey: ['messages', selectedConversation?.id],
  queryFn: () => messageService.getMessages(selectedConversation!.id),
  enabled: !!selectedConversation,
});

const { mutate: sendMessage } = useMutation({
  mutationFn: ({ conversationId, content }: { conversationId: string; content: string }) =>
    messageService.sendMessage(conversationId, content),
  onSuccess: () => {
    queryClient.invalidateQueries(['messages', selectedConversation?.id]);
  },
});
```

**Admin Pages**:
```tsx
// AdminDashboardPage.tsx
const { data: stats } = useQuery({
  queryKey: ['admin', 'stats'],
  queryFn: adminService.getStats,
});

// PendingApprovalsPage.tsx
const { data: pendingVehicles } = useQuery({
  queryKey: ['vehicles', 'pending'],
  queryFn: vehicleService.getPending,
});

const { mutate: approve } = useMutation({
  mutationFn: vehicleService.approve,
  onSuccess: () => {
    queryClient.invalidateQueries(['vehicles', 'pending']);
    toast.success('Vehicle approved');
  },
});

// UsersManagementPage.tsx
const { data: users } = useQuery({
  queryKey: ['admin', 'users'],
  queryFn: adminService.getUsers,
});
```

**NotificationDropdown.tsx**:
```tsx
const { data: notifications } = useQuery({
  queryKey: ['notifications'],
  queryFn: notificationService.getNotifications,
  refetchInterval: 30000,
});

const { mutate: markAsRead } = useMutation({
  mutationFn: notificationService.markAsRead,
  onSuccess: () => {
    queryClient.invalidateQueries(['notifications']);
  },
});
```

---

### Task 3.2: Testing End-to-End ⭐⭐ (2 SP)
**Duración**: 2 horas  
**Prioridad**: 🔴 CRÍTICA

**Crear**: `src/test/e2e/critical-flows.test.ts`

```typescript
describe('Critical User Flows', () => {
  describe('Authentication Flow', () => {
    it('should register new user', async () => {
      // Navigate to register
      // Fill form
      // Submit
      // Verify redirect to dashboard
    });

    it('should login existing user', async () => {
      // Navigate to login
      // Fill credentials
      // Submit
      // Verify user is authenticated
    });

    it('should persist session on refresh', async () => {
      // Login
      // Refresh page
      // Verify still authenticated
    });
  });

  describe('Vehicle Listing Flow', () => {
    it('should create new vehicle listing', async () => {
      // Login
      // Navigate to /sell
      // Fill all steps
      // Upload images
      // Submit
      // Verify success
    });

    it('should edit existing listing', async () => {
      // Login
      // Go to dashboard
      // Click edit
      // Update data
      // Save
      // Verify changes
    });
  });

  describe('Search and Browse Flow', () => {
    it('should search vehicles from homepage', async () => {
      // Enter search criteria
      // Submit
      // Verify results page with filters
    });

    it('should filter and paginate results', async () => {
      // Go to browse
      // Apply filters
      // Verify filtered results
      // Navigate to page 2
      // Verify pagination works
    });
  });

  describe('Favorites Flow', () => {
    it('should add vehicle to favorites', async () => {
      // Login
      // View vehicle detail
      // Click favorite
      // Go to wishlist
      // Verify vehicle is there
    });
  });

  describe('Messaging Flow', () => {
    it('should send message to seller', async () => {
      // Login
      // View vehicle
      // Click contact seller
      // Send message
      // Verify in messages page
    });
  });
});
```

**Testing Manual Checklist**:
```
✅ AUTENTICACIÓN:
- [ ] Register funciona
- [ ] Login funciona
- [ ] Logout funciona
- [ ] Session persiste en refresh
- [ ] Token refresh automático funciona
- [ ] Protected routes redirigen a login

✅ NAVEGACIÓN:
- [ ] Todos los links del Footer funcionan
- [ ] Todos los links del Navbar funcionan
- [ ] Breadcrumbs funcionan
- [ ] No hay 404 en links internos
- [ ] Mobile menu funciona

✅ VEHÍCULOS:
- [ ] Browse muestra vehículos de API
- [ ] Search funciona y navega con filtros
- [ ] Filters aplicados correctamente
- [ ] Paginación funciona
- [ ] Vehicle detail muestra datos correctos
- [ ] Similar vehicles funcionan
- [ ] Phone link funciona (tel:)

✅ CRUD VEHÍCULOS:
- [ ] Crear vehículo funciona
- [ ] Upload múltiples imágenes funciona
- [ ] Editar vehículo funciona
- [ ] Eliminar vehículo funciona
- [ ] Mis listings se muestran correctamente

✅ FAVORITOS:
- [ ] Agregar favorito funciona
- [ ] Remover favorito funciona
- [ ] Wishlist muestra favoritos
- [ ] Compartir wishlist funciona

✅ MENSAJERÍA:
- [ ] Ver conversaciones funciona
- [ ] Enviar mensaje funciona
- [ ] Mensajes se actualizan
- [ ] Contador de no leídos funciona

✅ NOTIFICACIONES:
- [ ] Dropdown muestra notificaciones
- [ ] Marcar como leída funciona
- [ ] Badge de contador funciona

✅ ADMIN:
- [ ] Dashboard muestra stats reales
- [ ] Pending approvals muestra vehículos
- [ ] Aprobar vehículo funciona
- [ ] Rechazar vehículo funciona
- [ ] Users management muestra usuarios
- [ ] Banear usuario funciona
- [ ] Activity logs se muestran

✅ PERFORMANCE:
- [ ] Imágenes se comprimen antes de subir
- [ ] Lazy loading funciona
- [ ] No memory leaks
- [ ] API calls se cachean correctamente
```

---

### Task 3.3: Fix Bugs y Optimizaciones ⭐ (1 SP)
**Duración**: 1 hora

**Bugs Conocidos a Corregir**:

1. **VehicleDetailPage - Phone Link**:
```tsx
// ANTES (incorrecto):
<Link to={`tel:${vehicle.seller.phone}`}>

// DESPUÉS (correcto):
<a href={`tel:${vehicle.seller.phone}`}>
```

2. **HomePage - Search Navigation**:
```tsx
// DESPUÉS:
const navigate = useNavigate();
const handleSearch = (filters: SearchFilters) => {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) params.append(key, String(value));
  });
  navigate(`/browse?${params.toString()}`);
};
```

3. **Error Handling Global**:
```tsx
// src/components/ErrorBoundary.tsx
import { Component, ErrorInfo, ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              Something went wrong
            </h1>
            <p className="text-gray-600 mb-4">
              {this.state.error?.message || 'An unexpected error occurred'}
            </p>
            <button
              onClick={() => window.location.href = '/'}
              className="btn btn-primary"
            >
              Go Home
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
```

4. **Loading States Mejorados**:
```tsx
// src/components/LoadingSpinner.tsx
export const LoadingSpinner = ({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) => {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-8 h-8',
    lg: 'w-12 h-12',
  };

  return (
    <div className="flex justify-center items-center">
      <div className={`${sizeClasses[size]} border-4 border-primary border-t-transparent rounded-full animate-spin`} />
    </div>
  );
};

// Uso en componentes:
{isLoading ? <LoadingSpinner /> : <VehicleGrid vehicles={data} />}
```

5. **Toast Notifications**:
```bash
npm install react-hot-toast
```

```tsx
// src/App.tsx
import { Toaster } from 'react-hot-toast';

function App() {
  return (
    <>
      <Toaster position="top-right" />
      <Router>
        {/* routes */}
      </Router>
    </>
  );
}
```

**Verificación Final Día 3**:
- [ ] Todos los tests pasan
- [ ] No hay console.errors
- [ ] No hay memory leaks
- [ ] Build de producción sin warnings
- [ ] Lighthouse score > 85

---

## 📋 CHECKLIST FINAL DEL SPRINT

### 🎯 Features Completadas:
- [ ] 9 páginas estáticas creadas
- [ ] 9 rutas agregadas a App.tsx
- [ ] authService integrado con API
- [ ] vehicleService completo
- [ ] uploadService funcional
- [ ] messageService funcional
- [ ] adminService funcional
- [ ] notificationService funcional
- [ ] HomePage usa API real
- [ ] BrowsePage usa API real
- [ ] VehicleDetailPage usa API real
- [ ] MessagesPage usa API real
- [ ] Admin pages usan API real
- [ ] NotificationDropdown usa API real
- [ ] Search navigation implementado
- [ ] Phone link corregido
- [ ] Error boundaries implementados
- [ ] Loading states en todos los componentes
- [ ] Toast notifications agregadas

### 🧪 Testing:
- [ ] Flujo de autenticación completo
- [ ] Flujo de CRUD vehículos
- [ ] Flujo de favoritos
- [ ] Flujo de mensajería
- [ ] Flujo de admin
- [ ] Testing manual completado
- [ ] No hay bugs críticos

### 📊 Métricas:
- [ ] Build exitoso sin errores
- [ ] Bundle size < 600KB
- [ ] Lighthouse Performance > 85
- [ ] Lighthouse Accessibility > 90
- [ ] No memory leaks detectados
- [ ] API calls optimizadas con cache

---

## 🚀 COMANDOS ÚTILES

```bash
# Día 1 - Desarrollo
npm run dev

# Verificar build
npm run build
npm run preview

# Día 2 - Testing
npm run test
npm run test:ui

# Día 3 - Verificación
npm run lint
npm run type-check  # tsc --noEmit

# Production build
npm run build
```

---

## 📈 DEFINICIÓN DE "DONE"

Una tarea se considera completa cuando:
1. ✅ Código implementado y funcionando
2. ✅ Sin errores en consola
3. ✅ Responsive design verificado
4. ✅ Integración con API funciona
5. ✅ Loading y error states implementados
6. ✅ Testeado manualmente
7. ✅ Build sin warnings
8. ✅ Documentación actualizada (si aplica)

---

## 🎯 PRIORIDADES SI HAY RETRASOS

**Must Have** (No negociables):
1. authService con API real
2. vehicleService con API real
3. Páginas About, FAQ, Contact
4. Search navigation
5. Phone link fix

**Should Have** (Importantes):
6. uploadService
7. messageService
8. adminService
9. Páginas Terms, Privacy

**Nice to Have** (Opcionales):
10. Real-time messaging (WebSocket)
11. Notificaciones push
12. Testing E2E automatizado

---

## 📞 SOPORTE

Si encuentras problemas:
1. Verifica que backend está corriendo
2. Verifica CORS en backend
3. Verifica tokens en localStorage
4. Chequea Network tab en DevTools
5. Revisa logs de backend

---

**¡ÉXITO EN EL SPRINT!** 🚀

