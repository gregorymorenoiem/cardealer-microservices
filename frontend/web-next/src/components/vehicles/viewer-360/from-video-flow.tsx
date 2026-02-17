'use client';

import { useState, useCallback, useRef } from 'react';
import { Upload, Video, Loader2, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';

// ============================================================
// TYPES
// ============================================================

interface FromVideoFlowProps {
  vehicleId: string;
  onVideoUploaded: (jobId: string) => void;
  className?: string;
}

// ============================================================
// CONSTANTS
// ============================================================

const MAX_VIDEO_SIZE = 500 * 1024 * 1024; // 500MB
const ACCEPTED_VIDEO_TYPES = ['video/mp4', 'video/quicktime', 'video/webm'];

// ============================================================
// COMPONENT
// ============================================================

export function Viewer360FromVideoFlow({
  vehicleId,
  onVideoUploaded,
  className = '',
}: FromVideoFlowProps) {
  const [videoFile, setVideoFile] = useState<File | null>(null);
  const [videoPreviewUrl, setVideoPreviewUrl] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate
    if (!ACCEPTED_VIDEO_TYPES.includes(file.type)) {
      toast.error('Formato de video no soportado. Usa MP4, MOV o WebM.');
      return;
    }
    if (file.size > MAX_VIDEO_SIZE) {
      toast.error('El video es demasiado grande. Máximo 500MB.');
      return;
    }

    setError(null);
    setVideoFile(file);

    // Preview
    const url = URL.createObjectURL(file);
    setVideoPreviewUrl(url);
  }, []);

  const handleUpload = useCallback(async () => {
    if (!videoFile) return;
    setIsUploading(true);
    setError(null);
    setUploadProgress(0);

    try {
      // Dynamic import to avoid SSR issues
      const { vehicle360Service } = await import('@/services/vehicle360');
      const response = await vehicle360Service.uploadVideo(
        videoFile,
        vehicleId,
        (progress) => setUploadProgress(progress)
      );

      if (response.jobId) {
        onVideoUploaded(response.jobId);
        toast.success('Video subido correctamente. Procesando...');
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Error al subir video';
      setError(message);
      toast.error(message);
    } finally {
      setIsUploading(false);
    }
  }, [videoFile, vehicleId, onVideoUploaded]);

  const handleRemoveVideo = useCallback(() => {
    setVideoFile(null);
    if (videoPreviewUrl) URL.revokeObjectURL(videoPreviewUrl);
    setVideoPreviewUrl(null);
    setError(null);
    setUploadProgress(0);
  }, [videoPreviewUrl]);

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Instructions */}
      <div className="rounded-lg border border-blue-200 bg-blue-50 p-3">
        <h4 className="text-sm font-semibold text-blue-900">
          📹 Cómo grabar tu video 360°
        </h4>
        <ul className="mt-2 space-y-1 text-xs text-blue-700">
          <li>• Graba caminando alrededor del vehículo en un círculo completo</li>
          <li>• Mantén el teléfono a la altura del centro del vehículo</li>
          <li>• Camina despacio y constante (30-60 segundos)</li>
          <li>• Buena iluminación, sin sombras fuertes</li>
          <li>• Formatos: MP4, MOV o WebM (máximo 500MB)</li>
        </ul>
      </div>

      {/* Upload area or preview */}
      {!videoFile ? (
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          className="flex w-full flex-col items-center justify-center rounded-xl border-2 border-dashed border-gray-300 p-10 transition-all hover:border-emerald-400 hover:bg-emerald-50"
        >
          <Video className="h-10 w-10 text-gray-400" />
          <span className="mt-2 text-sm font-medium text-gray-700">
            Seleccionar video 360°
          </span>
          <span className="mt-1 text-xs text-gray-500">
            MP4, MOV, WebM · Máximo 500MB
          </span>
        </button>
      ) : (
        <div className="space-y-3">
          {/* Video preview */}
          {videoPreviewUrl && (
            <div className="relative overflow-hidden rounded-xl bg-black">
              <video
                src={videoPreviewUrl}
                controls
                className="mx-auto max-h-[300px] w-full"
                preload="metadata"
              />
            </div>
          )}

          {/* File info */}
          <div className="flex items-center justify-between rounded-lg bg-gray-50 px-3 py-2">
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-gray-900">
                {videoFile.name}
              </p>
              <p className="text-xs text-gray-500">
                {(videoFile.size / (1024 * 1024)).toFixed(1)} MB
              </p>
            </div>
            <button
              type="button"
              onClick={handleRemoveVideo}
              disabled={isUploading}
              className="ml-2 text-sm text-red-500 hover:text-red-700 disabled:opacity-50"
            >
              Eliminar
            </button>
          </div>

          {/* Error */}
          {error && (
            <div className="flex items-center gap-2 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-600">
              <AlertTriangle className="h-4 w-4 flex-shrink-0" />
              {error}
            </div>
          )}

          {/* Upload progress */}
          {isUploading && (
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <Loader2 className="h-4 w-4 animate-spin text-emerald-500" />
                <span className="text-sm text-gray-600">
                  Subiendo... {uploadProgress}%
                </span>
              </div>
              <div className="h-2 w-full overflow-hidden rounded-full bg-gray-200">
                <div
                  className="h-full rounded-full bg-emerald-500 transition-all"
                  style={{ width: `${uploadProgress}%` }}
                />
              </div>
            </div>
          )}

          {/* Upload button */}
          {!isUploading && (
            <button
              type="button"
              onClick={handleUpload}
              className="flex w-full items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-emerald-700"
            >
              <Upload className="h-4 w-4" />
              Subir y procesar video
            </button>
          )}
        </div>
      )}

      <input
        ref={fileInputRef}
        type="file"
        accept="video/mp4,video/quicktime,video/webm"
        onChange={handleFileSelect}
        className="hidden"
      />
    </div>
  );
}
