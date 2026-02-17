import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FiThumbsUp, FiCheck } from 'react-icons/fi';
import type { Review } from '@/types/review';
import StarRating from '@/components/atoms/StarRating';

interface ReviewCardProps {
  review: Review;
}

export default function ReviewCard({ review }: ReviewCardProps) {
  const { t, i18n } = useTranslation('common');
  const [helpful, setHelpful] = useState(review.helpful);
  const [markedHelpful, setMarkedHelpful] = useState(false);

  const handleHelpful = () => {
    if (!markedHelpful) {
      setHelpful(prev => prev + 1);
      setMarkedHelpful(true);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    return date.toLocaleDateString(i18n.language === 'es' ? 'es-MX' : 'en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-start gap-3">
          <img
            src={review.userAvatar || 'https://i.pravatar.cc/150?u=default'}
            alt={review.userName}
            className="w-12 h-12 rounded-full object-cover"
          />
          <div>
            <div className="flex items-center gap-2">
              <h4 className="font-semibold text-gray-900">{review.userName}</h4>
              {review.verifiedPurchase && (
                <span className="flex items-center gap-1 text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full">
                  <FiCheck className="text-xs" />
                  {t('reviews.verifiedPurchase')}
                </span>
              )}
            </div>
            <p className="text-sm text-gray-500">{formatDate(review.date)}</p>
          </div>
        </div>
        <StarRating rating={review.rating} size="sm" />
      </div>

      {/* Title */}
      <h3 className="text-lg font-semibold text-gray-900 mb-2">
        {review.title}
      </h3>

      {/* Comment */}
      <p className="text-gray-700 mb-4 leading-relaxed">
        {review.comment}
      </p>

      {/* Pros & Cons */}
      {(review.pros && review.pros.length > 0) || (review.cons && review.cons.length > 0) ? (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          {review.pros && review.pros.length > 0 && (
            <div className="bg-green-50 rounded-lg p-3">
              <h5 className="text-sm font-semibold text-green-900 mb-2">{t('reviews.pros')}</h5>
              <ul className="space-y-1">
                {review.pros.map((pro, index) => (
                  <li key={index} className="text-sm text-green-800 flex items-start gap-2">
                    <span className="text-green-600 mt-0.5">+</span>
                    {pro}
                  </li>
                ))}
              </ul>
            </div>
          )}
          
          {review.cons && review.cons.length > 0 && (
            <div className="bg-red-50 rounded-lg p-3">
              <h5 className="text-sm font-semibold text-red-900 mb-2">{t('reviews.cons')}</h5>
              <ul className="space-y-1">
                {review.cons.map((con, index) => (
                  <li key={index} className="text-sm text-red-800 flex items-start gap-2">
                    <span className="text-red-600 mt-0.5">-</span>
                    {con}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      ) : null}

      {/* Photos */}
      {review.photos && review.photos.length > 0 && (
        <div className="mb-4">
          <div className="flex gap-2 overflow-x-auto pb-2">
            {review.photos.map((photo, index) => (
              <img
                key={index}
                src={photo}
                alt={t('reviews.reviewPhoto', { index: index + 1 })}
                className="h-32 w-48 object-cover rounded-lg cursor-pointer hover:opacity-90 transition-opacity"
              />
            ))}
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center gap-4 pt-4 border-t border-gray-100">
        <button
          onClick={handleHelpful}
          disabled={markedHelpful}
          className={`flex items-center gap-2 px-3 py-1.5 rounded-lg transition-colors ${
            markedHelpful
              ? 'bg-primary-100 text-primary-700 cursor-not-allowed'
              : 'hover:bg-gray-100 text-gray-600'
          }`}
        >
          <FiThumbsUp className={markedHelpful ? 'fill-primary-700' : ''} />
          <span className="text-sm font-medium">
            {helpful > 0 ? t('reviews.helpfulCount', { count: helpful }) : t('reviews.helpful')}
          </span>
        </button>
      </div>
    </div>
  );
}
