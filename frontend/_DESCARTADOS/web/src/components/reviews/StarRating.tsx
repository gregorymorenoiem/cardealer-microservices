import React from 'react';

interface StarRatingProps {
  rating: number;
  maxRating?: number;
  size?: 'sm' | 'md' | 'lg';
  showValue?: boolean;
  interactive?: boolean;
  onRatingChange?: (rating: number) => void;
  className?: string;
}

/**
 * Componente de calificación con estrellas
 * Puede ser solo visual o interactivo para recoger input del usuario
 */
export const StarRating: React.FC<StarRatingProps> = ({
  rating,
  maxRating = 5,
  size = 'md',
  showValue = false,
  interactive = false,
  onRatingChange,
  className = '',
}) => {
  const [hoverRating, setHoverRating] = React.useState<number>(0);

  // Tamaños de estrellas
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
  };

  const handleStarClick = (starRating: number) => {
    if (interactive && onRatingChange) {
      onRatingChange(starRating);
    }
  };

  const handleStarHover = (starRating: number) => {
    if (interactive) {
      setHoverRating(starRating);
    }
  };

  const handleMouseLeave = () => {
    if (interactive) {
      setHoverRating(0);
    }
  };

  const getStarColor = (starIndex: number): string => {
    const currentRating = interactive ? hoverRating || rating : rating;

    if (starIndex <= currentRating) {
      return 'text-yellow-400'; // Estrella llena
    } else if (starIndex - 0.5 <= currentRating) {
      return 'text-yellow-400'; // Media estrella
    } else {
      return 'text-gray-300'; // Estrella vacía
    }
  };

  const renderStar = (index: number) => {
    const starRating = index + 1;
    const isHalf = rating % 1 !== 0 && starRating === Math.ceil(rating);

    return (
      <button
        key={index}
        type="button"
        disabled={!interactive}
        onClick={() => handleStarClick(starRating)}
        onMouseEnter={() => handleStarHover(starRating)}
        className={`
          ${sizeClasses[size]} 
          ${interactive ? 'cursor-pointer hover:scale-110 transition-transform' : 'cursor-default'}
          ${getStarColor(starRating)}
          focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-opacity-50 rounded
        `}
      >
        {isHalf ? (
          // Media estrella usando clip-path
          <svg viewBox="0 0 24 24" fill="currentColor" className="w-full h-full">
            <defs>
              <clipPath id={`half-star-${index}`}>
                <rect x="0" y="0" width="12" height="24" />
              </clipPath>
            </defs>
            {/* Estrella completa (fondo gris) */}
            <path
              d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
              className="text-gray-300"
            />
            {/* Media estrella (amarilla) */}
            <path
              d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
              clipPath={`url(#half-star-${index})`}
              className="text-yellow-400"
            />
          </svg>
        ) : (
          // Estrella completa normal
          <svg viewBox="0 0 24 24" fill="currentColor" className="w-full h-full">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
          </svg>
        )}
      </button>
    );
  };

  return (
    <div className={`flex items-center space-x-1 ${className}`} onMouseLeave={handleMouseLeave}>
      {/* Estrellas */}
      <div className="flex items-center space-x-0.5">
        {Array.from({ length: maxRating }, (_, index) => renderStar(index))}
      </div>

      {/* Valor numérico */}
      {showValue && (
        <span className="ml-2 text-sm text-gray-600 font-medium">{rating.toFixed(1)}</span>
      )}

      {/* Rating hover para modo interactivo */}
      {interactive && hoverRating > 0 && (
        <span className="ml-2 text-sm text-blue-600 font-medium">{hoverRating}.0</span>
      )}
    </div>
  );
};

export default StarRating;
