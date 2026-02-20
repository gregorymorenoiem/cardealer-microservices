/**
 * Seller Wizard - Step 3: Vehicle Photo Uploader
 *
 * Drag-and-drop image uploader with:
 * - Preview grid with reorder
 * - Progress indicators per image
 * - Client-side validation (type, size)
 * - Server upload via MediaService
 * - Primary image selection
 * - Min 3, max 10 images
 */

'use client';

import * as React from 'react';
import { Camera, X, Star, Loader2, AlertCircle, ImagePlus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { IMAGE_CONSTRAINTS, validateVehicleImage } from '@/lib/validations/seller-onboarding';
import { uploadVehicleImage, type UploadProgress } from '@/services/media';

// =============================================================================
// TYPES
// =============================================================================

export interface UploadedImage {
  id: string;
  url: string;
  thumbnailUrl?: string;
  file?: File;
  previewUrl?: string;
  order: number;
  isPrimary: boolean;
  status: 'pending' | 'uploading' | 'done' | 'error';
  progress: number;
  error?: string;
}

interface PhotoUploaderProps {
  images: UploadedImage[];
  onImagesChange: (images: UploadedImage[]) => void;
  disabled?: boolean;
}

// =============================================================================
// COMPONENT
// =============================================================================

export function PhotoUploader({ images, onImagesChange, disabled }: PhotoUploaderProps) {
  const fileInputRef = React.useRef<HTMLInputElement>(null);
  const [isDragOver, setIsDragOver] = React.useState(false);

  const canAddMore = images.length < IMAGE_CONSTRAINTS.maxImages;
  const hasMinImages =
    images.filter(i => i.status === 'done').length >= IMAGE_CONSTRAINTS.minImages;

  // ── File selection ──

  const handleFileSelect = (files: FileList | null) => {
    if (!files || !canAddMore) return;

    const remainingSlots = IMAGE_CONSTRAINTS.maxImages - images.length;
    const filesToAdd = Array.from(files).slice(0, remainingSlots);
    const newImages: UploadedImage[] = [];

    for (const file of filesToAdd) {
      const validation = validateVehicleImage(file);
      if (!validation.valid) {
        // Add as error
        newImages.push({
          id: crypto.randomUUID(),
          url: '',
          file,
          previewUrl: URL.createObjectURL(file),
          order: images.length + newImages.length,
          isPrimary: images.length === 0 && newImages.length === 0,
          status: 'error',
          progress: 0,
          error: validation.error,
        });
        continue;
      }

      newImages.push({
        id: crypto.randomUUID(),
        url: '',
        file,
        previewUrl: URL.createObjectURL(file),
        order: images.length + newImages.length,
        isPrimary: images.length === 0 && newImages.length === 0,
        status: 'pending',
        progress: 0,
      });
    }

    const updated = [...images, ...newImages];
    onImagesChange(updated);

    // Auto-upload pending images
    for (const img of newImages) {
      if (img.status === 'pending' && img.file) {
        uploadImage(img, updated);
      }
    }
  };

  const uploadImage = async (image: UploadedImage, currentImages: UploadedImage[]) => {
    if (!image.file) return;

    // Mark as uploading
    const updateStatus = (imgs: UploadedImage[], id: string, patch: Partial<UploadedImage>) => {
      return imgs.map(i => (i.id === id ? { ...i, ...patch } : i));
    };

    let latest = updateStatus(currentImages, image.id, { status: 'uploading' as const });
    onImagesChange(latest);

    try {
      const result = await uploadVehicleImage(
        {
          file: image.file,
          sortOrder: image.order,
          isPrimary: image.isPrimary,
          compress: true,
        },
        (progress: UploadProgress) => {
          latest = updateStatus(latest, image.id, { progress: progress.percentage });
          onImagesChange([...latest]);
        }
      );

      latest = updateStatus(latest, image.id, {
        status: 'done' as const,
        progress: 100,
        url: result.originalUrl,
        thumbnailUrl: result.thumbnailUrl,
        id: result.mediaId,
      });
      onImagesChange([...latest]);
    } catch (err) {
      const error = err as { message?: string };
      latest = updateStatus(latest, image.id, {
        status: 'error' as const,
        error: error.message || 'Error al subir la imagen',
      });
      onImagesChange([...latest]);
    }
  };

  // ── Drag & Drop ──

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    if (!disabled && canAddMore) setIsDragOver(true);
  };

  const handleDragLeave = () => setIsDragOver(false);

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    if (!disabled && canAddMore) {
      handleFileSelect(e.dataTransfer.files);
    }
  };

  // ── Actions ──

  const removeImage = (id: string) => {
    const updated = images.filter(i => i.id !== id).map((i, idx) => ({ ...i, order: idx }));

    // Clean up preview URL
    const removed = images.find(i => i.id === id);
    if (removed?.previewUrl) URL.revokeObjectURL(removed.previewUrl);

    // If removed was primary, make first one primary
    if (removed?.isPrimary && updated.length > 0) {
      updated[0].isPrimary = true;
    }

    onImagesChange(updated);
  };

  const setPrimary = (id: string) => {
    const updated = images.map(i => ({
      ...i,
      isPrimary: i.id === id,
    }));
    onImagesChange(updated);
  };

  const retryUpload = (id: string) => {
    const image = images.find(i => i.id === id);
    if (image && image.file) {
      uploadImage({ ...image, status: 'pending', progress: 0, error: undefined }, images);
    }
  };

  // Cleanup on unmount
  React.useEffect(() => {
    return () => {
      images.forEach(img => {
        if (img.previewUrl) URL.revokeObjectURL(img.previewUrl);
      });
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="space-y-4">
      {/* Drop zone */}
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => !disabled && canAddMore && fileInputRef.current?.click()}
        className={cn(
          'flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 text-center transition-colors',
          isDragOver && 'border-[#00A870] bg-[#00A870]/5',
          !canAddMore && 'cursor-not-allowed opacity-50',
          disabled && 'cursor-not-allowed opacity-50',
          !isDragOver &&
            canAddMore &&
            !disabled &&
            'border-muted-foreground/30 hover:border-[#00A870]/50'
        )}
      >
        <Camera className="text-muted-foreground mb-3 h-10 w-10" />
        <p className="text-sm font-medium">
          {canAddMore
            ? 'Arrastra tus fotos aquí o haz clic para seleccionar'
            : 'Máximo de fotos alcanzado'}
        </p>
        <p className="text-muted-foreground mt-1 text-xs">
          JPG, PNG o WebP • Máximo {IMAGE_CONSTRAINTS.maxSizeMB}MB por imagen •{' '}
          {IMAGE_CONSTRAINTS.minImages}-{IMAGE_CONSTRAINTS.maxImages} fotos
        </p>
      </div>

      <input
        ref={fileInputRef}
        type="file"
        accept={IMAGE_CONSTRAINTS.allowedTypes.join(',')}
        multiple
        onChange={e => handleFileSelect(e.target.files)}
        className="hidden"
      />

      {/* Image count indicator */}
      <div className="flex items-center justify-between">
        <p className={cn('text-sm', hasMinImages ? 'text-muted-foreground' : 'text-amber-600')}>
          {images.filter(i => i.status === 'done').length} de mínimo {IMAGE_CONSTRAINTS.minImages}{' '}
          fotos
          {!hasMinImages && (
            <span className="ml-1 font-medium">
              (faltan {IMAGE_CONSTRAINTS.minImages - images.filter(i => i.status === 'done').length}
              )
            </span>
          )}
        </p>
        {canAddMore && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => fileInputRef.current?.click()}
            disabled={disabled}
          >
            <ImagePlus className="mr-1 h-4 w-4" />
            Agregar más
          </Button>
        )}
      </div>

      {/* Image grid */}
      {images.length > 0 && (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
          {images.map(image => (
            <div
              key={image.id}
              className={cn(
                'group relative aspect-[4/3] overflow-hidden rounded-lg border',
                image.isPrimary && 'ring-2 ring-[#00A870] ring-offset-2',
                image.status === 'error' && 'border-red-300'
              )}
            >
              {/* Preview */}
              {(image.previewUrl || image.thumbnailUrl || image.url) && (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={image.thumbnailUrl || image.previewUrl || image.url}
                  alt={`Foto ${image.order + 1}`}
                  className="h-full w-full object-cover"
                />
              )}

              {/* Upload progress overlay */}
              {image.status === 'uploading' && (
                <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/50">
                  <Loader2 className="h-6 w-6 animate-spin text-white" />
                  <span className="mt-1 text-xs font-medium text-white">{image.progress}%</span>
                </div>
              )}

              {/* Error overlay */}
              {image.status === 'error' && (
                <div className="absolute inset-0 flex flex-col items-center justify-center bg-red-900/60 p-2">
                  <AlertCircle className="h-5 w-5 text-red-200" />
                  <p className="mt-1 text-center text-[10px] text-red-100">{image.error}</p>
                  <Button
                    type="button"
                    size="sm"
                    variant="ghost"
                    className="mt-1 h-6 text-xs text-white hover:text-white"
                    onClick={() => retryUpload(image.id)}
                  >
                    Reintentar
                  </Button>
                </div>
              )}

              {/* Actions overlay (hover) */}
              {image.status === 'done' && (
                <div className="absolute inset-0 flex items-start justify-between bg-gradient-to-b from-black/40 to-transparent p-1.5 opacity-0 transition-opacity group-hover:opacity-100">
                  {/* Primary badge / button */}
                  {image.isPrimary ? (
                    <span className="flex items-center gap-1 rounded bg-[#00A870] px-1.5 py-0.5 text-[10px] font-medium text-white">
                      <Star className="h-3 w-3" />
                      Principal
                    </span>
                  ) : (
                    <button
                      type="button"
                      onClick={() => setPrimary(image.id)}
                      className="flex items-center gap-1 rounded bg-white/80 px-1.5 py-0.5 text-[10px] font-medium text-gray-700 hover:bg-white"
                    >
                      <Star className="h-3 w-3" />
                      Hacer principal
                    </button>
                  )}

                  {/* Remove button */}
                  <button
                    type="button"
                    onClick={() => removeImage(image.id)}
                    className="rounded-full bg-red-500 p-1 text-white hover:bg-red-600"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </div>
              )}

              {/* Primary indicator */}
              {image.isPrimary && image.status === 'done' && (
                <div className="absolute right-0 bottom-0 left-0 bg-[#00A870]/90 px-2 py-0.5 text-center text-[10px] font-medium text-white group-hover:opacity-0">
                  Foto principal
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
