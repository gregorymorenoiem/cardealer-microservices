/**
 * Stats Card Component
 *
 * Reusable card for displaying statistics with icon, value, and optional trend.
 * Used in account, admin, and dealer dashboards.
 */

import * as React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export type StatColor =
  | 'blue'
  | 'green'
  | 'purple'
  | 'yellow'
  | 'red'
  | 'primary'
  | 'amber'
  | 'slate';

export type TrendDirection = 'up' | 'down' | 'neutral';

export interface StatsCardProps {
  /** Title/label of the stat */
  title: string;
  /** Main value to display */
  value: number | string;
  /** Icon component to display */
  icon: React.ElementType;
  /** Color theme */
  color?: StatColor;
  /** Additional suffix text (e.g., reviews count) */
  suffix?: string;
  /** Trend percentage change */
  change?: string;
  /** Trend direction */
  trend?: TrendDirection;
  /** Custom className */
  className?: string;
  /** Format value as currency */
  formatAsCurrency?: boolean;
  /** Currency symbol for formatting */
  currencySymbol?: string;
}

// =============================================================================
// COLOR CONFIGURATIONS
// =============================================================================

const COLOR_CLASSES: Record<StatColor, { bg: string; text: string }> = {
  blue: { bg: 'bg-blue-100', text: 'text-blue-600' },
  green: { bg: 'bg-green-100', text: 'text-green-600' },
  primary: { bg: 'bg-primary/10', text: 'text-primary' },
  purple: { bg: 'bg-purple-100', text: 'text-purple-600' },
  yellow: { bg: 'bg-yellow-100', text: 'text-yellow-600' },
  amber: { bg: 'bg-amber-100', text: 'text-amber-600' },
  red: { bg: 'bg-red-100', text: 'text-red-600' },
  slate: { bg: 'bg-slate-100', text: 'text-slate-600' },
};

// =============================================================================
// COMPONENT
// =============================================================================

/**
 * Stats Card - displays a single statistic with icon and optional trend
 *
 * @example
 * // Basic usage
 * <StatsCard title="Users" value={1234} icon={Users} color="blue" />
 *
 * @example
 * // With trend
 * <StatsCard
 *   title="Revenue"
 *   value="RD$ 50K"
 *   icon={DollarSign}
 *   color="green"
 *   change="+12%"
 *   trend="up"
 * />
 */
export function StatsCard({
  title,
  value,
  icon: Icon,
  color = 'blue',
  suffix,
  change,
  trend,
  className,
}: StatsCardProps) {
  const colorClasses = COLOR_CLASSES[color];
  const TrendIcon = trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;

  return (
    <Card className={className}>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className={cn('rounded-lg p-3', colorClasses.bg)}>
            <Icon className={cn('h-6 w-6', colorClasses.text)} />
          </div>
          {change && trend && (
            <div
              className={cn(
                'flex items-center gap-1 text-sm',
                trend === 'up' && 'text-primary',
                trend === 'down' && 'text-red-600',
                trend === 'neutral' && 'text-gray-500'
              )}
            >
              <TrendIcon className="h-4 w-4" />
              {change}
            </div>
          )}
        </div>
        <div className="mt-4">
          <p className="text-foreground text-2xl font-bold">
            {typeof value === 'number' ? value.toLocaleString() : value}
            {suffix && (
              <span className="text-muted-foreground ml-1 text-sm font-normal">{suffix}</span>
            )}
          </p>
          <p className="text-muted-foreground text-sm">{title}</p>
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// COMPACT VARIANT
// =============================================================================

export interface CompactStatsCardProps {
  title: string;
  value: number | string;
  icon: React.ElementType;
  color?: StatColor;
  className?: string;
}

/**
 * Compact Stats Card - smaller variant for grids with many stats
 */
export function CompactStatsCard({
  title,
  value,
  icon: Icon,
  color = 'blue',
  className,
}: CompactStatsCardProps) {
  const colorClasses = COLOR_CLASSES[color];

  return (
    <Card className={className}>
      <CardContent className="p-4">
        <div className="flex items-center gap-3">
          <div className={cn('rounded-lg p-2', colorClasses.bg)}>
            <Icon className={cn('h-5 w-5', colorClasses.text)} />
          </div>
          <div>
            <p className="text-foreground text-xl font-bold">
              {typeof value === 'number' ? value.toLocaleString() : value}
            </p>
            <p className="text-muted-foreground text-sm">{title}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// =============================================================================
// HORIZONTAL VARIANT
// =============================================================================

/**
 * Horizontal Stats Card - icon on left, value and title on right
 * Similar to the original StatsCard in cuenta/page.tsx
 */
export function HorizontalStatsCard({
  title,
  value,
  icon: Icon,
  color = 'blue',
  suffix,
  className,
}: Omit<StatsCardProps, 'change' | 'trend'>) {
  const colorClasses = COLOR_CLASSES[color];

  return (
    <Card className={className}>
      <CardContent className="pt-6">
        <div className="flex items-center gap-3">
          <div
            className={cn('flex h-10 w-10 items-center justify-center rounded-lg', colorClasses.bg)}
          >
            <Icon className={cn('h-5 w-5', colorClasses.text)} />
          </div>
          <div>
            <p className="text-foreground text-2xl font-bold">
              {typeof value === 'number' ? value.toLocaleString() : value}
              {suffix && (
                <span className="text-muted-foreground ml-1 text-sm font-normal">{suffix}</span>
              )}
            </p>
            <p className="text-muted-foreground text-sm">{title}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
