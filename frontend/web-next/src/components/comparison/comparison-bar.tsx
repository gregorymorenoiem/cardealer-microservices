/**
 * ComparisonBar — Floating bottom bar showing vehicles selected for comparison.
 *
 * Renders only when 1+ vehicles are selected via `useLocalComparison`.
 * Shows vehicle thumbnails, count, and a CTA to navigate to /comparar.
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { X, ArrowLeftRight } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useLocalComparison } from '@/hooks/use-comparisons';
import { useVehicleDtosForComparison } from '@/hooks/use-vehicles';
import { Button } from '@/components/ui/button';

export function ComparisonBar() {
  const { vehicleIds, removeVehicle, clearAll } = useLocalComparison();
  const { data: vehicles } = useVehicleDtosForComparison(vehicleIds);

  // Don't render if no vehicles selected
  if (vehicleIds.length === 0) return null;

  return (
    <div className="border-border bg-background/95 fixed right-0 bottom-0 left-0 z-50 border-t shadow-2xl backdrop-blur-md">
      <div className="container mx-auto flex items-center justify-between gap-4 px-4 py-3">
        {/* Left: Vehicle thumbnails */}
        <div className="flex items-center gap-3">
          <div className="text-muted-foreground flex items-center gap-1 text-sm font-medium">
            <ArrowLeftRight className="h-4 w-4" />
            <span className="hidden sm:inline">Comparar</span>
          </div>

          <div className="flex items-center gap-2">
            {vehicleIds.map(id => {
              const vehicle = vehicles?.find(v => v.id === id);
              const imageUrl =
                vehicle?.images?.slice().sort((a, b) => (a.order ?? 99) - (b.order ?? 99))[0]
                  ?.url || '/placeholder-car.jpg';
              const label = vehicle
                ? `${vehicle.year} ${vehicle.make} ${vehicle.model}`
                : 'Cargando…';

              return (
                <div
                  key={id}
                  className="group/thumb border-border bg-muted/50 relative flex items-center gap-2 rounded-lg border p-1.5 pr-7"
                >
                  <div className="relative h-10 w-14 shrink-0 overflow-hidden rounded-md">
                    <Image src={imageUrl} alt={label} fill className="object-cover" sizes="56px" />
                  </div>
                  <span className="text-foreground hidden max-w-[120px] truncate text-xs font-medium md:block">
                    {label}
                  </span>
                  <button
                    onClick={() => removeVehicle(id)}
                    className="bg-muted text-muted-foreground hover:bg-destructive absolute top-1 right-1 flex h-5 w-5 items-center justify-center rounded-full transition-colors hover:text-white"
                    aria-label={`Quitar ${label} de la comparación`}
                  >
                    <X className="h-3 w-3" />
                  </button>
                </div>
              );
            })}

            {/* Empty slot indicators */}
            {Array.from({ length: 3 - vehicleIds.length }).map((_, i) => (
              <div
                key={`empty-${i}`}
                className="border-border text-muted-foreground/40 flex h-[52px] w-14 items-center justify-center rounded-lg border-2 border-dashed sm:w-24"
              >
                <span className="text-xs">+</span>
              </div>
            ))}
          </div>
        </div>

        {/* Right: Count + CTA */}
        <div className="flex items-center gap-3">
          <span className="text-muted-foreground text-sm font-medium">{vehicleIds.length}/3</span>

          {vehicleIds.length > 1 && (
            <Button asChild size="sm" className="gap-2">
              <Link href={`/comparar?ids=${vehicleIds.join(',')}`}>
                <ArrowLeftRight className="h-4 w-4" />
                <span className="hidden sm:inline">Comparar</span>
              </Link>
            </Button>
          )}

          <button
            onClick={clearAll}
            className={cn(
              'text-muted-foreground hover:text-destructive text-xs underline-offset-2 hover:underline'
            )}
          >
            Limpiar
          </button>
        </div>
      </div>
    </div>
  );
}
