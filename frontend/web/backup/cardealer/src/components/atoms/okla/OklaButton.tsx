import React from 'react';
import { clsx } from 'clsx';
import { Loader2 } from 'lucide-react';

/**
 * OKLA Button Component
 * 
 * Botones premium con diseño elegante y sofisticado
 * que transmiten profesionalidad y confianza.
 */

export interface OklaButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'gold' | 'danger';
  size?: 'sm' | 'md' | 'lg' | 'xl';
  isLoading?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  fullWidth?: boolean;
}

const variantStyles = {
  primary: clsx(
    'bg-gray-900 text-white',
    'hover:bg-gray-800 active:bg-gray-700',
    'focus-visible:ring-gray-500/50',
    'shadow-elegant hover:shadow-elegant-lg',
    'dark:bg-white dark:text-gray-900',
    'dark:hover:bg-gray-100'
  ),
  secondary: clsx(
    'bg-gold-500 text-gray-900',
    'hover:bg-gold-600 active:bg-gold-700',
    'focus-visible:ring-gold-500/50'
  ),
  outline: clsx(
    'bg-transparent text-gray-900',
    'border border-gray-900/20',
    'hover:bg-gray-900/5 hover:border-gray-900/40',
    'active:bg-gray-900/10',
    'focus-visible:ring-gray-500/50',
    'dark:text-white dark:border-white/20',
    'dark:hover:bg-white/5 dark:hover:border-white/40'
  ),
  ghost: clsx(
    'bg-transparent text-gray-900',
    'hover:bg-gray-900/5',
    'active:bg-gray-900/10',
    'focus-visible:ring-gray-500/50',
    'dark:text-white dark:hover:bg-white/5'
  ),
  gold: clsx(
    'bg-gradient-to-r from-gold-400 via-gold-500 to-gold-600',
    'text-gray-900 font-semibold',
    'hover:from-gold-500 hover:via-gold-600 hover:to-gold-700',
    'focus-visible:ring-gold-500/50'
  ),
  danger: clsx(
    'bg-red-600 text-white',
    'hover:bg-red-700 active:bg-red-800',
    'focus-visible:ring-red-500/50',
    'shadow-elegant hover:shadow-elegant-lg'
  ),
};

const sizeStyles = {
  sm: 'h-8 px-3 text-sm gap-1.5 rounded-md',
  md: 'h-10 px-4 text-sm gap-2 rounded-lg',
  lg: 'h-12 px-6 text-base gap-2.5 rounded-lg',
  xl: 'h-14 px-8 text-lg gap-3 rounded-xl',
};

const iconSizes = {
  sm: 'w-3.5 h-3.5',
  md: 'w-4 h-4',
  lg: 'w-5 h-5',
  xl: 'w-6 h-6',
};

export const OklaButton = React.forwardRef<HTMLButtonElement, OklaButtonProps>(
  (
    {
      className,
      variant = 'primary',
      size = 'md',
      isLoading = false,
      leftIcon,
      rightIcon,
      fullWidth = false,
      disabled,
      children,
      ...props
    },
    ref
  ) => {
    const isDisabled = disabled || isLoading;

    return (
      <button
        ref={ref}
        disabled={isDisabled}
        className={clsx(
          // Base styles
          'inline-flex items-center justify-center',
          'font-medium',
          'transition-all duration-200 ease-elegant',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
          'disabled:opacity-50 disabled:cursor-not-allowed disabled:pointer-events-none',
          
          // Variant styles
          variantStyles[variant],
          
          // Size styles
          sizeStyles[size],
          
          // Full width
          fullWidth && 'w-full',
          
          className
        )}
        {...props}
      >
        {/* Loading spinner */}
        {isLoading && (
          <Loader2 
            className={clsx(
              'animate-spin',
              iconSizes[size],
              children && 'mr-2'
            )} 
          />
        )}
        
        {/* Left icon */}
        {!isLoading && leftIcon && (
          <span className={clsx(iconSizes[size], 'flex-shrink-0')}>
            {leftIcon}
          </span>
        )}
        
        {/* Button text */}
        {children}
        
        {/* Right icon */}
        {!isLoading && rightIcon && (
          <span className={clsx(iconSizes[size], 'flex-shrink-0')}>
            {rightIcon}
          </span>
        )}
      </button>
    );
  }
);

OklaButton.displayName = 'OklaButton';

/**
 * Icon Button - Para botones solo con icono
 */
export interface OklaIconButtonProps extends Omit<OklaButtonProps, 'leftIcon' | 'rightIcon' | 'children'> {
  icon: React.ReactNode;
  'aria-label': string;
}

const iconButtonSizes = {
  sm: 'w-8 h-8',
  md: 'w-10 h-10',
  lg: 'w-12 h-12',
  xl: 'w-14 h-14',
};

export const OklaIconButton = React.forwardRef<HTMLButtonElement, OklaIconButtonProps>(
  ({ icon, size = 'md', className, ...props }, ref) => {
    return (
      <button
        ref={ref}
        className={clsx(
          'inline-flex items-center justify-center',
          'rounded-full',
          'transition-all duration-200 ease-elegant',
          'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2',
          'disabled:opacity-50 disabled:cursor-not-allowed',
          variantStyles[props.variant || 'ghost'],
          iconButtonSizes[size],
          className
        )}
        {...props}
      >
        <span className={iconSizes[size]}>
          {icon}
        </span>
      </button>
    );
  }
);

OklaIconButton.displayName = 'OklaIconButton';

/**
 * Button Group - Para agrupar botones
 */
interface OklaButtonGroupProps {
  children: React.ReactNode;
  className?: string;
  spacing?: 'tight' | 'normal' | 'loose';
}

const spacingStyles = {
  tight: 'gap-1',
  normal: 'gap-2',
  loose: 'gap-4',
};

export const OklaButtonGroup: React.FC<OklaButtonGroupProps> = ({
  children,
  className,
  spacing = 'normal',
}) => {
  return (
    <div className={clsx('inline-flex items-center', spacingStyles[spacing], className)}>
      {children}
    </div>
  );
};

export default OklaButton;
