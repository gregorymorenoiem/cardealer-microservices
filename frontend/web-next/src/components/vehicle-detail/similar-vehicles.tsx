/**
 * Similar Vehicles Component
 * Shows related vehicles based on make, price range, etc.
 * Uses TanStack Query for caching, deduplication, and automatic retry.
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { ChevronRight } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { VehicleCard, VehicleCardSkeleton } from '@/components/ui/vehicle-card';
import { cn } from '@/lib/utils';
import { vehicleService } from '@/services/vehicles';
import type { VehicleCardData } from '@/types';

interface SimilarVehiclesProps {
  vehicleId: string;
  makeId?: string;
  priceRange?: number;
  limit?: number;
  variant?: 'default' | 'compact';
  className?: string;
}

export function SimilarVehicles({
  vehicleId,
  limit = 4,
  variant = 'default',
  className,
}: SimilarVehiclesProps) {
  const { data: vehicles = [], isLoading } = useQuery<VehicleCardData[]>({
    queryKey: ['similar-vehicles', vehicleId, limit],
    queryFn: () => vehicleService.getSimilar(vehicleId, limit),
    staleTime: 5 * 60 * 1000, // 5 min — similar vehicles don't change often
    gcTime: 10 * 60 * 1000, // 10 min garbage collection
    retry: 2,
    enabled: !!vehicleId,
  });

  if (isLoading) {
    return (
      <div className={className}>
        <div className="mb-4 flex items-center justify-between">
          <h2
            className={cn(
              variant === 'compact'
                ? 'text-sm font-semibold tracking-wide text-gray-700 uppercase'
                : 'text-foreground text-xl font-bold'
            )}
          >
            Vehículos similares
          </h2>
        </div>
        <div
          className={cn(
            variant === 'compact'
              ? 'grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4'
              : 'grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4'
          )}
        >
          {Array.from({ length: limit }).map((_, i) => (
            <VehicleCardSkeleton key={i} variant={variant} />
          ))}
        </div>
      </div>
    );
  }

  if (vehicles.length === 0) {
    return null; // Don't show section if no similar vehicles
  }

  return (
    <div className={className}>
      <div className="mb-4 flex items-center justify-between">
        <h2
          className={cn(
            variant === 'compact'
              ? 'text-sm font-semibold tracking-wide text-gray-700 uppercase'
              : 'text-foreground text-xl font-bold'
          )}
        >
          Vehículos similares
        </h2>
        <Link
          href="/vehiculos"
          className="text-primary flex items-center gap-1 text-sm hover:underline"
        >
          Ver todos
          <ChevronRight className="h-4 w-4" />
        </Link>
      </div>
      <div
        className={cn(
          variant === 'compact'
            ? 'grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4'
            : 'grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4'
        )}
      >
        {vehicles.map(vehicle => (
          <VehicleCard key={vehicle.id} vehicle={vehicle} variant={variant} />
        ))}
      </div>
    </div>
  );
}

export default SimilarVehicles;
