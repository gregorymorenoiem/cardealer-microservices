import axios from 'axios';

// API Gateway URL (routes to appropriate microservices)
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// VehiclesSaleService via Gateway - base path without /vehicles
const VEHICLES_SALE_API_URL = `${API_URL}/api`;

// ProductService via Gateway (legacy)
const PRODUCT_API_URL = `${API_URL}/api/products`;

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
  isNew?: boolean;  // Added for new/used indication
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
 * Uses VehiclesSaleService for real database data
 */
export const getAllVehicles = async (
  filters?: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
): Promise<PaginatedVehicles> => {
  try {
    const params = new URLSearchParams();
    // Backend uses 0-based pagination, frontend uses 1-based
    params.append('page', (page - 1).toString());
    params.append('pageSize', pageSize.toString());

    if (filters?.search) {
      params.append('search', filters.search);
    }
    if (filters?.minPrice) {
      params.append('minPrice', filters.minPrice.toString());
    }
    if (filters?.maxPrice) {
      params.append('maxPrice', filters.maxPrice.toString());
    }

    // VehiclesSaleService returns paginated response: { vehicles: [], totalCount: N }
    const response = await axios.get(
      `${VEHICLES_SALE_API_URL}/vehicles?${params.toString()}`
    );

    const data = response.data;
    const vehiclesArray = data.vehicles || [];
    const vehicles = Array.isArray(vehiclesArray)
      ? vehiclesArray.map(transformBackendVehicleToFrontend)
      : [];

    return {
      vehicles,
      total: data.totalCount || vehicles.length,
      page: (data.page !== undefined ? data.page + 1 : page), // Convert 0-based to 1-based
      pageSize: data.pageSize || pageSize,
      totalPages: data.totalPages || Math.ceil((data.totalCount || vehicles.length) / pageSize) || 1,
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
    // VehiclesSaleService - get active vehicles
    const response = await axios.get(
      `${VEHICLES_SALE_API_URL}/vehicles?pageSize=${limit}&status=1`
    );

    const data = response.data;
    const vehiclesArray = data.vehicles || [];
    return Array.isArray(vehiclesArray)
      ? vehiclesArray.map(transformBackendVehicleToFrontend)
      : [];
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
    // VehiclesSaleService returns single vehicle object
    const response = await axios.get(
      `${VEHICLES_SALE_API_URL}/vehicles/${id}`
    );

    if (!response.data) {
      throw new Error('Vehicle not found');
    }

    return transformBackendVehicleToFrontend(response.data);
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
 * Create new vehicle listing using VehiclesSaleService
 * Maps frontend form data to backend Vehicle entity
 */
export const createVehicle = async (vehicleData: Partial<VehicleFormData>): Promise<Vehicle> => {
  try {
    // Map form data to backend Vehicle entity format
    const backendPayload = {
      dealerId: '00000000-0000-0000-0000-000000000001', // Default dealer
      title: `${vehicleData.make} ${vehicleData.model} ${vehicleData.year}`,
      description: vehicleData.description || '',
      price: vehicleData.price || 0,
      currency: 'USD',
      make: vehicleData.make,
      model: vehicleData.model,
      year: vehicleData.year,
      vin: vehicleData.vin || '',
      mileage: vehicleData.mileage || 0,
      condition: mapConditionToEnum(vehicleData.condition),
      bodyStyle: mapBodyStyleToEnum(vehicleData.bodyType),
      fuelType: mapFuelTypeToEnum(vehicleData.fuelType),
      transmission: mapTransmissionToEnum(vehicleData.transmission),
      driveType: mapDrivetrainToEnum(vehicleData.drivetrain),
      doors: 4, // Could be extracted from form
      seats: 5, // Could be extracted from form
      exteriorColor: vehicleData.exteriorColor || '',
      interiorColor: vehicleData.interiorColor || '',
      city: vehicleData.location || '',
      state: '', // Could be extracted from location
      country: 'USA',
      horsepower: parseInt(vehicleData.horsepower || '0'),
      cylinders: extractCylinders(vehicleData.engine),
      mpgCity: extractMpgCity(vehicleData.mpg),
      mpgHighway: extractMpgHighway(vehicleData.mpg),
      hasCleanTitle: true,
      accidentHistory: false,
      featuresJson: JSON.stringify(vehicleData.features || []),
      status: 0, // 0=Draft, 1=Active/Published
    };

    const response = await axios.post(
      `${VEHICLES_SALE_API_URL}/vehicles`,
      backendPayload
    );

    // Backend returns the created vehicle directly
    return transformBackendVehicleToFrontend(response.data);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      }
      if (error.response?.data?.title) {
        throw new Error(error.response.data.title);
      }
    }
    console.error('Error creating vehicle:', error);
    throw new Error('Failed to create vehicle listing');
  }
};

// Helper mappers
const mapConditionToEnum = (condition?: string): number => {
  const map: Record<string, number> = {
    'new': 0,
    'used-like-new': 1,
    'used-excellent': 1,
    'used-good': 2,
    'used-fair': 3,
  };
  return map[condition?.toLowerCase() || ''] ?? 2;
};

const mapBodyStyleToEnum = (bodyType?: string): number => {
  const map: Record<string, number> = {
    'sedan': 1,
    'coupe': 2,
    'hatchback': 3,
    'suv': 4,
    'truck': 5,
    'van': 6,
    'wagon': 7,
    'convertible': 8,
  };
  return map[bodyType?.toLowerCase() || ''] ?? 1;
};

const mapFuelTypeToEnum = (fuelType?: string): number => {
  const map: Record<string, number> = {
    'gasoline': 0,
    'diesel': 1,
    'electric': 2,
    'hybrid': 3,
    'plug-in hybrid': 4,
  };
  return map[fuelType?.toLowerCase() || ''] ?? 0;
};

const mapTransmissionToEnum = (transmission?: string): number => {
  const map: Record<string, number> = {
    'automatic': 0,
    'manual': 1,
    'cvt': 2,
    'dct': 3,
  };
  return map[transmission?.toLowerCase() || ''] ?? 0;
};

const mapDrivetrainToEnum = (drivetrain?: string): number => {
  const map: Record<string, number> = {
    'fwd': 0,
    'rwd': 1,
    'awd': 2,
    '4wd': 3,
  };
  return map[drivetrain?.toLowerCase() || ''] ?? 0;
};

const extractCylinders = (engine?: string): number => {
  if (!engine) return 4;
  const match = engine.match(/(\d+).*cyl/i);
  return match ? parseInt(match[1]) : 4;
};

const extractMpgCity = (mpg?: string): number | null => {
  if (!mpg) return null;
  const match = mpg.match(/(\d+)\s*city/i);
  return match ? parseInt(match[1]) : null;
};

const extractMpgHighway = (mpg?: string): number | null => {
  if (!mpg) return null;
  const match = mpg.match(/(\d+)\s*hwy/i);
  return match ? parseInt(match[1]) : null;
};

const transformBackendVehicleToFrontend = (data: any): Vehicle => {
  // Parse CustomFieldsJson if it's a string
  const customFields = typeof data.customFieldsJson === 'string' 
    ? JSON.parse(data.customFieldsJson) 
    : (data.customFieldsJson || {});

  // Estimate MPG based on vehicle type if not provided
  const estimateMpg = () => {
    // If we have explicit MPG values, use them
    if (customFields.mpgCity && customFields.mpgHighway) {
      return {
        city: parseInt(customFields.mpgCity),
        highway: parseInt(customFields.mpgHighway),
      };
    }

    // Otherwise, estimate based on fuel type
    const fuelType = data.fuelType?.toString().toLowerCase() || '';
    if (fuelType.includes('electric') || fuelType === '4') {
      return { city: 0, highway: 0 }; // Electric - show MPGe differently
    }
    
    // Default estimates for gas vehicles
    return { city: 20, highway: 28 };
  };

  // Transform images - handle S3 URLs
  const images = data.images?.map((img: any) => {
    const url = img.url || '';
    
    // If it's already a full URL (S3, CloudFront, etc.), use it as is
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    
    // If it's just a filename or relative path, build full S3 URL
    // Pattern: https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/filename.jpg
    if (url && !url.startsWith('/')) {
      const s3Bucket = import.meta.env.VITE_S3_BUCKET || 'okla-images-2026';
      const s3Region = import.meta.env.VITE_S3_REGION || 'us-east-2';
      const basePath = import.meta.env.VITE_S3_BASE_PATH || 'frontend/assets/vehicles/sale';
      
      // If url already includes the full path, use it
      if (url.startsWith('frontend/')) {
        return `https://${s3Bucket}.s3.${s3Region}.amazonaws.com/${url}`;
      }
      
      // Otherwise, prepend the base path
      return `https://${s3Bucket}.s3.${s3Region}.amazonaws.com/${basePath}/${url}`;
    }
    
    // Fallback to placeholder
    return url || '/placeholder-car.jpg';
  }) || ['/placeholder-car.jpg'];

  // Map backend DriveType enum to frontend drivetrain string
  const mapDriveType = (driveType: string): string => {
    const map: Record<string, string> = {
      '0': 'FWD',
      '1': 'AWD',
      '2': 'RWD',
      '3': '4WD',
    };
    return map[driveType] || 'FWD';
  };

  // Map backend Condition enum to frontend condition string
  const mapCondition = (condition: string): string => {
    const map: Record<string, string> = {
      '0': 'New',
      '1': 'Used',
      '2': 'Certified Pre-Owned',
    };
    return map[condition] || 'Used';
  };

  return {
    id: data.id,
    title: data.title,
    make: data.make,
    model: data.model,
    year: data.year,
    price: data.price,
    mileage: data.mileage,
    fuelType: data.fuelType?.toString() || 'Gasoline',
    transmission: data.transmission?.toString() || 'Automatic',
    bodyType: data.bodyStyle?.toString() || 'Sedan',
    drivetrain: mapDriveType(data.driveType?.toString() || '0'),
    color: data.exteriorColor,
    interiorColor: data.interiorColor,
    engine: data.engineSize,
    horsepower: data.horsepower,
    mpg: estimateMpg(),
    vin: data.vin || 'N/A',
    condition: mapCondition(data.condition?.toString() || '1'),
    description: data.description,
    features: JSON.parse(data.featuresJson || '[]'),
    images,
    location: data.city && data.state ? `${data.city}, ${data.state}` : (data.city || ''),
    sellerId: data.sellerId || data.dealerId,
    sellerName: data.sellerName || 'Unknown',
    seller: {
      id: data.sellerId || data.dealerId,
      name: data.sellerName || 'Unknown Dealer',
      type: 'Dealer',
      phone: data.sellerPhone || '',
      email: data.sellerEmail || '',
      rating: data.sellerRating ? parseFloat(data.sellerRating.toFixed(1)) : 4.8, // Default rating with 1 decimal
    },
    status: mapBackendStatusToFrontend(data.status),
    isFeatured: data.isFeatured || false,
    createdAt: data.createdAt,
    updatedAt: data.updatedAt,
  };
};

const mapBackendStatusToFrontend = (status: number): 'pending' | 'approved' | 'rejected' | 'sold' => {
  const map: Record<number, 'pending' | 'approved' | 'rejected' | 'sold'> = {
    0: 'pending',
    1: 'approved',
    2: 'approved',
    3: 'sold',
  };
  return map[status] || 'pending';
};

// Type for form data from SellYourCarPage
export interface VehicleFormData {
  make: string;
  model: string;
  year: number;
  mileage: number;
  vin: string;
  transmission: string;
  fuelType: string;
  bodyType: string;
  drivetrain: string;
  engine: string;
  horsepower: string;
  mpg: string;
  exteriorColor: string;
  interiorColor: string;
  condition: string;
  features: string[];
  images: File[];
  price: number;
  description: string;
  location: string;
  sellerName: string;
  sellerPhone: string;
  sellerEmail: string;
  sellerType: 'private' | 'dealer';
}

/**
 * Update vehicle listing (legacy - using ProductService)
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

export const compareVehicles = async (vehicleIds: string[]): Promise<Vehicle[]> => {
  try {
    console.log('📊 Comparing vehicles:', vehicleIds);
    
    const response = await axios.post(`${VEHICLES_SALE_API_URL}/vehicles/compare`, {
      vehicleIds
    });

    console.log('✅ Compare response:', response.data);

    // Transform backend data to frontend format
    const vehicles = Array.isArray(response.data) ? response.data : [];
    return vehicles.map(transformBackendVehicleToFrontend);
  } catch (error) {
    console.error('❌ Error comparing vehicles:', error);
    throw error;
  }
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
  compareVehicles,
  
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
