/**
 * Vehicles Service - API client for vehicle operations
 * Connects via API Gateway to VehiclesSaleService
 */

import { apiClient } from '@/lib/api-client';
import type {
  Vehicle,
  VehicleCardData,
  VehicleSearchParams,
  PaginatedResponse,
  DealRating,
} from '@/types';

// ============================================================
// API TYPES (matching backend DTOs)
// ============================================================

export interface VehicleDto {
  id: string;
  slug: string;
  make: string;
  model: string;
  year: number;
  trim?: string;
  bodyType: string;
  price: number;
  originalPrice?: number;
  marketPrice?: number;
  currency: string;
  mileage: number;
  transmission: string;
  fuelType: string;
  drivetrain?: string;
  engineSize?: string;
  horsepower?: number;
  exteriorColor?: string;
  interiorColor?: string;
  doors?: number;
  seats?: number;
  features?: string[];
  images: VehicleImageDto[];
  has360View?: boolean;
  hasVideo?: boolean;
  status: string;
  condition: string;
  isFeatured?: boolean;
  viewCount?: number;
  favoriteCount?: number;
  sellerId: string;
  sellerType: string;
  seller?: SellerDto;
  city: string;
  province: string;
  country: string;
  latitude?: number;
  longitude?: number;
  description?: string;
  vin?: string;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
  isNegotiable?: boolean;
  isCertified?: boolean;
  isVerified?: boolean;
}

export interface VehicleImageDto {
  id: string;
  url: string;
  thumbnailUrl?: string;
  alt?: string;
  order: number;
  isPrimary?: boolean;
}

export interface SellerDto {
  id: string;
  name: string;
  type: 'individual' | 'dealer';
  avatar?: string;
  phone?: string;
  email?: string;
  city?: string;
  rating?: number;
  reviewCount?: number;
  responseRate?: number;
  responseTime?: string;
  isVerified?: boolean;
  memberSince?: string;
  listingsCount?: number;
}

export interface VehicleSearchResponse {
  items: VehicleDto[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// ============================================================
// TRANSFORM FUNCTIONS
// ============================================================

export const transformVehicle = (dto: VehicleDto): Vehicle => ({
  id: dto.id,
  slug: dto.slug,
  make: dto.make,
  model: dto.model,
  year: dto.year,
  trim: dto.trim,
  bodyType: dto.bodyType as Vehicle['bodyType'],
  price: dto.price,
  originalPrice: dto.originalPrice,
  marketPrice: dto.marketPrice,
  currency: (dto.currency || 'DOP') as 'DOP' | 'USD',
  dealRating: calculateDealRating(dto.price, dto.marketPrice),
  mileage: dto.mileage,
  transmission: dto.transmission as Vehicle['transmission'],
  fuelType: dto.fuelType as Vehicle['fuelType'],
  drivetrain: dto.drivetrain as Vehicle['drivetrain'],
  engineSize: dto.engineSize,
  horsepower: dto.horsepower,
  exteriorColor: dto.exteriorColor,
  interiorColor: dto.interiorColor,
  doors: dto.doors,
  seats: dto.seats,
  features: dto.features || [],
  images: dto.images.map(img => ({
    id: img.id,
    url: img.url,
    thumbnailUrl: img.thumbnailUrl,
    alt: img.alt,
    order: img.order,
    isPrimary: img.isPrimary,
  })),
  has360View: dto.has360View,
  hasVideo: dto.hasVideo,
  status: dto.status as Vehicle['status'],
  condition: dto.condition as Vehicle['condition'],
  isFeatured: dto.isFeatured,
  viewCount: dto.viewCount,
  favoriteCount: dto.favoriteCount,
  sellerId: dto.sellerId,
  sellerType: dto.sellerType as Vehicle['sellerType'],
  location: {
    city: dto.city,
    province: dto.province,
    country: dto.country || 'DO',
    coordinates:
      dto.latitude && dto.longitude
        ? { latitude: dto.latitude, longitude: dto.longitude }
        : undefined,
  },
  createdAt: dto.createdAt,
  updatedAt: dto.updatedAt,
  publishedAt: dto.publishedAt,
});

export const transformToCardData = (dto: VehicleDto): VehicleCardData => ({
  id: dto.id,
  slug: dto.slug,
  make: dto.make,
  model: dto.model,
  year: dto.year,
  price: dto.price,
  currency: (dto.currency as 'DOP' | 'USD') || 'DOP',
  mileage: dto.mileage,
  transmission: dto.transmission,
  fuelType: dto.fuelType,
  imageUrl: dto.images[0]?.url || '/placeholder-car.jpg',
  dealRating: calculateDealRating(dto.price, dto.marketPrice),
  location: `${dto.city}, ${dto.province}`,
  trim: dto.trim,
  photoCount: dto.images.length,
  isNew: dto.condition === 'new',
  isCertified: dto.isCertified,
  monthlyPayment: calculateMonthlyPayment(dto.price),
  dealerName: dto.seller?.name,
  dealerRating: dto.seller?.rating,
  // Status and metadata
  status: (dto.status as VehicleCardData['status']) || 'active',
  viewCount: dto.viewCount,
  createdAt: dto.createdAt,
});

function calculateDealRating(price: number, marketPrice?: number): DealRating | undefined {
  if (!marketPrice || marketPrice === 0) return undefined;

  const priceDiff = ((marketPrice - price) / marketPrice) * 100;

  if (priceDiff >= 10) return 'great';
  if (priceDiff >= 5) return 'good';
  if (priceDiff >= -5) return 'fair';
  if (priceDiff < -10) return 'high';
  return 'uncertain';
}

function calculateMonthlyPayment(price: number): number {
  // Simple calculation: 60 months, 12% annual rate
  const months = 60;
  const annualRate = 0.12;
  const monthlyRate = annualRate / 12;
  const payment =
    (price * monthlyRate * Math.pow(1 + monthlyRate, months)) /
    (Math.pow(1 + monthlyRate, months) - 1);
  return Math.round(payment);
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get vehicle by slug
 */
export async function getVehicleBySlug(slug: string): Promise<Vehicle> {
  const response = await apiClient.get<VehicleDto>(`/api/vehicles/slug/${slug}`);
  return transformVehicle(response.data);
}

/**
 * Get vehicle by ID
 */
export async function getVehicleById(id: string): Promise<Vehicle> {
  const response = await apiClient.get<VehicleDto>(`/api/vehicles/${id}`);
  return transformVehicle(response.data);
}

/**
 * Search vehicles with filters
 */
export async function searchVehicles(
  params: VehicleSearchParams
): Promise<PaginatedResponse<VehicleCardData>> {
  const response = await apiClient.get<VehicleSearchResponse>('/api/vehicles/search', {
    params,
  });

  return {
    items: response.data.items.map(transformToCardData),
    pagination: {
      page: response.data.page,
      pageSize: response.data.pageSize,
      totalItems: response.data.totalItems,
      totalPages: response.data.totalPages,
      hasNextPage: response.data.page < response.data.totalPages,
      hasPreviousPage: response.data.page > 1,
    },
  };
}

/**
 * Get similar vehicles
 */
export async function getSimilarVehicles(
  vehicleId: string,
  limit: number = 4
): Promise<VehicleCardData[]> {
  const response = await apiClient.get<VehicleDto[]>(`/api/vehicles/${vehicleId}/similar`, {
    params: { limit },
  });
  return response.data.map(transformToCardData);
}

/**
 * Get featured vehicles
 */
export async function getFeaturedVehicles(limit: number = 8): Promise<VehicleCardData[]> {
  const response = await apiClient.get<VehicleDto[]>('/api/vehicles/featured', {
    params: { limit },
  });
  return response.data.map(transformToCardData);
}

/**
 * Track vehicle view
 */
export async function trackVehicleView(vehicleId: string): Promise<void> {
  await apiClient.post(`/api/vehicles/${vehicleId}/view`);
}

/**
 * Get vehicles by dealer ID
 */
export async function getVehiclesByDealer(
  dealerId: string,
  params: { page?: number; pageSize?: number; status?: string } = {}
): Promise<PaginatedResponse<VehicleCardData>> {
  const response = await apiClient.get<VehicleSearchResponse>('/api/vehicles/search', {
    params: {
      sellerId: dealerId,
      page: params.page || 1,
      pageSize: params.pageSize || 20,
      ...(params.status && { status: params.status }),
    },
  });

  return {
    items: response.data.items.map(transformToCardData),
    pagination: {
      page: response.data.page,
      pageSize: response.data.pageSize,
      totalItems: response.data.totalItems,
      totalPages: response.data.totalPages,
      hasNextPage: response.data.page < response.data.totalPages,
      hasPreviousPage: response.data.page > 1,
    },
  };
}

/**
 * Get vehicles by IDs (for comparison)
 */
export async function getVehiclesByIds(ids: string[]): Promise<VehicleCardData[]> {
  if (ids.length === 0) return [];

  // Use the compare endpoint which accepts vehicle IDs
  try {
    const response = await apiClient.post<VehicleDto[]>('/api/vehicles/batch', {
      ids,
    });
    return response.data.map(transformToCardData);
  } catch {
    // Fallback: Fetch each vehicle individually
    const vehicles = await Promise.all(
      ids.map(async id => {
        try {
          const response = await apiClient.get<VehicleDto>(`/api/vehicles/${id}`);
          return transformToCardData(response.data);
        } catch {
          return null;
        }
      })
    );
    return vehicles.filter((v): v is VehicleCardData => v !== null);
  }
}

// ============================================================
// 360° VIEW TYPES
// ============================================================

export interface Hotspot360 {
  id: string;
  x: number; // percentage 0-100
  y: number; // percentage 0-100
  label: string;
  description: string;
}

export interface View360Data {
  vehicleId: string;
  vehicleTitle: string;
  exteriorImages: string[];
  interiorImages: string[];
  hotspots: Hotspot360[];
  hasExterior360: boolean;
  hasInterior360: boolean;
}

/**
 * Get 360° view data for a vehicle
 */
export async function getVehicle360View(vehicleIdOrSlug: string): Promise<View360Data> {
  try {
    const response = await apiClient.get<View360Data>(`/api/vehicles/${vehicleIdOrSlug}/360`);
    return response.data;
  } catch {
    // Return empty data if 360 view not available
    // The API will return vehicle basic info if no 360 images uploaded
    const vehicle = await getVehicleBySlug(vehicleIdOrSlug);
    return {
      vehicleId: vehicle.id,
      vehicleTitle: `${vehicle.year} ${vehicle.make} ${vehicle.model}`,
      exteriorImages: vehicle.images.map(img => img.url),
      interiorImages: [],
      hotspots: [],
      hasExterior360: false,
      hasInterior360: false,
    };
  }
}

// ============================================================
// CREATE VEHICLE TYPES
// ============================================================

export interface CreateVehicleRequest {
  make: string;
  model: string;
  year: number;
  trim?: string;
  mileage: number;
  vin?: string;
  transmission: string;
  fuelType: string;
  bodyType: string;
  exteriorColor?: string;
  interiorColor?: string;
  condition: string;
  price: number;
  currency?: 'DOP' | 'USD';
  description?: string;
  features: string[];
  images: CreateVehicleImage[];
  city: string;
  province: string;
  sellerName?: string;
  sellerPhone?: string;
  sellerEmail?: string;
  isNegotiable?: boolean;
}

export interface CreateVehicleImage {
  url: string;
  order: number;
  isPrimary?: boolean;
  alt?: string;
}

export interface CreateVehicleResponse {
  id: string;
  slug: string;
  status: string;
  message: string;
}

/**
 * Update vehicle request - extends CreateVehicleRequest with status management
 */
export interface UpdateVehicleRequest extends Partial<CreateVehicleRequest> {
  status?: 'draft' | 'pending' | 'active' | 'paused' | 'sold' | 'reserved' | 'expired' | 'rejected';
}

// ============================================================
// CREATE VEHICLE FUNCTION
// ============================================================

/**
 * Create a new vehicle listing
 */
export async function createVehicle(data: CreateVehicleRequest): Promise<CreateVehicleResponse> {
  const response = await apiClient.post<CreateVehicleResponse>('/api/vehicles', data);
  return response.data;
}

/**
 * Update an existing vehicle
 */
export async function updateVehicle(id: string, data: UpdateVehicleRequest): Promise<Vehicle> {
  const response = await apiClient.put<VehicleDto>(`/api/vehicles/${id}`, data);
  return transformVehicle(response.data);
}

/**
 * Delete a vehicle
 */
export async function deleteVehicle(id: string): Promise<void> {
  await apiClient.delete(`/api/vehicles/${id}`);
}

// ============================================================
// CATALOG TYPES
// ============================================================

export interface VehicleMake {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  modelsCount?: number;
}

export interface VehicleModel {
  id: string;
  name: string;
  slug: string;
  makeId: string;
  years?: number[];
}

export interface CatalogOption {
  value: string;
  label: string;
}

// ============================================================
// CATALOG FUNCTIONS
// ============================================================

/**
 * Get all vehicle makes
 */
export async function getMakes(): Promise<VehicleMake[]> {
  try {
    const response = await apiClient.get<VehicleMake[]>('/api/catalog/makes');
    return response.data;
  } catch {
    // Fallback to static list if API fails
    return getStaticMakes();
  }
}

/**
 * Get models for a specific make
 */
export async function getModelsByMake(makeId: string): Promise<VehicleModel[]> {
  try {
    const response = await apiClient.get<VehicleModel[]>(`/api/catalog/makes/${makeId}/models`);
    return response.data;
  } catch {
    // Fallback to empty list
    return [];
  }
}

/**
 * Get body types
 */
export async function getBodyTypes(): Promise<CatalogOption[]> {
  try {
    const response = await apiClient.get<CatalogOption[]>('/api/catalog/body-types');
    return response.data;
  } catch {
    return getStaticBodyTypes();
  }
}

/**
 * Get fuel types
 */
export async function getFuelTypes(): Promise<CatalogOption[]> {
  try {
    const response = await apiClient.get<CatalogOption[]>('/api/catalog/fuel-types');
    return response.data;
  } catch {
    return getStaticFuelTypes();
  }
}

/**
 * Get transmissions
 */
export async function getTransmissions(): Promise<CatalogOption[]> {
  try {
    const response = await apiClient.get<CatalogOption[]>('/api/catalog/transmissions');
    return response.data;
  } catch {
    return getStaticTransmissions();
  }
}

/**
 * Get colors
 */
export async function getColors(): Promise<CatalogOption[]> {
  try {
    const response = await apiClient.get<CatalogOption[]>('/api/catalog/colors');
    return response.data;
  } catch {
    return getStaticColors();
  }
}

/**
 * Get provinces (locations)
 */
export async function getProvinces(): Promise<CatalogOption[]> {
  try {
    const response = await apiClient.get<CatalogOption[]>('/api/catalog/provinces');
    return response.data;
  } catch {
    return getStaticProvinces();
  }
}

/**
 * Get features by category
 */
export async function getFeatures(): Promise<Record<string, string[]>> {
  try {
    const response = await apiClient.get<Record<string, string[]>>('/api/catalog/features');
    return response.data;
  } catch {
    return getStaticFeatures();
  }
}

// ============================================================
// STATIC FALLBACK DATA
// ============================================================

function getStaticMakes(): VehicleMake[] {
  const makes = [
    'Toyota',
    'Honda',
    'Hyundai',
    'Nissan',
    'Kia',
    'Mazda',
    'Ford',
    'Chevrolet',
    'BMW',
    'Mercedes-Benz',
    'Audi',
    'Volkswagen',
    'Lexus',
    'Jeep',
    'Mitsubishi',
    'Suzuki',
    'Subaru',
    'Dodge',
  ];
  return makes.map((name, i) => ({
    id: `make-${i + 1}`,
    name,
    slug: name.toLowerCase().replace(/\s+/g, '-'),
  }));
}

function getStaticBodyTypes(): CatalogOption[] {
  return [
    { value: 'sedan', label: 'Sedán' },
    { value: 'suv', label: 'SUV' },
    { value: 'pickup', label: 'Pickup' },
    { value: 'hatchback', label: 'Hatchback' },
    { value: 'coupe', label: 'Coupé' },
    { value: 'minivan', label: 'Minivan' },
    { value: 'crossover', label: 'Crossover' },
    { value: 'wagon', label: 'Wagon' },
    { value: 'convertible', label: 'Convertible' },
  ];
}

function getStaticFuelTypes(): CatalogOption[] {
  return [
    { value: 'gasoline', label: 'Gasolina' },
    { value: 'diesel', label: 'Diésel' },
    { value: 'hybrid', label: 'Híbrido' },
    { value: 'electric', label: 'Eléctrico' },
    { value: 'lpg', label: 'GLP' },
  ];
}

function getStaticTransmissions(): CatalogOption[] {
  return [
    { value: 'automatic', label: 'Automática' },
    { value: 'manual', label: 'Manual' },
    { value: 'cvt', label: 'CVT' },
    { value: 'semi-automatic', label: 'Semi-automática' },
  ];
}

function getStaticColors(): CatalogOption[] {
  return [
    { value: 'white', label: 'Blanco' },
    { value: 'black', label: 'Negro' },
    { value: 'gray', label: 'Gris' },
    { value: 'silver', label: 'Plata' },
    { value: 'red', label: 'Rojo' },
    { value: 'blue', label: 'Azul' },
    { value: 'green', label: 'Verde' },
    { value: 'brown', label: 'Marrón' },
    { value: 'beige', label: 'Beige' },
    { value: 'gold', label: 'Dorado' },
  ];
}

function getStaticProvinces(): CatalogOption[] {
  return [
    { value: 'santo-domingo', label: 'Santo Domingo' },
    { value: 'distrito-nacional', label: 'Distrito Nacional' },
    { value: 'santiago', label: 'Santiago' },
    { value: 'la-vega', label: 'La Vega' },
    { value: 'san-cristobal', label: 'San Cristóbal' },
    { value: 'puerto-plata', label: 'Puerto Plata' },
    { value: 'samana', label: 'Samaná' },
    { value: 'la-romana', label: 'La Romana' },
    { value: 'san-pedro', label: 'San Pedro de Macorís' },
    { value: 'punta-cana', label: 'Punta Cana' },
  ];
}

function getStaticFeatures(): Record<string, string[]> {
  return {
    seguridad: [
      'ABS',
      'Airbags',
      'Cámara de reversa',
      'Sensores de estacionamiento',
      'Control de estabilidad',
      'Alarma',
      'Bloqueo central',
    ],
    confort: [
      'Aire acondicionado',
      'Asientos de cuero',
      'Asientos calefactados',
      'Sunroof',
      'Control crucero',
      'Sensor de lluvia',
      'Techo panorámico',
    ],
    entretenimiento: [
      'Bluetooth',
      'Apple CarPlay',
      'Android Auto',
      'Sistema de sonido premium',
      'Pantalla táctil',
      'Navegación GPS',
      'Cargador inalámbrico',
    ],
    rendimiento: [
      'Modo Sport',
      'Paletas de cambio',
      'Turbo',
      'Tracción 4x4',
      'Suspensión deportiva',
      'Control de tracción',
    ],
  };
}

/**
 * Vehicle service object
 */
export const vehicleService = {
  // Read operations
  getBySlug: getVehicleBySlug,
  getById: getVehicleById,
  search: searchVehicles,
  getSimilar: getSimilarVehicles,
  getFeatured: getFeaturedVehicles,
  trackView: trackVehicleView,
  getByDealer: getVehiclesByDealer,
  getByIds: getVehiclesByIds,
  get360View: getVehicle360View,

  // Write operations
  create: createVehicle,
  update: updateVehicle,
  delete: deleteVehicle,

  // Catalog operations
  getMakes,
  getModelsByMake,
  getBodyTypes,
  getFuelTypes,
  getTransmissions,
  getColors,
  getProvinces,
  getFeatures,
};

export default vehicleService;
