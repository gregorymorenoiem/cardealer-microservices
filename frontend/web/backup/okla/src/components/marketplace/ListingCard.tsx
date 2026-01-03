/**
 * ListingCard - Unified card component for marketplace listings
 * Adapts to vehicle or property listings with appropriate display
 * Optimized for low-bandwidth with lazy loading images
 */

import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import type { Listing, VehicleListing, PropertyListing } from '@/types/marketplace';
import { OptimizedImage } from '@/components/performance';
import { LocalizedContent } from '@/components/common';

interface ListingCardProps {
  listing: Listing;
  variant?: 'default' | 'compact' | 'featured';
  showBadges?: boolean;
  className?: string;
  priority?: boolean; // For above-the-fold images
}

export const ListingCard: React.FC<ListingCardProps> = ({
  listing,
  variant = 'default',
  showBadges = true,
  className = '',
  priority = false,
}) => {
  const { t } = useTranslation('common');
  const isVehicle = listing.vertical === 'vehicles';
  const isProperty = listing.vertical === 'real-estate';

  const getDetailUrl = (): string => {
    if (isVehicle) {
      return `/vehicles/${(listing as VehicleListing).id}`;
    }
    if (isProperty) {
      return `/properties/${(listing as PropertyListing).id}`;
    }
    // This shouldn't happen with current verticals, but TypeScript needs the fallback
    return `/listings/${(listing as VehicleListing | PropertyListing).id}`;
  };

  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: currency,
      maximumFractionDigits: 0,
    }).format(price);
  };

  if (variant === 'compact') {
    return (
      <CompactCard
        listing={listing}
        detailUrl={getDetailUrl()}
        formatPrice={formatPrice}
        className={className}
      />
    );
  }

  if (variant === 'featured') {
    return (
      <FeaturedCard
        listing={listing}
        detailUrl={getDetailUrl()}
        formatPrice={formatPrice}
        showBadges={showBadges}
        className={className}
      />
    );
  }

  // Default card
  return (
    <motion.div
      whileHover={{ y: -4 }}
      className={`group bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-xl transition-all duration-300 ${className}`}
    >
      <Link to={getDetailUrl()} className="block">
        {/* Image - Optimized with lazy loading for low-bandwidth */}
        <div className="relative aspect-[16/10] overflow-hidden">
          <OptimizedImage
            src={listing.primaryImageUrl || listing.images[0]?.url || '/images/placeholder-image.svg'}
            alt={listing.title}
            className="w-full h-full group-hover:scale-105 transition-transform duration-500"
            priority={priority}
            placeholder="blur"
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
          />
          
          {/* Badges */}
          {showBadges && (
            <div className="absolute top-3 left-3 flex flex-wrap gap-2 z-20">
              {listing.isFeatured && (
                <span className="px-2.5 py-1 bg-gradient-to-r from-amber-500 to-orange-500 text-white text-xs font-semibold rounded-full shadow-lg">
                  ⭐ {t('listing.featured')}
                </span>
              )}
              {isVehicle && (listing as VehicleListing).condition === 'new' && (
                <span className="px-2.5 py-1 bg-emerald-500 text-white text-xs font-semibold rounded-full">
                  {t('listing.new')}
                </span>
              )}
              {isProperty && (
                <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${
                  (listing as PropertyListing).listingType === 'sale'
                    ? 'bg-blue-500 text-white'
                    : 'bg-purple-500 text-white'
                }`}>
                  {(listing as PropertyListing).listingType === 'sale' ? t('listing.sale') : t('listing.rent')}
                </span>
              )}
            </div>
          )}

          {/* Favorite button */}
          <button
            onClick={(e) => {
              e.preventDefault();
              // TODO: Toggle favorite
            }}
            className="absolute top-3 right-3 w-9 h-9 bg-white/90 rounded-full flex items-center justify-center shadow-md hover:bg-white transition-colors"
          >
            <svg className="w-5 h-5 text-gray-600 hover:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
          </button>

          {/* Category indicator */}
          <div className="absolute bottom-3 left-3">
            <span className="px-2 py-1 bg-black/50 backdrop-blur-sm text-white text-xs rounded-md">
              {isVehicle ? `🚗 ${t('listing.vehicle')}` : `🏠 ${t('listing.property')}`}
            </span>
          </div>
        </div>

        {/* Content */}
        <div className="p-4">
          {/* Title */}
          <h3 className="font-semibold text-gray-900 text-lg line-clamp-1 group-hover:text-blue-600 transition-colors">
            <LocalizedContent 
              content={listing.title}
              showBadge={false}
            />
          </h3>

          {/* Subtitle / specs */}
          <div className="mt-1 text-sm text-gray-500">
            {isVehicle && <VehicleSpecs vehicle={listing as VehicleListing} />}
            {isProperty && <PropertySpecs property={listing as PropertyListing} />}
          </div>

          {/* Location */}
          <div className="mt-2 flex items-center text-sm text-gray-500">
            <svg className="w-4 h-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            {listing.location.city}, {listing.location.state}
          </div>

          {/* Price */}
          <div className="mt-3 flex items-center justify-between">
            <div>
              <span className="text-xl font-bold text-gray-900">
                {formatPrice(listing.price, listing.currency)}
              </span>
              {isProperty && (listing as PropertyListing).listingType === 'rent' && (
                <span className="text-sm text-gray-500">{t('listing.perMonth')}</span>
              )}
            </div>

            {/* Quick stats */}
            <div className="flex items-center gap-3 text-xs text-gray-400">
              <span className="flex items-center gap-1">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                </svg>
                {listing.viewCount}
              </span>
              <span className="flex items-center gap-1">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
                {listing.favoriteCount}
              </span>
            </div>
          </div>
        </div>
      </Link>
    </motion.div>
  );
};

// Vehicle specifications display
const VehicleSpecs: React.FC<{ vehicle: VehicleListing }> = ({ vehicle }) => (
  <div className="flex items-center gap-2 flex-wrap">
    <span>{vehicle.year}</span>
    <span className="text-gray-300">•</span>
    <span>{vehicle.mileage.toLocaleString()} {vehicle.mileageUnit}</span>
    <span className="text-gray-300">•</span>
    <span className="capitalize">{vehicle.transmission}</span>
    <span className="text-gray-300">•</span>
    <span className="capitalize">{vehicle.fuelType.replace('-', ' ')}</span>
  </div>
);

// Property specifications display
const PropertySpecs: React.FC<{ property: PropertyListing }> = ({ property }) => {
  const { t } = useTranslation('common');
  return (
    <div className="flex items-center gap-3">
      {property.bedrooms > 0 && (
        <span className="flex items-center gap-1">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
          </svg>
          {property.bedrooms} {t('listing.bedrooms')}
        </span>
      )}
      {property.bathrooms > 0 && (
        <span className="flex items-center gap-1">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 14v3m4-3v3m4-3v3M3 21h18M3 10h18M3 7l9-4 9 4M4 10h16v11H4V10z" />
          </svg>
          {property.bathrooms} {t('listing.bathrooms')}
        </span>
      )}
      <span className="flex items-center gap-1">
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
        </svg>
        {property.totalArea} {t('listing.sqMeters')}
      </span>
    </div>
  );
};

// Compact card variant
const CompactCard: React.FC<{
  listing: Listing;
  detailUrl: string;
  formatPrice: (price: number, currency: string) => string;
  className: string;
}> = ({ listing, detailUrl, formatPrice, className }) => (
  <Link to={detailUrl} className={`flex gap-3 p-2 rounded-lg hover:bg-gray-50 transition-colors ${className}`}>
    <img
      src={listing.primaryImageUrl || listing.images[0]?.url || '/placeholder.jpg'}
      alt={listing.title}
      className="w-20 h-20 rounded-lg object-cover flex-shrink-0"
    />
    <div className="flex-1 min-w-0">
      <h4 className="font-medium text-gray-900 text-sm line-clamp-1">
        <LocalizedContent 
          content={listing.title}
          showBadge={false}
        />
      </h4>
      <p className="text-xs text-gray-500 mt-0.5">
        {listing.location.city}
      </p>
      <p className="text-sm font-semibold text-gray-900 mt-1">
        {formatPrice(listing.price, listing.currency)}
      </p>
    </div>
  </Link>
);

// Featured card variant
const FeaturedCard: React.FC<{
  listing: Listing;
  detailUrl: string;
  formatPrice: (price: number, currency: string) => string;
  showBadges: boolean;
  className: string;
}> = ({ listing, detailUrl, formatPrice, showBadges, className }) => {
  const { t } = useTranslation('common');
  const isVehicle = listing.vertical === 'vehicles';
  const isProperty = listing.vertical === 'real-estate';

  return (
    <motion.div
      whileHover={{ y: -6, scale: 1.02 }}
      className={`relative group rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all duration-300 ${className}`}
    >
      <Link to={detailUrl} className="block">
        {/* Large image */}
        <div className="relative aspect-[4/3] overflow-hidden">
          <img
            src={listing.primaryImageUrl || listing.images[0]?.url || '/placeholder.jpg'}
            alt={listing.title}
            className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
          />
          
          {/* Gradient overlay */}
          <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

          {/* Badges */}
          {showBadges && (
            <div className="absolute top-4 left-4 flex flex-wrap gap-2">
              <span className="px-3 py-1 bg-gradient-to-r from-amber-500 to-orange-500 text-white text-xs font-bold rounded-full shadow-lg">
                ⭐ {t('listing.featured').toUpperCase()}
              </span>
              <span className="px-3 py-1 bg-white/90 text-gray-900 text-xs font-medium rounded-full">
                {isVehicle ? `🚗 ${t('listing.vehicle')}` : `🏠 ${t('listing.property')}`}
              </span>
            </div>
          )}

          {/* Content overlay */}
          <div className="absolute bottom-0 left-0 right-0 p-5">
            <h3 className="text-xl font-bold text-white mb-2 drop-shadow-lg">
              <LocalizedContent 
                content={listing.title}
                showBadge={false}
              />
            </h3>
            
            <div className="flex items-center justify-between">
              <div>
                <span className="text-2xl font-bold text-white drop-shadow-lg">
                  {formatPrice(listing.price, listing.currency)}
                </span>
                {isProperty && (listing as PropertyListing).listingType === 'rent' && (
                  <span className="text-white/80 text-sm">{t('listing.perMonth')}</span>
                )}
              </div>

              <div className="flex items-center gap-1 text-white/80 text-sm">
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                </svg>
                {listing.location.city}
              </div>
            </div>

            {/* Quick specs */}
            <div className="mt-3 flex items-center gap-3 text-white/70 text-sm">
              {isVehicle && (
                <>
                  <span>{(listing as VehicleListing).year}</span>
                  <span>•</span>
                  <span>{(listing as VehicleListing).mileage.toLocaleString()} mi</span>
                  <span>•</span>
                  <span className="capitalize">{(listing as VehicleListing).transmission}</span>
                </>
              )}
              {isProperty && (
                <>
                  <span>{(listing as PropertyListing).bedrooms} {t('listing.bedrooms')}</span>
                  <span>•</span>
                  <span>{(listing as PropertyListing).bathrooms} {t('listing.bathrooms')}</span>
                  <span>•</span>
                  <span>{(listing as PropertyListing).totalArea} {t('listing.sqMeters')}</span>
                </>
              )}
            </div>
          </div>
        </div>
      </Link>
    </motion.div>
  );
};

export default ListingCard;
