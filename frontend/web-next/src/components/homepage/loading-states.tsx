import { Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface LoadingSectionProps {
  message?: string;
  className?: string;
}

export function LoadingSection({
  message = 'Cargando vehículos...',
  className,
}: LoadingSectionProps) {
  return (
    <section className={cn('bg-muted py-12', className)}>
      <div className="mx-auto max-w-7xl px-4 text-center">
        <Loader2 className="text-primary mx-auto mb-4 h-12 w-12 animate-spin" />
        <p className="text-muted-foreground">{message}</p>
      </div>
    </section>
  );
}

interface ErrorSectionProps {
  title?: string;
  message?: string;
  className?: string;
}

export function ErrorSection({
  title = 'Error al cargar vehículos',
  message,
  className,
}: ErrorSectionProps) {
  return (
    <section className={cn('bg-red-50 py-12', className)}>
      <div className="mx-auto max-w-7xl px-4 text-center">
        <p className="mb-2 text-red-600">{title}</p>
        {message && <p className="text-sm text-red-500">{message}</p>}
      </div>
    </section>
  );
}

interface SkeletonGridProps {
  count?: number;
  columns?: 2 | 3 | 4;
  className?: string;
}

export function SkeletonGrid({ count = 6, columns = 3, className }: SkeletonGridProps) {
  const gridCols = {
    2: 'md:grid-cols-2',
    3: 'md:grid-cols-2 lg:grid-cols-3',
    4: 'md:grid-cols-2 lg:grid-cols-4',
  };

  return (
    <div className={cn('grid grid-cols-1 gap-4 md:gap-6', gridCols[columns], className)}>
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="bg-muted aspect-[4/3] animate-pulse rounded-xl" />
      ))}
    </div>
  );
}
