/**
 * Similar Vehicles Component
 * Shows related vehicles based on make, price range, etc.
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { ChevronRight } from 'lucide-react';
import { VehicleCard, VehicleCardSkeleton } from '@/components/ui/vehicle-card';
import { vehicleService } from '@/services/vehicles';
import type { VehicleCardData } from '@/types';

interface SimilarVehiclesProps {
  vehicleId: string;
  makeId?: string;
  priceRange?: number;
  limit?: number;
  className?: string;
}

export function SimilarVehicles({ vehicleId, limit = 4, className }: SimilarVehiclesProps) {
  const [vehicles, setVehicles] = React.useState<VehicleCardData[]>([]);
  const [isLoading, setIsLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    const fetchSimilar = async () => {
      try {
        setIsLoading(true);
        const data = await vehicleService.getSimilar(vehicleId, limit);
        setVehicles(data);
      } catch (err) {
        console.error('Error fetching similar vehicles:', err);
        setError('No se pudieron cargar vehículos similares');
      } finally {
        setIsLoading(false);
      }
    };

    fetchSimilar();
  }, [vehicleId, limit]);

  if (error) {
    return null; // Silently fail for similar vehicles
  }

  if (isLoading) {
    return (
      <div className={className}>
        <div className="mb-6 flex items-center justify-between">
          <h2 className="text-xl font-bold text-foreground">Vehículos similares</h2>
        </div>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: limit }).map((_, i) => (
            <VehicleCardSkeleton key={i} />
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
      <div className="mb-6 flex items-center justify-between">
        <h2 className="text-xl font-bold text-foreground">Vehículos similares</h2>
        <Link
          href="/vehiculos"
          className="text-primary flex items-center gap-1 text-sm hover:underline"
        >
          Ver todos
          <ChevronRight className="h-4 w-4" />
        </Link>
      </div>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {vehicles.map(vehicle => (
          <VehicleCard key={vehicle.id} vehicle={vehicle} />
        ))}
      </div>
    </div>
  );
}

export default SimilarVehicles;
