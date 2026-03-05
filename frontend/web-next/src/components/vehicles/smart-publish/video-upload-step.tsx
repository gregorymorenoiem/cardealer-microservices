/**
 * Video Upload Step — Smart Publish Wizard
 *
 * Allows users to upload a video walkthrough of their vehicle.
 * Feature is gated by subscription plan (maxVideos).
 * - Plans with maxVideos: 0 → show upgrade CTA
 * - Plans with maxVideos >= 1 → show upload area
 *
 * Supported formats: MP4, MOV, WebM (max 500 MB)
 */

'use client';

import { useState, useCallback, useRef } from 'react';
import { Video, Upload, Play, Trash2, Crown, Lock } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { toast } from 'sonner';
import {
  type DealerPlan,
  type SellerPlan,
  DEALER_PLAN_LIMITS,
  SELLER_PLAN_LIMITS,
} from '@/lib/plan-config';

// ============================================================
// TYPES
// ============================================================

interface VideoUploadStepProps {
  accountType: 'individual' | 'dealer';
  dealerPlan?: DealerPlan;
  sellerPlan?: SellerPlan;
  videoUrl?: string;
  onVideoUploaded: (url: string, thumbnailUrl?: string) => void;
  onSkip: () => void;
  onComplete: () => void;
}

const ACCEPTED_VIDEO_TYPES = ['video/mp4', 'video/quicktime', 'video/webm', 'video/mov'];
const MAX_VIDEO_SIZE_MB = 500;
const MAX_VIDEO_SIZE_BYTES = MAX_VIDEO_SIZE_MB * 1024 * 1024;

// ============================================================
// HELPERS
// ============================================================

function getMaxVideos(
  accountType: 'individual' | 'dealer',
  dealerPlan?: DealerPlan,
  sellerPlan?: SellerPlan
): number {
  if (accountType === 'dealer' && dealerPlan) {
    return DEALER_PLAN_LIMITS[dealerPlan]?.maxVideos ?? 0;
  }
  if (sellerPlan) {
    return SELLER_PLAN_LIMITS[sellerPlan]?.maxVideos ?? 0;
  }
  // Default: free plan
  return 0;
}

// ============================================================
// MAIN COMPONENT
// ============================================================

export function VideoUploadStep({
  accountType,
  dealerPlan,
  sellerPlan,
  videoUrl: existingVideoUrl,
  onVideoUploaded,
  onSkip,
  onComplete,
}: VideoUploadStepProps) {
  const [videoFile, setVideoFile] = useState<File | null>(null);
  const [videoPreviewUrl, setVideoPreviewUrl] = useState<string | null>(existingVideoUrl || null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const maxVideos = getMaxVideos(accountType, dealerPlan, sellerPlan);

  // ============================================================
  // HANDLERS
  // ============================================================

  const validateFile = useCallback((file: File): boolean => {
    if (!ACCEPTED_VIDEO_TYPES.includes(file.type)) {
      toast.error('Formato no soportado. Usa MP4, MOV o WebM.');
      return false;
    }
    if (file.size > MAX_VIDEO_SIZE_BYTES) {
      toast.error(`El video no debe exceder ${MAX_VIDEO_SIZE_MB} MB.`);
      return false;
    }
    return true;
  }, []);

  const handleUpload = useCallback(
    async (file: File) => {
      if (!validateFile(file)) return;

      setVideoFile(file);
      setVideoPreviewUrl(URL.createObjectURL(file));
      setIsUploading(true);
      setUploadProgress(0);

      try {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('mediaType', 'video');

        const xhr = new XMLHttpRequest();

        await new Promise<void>((resolve, reject) => {
          xhr.upload.addEventListener('progress', e => {
            if (e.lengthComputable) {
              const pct = Math.round((e.loaded / e.total) * 100);
              setUploadProgress(pct);
            }
          });

          xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
              try {
                const response = JSON.parse(xhr.responseText);
                const url = response.data?.url || response.url || '';
                const thumbnailUrl = response.data?.thumbnailUrl || response.thumbnailUrl || '';
                onVideoUploaded(url, thumbnailUrl);
                toast.success('¡Video subido exitosamente!');
              } catch {
                onVideoUploaded('', '');
                toast.success('Video subido.');
              }
              resolve();
            } else {
              reject(new Error(`Upload failed: ${xhr.status}`));
            }
          });

          xhr.addEventListener('error', () => reject(new Error('Network error')));
          xhr.addEventListener('abort', () => reject(new Error('Upload aborted')));

          xhr.open('POST', '/api/media/upload');
          xhr.send(formData);
        });

        setUploadProgress(100);
      } catch {
        toast.error('Error al subir el video. Intenta de nuevo.');
        setVideoFile(null);
        setVideoPreviewUrl(null);
        setUploadProgress(0);
      } finally {
        setIsUploading(false);
      }
    },
    [validateFile, onVideoUploaded]
  );

  const handleFileChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) handleUpload(file);
    },
    [handleUpload]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent<HTMLLabelElement>) => {
      e.preventDefault();
      const file = e.dataTransfer.files?.[0];
      if (file) handleUpload(file);
    },
    [handleUpload]
  );

  const handleDragOver = useCallback((e: React.DragEvent<HTMLLabelElement>) => {
    e.preventDefault();
  }, []);

  const handleRemoveVideo = useCallback(() => {
    setVideoFile(null);
    setVideoPreviewUrl(null);
    setUploadProgress(0);
    onVideoUploaded('', '');
    if (fileInputRef.current) fileInputRef.current.value = '';
  }, [onVideoUploaded]);

  // ============================================================
  // LOCKED STATE (Plan does not allow video)
  // ============================================================

  if (maxVideos === 0) {
    return (
      <div className="mx-auto max-w-2xl space-y-6 py-8">
        <Card className="border-amber-200 bg-gradient-to-br from-amber-50 to-yellow-50">
          <CardContent className="p-8 text-center">
            <Lock className="mx-auto mb-4 h-16 w-16 text-amber-500" />
            <h3 className="mb-2 text-xl font-bold text-gray-900">Video — Función Premium</h3>
            <p className="mb-6 text-gray-600">
              Los listados con video reciben hasta un 3× más consultas. Actualiza tu plan para
              agregar videos a tus publicaciones.
            </p>
            <Button className="gap-2 bg-gradient-to-r from-amber-500 to-yellow-500 text-white">
              <Crown className="h-4 w-4" />
              Actualizar Plan
            </Button>
            <Button variant="ghost" onClick={onSkip} className="ml-3">
              Saltar
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // ============================================================
  // UPLOAD UI
  // ============================================================

  return (
    <div className="mx-auto max-w-2xl space-y-6 py-4">
      {/* Header */}
      <div className="text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-100">
          <Video className="h-8 w-8 text-blue-600" />
        </div>
        <h2 className="text-2xl font-bold text-gray-900">Video del Vehículo</h2>
        <p className="mx-auto mt-2 max-w-md text-gray-500">
          Agrega un video para que los compradores vean tu vehículo en movimiento. Los listados con
          video reciben hasta 3× más consultas.
        </p>
        <Badge variant="secondary" className="mt-3">
          Tu plan permite {maxVideos} video{maxVideos !== 1 ? 's' : ''} por publicación
        </Badge>
      </div>

      {/* Upload Area or Preview */}
      {!videoPreviewUrl ? (
        <label
          className="flex cursor-pointer flex-col items-center justify-center rounded-2xl border-3 border-dashed border-gray-300 bg-gray-50 px-6 py-16 transition-colors hover:border-blue-400 hover:bg-blue-50"
          onDrop={handleDrop}
          onDragOver={handleDragOver}
        >
          <Upload className="mb-3 h-10 w-10 text-gray-400" />
          <p className="text-lg font-semibold text-gray-600">
            Arrastra tu video aquí o haz clic para seleccionar
          </p>
          <p className="mt-1 text-sm text-gray-400">
            MP4, MOV, WebM — máximo {MAX_VIDEO_SIZE_MB} MB
          </p>
          <input
            ref={fileInputRef}
            type="file"
            accept="video/mp4,video/mov,video/webm,video/quicktime"
            onChange={handleFileChange}
            className="hidden"
          />
        </label>
      ) : (
        <Card>
          <CardContent className="p-6">
            {/* Video Preview */}
            <div className="relative mb-4 overflow-hidden rounded-xl bg-black">
              <video
                src={videoPreviewUrl}
                controls
                className="mx-auto max-h-80 w-full"
                preload="metadata"
              >
                Tu navegador no soporta la reproducción de video.
              </video>
              <div className="absolute top-3 left-3">
                <Badge className="gap-1 bg-black/60 text-white backdrop-blur-sm">
                  <Play className="h-3 w-3" />
                  Video
                </Badge>
              </div>
            </div>

            {/* File info */}
            <div className="flex items-center gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-100">
                <Video className="h-6 w-6 text-blue-600" />
              </div>
              <div className="flex-1">
                <p className="font-medium">{videoFile?.name || 'Video subido'}</p>
                {videoFile && (
                  <p className="text-sm text-gray-500">
                    {(videoFile.size / (1024 * 1024)).toFixed(1)} MB
                  </p>
                )}
                {isUploading && (
                  <div className="mt-2">
                    <Progress value={uploadProgress} className="h-2" />
                    <p className="mt-1 text-xs text-gray-500">
                      {uploadProgress < 50
                        ? 'Subiendo video...'
                        : uploadProgress < 90
                          ? 'Procesando...'
                          : 'Finalizando...'}
                    </p>
                  </div>
                )}
              </div>
              {!isUploading && (
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleRemoveVideo}
                  className="text-red-500 hover:bg-red-50 hover:text-red-700"
                >
                  <Trash2 className="h-5 w-5" />
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex items-center justify-between">
        <Button variant="ghost" onClick={onSkip}>
          Saltar
        </Button>
        <Button
          onClick={onComplete}
          disabled={isUploading}
          className="gap-2 bg-emerald-600 hover:bg-emerald-700"
        >
          Continuar
        </Button>
      </div>
    </div>
  );
}
