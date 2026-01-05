/**
 * useHomepageSections - React hook for homepage sections with vehicles
 * 
 * Fetches configured sections from /api/homepagesections/homepage
 * Each section contains vehicles assigned by admin with configurable max items.
 */

import { useState, useEffect, useCallback } from 'react';
import { getHomepageSections, type HomepageSectionDto, type HomepageVehicleDto } from '@/services/homepageSectionsService';

// ============================================================
// TYPES
// ============================================================

export interface HomepageSection {
  id: string;
  name: string;
  slug: string;
  description: string;
  displayOrder: number;
  maxItems: number;
  isActive: boolean;
  icon: string | null;
  accentColor: string;
  viewAllHref: string;
  layoutType: 'Hero' | 'Carousel' | 'Grid' | 'Featured';
  subtitle: string;
  vehicles: HomepageVehicle[];
}

export interface HomepageVehicle {
  id: string;
  name: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  exteriorColor: string;
  bodyStyle: string;
  imageUrl: string;
  imageUrls: string[];
  sortOrder: number;
  isPinned: boolean;
}

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

// ============================================================
// HOOK
// ============================================================

export const useHomepageSections = (): UseHomepageSectionsResult => {
  const [sections, setSections] = useState<HomepageSection[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const transformSection = (dto: HomepageSectionDto): HomepageSection => ({
    id: dto.id,
    name: dto.name,
    slug: dto.slug,
    description: dto.description,
    displayOrder: dto.displayOrder,
    maxItems: dto.maxItems,
    isActive: dto.isActive,
    icon: dto.icon,
    accentColor: dto.accentColor || 'blue',
    viewAllHref: dto.viewAllHref || `/vehicles`,
    layoutType: dto.layoutType,
    subtitle: dto.subtitle || '',
    vehicles: dto.vehicles.map((v: HomepageVehicleDto) => ({
      id: v.id,
      name: v.name,
      make: v.make,
      model: v.model,
      year: v.year,
      price: v.price,
      mileage: v.mileage,
      fuelType: v.fuelType,
      transmission: v.transmission,
      exteriorColor: v.exteriorColor,
      bodyStyle: v.bodyStyle,
      imageUrl: v.imageUrl,
      imageUrls: v.imageUrls,
      sortOrder: v.sortOrder,
      isPinned: v.isPinned,
    })),
  });

  const fetchSections = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getHomepageSections();
      const transformed = data.map(transformSection);
      setSections(transformed);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch homepage sections');
      setSections([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchSections();
  }, [fetchSections]);

  const getSection = useCallback((slug: string) => {
    return sections.find(s => s.slug === slug);
  }, [sections]);

  return {
    sections,
    isLoading,
    error,
    refetch: fetchSections,
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

// Re-export types
export type { HomepageSectionDto, HomepageVehicleDto };
