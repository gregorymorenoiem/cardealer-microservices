'use client';

import { useState, useMemo } from 'react';
import Image from 'next/image';
import {
  Star,
  X,
  Crop,
  ZoomIn,
  AlertTriangle,
  CheckCircle2,
  Loader2,
  RotateCcw,
  GripVertical,
} from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

export type PhotoStatus =
  | 'pending'
  | 'compressing'
  | 'uploading'
  | 'uploaded'
  | 'processing'
  | 'processed'
  | 'error';

export interface PhotoItem {
  id: string;
  file: File;
  url: string; // local object URL or remote URL
  order: number;
  isPrimary: boolean;
  alt?: string;
  status: PhotoStatus;
  progress: number; // 0-100
  qualityScore?: number; // 0-100
  mediaId?: string; // backend media asset id
  thumbnailUrl?: string;
  errorMessage?: string;
  dimensions?: { width: number; height: number };
  compressedSize?: number;
  originalSize?: number;
}

interface PhotoCardProps {
  photo: PhotoItem;
  onSetPrimary: (id: string) => void;
  onRemove: (id: string) => void;
  onCrop: (id: string) => void;
  onView: (id: string) => void;
  onRetry?: (id: string) => void;
  isDragging?: boolean;
  dragHandleProps?: Record<string, unknown>;
}

// ============================================================
// HELPERS
// ============================================================

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes}B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)}KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)}MB`;
}

function getQualityColor(score: number): string {
  if (score >= 80) return 'text-emerald-600 bg-emerald-50';
  if (score >= 60) return 'text-amber-600 bg-amber-50';
  return 'text-red-600 bg-red-50';
}

function getStatusLabel(status: PhotoStatus): string {
  switch (status) {
    case 'pending':
      return 'Pendiente';
    case 'compressing':
      return 'Comprimiendo...';
    case 'uploading':
      return 'Subiendo...';
    case 'uploaded':
      return 'Subido';
    case 'processing':
      return 'Procesando...';
    case 'processed':
      return 'Listo';
    case 'error':
      return 'Error';
  }
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoCard({
  photo,
  onSetPrimary,
  onRemove,
  onCrop,
  onView,
  onRetry,
  isDragging = false,
  dragHandleProps = {},
}: PhotoCardProps) {
  const [imageError, setImageError] = useState(false);

  const isProcessing = ['compressing', 'uploading', 'processing'].includes(photo.status);
  const isError = photo.status === 'error';
  const isReady = photo.status === 'processed' || photo.status === 'uploaded';

  const compressionSavings = useMemo(() => {
    if (!photo.originalSize || !photo.compressedSize) return null;
    const saved = photo.originalSize - photo.compressedSize;
    const pct = Math.round((saved / photo.originalSize) * 100);
    return pct > 0 ? `−${pct}%` : null;
  }, [photo.originalSize, photo.compressedSize]);

  return (
    <div
      className={`group relative overflow-hidden rounded-xl border bg-white transition-all ${
        isDragging ? 'z-50 rotate-2 shadow-xl ring-2 ring-emerald-400' : ''
      } ${photo.isPrimary ? 'ring-2 ring-emerald-500' : 'hover:shadow-md'} ${
        isError ? 'border-red-300' : 'border-gray-200'
      }`}
    >
      {/* Primary badge */}
      {photo.isPrimary && (
        <div className="absolute top-2 left-2 z-10 flex items-center gap-1 rounded-full bg-emerald-500 px-2 py-0.5 text-xs font-semibold text-white shadow">
          <Star className="h-3 w-3" fill="currentColor" />
          Principal
        </div>
      )}

      {/* Drag handle */}
      <div
        {...dragHandleProps}
        className="absolute top-2 right-2 z-10 cursor-grab rounded bg-black/30 p-1 opacity-0 backdrop-blur-sm transition-opacity group-hover:opacity-100"
      >
        <GripVertical className="h-4 w-4 text-white" />
      </div>

      {/* Image */}
      <div className="relative aspect-[4/3] w-full overflow-hidden bg-gray-100">
        {imageError ? (
          <div className="flex h-full items-center justify-center text-gray-400">
            <AlertTriangle className="h-8 w-8" />
          </div>
        ) : (
          <Image
            src={photo.thumbnailUrl || photo.url}
            alt={photo.alt || `Foto ${photo.order + 1}`}
            fill
            className={`object-cover transition-all ${isProcessing ? 'blur-sm' : ''}`}
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
            onError={() => setImageError(true)}
            unoptimized
          />
        )}

        {/* Processing overlay */}
        {isProcessing && (
          <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/40">
            <Loader2 className="h-6 w-6 animate-spin text-white" />
            <span className="mt-1 text-xs font-medium text-white">
              {getStatusLabel(photo.status)}
            </span>
            {photo.progress > 0 && photo.progress < 100 && (
              <div className="mt-2 h-1 w-3/4 overflow-hidden rounded-full bg-white/30">
                <div
                  className="h-full rounded-full bg-white transition-all duration-300"
                  style={{ width: `${photo.progress}%` }}
                />
              </div>
            )}
          </div>
        )}

        {/* Error overlay */}
        {isError && (
          <div className="absolute inset-0 flex flex-col items-center justify-center bg-red-900/60">
            <AlertTriangle className="h-6 w-6 text-red-200" />
            <span className="mt-1 max-w-[90%] truncate text-xs text-red-100">
              {photo.errorMessage || 'Error al subir'}
            </span>
            {onRetry && (
              <button
                type="button"
                onClick={e => {
                  e.stopPropagation();
                  onRetry(photo.id);
                }}
                className="mt-2 flex items-center gap-1 rounded-md bg-white/90 px-2 py-1 text-xs font-medium text-red-700 hover:bg-white"
              >
                <RotateCcw className="h-3 w-3" />
                Reintentar
              </button>
            )}
          </div>
        )}

        {/* Processed check */}
        {isReady && !isProcessing && (
          <div className="absolute right-2 bottom-2">
            <CheckCircle2 className="h-5 w-5 text-emerald-400 drop-shadow" />
          </div>
        )}

        {/* Quality badge */}
        {photo.qualityScore !== undefined && (
          <div
            className={`absolute bottom-2 left-2 rounded-full px-1.5 py-0.5 text-[10px] font-bold ${getQualityColor(photo.qualityScore)}`}
          >
            {photo.qualityScore}
          </div>
        )}

        {/* Compression badge */}
        {compressionSavings && (
          <div className="absolute right-2 bottom-2 rounded-full bg-blue-50 px-1.5 py-0.5 text-[10px] font-medium text-blue-600">
            {compressionSavings}
          </div>
        )}
      </div>

      {/* Actions bar */}
      <div className="flex items-center justify-between border-t border-gray-100 px-2 py-1.5">
        <div className="flex items-center gap-1 text-xs text-gray-500">
          <span>#{photo.order + 1}</span>
          {photo.dimensions && (
            <span className="hidden sm:inline">
              · {photo.dimensions.width}×{photo.dimensions.height}
            </span>
          )}
          {photo.compressedSize && (
            <span className="hidden sm:inline">· {formatFileSize(photo.compressedSize)}</span>
          )}
        </div>

        <div className="flex items-center gap-0.5">
          {!photo.isPrimary && isReady && (
            <button
              type="button"
              title="Establecer como principal"
              onClick={() => onSetPrimary(photo.id)}
              className="rounded p-1 text-gray-400 transition-colors hover:bg-amber-50 hover:text-amber-500"
            >
              <Star className="h-3.5 w-3.5" />
            </button>
          )}
          {isReady && (
            <button
              type="button"
              title="Recortar"
              onClick={() => onCrop(photo.id)}
              className="rounded p-1 text-gray-400 transition-colors hover:bg-blue-50 hover:text-blue-500"
            >
              <Crop className="h-3.5 w-3.5" />
            </button>
          )}
          <button
            type="button"
            title="Ver ampliada"
            onClick={() => onView(photo.id)}
            className="rounded p-1 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
          >
            <ZoomIn className="h-3.5 w-3.5" />
          </button>
          <button
            type="button"
            title="Eliminar"
            onClick={() => onRemove(photo.id)}
            className="rounded p-1 text-gray-400 transition-colors hover:bg-red-50 hover:text-red-500"
          >
            <X className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>
    </div>
  );
}
