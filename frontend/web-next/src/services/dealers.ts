/**
 * Dealers Service - API client for dealer operations
 * Connects via API Gateway to DealerManagementService
 */

import { apiClient } from '@/lib/api-client';
import type {
  Dealer,
  DealerType,
  DealerStatus,
  VerificationStatus,
  DealerPlan,
  PaginatedResponse,
} from '@/types';

// ============================================================
// API TYPES
// ============================================================

export interface DealerDto {
  id: string;
  userId: string;
  businessName: string;
  legalName?: string;
  rnc?: string;
  type: DealerType;
  email: string;
  phone: string;
  mobilePhone?: string;
  website?: string;
  address: string;
  city: string;
  province: string;
  logoUrl?: string;
  bannerUrl?: string;
  status: DealerStatus;
  verificationStatus: VerificationStatus;
  currentPlan: DealerPlan;
  isSubscriptionActive: boolean;
  maxActiveListings: number;
  currentActiveListings: number;
  rating?: number;
  reviewCount?: number;
  responseRate?: number;
  avgResponseTimeMinutes?: number;
  createdAt: string;
  verifiedAt?: string;
  description?: string;
  establishedDate?: string;
  employeeCount?: number;
}

export interface DealerLocationDto {
  id: string;
  dealerId: string;
  name: string;
  address: string;
  city: string;
  province: string;
  phone?: string;
  email?: string;
  isPrimary: boolean;
  locationType: 'headquarters' | 'branch' | 'showroom' | 'serviceCenter' | 'warehouse';
  latitude?: number;
  longitude?: number;
  businessHours?: BusinessHours;
}

export interface BusinessHours {
  monday?: DayHours;
  tuesday?: DayHours;
  wednesday?: DayHours;
  thursday?: DayHours;
  friday?: DayHours;
  saturday?: DayHours;
  sunday?: DayHours;
}

export interface DayHours {
  open: string;
  close: string;
  isClosed?: boolean;
}

export interface CreateDealerRequest {
  businessName: string;
  legalName?: string;
  rnc?: string;
  type: DealerType;
  email: string;
  phone: string;
  mobilePhone?: string;
  website?: string;
  address: string;
  city: string;
  province: string;
  description?: string;
  establishedDate?: string;
  employeeCount?: number;
}

export interface UpdateDealerRequest {
  businessName?: string;
  legalName?: string;
  phone?: string;
  mobilePhone?: string;
  website?: string;
  address?: string;
  city?: string;
  province?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
}

export interface DealerSearchParams {
  page?: number;
  pageSize?: number;
  status?: DealerStatus;
  verificationStatus?: VerificationStatus;
  city?: string;
  province?: string;
  searchTerm?: string;
}

export interface DealerStatsDto {
  totalListings: number;
  activeListings: number;
  totalViews: number;
  viewsThisMonth: number;
  viewsChange: number;
  totalInquiries: number;
  inquiriesThisMonth: number;
  inquiriesChange: number;
  pendingInquiries: number;
  responseRate: number;
  avgResponseTimeMinutes: number;
  totalRevenue: number;
  revenueThisMonth: number;
  revenueChange: number;
}

// ============================================================
// TRANSFORM FUNCTIONS
// ============================================================

export function transformDealer(dto: DealerDto): Dealer {
  return {
    id: dto.id,
    userId: dto.userId,
    businessName: dto.businessName,
    legalName: dto.legalName,
    rnc: dto.rnc,
    type: dto.type,
    description: dto.description,
    establishedDate: dto.establishedDate,
    employeeCount: dto.employeeCount,
    email: dto.email,
    phone: dto.phone,
    mobilePhone: dto.mobilePhone,
    website: dto.website,
    address: dto.address,
    city: dto.city,
    province: dto.province,
    logoUrl: dto.logoUrl,
    bannerUrl: dto.bannerUrl,
    status: dto.status,
    verificationStatus: dto.verificationStatus,
    plan: dto.currentPlan,
    isSubscriptionActive: dto.isSubscriptionActive,
    maxActiveListings: dto.maxActiveListings,
    currentActiveListings: dto.currentActiveListings,
    rating: dto.rating,
    reviewCount: dto.reviewCount,
    responseRate: dto.responseRate,
    avgResponseTimeMinutes: dto.avgResponseTimeMinutes,
    responseTime: dto.avgResponseTimeMinutes
      ? formatResponseTime(dto.avgResponseTimeMinutes)
      : undefined,
    createdAt: dto.createdAt,
    verifiedAt: dto.verifiedAt,
  };
}

function formatResponseTime(minutes: number): string {
  if (minutes < 60) {
    return `${minutes} min`;
  }
  const hours = Math.floor(minutes / 60);
  if (hours < 24) {
    return `${hours}h`;
  }
  const days = Math.floor(hours / 24);
  return `${days}d`;
}

// ============================================================
// API FUNCTIONS
// ============================================================

/**
 * Get list of dealers with pagination and filters
 */
export async function getDealers(
  params: DealerSearchParams = {}
): Promise<PaginatedResponse<Dealer>> {
  const response = await apiClient.get<{
    items: DealerDto[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>('/api/dealers', { params });

  return {
    items: response.data.items.map(transformDealer),
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Get dealer by ID
 */
export async function getDealerById(id: string): Promise<Dealer> {
  const response = await apiClient.get<DealerDto>(`/api/dealers/${id}`);
  return transformDealer(response.data);
}

/**
 * Get dealer by slug (URL-friendly business name)
 */
export async function getDealerBySlug(slug: string): Promise<Dealer> {
  const response = await apiClient.get<DealerDto>(`/api/dealers/slug/${slug}`);
  return transformDealer(response.data);
}

/**
 * Get dealer by user ID (for logged-in dealer users)
 */
export async function getDealerByUserId(userId: string): Promise<Dealer | null> {
  try {
    const response = await apiClient.get<DealerDto>(`/api/dealers/user/${userId}`);
    return transformDealer(response.data);
  } catch {
    return null;
  }
}

/**
 * Get current dealer (for logged-in dealer users)
 */
export async function getCurrentDealer(): Promise<Dealer | null> {
  try {
    const response = await apiClient.get<DealerDto>('/api/dealers/me');
    return transformDealer(response.data);
  } catch {
    return null;
  }
}

/**
 * Create new dealer
 */
export async function createDealer(data: CreateDealerRequest): Promise<Dealer> {
  const response = await apiClient.post<DealerDto>('/api/dealers', data);
  return transformDealer(response.data);
}

/**
 * Update dealer profile
 */
export async function updateDealer(id: string, data: UpdateDealerRequest): Promise<Dealer> {
  const response = await apiClient.put<DealerDto>(`/api/dealers/${id}`, data);
  return transformDealer(response.data);
}

/**
 * Get dealer statistics
 */
export async function getDealerStats(dealerId: string): Promise<DealerStatsDto> {
  const response = await apiClient.get<DealerStatsDto>(`/api/dealers/${dealerId}/stats`);
  return response.data;
}

/**
 * Get dealer locations
 */
export async function getDealerLocations(dealerId: string): Promise<DealerLocationDto[]> {
  const response = await apiClient.get<DealerLocationDto[]>(`/api/dealers/${dealerId}/locations`);
  return response.data;
}

/**
 * Add dealer location
 */
export async function addDealerLocation(
  dealerId: string,
  data: Omit<DealerLocationDto, 'id' | 'dealerId'>
): Promise<DealerLocationDto> {
  const response = await apiClient.post<DealerLocationDto>(
    `/api/dealers/${dealerId}/locations`,
    data
  );
  return response.data;
}

/**
 * Update dealer location
 */
export async function updateDealerLocation(
  dealerId: string,
  locationId: string,
  data: Partial<DealerLocationDto>
): Promise<DealerLocationDto> {
  const response = await apiClient.put<DealerLocationDto>(
    `/api/dealers/${dealerId}/locations/${locationId}`,
    data
  );
  return response.data;
}

/**
 * Delete dealer location
 */
export async function deleteDealerLocation(dealerId: string, locationId: string): Promise<void> {
  await apiClient.delete(`/api/dealers/${dealerId}/locations/${locationId}`);
}

// ============================================================
// DOCUMENT TYPES & FUNCTIONS
// ============================================================

export type DocumentType =
  | 'RNC'
  | 'BusinessLicense'
  | 'IdentificationCard'
  | 'ProofOfAddress'
  | 'BankStatement'
  | 'TaxCertificate'
  | 'IncorporationDocs'
  | 'PowerOfAttorney'
  | 'PreviousSalesRecords'
  | 'InsurancePolicy'
  | 'Other';

export type DocumentVerificationStatus =
  | 'Pending'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected'
  | 'Expired';

export interface DealerDocumentDto {
  id: string;
  dealerId: string;
  type: DocumentType;
  fileName: string;
  fileUrl: string;
  fileSizeBytes: number;
  mimeType: string;
  verificationStatus: DocumentVerificationStatus;
  verifiedAt?: string;
  rejectionReason?: string;
  expiryDate?: string;
  isExpired: boolean;
  uploadedAt: string;
}

export interface UploadDocumentRequest {
  type: DocumentType;
  file: File;
  expiryDate?: string;
}

// Document type labels in Spanish
export const DOCUMENT_TYPE_LABELS: Record<DocumentType, string> = {
  RNC: 'Registro Nacional del Contribuyente (RNC)',
  BusinessLicense: 'Licencia Comercial',
  IdentificationCard: 'Cédula del Representante Legal',
  ProofOfAddress: 'Comprobante de Domicilio',
  BankStatement: 'Certificación Bancaria',
  TaxCertificate: 'Certificación Tributaria',
  IncorporationDocs: 'Acta Constitutiva',
  PowerOfAttorney: 'Poder Notarial',
  PreviousSalesRecords: 'Récords de Ventas Anteriores',
  InsurancePolicy: 'Póliza de Seguro',
  Other: 'Otro Documento',
};

// Required documents for verification
export const REQUIRED_DOCUMENT_TYPES: DocumentType[] = [
  'RNC',
  'BusinessLicense',
  'IdentificationCard',
  'ProofOfAddress',
];

// Optional documents
export const OPTIONAL_DOCUMENT_TYPES: DocumentType[] = [
  'BankStatement',
  'TaxCertificate',
  'IncorporationDocs',
  'InsurancePolicy',
];

/**
 * Get dealer documents
 */
export async function getDealerDocuments(dealerId: string): Promise<DealerDocumentDto[]> {
  const response = await apiClient.get<DealerDocumentDto[]>(`/api/dealers/${dealerId}/documents`);
  return response.data;
}

/**
 * Upload dealer document
 */
export async function uploadDealerDocument(
  dealerId: string,
  request: UploadDocumentRequest
): Promise<DealerDocumentDto> {
  const formData = new FormData();
  formData.append('file', request.file);
  formData.append('type', request.type);
  if (request.expiryDate) {
    formData.append('expiryDate', request.expiryDate);
  }

  const response = await apiClient.post<DealerDocumentDto>(
    `/api/dealers/${dealerId}/documents`,
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  );
  return response.data;
}

/**
 * Delete dealer document
 */
export async function deleteDealerDocument(dealerId: string, documentId: string): Promise<void> {
  await apiClient.delete(`/api/dealers/${dealerId}/documents/${documentId}`);
}

/**
 * Get document download URL
 */
export async function getDocumentDownloadUrl(
  dealerId: string,
  documentId: string
): Promise<string> {
  const response = await apiClient.get<{ url: string }>(
    `/api/dealers/${dealerId}/documents/${documentId}/download`
  );
  return response.data.url;
}

// ============================================================
// SUBSCRIPTION / PLAN FUNCTIONS
// ============================================================

export interface PlanInfo {
  plan: DealerPlan;
  name: string;
  price: number;
  maxListings: number;
  features: string[];
  isPopular?: boolean;
}

export const DEALER_PLANS: PlanInfo[] = [
  {
    plan: 'starter',
    name: 'Starter',
    price: 49,
    maxListings: 15,
    features: [
      'Hasta 15 vehículos',
      'Dashboard básico',
      'Soporte por email',
      'Badge de verificación',
      'Perfil de dealer',
      'Estadísticas básicas',
    ],
  },
  {
    plan: 'pro',
    name: 'Pro',
    price: 129,
    maxListings: 50,
    isPopular: true,
    features: [
      'Hasta 50 vehículos',
      'Dashboard avanzado',
      'Soporte prioritario',
      'Badge de verificación',
      'Perfil destacado',
      'Estadísticas avanzadas',
      'CRM integrado',
      'Importación CSV',
    ],
  },
  {
    plan: 'enterprise',
    name: 'Enterprise',
    price: 299,
    maxListings: -1, // unlimited
    features: [
      'Vehículos ilimitados',
      'Dashboard premium',
      'Soporte 24/7',
      'Badge premium',
      'Perfil destacado',
      'Analytics completo',
      'CRM avanzado',
      'API access',
      'Múltiples ubicaciones',
    ],
  },
];

/**
 * Get plan info
 */
export function getPlanInfo(plan: DealerPlan): PlanInfo | undefined {
  return DEALER_PLANS.find(p => p.plan === plan);
}

/**
 * Check if early bird offer is active (until Jan 31, 2026)
 */
export function isEarlyBirdActive(): boolean {
  const deadline = new Date('2026-01-31T23:59:59');
  return new Date() < deadline;
}

/**
 * Calculate early bird price (20% discount)
 */
export function getEarlyBirdPrice(regularPrice: number): number {
  return Math.round(regularPrice * 0.8);
}

/**
 * Get days remaining for early bird
 */
export function getEarlyBirdDaysRemaining(): number {
  const deadline = new Date('2026-01-31T23:59:59');
  const now = new Date();
  const diff = deadline.getTime() - now.getTime();
  return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const dealerService = {
  getDealers,
  getDealerById,
  getDealerBySlug,
  getDealerByUserId,
  getCurrentDealer,
  createDealer,
  updateDealer,
  getDealerStats,
  getDealerLocations,
  addDealerLocation,
  updateDealerLocation,
  deleteDealerLocation,
  getDealerDocuments,
  uploadDealerDocument,
  deleteDealerDocument,
  getDocumentDownloadUrl,
  getPlanInfo,
  isEarlyBirdActive,
  getEarlyBirdPrice,
  getEarlyBirdDaysRemaining,
  DEALER_PLANS,
  DOCUMENT_TYPE_LABELS,
  REQUIRED_DOCUMENT_TYPES,
  OPTIONAL_DOCUMENT_TYPES,
};

export default dealerService;
