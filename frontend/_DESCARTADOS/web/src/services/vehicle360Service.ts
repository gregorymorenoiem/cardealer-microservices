/**
 * Vehicle 360° Processing Service
 *
 * Servicio TypeScript para interactuar con Vehicle360ProcessingService.
 * Maneja el procesamiento de videos 360°, extracción de frames y remoción de fondos.
 *
 * @module services/vehicle360Service
 * @version 1.0.0
 * @since Enero 28, 2026
 */

import api from './api';

// ============================================================================
// Types & Interfaces
// ============================================================================

/** Estados posibles de un job de procesamiento 360° */
export type Vehicle360JobStatus =
  | 'Pending'
  | 'Queued'
  | 'Validating'
  | 'ValidationFailed'
  | 'Uploading'
  | 'UploadFailed'
  | 'ExtractingFrames'
  | 'ExtractionFailed'
  | 'RemovingBackground'
  | 'BackgroundRemovalFailed'
  | 'Finalizing'
  | 'FinalizationFailed'
  | 'Completed'
  | 'Failed'
  | 'Cancelled';

/** Opciones de procesamiento para el video 360° */
export interface ProcessingOptions {
  /** Ancho de salida de las imágenes (default: 1920) */
  outputWidth?: number;
  /** Alto de salida de las imágenes (default: 1080) */
  outputHeight?: number;
  /** Formato de salida: 'png' | 'jpg' | 'webp' */
  outputFormat?: string;
  /** Selección inteligente de frames */
  smartFrameSelection?: boolean;
  /** Corrección automática de exposición */
  autoCorrectExposure?: boolean;
  /** Generar thumbnails */
  generateThumbnails?: boolean;
  /** Color de fondo: 'transparent' | 'white' | 'studio' | hex color */
  backgroundColor?: string;
}

/** Request para iniciar procesamiento desde archivo de video */
export interface StartProcessingRequest {
  /** ID del vehículo */
  vehicleId: string;
  /** Archivo de video */
  video: File;
  /** Número de frames a extraer (default: 6) */
  frameCount?: number;
  /** Opciones de procesamiento */
  options?: ProcessingOptions;
}

/** Request para iniciar procesamiento desde URL */
export interface StartProcessingFromUrlRequest {
  /** ID del vehículo */
  vehicleId: string;
  /** URL del video ya subido */
  videoUrl: string;
  /** Número de frames a extraer (default: 6) */
  frameCount?: number;
  /** Opciones de procesamiento */
  options?: ProcessingOptions;
}

/** Response al iniciar procesamiento */
export interface StartProcessingResponse {
  /** ID del job creado */
  jobId: string;
  /** Estado inicial */
  status: Vehicle360JobStatus;
  /** Tiempo estimado en minutos */
  estimatedMinutes: number;
  /** Mensaje descriptivo */
  message: string;
  /** URL para verificar estado */
  statusCheckUrl: string;
}

/** Progreso del job */
export interface JobProgress {
  /** Porcentaje completado (0-100) */
  percentage: number;
  /** Paso actual */
  currentStep: string;
  /** Proveedor actual siendo usado */
  currentProvider?: string;
  /** Frames procesados */
  framesProcessed?: number;
  /** Total de frames */
  totalFrames?: number;
}

/** Response de estado del job */
export interface JobStatusResponse {
  /** ID del job */
  jobId: string;
  /** Estado actual */
  status: Vehicle360JobStatus;
  /** Progreso detallado */
  progress: JobProgress;
  /** Si el job está completo */
  isComplete: boolean;
  /** Si el job falló */
  isFailed: boolean;
  /** Mensaje de error si aplica */
  errorMessage?: string;
  /** Código de error */
  errorCode?: string;
  /** Fecha de creación */
  createdAt: string;
  /** Fecha de última actualización */
  updatedAt: string;
}

/** Resultado del procesamiento */
export interface ProcessingResult {
  /** ID de la vista 360° */
  view360Id: string;
  /** Cantidad de frames extraídos */
  extractedFrameCount: number;
  /** URLs de las imágenes procesadas */
  processedImageUrls: string[];
  /** URL del visor 360° */
  viewerUrl: string;
  /** URL del thumbnail */
  thumbnailUrl: string;
  /** Proveedores utilizados */
  providersUsed: {
    video: string;
    background: string;
  };
  /** Costo total del procesamiento */
  totalCost: number;
}

/** Response completa del job */
export interface Vehicle360JobResponse {
  /** ID del job */
  jobId: string;
  /** ID del vehículo */
  vehicleId: string;
  /** ID del usuario */
  userId: string;
  /** Estado actual */
  status: Vehicle360JobStatus;
  /** Progreso */
  progress: JobProgress;
  /** Resultado (solo si status = Completed) */
  result?: ProcessingResult;
  /** Configuración usada */
  options: ProcessingOptions;
  /** Cantidad de reintentos */
  retryCount: number;
  /** Máximo de reintentos */
  maxRetries: number;
  /** Fecha de creación */
  createdAt: string;
  /** Fecha de inicio */
  startedAt?: string;
  /** Fecha de completado */
  completedAt?: string;
  /** Errores ocurridos */
  errors: string[];
}

/** Datos del visor 360° */
export interface Vehicle360ViewerData {
  /** ID de la vista */
  viewId: string;
  /** ID del vehículo */
  vehicleId: string;
  /** Estado de la vista */
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  /** URL del visor embebido (si aplica) */
  spinViewerUrl?: string;
  /** Código de embed (si aplica) */
  spinEmbedCode?: string;
  /** URLs de los frames extraídos */
  extractedFrameUrls: string[];
  /** Cantidad de frames */
  extractedFrameCount: number;
  /** URL del thumbnail */
  thumbnailUrl?: string;
  /** Porcentaje de progreso */
  progressPercent: number;
  /** Mensaje de error */
  errorMessage?: string;
  /** Hotspots configurados */
  hotspots?: Hotspot[];
}

/** Hotspot en la vista 360° */
export interface Hotspot {
  id: string;
  x: number;
  y: number;
  degrees: number;
  label: string;
  description: string;
  type: 'feature' | 'damage' | 'upgrade' | 'info';
}

/** Estado de salud de los servicios */
export interface ServicesHealthResponse {
  allHealthy: boolean;
  services: {
    name: string;
    isHealthy: boolean;
    latencyMs: number;
    lastChecked: string;
    errorMessage?: string;
  }[];
}

// ============================================================================
// API Functions
// ============================================================================

const VEHICLE360_BASE = '/api/vehicle360processing';

/**
 * Inicia el procesamiento 360° de un video
 * @param request - Request con el archivo de video y opciones
 * @returns Response con el ID del job creado
 */
export async function startProcessing(
  request: StartProcessingRequest
): Promise<StartProcessingResponse> {
  const formData = new FormData();
  formData.append('video', request.video);
  formData.append('vehicleId', request.vehicleId);
  formData.append('frameCount', String(request.frameCount || 6));

  if (request.options) {
    if (request.options.outputWidth) {
      formData.append('outputWidth', String(request.options.outputWidth));
    }
    if (request.options.outputHeight) {
      formData.append('outputHeight', String(request.options.outputHeight));
    }
    if (request.options.outputFormat) {
      formData.append('outputFormat', request.options.outputFormat);
    }
    if (request.options.smartFrameSelection !== undefined) {
      formData.append('smartFrameSelection', String(request.options.smartFrameSelection));
    }
    if (request.options.autoCorrectExposure !== undefined) {
      formData.append('autoCorrectExposure', String(request.options.autoCorrectExposure));
    }
    if (request.options.generateThumbnails !== undefined) {
      formData.append('generateThumbnails', String(request.options.generateThumbnails));
    }
    if (request.options.backgroundColor) {
      formData.append('backgroundColor', request.options.backgroundColor);
    }
  }

  const response = await api.post<StartProcessingResponse>(`${VEHICLE360_BASE}/process`, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });
  return response.data;
}

/**
 * Inicia el procesamiento 360° desde una URL de video existente
 * @param request - Request con la URL del video y opciones
 * @returns Response con el ID del job creado
 */
export async function startProcessingFromUrl(
  request: StartProcessingFromUrlRequest
): Promise<StartProcessingResponse> {
  const response = await api.post<StartProcessingResponse>(`${VEHICLE360_BASE}/process-from-url`, {
    vehicleId: request.vehicleId,
    videoUrl: request.videoUrl,
    frameCount: request.frameCount || 6,
    outputWidth: request.options?.outputWidth,
    outputHeight: request.options?.outputHeight,
    outputFormat: request.options?.outputFormat,
    smartFrameSelection: request.options?.smartFrameSelection ?? true,
    autoCorrectExposure: request.options?.autoCorrectExposure ?? true,
    generateThumbnails: request.options?.generateThumbnails ?? true,
    backgroundColor: request.options?.backgroundColor ?? 'transparent',
  });
  return response.data;
}

/**
 * Obtiene el estado actual de un job
 * @param jobId - ID del job
 * @returns Estado actual del job
 */
export async function getJobStatus(jobId: string): Promise<JobStatusResponse> {
  const response = await api.get<JobStatusResponse>(`${VEHICLE360_BASE}/jobs/${jobId}/status`);
  return response.data;
}

/**
 * Obtiene los detalles completos de un job
 * @param jobId - ID del job
 * @returns Detalles completos del job
 */
export async function getJob(jobId: string): Promise<Vehicle360JobResponse> {
  const response = await api.get<Vehicle360JobResponse>(`${VEHICLE360_BASE}/jobs/${jobId}`);
  return response.data;
}

/**
 * Obtiene el resultado del procesamiento
 * @param jobId - ID del job
 * @returns Resultado con URLs de imágenes procesadas
 */
export async function getJobResult(jobId: string): Promise<ProcessingResult> {
  const response = await api.get<ProcessingResult>(`${VEHICLE360_BASE}/jobs/${jobId}/result`);
  return response.data;
}

/**
 * Reintenta un job fallido
 * @param jobId - ID del job a reintentar
 * @returns Job actualizado
 */
export async function retryJob(jobId: string): Promise<Vehicle360JobResponse> {
  const response = await api.post<Vehicle360JobResponse>(`${VEHICLE360_BASE}/jobs/${jobId}/retry`);
  return response.data;
}

/**
 * Cancela un job en progreso
 * @param jobId - ID del job a cancelar
 * @param reason - Razón de la cancelación (opcional)
 */
export async function cancelJob(jobId: string, reason?: string): Promise<void> {
  await api.post(`${VEHICLE360_BASE}/jobs/${jobId}/cancel`, { reason });
}

/**
 * Obtiene los datos del visor 360° para un vehículo
 * @param vehicleId - ID del vehículo
 * @returns Datos del visor 360°
 */
export async function getVehicleViewer(vehicleId: string): Promise<Vehicle360ViewerData> {
  const response = await api.get<Vehicle360ViewerData>(`${VEHICLE360_BASE}/viewer/${vehicleId}`);
  return response.data;
}

/**
 * Obtiene todos los jobs de un vehículo
 * @param vehicleId - ID del vehículo
 * @returns Lista de jobs del vehículo
 */
export async function getVehicleJobs(vehicleId: string): Promise<Vehicle360JobResponse[]> {
  const response = await api.get<Vehicle360JobResponse[]>(
    `${VEHICLE360_BASE}/vehicles/${vehicleId}/jobs`
  );
  return response.data;
}

/**
 * Obtiene los jobs del usuario actual
 * @param page - Número de página
 * @param pageSize - Tamaño de página
 * @returns Lista paginada de jobs del usuario
 */
export async function getMyJobs(
  page: number = 1,
  pageSize: number = 20
): Promise<Vehicle360JobResponse[]> {
  const response = await api.get<Vehicle360JobResponse[]>(`${VEHICLE360_BASE}/my-jobs`, {
    params: { page, pageSize },
  });
  return response.data;
}

/**
 * Verifica la salud de los servicios dependientes
 * @returns Estado de salud de todos los servicios
 */
export async function checkServicesHealth(): Promise<ServicesHealthResponse> {
  const response = await api.get<ServicesHealthResponse>(`${VEHICLE360_BASE}/health/services`);
  return response.data;
}

/**
 * Hace polling del estado del job hasta que complete o falle
 * @param jobId - ID del job
 * @param onProgress - Callback para actualizaciones de progreso
 * @param maxAttempts - Máximo de intentos de polling (default: 120 = 10 min)
 * @param intervalMs - Intervalo entre polls en ms (default: 5000)
 * @returns Job completo
 */
export async function pollJobUntilComplete(
  jobId: string,
  onProgress?: (status: JobStatusResponse) => void,
  maxAttempts: number = 120,
  intervalMs: number = 5000
): Promise<Vehicle360JobResponse> {
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    const status = await getJobStatus(jobId);

    if (onProgress) {
      onProgress(status);
    }

    if (status.isComplete) {
      return await getJob(jobId);
    }

    if (status.isFailed) {
      throw new Error(status.errorMessage || 'El procesamiento falló');
    }

    // Esperar antes del siguiente poll
    await new Promise((resolve) => setTimeout(resolve, intervalMs));
  }

  throw new Error('Timeout: El procesamiento tomó demasiado tiempo');
}

/**
 * Verifica si un vehículo tiene vista 360° disponible
 * @param vehicleId - ID del vehículo
 * @returns true si hay vista 360° lista
 */
export async function hasVehicle360View(vehicleId: string): Promise<boolean> {
  try {
    const viewer = await getVehicleViewer(vehicleId);
    return viewer.status === 'Completed' && viewer.extractedFrameUrls.length > 0;
  } catch {
    return false;
  }
}

// Export como objeto para uso con import
export const vehicle360Service = {
  startProcessing,
  startProcessingFromUrl,
  getJobStatus,
  getJob,
  getJobResult,
  retryJob,
  cancelJob,
  getVehicleViewer,
  getVehicleJobs,
  getMyJobs,
  checkServicesHealth,
  pollJobUntilComplete,
  hasVehicle360View,
};

export default vehicle360Service;
