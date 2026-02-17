/**
 * Dealer Profile Page — Server Component
 *
 * Wraps the DealerProfileClient with SEO metadata and ISR caching.
 * The client component handles all interactivity (tabs, reviews, etc.)
 *
 * Performance: SSR sends HTML shell with Suspense fallback while client hydrates.
 * SEO: Metadata is generated server-side for crawlers.
 *
 * Route: /dealers/[slug]
 */

import { Suspense } from 'react';
import type { Metadata } from 'next';
import DealerProfileClient from './dealer-profile-client';
import { Skeleton } from '@/components/ui/skeleton';

// ISR: Revalidate every 5 minutes
export const revalidate = 300;

// Allow dynamic slugs
export const dynamicParams = true;

interface DealerPageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: DealerPageProps): Promise<Metadata> {
  const { slug } = await params;
  const dealerName = slug
    .split('-')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');

  return {
    title: `${dealerName} | OKLA Dealers`,
    description: `Visita el perfil de ${dealerName} en OKLA. Encuentra su inventario de vehículos, reseñas y ubicación en República Dominicana.`,
    openGraph: {
      title: `${dealerName} | OKLA Dealers`,
      description: `Perfil del concesionario ${dealerName} en OKLA.`,
      type: 'website',
      siteName: 'OKLA',
      locale: 'es_DO',
    },
  };
}

function DealerProfileSkeleton() {
  return (
    <div className="bg-muted/50 min-h-screen">
      <Skeleton className="h-48 w-full md:h-64 lg:h-80" />
      <div className="relative z-10 container mx-auto -mt-16 px-4">
        <div className="bg-card rounded-xl p-6 shadow-lg">
          <div className="flex flex-col gap-6 md:flex-row">
            <Skeleton className="-mt-16 h-24 w-24 rounded-xl md:-mt-20 md:h-32 md:w-32" />
            <div className="flex-1 space-y-3">
              <Skeleton className="h-8 w-64" />
              <Skeleton className="h-4 w-96" />
              <div className="flex gap-4">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-24" />
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-4 lg:col-span-2">
            <Skeleton className="h-10 w-64" />
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              {[1, 2, 3, 4].map(i => (
                <Skeleton key={i} className="h-64 w-full" />
              ))}
            </div>
          </div>
          <div className="space-y-6">
            <Skeleton className="h-48 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        </div>
      </div>
    </div>
  );
}

export default function DealerProfilePage({ params }: DealerPageProps) {
  return (
    <Suspense fallback={<DealerProfileSkeleton />}>
      <DealerProfileClient params={params} />
    </Suspense>
  );
}
