# 🔐 SPRINT 2 - Integración Completa de Autenticación

**Fecha:** 2 Enero 2026  
**Duración estimada:** 4-5 horas  
**Tokens estimados:** ~25,000  
**Prioridad:** 🔴 CRÍTICO

---

## 🎯 OBJETIVOS

1. Conectar frontend React con AuthService backend
2. Implementar login/registro con JWT
3. Agregar OAuth2 (Google y Microsoft)
4. Implementar refresh token automático
5. Gestionar estado de autenticación con Zustand
6. Crear guards para rutas protegidas
7. Implementar recuperación de contraseña

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: AuthService Backend Validation (30 min)

- [ ] 1.1. Verificar que AuthService está UP
- [ ] 1.2. Validar endpoints con Swagger
- [ ] 1.3. Probar registro de usuario vía Postman
- [ ] 1.4. Probar login y obtener JWT
- [ ] 1.5. Documentar estructura de respuestas

### Fase 2: Frontend Auth Store (45 min)

- [ ] 2.1. Crear Zustand store para auth
- [ ] 2.2. Implementar actions (login, logout, register)
- [ ] 2.3. Persistir tokens en localStorage
- [ ] 2.4. Implementar auto-refresh de tokens
- [ ] 2.5. Agregar loading y error states

### Fase 3: Auth Service Layer (60 min)

- [ ] 3.1. Actualizar `authService.ts` para usar backend real
- [ ] 3.2. Eliminar mocks de autenticación
- [ ] 3.3. Implementar registro de usuarios
- [ ] 3.4. Implementar login con email/password
- [ ] 3.5. Implementar logout
- [ ] 3.6. Implementar refresh token
- [ ] 3.7. Implementar recuperación de contraseña

### Fase 4: OAuth2 Integration (60 min)

- [ ] 4.1. Configurar Google OAuth en backend
- [ ] 4.2. Configurar Microsoft OAuth en backend
- [ ] 4.3. Implementar botones de OAuth en frontend
- [ ] 4.4. Manejar callbacks de OAuth
- [ ] 4.5. Validar flujo completo OAuth

### Fase 5: Protected Routes (30 min)

- [ ] 5.1. Crear componente PrivateRoute
- [ ] 5.2. Proteger rutas de dealer dashboard
- [ ] 5.3. Proteger rutas de admin panel
- [ ] 5.4. Implementar redirección a login
- [ ] 5.5. Mantener URL original post-login

### Fase 6: UI Components (45 min)

- [ ] 6.1. Actualizar LoginPage
- [ ] 6.2. Actualizar RegisterPage
- [ ] 6.3. Crear ForgotPasswordPage
- [ ] 6.4. Crear ResetPasswordPage
- [ ] 6.5. Agregar validaciones con Zod

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Validar AuthService Backend

**Verificar que AuthService esté corriendo:**

```bash
# Health check
curl http://localhost:15085/health

# Swagger UI
open http://localhost:15085/swagger
```

**Endpoints disponibles en AuthService:**

| Method | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Registrar nuevo usuario | No |
| POST | `/api/auth/login` | Login con email/password | No |
| POST | `/api/auth/refresh-token` | Obtener nuevo access token | No |
| POST | `/api/auth/logout` | Cerrar sesión | Sí |
| GET | `/api/auth/me` | Obtener usuario actual | Sí |
| PUT | `/api/auth/change-password` | Cambiar contraseña | Sí |
| POST | `/api/auth/forgot-password` | Solicitar reset password | No |
| POST | `/api/auth/reset-password` | Resetear password con token | No |
| POST | `/api/auth/google` | Login con Google | No |
| POST | `/api/auth/microsoft` | Login con Microsoft | No |
| POST | `/api/auth/enable-2fa` | Habilitar 2FA | Sí |
| POST | `/api/auth/verify-2fa` | Verificar código 2FA | No |

---

### 2️⃣ Frontend - Auth Store (Zustand)

**Archivo:** `frontend/web/original/src/store/authStore.ts`

```typescript
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '@/types';
import { authService } from '@/services/authService';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthActions {
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<void>;
  setUser: (user: User) => void;
  clearError: () => void;
}

type AuthStore = AuthState & AuthActions;

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      // State
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Actions
      login: async (email: string, password: string) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.login({ email, password });
          
          set({
            user: response.user,
            accessToken: response.accessToken,
            refreshToken: response.refreshToken,
            isAuthenticated: true,
            isLoading: false,
          });

          // Guardar tokens en localStorage (para axios interceptor)
          localStorage.setItem('accessToken', response.accessToken);
          localStorage.setItem('refreshToken', response.refreshToken);
        } catch (error: any) {
          set({
            error: error.message || 'Login failed',
            isLoading: false,
          });
          throw error;
        }
      },

      register: async (data: RegisterData) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.register(data);
          
          set({
            user: response.user,
            accessToken: response.accessToken,
            refreshToken: response.refreshToken,
            isAuthenticated: true,
            isLoading: false,
          });

          localStorage.setItem('accessToken', response.accessToken);
          localStorage.setItem('refreshToken', response.refreshToken);
        } catch (error: any) {
          set({
            error: error.message || 'Registration failed',
            isLoading: false,
          });
          throw error;
        }
      },

      logout: async () => {
        try {
          await authService.logout();
        } catch (error) {
          console.error('Logout error:', error);
        } finally {
          // Limpiar estado incluso si falla la llamada al backend
          set({
            user: null,
            accessToken: null,
            refreshToken: null,
            isAuthenticated: false,
            error: null,
          });

          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      },

      refreshAccessToken: async () => {
        const { refreshToken } = get();
        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        try {
          const response = await authService.refreshToken(refreshToken);
          
          set({
            accessToken: response.accessToken,
            refreshToken: response.refreshToken,
          });

          localStorage.setItem('accessToken', response.accessToken);
          localStorage.setItem('refreshToken', response.refreshToken);
        } catch (error) {
          // Si falla el refresh, logout
          get().logout();
          throw error;
        }
      },

      setUser: (user: User) => {
        set({ user });
      },

      clearError: () => {
        set({ error: null });
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

// Hook para obtener usuario actual
export const useCurrentUser = () => useAuthStore((state) => state.user);

// Hook para verificar si está autenticado
export const useIsAuthenticated = () => useAuthStore((state) => state.isAuthenticated);
```

---

### 3️⃣ Frontend - Auth Service (Sin Mocks)

**Archivo:** `frontend/web/original/src/services/authService.ts`

```typescript
import { api } from './api';
import type { User } from '@/types';

const AUTH_API_URL = import.meta.env.VITE_AUTH_SERVICE_URL || 'http://localhost:15085/api';

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface RegisterData {
  email: string;
  password: string;
  fullName: string;
  phone?: string;
  accountType?: 'individual' | 'dealer';
}

interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
}

interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

interface ForgotPasswordRequest {
  email: string;
}

interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/**
 * Authentication service - handles login, register, logout, and token management
 */
export const authService = {
  /**
   * Login with email and password
   */
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/login', {
      email: credentials.email,
      password: credentials.password,
    });

    return response.data;
  },

  /**
   * Register new user
   */
  async register(data: RegisterData): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/register', {
      email: data.email,
      password: data.password,
      fullName: data.fullName,
      phone: data.phone,
      accountType: data.accountType || 'individual',
    });

    return response.data;
  },

  /**
   * Logout current user
   */
  async logout(): Promise<void> {
    await api.post('/auth/logout');
  },

  /**
   * Refresh access token using refresh token
   */
  async refreshToken(refreshToken: string): Promise<RefreshTokenResponse> {
    const response = await api.post<RefreshTokenResponse>('/auth/refresh-token', {
      refreshToken,
    });

    return response.data;
  },

  /**
   * Get current user profile
   */
  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },

  /**
   * Update user profile
   */
  async updateProfile(data: Partial<User>): Promise<User> {
    const response = await api.put<User>('/auth/profile', data);
    return response.data;
  },

  /**
   * Change password
   */
  async changePassword(data: ChangePasswordRequest): Promise<void> {
    await api.put('/auth/change-password', data);
  },

  /**
   * Request password reset email
   */
  async forgotPassword(data: ForgotPasswordRequest): Promise<void> {
    await api.post('/auth/forgot-password', data);
  },

  /**
   * Reset password with token
   */
  async resetPassword(data: ResetPasswordRequest): Promise<void> {
    await api.post('/auth/reset-password', data);
  },

  /**
   * Login with Google OAuth
   */
  async loginWithGoogle(tokenId: string): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/google', {
      tokenId,
    });

    return response.data;
  },

  /**
   * Login with Microsoft OAuth
   */
  async loginWithMicrosoft(accessToken: string): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/auth/microsoft', {
      accessToken,
    });

    return response.data;
  },

  /**
   * Enable 2FA for user
   */
  async enable2FA(): Promise<{ qrCode: string; secret: string }> {
    const response = await api.post<{ qrCode: string; secret: string }>('/auth/enable-2fa');
    return response.data;
  },

  /**
   * Verify 2FA code
   */
  async verify2FA(code: string): Promise<void> {
    await api.post('/auth/verify-2fa', { code });
  },
};
```

---

### 4️⃣ Frontend - Axios Interceptor Mejorado

**Archivo:** `frontend/web/original/src/services/api.ts`

Actualizar con mejor manejo de refresh token:

```typescript
import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443/api';

// Create axios instance
export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: parseInt(import.meta.env.VITE_API_TIMEOUT || '30000'),
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Flag para evitar múltiples refresh simultáneos
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: any) => void;
  reject: (reason?: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });

  failedQueue = [];
};

// Request interceptor - Add auth token
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle token refresh
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Si es 401 y no es el endpoint de refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (originalRequest.url?.includes('/auth/refresh-token')) {
        // Si falla el refresh, logout
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        // Si ya hay un refresh en curso, agregar a la cola
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        isRefreshing = false;
        localStorage.removeItem('accessToken');
        window.location.href = '/login';
        return Promise.reject(error);
      }

      try {
        const response = await axios.post(`${API_BASE_URL}/auth/refresh-token`, {
          refreshToken,
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data;

        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        processQueue(null, accessToken);

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }

        return api(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

---

### 5️⃣ Frontend - Protected Routes

**Archivo:** `frontend/web/original/src/components/auth/PrivateRoute.tsx`

```typescript
import { Navigate, useLocation } from 'react-router-dom';
import { useIsAuthenticated } from '@/store/authStore';
import { Loader2 } from 'lucide-react';

interface PrivateRouteProps {
  children: React.ReactNode;
  requiredRole?: 'admin' | 'dealer' | 'individual';
}

export const PrivateRoute = ({ children, requiredRole }: PrivateRouteProps) => {
  const isAuthenticated = useIsAuthenticated();
  const location = useLocation();

  // Mostrar loading mientras verifica autenticación
  if (isAuthenticated === undefined) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-primary-600" />
      </div>
    );
  }

  // Si no está autenticado, redirigir a login
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // TODO: Verificar rol si se proporciona requiredRole

  return <>{children}</>;
};
```

**Uso en App.tsx:**

```typescript
import { PrivateRoute } from '@/components/auth/PrivateRoute';

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      
      {/* Rutas protegidas */}
      <Route
        path="/dashboard"
        element={
          <PrivateRoute>
            <DealerDashboard />
          </PrivateRoute>
        }
      />
      
      {/* Ruta de admin (requiere rol) */}
      <Route
        path="/admin/*"
        element={
          <PrivateRoute requiredRole="admin">
            <AdminPanel />
          </PrivateRoute>
        }
      />
    </Routes>
  );
}
```

---

### 6️⃣ Frontend - Login Page Actualizado

**Archivo:** `frontend/web/original/src/pages/auth/LoginPage.tsx`

```typescript
import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuthStore } from '@/store/authStore';
import toast from 'react-hot-toast';
import { Loader2, Mail, Lock, Eye, EyeOff } from 'lucide-react';

const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Mínimo 6 caracteres'),
  rememberMe: z.boolean().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

export const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const login = useAuthStore((state) => state.login);
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const from = (location.state as any)?.from?.pathname || '/';

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      await login(data.email, data.password);
      toast.success('¡Bienvenido de nuevo!');
      navigate(from, { replace: true });
    } catch (error: any) {
      toast.error(error.message || 'Error al iniciar sesión');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Iniciar Sesión
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            ¿No tienes cuenta?{' '}
            <Link to="/register" className="font-medium text-primary-600 hover:text-primary-500">
              Regístrate aquí
            </Link>
          </p>
        </div>

        <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)}>
          {/* Email */}
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <div className="mt-1 relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Mail className="h-5 w-5 text-gray-400" />
              </div>
              <input
                {...register('email')}
                type="email"
                className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md"
                placeholder="tu@email.com"
              />
            </div>
            {errors.email && (
              <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
            )}
          </div>

          {/* Password */}
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              Contraseña
            </label>
            <div className="mt-1 relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <Lock className="h-5 w-5 text-gray-400" />
              </div>
              <input
                {...register('password')}
                type={showPassword ? 'text' : 'password'}
                className="block w-full pl-10 pr-10 py-2 border border-gray-300 rounded-md"
                placeholder="••••••••"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                {showPassword ? (
                  <EyeOff className="h-5 w-5 text-gray-400" />
                ) : (
                  <Eye className="h-5 w-5 text-gray-400" />
                )}
              </button>
            </div>
            {errors.password && (
              <p className="mt-1 text-sm text-red-600">{errors.password.message}</p>
            )}
          </div>

          {/* Remember me */}
          <div className="flex items-center justify-between">
            <div className="flex items-center">
              <input
                {...register('rememberMe')}
                type="checkbox"
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <label htmlFor="rememberMe" className="ml-2 block text-sm text-gray-900">
                Recordarme
              </label>
            </div>

            <div className="text-sm">
              <Link
                to="/forgot-password"
                className="font-medium text-primary-600 hover:text-primary-500"
              >
                ¿Olvidaste tu contraseña?
              </Link>
            </div>
          </div>

          {/* Submit button */}
          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
          >
            {isLoading ? (
              <>
                <Loader2 className="animate-spin h-5 w-5 mr-2" />
                Iniciando sesión...
              </>
            ) : (
              'Iniciar Sesión'
            )}
          </button>

          {/* OAuth buttons */}
          <div className="mt-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-gray-50 text-gray-500">O continúa con</span>
              </div>
            </div>

            <div className="mt-6 grid grid-cols-2 gap-3">
              <button
                type="button"
                className="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
              >
                <img src="/google-icon.svg" alt="Google" className="h-5 w-5" />
                <span className="ml-2">Google</span>
              </button>

              <button
                type="button"
                className="w-full inline-flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-500 hover:bg-gray-50"
              >
                <img src="/microsoft-icon.svg" alt="Microsoft" className="h-5 w-5" />
                <span className="ml-2">Microsoft</span>
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  );
};
```

---

### 7️⃣ Backend - Configurar OAuth2

**Archivo:** `backend/AuthService/AuthService.Api/appsettings.json`

Agregar configuración de OAuth:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "${GOOGLE_OAUTH_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_OAUTH_CLIENT_SECRET}"
    },
    "Microsoft": {
      "ClientId": "${MICROSOFT_OAUTH_CLIENT_ID}",
      "ClientSecret": "${MICROSOFT_OAUTH_CLIENT_SECRET}",
      "TenantId": "common"
    }
  }
}
```

**Archivo:** `backend/AuthService/AuthService.Api/Program.cs`

Verificar que tenga configuración OAuth:

```csharp
// Google OAuth
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
        options.TenantId = builder.Configuration["Authentication:Microsoft:TenantId"];
    });
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Test 1: Registro de Usuario

```bash
curl -X POST http://localhost:18443/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@test.com",
    "password": "Test123!",
    "fullName": "New User",
    "accountType": "individual"
  }'
```

**Resultado esperado:** Usuario creado, retorna JWT tokens

### Test 2: Login

```bash
curl -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@test.com",
    "password": "Test123!"
  }'
```

**Resultado esperado:** Retorna accessToken y refreshToken válidos

### Test 3: Get Current User

```bash
TOKEN="your_access_token_here"
curl -X GET http://localhost:18443/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

**Resultado esperado:** Retorna datos del usuario autenticado

### Test 4: Refresh Token

```bash
curl -X POST http://localhost:18443/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token_here"
  }'
```

**Resultado esperado:** Retorna nuevo accessToken

---

## 🧪 TESTING DESDE FRONTEND

1. **Levantar backend:**
   ```bash
   docker-compose up -d gateway authservice
   ```

2. **Levantar frontend:**
   ```bash
   cd frontend/web/original
   npm run dev
   ```

3. **Probar registro:**
   - Ir a http://localhost:5174/register
   - Completar formulario
   - Verificar que redirige a dashboard
   - Verificar tokens en DevTools → Application → Local Storage

4. **Probar login:**
   - Logout
   - Ir a http://localhost:5174/login
   - Login con credenciales creadas
   - Verificar que funciona

5. **Probar refresh automático:**
   - Esperar a que expire el access token (o cambiar expiración a 1 minuto)
   - Hacer request a endpoint protegido
   - Verificar en Network que se hace refresh automático

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Auth Store con Zustand | 3,000 |
| Auth Service actualizado | 4,000 |
| Axios interceptor mejorado | 2,500 |
| PrivateRoute component | 1,500 |
| LoginPage actualizado | 3,500 |
| RegisterPage actualizado | 3,500 |
| OAuth configuration | 2,000 |
| Testing y validación | 3,000 |
| **TOTAL** | **~23,000** |

**Con buffer 15%:** ~26,500 tokens

---

## 🚨 TROUBLESHOOTING

### Error: "CORS policy blocks request"

**Solución:**
```bash
# Verificar CORS en Gateway
docker logs gateway | grep CORS

# Reiniciar Gateway
docker-compose restart gateway
```

### Error: "Invalid JWT token"

**Solución:**
1. Verificar que JWT__KEY es el mismo en AuthService y Gateway
2. Verificar formato del token en Authorization header
3. Verificar que token no expiró

### Error: "Refresh token failed"

**Solución:**
1. Limpiar localStorage
2. Login nuevo
3. Verificar que refreshToken se guarda correctamente

---

## ➡️ PRÓXIMO SPRINT

**Sprint 3:** [SPRINT_3_VEHICLE_SERVICE.md](SPRINT_3_VEHICLE_SERVICE.md)

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
