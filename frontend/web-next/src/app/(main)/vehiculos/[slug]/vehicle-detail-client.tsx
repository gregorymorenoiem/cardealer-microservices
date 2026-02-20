/**
 * Vehicle Detail Client Component
 * Handles interactive parts of the vehicle detail page
 */

'use client';

import * as React from 'react';
import { Breadcrumbs } from '@/components/ui/breadcrumbs';
import {
  VehicleGallery,
  VehicleHeader,
  SellerCard,
  VehicleTabs,
  SimilarVehicles,
} from '@/components/vehicle-detail';
import { VehicleCardSkeleton } from '@/components/ui/vehicle-card';
import { ReviewsSection } from '@/components/reviews';
import type { Vehicle } from '@/types';

interface VehicleDetailClientProps {
  vehicle: Vehicle;
}

export function VehicleDetailClient({ vehicle }: VehicleDetailClientProps) {
  const title = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;

  return (
    <div className="bg-muted/50 min-h-screen">
      {/* Breadcrumbs */}
      <div className="border-border bg-card border-b">
        <div className="container py-3">
          <Breadcrumbs items={[{ label: 'Vehículos', href: '/vehiculos' }, { label: title }]} />
        </div>
      </div>

      {/* Main content */}
      <div className="container py-6 lg:py-8">
        <div className="grid gap-8 lg:grid-cols-3">
          {/* Left column - Gallery and Details */}
          <div className="space-y-6 lg:col-span-2">
            {/* Gallery */}
            <VehicleGallery
              images={vehicle.images ?? []}
              title={title}
              has360View={vehicle.has360View}
              hasVideo={vehicle.hasVideo}
              className="shadow-sm"
            />

            {/* Mobile: Header (hidden on desktop) */}
            <div className="lg:hidden">
              <VehicleHeader vehicle={vehicle} />
            </div>

            {/* Mobile: Seller Card */}
            <div className="lg:hidden">
              <SellerCard vehicle={vehicle} />
            </div>

            {/* Tabs - Description, Specs, Features */}
            <VehicleTabs vehicle={vehicle} />

            {/* Reviews for the seller/dealer */}
            <ReviewsSection
              targetId={vehicle.sellerId}
              targetType={vehicle.sellerType === 'dealer' ? 'dealer' : 'seller'}
              vehicleId={vehicle.id}
              vehicleTitle={title}
            />
          </div>

          {/* Right column - Sticky sidebar */}
          <div className="hidden lg:block">
            <div className="sticky top-24 space-y-6">
              {/* Header with price */}
              <VehicleHeader vehicle={vehicle} />

              {/* Seller info */}
              <SellerCard vehicle={vehicle} />
            </div>
          </div>
        </div>

        {/* Similar vehicles */}
        <div className="mt-12">
          <React.Suspense
            fallback={
              <div>
                <h2 className="text-foreground mb-6 text-xl font-bold">Vehículos similares</h2>
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
                  {Array.from({ length: 4 }).map((_, i) => (
                    <VehicleCardSkeleton key={i} />
                  ))}
                </div>
              </div>
            }
          >
            <SimilarVehicles vehicleId={vehicle.id} limit={4} />
          </React.Suspense>
        </div>
      </div>
    </div>
  );
}

export default VehicleDetailClient;
