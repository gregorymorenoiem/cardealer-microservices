/**
 * FavoritesService - API client for vehicle favorites
 * Connects via API Gateway to VehiclesSaleService FavoritesController
 *
 * @author OKLA Team
 * @date January 28, 2026
 */

import api from './api';

// ============================================================
// TYPES
// ============================================================

export interface FavoriteVehicle {
  id: string;
  title: string;
  description: string;
  price: number;
  currency: string;
  status: string;
  make: string;
  model: string;
  year: number;
  mileage: number | null;
  fuelType: string;
  transmission: string;
  bodyStyle: string;
  primaryImageUrl: string | null;
  sellerName: string;
  viewCount: number;
  favoriteCount: number;
  createdAt: string;
}

export interface Favorite {
  id: string;
  userId: string;
  vehicleId: string;
  createdAt: string;
  notes: string | null;
  notifyPriceChange: boolean;
  vehicle?: FavoriteVehicle;
}

export interface AddFavoriteRequest {
  notes?: string;
  notifyPriceChange?: boolean;
}

export interface UpdateFavoriteRequest {
  notes?: string;
  notifyPriceChange?: boolean;
}

export interface FavoriteResponse {
  id: string;
  userId: string;
  vehicleId: string;
  createdAt: string;
  notes: string | null;
  notifyPriceChange: boolean;
}

export interface FavoriteCountResponse {
  count: number;
}

export interface IsFavoriteResponse {
  isFavorite: boolean;
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get all favorites for the current user
 */
export const getFavorites = async (): Promise<FavoriteVehicle[]> => {
  try {
    const response = await api.get<FavoriteVehicle[]>('/api/favorites');
    return response.data;
  } catch (error) {
    console.error('Error fetching favorites:', error);
    throw error;
  }
};

/**
 * Get favorite count for the current user
 */
export const getFavoriteCount = async (): Promise<number> => {
  try {
    const response = await api.get<FavoriteCountResponse>('/api/favorites/count');
    return response.data.count;
  } catch (error) {
    console.error('Error fetching favorite count:', error);
    return 0;
  }
};

/**
 * Check if a vehicle is in favorites
 */
export const checkFavorite = async (vehicleId: string): Promise<boolean> => {
  try {
    const response = await api.get<IsFavoriteResponse>(`/api/favorites/check/${vehicleId}`);
    return response.data.isFavorite;
  } catch (error) {
    console.error(`Error checking favorite ${vehicleId}:`, error);
    return false;
  }
};

/**
 * Add a vehicle to favorites
 */
export const addFavorite = async (
  vehicleId: string,
  request?: AddFavoriteRequest
): Promise<FavoriteResponse> => {
  try {
    const response = await api.post<FavoriteResponse>(`/api/favorites/${vehicleId}`, request || {});
    return response.data;
  } catch (error) {
    console.error(`Error adding favorite ${vehicleId}:`, error);
    throw error;
  }
};

/**
 * Remove a vehicle from favorites
 */
export const removeFavorite = async (vehicleId: string): Promise<void> => {
  try {
    await api.delete(`/api/favorites/${vehicleId}`);
  } catch (error) {
    console.error(`Error removing favorite ${vehicleId}:`, error);
    throw error;
  }
};

/**
 * Update favorite notes and notification settings
 */
export const updateFavorite = async (
  vehicleId: string,
  request: UpdateFavoriteRequest
): Promise<FavoriteResponse> => {
  try {
    const response = await api.put<FavoriteResponse>(`/api/favorites/${vehicleId}`, request);
    return response.data;
  } catch (error) {
    console.error(`Error updating favorite ${vehicleId}:`, error);
    throw error;
  }
};

/**
 * Toggle favorite status (add if not exists, remove if exists)
 */
export const toggleFavorite = async (
  vehicleId: string,
  request?: AddFavoriteRequest
): Promise<{ isFavorite: boolean; favorite?: FavoriteResponse }> => {
  const isFavorite = await checkFavorite(vehicleId);

  if (isFavorite) {
    await removeFavorite(vehicleId);
    return { isFavorite: false };
  } else {
    const favorite = await addFavorite(vehicleId, request);
    return { isFavorite: true, favorite };
  }
};

// ============================================================
// HOOKS SUPPORT TYPES
// ============================================================

export interface UseFavoritesState {
  favorites: FavoriteVehicle[];
  count: number;
  loading: boolean;
  error: string | null;
}

// ============================================================
// DEFAULT EXPORT
// ============================================================

export default {
  getFavorites,
  getFavoriteCount,
  checkFavorite,
  addFavorite,
  removeFavorite,
  updateFavorite,
  toggleFavorite,
};
