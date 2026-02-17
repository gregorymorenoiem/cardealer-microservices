/**
 * Reviews Service - API client for reviews and ratings
 * Connects via API Gateway to ReviewService
 */

import { apiClient } from '@/lib/api-client';
import type { PaginatedResponse } from '@/types';

// ============================================================
// API TYPES
// ============================================================

export interface Review {
  id: string;
  reviewerId: string;
  reviewerName: string;
  reviewerAvatarUrl?: string;

  // Target (either dealer or vehicle)
  targetType: 'dealer' | 'vehicle' | 'seller';
  targetId: string;
  targetName?: string;

  // Rating
  overallRating: number; // 1-5
  communicationRating?: number;
  responseTimeRating?: number;
  accuracyRating?: number;
  valueRating?: number;

  // Content
  title?: string;
  content: string;
  pros?: string[];
  cons?: string[];

  // Media
  images?: ReviewImage[];

  // Status
  status: ReviewStatus;
  isVerifiedPurchase?: boolean;

  // Response
  response?: ReviewResponse;

  // Helpful votes
  helpfulCount: number;
  notHelpfulCount: number;
  hasVoted?: boolean;
  userVote?: 'helpful' | 'notHelpful';

  // Timestamps
  createdAt: string;
  updatedAt?: string;
}

export interface ReviewImage {
  id: string;
  url: string;
  thumbnailUrl?: string;
  caption?: string;
}

export interface ReviewResponse {
  content: string;
  respondedAt: string;
  respondedBy: string;
}

export type ReviewStatus = 'pending' | 'approved' | 'rejected' | 'flagged';

export interface CreateReviewRequest {
  targetType: 'dealer' | 'vehicle' | 'seller';
  targetId: string;
  overallRating: number;
  communicationRating?: number;
  responseTimeRating?: number;
  accuracyRating?: number;
  valueRating?: number;
  title?: string;
  content: string;
  pros?: string[];
  cons?: string[];
  imageUrls?: string[];
}

export interface UpdateReviewRequest {
  overallRating?: number;
  title?: string;
  content?: string;
  pros?: string[];
  cons?: string[];
}

export interface ReviewResponseRequest {
  content: string;
}

export interface ReviewSearchParams {
  targetType?: 'dealer' | 'vehicle' | 'seller';
  targetId?: string;
  rating?: number;
  status?: ReviewStatus;
  sortBy?: 'newest' | 'oldest' | 'highest' | 'lowest' | 'helpful';
  page?: number;
  pageSize?: number;
}

export interface ReviewStats {
  averageRating: number;
  totalReviews: number;
  ratingDistribution: {
    1: number;
    2: number;
    3: number;
    4: number;
    5: number;
  };
  averageCommunication?: number;
  averageResponseTime?: number;
  averageAccuracy?: number;
  averageValue?: number;
  verifiedPurchaseCount: number;
}

export interface ReviewSummary {
  targetId: string;
  targetType: 'dealer' | 'vehicle' | 'seller';
  averageRating: number;
  totalReviews: number;
  recentReviews: Review[];
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get reviews with pagination and filters
 */
export async function getReviews(
  params: ReviewSearchParams = {}
): Promise<PaginatedResponse<Review>> {
  const response = await apiClient.get<{
    items: Review[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>('/api/reviews', { params });

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Get reviews for a specific target (dealer/vehicle/seller)
 */
export async function getReviewsForTarget(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string,
  params: Omit<ReviewSearchParams, 'targetType' | 'targetId'> = {}
): Promise<PaginatedResponse<Review>> {
  return getReviews({ ...params, targetType, targetId });
}

/**
 * Get review by ID
 */
export async function getReviewById(id: string): Promise<Review> {
  const response = await apiClient.get<Review>(`/api/reviews/${id}`);
  return response.data;
}

/**
 * Get review statistics for a target
 */
export async function getReviewStats(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string
): Promise<ReviewStats> {
  const response = await apiClient.get<ReviewStats>(`/api/reviews/stats/${targetType}/${targetId}`);
  return response.data;
}

/**
 * Get review summary (stats + recent reviews)
 */
export async function getReviewSummary(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string
): Promise<ReviewSummary> {
  const response = await apiClient.get<ReviewSummary>(
    `/api/reviews/summary/${targetType}/${targetId}`
  );
  return response.data;
}

/**
 * Create a new review
 */
export async function createReview(data: CreateReviewRequest): Promise<Review> {
  const response = await apiClient.post<Review>('/api/reviews', data);
  return response.data;
}

/**
 * Update an existing review (only by review author)
 */
export async function updateReview(id: string, data: UpdateReviewRequest): Promise<Review> {
  const response = await apiClient.put<Review>(`/api/reviews/${id}`, data);
  return response.data;
}

/**
 * Delete a review (only by review author or admin)
 */
export async function deleteReview(id: string): Promise<void> {
  await apiClient.delete(`/api/reviews/${id}`);
}

/**
 * Respond to a review (only by target owner - dealer/seller)
 */
export async function respondToReview(
  reviewId: string,
  data: ReviewResponseRequest
): Promise<Review> {
  const response = await apiClient.post<Review>(`/api/reviews/${reviewId}/respond`, data);
  return response.data;
}

/**
 * Vote on a review (helpful/not helpful)
 */
export async function voteReview(
  reviewId: string,
  vote: 'helpful' | 'notHelpful'
): Promise<Review> {
  const response = await apiClient.post<Review>(`/api/reviews/${reviewId}/vote`, { vote });
  return response.data;
}

/**
 * Remove vote from a review
 */
export async function removeVote(reviewId: string): Promise<Review> {
  const response = await apiClient.delete<Review>(`/api/reviews/${reviewId}/vote`);
  return response.data;
}

/**
 * Report a review (flag for moderation)
 */
export async function reportReview(reviewId: string, reason: string): Promise<void> {
  await apiClient.post(`/api/reviews/${reviewId}/report`, { reason });
}

/**
 * Get user's reviews
 */
export async function getUserReviews(
  userId?: string,
  params: { page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<Review>> {
  const endpoint = userId ? `/api/reviews/user/${userId}` : '/api/reviews/me';
  const response = await apiClient.get<{
    items: Review[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>(endpoint, { params });

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Check if user can review a target (has transaction with them)
 */
export async function canReview(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string
): Promise<{ canReview: boolean; reason?: string }> {
  const response = await apiClient.get<{ canReview: boolean; reason?: string }>(
    `/api/reviews/can-review/${targetType}/${targetId}`
  );
  return response.data;
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

/**
 * Format rating as stars text
 */
export function formatRatingStars(rating: number): string {
  const fullStars = Math.floor(rating);
  const hasHalf = rating % 1 >= 0.5;
  return (
    '★'.repeat(fullStars) + (hasHalf ? '½' : '') + '☆'.repeat(5 - fullStars - (hasHalf ? 1 : 0))
  );
}

/**
 * Get rating label
 */
export function getRatingLabel(rating: number): string {
  if (rating >= 4.5) return 'Excelente';
  if (rating >= 4) return 'Muy Bueno';
  if (rating >= 3.5) return 'Bueno';
  if (rating >= 3) return 'Regular';
  if (rating >= 2) return 'Bajo';
  return 'Muy Bajo';
}

/**
 * Get rating color class
 */
export function getRatingColor(rating: number): string {
  if (rating >= 4) return 'text-emerald-600';
  if (rating >= 3) return 'text-amber-500';
  return 'text-red-500';
}

/**
 * Get rating background color class
 */
export function getRatingBgColor(rating: number): string {
  if (rating >= 4) return 'bg-emerald-100';
  if (rating >= 3) return 'bg-amber-100';
  return 'bg-red-100';
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const reviewService = {
  getReviews,
  getReviewsForTarget,
  getReviewById,
  getReviewStats,
  getReviewSummary,
  createReview,
  updateReview,
  deleteReview,
  respondToReview,
  voteReview,
  removeVote,
  reportReview,
  getUserReviews,
  canReview,
  formatRatingStars,
  getRatingLabel,
  getRatingColor,
  getRatingBgColor,
};

export default reviewService;
