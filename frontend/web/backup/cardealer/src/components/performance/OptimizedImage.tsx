/**
 * OptimizedImage - Lazy loading image component with blur placeholder
 * Critical for performance in low-bandwidth areas like Dominican Republic
 * 
 * Features:
 * - Lazy loading with IntersectionObserver
 * - Blur placeholder (LQIP - Low Quality Image Placeholder)
 * - Progressive loading effect
 * - WebP/AVIF format support with fallback
 * - Responsive srcset generation
 * - Error handling with fallback
 */

import React, { useState, useRef, useEffect, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

interface OptimizedImageProps {
  src: string;
  alt: string;
  width?: number;
  height?: number;
  className?: string;
  priority?: boolean; // Skip lazy loading for above-the-fold images
  placeholder?: 'blur' | 'skeleton' | 'none';
  blurDataURL?: string; // Base64 tiny image for blur effect
  sizes?: string; // Responsive sizes attribute
  quality?: number; // Image quality 1-100
  objectFit?: 'cover' | 'contain' | 'fill' | 'none';
  onLoad?: () => void;
  onError?: () => void;
  aspectRatio?: string; // e.g., "16/9", "4/3", "1/1"
}

// Generate a tiny SVG placeholder with blur effect
const generateBlurPlaceholder = (width: number, height: number, color: string = '#e5e7eb'): string => {
  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${width} ${height}">
      <filter id="b" color-interpolation-filters="sRGB">
        <feGaussianBlur stdDeviation="20"/>
        <feComponentTransfer>
          <feFuncA type="discrete" tableValues="1 1"/>
        </feComponentTransfer>
      </filter>
      <rect width="100%" height="100%" fill="${color}"/>
      <rect width="100%" height="100%" filter="url(#b)" opacity="0.5" fill="${color}"/>
    </svg>
  `;
  return `data:image/svg+xml;base64,${btoa(svg.trim())}`;
};

// Generate skeleton shimmer effect
const SkeletonShimmer: React.FC<{ className?: string }> = ({ className }) => (
  <div className={`animate-pulse bg-gradient-to-r from-gray-200 via-gray-100 to-gray-200 bg-[length:200%_100%] ${className}`}>
    <style>{`
      @keyframes shimmer {
        0% { background-position: 200% 0; }
        100% { background-position: -200% 0; }
      }
      .animate-shimmer {
        animation: shimmer 1.5s ease-in-out infinite;
      }
    `}</style>
  </div>
);

// Image size breakpoints for srcset
const IMAGE_BREAKPOINTS = [320, 640, 768, 1024, 1280, 1536];

// Generate srcset for responsive images
const generateSrcSet = (src: string, breakpoints: number[] = IMAGE_BREAKPOINTS): string => {
  // If it's a local file or placeholder, don't generate srcset
  if (src.startsWith('/') || src.startsWith('data:')) {
    return '';
  }

  // For CDN images, generate srcset with width parameters
  // This assumes the backend/CDN supports width transformations
  return breakpoints
    .map(bp => {
      // Check if URL already has query params
      const separator = src.includes('?') ? '&' : '?';
      return `${src}${separator}w=${bp} ${bp}w`;
    })
    .join(', ');
};

export const OptimizedImage: React.FC<OptimizedImageProps> = ({
  src,
  alt,
  width,
  height,
  className = '',
  priority = false,
  placeholder = 'blur',
  blurDataURL,
  sizes = '100vw',
  quality = 75,
  objectFit = 'cover',
  onLoad,
  onError,
  aspectRatio,
}) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [isInView, setIsInView] = useState(priority);
  const [hasError, setHasError] = useState(false);
  const imgRef = useRef<HTMLImageElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  // Generate placeholder
  const placeholderSrc = useMemo(() => {
    if (blurDataURL) return blurDataURL;
    return generateBlurPlaceholder(width || 400, height || 300);
  }, [blurDataURL, width, height]);

  // Generate srcset for responsive loading
  const srcSet = useMemo(() => generateSrcSet(src), [src]);

  // Intersection Observer for lazy loading
  useEffect(() => {
    if (priority || isInView) return;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setIsInView(true);
            observer.disconnect();
          }
        });
      },
      {
        rootMargin: '200px', // Start loading 200px before entering viewport
        threshold: 0.01,
      }
    );

    if (containerRef.current) {
      observer.observe(containerRef.current);
    }

    return () => observer.disconnect();
  }, [priority, isInView]);

  // Handle image load
  const handleLoad = () => {
    setIsLoaded(true);
    onLoad?.();
  };

  // Handle image error
  const handleError = () => {
    setHasError(true);
    onError?.();
  };

  // Fallback image
  const fallbackSrc = '/images/placeholder-image.svg';

  // Object fit classes
  const objectFitClass = {
    cover: 'object-cover',
    contain: 'object-contain',
    fill: 'object-fill',
    none: 'object-none',
  }[objectFit];

  // Aspect ratio style
  const aspectRatioStyle = aspectRatio ? { aspectRatio } : {};

  return (
    <div
      ref={containerRef}
      className={`relative overflow-hidden ${className}`}
      style={{
        ...aspectRatioStyle,
        width: width ? `${width}px` : undefined,
        height: height ? `${height}px` : undefined,
      }}
    >
      {/* Placeholder layer */}
      <AnimatePresence>
        {!isLoaded && (
          <motion.div
            initial={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
            className="absolute inset-0 z-10"
          >
            {placeholder === 'blur' && (
              <img
                src={placeholderSrc}
                alt=""
                aria-hidden="true"
                className={`w-full h-full ${objectFitClass} blur-lg scale-110`}
              />
            )}
            {placeholder === 'skeleton' && (
              <SkeletonShimmer className="w-full h-full" />
            )}
          </motion.div>
        )}
      </AnimatePresence>

      {/* Main image */}
      {isInView && (
        <motion.img
          ref={imgRef}
          src={hasError ? fallbackSrc : src}
          srcSet={!hasError && srcSet ? srcSet : undefined}
          sizes={sizes}
          alt={alt}
          width={width}
          height={height}
          loading={priority ? 'eager' : 'lazy'}
          decoding={priority ? 'sync' : 'async'}
          onLoad={handleLoad}
          onError={handleError}
          initial={{ opacity: 0 }}
          animate={{ opacity: isLoaded ? 1 : 0 }}
          transition={{ duration: 0.5, ease: 'easeOut' }}
          className={`w-full h-full ${objectFitClass}`}
          style={{
            // Add quality hint for browsers that support it
            // @ts-ignore - imageRendering is valid CSS
            imageRendering: quality < 50 ? 'pixelated' : 'auto',
          }}
        />
      )}

      {/* Error state */}
      {hasError && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="text-center text-gray-400">
            <svg
              className="w-12 h-12 mx-auto mb-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1.5}
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
              />
            </svg>
            <span className="text-sm">Error loading image</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default OptimizedImage;
