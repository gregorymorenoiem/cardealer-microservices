/**
 * Spyne AI Image Processing Service
 *
 * Provides vehicle image transformation using Spyne AI
 * - Background replacement with professional presets
 * - License plate masking
 * - 360 spin generation
 * - Batch processing
 */

import api from './api';

// ============================================================================
// Types & Interfaces
// ============================================================================

export interface VehicleTransformRequest {
  /** URL of the image to transform */
  imageUrl: string;
  /** Background ID to use (default: 923 - Studio White) */
  backgroundId?: string;
  /** VIN of the vehicle (optional) */
  vin?: string;
  /** Stock number for tracking (optional) */
  stockNumber?: string;
  /** Whether to mask/blur the license plate (default: true) */
  maskLicensePlate?: boolean;
}

export interface BatchTransformRequest {
  /** List of image URLs to transform */
  imageUrls: string[];
  /** List of video URLs (optional, for 360 spin) */
  videoUrls?: string[];
  /** Background ID to use */
  backgroundId?: string;
  /** VIN of the vehicle */
  vin?: string;
  /** Stock number for tracking */
  stockNumber?: string;
  /** Whether to mask the license plate */
  maskLicensePlate?: boolean;
  /** Generate 360 spin from images */
  generate360Spin?: boolean;
  /** Generate feature video */
  generateVideo?: boolean;
  /** Enable hotspots in 360 spin */
  enableHotspots?: boolean;
  /** Number of frames for 360 spin (default: 72) */
  spinFrameCount?: number;
}

export interface TransformResponse {
  jobId: string;
  status: 'processing' | 'completed' | 'failed';
  message: string;
  estimatedSeconds: number;
  checkStatusUrl: string;
}

export interface ProcessedImage {
  imageId?: string;
  originalUrl?: string;
  processedUrl?: string;
  status?: string;
  category?: string;
  viewAngle?: string;
}

export interface ProcessedSpin {
  spinId?: string;
  embedUrl?: string;
  status?: string;
}

export interface ProcessedVideo {
  videoId?: string;
  outputUrl?: string;
  status?: string;
}

export interface JobStatusResponse {
  jobId: string;
  status: 'processing' | 'completed' | 'failed';
  images?: ProcessedImage[];
  spin?: ProcessedSpin;
  video?: ProcessedVideo;
}

export interface BackgroundOption {
  id: string;
  name: string;
  category: string;
  previewUrl: string;
}

// ============================================================================
// API Functions
// ============================================================================

const SPYNE_BASE = '/api/spyne/vehicle-images';

/**
 * Transform a single vehicle image with AI background replacement
 * @param request - Transform request with image URL and options
 * @returns Job ID and status information
 */
export async function transformImage(request: VehicleTransformRequest): Promise<TransformResponse> {
  const response = await api.post<TransformResponse>(`${SPYNE_BASE}/transform`, request);
  return response.data;
}

/**
 * Transform multiple images in batch
 * @param request - Batch transform request
 * @returns Job ID and status for batch
 */
export async function transformBatch(request: BatchTransformRequest): Promise<TransformResponse> {
  const response = await api.post<TransformResponse>(`${SPYNE_BASE}/transform/batch`, request);
  return response.data;
}

/**
 * Get the status of a transformation job
 * @param jobId - The job ID returned from transform
 * @returns Current status and processed images
 */
export async function getJobStatus(jobId: string): Promise<JobStatusResponse> {
  const response = await api.get<JobStatusResponse>(`${SPYNE_BASE}/status/${jobId}`);
  return response.data;
}

/**
 * Poll for job completion
 * @param jobId - The job ID to poll
 * @param onProgress - Callback for progress updates
 * @param maxAttempts - Maximum poll attempts (default: 60)
 * @param intervalMs - Poll interval in ms (default: 2000)
 * @returns Final job status
 */
export async function pollJobStatus(
  jobId: string,
  onProgress?: (status: JobStatusResponse) => void,
  maxAttempts: number = 60,
  intervalMs: number = 2000
): Promise<JobStatusResponse> {
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    const status = await getJobStatus(jobId);

    if (onProgress) {
      onProgress(status);
    }

    if (status.status === 'completed' || status.status === 'failed') {
      return status;
    }

    // Wait before next poll
    await new Promise((resolve) => setTimeout(resolve, intervalMs));
  }

  throw new Error(
    `Job ${jobId} did not complete within ${(maxAttempts * intervalMs) / 1000} seconds`
  );
}

/**
 * Get available background presets
 * @returns List of background options
 */
export async function getBackgrounds(): Promise<BackgroundOption[]> {
  const response = await api.get<BackgroundOption[]>(`${SPYNE_BASE}/backgrounds`);
  return response.data;
}

/**
 * Transform image and wait for result
 * Convenience function that combines transform + polling
 * @param request - Transform request
 * @param onProgress - Optional progress callback
 * @returns Processed image URLs
 */
export async function transformAndWait(
  request: VehicleTransformRequest,
  onProgress?: (status: JobStatusResponse) => void
): Promise<ProcessedImage[]> {
  const transformResult = await transformImage(request);
  const finalStatus = await pollJobStatus(transformResult.jobId, onProgress);

  if (finalStatus.status === 'failed') {
    throw new Error('Image transformation failed');
  }

  return finalStatus.images || [];
}

// ============================================================================
// Background Presets (Common ones for quick reference)
// ============================================================================

export const BACKGROUND_PRESETS = {
  STUDIO_WHITE: '923',
  SHOWROOM_FLOOR: '924',
  OUTDOOR_STREET: '925',
  LUXURY_GARAGE: '926',
  MODERN_DEALERSHIP: '927',
  URBAN_NIGHT: '928',
  CLEAN_BACKGROUND: '929',
} as const;

// ============================================================================
// React Hooks (if needed)
// ============================================================================

import { useState, useCallback } from 'react';

export interface UseSpyneTransformResult {
  isLoading: boolean;
  error: string | null;
  jobId: string | null;
  status: JobStatusResponse | null;
  processedImages: ProcessedImage[];
  transform: (request: VehicleTransformRequest) => Promise<void>;
  reset: () => void;
}

/**
 * React hook for Spyne image transformation
 */
export function useSpyneTransform(): UseSpyneTransformResult {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [jobId, setJobId] = useState<string | null>(null);
  const [status, setStatus] = useState<JobStatusResponse | null>(null);
  const [processedImages, setProcessedImages] = useState<ProcessedImage[]>([]);

  const transform = useCallback(async (request: VehicleTransformRequest) => {
    setIsLoading(true);
    setError(null);
    setJobId(null);
    setStatus(null);
    setProcessedImages([]);

    try {
      const result = await transformImage(request);
      setJobId(result.jobId);

      const finalStatus = await pollJobStatus(result.jobId, (s) => setStatus(s));

      if (finalStatus.status === 'completed' && finalStatus.images) {
        setProcessedImages(finalStatus.images);
      } else if (finalStatus.status === 'failed') {
        setError('Image transformation failed');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const reset = useCallback(() => {
    setIsLoading(false);
    setError(null);
    setJobId(null);
    setStatus(null);
    setProcessedImages([]);
  }, []);

  return {
    isLoading,
    error,
    jobId,
    status,
    processedImages,
    transform,
    reset,
  };
}

// ============================================================================
// Export Default
// ============================================================================

const spyneService = {
  transformImage,
  transformBatch,
  getJobStatus,
  pollJobStatus,
  getBackgrounds,
  transformAndWait,
  BACKGROUND_PRESETS,
};

export default spyneService;
