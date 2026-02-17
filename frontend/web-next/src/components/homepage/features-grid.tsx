'use client';

import { type LucideIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { SectionHeader } from './section-header';
import { useInView } from '@/hooks/use-in-view';

export interface Feature {
  icon: LucideIcon;
  title: string;
  description: string;
}

interface FeaturesGridProps {
  title?: string;
  subtitle?: string;
  features: Feature[];
  columns?: 2 | 3 | 4;
  className?: string;
}

export function FeaturesGrid({
  title = 'Todo lo que Necesitas',
  subtitle = 'Compra y vende vehículos de manera fácil, rápida y segura',
  features,
  columns = 4,
  className,
}: FeaturesGridProps) {
  const { ref, inView } = useInView();
  const gridCols = {
    2: 'sm:grid-cols-2',
    3: 'sm:grid-cols-2 lg:grid-cols-3',
    4: 'sm:grid-cols-2 lg:grid-cols-4',
  };

  return (
    <section className={cn('bg-background py-6', className)}>
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <SectionHeader title={title} subtitle={subtitle} size="md" />

        <div ref={ref} className={cn('grid grid-cols-1 gap-4', gridCols[columns])}>
          {features.map((feature, index) => (
            <div
              key={index}
              className={cn(
                'bg-muted rounded-2xl p-4 text-center transition-all duration-500',
                inView ? 'translate-y-0 opacity-100' : 'translate-y-5 opacity-0'
              )}
              style={{ transitionDelay: `${index * 100}ms` }}
            >
              <div className="bg-primary/10 mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-xl">
                <feature.icon className="text-primary h-6 w-6" />
              </div>
              <h3 className="text-foreground mb-1 text-lg font-semibold">{feature.title}</h3>
              <p className="text-muted-foreground text-sm">{feature.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
