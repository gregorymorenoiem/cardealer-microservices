/**
 * ImageGallery - Optimized image gallery with lazy loading and lightbox
 * Designed for low-bandwidth optimization in Dominican Republic
 * 
 * Features:
 * - Virtual scrolling for large galleries
 * - Lazy loading thumbnails
 * - Preloading adjacent images
 * - Touch gestures support
 * - Fullscreen lightbox with zoom
 */

import React, { useState, useCallback, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { PanInfo } from 'framer-motion';
import { OptimizedImage } from './OptimizedImage';
import { FiX, FiChevronLeft, FiChevronRight, FiZoomIn, FiZoomOut, FiMaximize2 } from 'react-icons/fi';

interface GalleryImage {
  url: string;
  thumbnailUrl?: string;
  alt?: string;
  width?: number;
  height?: number;
}

interface ImageGalleryProps {
  images: GalleryImage[];
  initialIndex?: number;
  showThumbnails?: boolean;
  autoPlay?: boolean;
  autoPlayInterval?: number;
  className?: string;
  aspectRatio?: string;
  onImageChange?: (index: number) => void;
}

// Swipe threshold for touch gestures
const SWIPE_THRESHOLD = 50;

export const ImageGallery: React.FC<ImageGalleryProps> = ({
  images,
  initialIndex = 0,
  showThumbnails = true,
  autoPlay = false,
  autoPlayInterval = 5000,
  className = '',
  aspectRatio = '16/10',
  onImageChange,
}) => {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [isLightboxOpen, setIsLightboxOpen] = useState(false);
  const [zoomLevel, setZoomLevel] = useState(1);
  const [isPaused, setIsPaused] = useState(false);
  const thumbnailsRef = useRef<HTMLDivElement>(null);

  // Auto-play functionality
  useEffect(() => {
    if (!autoPlay || isPaused || isLightboxOpen) return;

    const interval = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % images.length);
    }, autoPlayInterval);

    return () => clearInterval(interval);
  }, [autoPlay, autoPlayInterval, isPaused, isLightboxOpen, images.length]);

  // Notify parent of image change
  useEffect(() => {
    onImageChange?.(currentIndex);
  }, [currentIndex, onImageChange]);

  // Preload adjacent images
  useEffect(() => {
    const preloadIndices = [
      (currentIndex - 1 + images.length) % images.length,
      (currentIndex + 1) % images.length,
    ];

    preloadIndices.forEach((idx) => {
      const img = new Image();
      img.src = images[idx].url;
    });
  }, [currentIndex, images]);

  // Scroll thumbnail into view
  useEffect(() => {
    if (thumbnailsRef.current && showThumbnails) {
      const thumbnail = thumbnailsRef.current.children[currentIndex] as HTMLElement;
      if (thumbnail) {
        thumbnail.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
      }
    }
  }, [currentIndex, showThumbnails]);

  const goToNext = useCallback(() => {
    setCurrentIndex((prev) => (prev + 1) % images.length);
  }, [images.length]);

  const goToPrevious = useCallback(() => {
    setCurrentIndex((prev) => (prev - 1 + images.length) % images.length);
  }, [images.length]);

  const goToIndex = useCallback((index: number) => {
    setCurrentIndex(index);
  }, []);

  // Handle swipe gestures
  const handleDragEnd = useCallback(
    (_: MouseEvent | TouchEvent | PointerEvent, info: PanInfo) => {
      if (info.offset.x > SWIPE_THRESHOLD) {
        goToPrevious();
      } else if (info.offset.x < -SWIPE_THRESHOLD) {
        goToNext();
      }
    },
    [goToNext, goToPrevious]
  );

  // Handle keyboard navigation
  useEffect(() => {
    if (!isLightboxOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      switch (e.key) {
        case 'ArrowLeft':
          goToPrevious();
          break;
        case 'ArrowRight':
          goToNext();
          break;
        case 'Escape':
          setIsLightboxOpen(false);
          break;
        case '+':
        case '=':
          setZoomLevel((prev) => Math.min(prev + 0.5, 3));
          break;
        case '-':
          setZoomLevel((prev) => Math.max(prev - 0.5, 1));
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isLightboxOpen, goToNext, goToPrevious]);

  if (images.length === 0) {
    return (
      <div className={`bg-gray-100 rounded-xl flex items-center justify-center ${className}`} style={{ aspectRatio }}>
        <span className="text-gray-400">No images available</span>
      </div>
    );
  }

  return (
    <>
      {/* Main Gallery */}
      <div
        className={`relative group ${className}`}
        onMouseEnter={() => setIsPaused(true)}
        onMouseLeave={() => setIsPaused(false)}
      >
        {/* Main Image Container */}
        <div className="relative overflow-hidden rounded-xl" style={{ aspectRatio }}>
          <motion.div
            key={currentIndex}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.3 }}
            drag="x"
            dragConstraints={{ left: 0, right: 0 }}
            dragElastic={0.2}
            onDragEnd={handleDragEnd}
            className="w-full h-full cursor-grab active:cursor-grabbing"
            onClick={() => setIsLightboxOpen(true)}
          >
            <OptimizedImage
              src={images[currentIndex].url}
              alt={images[currentIndex].alt || `Image ${currentIndex + 1}`}
              className="w-full h-full"
              objectFit="cover"
              priority={currentIndex === 0}
            />
          </motion.div>

          {/* Navigation Arrows */}
          {images.length > 1 && (
            <>
              <button
                onClick={(e) => { e.stopPropagation(); goToPrevious(); }}
                className="absolute left-3 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 rounded-full flex items-center justify-center shadow-lg opacity-0 group-hover:opacity-100 transition-opacity duration-200 hover:bg-white"
                aria-label="Previous image"
              >
                <FiChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={(e) => { e.stopPropagation(); goToNext(); }}
                className="absolute right-3 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 rounded-full flex items-center justify-center shadow-lg opacity-0 group-hover:opacity-100 transition-opacity duration-200 hover:bg-white"
                aria-label="Next image"
              >
                <FiChevronRight className="w-5 h-5" />
              </button>
            </>
          )}

          {/* Fullscreen Button */}
          <button
            onClick={() => setIsLightboxOpen(true)}
            className="absolute top-3 right-3 w-10 h-10 bg-white/90 rounded-full flex items-center justify-center shadow-lg opacity-0 group-hover:opacity-100 transition-opacity duration-200 hover:bg-white"
            aria-label="View fullscreen"
          >
            <FiMaximize2 className="w-4 h-4" />
          </button>

          {/* Image Counter */}
          <div className="absolute bottom-3 right-3 px-3 py-1.5 bg-black/60 text-white text-sm rounded-full">
            {currentIndex + 1} / {images.length}
          </div>

          {/* Dots Indicator (for mobile) */}
          {images.length <= 10 && (
            <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5 md:hidden">
              {images.map((_, idx) => (
                <button
                  key={idx}
                  onClick={(e) => { e.stopPropagation(); goToIndex(idx); }}
                  className={`w-2 h-2 rounded-full transition-all duration-200 ${
                    idx === currentIndex
                      ? 'bg-white w-4'
                      : 'bg-white/50 hover:bg-white/70'
                  }`}
                  aria-label={`Go to image ${idx + 1}`}
                />
              ))}
            </div>
          )}
        </div>

        {/* Thumbnails */}
        {showThumbnails && images.length > 1 && (
          <div
            ref={thumbnailsRef}
            className="flex gap-2 mt-3 overflow-x-auto pb-2 scrollbar-hide"
            style={{ scrollSnapType: 'x mandatory' }}
          >
            {images.map((image, idx) => (
              <button
                key={idx}
                onClick={() => goToIndex(idx)}
                className={`flex-shrink-0 relative rounded-lg overflow-hidden transition-all duration-200 ${
                  idx === currentIndex
                    ? 'ring-2 ring-blue-500 ring-offset-2'
                    : 'opacity-60 hover:opacity-100'
                }`}
                style={{ scrollSnapAlign: 'center' }}
              >
                <OptimizedImage
                  src={image.thumbnailUrl || image.url}
                  alt={image.alt || `Thumbnail ${idx + 1}`}
                  className="w-16 h-12 md:w-20 md:h-14"
                  objectFit="cover"
                  placeholder="skeleton"
                />
              </button>
            ))}
          </div>
        )}
      </div>

      {/* Lightbox */}
      <AnimatePresence>
        {isLightboxOpen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 bg-black/95 flex items-center justify-center"
            onClick={() => setIsLightboxOpen(false)}
          >
            {/* Close Button */}
            <button
              onClick={() => setIsLightboxOpen(false)}
              className="absolute top-4 right-4 w-12 h-12 bg-white/10 hover:bg-white/20 rounded-full flex items-center justify-center transition-colors z-10"
              aria-label="Close lightbox"
            >
              <FiX className="w-6 h-6 text-white" />
            </button>

            {/* Zoom Controls */}
            <div className="absolute top-4 left-4 flex gap-2 z-10">
              <button
                onClick={(e) => { e.stopPropagation(); setZoomLevel((prev) => Math.min(prev + 0.5, 3)); }}
                className="w-12 h-12 bg-white/10 hover:bg-white/20 rounded-full flex items-center justify-center transition-colors"
                aria-label="Zoom in"
              >
                <FiZoomIn className="w-5 h-5 text-white" />
              </button>
              <button
                onClick={(e) => { e.stopPropagation(); setZoomLevel((prev) => Math.max(prev - 0.5, 1)); }}
                className="w-12 h-12 bg-white/10 hover:bg-white/20 rounded-full flex items-center justify-center transition-colors"
                aria-label="Zoom out"
              >
                <FiZoomOut className="w-5 h-5 text-white" />
              </button>
              {zoomLevel > 1 && (
                <span className="px-3 py-2 bg-white/10 rounded-full text-white text-sm">
                  {Math.round(zoomLevel * 100)}%
                </span>
              )}
            </div>

            {/* Image */}
            <motion.img
              key={currentIndex}
              src={images[currentIndex].url}
              alt={images[currentIndex].alt || `Image ${currentIndex + 1}`}
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: zoomLevel }}
              exit={{ opacity: 0, scale: 0.9 }}
              transition={{ duration: 0.2 }}
              className="max-w-[90vw] max-h-[85vh] object-contain cursor-move"
              onClick={(e) => e.stopPropagation()}
              drag
              dragConstraints={{ left: -200, right: 200, top: -200, bottom: 200 }}
            />

            {/* Navigation */}
            {images.length > 1 && (
              <>
                <button
                  onClick={(e) => { e.stopPropagation(); goToPrevious(); }}
                  className="absolute left-4 top-1/2 -translate-y-1/2 w-14 h-14 bg-white/10 hover:bg-white/20 rounded-full flex items-center justify-center transition-colors"
                  aria-label="Previous image"
                >
                  <FiChevronLeft className="w-7 h-7 text-white" />
                </button>
                <button
                  onClick={(e) => { e.stopPropagation(); goToNext(); }}
                  className="absolute right-4 top-1/2 -translate-y-1/2 w-14 h-14 bg-white/10 hover:bg-white/20 rounded-full flex items-center justify-center transition-colors"
                  aria-label="Next image"
                >
                  <FiChevronRight className="w-7 h-7 text-white" />
                </button>
              </>
            )}

            {/* Counter */}
            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 px-4 py-2 bg-white/10 rounded-full text-white">
              {currentIndex + 1} / {images.length}
            </div>

            {/* Thumbnail Strip */}
            <div className="absolute bottom-16 left-1/2 -translate-x-1/2 flex gap-2 max-w-[80vw] overflow-x-auto pb-2">
              {images.map((image, idx) => (
                <button
                  key={idx}
                  onClick={(e) => { e.stopPropagation(); goToIndex(idx); }}
                  className={`flex-shrink-0 rounded-lg overflow-hidden transition-all duration-200 ${
                    idx === currentIndex
                      ? 'ring-2 ring-white ring-offset-2 ring-offset-black'
                      : 'opacity-50 hover:opacity-80'
                  }`}
                >
                  <img
                    src={image.thumbnailUrl || image.url}
                    alt=""
                    className="w-12 h-9 object-cover"
                  />
                </button>
              ))}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
};

export default ImageGallery;
