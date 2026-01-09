import React, { useState, useEffect } from 'react';
import { StarRating } from './StarRating';
import { RatingDistributionChart } from './RatingDistributionChart';
import { ReviewsList } from './ReviewsList';
import { ReviewForm } from './ReviewForm';
import { reviewService, ReviewSummaryDto } from '../../services/reviewService';

interface ReviewsSectionProps {
  sellerUserId: string;
  vehicleId?: string;
  currentUserId?: string;
  allowReviews?: boolean;
  className?: string;
}

/**
 * Componente principal que combina el resumen de reseñas,
 * distribución de calificaciones, lista de reseñas y formulario
 */
export const ReviewsSection: React.FC<ReviewsSectionProps> = ({
  sellerUserId,
  vehicleId,
  currentUserId,
  allowReviews = true,
  className = '',
}) => {
  const [summary, setSummary] = useState<ReviewSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const loadSummary = async () => {
    try {
      setLoading(true);
      setError(null);

      const result = await reviewService.getSellerReviewSummary(sellerUserId);

      if (result.success && result.data) {
        setSummary(result.data);
      } else {
        setError(result.error || 'Error al cargar el resumen de reseñas');
      }
    } catch (err) {
      setError('Error inesperado al cargar el resumen');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (sellerUserId) {
      loadSummary();
    }
  }, [sellerUserId, refreshKey]);

  const handleReviewSubmitted = () => {
    setShowReviewForm(false);
    setRefreshKey((prev) => prev + 1); // Trigger refresh of data
  };

  const canWriteReview = () => {
    return allowReviews && currentUserId && currentUserId !== sellerUserId && !showReviewForm;
  };

  const renderSummaryHeader = () => {
    if (loading || !summary) return null;

    return (
      <div className="bg-white rounded-lg border p-6 mb-6">
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between">
          {/* Left side - Summary stats */}
          <div className="flex items-center space-x-6 mb-4 lg:mb-0">
            {/* Overall rating */}
            <div className="text-center">
              <div className="text-3xl font-bold text-gray-900 mb-1">
                {summary.averageRating.toFixed(1)}
              </div>
              <StarRating
                rating={summary.averageRating}
                interactive={false}
                size="lg"
                showHalfStars={true}
              />
              <div className="text-sm text-gray-500 mt-1">
                {summary.totalReviews} reseña{summary.totalReviews !== 1 ? 's' : ''}
              </div>
            </div>

            {/* Divider */}
            <div className="hidden sm:block w-px h-16 bg-gray-200"></div>

            {/* Key metrics */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 text-center">
              <div>
                <div className="text-lg font-semibold text-green-600">
                  {summary.positivePercentage.toFixed(0)}%
                </div>
                <div className="text-xs text-gray-500">Positivas</div>
              </div>

              <div>
                <div className="text-lg font-semibold text-blue-600">
                  {summary.verifiedPurchaseReviews}
                </div>
                <div className="text-xs text-gray-500">Verificadas</div>
              </div>

              {summary.lastReviewDate && (
                <div className="hidden sm:block">
                  <div className="text-lg font-semibold text-gray-700">
                    {Math.floor(
                      (Date.now() - new Date(summary.lastReviewDate).getTime()) /
                        (1000 * 60 * 60 * 24)
                    )}
                  </div>
                  <div className="text-xs text-gray-500">Días desde última</div>
                </div>
              )}
            </div>
          </div>

          {/* Right side - Action button */}
          {canWriteReview() && (
            <div>
              <button
                onClick={() => setShowReviewForm(true)}
                className="w-full lg:w-auto px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
              >
                Escribir Reseña
              </button>
            </div>
          )}
        </div>
      </div>
    );
  };

  const renderLoadingState = () => (
    <div className={`space-y-6 ${className}`}>
      {/* Summary loading */}
      <div className="bg-white rounded-lg border p-6 animate-pulse">
        <div className="flex items-center space-x-6">
          <div className="text-center">
            <div className="w-16 h-8 bg-gray-200 rounded mb-2"></div>
            <div className="w-24 h-6 bg-gray-200 rounded mb-2"></div>
            <div className="w-20 h-4 bg-gray-200 rounded"></div>
          </div>
          <div className="w-px h-16 bg-gray-200"></div>
          <div className="grid grid-cols-3 gap-4">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="text-center">
                <div className="w-12 h-6 bg-gray-200 rounded mb-1"></div>
                <div className="w-16 h-3 bg-gray-200 rounded"></div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Distribution loading */}
      <div className="bg-white rounded-lg border p-6 animate-pulse">
        <div className="w-48 h-6 bg-gray-200 rounded mb-4"></div>
        <div className="space-y-3">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="flex items-center space-x-3">
              <div className="w-8 h-4 bg-gray-200 rounded"></div>
              <div className="flex-1 h-3 bg-gray-200 rounded"></div>
              <div className="w-16 h-4 bg-gray-200 rounded"></div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  const renderErrorState = () => (
    <div className={`bg-white rounded-lg border p-6 ${className}`}>
      <div className="text-center">
        <svg
          className="w-12 h-12 text-gray-400 mx-auto mb-4"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
          />
        </svg>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Error al cargar reseñas</h3>
        <p className="text-gray-500 mb-4">{error}</p>
        <button
          onClick={loadSummary}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
        >
          Reintentar
        </button>
      </div>
    </div>
  );

  const renderEmptyState = () => (
    <div className={`bg-white rounded-lg border p-8 text-center ${className}`}>
      <div className="w-16 h-16 mx-auto mb-4 rounded-full bg-gray-100 flex items-center justify-center">
        <svg
          className="w-8 h-8 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
          />
        </svg>
      </div>

      <h3 className="text-xl font-semibold text-gray-900 mb-2">Aún no hay reseñas</h3>

      <p className="text-gray-500 mb-6 max-w-md mx-auto">
        {currentUserId === sellerUserId
          ? 'Cuando los clientes compren tus vehículos podrán dejarte reseñas aquí.'
          : 'Sé el primero en compartir tu experiencia con este vendedor.'}
      </p>

      {canWriteReview() && (
        <button
          onClick={() => setShowReviewForm(true)}
          className="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
        >
          Escribir Primera Reseña
        </button>
      )}
    </div>
  );

  // Loading state
  if (loading) {
    return renderLoadingState();
  }

  // Error state
  if (error) {
    return renderErrorState();
  }

  // No reviews yet
  if (!summary || summary.totalReviews === 0) {
    return (
      <div className={className}>
        {renderEmptyState()}

        {/* Show form if user clicked to write first review */}
        {showReviewForm && canWriteReview() && (
          <div className="mt-6">
            <ReviewForm
              sellerUserId={sellerUserId}
              vehicleId={vehicleId}
              buyerUserId={currentUserId}
              onReviewSubmitted={handleReviewSubmitted}
              onCancel={() => setShowReviewForm(false)}
            />
          </div>
        )}
      </div>
    );
  }

  // Full reviews display
  return (
    <div className={`space-y-6 ${className}`}>
      {/* Summary header */}
      {renderSummaryHeader()}

      {/* Review form (when open) */}
      {showReviewForm && (
        <ReviewForm
          sellerUserId={sellerUserId}
          vehicleId={vehicleId}
          buyerUserId={currentUserId}
          onReviewSubmitted={handleReviewSubmitted}
          onCancel={() => setShowReviewForm(false)}
        />
      )}

      {/* Two column layout for distribution and list */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Rating distribution - left sidebar on desktop */}
        <div className="lg:col-span-1">
          <RatingDistributionChart summary={summary} />
        </div>

        {/* Reviews list - main content */}
        <div className="lg:col-span-2">
          <ReviewsList
            sellerUserId={sellerUserId}
            vehicleId={vehicleId}
            key={refreshKey} // Force re-render when reviews change
          />
        </div>
      </div>
    </div>
  );
};

export default ReviewsSection;
