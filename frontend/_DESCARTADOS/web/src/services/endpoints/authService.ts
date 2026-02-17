import api from '../api';
import type { 
  LoginRequest, 
  LoginResponse, 
  RegisterRequest, 
  ApiResponse 
} from '../../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post('/api/auth/login', credentials);
    return response.data;
  },

  async register(data: RegisterRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await api.post('/api/auth/register', data);
    return response.data;
  },

  async logout(): Promise<ApiResponse<void>> {
    const refreshToken = localStorage.getItem('refreshToken');
    const response = await api.post('/api/auth/logout', { refreshToken });
    return response.data;
  },

  async refreshToken(refreshToken: string): Promise<ApiResponse<{ accessToken: string; refreshToken: string }>> {
    const response = await api.post('/api/auth/refresh-token', { refreshToken });
    return response.data;
  },

  async forgotPassword(email: string): Promise<ApiResponse<void>> {
    const response = await api.post('/api/auth/forgot-password', { email });
    return response.data;
  },

  async resetPassword(token: string, newPassword: string): Promise<ApiResponse<void>> {
    const response = await api.post('/api/auth/reset-password', {
      token,
      newPassword,
      confirmPassword: newPassword,
    });
    return response.data;
  },

  async verifyEmail(token: string): Promise<ApiResponse<void>> {
    const response = await api.post('/api/auth/verify-email', { token });
    return response.data;
  },
};
