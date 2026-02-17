/**
 * Dealer & Seller Types - Based on Backend DTOs
 */

// ============================================================================
// ENUMS
// ============================================================================

export const DealerType = {
  INDEPENDENT: 0,
  FRANCHISE: 1,
  MANUFACTURER: 2,
  AUCTION_HOUSE: 3,
  RENTAL_COMPANY: 4,
  FLEET_SALES: 5,
} as const;

export type DealerType = typeof DealerType[keyof typeof DealerType];

export const DealerTypeLabels: Record<number, string> = {
  0: 'Concesionario',
  1: 'Dealer Certificado',
  2: 'Fabricante',
  3: 'Casa de Subastas',
  4: 'Compañía de Alquiler',
  5: 'Ventas de Flotas',
};

export const VerificationStatus = {
  PENDING: 0,
  UNDER_REVIEW: 1,
  VERIFIED: 2,
  REJECTED: 3,
  SUSPENDED: 4,
} as const;

export type VerificationStatus = typeof VerificationStatus[keyof typeof VerificationStatus];

export const VerificationStatusLabels: Record<number, string> = {
  0: 'Pending',
  1: 'Under Review',
  2: 'Verified',
  3: 'Rejected',
  4: 'Suspended',
};

export const VerificationStatusColors: Record<number, string> = {
  0: 'bg-yellow-100 text-yellow-800',
  1: 'bg-blue-100 text-blue-800',
  2: 'bg-green-100 text-green-800',
  3: 'bg-red-100 text-red-800',
  4: 'bg-gray-100 text-gray-800',
};

// ============================================================================
// DEALER INTERFACES
// ============================================================================

export interface DealerEmployee {
  id: string;
  userId: string;
  userEmail?: string;
  userFullName?: string;
  role: string;
  status: string;
  permissions?: string;
  invitationDate?: string;
  activationDate?: string;
}

export interface DealerSubscription {
  id: string;
  planId: string;
  planName: string;
  status: string;
  startDate: string;
  endDate?: string;
  autoRenew: boolean;
  price: number;
  currency: string;
}

export interface DealerModuleSubscription {
  moduleId: string;
  moduleName: string;
  isActive: boolean;
  activatedAt?: string;
  expiresAt?: string;
}

export interface Dealer {
  id: string;
  ownerUserId: string;
  businessName: string;
  tradeName?: string;
  description?: string;
  dealerType: DealerType;
  email: string;
  phone: string;
  whatsApp?: string;
  website?: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  latitude?: number;
  longitude?: number;
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  businessRegistrationNumber?: string;
  taxId?: string;
  dealerLicenseNumber?: string;
  licenseExpiryDate?: string;
  verificationStatus: VerificationStatus;
  verifiedAt?: string;
  rejectionReason?: string;
  totalListings: number;
  activeListings: number;
  totalSales: number;
  averageRating: number;
  totalReviews: number;
  responseTimeMinutes: number;
  isActive: boolean;
  isVerified: boolean;
  rating: number;
  reviewCount: number;
  acceptsFinancing: boolean;
  acceptsTradeIn: boolean;
  offersWarranty: boolean;
  homeDelivery: boolean;
  businessHours?: string;
  socialMediaLinks?: string;
  maxListings: number;
  isFeatured: boolean;
  featuredUntil?: string;
  createdAt: string;
  updatedAt?: string;
  employees?: DealerEmployee[];
  currentSubscription?: DealerSubscription;
}

export interface CreateDealerRequest {
  ownerUserId: string;
  businessName: string;
  tradeName?: string;
  description?: string;
  dealerType: DealerType;
  email: string;
  phone: string;
  whatsApp?: string;
  website?: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  latitude?: number;
  longitude?: number;
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  businessRegistrationNumber?: string;
  taxId?: string;
  dealerLicenseNumber?: string;
  licenseExpiryDate?: string;
  acceptsFinancing?: boolean;
  acceptsTradeIn?: boolean;
  offersWarranty?: boolean;
  homeDelivery?: boolean;
  businessHours?: string;
  socialMediaLinks?: string;
}

export interface UpdateDealerRequest {
  businessName?: string;
  tradeName?: string;
  description?: string;
  dealerType?: DealerType;
  email?: string;
  phone?: string;
  whatsApp?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  logoUrl?: string;
  bannerUrl?: string;
  primaryColor?: string;
  businessRegistrationNumber?: string;
  taxId?: string;
  dealerLicenseNumber?: string;
  licenseExpiryDate?: string;
  acceptsFinancing?: boolean;
  acceptsTradeIn?: boolean;
  offersWarranty?: boolean;
  homeDelivery?: boolean;
  businessHours?: string;
  socialMediaLinks?: string;
}

export interface VerifyDealerRequest {
  isVerified: boolean;
  verifiedByUserId: string;
  notes?: string;
}

// ============================================================================
// SELLER INTERFACES
// ============================================================================

export interface IdentityDocument {
  id: string;
  documentType: string;
  documentNumber: string;
  issuingCountry: string;
  expiryDate?: string;
  documentUrl?: string;
  status: string;
  verifiedAt?: string;
  notes?: string;
}

export interface SellerProfile {
  id: string;
  userId: string;
  fullName: string;
  displayName?: string;
  dateOfBirth?: string;
  nationality?: string;
  bio?: string;
  avatarUrl?: string;
  isVerified: boolean;
  memberSince: string;
  totalSold: number;
  trustBadges?: string[];
  spokenLanguages?: string[];
  phone: string;
  alternatePhone?: string;
  whatsApp?: string;
  facebookUrl?: string;
  instagramUrl?: string;
  twitterUrl?: string;
  email: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  verificationStatus: VerificationStatus;
  verifiedAt?: string;
  rejectionReason?: string;
  totalListings: number;
  activeListings: number;
  totalSales: number;
  averageRating: number;
  totalReviews: number;
  responseTimeMinutes: number;
  isActive: boolean;
  acceptsOffers: boolean;
  showPhone: boolean;
  showLocation: boolean;
  preferredContactMethod?: string;
  maxActiveListings: number;
  canSellHighValue: boolean;
  createdAt: string;
  updatedAt?: string;
  userEmail?: string;
  userFullName?: string;
  identityDocuments?: IdentityDocument[];
}

export interface CreateSellerRequest {
  userId: string;
  fullName: string;
  displayName?: string;
  dateOfBirth?: string;
  nationality?: string;
  bio?: string;
  phone: string;
  facebookUrl?: string;
  instagramUrl?: string;
  twitterUrl?: string;
  spokenLanguages?: string[];
  specialization?: string;
  alternatePhone?: string;
  whatsApp?: string;
  email: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  acceptsOffers?: boolean;
  showPhone?: boolean;
  showLocation?: boolean;
  preferredContactMethod?: string;
}

export interface UpdateSellerRequest {
  fullName?: string;
  displayName?: string;
  dateOfBirth?: string;
  nationality?: string;
  bio?: string;
  avatarUrl?: string;
  phone?: string;
  facebookUrl?: string;
  instagramUrl?: string;
  twitterUrl?: string;
  spokenLanguages?: string[];
  specialization?: string;
  alternatePhone?: string;
  whatsApp?: string;
  email?: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  acceptsOffers?: boolean;
  showPhone?: boolean;
  showLocation?: boolean;
  preferredContactMethod?: string;
}

export interface VerifySellerRequest {
  isVerified: boolean;
  verifiedByUserId: string;
  notes?: string;
}

export interface SellerStats {
  sellerId: string;
  totalListings: number;
  activeListings: number;
  totalSales: number;
  totalSold: number;
  averageRating: number;
  totalReviews: number;
  responseTimeMinutes: number;
  avgResponseTimeMinutes: number;
  responseRate: number;
  maxActiveListings: number;
  canSellHighValue: boolean;
  memberSinceDays: number;
  totalViews30Days?: number;
  totalFavorites30Days?: number;
  totalInquiries30Days?: number;
  conversionRate?: number;
  pendingMessages?: number;
  pendingReviews?: number;
  newFollowers7Days?: number;
  estimatedMonthlyRevenue?: number;
}
