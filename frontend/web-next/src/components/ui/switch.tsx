/**
 * Switch Component
 *
 * A toggle switch control built on Radix UI
 * OKLA Theme - Verde #00A870
 */

'use client';

import * as React from 'react';
import * as SwitchPrimitives from '@radix-ui/react-switch';
import { cn } from '@/lib/utils';

const Switch = React.forwardRef<
  React.ComponentRef<typeof SwitchPrimitives.Root>,
  React.ComponentPropsWithoutRef<typeof SwitchPrimitives.Root>
>(({ className, checked, ...props }, ref) => (
  <SwitchPrimitives.Root
    className={cn(
      'peer focus-visible:ring-primary focus-visible:ring-offset-background inline-flex h-7 w-12 shrink-0 cursor-pointer items-center rounded-full border-2 shadow-inner transition-all duration-200 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50',
      // Use Tailwind classes for dark mode support
      checked ? 'border-primary bg-primary' : 'border-border bg-muted',
      className
    )}
    checked={checked}
    {...props}
    ref={ref}
  >
    <SwitchPrimitives.Thumb className="bg-background pointer-events-none block h-5 w-5 rounded-full shadow-lg ring-0 transition-transform duration-200 data-[state=checked]:translate-x-[22px] data-[state=unchecked]:translate-x-[2px]" />
  </SwitchPrimitives.Root>
));
Switch.displayName = SwitchPrimitives.Root.displayName;

export { Switch };
