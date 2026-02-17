'use client';

import { Sparkles, Loader2 } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

interface BgRemoveButtonProps {
  onClick: () => void;
  isProcessing?: boolean;
  isDisabled?: boolean;
  variant?: 'primary' | 'secondary' | 'icon';
  className?: string;
}

// ============================================================
// COMPONENT
// ============================================================

export function BgRemoveButton({
  onClick,
  isProcessing = false,
  isDisabled = false,
  variant = 'primary',
  className = '',
}: BgRemoveButtonProps) {
  if (variant === 'icon') {
    return (
      <button
        type="button"
        onClick={onClick}
        disabled={isProcessing || isDisabled}
        title="Eliminar fondo"
        className={`rounded-lg p-1.5 text-purple-500 transition-colors hover:bg-purple-50 disabled:opacity-50 ${className}`}
      >
        {isProcessing ? (
          <Loader2 className="h-4 w-4 animate-spin" />
        ) : (
          <Sparkles className="h-4 w-4" />
        )}
      </button>
    );
  }

  if (variant === 'secondary') {
    return (
      <button
        type="button"
        onClick={onClick}
        disabled={isProcessing || isDisabled}
        className={`flex items-center gap-1.5 rounded-lg border border-purple-200 bg-purple-50 px-3 py-1.5 text-sm font-medium text-purple-700 transition-colors hover:bg-purple-100 disabled:opacity-50 ${className}`}
      >
        {isProcessing ? (
          <Loader2 className="h-4 w-4 animate-spin" />
        ) : (
          <Sparkles className="h-4 w-4" />
        )}
        {isProcessing ? 'Procesando...' : 'Eliminar fondo'}
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={onClick}
      disabled={isProcessing || isDisabled}
      className={`flex items-center gap-2 rounded-lg bg-purple-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-purple-700 disabled:opacity-50 ${className}`}
    >
      {isProcessing ? (
        <Loader2 className="h-4 w-4 animate-spin" />
      ) : (
        <Sparkles className="h-4 w-4" />
      )}
      {isProcessing ? 'Eliminando fondo...' : 'Eliminar fondo con IA'}
    </button>
  );
}
