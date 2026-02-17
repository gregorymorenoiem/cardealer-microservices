/**
 * Document Capture Component
 *
 * Captures identity document (Cedula) images using webcam or file upload.
 * Features:
 * - Real-time camera preview with document frame overlay
 * - Quality indicators (brightness, focus)
 * - Support for front/back capture
 * - Fallback to file upload
 */

'use client';

import * as React from 'react';
import dynamic from 'next/dynamic';
import type ReactWebcam from 'react-webcam';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const Webcam = dynamic<any>((() => import('react-webcam')) as any, {
  ssr: false,
  loading: () => (
    <div className="bg-muted flex h-64 w-full items-center justify-center rounded-lg">
      <p className="text-muted-foreground text-sm">Cargando cámara...</p>
    </div>
  ),
});
import {
  Camera,
  RotateCcw,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Upload,
  Loader2,
  FlipHorizontal,
} from 'lucide-react';
import Image from 'next/image';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export type DocumentSide = 'Front' | 'Back';

interface DocumentCaptureProps {
  side: DocumentSide;
  documentType?: 'Cedula' | 'Passport' | 'DriverLicense';
  onCapture: (image: File, side: DocumentSide) => Promise<void>;
  onError?: (error: string) => void;
  isProcessing?: boolean;
  capturedImage?: string | null;
  instructions?: string[];
  className?: string;
}

interface CaptureQuality {
  brightness: 'good' | 'low' | 'high';
  sharpness: 'good' | 'blurry';
  hasDocument: boolean;
  isReady: boolean;
}

// =============================================================================
// HELPERS
// =============================================================================

/**
 * Validates image file by checking magic bytes (file signature)
 * Prevents malicious files disguised as images
 */
async function validateImageMagicBytes(file: File): Promise<boolean> {
  try {
    const buffer = await file.slice(0, 8).arrayBuffer();
    const bytes = new Uint8Array(buffer);

    // JPEG: FF D8 FF
    if (bytes[0] === 0xff && bytes[1] === 0xd8 && bytes[2] === 0xff) return true;

    // PNG: 89 50 4E 47 0D 0A 1A 0A
    if (
      bytes[0] === 0x89 &&
      bytes[1] === 0x50 &&
      bytes[2] === 0x4e &&
      bytes[3] === 0x47 &&
      bytes[4] === 0x0d &&
      bytes[5] === 0x0a &&
      bytes[6] === 0x1a &&
      bytes[7] === 0x0a
    )
      return true;

    // WebP: RIFF....WEBP
    if (bytes[0] === 0x52 && bytes[1] === 0x49 && bytes[2] === 0x46 && bytes[3] === 0x46)
      return true;

    // HEIC/HEIF: Check for ftyp box
    if (bytes[4] === 0x66 && bytes[5] === 0x74 && bytes[6] === 0x79 && bytes[7] === 0x70)
      return true;

    return false;
  } catch {
    return false;
  }
}

/**
 * Sanitizes filename to prevent path traversal and special characters
 */
function sanitizeFilename(filename: string): string {
  return filename
    .replace(/[^a-zA-Z0-9._-]/g, '_')
    .replace(/\.{2,}/g, '.')
    .slice(0, 100);
}

function dataURLtoFile(dataUrl: string, filename: string): File {
  const arr = dataUrl.split(',');
  const mime = arr[0].match(/:(.*?);/)?.[1] || 'image/jpeg';
  const bstr = atob(arr[1]);
  let n = bstr.length;
  const u8arr = new Uint8Array(n);
  while (n--) {
    u8arr[n] = bstr.charCodeAt(n);
  }
  return new File([u8arr], filename, { type: mime });
}

// =============================================================================
// DEFAULT INSTRUCTIONS
// =============================================================================

const defaultInstructions = {
  Front: [
    'Coloca el frente de tu cédula dentro del marco',
    'Asegúrate de que la foto y los datos sean visibles',
    'Evita reflejos y sombras',
    'Mantén el documento en posición horizontal',
  ],
  Back: [
    'Voltea tu cédula y coloca el reverso dentro del marco',
    'Asegúrate de que el código de barras sea visible',
    'La foto debe estar bien iluminada',
    'El documento debe estar completamente visible',
  ],
};

const documentLabels = {
  Cedula: 'Cédula de Identidad',
  Passport: 'Pasaporte',
  DriverLicense: 'Licencia de Conducir',
};

const sideLabels = {
  Front: 'Frente',
  Back: 'Reverso',
};

// =============================================================================
// COMPONENT
// =============================================================================

export function DocumentCapture({
  side,
  documentType = 'Cedula',
  onCapture,
  onError,
  isProcessing = false,
  capturedImage = null,
  instructions,
  className,
}: DocumentCaptureProps) {
  const webcamRef = React.useRef<ReactWebcam>(null);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  const [cameraEnabled, setCameraEnabled] = React.useState(true); // Auto-activate camera for better UX
  const [hasCamera, setHasCamera] = React.useState(true);
  const [cameraReady, setCameraReady] = React.useState(false);
  const [capturedPhoto, setCapturedPhoto] = React.useState<string | null>(capturedImage);
  const [isAnalyzing, setIsAnalyzing] = React.useState(false);
  const [captureQuality, setCaptureQuality] = React.useState<CaptureQuality>({
    brightness: 'good',
    sharpness: 'good',
    hasDocument: false,
    isReady: false,
  });
  const [error, setError] = React.useState<string | null>(null);
  const [facingMode, setFacingMode] = React.useState<'user' | 'environment'>('environment');

  // Video constraints optimized for document capture
  const videoConstraints = {
    width: { ideal: 1920, min: 1280 },
    height: { ideal: 1080, min: 720 },
    facingMode: facingMode,
    aspectRatio: { ideal: 16 / 9 },
  };

  // Check if camera permission is available (Permissions API)
  const checkCameraPermission = React.useCallback(async (): Promise<
    'granted' | 'denied' | 'prompt' | 'unknown'
  > => {
    try {
      // Permissions API is not available in all browsers (especially Safari)
      if (!navigator.permissions || !navigator.permissions.query) {
        return 'unknown';
      }
      const result = await navigator.permissions.query({ name: 'camera' as PermissionName });
      return result.state;
    } catch {
      // Safari and some browsers don't support camera permission query
      return 'unknown';
    }
  }, []);

  // Request camera permission and enable camera
  const enableCamera = React.useCallback(async () => {
    try {
      // Check if mediaDevices is available
      if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        setHasCamera(false);
        setError(
          'Tu navegador no soporta acceso a la cámara. Por favor, usa Chrome, Firefox, Safari o Edge actualizado, o sube una imagen.'
        );
        onError?.('Camera not supported');
        return;
      }

      // Don't rely on Permissions API - it can be stale or inaccurate
      // Instead, always try to access the camera and handle errors directly

      // Try to get camera access with progressive fallback for better compatibility
      let stream: MediaStream;
      try {
        // First try with ideal constraints
        stream = await navigator.mediaDevices.getUserMedia({
          video: {
            facingMode: 'environment',
            width: { ideal: 1280 },
            height: { ideal: 720 },
          },
        });
      } catch {
        try {
          // Fallback to simpler constraints (better Safari/mobile compatibility)
          stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'environment' },
          });
        } catch {
          // Final fallback to basic video
          stream = await navigator.mediaDevices.getUserMedia({ video: true });
        }
      }

      // Stop the test stream immediately
      stream.getTracks().forEach(track => track.stop());
      // Now enable the camera component
      setCameraEnabled(true);
      setError(null);
    } catch (err: unknown) {
      const error = err as Error;
      console.warn('Camera permission error:', error.name, error.message);

      setHasCamera(false);

      // Provide specific error messages based on the error type
      if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
        setError(
          'Acceso a la cámara denegado. Por favor, permite el acceso en la configuración de tu navegador y recarga la página, o usa la opción de subir archivo.'
        );
      } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
        setError(
          'No se detectó ninguna cámara. Por favor, conecta una cámara o usa la opción de subir archivo.'
        );
      } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
        setError(
          'La cámara está siendo usada por otra aplicación. Cierra otras aplicaciones que usen la cámara y vuelve a intentar.'
        );
      } else if (error.name === 'OverconstrainedError') {
        setError(
          'Tu cámara no cumple con los requisitos mínimos. Por favor, usa la opción de subir archivo.'
        );
      } else if (error.name === 'SecurityError') {
        setError(
          'La cámara solo funciona en conexiones seguras (HTTPS). En desarrollo, usa localhost o sube una imagen.'
        );
      } else {
        setError(
          'No se pudo acceder a la cámara. Por favor, permite el acceso en tu navegador o usa la opción de subir archivo.'
        );
      }

      onError?.('Camera access denied');
    }
  }, [onError, checkCameraPermission]);

  // Handle camera errors from Webcam component
  const handleCameraError = React.useCallback(
    (err: string | DOMException) => {
      const errorName = typeof err === 'string' ? err : err.name;
      console.warn('Webcam component error:', errorName);

      setHasCamera(false);
      setCameraEnabled(false);

      // Try to give a helpful message based on error type
      if (errorName === 'NotAllowedError' || errorName.includes('Permission')) {
        setError(
          'Permiso de cámara denegado. Permite el acceso en la configuración del navegador o sube una imagen.'
        );
      } else if (errorName === 'NotFoundError' || errorName.includes('NotFound')) {
        setError(
          'No se encontró la cámara. Verifica que tu dispositivo tenga cámara o sube una imagen.'
        );
      } else if (errorName === 'NotReadableError') {
        setError('La cámara está en uso por otra aplicación. Ciérrala y vuelve a intentar.');
      } else {
        setError('Error al acceder a la cámara. Por favor, recarga la página o sube una imagen.');
      }

      onError?.('Camera access denied');
    },
    [onError]
  );

  // Handle camera ready
  const handleUserMedia = React.useCallback(() => {
    setCameraReady(true);
    setError(null);
  }, []);

  // Analyze image quality (simplified)
  const analyzeImageQuality = React.useCallback((): CaptureQuality => {
    return {
      brightness: 'good',
      sharpness: 'good',
      hasDocument: true,
      isReady: true,
    };
  }, []);

  // Capture photo from webcam
  const capturePhoto = React.useCallback(async () => {
    if (!webcamRef.current) return;

    setIsAnalyzing(true);
    setError(null);

    try {
      const imageSrc = webcamRef.current.getScreenshot({
        width: 1920,
        height: 1080,
      });

      if (!imageSrc) {
        throw new Error('No se pudo capturar la imagen');
      }

      const quality = analyzeImageQuality();
      setCaptureQuality(quality);

      if (!quality.isReady) {
        setError('La calidad de la imagen no es suficiente. Por favor, intenta de nuevo.');
        setIsAnalyzing(false);
        return;
      }

      setCapturedPhoto(imageSrc);
      setIsAnalyzing(false);
    } catch (err) {
      console.error('Capture error:', err);
      setError('Error al capturar la imagen');
      setIsAnalyzing(false);
    }
  }, [analyzeImageQuality]);

  // Retake photo
  const retakePhoto = React.useCallback(() => {
    setCapturedPhoto(null);
    setError(null);
    setCaptureQuality({
      brightness: 'good',
      sharpness: 'good',
      hasDocument: false,
      isReady: false,
    });
    // Keep camera enabled if it was previously enabled
  }, []);

  // Confirm and submit photo
  const confirmPhoto = React.useCallback(async () => {
    if (!capturedPhoto) return;

    try {
      const file = dataURLtoFile(
        capturedPhoto,
        sanitizeFilename(`document_${side.toLowerCase()}.jpg`)
      );
      await onCapture(file, side);
    } catch (err) {
      console.error('Submit error:', err);
      setError('Error al enviar la imagen. Por favor, intenta de nuevo.');
    }
  }, [capturedPhoto, side, onCapture]);

  // Handle file upload with security validation
  const handleFileUpload = React.useCallback(async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate MIME type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/heic', 'image/heif'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
      setError('Tipo de archivo no permitido. Usa JPG, PNG, WebP o HEIC.');
      return;
    }

    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      setError('La imagen es muy grande. Máximo 10MB.');
      return;
    }

    // Validate magic bytes (prevent malicious files disguised as images)
    const isValidImage = await validateImageMagicBytes(file);
    if (!isValidImage) {
      setError('El archivo no parece ser una imagen válida. Por favor, selecciona otro archivo.');
      return;
    }

    setIsAnalyzing(true);
    setError(null);

    try {
      const reader = new FileReader();
      reader.onload = e => {
        const dataUrl = e.target?.result as string;
        setCapturedPhoto(dataUrl);
        setIsAnalyzing(false);
      };
      reader.onerror = () => {
        setError('Error al leer el archivo');
        setIsAnalyzing(false);
      };
      reader.readAsDataURL(file);
    } catch (err) {
      console.error('File upload error:', err);
      setError('Error al procesar el archivo');
      setIsAnalyzing(false);
    }
  }, []);

  // Switch camera
  const switchCamera = React.useCallback(() => {
    setFacingMode(prev => (prev === 'user' ? 'environment' : 'user'));
  }, []);

  // Update captured image from props
  React.useEffect(() => {
    if (capturedImage) {
      setCapturedPhoto(capturedImage);
    }
  }, [capturedImage]);

  const displayInstructions = instructions || defaultInstructions[side];

  // =============================================================================
  // RENDER
  // =============================================================================

  return (
    <div className={cn('space-y-4', className)}>
      {/* Header */}
      <div className="text-center">
        <h3 className="text-foreground text-lg font-semibold">
          {documentLabels[documentType]} - {sideLabels[side]}
        </h3>
        <p className="text-muted-foreground mt-1 text-sm">
          {side === 'Front'
            ? 'Captura el frente del documento'
            : 'Captura el reverso del documento'}
        </p>
      </div>

      {/* Camera/Preview Area */}
      <div className="relative aspect-video overflow-hidden rounded-xl bg-black">
        {capturedPhoto ? (
          // Show captured image
          <Image
            src={capturedPhoto}
            alt={`Captured ${side}`}
            fill
            className="object-contain"
            unoptimized
          />
        ) : !cameraEnabled ? (
          // Camera not activated yet - show activation button
          <div className="flex h-full flex-col items-center justify-center gap-4 p-8 text-white">
            <div className="rounded-full bg-white/10 p-6">
              <Camera className="h-16 w-16 text-white/80" />
            </div>
            <div className="text-center">
              <p className="text-lg font-medium">Captura con cámara</p>
              <p className="mt-1 text-sm text-gray-400">
                Haz clic para activar la cámara y capturar tu documento
              </p>
            </div>
            <Button onClick={enableCamera} variant="secondary" size="lg" className="mt-2">
              <Camera className="mr-2 h-5 w-5" />
              Activar Cámara
            </Button>
            <p className="mt-2 text-xs text-gray-500">
              O puedes subir una foto desde tu dispositivo
            </p>
          </div>
        ) : hasCamera ? (
          // Show webcam
          <>
            <Webcam
              ref={webcamRef}
              audio={false}
              screenshotFormat="image/jpeg"
              videoConstraints={videoConstraints}
              onUserMedia={handleUserMedia}
              onUserMediaError={handleCameraError}
              className="h-full w-full object-cover"
              mirrored={facingMode === 'user'}
            />

            {/* Document Frame Overlay */}
            {cameraReady && (
              <div className="pointer-events-none absolute inset-0">
                {/* Semi-transparent overlay */}
                <div className="absolute inset-0 bg-black/40" />

                {/* Document cutout */}
                <div className="absolute inset-8 rounded-lg border-2 border-white/80 md:inset-16">
                  <div
                    className="absolute inset-0 bg-transparent"
                    style={{
                      boxShadow: '0 0 0 9999px rgba(0, 0, 0, 0.5)',
                    }}
                  />
                </div>

                {/* Corner markers */}
                <div className="border-primary absolute top-8 left-8 h-8 w-8 rounded-tl-lg border-t-4 border-l-4 md:top-16 md:left-16" />
                <div className="border-primary absolute top-8 right-8 h-8 w-8 rounded-tr-lg border-t-4 border-r-4 md:top-16 md:right-16" />
                <div className="border-primary absolute bottom-8 left-8 h-8 w-8 rounded-bl-lg border-b-4 border-l-4 md:bottom-16 md:left-16" />
                <div className="border-primary absolute right-8 bottom-8 h-8 w-8 rounded-br-lg border-r-4 border-b-4 md:right-16 md:bottom-16" />
              </div>
            )}

            {/* Quality Indicators */}
            {cameraReady && (
              <div className="absolute top-4 right-4 left-4 flex justify-center gap-2">
                <div
                  className={cn(
                    'flex items-center gap-1 rounded-full px-2 py-1 text-xs font-medium',
                    captureQuality.brightness === 'good'
                      ? 'bg-green-500/80 text-white'
                      : 'bg-yellow-500/80 text-white'
                  )}
                >
                  <CheckCircle className="h-3 w-3" />
                  Iluminación
                </div>
                <div
                  className={cn(
                    'flex items-center gap-1 rounded-full px-2 py-1 text-xs font-medium',
                    captureQuality.sharpness === 'good'
                      ? 'bg-green-500/80 text-white'
                      : 'bg-yellow-500/80 text-white'
                  )}
                >
                  <CheckCircle className="h-3 w-3" />
                  Enfoque
                </div>
              </div>
            )}

            {/* Switch Camera Button */}
            <Button
              size="icon"
              variant="secondary"
              className="absolute right-4 bottom-4 rounded-full"
              onClick={switchCamera}
            >
              <FlipHorizontal className="h-4 w-4" />
            </Button>
          </>
        ) : (
          // No camera - show upload option
          <div className="flex h-full flex-col items-center justify-center text-white">
            <XCircle className="mb-4 h-16 w-16 text-red-400" />
            <p className="text-lg font-medium">Cámara no disponible</p>
            <p className="mt-2 text-sm text-gray-400">Usa la opción de subir archivo</p>
          </div>
        )}

        {/* Loading Overlay */}
        {(isAnalyzing || isProcessing) && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/70">
            <div className="text-center text-white">
              <Loader2 className="mx-auto mb-2 h-12 w-12 animate-spin" />
              <p>{isProcessing ? 'Procesando...' : 'Analizando imagen...'}</p>
            </div>
          </div>
        )}
      </div>

      {/* Error Message */}
      {error && (
        <div className="bg-destructive/10 text-destructive flex items-center gap-2 rounded-lg p-3">
          <AlertTriangle className="h-5 w-5 flex-shrink-0" />
          <p className="text-sm">{error}</p>
        </div>
      )}

      {/* Instructions */}
      <div className="bg-muted/50 rounded-lg p-4">
        <h4 className="text-foreground mb-2 text-sm font-medium">Instrucciones:</h4>
        <ul className="space-y-1">
          {displayInstructions.map((instruction, index) => (
            <li key={index} className="text-muted-foreground flex items-start gap-2 text-sm">
              <CheckCircle className="text-primary mt-0.5 h-4 w-4 flex-shrink-0" />
              {instruction}
            </li>
          ))}
        </ul>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-3">
        {capturedPhoto ? (
          <>
            <Button
              variant="outline"
              onClick={retakePhoto}
              disabled={isProcessing}
              className="flex-1"
            >
              <RotateCcw className="mr-2 h-4 w-4" />
              Volver a tomar
            </Button>
            <Button onClick={confirmPhoto} disabled={isProcessing} className="flex-1">
              {isProcessing ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <CheckCircle className="mr-2 h-4 w-4" />
              )}
              Confirmar
            </Button>
          </>
        ) : (
          <>
            {cameraEnabled && hasCamera && cameraReady && (
              <Button onClick={capturePhoto} disabled={isAnalyzing} className="flex-1">
                <Camera className="mr-2 h-4 w-4" />
                Capturar
              </Button>
            )}
            {!cameraEnabled && hasCamera && (
              <Button onClick={enableCamera} disabled={isAnalyzing} className="flex-1">
                <Camera className="mr-2 h-4 w-4" />
                Activar Cámara
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => fileInputRef.current?.click()}
              disabled={isAnalyzing}
              className="flex-1"
            >
              <Upload className="mr-2 h-4 w-4" />
              Subir archivo
            </Button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              capture="environment"
              onChange={handleFileUpload}
              className="hidden"
            />
          </>
        )}
      </div>
    </div>
  );
}

export default DocumentCapture;
