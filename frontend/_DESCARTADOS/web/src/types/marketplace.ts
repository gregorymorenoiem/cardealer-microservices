// ============================================================================
// MARKETPLACE TYPES - Multi-Vertical (Vehicles + Real Estate)
// ============================================================================

/**
 * Verticales del marketplace
 */
export type MarketplaceVertical = 'vehicles' | 'real-estate';

/**
 * Tipo para selección de categoría (incluye opción 'all')
 */
export type MarketplaceCategorySelection = MarketplaceVertical | 'all';

/**
 * Tipo base abstracto para listados
 */
export interface BaseListing {
  id: string;
  vertical: MarketplaceVertical;
  title: string;
  description: string;
  price: number;
  currency: string;
  status: ListingStatus;
  isFeatured: boolean;
  images: ListingImage[];
  primaryImageUrl?: string;
  location: ListingLocation;
  seller: ListingSeller;
  viewCount: number;
  favoriteCount: number;
  inquiryCount: number;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
}

export type ListingStatus = 'draft' | 'active' | 'pending' | 'sold' | 'rented' | 'reserved' | 'archived';

export interface ListingImage {
  id: string;
  url: string;
  thumbnailUrl?: string;
  caption?: string;
  category?: string;
  sortOrder: number;
  isPrimary: boolean;
}

export interface ListingLocation {
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  neighborhood?: string;
  latitude?: number;
  longitude?: number;
}

export interface ListingSeller {
  id: string;
  name: string;
  phone?: string;
  email?: string;
  avatar?: string;
  rating?: number;
  reviewCount?: number;
  memberSince?: string;
  isVerified: boolean;
  isDealership: boolean;
}

// ============================================================================
// VEHICLE TYPES
// ============================================================================

export interface VehicleListing extends BaseListing {
  vertical: 'vehicles';
  vehicleType: VehicleType;
  make: string;
  model: string;
  year: number;
  mileage: number;
  mileageUnit: 'km' | 'miles';
  transmission: VehicleTransmission;
  fuelType: VehicleFuelType;
  bodyType: VehicleBodyType;
  drivetrain: VehicleDrivetrain;
  engine?: string;
  horsepower?: number;
  mpg?: {
    city: number;
    highway: number;
  };
  exteriorColor: string;
  interiorColor?: string;
  vin?: string;
  condition: VehicleCondition;
  features: string[];
}

export type VehicleType = 'car' | 'motorcycle' | 'truck' | 'suv' | 'van';
export type VehicleTransmission = 'automatic' | 'manual' | 'cvt';
export type VehicleFuelType = 'gasoline' | 'diesel' | 'electric' | 'hybrid' | 'plug-in-hybrid';
export type VehicleBodyType = 'sedan' | 'suv' | 'truck' | 'coupe' | 'hatchback' | 'van' | 'convertible' | 'wagon';
export type VehicleDrivetrain = 'fwd' | 'rwd' | 'awd' | '4wd';
export type VehicleCondition = 'new' | 'used' | 'certified-pre-owned';

// ============================================================================
// REAL ESTATE TYPES
// ============================================================================

export interface PropertyListing extends BaseListing {
  vertical: 'real-estate';
  propertyType: PropertyType;
  listingType: PropertyListingType;
  pricePerSqMeter?: number;
  maintenanceFee?: number;
  isNegotiable: boolean;
  // Physical features
  totalArea: number;
  areaUnit: 'sqm' | 'sqft';
  builtArea?: number;
  lotArea?: number;
  bedrooms: number;
  bathrooms: number;
  halfBathrooms?: number;
  parkingSpaces?: number;
  floor?: number;
  totalFloors?: number;
  yearBuilt?: number;
  // Amenities
  hasGarden: boolean;
  hasPool: boolean;
  hasGym: boolean;
  hasElevator: boolean;
  hasSecurity: boolean;
  isFurnished: boolean;
  allowsPets: boolean;
  amenities: PropertyAmenity[];
}

export type PropertyType = 'house' | 'apartment' | 'condo' | 'townhouse' | 'land' | 'commercial' | 'office' | 'warehouse' | 'building';
export type PropertyListingType = 'sale' | 'rent' | 'sale-or-rent';

export interface PropertyAmenity {
  name: string;
  category: AmenityCategory;
  icon?: string;
}

export type AmenityCategory = 'security' | 'recreation' | 'comfort' | 'services' | 'outdoor' | 'technology';

// ============================================================================
// UNIFIED LISTING TYPE
// ============================================================================

export type Listing = VehicleListing | PropertyListing;

// Type guards
export function isVehicleListing(listing: Listing): listing is VehicleListing {
  return listing.vertical === 'vehicles';
}

export function isPropertyListing(listing: Listing): listing is PropertyListing {
  return listing.vertical === 'real-estate';
}

// ============================================================================
// SEARCH TYPES
// ============================================================================

export interface MarketplaceSearchParams {
  vertical?: MarketplaceVertical;
  query?: string;
  minPrice?: number;
  maxPrice?: number;
  city?: string;
  state?: string;
  isFeatured?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface VehicleSearchParams extends MarketplaceSearchParams {
  vertical: 'vehicles';
  make?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minMileage?: number;
  maxMileage?: number;
  transmission?: VehicleTransmission;
  fuelType?: VehicleFuelType;
  bodyType?: VehicleBodyType;
  condition?: VehicleCondition;
}

export interface PropertySearchParams extends MarketplaceSearchParams {
  vertical: 'real-estate';
  propertyType?: PropertyType;
  listingType?: PropertyListingType;
  minArea?: number;
  maxArea?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minBathrooms?: number;
  maxBathrooms?: number;
  hasPool?: boolean;
  hasGarden?: boolean;
  hasGym?: boolean;
  hasSecurity?: boolean;
  isFurnished?: boolean;
  allowsPets?: boolean;
}

export interface MarketplaceSearchResponse<T extends Listing> {
  listings: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ============================================================================
// CATEGORY CARD TYPES
// ============================================================================

export interface VerticalCategory {
  id: MarketplaceVertical;
  name: string;
  description: string;
  icon: string;
  gradient: string;
  stats: {
    listingCount: number;
    label: string;
  };
  subcategories: SubCategory[];
}

export interface SubCategory {
  id: string;
  name: string;
  icon?: string;
  count: number;
}

// ============================================================================
// UI CONFIGURATION
// ============================================================================

export const VERTICAL_CONFIG: Record<MarketplaceVertical, {
  name: string;
  namePlural: string;
  icon: string;
  color: string;
  gradient: string;
  bgLight: string;
}> = {
  'vehicles': {
    name: 'Vehículo',
    namePlural: 'Vehículos',
    icon: '🚗',
    color: 'blue',
    gradient: 'from-blue-500 to-cyan-500',
    bgLight: 'bg-blue-50',
  },
  'real-estate': {
    name: 'Inmueble',
    namePlural: 'Bienes Raíces',
    icon: '🏠',
    color: 'emerald',
    gradient: 'from-emerald-500 to-teal-500',
    bgLight: 'bg-emerald-50',
  },
};

export const PROPERTY_TYPE_LABELS: Record<PropertyType, string> = {
  'house': 'Casa',
  'apartment': 'Apartamento',
  'condo': 'Condominio',
  'townhouse': 'Casa Adosada',
  'land': 'Terreno',
  'commercial': 'Local Comercial',
  'office': 'Oficina',
  'warehouse': 'Bodega',
  'building': 'Edificio',
};

export const PROPERTY_LISTING_TYPE_LABELS: Record<PropertyListingType, string> = {
  'sale': 'En Venta',
  'rent': 'En Renta',
  'sale-or-rent': 'Venta o Renta',
};

export const VEHICLE_BODY_TYPE_LABELS: Record<VehicleBodyType, string> = {
  'sedan': 'Sedán',
  'suv': 'SUV',
  'truck': 'Pickup',
  'coupe': 'Coupé',
  'hatchback': 'Hatchback',
  'van': 'Van',
  'convertible': 'Convertible',
  'wagon': 'Wagon',
};

export const VEHICLE_FUEL_TYPE_LABELS: Record<VehicleFuelType, string> = {
  'gasoline': 'Gasolina',
  'diesel': 'Diésel',
  'electric': 'Eléctrico',
  'hybrid': 'Híbrido',
  'plug-in-hybrid': 'Híbrido Enchufable',
};
