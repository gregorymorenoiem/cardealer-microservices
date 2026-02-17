/**
 * DestacadoBadge Component
 * Sprint 2: Badge Components & Visual System
 * 
 * Subtle badge for featured listings
 * UX Rule: Max 24px height, 90% opacity
 */

import { Star } from 'lucide-react';

interface DestacadoBadgeProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const sizeClasses = {
  sm: 'px-2 py-0.5 text-[10px]',
  md: 'px-3 py-1 text-xs',
  lg: 'px-4 py-1.5 text-sm'
};

const iconSizes = {
  sm: 10,
  md: 12,
  lg: 14
};

export default function DestacadoBadge({ size = 'md', className = '' }: DestacadoBadgeProps) {
  return (
    <span 
      className={`
        inline-flex items-center gap-1
        bg-gradient-to-r from-blue-500 to-emerald-500
        rounded-full text-white font-medium
        shadow-sm
        opacity-90
        transition-opacity duration-200
        hover:opacity-100
        ${sizeClasses[size]}
        ${className}
      `}
      role="status"
      aria-label="Anuncio destacado"
    >
      <Star size={iconSizes[size]} fill="currentColor" strokeWidth={0} />
      <span>Destacado</span>
    </span>
  );
}
