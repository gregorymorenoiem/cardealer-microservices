import type { LabelHTMLAttributes } from 'react';
import clsx from 'clsx';

export interface LabelProps extends LabelHTMLAttributes<HTMLLabelElement> {
  required?: boolean;
  error?: boolean;
}

export default function Label({
  children,
  required = false,
  error = false,
  className,
  ...props
}: LabelProps) {
  return (
    <label
      className={clsx(
        'text-sm font-medium',
        error ? 'text-red-600' : 'text-gray-700',
        className
      )}
      {...props}
    >
      {children}
      {required && <span className="text-red-500 ml-1">*</span>}
    </label>
  );
}
