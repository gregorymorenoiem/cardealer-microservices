import { cn } from '@/lib/utils';

interface SectionHeaderProps {
  title: string;
  subtitle?: string;
  align?: 'left' | 'center' | 'right';
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export function SectionHeader({
  title,
  subtitle,
  align = 'center',
  size = 'md',
  className,
}: SectionHeaderProps) {
  const alignmentClasses = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right',
  };

  const titleSizes = {
    sm: 'text-xl font-bold leading-tight tracking-tight text-foreground md:text-2xl',
    md: 'text-2xl font-bold leading-tight tracking-tight text-foreground md:text-3xl',
    lg: 'text-3xl font-bold leading-tight tracking-tight text-foreground md:text-4xl',
  };

  const subtitleSizes = {
    sm: 'text-sm leading-relaxed text-muted-foreground',
    md: 'text-base leading-relaxed text-muted-foreground',
    lg: 'text-lg leading-relaxed text-muted-foreground',
  };

  return (
    <div className={cn('mb-8', alignmentClasses[align], className)}>
      <h2 className={cn('mb-3', titleSizes[size])}>{title}</h2>
      {subtitle && <p className={cn('mx-auto max-w-2xl', subtitleSizes[size])}>{subtitle}</p>}
    </div>
  );
}
