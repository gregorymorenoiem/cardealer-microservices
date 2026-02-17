import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const IDENTITY_VERIFICATION_URL = `${API_BASE_URL}/api/kyc/identity-verification`;

// ===== Enums =====

export enum VerificationSessionStatus {
  Started = 1,
  DocumentFrontCaptured = 2,
  DocumentBackCaptured = 3,
  DocumentProcessing = 4,
  AwaitingSelfie = 5,
  SelfieCaptured = 6,
  ProcessingBiometrics = 7,
  Completed = 8,
  Failed = 9,
  Expired = 10,
}

export enum LivenessChallengeType {
  Blink = 'Blink',
  Smile = 'Smile',
  TurnLeft = 'TurnLeft',
  TurnRight = 'TurnRight',
  Nod = 'Nod',
  OpenMouth = 'OpenMouth',
}

export enum DocumentType {
  Cedula = 0,
  Passport = 1,
  DriverLicense = 2,
}

export enum VerificationFailureReason {
  None = 'None',
  DocumentBlurry = 'DocumentBlurry',
  DocumentCutOff = 'DocumentCutOff',
  DocumentGlare = 'DocumentGlare',
  DocumentExpired = 'DocumentExpired',
  DocumentFake = 'DocumentFake',
  FaceNotDetected = 'FaceNotDetected',
  FaceMismatch = 'FaceMismatch',
  LivenessCheckFailed = 'LivenessCheckFailed',
  MultipleAttemptsFailed = 'MultipleAttemptsFailed',
  SessionExpired = 'SessionExpired',
  OCRFailed = 'OCRFailed',
  InvalidDocumentNumber = 'InvalidDocumentNumber',
}

// ===== Interfaces =====

export interface DeviceInfo {
  platform?: string;
  version?: string;
  model?: string;
  appVersion?: string;
  browser?: string;
  userAgent?: string;
}

export interface LocationInfo {
  latitude?: number;
  longitude?: number;
}

export interface StartVerificationRequest {
  documentType: DocumentType;
  deviceInfo?: DeviceInfo;
  location?: LocationInfo;
}

export interface VerificationInstructions {
  title: string;
  steps: string[];
  tips: string[];
}

export interface StartVerificationResponse {
  sessionId: string;
  status: string;
  documentType: string;
  expiresAt: string;
  expiresInSeconds: number;
  nextStep: string;
  instructions: VerificationInstructions;
  requiredChallenges: string[];
}

export interface OcrExtractedData {
  fullName?: string;
  firstName?: string;
  lastName?: string;
  documentNumber?: string;
  dateOfBirth?: string;
  expiryDate?: string;
  nationality?: string;
  gender?: string;
  address?: string;
}

export interface OcrResult {
  success: boolean;
  extractedData?: OcrExtractedData;
  confidence: number;
  errors: string[];
}

export interface DocumentValidation {
  formatValid: boolean;
  checksumValid: boolean;
  notExpired: boolean;
  ageValid: boolean;
  issues: string[];
}

export interface DocumentProcessedResponse {
  sessionId: string;
  side: string;
  status: string;
  ocrResult?: OcrResult;
  documentValidation?: DocumentValidation;
  nextStep: string;
  instructions?: VerificationInstructions;
}

export interface ChallengeResult {
  type: string;
  passed: boolean;
  timestamp: string;
  confidence?: number;
}

export interface LivenessData {
  challenges: ChallengeResult[];
  videoFrames?: string[];
  deviceGyroscope?: string;
}

export interface DocumentAuthenticityResult {
  passed: boolean;
  score: number;
  checks: string[];
}

export interface LivenessDetectionResult {
  passed: boolean;
  score: number;
  challengesPassed: number;
  challengesTotal: number;
}

export interface FaceMatchResult {
  passed: boolean;
  score: number;
  threshold: number;
}

export interface OcrAccuracyResult {
  confidence: number;
  fieldsExtracted: number;
  fieldsTotal: number;
}

export interface VerificationDetails {
  documentAuthenticity: DocumentAuthenticityResult;
  livenessDetection: LivenessDetectionResult;
  faceMatch: FaceMatchResult;
  ocrAccuracy: OcrAccuracyResult;
}

export interface VerificationResult {
  verified: boolean;
  overallScore: number;
  details: VerificationDetails;
}

export interface ExtractedProfile {
  fullName: string;
  firstName?: string;
  lastName?: string;
  documentNumber: string;
  documentType: string;
  dateOfBirth?: string;
  nationality?: string;
  gender?: string;
  address?: string;
}

export interface VerificationCompletedResponse {
  sessionId: string;
  status: string;
  result: VerificationResult;
  extractedProfile?: ExtractedProfile;
  kycProfileId?: string;
  kycStatus: string;
  message: string;
}

export interface ScoreResult {
  score: number;
  threshold?: number;
  passed: boolean;
}

export interface FailedResult {
  verified: boolean;
  failureReason: string;
  failureDetails: string;
  scores: Record<string, ScoreResult>;
}

export interface RetryInstructions {
  title: string;
  reason: string;
  suggestions: string[];
}

export interface SupportContact {
  email: string;
  phone: string;
  whatsApp: string;
}

export interface VerificationFailedResponse {
  sessionId: string;
  status: string;
  result: FailedResult;
  attemptsRemaining: number;
  canRetry: boolean;
  retryInstructions?: RetryInstructions;
  supportContact?: SupportContact;
}

export interface VerificationSessionDetails {
  sessionId: string;
  status: string;
  failureReason?: string;
  attemptNumber: number;
  maxAttempts: number;
  canRetry: boolean;
  createdAt: string;
  expiresAt: string;
  isExpired: boolean;
  currentStep: string;
  documentFrontCaptured: boolean;
  documentBackCaptured: boolean;
  selfieCapture: boolean;
  livenessCompleted: boolean;
  livenessScore?: number;
  faceMatchScore?: number;
  ocrConfidence?: number;
  overallScore?: number;
}

export interface VerificationSessionSummary {
  id: string;
  userId: string;
  status: string;
  documentType: string;
  attemptNumber: number;
  createdAt: string;
  completedAt?: string;
  overallScore?: number;
  verified: boolean;
}

export interface CanStartVerificationResult {
  canStart: boolean;
  reason?: string;
  canRetryAfter?: string;
  hasActiveSession: boolean;
  activeSessionId?: string;
  hasApprovedKYC: boolean;
  totalAttemptsToday: number;
}

// ===== Service Class =====

class IdentityVerificationService {
  private getAuthHeader() {
    const token = localStorage.getItem('accessToken');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  /**
   * Verificar si el usuario puede iniciar una nueva verificación
   */
  async canStartVerification(): Promise<CanStartVerificationResult> {
    const response = await axios.get<CanStartVerificationResult>(
      `${IDENTITY_VERIFICATION_URL}/can-start`,
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  /**
   * Iniciar sesión de verificación de identidad
   */
  async startVerification(request: StartVerificationRequest): Promise<StartVerificationResponse> {
    const response = await axios.post<StartVerificationResponse>(
      `${IDENTITY_VERIFICATION_URL}/start`,
      request,
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  /**
   * Subir foto del documento (frente o reverso)
   */
  async uploadDocument(
    sessionId: string,
    side: 'Front' | 'Back',
    image: File
  ): Promise<DocumentProcessedResponse> {
    const formData = new FormData();
    formData.append('sessionId', sessionId);
    formData.append('side', side);
    formData.append('image', image);

    const response = await axios.post<DocumentProcessedResponse>(
      `${IDENTITY_VERIFICATION_URL}/document`,
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

  /**
   * Subir selfie con datos de liveness
   */
  async uploadSelfie(
    sessionId: string,
    selfieImage: File | Blob,
    livenessData?: LivenessData
  ): Promise<VerificationCompletedResponse | VerificationFailedResponse> {
    const formData = new FormData();
    formData.append('sessionId', sessionId);
    formData.append('selfie', selfieImage, 'selfie.jpg');

    if (livenessData) {
      formData.append('livenessDataJson', JSON.stringify(livenessData));
    }

    const response = await axios.post(`${IDENTITY_VERIFICATION_URL}/selfie`, formData, {
      headers: {
        ...this.getAuthHeader(),
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  }

  /**
   * Completar verificación
   */
  async completeVerification(sessionId: string): Promise<VerificationCompletedResponse> {
    const response = await axios.post<VerificationCompletedResponse>(
      `${IDENTITY_VERIFICATION_URL}/complete`,
      { sessionId },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  /**
   * Obtener estado de una sesión
   */
  async getSessionStatus(sessionId: string): Promise<VerificationSessionDetails> {
    const response = await axios.get<VerificationSessionDetails>(
      `${IDENTITY_VERIFICATION_URL}/${sessionId}`,
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  /**
   * Obtener sesión activa
   */
  async getActiveSession(): Promise<VerificationSessionDetails | null> {
    try {
      const response = await axios.get<VerificationSessionDetails>(
        `${IDENTITY_VERIFICATION_URL}/active`,
        { headers: this.getAuthHeader() }
      );
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Reintentar verificación fallida
   */
  async retryVerification(sessionId: string): Promise<StartVerificationResponse> {
    const response = await axios.post<StartVerificationResponse>(
      `${IDENTITY_VERIFICATION_URL}/retry`,
      { sessionId },
      { headers: this.getAuthHeader() }
    );
    return response.data;
  }

  /**
   * Cancelar sesión
   */
  async cancelSession(sessionId: string, reason?: string): Promise<void> {
    await axios.delete(`${IDENTITY_VERIFICATION_URL}/${sessionId}`, {
      headers: this.getAuthHeader(),
      params: { reason },
    });
  }

  /**
   * Obtener historial de verificaciones
   */
  async getVerificationHistory(limit = 10): Promise<VerificationSessionSummary[]> {
    const response = await axios.get<VerificationSessionSummary[]>(
      `${IDENTITY_VERIFICATION_URL}/history`,
      {
        headers: this.getAuthHeader(),
        params: { limit },
      }
    );
    return response.data;
  }

  /**
   * Convertir File/Blob a base64
   */
  async fileToBase64(file: File | Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        // Remove data URL prefix
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = (error) => reject(error);
    });
  }

  /**
   * Obtener información del dispositivo
   */
  getDeviceInfo(): DeviceInfo {
    const userAgent = navigator.userAgent;
    let platform = 'Web';
    let browser = 'Unknown';

    if (/iPhone|iPad|iPod/.test(userAgent)) {
      platform = 'iOS';
    } else if (/Android/.test(userAgent)) {
      platform = 'Android';
    } else if (/Windows/.test(userAgent)) {
      platform = 'Windows';
    } else if (/Mac/.test(userAgent)) {
      platform = 'macOS';
    }

    if (/Chrome/.test(userAgent)) {
      browser = 'Chrome';
    } else if (/Safari/.test(userAgent)) {
      browser = 'Safari';
    } else if (/Firefox/.test(userAgent)) {
      browser = 'Firefox';
    } else if (/Edge/.test(userAgent)) {
      browser = 'Edge';
    }

    return {
      platform,
      browser,
      userAgent,
      appVersion: '1.0.0',
    };
  }

  /**
   * Obtener ubicación del usuario
   */
  async getLocation(): Promise<LocationInfo | null> {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        resolve(null);
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          });
        },
        () => {
          resolve(null);
        },
        { timeout: 5000 }
      );
    });
  }

  /**
   * Mapear razón de fallo a mensaje amigable
   */
  getFailureMessage(reason: string): string {
    const messages: Record<string, string> = {
      DocumentBlurry: 'La imagen del documento está borrosa',
      DocumentCutOff: 'El documento no está completo en la imagen',
      DocumentGlare: 'Hay reflejos en el documento',
      DocumentExpired: 'El documento está expirado',
      DocumentFake: 'Se detectó un problema con la autenticidad del documento',
      FaceNotDetected: 'No se pudo detectar un rostro en la imagen',
      FaceMismatch: 'El rostro no coincide con el documento',
      LivenessCheckFailed: 'No se pudo verificar que eres una persona real',
      MultipleAttemptsFailed: 'Se excedió el número máximo de intentos',
      SessionExpired: 'La sesión ha expirado',
      OCRFailed: 'No se pudieron leer los datos del documento',
      InvalidDocumentNumber: 'El número de documento no es válido',
    };

    return messages[reason] || 'Error desconocido en la verificación';
  }

  /**
   * Mapear challenge a instrucción en español
   */
  getChallengeInstruction(challenge: string): string {
    const instructions: Record<string, string> = {
      Blink: 'Parpadea 2 veces',
      Smile: 'Sonríe',
      TurnLeft: 'Gira la cabeza a la izquierda',
      TurnRight: 'Gira la cabeza a la derecha',
      Nod: 'Asiente con la cabeza',
      OpenMouth: 'Abre la boca',
    };

    return instructions[challenge] || challenge;
  }
}

// Export singleton instance
export const identityVerificationService = new IdentityVerificationService();
export default identityVerificationService;
