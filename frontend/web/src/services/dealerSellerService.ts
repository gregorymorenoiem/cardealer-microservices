/**
 * Dealer & Seller API Service
 *
 * Connects to UserService endpoints for managing dealers and sellers
 */

import axios, { type AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';
import type {
  Dealer,
  CreateDealerRequest,
  UpdateDealerRequest,
  VerifyDealerRequest,
  DealerModuleSubscription,
  SellerProfile,
  CreateSellerRequest,
  UpdateSellerRequest,
  VerifySellerRequest,
  SellerStats,
} from '@/types/dealer';

// API Base URL - Use Gateway
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add refresh token interceptor for automatic token refresh on 401
addRefreshTokenInterceptor(apiClient);

// ============================================================================
// DEALER API
// ============================================================================

export const dealerApi = {
  /**
   * Create a new dealer
   */
  create: async (data: CreateDealerRequest): Promise<Dealer> => {
    const response = await apiClient.post<Dealer>('/api/dealers', data);
    return response.data;
  },

  /**
   * Get dealer by ID
   */
  getById: async (dealerId: string): Promise<Dealer> => {
    const response = await apiClient.get<Dealer>(`/api/dealers/${dealerId}`);
    return response.data;
  },

  /**
   * Get dealer by owner user ID
   * Handles AuthService ID -> UserService ID mapping via email lookup
   */
  getByOwner: async (ownerUserId: string): Promise<Dealer> => {
    try {
      // First try direct lookup (in case it's already the UserService ID)
      const response = await apiClient.get<Dealer>(`/api/dealers/owner/${ownerUserId}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        // If not found, it might be an AuthService ID
        // Get user email from localStorage (set during login)
        const userEmail = localStorage.getItem('userEmail');

        if (!userEmail) {
          throw new Error('User email not found. Please login again.');
        }

        // Find the UserService user by email
        const usersResponse = await apiClient.get<{
          users: Array<{ id: string; email: string }>;
        }>(`/api/users?email=${encodeURIComponent(userEmail)}`);

        const userServiceUser = usersResponse.data.users.find(
          (u) => u.email.toLowerCase() === userEmail.toLowerCase()
        );

        if (!userServiceUser) {
          throw new Error('User not found in UserService');
        }

        // Now try with the UserService ID
        const dealerResponse = await apiClient.get<Dealer>(
          `/api/dealers/owner/${userServiceUser.id}`
        );
        return dealerResponse.data;
      }
      throw error;
    }
  },

  /**
   * Update dealer
   */
  update: async (dealerId: string, data: UpdateDealerRequest): Promise<Dealer> => {
    const response = await apiClient.put<Dealer>(`/api/dealers/${dealerId}`, data);
    return response.data;
  },

  /**
   * Verify or reject dealer (Admin only)
   */
  verify: async (dealerId: string, data: VerifyDealerRequest): Promise<Dealer> => {
    const response = await apiClient.post<Dealer>(`/api/dealers/${dealerId}/verify`, data);
    return response.data;
  },

  /**
   * Get dealer subscription
   */
  getSubscription: async (dealerId: string) => {
    const response = await apiClient.get(`/api/dealers/${dealerId}/subscription`);
    return response.data;
  },

  /**
   * Get active modules
   */
  getActiveModules: async (dealerId: string): Promise<DealerModuleSubscription[]> => {
    const response = await apiClient.get<string[]>(`/api/dealers/${dealerId}/active-modules`);
    // Backend returns string[], convert to DealerModuleSubscription[]
    return response.data.map((moduleName) => ({
      moduleId: moduleName,
      moduleName: moduleName,
      isActive: true,
    }));
  },

  /**
   * Subscribe to module
   */
  subscribeToModule: async (dealerId: string, moduleCode: string) => {
    const response = await apiClient.post(
      `/api/dealers/${dealerId}/modules/${moduleCode}/subscribe`
    );
    return response.data;
  },

  /**
   * Unsubscribe from module
   */
  unsubscribeFromModule: async (dealerId: string, moduleCode: string) => {
    const response = await apiClient.post(
      `/api/dealers/${dealerId}/modules/${moduleCode}/unsubscribe`
    );
    return response.data;
  },

  /**
   * Register new dealer (onboarding flow)
   */
  register: async (data: {
    dealerId?: string;
    email: string;
    businessName: string;
    phone: string;
  }) => {
    const response = await apiClient.post('/api/dealers/register', data);
    return response.data;
  },
};

// ============================================================================
// SELLER API
// ============================================================================

export const sellerApi = {
  /**
   * Create a new seller profile
   */
  create: async (data: CreateSellerRequest): Promise<SellerProfile> => {
    const response = await apiClient.post<SellerProfile>('/api/sellers', data);
    return response.data;
  },

  /**
   * Get seller by ID
   */
  getById: async (sellerId: string): Promise<SellerProfile> => {
    const response = await apiClient.get<SellerProfile>(`/api/sellers/${sellerId}`);
    return response.data;
  },

  /**
   * Get seller by user ID
   * Handles AuthService ID -> UserService ID mapping via email lookup
   */
  getByUser: async (userId: string): Promise<SellerProfile> => {
    try {
      // First try direct lookup (in case it's already the UserService ID)
      const response = await apiClient.get<SellerProfile>(`/api/sellers/user/${userId}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        // If not found, it might be an AuthService ID
        // Get user email from localStorage (set during login)
        const userEmail = localStorage.getItem('userEmail');

        if (!userEmail) {
          throw new Error('User email not found. Please login again.');
        }

        // Find the UserService user by email
        const usersResponse = await apiClient.get<{
          users: Array<{ id: string; email: string }>;
        }>(`/api/users?email=${encodeURIComponent(userEmail)}`);

        const userServiceUser = usersResponse.data.users.find(
          (u) => u.email.toLowerCase() === userEmail.toLowerCase()
        );

        if (!userServiceUser) {
          throw new Error('User not found in UserService');
        }

        // Now try with the UserService ID
        const sellerResponse = await apiClient.get<SellerProfile>(
          `/api/sellers/user/${userServiceUser.id}`
        );
        return sellerResponse.data;
      }
      throw error;
    }
  },

  /**
   * Update seller profile
   */
  update: async (sellerId: string, data: UpdateSellerRequest): Promise<SellerProfile> => {
    const response = await apiClient.put<SellerProfile>(`/api/sellers/${sellerId}`, data);
    return response.data;
  },

  /**
   * Get seller stats
   */
  getStats: async (sellerId: string): Promise<SellerStats> => {
    const response = await apiClient.get<SellerStats>(`/api/sellers/${sellerId}/stats`);
    return response.data;
  },

  /**
   * Verify or reject seller (Admin only)
   */
  verify: async (sellerId: string, data: VerifySellerRequest): Promise<{ message: string }> => {
    const response = await apiClient.post<{ message: string }>(
      `/api/sellers/${sellerId}/verify`,
      data
    );
    return response.data;
  },
};

export default {
  dealer: dealerApi,
  seller: sellerApi,
};
