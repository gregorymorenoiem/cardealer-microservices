import axios from 'axios';

const VEHICLE_API_URL = import.meta.env.VITE_VEHICLE_SERVICE_URL || 'http://localhost:5002/api';

export interface Vehicle {
  id: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  bodyType: string;
  color: string;
  description: string;
  features: string[];
  images: string[];
  location: string;
  sellerId: string;
  sellerName: string;
  sellerPhone: string;
  sellerEmail: string;
  status: 'pending' | 'approved' | 'rejected' | 'sold';
  isFeatured: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleFilters {
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

export interface PaginatedVehicles {
  vehicles: Vehicle[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Get all vehicles with filters and pagination
export const getAllVehicles = async (
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
): Promise<PaginatedVehicles> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString());
        }
      });
    }

    const response = await axios.get(`${VEHICLE_API_URL}/vehicles?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicles:', error);
    throw new Error('Failed to fetch vehicles');
  }
};

// Get featured vehicles for homepage
export const getFeaturedVehicles = async (limit: number = 6): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/featured?limit=${limit}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching featured vehicles:', error);
    throw new Error('Failed to fetch featured vehicles');
  }
};

// Get vehicle by ID
export const getVehicleById = async (id: string): Promise<Vehicle> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/${id}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicle:', error);
    throw new Error('Failed to fetch vehicle details');
  }
};

// Create new vehicle listing
export const createVehicle = async (vehicleData: Partial<Vehicle>): Promise<Vehicle> => {
  try {
    const response = await axios.post(`${VEHICLE_API_URL}/vehicles`, vehicleData);
    return response.data;
  } catch (error) {
    console.error('Error creating vehicle:', error);
    throw new Error('Failed to create vehicle listing');
  }
};

// Update vehicle listing
export const updateVehicle = async (id: string, vehicleData: Partial<Vehicle>): Promise<Vehicle> => {
  try {
    const response = await axios.put(`${VEHICLE_API_URL}/vehicles/${id}`, vehicleData);
    return response.data;
  } catch (error) {
    console.error('Error updating vehicle:', error);
    throw new Error('Failed to update vehicle listing');
  }
};

// Delete vehicle listing
export const deleteVehicle = async (id: string): Promise<void> => {
  try {
    await axios.delete(`${VEHICLE_API_URL}/vehicles/${id}`);
  } catch (error) {
    console.error('Error deleting vehicle:', error);
    throw new Error('Failed to delete vehicle listing');
  }
};

// Get user's own vehicles
export const getMyVehicles = async (): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/my-listings`);
    return response.data;
  } catch (error) {
    console.error('Error fetching my vehicles:', error);
    throw new Error('Failed to fetch your listings');
  }
};

// Search vehicles (with full-text search)
export const searchVehicles = async (
  query: string,
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
): Promise<PaginatedVehicles> => {
  try {
    const params = new URLSearchParams();
    params.append('q', query);
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString());
        }
      });
    }

    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/search?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Error searching vehicles:', error);
    throw new Error('Failed to search vehicles');
  }
};

// Get similar vehicles
export const getSimilarVehicles = async (id: string, limit: number = 4): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/${id}/similar?limit=${limit}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching similar vehicles:', error);
    throw new Error('Failed to fetch similar vehicles');
  }
};

// Add to favorites/wishlist
export const addToFavorites = async (vehicleId: string): Promise<void> => {
  try {
    await axios.post(`${VEHICLE_API_URL}/favorites/${vehicleId}`);
  } catch (error) {
    console.error('Error adding to favorites:', error);
    throw new Error('Failed to add to favorites');
  }
};

// Remove from favorites/wishlist
export const removeFromFavorites = async (vehicleId: string): Promise<void> => {
  try {
    await axios.delete(`${VEHICLE_API_URL}/favorites/${vehicleId}`);
  } catch (error) {
    console.error('Error removing from favorites:', error);
    throw new Error('Failed to remove from favorites');
  }
};

// Get user's favorites
export const getFavorites = async (): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/favorites`);
    return response.data;
  } catch (error) {
    console.error('Error fetching favorites:', error);
    throw new Error('Failed to fetch favorites');
  }
};

// Check if vehicle is in favorites
export const isFavorite = async (vehicleId: string): Promise<boolean> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/favorites/${vehicleId}/check`);
    return response.data.isFavorite;
  } catch (error) {
    console.error('Error checking favorite status:', error);
    return false;
  }
};

// Get vehicle statistics (for admin)
export const getVehicleStats = async (): Promise<{
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  sold: number;
}> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/admin/vehicles/stats`);
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicle stats:', error);
    throw new Error('Failed to fetch vehicle statistics');
  }
};

// Approve vehicle (admin only)
export const approveVehicle = async (id: string): Promise<Vehicle> => {
  try {
    const response = await axios.patch(`${VEHICLE_API_URL}/admin/vehicles/${id}/approve`);
    return response.data;
  } catch (error) {
    console.error('Error approving vehicle:', error);
    throw new Error('Failed to approve vehicle');
  }
};

// Reject vehicle (admin only)
export const rejectVehicle = async (id: string, reason?: string): Promise<Vehicle> => {
  try {
    const response = await axios.patch(`${VEHICLE_API_URL}/admin/vehicles/${id}/reject`, { reason });
    return response.data;
  } catch (error) {
    console.error('Error rejecting vehicle:', error);
    throw new Error('Failed to reject vehicle');
  }
};

// Get pending vehicles (admin only)
export const getPendingVehicles = async (): Promise<Vehicle[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/admin/vehicles/pending`);
    return response.data;
  } catch (error) {
    console.error('Error fetching pending vehicles:', error);
    throw new Error('Failed to fetch pending vehicles');
  }
};

// Mark vehicle as sold
export const markAsSold = async (id: string): Promise<Vehicle> => {
  try {
    const response = await axios.patch(`${VEHICLE_API_URL}/vehicles/${id}/sold`);
    return response.data;
  } catch (error) {
    console.error('Error marking vehicle as sold:', error);
    throw new Error('Failed to mark vehicle as sold');
  }
};

// Get vehicle makes (for filters)
export const getVehicleMakes = async (): Promise<string[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/makes`);
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicle makes:', error);
    throw new Error('Failed to fetch vehicle makes');
  }
};

// Get vehicle models by make (for filters)
export const getVehicleModels = async (make: string): Promise<string[]> => {
  try {
    const response = await axios.get(`${VEHICLE_API_URL}/vehicles/models?make=${make}`);
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicle models:', error);
    throw new Error('Failed to fetch vehicle models');
  }
};

export default {
  getAllVehicles,
  getFeaturedVehicles,
  getVehicleById,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  getMyVehicles,
  searchVehicles,
  getSimilarVehicles,
  addToFavorites,
  removeFromFavorites,
  getFavorites,
  isFavorite,
  getVehicleStats,
  approveVehicle,
  rejectVehicle,
  getPendingVehicles,
  markAsSold,
  getVehicleMakes,
  getVehicleModels,
};
