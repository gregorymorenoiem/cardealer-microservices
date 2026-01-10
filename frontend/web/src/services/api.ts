import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import { captureError, addBreadcrumb } from '../lib/sentry';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Create axios instance
export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token and breadcrumb
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Add breadcrumb for API request
    addBreadcrumb(
      'http',
      `${config.method?.toUpperCase()} ${config.url}`,
      {
        method: config.method,
        url: config.url,
      },
      'info'
    );

    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle token refresh and errors
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Capture API errors in Sentry (skip 401 as they're handled)
    if (error.response?.status !== 401) {
      captureError(error as Error, {
        tags: {
          source: 'api',
          endpoint: originalRequest?.url || 'unknown',
          method: originalRequest?.method || 'unknown',
        },
        extra: {
          status: error.response?.status,
          statusText: error.response?.statusText,
          responseData: error.response?.data,
        },
        level: error.response?.status && error.response.status >= 500 ? 'error' : 'warning',
      });
    }

    // If 401 and not already retried, try to refresh token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        // Call refresh token endpoint
        const response = await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {
          refreshToken,
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data.data;

        // Save new tokens
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        addBreadcrumb('auth', 'Token refreshed successfully', undefined, 'info');

        // Retry original request with new token
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        return api(originalRequest);
      } catch (refreshError) {
        // Refresh failed - logout user
        addBreadcrumb('auth', 'Token refresh failed, logging out', undefined, 'warning');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
