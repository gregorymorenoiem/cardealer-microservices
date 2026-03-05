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
import HomepageClient from './homepage-client';
import { LoadingSection } from '@/components/homepage';

// =============================================
// PAGE COMPONENT (Server Component)
// =============================================

export default function HomePage() {
  return (
    <Suspense fallback={<LoadingSection />}>
      <HomepageClient />
    </Suspense>
  );
}
