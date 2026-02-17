import { useState, useCallback } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  uploadImage,
  uploadMultipleImages,
  deleteImage,
  uploadProfilePicture,
  compressImage,
  validateImageFiles,
  isValidImageFile,
  isValidFileSize,
  type UploadResponse,
  type UploadProgress,
} from '@/services/uploadService';

// ============================================================================
// Types
// ============================================================================

export interface UploadState {
  isUploading: boolean;
  progress: number;
  error: string | null;
  uploadedFiles: UploadResponse[];
}

export interface MultiUploadProgress {
  [index: number]: UploadProgress;
}

export interface UseUploadImageOptions {
  folder?: string;
  onSuccess?: (data: UploadResponse) => void;
  onError?: (error: Error) => void;
  onProgress?: (progress: UploadProgress) => void;
}

export interface UseUploadMultipleOptions {
  folder?: string;
  onSuccess?: (data: UploadResponse[]) => void;
  onError?: (error: Error) => void;
  onFileProgress?: (index: number, progress: UploadProgress) => void;
}

// ============================================================================
// Hook: useUploadImage - Upload single image with mutation
// ============================================================================

export function useUploadImage(options: UseUploadImageOptions = {}) {
  const { folder = 'vehicles', onSuccess, onError, onProgress } = options;
  const [progress, setProgress] = useState<UploadProgress | null>(null);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (file: File) => {
      // Reset progress
      setProgress(null);

      // Validate file
      if (!isValidImageFile(file)) {
        throw new Error('Invalid file type. Only images are allowed.');
      }

      if (!isValidFileSize(file)) {
        throw new Error('File is too large. Maximum size is 10MB.');
      }

      return uploadImage(file, folder, (p) => {
        setProgress(p);
        onProgress?.(p);
      });
    },
    onSuccess: (data) => {
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: ['media'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      onSuccess?.(data);
    },
    onError: (error: Error) => {
      setProgress(null);
      onError?.(error);
    },
  });

  const reset = useCallback(() => {
    setProgress(null);
    mutation.reset();
  }, [mutation]);

  return {
    upload: mutation.mutate,
    uploadAsync: mutation.mutateAsync,
    isUploading: mutation.isPending,
    progress,
    error: mutation.error,
    data: mutation.data,
    reset,
    isSuccess: mutation.isSuccess,
    isError: mutation.isError,
  };
}

// ============================================================================
// Hook: useUploadMultipleImages - Upload multiple images with progress tracking
// ============================================================================

export function useUploadMultipleImages(options: UseUploadMultipleOptions = {}) {
  const { folder = 'vehicles', onSuccess, onError, onFileProgress } = options;
  const [multiProgress, setMultiProgress] = useState<MultiUploadProgress>({});
  const [overallProgress, setOverallProgress] = useState(0);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (files: File[]) => {
      // Reset progress
      setMultiProgress({});
      setOverallProgress(0);

      // Validate all files
      const invalidFiles = files.filter((f) => !isValidImageFile(f) || !isValidFileSize(f));
      if (invalidFiles.length > 0) {
        throw new Error(`${invalidFiles.length} file(s) are invalid or too large.`);
      }

      return uploadMultipleImages(files, folder, (index, progress) => {
        setMultiProgress((prev) => ({ ...prev, [index]: progress }));
        
        // Calculate overall progress
        const totalFiles = files.length;
        const progressSum = Object.values({ ...multiProgress, [index]: progress }).reduce(
          (sum, p) => sum + p.percentage,
          0
        );
        setOverallProgress(Math.round(progressSum / totalFiles));
        
        onFileProgress?.(index, progress);
      });
    },
    onSuccess: (data) => {
      setOverallProgress(100);
      queryClient.invalidateQueries({ queryKey: ['media'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      onSuccess?.(data);
    },
    onError: (error: Error) => {
      setMultiProgress({});
      setOverallProgress(0);
      onError?.(error);
    },
  });

  const reset = useCallback(() => {
    setMultiProgress({});
    setOverallProgress(0);
    mutation.reset();
  }, [mutation]);

  return {
    upload: mutation.mutate,
    uploadAsync: mutation.mutateAsync,
    isUploading: mutation.isPending,
    multiProgress,
    overallProgress,
    error: mutation.error,
    data: mutation.data,
    reset,
    isSuccess: mutation.isSuccess,
    isError: mutation.isError,
  };
}

// ============================================================================
// Hook: useUploadProfilePicture - Upload profile picture with compression
// ============================================================================

export function useUploadProfilePicture(options: Omit<UseUploadImageOptions, 'folder'> = {}) {
  const { onSuccess, onError, onProgress } = options;
  const [progress, setProgress] = useState<UploadProgress | null>(null);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async (file: File) => {
      setProgress(null);

      if (!isValidImageFile(file)) {
        throw new Error('Invalid file type. Only images are allowed.');
      }

      return uploadProfilePicture(file, (p) => {
        setProgress(p);
        onProgress?.(p);
      });
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['user'] });
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      onSuccess?.(data);
    },
    onError: (error: Error) => {
      setProgress(null);
      onError?.(error);
    },
  });

  return {
    upload: mutation.mutate,
    uploadAsync: mutation.mutateAsync,
    isUploading: mutation.isPending,
    progress,
    error: mutation.error,
    data: mutation.data,
    reset: mutation.reset,
    isSuccess: mutation.isSuccess,
    isError: mutation.isError,
  };
}

// ============================================================================
// Hook: useDeleteImage - Delete uploaded image
// ============================================================================

export function useDeleteImage(options: { onSuccess?: () => void; onError?: (error: Error) => void } = {}) {
  const { onSuccess, onError } = options;
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: deleteImage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['media'] });
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
      onSuccess?.();
    },
    onError: (error: Error) => {
      onError?.(error);
    },
  });

  return {
    deleteImage: mutation.mutate,
    deleteImageAsync: mutation.mutateAsync,
    isDeleting: mutation.isPending,
    error: mutation.error,
    isSuccess: mutation.isSuccess,
    isError: mutation.isError,
  };
}

// ============================================================================
// Hook: useImageCompression - Compress image before upload (standalone)
// ============================================================================

export function useImageCompression() {
  const [isCompressing, setIsCompressing] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const compress = useCallback(async (file: File): Promise<File> => {
    setIsCompressing(true);
    setError(null);

    try {
      const compressed = await compressImage(file);
      return compressed;
    } catch (err) {
      const error = err instanceof Error ? err : new Error('Compression failed');
      setError(error);
      throw error;
    } finally {
      setIsCompressing(false);
    }
  }, []);

  return {
    compress,
    isCompressing,
    error,
  };
}

// ============================================================================
// Hook: useImageValidation - Validate files before upload
// ============================================================================

export function useImageValidation() {
  const [validationErrors, setValidationErrors] = useState<string[]>([]);

  const validate = useCallback((files: File[]): boolean => {
    const errors: string[] = [];

    files.forEach((file, index) => {
      if (!isValidImageFile(file)) {
        errors.push(`File ${index + 1} (${file.name}): Invalid file type`);
      }
      if (!isValidFileSize(file)) {
        errors.push(`File ${index + 1} (${file.name}): File too large (max 10MB)`);
      }
    });

    setValidationErrors(errors);
    return errors.length === 0;
  }, []);

  const validateSingle = useCallback((file: File): boolean => {
    return validate([file]);
  }, [validate]);

  const clearErrors = useCallback(() => {
    setValidationErrors([]);
  }, []);

  return {
    validate,
    validateSingle,
    validationErrors,
    hasErrors: validationErrors.length > 0,
    clearErrors,
    isValidImageFile,
    isValidFileSize,
    validateImageFiles,
  };
}

// ============================================================================
// Composite Hook: useMediaUpload - All-in-one media upload functionality
// ============================================================================

export function useMediaUpload(folder: string = 'vehicles') {
  const singleUpload = useUploadImage({ folder });
  const multiUpload = useUploadMultipleImages({ folder });
  const profileUpload = useUploadProfilePicture();
  const deleteImg = useDeleteImage();
  const compression = useImageCompression();
  const validation = useImageValidation();

  return {
    // Single upload
    uploadImage: singleUpload.upload,
    uploadImageAsync: singleUpload.uploadAsync,
    singleProgress: singleUpload.progress,
    singleData: singleUpload.data,

    // Multiple upload
    uploadMultiple: multiUpload.upload,
    uploadMultipleAsync: multiUpload.uploadAsync,
    multiProgress: multiUpload.multiProgress,
    overallProgress: multiUpload.overallProgress,
    multiData: multiUpload.data,

    // Profile picture
    uploadProfile: profileUpload.upload,
    uploadProfileAsync: profileUpload.uploadAsync,
    profileProgress: profileUpload.progress,
    profileData: profileUpload.data,

    // Delete
    deleteImage: deleteImg.deleteImage,
    deleteImageAsync: deleteImg.deleteImageAsync,

    // Status
    isUploading: singleUpload.isUploading || multiUpload.isUploading || profileUpload.isUploading,
    isDeleting: deleteImg.isDeleting,
    isCompressing: compression.isCompressing,

    // Errors
    error: singleUpload.error || multiUpload.error || profileUpload.error || deleteImg.error,

    // Utilities
    compress: compression.compress,
    validate: validation.validate,
    validateSingle: validation.validateSingle,
    validationErrors: validation.validationErrors,
    clearValidationErrors: validation.clearErrors,

    // Reset
    reset: () => {
      singleUpload.reset();
      multiUpload.reset();
      profileUpload.reset();
      validation.clearErrors();
    },
  };
}

// Default export
export default useMediaUpload;
