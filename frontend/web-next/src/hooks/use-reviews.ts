/**
 * Reviews Hook - React hook for review operations
 *
 * Provides:
 * - Reviews for dealers/vehicles/sellers
 * - Review creation and management
 * - Review statistics
 */

'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  reviewService,
  type ReviewSearchParams,
  type CreateReviewRequest,
  type UpdateReviewRequest,
  type Review,
  type ReviewStats,
} from '@/services/reviews';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const reviewKeys = {
  all: ['reviews'] as const,
  lists: () => [...reviewKeys.all, 'list'] as const,
  list: (params: ReviewSearchParams) => [...reviewKeys.lists(), params] as const,
  details: () => [...reviewKeys.all, 'detail'] as const,
  detail: (id: string) => [...reviewKeys.details(), id] as const,
  target: (type: string, id: string) => [...reviewKeys.all, 'target', type, id] as const,
  stats: (type: string, id: string) => [...reviewKeys.all, 'stats', type, id] as const,
  summary: (type: string, id: string) => [...reviewKeys.all, 'summary', type, id] as const,
  user: (userId?: string) => [...reviewKeys.all, 'user', userId] as const,
  canReview: (type: string, id: string) => [...reviewKeys.all, 'canReview', type, id] as const,
};

// =============================================================================
// HOOKS - QUERIES
// =============================================================================

/**
 * Get reviews for a target (dealer/vehicle/seller)
 */
export function useReviewsForTarget(
  targetType: 'dealer' | 'vehicle' | 'seller' | undefined,
  targetId: string | undefined,
  params: Omit<ReviewSearchParams, 'targetType' | 'targetId'> = {}
) {
  return useQuery({
    queryKey: reviewKeys.target(targetType || '', targetId || ''),
    queryFn: () => reviewService.getReviewsForTarget(targetType!, targetId!, params),
    enabled: !!targetType && !!targetId,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Get review by ID
 */
export function useReview(id: string | undefined) {
  return useQuery({
    queryKey: reviewKeys.detail(id || ''),
    queryFn: () => reviewService.getReviewById(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get review statistics for a target
 */
export function useReviewStats(
  targetType: 'dealer' | 'vehicle' | 'seller' | undefined,
  targetId: string | undefined
) {
  return useQuery({
    queryKey: reviewKeys.stats(targetType || '', targetId || ''),
    queryFn: () => reviewService.getReviewStats(targetType!, targetId!),
    enabled: !!targetType && !!targetId,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get review summary (stats + recent reviews)
 */
export function useReviewSummary(
  targetType: 'dealer' | 'vehicle' | 'seller' | undefined,
  targetId: string | undefined
) {
  return useQuery({
    queryKey: reviewKeys.summary(targetType || '', targetId || ''),
    queryFn: () => reviewService.getReviewSummary(targetType!, targetId!),
    enabled: !!targetType && !!targetId,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get current user's reviews
 */
export function useUserReviews(userId?: string, params: { page?: number; pageSize?: number } = {}) {
  return useQuery({
    queryKey: reviewKeys.user(userId),
    queryFn: () => reviewService.getUserReviews(userId, params),
    staleTime: 2 * 60 * 1000,
  });
}

/**
 * Check if user can review a target
 */
export function useCanReview(
  targetType: 'dealer' | 'vehicle' | 'seller' | undefined,
  targetId: string | undefined
) {
  return useQuery({
    queryKey: reviewKeys.canReview(targetType || '', targetId || ''),
    queryFn: () => reviewService.canReview(targetType!, targetId!),
    enabled: !!targetType && !!targetId,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

// =============================================================================
// HOOKS - MUTATIONS
// =============================================================================

/**
 * Create review mutation
 */
export function useCreateReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateReviewRequest) => reviewService.createReview(data),
    onSuccess: newReview => {
      // Invalidate related queries
      queryClient.invalidateQueries({
        queryKey: reviewKeys.target(newReview.targetType, newReview.targetId),
      });
      queryClient.invalidateQueries({
        queryKey: reviewKeys.stats(newReview.targetType, newReview.targetId),
      });
      queryClient.invalidateQueries({
        queryKey: reviewKeys.summary(newReview.targetType, newReview.targetId),
      });
      queryClient.invalidateQueries({ queryKey: reviewKeys.user() });
    },
  });
}

/**
 * Update review mutation
 */
export function useUpdateReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateReviewRequest }) =>
      reviewService.updateReview(id, data),
    onSuccess: updatedReview => {
      queryClient.setQueryData(reviewKeys.detail(updatedReview.id), updatedReview);
      queryClient.invalidateQueries({
        queryKey: reviewKeys.target(updatedReview.targetType, updatedReview.targetId),
      });
    },
  });
}

/**
 * Delete review mutation
 */
export function useDeleteReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => reviewService.deleteReview(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: reviewKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: reviewKeys.lists() });
      queryClient.invalidateQueries({ queryKey: reviewKeys.user() });
    },
  });
}

/**
 * Respond to review mutation (for target owners)
 */
export function useRespondToReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ reviewId, content }: { reviewId: string; content: string }) =>
      reviewService.respondToReview(reviewId, { content }),
    onSuccess: updatedReview => {
      queryClient.setQueryData(reviewKeys.detail(updatedReview.id), updatedReview);
      queryClient.invalidateQueries({
        queryKey: reviewKeys.target(updatedReview.targetType, updatedReview.targetId),
      });
    },
  });
}

/**
 * Vote on review mutation
 */
export function useVoteReview() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ reviewId, vote }: { reviewId: string; vote: 'helpful' | 'notHelpful' }) =>
      reviewService.voteReview(reviewId, vote),
    onSuccess: updatedReview => {
      queryClient.setQueryData(reviewKeys.detail(updatedReview.id), updatedReview);
    },
  });
}

/**
 * Report review mutation
 */
export function useReportReview() {
  return useMutation({
    mutationFn: ({ reviewId, reason }: { reviewId: string; reason: string }) =>
      reviewService.reportReview(reviewId, reason),
  });
}

// =============================================================================
// UTILITY FUNCTIONS (re-exported from service)
// =============================================================================

export const { formatRatingStars, getRatingLabel, getRatingColor, getRatingBgColor } =
  reviewService;

// =============================================================================
// EXPORTS
// =============================================================================

export type { Review, ReviewStats, ReviewSearchParams, CreateReviewRequest };
