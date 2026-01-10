import axios from 'axios';
import type { User, AccountType } from '@/types';

const AUTH_API_URL = import.meta.env.VITE_AUTH_SERVICE_URL || 'http://localhost:15085/api';

/**
 * Decode JWT token payload (without verification)
 */
function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

/**
 * Extract account type from JWT claims
 * Backend sends account_type as integer: Guest=0, Individual=1, Dealer=2, DealerEmployee=3, Admin=4, PlatformEmployee=5
 */
function getAccountTypeFromJwt(token: string): AccountType {
  const payload = decodeJwtPayload(token);
  console.log('🐛 DEBUG JWT payload:', payload);
  
  if (!payload) return 'individual';
  
  // Check for account_type claim (custom claim) - backend sends as integer
  const accountTypeValue = payload['account_type'] || payload['accountType'];
  console.log('🐛 DEBUG accountTypeValue from JWT:', accountTypeValue);
  
  if (accountTypeValue !== undefined) {
    const accountTypeInt = typeof accountTypeValue === 'string' 
      ? parseInt(accountTypeValue, 10) 
      : accountTypeValue;
    
    console.log('🐛 DEBUG accountTypeInt:', accountTypeInt);
    
    // Map backend enum to frontend string
    switch (accountTypeInt) {
      case 0: 
        console.log('🎯 Mapping to: guest');
        return 'guest';
      case 1: 
        console.log('🎯 Mapping to: individual');
        return 'individual';
      case 2: 
        console.log('🎯 Mapping to: dealer');
        return 'dealer';
      case 3: 
        console.log('🎯 Mapping to: dealer_employee');
        return 'dealer_employee';
      case 4: 
        console.log('🎯 Mapping to: admin');
        return 'admin';
      case 5: 
        console.log('🎯 Mapping to: platform_employee');
        return 'platform_employee';
      default: 
        console.log('🎯 Default mapping to: individual');
        return 'individual';
    }
  }
  
  // Check for role claims
  const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
                payload['role'] || 
                payload['roles'] || 
                [];
  
  const roleList = Array.isArray(roles) ? roles : [roles];
  
  if (roleList.includes('admin') || roleList.includes('Admin')) {
    return 'admin';
  }
  if (roleList.includes('dealer') || roleList.includes('Dealer')) {
    return 'dealer';
  }
  if (roleList.includes('dealer_employee') || roleList.includes('DealerEmployee')) {
    return 'dealer_employee';
  }
  
  // Check dealerId - if present and not empty, user is a dealer
  const dealerId = payload['dealerId'] || payload['dealer_id'];
  if (dealerId && dealerId !== '') {
    return 'dealer';
  }
  
  return 'individual';
}

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface RegisterData {
  email: string;
  password: string;
  fullName: string;
  userName: string;
  phone?: string;
  accountType: 'individual' | 'dealer';
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

// Backend API response types (from AuthService)
interface BackendAuthResponse {
  success: boolean;
  data: {
    userId: string;
    email: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    requiresTwoFactor: boolean;
    tempToken: string | null;
  };
  error: string | null;
}

/**
 * Authentication service - handles login, register, logout, and token management
 */
export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/Auth/login`, {
        email: credentials.email,
        password: credentials.password,
      });

      const { data } = response.data;

      if (!data) {
        throw new Error('Invalid response from server');
      }

      // Transform backend response to frontend format
      // Get account type from JWT or backend response
      const accountType = data.accountType 
        ? (data.accountType as AccountType)
        : getAccountTypeFromJwt(data.accessToken);
      
      // Get fullName from JWT claims
      const jwtPayload = decodeJwtPayload(data.accessToken);
      const fullName = jwtPayload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string 
        || data.email.split('@')[0];
      
      const user: User = {
        id: data.userId,
        email: data.email,
        fullName: fullName,
        accountType: accountType,
        emailVerified: true,
        createdAt: new Date().toISOString(),
      };

      const loginResponse: LoginResponse = {
        user,
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      };

      // Store tokens
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('userId', data.userId);

      if (credentials.rememberMe) {
        localStorage.setItem('rememberMe', 'true');
      }

      return loginResponse;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Invalid email or password';
        throw new Error(message);
      }
      throw new Error('Login failed. Please try again.');
    }
  },

  async register(data: RegisterData): Promise<LoginResponse> {
    try {
      // Backend only expects: userName, email, password
      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/Auth/register`, {
        userName: data.userName,
        email: data.email,
        password: data.password,
      });

      const { data: backendData } = response.data;

      if (!backendData) {
        throw new Error('Invalid response from server');
      }

      // Transform backend response to frontend format
      const user: User = {
        id: backendData.userId,
        email: backendData.email,
        fullName: data.fullName,
        accountType: data.accountType,
        emailVerified: false, // User needs to verify email
        createdAt: new Date().toISOString(),
      };

      const loginResponse: LoginResponse = {
        user,
        accessToken: backendData.accessToken,
        refreshToken: backendData.refreshToken,
      };

      // Store tokens
      localStorage.setItem('accessToken', backendData.accessToken);
      localStorage.setItem('refreshToken', backendData.refreshToken);
      localStorage.setItem('userId', backendData.userId);

      return loginResponse;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Registration failed';
        throw new Error(message);
      }
      throw new Error('Registration failed. Please try again.');
    }
  },

  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      
      if (refreshToken) {
        await axios.post(`${AUTH_API_URL}/Auth/logout`, { refreshToken });
      }
    } catch (error) {
      console.error('Error during logout:', error);
    } finally {
      // Clear tokens regardless of API call success
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userId');
      localStorage.removeItem('rememberMe');
    }
  },

  async refreshToken(): Promise<RefreshTokenResponse> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');

      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/Auth/refresh-token`, { 
        refreshToken 
      });

      const { data } = response.data;

      if (!data) {
        throw new Error('Invalid response from server');
      }

      // Update tokens
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);

      return {
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      };
    } catch (error) {
      // If refresh fails, clear tokens and force re-login
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userId');
      localStorage.removeItem('rememberMe');
      throw new Error('Session expired. Please login again.');
    }
  },

  async getCurrentUser(): Promise<User> {
    try {
      const response = await axios.get(`${AUTH_API_URL}/auth/me`);
      return response.data;
    } catch (error) {
      console.error('Error fetching current user:', error);
      throw new Error('Failed to fetch user information');
    }
  },

  async updateProfile(updates: Partial<User>): Promise<User> {
    try {
      const response = await axios.put(`${AUTH_API_URL}/auth/profile`, updates);
      return response.data;
    } catch (error) {
      console.error('Error updating profile:', error);
      throw new Error('Failed to update profile');
    }
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/auth/change-password`, {
        currentPassword,
        newPassword,
      });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.message || 'Failed to change password';
        throw new Error(message);
      }
      throw new Error('Failed to change password');
    }
  },

  async forgotPassword(email: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/Auth/forgot-password`, { email });
    } catch (error) {
      console.error('Error sending password reset email:', error);
      throw new Error('Failed to send password reset email');
    }
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/Auth/reset-password`, {
        token,
        newPassword,
      });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to reset password';
        throw new Error(message);
      }
      throw new Error('Failed to reset password');
    }
  },

  async verifyEmail(token: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/Auth/verify-email`, { token });
    } catch (error) {
      console.error('Error verifying email:', error);
      throw new Error('Failed to verify email');
    }
  },

  async resendVerificationEmail(): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/auth/resend-verification`);
    } catch (error) {
      console.error('Error resending verification email:', error);
      throw new Error('Failed to resend verification email');
    }
  },

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  },

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  },

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  },

  // OAuth2 methods
  async loginWithGoogle(): Promise<void> {
    const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
    if (!googleClientId) {
      throw new Error('Google Client ID not configured');
    }

    // Construct OAuth URL
    const redirectUri = `${window.location.origin}/auth/callback/google`;
    const scope = 'openid email profile';
    const responseType = 'code';
    
    const authUrl = new URL('https://accounts.google.com/o/oauth2/v2/auth');
    authUrl.searchParams.append('client_id', googleClientId);
    authUrl.searchParams.append('redirect_uri', redirectUri);
    authUrl.searchParams.append('response_type', responseType);
    authUrl.searchParams.append('scope', scope);
    authUrl.searchParams.append('access_type', 'offline');
    authUrl.searchParams.append('prompt', 'consent');

    // Redirect to Google OAuth
    window.location.href = authUrl.toString();
  },

  async loginWithMicrosoft(): Promise<void> {
    const microsoftClientId = import.meta.env.VITE_MICROSOFT_CLIENT_ID;
    if (!microsoftClientId) {
      throw new Error('Microsoft Client ID not configured');
    }

    // Construct OAuth URL
    const redirectUri = `${window.location.origin}/auth/callback/microsoft`;
    const scope = 'openid email profile';
    const responseType = 'code';
    
    const authUrl = new URL('https://login.microsoftonline.com/common/oauth2/v2.0/authorize');
    authUrl.searchParams.append('client_id', microsoftClientId);
    authUrl.searchParams.append('redirect_uri', redirectUri);
    authUrl.searchParams.append('response_type', responseType);
    authUrl.searchParams.append('scope', scope);
    authUrl.searchParams.append('response_mode', 'query');

    // Redirect to Microsoft OAuth
    window.location.href = authUrl.toString();
  },

  async handleOAuthCallback(provider: 'google' | 'microsoft', code: string): Promise<LoginResponse> {
    try {
      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/ExternalAuth/callback`, {
        provider,
        code,
        redirectUri: `${window.location.origin}/auth/callback/${provider}`,
      });

      const { data } = response.data;

      if (!data) {
        throw new Error('Invalid response from server');
      }

      // Transform backend response to frontend format
      const user: User = {
        id: data.userId,
        email: data.email,
        fullName: data.email.split('@')[0],
        accountType: 'individual',
        emailVerified: true,
        createdAt: new Date().toISOString(),
      };

      const loginResponse: LoginResponse = {
        user,
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
      };

      // Store tokens
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('userId', data.userId);

      return loginResponse;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'OAuth authentication failed';
        throw new Error(message);
      }
      throw new Error('OAuth authentication failed. Please try again.');
    }
  },
};
