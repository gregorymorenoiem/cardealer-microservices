import axios from 'axios';

// ProductService API URL
const PRODUCT_API_URL = import.meta.env.VITE_PRODUCT_SERVICE_URL || 'http://localhost:15006/api';

// ============================================================
// BACKEND TYPES (matching ProductService schema)
// ============================================================

interface BackendProduct {
  id: string;
  dealerId: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  status: number; // 0=Draft, 1=Active, 2=Inactive, 3=Sold
  imageUrl: string | null;
  sellerId: string;
  sellerName: string | null;
  categoryId: string;
  categoryName: string | null;
  createdAt: string;
  updatedAt: string;
  isDeleted: boolean;
  customFieldsJson: string | null;
  images: BackendProductImage[] | null;
}

interface BackendProductImage {
  id: string;
  productId: string;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
}

interface BackendCategory {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  parentId: string | null;
  imageUrl: string | null;
  displayOrder: number;
  isActive: boolean;
}

// Preparado para integración con backend real
interface _BackendApiResponse<T> {
  success: boolean;
  data: T;
  error: string | null;
  metadata?: {
    totalCount?: number;
    pageSize?: number;
    currentPage?: number;
    totalPages?: number;
  };
}

// Exportar para uso futuro
export type BackendApiResponse<T> = _BackendApiResponse<T>;

// ============================================================
// FRONTEND TYPES (used in UI components)
// ============================================================

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
  sellerPhone?: string;
  sellerEmail?: string;
  status: 'pending' | 'approved' | 'rejected' | 'sold';
  isFeatured: boolean;
  createdAt: string;
  updatedAt: string;
  categoryId?: string;
  categoryName?: string;
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
  categoryId?: string;
}

export interface PaginatedVehicles {
  vehicles: Vehicle[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  parentId?: string;
  imageUrl?: string;
  children?: Category[];
}

// ============================================================
// DATA TRANSFORMERS
// ============================================================

/**
 * Parse custom fields JSON from backend to extract vehicle-specific data
 */
const parseCustomFields = (customFieldsJson: string | null): Record<string, unknown> => {
  if (!customFieldsJson) return {};
  try {
    return JSON.parse(customFieldsJson);
  } catch {
    return {};
  }
};

/**
 * Transform backend Product to frontend Vehicle
 */
const transformProductToVehicle = (product: BackendProduct): Vehicle => {
  const customFields = parseCustomFields(product.customFieldsJson);
  
  // Get images from the images array or fallback to imageUrl
  const images: string[] = product.images?.map(img => img.imageUrl) || [];
  if (product.imageUrl && !images.includes(product.imageUrl)) {
    images.unshift(product.imageUrl);
  }

  // Map backend status (number) to frontend status
  const statusMap: Record<number, 'pending' | 'approved' | 'rejected' | 'sold'> = {
    0: 'pending',   // Draft
    1: 'approved',  // Active
    2: 'rejected',  // Inactive
    3: 'sold',      // Sold
  };

  return {
    id: product.id,
    title: product.name,
    make: (customFields.make as string) || '',
    model: (customFields.model as string) || '',
    year: (customFields.year as number) || new Date().getFullYear(),
    price: product.price,
    mileage: (customFields.mileage as number) || 0,
    fuelType: (customFields.fuelType as string) || 'Gasoline',
    transmission: (customFields.transmission as string) || 'Automatic',
    bodyType: (customFields.bodyType as string) || 'Sedan',
    color: (customFields.color as string) || '',
    description: product.description || '',
    features: (customFields.features as string[]) || [],
    images: images.length > 0 ? images : ['/placeholder-car.jpg'],
    location: (customFields.location as string) || '',
    sellerId: product.sellerId,
    sellerName: product.sellerName || 'Unknown Seller',
    sellerPhone: customFields.sellerPhone as string,
    sellerEmail: customFields.sellerEmail as string,
    status: statusMap[product.status] || 'pending',
    isFeatured: (customFields.isFeatured as boolean) || false,
    createdAt: product.createdAt,
    updatedAt: product.updatedAt,
    categoryId: product.categoryId,
    categoryName: product.categoryName || undefined,
  };
};

/**
 * Transform frontend Vehicle to backend Product for create/update
 */
const transformVehicleToProduct = (vehicle: Partial<Vehicle>): Record<string, unknown> => {
  const customFields = {
    make: vehicle.make,
    model: vehicle.model,
    year: vehicle.year,
    mileage: vehicle.mileage,
    fuelType: vehicle.fuelType,
    transmission: vehicle.transmission,
    bodyType: vehicle.bodyType,
    color: vehicle.color,
    features: vehicle.features,
    location: vehicle.location,
    sellerPhone: vehicle.sellerPhone,
    sellerEmail: vehicle.sellerEmail,
    isFeatured: vehicle.isFeatured,
  };

  // Map frontend status to backend status (numeric)
  const statusMap: Record<string, number> = {
    'pending': 0,   // Draft
    'approved': 1,  // Active
    'rejected': 2,  // Inactive
    'sold': 3,      // Sold
  };

  return {
    name: vehicle.title,
    description: vehicle.description,
    price: vehicle.price,
    currency: 'USD',
    status: vehicle.status ? statusMap[vehicle.status] : 0,
    imageUrl: vehicle.images?.[0],
    categoryId: vehicle.categoryId,
    customFieldsJson: JSON.stringify(customFields),
  };
};

const transformCategory = (cat: BackendCategory): Category => ({
  id: cat.id,
  name: cat.name,
  slug: cat.slug,
  description: cat.description || undefined,
  parentId: cat.parentId || undefined,
  imageUrl: cat.imageUrl || undefined,
});

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get all vehicles with filters and pagination
 * Note: ProductService returns array directly, not wrapped in {success, data}
 */
export const getAllVehicles = async (
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
): Promise<PaginatedVehicles> => {
  try {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    if (filters?.search) {
      params.append('search', filters.search);
    }
    if (filters?.categoryId) {
      params.append('categoryId', filters.categoryId);
    }
    if (filters?.minPrice) {
      params.append('minPrice', filters.minPrice.toString());
    }
    if (filters?.maxPrice) {
      params.append('maxPrice', filters.maxPrice.toString());
    }

    // ProductService returns array directly
    const response = await axios.get<BackendProduct[]>(
      `${PRODUCT_API_URL}/Products?${params.toString()}`
    );

    const products = response.data || [];
    const vehicles = products.map(transformProductToVehicle);

    // Note: Backend doesn't return pagination metadata, we estimate from response
    return {
      vehicles,
      total: vehicles.length,
      page: page,
      pageSize: pageSize,
      totalPages: Math.ceil(vehicles.length / pageSize) || 1,
    };
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    console.error('Error fetching vehicles:', error);
    throw new Error('Failed to fetch vehicles');
  }
};

/**
 * Get featured vehicles for homepage
 */
export const getFeaturedVehicles = async (limit: number = 6): Promise<Vehicle[]> => {
  try {
    // ProductService returns array directly
    const response = await axios.get<BackendProduct[]>(
      `${PRODUCT_API_URL}/Products?pageSize=${limit}&status=1`
    );

    return (response.data || []).map(transformProductToVehicle);
  } catch (error) {
    console.error('Error fetching featured vehicles:', error);
    return [];
  }
};

/**
 * Get vehicle by ID
 */
export const getVehicleById = async (id: string): Promise<Vehicle> => {
  try {
    // ProductService returns single object directly
    const response = await axios.get<BackendProduct>(
      `${PRODUCT_API_URL}/Products/${id}`
    );

    if (!response.data) {
      throw new Error('Vehicle not found');
    }

    return transformProductToVehicle(response.data);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 404) {
        throw new Error('Vehicle not found');
      }
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      }
    }
    console.error('Error fetching vehicle:', error);
    throw new Error('Failed to fetch vehicle details');
  }
};

/**
 * Create new vehicle listing
 */
export const createVehicle = async (vehicleData: Partial<Vehicle>): Promise<Vehicle> => {
  try {
    const token = localStorage.getItem('accessToken');
    const productData = transformVehicleToProduct(vehicleData);

    // ProductService returns created object directly
    const response = await axios.post<BackendProduct>(
      `${PRODUCT_API_URL}/Products`,
      productData,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (!response.data) {
      throw new Error('Failed to create vehicle');
    }

    return transformProductToVehicle(response.data);
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    console.error('Error creating vehicle:', error);
    throw new Error('Failed to create vehicle listing');
  }
};

/**
 * Update vehicle listing
 */
export const updateVehicle = async (id: string, vehicleData: Partial<Vehicle>): Promise<Vehicle> => {
  try {
    const token = localStorage.getItem('accessToken');
    const productData = transformVehicleToProduct(vehicleData);

    // ProductService returns updated object directly
    const response = await axios.put<BackendProduct>(
      `${PRODUCT_API_URL}/Products/${id}`,
      productData,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    if (!response.data) {
      throw new Error('Failed to update vehicle');
    }

    return transformProductToVehicle(response.data);
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    console.error('Error updating vehicle:', error);
    throw new Error('Failed to update vehicle listing');
  }
};

/**
 * Delete vehicle listing
 */
export const deleteVehicle = async (id: string): Promise<void> => {
  try {
    const token = localStorage.getItem('accessToken');

    await axios.delete(`${PRODUCT_API_URL}/Products/${id}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    console.error('Error deleting vehicle:', error);
    throw new Error('Failed to delete vehicle listing');
  }
};

/**
 * Get user's own vehicles by sellerId
 */
export const getMyVehicles = async (sellerId: string): Promise<Vehicle[]> => {
  try {
    const token = localStorage.getItem('accessToken');

    // ProductService returns array directly
    const response = await axios.get<BackendProduct[]>(
      `${PRODUCT_API_URL}/Products/seller/${sellerId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    );

    return (response.data || []).map(transformProductToVehicle);
  } catch (error) {
    console.error('Error fetching my vehicles:', error);
    return [];
  }
};

/**
 * Get all categories
 */
export const getCategories = async (): Promise<Category[]> => {
  try {
    // Categories endpoint returns array directly
    const response = await axios.get<BackendCategory[]>(
      `${PRODUCT_API_URL}/Categories`
    );

    return (response.data || []).map(transformCategory);
  } catch (error) {
    console.error('Error fetching categories:', error);
    return [];
  }
};

/**
 * Get root categories (top-level)
 */
export const getRootCategories = async (): Promise<Category[]> => {
  try {
    const response = await axios.get<BackendCategory[]>(
      `${PRODUCT_API_URL}/Categories/root`
    );

    return (response.data || []).map(transformCategory);
  } catch (error) {
    console.error('Error fetching root categories:', error);
    return [];
  }
};

/**
 * Get category by slug
 */
export const getCategoryBySlug = async (slug: string): Promise<Category | null> => {
  try {
    // ProductService returns single object directly
    const response = await axios.get<BackendCategory>(
      `${PRODUCT_API_URL}/Categories/slug/${slug}`
    );

    if (!response.data) {
      return null;
    }

    return transformCategory(response.data);
  } catch (error) {
    console.error('Error fetching category:', error);
    return null;
  }
};

/**
 * Search vehicles (with full-text search)
 */
export const searchVehicles = async (
  query: string,
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
): Promise<PaginatedVehicles> => {
  return getAllVehicles({ ...filters, search: query }, page, pageSize);
};

// ============================================================
// PLACEHOLDER FUNCTIONS (to be implemented when backend supports)
// ============================================================

export const getSimilarVehicles = async (_id: string, limit: number = 4): Promise<Vehicle[]> => {
  // TODO: Implement when backend has similar products endpoint
  const result = await getAllVehicles({}, 1, limit);
  return result.vehicles;
};

export const addToFavorites = async (_vehicleId: string): Promise<void> => {
  // TODO: Implement when backend has favorites endpoint
  console.warn('Favorites feature not yet implemented');
};

export const removeFromFavorites = async (_vehicleId: string): Promise<void> => {
  // TODO: Implement when backend has favorites endpoint
  console.warn('Favorites feature not yet implemented');
};

export const getFavorites = async (): Promise<Vehicle[]> => {
  // TODO: Implement when backend has favorites endpoint
  return [];
};

export const isFavorite = async (_vehicleId: string): Promise<boolean> => {
  // TODO: Implement when backend has favorites endpoint
  return false;
};

export const getVehicleStats = async (): Promise<{
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  sold: number;
}> => {
  // TODO: Implement when backend has stats endpoint
  return { total: 0, pending: 0, approved: 0, rejected: 0, sold: 0 };
};

export const approveVehicle = async (id: string): Promise<Vehicle> => {
  return updateVehicle(id, { status: 'approved' });
};

export const rejectVehicle = async (id: string, _reason?: string): Promise<Vehicle> => {
  return updateVehicle(id, { status: 'rejected' });
};

export const getPendingVehicles = async (): Promise<Vehicle[]> => {
  const result = await getAllVehicles({}, 1, 100);
  return result.vehicles.filter(v => v.status === 'pending');
};

export const markAsSold = async (id: string): Promise<Vehicle> => {
  return updateVehicle(id, { status: 'sold' });
};

export const getVehicleMakes = async (): Promise<string[]> => {
  // TODO: Implement when backend has makes endpoint or populate from categories
  return ['Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz', 'Audi', 'Nissan', 'Hyundai', 'Kia'];
};

export const getVehicleModels = async (_make: string): Promise<string[]> => {
  // TODO: Implement when backend has models endpoint
  return [];
};

// ============================================================
// VEHICLE SERVICE OBJECT
// ============================================================

const vehicleService = {
  // Core CRUD
  getAllVehicles,
  getVehicleById,
  createVehicle,
  updateVehicle,
  deleteVehicle,
  
  // Queries
  getFeaturedVehicles,
  getMyVehicles,
  searchVehicles,
  getSimilarVehicles,
  
  // Categories
  getCategories,
  getRootCategories,
  getCategoryBySlug,
  
  // Favorites (placeholder)
  addToFavorites,
  removeFromFavorites,
  getFavorites,
  isFavorite,
  
  // Admin functions
  getVehicleStats,
  approveVehicle,
  rejectVehicle,
  getPendingVehicles,
  markAsSold,
  
  // Metadata
  getVehicleMakes,
  getVehicleModels,
};

export default vehicleService;
