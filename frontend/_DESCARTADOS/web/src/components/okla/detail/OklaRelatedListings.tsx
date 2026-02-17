import { motion } from 'framer-motion';
import { ChevronLeft, ChevronRight, Heart, MapPin, Eye, Clock } from 'lucide-react';
import { OklaBadge } from '../../atoms/okla/OklaBadge';
import { useRef, useState } from 'react';

interface RelatedListing {
  id: string;
  title: string;
  price: number;
  image: string;
  location: string;
  isNew?: boolean;
  isFavorite?: boolean;
  views?: number;
  daysOnMarket?: number;
}

interface OklaRelatedListingsProps {
  listings: RelatedListing[];
  title?: string;
  subtitle?: string;
  onListingClick?: (id: string) => void;
  onFavorite?: (id: string) => void;
}

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(price);
};

export const OklaRelatedListings = ({
  listings,
  title = 'También te puede interesar',
  subtitle,
  onListingClick,
  onFavorite,
}: OklaRelatedListingsProps) => {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);

  const checkScroll = () => {
    if (scrollContainerRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollContainerRef.current;
      setCanScrollLeft(scrollLeft > 0);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  const scroll = (direction: 'left' | 'right') => {
    if (scrollContainerRef.current) {
      const scrollAmount = 320;
      scrollContainerRef.current.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth',
      });
    }
  };

  if (listings.length === 0) return null;

  return (
    <section className="py-12">
      {/* Header */}
      <div className="flex items-end justify-between mb-8">
        <div>
          <h2 className="text-2xl md:text-3xl font-display font-bold text-okla-navy">
            {title}
          </h2>
          {subtitle && (
            <p className="text-okla-slate mt-1">{subtitle}</p>
          )}
        </div>

        {/* Navigation Arrows */}
        <div className="hidden sm:flex items-center gap-2">
          <motion.button
            onClick={() => scroll('left')}
            className={`w-10 h-10 rounded-full border flex items-center justify-center transition-colors ${
              canScrollLeft
                ? 'border-okla-navy text-okla-navy hover:bg-okla-navy hover:text-white'
                : 'border-gray-200 text-gray-300 cursor-not-allowed'
            }`}
            disabled={!canScrollLeft}
            whileHover={canScrollLeft ? { scale: 1.05 } : {}}
            whileTap={canScrollLeft ? { scale: 0.95 } : {}}
          >
            <ChevronLeft className="w-5 h-5" />
          </motion.button>
          <motion.button
            onClick={() => scroll('right')}
            className={`w-10 h-10 rounded-full border flex items-center justify-center transition-colors ${
              canScrollRight
                ? 'border-okla-navy text-okla-navy hover:bg-okla-navy hover:text-white'
                : 'border-gray-200 text-gray-300 cursor-not-allowed'
            }`}
            disabled={!canScrollRight}
            whileHover={canScrollRight ? { scale: 1.05 } : {}}
            whileTap={canScrollRight ? { scale: 0.95 } : {}}
          >
            <ChevronRight className="w-5 h-5" />
          </motion.button>
        </div>
      </div>

      {/* Listings Carousel */}
      <div
        ref={scrollContainerRef}
        onScroll={checkScroll}
        className="flex gap-6 overflow-x-auto pb-4 scrollbar-hide snap-x snap-mandatory"
        style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
      >
        {listings.map((listing, index) => (
          <motion.div
            key={listing.id}
            className="flex-shrink-0 w-[300px] snap-start"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.1 }}
          >
            <div
              className="bg-white rounded-xl border border-okla-cream overflow-hidden shadow-sm hover:shadow-lg transition-shadow cursor-pointer group"
              onClick={() => onListingClick?.(listing.id)}
            >
              {/* Image */}
              <div className="relative aspect-[4/3] overflow-hidden">
                <img
                  src={listing.image}
                  alt={listing.title}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                />

                {/* Badges */}
                <div className="absolute top-3 left-3">
                  {listing.isNew && (
                    <OklaBadge variant="solid">Nuevo</OklaBadge>
                  )}
                </div>

                {/* Favorite Button */}
                <motion.button
                  onClick={(e) => {
                    e.stopPropagation();
                    onFavorite?.(listing.id);
                  }}
                  className={`absolute top-3 right-3 w-9 h-9 rounded-full flex items-center justify-center transition-colors ${
                    listing.isFavorite
                      ? 'bg-red-500 text-white'
                      : 'bg-white/90 backdrop-blur-sm text-okla-navy hover:bg-white'
                  }`}
                  whileHover={{ scale: 1.1 }}
                  whileTap={{ scale: 0.9 }}
                >
                  <Heart
                    className={`w-4 h-4 ${listing.isFavorite ? 'fill-current' : ''}`}
                  />
                </motion.button>

                {/* Price Overlay */}
                <div className="absolute bottom-0 inset-x-0 bg-gradient-to-t from-black/60 to-transparent p-4">
                  <p className="text-white font-display font-bold text-xl">
                    {formatPrice(listing.price)}
                  </p>
                </div>
              </div>

              {/* Content */}
              <div className="p-4">
                <h3 className="font-semibold text-okla-navy group-hover:text-okla-gold transition-colors line-clamp-2 mb-2">
                  {listing.title}
                </h3>

                <div className="flex items-center justify-between text-sm text-okla-slate">
                  <span className="flex items-center gap-1">
                    <MapPin className="w-3.5 h-3.5" />
                    {listing.location}
                  </span>

                  <div className="flex items-center gap-3">
                    {listing.views && (
                      <span className="flex items-center gap-1">
                        <Eye className="w-3.5 h-3.5" />
                        {listing.views}
                      </span>
                    )}
                    {listing.daysOnMarket !== undefined && (
                      <span className="flex items-center gap-1">
                        <Clock className="w-3.5 h-3.5" />
                        {listing.daysOnMarket}d
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </motion.div>
        ))}
      </div>
    </section>
  );
};

export default OklaRelatedListings;
