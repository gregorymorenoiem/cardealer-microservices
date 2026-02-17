/**
 * OKLA Homepage — Server Component
 *
 * Fetches homepage data server-side for fast First Contentful Paint.
 * Interactive components are rendered as client islands via HomepageClient.
 *
 * Performance: SSR sends fully-rendered HTML — critical for users in
 * Dominican Republic on mobile connections (~10-15 Mbps).
 */

import { Suspense } from 'react';
import { getInternalApiUrl } from '@/lib/api-url';
import { transformSection, type HomepageSectionDto } from '@/services/homepage-sections';
import HomepageClient from './homepage-client';
import { LoadingSection } from '@/components/homepage';

// =============================================
// SERVER-SIDE DATA FETCHING
// =============================================

async function getHomepageSections() {
  try {
    const apiUrl = getInternalApiUrl();
    const res = await fetch(`${apiUrl}/api/homepagesections/homepage`, {
      next: { revalidate: 300 }, // ISR: revalidate every 5 minutes
      headers: { Accept: 'application/json' },
    });

    if (!res.ok) {
      console.error(`[Homepage] Failed to fetch sections: ${res.status}`);
      return [];
    }

    const data: HomepageSectionDto[] = await res.json();
    return data.map(transformSection);
  } catch (error) {
    console.error('[Homepage] Error fetching sections:', error);
    return [];
  }
}

// =============================================
// PAGE COMPONENT (Server Component)
// =============================================

export default async function HomePage() {
  const sections = await getHomepageSections();

  return (
    <Suspense fallback={<LoadingSection />}>
      <HomepageClient sections={sections} />
    </Suspense>
  );
}
