'use client';

import { useMemo } from 'react';
import { X, CheckCircle2, AlertTriangle, Loader2, Pause, Play } from 'lucide-react';
import type { PhotoItem } from './photo-card';

// ============================================================
// TYPES
// ============================================================

interface UploadProgressProps {
  photos: PhotoItem[];
  isUploading: boolean;
  isPaused: boolean;
  onPause: () => void;
  onResume: () => void;
  onCancel: () => void;
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoUploadProgress({
  photos,
  isUploading,
  isPaused,
  onPause,
  onResume,
  onCancel,
}: UploadProgressProps) {
  const stats = useMemo(() => {
    const total = photos.length;
    const completed = photos.filter(
      p => p.status === 'uploaded' || p.status === 'processed'
    ).length;
    const errors = photos.filter(p => p.status === 'error').length;
    const inProgress = photos.filter(
      p => p.status === 'compressing' || p.status === 'uploading'
    ).length;
    const pending = photos.filter(p => p.status === 'pending').length;

    // Average progress across all
    const totalProgress =
      total > 0
        ? photos.reduce((sum, p) => {
            if (p.status === 'uploaded' || p.status === 'processed') return sum + 100;
            if (p.status === 'error') return sum + 100; // count as done
            return sum + p.progress;
          }, 0) / total
        : 0;

    return { total, completed, errors, inProgress, pending, totalProgress };
  }, [photos]);

  // Only show when uploading or has active uploads
  if (!isUploading && stats.inProgress === 0 && stats.pending === 0) {
    return null;
  }

  const allDone = stats.completed + stats.errors === stats.total;

  return (
    <div className="sticky bottom-4 z-30 mx-auto max-w-lg">
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-lg">
        {/* Progress bar */}
        <div className="h-1 w-full bg-gray-100">
          <div
            className={`h-full transition-all duration-500 ${
              stats.errors > 0 ? 'bg-amber-500' : 'bg-emerald-500'
            }`}
            style={{ width: `${stats.totalProgress}%` }}
          />
        </div>

        <div className="flex items-center gap-3 px-4 py-3">
          {/* Status icon */}
          {allDone ? (
            stats.errors > 0 ? (
              <AlertTriangle className="h-5 w-5 flex-shrink-0 text-amber-500" />
            ) : (
              <CheckCircle2 className="h-5 w-5 flex-shrink-0 text-emerald-500" />
            )
          ) : (
            <Loader2 className="h-5 w-5 flex-shrink-0 animate-spin text-emerald-500" />
          )}

          {/* Text */}
          <div className="min-w-0 flex-1">
            <p className="text-sm font-medium text-gray-900">
              {allDone
                ? stats.errors > 0
                  ? `${stats.completed} de ${stats.total} fotos subidas (${stats.errors} con error)`
                  : `${stats.completed} fotos subidas correctamente`
                : isPaused
                  ? 'Subida pausada'
                  : `Subiendo ${stats.completed}/${stats.total} fotos...`}
            </p>
            {!allDone && (
              <p className="text-xs text-gray-500">
                {stats.inProgress > 0 && `${stats.inProgress} en progreso`}
                {stats.pending > 0 && ` · ${stats.pending} en cola`}
              </p>
            )}
          </div>

          {/* Actions */}
          <div className="flex items-center gap-1">
            {!allDone && (
              <>
                <button
                  type="button"
                  onClick={isPaused ? onResume : onPause}
                  className="rounded-lg p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
                  title={isPaused ? 'Reanudar' : 'Pausar'}
                >
                  {isPaused ? <Play className="h-4 w-4" /> : <Pause className="h-4 w-4" />}
                </button>
                <button
                  type="button"
                  onClick={onCancel}
                  className="rounded-lg p-1.5 text-gray-400 hover:bg-red-50 hover:text-red-500"
                  title="Cancelar"
                >
                  <X className="h-4 w-4" />
                </button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
