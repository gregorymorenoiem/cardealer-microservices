import axiosModule, { isAxiosError, type AxiosError } from 'axios';
import api from './api';

// API Gateway URL (routes to appropriate microservices)
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Use api instance with refresh token interceptor
const axios = api;

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
  interiorColor?: string;
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
  isNew?: boolean; // Added for new/used indication
  vin?: string;
  condition?: string;
  createdAt: string;
  updatedAt: string;
  categoryId?: string;
  categoryName?: string;
  // New fields from VIN decoder and backend
  trim?: string;
  doors?: number;
  seats?: number;
  drivetrain?: string;
  engine?: string;
  horsepower?: number;
  mpg?: {
    city: number;
    highway: number;
  };
  seller?: {
    id: string;
    name: string;
    type: string;
    phone: string;
    email?: string;
    rating?: number;
  };
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

// Extended vehicle type for dealer inventory with engagement metrics
export interface DealerVehicle extends Vehicle {
  viewCount: number;
  inquiryCount: number;
  favoriteCount: number;
  stockNumber?: string;
  dealerId: string;
  publishedAt?: string;
  soldAt?: string;
}

export interface DealerInventoryResponse {
  vehicles: DealerVehicle[];
  total: number;
  activeCount: number;
  pausedCount: number;
  soldCount: number;
  draftCount: number;
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
  const images: string[] = product.images?.map((img) => img.imageUrl) || [];
  if (product.imageUrl && !images.includes(product.imageUrl)) {
    images.unshift(product.imageUrl);
  }

  // Map backend status (number) to frontend status
  const statusMap: Record<number, 'pending' | 'approved' | 'rejected' | 'sold'> = {
    0: 'pending', // Draft
    1: 'approved', // Active
    2: 'rejected', // Inactive
    3: 'sold', // Sold
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
    pending: 0, // Draft
    approved: 1, // Active
    rejected: 2, // Inactive
    sold: 3, // Sold
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
    const response = await axios.get(`${VEHICLES_SALE_API_URL}/vehicles?${params.toString()}`);

    const data = response.data;
    const vehiclesArray = data.vehicles || [];
    const vehicles = Array.isArray(vehiclesArray)
      ? vehiclesArray.map(transformBackendVehicleToFrontend)
      : [];

    return {
      vehicles,
      total: data.totalCount || vehicles.length,
      page: data.page !== undefined ? data.page + 1 : page, // Convert 0-based to 1-based
      pageSize: data.pageSize || pageSize,
      totalPages:
        data.totalPages || Math.ceil((data.totalCount || vehicles.length) / pageSize) || 1,
    };
  } catch (error: unknown) {
    if (isAxiosError(error) && error.response?.data?.message) {
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
    return Array.isArray(vehiclesArray) ? vehiclesArray.map(transformBackendVehicleToFrontend) : [];
  } catch (error: unknown) {
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
    const response = await axios.get(`${VEHICLES_SALE_API_URL}/vehicles/${id}`);

    if (!response.data) {
      throw new Error('Vehicle not found');
    }

    return transformBackendVehicleToFrontend(response.data);
  } catch (error: unknown) {
    if (isAxiosError(error)) {
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
      trim: vehicleData.trim || null, // Trim del VIN decode (LE, SE, XLE, Sport)
      year: vehicleData.year,
      vin: vehicleData.vin || '',
      mileage: vehicleData.mileage || 0,
      condition: mapConditionToEnum(vehicleData.condition),
      bodyStyle: vehicleData.bodyType ? mapBodyStyleToEnum(vehicleData.bodyType) : null, // Opcional
      fuelType: vehicleData.fuelType ? mapFuelTypeToEnum(vehicleData.fuelType) : null, // Opcional - VIN no siempre tiene
      transmission: vehicleData.transmission
        ? mapTransmissionToEnum(vehicleData.transmission)
        : null, // Opcional
      driveType: vehicleData.drivetrain ? mapDrivetrainToEnum(vehicleData.drivetrain) : null, // Opcional
      doors: vehicleData.doors || 4,
      seats: vehicleData.seats || 5,
      exteriorColor: vehicleData.exteriorColor || '',
      interiorColor: vehicleData.interiorColor || '',
      city: vehicleData.location || '',
      state: '', // Could be extracted from location
      country: 'USA',
      engineSize: vehicleData.engine || null, // 2.5L, 3.0L
      horsepower: vehicleData.horsepower ? parseInt(vehicleData.horsepower) : null,
      cylinders: extractCylinders(vehicleData.engine),
      hasCleanTitle: true,
      // Seller info from PricingStep
      sellerName: vehicleData.sellerName || '',
      sellerPhone: vehicleData.sellerPhone || '',
      sellerEmail: vehicleData.sellerEmail || '',
      // Features
      featuresJson: JSON.stringify(vehicleData.features || []),
    };

    const response = await axios.post(`${VEHICLES_SALE_API_URL}/vehicles`, backendPayload);

    // Backend returns the created vehicle directly
    return transformBackendVehicleToFrontend(response.data);
  } catch (error: unknown) {
    if (isAxiosError(error)) {
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
    new: 0,
    'used-like-new': 1,
    'used-excellent': 1,
    'used-good': 2,
    'used-fair': 3,
  };
  return map[condition?.toLowerCase() || ''] ?? 2;
};

const mapBodyStyleToEnum = (bodyType?: string): number => {
  const map: Record<string, number> = {
    sedan: 1,
    coupe: 2,
    hatchback: 3,
    suv: 4,
    truck: 5,
    van: 6,
    wagon: 7,
    convertible: 8,
  };
  return map[bodyType?.toLowerCase() || ''] ?? 1;
};

const mapFuelTypeToEnum = (fuelType?: string): number => {
  const map: Record<string, number> = {
    gasoline: 0,
    diesel: 1,
    electric: 2,
    hybrid: 3,
    'plug-in hybrid': 4,
  };
  return map[fuelType?.toLowerCase() || ''] ?? 0;
};

const mapTransmissionToEnum = (transmission?: string): number => {
  const map: Record<string, number> = {
    automatic: 0,
    manual: 1,
    cvt: 2,
    dct: 3,
  };
  return map[transmission?.toLowerCase() || ''] ?? 0;
};

const mapDrivetrainToEnum = (drivetrain?: string): number => {
  const map: Record<string, number> = {
    fwd: 0,
    rwd: 1,
    awd: 2,
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
  const customFields =
    typeof data.customFieldsJson === 'string'
      ? JSON.parse(data.customFieldsJson)
      : data.customFieldsJson || {};

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
    engine: data.engineSize || undefined,
    horsepower: data.horsepower || undefined,
    mpg: estimateMpg(),
    vin: data.vin || undefined,
    condition: mapCondition(data.condition?.toString() || '1'),
    description: data.description,
    features: JSON.parse(data.featuresJson || '[]'),
    images,
    location: data.city && data.state ? `${data.city}, ${data.state}` : data.city || '',
    sellerId: data.sellerId || data.dealerId,
    sellerName: data.sellerName || 'Unknown',
    sellerPhone: data.sellerPhone || undefined,
    sellerEmail: data.sellerEmail || undefined,
    // New fields from VIN decoder
    trim: data.trim || undefined,
    doors: data.doors || undefined,
    seats: data.seats || undefined,
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

const mapBackendStatusToFrontend = (
  status: number
): 'pending' | 'approved' | 'rejected' | 'sold' => {
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
  // Optional fields from VIN decode
  trim?: string;
  doors?: number;
  seats?: number;
}

/**
 * Update vehicle listing using VehiclesSaleService
 * PUT /api/vehicles/{id}
 */
export const updateVehicle = async (
  id: string,
  vehicleData: Partial<Vehicle>
): Promise<Vehicle> => {
  try {
    const token = localStorage.getItem('accessToken');

    // Map frontend Vehicle fields to backend Vehicle entity
    const backendPayload: Record<string, unknown> = {};

    // Only include fields that are present in vehicleData
    if (vehicleData.title !== undefined) backendPayload.title = vehicleData.title;
    if (vehicleData.description !== undefined) backendPayload.description = vehicleData.description;
    if (vehicleData.price !== undefined) backendPayload.price = vehicleData.price;
    if (vehicleData.make !== undefined) backendPayload.make = vehicleData.make;
    if (vehicleData.model !== undefined) backendPayload.model = vehicleData.model;
    if (vehicleData.year !== undefined) backendPayload.year = vehicleData.year;
    if (vehicleData.mileage !== undefined) backendPayload.mileage = vehicleData.mileage;
    if (vehicleData.color !== undefined) backendPayload.exteriorColor = vehicleData.color;
    if (vehicleData.interiorColor !== undefined)
      backendPayload.interiorColor = vehicleData.interiorColor;
    if (vehicleData.location !== undefined) backendPayload.city = vehicleData.location;
    if (vehicleData.vin !== undefined) backendPayload.vin = vehicleData.vin;

    // Map fuelType string to enum
    if (vehicleData.fuelType !== undefined) {
      const fuelTypeMap: Record<string, number> = {
        Gasoline: 0,
        Diesel: 1,
        Electric: 4,
        Hybrid: 2,
        PluginHybrid: 3,
      };
      backendPayload.fuelType = fuelTypeMap[vehicleData.fuelType] ?? 0;
    }

    // Map transmission string to enum
    if (vehicleData.transmission !== undefined) {
      const transmissionMap: Record<string, number> = {
        Manual: 0,
        Automatic: 1,
        CVT: 2,
        SemiAutomatic: 3,
        DualClutch: 4,
      };
      backendPayload.transmission = transmissionMap[vehicleData.transmission] ?? 1;
    }

    // Map bodyType string to enum
    if (vehicleData.bodyType !== undefined) {
      const bodyStyleMap: Record<string, number> = {
        Sedan: 0,
        Coupe: 1,
        SUV: 2,
        Crossover: 3,
        Pickup: 4,
        Wagon: 5,
        Hatchback: 6,
        Van: 7,
        Convertible: 8,
        SportsCar: 9,
        Truck: 4,
      };
      backendPayload.bodyStyle = bodyStyleMap[vehicleData.bodyType] ?? 0;
    }

    // Map condition string to enum
    if (vehicleData.condition !== undefined) {
      const conditionMap: Record<string, number> = {
        New: 0,
        Used: 1,
        'Certified Pre-Owned': 2,
      };
      backendPayload.condition = conditionMap[vehicleData.condition] ?? 1;
    }

    // Map features to JSON
    if (vehicleData.features !== undefined) {
      backendPayload.featuresJson = JSON.stringify(vehicleData.features);
    }

    // VehiclesSaleService PUT endpoint
    const response = await axios.put(`${VEHICLES_SALE_API_URL}/vehicles/${id}`, backendPayload, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.data) {
      throw new Error('Failed to update vehicle');
    }

    return transformBackendVehicleToFrontend(response.data);
  } catch (error: unknown) {
    if (isAxiosError(error) && error.response?.data?.message) {
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
  } catch (error: unknown) {
    if (isAxiosError(error) && error.response?.data?.message) {
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
  } catch (error: unknown) {
    console.error('Error fetching my vehicles:', error);
    return [];
  }
};

/**
 * Get dealer's inventory vehicles with engagement metrics
 * Uses VehiclesSaleService endpoint: GET /api/vehicles/dealer/{dealerId}
 */
export const getDealerVehicles = async (dealerId: string): Promise<DealerInventoryResponse> => {
  try {
    const token = localStorage.getItem('accessToken');

    const response = await axios.get(`${VEHICLES_SALE_API_URL}/vehicles/dealer/${dealerId}`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    const vehiclesData = response.data || [];
    const vehicles: DealerVehicle[] = Array.isArray(vehiclesData)
      ? vehiclesData.map((v: any) => transformBackendToDealerVehicle(v))
      : [];

    // Calculate counts by status
    const activeCount = vehicles.filter((v) => v.status === 'approved').length;
    const pausedCount = vehicles.filter((v) => v.status === 'pending').length;
    const soldCount = vehicles.filter((v) => v.status === 'sold').length;
    const draftCount = vehicles.filter((v) => v.status === 'rejected').length;

    return {
      vehicles,
      total: vehicles.length,
      activeCount,
      pausedCount,
      soldCount,
      draftCount,
    };
  } catch (error: unknown) {
    console.error('Error fetching dealer vehicles:', error);
    return {
      vehicles: [],
      total: 0,
      activeCount: 0,
      pausedCount: 0,
      soldCount: 0,
      draftCount: 0,
    };
  }
};

/**
 * Transform backend vehicle to DealerVehicle with engagement metrics
 */
const transformBackendToDealerVehicle = (data: any): DealerVehicle => {
  // Get images
  const images = data.images?.map((img: any) => {
    const url = img.url || img.imageUrl || '';
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    return url || '/placeholder-car.jpg';
  }) || ['/placeholder-car.jpg'];

  // Map backend status to frontend
  // Backend VehicleStatus enum:
  // 0=Draft, 1=PendingReview, 2=Active, 3=Reserved, 4=Sold, 5=Archived, 6=Rejected
  const mapStatus = (status: number | string): 'pending' | 'approved' | 'rejected' | 'sold' => {
    // Handle numeric status
    const numericMap: Record<number, 'pending' | 'approved' | 'rejected' | 'sold'> = {
      0: 'rejected', // Draft -> Borrador (use 'rejected' as display key)
      1: 'pending', // PendingReview -> En Evaluación
      2: 'approved', // Active -> Publicado
      3: 'approved', // Reserved -> (treat as published)
      4: 'sold', // Sold -> Vendido
      5: 'rejected', // Archived -> (treat as draft)
      6: 'rejected', // Rejected -> (treat as draft)
    };

    // Handle string status (in case backend returns status name instead of number)
    const stringMap: Record<string, 'pending' | 'approved' | 'rejected' | 'sold'> = {
      draft: 'rejected',
      pendingreview: 'pending',
      pending: 'pending',
      active: 'approved',
      approved: 'approved',
      published: 'approved',
      reserved: 'approved',
      sold: 'sold',
      archived: 'rejected',
      rejected: 'rejected',
    };

    if (typeof status === 'number') {
      return numericMap[status] || 'rejected';
    }

    if (typeof status === 'string') {
      return stringMap[status.toLowerCase()] || 'rejected';
    }

    return 'rejected';
  };

  return {
    id: data.id,
    title: data.title || `${data.year} ${data.make} ${data.model}`,
    make: data.make || '',
    model: data.model || '',
    year: data.year || 0,
    price: data.price || 0,
    mileage: data.mileage || 0,
    fuelType: data.fuelType?.toString() || 'Gasoline',
    transmission: data.transmission?.toString() || 'Automatic',
    bodyType: data.bodyStyle?.toString() || 'Sedan',
    color: data.exteriorColor || '',
    description: data.description || '',
    features: JSON.parse(data.featuresJson || '[]'),
    images,
    location: data.city && data.state ? `${data.city}, ${data.state}` : data.city || '',
    sellerId: data.sellerId || data.dealerId,
    sellerName: data.sellerName || 'Dealer',
    status: mapStatus(data.status),
    isFeatured: data.isFeatured || false,
    createdAt: data.createdAt,
    updatedAt: data.updatedAt,
    // Engagement metrics
    viewCount: data.viewCount || 0,
    inquiryCount: data.inquiryCount || 0,
    favoriteCount: data.favoriteCount || 0,
    stockNumber: data.stockNumber,
    dealerId: data.dealerId,
    publishedAt: data.publishedAt,
    soldAt: data.soldAt,
  };
};

/**
 * Get all categories
 */
export const getCategories = async (): Promise<Category[]> => {
  try {
    // Categories endpoint returns array directly
    const response = await axios.get<BackendCategory[]>(`${PRODUCT_API_URL}/Categories`);

    return (response.data || []).map(transformCategory);
  } catch (error: unknown) {
    console.error('Error fetching categories:', error);
    return [];
  }
};

/**
 * Get root categories (top-level)
 */
export const getRootCategories = async (): Promise<Category[]> => {
  try {
    const response = await axios.get<BackendCategory[]>(`${PRODUCT_API_URL}/Categories/root`);

    return (response.data || []).map(transformCategory);
  } catch (error: unknown) {
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
    const response = await axios.get<BackendCategory>(`${PRODUCT_API_URL}/Categories/slug/${slug}`);

    if (!response.data) {
      return null;
    }

    return transformCategory(response.data);
  } catch (error: unknown) {
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
  return result.vehicles.filter((v) => v.status === 'pending');
};

export const markAsSold = async (id: string): Promise<Vehicle> => {
  return updateVehicle(id, { status: 'sold' });
};

/**
 * Update vehicle status using backend VehicleStatus enum values directly
 *
 * VehicleStatus enum values:
 * - 0: Draft (not published)
 * - 1: PendingReview (awaiting moderation)
 * - 2: Active (published and visible)
 * - 3: Reserved (in negotiation)
 * - 4: Sold (sale completed)
 * - 5: Archived (hidden from public)
 * - 6: Rejected (failed moderation)
 */
export const updateVehicleStatus = async (id: string, status: number): Promise<void> => {
  try {
    const token = localStorage.getItem('accessToken');

    await axios.put(
      `${VEHICLES_SALE_API_URL}/vehicles/${id}`,
      { status },
      {
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      }
    );
  } catch (error: unknown) {
    console.error('Error updating vehicle status:', error);
    if (isAxiosError(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message);
    }
    throw new Error('Failed to update vehicle status');
  }
};

export const getVehicleMakes = async (): Promise<string[]> => {
  // TODO: Implement when backend has makes endpoint or populate from categories
  return [
    'Toyota',
    'Honda',
    'Ford',
    'Chevrolet',
    'BMW',
    'Mercedes-Benz',
    'Audi',
    'Nissan',
    'Hyundai',
    'Kia',
  ];
};

export const getVehicleModels = async (_make: string): Promise<string[]> => {
  // TODO: Implement when backend has models endpoint
  return [];
};

export const compareVehicles = async (vehicleIds: string[]): Promise<Vehicle[]> => {
  try {
    console.log('📊 Comparing vehicles:', vehicleIds);

    const response = await axios.post(`${VEHICLES_SALE_API_URL}/vehicles/compare`, {
      vehicleIds,
    });

    console.log('✅ Compare response:', response.data);

    // Transform backend data to frontend format
    const vehicles = Array.isArray(response.data) ? response.data : [];
    return vehicles.map(transformBackendVehicleToFrontend);
  } catch (error: unknown) {
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
  updateVehicleStatus,
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
