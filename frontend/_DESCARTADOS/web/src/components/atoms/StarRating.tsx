import { FiStar } from 'react-icons/fi';

interface StarRatingProps {
  rating: number; // 0-5, can be decimal like 4.5
  maxStars?: number;
  size?: 'sm' | 'md' | 'lg';
  showNumber?: boolean;
  interactive?: boolean;
  onChange?: (rating: number) => void;
}

export default function StarRating({
  rating,
  maxStars = 5,
  size = 'md',
  showNumber = false,
  interactive = false,
  onChange,
}: StarRatingProps) {
  const sizeClasses = {
    sm: 'text-sm',
    md: 'text-lg',
    lg: 'text-2xl',
  };

  const handleClick = (index: number) => {
    if (interactive && onChange) {
      onChange(index + 1);
    }
  };

  return (
    <div className="flex items-center gap-1">
      {Array.from({ length: maxStars }, (_, index) => {
        const fillPercentage = Math.min(Math.max((rating - index) * 100, 0), 100);
        
        return (
          <div
            key={index}
            className={`relative ${interactive ? 'cursor-pointer' : ''}`}
            onClick={() => handleClick(index)}
          >
            {/* Background star (empty) */}
            <FiStar className={`${sizeClasses[size]} text-gray-300`} />
            
            {/* Foreground star (filled) */}
            <div
              className="absolute top-0 left-0 overflow-hidden"
              style={{ width: `${fillPercentage}%` }}
            >
              <FiStar
                className={`${sizeClasses[size]} text-yellow-400 fill-yellow-400`}
              />
            </div>
          </div>
        );
      })}
      
      {showNumber && (
        <span className="ml-1 text-sm text-gray-600 font-medium">
          {rating.toFixed(1)}
        </span>
      )}
    </div>
  );
}
