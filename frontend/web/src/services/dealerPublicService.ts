import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// ============================================
// Interfaces (Sprint 7)
// ============================================

export interface PublicDealerProfile {
  id: string;
  businessName: string;
  slogan?: string;
  description?: string;
  aboutUs?: string;
  logoUrl?: string;
  bannerUrl?: string;
  establishedDate?: string;
  slug: string;
  city: string;
  province: string;
  isTrustedDealer: boolean;
  isFoundingMember: boolean;
  trustedDealerSince?: string;
  averageRating: number;
  totalReviews: number;
  totalSales: number;
  activeListings: number;
  specialties: string[];
  supportedBrands: string[];
  contactInfo: PublicContactInfo;
  features: DealerFeature[];
  locations: PublicLocation[];
  socialMedia?: SocialMediaLinks;
  seo?: SEOMetadata;
}

export interface PublicContactInfo {
  phone?: string;
  email?: string;
  website?: string;
  whatsAppNumber?: string;
  showPhone: boolean;
  showEmail: boolean;
}

export interface DealerFeature {
  name: string;
  icon: string;
  isAvailable: boolean;
}

export interface PublicLocation {
  id: string;
  name: string;
  type: string;
  isPrimary: boolean;
  address: string;
  city: string;
  province: string;
  latitude?: number;
  longitude?: number;
  phone: string;
  email?: string;
  businessHours: BusinessHoursDto[];
  hasShowroom: boolean;
  hasServiceCenter: boolean;
  hasParking: boolean;
  parkingSpaces?: number;
  isActive: boolean;
}

export interface BusinessHoursDto {
  dayOfWeek: string;
  isOpen: boolean;
  openTime?: string;
  closeTime?: string;
  breakStartTime?: string;
  breakEndTime?: string;
  notes?: string;
  formattedHours: string;
}

export interface SocialMediaLinks {
  facebookUrl?: string;
  instagramUrl?: string;
  twitterUrl?: string;
  youTubeUrl?: string;
}

export interface SEOMetadata {
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
}

export interface UpdateProfileRequest {
  slogan?: string;
  aboutUs?: string;
  specialties?: string[];
  supportedBrands?: string[];
  logoUrl?: string;
  bannerUrl?: string;
  facebookUrl?: string;
  instagramUrl?: string;
  twitterUrl?: string;
  youTubeUrl?: string;
  whatsAppNumber?: string;
  showPhoneOnProfile?: boolean;
  showEmailOnProfile?: boolean;
  acceptsTradeIns?: boolean;
  offersFinancing?: boolean;
  offersWarranty?: boolean;
  offersHomeDelivery?: boolean;
  metaTitle?: string;
  metaDescription?: string;
  metaKeywords?: string;
}

export interface ProfileCompletion {
  completionPercentage: number;
  missingFields: string[];
  completedSections: string[];
}

// ============================================
// Service Class
// ============================================

class DealerPublicService {
  private baseUrl = `${API_URL}/api/dealers`;

  /**
   * Get public dealer profile by slug
   */
  async getPublicProfile(slug: string): Promise<PublicDealerProfile> {
    const response = await axios.get<PublicDealerProfile>(`${this.baseUrl}/public/${slug}`);
    return response.data;
  }

  /**
   * Get all trusted dealers
   */
  async getTrustedDealers(): Promise<PublicDealerProfile[]> {
    const response = await axios.get<PublicDealerProfile[]>(`${this.baseUrl}/trusted`);
    return response.data;
  }

  /**
   * Update dealer profile (authenticated)
   */
  async updateProfile(
    dealerId: string,
    request: UpdateProfileRequest
  ): Promise<PublicDealerProfile> {
    const response = await axios.put<PublicDealerProfile>(
      `${this.baseUrl}/${dealerId}/profile`,
      request
    );
    return response.data;
  }

  /**
   * Get profile completion percentage
   */
  async getProfileCompletion(dealerId: string): Promise<ProfileCompletion> {
    const response = await axios.get<ProfileCompletion>(
      `${this.baseUrl}/${dealerId}/profile/completion`
    );
    return response.data;
  }

  // ============================================
  // Helper Methods
  // ============================================

  /**
   * Format rating for display
   */
  formatRating(rating: number): string {
    return rating.toFixed(1);
  }

  /**
   * Get rating stars
   */
  getRatingStars(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return '★'.repeat(fullStars) + (hasHalfStar ? '½' : '') + '☆'.repeat(emptyStars);
  }

  /**
   * Get badge color by type
   */
  getBadgeColor(type: 'trusted' | 'founding'): string {
    return type === 'trusted' ? 'bg-blue-600' : 'bg-amber-600';
  }

  /**
   * Format date for "Member since"
   */
  formatMemberSince(date?: string): string {
    if (!date) return '';

    const d = new Date(date);
    const year = d.getFullYear();
    return `Miembro desde ${year}`;
  }

  /**
   * Generate WhatsApp link
   */
  getWhatsAppLink(number: string, message?: string): string {
    const cleanNumber = number.replace(/[^0-9]/g, '');
    const encodedMessage = message ? encodeURIComponent(message) : '';
    return `https://wa.me/${cleanNumber}${encodedMessage ? `?text=${encodedMessage}` : ''}`;
  }

  /**
   * Get location map URL (Google Maps)
   */
  getMapUrl(location: PublicLocation): string {
    if (location.latitude && location.longitude) {
      return `https://www.google.com/maps?q=${location.latitude},${location.longitude}`;
    }

    const address = encodeURIComponent(
      `${location.address}, ${location.city}, ${location.province}`
    );
    return `https://www.google.com/maps/search/?api=1&query=${address}`;
  }

  /**
   * Get open/closed status for today
   */
  getOpenStatus(location: PublicLocation): 'open' | 'closed' | 'unknown' {
    const today = new Date().toLocaleDateString('en-US', { weekday: 'long' });
    const todayHours = location.businessHours.find(
      (h) => h.dayOfWeek.toLowerCase() === today.toLowerCase()
    );

    if (!todayHours) return 'unknown';
    return todayHours.isOpen ? 'open' : 'closed';
  }

  /**
   * Get today's business hours
   */
  getTodayHours(location: PublicLocation): string {
    const today = new Date().toLocaleDateString('en-US', { weekday: 'long' });
    const todayHours = location.businessHours.find(
      (h) => h.dayOfWeek.toLowerCase() === today.toLowerCase()
    );

    return todayHours?.formattedHours || 'Horario no disponible';
  }
}

export const dealerPublicService = new DealerPublicService();
