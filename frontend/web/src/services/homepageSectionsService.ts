/**
 * HomepageSectionsService - API client for homepage sections
 * Connects via API Gateway to VehiclesSaleService /api/homepagesections
 */

import axios from 'axios';

// ============================================================
// API CONFIGURATION
// ============================================================

// API Gateway URL - routes to VehiclesSaleService
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const HOMEPAGE_SECTIONS_API_URL = `${API_URL}/api/homepagesections`;

const sectionsApi = axios.create({
  baseURL: HOMEPAGE_SECTIONS_API_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

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
    const response = await sectionsApi.get<HomepageSectionDto[]>('/homepage');
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
export const getHomepageSectionBySlug = async (slug: string): Promise<HomepageSectionDto | null> => {
  try {
    const response = await sectionsApi.get<HomepageSectionDto>(`/${slug}`);
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
    const response = await sectionsApi.get<HomepageSectionDto[]>('/');
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
