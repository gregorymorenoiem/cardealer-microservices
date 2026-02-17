import React from 'react';
import { clsx } from 'clsx';
import { motion } from 'framer-motion';

/**
 * OKLA Card Component
 * 
 * Tarjetas sofisticadas con efectos de elevación sutiles
 * y transiciones elegantes.
 */

export interface OklaCardProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'elevated' | 'outlined' | 'glass' | 'premium';
  padding?: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  hover?: boolean;
  clickable?: boolean;
  as?: 'div' | 'article' | 'section' | typeof motion.div;
}

const variantStyles = {
  default: clsx(
    'bg-white dark:bg-gray-900',
    'border border-gray-100 dark:border-gray-800',
    'shadow-card'
  ),
  elevated: clsx(
    'bg-white dark:bg-gray-900',
    'shadow-elegant-lg'
  ),
  outlined: clsx(
    'bg-transparent',
    'border border-gray-200 dark:border-gray-700'
  ),
  glass: clsx(
    'bg-white/70 dark:bg-gray-900/70',
    'backdrop-blur-md',
    'border border-white/20 dark:border-gray-700/50'
  ),
  premium: clsx(
    'bg-gradient-to-br from-white to-gold-50',
    'dark:from-gray-900 dark:to-gray-800',
    'border border-gold-200/50 dark:border-gold-700/30'
  ),
};

const paddingStyles = {
  none: '',
  sm: 'p-3',
  md: 'p-4 sm:p-5',
  lg: 'p-5 sm:p-6',
  xl: 'p-6 sm:p-8',
};

export const OklaCard: React.FC<OklaCardProps> = ({
  className,
  children,
  variant = 'default',
  padding = 'md',
  hover = false,
  clickable = false,
  as: Component = 'div',
  ...props
}) => {
  const isMotion = Component === motion.div;
  
  const cardClasses = clsx(
    'rounded-xl',
    'transition-all duration-300 ease-elegant',
    variantStyles[variant],
    paddingStyles[padding],
    
    // Hover effects
    hover && [
      'hover:shadow-elegant-xl',
      'hover:-translate-y-0.5',
    ],
    
    // Clickable
    clickable && 'cursor-pointer',
    
    className
  );

  if (isMotion) {
    return (
      <motion.div
        className={cardClasses}
        whileHover={hover ? { y: -2 } : undefined}
        transition={{ duration: 0.2, ease: [0.4, 0, 0.2, 1] }}
        {...(props as React.ComponentProps<typeof motion.div>)}
      >
        {children}
      </motion.div>
    );
  }

  const Comp = Component as React.ElementType;
  return (
    <Comp className={cardClasses} {...props}>
      {children}
    </Comp>
  );
};

/**
 * Card Header
 */
export interface OklaCardHeaderProps extends Omit<React.HTMLAttributes<HTMLDivElement>, 'title'> {
  title?: React.ReactNode;
  subtitle?: React.ReactNode;
  action?: React.ReactNode;
}

export const OklaCardHeader: React.FC<OklaCardHeaderProps> = ({
  className,
  children,
  title,
  subtitle,
  action,
  ...props
}) => {
  return (
    <div 
      className={clsx(
        'flex items-start justify-between gap-4',
        'mb-4',
        className
      )} 
      {...props}
    >
      <div className="flex-1 min-w-0">
        {title && (
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white truncate">
            {title}
          </h3>
        )}
        {subtitle && (
          <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
            {subtitle}
          </p>
        )}
        {children}
      </div>
      {action && (
        <div className="flex-shrink-0">
          {action}
        </div>
      )}
    </div>
  );
};

/**
 * Card Body
 */
export interface OklaCardBodyProps extends React.HTMLAttributes<HTMLDivElement> {}

export const OklaCardBody: React.FC<OklaCardBodyProps> = ({
  className,
  children,
  ...props
}) => {
  return (
    <div className={clsx('', className)} {...props}>
      {children}
    </div>
  );
};

/**
 * Card Footer
 */
export interface OklaCardFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  separator?: boolean;
}

export const OklaCardFooter: React.FC<OklaCardFooterProps> = ({
  className,
  children,
  separator = true,
  ...props
}) => {
  return (
    <div 
      className={clsx(
        'mt-4 pt-4',
        separator && 'border-t border-gray-100 dark:border-gray-800',
        className
      )} 
      {...props}
    >
      {children}
    </div>
  );
};

/**
 * Card Image
 */
export interface OklaCardImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  position?: 'top' | 'bottom';
  aspectRatio?: 'auto' | 'square' | 'video' | 'wide';
  overlay?: boolean;
}

const aspectRatioStyles = {
  auto: '',
  square: 'aspect-square',
  video: 'aspect-video',
  wide: 'aspect-[2/1]',
};

export const OklaCardImage: React.FC<OklaCardImageProps> = ({
  className,
  position = 'top',
  aspectRatio = 'video',
  overlay = false,
  alt = '',
  ...props
}) => {
  return (
    <div 
      className={clsx(
        'relative overflow-hidden',
        aspectRatioStyles[aspectRatio],
        position === 'top' ? '-mt-4 -mx-4 sm:-mt-5 sm:-mx-5 mb-4' : '-mb-4 -mx-4 sm:-mb-5 sm:-mx-5 mt-4',
        position === 'top' ? 'rounded-t-xl' : 'rounded-b-xl',
        className
      )}
    >
      <img
        alt={alt}
        className="w-full h-full object-cover"
        {...props}
      />
      {overlay && (
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent" />
      )}
    </div>
  );
};

/**
 * Product Card - Tarjeta específica para productos
 */
export interface OklaProductCardProps {
  image: string;
  title: string;
  price: string | number;
  originalPrice?: string | number;
  rating?: number;
  reviews?: number;
  badge?: string;
  badgeColor?: 'gold' | 'green' | 'red' | 'blue';
  isFavorite?: boolean;
  onFavoriteClick?: () => void;
  onClick?: () => void;
  className?: string;
}

const badgeColors = {
  gold: 'bg-gold-500 text-gray-900',
  green: 'bg-green-500 text-white',
  red: 'bg-red-500 text-white',
  blue: 'bg-blue-500 text-white',
};

export const OklaProductCard: React.FC<OklaProductCardProps> = ({
  image,
  title,
  price,
  originalPrice,
  rating,
  reviews,
  badge,
  badgeColor = 'gold',
  isFavorite = false,
  onFavoriteClick,
  onClick,
  className,
}) => {
  return (
    <motion.article
      className={clsx(
        'group relative',
        'bg-white dark:bg-gray-900',
        'rounded-xl overflow-hidden',
        'border border-gray-100 dark:border-gray-800',
        'shadow-card hover:shadow-elegant-xl',
        'transition-all duration-300',
        'cursor-pointer',
        className
      )}
      whileHover={{ y: -4 }}
      transition={{ duration: 0.25, ease: [0.4, 0, 0.2, 1] }}
      onClick={onClick}
    >
      {/* Image container */}
      <div className="relative aspect-[4/3] overflow-hidden bg-gray-100 dark:bg-gray-800">
        <img
          src={image}
          alt={title}
          className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
        />
        
        {/* Badge */}
        {badge && (
          <span 
            className={clsx(
              'absolute top-3 left-3',
              'px-2.5 py-1 rounded-full',
              'text-xs font-semibold uppercase tracking-wide',
              badgeColors[badgeColor]
            )}
          >
            {badge}
          </span>
        )}
        
        {/* Favorite button */}
        <button
          onClick={(e) => {
            e.stopPropagation();
            onFavoriteClick?.();
          }}
          className={clsx(
            'absolute top-3 right-3',
            'w-9 h-9 rounded-full',
            'flex items-center justify-center',
            'bg-white/90 dark:bg-gray-900/90',
            'backdrop-blur-sm',
            'transition-all duration-200',
            'hover:bg-white dark:hover:bg-gray-900',
            'hover:scale-110',
            isFavorite ? 'text-red-500' : 'text-gray-400 hover:text-red-500'
          )}
        >
          <svg
            className="w-5 h-5"
            fill={isFavorite ? 'currentColor' : 'none'}
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
            />
          </svg>
        </button>
        
        {/* Gradient overlay on hover */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
      </div>
      
      {/* Content */}
      <div className="p-4">
        <h3 className="text-base font-semibold text-gray-900 dark:text-white line-clamp-2 mb-2">
          {title}
        </h3>
        
        {/* Rating */}
        {rating !== undefined && (
          <div className="flex items-center gap-1.5 mb-3">
            <div className="flex items-center">
              {[...Array(5)].map((_, i) => (
                <svg
                  key={i}
                  className={clsx(
                    'w-4 h-4',
                    i < Math.floor(rating) ? 'text-gold-500' : 'text-gray-200 dark:text-gray-700'
                  )}
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
              ))}
            </div>
            {reviews !== undefined && (
              <span className="text-sm text-gray-500 dark:text-gray-400">
                ({reviews})
              </span>
            )}
          </div>
        )}
        
        {/* Price */}
        <div className="flex items-baseline gap-2">
          <span className="text-xl font-bold text-gray-900 dark:text-white">
            {typeof price === 'number' ? `$${price.toLocaleString()}` : price}
          </span>
          {originalPrice && (
            <span className="text-sm text-gray-400 line-through">
              {typeof originalPrice === 'number' ? `$${originalPrice.toLocaleString()}` : originalPrice}
            </span>
          )}
        </div>
      </div>
    </motion.article>
  );
};

export default OklaCard;

