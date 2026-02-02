/**
 * Vehicle Detail Page
 *
 * Shows complete details of a single vehicle including:
 * - Image gallery with lightbox
 * - Vehicle specs and features
 * - Seller information and contact
 * - Similar vehicles
 *
 * Route: /vehiculos/[slug]
 */

import { Metadata } from 'next';
import { notFound } from 'next/navigation';
import { Suspense } from 'react';
import { VehicleDetailClient } from './vehicle-detail-client';
import { vehicleService } from '@/services/vehicles';
import { formatCurrency } from '@/lib/utils';

interface VehiclePageProps {
  params: Promise<{ slug: string }>;
}

// Generate metadata for SEO
export async function generateMetadata({ params }: VehiclePageProps): Promise<Metadata> {
  const { slug } = await params;

  try {
    const vehicle = await vehicleService.getBySlug(slug);

    const title = `${vehicle.year} ${vehicle.make} ${vehicle.model} ${vehicle.trim || ''} | OKLA`;
    const description = `Compra ${vehicle.year} ${vehicle.make} ${vehicle.model} en ${vehicle.location.city}. ${formatCurrency(vehicle.price)}. ${vehicle.mileage?.toLocaleString() || 0} km. ${vehicle.condition === 'new' ? 'Nuevo' : 'Usado'}.`;
    const imageUrl = vehicle.images?.[0]?.url;

    return {
      title,
      description,
      openGraph: {
        title,
        description,
        images: imageUrl ? [imageUrl] : [],
        type: 'website',
        siteName: 'OKLA',
        locale: 'es_DO',
      },
      twitter: {
        card: 'summary_large_image',
        title,
        description,
        images: imageUrl ? [imageUrl] : [],
      },
      other: {
        'og:price:amount': vehicle.price.toString(),
        'og:price:currency': vehicle.currency,
      },
    };
  } catch {
    return {
      title: 'Vehículo no encontrado | OKLA',
      description: 'El vehículo que buscas no está disponible.',
    };
  }
}

// Server component for initial data fetching
export default async function VehiclePage({ params }: VehiclePageProps) {
  const { slug } = await params;

  let vehicle;
  try {
    vehicle = await vehicleService.getBySlug(slug);
  } catch (error) {
    console.error('Error fetching vehicle:', error);
    notFound();
  }

  if (!vehicle) {
    notFound();
  }

  // Track view (fire and forget)
  vehicleService.trackView(vehicle.id).catch(() => {
    // Silently fail - view tracking is non-critical
  });

  return (
    <Suspense fallback={<VehicleDetailSkeleton />}>
      <VehicleDetailClient vehicle={vehicle} />
    </Suspense>
  );
}

// Loading skeleton
function VehicleDetailSkeleton() {
  return (
    <div className="min-h-screen animate-pulse bg-gray-50">
      {/* Breadcrumb skeleton */}
      <div className="border-b bg-white">
        <div className="container py-3">
          <div className="h-4 w-64 rounded bg-gray-200"></div>
        </div>
      </div>

      {/* Main content skeleton */}
      <div className="container py-6 lg:py-8">
        <div className="grid gap-8 lg:grid-cols-3">
          {/* Left column */}
          <div className="space-y-6 lg:col-span-2">
            {/* Gallery skeleton */}
            <div className="rounded-xl bg-white p-4">
              <div className="aspect-[16/10] rounded-lg bg-gray-200"></div>
              <div className="mt-3 flex gap-2">
                {[1, 2, 3, 4, 5].map(i => (
                  <div key={i} className="h-12 w-16 flex-shrink-0 rounded-lg bg-gray-200"></div>
                ))}
              </div>
            </div>

            {/* Tabs skeleton */}
            <div className="rounded-xl bg-white p-6">
              <div className="flex gap-4 border-b pb-3">
                <div className="h-6 w-24 rounded bg-gray-200"></div>
                <div className="h-6 w-32 rounded bg-gray-200"></div>
                <div className="h-6 w-28 rounded bg-gray-200"></div>
              </div>
              <div className="mt-4 space-y-3">
                <div className="h-4 w-full rounded bg-gray-200"></div>
                <div className="h-4 w-3/4 rounded bg-gray-200"></div>
                <div className="h-4 w-5/6 rounded bg-gray-200"></div>
              </div>
            </div>
          </div>

          {/* Right column */}
          <div className="hidden space-y-6 lg:block">
            {/* Header skeleton */}
            <div className="rounded-xl bg-white p-6">
              <div className="mb-2 h-8 w-48 rounded bg-gray-200"></div>
              <div className="mb-4 h-6 w-32 rounded bg-gray-200"></div>
              <div className="mt-6 h-10 w-full rounded bg-gray-200"></div>
            </div>

            {/* Seller skeleton */}
            <div className="rounded-xl bg-white p-6">
              <div className="flex gap-4">
                <div className="h-14 w-14 rounded-full bg-gray-200"></div>
                <div className="flex-1">
                  <div className="mb-2 h-5 w-32 rounded bg-gray-200"></div>
                  <div className="h-4 w-24 rounded bg-gray-200"></div>
                </div>
              </div>
              <div className="mt-6 space-y-3">
                <div className="h-10 w-full rounded bg-gray-200"></div>
                <div className="h-10 w-full rounded bg-gray-200"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
