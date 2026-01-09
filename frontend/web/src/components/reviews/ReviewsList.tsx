import React, { useState, useEffect } from 'react';
import { StarRating } from './StarRating';
import {
  reviewService,
  ReviewDto,
  ReviewSortBy,
  GetReviewsRequest,
} from '../../services/reviewService';

interface ReviewsListProps {
  sellerUserId: string;
  vehicleId?: string;
  className?: string;
}

/**
 * Lista de reseñas con paginación, filtros y ordenamiento
 * Similar al formato de Amazon/eBay reviews
 */
export const ReviewsList: React.FC<ReviewsListProps> = ({
  sellerUserId,
  vehicleId,
  className = '',
}) => {
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalReviews, setTotalReviews] = useState(0);
  const [sortBy, setSortBy] = useState<ReviewSortBy>('date_desc');
  const [ratingFilter, setRatingFilter] = useState<number | null>(null);
  const [expandedReviews, setExpandedReviews] = useState<Set<string>>(new Set());

  const pageSize = 10;

  const loadReviews = async () => {
    try {
      setLoading(true);
      setError(null);

      const request: GetReviewsRequest = {
        sellerUserId,
        vehicleId,
        page: currentPage,
        pageSize,
        sortBy,
        rating: ratingFilter,
      };

      const result = await reviewService.getReviews(request);

      if (result.success && result.data) {
        setReviews(result.data.reviews);
        setTotalPages(result.data.totalPages);
        setTotalReviews(result.data.totalReviews);
      } else {
        setError(result.error || 'Error al cargar las reseñas');
      }
    } catch (err) {
      setError('Error inesperado al cargar las reseñas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadReviews();
  }, [sellerUserId, vehicleId, currentPage, sortBy, ratingFilter]);

  const handleSortChange = (newSortBy: ReviewSortBy) => {
    setSortBy(newSortBy);
    setCurrentPage(1);
  };

  const handleRatingFilter = (rating: number | null) => {
    setRatingFilter(rating);
    setCurrentPage(1);
  };

  const toggleExpandReview = (reviewId: string) => {
    const newExpanded = new Set(expandedReviews);
    if (newExpanded.has(reviewId)) {
      newExpanded.delete(reviewId);
    } else {
      newExpanded.add(reviewId);
    }
    setExpandedReviews(newExpanded);
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const shouldTruncateContent = (content: string): boolean => {
    return content.length > 200;
  };

  const getTruncatedContent = (content: string): string => {
    return content.substring(0, 200) + '...';
  };

  const renderReview = (review: ReviewDto) => {
    const isExpanded = expandedReviews.has(review.id);
    const shouldTruncate = shouldTruncateContent(review.content);
    const displayContent =
      shouldTruncate && !isExpanded ? getTruncatedContent(review.content) : review.content;

    return (
      <div key={review.id} className="border-b border-gray-200 py-6 last:border-b-0">
        {/* Header */}
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-center space-x-3">
            {/* Avatar placeholder */}
            <div className="w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center">
              <span className="text-sm font-medium text-gray-600">
                {review.buyerName?.charAt(0).toUpperCase() || '?'}
              </span>
            </div>

            <div>
              <div className="flex items-center space-x-2">
                <span className="font-medium text-gray-900">
                  {review.buyerName || 'Usuario Anónimo'}
                </span>
                {review.isVerifiedPurchase && (
                  <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    <svg className="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    Compra Verificada
                  </span>
                )}
              </div>

              <div className="flex items-center space-x-2 mt-1">
                <StarRating rating={review.rating} interactive={false} size="sm" />
                <span className="text-sm text-gray-500">{formatDate(review.createdAt)}</span>
              </div>
            </div>
          </div>

          {/* Helpful votes (future feature) */}
          <div className="text-sm text-gray-500">#{review.id.substring(0, 8)}</div>
        </div>

        {/* Title */}
        <h4 className="font-semibold text-gray-900 mb-2">{review.title}</h4>

        {/* Content */}
        <div className="text-gray-700 leading-relaxed mb-3">
          <p className="whitespace-pre-wrap">{displayContent}</p>

          {shouldTruncate && (
            <button
              onClick={() => toggleExpandReview(review.id)}
              className="text-blue-600 hover:text-blue-800 text-sm font-medium mt-1"
            >
              {isExpanded ? 'Ver menos' : 'Ver más'}
            </button>
          )}
        </div>

        {/* Seller Response */}
        {review.response && (
          <div className="mt-4 ml-8 p-4 bg-blue-50 border-l-4 border-blue-400 rounded-r-lg">
            <div className="flex items-center space-x-2 mb-2">
              <span className="font-medium text-blue-900">Respuesta del vendedor</span>
              <span className="text-xs text-blue-700">{formatDate(review.response.createdAt)}</span>
            </div>
            <p className="text-blue-800 text-sm leading-relaxed">{review.response.content}</p>
          </div>
        )}

        {/* Actions (future: helpful, report, etc.) */}
        <div className="mt-4 flex items-center space-x-4 text-sm">
          <button className="text-gray-500 hover:text-gray-700 flex items-center space-x-1">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M14 10h4.764a2 2 0 011.789 2.894l-3.5 7A2 2 0 0115.263 21h-4.017c-.163 0-.326-.02-.485-.06L7 20m7-10V8a2 2 0 00-2-2H4.5A2.5 2.5 0 002 8.5v1A2.5 2.5 0 004.5 12H7m7-2v10"
              />
            </svg>
            <span>Útil</span>
          </button>

          <button className="text-gray-500 hover:text-gray-700 flex items-center space-x-1">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.081 16.5c-.77.833.192 2.5 1.732 2.5z"
              />
            </svg>
            <span>Reportar</span>
          </button>
        </div>
      </div>
    );
  };

  const renderFiltersAndSort = () => (
    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
      {/* Filters */}
      <div className="flex items-center space-x-4">
        <span className="text-sm font-medium text-gray-700">Filtrar:</span>

        {/* Rating filter */}
        <select
          value={ratingFilter || ''}
          onChange={(e) => handleRatingFilter(e.target.value ? parseInt(e.target.value) : null)}
          className="text-sm border border-gray-300 rounded-md px-2 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Todas las calificaciones</option>
          <option value="5">5 estrellas</option>
          <option value="4">4 estrellas</option>
          <option value="3">3 estrellas</option>
          <option value="2">2 estrellas</option>
          <option value="1">1 estrella</option>
        </select>
      </div>

      {/* Sort */}
      <div className="flex items-center space-x-4">
        <span className="text-sm font-medium text-gray-700">Ordenar:</span>
        <select
          value={sortBy}
          onChange={(e) => handleSortChange(e.target.value as ReviewSortBy)}
          className="text-sm border border-gray-300 rounded-md px-2 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="date_desc">Más recientes</option>
          <option value="date_asc">Más antiguos</option>
          <option value="rating_desc">Mayor calificación</option>
          <option value="rating_asc">Menor calificación</option>
        </select>
      </div>
    </div>
  );

  const renderPagination = () => {
    if (totalPages <= 1) return null;

    const pages = [];
    const maxVisiblePages = 5;

    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return (
      <div className="flex items-center justify-center space-x-2 mt-8">
        <button
          onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
          disabled={currentPage === 1}
          className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Anterior
        </button>

        {pages.map((page) => (
          <button
            key={page}
            onClick={() => setCurrentPage(page)}
            className={`px-3 py-2 text-sm font-medium rounded-md ${
              page === currentPage
                ? 'text-white bg-blue-600 border border-blue-600'
                : 'text-gray-700 bg-white border border-gray-300 hover:bg-gray-50'
            }`}
          >
            {page}
          </button>
        ))}

        <button
          onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
          disabled={currentPage === totalPages}
          className="px-3 py-2 text-sm font-medium text-gray-500 bg-white border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Siguiente
        </button>
      </div>
    );
  };

  if (loading) {
    return (
      <div className={`bg-white rounded-lg border p-6 ${className}`}>
        <div className="animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-1/4 mb-4"></div>
          {[...Array(3)].map((_, i) => (
            <div key={i} className="border-b border-gray-200 pb-4 mb-4">
              <div className="flex items-center space-x-3 mb-3">
                <div className="w-10 h-10 bg-gray-200 rounded-full"></div>
                <div className="space-y-2">
                  <div className="h-4 bg-gray-200 rounded w-24"></div>
                  <div className="h-3 bg-gray-200 rounded w-32"></div>
                </div>
              </div>
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-20 bg-gray-200 rounded"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
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
            onClick={loadReviews}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700"
          >
            Reintentar
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={`bg-white rounded-lg border ${className}`}>
      <div className="p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-xl font-semibold text-gray-900">Reseñas ({totalReviews})</h3>
        </div>

        {reviews.length === 0 ? (
          <div className="text-center py-12">
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
                d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
              />
            </svg>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Aún no hay reseñas</h3>
            <p className="text-gray-500">Sé el primero en compartir tu experiencia</p>
          </div>
        ) : (
          <>
            {renderFiltersAndSort()}
            <div className="space-y-0">{reviews.map(renderReview)}</div>
            {renderPagination()}
          </>
        )}
      </div>
    </div>
  );
};

export default ReviewsList;
