'use client';

import { useState, useRef, useCallback } from 'react';
import Webcam from 'react-webcam';
import type { SmartVinDecodeResult } from '@/services/vehicles';
import { useDecodeVin } from '@/hooks/use-vehicles';
import { sanitizeVIN } from '@/lib/security/sanitize';
import { Camera, Upload, X, RotateCcw, Loader2, AlertTriangle, ScanLine } from 'lucide-react';

// ============================================================
// Component
// ============================================================

interface VinScannerProps {
  onDecoded: (result: SmartVinDecodeResult) => void;
  onCancel: () => void;
}

export function VinScanner({ onDecoded, onCancel }: VinScannerProps) {
  const webcamRef = useRef<Webcam>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isCapturing, setIsCapturing] = useState(true);
  const [extractedVin, setExtractedVin] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [shouldDecode, setShouldDecode] = useState(false);
  const [facingMode, setFacingMode] = useState<'environment' | 'user'>('environment');

  const sanitized = sanitizeVIN(extractedVin);

  // VIN decode
  const { data: decodeResult, isFetching: isDecoding } = useDecodeVin(sanitized, {
    enabled: shouldDecode && sanitized.length === 17,
  });

  // When decode completes
  if (decodeResult && shouldDecode) {
    setShouldDecode(false);
    onDecoded(decodeResult);
  }

  const processImage = useCallback(async (imageSrc: string) => {
    setIsProcessing(true);
    setError(null);

    try {
      // Dynamically import Tesseract.js only when needed
      const Tesseract = await import('tesseract.js');
      const { data } = await Tesseract.recognize(imageSrc, 'eng', {
        logger: () => {}, // suppress logs
      });

      // Extract potential VIN from OCR text
      const text = data.text.toUpperCase().replace(/\s+/g, '');
      // Look for 17 consecutive valid VIN characters
      const vinPattern = /[A-HJ-NPR-Z0-9]{17}/;
      const match = text.match(vinPattern);

      if (match) {
        setExtractedVin(match[0]);
        setIsCapturing(false);
      } else {
        // Try looser matching - find longest valid VIN sequence
        const chars = text.replace(/[^A-HJ-NPR-Z0-9]/g, '');
        if (chars.length >= 17) {
          setExtractedVin(chars.slice(0, 17));
          setIsCapturing(false);
        } else {
          setError('No se detectó un VIN en la imagen. Intenta con mejor iluminación o más cerca.');
        }
      }
    } catch {
      setError('Error al procesar la imagen. Intenta de nuevo.');
    } finally {
      setIsProcessing(false);
    }
  }, []);

  const handleCapture = useCallback(() => {
    const imageSrc = webcamRef.current?.getScreenshot();
    if (imageSrc) {
      processImage(imageSrc);
    }
  }, [processImage]);

  const handleFileUpload = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) return;

      const reader = new FileReader();
      reader.onload = () => {
        if (reader.result) {
          processImage(reader.result as string);
        }
      };
      reader.readAsDataURL(file);
      e.target.value = '';
    },
    [processImage]
  );

  const handleRetry = useCallback(() => {
    setExtractedVin('');
    setError(null);
    setIsCapturing(true);
    setShouldDecode(false);
  }, []);

  const handleConfirmVin = useCallback(() => {
    if (sanitized.length === 17) {
      setShouldDecode(true);
    }
  }, [sanitized]);

  return (
    <div className="space-y-4">
      <div className="text-center">
        <h2 className="text-xl font-bold text-gray-900">Escanear VIN</h2>
        <p className="mt-1 text-sm text-gray-500">Apunta la cámara al número VIN del vehículo</p>
      </div>

      {isCapturing ? (
        <>
          {/* Camera View */}
          <div className="relative aspect-video overflow-hidden rounded-xl bg-black">
            <Webcam
              ref={webcamRef}
              audio={false}
              screenshotFormat="image/jpeg"
              videoConstraints={{
                facingMode,
                width: 1280,
                height: 720,
              }}
              className="h-full w-full object-cover"
              onUserMediaError={() => setError('No se pudo acceder a la cámara')}
            />
            {/* Guide overlay */}
            <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
              <div className="relative h-16 w-[80%] rounded-lg border-2 border-emerald-400">
                <ScanLine className="absolute -top-3 left-1/2 h-6 w-6 -translate-x-1/2 animate-pulse text-emerald-400" />
                <p className="absolute -bottom-6 left-1/2 -translate-x-1/2 rounded bg-black/50 px-2 py-0.5 text-xs whitespace-nowrap text-white">
                  Alinea el VIN dentro del recuadro
                </p>
              </div>
            </div>

            {isProcessing && (
              <div className="absolute inset-0 flex items-center justify-center bg-black/60">
                <div className="flex flex-col items-center gap-2">
                  <Loader2 className="h-8 w-8 animate-spin text-white" />
                  <p className="text-sm text-white">Procesando imagen...</p>
                </div>
              </div>
            )}
          </div>

          {/* Camera Controls */}
          <div className="flex items-center justify-center gap-4">
            <button
              onClick={() => setFacingMode(f => (f === 'environment' ? 'user' : 'environment'))}
              className="flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
            >
              <RotateCcw className="h-4 w-4" />
              Cambiar cámara
            </button>
            <button
              onClick={handleCapture}
              disabled={isProcessing}
              className="flex items-center gap-2 rounded-xl bg-emerald-600 px-6 py-3 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            >
              <Camera className="h-4 w-4" />
              Capturar
            </button>
            <button
              onClick={() => fileInputRef.current?.click()}
              className="flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
            >
              <Upload className="h-4 w-4" />
              Subir foto
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={handleFileUpload}
            />
          </div>
        </>
      ) : (
        /* VIN Confirmation */
        <div className="space-y-4">
          <div className="rounded-xl border-2 border-emerald-200 bg-emerald-50 p-6 text-center">
            <p className="mb-2 text-xs font-medium tracking-wider text-emerald-600 uppercase">
              VIN Detectado
            </p>
            <input
              type="text"
              value={extractedVin}
              onChange={e =>
                setExtractedVin(
                  e.target.value
                    .toUpperCase()
                    .replace(/[^A-HJ-NPR-Z0-9]/gi, '')
                    .slice(0, 17)
                )
              }
              maxLength={17}
              className="w-full border-0 bg-transparent text-center font-mono text-2xl tracking-[0.2em] text-gray-900 focus:outline-none"
            />
            <p className="mt-2 text-xs text-gray-500">
              Verifica y corrige si es necesario ({extractedVin.length}/17 caracteres)
            </p>
          </div>

          <div className="flex gap-3">
            <button
              onClick={handleRetry}
              className="flex flex-1 items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              <RotateCcw className="h-4 w-4" />
              Escanear de nuevo
            </button>
            <button
              onClick={handleConfirmVin}
              disabled={sanitized.length !== 17 || isDecoding}
              className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-3 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            >
              {isDecoding ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
              {isDecoding ? 'Decodificando...' : 'Confirmar y Decodificar'}
            </button>
          </div>
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-700">
          <AlertTriangle className="h-4 w-4 flex-shrink-0" />
          {error}
        </div>
      )}

      {/* Cancel */}
      <div className="text-center">
        <button onClick={onCancel} className="text-sm text-gray-500 hover:text-gray-700">
          <X className="mr-1 inline h-4 w-4" />
          Cancelar escaneo
        </button>
      </div>
    </div>
  );
}
