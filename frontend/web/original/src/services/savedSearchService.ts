import axios from 'axios';

const VEHICLE_API_URL = import.meta.env.VITE_VEHICLE_SERVICE_URL || 'http://localhost:5002/api';

export interface SavedSearchFilters {
  make?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  minMileage?: number;
  maxMileage?: number;
  fuelType?: string;
  transmission?: string;
  bodyType?: string;
  location?: string;
  search?: string;
}

export interface SavedSearch {
  id: string;
  userId: string;
  name: string;
  filters: SavedSearchFilters;
  resultsCount: number;
  notificationsEnabled: boolean;
  createdAt: string;
  updatedAt: string;
  lastChecked?: string;
}

// Get all saved searches for current user
export const getSavedSearches = async (): Promise<SavedSearch[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/saved-searches`);
    return response.data;
  } catch (error) {
    console.error('Error fetching saved searches:', error);
    throw new Error('Failed to fetch saved searches');
  }
};

// Get saved search by ID
export const getSavedSearchById = async (id: string): Promise<SavedSearch> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/saved-searches/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching saved search:', error);
    throw new Error('Failed to fetch saved search');
  }
};

// Create new saved search
export const createSavedSearch = async (
  name: string,
  filters: SavedSearchFilters,
  notificationsEnabled: boolean = false
): Promise<SavedSearch> => {
  try {
    const response = await axios.post(`${VEHICLE_API_URL}/saved-searches`, {
      name,
      filters,
      notificationsEnabled,
    });
    return response.data;
  } catch (error) {
    console.error('Error creating saved search:', error);
    throw new Error('Failed to create saved search');
  }
};

// Update saved search
export const updateSavedSearch = async (
  id: string,
  updates: {
    name?: string;
    filters?: SavedSearchFilters;
    notificationsEnabled?: boolean;
  }
): Promise<SavedSearch> => {
  try {
    const response = await axios.put(`${VEHICLE_API_URL}/saved-searches/${id}`, updates);
    return response.data;
  } catch (error) {
    console.error('Error updating saved search:', error);
    throw new Error('Failed to update saved search');
  }
};

// Delete saved search
export const deleteSavedSearch = async (id: string): Promise<void> => {
  try {
    await axios.delete(`${VEHICLE_API_URL}/saved-searches/${id}`);
  } catch (error) {
    console.error('Error deleting saved search:', error);
    throw new Error('Failed to delete saved search');
  }
};

// Toggle notifications for saved search
export const toggleNotifications = async (id: string, enabled: boolean): Promise<SavedSearch> => {
  try {
    const response = await axios.patch(`${VEHICLE_API_URL}/saved-searches/${id}/notifications`, {
      enabled,
    });
    return response.data;
  } catch (error) {
    console.error('Error toggling notifications:', error);
    throw new Error('Failed to toggle notifications');
  }
};

// Check for new results in saved search
export const checkForNewResults = async (id: string): Promise<{
  newResults: number;
  totalResults: number;
}> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/saved-searches/${id}/check`);
    return response.data;
  } catch (error) {
    console.error('Error checking for new results:', error);
    throw new Error('Failed to check for new results');
  }
};

// Run saved search and get results
export const runSavedSearch = async (id: string, page: number = 1, pageSize: number = 12) => {
  try {
    const response = await axios.get(
      `${VEHICLE_API_URL}/saved-searches/${id}/results?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  } catch (error) {
    console.error('Error running saved search:', error);
    throw new Error('Failed to run saved search');
  }
};

// Build URL with filters for navigation
export const buildSearchUrl = (filters: SavedSearchFilters): string => {
  const params = new URLSearchParams();
  
  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, value.toString());
    }
  });

  return `/browse?${params.toString()}`;
};

// Format filters for display
export const formatFilters = (filters: SavedSearchFilters): string => {
  const parts: string[] = [];

  if (filters.make) parts.push(filters.make);
  if (filters.model) parts.push(filters.model);
  
  if (filters.minYear && filters.maxYear) {
    parts.push(`${filters.minYear}-${filters.maxYear}`);
  } else if (filters.minYear) {
    parts.push(`From ${filters.minYear}`);
  } else if (filters.maxYear) {
    parts.push(`Up to ${filters.maxYear}`);
  }

  if (filters.minPrice && filters.maxPrice) {
    parts.push(`$${filters.minPrice.toLocaleString()}-$${filters.maxPrice.toLocaleString()}`);
  } else if (filters.maxPrice) {
    parts.push(`Under $${filters.maxPrice.toLocaleString()}`);
  } else if (filters.minPrice) {
    parts.push(`Over $${filters.minPrice.toLocaleString()}`);
  }

  if (filters.fuelType) parts.push(filters.fuelType);
  if (filters.transmission) parts.push(filters.transmission);
  if (filters.bodyType) parts.push(filters.bodyType);
  if (filters.location) parts.push(filters.location);

  return parts.length > 0 ? parts.join(' • ') : 'All vehicles';
};

export default {
  getSavedSearches,
  getSavedSearchById,
  createSavedSearch,
  updateSavedSearch,
  deleteSavedSearch,
  toggleNotifications,
  checkForNewResults,
  runSavedSearch,
  buildSearchUrl,
  formatFilters,
};
