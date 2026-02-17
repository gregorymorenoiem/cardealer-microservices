'use client';

import { useState, useCallback, useRef } from 'react';
import { PhotoGuide } from './photo-guide';
import type { UploadedImage } from './smart-publish-wizard';
import { sanitizeFilename } from '@/lib/security/sanitize';
import { Upload, X, Star, GripVertical, ImagePlus, AlertCircle, Loader2 } from 'lucide-react';
import { toast } from 'sonner';

// ============================================================
// Constants
// ============================================================

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MIN_PHOTOS_INDIVIDUAL = 3;
const MAX_PHOTOS_INDIVIDUAL = 20;
const MIN_PHOTOS_DEALER = 5;
const MAX_PHOTOS_DEALER = 50;

// ============================================================
// Component
// ============================================================

interface PhotoUploadStepProps {
  images: UploadedImage[];
  onChange: (images: UploadedImage[]) => void;
  mode: 'individual' | 'dealer';
}

export function PhotoUploadStep({ images, onChange, mode }: PhotoUploadStepProps) {
  const [isDragOver, setIsDragOver] = useState(false);
  const [uploadingCount, setUploadingCount] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const dragItem = useRef<number | null>(null);
  const dragOverItem = useRef<number | null>(null);

  const minPhotos = mode === 'dealer' ? MIN_PHOTOS_DEALER : MIN_PHOTOS_INDIVIDUAL;
  const maxPhotos = mode === 'dealer' ? MAX_PHOTOS_DEALER : MAX_PHOTOS_INDIVIDUAL;

  // Handle file validation and upload
  const processFiles = useCallback(
    async (files: FileList | File[]) => {
      const fileArray = Array.from(files);
      const remaining = maxPhotos - images.length;

      if (remaining <= 0) {
        toast.error(`Máximo ${maxPhotos} fotos permitidas`);
        return;
      }

      const validFiles = fileArray.slice(0, remaining).filter(file => {
        if (!ACCEPTED_TYPES.includes(file.type)) {
          toast.error(`${file.name}: Formato no soportado. Usa JPG, PNG o WebP`);
          return false;
        }
        if (file.size > MAX_FILE_SIZE) {
          toast.error(`${file.name}: Archivo demasiado grande (máximo 10MB)`);
          return false;
        }
        return true;
      });

      if (validFiles.length === 0) return;

      setUploadingCount(validFiles.length);

      // Create preview URLs (in a real app, this would upload to MediaService)
      const newImages: UploadedImage[] = validFiles.map((file, index) => ({
        id: `img_${Date.now()}_${index}`,
        file,
        url: URL.createObjectURL(file),
        order: images.length + index,
        isPrimary: images.length === 0 && index === 0,
        alt: sanitizeFilename(file.name.replace(/\.[^/.]+$/, '')),
      }));

      // Simulate brief upload delay
      await new Promise(resolve => setTimeout(resolve, 300));

      onChange([...images, ...newImages]);
      setUploadingCount(0);
      toast.success(
        `${validFiles.length} foto${validFiles.length > 1 ? 's' : ''} agregada${validFiles.length > 1 ? 's' : ''}`
      );
    },
    [images, maxPhotos, onChange]
  );

  // Drag & drop handlers
  const handleDragEnter = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragOver(false);
      if (e.dataTransfer.files) {
        processFiles(e.dataTransfer.files);
      }
    },
    [processFiles]
  );

  const handleFileChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      if (e.target.files) {
        processFiles(e.target.files);
        e.target.value = '';
      }
    },
    [processFiles]
  );

  // Image management
  const handleRemove = useCallback(
    (id: string) => {
      const updated = images.filter(img => img.id !== id);
      // If primary was removed, set first as primary
      if (updated.length > 0 && !updated.some(img => img.isPrimary)) {
        updated[0].isPrimary = true;
      }
      onChange(updated.map((img, i) => ({ ...img, order: i })));
    },
    [images, onChange]
  );

  const handleSetPrimary = useCallback(
    (id: string) => {
      onChange(images.map(img => ({ ...img, isPrimary: img.id === id })));
    },
    [images, onChange]
  );

  // Drag reorder
  const handleDragStart = useCallback((index: number) => {
    dragItem.current = index;
  }, []);

  const handleDragOver = useCallback((e: React.DragEvent, index: number) => {
    e.preventDefault();
    dragOverItem.current = index;
  }, []);

  const handleDragEnd = useCallback(() => {
    if (dragItem.current === null || dragOverItem.current === null) return;
    const updated = [...images];
    const draggedItem = updated.splice(dragItem.current, 1)[0];
    updated.splice(dragOverItem.current, 0, draggedItem);
    onChange(updated.map((img, i) => ({ ...img, order: i })));
    dragItem.current = null;
    dragOverItem.current = null;
  }, [images, onChange]);

  const photosNeeded = Math.max(0, minPhotos - images.length);

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-xl font-bold text-gray-900">Fotos del Vehículo</h2>
        <p className="mt-1 text-sm text-gray-500">
          Mínimo {minPhotos} fotos, máximo {maxPhotos}. JPG, PNG o WebP hasta 10MB.
        </p>
      </div>

      {/* Photo Guide */}
      <PhotoGuide />

      {/* Drop Zone */}
      <div
        onDragEnter={handleDragEnter}
        onDragOver={e => e.preventDefault()}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        onClick={() => fileInputRef.current?.click()}
        className={`cursor-pointer rounded-xl border-2 border-dashed p-8 text-center transition-colors ${
          isDragOver
            ? 'border-emerald-500 bg-emerald-50'
            : 'border-gray-300 hover:border-emerald-400 hover:bg-gray-50'
        }`}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          multiple
          className="hidden"
          onChange={handleFileChange}
        />
        {uploadingCount > 0 ? (
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-10 w-10 animate-spin text-emerald-500" />
            <p className="text-sm text-gray-600">
              Procesando {uploadingCount} foto{uploadingCount > 1 ? 's' : ''}...
            </p>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-2">
            <div className="flex h-14 w-14 items-center justify-center rounded-full bg-gray-100">
              <Upload className="h-6 w-6 text-gray-400" />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-700">
                Arrastra fotos aquí o{' '}
                <span className="text-emerald-600">haz clic para seleccionar</span>
              </p>
              <p className="mt-1 text-xs text-gray-500">
                {images.length}/{maxPhotos} fotos ·{' '}
                {photosNeeded > 0
                  ? `Faltan ${photosNeeded} mínimo`
                  : 'Requisito mínimo completado ✓'}
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Minimum photos warning */}
      {photosNeeded > 0 && images.length > 0 && (
        <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-700">
          <AlertCircle className="h-4 w-4 flex-shrink-0" />
          <p>
            Necesitas al menos {photosNeeded} foto{photosNeeded > 1 ? 's' : ''} más para publicar
          </p>
        </div>
      )}

      {/* Image Grid */}
      {images.length > 0 && (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {images.map((img, index) => (
            <div
              key={img.id}
              draggable
              onDragStart={() => handleDragStart(index)}
              onDragOver={e => handleDragOver(e, index)}
              onDragEnd={handleDragEnd}
              className={`group relative aspect-[4/3] overflow-hidden rounded-xl border-2 transition-all ${
                img.isPrimary ? 'border-emerald-500 ring-2 ring-emerald-200' : 'border-gray-200'
              }`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={img.url} alt={`Foto ${index + 1}`} className="h-full w-full object-cover" />

              {/* Overlay controls */}
              <div className="absolute inset-0 flex items-start justify-between bg-gradient-to-b from-black/40 via-transparent to-transparent p-2 opacity-0 transition-opacity group-hover:opacity-100">
                <button
                  type="button"
                  onClick={e => {
                    e.stopPropagation();
                    handleSetPrimary(img.id);
                  }}
                  className={`rounded-full p-1.5 ${
                    img.isPrimary
                      ? 'bg-emerald-500 text-white'
                      : 'bg-black/30 text-white hover:bg-emerald-500'
                  }`}
                  title={img.isPrimary ? 'Foto principal' : 'Establecer como principal'}
                >
                  <Star className="h-3.5 w-3.5" fill={img.isPrimary ? 'currentColor' : 'none'} />
                </button>
                <button
                  type="button"
                  onClick={e => {
                    e.stopPropagation();
                    handleRemove(img.id);
                  }}
                  className="rounded-full bg-red-500/80 p-1.5 text-white hover:bg-red-600"
                  title="Eliminar"
                >
                  <X className="h-3.5 w-3.5" />
                </button>
              </div>

              {/* Drag handle */}
              <div className="absolute bottom-2 left-2 opacity-0 transition-opacity group-hover:opacity-100">
                <div className="rounded bg-black/30 p-1">
                  <GripVertical className="h-3.5 w-3.5 text-white" />
                </div>
              </div>

              {/* Primary badge */}
              {img.isPrimary && (
                <div className="absolute right-2 bottom-2">
                  <span className="rounded-full bg-emerald-500 px-2 py-0.5 text-[10px] font-bold text-white">
                    PRINCIPAL
                  </span>
                </div>
              )}

              {/* Order number */}
              <div className="absolute top-2 right-2 flex h-5 w-5 items-center justify-center rounded-full bg-black/40 text-[10px] font-bold text-white opacity-0 transition-opacity group-hover:opacity-100">
                {index + 1}
              </div>
            </div>
          ))}

          {/* Add more button */}
          {images.length < maxPhotos && (
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              className="flex aspect-[4/3] flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed border-gray-300 text-gray-400 transition-colors hover:border-emerald-400 hover:text-emerald-500"
            >
              <ImagePlus className="h-8 w-8" />
              <span className="text-xs font-medium">Agregar</span>
            </button>
          )}
        </div>
      )}
    </div>
  );
}
