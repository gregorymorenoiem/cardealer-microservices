/**
 * HomepageSectionsService - API client for homepage sections
 * Connects via API Gateway to VehiclesSaleService /api/homepagesections
 */

import api from './api';

// ============================================================
// API CONFIGURATION
// ============================================================

// Use api instance with refresh token interceptor
const sectionsApi = api;

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
// API FUNCTIONS
// ============================================================

/**
 * Get all homepage sections with their vehicles
 * Uses endpoint: GET /api/homepagesections/homepage
 */
export const getHomepageSections = async (): Promise<HomepageSectionDto[]> => {
  try {
    const response = await sectionsApi.get<HomepageSectionDto[]>('/api/homepagesections/homepage');
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
    const response = await sectionsApi.get<HomepageSectionDto>(`/api/homepagesections/${slug}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching section ${slug}:`, error);
    return null;
  }
};

/**
 * Get all section configurations (admin use)
 * Uses endpoint: GET /api/homepagesections
 */
export const getAllSectionConfigs = async (): Promise<HomepageSectionDto[]> => {
  try {
    const response = await sectionsApi.get<HomepageSectionDto[]>('/api/homepagesections');
    return response.data;
  } catch (error) {
    console.error('Error fetching section configs:', error);
    throw error;
  }
};

export default {
  getHomepageSections,
  getHomepageSectionBySlug,
  getAllSectionConfigs,
};
