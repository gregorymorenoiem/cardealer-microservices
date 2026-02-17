/**
 * Image Compressor — Client-side image compression using browser-image-compression.
 * Reduces file sizes before upload to save bandwidth, especially critical for
 * mobile users on slow connections in Dominican Republic (3G/4G).
 */

import imageCompression from 'browser-image-compression';

// ============================================================
// TYPES
// ============================================================

export interface CompressionOptions {
  maxSizeMB: number;
  maxWidthOrHeight: number;
  useWebWorker: boolean;
  preserveExif: boolean;
  fileType?: string;
  onProgress?: (progress: number) => void;
}

export interface CompressionResult {
  compressed: File;
  savings: number;
  originalSize: number;
  compressedSize: number;
  savingsPercent: number;
  wasCompressed: boolean;
}

// ============================================================
// DEFAULT CONFIGS
// ============================================================

const DEFAULT_OPTIONS: CompressionOptions = {
  maxSizeMB: 1.5,
  maxWidthOrHeight: 2048,
  useWebWorker: true,
  preserveExif: true,
};

/** Compression configs by context */
export const COMPRESSION_PRESETS = {
  standard: { maxSizeMB: 1.5, maxWidthOrHeight: 2048 },
  dealer: { maxSizeMB: 2.0, maxWidthOrHeight: 2560 },
  thumbnail: { maxSizeMB: 0.3, maxWidthOrHeight: 800 },
  viewer360: { maxSizeMB: 1.0, maxWidthOrHeight: 1920 },
} as const;

export type CompressionPreset = keyof typeof COMPRESSION_PRESETS;

/** Threshold above which compression is applied (2MB) */
const COMPRESSION_THRESHOLD = 2 * 1024 * 1024;

// ============================================================
// FUNCTIONS
// ============================================================

/**
 * Determine if a file should be compressed based on its size.
 */
export function shouldCompress(file: File, thresholdBytes = COMPRESSION_THRESHOLD): boolean {
  return file.size > thresholdBytes;
}

/**
 * Compress an image file for optimal upload size.
 * Returns the compressed file with savings statistics.
 */
export async function compressImage(
  file: File,
  options?: Partial<CompressionOptions>
): Promise<CompressionResult> {
  const opts = { ...DEFAULT_OPTIONS, ...options };
  const originalSize = file.size;

  // Skip compression if already under threshold
  if (!shouldCompress(file, opts.maxSizeMB * 1024 * 1024)) {
    return {
      compressed: file,
      savings: 0,
      originalSize,
      compressedSize: originalSize,
      savingsPercent: 0,
      wasCompressed: false,
    };
  }

  try {
    const compressed = await imageCompression(file, {
      maxSizeMB: opts.maxSizeMB,
      maxWidthOrHeight: opts.maxWidthOrHeight,
      useWebWorker: opts.useWebWorker,
      preserveExif: opts.preserveExif,
      fileType: opts.fileType,
      onProgress: opts.onProgress,
    });

    const compressedSize = compressed.size;
    const savings = originalSize - compressedSize;
    const savingsPercent = Math.round((savings / originalSize) * 100);

    // Only use compressed version if it's actually smaller
    if (compressedSize >= originalSize) {
      return {
        compressed: file,
        savings: 0,
        originalSize,
        compressedSize: originalSize,
        savingsPercent: 0,
        wasCompressed: false,
      };
    }

    return {
      compressed: new File([compressed], file.name, { type: compressed.type }),
      savings,
      originalSize,
      compressedSize,
      savingsPercent,
      wasCompressed: true,
    };
  } catch (error) {
    console.warn('Image compression failed, using original file:', error);
    return {
      compressed: file,
      savings: 0,
      originalSize,
      compressedSize: originalSize,
      savingsPercent: 0,
      wasCompressed: false,
    };
  }
}

/**
 * Get human-readable compression statistics.
 */
export function getCompressionStats(result: CompressionResult): string {
  if (!result.wasCompressed) return '';
  return `${formatFileSize(result.originalSize)} → ${formatFileSize(result.compressedSize)} (${result.savingsPercent}% ahorrado)`;
}

/**
 * Format bytes to human-readable string.
 */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
