import React from 'react';
import { clsx } from 'clsx';
import { X, Check, Star, Shield, Sparkles, Crown, Verified } from 'lucide-react';

/**
 * OKLA Badge Component
 * 
 * Badges y tags elegantes para estados de productos,
 * categorías e indicadores visuales.
 */

export interface OklaBadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  variant?: 'default' | 'outline' | 'subtle' | 'solid';
  color?: 'gold' | 'gray' | 'green' | 'red' | 'blue' | 'purple' | 'orange';
  size?: 'xs' | 'sm' | 'md' | 'lg';
  icon?: React.ReactNode;
  closable?: boolean;
  onClose?: () => void;
}

const colorStyles = {
  gold: {
    default: 'bg-gold-100 text-gold-800 dark:bg-gold-900/30 dark:text-gold-300',
    outline: 'border-gold-300 text-gold-700 dark:border-gold-600 dark:text-gold-400',
    subtle: 'bg-gold-50 text-gold-600 dark:bg-gold-900/20 dark:text-gold-400',
    solid: 'bg-gold-500 text-white',
  },
  gray: {
    default: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
    outline: 'border-gray-300 text-gray-600 dark:border-gray-600 dark:text-gray-400',
    subtle: 'bg-gray-50 text-gray-500 dark:bg-gray-800/50 dark:text-gray-400',
    solid: 'bg-gray-700 text-white dark:bg-gray-600',
  },
  green: {
    default: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300',
    outline: 'border-green-300 text-green-700 dark:border-green-600 dark:text-green-400',
    subtle: 'bg-green-50 text-green-600 dark:bg-green-900/20 dark:text-green-400',
    solid: 'bg-green-600 text-white',
  },
  red: {
    default: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300',
    outline: 'border-red-300 text-red-700 dark:border-red-600 dark:text-red-400',
    subtle: 'bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400',
    solid: 'bg-red-600 text-white',
  },
  blue: {
    default: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300',
    outline: 'border-blue-300 text-blue-700 dark:border-blue-600 dark:text-blue-400',
    subtle: 'bg-blue-50 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400',
    solid: 'bg-blue-600 text-white',
  },
  purple: {
    default: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-300',
    outline: 'border-purple-300 text-purple-700 dark:border-purple-600 dark:text-purple-400',
    subtle: 'bg-purple-50 text-purple-600 dark:bg-purple-900/20 dark:text-purple-400',
    solid: 'bg-purple-600 text-white',
  },
  orange: {
    default: 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-300',
    outline: 'border-orange-300 text-orange-700 dark:border-orange-600 dark:text-orange-400',
    subtle: 'bg-orange-50 text-orange-600 dark:bg-orange-900/20 dark:text-orange-400',
    solid: 'bg-orange-500 text-white',
  },
};

const sizeStyles = {
  xs: 'px-1.5 py-0.5 text-[10px] gap-0.5',
  sm: 'px-2 py-0.5 text-xs gap-1',
  md: 'px-2.5 py-1 text-xs gap-1.5',
  lg: 'px-3 py-1.5 text-sm gap-2',
};

const iconSizes = {
  xs: 'w-2.5 h-2.5',
  sm: 'w-3 h-3',
  md: 'w-3.5 h-3.5',
  lg: 'w-4 h-4',
};

export const OklaBadge: React.FC<OklaBadgeProps> = ({
  className,
  children,
  variant = 'default',
  color = 'gray',
  size = 'md',
  icon,
  closable = false,
  onClose,
  ...props
}) => {
  return (
    <span
      className={clsx(
        'inline-flex items-center justify-center',
        'font-medium uppercase tracking-wide',
        'rounded-full',
        'transition-colors duration-150',
        
        // Variant + Color
        colorStyles[color][variant],
        
        // Outline needs border
        variant === 'outline' && 'border',
        
        // Size
        sizeStyles[size],
        
        className
      )}
      {...props}
    >
      {icon && (
        <span className={iconSizes[size]}>
          {icon}
        </span>
      )}
      
      {children}
      
      {closable && (
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onClose?.();
          }}
          className={clsx(
            'ml-1 -mr-0.5 rounded-full',
            'hover:bg-black/10 dark:hover:bg-white/10',
            'focus:outline-none focus:ring-1 focus:ring-current',
            'transition-colors duration-150',
            iconSizes[size]
          )}
        >
          <X className="w-full h-full" />
        </button>
      )}
    </span>
  );
};

/**
 * Status Badge - Para estados específicos
 */
export interface OklaStatusBadgeProps {
  status: 'new' | 'featured' | 'premium' | 'verified' | 'sale' | 'sold' | 'pending' | 'active';
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const statusConfig = {
  new: { label: 'Nuevo', color: 'green' as const, icon: Sparkles },
  featured: { label: 'Destacado', color: 'gold' as const, icon: Star },
  premium: { label: 'Premium', color: 'purple' as const, icon: Crown },
  verified: { label: 'Verificado', color: 'blue' as const, icon: Verified },
  sale: { label: 'Oferta', color: 'red' as const, icon: null },
  sold: { label: 'Vendido', color: 'gray' as const, icon: Check },
  pending: { label: 'Pendiente', color: 'orange' as const, icon: null },
  active: { label: 'Activo', color: 'green' as const, icon: null },
};

export const OklaStatusBadge: React.FC<OklaStatusBadgeProps> = ({
  status,
  size = 'md',
  className,
}) => {
  const config = statusConfig[status];
  const IconComponent = config.icon;
  
  return (
    <OklaBadge
      variant="solid"
      color={config.color}
      size={size}
      icon={IconComponent ? <IconComponent className="w-full h-full" /> : undefined}
      className={className}
    >
      {config.label}
    </OklaBadge>
  );
};

/**
 * Trust Badge - Para elementos de confianza
 */
export interface OklaTrustBadgeProps {
  type: 'secure' | 'guarantee' | 'verified-seller' | 'fast-shipping' | 'returns';
  size?: 'sm' | 'md' | 'lg';
  showLabel?: boolean;
  className?: string;
}

const trustBadgeConfig = {
  'secure': { label: 'Pago Seguro', icon: Shield },
  'guarantee': { label: 'Garantía', icon: Check },
  'verified-seller': { label: 'Vendedor Verificado', icon: Verified },
  'fast-shipping': { label: 'Envío Rápido', icon: null },
  'returns': { label: '30 Días Devolución', icon: null },
};

export const OklaTrustBadge: React.FC<OklaTrustBadgeProps> = ({
  type,
  size = 'md',
  showLabel = true,
  className,
}) => {
  const config = trustBadgeConfig[type];
  const IconComponent = config.icon;
  
  const iconSizeMap = {
    sm: 'w-4 h-4',
    md: 'w-5 h-5',
    lg: 'w-6 h-6',
  };
  
  const textSizeMap = {
    sm: 'text-xs',
    md: 'text-sm',
    lg: 'text-base',
  };

  return (
    <div 
      className={clsx(
        'inline-flex items-center gap-2',
        'text-gray-600 dark:text-gray-400',
        className
      )}
    >
      {IconComponent && (
        <IconComponent className={clsx(iconSizeMap[size], 'text-gold-500')} />
      )}
      {showLabel && (
        <span className={clsx('font-medium', textSizeMap[size])}>
          {config.label}
        </span>
      )}
    </div>
  );
};

/**
 * Discount Badge - Para descuentos
 */
export interface OklaDiscountBadgeProps {
  discount: number | string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export const OklaDiscountBadge: React.FC<OklaDiscountBadgeProps> = ({
  discount,
  size = 'md',
  className,
}) => {
  const displayDiscount = typeof discount === 'number' ? `-${discount}%` : discount;
  
  return (
    <OklaBadge
      variant="solid"
      color="red"
      size={size}
      className={clsx('font-bold', className)}
    >
      {displayDiscount}
    </OklaBadge>
  );
};

/**
 * Rating Badge - Para calificaciones
 */
export interface OklaRatingBadgeProps {
  rating: number;
  reviews?: number;
  size?: 'sm' | 'md' | 'lg';
  showReviews?: boolean;
  className?: string;
}

export const OklaRatingBadge: React.FC<OklaRatingBadgeProps> = ({
  rating,
  reviews,
  size = 'md',
  showReviews = true,
  className,
}) => {
  const textSizeMap = {
    sm: 'text-xs',
    md: 'text-sm',
    lg: 'text-base',
  };
  
  const starSizeMap = {
    sm: 'w-3 h-3',
    md: 'w-4 h-4',
    lg: 'w-5 h-5',
  };

  return (
    <div 
      className={clsx(
        'inline-flex items-center gap-1.5',
        textSizeMap[size],
        className
      )}
    >
      <Star className={clsx(starSizeMap[size], 'text-gold-500 fill-gold-500')} />
      <span className="font-semibold text-gray-900 dark:text-white">
        {rating.toFixed(1)}
      </span>
      {showReviews && reviews !== undefined && (
        <span className="text-gray-500 dark:text-gray-400">
          ({reviews.toLocaleString()})
        </span>
      )}
    </div>
  );
};

export default OklaBadge;

