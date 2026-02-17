import { useState } from 'react';
import { FiEdit } from 'react-icons/fi';
import type { ReviewStats } from '@/types/review';
import type { Review } from '@/types/review';
import StarRating from '@/components/atoms/StarRating';
import ReviewCard from '@/components/molecules/ReviewCard';
import ReviewForm, { type ReviewFormData } from '@/components/molecules/ReviewForm';

interface ReviewsSectionProps {
  vehicleId: string;
  stats: ReviewStats;
  reviews: Review[];
}

export default function ReviewsSection({ vehicleId, stats, reviews }: ReviewsSectionProps) {
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const reviewsPerPage = 5;

  const totalPages = Math.ceil(reviews.length / reviewsPerPage);
  const startIndex = (currentPage - 1) * reviewsPerPage;
  const endIndex = startIndex + reviewsPerPage;
  const currentReviews = reviews.slice(startIndex, endIndex);

  const handleSubmitReview = (data: ReviewFormData) => {
    console.log('New review:', { vehicleId, ...data });
    // TODO: Implement API call to submit review
    alert('Thank you for your review! It will be published after moderation.');
    setShowReviewForm(false);
  };

  const getDistributionPercentage = (count: number) => {
    return stats.totalReviews > 0 ? (count / stats.totalReviews) * 100 : 0;
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6 md:p-8">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Customer Reviews</h2>
        <button
          onClick={() => setShowReviewForm(true)}
          className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors font-semibold"
        >
          <FiEdit />
          Write a Review
        </button>
      </div>

      {stats.totalReviews === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-600 mb-4">No reviews yet. Be the first to review this vehicle!</p>
          <button
            onClick={() => setShowReviewForm(true)}
            className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors font-semibold"
          >
            Write the First Review
          </button>
        </div>
      ) : (
        <>
          {/* Rating Summary */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8 pb-8 border-b border-gray-200">
            {/* Overall Rating */}
            <div className="flex flex-col items-center justify-center bg-gray-50 rounded-lg p-6">
              <div className="text-5xl font-bold text-gray-900 mb-2">
                {stats.averageRating.toFixed(1)}
              </div>
              <StarRating rating={stats.averageRating} size="lg" />
              <p className="text-gray-600 mt-2">
                Based on {stats.totalReviews} {stats.totalReviews === 1 ? 'review' : 'reviews'}
              </p>
            </div>

            {/* Rating Distribution */}
            <div className="space-y-2">
              {[5, 4, 3, 2, 1].map((star) => {
                const count = stats.distribution[star as keyof typeof stats.distribution];
                const percentage = getDistributionPercentage(count);
                
                return (
                  <div key={star} className="flex items-center gap-3">
                    <span className="text-sm font-medium text-gray-700 w-12">
                      {star} star
                    </span>
                    <div className="flex-1 h-3 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-yellow-400 rounded-full transition-all duration-300"
                        style={{ width: `${percentage}%` }}
                      />
                    </div>
                    <span className="text-sm text-gray-600 w-12 text-right">
                      {count}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Reviews List */}
          <div className="space-y-6">
            <h3 className="text-xl font-bold text-gray-900">
              Customer Opinions
            </h3>
            
            <div className="space-y-4">
              {currentReviews.map((review) => (
                <ReviewCard key={review.id} review={review} />
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 mt-8">
                <button
                  onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                  disabled={currentPage === 1}
                  className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                >
                  Previous
                </button>
                
                <div className="flex gap-2">
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                    <button
                      key={page}
                      onClick={() => setCurrentPage(page)}
                      className={`w-10 h-10 rounded-lg font-semibold transition-colors ${
                        currentPage === page
                          ? 'bg-primary text-white'
                          : 'border border-gray-300 hover:bg-gray-50'
                      }`}
                    >
                      {page}
                    </button>
                  ))}
                </div>

                <button
                  onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                  disabled={currentPage === totalPages}
                  className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                >
                  Next
                </button>
              </div>
            )}
          </div>
        </>
      )}

      {/* Review Form Modal */}
      {showReviewForm && (
        <ReviewForm
          onSubmit={handleSubmitReview}
          onCancel={() => setShowReviewForm(false)}
        />
      )}
    </div>
  );
}
