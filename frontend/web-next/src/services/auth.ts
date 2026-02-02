/**
 * Authentication Service - API client for auth operations
 * Connects via API Gateway to AuthService
 */

import { apiClient, authTokens } from '@/lib/api-client';
import type { User } from '@/types';

// ============================================================
// API TYPES
// ============================================================

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  password: string;
  acceptTerms: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  phone?: string;
  accountType: 'individual' | 'dealer' | 'admin';
  isVerified: boolean;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  preferredLocale: string;
  preferredCurrency: 'DOP' | 'USD';
  createdAt: string;
  lastLoginAt?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  password: string;
  confirmPassword: string;
}

export interface VerifyEmailRequest {
  token: string;
}

export interface Session {
  id: string;
  deviceName: string;
  deviceType: 'desktop' | 'mobile' | 'tablet' | 'unknown';
  browser: string;
  os: string;
  ipAddress: string;
  location?: string;
  lastActiveAt: string;
  createdAt: string;
  isCurrent: boolean;
}

export interface TwoFactorSetupResponse {
  qrCodeUrl: string;
  secret: string;
  backupCodes: string[];
}

export interface TwoFactorStatus {
  isEnabled: boolean;
  enabledAt?: string;
  backupCodesRemaining: number;
}

// ============================================================
// TRANSFORM FUNCTIONS
// ============================================================

export const transformUser = (dto: UserDto): User => ({
  id: dto.id,
  email: dto.email,
  firstName: dto.firstName,
  lastName: dto.lastName,
  fullName: dto.fullName,
  avatarUrl: dto.avatarUrl,
  phone: dto.phone,
  accountType: dto.accountType,
  isVerified: dto.isVerified,
  isEmailVerified: dto.isEmailVerified,
  isPhoneVerified: dto.isPhoneVerified,
  preferredLocale: dto.preferredLocale,
  preferredCurrency: dto.preferredCurrency,
  createdAt: dto.createdAt,
  lastLoginAt: dto.lastLoginAt,
});

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Login with email and password
 */
export async function login(data: LoginRequest): Promise<{ user: User }> {
  const response = await apiClient.post<AuthResponse>('/api/auth/login', data);

  // Store tokens
  authTokens.setTokens(response.data.accessToken, response.data.refreshToken);

  return {
    user: transformUser(response.data.user),
  };
}

/**
 * Register a new user
 */
export async function register(data: RegisterRequest): Promise<{ user: User }> {
  const response = await apiClient.post<AuthResponse>('/api/auth/register', data);

  // Store tokens
  authTokens.setTokens(response.data.accessToken, response.data.refreshToken);

  return {
    user: transformUser(response.data.user),
  };
}

/**
 * Logout current user
 */
export async function logout(): Promise<void> {
  try {
    await apiClient.post('/api/auth/logout');
  } finally {
    // Always clear tokens, even if API call fails
    authTokens.clearTokens();
  }
}

/**
 * Get current authenticated user
 */
export async function getCurrentUser(): Promise<User | null> {
  try {
    const response = await apiClient.get<UserDto>('/api/auth/me');
    return transformUser(response.data);
  } catch {
    return null;
  }
}

/**
 * Request password reset email
 */
export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post('/api/auth/forgot-password', { email });
}

/**
 * Reset password with token
 */
export async function resetPassword(data: { token: string; password: string }): Promise<void> {
  await apiClient.post('/api/auth/reset-password', data);
}

/**
 * Verify email with token
 */
export async function verifyEmail(token: string): Promise<void> {
  await apiClient.post('/api/auth/verify-email', { token });
}

/**
 * Resend verification email
 */
export async function resendVerification(email: string): Promise<void> {
  await apiClient.post('/api/auth/resend-verification', { email });
}

/**
 * Set password for OAuth users
 */
export async function setPassword(password: string): Promise<void> {
  await apiClient.post('/api/auth/set-password', { password });
}

/**
 * Change password (for authenticated users)
 */
export async function changePassword(currentPassword: string, newPassword: string): Promise<void> {
  await apiClient.post('/api/auth/change-password', { currentPassword, newPassword });
}

/**
 * Login with OAuth provider
 */
export async function loginWithProvider(provider: 'google' | 'apple'): Promise<void> {
  // Redirect to OAuth provider
  window.location.href = `${process.env.NEXT_PUBLIC_API_URL}/api/auth/oauth/${provider}`;
}

// ============================================================
// SESSION MANAGEMENT
// ============================================================

/**
 * Get all active sessions for the current user
 */
export async function getSessions(): Promise<Session[]> {
  const response = await apiClient.get<Session[]>('/api/auth/sessions');
  return response.data;
}

/**
 * Revoke a specific session
 */
export async function revokeSession(sessionId: string): Promise<void> {
  await apiClient.delete(`/api/auth/sessions/${sessionId}`);
}

/**
 * Revoke all sessions except current
 */
export async function revokeAllSessions(): Promise<void> {
  await apiClient.delete('/api/auth/sessions');
}

// ============================================================
// TWO-FACTOR AUTHENTICATION
// ============================================================

/**
 * Get 2FA status
 */
export async function get2FAStatus(): Promise<TwoFactorStatus> {
  const response = await apiClient.get<TwoFactorStatus>('/api/auth/2fa/status');
  return response.data;
}

/**
 * Initialize 2FA setup (get QR code)
 */
export async function setup2FA(): Promise<TwoFactorSetupResponse> {
  const response = await apiClient.post<TwoFactorSetupResponse>('/api/auth/2fa/setup');
  return response.data;
}

/**
 * Verify and enable 2FA
 */
export async function enable2FA(code: string): Promise<void> {
  await apiClient.post('/api/auth/2fa/enable', { code });
}

/**
 * Disable 2FA
 */
export async function disable2FA(code: string): Promise<void> {
  await apiClient.post('/api/auth/2fa/disable', { code });
}

/**
 * Regenerate backup codes
 */
export async function regenerateBackupCodes(code: string): Promise<string[]> {
  const response = await apiClient.post<{ backupCodes: string[] }>('/api/auth/2fa/backup-codes', {
    code,
  });
  return response.data.backupCodes;
}

// ============================================================
// ACCOUNT MANAGEMENT
// ============================================================

/**
 * Delete user account
 */
export async function deleteAccount(password: string): Promise<void> {
  await apiClient.delete('/api/auth/account', { data: { password } });
  authTokens.clearTokens();
}

/**
 * Auth service object
 */
export const authService = {
  login,
  register,
  logout,
  getCurrentUser,
  forgotPassword,
  resetPassword,
  verifyEmail,
  resendVerification,
  setPassword,
  changePassword,
  loginWithProvider,
  // Session management
  getSessions,
  revokeSession,
  revokeAllSessions,
  // 2FA
  get2FAStatus,
  setup2FA,
  enable2FA,
  disable2FA,
  regenerateBackupCodes,
  // Account
  deleteAccount,
};

export default authService;
