/**
 * SkeletonLoader - Skeleton loading components for perceived performance
 * Creates the impression of faster loading for users in low-bandwidth areas
 * 
 * Features:
 * - Multiple skeleton variants (card, list, detail, etc.)
 * - Shimmer animation effect
 * - Matches actual component dimensions
 * - Dark mode support
 */

import React from 'react';
import { motion } from 'framer-motion';

interface SkeletonProps {
  className?: string;
  animate?: boolean;
}

// Base skeleton with shimmer effect
export const Skeleton: React.FC<SkeletonProps> = ({ className = '', animate = true }) => (
  <div
    className={`bg-gray-200 rounded ${
      animate ? 'animate-pulse' : ''
    } ${className}`}
  />
);

// Shimmer overlay effect (can be used in future components)
// @ts-ignore - Reserved for future use
const _ShimmerOverlay: React.FC = () => (
  <div className="absolute inset-0 -translate-x-full animate-[shimmer_2s_infinite] bg-gradient-to-r from-transparent via-white/20 to-transparent" />
);

// Card skeleton for listings
export const CardSkeleton: React.FC<{ variant?: 'default' | 'compact' | 'featured' }> = ({ 
  variant = 'default' 
}) => {
  if (variant === 'compact') {
    return (
      <div className="bg-white rounded-xl overflow-hidden shadow-sm">
        <div className="flex">
          <Skeleton className="w-32 h-24" />
          <div className="flex-1 p-3 space-y-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-1/2" />
            <Skeleton className="h-5 w-1/3" />
          </div>
        </div>
      </div>
    );
  }

  if (variant === 'featured') {
    return (
      <div className="bg-white rounded-2xl overflow-hidden shadow-lg">
        <Skeleton className="aspect-[16/9]" />
        <div className="p-5 space-y-3">
          <Skeleton className="h-6 w-4/5" />
          <Skeleton className="h-4 w-2/3" />
          <div className="flex gap-3 pt-2">
            <Skeleton className="h-8 w-24 rounded-full" />
            <Skeleton className="h-8 w-24 rounded-full" />
          </div>
          <Skeleton className="h-8 w-1/3 mt-4" />
        </div>
      </div>
    );
  }

  // Default card
  return (
    <div className="bg-white rounded-2xl overflow-hidden shadow-sm">
      <Skeleton className="aspect-[16/10]" />
      <div className="p-4 space-y-3">
        <Skeleton className="h-5 w-4/5" />
        <Skeleton className="h-4 w-2/3" />
        <div className="flex gap-2 pt-1">
          <Skeleton className="h-6 w-16 rounded-full" />
          <Skeleton className="h-6 w-16 rounded-full" />
          <Skeleton className="h-6 w-16 rounded-full" />
        </div>
        <Skeleton className="h-7 w-1/3 mt-2" />
      </div>
    </div>
  );
};

// Grid of card skeletons
export const CardGridSkeleton: React.FC<{ 
  count?: number; 
  columns?: number;
  variant?: 'default' | 'compact' | 'featured';
}> = ({ 
  count = 6, 
  columns = 3,
  variant = 'default'
}) => (
  <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-${columns} gap-6`}>
    {Array.from({ length: count }).map((_, i) => (
      <CardSkeleton key={i} variant={variant} />
    ))}
  </div>
);

// List item skeleton
export const ListItemSkeleton: React.FC = () => (
  <div className="flex items-center gap-4 p-4 bg-white rounded-xl">
    <Skeleton className="w-24 h-24 rounded-lg flex-shrink-0" />
    <div className="flex-1 space-y-2">
      <Skeleton className="h-5 w-3/4" />
      <Skeleton className="h-4 w-1/2" />
      <Skeleton className="h-4 w-1/4" />
    </div>
    <Skeleton className="w-24 h-10 rounded-lg" />
  </div>
);

// Detail page skeleton
export const DetailPageSkeleton: React.FC<{ type?: 'vehicle' | 'property' }> = ({ type = 'vehicle' }) => (
  <div className="max-w-7xl mx-auto px-4 py-8">
    {/* Breadcrumb */}
    <div className="flex gap-2 mb-6">
      <Skeleton className="h-4 w-20" />
      <Skeleton className="h-4 w-4" />
      <Skeleton className="h-4 w-32" />
    </div>

    <div className="grid lg:grid-cols-3 gap-8">
      {/* Gallery */}
      <div className="lg:col-span-2 space-y-4">
        <Skeleton className="aspect-[16/10] rounded-2xl" />
        <div className="flex gap-2">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="w-20 h-14 rounded-lg" />
          ))}
        </div>
      </div>

      {/* Info Panel */}
      <div className="space-y-6">
        <div className="bg-white rounded-2xl p-6 shadow-lg space-y-4">
          <Skeleton className="h-8 w-1/2" />
          <Skeleton className="h-6 w-full" />
          <Skeleton className="h-6 w-3/4" />
          <div className="pt-4 border-t space-y-3">
            {type === 'vehicle' ? (
              <>
                <div className="flex justify-between">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-24" />
                </div>
                <div className="flex justify-between">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-16" />
                </div>
                <div className="flex justify-between">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-20" />
                </div>
              </>
            ) : (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <Skeleton className="h-12 rounded-lg" />
                  <Skeleton className="h-12 rounded-lg" />
                  <Skeleton className="h-12 rounded-lg" />
                  <Skeleton className="h-12 rounded-lg" />
                </div>
              </>
            )}
          </div>
          <Skeleton className="h-12 rounded-xl" />
          <Skeleton className="h-12 rounded-xl" />
        </div>
      </div>
    </div>

    {/* Description Section */}
    <div className="mt-8 space-y-4">
      <Skeleton className="h-7 w-48" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-3/4" />
    </div>

    {/* Features Grid */}
    <div className="mt-8">
      <Skeleton className="h-7 w-48 mb-4" />
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[...Array(8)].map((_, i) => (
          <Skeleton key={i} className="h-12 rounded-lg" />
        ))}
      </div>
    </div>
  </div>
);

// Search/Filter skeleton
export const FiltersSkeleton: React.FC = () => (
  <div className="space-y-6">
    {/* Category Pills */}
    <div className="flex gap-3">
      <Skeleton className="h-10 w-28 rounded-full" />
      <Skeleton className="h-10 w-32 rounded-full" />
      <Skeleton className="h-10 w-24 rounded-full" />
    </div>

    {/* Search Bar */}
    <Skeleton className="h-14 rounded-xl" />

    {/* Filter Groups */}
    {[...Array(4)].map((_, i) => (
      <div key={i} className="space-y-3">
        <Skeleton className="h-5 w-24" />
        <div className="grid grid-cols-2 gap-2">
          <Skeleton className="h-10 rounded-lg" />
          <Skeleton className="h-10 rounded-lg" />
        </div>
      </div>
    ))}

    {/* Action Buttons */}
    <div className="flex gap-3">
      <Skeleton className="h-12 flex-1 rounded-xl" />
      <Skeleton className="h-12 w-32 rounded-xl" />
    </div>
  </div>
);

// Stats card skeleton
export const StatsCardSkeleton: React.FC = () => (
  <div className="bg-white rounded-2xl p-6 shadow-sm">
    <div className="flex items-center justify-between">
      <div className="space-y-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-8 w-16" />
      </div>
      <Skeleton className="w-12 h-12 rounded-xl" />
    </div>
    <Skeleton className="h-3 w-32 mt-4" />
  </div>
);

// Table skeleton
export const TableSkeleton: React.FC<{ rows?: number; columns?: number }> = ({ 
  rows = 5, 
  columns = 5 
}) => (
  <div className="bg-white rounded-xl overflow-hidden">
    {/* Header */}
    <div className="flex gap-4 p-4 border-b border-gray-100">
      {[...Array(columns)].map((_, i) => (
        <Skeleton key={i} className="h-4 flex-1" />
      ))}
    </div>
    {/* Rows */}
    {[...Array(rows)].map((_, rowIndex) => (
      <div key={rowIndex} className="flex gap-4 p-4 border-b border-gray-50">
        {[...Array(columns)].map((_, colIndex) => (
          <Skeleton key={colIndex} className="h-4 flex-1" />
        ))}
      </div>
    ))}
  </div>
);

// Hero section skeleton
export const HeroSkeleton: React.FC = () => (
  <div className="relative bg-gray-100 py-20 px-4">
    <div className="max-w-4xl mx-auto text-center space-y-6">
      <Skeleton className="h-12 w-3/4 mx-auto" />
      <Skeleton className="h-6 w-2/3 mx-auto" />
      <Skeleton className="h-16 w-full max-w-2xl mx-auto rounded-2xl" />
      <div className="flex justify-center gap-4 pt-4">
        <Skeleton className="h-12 w-32 rounded-xl" />
        <Skeleton className="h-12 w-32 rounded-xl" />
      </div>
    </div>
  </div>
);

// Animated loading wrapper
export const LoadingWrapper: React.FC<{
  isLoading: boolean;
  skeleton: React.ReactNode;
  children: React.ReactNode;
  minHeight?: string;
}> = ({ isLoading, skeleton, children, minHeight }) => (
  <div style={{ minHeight }}>
    {isLoading ? (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
      >
        {skeleton}
      </motion.div>
    ) : (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.1 }}
      >
        {children}
      </motion.div>
    )}
  </div>
);

export default Skeleton;
