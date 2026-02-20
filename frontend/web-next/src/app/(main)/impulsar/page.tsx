/**
 * Impulsar tu Vehículo — Seller Boost Page
 *
 * Landing page where individual sellers and dealers can promote their vehicles
 * using the OKLA advertising system. Shows pricing, benefits, and CTA.
 */

import { Suspense } from 'react';
import { getInternalApiUrl } from '@/lib/api-url';
import ImpulsarClient from './impulsar-client';

async function getPricingData() {
  try {
    const apiUrl = getInternalApiUrl();
    const [featuredRes, premiumRes] = await Promise.all([
      fetch(`${apiUrl}/api/advertising/reports/pricing/FeaturedSpot`, {
        next: { revalidate: 3600 },
        headers: { Accept: 'application/json' },
      }),
      fetch(`${apiUrl}/api/advertising/reports/pricing/PremiumSpot`, {
        next: { revalidate: 3600 },
        headers: { Accept: 'application/json' },
      }),
    ]);

    const featured = featuredRes.ok ? (await featuredRes.json()).data : null;
    const premium = premiumRes.ok ? (await premiumRes.json()).data : null;

    return { featured, premium };
  } catch (error) {
    console.error('[Impulsar] Error fetching pricing:', error);
    return { featured: null, premium: null };
  }
}

export default async function ImpulsarPage() {
  const pricing = await getPricingData();

  return (
    <Suspense fallback={<div className="bg-muted min-h-screen animate-pulse" />}>
      <ImpulsarClient pricing={pricing} />
    </Suspense>
  );
}
