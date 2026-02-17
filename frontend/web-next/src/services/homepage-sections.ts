/**
 * Homepage Sections Service - API client for homepage sections
 * Connects via API Gateway to VehiclesSaleService /api/homepagesections
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// API TYPES (matching backend HomepageSectionWithVehiclesDto)
// ============================================================

export interface HomepageVehicleDto {
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

export interface HomepageSectionDto {
  id: string;
  name: string;
  slug: string;
  description: string;
  displayOrder: number;
  maxItems: number;
  isActive: boolean;
  icon: string | null;
  accentColor: string | null;
  viewAllHref: string | null;
  layoutType: 'Hero' | 'Carousel' | 'Grid' | 'Featured';
  subtitle: string | null;
  vehicles: HomepageVehicleDto[];
}

// ============================================================
// TRANSFORMED TYPES (for components)
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

// ============================================================
// TRANSFORM FUNCTIONS
// ============================================================

export const transformSection = (dto: HomepageSectionDto): HomepageSection => ({
  id: dto.id,
  name: dto.name,
  slug: dto.slug,
  description: dto.description,
  displayOrder: dto.displayOrder,
  maxItems: dto.maxItems,
  isActive: dto.isActive,
  icon: dto.icon,
  accentColor: dto.accentColor || 'blue',
  viewAllHref: dto.viewAllHref || `/vehiculos`,
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

// Transform to Vehicle format for components
export interface Vehicle {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  exteriorColor: string;
  bodyStyle: string;
  images: string[];
  condition: 'New' | 'Used' | 'Certified Pre-Owned';
  location: string;
  tier?: 'basic' | 'featured' | 'premium' | 'enterprise';
  featuredBadge?: 'destacado' | 'premium' | 'certificado' | 'top-dealer';
}

export const transformHomepageVehicleToVehicle = (v: HomepageVehicle): Vehicle => ({
  id: v.id,
  make: v.make,
  model: v.model,
  year: v.year,
  price: v.price,
  mileage: v.mileage,
  fuelType: v.fuelType,
  transmission: v.transmission,
  exteriorColor: v.exteriorColor,
  bodyStyle: v.bodyStyle,
  images: v.imageUrls.length > 0 ? v.imageUrls : [v.imageUrl],
  condition: 'Used',
  location: 'Santo Domingo',
  tier: 'basic',
});

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get all homepage sections with their vehicles
 * Uses endpoint: GET /api/homepagesections/homepage
 */
export const getHomepageSections = async (): Promise<HomepageSectionDto[]> => {
  try {
    const response = await apiClient.get<HomepageSectionDto[]>('/api/homepagesections/homepage');
    return response.data;
  } catch (error) {
    console.error('Error fetching homepage sections:', error);
    throw error;
  }
};

/**
 * Get a specific section by slug with its vehicles
 * Uses endpoint: GET /api/homepagesections/{slug}
 */
export const getHomepageSectionBySlug = async (
  slug: string
): Promise<HomepageSectionDto | null> => {
  try {
    const response = await apiClient.get<HomepageSectionDto>(`/api/homepagesections/${slug}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching section ${slug}:`, error);
    return null;
  }
};
