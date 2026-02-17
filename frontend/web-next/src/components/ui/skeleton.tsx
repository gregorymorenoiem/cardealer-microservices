import * as React from 'react';
import { cn } from '@/lib/utils';

// Skeleton component for loading states
const Skeleton = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      role="status"
      aria-busy="true"
      aria-label="Cargando"
      className={cn('bg-muted animate-pulse rounded-md', className)}
      {...props}
    />
  )
);
Skeleton.displayName = 'Skeleton';

// Avatar Skeleton
const SkeletonAvatar = ({ size = 'default' }: { size?: 'sm' | 'default' | 'lg' }) => {
  const sizes = {
    sm: 'h-8 w-8',
    default: 'h-10 w-10',
    lg: 'h-12 w-12',
  };

  return <Skeleton className={cn('rounded-full', sizes[size])} />;
};

// Text Skeleton
const SkeletonText = ({ lines = 1, className }: { lines?: number; className?: string }) => {
  return (
    <div className={cn('space-y-2', className)}>
      {Array.from({ length: lines }).map((_, i) => (
        <Skeleton
          key={i}
          className={cn('h-4', i === lines - 1 && lines > 1 ? 'w-3/4' : 'w-full')}
        />
      ))}
    </div>
  );
};

// Card Skeleton
const SkeletonCard = ({ className }: { className?: string }) => {
  return (
    <div className={cn('border-border bg-card rounded-xl border p-4', className)}>
      <Skeleton className="mb-4 aspect-video w-full rounded-lg" />
      <div className="space-y-3">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-4 w-1/2" />
        <div className="flex gap-2">
          <Skeleton className="h-6 w-16 rounded-full" />
          <Skeleton className="h-6 w-20 rounded-full" />
        </div>
        <div className="flex items-center justify-between pt-2">
          <Skeleton className="h-6 w-24" />
          <Skeleton className="h-9 w-24 rounded-lg" />
        </div>
      </div>
    </div>
  );
};

// Vehicle Card Skeleton
const SkeletonVehicleCard = ({ className }: { className?: string }) => {
  return (
    <div className={cn('border-border bg-card overflow-hidden rounded-xl border', className)}>
      {/* Image placeholder */}
      <Skeleton className="aspect-[4/3] w-full" />

      {/* Content */}
      <div className="space-y-3 p-4">
        {/* Title */}
        <div className="space-y-1">
          <Skeleton className="h-5 w-4/5" />
          <Skeleton className="h-4 w-2/3" />
        </div>

        {/* Specs */}
        <div className="flex gap-3">
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-14" />
        </div>

        {/* Price and CTA */}
        <div className="border-border flex items-center justify-between border-t pt-2">
          <div className="space-y-1">
            <Skeleton className="h-6 w-28" />
            <Skeleton className="h-4 w-20" />
          </div>
          <Skeleton className="h-9 w-24 rounded-lg" />
        </div>
      </div>
    </div>
  );
};

// Table Skeleton
const SkeletonTable = ({
  rows = 5,
  columns = 4,
  className,
}: {
  rows?: number;
  columns?: number;
  className?: string;
}) => {
  return (
    <div className={cn('border-border overflow-hidden rounded-lg border', className)}>
      {/* Header */}
      <div className="border-border bg-muted flex gap-4 border-b p-4">
        {Array.from({ length: columns }).map((_, i) => (
          <Skeleton key={i} className="h-4 flex-1" />
        ))}
      </div>
      {/* Rows */}
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <div key={rowIndex} className="border-border/50 flex gap-4 border-b p-4 last:border-0">
          {Array.from({ length: columns }).map((_, colIndex) => (
            <Skeleton key={colIndex} className="h-4 flex-1" />
          ))}
        </div>
      ))}
    </div>
  );
};

export { Skeleton, SkeletonAvatar, SkeletonText, SkeletonCard, SkeletonVehicleCard, SkeletonTable };
