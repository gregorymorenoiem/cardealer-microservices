import React from 'react';
import type { ReviewSummaryDto, RatingDistribution } from '../../services/reviewService';

interface RatingDistributionProps {
  summary: ReviewSummaryDto;
  className?: string;
}

/**
 * Componente que muestra la distribución de calificaciones
 * Similar al formato de Amazon/Google Reviews
 */
export const RatingDistributionChart: React.FC<RatingDistributionProps> = ({
  summary,
  className = '',
}) => {
  const distribution: RatingDistribution = {
    5: summary.fiveStarReviews,
    4: summary.fourStarReviews,
    3: summary.threeStarReviews,
    2: summary.twoStarReviews,
    1: summary.oneStarReviews,
  };

  const maxCount = Math.max(...Object.values(distribution));

  const getPercentage = (count: number): number => {
    if (summary.totalReviews === 0) return 0;
    return (count / summary.totalReviews) * 100;
  };

  const getBarWidth = (count: number): string => {
    if (maxCount === 0) return '0%';
    return `${(count / maxCount) * 100}%`;
  };

  const renderRatingRow = (rating: number) => {
    const count = distribution[rating];
    const percentage = getPercentage(count);
    const barWidth = getBarWidth(count);

    return (
      <div key={rating} className="flex items-center space-x-3 py-1">
        {/* Rating number */}
        <div className="flex items-center space-x-1 w-8">
          <span className="text-sm font-medium text-gray-700">{rating}</span>
          <svg className="w-3 h-3 text-yellow-400" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
          </svg>
        </div>

        {/* Progress bar */}
        <div className="flex-1 h-3 bg-gray-200 rounded-full overflow-hidden">
          <div
            className="h-full bg-yellow-400 transition-all duration-500 ease-out rounded-full"
            style={{ width: barWidth }}
          />
        </div>

        {/* Count and percentage */}
        <div className="w-16 text-right">
          <span className="text-sm text-gray-600">
            {count}
            <span className="text-xs ml-1">({percentage.toFixed(0)}%)</span>
          </span>
        </div>
      </div>
    );
  };

  return (
    <div className={`bg-white rounded-lg border p-4 ${className}`}>
      {/* Header */}
      <div className="mb-4">
        <h3 className="text-lg font-semibold text-gray-900 mb-2">Distribución de Calificaciones</h3>

        {/* Summary stats */}
        <div className="flex items-center space-x-4 text-sm text-gray-600">
          <span>{summary.totalReviews} reseñas totales</span>
          <span>•</span>
          <span>{summary.positivePercentage.toFixed(0)}% positivas</span>
          {summary.verifiedPurchaseReviews > 0 && (
            <>
              <span>•</span>
              <span>{summary.verifiedPurchaseReviews} verificadas</span>
            </>
          )}
        </div>
      </div>

      {/* Rating distribution */}
      <div className="space-y-2">{[5, 4, 3, 2, 1].map((rating) => renderRatingRow(rating))}</div>

      {/* Footer stats */}
      {summary.lastReviewDate && (
        <div className="mt-4 pt-4 border-t">
          <p className="text-xs text-gray-500">
            Última reseña:{' '}
            {new Date(summary.lastReviewDate).toLocaleDateString('es-ES', {
              year: 'numeric',
              month: 'long',
              day: 'numeric',
            })}
          </p>
        </div>
      )}
    </div>
  );
};

export default RatingDistributionChart;
