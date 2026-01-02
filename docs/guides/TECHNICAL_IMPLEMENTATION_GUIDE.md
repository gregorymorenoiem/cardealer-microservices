# 🔧 ANÁLISIS TÉCNICO DE IMPLEMENTACIÓN - DEALER PLATFORM

**Fecha**: Diciembre 5, 2025  
**Complemento de**: ARCHITECTURE_REFACTORING_PLAN.md

---

## 📐 DECISIONES TÉCNICAS CRÍTICAS

### 1. Monorepo Strategy - Frontend

#### Opción A: **pnpm Workspaces** ⭐ RECOMENDADO
```json
// package.json (root)
{
  "name": "cardealer-frontend",
  "private": true,
  "workspaces": [
    "packages/*"
  ],
  "scripts": {
    "dev:web": "pnpm --filter @cardealer/web dev",
    "dev:mobile": "pnpm --filter @cardealer/mobile start",
    "build:all": "pnpm -r build",
    "test:all": "pnpm -r test"
  },
  "devDependencies": {
    "typescript": "^5.3.0",
    "turbo": "^1.11.0"
  }
}

// packages/web/package.json
{
  "name": "@cardealer/web",
  "dependencies": {
    "@cardealer/shared": "workspace:*",
    "@cardealer/ui-components": "workspace:*",
    "react": "^18.2.0",
    "vite": "^5.0.0"
  }
}

// packages/mobile/package.json
{
  "name": "@cardealer/mobile",
  "dependencies": {
    "@cardealer/shared": "workspace:*",
    "@cardealer/ui-components": "workspace:*",
    "react-native": "^0.73.0"
  }
}
```

**Ventajas**:
- Compartir código entre web y mobile (60-70% de reutilización)
- Hot reload en desarrollo
- Build optimization con Turbo
- Type-safety entre packages

#### Opción B: Separate Repos (NO RECOMENDADO)
- Duplicación de código (types, API, utils)
- Sincronización manual
- Más difícil de mantener

**DECISIÓN**: Usar pnpm workspaces

---

### 2. State Management - Multi-Platform

#### Zustand (Actual) - Mantener y Extender ⭐

```typescript
// packages/shared/store/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User, AccountType } from '../types';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  
  // Getters computados
  isAuthenticated: boolean;
  accountType: AccountType | null;
  isDealer: boolean;
  isAdmin: boolean;
  hasActiveSubscription: boolean;
  
  // Actions
  login: (tokens: LoginResponse) => void;
  logout: () => void;
  updateUser: (user: User) => void;
  refreshTokens: (tokens: RefreshResponse) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      
      // Computed
      get isAuthenticated() {
        return !!get().accessToken && !!get().user;
      },
      
      get accountType() {
        return get().user?.accountType || null;
      },
      
      get isDealer() {
        return get().accountType === AccountType.DEALER;
      },
      
      get isAdmin() {
        return get().accountType === AccountType.ADMIN;
      },
      
      get hasActiveSubscription() {
        const user = get().user;
        if (!user?.subscription) return false;
        return new Date(user.subscription.endDate) > new Date();
      },
      
      // Actions
      login: (tokens) => {
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          user: tokens.user,
        });
      },
      
      logout: () => {
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
        });
      },
      
      updateUser: (user) => set({ user }),
      
      refreshTokens: (tokens) => {
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
        });
      },
    }),
    {
      name: 'auth-storage',
      // Web: localStorage, Mobile: AsyncStorage
      storage: createStorage(),
    }
  )
);

// packages/shared/store/dealerStore.ts
interface DealerState {
  dealerInfo: DealerInfo | null;
  inventory: Vehicle[];
  leads: Lead[];
  analytics: DealerAnalytics | null;
  
  // Actions
  setDealerInfo: (info: DealerInfo) => void;
  setInventory: (vehicles: Vehicle[]) => void;
  addVehicle: (vehicle: Vehicle) => void;
  removeVehicle: (id: string) => void;
  setLeads: (leads: Lead[]) => void;
  setAnalytics: (analytics: DealerAnalytics) => void;
}

export const useDealerStore = create<DealerState>((set) => ({
  dealerInfo: null,
  inventory: [],
  leads: [],
  analytics: null,
  
  setDealerInfo: (info) => set({ dealerInfo: info }),
  setInventory: (vehicles) => set({ inventory: vehicles }),
  addVehicle: (vehicle) => set((state) => ({
    inventory: [...state.inventory, vehicle],
  })),
  removeVehicle: (id) => set((state) => ({
    inventory: state.inventory.filter((v) => v.id !== id),
  })),
  setLeads: (leads) => set({ leads }),
  setAnalytics: (analytics) => set({ analytics }),
}));
```

**Ventajas de Zustand**:
- Compatible con React Native (no depende de Context API)
- Pequeño bundle size (2.9kb)
- No requiere providers
- Middleware para persist (AsyncStorage en mobile)

---

### 3. Routing - Multi-Platform

#### Web: React Router v6 (Actual) ✅

```typescript
// packages/web/src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute, AccountTypeGuard } from './components/guards';
import { AccountType } from '@cardealer/shared/types';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public */}
        <Route path="/" element={<HomePage />} />
        <Route path="/browse" element={<BrowsePage />} />
        
        {/* User Panel */}
        <Route path="/dashboard" element={
          <ProtectedRoute accountTypes={[AccountType.INDIVIDUAL, AccountType.DEALER]}>
            <UserDashboard />
          </ProtectedRoute>
        } />
        
        {/* Dealer Panel */}
        <Route path="/dealer" element={
          <ProtectedRoute accountTypes={[AccountType.DEALER]}>
            <DealerLayout />
          </ProtectedRoute>
        }>
          <Route index element={<DealerDashboard />} />
          <Route path="inventory" element={<DealerInventory />} />
          <Route path="analytics" element={<DealerAnalytics />} />
          <Route path="leads" element={<DealerLeads />} />
          <Route path="billing" element={<DealerBilling />} />
        </Route>
        
        {/* Admin Panel */}
        <Route path="/admin" element={
          <ProtectedRoute accountTypes={[AccountType.ADMIN]}>
            <AdminLayout />
          </ProtectedRoute>
        }>
          {/* ... rutas admin existentes ... */}
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
```

#### Mobile: React Navigation 6

```typescript
// packages/mobile/src/navigation/AppNavigator.tsx
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { useAuthStore } from '@cardealer/shared/store';
import { AccountType } from '@cardealer/shared/types';

const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

// Tab Navigator para usuarios
function UserTabs() {
  return (
    <Tab.Navigator>
      <Tab.Screen name="Browse" component={BrowseScreen} />
      <Tab.Screen name="Favorites" component={FavoritesScreen} />
      <Tab.Screen name="Messages" component={MessagesScreen} />
      <Tab.Screen name="Profile" component={ProfileScreen} />
    </Tab.Navigator>
  );
}

// Tab Navigator para dealers
function DealerTabs() {
  return (
    <Tab.Navigator>
      <Tab.Screen name="Dashboard" component={DealerDashboardScreen} />
      <Tab.Screen name="Inventory" component={DealerInventoryScreen} />
      <Tab.Screen name="Leads" component={DealerLeadsScreen} />
      <Tab.Screen name="More" component={DealerMoreScreen} />
    </Tab.Navigator>
  );
}

export function AppNavigator() {
  const { isAuthenticated, accountType } = useAuthStore();
  
  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {!isAuthenticated ? (
          // Auth Stack
          <>
            <Stack.Screen name="Login" component={LoginScreen} />
            <Stack.Screen name="Register" component={RegisterScreen} />
          </>
        ) : accountType === AccountType.DEALER ? (
          // Dealer Stack
          <>
            <Stack.Screen name="DealerMain" component={DealerTabs} />
            <Stack.Screen name="AddVehicle" component={AddVehicleScreen} />
            <Stack.Screen name="VehicleDetail" component={VehicleDetailScreen} />
            <Stack.Screen name="Analytics" component={AnalyticsScreen} />
          </>
        ) : (
          // User Stack
          <>
            <Stack.Screen name="UserMain" component={UserTabs} />
            <Stack.Screen name="VehicleDetail" component={VehicleDetailScreen} />
            <Stack.Screen name="Search" component={SearchScreen} />
          </>
        )}
      </Stack.Navigator>
    </NavigationContainer>
  );
}
```

---

### 4. Authentication Flow - JWT con Multi-Rol

#### Backend: JWT Claims Extendido

```csharp
// AuthService.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs
public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
{
    var user = await _userRepository.GetByEmailAsync(request.Email);
    if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        throw new UnauthorizedException("Invalid credentials");

    // Obtener roles del usuario
    var roles = await _roleRepository.GetUserRolesAsync(user.Id);
    
    // Determinar accountType
    var accountType = DetermineAccountType(user, roles);
    
    // Obtener información de dealer si aplica
    DealerInfo? dealerInfo = null;
    if (accountType == AccountType.Dealer)
    {
        dealerInfo = await _dealerRepository.GetByUserIdAsync(user.Id);
    }
    
    // Crear claims
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("accountType", accountType.ToString()),
    };
    
    // Agregar roles
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role.Name));
    }
    
    // Agregar dealerId si es dealer
    if (dealerInfo != null)
    {
        claims.Add(new Claim("dealerId", dealerInfo.Id.ToString()));
        claims.Add(new Claim("dealerPlan", dealerInfo.Plan.ToString()));
    }
    
    // Agregar permisos (opcional, para RBAC granular)
    var permissions = await _permissionRepository.GetUserPermissionsAsync(user.Id);
    foreach (var permission in permissions)
    {
        claims.Add(new Claim("permission", permission.Name));
    }
    
    // Generar tokens
    var accessToken = _jwtService.GenerateAccessToken(claims);
    var refreshToken = _jwtService.GenerateRefreshToken(user.Id);
    
    return new LoginResponse
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddMinutes(15),
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            AccountType = accountType,
            Roles = roles.Select(r => r.Name).ToList(),
            Dealer = dealerInfo != null ? new DealerDto
            {
                Id = dealerInfo.Id,
                BusinessName = dealerInfo.BusinessName,
                Plan = dealerInfo.Plan,
                Subscription = new SubscriptionDto
                {
                    Plan = dealerInfo.Plan,
                    StartDate = dealerInfo.SubscriptionStartDate,
                    EndDate = dealerInfo.SubscriptionEndDate,
                    Features = GetPlanFeatures(dealerInfo.Plan),
                    Limits = GetPlanLimits(dealerInfo.Plan),
                }
            } : null,
        }
    };
}

private AccountType DetermineAccountType(User user, List<Role> roles)
{
    if (roles.Any(r => r.Name == "SuperAdmin" || r.Name == "Admin"))
        return AccountType.Admin;
    
    if (user.DealerId != null)
        return AccountType.Dealer;
    
    if (user.IsRegistered)
        return AccountType.Individual;
    
    return AccountType.Guest;
}
```

#### Frontend: Axios Interceptor con Claims

```typescript
// packages/shared/api/client.ts
import axios from 'axios';
import { useAuthStore } from '../store';
import { jwtDecode } from 'jwt-decode';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:15095',
  timeout: 10000,
});

// Request interceptor - Agregar token
apiClient.interceptors.request.use(
  (config) => {
    const { accessToken } = useAuthStore.getState();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Refresh token automático
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    // Si es 401 y no es retry
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      const { refreshToken, refreshTokens, logout } = useAuthStore.getState();
      
      if (!refreshToken) {
        logout();
        return Promise.reject(error);
      }
      
      try {
        // Refresh token
        const response = await axios.post(
          `${apiClient.defaults.baseURL}/api/auth/refresh`,
          { refreshToken }
        );
        
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        
        // Actualizar tokens en store
        refreshTokens({ accessToken, refreshToken: newRefreshToken });
        
        // Reintentar request original
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh falló, logout
        logout();
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

// Helper para decodificar JWT y obtener claims
export function decodeToken(token: string) {
  try {
    const decoded = jwtDecode<{
      sub: string;
      email: string;
      accountType: string;
      dealerId?: string;
      dealerPlan?: string;
      role: string[];
      permission: string[];
      exp: number;
    }>(token);
    
    return {
      userId: decoded.sub,
      email: decoded.email,
      accountType: decoded.accountType as AccountType,
      dealerId: decoded.dealerId,
      dealerPlan: decoded.dealerPlan,
      roles: decoded.role || [],
      permissions: decoded.permission || [],
      expiresAt: new Date(decoded.exp * 1000),
    };
  } catch {
    return null;
  }
}
```

---

### 5. Protected Routes - Multi-Level

```typescript
// packages/web/src/components/guards/ProtectedRoute.tsx
import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '@cardealer/shared/store';
import { AccountType } from '@cardealer/shared/types';

interface ProtectedRouteProps {
  children: React.ReactNode;
  accountTypes?: AccountType[];
  requireSubscription?: boolean;
  requiredPermissions?: string[];
  fallbackPath?: string;
}

export function ProtectedRoute({
  children,
  accountTypes,
  requireSubscription = false,
  requiredPermissions = [],
  fallbackPath = '/login',
}: ProtectedRouteProps) {
  const location = useLocation();
  const { 
    isAuthenticated, 
    accountType, 
    user,
    hasActiveSubscription 
  } = useAuthStore();
  
  // Check authentication
  if (!isAuthenticated) {
    return <Navigate to={fallbackPath} state={{ from: location }} replace />;
  }
  
  // Check account type
  if (accountTypes && accountTypes.length > 0) {
    if (!accountType || !accountTypes.includes(accountType)) {
      return <Navigate to="/unauthorized" replace />;
    }
  }
  
  // Check subscription (solo para dealers)
  if (requireSubscription && accountType === AccountType.DEALER) {
    if (!hasActiveSubscription) {
      return <Navigate to="/dealer/subscription-expired" replace />;
    }
  }
  
  // Check permissions (RBAC granular)
  if (requiredPermissions.length > 0) {
    const userPermissions = user?.permissions || [];
    const hasPermission = requiredPermissions.every(
      (perm) => userPermissions.includes(perm)
    );
    
    if (!hasPermission) {
      return <Navigate to="/unauthorized" replace />;
    }
  }
  
  return <>{children}</>;
}

// Usage examples:
/*
// Solo dealers activos
<ProtectedRoute 
  accountTypes={[AccountType.DEALER]} 
  requireSubscription={true}
>
  <DealerAnalytics />
</ProtectedRoute>

// Dealers o admins
<ProtectedRoute accountTypes={[AccountType.DEALER, AccountType.ADMIN]}>
  <BulkUpload />
</ProtectedRoute>

// Con permisos específicos
<ProtectedRoute 
  accountTypes={[AccountType.ADMIN]}
  requiredPermissions={['admin.users.delete']}
>
  <DeleteUserButton />
</ProtectedRoute>
*/
```

---

### 6. Database Schema - Dealers

```sql
-- PostgreSQL Schema for DealerService

-- Tabla de Dealers
CREATE TABLE dealers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    business_name VARCHAR(255) NOT NULL,
    business_type VARCHAR(50) NOT NULL, -- 'independent', 'franchise', 'corporate'
    license_number VARCHAR(100),
    tax_id VARCHAR(50),
    
    -- Verification
    verification_status VARCHAR(20) NOT NULL DEFAULT 'pending', -- 'pending', 'verified', 'rejected'
    verification_date TIMESTAMPTZ,
    verification_notes TEXT,
    
    -- Contact
    business_phone VARCHAR(20),
    business_email VARCHAR(255),
    website VARCHAR(255),
    
    -- Address
    street_address VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    zip_code VARCHAR(20),
    country VARCHAR(100) DEFAULT 'USA',
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    
    -- Plan & Subscription
    plan_type VARCHAR(20) NOT NULL DEFAULT 'basic', -- 'basic', 'pro', 'enterprise'
    subscription_start_date TIMESTAMPTZ NOT NULL,
    subscription_end_date TIMESTAMPTZ NOT NULL,
    auto_renew BOOLEAN DEFAULT true,
    
    -- Limits (based on plan)
    max_listings INTEGER NOT NULL DEFAULT 50,
    max_images_per_listing INTEGER NOT NULL DEFAULT 10,
    max_featured_listings INTEGER NOT NULL DEFAULT 2,
    
    -- Stats
    total_listings INTEGER DEFAULT 0,
    total_sold INTEGER DEFAULT 0,
    total_revenue DECIMAL(12, 2) DEFAULT 0,
    
    -- Metadata
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    created_by VARCHAR(255),
    updated_by VARCHAR(255),
    
    -- Indexes
    CONSTRAINT chk_verification_status CHECK (verification_status IN ('pending', 'verified', 'rejected')),
    CONSTRAINT chk_plan_type CHECK (plan_type IN ('basic', 'pro', 'enterprise'))
);

CREATE INDEX idx_dealers_user_id ON dealers(user_id);
CREATE INDEX idx_dealers_verification_status ON dealers(verification_status);
CREATE INDEX idx_dealers_plan_type ON dealers(plan_type);
CREATE INDEX idx_dealers_subscription_end ON dealers(subscription_end_date);

-- Tabla de Inventory (relación entre dealer y vehículo)
CREATE TABLE dealer_inventory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    dealer_id UUID NOT NULL REFERENCES dealers(id) ON DELETE CASCADE,
    vehicle_id UUID NOT NULL REFERENCES vehicles(id) ON DELETE CASCADE,
    
    -- Financial
    acquisition_cost DECIMAL(10, 2),
    listing_price DECIMAL(10, 2) NOT NULL,
    minimum_price DECIMAL(10, 2),
    target_margin_percentage DECIMAL(5, 2),
    
    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'in_stock', -- 'in_stock', 'on_hold', 'sold', 'removed'
    location VARCHAR(100), -- 'lot', 'service', 'transport'
    days_in_inventory INTEGER DEFAULT 0,
    
    -- Timestamps
    acquired_at TIMESTAMPTZ,
    listed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sold_at TIMESTAMPTZ,
    
    UNIQUE(dealer_id, vehicle_id),
    CONSTRAINT chk_inventory_status CHECK (status IN ('in_stock', 'on_hold', 'sold', 'removed'))
);

CREATE INDEX idx_inventory_dealer_id ON dealer_inventory(dealer_id);
CREATE INDEX idx_inventory_status ON dealer_inventory(status);
CREATE INDEX idx_inventory_days ON dealer_inventory(days_in_inventory);

-- Tabla de Leads
CREATE TABLE dealer_leads (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    dealer_id UUID NOT NULL REFERENCES dealers(id) ON DELETE CASCADE,
    vehicle_id UUID REFERENCES vehicles(id) ON DELETE SET NULL,
    
    -- Lead info
    customer_name VARCHAR(255),
    customer_email VARCHAR(255),
    customer_phone VARCHAR(20),
    lead_source VARCHAR(50) NOT NULL, -- 'website', 'phone', 'walk-in', 'referral'
    
    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'new', -- 'new', 'contacted', 'qualified', 'negotiating', 'won', 'lost'
    lead_score INTEGER DEFAULT 0 CHECK (lead_score BETWEEN 0 AND 100),
    
    -- Activity
    first_contact_date TIMESTAMPTZ,
    last_contact_date TIMESTAMPTZ,
    follow_up_date TIMESTAMPTZ,
    close_date TIMESTAMPTZ,
    
    -- Notes
    notes TEXT,
    lost_reason TEXT,
    
    -- Metadata
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    
    CONSTRAINT chk_lead_status CHECK (status IN ('new', 'contacted', 'qualified', 'negotiating', 'won', 'lost'))
);

CREATE INDEX idx_leads_dealer_id ON dealer_leads(dealer_id);
CREATE INDEX idx_leads_status ON dealer_leads(status);
CREATE INDEX idx_leads_vehicle_id ON dealer_leads(vehicle_id);
CREATE INDEX idx_leads_follow_up ON dealer_leads(follow_up_date);

-- Tabla de Campaigns (Featured/Boosted Listings)
CREATE TABLE dealer_campaigns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    dealer_id UUID NOT NULL REFERENCES dealers(id) ON DELETE CASCADE,
    vehicle_id UUID NOT NULL REFERENCES vehicles(id) ON DELETE CASCADE,
    
    campaign_type VARCHAR(20) NOT NULL, -- 'featured', 'boost', 'premium'
    
    -- Budget & Performance
    budget_amount DECIMAL(10, 2) NOT NULL,
    spent_amount DECIMAL(10, 2) DEFAULT 0,
    cost_per_view DECIMAL(6, 4),
    cost_per_click DECIMAL(6, 4),
    
    -- Stats
    impressions INTEGER DEFAULT 0,
    clicks INTEGER DEFAULT 0,
    conversions INTEGER DEFAULT 0,
    
    -- Schedule
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active', -- 'active', 'paused', 'completed', 'cancelled'
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    
    CONSTRAINT chk_campaign_type CHECK (campaign_type IN ('featured', 'boost', 'premium')),
    CONSTRAINT chk_campaign_status CHECK (status IN ('active', 'paused', 'completed', 'cancelled'))
);

CREATE INDEX idx_campaigns_dealer_id ON dealer_campaigns(dealer_id);
CREATE INDEX idx_campaigns_vehicle_id ON dealer_campaigns(vehicle_id);
CREATE INDEX idx_campaigns_status ON dealer_campaigns(status);
CREATE INDEX idx_campaigns_dates ON dealer_campaigns(start_date, end_date);

-- Tabla de Plan Features (para control de features por plan)
CREATE TABLE dealer_plan_features (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_type VARCHAR(20) NOT NULL,
    feature_key VARCHAR(100) NOT NULL,
    feature_value TEXT,
    is_enabled BOOLEAN DEFAULT true,
    
    UNIQUE(plan_type, feature_key),
    CONSTRAINT chk_plan_type CHECK (plan_type IN ('basic', 'pro', 'enterprise'))
);

-- Seed initial plan features
INSERT INTO dealer_plan_features (plan_type, feature_key, feature_value, is_enabled) VALUES
-- Basic Plan
('basic', 'max_listings', '50', true),
('basic', 'analytics_access', 'false', false),
('basic', 'bulk_upload', 'false', false),
('basic', 'featured_listings', '2', true),
('basic', 'custom_branding', 'false', false),
('basic', 'api_access', 'false', false),

-- Pro Plan
('pro', 'max_listings', '200', true),
('pro', 'analytics_access', 'true', true),
('pro', 'bulk_upload', 'true', true),
('pro', 'featured_listings', '10', true),
('pro', 'custom_branding', 'true', true),
('pro', 'api_access', 'false', false),
('pro', 'lead_management', 'true', true),
('pro', 'email_automation', 'true', true),

-- Enterprise Plan
('enterprise', 'max_listings', 'unlimited', true),
('enterprise', 'analytics_access', 'true', true),
('enterprise', 'bulk_upload', 'true', true),
('enterprise', 'featured_listings', 'unlimited', true),
('enterprise', 'custom_branding', 'true', true),
('enterprise', 'api_access', 'true', true),
('enterprise', 'lead_management', 'true', true),
('enterprise', 'email_automation', 'true', true),
('enterprise', 'white_label', 'true', true),
('enterprise', 'priority_support', 'true', true);
```

---

### 7. API Endpoints - DealerService

```csharp
// DealerService.Api/Controllers/DealersController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DealersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    // GET /api/dealers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDealer(Guid id)
    {
        var query = new GetDealerQuery(id);
        var dealer = await _mediator.Send(query);
        return Ok(ApiResponse<DealerDto>.Ok(dealer));
    }
    
    // POST /api/dealers/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsDealer([FromBody] RegisterDealerRequest request)
    {
        var command = new RegisterDealerCommand(
            request.UserId,
            request.BusinessName,
            request.BusinessType,
            request.LicenseNumber,
            request.PlanType
        );
        var dealer = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetDealer), new { id = dealer.Id }, dealer);
    }
    
    // PUT /api/dealers/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "DealerOrAdmin")]
    public async Task<IActionResult> UpdateDealer(Guid id, [FromBody] UpdateDealerRequest request)
    {
        var command = new UpdateDealerCommand(id, request);
        var dealer = await _mediator.Send(command);
        return Ok(ApiResponse<DealerDto>.Ok(dealer));
    }
    
    // GET /api/dealers/{id}/inventory
    [HttpGet("{id}/inventory")]
    [Authorize(Policy = "DealerOrAdmin")]
    public async Task<IActionResult> GetInventory(Guid id, [FromQuery] InventoryFilters filters)
    {
        var query = new GetDealerInventoryQuery(id, filters);
        var inventory = await _mediator.Send(query);
        return Ok(ApiResponse<List<InventoryItemDto>>.Ok(inventory));
    }
    
    // POST /api/dealers/{id}/inventory
    [HttpPost("{id}/inventory")]
    [Authorize(Policy = "Dealer")]
    public async Task<IActionResult> AddToInventory(Guid id, [FromBody] AddInventoryRequest request)
    {
        var command = new AddInventoryCommand(id, request.VehicleId, request.AcquisitionCost, request.ListingPrice);
        var item = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetInventory), new { id }, item);
    }
    
    // POST /api/dealers/{id}/bulk-upload
    [HttpPost("{id}/bulk-upload")]
    [Authorize(Policy = "DealerProOrHigher")]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<IActionResult> BulkUpload(Guid id, [FromForm] IFormFile file)
    {
        var command = new BulkUploadVehiclesCommand(id, file);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<BulkUploadResultDto>.Ok(result));
    }
    
    // GET /api/dealers/{id}/analytics
    [HttpGet("{id}/analytics")]
    [Authorize(Policy = "DealerProOrHigher")]
    public async Task<IActionResult> GetAnalytics(Guid id, [FromQuery] AnalyticsFilters filters)
    {
        var query = new GetDealerAnalyticsQuery(id, filters);
        var analytics = await _mediator.Send(query);
        return Ok(ApiResponse<DealerAnalyticsDto>.Ok(analytics));
    }
    
    // GET /api/dealers/{id}/leads
    [HttpGet("{id}/leads")]
    [Authorize(Policy = "DealerProOrHigher")]
    public async Task<IActionResult> GetLeads(Guid id, [FromQuery] LeadFilters filters)
    {
        var query = new GetDealerLeadsQuery(id, filters);
        var leads = await _mediator.Send(query);
        return Ok(ApiResponse<List<LeadDto>>.Ok(leads));
    }
    
    // POST /api/dealers/{id}/campaigns
    [HttpPost("{id}/campaigns")]
    [Authorize(Policy = "Dealer")]
    public async Task<IActionResult> CreateCampaign(Guid id, [FromBody] CreateCampaignRequest request)
    {
        var command = new CreateCampaignCommand(id, request);
        var campaign = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
    }
    
    // GET /api/dealers/{id}/subscription
    [HttpGet("{id}/subscription")]
    [Authorize(Policy = "DealerOrAdmin")]
    public async Task<IActionResult> GetSubscription(Guid id)
    {
        var query = new GetDealerSubscriptionQuery(id);
        var subscription = await _mediator.Send(query);
        return Ok(ApiResponse<SubscriptionDto>.Ok(subscription));
    }
    
    // POST /api/dealers/{id}/subscription/upgrade
    [HttpPost("{id}/subscription/upgrade")]
    [Authorize(Policy = "Dealer")]
    public async Task<IActionResult> UpgradePlan(Guid id, [FromBody] UpgradePlanRequest request)
    {
        var command = new UpgradePlanCommand(id, request.NewPlan);
        var subscription = await _mediator.Send(command);
        return Ok(ApiResponse<SubscriptionDto>.Ok(subscription));
    }
}
```

---

## 📱 MOBILE APP - DECISIONES TÉCNICAS

### React Native - Bare Workflow (NO Expo)

**Razones**:
1. **Necesidad de módulos nativos**:
   - Camera avanzada (fotos de vehículos)
   - Image picker con compresión
   - Biometric authentication
   - Push notifications (Firebase)
   - Maps (Google Maps/Apple Maps)

2. **Control total del build**:
   - Custom splash screens
   - App icons
   - Native code cuando sea necesario

3. **Performance**:
   - Hermes engine
   - React Native New Architecture (TurboModules)

### Estructura Mobile

```
packages/mobile/
├── android/                    # Android native code
├── ios/                        # iOS native code
├── src/
│   ├── screens/               # Pantallas principales
│   │   ├── auth/
│   │   │   ├── LoginScreen.tsx
│   │   │   └── RegisterScreen.tsx
│   │   ├── browse/
│   │   │   ├── BrowseScreen.tsx
│   │   │   ├── SearchScreen.tsx
│   │   │   └── VehicleDetailScreen.tsx
│   │   ├── dealer/
│   │   │   ├── DealerDashboardScreen.tsx
│   │   │   ├── AddVehicleScreen.tsx
│   │   │   ├── InventoryScreen.tsx
│   │   │   └── LeadsScreen.tsx
│   │   └── user/
│   │       ├── FavoritesScreen.tsx
│   │       ├── MessagesScreen.tsx
│   │       └── ProfileScreen.tsx
│   │
│   ├── components/            # Componentes mobile
│   │   ├── VehicleCard.tsx
│   │   ├── SearchBar.tsx
│   │   ├── ImagePicker.tsx
│   │   └── ... (más componentes)
│   │
│   ├── navigation/            # React Navigation
│   │   ├── AppNavigator.tsx
│   │   ├── AuthNavigator.tsx
│   │   └── types.ts
│   │
│   ├── hooks/                 # Custom hooks
│   │   ├── useCamera.ts
│   │   ├── useLocation.ts
│   │   └── usePushNotifications.ts
│   │
│   ├── utils/                 # Utilidades mobile
│   │   ├── permissions.ts
│   │   ├── imageCompression.ts
│   │   └── platform.ts
│   │
│   └── styles/                # Styles (NativeWind)
│       ├── theme.ts
│       └── colors.ts
│
├── App.tsx
├── index.js
├── tailwind.config.js         # NativeWind config
└── package.json
```

---

## 🎨 UI/UX - Dealer Panel Wireframes

### Dashboard Principal
```
┌─────────────────────────────────────────────────────────┐
│ 🏠 Dealer Dashboard                    [Profile] [Help] │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  📊 Quick Stats                                         │
│  ┌────────────┬────────────┬────────────┬────────────┐ │
│  │ Active     │ Total      │ Leads      │ Revenue    │ │
│  │ Listings   │ Views      │ This Week  │ This Month │ │
│  │    47      │   2,345    │     23     │  $45,230   │ │
│  └────────────┴────────────┴────────────┴────────────┘ │
│                                                          │
│  📈 Performance (Last 30 Days)                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  [Line Chart: Views, Inquiries, Sales]           │  │
│  │                                                   │  │
│  └──────────────────────────────────────────────────┘  │
│                                                          │
│  🚗 Recent Listings                    [View All →]    │
│  ┌──────────────┬──────────────┬──────────────┐       │
│  │ [Vehicle]    │ [Vehicle]    │ [Vehicle]    │       │
│  │ 2024 BMW X5  │ 2023 Tesla 3 │ 2024 Audi Q7 │       │
│  │ $58,500      │ $42,300      │ $67,900      │       │
│  │ 23 views     │ 45 views     │ 12 views     │       │
│  └──────────────┴──────────────┴──────────────┘       │
│                                                          │
│  ⚡ Quick Actions                                       │
│  [+ Add Vehicle] [📊 Analytics] [💬 Leads] [⚙️ Settings]│
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Inventory Management
```
┌─────────────────────────────────────────────────────────┐
│ 🏭 Inventory Management                                 │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  [🔍 Search] [🔽 Filter] [📤 Bulk Upload] [+ Add]      │
│                                                          │
│  Filters: [All] [In Stock] [On Hold] [Sold]           │
│                                                          │
│  ┌────────────────────────────────────────────────────┐│
│  │ [Image] 2024 BMW X5 M Sport                      │││
│  │ VIN: WBA5U7C07LHK53210                           │││
│  │ Cost: $55,000 | Listed: $58,500 | Margin: 6.4%  │││
│  │ Days in Inventory: 14 | Status: In Stock         │││
│  │ Views: 234 | Inquiries: 12 | Test Drives: 3     │││
│  │ [✏️ Edit] [🚀 Boost] [❌ Remove]                 │││
│  └────────────────────────────────────────────────────┘│
│  ┌────────────────────────────────────────────────────┐│
│  │ [Image] 2023 Tesla Model 3 Long Range           │││
│  │ ... (similar structure)                           │││
│  └────────────────────────────────────────────────────┘│
│                                                          │
│  [◀ Previous] Page 1 of 5 [Next ▶]                    │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🚀 PRÓXIMOS PASOS CONCRETOS

### 1. Setup Inicial (Esta semana)

```bash
# 1. Crear estructura del monorepo
cd frontend
mkdir -p packages/{shared,web,mobile,ui-components}

# 2. Configurar pnpm workspace
cat > pnpm-workspace.yaml << EOF
packages:
  - 'packages/*'
EOF

# 3. Mover proyecto actual a packages/web
mv src packages/web/
mv public packages/web/
mv index.html packages/web/
# ... mover archivos config

# 4. Crear package.json en cada paquete
# packages/shared/package.json
# packages/web/package.json
# packages/mobile/package.json (React Native init)
# packages/ui-components/package.json

# 5. Instalar dependencias
pnpm install

# 6. Verificar build
pnpm --filter @cardealer/web dev
```

### 2. Backend - DealerService MVP (Esta semana)

```bash
# Backend setup
cd backend

# Crear DealerService
dotnet new webapi -n DealerService.Api
dotnet new classlib -n DealerService.Domain
dotnet new classlib -n DealerService.Application
dotnet new classlib -n DealerService.Infrastructure

# Agregar a solution
dotnet sln add DealerService.Api
dotnet sln add DealerService.Domain
dotnet sln add DealerService.Application
dotnet sln add DealerService.Infrastructure

# Crear migrations
cd DealerService.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

**¿Procedo con alguna implementación específica?**

Opciones:
1. ✅ Crear estructura del monorepo frontend
2. ✅ Refactorizar AuthService para multi-rol
3. ✅ Crear DealerService básico
4. ✅ Actualizar frontend con AccountType enum
5. ✅ Otro (especifica)
