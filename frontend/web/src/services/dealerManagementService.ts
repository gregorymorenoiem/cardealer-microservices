import axios from 'axios';
import type { AxiosInstance } from 'axios';

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
export type DealerPlan = 'None' | 'Starter' | 'Pro' | 'Enterprise';

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
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
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

  // Plan information (static)
  getPlanInfo(): DealerPlanInfo[] {
    const isEarlyBird = this.isEarlyBirdActive();

    return [
      {
        name: 'Starter',
        displayName: 'Dealer Starter',
        price: 49,
        earlyBirdPrice: 39,
        maxListings: 15,
        features: [
          'Hasta 15 vehículos activos',
          'Galería de fotos (hasta 20 por vehículo)',
          'Panel de control básico',
          'Estadísticas de vistas',
          'Formulario de contacto',
          'Badge "Dealer Verificado"',
        ],
      },
      {
        name: 'Pro',
        displayName: 'Dealer Pro',
        price: 129,
        earlyBirdPrice: 103,
        maxListings: 50,
        features: [
          'Hasta 50 vehículos activos',
          'Galería de fotos ilimitadas',
          'Panel de control avanzado',
          'Estadísticas detalladas',
          'Import masivo (CSV/Excel)',
          'Múltiples sucursales',
          'Prioridad en búsquedas',
          'Badge "Dealer Pro"',
        ],
        recommended: true,
      },
      {
        name: 'Enterprise',
        displayName: 'Dealer Enterprise',
        price: 299,
        earlyBirdPrice: 239,
        maxListings: 'Ilimitado',
        features: [
          '✨ Vehículos ILIMITADOS',
          'Galería de fotos ilimitadas',
          'Panel ejecutivo completo',
          'Analytics avanzado con IA',
          'API de integración',
          'Soporte prioritario 24/7',
          'Gerente de cuenta dedicado',
          'Branding personalizado',
          'Badge "Premium Dealer"',
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
