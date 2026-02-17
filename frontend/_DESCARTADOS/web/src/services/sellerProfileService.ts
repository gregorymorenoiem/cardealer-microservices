/**
 * Seller Profile Service
 * Servicio para gestión de perfiles de vendedores
 * Implementa procesos SELLER-001 a SELLER-005
 */

import api from './api';

// ============================================
// TYPES & ENUMS
// ============================================

export enum SellerType {
  Individual = 0,
  Dealer = 1,
  PremiumDealer = 2,
}

export enum SellerVerificationStatus {
  NotSubmitted = 0,
  PendingReview = 1,
  InReview = 2,
  Verified = 3,
  Rejected = 4,
  Expired = 5,
  Suspended = 6,
}

export enum SellerBadge {
  Verified = 0,
  TopSeller = 1,
  FastResponder = 2,
  TrustedSeller = 3,
  FounderMember = 4,
  SuperHost = 5,
  PowerSeller = 6,
  NewSeller = 7,
}

// ============================================
// INTERFACES
// ============================================

export interface SellerPublicStats {
  totalListings: number;
  activeListings: number;
  soldCount: number;
  averageRating: number;
  reviewCount: number;
  responseTime: string;
  responseRate: number;
}

export interface SellerDealerInfo {
  businessName?: string;
  website?: string;
  isKYCVerified: boolean;
}

export interface SellerPublicProfile {
  id: string;
  userId: string;
  displayName: string;
  type: SellerType;
  bio?: string;
  profilePhotoUrl?: string;
  coverPhotoUrl?: string;
  city: string;
  province: string;
  memberSince: string;
  isVerified: boolean;
  badges: string[];
  stats: SellerPublicStats;
  dealer?: SellerDealerInfo;
}

export interface SellerBadgeInfo {
  name: string;
  icon: string;
  description: string;
  earnedAt: string;
  expiresAt?: string;
}

export interface ContactPreferences {
  id: string;
  sellerId: string;
  allowPhoneCalls: boolean;
  allowWhatsApp: boolean;
  allowEmail: boolean;
  allowInAppChat: boolean;
  contactHoursStart: string;
  contactHoursEnd: string;
  contactDays: string[];
  showPhoneNumber: boolean;
  showWhatsAppNumber: boolean;
  showEmail: boolean;
  preferredContactMethod: string;
  autoReplyEnabled: boolean;
  autoReplyMessage?: string;
  awayMessage?: string;
  requireVerifiedBuyers: boolean;
  blockAnonymousContacts: boolean;
  blockNewAccounts: boolean;
}

export interface SellerListing {
  id: string;
  title: string;
  slug: string;
  price: number;
  currency: string;
  status: string;
  mainImageUrl?: string;
  year: number;
  make: string;
  model: string;
  mileage: number;
  transmission?: string;
  fuelType?: string;
  views: number;
  favorites: number;
  createdAt: string;
}

export interface SellerListingsResponse {
  listings: SellerListing[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SellerReview {
  id: string;
  reviewerId: string;
  reviewerName: string;
  reviewerPhotoUrl?: string;
  rating: number;
  comment?: string;
  createdAt: string;
  vehicleTitle?: string;
  isVerifiedPurchase: boolean;
  reply?: {
    content: string;
    repliedAt: string;
  };
}

export interface SellerReviewsResponse {
  reviews: SellerReview[];
  averageRating: number;
  totalCount: number;
  ratingDistribution: Record<number, number>;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SellerMyStats {
  sellerId: string;
  totalListings: number;
  activeListings: number;
  pendingListings: number;
  soldListings: number;
  soldCount: number;
  expiredListings: number;
  totalViews: number;
  viewsThisMonth: number;
  viewsChange: number;
  totalInquiries: number;
  inquiriesThisMonth: number;
  leadsThisMonth: number;
  leadsChange: number;
  unrespondedInquiries: number;
  pendingResponses: number;
  totalValue: number;
  averagePrice: number;
  averageRating: number;
  reviewCount: number;
  responseTimeMinutes: number;
  averageResponseTime: string;
  responseRate: number;
  conversionRate: number;
  maxActiveListings: number;
  remainingListings: number;
  canSellHighValue: boolean;
  badges: SellerBadgeInfo[];
  verificationStatus: SellerVerificationStatus;
}

export interface SellerProfile {
  id: string;
  userId: string;
  fullName: string;
  displayName?: string;
  sellerType: SellerType;
  dateOfBirth?: string;
  nationality?: string;
  bio?: string;
  avatarUrl?: string;
  profilePhotoUrl?: string;
  coverPhotoUrl?: string;
  phone: string;
  alternatePhone?: string;
  whatsApp?: string;
  email: string;
  address: string;
  city: string;
  state: string;
  province?: string;
  zipCode?: string;
  country: string;
  latitude?: number;
  longitude?: number;
  verificationStatus: SellerVerificationStatus;
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
  badges?: SellerBadgeInfo[];
  contactPreferences?: ContactPreferences;
  createdAt: string;
  updatedAt?: string;
}

export interface SellerProfileSummary {
  id: string;
  userId: string;
  fullName: string;
  avatarUrl?: string;
  city: string;
  state: string;
  verificationStatus: SellerVerificationStatus;
  averageRating: number;
  totalReviews: number;
  activeListings: number;
  totalSales: number;
  createdAt: string;
  responseTimeMinutes: number;
}

export interface PaginatedSellersResponse {
  sellers: SellerProfileSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ============================================
// REQUEST INTERFACES
// ============================================

export interface UpdateMyProfileRequest {
  displayName?: string;
  bio?: string;
  city?: string;
  province?: string;
  phone?: string;
  whatsApp?: string;
  website?: string;
  acceptsOffers?: boolean;
  showPhone?: boolean;
  showLocation?: boolean;
}

export interface UpdateContactPreferencesRequest {
  allowPhoneCalls?: boolean;
  allowWhatsApp?: boolean;
  allowEmail?: boolean;
  allowInAppChat?: boolean;
  contactHoursStart?: string;
  contactHoursEnd?: string;
  contactDays?: string[];
  showPhoneNumber?: boolean;
  showWhatsAppNumber?: boolean;
  showEmail?: boolean;
  preferredContactMethod?: string;
  autoReplyEnabled?: boolean;
  autoReplyMessage?: string;
  awayMessage?: string;
  requireVerifiedBuyers?: boolean;
  blockAnonymousContacts?: boolean;
  blockNewAccounts?: boolean;
}

export interface CreateSellerProfileRequest {
  fullName: string;
  dateOfBirth?: string;
  nationality?: string;
  phone: string;
  alternatePhone?: string;
  whatsApp?: string;
  email: string;
  address: string;
  city: string;
  state: string;
  zipCode?: string;
  country?: string;
  latitude?: number;
  longitude?: number;
  acceptsOffers?: boolean;
  showPhone?: boolean;
  showLocation?: boolean;
  preferredContactMethod?: string;
}

export interface UpdateProfilePhotoRequest {
  photoUrl?: string;
  photoType?: 'profile' | 'cover';
  isCoverPhoto?: boolean;
  file?: File;
}

export interface AssignBadgeRequest {
  sellerProfileId: string;
  badge: SellerBadge;
  expiresAt?: string;
  reason?: string;
}

// ============================================
// BADGE METADATA
// ============================================

export const BADGE_INFO: Record<string, { icon: string; description: string; color: string }> = {
  Verified: {
    icon: '✓',
    description: 'Identidad verificada por OKLA',
    color: 'blue',
  },
  TopSeller: {
    icon: '⭐',
    description: 'Top 10 vendedores del mes',
    color: 'yellow',
  },
  FastResponder: {
    icon: '⚡',
    description: 'Responde en menos de 1 hora',
    color: 'purple',
  },
  TrustedSeller: {
    icon: '🛡️',
    description: 'Más de 10 ventas con rating 4.5+',
    color: 'green',
  },
  FounderMember: {
    icon: '🏆',
    description: 'Miembro fundador de OKLA',
    color: 'amber',
  },
  SuperHost: {
    icon: '🌟',
    description: 'Rating 5.0 con más de 20 reseñas',
    color: 'pink',
  },
  PowerSeller: {
    icon: '💪',
    description: 'Más de 50 ventas totales',
    color: 'red',
  },
  NewSeller: {
    icon: '🆕',
    description: 'Nuevo vendedor en OKLA',
    color: 'cyan',
  },
};

// ============================================
// API SERVICE CLASS
// ============================================

class SellerProfileService {
  private baseUrl = '/api/sellers';

  // ========================================
  // PUBLIC ENDPOINTS
  // ========================================

  /**
   * SELLER-001: Obtener perfil público de vendedor
   */
  async getPublicProfile(sellerId: string): Promise<SellerPublicProfile> {
    const response = await api.get<SellerPublicProfile>(`${this.baseUrl}/${sellerId}/profile`);
    return response.data;
  }

  /**
   * SELLER-001: Obtener listados del vendedor
   */
  async getSellerListings(
    sellerId: string,
    page = 1,
    pageSize = 12,
    status?: string
  ): Promise<SellerListingsResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (status) params.append('status', status);

    const response = await api.get<SellerListingsResponse>(
      `${this.baseUrl}/${sellerId}/listings?${params}`
    );
    return response.data;
  }

  /**
   * SELLER-001: Obtener reseñas del vendedor
   */
  async getSellerReviews(
    sellerId: string,
    page = 1,
    pageSize = 10,
    rating?: number
  ): Promise<SellerReviewsResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    if (rating) params.append('rating', rating.toString());

    const response = await api.get<SellerReviewsResponse>(
      `${this.baseUrl}/${sellerId}/reviews?${params}`
    );
    return response.data;
  }

  /**
   * SELLER-001: Obtener estadísticas públicas del vendedor
   */
  async getSellerStats(sellerId: string): Promise<SellerPublicStats> {
    const response = await api.get<SellerPublicStats>(`${this.baseUrl}/${sellerId}/stats`);
    return response.data;
  }

  /**
   * SELLER-003: Obtener preferencias de contacto del vendedor
   */
  async getSellerContactPreferences(sellerId: string): Promise<ContactPreferences> {
    const response = await api.get<ContactPreferences>(
      `${this.baseUrl}/${sellerId}/contact-preferences`
    );
    return response.data;
  }

  /**
   * Buscar vendedores
   */
  async searchSellers(params: {
    q?: string;
    city?: string;
    type?: SellerType;
    page?: number;
    pageSize?: number;
  }): Promise<PaginatedSellersResponse> {
    const urlParams = new URLSearchParams();
    if (params.q) urlParams.append('q', params.q);
    if (params.city) urlParams.append('city', params.city);
    if (params.type !== undefined) urlParams.append('type', params.type.toString());
    if (params.page) urlParams.append('page', params.page.toString());
    if (params.pageSize) urlParams.append('pageSize', params.pageSize.toString());

    const response = await api.get<PaginatedSellersResponse>(`${this.baseUrl}/search?${urlParams}`);
    return response.data;
  }

  /**
   * Obtener top vendedores
   */
  async getTopSellers(count = 10, city?: string): Promise<SellerProfileSummary[]> {
    const params = new URLSearchParams({ count: count.toString() });
    if (city) params.append('city', city);

    const response = await api.get<SellerProfileSummary[]>(`${this.baseUrl}/top?${params}`);
    return response.data;
  }

  // ========================================
  // AUTHENTICATED ENDPOINTS (MY PROFILE)
  // ========================================

  /**
   * SELLER-002: Obtener mi perfil de vendedor
   */
  async getMyProfile(): Promise<SellerProfile> {
    const response = await api.get<SellerProfile>(`${this.baseUrl}/profile`);
    return response.data;
  }

  /**
   * SELLER-002: Actualizar mi perfil
   */
  async updateMyProfile(data: UpdateMyProfileRequest): Promise<SellerProfile> {
    const response = await api.put<SellerProfile>(`${this.baseUrl}/profile`, data);
    return response.data;
  }

  /**
   * SELLER-002: Subir foto de perfil
   */
  async updateProfilePhoto(data: UpdateProfilePhotoRequest): Promise<{ photoUrl: string }> {
    const response = await api.put<{ message: string; photoUrl: string }>(
      `${this.baseUrl}/profile/photo`,
      data
    );
    return { photoUrl: response.data.photoUrl };
  }

  /**
   * SELLER-003: Actualizar preferencias de contacto
   */
  async updateContactPreferences(
    data: UpdateContactPreferencesRequest
  ): Promise<ContactPreferences> {
    const response = await api.put<ContactPreferences>(`${this.baseUrl}/contact-preferences`, data);
    return response.data;
  }

  /**
   * SELLER-005: Obtener mis estadísticas
   */
  async getMyStats(): Promise<SellerMyStats> {
    const response = await api.get<SellerMyStats>(`${this.baseUrl}/my-stats`);
    return response.data;
  }

  /**
   * Crear perfil de vendedor
   */
  async createProfile(data: CreateSellerProfileRequest): Promise<SellerProfile> {
    const response = await api.post<SellerProfile>(`${this.baseUrl}/profile`, data);
    return response.data;
  }

  // ========================================
  // ADMIN ENDPOINTS
  // ========================================

  /**
   * SELLER-004: Asignar badge a vendedor
   */
  async assignBadge(
    sellerId: string,
    data: Omit<AssignBadgeRequest, 'sellerProfileId'>
  ): Promise<{ badge: SellerBadgeInfo }> {
    const response = await api.post<{ message: string; badge: SellerBadgeInfo }>(
      `${this.baseUrl}/${sellerId}/badges`,
      { ...data, sellerProfileId: sellerId }
    );
    return { badge: response.data.badge };
  }

  /**
   * SELLER-004: Quitar badge de vendedor
   */
  async removeBadge(sellerId: string, badge: SellerBadge): Promise<void> {
    await api.delete(`${this.baseUrl}/${sellerId}/badges/${badge}`);
  }

  /**
   * Verificar vendedor
   */
  async verifySeller(sellerId: string, verifiedByUserId: string, notes?: string): Promise<void> {
    await api.post(`${this.baseUrl}/${sellerId}/verify`, {
      isVerified: true,
      verifiedByUserId,
      notes,
    });
  }

  /**
   * Obtener vendedores pendientes de verificación
   */
  async getPendingVerifications(page = 1, pageSize = 20): Promise<SellerProfile[]> {
    const response = await api.get<SellerProfile[]>(
      `${this.baseUrl}/pending-verifications?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  }

  // ========================================
  // HELPER METHODS
  // ========================================

  /**
   * Obtener información del badge
   */
  getBadgeInfo(badgeName: string): { icon: string; description: string; color: string } {
    return (
      BADGE_INFO[badgeName] || {
        icon: '🏷️',
        description: 'Badge desconocido',
        color: 'gray',
      }
    );
  }

  /**
   * Formatear tiempo de respuesta
   */
  formatResponseTime(minutes: number): string {
    if (minutes <= 0) return 'N/A';
    if (minutes < 60) return `${minutes} min`;
    if (minutes < 1440) return `${Math.floor(minutes / 60)}h`;
    return `${Math.floor(minutes / 1440)}d`;
  }

  /**
   * Obtener color del estado de verificación
   */
  getVerificationStatusColor(status: SellerVerificationStatus): string {
    switch (status) {
      case SellerVerificationStatus.Verified:
        return 'green';
      case SellerVerificationStatus.PendingReview:
      case SellerVerificationStatus.InReview:
        return 'yellow';
      case SellerVerificationStatus.Rejected:
      case SellerVerificationStatus.Suspended:
        return 'red';
      case SellerVerificationStatus.Expired:
        return 'orange';
      default:
        return 'gray';
    }
  }

  /**
   * Obtener texto del estado de verificación
   */
  getVerificationStatusText(status: SellerVerificationStatus): string {
    switch (status) {
      case SellerVerificationStatus.NotSubmitted:
        return 'Sin verificar';
      case SellerVerificationStatus.PendingReview:
        return 'Pendiente de revisión';
      case SellerVerificationStatus.InReview:
        return 'En revisión';
      case SellerVerificationStatus.Verified:
        return 'Verificado';
      case SellerVerificationStatus.Rejected:
        return 'Rechazado';
      case SellerVerificationStatus.Expired:
        return 'Expirado';
      case SellerVerificationStatus.Suspended:
        return 'Suspendido';
      default:
        return 'Desconocido';
    }
  }
}

export const sellerProfileService = new SellerProfileService();
export default sellerProfileService;
