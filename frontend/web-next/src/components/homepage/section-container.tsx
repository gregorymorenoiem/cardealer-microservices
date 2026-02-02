import { cn } from '@/lib/utils';
import { SectionHeader } from './section-header';

interface SectionContainerProps {
  title?: string;
  subtitle?: string;
  children: React.ReactNode;
  background?: 'white' | 'gray' | 'gradient';
  className?: string;
}

export function SectionContainer({
  title,
  subtitle,
  children,
  background = 'white',
  className,
}: SectionContainerProps) {
  const bgClasses = {
    white: 'bg-white',
    gray: 'bg-slate-50',
    gradient: 'bg-gradient-to-b from-white to-slate-50',
  };

  return (
    <section className={cn('py-12 lg:py-16', bgClasses[background], className)}>
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        {title && <SectionHeader title={title} subtitle={subtitle} size="lg" />}
        {children}
      </div>
    </section>
  );
}
