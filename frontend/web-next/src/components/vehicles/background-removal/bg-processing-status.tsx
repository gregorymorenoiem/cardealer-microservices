'use client';

import { Loader2, CheckCircle2, AlertTriangle, Clock } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

type RemovalStatus = 'pending' | 'processing' | 'completed' | 'failed';

interface BgProcessingStatusProps {
  status: RemovalStatus;
  progress?: number;
  processedCount?: number;
  totalCount?: number;
  errorMessage?: string;
  className?: string;
}

// ============================================================
// COMPONENT
// ============================================================

export function BgProcessingStatus({
  status,
  progress = 0,
  processedCount,
  totalCount,
  errorMessage,
  className = '',
}: BgProcessingStatusProps) {
  const config: Record<RemovalStatus, { label: string; color: string; bg: string }> = {
    pending: { label: 'En cola...', color: 'text-gray-500', bg: 'bg-gray-50' },
    processing: { label: 'Eliminando fondo...', color: 'text-purple-600', bg: 'bg-purple-50' },
    completed: { label: 'Fondo eliminado', color: 'text-emerald-600', bg: 'bg-emerald-50' },
    failed: { label: 'Error al procesar', color: 'text-red-600', bg: 'bg-red-50' },
  };

  const c = config[status];

  return (
    <div className={`flex items-center gap-3 rounded-lg px-3 py-2 ${c.bg} ${className}`}>
      {status === 'pending' && <Clock className={`h-4 w-4 ${c.color}`} />}
      {status === 'processing' && <Loader2 className={`h-4 w-4 animate-spin ${c.color}`} />}
      {status === 'completed' && <CheckCircle2 className={`h-4 w-4 ${c.color}`} />}
      {status === 'failed' && <AlertTriangle className={`h-4 w-4 ${c.color}`} />}

      <div className="flex-1 min-w-0">
        <p className={`text-sm font-medium ${c.color}`}>{c.label}</p>
        {processedCount !== undefined && totalCount !== undefined && (
          <p className="text-xs text-gray-500">
            {processedCount}/{totalCount} fotos procesadas
          </p>
        )}
        {errorMessage && (
          <p className="text-xs text-red-500">{errorMessage}</p>
        )}
      </div>

      {status === 'processing' && progress > 0 && (
        <span className="text-xs font-medium text-purple-600">{progress}%</span>
      )}
    </div>
  );
}
