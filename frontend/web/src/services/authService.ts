import axios from 'axios';
import type { User, AccountType } from '@/types';

// Use Gateway URL for all API calls
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const AUTH_API_URL = `${API_BASE_URL}/api/auth`;
const EXTERNAL_AUTH_API_URL = `${API_BASE_URL}/api/ExternalAuth`;

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
    const accountTypeInt =
      typeof accountTypeValue === 'string' ? parseInt(accountTypeValue, 10) : accountTypeValue;

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
  const roles =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
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
  // 2FA fields (when requiresTwoFactor is true)
  requiresTwoFactor?: boolean;
  sessionToken?: string;
  twoFactorType?: string;
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
    userName?: string;
    email: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    requiresTwoFactor: boolean;
    tempToken: string | null;
    twoFactorType: string | null; // "authenticator", "sms", or "email"
    accountType?: string; // optional, may come from JWT
    // Profile data from OAuth providers
    firstName?: string;
    lastName?: string;
    profilePictureUrl?: string;
    isNewUser?: boolean;
  };
  error: string | null;
}

/**
 * Authentication service - handles login, register, logout, and token management
 */
export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/login`, {
        email: credentials.email,
        password: credentials.password,
      });

      const { data } = response.data;

      if (!data) {
        throw new Error('Invalid response from server');
      }

      // Check if 2FA is required
      if (data.requiresTwoFactor) {
        // Return 2FA response - login not complete yet
        return {
          user: {} as User, // Empty user, will be populated after 2FA
          accessToken: '',
          refreshToken: '',
          requiresTwoFactor: true,
          sessionToken: data.tempToken || '',
          twoFactorType: data.twoFactorType || 'authenticator',
        };
      }

      // Normal login flow (no 2FA or 2FA already completed)
      // Get account type from JWT or backend response
      const accountType = data.accountType
        ? (data.accountType as AccountType)
        : getAccountTypeFromJwt(data.accessToken);

      // Get claims from JWT
      const jwtPayload = decodeJwtPayload(data.accessToken);
      const fullName =
        (jwtPayload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string) ||
        data.email.split('@')[0];

      // Extract dealerId from JWT claims (if present and not empty)
      const rawDealerId = (jwtPayload?.['dealerId'] || jwtPayload?.['dealer_id']) as
        | string
        | undefined;
      const dealerId = rawDealerId && rawDealerId.trim() !== '' ? rawDealerId : undefined;

      const roles =
        jwtPayload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        jwtPayload?.['role'] ||
        jwtPayload?.['roles'];
      const roleList = Array.isArray(roles) ? roles : roles ? [roles] : [];

      console.log('🐛 DEBUG JWT payload (full):', jwtPayload);
      console.log('🐛 DEBUG extracted dealerId (raw):', rawDealerId, '→ (cleaned):', dealerId);
      console.log('🐛 DEBUG extracted roles:', roleList);

      const user: User = {
        id: data.userId,
        email: data.email,
        name: fullName, // Use 'name' to match User interface
        accountType: accountType,
        dealerId: dealerId, // Include dealerId from JWT
        roles: roleList as string[],
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
      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/register`, {
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
        name: data.fullName, // Use 'name' to match User interface
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
      // Store email for potential verification resend
      localStorage.setItem('pendingVerificationEmail', data.email);

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
      const accessToken = localStorage.getItem('accessToken');

      if (refreshToken && accessToken) {
        await axios.post(
          `${AUTH_API_URL}/logout`,
          { refreshToken },
          {
            headers: {
              Authorization: `Bearer ${accessToken}`,
            },
          }
        );
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

      const response = await axios.post<BackendAuthResponse>(`${AUTH_API_URL}/refresh-token`, {
        refreshToken,
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
      const response = await axios.get(`${AUTH_API_URL}/me`);
      return response.data;
    } catch (error) {
      console.error('Error fetching current user:', error);
      throw new Error('Failed to fetch user information');
    }
  },

  async updateProfile(updates: Partial<User>): Promise<User> {
    try {
      const response = await axios.put(`${AUTH_API_URL}/profile`, updates);
      return response.data;
    } catch (error) {
      console.error('Error updating profile:', error);
      throw new Error('Failed to update profile');
    }
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/change-password`, {
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
      await axios.post(`${AUTH_API_URL}/forgot-password`, { email });
    } catch (error) {
      console.error('Error sending password reset email:', error);
      throw new Error('Failed to send password reset email');
    }
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/reset-password`, {
        token,
        newPassword,
        confirmPassword: newPassword,
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
      await axios.post(`${AUTH_API_URL}/verify-email`, { token });
    } catch (error) {
      console.error('Error verifying email:', error);
      throw new Error('Failed to verify email');
    }
  },

  async resendVerificationEmail(email?: string): Promise<void> {
    try {
      // If email not provided, try to get it from localStorage (stored during registration)
      const userEmail = email || localStorage.getItem('pendingVerificationEmail');
      if (!userEmail) {
        throw new Error('Email address not found. Please try registering again.');
      }
      await axios.post(`${AUTH_API_URL}/resend-verification`, { email: userEmail });
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

  // ========================================
  // OAuth2 methods - Uses Backend API for authorization URLs
  // AUTH-EXT-001: Google Login
  // AUTH-EXT-002: Facebook Login
  // AUTH-EXT-003: Apple Login
  // AUTH-EXT-008: Microsoft Login
  // ========================================

  /**
   * Generic OAuth login method - calls backend to get authorization URL
   * More secure: credentials stay on backend
   */
  async initiateOAuthLogin(provider: 'google' | 'microsoft' | 'facebook' | 'apple'): Promise<void> {
    try {
      const redirectUri = `${window.location.origin}/auth/callback/${provider}`;

      // Backend response format: { data: { authorizationUrl: string }, success: boolean }
      const response = await axios.post<{
        data: { authorizationUrl: string };
        success: boolean;
        error?: string;
      }>(`${EXTERNAL_AUTH_API_URL}/login`, {
        provider,
        redirectUri,
      });

      const authUrl = response.data?.data?.authorizationUrl;
      if (authUrl) {
        // Redirect to provider's OAuth page
        window.location.href = authUrl;
      } else {
        throw new Error(`Failed to get authorization URL for ${provider}`);
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message =
          error.response.data?.error || error.response.data?.message || `${provider} login failed`;
        throw new Error(message);
      }
      throw new Error(`Failed to initiate ${provider} login. Please try again.`);
    }
  },

  async loginWithGoogle(): Promise<void> {
    // First try backend API, fallback to client-side if VITE_GOOGLE_CLIENT_ID is set
    const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

    if (googleClientId) {
      // Client-side OAuth (legacy support)
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

      window.location.href = authUrl.toString();
    } else {
      // Use backend API
      await this.initiateOAuthLogin('google');
    }
  },

  async loginWithMicrosoft(): Promise<void> {
    // Use backend API for OAuth authorization URL
    await this.initiateOAuthLogin('microsoft');
  },

  async loginWithFacebook(): Promise<void> {
    // Use backend API for OAuth authorization URL
    await this.initiateOAuthLogin('facebook');
  },

  async loginWithApple(): Promise<void> {
    // Use backend API for OAuth authorization URL
    await this.initiateOAuthLogin('apple');
  },

  async handleOAuthCallback(
    provider: 'google' | 'microsoft' | 'facebook' | 'apple',
    code: string,
    idToken?: string // Apple provides id_token
  ): Promise<LoginResponse> {
    try {
      const response = await axios.post<BackendAuthResponse>(`${EXTERNAL_AUTH_API_URL}/callback`, {
        provider,
        code,
        idToken,
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
        name: data.userName,
        fullName:
          data.firstName && data.lastName
            ? `${data.firstName} ${data.lastName}`
            : data.email.split('@')[0],
        firstName: data.firstName,
        lastName: data.lastName,
        avatar: data.profilePictureUrl,
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

  // ========================================
  // LINKED ACCOUNTS (External Providers)
  // AUTH-EXT-005: Link Account
  // AUTH-EXT-006: Unlink Account
  // AUTH-EXT-007: List Linked Accounts
  // ========================================

  async getLinkedAccounts(): Promise<LinkedAccount[]> {
    try {
      const response = await axios.get<{ data: LinkedAccount[] }>(
        `${EXTERNAL_AUTH_API_URL}/linked-accounts`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );
      return response.data.data || [];
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to get linked accounts';
        throw new Error(message);
      }
      throw new Error('Failed to get linked accounts');
    }
  },

  /**
   * AUTH-EXT-005: Link an external OAuth provider to the current user's account
   * @param provider - The OAuth provider (google, microsoft, facebook, apple)
   * @param idToken - The ID token received from the OAuth flow
   * @returns LinkAccountResponse with success status and new tokens
   */
  async linkExternalAccount(
    provider: 'google' | 'microsoft' | 'facebook' | 'apple',
    idToken: string
  ): Promise<LinkAccountResponse> {
    try {
      const response = await axios.post<{
        data: LinkAccountResponse;
        metadata: Record<string, unknown>;
      }>(
        `${EXTERNAL_AUTH_API_URL}/link-account`,
        {
          provider,
          idToken,
        },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );

      const result = response.data.data;

      // Update tokens in localStorage if new tokens were returned
      if (result.accessToken) {
        localStorage.setItem('accessToken', result.accessToken);
      }
      if (result.refreshToken) {
        localStorage.setItem('refreshToken', result.refreshToken);
      }

      return result;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to link account';
        throw new Error(message);
      }
      throw new Error('Failed to link account. Please try again.');
    }
  },

  /**
   * AUTH-EXT-006: Unlink an external OAuth provider from the current user's account
   * Security: User must have a password set before unlinking
   * @param provider - The OAuth provider to unlink
   * @returns UnlinkAccountResponse with success status
   */
  async unlinkExternalAccount(provider: string): Promise<UnlinkAccountResponse> {
    try {
      const response = await axios.delete<{ data: UnlinkAccountResponse }>(
        `${EXTERNAL_AUTH_API_URL}/unlink-account`,
        {
          data: { provider },
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to unlink account';
        throw new Error(message);
      }
      throw new Error('Failed to unlink account. Please try again.');
    }
  },

  // Start OAuth flow for linking (different from login - requires auth)
  async startLinkAccount(provider: 'google' | 'microsoft' | 'facebook' | 'apple'): Promise<void> {
    // Store that we're linking, not logging in
    sessionStorage.setItem('oauth_mode', 'link');

    switch (provider) {
      case 'google':
        await this.loginWithGoogle();
        break;
      case 'microsoft':
        await this.loginWithMicrosoft();
        break;
      case 'facebook':
        await this.loginWithFacebook();
        break;
      case 'apple':
        await this.loginWithApple();
        break;
    }
  },
};

// Type for linked external accounts (AUTH-EXT-007 response)
export interface LinkedAccount {
  provider: string;
  providerUserId: string;
  email: string;
  name?: string;
  linkedAt: string;
}

// Response from linking an external account (AUTH-EXT-005)
export interface LinkAccountResponse {
  userId: string;
  userName: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  isNewUser: boolean;
}

// Response from unlinking an external account (AUTH-EXT-006)
export interface UnlinkAccountResponse {
  success: boolean;
  message: string;
  provider: string;
  unlinkedAt: string;
}
