import { useCallback, useState, type DragEvent, type ChangeEvent } from 'react';
import { useUploadMultipleImages, useImageValidation } from '@/hooks/useMediaUpload';
import { createThumbnailPreview } from '@/services/uploadService';
import type { UploadResponse } from '@/services/uploadService';

// ============================================================================
// Types
// ============================================================================

export interface ImagePreview {
  id: string;
  file: File;
  preview: string;
  progress: number;
  status: 'pending' | 'uploading' | 'success' | 'error';
  error?: string;
  uploadResult?: UploadResponse;
}

export interface ImageDropZoneProps {
  /** Maximum number of files allowed */
  maxFiles?: number;
  /** Maximum file size in MB */
  maxSizeMB?: number;
  /** Upload folder destination */
  folder?: string;
  /** Whether to auto-upload on drop */
  autoUpload?: boolean;
  /** Callback when files are uploaded successfully */
  onUploadComplete?: (results: UploadResponse[]) => void;
  /** Callback when upload fails */
  onUploadError?: (error: Error) => void;
  /** Callback when files are selected (before upload) */
  onFilesSelected?: (files: File[]) => void;
  /** Custom class name */
  className?: string;
  /** Disabled state */
  disabled?: boolean;
  /** Show upload button */
  showUploadButton?: boolean;
  /** Placeholder text */
  placeholder?: string;
  /** Already uploaded images (for edit mode) */
  existingImages?: { url: string; publicId: string }[];
  /** Callback when existing image is removed */
  onRemoveExisting?: (publicId: string) => void;
}

// ============================================================================
// Component
// ============================================================================

export function ImageDropZone({
  maxFiles = 10,
  maxSizeMB = 10,
  folder = 'vehicles',
  autoUpload = true,
  onUploadComplete,
  onUploadError,
  onFilesSelected,
  className = '',
  disabled = false,
  showUploadButton = true,
  placeholder = 'Drag and drop images here, or click to select',
  existingImages = [],
  onRemoveExisting,
}: ImageDropZoneProps) {
  const [isDragOver, setIsDragOver] = useState(false);
  const [previews, setPreviews] = useState<ImagePreview[]>([]);
  const [removedExisting, setRemovedExisting] = useState<string[]>([]);

  const { validate, validationErrors, clearErrors } = useImageValidation();
  
  const { 
    uploadAsync, 
    isUploading, 
    overallProgress,
    reset: resetUpload 
  } = useUploadMultipleImages({
    folder,
    onSuccess: (results) => {
      // Update previews with success status
      setPreviews((prev) =>
        prev.map((p, i) => ({
          ...p,
          status: 'success' as const,
          progress: 100,
          uploadResult: results[i],
        }))
      );
      onUploadComplete?.(results);
    },
    onError: (error) => {
      setPreviews((prev) =>
        prev.map((p) => ({
          ...p,
          status: p.status === 'uploading' ? 'error' : p.status,
          error: error.message,
        }))
      );
      onUploadError?.(error);
    },
    onFileProgress: (index, progress) => {
      setPreviews((prev) =>
        prev.map((p, i) =>
          i === index
            ? { ...p, progress: progress.percentage, status: 'uploading' as const }
            : p
        )
      );
    },
  });

  // Generate unique ID
  const generateId = () => `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

  // Handle file selection
  const handleFiles = useCallback(
    async (files: File[]) => {
      if (disabled) return;

      clearErrors();

      // Check max files limit
      const currentCount = previews.length + existingImages.length - removedExisting.length;
      const availableSlots = maxFiles - currentCount;

      if (files.length > availableSlots) {
        alert(`You can only add ${availableSlots} more image(s). Maximum is ${maxFiles}.`);
        files = files.slice(0, availableSlots);
      }

      // Validate files
      if (!validate(files)) {
        return;
      }

      // Create previews
      const newPreviews: ImagePreview[] = await Promise.all(
        files.map(async (file) => ({
          id: generateId(),
          file,
          preview: await createThumbnailPreview(file),
          progress: 0,
          status: 'pending' as const,
        }))
      );

      setPreviews((prev) => [...prev, ...newPreviews]);
      onFilesSelected?.(files);

      // Auto upload if enabled
      if (autoUpload && files.length > 0) {
        try {
          await uploadAsync(files);
        } catch {
          // Error handled in mutation callbacks
        }
      }
    },
    [
      disabled,
      previews.length,
      existingImages.length,
      removedExisting.length,
      maxFiles,
      validate,
      clearErrors,
      autoUpload,
      uploadAsync,
      onFilesSelected,
    ]
  );

  // Drag handlers
  const handleDragEnter = useCallback((e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);
  }, []);

  const handleDragOver = useCallback((e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
  }, []);

  const handleDrop = useCallback(
    (e: DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragOver(false);

      const files = Array.from(e.dataTransfer.files).filter((f) =>
        f.type.startsWith('image/')
      );
      if (files.length > 0) {
        handleFiles(files);
      }
    },
    [handleFiles]
  );

  // File input handler
  const handleInputChange = useCallback(
    (e: ChangeEvent<HTMLInputElement>) => {
      const files = e.target.files ? Array.from(e.target.files) : [];
      if (files.length > 0) {
        handleFiles(files);
      }
      // Reset input value to allow selecting same files again
      e.target.value = '';
    },
    [handleFiles]
  );

  // Remove preview
  const removePreview = useCallback((id: string) => {
    setPreviews((prev) => prev.filter((p) => p.id !== id));
  }, []);

  // Remove existing image
  const handleRemoveExisting = useCallback(
    (publicId: string) => {
      setRemovedExisting((prev) => [...prev, publicId]);
      onRemoveExisting?.(publicId);
    },
    [onRemoveExisting]
  );

  // Manual upload trigger
  const triggerUpload = useCallback(async () => {
    const pendingFiles = previews.filter((p) => p.status === 'pending').map((p) => p.file);
    if (pendingFiles.length > 0) {
      try {
        await uploadAsync(pendingFiles);
      } catch {
        // Error handled in mutation callbacks
      }
    }
  }, [previews, uploadAsync]);

  // Clear all
  const clearAll = useCallback(() => {
    setPreviews([]);
    resetUpload();
    clearErrors();
  }, [resetUpload, clearErrors]);

  // Filtered existing images
  const visibleExistingImages = existingImages.filter(
    (img) => !removedExisting.includes(img.publicId)
  );

  return (
    <div className={`w-full ${className}`}>
      {/* Drop Zone */}
      <div
        className={`
          relative border-2 border-dashed rounded-lg p-8 text-center cursor-pointer
          transition-all duration-200 ease-in-out
          ${isDragOver ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'}
          ${disabled ? 'opacity-50 cursor-not-allowed' : ''}
        `}
        onDragEnter={handleDragEnter}
        onDragLeave={handleDragLeave}
        onDragOver={handleDragOver}
        onDrop={handleDrop}
        onClick={() => !disabled && document.getElementById('file-input')?.click()}
      >
        <input
          id="file-input"
          type="file"
          multiple
          accept="image/jpeg,image/png,image/webp"
          className="hidden"
          onChange={handleInputChange}
          disabled={disabled}
        />

        <div className="flex flex-col items-center gap-2">
          {/* Upload Icon */}
          <svg
            className={`w-12 h-12 ${isDragOver ? 'text-blue-500' : 'text-gray-400'}`}
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={1.5}
              d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
            />
          </svg>

          <p className="text-gray-600">{placeholder}</p>
          <p className="text-sm text-gray-400">
            JPEG, PNG, WebP • Max {maxSizeMB}MB • Up to {maxFiles} images
          </p>
        </div>

        {/* Overall Progress Bar */}
        {isUploading && (
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gray-200 rounded-b-lg overflow-hidden">
            <div
              className="h-full bg-blue-500 transition-all duration-300"
              style={{ width: `${overallProgress}%` }}
            />
          </div>
        )}
      </div>

      {/* Validation Errors */}
      {validationErrors.length > 0 && (
        <div className="mt-2 p-3 bg-red-50 border border-red-200 rounded-lg">
          <ul className="text-sm text-red-600 list-disc list-inside">
            {validationErrors.map((error, i) => (
              <li key={i}>{error}</li>
            ))}
          </ul>
        </div>
      )}

      {/* Existing Images */}
      {visibleExistingImages.length > 0 && (
        <div className="mt-4">
          <h4 className="text-sm font-medium text-gray-700 mb-2">Current Images</h4>
          <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-3">
            {visibleExistingImages.map((img) => (
              <div key={img.publicId} className="relative aspect-square group">
                <img
                  src={img.url}
                  alt="Existing"
                  className="w-full h-full object-cover rounded-lg"
                />
                <button
                  type="button"
                  onClick={() => handleRemoveExisting(img.publicId)}
                  className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* New Previews */}
      {previews.length > 0 && (
        <div className="mt-4">
          <div className="flex justify-between items-center mb-2">
            <h4 className="text-sm font-medium text-gray-700">
              New Images ({previews.length})
            </h4>
            <button
              type="button"
              onClick={clearAll}
              className="text-sm text-red-600 hover:text-red-800"
              disabled={isUploading}
            >
              Clear All
            </button>
          </div>
          <div className="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-3">
            {previews.map((preview) => (
              <div key={preview.id} className="relative aspect-square group">
                <img
                  src={preview.preview}
                  alt="Preview"
                  className="w-full h-full object-cover rounded-lg"
                />

                {/* Progress Overlay */}
                {preview.status === 'uploading' && (
                  <div className="absolute inset-0 bg-black/50 rounded-lg flex items-center justify-center">
                    <div className="w-3/4 h-2 bg-gray-300 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-blue-500 transition-all duration-300"
                        style={{ width: `${preview.progress}%` }}
                      />
                    </div>
                  </div>
                )}

                {/* Success Indicator */}
                {preview.status === 'success' && (
                  <div className="absolute inset-0 bg-green-500/20 rounded-lg flex items-center justify-center">
                    <svg
                      className="w-8 h-8 text-green-600"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M5 13l4 4L19 7"
                      />
                    </svg>
                  </div>
                )}

                {/* Error Indicator */}
                {preview.status === 'error' && (
                  <div className="absolute inset-0 bg-red-500/20 rounded-lg flex items-center justify-center">
                    <svg
                      className="w-8 h-8 text-red-600"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M6 18L18 6M6 6l12 12"
                      />
                    </svg>
                  </div>
                )}

                {/* Remove Button */}
                <button
                  type="button"
                  onClick={() => removePreview(preview.id)}
                  className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                  disabled={preview.status === 'uploading'}
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Manual Upload Button */}
      {!autoUpload && showUploadButton && previews.some((p) => p.status === 'pending') && (
        <div className="mt-4">
          <button
            type="button"
            onClick={triggerUpload}
            disabled={isUploading}
            className={`
              w-full py-2 px-4 rounded-lg font-medium transition-colors
              ${
                isUploading
                  ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  : 'bg-blue-600 text-white hover:bg-blue-700'
              }
            `}
          >
            {isUploading ? `Uploading... ${overallProgress}%` : 'Upload Images'}
          </button>
        </div>
      )}
    </div>
  );
}

export default ImageDropZone;
