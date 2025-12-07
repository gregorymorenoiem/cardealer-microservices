import { motion } from 'framer-motion';
import { ArrowRight, Star, Heart, Eye, ChevronLeft, ChevronRight } from 'lucide-react';
import { useState, useRef } from 'react';
import { FadeIn } from '../animations/FadeIn';
import { HoverLift } from '../animations/ScaleEffects';
import { OklaButton } from '../../atoms/okla/OklaButton';
import { OklaBadge } from '../../atoms/okla/OklaBadge';

interface FeaturedListing {
  id: string;
  title: string;
  price: number;
  currency?: string;
  image: string;
  category: string;
  location: string;
  rating?: number;
  reviews?: number;
  featured?: boolean;
  isNew?: boolean;
  views?: number;
}

interface OklaFeaturedGridProps {
  title?: string;
  subtitle?: string;
  listings: FeaturedListing[];
  onViewAll?: () => void;
  onListingClick?: (id: string) => void;
  onFavorite?: (id: string) => void;
}

const formatPrice = (price: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
};

export const OklaFeaturedGrid = ({
  title = 'Destacados de la Semana',
  subtitle = 'Selección exclusiva de nuestros mejores listados',
  listings,
  onViewAll,
  onListingClick,
  onFavorite,
}: OklaFeaturedGridProps) => {
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(true);

  const checkScrollButtons = () => {
    if (scrollContainerRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollContainerRef.current;
      setCanScrollLeft(scrollLeft > 0);
      setCanScrollRight(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  const scroll = (direction: 'left' | 'right') => {
    if (scrollContainerRef.current) {
      const scrollAmount = 400;
      scrollContainerRef.current.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth',
      });
      setTimeout(checkScrollButtons, 300);
    }
  };

  return (
    <section className="py-12 px-6 bg-okla-cream">
      <div className="max-w-7xl mx-auto">
        {/* Section Header */}
        <div className="flex flex-col md:flex-row md:items-end justify-between mb-8">
          <FadeIn>
            <div>
              <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
                Exclusivo
              </span>
              <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
                {title}
              </h2>
              <p className="text-okla-slate mt-3 max-w-xl">{subtitle}</p>
            </div>
          </FadeIn>
          <FadeIn delay={0.2}>
            <div className="flex items-center gap-4 mt-6 md:mt-0">
              {/* Navigation Arrows */}
              <div className="flex items-center gap-2">
                <motion.button
                  onClick={() => scroll('left')}
                  disabled={!canScrollLeft}
                  className={`w-12 h-12 rounded-full border-2 flex items-center justify-center transition-all ${
                    canScrollLeft
                      ? 'border-gold-500 text-gold-600 hover:bg-gold-500 hover:text-white'
                      : 'border-gray-300 text-gray-300 cursor-not-allowed'
                  }`}
                  whileHover={canScrollLeft ? { scale: 1.05 } : {}}
                  whileTap={canScrollLeft ? { scale: 0.95 } : {}}
                >
                  <ChevronLeft className="w-6 h-6" />
                </motion.button>
                <motion.button
                  onClick={() => scroll('right')}
                  disabled={!canScrollRight}
                  className={`w-12 h-12 rounded-full border-2 flex items-center justify-center transition-all ${
                    canScrollRight
                      ? 'border-gold-500 text-gold-600 hover:bg-gold-500 hover:text-white'
                      : 'border-gray-300 text-gray-300 cursor-not-allowed'
                  }`}
                  whileHover={canScrollRight ? { scale: 1.05 } : {}}
                  whileTap={canScrollRight ? { scale: 0.95 } : {}}
                >
                  <ChevronRight className="w-6 h-6" />
                </motion.button>
              </div>
              <OklaButton
                variant="outline"
                onClick={onViewAll}
                rightIcon={<ArrowRight className="w-4 h-4" />}
              >
                Ver Todos
              </OklaButton>
            </div>
          </FadeIn>
        </div>

        {/* Listings Carousel */}
        <div className="relative">

          {/* Carousel Container */}
          <div className="px-8">
          <div 
            ref={scrollContainerRef}
            onScroll={checkScrollButtons}
            className="flex gap-8 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4 snap-x snap-mandatory"
            style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
          >
            {listings.map((listing) => (
              <div key={listing.id} className="flex-shrink-0 w-[350px] snap-start">
                <HoverLift>
                  <motion.article
                    className="bg-white rounded-2xl overflow-hidden shadow-lg cursor-pointer group h-full"
                    onClick={() => onListingClick?.(listing.id)}
                    whileHover={{ boxShadow: '0 20px 40px rgba(10, 25, 47, 0.15)' }}
                  >
                  {/* Image Container */}
                  <div className="relative aspect-[4/3] overflow-hidden">
                    <motion.img
                      src={listing.image}
                      alt={listing.title}
                      className="w-full h-full object-cover"
                      whileHover={{ scale: 1.05 }}
                      transition={{ duration: 0.6 }}
                    />
                    
                    {/* Gradient Overlay */}
                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                    
                    {/* Badges */}
                    <div className="absolute top-4 left-4 flex gap-2">
                      {listing.featured && (
                        <OklaBadge variant="solid" color="gold">
                          <Star className="w-3 h-3 mr-1" fill="currentColor" />
                          Destacado
                        </OklaBadge>
                      )}
                      {listing.isNew && (
                        <OklaBadge variant="solid" color="green">Nuevo</OklaBadge>
                      )}
                    </div>

                    {/* Favorite Button */}
                    <motion.button
                      className="absolute top-4 right-4 p-2 bg-white/90 rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity"
                      whileHover={{ scale: 1.1 }}
                      whileTap={{ scale: 0.9 }}
                      onClick={(e) => {
                        e.stopPropagation();
                        onFavorite?.(listing.id);
                      }}
                    >
                      <Heart className="w-5 h-5 text-okla-navy" />
                    </motion.button>

                    {/* Quick Stats */}
                    <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between opacity-0 group-hover:opacity-100 transition-opacity">
                      {listing.views && (
                        <div className="flex items-center gap-1 text-white text-sm">
                          <Eye className="w-4 h-4" />
                          {listing.views.toLocaleString()}
                        </div>
                      )}
                      {listing.rating && (
                        <div className="flex items-center gap-1 text-white text-sm">
                          <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
                          {listing.rating.toFixed(1)}
                          {listing.reviews && (
                            <span className="text-white/70">({listing.reviews})</span>
                          )}
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Content */}
                  <div className="p-6">
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex-1 min-w-0">
                        <span className="text-okla-gold text-sm font-medium">
                          {listing.category}
                        </span>
                        <h3 className="text-xl font-semibold text-okla-navy mt-1 truncate group-hover:text-okla-gold transition-colors">
                          {listing.title}
                        </h3>
                        <p className="text-okla-slate text-sm mt-1">{listing.location}</p>
                      </div>
                      <div className="text-right">
                        <span className="text-2xl font-bold text-okla-navy">
                          {formatPrice(listing.price, listing.currency)}
                        </span>
                      </div>
                    </div>
                  </div>
                </motion.article>
              </HoverLift>
              </div>
            ))}
          </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default OklaFeaturedGrid;
