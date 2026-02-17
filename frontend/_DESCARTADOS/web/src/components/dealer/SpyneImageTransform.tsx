/**
 * SpyneImageTransform Component
 *
 * Allows users to upload or provide a vehicle image URL
 * and transform it with AI background replacement
 */

import { useState, useEffect } from 'react';
import {
  transformImage,
  getBackgrounds,
  pollJobStatus,
  type VehicleTransformRequest,
  type BackgroundOption,
  type ProcessedImage,
  type JobStatusResponse,
  BACKGROUND_PRESETS,
} from '@/services/spyneService';

interface SpyneImageTransformProps {
  /** Callback when transformation completes */
  onComplete?: (images: ProcessedImage[]) => void;
  /** Callback when transformation fails */
  onError?: (error: string) => void;
  /** Pre-fill with an image URL */
  initialImageUrl?: string;
  /** Hide the URL input (use when providing initialImageUrl) */
  compact?: boolean;
}

export function SpyneImageTransform({
  onComplete,
  onError,
  initialImageUrl,
  compact = false,
}: SpyneImageTransformProps) {
  // State
  const [imageUrl, setImageUrl] = useState(initialImageUrl || '');
  const [selectedBackground, setSelectedBackground] = useState(BACKGROUND_PRESETS.STUDIO_WHITE);
  const [backgrounds, setBackgrounds] = useState<BackgroundOption[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [status, setStatus] = useState<JobStatusResponse | null>(null);
  const [processedImages, setProcessedImages] = useState<ProcessedImage[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);

  // Load backgrounds on mount
  useEffect(() => {
    const loadBackgrounds = async () => {
      try {
        const bgs = await getBackgrounds();
        setBackgrounds(bgs);
      } catch (err) {
        console.error('Failed to load backgrounds:', err);
      }
    };
    loadBackgrounds();
  }, []);

  // Handle transform
  const handleTransform = async () => {
    if (!imageUrl.trim()) {
      setError('Please enter an image URL');
      return;
    }

    setIsLoading(true);
    setError(null);
    setProgress(0);
    setProcessedImages([]);

    try {
      const request: VehicleTransformRequest = {
        imageUrl: imageUrl.trim(),
        backgroundId: selectedBackground,
        maskLicensePlate: true,
      };

      const result = await transformImage(request);
      setProgress(10);

      // Poll for completion
      const finalStatus = await pollJobStatus(
        result.jobId,
        (s) => {
          setStatus(s);
          // Estimate progress based on status
          if (s.status === 'processing') {
            setProgress((prev) => Math.min(prev + 5, 90));
          }
        },
        60, // max attempts
        2000 // poll every 2 seconds
      );

      setProgress(100);

      if (finalStatus.status === 'completed' && finalStatus.images) {
        setProcessedImages(finalStatus.images);
        onComplete?.(finalStatus.images);
      } else if (finalStatus.status === 'failed') {
        const errorMsg = 'Image transformation failed';
        setError(errorMsg);
        onError?.(errorMsg);
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Unknown error';
      setError(errorMsg);
      onError?.(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  // Reset state
  const handleReset = () => {
    setImageUrl('');
    setProcessedImages([]);
    setStatus(null);
    setError(null);
    setProgress(0);
  };

  return (
    <div className="w-full max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-lg">
      <h2 className="text-2xl font-bold text-gray-800 mb-4">🚗 AI Image Transform</h2>

      {/* Image URL Input */}
      {!compact && (
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">Image URL</label>
          <input
            type="url"
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            placeholder="https://example.com/car-image.jpg"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            disabled={isLoading}
          />
        </div>
      )}

      {/* Background Selection */}
      <div className="mb-4">
        <label className="block text-sm font-medium text-gray-700 mb-2">Background</label>
        <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
          {backgrounds.slice(0, 8).map((bg) => (
            <button
              key={bg.id}
              onClick={() => setSelectedBackground(bg.id)}
              className={`relative p-1 rounded-lg border-2 transition-all ${
                selectedBackground === bg.id
                  ? 'border-blue-500 ring-2 ring-blue-200'
                  : 'border-gray-200 hover:border-gray-300'
              }`}
              disabled={isLoading}
            >
              <img src={bg.previewUrl} alt={bg.name} className="w-full h-16 object-cover rounded" />
              <span className="absolute bottom-0 left-0 right-0 bg-black/60 text-white text-xs p-1 truncate">
                {bg.name}
              </span>
            </button>
          ))}
        </div>
      </div>

      {/* Error Message */}
      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700">
          ❌ {error}
        </div>
      )}

      {/* Progress Bar */}
      {isLoading && (
        <div className="mb-4">
          <div className="flex justify-between text-sm text-gray-600 mb-1">
            <span>Processing...</span>
            <span>{progress}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-blue-500 h-2 rounded-full transition-all duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
          {status && <p className="text-sm text-gray-500 mt-1">Status: {status.status}</p>}
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex gap-3 mb-6">
        <button
          onClick={handleTransform}
          disabled={isLoading || !imageUrl.trim()}
          className={`flex-1 py-3 px-6 rounded-lg font-semibold text-white transition-all ${
            isLoading || !imageUrl.trim()
              ? 'bg-gray-400 cursor-not-allowed'
              : 'bg-blue-600 hover:bg-blue-700'
          }`}
        >
          {isLoading ? '⏳ Processing...' : '✨ Transform Image'}
        </button>

        {processedImages.length > 0 && (
          <button
            onClick={handleReset}
            className="py-3 px-6 rounded-lg font-semibold text-gray-700 bg-gray-100 hover:bg-gray-200 transition-all"
          >
            🔄 Reset
          </button>
        )}
      </div>

      {/* Preview Images */}
      {(imageUrl || processedImages.length > 0) && (
        <div className="grid grid-cols-2 gap-4">
          {/* Original */}
          {imageUrl && (
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-2">Original</h3>
              <div className="aspect-video bg-gray-100 rounded-lg overflow-hidden">
                <img
                  src={imageUrl}
                  alt="Original"
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src =
                      'https://via.placeholder.com/400x300?text=Invalid+URL';
                  }}
                />
              </div>
            </div>
          )}

          {/* Processed */}
          {processedImages.length > 0 && (
            <div>
              <h3 className="text-sm font-medium text-gray-700 mb-2">✅ Transformed</h3>
              <div className="aspect-video bg-gray-100 rounded-lg overflow-hidden">
                <img
                  src={processedImages[0].processedUrl || ''}
                  alt="Processed"
                  className="w-full h-full object-cover"
                />
              </div>
              <a
                href={processedImages[0].processedUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-block mt-2 text-sm text-blue-600 hover:underline"
              >
                📥 Download Full Resolution
              </a>
            </div>
          )}
        </div>
      )}

      {/* Multiple Processed Images */}
      {processedImages.length > 1 && (
        <div className="mt-6">
          <h3 className="text-sm font-medium text-gray-700 mb-2">All Processed Images</h3>
          <div className="grid grid-cols-4 gap-2">
            {processedImages.map((img, index) => (
              <a
                key={img.imageId || index}
                href={img.processedUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="aspect-square bg-gray-100 rounded overflow-hidden hover:opacity-80 transition-opacity"
              >
                <img
                  src={img.processedUrl || ''}
                  alt={`Processed ${index + 1}`}
                  className="w-full h-full object-cover"
                />
              </a>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default SpyneImageTransform;
