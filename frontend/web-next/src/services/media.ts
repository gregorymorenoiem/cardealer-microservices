/**
 * Media Service - API client for file upload operations
 * Connects via API Gateway to MediaService
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

export interface UploadResponse {
  url: string;
  publicId: string;
  storageKey: string;
  size: number;
  contentType: string;
  fileName: string;
}

export interface ImageUploadResponse {
  url: string;
  publicId: string;
  width: number;
  height: number;
  format: string;
  size: number;
}

export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Upload a single image file
 */
export async function uploadImage(
  file: File,
  folder: string = 'vehicles',
  onProgress?: (progress: UploadProgress) => void
): Promise<ImageUploadResponse> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('folder', folder);

  const response = await apiClient.post<ImageUploadResponse>('/api/media/upload/image', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
    onUploadProgress: progressEvent => {
      if (onProgress && progressEvent.total) {
        onProgress({
          loaded: progressEvent.loaded,
          total: progressEvent.total,
          percentage: Math.round((progressEvent.loaded * 100) / progressEvent.total),
        });
      }
    },
  });

  return response.data;
}

/**
 * Upload multiple images
 */
export async function uploadImages(
  files: File[],
  folder: string = 'vehicles',
  onProgress?: (index: number, progress: UploadProgress) => void
): Promise<ImageUploadResponse[]> {
  const results: ImageUploadResponse[] = [];

  for (let i = 0; i < files.length; i++) {
    const file = files[i];
    const result = await uploadImage(file, folder, progress => {
      onProgress?.(i, progress);
    });
    results.push(result);
  }

  return results;
}

/**
 * Upload a generic file (video, document, etc.)
 */
export async function uploadFile(
  file: File,
  folder: string = 'uploads',
  type: string = 'file',
  onProgress?: (progress: UploadProgress) => void
): Promise<UploadResponse> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('folder', folder);
  formData.append('type', type);

  const response = await apiClient.post<UploadResponse>('/api/media/upload', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
    onUploadProgress: progressEvent => {
      if (onProgress && progressEvent.total) {
        onProgress({
          loaded: progressEvent.loaded,
          total: progressEvent.total,
          percentage: Math.round((progressEvent.loaded * 100) / progressEvent.total),
        });
      }
    },
  });

  return response.data;
}

/**
 * Delete a media file by publicId
 */
export async function deleteMedia(publicId: string): Promise<void> {
  await apiClient.delete(`/api/media/${publicId}`);
}

/**
 * Get media info by ID
 */
export async function getMedia(mediaId: string): Promise<UploadResponse> {
  const response = await apiClient.get<UploadResponse>(`/api/media/${mediaId}`);
  return response.data;
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

/**
 * Validate image file before upload
 */
export function validateImageFile(file: File): { valid: boolean; error?: string } {
  const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
  const maxSize = 10 * 1024 * 1024; // 10MB

  if (!allowedTypes.includes(file.type)) {
    return { valid: false, error: 'Tipo de archivo no permitido. Use JPEG, PNG, GIF o WebP.' };
  }

  if (file.size > maxSize) {
    return { valid: false, error: 'El archivo es muy grande. Máximo 10MB.' };
  }

  return { valid: true };
}

/**
 * Validate video file before upload
 */
export function validateVideoFile(file: File): { valid: boolean; error?: string } {
  const allowedTypes = ['video/mp4', 'video/webm', 'video/quicktime'];
  const maxSize = 500 * 1024 * 1024; // 500MB

  if (!allowedTypes.includes(file.type)) {
    return { valid: false, error: 'Tipo de video no permitido. Use MP4, WebM o MOV.' };
  }

  if (file.size > maxSize) {
    return { valid: false, error: 'El video es muy grande. Máximo 500MB.' };
  }

  return { valid: true };
}

/**
 * Create object URL for preview
 */
export function createPreviewUrl(file: File): string {
  return URL.createObjectURL(file);
}

/**
 * Revoke object URL to free memory
 */
export function revokePreviewUrl(url: string): void {
  URL.revokeObjectURL(url);
}

// ============================================================
// VEHICLE IMAGE TYPES
// ============================================================

export interface VehicleImageUploadParams {
  file: File;
  vehicleId?: string;
  imageType?: string;
  sortOrder?: number;
  isPrimary?: boolean;
  compress?: boolean;
}

export interface VehicleImageUploadResponse {
  mediaId: string;
  originalUrl: string;
  thumbnailUrl: string;
  width: number;
  height: number;
  fileSize: number;
  processingStatus: string;
}

export interface PresignedUrlFile {
  fileName: string;
  contentType: string;
  fileSize: number;
}

export interface PresignedUrlRequest {
  files: PresignedUrlFile[];
  vehicleId?: string;
  category?: string;
}

export interface PresignedUrlResult {
  mediaId: string;
  presignedUrl: string;
  expiresAt: string;
  storageKey: string;
  headers: Record<string, string>;
}

export interface ImageQualityResult {
  overallScore: number;
  isTooSmall: boolean;
  warnings: string[];
  suggestions: string[];
  width: number;
  height: number;
  fileSize: number;
}

// ============================================================
// VEHICLE IMAGE FUNCTIONS
// ============================================================

/**
 * Upload a vehicle image with server-side compression & inline thumbnail
 */
export async function uploadVehicleImage(
  params: VehicleImageUploadParams,
  onProgress?: (progress: UploadProgress) => void
): Promise<VehicleImageUploadResponse> {
  const formData = new FormData();
  formData.append('File', params.file);
  if (params.vehicleId) formData.append('VehicleId', params.vehicleId);
  if (params.imageType) formData.append('ImageType', params.imageType);
  if (params.sortOrder !== undefined) formData.append('SortOrder', String(params.sortOrder));
  if (params.isPrimary !== undefined) formData.append('IsPrimary', String(params.isPrimary));
  if (params.compress !== undefined) formData.append('Compress', String(params.compress));

  const response = await apiClient.post<VehicleImageUploadResponse>(
    '/api/media/upload/vehicle-image',
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 120_000,
      onUploadProgress: progressEvent => {
        if (onProgress && progressEvent.total) {
          onProgress({
            loaded: progressEvent.loaded,
            total: progressEvent.total,
            percentage: Math.round((progressEvent.loaded * 100) / progressEvent.total),
          });
        }
      },
    }
  );

  return response.data;
}

/**
 * Get batch pre-signed S3 upload URLs
 */
export async function getPresignedUrls(
  request: PresignedUrlRequest
): Promise<PresignedUrlResult[]> {
  const response = await apiClient.post<PresignedUrlResult[]>(
    '/api/media/upload/presigned-urls',
    request
  );
  return response.data;
}

/**
 * Validate image quality before upload (client-side check via backend)
 */
export async function validateImageQuality(
  file: File
): Promise<ImageQualityResult> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await apiClient.post<ImageQualityResult>(
    '/api/media/validate/quality',
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );

  return response.data;
}

/**
 * Finalize a pre-signed URL upload (confirm upload completed)
 */
export async function finalizeUpload(mediaId: string): Promise<void> {
  await apiClient.post(`/api/media/${mediaId}/finalize`);
}

// ============================================================
// SERVICE OBJECT
// ============================================================

export const mediaService = {
  uploadImage,
  uploadImages,
  uploadFile,
  uploadVehicleImage,
  getPresignedUrls,
  validateImageQuality,
  finalizeUpload,
  deleteMedia,
  getMedia,
  validateImageFile,
  validateVideoFile,
  createPreviewUrl,
  revokePreviewUrl,
};

export default mediaService;
