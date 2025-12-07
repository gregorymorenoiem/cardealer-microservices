import React, { useState, forwardRef } from 'react';
import { clsx } from 'clsx';
import { Eye, EyeOff, AlertCircle, CheckCircle2 } from 'lucide-react';

/**
 * OKLA Input Component
 * 
 * Campos de entrada elegantes con animación de label flotante
 * y feedback visual sofisticado.
 */

export interface OklaInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label?: string;
  error?: string;
  success?: string;
  hint?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  size?: 'sm' | 'md' | 'lg';
  variant?: 'default' | 'filled' | 'flushed';
}

const sizeStyles = {
  sm: {
    input: 'h-9 text-sm px-3',
    label: 'text-xs',
    icon: 'w-4 h-4',
    iconPadding: 'pl-9',
  },
  md: {
    input: 'h-11 text-base px-4',
    label: 'text-sm',
    icon: 'w-5 h-5',
    iconPadding: 'pl-11',
  },
  lg: {
    input: 'h-14 text-lg px-5',
    label: 'text-base',
    icon: 'w-6 h-6',
    iconPadding: 'pl-14',
  },
};

const variantStyles = {
  default: clsx(
    'border border-gray-200 rounded-lg',
    'bg-white',
    'hover:border-gray-300',
    'focus:border-gold-500 focus:ring-2 focus:ring-gold-500/20',
    'dark:bg-gray-900 dark:border-gray-700',
    'dark:hover:border-gray-600',
    'dark:focus:border-gold-400 dark:focus:ring-gold-400/20'
  ),
  filled: clsx(
    'border-0 rounded-lg',
    'bg-gray-100',
    'hover:bg-gray-150',
    'focus:bg-white focus:ring-2 focus:ring-gold-500/20 focus:shadow-elegant',
    'dark:bg-gray-800',
    'dark:hover:bg-gray-750',
    'dark:focus:bg-gray-900'
  ),
  flushed: clsx(
    'border-0 border-b-2 border-gray-200 rounded-none',
    'bg-transparent px-0',
    'hover:border-gray-300',
    'focus:border-gold-500',
    'dark:border-gray-700',
    'dark:hover:border-gray-600',
    'dark:focus:border-gold-400'
  ),
};

export const OklaInput = forwardRef<HTMLInputElement, OklaInputProps>(
  (
    {
      className,
      label,
      error,
      success,
      hint,
      leftIcon,
      rightIcon,
      size = 'md',
      variant = 'default',
      type = 'text',
      disabled,
      id,
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = useState(false);
    const [isFocused, setIsFocused] = useState(false);
    
    const inputId = id || `okla-input-${Math.random().toString(36).substr(2, 9)}`;
    const isPassword = type === 'password';
    const inputType = isPassword && showPassword ? 'text' : type;
    
    const hasValue = props.value !== undefined && props.value !== '';
    const isFloating = isFocused || hasValue || props.placeholder;
    
    const currentSize = sizeStyles[size];

    return (
      <div className={clsx('relative w-full', className)}>
        {/* Input container */}
        <div className="relative">
          {/* Left icon */}
          {leftIcon && (
            <div 
              className={clsx(
                'absolute left-3 top-1/2 -translate-y-1/2',
                'text-gray-400',
                'pointer-events-none',
                currentSize.icon,
                isFocused && 'text-gold-500'
              )}
            >
              {leftIcon}
            </div>
          )}
          
          {/* Input field */}
          <input
            ref={ref}
            id={inputId}
            type={inputType}
            disabled={disabled}
            onFocus={(e) => {
              setIsFocused(true);
              props.onFocus?.(e);
            }}
            onBlur={(e) => {
              setIsFocused(false);
              props.onBlur?.(e);
            }}
            className={clsx(
              'w-full',
              'font-body',
              'text-gray-900 dark:text-white',
              'placeholder:text-gray-400',
              'transition-all duration-200 ease-elegant',
              'focus:outline-none',
              'disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-gray-50',
              
              // Size styles
              currentSize.input,
              
              // Variant styles
              variantStyles[variant],
              
              // Icon padding
              leftIcon && currentSize.iconPadding,
              (rightIcon || isPassword) && 'pr-11',
              
              // Error/Success states
              error && 'border-red-500 focus:border-red-500 focus:ring-red-500/20',
              success && 'border-green-500 focus:border-green-500 focus:ring-green-500/20'
            )}
            {...props}
          />
          
          {/* Floating label */}
          {label && (
            <label
              htmlFor={inputId}
              className={clsx(
                'absolute left-0 top-0',
                'pointer-events-none',
                'transition-all duration-200 ease-elegant',
                'origin-left',
                currentSize.label,
                
                // Position based on state
                isFloating ? [
                  '-translate-y-6',
                  'text-xs',
                  isFocused ? 'text-gold-600 dark:text-gold-400' : 'text-gray-500 dark:text-gray-400',
                ] : [
                  'top-1/2 -translate-y-1/2',
                  leftIcon ? currentSize.iconPadding : 'left-4',
                  'text-gray-400',
                ],
                
                // Error/Success label colors
                error && isFloating && 'text-red-500',
                success && isFloating && 'text-green-500'
              )}
            >
              {label}
            </label>
          )}
          
          {/* Right icon / Password toggle / Status icon */}
          <div className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center gap-2">
            {/* Status icons */}
            {error && (
              <AlertCircle className="w-5 h-5 text-red-500" />
            )}
            {success && !error && (
              <CheckCircle2 className="w-5 h-5 text-green-500" />
            )}
            
            {/* Password toggle */}
            {isPassword && (
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className={clsx(
                  'p-1 rounded-md',
                  'text-gray-400 hover:text-gray-600',
                  'transition-colors duration-150',
                  'focus:outline-none focus:ring-2 focus:ring-gold-500/20'
                )}
                tabIndex={-1}
              >
                {showPassword ? (
                  <EyeOff className={currentSize.icon} />
                ) : (
                  <Eye className={currentSize.icon} />
                )}
              </button>
            )}
            
            {/* Custom right icon */}
            {rightIcon && !isPassword && !error && !success && (
              <span className={clsx('text-gray-400', currentSize.icon)}>
                {rightIcon}
              </span>
            )}
          </div>
        </div>
        
        {/* Helper text */}
        {(error || success || hint) && (
          <p 
            className={clsx(
              'mt-1.5 text-xs',
              error && 'text-red-500',
              success && !error && 'text-green-500',
              !error && !success && 'text-gray-500'
            )}
          >
            {error || success || hint}
          </p>
        )}
      </div>
    );
  }
);

OklaInput.displayName = 'OklaInput';

/**
 * OKLA Textarea Component
 */
export interface OklaTextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  success?: string;
  hint?: string;
  resize?: 'none' | 'vertical' | 'horizontal' | 'both';
}

export const OklaTextarea = forwardRef<HTMLTextAreaElement, OklaTextareaProps>(
  (
    {
      className,
      label,
      error,
      success,
      hint,
      resize = 'vertical',
      disabled,
      id,
      ...props
    },
    ref
  ) => {
    const [isFocused, setIsFocused] = useState(false);
    const inputId = id || `okla-textarea-${Math.random().toString(36).substr(2, 9)}`;
    const hasValue = props.value !== undefined && props.value !== '';
    const isFloating = isFocused || hasValue || props.placeholder;

    const resizeStyles = {
      none: 'resize-none',
      vertical: 'resize-y',
      horizontal: 'resize-x',
      both: 'resize',
    };

    return (
      <div className={clsx('relative w-full', className)}>
        {/* Floating label */}
        {label && (
          <label
            htmlFor={inputId}
            className={clsx(
              'absolute left-0 top-0',
              'pointer-events-none',
              'transition-all duration-200 ease-elegant',
              'origin-left',
              
              isFloating ? [
                '-translate-y-6',
                'text-xs',
                isFocused ? 'text-gold-600 dark:text-gold-400' : 'text-gray-500',
              ] : [
                'top-3 left-4',
                'text-gray-400',
              ],
              
              error && isFloating && 'text-red-500',
              success && isFloating && 'text-green-500'
            )}
          >
            {label}
          </label>
        )}
        
        <textarea
          ref={ref}
          id={inputId}
          disabled={disabled}
          onFocus={(e) => {
            setIsFocused(true);
            props.onFocus?.(e);
          }}
          onBlur={(e) => {
            setIsFocused(false);
            props.onBlur?.(e);
          }}
          className={clsx(
            'w-full min-h-[120px]',
            'py-3 px-4',
            'font-body text-base',
            'text-gray-900 dark:text-white',
            'placeholder:text-gray-400',
            'border border-gray-200 rounded-lg',
            'bg-white dark:bg-gray-900',
            'hover:border-gray-300 dark:hover:border-gray-600',
            'focus:border-gold-500 focus:ring-2 focus:ring-gold-500/20',
            'dark:border-gray-700 dark:focus:border-gold-400',
            'transition-all duration-200 ease-elegant',
            'focus:outline-none',
            'disabled:opacity-50 disabled:cursor-not-allowed disabled:bg-gray-50',
            
            resizeStyles[resize],
            
            error && 'border-red-500 focus:border-red-500 focus:ring-red-500/20',
            success && 'border-green-500 focus:border-green-500 focus:ring-green-500/20'
          )}
          {...props}
        />
        
        {/* Helper text */}
        {(error || success || hint) && (
          <p 
            className={clsx(
              'mt-1.5 text-xs',
              error && 'text-red-500',
              success && !error && 'text-green-500',
              !error && !success && 'text-gray-500'
            )}
          >
            {error || success || hint}
          </p>
        )}
      </div>
    );
  }
);

OklaTextarea.displayName = 'OklaTextarea';

export default OklaInput;

