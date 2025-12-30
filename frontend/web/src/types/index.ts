export interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: ApiError;
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalItems: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

// Auth Types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt?: string;
  requiresTwoFactor?: boolean;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export type AccountType = 'guest' | 'individual' | 'dealer' | 'dealer_employee' | 'admin' | 'platform_employee';

export interface User {
  id: string;
  email: string;
  name?: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  avatar?: string;
  phone?: string;
  role?: string;
  roles?: string[];
  accountType: AccountType;
  dealerId?: string;
  dealer?: {
    subscription?: {
      status: string;
      plan?: string;
    };
  };
  permissions?: string[];
  isVerified?: boolean;
  memberSince?: string;
}

// Vehicle Types
export interface Vehicle {
  id: string;
  brand: string;
  model: string;
  year: number;
  trim?: string;
  vin?: string;
  price: number;
  mileage: number;
  transmission: string;
  fuelType: string;
  bodyType: string;
  doors?: number;
  seats?: number;
  exteriorColor: string;
  interiorColor?: string;
  description?: string;
  images: VehicleImage[];
  primaryImage?: string;
  features?: string[];
  specs?: VehicleSpecs;
  location: Location;
  seller: Seller;
  status: string;
  condition?: string;
  isFeatured: boolean;
  isVerified: boolean;
  isCertified?: boolean;
  views: number;
  favorites: number;
  inquiries?: number;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleImage {
  id: string;
  url: string;
  isPrimary: boolean;
  order: number;
}

export interface VehicleSpecs {
  engine?: string;
  horsepower?: number;
  torque?: string;
  drivetrain?: string;
  mpgCity?: number;
  mpgHighway?: number;
  fuelCapacity?: string;
}

export interface Location {
  city: string;
  state: string;
  zipCode: string;
  latitude?: number;
  longitude?: number;
}

export interface Seller {
  id: string;
  name: string;
  avatar?: string;
  rating?: number;
  reviewCount?: number;
  memberSince: string;
  phone?: string;
  isVerified: boolean;
  isDealership: boolean;
}

export interface VehicleSearchParams extends PaginationParams {
  q?: string;
  brands?: string[];
  minPrice?: number;
  maxPrice?: number;
  minYear?: number;
  maxYear?: number;
  fuelTypes?: string[];
  transmission?: string;
  bodyTypes?: string[];
  minMileage?: number;
  maxMileage?: number;
  features?: string[];
  location?: string;
  radius?: number;
}

export interface CreateVehicleRequest {
  brand: string;
  model: string;
  year: number;
  trim?: string;
  vin?: string;
  price: number;
  mileage: number;
  transmission: string;
  fuelType: string;
  bodyType: string;
  doors?: number;
  seats?: number;
  exteriorColor: string;
  interiorColor?: string;
  description?: string;
  features?: string[];
  specs?: VehicleSpecs;
  location: Location;
  imageIds: string[];
  status?: string;
}
