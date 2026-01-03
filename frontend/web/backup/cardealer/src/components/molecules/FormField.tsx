import { forwardRef } from 'react';
import type { InputHTMLAttributes, ReactNode } from 'react';
import Input from '../atoms/Input';
import clsx from 'clsx';

export interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  fullWidth?: boolean;
}

/**
 * FormField - Componente molecular que combina Label + Input + Error
 * Ideal para formularios con React Hook Form
 */
const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
  (
    {
      label,
      error,
      helperText,
      leftIcon,
      rightIcon,
      fullWidth = true,
      className,
      ...props
    },
    ref
  ) => {
    return (
      <div className={clsx('flex flex-col gap-1.5', fullWidth && 'w-full')}>
        <Input
          ref={ref}
          label={label}
          error={error}
          helperText={helperText}
          leftIcon={leftIcon}
          rightIcon={rightIcon}
          fullWidth={fullWidth}
          className={className}
          {...props}
        />
      </div>
    );
  }
);

FormField.displayName = 'FormField';

export default FormField;
