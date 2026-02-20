// =============================================================================
// Sellers API Client
// =============================================================================
// Service for seller-related operations: conversion, registration, profile

import { apiClient } from '@/lib/api-client';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface ConvertToSellerRequest {
  businessName: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
  acceptTerms: boolean;
}

export interface SellerConversionResult {
  conversionId: string;
  sellerProfileId: string;
  userId: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Reverted';
  source: string;
  pendingVerification: boolean;
  message?: string;
  createdAt: string;
}

export interface SellerProfile {
  id: string;
  userId: string;
  businessName: string;
  displayName: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
  isVerified: boolean;
  averageRating: number;
  totalReviews: number;
  totalListings: number;
  activeSales: number;
  memberSince: string;
  createdAt: string;
  updatedAt?: string;
}

export interface SellerStats {
  totalListings: number;
  activeListings: number;
  totalSales: number;
  averageRating: number;
  totalReviews: number;
  responseRate: number;
  responseTimeMinutes: number;
}

export interface CreateSellerProfileRequest {
  userId: string;
  businessName: string;
  displayName: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
}

export interface UpdateSellerProfileRequest {
  businessName?: string;
  displayName?: string;
  description?: string;
  phone?: string;
  location?: string;
  specialties?: string[];
}

// ─── API Functions ──────────────────────────────────────────────────────────

/**
 * Convert a buyer account to a seller account.
 * Requires authentication. Supports Idempotency-Key header.
 */
export async function convertToSeller(
  data: ConvertToSellerRequest,
  idempotencyKey?: string
): Promise<SellerConversionResult> {
  const headers: Record<string, string> = {};
  if (idempotencyKey) {
    headers['Idempotency-Key'] = idempotencyKey;
  }
  const response = await apiClient.post<SellerConversionResult>(
    '/api/sellers/convert',
    data,
    { headers }
  );
  return response.data;
}

/**
 * Get seller profile by ID.
 */
export async function getSellerProfile(sellerId: string): Promise<SellerProfile> {
  const response = await apiClient.get<SellerProfile>(`/api/sellers/${sellerId}`);
  return response.data;
}

/**
 * Get seller profile by user ID.
 */
export async function getSellerByUserId(userId: string): Promise<SellerProfile> {
  const response = await apiClient.get<SellerProfile>(`/api/sellers/user/${userId}`);
  return response.data;
}

/**
 * Create a new seller profile (direct registration).
 */
export async function createSellerProfile(
  data: CreateSellerProfileRequest
): Promise<SellerProfile> {
  const response = await apiClient.post<SellerProfile>('/api/sellers', data);
  return response.data;
}

/**
 * Update seller profile.
 */
export async function updateSellerProfile(
  sellerId: string,
  data: UpdateSellerProfileRequest
): Promise<SellerProfile> {
  const response = await apiClient.put<SellerProfile>(`/api/sellers/${sellerId}`, data);
  return response.data;
}

/**
 * Get seller statistics.
 */
export async function getSellerStats(sellerId: string): Promise<SellerStats> {
  const response = await apiClient.get<SellerStats>(`/api/sellers/${sellerId}/stats`);
  return response.data;
}

// ─── Dealer Registration API ────────────────────────────────────────────────

export interface RegisterDealerRequest {
  businessName: string;
  tradeName?: string;
  description?: string;
  dealerType: 'Independent' | 'Franchise' | 'MultiLocation' | 'OnlineOnly' | 'Wholesale';
  email: string;
  phone: string;
  whatsApp?: string;
  website?: string;
  address: string;
  city: string;
  state: string;
  zipCode?: string;
  country?: string;
  businessRegistrationNumber?: string;
  taxId?: string;
  dealerLicenseNumber?: string;
  licenseExpiryDate?: string;
  logoUrl?: string;
  bannerUrl?: string;
}

export interface DealerRegistrationResult {
  id: string;
  ownerUserId: string;
  businessName: string;
  verificationStatus: 'Pending' | 'UnderReview' | 'Verified' | 'Rejected' | 'Suspended';
  isActive: boolean;
  createdAt: string;
}

/**
 * Register a new dealer (company). Requires authentication.
 * Dealer starts with Pending status until admin approval.
 */
export async function registerDealer(
  data: RegisterDealerRequest
): Promise<DealerRegistrationResult> {
  const response = await apiClient.post<DealerRegistrationResult>('/api/dealers', data);
  return response.data;
}

/**
 * Get the dealer profile for the currently authenticated user.
 */
export async function getMyDealer(): Promise<DealerRegistrationResult | null> {
  try {
    const response = await apiClient.get<DealerRegistrationResult>('/api/dealers/me');
    return response.data;
  } catch {
    return null;
  }
}
