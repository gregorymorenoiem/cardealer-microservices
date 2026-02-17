/**
 * Vehicle 360° Service — API client for Vehicle360ProcessingService.
 * Handles video upload, processing, and viewer data for 360° vehicle views.
 * Pipeline: Video → FFmpeg-API (frame extraction) → ClipDrop (bg removal) → S3.
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

export interface ProcessingJob {
  jobId: string;
  vehicleId: string;
  status: Vehicle360Status;
  progress?: number;
  currentStage?: string;
  frameCount?: number;
  createdAt: string;
  completedAt?: string;
  errorMessage?: string;
}

export type Vehicle360Status =
  | 'Pending'
  | 'Queued'
  | 'Processing'
  | 'UploadingVideo'
  | 'VideoUploaded'
  | 'ExtractingFrames'
  | 'FramesExtracted'
  | 'RemovingBackgrounds'
  | 'UploadingResults'
  | 'Completed'
  | 'Failed'
  | 'Cancelled';

export interface ProcessingOptions {
  frameCount?: number;
  width?: number;
  height?: number;
  format?: string;
  removeBackground?: boolean;
  autoCorrectExposure?: boolean;
  backgroundColor?: string;
}

export interface ViewerData {
  id: string;
  type: 'photos' | 'video';
  status: string;
  frameCount: number;
  frameUrls: string[];
  thumbnailUrl?: string;
  config: ViewerConfig;
}

export interface ViewerConfig {
  initialFrame: number;
  autoRotate: boolean;
  autoRotateSpeed: number;
  allowZoom: boolean;
  maxZoom: number;
  showControls: boolean;
  invertDrag: boolean;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Upload a video for 360° processing (internal pipeline).
 */
export async function uploadVideo(
  file: File,
  vehicleId: string,
  onProgress?: (progress: number) => void
): Promise<ProcessingJob> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('vehicleId', vehicleId);

  const response = await apiClient.post<ProcessingJob>('/api/vehicle360/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    timeout: 300000, // 5 min timeout for video upload
    onUploadProgress: e => {
      if (onProgress && e.total) {
        onProgress(Math.round((e.loaded * 100) / e.total));
      }
    },
  });
  return response.data;
}

/**
 * Process a video already uploaded to S3.
 */
export async function processVideo(
  storageUrl: string,
  vehicleId: string,
  options?: ProcessingOptions
): Promise<ProcessingJob> {
  const response = await apiClient.post<ProcessingJob>('/api/vehicle360/process', {
    storageUrl,
    vehicleId,
    ...options,
  });
  return response.data;
}

/**
 * Get the status of a processing job with progress details.
 */
export async function getJobStatus(jobId: string): Promise<ProcessingJob> {
  const response = await apiClient.get<ProcessingJob>(`/api/vehicle360/jobs/${jobId}/status`);
  return response.data;
}

/**
 * Get the full result of a completed processing job.
 */
export async function getJobResult(jobId: string): Promise<ProcessingJob> {
  const response = await apiClient.get<ProcessingJob>(`/api/vehicle360/jobs/${jobId}/result`);
  return response.data;
}

/**
 * Retry a failed processing job (max 3 retries).
 */
export async function retryJob(jobId: string): Promise<ProcessingJob> {
  const response = await apiClient.post<ProcessingJob>(`/api/vehicle360/jobs/${jobId}/retry`);
  return response.data;
}

/**
 * Cancel an in-progress processing job.
 */
export async function cancelJob(jobId: string): Promise<void> {
  await apiClient.post(`/api/vehicle360/jobs/${jobId}/cancel`);
}

/**
 * Get viewer data for a vehicle's 360° view (used by the viewer component).
 */
export async function getViewerData(vehicleId: string): Promise<ViewerData> {
  const response = await apiClient.get<ViewerData>(`/api/vehicle360/viewer/${vehicleId}`);
  return response.data;
}

/**
 * Get processing job history for a vehicle.
 */
export async function getVehicleJobs(vehicleId: string): Promise<ProcessingJob[]> {
  const response = await apiClient.get<ProcessingJob[]>(
    `/api/vehicle360/vehicles/${vehicleId}/jobs`
  );
  return response.data;
}

/**
 * Get user's processing jobs (paginated).
 */
export async function getUserJobs(
  userId: string,
  page = 1,
  pageSize = 10
): Promise<PaginatedResult<ProcessingJob>> {
  const response = await apiClient.get<PaginatedResult<ProcessingJob>>(
    `/api/vehicle360/user/${userId}/jobs`,
    { params: { page, pageSize } }
  );
  return response.data;
}

// ============================================================
// SERVICE OBJECT
// ============================================================

export const vehicle360Service = {
  uploadVideo,
  processVideo,
  getJobStatus,
  getJobResult,
  retryJob,
  cancelJob,
  getViewerData,
  getVehicleJobs,
  getUserJobs,
};

export default vehicle360Service;
