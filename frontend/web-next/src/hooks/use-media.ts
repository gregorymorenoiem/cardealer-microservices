/**
 * React Query hooks for Media operations
 * Provides upload and management functionality
 */

import { useMutation, useQuery } from '@tanstack/react-query';
import {
  uploadImage,
  uploadImages,
  uploadFile,
  deleteMedia,
  uploadVehicleImage,
  getPresignedUrls,
  validateImageQuality,
  finalizeUpload,
  validateImageFile,
  validateVideoFile,
  type ImageUploadResponse,
  type UploadResponse,
  type UploadProgress,
  type VehicleImageUploadParams,
  type VehicleImageUploadResponse,
  type PresignedUrlRequest,
  type PresignedUrlResult,
  type ImageQualityResult,
} from '@/services/media';

// =============================================================================
// TYPES
// =============================================================================

export interface UploadImageParams {
  file: File;
  folder?: string;
  onProgress?: (progress: UploadProgress) => void;
}

export interface UploadImagesParams {
  files: File[];
  folder?: string;
  onProgress?: (index: number, progress: UploadProgress) => void;
}

export interface UploadFileParams {
  file: File;
  folder?: string;
  type?: string;
  onProgress?: (progress: UploadProgress) => void;
}

// =============================================================================
// UPLOAD HOOKS
// =============================================================================

/**
 * Upload a single image
 */
export function useUploadImage() {
  return useMutation({
    mutationFn: ({ file, folder, onProgress }: UploadImageParams) =>
      uploadImage(file, folder, onProgress),
  });
}

/**
 * Upload multiple images
 */
export function useUploadImages() {
  return useMutation({
    mutationFn: ({ files, folder, onProgress }: UploadImagesParams) =>
      uploadImages(files, folder, onProgress),
  });
}

/**
 * Upload a generic file
 */
export function useUploadFile() {
  return useMutation({
    mutationFn: ({ file, folder, type, onProgress }: UploadFileParams) =>
      uploadFile(file, folder, type, onProgress),
  });
}

/**
 * Upload a vehicle image with server-side compression & inline thumbnail
 */
export function useUploadVehicleImage() {
  return useMutation<VehicleImageUploadResponse, Error, VehicleImageUploadParams & { onProgress?: (progress: UploadProgress) => void }>({
    mutationFn: ({ onProgress, ...params }) =>
      uploadVehicleImage(params, onProgress),
  });
}

/**
 * Get batch pre-signed S3 upload URLs
 */
export function useGetPresignedUrls() {
  return useMutation<PresignedUrlResult[], Error, PresignedUrlRequest>({
    mutationFn: (request) => getPresignedUrls(request),
  });
}

/**
 * Validate image quality before upload
 */
export function useValidateImageQuality(file: File | null) {
  return useQuery<ImageQualityResult>({
    queryKey: ['image-quality', file?.name, file?.size],
    queryFn: () => validateImageQuality(file!),
    enabled: !!file,
    staleTime: Infinity, // quality doesn't change for same file
  });
}

/**
 * Finalize a pre-signed URL upload
 */
export function useFinalizeUpload() {
  return useMutation<void, Error, string>({
    mutationFn: (mediaId) => finalizeUpload(mediaId),
  });
}

/**
 * Delete a media file
 */
export function useDeleteMedia() {
  return useMutation({
    mutationFn: (publicId: string) => deleteMedia(publicId),
  });
}

// =============================================================================
// RE-EXPORTS
// =============================================================================

export { validateImageFile, validateVideoFile };
export type { ImageUploadResponse, UploadResponse, UploadProgress, VehicleImageUploadResponse, PresignedUrlResult, ImageQualityResult };
