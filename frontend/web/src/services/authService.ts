import axios from 'axios';
import type { User } from '@/types';
import { getMockUserByEmail } from '@/mocks/mockUsers';

const AUTH_API_URL = import.meta.env.VITE_AUTH_SERVICE_URL || 'http://localhost:5001/api';
const USE_MOCK_AUTH = import.meta.env.VITE_USE_MOCK_AUTH === 'true' || true; // Enable mock by default

interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
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

/**
 * Authentication service - handles login, register, logout, and token management
 */
export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    // Mock authentication for testing
    if (USE_MOCK_AUTH) {
      const mockUser = getMockUserByEmail(credentials.email);
      
      if (mockUser) {
        // Validate password (all dealers use "password123", admin uses "admin123")
        const validPassword = 
          (mockUser.accountType === 'admin' && credentials.password === 'admin123') ||
          (mockUser.accountType !== 'admin' && credentials.password === 'password123');

        if (!validPassword) {
          throw new Error('Invalid email or password');
        }

        const mockResponse: LoginResponse = {
          user: mockUser,
          accessToken: 'mock-access-token-' + Date.now(),
          refreshToken: 'mock-refresh-token-' + Date.now(),
        };

        // Store tokens
        localStorage.setItem('accessToken', mockResponse.accessToken);
        localStorage.setItem('refreshToken', mockResponse.refreshToken);

        if (credentials.rememberMe) {
          localStorage.setItem('rememberMe', 'true');
        }

        return mockResponse;
      }

      throw new Error('User not found');
    }

    // Real API authentication
    try {
      const response = await axios.post(`${AUTH_API_URL}/auth/login`, {
        email: credentials.email,
        password: credentials.password,
      });

      const { user: _loginUser, accessToken, refreshToken } = response.data;

      // Store tokens
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);

      if (credentials.rememberMe) {
        localStorage.setItem('rememberMe', 'true');
      }

      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.message || 'Invalid email or password';
        throw new Error(message);
      }
      throw new Error('Login failed. Please try again.');
    }
  },

  async register(data: RegisterData): Promise<LoginResponse> {
    try {
      const response = await axios.post(`${AUTH_API_URL}/auth/register`, {
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone,
      });

      const { user: _registerUser, accessToken, refreshToken } = response.data;

      // Store tokens
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);

      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.message || 'Registration failed';
        throw new Error(message);
      }
      throw new Error('Registration failed. Please try again.');
    }
  },

  async logout(): Promise<void> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      
      if (refreshToken) {
        await axios.post(`${AUTH_API_URL}/auth/logout`, { refreshToken });
      }
    } catch (error) {
      console.error('Error during logout:', error);
    } finally {
      // Clear tokens regardless of API call success
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('rememberMe');
    }
  },

  async refreshToken(): Promise<RefreshTokenResponse> {
    try {
      const refreshToken = localStorage.getItem('refreshToken');

      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await axios.post(`${AUTH_API_URL}/auth/refresh`, { refreshToken });

      const { accessToken, refreshToken: newRefreshToken } = response.data;

      // Update tokens
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', newRefreshToken);

      return response.data;
    } catch (error) {
      // If refresh fails, clear tokens and force re-login
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
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
      await axios.post(`${AUTH_API_URL}/auth/forgot-password`, { email });
    } catch (error) {
      console.error('Error sending password reset email:', error);
      throw new Error('Failed to send password reset email');
    }
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/auth/reset-password`, {
        token,
        newPassword,
      });
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.message || 'Failed to reset password';
        throw new Error(message);
      }
      throw new Error('Failed to reset password');
    }
  },

  async verifyEmail(token: string): Promise<void> {
    try {
      await axios.post(`${AUTH_API_URL}/auth/verify-email`, { token });
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
};
