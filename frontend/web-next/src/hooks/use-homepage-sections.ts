/**
 * useHomepageSections - React hook for homepage sections with vehicles
 *
 * Fetches configured sections from /api/homepagesections/homepage
 * Each section contains vehicles assigned by admin with configurable max items.
 */

'use client';

import { useQuery } from '@tanstack/react-query';
import {
  getHomepageSections,
  transformSection,
  type HomepageSection,
} from '@/services/homepage-sections';

export interface UseHomepageSectionsResult {
  sections: HomepageSection[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
  // Helper getters for specific sections
  getSection: (slug: string) => HomepageSection | undefined;
  carousel: HomepageSection | undefined;
  sedanes: HomepageSection | undefined;
  suvs: HomepageSection | undefined;
  camionetas: HomepageSection | undefined;
  deportivos: HomepageSection | undefined;
  destacados: HomepageSection | undefined;
  lujo: HomepageSection | undefined;
}

export const useHomepageSections = (): UseHomepageSectionsResult => {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['homepage-sections'],
    queryFn: async () => {
      const data = await getHomepageSections();
      return data.map(transformSection);
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
  });

  const sections = data || [];

  const getSection = (slug: string) => {
    return sections.find(s => s.slug === slug);
  };

  return {
    sections,
    isLoading,
    error: error
      ? error instanceof Error
        ? error.message
        : 'Failed to fetch homepage sections'
      : null,
    refetch,
    getSection,
    // Named sections for convenience
    carousel: sections.find(s => s.slug === 'carousel'),
    sedanes: sections.find(s => s.slug === 'sedanes'),
    suvs: sections.find(s => s.slug === 'suvs'),
    camionetas: sections.find(s => s.slug === 'camionetas'),
    deportivos: sections.find(s => s.slug === 'deportivos'),
    destacados: sections.find(s => s.slug === 'destacados'),
    lujo: sections.find(s => s.slug === 'lujo'),
  };
};

export default useHomepageSections;
