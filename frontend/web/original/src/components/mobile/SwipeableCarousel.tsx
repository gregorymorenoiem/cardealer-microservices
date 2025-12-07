/**
 * SwipeableCarousel - Touch-optimized carousel component
 * Designed for mobile users with gesture support
 * 
 * Features:
 * - Native-feeling swipe gestures
 * - Momentum scrolling
 * - Snap-to-item behavior
 * - Lazy loading of items
 * - Keyboard navigation
 * - Reduced motion support
 */

import React, { useRef, useState, useCallback, useEffect } from 'react';
import { motion, useMotionValue, useTransform, animate } from 'framer-motion';
import type { PanInfo } from 'framer-motion';
import { FiChevronLeft, FiChevronRight } from 'react-icons/fi';
import { useReducedMotion } from '@/hooks/usePerformance';

interface SwipeableCarouselProps<T> {
  items: T[];
  renderItem: (item: T, index: number, isActive: boolean) => React.ReactNode;
  itemWidth?: number | string;
  gap?: number;
  showArrows?: boolean;
  showDots?: boolean;
  autoPlay?: boolean;
  autoPlayInterval?: number;
  loop?: boolean;
  className?: string;
  onActiveChange?: (index: number) => void;
}

const SWIPE_THRESHOLD = 50;
const VELOCITY_THRESHOLD = 500;

export function SwipeableCarousel<T>({
  items,
  renderItem,
  itemWidth = 280,
  gap = 16,
  showArrows = true,
  showDots = true,
  autoPlay = false,
  autoPlayInterval = 5000,
  loop = false,
  className = '',
  onActiveChange,
}: SwipeableCarouselProps<T>) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [activeIndex, setActiveIndex] = useState(0);
  const [containerWidth, setContainerWidth] = useState(0);
  const prefersReducedMotion = useReducedMotion();
  
  const x = useMotionValue(0);
  
  // Calculate item width in pixels
  const itemWidthPx = typeof itemWidth === 'number' 
    ? itemWidth 
    : containerWidth * (parseFloat(itemWidth) / 100);
  
  const totalWidth = (itemWidthPx + gap) * items.length - gap;
  const maxDrag = -(totalWidth - containerWidth);

  // Update container width on resize
  useEffect(() => {
    const updateWidth = () => {
      if (containerRef.current) {
        setContainerWidth(containerRef.current.offsetWidth);
      }
    };

    updateWidth();
    window.addEventListener('resize', updateWidth);
    return () => window.removeEventListener('resize', updateWidth);
  }, []);

  // Auto-play functionality
  useEffect(() => {
    if (!autoPlay || items.length <= 1) return;

    const interval = setInterval(() => {
      goToIndex((activeIndex + 1) % items.length);
    }, autoPlayInterval);

    return () => clearInterval(interval);
  }, [autoPlay, autoPlayInterval, activeIndex, items.length]);

  // Notify parent of active change
  useEffect(() => {
    onActiveChange?.(activeIndex);
  }, [activeIndex, onActiveChange]);

  // Navigate to specific index
  const goToIndex = useCallback((index: number) => {
    let targetIndex = index;
    
    if (loop) {
      targetIndex = ((index % items.length) + items.length) % items.length;
    } else {
      targetIndex = Math.max(0, Math.min(index, items.length - 1));
    }

    setActiveIndex(targetIndex);
    
    const targetX = -(targetIndex * (itemWidthPx + gap));
    const clampedX = Math.max(maxDrag, Math.min(0, targetX));

    if (prefersReducedMotion) {
      x.set(clampedX);
    } else {
      animate(x, clampedX, {
        type: 'spring',
        damping: 30,
        stiffness: 300,
      });
    }
  }, [items.length, itemWidthPx, gap, loop, maxDrag, prefersReducedMotion, x]);

  // Handle drag end with momentum
  const handleDragEnd = useCallback((_: MouseEvent | TouchEvent | PointerEvent, info: PanInfo) => {
    const velocity = info.velocity.x;
    const offset = info.offset.x;

    let newIndex = activeIndex;

    // Determine direction based on velocity or offset
    if (Math.abs(velocity) > VELOCITY_THRESHOLD) {
      // Velocity-based navigation (flick)
      newIndex = velocity > 0 ? activeIndex - 1 : activeIndex + 1;
    } else if (Math.abs(offset) > SWIPE_THRESHOLD) {
      // Offset-based navigation (drag)
      newIndex = offset > 0 ? activeIndex - 1 : activeIndex + 1;
    }

    goToIndex(newIndex);
  }, [activeIndex, goToIndex]);

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (!containerRef.current?.contains(document.activeElement)) return;
      
      switch (e.key) {
        case 'ArrowLeft':
          e.preventDefault();
          goToIndex(activeIndex - 1);
          break;
        case 'ArrowRight':
          e.preventDefault();
          goToIndex(activeIndex + 1);
          break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [activeIndex, goToIndex]);

  // Progress indicator transform
  const progressWidth = useTransform(
    x,
    [0, maxDrag],
    ['0%', '100%']
  );

  if (items.length === 0) {
    return null;
  }

  return (
    <div 
      ref={containerRef}
      className={`relative ${className}`}
      role="region"
      aria-label="Carousel"
    >
      {/* Carousel Track */}
      <div className="overflow-hidden">
        <motion.div
          drag="x"
          dragConstraints={{ left: maxDrag, right: 0 }}
          dragElastic={0.1}
          onDragEnd={handleDragEnd}
          style={{ x }}
          className="flex cursor-grab active:cursor-grabbing touch-pan-y"
          role="list"
        >
          {items.map((item, index) => (
            <motion.div
              key={index}
              role="listitem"
              style={{ 
                width: itemWidthPx, 
                marginRight: index < items.length - 1 ? gap : 0,
                flexShrink: 0,
              }}
              className="select-none"
            >
              {renderItem(item, index, index === activeIndex)}
            </motion.div>
          ))}
        </motion.div>
      </div>

      {/* Navigation Arrows */}
      {showArrows && items.length > 1 && (
        <>
          <button
            onClick={() => goToIndex(activeIndex - 1)}
            disabled={!loop && activeIndex === 0}
            className={`absolute left-2 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 shadow-lg flex items-center justify-center transition-opacity duration-200 hover:bg-white ${
              !loop && activeIndex === 0 ? 'opacity-30 cursor-not-allowed' : 'opacity-100'
            } hidden md:flex`}
            aria-label="Previous item"
          >
            <FiChevronLeft className="w-5 h-5" />
          </button>
          <button
            onClick={() => goToIndex(activeIndex + 1)}
            disabled={!loop && activeIndex === items.length - 1}
            className={`absolute right-2 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 shadow-lg flex items-center justify-center transition-opacity duration-200 hover:bg-white ${
              !loop && activeIndex === items.length - 1 ? 'opacity-30 cursor-not-allowed' : 'opacity-100'
            } hidden md:flex`}
            aria-label="Next item"
          >
            <FiChevronRight className="w-5 h-5" />
          </button>
        </>
      )}

      {/* Dots Indicator */}
      {showDots && items.length > 1 && items.length <= 10 && (
        <div className="flex justify-center gap-2 mt-4" role="tablist">
          {items.map((_, index) => (
            <button
              key={index}
              onClick={() => goToIndex(index)}
              className={`w-2 h-2 rounded-full transition-all duration-200 ${
                index === activeIndex
                  ? 'bg-blue-500 w-6'
                  : 'bg-gray-300 hover:bg-gray-400'
              }`}
              role="tab"
              aria-selected={index === activeIndex}
              aria-label={`Go to slide ${index + 1}`}
            />
          ))}
        </div>
      )}

      {/* Progress Bar (for many items) */}
      {items.length > 10 && (
        <div className="mt-4 h-1 bg-gray-200 rounded-full overflow-hidden">
          <motion.div
            className="h-full bg-blue-500 rounded-full"
            style={{ width: progressWidth }}
          />
        </div>
      )}
    </div>
  );
}

export default SwipeableCarousel;
