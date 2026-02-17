/**
 * Background Removal Service — API client for BackgroundRemovalService.
 * Handles single and batch background removal via the internal pipeline
 * (ClipDrop → Remove.bg → Photoroom → Slazzer fallback chain).
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

export interface BackgroundRemovalJob {
  jobId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  provider?: string;
  estimatedTimeSeconds?: number;
}

export interface BackgroundRemovalStatus {
  jobId: string;
  status: 'pending' | 'processing' | 'completed' | 'failed';
  provider: string;
  progress?: number;
  errorMessage?: string;
}

export interface BackgroundRemovalResult {
  jobId: string;
  originalUrl: string;
  processedUrl: string;
  provider: string;
  processingTimeMs: number;
}

export interface BatchRemovalJob {
  batchId: string;
  jobs: BackgroundRemovalJob[];
  totalImages: number;
  completedImages: number;
  status: 'pending' | 'processing' | 'completed' | 'partial' | 'failed';
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Remove background from a single image.
 */
export async function removeBackground(imageUrl: string): Promise<BackgroundRemovalJob> {
  const response = await apiClient.post<BackgroundRemovalJob>('/api/backgroundremoval/remove', {
    imageUrl,
  });
  return response.data;
}

/**
 * Remove background from multiple images (batch, max 10).
 */
export async function removeBackgroundBatch(imageUrls: string[]): Promise<BatchRemovalJob> {
  const response = await apiClient.post<BatchRemovalJob>('/api/backgroundremoval/batch', {
    imageUrls,
  });
  return response.data;
}

/**
 * Get the status of a background removal job.
 */
export async function getRemovalStatus(jobId: string): Promise<BackgroundRemovalStatus> {
  const response = await apiClient.get<BackgroundRemovalStatus>(
    `/api/backgroundremoval/${jobId}/status`
  );
  return response.data;
}

/**
 * Get the result of a completed background removal job.
 */
export async function getRemovalResult(jobId: string): Promise<BackgroundRemovalResult> {
  const response = await apiClient.get<BackgroundRemovalResult>(
    `/api/backgroundremoval/${jobId}/result`
  );
  return response.data;
}

// ============================================================
// SERVICE OBJECT
// ============================================================

export const backgroundRemovalService = {
  removeBackground,
  removeBackgroundBatch,
  getRemovalStatus,
  getRemovalResult,
};

export default backgroundRemovalService;
