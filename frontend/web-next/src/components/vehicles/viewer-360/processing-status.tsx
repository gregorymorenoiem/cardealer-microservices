'use client';

import { Loader2, CheckCircle2, AlertTriangle, Clock, RotateCcw } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

type ProcessingStatus =
  | 'Pending'
  | 'Uploading'
  | 'Uploaded'
  | 'Queued'
  | 'Extracting'
  | 'Processing'
  | 'Enhancing'
  | 'Completed'
  | 'Failed'
  | 'Cancelled';

interface ProcessingStatusProps {
  status: ProcessingStatus;
  progress?: number;
  framesExtracted?: number;
  totalFrames?: number;
  errorMessage?: string;
  onRetry?: () => void;
  onCancel?: () => void;
  className?: string;
}

// ============================================================
// HELPERS
// ============================================================

const STATUS_CONFIG: Record<
  ProcessingStatus,
  { label: string; icon: typeof Loader2; color: string; bg: string }
> = {
  Pending: {
    label: 'En cola...',
    icon: Clock,
    color: 'text-gray-500',
    bg: 'bg-gray-50',
  },
  Uploading: {
    label: 'Subiendo video...',
    icon: Loader2,
    color: 'text-blue-500',
    bg: 'bg-blue-50',
  },
  Uploaded: {
    label: 'Video subido, esperando procesamiento...',
    icon: Clock,
    color: 'text-blue-500',
    bg: 'bg-blue-50',
  },
  Queued: {
    label: 'En cola de procesamiento...',
    icon: Clock,
    color: 'text-amber-500',
    bg: 'bg-amber-50',
  },
  Extracting: {
    label: 'Extrayendo frames del video...',
    icon: Loader2,
    color: 'text-purple-500',
    bg: 'bg-purple-50',
  },
  Processing: {
    label: 'Procesando frames...',
    icon: Loader2,
    color: 'text-emerald-500',
    bg: 'bg-emerald-50',
  },
  Enhancing: {
    label: 'Mejorando calidad de imágenes...',
    icon: Loader2,
    color: 'text-emerald-500',
    bg: 'bg-emerald-50',
  },
  Completed: {
    label: 'Vista 360° lista',
    icon: CheckCircle2,
    color: 'text-emerald-600',
    bg: 'bg-emerald-50',
  },
  Failed: {
    label: 'Error en procesamiento',
    icon: AlertTriangle,
    color: 'text-red-500',
    bg: 'bg-red-50',
  },
  Cancelled: {
    label: 'Procesamiento cancelado',
    icon: AlertTriangle,
    color: 'text-gray-500',
    bg: 'bg-gray-50',
  },
};

// ============================================================
// COMPONENT
// ============================================================

export function Viewer360ProcessingStatus({
  status,
  progress = 0,
  framesExtracted,
  totalFrames,
  errorMessage,
  onRetry,
  onCancel,
  className = '',
}: ProcessingStatusProps) {
  const config = STATUS_CONFIG[status];
  const Icon = config.icon;
  const isAnimated = ['Uploading', 'Extracting', 'Processing', 'Enhancing'].includes(status);
  const isTerminal = ['Completed', 'Failed', 'Cancelled'].includes(status);

  return (
    <div className={`rounded-xl border p-4 ${config.bg} ${className}`}>
      {/* Status header */}
      <div className="flex items-center gap-3">
        <Icon className={`h-5 w-5 ${config.color} ${isAnimated ? 'animate-spin' : ''}`} />
        <div className="flex-1">
          <p className={`text-sm font-semibold ${config.color}`}>{config.label}</p>
          {framesExtracted !== undefined && totalFrames !== undefined && (
            <p className="text-xs text-gray-500">
              {framesExtracted}/{totalFrames} frames procesados
            </p>
          )}
          {errorMessage && status === 'Failed' && (
            <p className="mt-0.5 text-xs text-red-500">{errorMessage}</p>
          )}
        </div>
      </div>

      {/* Progress bar */}
      {!isTerminal && progress > 0 && (
        <div className="mt-3 h-2 w-full overflow-hidden rounded-full bg-gray-200">
          <div
            className={`h-full rounded-full transition-all duration-500 ${
              status === 'Failed' ? 'bg-red-500' : 'bg-emerald-500'
            }`}
            style={{ width: `${Math.min(progress, 100)}%` }}
          />
        </div>
      )}

      {/* Processing steps */}
      {!isTerminal && (
        <div className="mt-3 flex items-center gap-2">
          {['Uploaded', 'Extracting', 'Processing', 'Enhancing', 'Completed'].map((step, i) => {
            const stepIndex = ['Uploaded', 'Extracting', 'Processing', 'Enhancing', 'Completed'].indexOf(status);
            const currentIndex = i;
            const isDone = currentIndex < stepIndex;
            const isCurrent = currentIndex === stepIndex;

            return (
              <div key={step} className="flex items-center gap-2">
                {i > 0 && (
                  <div className={`h-px w-4 ${isDone ? 'bg-emerald-400' : 'bg-gray-300'}`} />
                )}
                <div
                  className={`h-2 w-2 rounded-full ${
                    isDone
                      ? 'bg-emerald-500'
                      : isCurrent
                        ? 'animate-pulse bg-emerald-400'
                        : 'bg-gray-300'
                  }`}
                />
              </div>
            );
          })}
        </div>
      )}

      {/* Actions */}
      {(status === 'Failed' || status === 'Cancelled') && (
        <div className="mt-3 flex gap-2">
          {onRetry && (
            <button
              type="button"
              onClick={onRetry}
              className="flex items-center gap-1 rounded-lg bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700"
            >
              <RotateCcw className="h-3 w-3" />
              Reintentar
            </button>
          )}
        </div>
      )}

      {!isTerminal && onCancel && (
        <div className="mt-3">
          <button
            type="button"
            onClick={onCancel}
            className="text-xs text-gray-500 hover:text-red-500"
          >
            Cancelar procesamiento
          </button>
        </div>
      )}
    </div>
  );
}
