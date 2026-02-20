/**
 * Star Rating Component
 * Interactive or read-only star rating display
 */

'use client';

import * as React from 'react';
import { Star } from 'lucide-react';
import { cn } from '@/lib/utils';

interface StarRatingProps {
  value: number;
  onChange?: (rating: number) => void;
  readonly?: boolean;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  showValue?: boolean;
}

const sizeMap = {
  sm: 'h-3.5 w-3.5',
  md: 'h-5 w-5',
  lg: 'h-7 w-7',
};

export function StarRating({
  value,
  onChange,
  readonly = false,
  size = 'md',
  className,
  showValue = false,
}: StarRatingProps) {
  const [hoverValue, setHoverValue] = React.useState<number | null>(null);
  const isInteractive = !readonly && !!onChange;
  const displayValue = hoverValue ?? value;

  return (
    <div
      className={cn('flex items-center gap-0.5', className)}
      role={isInteractive ? 'radiogroup' : 'img'}
      aria-label={`Calificación: ${value} de 5 estrellas`}
    >
      {[1, 2, 3, 4, 5].map(star => {
        const filled = star <= Math.round(displayValue);
        return (
          <button
            key={star}
            type="button"
            disabled={!isInteractive}
            className={cn(
              'transition-colors focus:outline-none',
              isInteractive &&
                'cursor-pointer rounded-sm hover:scale-110 focus:ring-2 focus:ring-amber-400',
              !isInteractive && 'cursor-default'
            )}
            onClick={() => isInteractive && onChange?.(star)}
            onMouseEnter={() => isInteractive && setHoverValue(star)}
            onMouseLeave={() => isInteractive && setHoverValue(null)}
            onKeyDown={e => {
              if (!isInteractive) return;
              if (e.key === 'ArrowRight' && star < 5) onChange?.(star + 1);
              if (e.key === 'ArrowLeft' && star > 1) onChange?.(star - 1);
            }}
            aria-label={`${star} estrella${star > 1 ? 's' : ''}`}
            role={isInteractive ? 'radio' : undefined}
            aria-checked={isInteractive ? star === Math.round(value) : undefined}
            tabIndex={
              isInteractive
                ? star === Math.round(value) || (value === 0 && star === 1)
                  ? 0
                  : -1
                : -1
            }
          >
            <Star
              className={cn(
                sizeMap[size],
                filled ? 'fill-amber-400 text-amber-400' : 'fill-transparent text-gray-300'
              )}
            />
          </button>
        );
      })}
      {showValue && (
        <span className="text-muted-foreground ml-1.5 text-sm font-medium">
          {value > 0 ? value.toFixed(1) : ''}
        </span>
      )}
    </div>
  );
}

export default StarRating;
