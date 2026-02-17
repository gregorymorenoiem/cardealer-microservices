/**
 * Quick Action Component
 *
 * Reusable component for displaying quick action links in dashboards.
 * Used in account, admin, and dealer dashboards.
 */

import * as React from 'react';
import Link from 'next/link';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export type ActionColor =
  | 'blue'
  | 'green'
  | 'purple'
  | 'yellow'
  | 'red'
  | 'primary'
  | 'amber'
  | 'slate';

export interface QuickActionProps {
  /** Link destination */
  href: string;
  /** Icon component */
  icon: React.ElementType;
  /** Action label */
  label: string;
  /** Optional description */
  description?: string;
  /** Color theme */
  color?: ActionColor;
  /** Custom className */
  className?: string;
  /** Badge to show (e.g., count) */
  badge?: string | number;
  /** Whether the action is disabled */
  disabled?: boolean;
}

// =============================================================================
// COLOR CONFIGURATIONS
// =============================================================================

const COLOR_CLASSES: Record<ActionColor, { bg: string; text: string; hover: string }> = {
  blue: {
    bg: 'bg-blue-100',
    text: 'text-blue-600',
    hover: 'group-hover:bg-blue-200',
  },
  green: {
    bg: 'bg-green-100',
    text: 'text-green-600',
    hover: 'group-hover:bg-green-200',
  },
  primary: {
    bg: 'bg-primary/10',
    text: 'text-primary',
    hover: 'group-hover:bg-primary/20',
  },
  purple: {
    bg: 'bg-purple-100',
    text: 'text-purple-600',
    hover: 'group-hover:bg-purple-200',
  },
  yellow: {
    bg: 'bg-yellow-100',
    text: 'text-yellow-600',
    hover: 'group-hover:bg-yellow-200',
  },
  amber: {
    bg: 'bg-amber-100',
    text: 'text-amber-600',
    hover: 'group-hover:bg-amber-200',
  },
  red: {
    bg: 'bg-red-100',
    text: 'text-red-600',
    hover: 'group-hover:bg-red-200',
  },
  slate: {
    bg: 'bg-slate-100',
    text: 'text-slate-600',
    hover: 'group-hover:bg-slate-200',
  },
};

// =============================================================================
// COMPONENTS
// =============================================================================

/**
 * Quick Action - vertical card style with centered icon and label
 *
 * @example
 * <QuickAction
 *   href="/vender"
 *   icon={Plus}
 *   label="Publicar Vehículo"
 *   color="green"
 * />
 */
export function QuickAction({
  href,
  icon: Icon,
  label,
  color = 'blue',
  className,
  badge,
  disabled,
}: QuickActionProps) {
  const colorClasses = COLOR_CLASSES[color];

  const content = (
    <div
      className={cn(
        'group border-border flex flex-col items-center gap-3 rounded-lg border p-4 transition-all',
        !disabled && 'hover:border-border cursor-pointer hover:shadow-sm',
        disabled && 'cursor-not-allowed opacity-50',
        className
      )}
    >
      <div
        className={cn(
          'flex h-12 w-12 items-center justify-center rounded-lg transition-colors',
          colorClasses.bg,
          !disabled && colorClasses.hover
        )}
      >
        <Icon className={cn('h-6 w-6', colorClasses.text)} />
      </div>
      <div className="flex items-center gap-2">
        <span className="text-foreground text-center text-sm font-medium">{label}</span>
        {badge !== undefined && (
          <span className="bg-primary/10 text-primary rounded-full px-2 py-0.5 text-xs font-medium">
            {badge}
          </span>
        )}
      </div>
    </div>
  );

  if (disabled) {
    return content;
  }

  return <Link href={href}>{content}</Link>;
}

/**
 * Quick Action Button - horizontal button style for action rows
 *
 * @example
 * <QuickActionButton
 *   href="/admin/usuarios"
 *   icon={Users}
 *   label="Gestionar Usuarios"
 * />
 */
export function QuickActionButton({
  href,
  icon: Icon,
  label,
  description,
  className,
  badge,
  disabled,
}: QuickActionProps) {
  const content = (
    <div
      className={cn(
        'border-border flex w-full items-center justify-start gap-3 rounded-lg border px-4 py-3 transition-colors',
        !disabled && 'hover:bg-muted cursor-pointer',
        disabled && 'cursor-not-allowed opacity-50',
        className
      )}
    >
      <Icon className="text-muted-foreground h-4 w-4" />
      <div className="flex-1 text-left">
        <span className="text-foreground text-sm font-medium">{label}</span>
        {description && <p className="text-muted-foreground text-xs">{description}</p>}
      </div>
      {badge !== undefined && (
        <span className="bg-primary/10 text-primary rounded-full px-2 py-0.5 text-xs font-medium">
          {badge}
        </span>
      )}
    </div>
  );

  if (disabled) {
    return content;
  }

  return <Link href={href}>{content}</Link>;
}

/**
 * Quick Action Card - larger card with description
 *
 * @example
 * <QuickActionCard
 *   href="/dealer/publicar"
 *   icon={Upload}
 *   label="Publicar Vehículo"
 *   description="Agrega un nuevo vehículo a tu inventario"
 *   color="green"
 * />
 */
export function QuickActionCard({
  href,
  icon: Icon,
  label,
  description,
  color = 'blue',
  className,
  badge,
  disabled,
}: QuickActionProps) {
  const colorClasses = COLOR_CLASSES[color];

  const content = (
    <div
      className={cn(
        'group border-border rounded-lg border p-4 transition-all',
        !disabled && 'hover:border-primary/50 cursor-pointer hover:shadow-md',
        disabled && 'cursor-not-allowed opacity-50',
        className
      )}
    >
      <div className="flex items-start gap-4">
        <div
          className={cn(
            'flex h-12 w-12 shrink-0 items-center justify-center rounded-lg transition-colors',
            colorClasses.bg,
            !disabled && colorClasses.hover
          )}
        >
          <Icon className={cn('h-6 w-6', colorClasses.text)} />
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <h3 className="text-foreground font-medium">{label}</h3>
            {badge !== undefined && (
              <span className="bg-primary/10 text-primary rounded-full px-2 py-0.5 text-xs font-medium">
                {badge}
              </span>
            )}
          </div>
          {description && <p className="text-muted-foreground mt-1 text-sm">{description}</p>}
        </div>
      </div>
    </div>
  );

  if (disabled) {
    return content;
  }

  return <Link href={href}>{content}</Link>;
}
