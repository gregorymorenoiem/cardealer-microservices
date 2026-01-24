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
  // AUTH-SEC-005: Revoked device verification fields
  requiresRevokedDeviceVerification?: boolean;
  deviceFingerprint?: string;
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
    // AUTH-SEC-005: Revoked device verification
    requiresRevokedDeviceVerification?: boolean;
    deviceFingerprint?: string | null;
  };
  error: string | null;
}

// AUTH-SEC-005: Revoked device verification types
interface RevokedDeviceCodeRequest {
  userId: string;
  email: string;
  deviceFingerprint: string;
}

interface RevokedDeviceCodeResponse {
  success: boolean;
  data: {
    requiresVerification: boolean;
    message: string;
    verificationToken?: string;
    codeExpiresAt?: string;
  };
}

interface RevokedDeviceVerifyRequest {
  verificationToken: string;
  code: string;
}

interface RevokedDeviceVerifyResponse {
  success: boolean;
  data: {
    success: boolean;
    message: string;
    deviceCleared?: boolean;
    remainingAttempts?: number;
  };
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

      // AUTH-SEC-005: Check if revoked device verification is required
      if (data.requiresRevokedDeviceVerification) {
        // Store pending login credentials for verification flow
        this.storePendingRevokedDeviceLogin(
          credentials.email,
          credentials.password,
          data.deviceFingerprint || ''
        );

        // Return revoked device response - login blocked until verification
        return {
          user: {} as User,
          accessToken: '',
          refreshToken: '',
          requiresRevokedDeviceVerification: true,
          deviceFingerprint: data.deviceFingerprint || '',
        };
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

  // ========================================
  // AUTH-PWD-001: Set Password for OAuth User
  // ========================================

  /**
   * Request password setup email for OAuth-only users
   * Called when user wants to set a password before unlinking their OAuth provider
   */
  async requestPasswordSetup(): Promise<RequestPasswordSetupResponse> {
    try {
      const response = await axios.post<{ data: RequestPasswordSetupResponse }>(
        `${AUTH_API_URL}/password/setup-request`,
        {},
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to request password setup';
        throw new Error(message);
      }
      throw new Error('Failed to request password setup. Please try again.');
    }
  },

  /**
   * Validate password setup token from email link
   * Called when user clicks the link in their email
   */
  async validatePasswordSetupToken(token: string): Promise<ValidatePasswordSetupTokenResponse> {
    try {
      const response = await axios.get<{ data: ValidatePasswordSetupTokenResponse }>(
        `${AUTH_API_URL}/password/setup-validate`,
        {
          params: { token },
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Invalid or expired token';
        throw new Error(message);
      }
      throw new Error('Failed to validate token. Please try again.');
    }
  },

  /**
   * Complete password setup for OAuth user
   * Sets the password using the token from email
   */
  async completePasswordSetup(
    token: string,
    newPassword: string,
    confirmPassword: string
  ): Promise<SetPasswordForOAuthUserResponse> {
    try {
      const response = await axios.post<{ data: SetPasswordForOAuthUserResponse }>(
        `${AUTH_API_URL}/password/setup-complete`,
        {
          token,
          newPassword,
          confirmPassword,
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to set password';
        throw new Error(message);
      }
      throw new Error('Failed to set password. Please try again.');
    }
  },

  // ========================================
  // AUTH-EXT-008: Unlink Active Provider
  // ========================================

  /**
   * Validate if user can unlink their OAuth account
   * Returns info about whether they have password, if it's active provider, etc.
   */
  async validateUnlinkAccount(provider: string): Promise<ValidateUnlinkAccountResponse> {
    try {
      const response = await axios.post<{ data: ValidateUnlinkAccountResponse }>(
        `${EXTERNAL_AUTH_API_URL}/unlink-account/validate`,
        { provider },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to validate unlink request';
        throw new Error(message);
      }
      throw new Error('Failed to validate. Please try again.');
    }
  },

  /**
   * Request verification code for unlinking active provider
   * Sends a 6-digit code to user's email
   */
  async requestUnlinkCode(provider: string): Promise<RequestUnlinkCodeResponse> {
    try {
      const response = await axios.post<{ data: RequestUnlinkCodeResponse }>(
        `${EXTERNAL_AUTH_API_URL}/unlink-account/request-code`,
        { provider },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          },
        }
      );
      return response.data.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Failed to send verification code';
        throw new Error(message);
      }
      throw new Error('Failed to send code. Please try again.');
    }
  },

  /**
   * Unlink active OAuth provider with verification code
   * This will revoke all sessions and force re-login
   */
  async unlinkActiveProvider(
    provider: string,
    verificationCode: string
  ): Promise<UnlinkActiveProviderResponse> {
    try {
      const response = await axios.post<{ data: UnlinkActiveProviderResponse }>(
        `${EXTERNAL_AUTH_API_URL}/unlink-account/confirm`,
        { provider, verificationCode },
        {
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

  // ========================================
  // AUTH-SEC-005: Revoked Device Verification
  // When a user tries to login from a device that was previously revoked,
  // they must verify via email code before proceeding
  // ========================================

  /**
   * Request a verification code for a revoked device login attempt
   * This is called when login returns requiresRevokedDeviceVerification: true
   * @param email - The user's email address
   * @param deviceFingerprint - The device fingerprint from login response
   * @returns Promise with success status and message
   */
  async requestRevokedDeviceCode(
    request: RevokedDeviceCodeRequest
  ): Promise<RevokedDeviceCodeResponse> {
    try {
      const response = await axios.post<{ data: RevokedDeviceCodeResponse }>(
        `${AUTH_API_URL}/revoked-device/request-code`,
        {
          email: request.email,
          deviceFingerprint: request.deviceFingerprint,
        }
      );

      return (
        response.data.data || {
          success: true,
          message: 'Verification code sent to your email',
          expiresInMinutes: 10,
        }
      );
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const errorData = error.response.data;
        // Check if it's a lockout response
        if (errorData?.data?.isLockedOut) {
          return {
            success: false,
            message: errorData.data.message || 'Too many attempts. Please try again later.',
            isLockedOut: true,
            lockoutMinutesRemaining: errorData.data.lockoutMinutesRemaining,
          };
        }
        throw new Error(errorData?.error || 'Failed to request verification code');
      }
      throw new Error('Failed to request verification code. Please try again.');
    }
  },

  /**
   * Verify the code sent to email for revoked device login
   * On success, clears the device from revoked list and returns login tokens
   * @param request - Contains email, deviceFingerprint, verificationCode, and original password
   * @returns Promise with login response on success
   */
  async verifyRevokedDevice(
    request: RevokedDeviceVerifyRequest
  ): Promise<RevokedDeviceVerifyResponse> {
    try {
      const response = await axios.post<{ data: RevokedDeviceVerifyResponse }>(
        `${AUTH_API_URL}/revoked-device/verify`,
        {
          email: request.email,
          deviceFingerprint: request.deviceFingerprint,
          verificationCode: request.verificationCode,
          password: request.password,
        }
      );

      const data = response.data.data;

      if (data.success && data.accessToken && data.refreshToken) {
        // Store tokens on successful verification
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        if (data.userId) {
          localStorage.setItem('userId', data.userId);
        }
      }

      return data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const errorData = error.response.data;

        // Handle lockout
        if (errorData?.data?.isLockedOut) {
          return {
            success: false,
            message:
              errorData.data.message || 'Account temporarily locked. Please try again later.',
            isLockedOut: true,
            lockoutMinutesRemaining: errorData.data.lockoutMinutesRemaining,
          };
        }

        // Handle invalid code with remaining attempts
        if (errorData?.data?.remainingAttempts !== undefined) {
          return {
            success: false,
            message: errorData.data.message || 'Invalid verification code',
            remainingAttempts: errorData.data.remainingAttempts,
          };
        }

        throw new Error(errorData?.error || 'Verification failed');
      }
      throw new Error('Verification failed. Please try again.');
    }
  },

  /**
   * Store pending login credentials temporarily during revoked device verification
   * Uses sessionStorage (cleared on tab close) for security
   */
  storePendingRevokedDeviceLogin(email: string, password: string, deviceFingerprint: string): void {
    sessionStorage.setItem(
      'revoked_device_pending',
      JSON.stringify({
        email,
        password,
        deviceFingerprint,
        timestamp: Date.now(),
      })
    );
  },

  /**
   * Get pending revoked device login credentials
   * Returns null if expired (10 minutes) or not found
   */
  getPendingRevokedDeviceLogin(): {
    email: string;
    password: string;
    deviceFingerprint: string;
  } | null {
    const stored = sessionStorage.getItem('revoked_device_pending');
    if (!stored) return null;

    try {
      const data = JSON.parse(stored);
      // Check if expired (10 minutes = 600000 ms)
      if (Date.now() - data.timestamp > 600000) {
        sessionStorage.removeItem('revoked_device_pending');
        return null;
      }
      return {
        email: data.email,
        password: data.password,
        deviceFingerprint: data.deviceFingerprint,
      };
    } catch {
      sessionStorage.removeItem('revoked_device_pending');
      return null;
    }
  },

  /**
   * Clear pending revoked device login credentials
   * Called after successful verification or on cancel
   */
  clearPendingRevokedDeviceLogin(): void {
    sessionStorage.removeItem('revoked_device_pending');
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

// ========================================
// AUTH-PWD-001: Set Password for OAuth User
// ========================================

export interface RequestPasswordSetupResponse {
  success: boolean;
  message: string;
  expiresAt: string;
}

export interface ValidatePasswordSetupTokenResponse {
  isValid: boolean;
  message: string;
  email?: string;
  provider?: string;
  expiresAt?: string;
}

export interface SetPasswordForOAuthUserResponse {
  success: boolean;
  message: string;
  email?: string;
  canNowUnlinkProvider: boolean;
}

// ========================================
// AUTH-EXT-008: Unlink Active Provider
// ========================================

export interface ValidateUnlinkAccountResponse {
  canUnlink: boolean;
  hasPassword: boolean;
  isActiveProvider: boolean;
  requiresPasswordSetup: boolean;
  requiresEmailVerification: boolean;
  message: string;
}

export interface RequestUnlinkCodeResponse {
  success: boolean;
  message: string;
  maskedEmail: string;
  expiresInMinutes: number;
}

export interface UnlinkActiveProviderResponse {
  success: boolean;
  message: string;
  provider: string;
  sessionsRevoked: number;
  requiresReLogin: boolean;
}
