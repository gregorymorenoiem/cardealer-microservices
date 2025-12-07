/**
 * FeaturedListingCard Component
 * Sprint 3: FeaturedListingCard Component
 * 
 * Enhanced vehicle card with featured listing badges and gradient borders
 * Follows 40% UX balance rule - subtle differentiation
 */

import { Link } from 'react-router-dom';
import { MapPin, Gauge, Calendar, Heart } from 'lucide-react';
import { useState } from 'react';
import type { Vehicle } from '@/data/mockVehicles';
import { getCardBorderClass, innerCardClass } from '@/utils/gradientBorders';
import { 
  DestacadoBadge, 
  PremiumBadge, 
  CertificadoBadge, 
  TopDealerBadge 
} from '@/components/atoms';

interface FeaturedListingCardProps {
  vehicle: Vehicle;
  priority?: 'high' | 'normal';
  context?: 'hero' | 'grid' | 'detail-similar' | 'detail-dealer' | 'browse';
  className?: string;
}

export default function FeaturedListingCard({ 
  vehicle, 
  priority = 'normal',
  context = 'grid',
  className = '' 
}: FeaturedListingCardProps) {
  const [isFavorite, setIsFavorite] = useState(false);
  
  // Determine which badge to show based on featured status
  const renderBadge = () => {
    if (!vehicle.tier || vehicle.tier === 'basic') return null;
    
    // Smaller badges for mobile
    const badgeSize = priority === 'high' ? 'md' : 'sm';
    
    switch (vehicle.featuredBadge) {
      case 'top-dealer':
        return <TopDealerBadge size={badgeSize} />;
      case 'premium':
        return <PremiumBadge size={badgeSize} />;
      case 'certificado':
        return <CertificadoBadge size={badgeSize} />;
      case 'destacado':
        return <DestacadoBadge size={badgeSize} />;
      default:
        return vehicle.tier === 'enterprise' ? <TopDealerBadge size={badgeSize} /> :
               vehicle.tier === 'premium' ? <PremiumBadge size={badgeSize} /> :
               vehicle.tier === 'featured' ? <DestacadoBadge size={badgeSize} /> :
               null;
    }
  };

  const borderClass = getCardBorderClass(vehicle.tier);
  const hasBorder = borderClass !== '';

  return (
    <div className={`${borderClass} ${className}`}>
      <div className={hasBorder ? innerCardClass : 'bg-white rounded-lg shadow-md hover:shadow-xl transition-shadow duration-300 overflow-hidden'}>
        <Link 
          to={`/vehicles/${vehicle.id}`}
          className="block group"
        >
          {/* Image Container */}
          <div className="relative aspect-[4/3] overflow-hidden">
            <img
              src={vehicle.images[0]}
              alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
              loading={priority === 'high' ? 'eager' : 'lazy'}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
            />
            
            {/* Badge Overlay - Top Right */}
            {renderBadge() && (
              <div className="absolute top-2 sm:top-3 right-2 sm:right-3">
                {renderBadge()}
              </div>
            )}
            
            {/* Favorite Button - Top Left */}
            <button
              onClick={(e) => {
                e.preventDefault();
                setIsFavorite(!isFavorite);
              }}
              className="absolute top-2 sm:top-3 left-2 sm:left-3 p-1.5 sm:p-2 bg-white/90 hover:bg-white active:bg-white rounded-full shadow-md transition-colors touch-manipulation"
              aria-label={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
            >
              <Heart 
                size={18} 
                className="sm:w-5 sm:h-5"
                fill={isFavorite ? '#ef4444' : 'none'}
                stroke={isFavorite ? '#ef4444' : '#6b7280'}
              />
            </button>

            {/* Condition Badge - Bottom Left */}
            {vehicle.condition === 'New' && (
              <div className="absolute bottom-2 sm:bottom-3 left-2 sm:left-3">
                <span className="bg-blue-600 text-white text-xs font-semibold px-2 sm:px-3 py-1 rounded-full">
                  Nuevo
                </span>
              </div>
            )}
            {vehicle.condition === 'Certified Pre-Owned' && (
              <div className="absolute bottom-2 sm:bottom-3 left-2 sm:left-3">
                <span className="bg-green-600 text-white text-xs font-semibold px-2 sm:px-3 py-1 rounded-full">
                  Certificado
                </span>
              </div>
            )}
          </div>

          {/* Content */}
          <div className="p-3 sm:p-4">
            {/* Title & Price */}
            <div className="mb-2 sm:mb-3">
              <h3 className="text-base sm:text-lg font-bold text-gray-900 group-hover:text-blue-600 transition-colors line-clamp-1">
                {vehicle.year} {vehicle.make} {vehicle.model}
              </h3>
              <p className="text-xl sm:text-2xl font-bold text-blue-600 mt-1">
                ${vehicle.price.toLocaleString('en-US')}
              </p>
            </div>

            {/* Key Details */}
            <div className="grid grid-cols-3 gap-2 sm:gap-3 mb-2 sm:mb-3 text-xs sm:text-sm text-gray-600">
              <div className="flex items-center gap-1">
                <Gauge size={14} className="sm:w-4 sm:h-4 text-gray-400 flex-shrink-0" />
                <span className="truncate">{vehicle.mileage.toLocaleString()} mi</span>
              </div>
              <div className="flex items-center gap-1">
                <Calendar size={14} className="sm:w-4 sm:h-4 text-gray-400 flex-shrink-0" />
                <span>{vehicle.year}</span>
              </div>
              <div className="flex items-center gap-1">
                <MapPin size={14} className="sm:w-4 sm:h-4 text-gray-400 flex-shrink-0" />
                <span className="truncate">{vehicle.location.split(',')[0]}</span>
              </div>
            </div>

            {/* Transmission & Fuel Type */}
            <div className="flex gap-1.5 sm:gap-2 mb-2 sm:mb-3 flex-wrap">
              <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">
                {vehicle.transmission}
              </span>
              <span className="text-xs bg-gray-100 text-gray-700 px-2 py-1 rounded">
                {vehicle.fuelType}
              </span>
            </div>

            {/* Dealer Info */}
            {vehicle.seller.type === 'Dealer' && (
              <div className="pt-2 sm:pt-3 border-t border-gray-100">
                <div className="flex items-center justify-between text-xs sm:text-sm">
                  <span className="text-gray-700 font-medium truncate mr-2">
                    {vehicle.seller.name}
                  </span>
                  <div className="flex items-center gap-1 flex-shrink-0">
                    <span className="text-yellow-500">★</span>
                    <span className="text-gray-600">{vehicle.seller.rating.toFixed(1)}</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </Link>
      </div>
    </div>
  );
}
