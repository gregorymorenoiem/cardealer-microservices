/**
 * Reviews Section — Orchestrator component
 * Combines ReviewSummaryBar + ReviewCard list + WriteReviewDialog
 * Used on vehicle detail and dealer profile pages
 */

'use client';

import * as React from 'react';
import { Star, MessageSquarePlus } from 'lucide-react';
import { ReviewSummaryBar } from './review-summary-bar';
import { ReviewCard } from './review-card';
import { WriteReviewDialog } from './write-review-dialog';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/use-auth';
import { useReviewsForTarget, useReviewStats } from '@/hooks/use-reviews';

interface ReviewsSectionProps {
  targetId: string;
  targetType: 'seller' | 'dealer';
  targetName?: string;
  vehicleId?: string;
  vehicleTitle?: string;
  className?: string;
}

export function ReviewsSection({
  targetId,
  targetType,
  targetName,
  vehicleId,
  vehicleTitle,
  className = '',
}: ReviewsSectionProps) {
  const { user, isAuthenticated } = useAuth();
  const [writeOpen, setWriteOpen] = React.useState(false);
  const [sortBy, setSortBy] = React.useState<'newest' | 'highest' | 'lowest' | 'helpful'>('newest');

  const {
    data: reviews,
    isLoading: reviewsLoading,
    refetch: refetchReviews,
  } = useReviewsForTarget(targetType, targetId, { sortBy, pageSize: 10 });

  const {
    data: stats,
    isLoading: statsLoading,
    refetch: refetchStats,
  } = useReviewStats(targetType, targetId);

  const reviewList = reviews?.items ?? [];
  const canWrite = isAuthenticated && user?.accountType !== 'admin';

  const handleReviewSuccess = () => {
    refetchReviews();
    refetchStats();
  };

  const sortOptions: { value: typeof sortBy; label: string }[] = [
    { value: 'newest', label: 'Más recientes' },
    { value: 'highest', label: 'Mayor calificación' },
    { value: 'lowest', label: 'Menor calificación' },
    { value: 'helpful', label: 'Más útiles' },
  ];

  return (
    <section className={className} id="reviews">
      {/* Header */}
      <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-2">
          <Star className="text-amber-500" size={22} />
          <h2 className="text-foreground text-xl font-semibold">
            Reseñas
            {stats && stats.totalReviews > 0 && (
              <span className="text-muted-foreground ml-2 text-base font-normal">
                ({stats.totalReviews})
              </span>
            )}
          </h2>
        </div>

        {canWrite && (
          <Button onClick={() => setWriteOpen(true)} size="sm" className="w-full sm:w-auto">
            <MessageSquarePlus className="mr-2 h-4 w-4" />
            Escribir reseña
          </Button>
        )}
      </div>

      {/* Stats Summary */}
      <ReviewSummaryBar stats={stats} isLoading={statsLoading} className="mb-6" />

      {/* Sort */}
      {reviewList.length > 0 && (
        <div className="mb-4 flex items-center gap-2">
          <span className="text-muted-foreground text-sm">Ordenar por:</span>
          <div className="flex flex-wrap gap-1">
            {sortOptions.map(option => (
              <button
                key={option.value}
                onClick={() => setSortBy(option.value)}
                className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                  sortBy === option.value
                    ? 'bg-primary text-primary-foreground'
                    : 'bg-muted text-muted-foreground hover:bg-muted/80'
                }`}
              >
                {option.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Reviews List */}
      {reviewsLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="bg-muted animate-pulse rounded-xl p-6">
              <div className="mb-3 flex items-center gap-3">
                <div className="bg-muted-foreground/20 h-10 w-10 rounded-full" />
                <div className="space-y-1.5">
                  <div className="bg-muted-foreground/20 h-4 w-28 rounded" />
                  <div className="bg-muted-foreground/20 h-3 w-20 rounded" />
                </div>
              </div>
              <div className="bg-muted-foreground/20 mb-2 h-4 w-full rounded" />
              <div className="bg-muted-foreground/20 h-4 w-3/4 rounded" />
            </div>
          ))}
        </div>
      ) : reviewList.length > 0 ? (
        <div className="space-y-4">
          {reviewList.map(review => (
            <ReviewCard key={review.id} review={review} />
          ))}
        </div>
      ) : (
        <div className="bg-muted/50 rounded-xl py-12 text-center">
          <Star className="text-muted-foreground/40 mx-auto mb-3 h-10 w-10" />
          <p className="text-muted-foreground font-medium">Aún no hay reseñas</p>
          <p className="text-muted-foreground/60 mt-1 text-sm">
            Sé el primero en compartir tu experiencia
            {targetName ? ` con ${targetName}` : ''}.
          </p>
          {canWrite && (
            <Button onClick={() => setWriteOpen(true)} variant="outline" size="sm" className="mt-4">
              <MessageSquarePlus className="mr-2 h-4 w-4" />
              Escribir la primera reseña
            </Button>
          )}
        </div>
      )}

      {/* Write Review Dialog */}
      <WriteReviewDialog
        targetId={targetId}
        targetType={targetType}
        vehicleId={vehicleId}
        vehicleTitle={vehicleTitle}
        open={writeOpen}
        onOpenChange={setWriteOpen}
        onSuccess={handleReviewSuccess}
      />
    </section>
  );
}

export default ReviewsSection;
