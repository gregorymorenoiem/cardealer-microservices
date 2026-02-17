/**
 * React Query hooks for Background Removal operations.
 * Provides mutations for removing backgrounds and queries with polling for status tracking.
 */

import { useMutation, useQuery } from '@tanstack/react-query';
import {
  removeBackground,
  removeBackgroundBatch,
  getRemovalStatus,
  getRemovalResult,
  type BackgroundRemovalJob,
  type BatchRemovalJob,
  type BackgroundRemovalStatus,
  type BackgroundRemovalResult,
} from '@/services/background-removal';

/**
 * Mutation: Remove background from a single image.
 */
export function useRemoveBackground() {
  return useMutation<BackgroundRemovalJob, Error, string>({
    mutationFn: (imageUrl: string) => removeBackground(imageUrl),
  });
}

/**
 * Mutation: Remove background from multiple images (batch).
 */
export function useBatchRemoveBackground() {
  return useMutation<BatchRemovalJob, Error, string[]>({
    mutationFn: (imageUrls: string[]) => removeBackgroundBatch(imageUrls),
  });
}

/**
 * Query: Poll for background removal job status.
 * Stops polling when status is 'completed' or 'failed'.
 */
export function useRemovalStatus(jobId: string | null) {
  return useQuery<BackgroundRemovalStatus>({
    queryKey: ['bg-removal-status', jobId],
    queryFn: () => getRemovalStatus(jobId!),
    enabled: !!jobId,
    refetchInterval: query => {
      const status = query.state.data?.status;
      if (status === 'completed' || status === 'failed') return false;
      return 3000; // Poll every 3 seconds
    },
  });
}

/**
 * Query: Get the result of a completed background removal job.
 * Only enabled when the job is completed.
 */
export function useRemovalResult(jobId: string | null, isCompleted: boolean) {
  return useQuery<BackgroundRemovalResult>({
    queryKey: ['bg-removal-result', jobId],
    queryFn: () => getRemovalResult(jobId!),
    enabled: !!jobId && isCompleted,
  });
}

// Re-exports
export type {
  BackgroundRemovalJob,
  BatchRemovalJob,
  BackgroundRemovalStatus,
  BackgroundRemovalResult,
};
