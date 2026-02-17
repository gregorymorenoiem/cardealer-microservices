/**
 * TopDealerBadge Component
 * Sprint 2: Badge Components & Visual System
 * 
 * Enterprise tier badge for top dealers
 * UX Rule: Max 24px height, 90% opacity
 */

import { Trophy } from 'lucide-react';

interface TopDealerBadgeProps {
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

export default function TopDealerBadge({ size = 'md', className = '' }: TopDealerBadgeProps) {
  return (
    <span 
      className={`
        inline-flex items-center gap-1
        bg-gradient-to-r from-purple-600 via-pink-600 to-rose-600
        rounded-full text-white font-semibold
        shadow-lg
        opacity-90
        transition-all duration-200
        hover:opacity-100 hover:shadow-xl
        ${sizeClasses[size]}
        ${className}
      `}
      role="status"
      aria-label="Top dealer"
    >
      <Trophy size={iconSizes[size]} fill="currentColor" strokeWidth={0} />
      <span>Top Dealer</span>
    </span>
  );
}
