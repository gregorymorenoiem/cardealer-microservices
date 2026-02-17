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

// ============================================================
// ADMIN API FUNCTIONS
// ============================================================

export interface CreateSectionRequest {
  name: string;
  slug: string;
  description?: string;
  displayOrder: number;
  maxItems: number;
  isActive?: boolean;
  icon?: string;
  accentColor?: string;
  viewAllHref?: string;
  layoutType?: 'Hero' | 'Carousel' | 'Grid' | 'Featured';
  subtitle?: string;
}

export interface UpdateSectionRequest {
  name?: string;
  description?: string;
  displayOrder?: number;
  maxItems?: number;
  isActive?: boolean;
  icon?: string;
  accentColor?: string;
  viewAllHref?: string;
  layoutType?: 'Hero' | 'Carousel' | 'Grid' | 'Featured';
  subtitle?: string;
}

export interface AssignVehicleRequest {
  vehicleId: string;
  sortOrder?: number;
  isPinned?: boolean;
  startDate?: string;
  endDate?: string;
}

/**
 * Create a new homepage section (admin only)
 * Uses endpoint: POST /api/homepagesections
 */
export const createSection = async (request: CreateSectionRequest): Promise<HomepageSectionDto> => {
  try {
    const response = await sectionsApi.post<HomepageSectionDto>('/api/homepagesections', request);
    return response.data;
  } catch (error) {
    console.error('Error creating section:', error);
    throw error;
  }
};

/**
 * Update an existing homepage section (admin only)
 * Uses endpoint: PUT /api/homepagesections/{slug}
 */
export const updateSection = async (
  slug: string,
  request: UpdateSectionRequest
): Promise<HomepageSectionDto> => {
  try {
    const response = await sectionsApi.put<HomepageSectionDto>(
      `/api/homepagesections/${slug}`,
      request
    );
    return response.data;
  } catch (error) {
    console.error(`Error updating section ${slug}:`, error);
    throw error;
  }
};

/**
 * Delete a homepage section (admin only)
 * Uses endpoint: DELETE /api/homepagesections/{slug}
 */
export const deleteSection = async (slug: string): Promise<void> => {
  try {
    await sectionsApi.delete(`/api/homepagesections/${slug}`);
  } catch (error) {
    console.error(`Error deleting section ${slug}:`, error);
    throw error;
  }
};

/**
 * Assign a vehicle to a section (admin only)
 * Uses endpoint: POST /api/homepagesections/{slug}/vehicles
 */
export const assignVehicleToSection = async (
  slug: string,
  request: AssignVehicleRequest
): Promise<void> => {
  try {
    await sectionsApi.post(`/api/homepagesections/${slug}/vehicles`, request);
  } catch (error) {
    console.error(`Error assigning vehicle to section ${slug}:`, error);
    throw error;
  }
};

/**
 * Remove a vehicle from a section (admin only)
 * Uses endpoint: DELETE /api/homepagesections/{slug}/vehicles/{vehicleId}
 */
export const removeVehicleFromSection = async (slug: string, vehicleId: string): Promise<void> => {
  try {
    await sectionsApi.delete(`/api/homepagesections/${slug}/vehicles/${vehicleId}`);
  } catch (error) {
    console.error(`Error removing vehicle from section ${slug}:`, error);
    throw error;
  }
};

export default {
  getHomepageSections,
  getHomepageSectionBySlug,
  getAllSectionConfigs,
  createSection,
  updateSection,
  deleteSection,
  assignVehicleToSection,
  removeVehicleFromSection,
};
