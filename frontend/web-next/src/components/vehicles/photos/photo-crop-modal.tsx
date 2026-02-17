'use client';

import { useState, useRef, useCallback, useEffect } from 'react';
import ReactCrop, { type Crop, type PixelCrop, centerCrop, makeAspectCrop } from 'react-image-crop';
import 'react-image-crop/dist/ReactCrop.css';
import { X, RotateCw, FlipHorizontal, Check, Undo2 } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

interface CropModalProps {
  isOpen: boolean;
  imageUrl: string;
  onClose: () => void;
  onConfirm: (croppedFile: File) => void;
  aspectRatio?: number; // e.g., 4/3, 16/9
  fileName?: string;
}

// ============================================================
// HELPERS
// ============================================================

function centerAspectCrop(mediaWidth: number, mediaHeight: number, aspect: number): Crop {
  return centerCrop(
    makeAspectCrop({ unit: '%', width: 80 }, aspect, mediaWidth, mediaHeight),
    mediaWidth,
    mediaHeight
  );
}

async function getCroppedImage(
  image: HTMLImageElement,
  crop: PixelCrop,
  rotation: number,
  flipH: boolean,
  fileName: string
): Promise<File> {
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('No canvas context');

  const scaleX = image.naturalWidth / image.width;
  const scaleY = image.naturalHeight / image.height;

  const pixelCrop = {
    x: crop.x * scaleX,
    y: crop.y * scaleY,
    width: crop.width * scaleX,
    height: crop.height * scaleY,
  };

  const radians = (rotation * Math.PI) / 180;

  canvas.width = pixelCrop.width;
  canvas.height = pixelCrop.height;

  ctx.save();
  ctx.translate(canvas.width / 2, canvas.height / 2);
  if (rotation) ctx.rotate(radians);
  if (flipH) ctx.scale(-1, 1);
  ctx.translate(-canvas.width / 2, -canvas.height / 2);

  ctx.drawImage(
    image,
    pixelCrop.x,
    pixelCrop.y,
    pixelCrop.width,
    pixelCrop.height,
    0,
    0,
    pixelCrop.width,
    pixelCrop.height
  );
  ctx.restore();

  return new Promise((resolve, reject) => {
    canvas.toBlob(
      blob => {
        if (!blob) {
          reject(new Error('Canvas toBlob failed'));
          return;
        }
        const file = new File([blob], fileName, { type: 'image/jpeg' });
        resolve(file);
      },
      'image/jpeg',
      0.92
    );
  });
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoCropModal({
  isOpen,
  imageUrl,
  onClose,
  onConfirm,
  aspectRatio = 4 / 3,
  fileName = 'cropped.jpg',
}: CropModalProps) {
  const [crop, setCrop] = useState<Crop>();
  const [completedCrop, setCompletedCrop] = useState<PixelCrop>();
  const [rotation, setRotation] = useState(0);
  const [flipH, setFlipH] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const imgRef = useRef<HTMLImageElement>(null);

  const onImageLoaded = useCallback(
    (e: React.SyntheticEvent<HTMLImageElement>) => {
      const { width, height } = e.currentTarget;
      setCrop(centerAspectCrop(width, height, aspectRatio));
    },
    [aspectRatio]
  );

  const handleConfirm = useCallback(async () => {
    if (!imgRef.current || !completedCrop) return;
    setIsProcessing(true);
    try {
      const croppedFile = await getCroppedImage(
        imgRef.current,
        completedCrop,
        rotation,
        flipH,
        fileName
      );
      onConfirm(croppedFile);
    } catch {
      // Silently fail - user can retry
    } finally {
      setIsProcessing(false);
    }
  }, [completedCrop, rotation, flipH, fileName, onConfirm]);

  const handleReset = useCallback(() => {
    setRotation(0);
    setFlipH(false);
    if (imgRef.current) {
      const { width, height } = imgRef.current;
      setCrop(centerAspectCrop(width, height, aspectRatio));
    }
  }, [aspectRatio]);

  // Reset state when modal opens
  useEffect(() => {
    if (isOpen) {
      setRotation(0);
      setFlipH(false);
      setCrop(undefined);
      setCompletedCrop(undefined);
    }
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4">
      <div className="flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-2xl bg-white shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between border-b px-4 py-3">
          <h3 className="text-lg font-semibold text-gray-900">Recortar foto</h3>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Crop area */}
        <div className="flex-1 overflow-auto bg-gray-900 p-4">
          <div
            className="mx-auto"
            style={{
              transform: `rotate(${rotation}deg) scaleX(${flipH ? -1 : 1})`,
              transition: 'transform 0.3s ease',
            }}
          >
            <ReactCrop
              crop={crop}
              onChange={c => setCrop(c)}
              onComplete={c => setCompletedCrop(c)}
              aspect={aspectRatio}
              className="mx-auto max-h-[50vh]"
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                ref={imgRef}
                src={imageUrl}
                alt="Recortar"
                onLoad={onImageLoaded}
                className="max-h-[50vh] w-auto"
                crossOrigin="anonymous"
              />
            </ReactCrop>
          </div>
        </div>

        {/* Toolbar */}
        <div className="flex items-center justify-between border-t px-4 py-3">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setRotation(r => (r + 90) % 360)}
              className="flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100"
            >
              <RotateCw className="h-4 w-4" />
              Rotar
            </button>
            <button
              type="button"
              onClick={() => setFlipH(f => !f)}
              className={`flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm transition-colors ${flipH ? 'bg-blue-50 text-blue-600' : 'text-gray-600 hover:bg-gray-100'}`}
            >
              <FlipHorizontal className="h-4 w-4" />
              Voltear
            </button>
            <button
              type="button"
              onClick={handleReset}
              className="flex items-center gap-1 rounded-lg px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100"
            >
              <Undo2 className="h-4 w-4" />
              Restablecer
            </button>
          </div>

          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100"
            >
              Cancelar
            </button>
            <button
              type="button"
              onClick={handleConfirm}
              disabled={!completedCrop || isProcessing}
              className="flex items-center gap-1.5 rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            >
              <Check className="h-4 w-4" />
              {isProcessing ? 'Procesando...' : 'Aplicar recorte'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
