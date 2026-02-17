import React, { useRef, useState, useCallback, useEffect } from 'react';
import Webcam from 'react-webcam';
import {
  Camera,
  RotateCcw,
  CheckCircle,
  XCircle,
  AlertTriangle,
  CreditCard,
  Upload,
  Loader2,
} from 'lucide-react';

// ===== Types =====
export type DocumentSide = 'Front' | 'Back';

interface DocumentCaptureProps {
  side: DocumentSide;
  documentType?: 'Cedula' | 'Passport' | 'DriverLicense';
  onCapture: (image: File, side: DocumentSide) => Promise<void>;
  onError?: (error: string) => void;
  isProcessing?: boolean;
  capturedImage?: string | null;
  instructions?: string[];
}

interface CaptureQuality {
  brightness: 'good' | 'low' | 'high';
  sharpness: 'good' | 'blurry';
  hasDocument: boolean;
  isReady: boolean;
}

// ===== Helper Functions =====
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

// ===== Component =====
export function DocumentCapture({
  side,
  documentType = 'Cedula',
  onCapture,
  onError,
  isProcessing = false,
  capturedImage = null,
  instructions,
}: DocumentCaptureProps) {
  const webcamRef = useRef<Webcam>(null);
  const [hasCamera, setHasCamera] = useState(true);
  const [cameraReady, setCameraReady] = useState(false);
  const [capturedPhoto, setCapturedPhoto] = useState<string | null>(capturedImage);
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [captureQuality, setCaptureQuality] = useState<CaptureQuality>({
    brightness: 'good',
    sharpness: 'good',
    hasDocument: false,
    isReady: false,
  });
  const [error, setError] = useState<string | null>(null);
  const [facingMode, setFacingMode] = useState<'user' | 'environment'>('environment');

  // Default instructions based on document side
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

  // Video constraints optimized for document capture
  const videoConstraints = {
    width: { ideal: 1920, min: 1280 },
    height: { ideal: 1080, min: 720 },
    facingMode: facingMode,
    aspectRatio: { ideal: 16 / 9 },
  };

  // Handle camera errors
  const handleCameraError = useCallback(
    (err: string | DOMException) => {
      console.error('Camera error:', err);
      setHasCamera(false);
      setError(
        'No se pudo acceder a la cámara. Por favor, permite el acceso o usa la opción de subir archivo.'
      );
      onError?.('Camera access denied');
    },
    [onError]
  );

  // Handle camera ready
  const handleUserMedia = useCallback(() => {
    setCameraReady(true);
    setError(null);
  }, []);

  // Analyze image quality (simplified version)
  const analyzeImageQuality = useCallback((imageData: string): CaptureQuality => {
    // In a real implementation, this would use computer vision
    // For now, we'll return optimistic values
    return {
      brightness: 'good',
      sharpness: 'good',
      hasDocument: true,
      isReady: true,
    };
  }, []);

  // Capture photo from webcam
  const capturePhoto = useCallback(async () => {
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

      // Analyze quality
      const quality = analyzeImageQuality(imageSrc);
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
  const retakePhoto = useCallback(() => {
    setCapturedPhoto(null);
    setError(null);
    setCaptureQuality({
      brightness: 'good',
      sharpness: 'good',
      hasDocument: false,
      isReady: false,
    });
  }, []);

  // Confirm and submit photo
  const confirmPhoto = useCallback(async () => {
    if (!capturedPhoto) return;

    try {
      const file = dataURLtoFile(capturedPhoto, `document_${side.toLowerCase()}.jpg`);
      await onCapture(file, side);
    } catch (err) {
      console.error('Submit error:', err);
      setError('Error al enviar la imagen. Por favor, intenta de nuevo.');
    }
  }, [capturedPhoto, side, onCapture]);

  // Handle file upload as fallback
  const handleFileUpload = useCallback(async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      setError('Por favor, selecciona una imagen válida');
      return;
    }

    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      setError('La imagen es muy grande. Máximo 10MB.');
      return;
    }

    setIsAnalyzing(true);
    setError(null);

    try {
      // Read file as data URL for preview
      const reader = new FileReader();
      reader.onload = async (e) => {
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

  // Switch camera (front/back)
  const switchCamera = useCallback(() => {
    setFacingMode((prev) => (prev === 'user' ? 'environment' : 'user'));
  }, []);

  // Update captured image from props
  useEffect(() => {
    if (capturedImage) {
      setCapturedPhoto(capturedImage);
    }
  }, [capturedImage]);

  // Render quality indicator
  const renderQualityIndicator = () => {
    if (!cameraReady) return null;

    const items = [
      {
        label: 'Iluminación',
        status: captureQuality.brightness === 'good' ? 'good' : 'bad',
        icon: captureQuality.brightness === 'good' ? CheckCircle : AlertTriangle,
      },
      {
        label: 'Enfoque',
        status: captureQuality.sharpness === 'good' ? 'good' : 'bad',
        icon: captureQuality.sharpness === 'good' ? CheckCircle : AlertTriangle,
      },
      {
        label: 'Documento',
        status: captureQuality.hasDocument ? 'good' : 'waiting',
        icon: captureQuality.hasDocument ? CheckCircle : CreditCard,
      },
    ];

    return (
      <div className="absolute top-4 left-4 right-4 flex justify-center gap-4">
        {items.map((item) => (
          <div
            key={item.label}
            className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
              item.status === 'good'
                ? 'bg-green-500/80 text-white'
                : item.status === 'bad'
                  ? 'bg-red-500/80 text-white'
                  : 'bg-gray-500/80 text-white'
            }`}
          >
            <item.icon className="w-3 h-3" />
            {item.label}
          </div>
        ))}
      </div>
    );
  };

  // Render document frame overlay
  const renderDocumentFrame = () => {
    return (
      <div className="absolute inset-0 pointer-events-none">
        {/* Semi-transparent overlay */}
        <div className="absolute inset-0 bg-black/40" />

        {/* Document frame cutout */}
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="relative w-[85%] max-w-md aspect-[1.586/1] rounded-lg overflow-hidden">
            {/* Clear area for document */}
            <div className="absolute inset-0 bg-transparent border-4 border-white/80 rounded-lg shadow-lg" />

            {/* Corner indicators */}
            <div className="absolute top-0 left-0 w-8 h-8 border-t-4 border-l-4 border-blue-500 rounded-tl-lg" />
            <div className="absolute top-0 right-0 w-8 h-8 border-t-4 border-r-4 border-blue-500 rounded-tr-lg" />
            <div className="absolute bottom-0 left-0 w-8 h-8 border-b-4 border-l-4 border-blue-500 rounded-bl-lg" />
            <div className="absolute bottom-0 right-0 w-8 h-8 border-b-4 border-r-4 border-blue-500 rounded-br-lg" />

            {/* Label */}
            <div className="absolute -top-8 left-1/2 -translate-x-1/2 bg-blue-600 text-white px-3 py-1 rounded-full text-sm font-medium whitespace-nowrap">
              {sideLabels[side]} del {documentLabels[documentType]}
            </div>
          </div>
        </div>
      </div>
    );
  };

  // Render captured preview
  if (capturedPhoto) {
    return (
      <div className="w-full max-w-2xl mx-auto">
        {/* Header */}
        <div className="mb-4 text-center">
          <h3 className="text-lg font-semibold text-gray-900">
            {sideLabels[side]} del {documentLabels[documentType]}
          </h3>
          <p className="text-sm text-gray-600">
            Verifica que la imagen sea clara y todos los datos sean legibles
          </p>
        </div>

        {/* Preview */}
        <div className="relative aspect-[16/9] bg-gray-900 rounded-xl overflow-hidden shadow-lg">
          <img
            src={capturedPhoto}
            alt={`${side} del documento`}
            className="w-full h-full object-contain"
          />

          {/* Processing overlay */}
          {isProcessing && (
            <div className="absolute inset-0 bg-black/50 flex flex-col items-center justify-center">
              <Loader2 className="w-12 h-12 text-white animate-spin mb-4" />
              <p className="text-white font-medium">Procesando documento...</p>
              <p className="text-white/70 text-sm">Extrayendo datos con OCR</p>
            </div>
          )}
        </div>

        {/* Error message */}
        {error && (
          <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
            <XCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-700">{error}</p>
          </div>
        )}

        {/* Action buttons */}
        {!isProcessing && (
          <div className="mt-6 flex gap-4">
            <button
              onClick={retakePhoto}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors"
            >
              <RotateCcw className="w-5 h-5" />
              Tomar de nuevo
            </button>
            <button
              onClick={confirmPhoto}
              disabled={isProcessing}
              className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
            >
              <CheckCircle className="w-5 h-5" />
              Confirmar
            </button>
          </div>
        )}
      </div>
    );
  }

  // Render camera view
  return (
    <div className="w-full max-w-2xl mx-auto">
      {/* Header */}
      <div className="mb-4 text-center">
        <h3 className="text-lg font-semibold text-gray-900">
          Captura {sideLabels[side]} del {documentLabels[documentType]}
        </h3>
        <p className="text-sm text-gray-600">Posiciona el documento dentro del marco</p>
      </div>

      {/* Instructions */}
      <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <h4 className="text-sm font-medium text-blue-800 mb-2">📋 Instrucciones:</h4>
        <ul className="text-sm text-blue-700 space-y-1">
          {(instructions || defaultInstructions[side]).map((instruction, index) => (
            <li key={index} className="flex items-start gap-2">
              <span className="text-blue-500">•</span>
              {instruction}
            </li>
          ))}
        </ul>
      </div>

      {/* Camera or fallback */}
      {hasCamera ? (
        <div className="relative aspect-[16/9] bg-gray-900 rounded-xl overflow-hidden shadow-lg">
          {/* Webcam */}
          <Webcam
            ref={webcamRef}
            audio={false}
            screenshotFormat="image/jpeg"
            screenshotQuality={0.95}
            videoConstraints={videoConstraints}
            onUserMedia={handleUserMedia}
            onUserMediaError={handleCameraError}
            className="w-full h-full object-cover"
            mirrored={facingMode === 'user'}
          />

          {/* Loading state */}
          {!cameraReady && (
            <div className="absolute inset-0 flex flex-col items-center justify-center bg-gray-900">
              <Loader2 className="w-12 h-12 text-white animate-spin mb-4" />
              <p className="text-white">Iniciando cámara...</p>
            </div>
          )}

          {/* Document frame overlay */}
          {cameraReady && renderDocumentFrame()}

          {/* Quality indicators */}
          {cameraReady && renderQualityIndicator()}

          {/* Analyzing overlay */}
          {isAnalyzing && (
            <div className="absolute inset-0 bg-black/50 flex flex-col items-center justify-center">
              <Loader2 className="w-12 h-12 text-white animate-spin mb-4" />
              <p className="text-white font-medium">Analizando imagen...</p>
            </div>
          )}
        </div>
      ) : (
        <div className="aspect-[16/9] bg-gray-100 rounded-xl flex flex-col items-center justify-center p-8">
          <CreditCard className="w-16 h-16 text-gray-400 mb-4" />
          <p className="text-gray-600 text-center mb-4">
            No se pudo acceder a la cámara. Puedes subir una foto del documento.
          </p>
          <label className="cursor-pointer inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors">
            <Upload className="w-5 h-5" />
            Subir imagen
            <input
              type="file"
              accept="image/*"
              capture="environment"
              onChange={handleFileUpload}
              className="hidden"
            />
          </label>
        </div>
      )}

      {/* Error message */}
      {error && (
        <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-start gap-2">
          <XCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      {/* Action buttons */}
      <div className="mt-6 flex gap-4">
        {/* File upload as alternative */}
        <label className="flex-1 cursor-pointer">
          <div className="flex items-center justify-center gap-2 px-4 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors">
            <Upload className="w-5 h-5" />
            Subir archivo
          </div>
          <input type="file" accept="image/*" onChange={handleFileUpload} className="hidden" />
        </label>

        {/* Capture button */}
        {hasCamera && cameraReady && (
          <button
            onClick={capturePhoto}
            disabled={isAnalyzing || isProcessing}
            className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
          >
            <Camera className="w-5 h-5" />
            Capturar
          </button>
        )}

        {/* Switch camera button (mobile) */}
        {hasCamera && cameraReady && (
          <button
            onClick={switchCamera}
            className="px-4 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg transition-colors"
            title="Cambiar cámara"
          >
            <RotateCcw className="w-5 h-5" />
          </button>
        )}
      </div>
    </div>
  );
}

export default DocumentCapture;
