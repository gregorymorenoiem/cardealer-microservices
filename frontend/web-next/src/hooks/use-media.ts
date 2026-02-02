/**
 * React Query hooks for Media operations
 * Provides upload and management functionality
 */

import { useMutation } from '@tanstack/react-query';
import {
  uploadImage,
  uploadImages,
  uploadFile,
  deleteMedia,
  validateImageFile,
  validateVideoFile,
  type ImageUploadResponse,
  type UploadResponse,
  type UploadProgress,
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
export type { ImageUploadResponse, UploadResponse, UploadProgress };
