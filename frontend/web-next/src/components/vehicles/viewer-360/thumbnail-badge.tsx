'use client';

import { RotateCcw } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

interface ThumbnailBadgeProps {
  frameCount: number;
  thumbnailUrl?: string;
  isProcessing?: boolean;
  onClick?: () => void;
  size?: 'sm' | 'md';
}

// ============================================================
// COMPONENT
// ============================================================

export function Viewer360ThumbnailBadge({
  frameCount,
  thumbnailUrl,
  isProcessing = false,
  onClick,
  size = 'sm',
}: ThumbnailBadgeProps) {
  const sizeClasses = size === 'sm' ? 'h-8 w-8' : 'h-12 w-12';
  const iconSize = size === 'sm' ? 'h-3.5 w-3.5' : 'h-5 w-5';
  const textSize = size === 'sm' ? 'text-[9px]' : 'text-[11px]';

  return (
    <button
      type="button"
      onClick={onClick}
      className={`group relative ${sizeClasses} overflow-hidden rounded-lg border-2 border-emerald-400 bg-gray-900 transition-transform hover:scale-110`}
      title={`Vista 360° (${frameCount} frames)`}
    >
      {thumbnailUrl ? (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={thumbnailUrl}
          alt="Vista 360°"
          className="h-full w-full object-cover"
        />
      ) : (
        <div className="flex h-full w-full items-center justify-center">
          <RotateCcw
            className={`${iconSize} text-emerald-400 ${isProcessing ? 'animate-spin' : ''}`}
          />
        </div>
      )}

      {/* Badge overlay */}
      <div className="absolute inset-x-0 bottom-0 bg-emerald-500/90 py-px text-center">
        <span className={`font-bold text-white ${textSize}`}>360°</span>
      </div>
    </button>
  );
}
