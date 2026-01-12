/**
 * FeaturedListingGrid Component
 * Sprint 3: FeaturedListingCard Component
 *
 * Grid layout for featured listings with 40% UX balance enforcement
 * Automatically mixes featured and organic listings
 */

import { useMemo } from 'react';
import type { Vehicle } from '@/services/vehicleService';
import FeaturedListingCard from './FeaturedListingCard';
import { mixFeaturedAndOrganic } from '@/utils/rankingAlgorithm';

interface FeaturedListingGridProps {
  vehicles: Vehicle[];
  page?: 'home' | 'browse' | 'detail';
  columns?: 2 | 3 | 4;
  maxItems?: number;
  className?: string;
}

const columnClasses = {
  2: 'grid-cols-1 md:grid-cols-2',
  3: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
  4: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
};

export default function FeaturedListingGrid({
  vehicles,
  page = 'browse',
  columns = 3,
  maxItems,
  className = '',
}: FeaturedListingGridProps) {
  // Apply ranking algorithm with 40% rule
  const rankedVehicles = useMemo(() => {
    const mixed = mixFeaturedAndOrganic(vehicles, page);
    return maxItems ? mixed.slice(0, maxItems) : mixed;
  }, [vehicles, page, maxItems]);

  // Determine priority for first few items (for lazy loading)
  const getPriority = (index: number): 'high' | 'normal' => {
    return index < 4 ? 'high' : 'normal';
  };

  return (
    <div className={`grid ${columnClasses[columns]} gap-4 ${className}`}>
      {rankedVehicles.map((vehicle, index) => (
        <FeaturedListingCard key={vehicle.id} vehicle={vehicle} priority={getPriority(index)} />
      ))}
    </div>
  );
}
