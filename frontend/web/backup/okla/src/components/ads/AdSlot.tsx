/**
 * AdSlot Component - Base component for all advertisement placements
 * Handles lazy loading, viewability tracking, and responsive sizing
 */

import React, { useEffect, useRef, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X } from 'lucide-react';

export type AdTier = 'premium' | 'high-value' | 'standard' | 'filler';
export type AdPosition = 'top' | 'middle' | 'bottom' | 'sidebar' | 'inline';

export interface AdSlotProps {
  /** Unique placement identifier for ad network */
  placement: string;
  
  /** Ad dimensions [width, height] */
  size: [number, number];
  
  /** Ad tier for pricing/priority */
  tier: AdTier;
  
  /** Position type affects styling and behavior */
  position: AdPosition;
  
  /** Enable lazy loading (default: true) */
  lazy?: boolean;
  
  /** Make ad sticky during scroll */
  sticky?: boolean;
  
  /** Allow user to close the ad */
  closeable?: boolean;
  
  /** Custom className for styling */
  className?: string;
  
  /** Callback when ad is viewed */
  onView?: () => void;
  
  /** Callback when ad is clicked */
  onClick?: () => void;
  
  /** Show only on specific breakpoints */
  breakpoint?: 'mobile' | 'tablet' | 'desktop' | 'all';
}

export const AdSlot: React.FC<AdSlotProps> = ({
  placement,
  size,
  tier,
  position,
  lazy = true,
  sticky = false,
  closeable = false,
  className = '',
  onView,
  onClick,
  breakpoint = 'all',
}) => {
  const adRef = useRef<HTMLDivElement>(null);
  const [isVisible, setIsVisible] = useState(!lazy);
  const [isViewed, setIsViewed] = useState(false);
  const [isClosed, setIsClosed] = useState(false);
  const [adLoaded, setAdLoaded] = useState(false);

  // Intersection Observer for lazy loading and viewability tracking
  useEffect(() => {
    if (!adRef.current) return;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            if (lazy && !isVisible) {
              setIsVisible(true);
            }
            
            // Track viewability (50% visible for 1 second)
            if (!isViewed && entry.intersectionRatio >= 0.5) {
              setTimeout(() => {
                if (entry.isIntersecting) {
                  setIsViewed(true);
                  onView?.();
                }
              }, 1000);
            }
          }
        });
      },
      {
        threshold: [0, 0.5, 1],
        rootMargin: '50px',
      }
    );

    observer.observe(adRef.current);

    return () => {
      if (adRef.current) {
        observer.unobserve(adRef.current);
      }
    };
  }, [lazy, isVisible, isViewed, onView]);

  // Handle ad close
  const handleClose = () => {
    setIsClosed(true);
  };

  // Handle ad click
  const handleClick = () => {
    onClick?.();
    // Track click event here (Google Analytics, etc.)
  };

  // Responsive display logic
  const getBreakpointClasses = () => {
    switch (breakpoint) {
      case 'mobile':
        return 'block md:hidden';
      case 'tablet':
        return 'hidden md:block lg:hidden';
      case 'desktop':
        return 'hidden lg:block';
      default:
        return 'block';
    }
  };

  // Tier-based styling
  const getTierStyles = () => {
    const baseStyles = 'rounded-xl backdrop-blur-md';
    
    switch (tier) {
      case 'premium':
        return `${baseStyles} border border-blue-500/20 bg-gradient-to-br from-blue-50/10 to-emerald-50/10`;
      case 'high-value':
        return `${baseStyles} border border-white/10 bg-white/5`;
      case 'standard':
        return `${baseStyles} border border-white/5 bg-white/3`;
      case 'filler':
        return `${baseStyles} bg-white/2`;
      default:
        return baseStyles;
    }
  };

  // Position-based styling
  const getPositionStyles = () => {
    if (sticky) {
      switch (position) {
        case 'sidebar':
          return 'sticky top-24 z-40';
        case 'top':
          return 'sticky top-0 z-50';
        default:
          return '';
      }
    }
    return '';
  };

  // Don't render if closed
  if (isClosed) return null;

  return (
    <AnimatePresence>
      <motion.div
        ref={adRef}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95 }}
        transition={{ duration: 0.3 }}
        className={`
          ad-slot
          ${getTierStyles()}
          ${getPositionStyles()}
          ${getBreakpointClasses()}
          ${className}
        `}
        style={{
          width: size[0] === -1 ? '100%' : size[0],
          minHeight: size[1],
          maxWidth: '100%',
        }}
        data-placement={placement}
        data-tier={tier}
        onClick={handleClick}
      >
        {/* Ad Label */}
        <div className="absolute top-2 left-2 z-10">
          <span className="text-[10px] text-gray-400 uppercase tracking-wider px-2 py-0.5 bg-black/20 rounded-full backdrop-blur-sm">
            Patrocinado
          </span>
        </div>

        {/* Close Button */}
        {closeable && (
          <button
            onClick={(e) => {
              e.stopPropagation();
              handleClose();
            }}
            className="absolute top-2 right-2 z-10 p-1.5 bg-black/30 hover:bg-black/50 rounded-full transition-colors"
            aria-label="Cerrar anuncio"
          >
            <X className="w-4 h-4 text-white" />
          </button>
        )}

        {/* Ad Content Container */}
        <div className="ad-content w-full h-full flex items-center justify-center p-4">
          {isVisible ? (
            // Ad network code injection point
            <div
              id={`ad-${placement}`}
              className="w-full h-full"
              data-ad-slot={placement}
            >
              {/* Placeholder for development */}
              {!adLoaded && (
                <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-500/10 to-emerald-500/10 rounded-lg">
                  <div className="text-center">
                    <div className="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-blue-500 to-emerald-500 rounded-xl flex items-center justify-center">
                      <span className="text-2xl">📢</span>
                    </div>
                    <p className="text-sm text-gray-400">Espacio Publicitario</p>
                    <p className="text-xs text-gray-500 mt-1">{placement}</p>
                    <p className="text-xs text-gray-600 mt-1">{size[0]}×{size[1]}</p>
                  </div>
                </div>
              )}
            </div>
          ) : (
            // Loading placeholder
            <div className="w-full h-full animate-pulse bg-white/5 rounded-lg" />
          )}
        </div>

        {/* Viewability indicator (development only) */}
        {process.env.NODE_ENV === 'development' && isViewed && (
          <div className="absolute bottom-2 right-2 z-10">
            <span className="text-[10px] text-green-400 px-2 py-0.5 bg-green-500/20 rounded-full">
              Viewed ✓
            </span>
          </div>
        )}
      </motion.div>
    </AnimatePresence>
  );
};

/**
 * Premium Banner Ad Component
 * For high-visibility banner placements
 */
export interface BannerAdProps {
  placement: string;
  width?: number;
  height: number;
  position?: 'top' | 'middle' | 'bottom';
  closeable?: boolean;
  className?: string;
}

export const BannerAd: React.FC<BannerAdProps> = ({
  placement,
  width = -1, // -1 means full width
  height,
  position = 'middle',
  closeable = false,
  className = '',
}) => {
  return (
    <div className={`w-full flex justify-center py-6 ${className}`}>
      <AdSlot
        placement={placement}
        size={[width, height]}
        tier="premium"
        position={position}
        closeable={closeable}
        className="max-w-7xl mx-auto"
      />
    </div>
  );
};

/**
 * Sidebar Ad Component
 * For sticky sidebar placements
 */
export interface SidebarAdProps {
  placement: string;
  ads?: Array<{ id: string; size: [number, number] }>;
  sticky?: boolean;
  className?: string;
}

export const SidebarAd: React.FC<SidebarAdProps> = ({
  placement,
  ads = [{ id: 'default', size: [300, 600] }],
  sticky = true,
  className = '',
}) => {
  return (
    <div className={`hidden lg:block space-y-6 ${className}`}>
      {ads.map((ad, index) => (
        <AdSlot
          key={ad.id}
          placement={`${placement}-${index}`}
          size={ad.size}
          tier={index === 0 ? 'high-value' : 'standard'}
          position="sidebar"
          sticky={sticky && index === 0}
        />
      ))}
    </div>
  );
};

/**
 * Native Listing Ad Component
 * Matches the design of organic listing cards
 */
export interface NativeListingAdProps {
  listing: {
    id: string;
    title: string;
    price: number;
    image: string;
    location: string;
    badge?: string;
  };
  onClick?: () => void;
}

export const NativeListingAd: React.FC<NativeListingAdProps> = ({
  listing,
  onClick,
}) => {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="group relative bg-white/5 backdrop-blur-md rounded-2xl overflow-hidden border border-white/10 hover:border-blue-500/30 transition-all duration-300 cursor-pointer"
      onClick={onClick}
    >
      {/* Sponsored Badge */}
      <div className="absolute top-4 left-4 z-10">
        <span className="text-xs font-medium text-white px-3 py-1.5 bg-gradient-to-r from-blue-500 to-emerald-500 rounded-full shadow-lg">
          ⭐ Destacado
        </span>
      </div>

      {/* Image */}
      <div className="aspect-[4/3] relative overflow-hidden">
        <img
          src={listing.image}
          alt={listing.title}
          className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent" />
      </div>

      {/* Content */}
      <div className="p-6">
        <h3 className="text-lg font-bold text-white mb-2 group-hover:text-blue-400 transition-colors">
          {listing.title}
        </h3>
        <p className="text-sm text-gray-400 mb-4 flex items-center gap-2">
          <span>📍</span>
          {listing.location}
        </p>
        <div className="flex items-center justify-between">
          <span className="text-2xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-blue-400 to-emerald-400">
            ${listing.price.toLocaleString()}
          </span>
          <span className="text-sm text-gray-500 uppercase tracking-wider">
            Patrocinado
          </span>
        </div>
      </div>
    </motion.div>
  );
};

/**
 * Mobile Bottom Banner Component
 * Sticky bottom banner for mobile devices
 */
export interface MobileBottomBannerProps {
  placement: string;
  autoCollapse?: boolean;
  collapseDelay?: number;
}

export const MobileBottomBanner: React.FC<MobileBottomBannerProps> = ({
  placement,
  autoCollapse = true,
  collapseDelay = 5000,
}) => {
  const [isCollapsed, setIsCollapsed] = useState(false);

  useEffect(() => {
    if (autoCollapse) {
      const timer = setTimeout(() => {
        setIsCollapsed(true);
      }, collapseDelay);

      return () => clearTimeout(timer);
    }
  }, [autoCollapse, collapseDelay]);

  if (isCollapsed) return null;

  return (
    <div className="lg:hidden fixed bottom-0 left-0 right-0 z-50 safe-area-pb">
      <AdSlot
        placement={placement}
        size={[-1, 50]}
        tier="high-value"
        position="bottom"
        closeable
        breakpoint="mobile"
        className="rounded-none border-t border-white/10"
      />
    </div>
  );
};
