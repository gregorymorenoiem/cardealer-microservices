/**
 * PremiumBadge Component
 * Sprint 2: Badge Components & Visual System
 * 
 * Premium tier badge with diamond icon
 * UX Rule: Max 24px height, 90% opacity
 */

import { Gem } from 'lucide-react';

interface PremiumBadgeProps {
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

export default function PremiumBadge({ size = 'md', className = '' }: PremiumBadgeProps) {
  return (
    <span 
      className={`
        inline-flex items-center gap-1
        bg-gradient-to-r from-amber-400 via-yellow-500 to-amber-600
        rounded-full text-white font-medium
        shadow-md
        opacity-90
        transition-opacity duration-200
        hover:opacity-100
        ${sizeClasses[size]}
        ${className}
      `}
      role="status"
      aria-label="Anuncio premium"
    >
      <Gem size={iconSizes[size]} fill="currentColor" strokeWidth={0} />
      <span>Premium</span>
    </span>
  );
}
