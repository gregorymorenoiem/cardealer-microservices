import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const KYC_API_URL = `${API_BASE_URL}/api/kyc`;

// Enums matching backend
export enum KYCStatus {
  NotStarted = 0,
  InProgress = 1,
  PendingReview = 2,
  UnderReview = 3,
  Approved = 4,
  Rejected = 5,
  Expired = 6,
  Suspended = 7,
}

export enum RiskLevel {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

export enum DocumentType {
  // Identity Documents
  Cedula = 0,
  Passport = 1,
  DriverLicense = 2,
  // Proof of Address
  UtilityBill = 10,
  BankStatement = 11,
  LeaseAgreement = 12,
  // Business Documents (Dealers)
  RNC = 20,
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

export interface KYCProfile {
  id: string;
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
  status: KYCStatus;
  riskLevel: RiskLevel;
  riskScore: number;
  isPEP: boolean;
  pepPosition?: string;
  sourceOfFunds: string;
  occupation: string;
  expectedMonthlyTransaction?: number;
  createdAt: string;
  approvedAt?: string;
  approvedBy?: string;
  expiresAt?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  documents: KYCDocument[];
}

export interface KYCDocument {
  id: string;
  profileId: string;
  documentType: DocumentType;
  fileName: string;
  fileUrl: string;
  fileSizeBytes: number;
  mimeType: string;
  side?: 'Front' | 'Back';
  uploadedAt: string;
  verifiedAt?: string;
  verifiedBy?: string;
  verificationStatus: 'Pending' | 'Verified' | 'Rejected';
  rejectionReason?: string;
  ocrData?: Record<string, string>;
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
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        return null;
      }
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

  // Create new KYC profile
  async createProfile(data: CreateKYCProfileRequest): Promise<KYCProfile> {
    const response = await axios.post<KYCProfile>(`${KYC_API_URL}/kycprofiles`, data, {
      headers: this.getAuthHeader(),
    });
    return response.data;
  }

  // Update profile
  async updateProfile(id: string, data: Partial<CreateKYCProfileRequest>): Promise<KYCProfile> {
    const response = await axios.put<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}`,
      { id, ...data },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Upload document
  async uploadDocument(request: UploadDocumentRequest): Promise<KYCDocument> {
    const formData = new FormData();
    formData.append('profileId', request.profileId);
    formData.append('documentType', request.documentType.toString());
    formData.append('file', request.file);
    if (request.side) {
      formData.append('side', request.side);
    }

    const response = await axios.post<KYCDocument>(
      `${KYC_API_URL}/kyc/profiles/${request.profileId}/documents`,
      formData,
      {
        headers: {
          ...this.getAuthHeader(),
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  }

  // Get documents for profile
  async getDocuments(profileId: string): Promise<KYCDocument[]> {
    const response = await axios.get<KYCDocument[]>(
      `${KYC_API_URL}/kyc/profiles/${profileId}/documents`,
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Delete document
  async deleteDocument(documentId: string): Promise<void> {
    await axios.delete(`${KYC_API_URL}/kyc/documents/${documentId}`, {
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
    approvedBy: string,
    comments: string,
    expiresAt: string
  ): Promise<KYCProfile> {
    const response = await axios.post<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}/approve`,
      { id, approvedBy, comments, expiresAt },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Admin: Reject profile
  async rejectProfile(
    id: string,
    rejectionReason: string,
    comments: string,
    canRetry: boolean
  ): Promise<KYCProfile> {
    const response = await axios.post<KYCProfile>(
      `${KYC_API_URL}/kycprofiles/${id}/reject`,
      { id, rejectionReason, comments, canRetry },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  // Admin: Verify document
  async verifyDocument(documentId: string, verifiedBy: string): Promise<KYCDocument> {
    const response = await axios.post<KYCDocument>(
      `${KYC_API_URL}/kyc/documents/${documentId}/verify`,
      { id: documentId, verifiedBy, approved: true },
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
    const labels: Record<KYCStatus, string> = {
      [KYCStatus.NotStarted]: 'No Iniciado',
      [KYCStatus.InProgress]: 'En Progreso',
      [KYCStatus.PendingReview]: 'Pendiente de Revisión',
      [KYCStatus.UnderReview]: 'En Revisión',
      [KYCStatus.Approved]: 'Aprobado',
      [KYCStatus.Rejected]: 'Rechazado',
      [KYCStatus.Expired]: 'Expirado',
      [KYCStatus.Suspended]: 'Suspendido',
    };
    return labels[status] || 'Desconocido';
  }

  // Helper: Get status color
  getStatusColor(status: KYCStatus): string {
    const colors: Record<KYCStatus, string> = {
      [KYCStatus.NotStarted]: 'gray',
      [KYCStatus.InProgress]: 'blue',
      [KYCStatus.PendingReview]: 'yellow',
      [KYCStatus.UnderReview]: 'orange',
      [KYCStatus.Approved]: 'green',
      [KYCStatus.Rejected]: 'red',
      [KYCStatus.Expired]: 'purple',
      [KYCStatus.Suspended]: 'red',
    };
    return colors[status] || 'gray';
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
}

export const kycService = new KYCService();
