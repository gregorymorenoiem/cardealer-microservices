/**
 * User Service - API client for user operations
 * Connects via API Gateway to UserService
 */

import { apiClient } from '@/lib/api-client';
import type { User } from '@/types';

// ============================================================
// API TYPES
// ============================================================

export interface UserProfileDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  phone?: string;
  bio?: string;
  location?: string;
  city?: string;
  province?: string;
  accountType: 'individual' | 'dealer' | 'admin';
  isVerified: boolean;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  vehicleCount: number;
  reviewCount: number;
  averageRating: number;
  responseRate: number;
  memberSince: string;
  lastActive?: string;
  badges: UserBadge[];
  preferredLocale: string;
  preferredCurrency: 'DOP' | 'USD';
}

export interface UserBadge {
  id: string;
  name: string;
  icon: string;
  description: string;
  earnedAt: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  phone?: string;
  bio?: string;
  location?: string;
  city?: string;
  province?: string;
  preferredLocale?: string;
  preferredCurrency?: 'DOP' | 'USD';
}

export interface UserStats {
  vehiclesPublished: number;
  vehiclesSold: number;
  totalViews: number;
  totalInquiries: number;
  responseRate: number;
  averageResponseTime: string;
  reviewCount: number;
  averageRating: number;
}

export interface UserVehicleDto {
  id: string;
  slug: string;
  title: string;
  price: number;
  currency: 'DOP' | 'USD';
  year: number;
  mileage: number;
  imageUrl: string;
  status: 'active' | 'pending' | 'paused' | 'sold' | 'expired' | 'rejected';
  isFeatured: boolean;
  viewCount: number;
  inquiryCount: number;
  createdAt: string;
  expiresAt: string;
}

// ============================================================
// TRANSFORM FUNCTIONS
// ============================================================

export const transformUserProfile = (dto: UserProfileDto): User => ({
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
  createdAt: dto.memberSince,
});

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get user profile by ID (public)
 */
export async function getUserById(id: string): Promise<UserProfileDto | null> {
  try {
    const response = await apiClient.get<UserProfileDto>(`/api/users/${id}`);
    return response.data;
  } catch {
    return null;
  }
}

/**
 * Get current user profile (authenticated)
 */
export async function getCurrentProfile(): Promise<UserProfileDto> {
  const response = await apiClient.get<UserProfileDto>('/api/users/me');
  return response.data;
}

/**
 * Update current user profile
 */
export async function updateProfile(data: UpdateProfileRequest): Promise<UserProfileDto> {
  const response = await apiClient.put<UserProfileDto>('/api/users/me', data);
  return response.data;
}

/**
 * Upload avatar image
 */
export async function uploadAvatar(file: File): Promise<{ avatarUrl: string }> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.post<{ avatarUrl: string }>('/api/users/me/avatar', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });

  return response.data;
}

/**
 * Delete avatar
 */
export async function deleteAvatar(): Promise<void> {
  await apiClient.delete('/api/users/me/avatar');
}

/**
 * Get user stats (for dashboard)
 */
export async function getUserStats(): Promise<UserStats> {
  const response = await apiClient.get<UserStats>('/api/users/me/stats');
  return response.data;
}

/**
 * Get user's vehicles
 */
export async function getUserVehicles(params?: {
  status?: 'active' | 'pending' | 'sold' | 'expired' | 'all';
  page?: number;
  limit?: number;
}): Promise<{
  vehicles: UserVehicleDto[];
  total: number;
  page: number;
  totalPages: number;
}> {
  const response = await apiClient.get<{
    vehicles: UserVehicleDto[];
    total: number;
    page: number;
    totalPages: number;
  }>('/api/users/me/vehicles', { params });

  return response.data;
}

/**
 * Get another user's public vehicles
 */
export async function getPublicUserVehicles(
  userId: string,
  params?: {
    page?: number;
    limit?: number;
  }
): Promise<{
  vehicles: UserVehicleDto[];
  total: number;
  page: number;
  totalPages: number;
}> {
  const response = await apiClient.get<{
    vehicles: UserVehicleDto[];
    total: number;
    page: number;
    totalPages: number;
  }>(`/api/users/${userId}/vehicles`, { params });

  return response.data;
}

/**
 * Delete user account
 */
export async function deleteAccount(password: string): Promise<void> {
  await apiClient.delete('/api/users/me', { data: { password } });
}

/**
 * User service object
 */
export const userService = {
  getUserById,
  getCurrentProfile,
  updateProfile,
  uploadAvatar,
  deleteAvatar,
  getUserStats,
  getUserVehicles,
  getPublicUserVehicles,
  deleteAccount,
};

export default userService;
