/**
 * Reviews Service - API client for reviews and ratings
 * Connects via API Gateway to ReviewService
 *
 * AUDIT: Complete rewrite to match backend ReviewService endpoints.
 * Backend routes: /api/reviews/seller/{sellerId}, /api/reviews/{id}, etc.
 *
 * KEY FIELD MAPPINGS (backend ReviewDto → frontend Review):
 *   buyerId        → reviewerId
 *   buyerName      → reviewerName
 *   buyerPhotoUrl  → reviewerAvatarUrl
 *   rating         → overallRating
 *   sellerId       → targetId (targetType = 'seller'|'dealer')
 *   helpfulVotes   → helpfulCount
 *   voteStats      → used for helpfulCount / notHelpfulCount
 *   userVotedHelpful → hasVoted / userVote
 */

import { apiClient } from '@/lib/api-client';
import type { PaginatedResponse } from '@/types';

// ============================================================
// BACKEND DTO TYPES (match backend JSON serialization)
// ============================================================

interface BackendReviewResponseDto {
  id: string;
  reviewId: string;
  sellerId: string;
  content: string;
  sellerName: string;
  createdAt: string;
  isApproved: boolean;
}

interface BackendVoteStatsDto {
  reviewId: string;
  helpfulVotes: number;
  totalVotes: number;
  helpfulPercentage: number;
  currentUserVotedHelpful?: boolean | null;
}

/** Raw DTO from backend ReviewService (camelCase JSON) */
interface BackendReviewDto {
  id: string;
  buyerId: string;
  sellerId: string;
  vehicleId?: string | null;
  orderId?: string | null;
  rating: number;
  title: string;
  content: string;
  isVerifiedPurchase: boolean;
  buyerName: string;
  buyerPhotoUrl?: string | null;
  createdAt: string;
  // Sprint 15 fields
  sellerResponse?: string | null;
  sellerRespondedAt?: string | null;
  voteStats?: BackendVoteStatsDto | null;
  userVotedHelpful?: boolean | null;
  trustScore: number;
  wasAutoRequested: boolean;
  response?: BackendReviewResponseDto | null;
}

interface BackendPagedReviewsDto {
  reviews: BackendReviewDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface BackendReviewSummaryDto {
  sellerId: string;
  totalReviews: number;
  averageRating: number;
  ratingDistribution: Record<string, number>;
  positivePercentage: number;
  verifiedPurchaseReviews: number;
  lastReviewDate?: string | null;
}

// ============================================================
// FRONTEND TYPES (used by components - preserved for compatibility)
// ============================================================

export interface Review {
  id: string;
  reviewerId: string; // ← buyerId
  reviewerName: string; // ← buyerName
  reviewerAvatarUrl?: string; // ← buyerPhotoUrl

  // Target (seller or dealer)
  targetType: 'dealer' | 'vehicle' | 'seller';
  targetId: string; // ← sellerId
  targetName?: string;

  // Rating
  overallRating: number; // ← rating (1-5)

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

  // Seller/Dealer response
  response?: ReviewResponse;

  // Helpful votes
  helpfulCount: number; // ← voteStats.helpfulVotes or 0
  notHelpfulCount: number; // ← voteStats.totalVotes - helpfulVotes or 0
  hasVoted?: boolean; // ← userVotedHelpful !== null
  userVote?: 'helpful' | 'notHelpful'; // ← userVotedHelpful

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
  targetId: string; // sellerId
  overallRating: number;
  title?: string;
  content: string;
  pros?: string[];
  cons?: string[];
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

/** Maps a raw backend ReviewDto to the frontend Review interface */
function mapBackendReview(dto: BackendReviewDto): Review {
  const helpfulVotes = dto.voteStats?.helpfulVotes ?? 0;
  const totalVotes = dto.voteStats?.totalVotes ?? 0;
  const notHelpfulVotes = totalVotes - helpfulVotes;

  // Resolve seller response: prefer `response` object (Sprint 15), fall back to legacy string
  let reviewResponse: ReviewResponse | undefined;
  if (dto.response?.content) {
    reviewResponse = {
      content: dto.response.content,
      respondedAt: dto.response.createdAt,
      respondedBy: dto.response.sellerName || 'Vendedor',
    };
  } else if (dto.sellerResponse) {
    reviewResponse = {
      content: dto.sellerResponse,
      respondedAt: dto.sellerRespondedAt ?? dto.createdAt,
      respondedBy: 'Vendedor',
    };
  }

  // Determine hasVoted and userVote
  const votedHelpful = dto.voteStats?.currentUserVotedHelpful ?? dto.userVotedHelpful;
  const hasVoted = votedHelpful !== null && votedHelpful !== undefined;
  const userVote: 'helpful' | 'notHelpful' | undefined = hasVoted
    ? votedHelpful
      ? 'helpful'
      : 'notHelpful'
    : undefined;

  return {
    id: dto.id,
    reviewerId: dto.buyerId,
    reviewerName: dto.buyerName || 'Usuario',
    reviewerAvatarUrl: dto.buyerPhotoUrl ?? undefined,
    targetType: 'seller', // ReviewService only tracks seller/dealer reviews
    targetId: dto.sellerId,
    overallRating: dto.rating,
    title: dto.title || undefined,
    content: dto.content,
    status: dto.isVerifiedPurchase ? 'approved' : 'approved', // approved by default if returned
    isVerifiedPurchase: dto.isVerifiedPurchase,
    response: reviewResponse,
    helpfulCount: helpfulVotes,
    notHelpfulCount: notHelpfulVotes >= 0 ? notHelpfulVotes : 0,
    hasVoted,
    userVote,
    createdAt: dto.createdAt,
  };
}

/** Maps backend paged response to frontend PaginatedResponse<Review> */
function mapBackendPaged(dto: BackendPagedReviewsDto): PaginatedResponse<Review> {
  return {
    items: dto.reviews.map(mapBackendReview),
    pagination: {
      page: dto.page,
      pageSize: dto.pageSize,
      totalItems: dto.totalCount,
      totalPages: dto.totalPages,
      hasNextPage: dto.hasNextPage,
      hasPreviousPage: dto.hasPreviousPage,
    },
  };
}

export interface ReviewSearchParams {
  targetType?: 'dealer' | 'vehicle' | 'seller';
  targetId?: string; // sellerId
  rating?: number;
  status?: ReviewStatus;
  sortBy?: 'newest' | 'oldest' | 'highest' | 'lowest' | 'helpful';
  page?: number;
  pageSize?: number;
}

/** Maps to BackendReviewSummaryDto — used by ReviewSummaryBar */
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
  verifiedPurchaseCount: number;
  positivePercentage?: number;
}

export interface ReviewSummary {
  targetId: string;
  targetType: 'dealer' | 'vehicle' | 'seller';
  averageRating: number;
  totalReviews: number;
  recentReviews: Review[];
}

// ============================================================
// API FUNCTIONS — mapped to real backend endpoints
// ============================================================

/**
 * Get reviews for a specific seller/dealer.
 * Backend: GET /api/reviews/seller/{sellerId}
 */
export async function getReviewsForTarget(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string,
  params: Omit<ReviewSearchParams, 'targetType' | 'targetId'> = {}
): Promise<PaginatedResponse<Review>> {
  const queryParams: Record<string, unknown> = {
    page: params.page ?? 1,
    pageSize: params.pageSize ?? 20,
  };
  if (params.rating) queryParams.rating = params.rating;

  const response = await apiClient.get<BackendPagedReviewsDto>(`/api/reviews/seller/${targetId}`, {
    params: queryParams,
  });

  return mapBackendPaged(response.data);
}

/**
 * Alias for getReviewsForTarget — kept for backwards compatibility.
 */
export async function getReviews(
  params: ReviewSearchParams = {}
): Promise<PaginatedResponse<Review>> {
  if (!params.targetId) {
    return {
      items: [],
      pagination: {
        page: 1,
        pageSize: 20,
        totalItems: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      },
    };
  }
  return getReviewsForTarget(params.targetType ?? 'seller', params.targetId, params);
}

/**
 * Get review by ID.
 * Backend: GET /api/reviews/{reviewId}
 */
export async function getReviewById(id: string): Promise<Review> {
  const response = await apiClient.get<BackendReviewDto>(`/api/reviews/${id}`);
  return mapBackendReview(response.data);
}

/**
 * Get review statistics for a seller/dealer.
 * Backend: GET /api/reviews/seller/{sellerId}/summary
 */
export async function getReviewStats(
  _targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string
): Promise<ReviewStats> {
  const response = await apiClient.get<BackendReviewSummaryDto>(
    `/api/reviews/seller/${targetId}/summary`
  );
  const dto = response.data;
  // Map numeric-keyed distribution from backend
  const dist = dto.ratingDistribution ?? {};
  return {
    averageRating: dto.averageRating ?? 0,
    totalReviews: dto.totalReviews ?? 0,
    ratingDistribution: {
      1: dist['1'] ?? 0,
      2: dist['2'] ?? 0,
      3: dist['3'] ?? 0,
      4: dist['4'] ?? 0,
      5: dist['5'] ?? 0,
    },
    verifiedPurchaseCount: dto.verifiedPurchaseReviews ?? 0,
    positivePercentage: dto.positivePercentage,
  };
}

/**
 * Get review summary (redirects to getReviewStats for compatibility).
 * Backend: GET /api/reviews/seller/{sellerId}/summary
 */
export async function getReviewSummary(
  targetType: 'dealer' | 'vehicle' | 'seller',
  targetId: string
): Promise<ReviewSummary> {
  const stats = await getReviewStats(targetType, targetId);
  return {
    targetId,
    targetType,
    averageRating: stats.averageRating,
    totalReviews: stats.totalReviews,
    recentReviews: [],
  };
}

/**
 * Create a new review.
 * Backend: POST /api/reviews
 * Maps frontend CreateReviewRequest → backend CreateReviewDto
 */
export async function createReview(data: CreateReviewRequest): Promise<Review> {
  // Map frontend payload to backend DTO
  const backendPayload = {
    sellerId: data.targetId,
    vehicleId: null,
    orderId: null,
    rating: data.overallRating,
    title: data.title ?? '',
    content: data.content,
  };

  const response = await apiClient.post<BackendReviewDto>('/api/reviews', backendPayload);
  return mapBackendReview(response.data);
}

/**
 * Update an existing review.
 * Backend: PUT /api/reviews/{reviewId}
 */
export async function updateReview(id: string, data: UpdateReviewRequest): Promise<Review> {
  const backendPayload = {
    rating: data.overallRating,
    title: data.title ?? '',
    content: data.content ?? '',
  };
  const response = await apiClient.put<BackendReviewDto>(`/api/reviews/${id}`, backendPayload);
  return mapBackendReview(response.data);
}

/**
 * Delete a review.
 * Backend: DELETE /api/reviews/{reviewId}
 */
export async function deleteReview(id: string): Promise<void> {
  await apiClient.delete(`/api/reviews/${id}`);
}

/**
 * Respond to a review (seller/dealer only).
 * Backend: POST /api/reviews/{reviewId}/respond
 * Body: { responseText: string }  (NOT `content`)
 */
export async function respondToReview(
  reviewId: string,
  data: ReviewResponseRequest
): Promise<void> {
  // Backend expects { responseText } not { content }
  await apiClient.post(`/api/reviews/${reviewId}/respond`, { responseText: data.content });
}

/**
 * Vote on a review (helpful/not helpful).
 * Backend: POST /api/reviews/{reviewId}/vote
 * Body: { isHelpful: bool }  (NOT `vote: string`)
 */
export async function voteReview(reviewId: string, vote: 'helpful' | 'notHelpful'): Promise<void> {
  await apiClient.post(`/api/reviews/${reviewId}/vote`, { isHelpful: vote === 'helpful' });
}

/**
 * Report a review (flag for moderation).
 * Backend: POST /api/reviews/{reviewId}/report
 */
export async function reportReview(reviewId: string, reason: string): Promise<void> {
  await apiClient.post(`/api/reviews/${reviewId}/report`, { reason });
}

/**
 * Get reviews written by the current buyer.
 * Backend: GET /api/reviews/buyer/{buyerId}
 */
export async function getUserReviews(
  buyerId?: string,
  params: { page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<Review>> {
  if (!buyerId) {
    return {
      items: [],
      pagination: {
        page: 1,
        pageSize: 20,
        totalItems: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      },
    };
  }
  const response = await apiClient.get<BackendPagedReviewsDto>(`/api/reviews/buyer/${buyerId}`, {
    params: { page: params.page ?? 1, pageSize: params.pageSize ?? 20 },
  });
  return mapBackendPaged(response.data);
}

/**
 * Check if the current user can review a seller.
 * Simplified: returns true if user is authenticated (backend validates duplicate on submit).
 */
export async function canReview(
  _targetType: 'dealer' | 'vehicle' | 'seller',
  _targetId: string
): Promise<{ canReview: boolean; reason?: string }> {
  // The backend prevents duplicates on POST /api/reviews.
  // We optimistically allow the form to open; the backend will reject if already reviewed.
  return { canReview: true };
}

/**
 * Remove vote from a review (cancel a previous vote).
 * Backend: POST /api/reviews/{reviewId}/vote  (sending same vote toggles it off)
 */
export async function removeVote(reviewId: string): Promise<void> {
  // Backend doesn't have DELETE vote; calling POST with same value toggles
  await apiClient.post(`/api/reviews/${reviewId}/vote`, { isHelpful: null });
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
