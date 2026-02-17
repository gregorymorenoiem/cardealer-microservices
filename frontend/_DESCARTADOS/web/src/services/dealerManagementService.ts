import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// Types for Dealer Management
export interface Dealer {
  id: string;
  userId: string;
  businessName: string;
  rnc: string;
  legalName?: string;
  tradeName?: string;
  type: DealerType;
  status: DealerStatus;
  verificationStatus: VerificationStatus;
  email: string;
  phone: string;
  mobilePhone?: string;
  website?: string;
  address: string;
  city: string;
  province: string;
  zipCode?: string;
  country: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  establishedDate?: string;
  employeeCount?: number;
  currentPlan: DealerPlan;
  subscriptionStartDate?: string;
  subscriptionEndDate?: string;
  isSubscriptionActive: boolean;
  maxActiveListings: number;
  currentActiveListings: number;
  remainingListings: number;
  createdAt: string;
  updatedAt?: string;
  verifiedAt?: string;
}

export type DealerType = 'Independent' | 'Chain' | 'MultipleStore' | 'Franchise';
export type DealerStatus =
  | 'Pending'
  | 'UnderReview'
  | 'Active'
  | 'Suspended'
  | 'Rejected'
  | 'Inactive';
export type VerificationStatus =
  | 'NotVerified'
  | 'DocumentsUploaded'
  | 'UnderReview'
  | 'Verified'
  | 'Rejected'
  | 'RequiresMoreInfo';
export type DealerPlan = 'Free' | 'Basic' | 'Pro' | 'Enterprise';

// Subscription Types
export interface DealerSubscription {
  dealerId: string;
  plan: DealerPlan;
  planDisplayName: string;
  monthlyPrice: number;
  isActive: boolean;
  startDate?: string;
  endDate?: string;
  daysRemaining: number;
  features: DealerPlanFeatures;
  usage: DealerUsage;
  limits: DealerLimits;
  canUpgrade: boolean;
  nextPlan?: string;
}

export interface DealerPlanFeatures {
  maxListings: number;
  maxImages: number;
  maxFeaturedListings: number;
  analyticsAccess: boolean;
  bulkUpload: boolean;
  prioritySupport: boolean;
  customBranding: boolean;
  apiAccess: boolean;
  leadManagement: boolean;
  emailAutomation: boolean;
  marketPriceAnalysis: boolean;
  advancedReporting: boolean;
  whiteLabel: boolean;
  dedicatedAccountManager: boolean;
}

export interface DealerUsage {
  currentListings: number;
  featuredListings: number;
  imagesUsed: number;
  leadsThisMonth: number;
  emailsSentThisMonth: number;
}

export interface DealerLimits {
  maxListings: number;
  maxFeaturedListings: number;
  maxImages: number;
  maxLeadsPerMonth: number;
  maxEmailsPerMonth: number;
  hasReachedListingLimit: boolean;
  hasReachedFeaturedLimit: boolean;
  remainingListings: number;
  remainingFeatured: number;
  listingsUsagePercent: number;
  featuredUsagePercent: number;
}

export interface PlanInfo {
  plan: string;
  displayName: string;
  monthlyPrice: number;
  annualPrice?: number;
  description: string;
  features: string[];
  notIncluded: string[];
  isPopular: boolean;
  maxListings: number;
  maxImages: number;
  maxFeaturedListings: number;
}

export interface AllPlansResponse {
  plans: PlanInfo[];
  currentPlan: string;
  recommendedPlan?: string;
}

export interface CreateDealerRequest {
  userId: string;
  businessName: string;
  rnc: string;
  legalName?: string;
  tradeName?: string;
  type: string;
  email: string;
  phone: string;
  mobilePhone?: string;
  website?: string;
  address: string;
  city: string;
  province: string;
  zipCode?: string;
  description?: string;
  establishedDate?: string;
  employeeCount?: number;
}

export interface UpdateDealerRequest {
  businessName?: string;
  legalName?: string;
  tradeName?: string;
  email?: string;
  phone?: string;
  mobilePhone?: string;
  website?: string;
  address?: string;
  city?: string;
  province?: string;
  zipCode?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  establishedDate?: string;
  employeeCount?: number;
}

export interface DealerListResponse {
  dealers: Dealer[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DealerPlanInfo {
  name: DealerPlan;
  displayName: string;
  price: number;
  earlyBirdPrice: number;
  maxListings: number | string;
  features: string[];
  recommended?: boolean;
}

class DealerManagementService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: `${API_URL}/api`,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token interceptor
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('accessToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add refresh token interceptor for automatic token renewal
    addRefreshTokenInterceptor(this.api);
  }

  // Dealer CRUD
  async getDealers(
    page: number = 1,
    pageSize: number = 20,
    status?: DealerStatus,
    verificationStatus?: VerificationStatus,
    searchTerm?: string
  ): Promise<DealerListResponse> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (status) params.append('status', status);
    if (verificationStatus) params.append('verificationStatus', verificationStatus);
    if (searchTerm) params.append('searchTerm', searchTerm);

    const response = await this.api.get<DealerListResponse>(`/dealers?${params.toString()}`);
    return response.data;
  }

  async getDealerById(id: string): Promise<Dealer> {
    const response = await this.api.get<Dealer>(`/dealers/${id}`);
    return response.data;
  }

  async getDealerByUserId(userId: string): Promise<Dealer> {
    const response = await this.api.get<Dealer>(`/dealers/user/${userId}`);
    return response.data;
  }

  async createDealer(request: CreateDealerRequest): Promise<Dealer> {
    const response = await this.api.post<Dealer>('/dealers', request);
    return response.data;
  }

  async updateDealer(id: string, request: UpdateDealerRequest): Promise<Dealer> {
    const response = await this.api.put<Dealer>(`/dealers/${id}`, request);
    return response.data;
  }

  async verifyDealer(id: string, approved: boolean, rejectionReason?: string): Promise<void> {
    await this.api.post(`/dealers/${id}/verify`, { approved, rejectionReason });
  }

  // ========================================
  // Subscription Management
  // ========================================

  /**
   * Get all available plans with features and pricing
   */
  async getPlans(): Promise<AllPlansResponse> {
    const response = await this.api.get<AllPlansResponse>('/subscriptions/plans');
    return response.data;
  }

  /**
   * Get dealer subscription with limits and usage
   */
  async getDealerSubscription(dealerId: string): Promise<DealerSubscription> {
    const response = await this.api.get<DealerSubscription>(`/subscriptions/dealer/${dealerId}`);
    return response.data;
  }

  /**
   * Get subscription by user ID
   */
  async getSubscriptionByUserId(userId: string): Promise<DealerSubscription> {
    const response = await this.api.get<DealerSubscription>(`/subscriptions/user/${userId}`);
    return response.data;
  }

  /**
   * Change dealer plan (upgrade/downgrade)
   */
  async changePlan(
    dealerId: string,
    newPlan: DealerPlan
  ): Promise<{ success: boolean; message: string; subscription?: DealerSubscription }> {
    const response = await this.api.post(`/subscriptions/dealer/${dealerId}/change-plan`, {
      newPlan,
    });
    return response.data;
  }

  /**
   * Check if dealer can perform an action based on limits
   */
  async canPerformAction(
    dealerId: string,
    action: 'add_listing' | 'add_featured' | 'bulk_upload' | 'analytics'
  ): Promise<{
    allowed: boolean;
    reason?: string;
    current: number;
    limit: number;
    remaining: number;
  }> {
    const response = await this.api.get(
      `/subscriptions/dealer/${dealerId}/can-action?action=${action}`
    );
    return response.data;
  }

  /**
   * Increment listings count (call after publishing a vehicle)
   */
  async incrementListings(
    dealerId: string
  ): Promise<{ currentListings: number; maxListings: number; remaining: number }> {
    const response = await this.api.post(`/subscriptions/dealer/${dealerId}/increment-listings`);
    return response.data;
  }

  /**
   * Decrement listings count (call after deleting/unpublishing a vehicle)
   */
  async decrementListings(
    dealerId: string
  ): Promise<{ currentListings: number; maxListings: number; remaining: number }> {
    const response = await this.api.post(`/subscriptions/dealer/${dealerId}/decrement-listings`);
    return response.data;
  }

  // Plan information (static)
  // Precios en Pesos Dominicanos (DOP)
  getPlanInfo(): DealerPlanInfo[] {
    const isEarlyBird = this.isEarlyBirdActive();

    return [
      {
        name: 'Starter',
        displayName: 'Dealer Starter',
        price: 2900, // RD$2,900/mes
        earlyBirdPrice: 2320, // RD$2,320/mes (-20%)
        maxListings: 10,
        features: [
          'Hasta 10 vehículos activos',
          'Galería de fotos (hasta 20 por vehículo)',
          'Panel de control básico',
          'Estadísticas de vistas',
          'Formulario de contacto',
          'Soporte por email',
        ],
      },
      {
        name: 'Professional',
        displayName: 'Dealer Professional',
        price: 5900, // RD$5,900/mes
        earlyBirdPrice: 4720, // RD$4,720/mes (-20%)
        maxListings: 50,
        features: [
          'Hasta 50 vehículos activos',
          'Galería de fotos ilimitadas',
          'Panel de control avanzado',
          'Estadísticas detalladas',
          'Import masivo (CSV/Excel)',
          'Badge "Dealer Verificado" ✓',
          'Prioridad en búsquedas',
          'Soporte prioritario',
        ],
        recommended: true,
      },
      {
        name: 'Enterprise',
        displayName: 'Dealer Enterprise',
        price: 14900, // RD$14,900/mes
        earlyBirdPrice: 11920, // RD$11,920/mes (-20%)
        maxListings: 'Ilimitado',
        features: [
          '✨ Vehículos ILIMITADOS',
          'Múltiples sucursales',
          'API de integración',
          'Analytics avanzado con IA',
          'Gerente de cuenta dedicado',
          'Soporte prioritario 24/7',
          'Branding personalizado',
          'Badge "Premium Dealer"',
          'Leads prioritarios',
        ],
      },
    ];
  }

  calculateEarlyBirdPrice(regularPrice: number): number {
    return Math.round(regularPrice * 0.8); // 20% discount
  }

  isEarlyBirdActive(): boolean {
    const deadline = new Date('2026-01-31T23:59:59');
    return new Date() < deadline;
  }

  getEarlyBirdDaysRemaining(): number {
    const deadline = new Date('2026-01-31T23:59:59');
    const now = new Date();
    const diff = deadline.getTime() - now.getTime();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }
}

export const dealerManagementService = new DealerManagementService();
