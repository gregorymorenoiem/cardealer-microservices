/**
 * Gradient Border Utility
 * Sprint 2: Badge Components & Visual System (Cardealer)
 * 
 * Subtle gradient borders for featured listing cards
 * UX Rule: Max 1px border, respects tier hierarchy
 */

export const gradientBorders = {
  // Featured tier - subtle blue-emerald
  featured: 'border border-transparent bg-gradient-to-r from-blue-500/20 to-emerald-500/20 rounded-lg',
  
  // Premium tier - gold shimmer
  premium: 'border border-transparent bg-gradient-to-r from-amber-400/30 via-yellow-500/30 to-amber-600/30 rounded-lg',
  
  // Enterprise tier - bold purple-pink
  enterprise: 'border border-transparent bg-gradient-to-r from-purple-600/40 via-pink-600/40 to-rose-600/40 rounded-lg',
  
  // Basic tier - no border
  basic: ''
} as const;

/**
 * Card wrapper utility classes
 * Applies subtle gradient border with proper padding
 */
export const getCardBorderClass = (tier?: 'basic' | 'featured' | 'premium' | 'enterprise') => {
  if (!tier || tier === 'basic') return '';
  
  const gradientClass = gradientBorders[tier];
  
  // Add padding to create border effect
  return `p-[1px] ${gradientClass}`;
};

/**
 * Inner card classes to maintain proper spacing
 */
export const innerCardClass = 'bg-white dark:bg-gray-800 rounded-lg overflow-hidden';
