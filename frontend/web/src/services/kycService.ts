import api from './api';
import { uploadImage } from './uploadService';

// Use api instance which has the auth interceptor configured
const axios = api;

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
// KYC endpoints go through /api/kyc/kycprofiles which maps to backend /api/kycprofiles
const KYC_API_URL = `${API_BASE_URL}/api/kyc`;

// Enums matching backend (KYCService.Domain.Entities.KYCEntities.cs)
export enum KYCStatus {
  Pending = 1,
  InProgress = 2,
  DocumentsRequired = 3,
  UnderReview = 4,
  Approved = 5,
  Rejected = 6,
  Expired = 7,
  Suspended = 8,
  // Aliases for backwards compatibility
  NotStarted = 0,
  PendingReview = 4, // Same as UnderReview
}

export enum RiskLevel {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum DocumentType {
  // Identity Documents (matching backend)
  Cedula = 1,
  Passport = 2,
  DriverLicense = 3,
  ResidencyCard = 4,
  RNC = 5,
  Other = 99,
  // Proof of Address (extended for frontend)
  UtilityBill = 10,
  BankStatement = 11,
  LeaseAgreement = 12,
  MercantileRegistry = 21,
  BusinessLicense = 22,
  TaxCertificate = 23,
  // Financial Documents
  IncomeProof = 30,
  TaxReturn = 31,
  // Selfie/Verification
  Selfie = 40,
  SelfieWithDocument = 41,
}

// Watchlist Types
export enum WatchlistType {
  PEP = 1,
  Sanctions = 2,
  Internal = 3,
  OFAC = 4,
  UN = 5,
  EU = 6,
}

// STR Status
export enum STRStatus {
  Draft = 1,
  PendingReview = 2,
  Approved = 3,
  SentToUAF = 4,
  Rejected = 5,
  Archived = 6,
}

// Watchlist Entry
export interface WatchlistEntry {
  id: string;
  fullName: string;
  listType: WatchlistType;
  position?: string;
  jurisdiction?: string;
  dateOfBirth?: string;
  documentNumber?: string;
  remarks?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

// Suspicious Transaction Report
export interface SuspiciousTransactionReport {
  id: string;
  reportNumber: string;
  userId: string;
  suspiciousActivityType: string;
  description: string;
  amount: number;
  currency: string;
  redFlags: string[];
  status: STRStatus;
  detectedAt?: string;
  reportingDeadline?: string;
  uafReportNumber?: string;
  sentToUafAt?: string;
  createdAt?: string;
  approvedAt?: string;
  approvedBy?: string;
}

export interface KYCProfile {
  id: string;
  userId: string;
  // Backend sends these fields
  fullName?: string;
  middleName?: string | null;
  lastName?: string;
  firstName?: string; // May not exist, derive from fullName
  entityType: number;
  status: number;
  statusName: string;
  riskLevel: number;
  riskLevelName: string;
  riskScore: number;
  riskFactors: string[];
  dateOfBirth: string;
  placeOfBirth?: string | null;
  nationality: string;
  primaryDocumentType: number;
  primaryDocumentNumber?: string | null;
  primaryDocumentExpiry?: string | null;
  email?: string | null;
  phone?: string | null;
  address: string;
  city: string;
  province: string;
  country?: string;
  isPEP: boolean;
  pepPosition?: string | null;
  businessName?: string | null;
  rnc?: string | null;
  isIdentityVerified: boolean;
  isAddressVerified: boolean;
  identityVerifiedAt?: string | null;
  addressVerifiedAt?: string | null;
  createdAt: string;
  approvedAt?: string | null;
  approvedBy?: string | null;
  approvedByName?: string | null; // Human-readable name
  expiresAt?: string | null;
  nextReviewAt?: string | null;
  rejectedAt?: string | null;
  rejectionReason?: string | null;
  rejectedBy?: string | null;
  rejectedByName?: string | null; // Human-readable name
  documents: KYCDocument[];
  verifications: any[];
  // Legacy/optional fields for backward compatibility
  occupation?: string;
  sourceOfFunds?: string;
  expectedMonthlyTransaction?: number;
  phoneNumber?: string; // Alias for phone
  documentNumber?: string; // Alias for primaryDocumentNumber
}

export interface KYCDocument {
  id: string;
  kycProfileId: string;
  // Backend sends 'type' as number and 'typeName' as string
  type: number;
  typeName?: string;
  documentName: string;
  fileName: string;
  fileUrl: string;
  fileType: string;
  fileSize: number;
  side?: string | null;
  status: number;
  statusName: string;
  rejectionReason?: string | null;
  extractedNumber?: string | null;
  extractedExpiry?: string | null;
  extractedName?: string | null;
  uploadedAt: string;
  verifiedAt?: string | null;
}

export interface CreateKYCProfileRequest {
  userId: string;
  firstName: string;
  lastName: string;
  documentNumber: string;
  documentType: DocumentType;
  dateOfBirth: string;
  nationality: string;
  address: string;
  city: string;
  province: string;
  phoneNumber: string;
  sourceOfFunds: string;
  occupation: string;
  expectedMonthlyTransaction?: number;
}

export interface UploadDocumentRequest {
  profileId: string;
  documentType: DocumentType;
  file: File;
  side?: 'Front' | 'Back';
}

export interface KYCProfileSummary {
  id: string;
  userId: string;
  fullName: string;
  documentNumber: string;
  status: KYCStatus;
  riskLevel: RiskLevel;
  isPEP: boolean;
  createdAt: string;
  documentsCount: number;
  pendingDocuments: number;
}

export interface PaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

class KYCService {
  private getAuthHeader() {
    const token = localStorage.getItem('accessToken');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  // Get profile by user ID
  async getProfileByUserId(userId: string): Promise<KYCProfile | null> {
    try {
      const response = await axios.get<KYCProfile>(`${KYC_API_URL}/kycprofiles/user/${userId}`, {
        headers: this.getAuthHeader(),
      });
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error)) {
        // 404 = profile not found, 401 = not authenticated yet, both are expected states
        if (error.response?.status === 404 || error.response?.status === 401) {
          return null;
        }
      }
      console.error('Error fetching KYC profile:', error);
      throw error;
    }
  }

  // Get profile by ID
  async getProfileById(id: string): Promise<KYCProfile> {
    const response = await axios.get<KYCProfile>(`${KYC_API_URL}/kycprofiles/${id}`, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Map frontend field names to backend field names
  private mapToBackendFormat(data: CreateKYCProfileRequest | Partial<CreateKYCProfileRequest>) {
    return {
      userId: data.userId,
      fullName:
        data.firstName && data.lastName
          ? `${data.firstName} ${data.lastName}`.trim()
          : data.firstName || '',
      lastName: data.lastName,
      primaryDocumentNumber: data.documentNumber,
      primaryDocumentType: data.documentType,
      dateOfBirth: data.dateOfBirth,
      nationality: data.nationality,
      address: data.address,
      city: data.city,
      province: data.province,
      country: 'DO',
      phone: data.phoneNumber,
      sourceOfFunds: data.sourceOfFunds,
      occupation: data.occupation,
      expectedTransactionVolume: data.expectedMonthlyTransaction?.toString(),
      entityType: 0, // Individual
    };
  }

  // Create new KYC profile
  async createProfile(data: CreateKYCProfileRequest): Promise<KYCProfile> {
    const mappedData = this.mapToBackendFormat(data);
    const response = await axios.post<KYCProfile>(`${KYC_API_URL}/kycprofiles`, mappedData, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Update profile
  async updateProfile(id: string, data: Partial<CreateKYCProfileRequest>): Promise<KYCProfile> {
    const mappedData = this.mapToBackendFormat(data);
    const response = await axios.put<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}`,
      { id, ...mappedData },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Upload document - Uses Upload Service for file upload, then registers in KYC
  async uploadDocument(request: UploadDocumentRequest): Promise<KYCDocument> {
    try {
      let fileUrl: string;
      let publicId: string;

      try {
        // Step 1: Try to upload file to Media/Upload Service (returns S3 URL)
        const uploadResponse = await uploadImage(request.file, 'kyc-documents');
        fileUrl = uploadResponse.url;
        publicId = uploadResponse.publicId || request.file.name;
      } catch (uploadError) {
        // If MediaService is down, use a placeholder URL
        // The actual file is stored locally in browser state (capturedImages)
        // Admin can request re-upload when MediaService is back
        console.warn('MediaService unavailable, using placeholder URL:', uploadError);

        // Use a placeholder URL that indicates the file was captured but not uploaded
        const timestamp = Date.now();
        publicId = `pending-upload-${timestamp}-${request.file.name}`;
        fileUrl = `https://pending-upload.okla.com.do/kyc/${publicId}`;
      }

      // Step 2: Register document in KYC with metadata + URL
      const payload = {
        kycProfileId: request.profileId,
        type: request.documentType,
        documentName: request.file.name,
        fileName: publicId,
        fileUrl: fileUrl,
        fileType: request.file.type,
        fileSize: request.file.size,
        uploadedBy: request.profileId,
        side: request.side,
      };

      const response = await axios.post<KYCDocument>(
        `${KYC_API_URL}/profiles/${request.profileId}/documents`,
        payload,
        {
          headers: {
            ...this.getAuthHeader(),
            'Content-Type': 'application/json',
          },
        }
      );
      return response.data;
    } catch (error) {
      console.error('Error uploading KYC document:', error);
      throw error;
    }
  }

  // Helper to convert File to base64
  private async fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  }

  // Get documents for profile
  async getDocuments(profileId: string): Promise<KYCDocument[]> {
    const response = await axios.get<KYCDocument[]>(
      `${KYC_API_URL}/profiles/${profileId}/documents`,
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Delete document
  async deleteDocument(documentId: string): Promise<void> {
    await axios.delete(`${KYC_API_URL}/documents/${documentId}`, {
      headers: this.getAuthHeader(),
    });
  }

  // Admin: Get all profiles (paginated)
  async getProfiles(params: {
    page?: number;
    pageSize?: number;
    status?: KYCStatus;
    riskLevel?: RiskLevel;
    isPEP?: boolean;
  }): Promise<PaginatedResult<KYCProfileSummary>> {
    const response = await axios.get<PaginatedResult<KYCProfileSummary>>(
      `${KYC_API_URL}/kycprofiles`,
      {
        params,
        headers: this.getAuthHeader(),
      }
    );
    return response.data;
  }

  // Admin: Get pending profiles
  async getPendingProfiles(page = 1, pageSize = 20): Promise<PaginatedResult<KYCProfileSummary>> {
    const response = await axios.get<PaginatedResult<KYCProfileSummary>>(
      `${KYC_API_URL}/kycprofiles/pending`,
      {
        params: { page, pageSize },
        headers: this.getAuthHeader(),
      }
    );
    return response.data;
  }

  // Admin: Approve profile
  async approveProfile(
    id: string,
    data: { approvedByName: string; notes?: string; validityDays?: number }
  ): Promise<KYCProfile> {
    const response = await axios.post<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}/approve`,
      { id, ...data },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Admin: Reject profile
  async rejectProfile(
    id: string,
    data: { rejectedByName: string; rejectionReason: string; canRetry?: boolean }
  ): Promise<KYCProfile> {
    const response = await axios.post<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}/reject`,
      { id, ...data },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Admin: Verify document
  async verifyDocument(
    documentId: string,
    verifiedBy: string,
    approved: boolean = true,
    rejectionReason?: string
  ): Promise<KYCDocument> {
    const response = await axios.post<KYCDocument>(
      `${KYC_API_URL}/documents/${documentId}/verify`,
      {
        id: documentId,
        verifiedBy: verifiedBy, // Must be a valid GUID
        approved: approved,
        rejectionReason: rejectionReason || null,
      },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Admin: Get statistics
  async getStatistics(): Promise<{
    totalProfiles: number;
    pendingReview: number;
    approved: number;
    rejected: number;
    expired: number;
    pepCount: number;
    avgApprovalTime: number;
  }> {
    const response = await axios.get(`${KYC_API_URL}/kycprofiles/statistics`, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Helper: Get status label
  getStatusLabel(status: KYCStatus): string {
    const labels: Record<number, string> = {
      [KYCStatus.NotStarted]: 'No Iniciado',
      [KYCStatus.Pending]: 'Pendiente',
      [KYCStatus.InProgress]: 'En Progreso',
      [KYCStatus.DocumentsRequired]: 'Documentos Requeridos',
      [KYCStatus.UnderReview]: 'En Revisión',
      [KYCStatus.PendingReview]: 'Pendiente de Revisión',
      [KYCStatus.Approved]: 'Aprobado',
      [KYCStatus.Rejected]: 'Rechazado',
      [KYCStatus.Expired]: 'Expirado',
      [KYCStatus.Suspended]: 'Suspendido',
    };
    return labels[status] || 'Desconocido';
  }

  // Helper: Get status color
  getStatusColor(status: KYCStatus): string {
    const colors: Record<number, string> = {
      [KYCStatus.NotStarted]: 'gray',
      [KYCStatus.Pending]: 'yellow',
      [KYCStatus.InProgress]: 'blue',
      [KYCStatus.DocumentsRequired]: 'orange',
      [KYCStatus.UnderReview]: 'orange',
      [KYCStatus.PendingReview]: 'yellow',
      [KYCStatus.Approved]: 'green',
      [KYCStatus.Rejected]: 'red',
      [KYCStatus.Expired]: 'purple',
      [KYCStatus.Suspended]: 'red',
    };
    return colors[status] || 'gray';
  }

  // Submit profile for review - updates status to PendingReview
  async submitForReview(profileId: string): Promise<KYCProfile> {
    try {
      const response = await axios.put<KYCProfile>(
        `${KYC_API_URL}/kycprofiles/${profileId}`,
        {
          id: profileId,
          status: KYCStatus.PendingReview,
        },
        { headers: this.getAuthHeader() }
      );
      return response.data;
    } catch (error) {
      console.error('Error submitting KYC for review:', error);
      throw error;
    }
  }

  // Helper: Get document type label
  getDocumentTypeLabel(type: DocumentType): string {
    const labels: Record<DocumentType, string> = {
      [DocumentType.Cedula]: 'Cédula',
      [DocumentType.Passport]: 'Pasaporte',
      [DocumentType.DriverLicense]: 'Licencia de Conducir',
      [DocumentType.UtilityBill]: 'Factura de Servicios',
      [DocumentType.BankStatement]: 'Estado de Cuenta',
      [DocumentType.LeaseAgreement]: 'Contrato de Alquiler',
      [DocumentType.RNC]: 'RNC',
      [DocumentType.MercantileRegistry]: 'Registro Mercantil',
      [DocumentType.BusinessLicense]: 'Licencia Comercial',
      [DocumentType.TaxCertificate]: 'Certificación DGII',
      [DocumentType.IncomeProof]: 'Comprobante de Ingresos',
      [DocumentType.TaxReturn]: 'Declaración de Impuestos',
      [DocumentType.Selfie]: 'Selfie',
      [DocumentType.SelfieWithDocument]: 'Selfie con Documento',
    };
    return labels[type] || 'Documento';
  }

  // Helper: Get required documents by user type
  getRequiredDocuments(userType: 'buyer' | 'seller' | 'dealer'): DocumentType[] {
    switch (userType) {
      case 'buyer':
        return [DocumentType.Cedula, DocumentType.UtilityBill];
      case 'seller':
        return [DocumentType.Cedula, DocumentType.UtilityBill, DocumentType.SelfieWithDocument];
      case 'dealer':
        return [
          DocumentType.RNC,
          DocumentType.MercantileRegistry,
          DocumentType.BusinessLicense,
          DocumentType.TaxCertificate,
          DocumentType.Cedula,
        ];
      default:
        return [DocumentType.Cedula];
    }
  }

  // =============== WATCHLIST METHODS ===============

  // Search watchlist entries
  async searchWatchlist(searchTerm: string, listType?: WatchlistType): Promise<WatchlistEntry[]> {
    try {
      const response = await axios.get<WatchlistEntry[]>(`${KYC_API_URL}/watchlist/search`, {
        params: { searchTerm, listType },
        headers: this.getAuthHeader(),
      });
      return response.data;
    } catch (error) {
      console.error('Error searching watchlist:', error);
      return [];
    }
  }

  // Add watchlist entry
  async addWatchlistEntry(
    entry: Omit<WatchlistEntry, 'id' | 'isActive' | 'createdAt' | 'updatedAt'>
  ): Promise<WatchlistEntry> {
    const response = await axios.post<WatchlistEntry>(`${KYC_API_URL}/watchlist`, entry, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Screen individual against watchlist
  async screenIndividual(
    fullName: string,
    documentNumber?: string,
    dateOfBirth?: string
  ): Promise<{ isMatch: boolean; matches: WatchlistEntry[] }> {
    const response = await axios.post<{ isMatch: boolean; matches: WatchlistEntry[] }>(
      `${KYC_API_URL}/watchlist/screen`,
      { fullName, documentNumber, dateOfBirth },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // =============== STR METHODS ===============

  // Get STRs (Suspicious Transaction Reports)
  async getSTRs(params: {
    page?: number;
    pageSize?: number;
    status?: STRStatus;
  }): Promise<PaginatedResult<SuspiciousTransactionReport>> {
    try {
      const response = await axios.get<PaginatedResult<SuspiciousTransactionReport>>(
        `${KYC_API_URL}/str`,
        {
          params,
          headers: this.getAuthHeader(),
        }
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching STRs:', error);
      return { items: [], page: 1, pageSize: 50, totalCount: 0, totalPages: 0 };
    }
  }

  // Get STR statistics
  async getSTRStatistics(): Promise<{
    draftCount: number;
    pendingCount: number;
    approvedCount: number;
    sentCount: number;
    overdueCount: number;
    totalCount: number;
  }> {
    try {
      const response = await axios.get(`${KYC_API_URL}/str/statistics`, {
        headers: this.getAuthHeader(),
      });
      return response.data;
    } catch (error) {
      console.error('Error fetching STR statistics:', error);
      return {
        draftCount: 0,
        pendingCount: 0,
        approvedCount: 0,
        sentCount: 0,
        overdueCount: 0,
        totalCount: 0,
      };
    }
  }

  // Create STR
  async createSTR(data: {
    userId: string;
    suspiciousActivityType: string;
    description: string;
    amount: number;
    currency: string;
    redFlags: string[];
    detectedAt: string;
  }): Promise<SuspiciousTransactionReport> {
    const response = await axios.post<SuspiciousTransactionReport>(`${KYC_API_URL}/str`, data, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Approve STR
  async approveSTR(strId: string): Promise<void> {
    await axios.post(
      `${KYC_API_URL}/str/${strId}/approve`,
      {},
      {
        headers: this.getAuthHeader(),
      }
    );
  }

  // Send STR to UAF
  async sendSTRToUAF(strId: string, uafReportNumber: string): Promise<void> {
    await axios.post(
      `${KYC_API_URL}/str/${strId}/send-to-uaf`,
      { uafReportNumber },
      {
        headers: this.getAuthHeader(),
      }
    );
  }
}

export const kycService = new KYCService();
