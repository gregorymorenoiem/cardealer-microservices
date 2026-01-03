/**
 * PullToRefresh - Native-feeling pull to refresh component
 * Optimized for mobile users expecting app-like behavior
 * 
 * Features:
 * - Native-feeling pull gesture
 * - Visual feedback with spinner
 * - Haptic feedback on release
 * - Customizable threshold and content
 * - Works with scroll containers
 */

import React, { useState, useRef, useCallback, useEffect } from 'react';
import { motion, useMotionValue, useTransform, animate } from 'framer-motion';
import { FiRefreshCw } from 'react-icons/fi';

interface PullToRefreshProps {
  onRefresh: () => Promise<void>;
  children: React.ReactNode;
  threshold?: number;
  maxPull?: number;
  className?: string;
  disabled?: boolean;
  pullContent?: React.ReactNode;
  refreshingContent?: React.ReactNode;
}

const DEFAULT_THRESHOLD = 80;
const DEFAULT_MAX_PULL = 120;

export const PullToRefresh: React.FC<PullToRefreshProps> = ({
  onRefresh,
  children,
  threshold = DEFAULT_THRESHOLD,
  maxPull = DEFAULT_MAX_PULL,
  className = '',
  disabled = false,
  pullContent,
  refreshingContent,
}) => {
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isPulling, setIsPulling] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const startY = useRef(0);
  const currentY = useRef(0);
  
  const pullDistance = useMotionValue(0);
  const progress = useTransform(pullDistance, [0, threshold], [0, 1]);
  const rotation = useTransform(pullDistance, [0, threshold], [0, 360]);
  const scale = useTransform(pullDistance, [0, threshold], [0.5, 1]);
  const opacity = useTransform(pullDistance, [0, threshold * 0.5], [0, 1]);

  // Trigger haptic feedback
  const triggerHaptic = useCallback((intensity: number = 10) => {
    if ('vibrate' in navigator) {
      navigator.vibrate(intensity);
    }
  }, []);

  // Handle refresh
  const handleRefresh = useCallback(async () => {
    if (isRefreshing || disabled) return;
    
    setIsRefreshing(true);
    triggerHaptic(20);

    try {
      await onRefresh();
    } finally {
      setIsRefreshing(false);
      animate(pullDistance, 0, { duration: 0.3 });
    }
  }, [isRefreshing, disabled, onRefresh, pullDistance, triggerHaptic]);

  // Touch handlers
  const handleTouchStart = useCallback((e: TouchEvent) => {
    if (disabled || isRefreshing) return;
    
    const container = containerRef.current;
    if (!container || container.scrollTop > 0) return;

    startY.current = e.touches[0].clientY;
    setIsPulling(true);
  }, [disabled, isRefreshing]);

  const handleTouchMove = useCallback((e: TouchEvent) => {
    if (!isPulling || disabled || isRefreshing) return;

    const container = containerRef.current;
    if (!container || container.scrollTop > 0) {
      setIsPulling(false);
      pullDistance.set(0);
      return;
    }

    currentY.current = e.touches[0].clientY;
    const delta = currentY.current - startY.current;

    if (delta > 0) {
      // Apply resistance - the more you pull, the harder it gets
      const resistance = 0.5;
      const resistedDelta = delta * Math.pow(resistance, delta / maxPull);
      const clampedDelta = Math.min(resistedDelta, maxPull);
      
      const previousDistance = pullDistance.get();
      pullDistance.set(clampedDelta);

      // Haptic at threshold
      if (clampedDelta >= threshold && previousDistance < threshold) {
        triggerHaptic(15);
      }

      // Prevent default scroll when pulling down
      if (delta > 10) {
        e.preventDefault();
      }
    }
  }, [isPulling, disabled, isRefreshing, maxPull, pullDistance, threshold, triggerHaptic]);

  const handleTouchEnd = useCallback(() => {
    if (!isPulling) return;
    
    setIsPulling(false);

    const distance = pullDistance.get();
    
    if (distance >= threshold) {
      // Animate to refresh position and trigger refresh
      animate(pullDistance, threshold * 0.6, { duration: 0.2 });
      handleRefresh();
    } else {
      // Snap back
      animate(pullDistance, 0, { duration: 0.2 });
    }
  }, [isPulling, pullDistance, threshold, handleRefresh]);

  // Attach touch listeners
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    container.addEventListener('touchstart', handleTouchStart, { passive: true });
    container.addEventListener('touchmove', handleTouchMove, { passive: false });
    container.addEventListener('touchend', handleTouchEnd, { passive: true });
    container.addEventListener('touchcancel', handleTouchEnd, { passive: true });

    return () => {
      container.removeEventListener('touchstart', handleTouchStart);
      container.removeEventListener('touchmove', handleTouchMove);
      container.removeEventListener('touchend', handleTouchEnd);
      container.removeEventListener('touchcancel', handleTouchEnd);
    };
  }, [handleTouchStart, handleTouchMove, handleTouchEnd]);

  return (
    <div 
      ref={containerRef}
      className={`relative overflow-auto overscroll-contain ${className}`}
      style={{ WebkitOverflowScrolling: 'touch' }}
    >
      {/* Pull indicator */}
      <motion.div
        className="absolute left-0 right-0 flex items-center justify-center pointer-events-none z-10"
        style={{
          top: -60,
          y: pullDistance,
        }}
      >
        <motion.div
          className="flex flex-col items-center gap-2"
          style={{ opacity }}
        >
          {isRefreshing ? (
            refreshingContent || (
              <div className="flex items-center gap-2 text-blue-500">
                <motion.div
                  animate={{ rotate: 360 }}
                  transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
                >
                  <FiRefreshCw className="w-6 h-6" />
                </motion.div>
                <span className="text-sm font-medium">Actualizando...</span>
              </div>
            )
          ) : (
            pullContent || (
              <div className="flex flex-col items-center">
                <motion.div
                  style={{ 
                    rotate: rotation,
                    scale,
                  }}
                  className="text-gray-400"
                >
                  <FiRefreshCw className="w-6 h-6" />
                </motion.div>
                <motion.span 
                  className="text-sm text-gray-500 mt-1"
                  style={{ opacity: progress }}
                >
                  {pullDistance.get() >= threshold ? 'Suelta para actualizar' : 'Desliza para actualizar'}
                </motion.span>
              </div>
            )
          )}
        </motion.div>
      </motion.div>

      {/* Content */}
      <motion.div style={{ y: isRefreshing ? threshold * 0.6 : 0 }}>
        {children}
      </motion.div>
    </div>
  );
};

export default PullToRefresh;
