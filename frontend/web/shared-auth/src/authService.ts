// Shared Authentication Service
// Este servicio es compartido entre todos los diseños: okla, original, cardealer

export interface User {
  id: string;
  email: string;
  name: string;
  role: 'user' | 'dealer' | 'admin';
  theme: 'okla' | 'original' | 'cardealer';
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token: string;
  redirectUrl: string; // URL del diseño correspondiente
}

class SharedAuthService {
  private static instance: SharedAuthService;
  private currentUser: User | null = null;

  private constructor() {
    // Load user from localStorage if exists
    const storedUser = localStorage.getItem('auth_user');
    if (storedUser) {
      this.currentUser = JSON.parse(storedUser);
    }
  }

  static getInstance(): SharedAuthService {
    if (!SharedAuthService.instance) {
      SharedAuthService.instance = new SharedAuthService();
    }
    return SharedAuthService.instance;
  }

  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    try {
      // TODO: Reemplazar con llamada real al backend
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        throw new Error('Login failed');
      }

      const data: AuthResponse = await response.json();
      
      // Guardar usuario y token
      this.currentUser = data.user;
      localStorage.setItem('auth_user', JSON.stringify(data.user));
      localStorage.setItem('auth_token', data.token);

      return data;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  logout(): void {
    this.currentUser = null;
    localStorage.removeItem('auth_user');
    localStorage.removeItem('auth_token');
    window.location.href = '/login';
  }

  getCurrentUser(): User | null {
    return this.currentUser;
  }

  isAuthenticated(): boolean {
    return this.currentUser !== null;
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  // Determina a qué diseño redirigir basado en el rol/preferencia del usuario
  getRedirectUrl(user: User): string {
    const themeUrls = {
      okla: 'http://localhost:5173',
      original: 'http://localhost:5174',
      cardealer: 'http://localhost:5175',
    };

    return themeUrls[user.theme] || themeUrls.okla;
  }
}

export const sharedAuthService = SharedAuthService.getInstance();
