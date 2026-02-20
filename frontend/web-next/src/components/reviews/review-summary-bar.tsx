/**
 * Review Summary Bar Component
 * Shows average rating and rating distribution
 */

'use client';

import { Star } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ReviewStats } from '@/services/reviews';

interface ReviewSummaryBarProps {
  stats: ReviewStats | undefined;
  isLoading?: boolean;
  className?: string;
}

export function ReviewSummaryBar({ stats, isLoading, className }: ReviewSummaryBarProps) {
  if (isLoading) {
    return (
      <div className={cn('animate-pulse space-y-3', className)}>
        <div className="bg-muted h-16 rounded" />
        <div className="space-y-2">
          {[5, 4, 3, 2, 1].map(i => (
            <div key={i} className="bg-muted h-4 rounded" />
          ))}
        </div>
      </div>
    );
  }

  if (!stats) return null;

  const totalReviews = stats.totalReviews || 0;
  const avgRating = stats.averageRating || 0;
  const dist = stats.ratingDistribution || { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 };

  return (
    <div className={cn('flex flex-col gap-4 sm:flex-row sm:gap-8', className)}>
      {/* Average rating */}
      <div className="flex flex-col items-center">
        <span className="text-foreground text-5xl font-bold">
          {avgRating > 0 ? avgRating.toFixed(1) : '—'}
        </span>
        <div className="mt-1 flex gap-0.5">
          {[1, 2, 3, 4, 5].map(star => (
            <Star
              key={star}
              className={cn(
                'h-4 w-4',
                star <= Math.round(avgRating)
                  ? 'fill-amber-400 text-amber-400'
                  : 'fill-transparent text-gray-300'
              )}
            />
          ))}
        </div>
        <span className="text-muted-foreground mt-1 text-sm">
          {totalReviews} {totalReviews === 1 ? 'reseña' : 'reseñas'}
        </span>
      </div>

      {/* Rating distribution */}
      <div className="flex-1 space-y-1.5">
        {[5, 4, 3, 2, 1].map(stars => {
          const count = dist[stars as keyof typeof dist] || 0;
          const pct = totalReviews > 0 ? (count / totalReviews) * 100 : 0;
          return (
            <div key={stars} className="flex items-center gap-2">
              <span className="text-muted-foreground w-4 text-right text-sm">{stars}</span>
              <Star className="h-3.5 w-3.5 fill-amber-400 text-amber-400" />
              <div className="bg-muted h-2.5 flex-1 overflow-hidden rounded-full">
                <div
                  className="h-full rounded-full bg-amber-400 transition-all duration-500"
                  style={{ width: `${pct}%` }}
                />
              </div>
              <span className="text-muted-foreground w-8 text-right text-xs">{count}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default ReviewSummaryBar;
