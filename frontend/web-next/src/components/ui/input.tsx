import * as React from 'react';
import { cn } from '@/lib/utils';

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, label, error, hint, leftIcon, rightIcon, id, ...props }, ref) => {
    const generatedId = React.useId();
    const inputId = id || generatedId;
    const errorId = `${inputId}-error`;
    const hintId = `${inputId}-hint`;

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="text-foreground mb-1.5 block text-sm font-medium">
            {label}
            {props.required && <span className="text-destructive ml-1">*</span>}
          </label>
        )}
        <div className="relative">
          {leftIcon && (
            <div className="text-muted-foreground pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
              {leftIcon}
            </div>
          )}
          <input
            type={type}
            id={inputId}
            data-lpignore="true"
            data-1p-ignore
            className={cn(
              'bg-background text-foreground flex h-10 w-full rounded-lg border px-3 py-2 text-sm shadow-sm transition-colors',
              'placeholder:text-muted-foreground',
              'focus:border-primary focus:ring-primary/20 focus:ring-2 focus:outline-none',
              'disabled:bg-muted disabled:text-muted-foreground disabled:cursor-not-allowed',
              error
                ? 'border-destructive focus:border-destructive focus:ring-destructive/20'
                : 'border-border hover:border-muted-foreground',
              leftIcon && 'pl-10',
              rightIcon && 'pr-10',
              className
            )}
            ref={ref}
            aria-invalid={error ? 'true' : 'false'}
            aria-describedby={error ? errorId : hint ? hintId : undefined}
            {...props}
          />
          {rightIcon && (
            <div className="text-muted-foreground absolute inset-y-0 right-0 flex items-center pr-3">
              {rightIcon}
            </div>
          )}
        </div>
        {error && (
          <p id={errorId} className="text-destructive mt-1.5 text-sm" role="alert">
            {error}
          </p>
        )}
        {hint && !error && (
          <p id={hintId} className="text-muted-foreground mt-1.5 text-sm">
            {hint}
          </p>
        )}
      </div>
    );
  }
);
Input.displayName = 'Input';

export { Input };
