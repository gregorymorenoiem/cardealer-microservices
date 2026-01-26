/**
 * VehiclesSaleService - Real API client for vehicle sales
 * Connects via API Gateway to VehiclesSaleService microservice
 */

import api from './api';
import { getVehicleSaleImageUrl } from '@utils/s3ImageUrl';

// ============================================================
// API CONFIGURATION
// ============================================================

// Use api instance with refresh token interceptor
const vehiclesApi = api;

// ============================================================
// BACKEND TYPES (matching VehiclesSaleService schema)
// ============================================================

export interface ApiVehicleImage {
  id: string;
  dealerId: string;
  vehicleId: string;
  url: string; // This is the photoId (e.g., "photo-1618843479313-40f8afb4b4d8")
  thumbnailUrl: string | null;
  caption: string | null;
  imageType: number;
  sortOrder: number;
  isPrimary: boolean;
  fileSize: number | null;
  mimeType: string | null;
  width: number | null;
  height: number | null;
  createdAt: string;
}

export interface ApiVehicle {
  id: string;
  dealerId: string;
  title: string;
  description: string;
  price: number;
  currency: string;
  status: number; // 0=Draft, 1=PendingReview, 2=Active, 3=Reserved, 4=Sold, 5=Archived, 6=Rejected
  sellerId: string;
  sellerName: string;
  sellerPhone: string | null;
  sellerEmail: string | null;
  vin: string | null;
  stockNumber: string | null;
  makeId: string | null;
  make: string;
  modelId: string | null;
  model: string;
  trim: string | null;
  year: number;
  generation: string | null;
  vehicleType: number; // 0=Car, 1=Truck, 2=SUV, 3=Van, 4=Motorcycle, etc.
  bodyStyle: number; // 0=Sedan, 1=Coupe, 2=Hatchback, 3=Wagon, 4=SUV, etc.
  doors: number;
  seats: number;
  fuelType: number; // 0=Gasoline, 1=Diesel, 2=Electric, 3=Hybrid, etc.
  engineSize: string | null;
  horsepower: number;
  torque: number;
  transmission: number; // 0=Automatic, 1=Manual, 2=CVT, 3=SemiAutomatic
  driveType: number; // 0=FWD, 1=RWD, 2=AWD, 3=FourWD
  cylinders: number;
  mileage: number;
  mileageUnit: number; // 0=Miles, 1=Kilometers
  condition: number; // 0=New, 1=CertifiedPreOwned, 2=Used, 3=Salvage, 4=Rebuilt
  previousOwners: number | null;
  accidentHistory: boolean;
  hasCleanTitle: boolean;
  exteriorColor: string | null;
  interiorColor: string | null;
  interiorMaterial: string | null;
  mpgCity: number | null;
  mpgHighway: number | null;
  mpgCombined: number | null;
  city: string;
  state: string;
  zipCode: string | null;
  country: string;
  latitude: number | null;
  longitude: number | null;
  isCertified: boolean;
  certificationProgram: string | null;
  carfaxReportUrl: string | null;
  lastServiceDate: string | null;
  serviceHistoryNotes: string | null;
  warrantyInfo: string | null;
  featuresJson: string;
  packagesJson: string;
  viewCount: number;
  favoriteCount: number;
  inquiryCount: number;
  createdAt: string;
  updatedAt: string;
  publishedAt: string | null;
  soldAt: string | null;
  isDeleted: boolean;
  isFeatured: boolean;
  categoryId: string | null;
  category: string | null;
  images: ApiVehicleImage[];
}

export interface ApiVehiclesResponse {
  vehicles: ApiVehicle[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================
// FRONTEND TYPES (for UI components)
// ============================================================

export interface VehicleListing {
  id: string;
  title: string;
  description: string;
  price: number;
  currency: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  fuelType: string;
  transmission: string;
  bodyStyle: string;
  condition: string;
  exteriorColor: string;
  location: string; // "City, State"
  images: string[]; // Full S3 URLs
  primaryImage: string; // First image URL
  isFeatured: boolean;
  viewCount: number;
  favoriteCount: number;
  sellerName: string;
  createdAt: string;
  status: 'draft' | 'pending' | 'active' | 'reserved' | 'sold' | 'archived' | 'rejected';
}

export interface VehiclesResponse {
  vehicles: VehicleListing[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
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
  bodyStyle?: string;
  condition?: string;
  city?: string;
  state?: string;
  search?: string;
}

// ============================================================
// ENUM MAPPINGS
// ============================================================

const BODY_STYLE_MAP: Record<number, string> = {
  0: 'Sedan',
  1: 'Coupe',
  2: 'Hatchback',
  3: 'Wagon',
  4: 'SUV',
  5: 'Crossover',
  6: 'Pickup', // Fixed: was incorrectly 'Convertible'
  7: 'Van',
  8: 'Minivan',
  9: 'Convertible', // Fixed: was incorrectly 'Minivan'
  10: 'SportsCar', // Fixed: was incorrectly 'Sports'
  99: 'Other', // Fixed: was incorrectly 11:'Luxury', 12:'Other'
};

const FUEL_TYPE_MAP: Record<number, string> = {
  0: 'Gasoline',
  1: 'Diesel',
  2: 'Electric',
  3: 'Hybrid',
  4: 'PlugInHybrid',
  5: 'Hydrogen',
  6: 'FlexFuel',
  7: 'NaturalGas',
  8: 'Other',
};

const TRANSMISSION_MAP: Record<number, string> = {
  0: 'Automatic',
  1: 'Manual',
  2: 'CVT',
  3: 'SemiAutomatic',
  4: 'DualClutch',
  5: 'Other',
};

const CONDITION_MAP: Record<number, string> = {
  0: 'New',
  1: 'Certified Pre-Owned',
  2: 'Used',
  3: 'Salvage',
  4: 'Rebuilt',
};

const STATUS_MAP: Record<number, VehicleListing['status']> = {
  0: 'draft',
  1: 'pending',
  2: 'active',
  3: 'reserved',
  4: 'sold',
  5: 'archived',
  6: 'rejected',
};

// ============================================================
// TRANSFORMERS
// ============================================================

/**
 * Transform API vehicle to frontend VehicleListing
 * Converts photoIds to full S3 URLs using s3ImageUrl utility
 */
const transformVehicle = (vehicle: ApiVehicle): VehicleListing => {
  // Transform images: photoId -> full S3 URL
  const images = vehicle.images.map((img) => getVehicleSaleImageUrl(img.url));

  // Get primary image (first one or placeholder)
  const primaryImage =
    images.length > 0 ? images[0] : getVehicleSaleImageUrl('placeholder-vehicle');

  return {
    id: vehicle.id,
    title: vehicle.title,
    description: vehicle.description,
    price: vehicle.price,
    currency: vehicle.currency,
    make: vehicle.make,
    model: vehicle.model,
    year: vehicle.year,
    mileage: vehicle.mileage,
    fuelType: FUEL_TYPE_MAP[vehicle.fuelType] || 'Unknown',
    transmission: TRANSMISSION_MAP[vehicle.transmission] || 'Unknown',
    bodyStyle: BODY_STYLE_MAP[vehicle.bodyStyle] || 'Other',
    condition: CONDITION_MAP[vehicle.condition] || 'Used',
    exteriorColor: vehicle.exteriorColor || 'Not specified',
    location: `${vehicle.city}, ${vehicle.state}`,
    images,
    primaryImage,
    isFeatured: vehicle.isFeatured,
    viewCount: vehicle.viewCount,
    favoriteCount: vehicle.favoriteCount,
    sellerName: vehicle.sellerName,
    createdAt: vehicle.createdAt,
    status: STATUS_MAP[vehicle.status] || 'draft',
  };
};

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get paginated list of vehicles
 */
export const getVehicles = async (
  page: number = 1,
  pageSize: number = 12,
  filters?: VehicleFilters
): Promise<VehiclesResponse> => {
  try {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    // Add filters if provided
    if (filters) {
      if (filters.make) params.append('make', filters.make);
      if (filters.model) params.append('model', filters.model);
      if (filters.minYear) params.append('minYear', filters.minYear.toString());
      if (filters.maxYear) params.append('maxYear', filters.maxYear.toString());
      if (filters.minPrice) params.append('minPrice', filters.minPrice.toString());
      if (filters.maxPrice) params.append('maxPrice', filters.maxPrice.toString());
      if (filters.search) params.append('search', filters.search);
    }

    const response = await vehiclesApi.get<ApiVehiclesResponse>(
      `/api/vehicles?${params.toString()}`
    );

    return {
      vehicles: response.data.vehicles.map(transformVehicle),
      total: response.data.totalCount,
      page: response.data.page,
      pageSize: response.data.pageSize,
      totalPages: response.data.totalPages,
    };
  } catch (error) {
    console.error('Error fetching vehicles:', error);
    throw error;
  }
};

/**
 * Get a single vehicle by ID
 */
export const getVehicleById = async (id: string): Promise<VehicleListing | null> => {
  try {
    const response = await vehiclesApi.get<ApiVehicle>(`/api/vehicles/${id}`);
    return transformVehicle(response.data);
  } catch (error) {
    console.error(`Error fetching vehicle ${id}:`, error);
    return null;
  }
};

/**
 * Get featured vehicles
 */
export const getFeaturedVehicles = async (limit: number = 6): Promise<VehicleListing[]> => {
  try {
    const response = await vehiclesApi.get<ApiVehiclesResponse>(
      `/api/vehicles?pageSize=${limit}&featured=true`
    );
    return response.data.vehicles.map(transformVehicle);
  } catch (error) {
    console.error('Error fetching featured vehicles:', error);
    // Return empty array on error - the component should handle this gracefully
    return [];
  }
};

/**
 * Get latest vehicles
 */
export const getLatestVehicles = async (limit: number = 6): Promise<VehicleListing[]> => {
  try {
    const response = await vehiclesApi.get<ApiVehiclesResponse>(
      `/api/vehicles?pageSize=${limit}&sortBy=createdAt&sortDesc=true`
    );
    return response.data.vehicles.map(transformVehicle);
  } catch (error) {
    console.error('Error fetching latest vehicles:', error);
    return [];
  }
};

/**
 * Search vehicles by text
 */
export const searchVehicles = async (
  query: string,
  page: number = 1,
  pageSize: number = 12
): Promise<VehiclesResponse> => {
  return getVehicles(page, pageSize, { search: query });
};

/**
 * Get vehicles by make
 */
export const getVehiclesByMake = async (
  make: string,
  page: number = 1,
  pageSize: number = 12
): Promise<VehiclesResponse> => {
  return getVehicles(page, pageSize, { make });
};

/**
 * Get vehicles by body style
 */
export const getVehiclesByBodyStyle = async (
  bodyStyle: string,
  page: number = 1,
  pageSize: number = 12
): Promise<VehiclesResponse> => {
  return getVehicles(page, pageSize, { bodyStyle });
};

// ============================================================
// CATALOG API (Makes, Models, Trims)
// ============================================================

export interface VehicleMake {
  id: string;
  name: string;
  logoUrl: string | null;
  country: string | null;
  isActive: boolean;
}

export interface VehicleModel {
  id: string;
  makeId: string;
  name: string;
  isActive: boolean;
}

/**
 * Get all vehicle makes
 */
export const getVehicleMakes = async (): Promise<VehicleMake[]> => {
  try {
    const response = await vehiclesApi.get<VehicleMake[]>('/api/vehicles/catalog/makes');
    return response.data;
  } catch (error) {
    console.error('Error fetching vehicle makes:', error);
    return [];
  }
};

/**
 * Get models by make
 */
export const getVehicleModels = async (makeId: string): Promise<VehicleModel[]> => {
  try {
    const response = await vehiclesApi.get<VehicleModel[]>(
      `/api/vehicles/catalog/makes/${makeId}/models`
    );
    return response.data;
  } catch (error) {
    console.error(`Error fetching models for make ${makeId}:`, error);
    return [];
  }
};

// ============================================================
// VEHICLE LIFECYCLE API (Publish/Unpublish/Sold/Feature/Views)
// ============================================================

export interface PublishVehicleRequest {
  expiresAt?: string;
}

export interface PublishVehicleResponse {
  id: string;
  status: number;
  publishedAt: string;
  expiresAt?: string;
  message: string;
}

export interface UnpublishVehicleRequest {
  reason?: string;
}

export interface UnpublishVehicleResponse {
  id: string;
  status: number;
  updatedAt: string;
  message: string;
}

export interface MarkSoldRequest {
  salePrice?: number;
  notes?: string;
}

export interface MarkSoldResponse {
  id: string;
  status: number;
  soldAt: string;
  salePrice?: number;
  message: string;
}

export interface FeatureVehicleRequest {
  isFeatured: boolean;
  homepageSections?: number;
}

export interface FeatureVehicleResponse {
  id: string;
  isFeatured: boolean;
  homepageSections: number;
  message: string;
}

export interface RegisterViewRequest {
  userId?: string;
  sessionId?: string;
  referrer?: string;
}

export interface RegisterViewResponse {
  vehicleId: string;
  totalViews: number;
  message: string;
}

/**
 * Publish a vehicle (Draft/Inactive -> Active)
 */
export const publishVehicle = async (
  vehicleId: string,
  request?: PublishVehicleRequest
): Promise<PublishVehicleResponse> => {
  const response = await vehiclesApi.post<PublishVehicleResponse>(
    `/api/vehicles/${vehicleId}/publish`,
    request || {}
  );
  return response.data;
};

/**
 * Unpublish a vehicle (Active -> Archived)
 */
export const unpublishVehicle = async (
  vehicleId: string,
  request?: UnpublishVehicleRequest
): Promise<UnpublishVehicleResponse> => {
  const response = await vehiclesApi.post<UnpublishVehicleResponse>(
    `/api/vehicles/${vehicleId}/unpublish`,
    request || {}
  );
  return response.data;
};

/**
 * Mark a vehicle as sold
 */
export const markVehicleAsSold = async (
  vehicleId: string,
  request?: MarkSoldRequest
): Promise<MarkSoldResponse> => {
  const response = await vehiclesApi.post<MarkSoldResponse>(
    `/api/vehicles/${vehicleId}/sold`,
    request || {}
  );
  return response.data;
};

/**
 * Feature or unfeature a vehicle (Admin only)
 */
export const featureVehicle = async (
  vehicleId: string,
  request: FeatureVehicleRequest
): Promise<FeatureVehicleResponse> => {
  const response = await vehiclesApi.post<FeatureVehicleResponse>(
    `/api/vehicles/${vehicleId}/feature`,
    request
  );
  return response.data;
};

/**
 * Register a view for a vehicle
 */
export const registerVehicleView = async (
  vehicleId: string,
  request?: RegisterViewRequest
): Promise<RegisterViewResponse> => {
  const response = await vehiclesApi.post<RegisterViewResponse>(
    `/api/vehicles/${vehicleId}/views`,
    request || {}
  );
  return response.data;
};

// ============================================================
// VIN DECODE API
// ============================================================

export interface VinSuggestedData {
  make: string;
  model: string;
  year: number;
  trim?: string;
  vehicleType: string;
  bodyStyle: string;
  fuelType: string;
  transmission: string;
  driveType: string;
  engineSize?: string;
  horsepower?: number;
  cylinders?: number;
}

export interface VinDecodeResponse {
  vin: string;
  isValid: boolean;
  make: string;
  model: string;
  year: number;
  trim?: string;
  vehicleType: string;
  bodyStyle: string;
  doors?: number;
  engineSize?: string;
  cylinders?: number;
  horsepower?: number;
  fuelType: string;
  transmission: string;
  driveType: string;
  plantCity?: string;
  plantCountry?: string;
  manufacturer?: string;
  series?: string;
  gvwr?: string;
  errorCode?: string;
  errorMessage?: string;
  suggestedData?: VinSuggestedData;
}

/**
 * Decode a VIN number using NHTSA API
 * Returns vehicle information for form auto-fill
 */
export const decodeVin = async (vin: string): Promise<VinDecodeResponse | null> => {
  try {
    const response = await vehiclesApi.get<VinDecodeResponse>(`/api/catalog/vin/${vin}/decode`);
    return response.data;
  } catch (error) {
    console.error(`Error decoding VIN ${vin}:`, error);
    return null;
  }
};

// Default export for convenience
export default {
  getVehicles,
  getVehicleById,
  getFeaturedVehicles,
  getLatestVehicles,
  searchVehicles,
  getVehiclesByMake,
  getVehiclesByBodyStyle,
  getVehicleMakes,
  getVehicleModels,
  // Lifecycle
  publishVehicle,
  unpublishVehicle,
  markVehicleAsSold,
  featureVehicle,
  registerVehicleView,
  // VIN
  decodeVin,
};
