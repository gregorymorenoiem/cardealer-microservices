import { motion } from 'framer-motion';
import { Heart, Star, MapPin, Eye, Calendar, Check } from 'lucide-react';
import { useState } from 'react';
import { OklaBadge } from '../../atoms/okla/OklaBadge';

interface OklaListingCardProps {
  id: string;
  title: string;
  price: number;
  currency?: string;
  image: string;
  images?: string[];
  category: string;
  location: string;
  rating?: number;
  reviews?: number;
  featured?: boolean;
  isNew?: boolean;
  verified?: boolean;
  views?: number;
  date?: string;
  specs?: { label: string; value: string }[];
  onClick?: () => void;
  onFavorite?: () => void;
  isFavorite?: boolean;
  variant?: 'default' | 'horizontal' | 'compact';
}

const formatPrice = (price: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
};

export const OklaListingCard = ({
  id,
  title,
  price,
  currency = 'USD',
  image,
  images = [],
  category,
  location,
  rating,
  reviews,
  featured,
  isNew,
  verified,
  views,
  date,
  specs = [],
  onClick,
  onFavorite,
  isFavorite = false,
  variant = 'default',
}: OklaListingCardProps) => {
  const [currentImage, setCurrentImage] = useState(0);
  const [isHovered, setIsHovered] = useState(false);
  const allImages = images.length > 0 ? images : [image];

  const handleFavoriteClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    onFavorite?.();
  };

  if (variant === 'horizontal') {
    return (
      <motion.article
        className="flex bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-xl transition-shadow cursor-pointer group"
        onClick={onClick}
        whileHover={{ y: -4 }}
        transition={{ duration: 0.3 }}
      >
        {/* Image */}
        <div className="relative w-72 flex-shrink-0 overflow-hidden">
          <motion.img
            src={allImages[currentImage]}
            alt={title}
            className="w-full h-full object-cover"
            whileHover={{ scale: 1.05 }}
            transition={{ duration: 0.6 }}
          />
          <div className="absolute top-3 left-3 flex gap-2">
            {featured && (
              <OklaBadge variant="premium">
                <Star className="w-3 h-3 mr-1" fill="currentColor" />
                Destacado
              </OklaBadge>
            )}
            {isNew && <OklaBadge variant="success">Nuevo</OklaBadge>}
          </div>
          <motion.button
            className="absolute top-3 right-3 p-2 bg-white/90 rounded-full shadow-md"
            whileHover={{ scale: 1.1 }}
            whileTap={{ scale: 0.9 }}
            onClick={handleFavoriteClick}
          >
            <Heart
              className={`w-5 h-5 ${isFavorite ? 'text-red-500 fill-red-500' : 'text-okla-navy'}`}
            />
          </motion.button>
        </div>

        {/* Content */}
        <div className="flex-1 p-6 flex flex-col justify-between">
          <div>
            <div className="flex items-center gap-2 mb-2">
              <span className="text-okla-gold text-sm font-medium">{category}</span>
              {verified && (
                <span className="flex items-center gap-1 text-green-600 text-sm">
                  <Check className="w-4 h-4" />
                  Verificado
                </span>
              )}
            </div>
            <h3 className="text-xl font-semibold text-okla-navy group-hover:text-okla-gold transition-colors">
              {title}
            </h3>
            <div className="flex items-center gap-2 mt-2 text-okla-slate text-sm">
              <MapPin className="w-4 h-4" />
              {location}
            </div>
            {specs.length > 0 && (
              <div className="flex flex-wrap gap-4 mt-4">
                {specs.slice(0, 4).map((spec, index) => (
                  <div key={index} className="text-sm">
                    <span className="text-okla-slate">{spec.label}: </span>
                    <span className="text-okla-navy font-medium">{spec.value}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
          <div className="flex items-end justify-between mt-4">
            <span className="text-2xl font-bold text-okla-navy">
              {formatPrice(price, currency)}
            </span>
            <div className="flex items-center gap-4 text-sm text-okla-slate">
              {rating && (
                <div className="flex items-center gap-1">
                  <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
                  {rating.toFixed(1)}
                </div>
              )}
              {views && (
                <div className="flex items-center gap-1">
                  <Eye className="w-4 h-4" />
                  {views.toLocaleString()}
                </div>
              )}
            </div>
          </div>
        </div>
      </motion.article>
    );
  }

  if (variant === 'compact') {
    return (
      <motion.article
        className="bg-white rounded-xl overflow-hidden shadow-sm hover:shadow-lg transition-shadow cursor-pointer group"
        onClick={onClick}
        whileHover={{ y: -2 }}
        transition={{ duration: 0.2 }}
      >
        <div className="flex gap-4 p-4">
          <div className="relative w-24 h-24 rounded-lg overflow-hidden flex-shrink-0">
            <img src={image} alt={title} className="w-full h-full object-cover" />
          </div>
          <div className="flex-1 min-w-0">
            <span className="text-okla-gold text-xs font-medium">{category}</span>
            <h3 className="text-sm font-semibold text-okla-navy truncate group-hover:text-okla-gold transition-colors">
              {title}
            </h3>
            <p className="text-xs text-okla-slate mt-1 truncate">{location}</p>
            <span className="text-lg font-bold text-okla-navy mt-2 block">
              {formatPrice(price, currency)}
            </span>
          </div>
        </div>
      </motion.article>
    );
  }

  // Default variant
  return (
    <motion.article
      className="bg-white rounded-2xl overflow-hidden shadow-sm hover:shadow-xl transition-all cursor-pointer group"
      onClick={onClick}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      whileHover={{ y: -8 }}
      transition={{ duration: 0.3 }}
    >
      {/* Image Container */}
      <div className="relative aspect-[4/3] overflow-hidden">
        <motion.img
          src={allImages[currentImage]}
          alt={title}
          className="w-full h-full object-cover"
          animate={{ scale: isHovered ? 1.05 : 1 }}
          transition={{ duration: 0.6 }}
        />

        {/* Image Navigation Dots */}
        {allImages.length > 1 && (
          <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5">
            {allImages.map((_, index) => (
              <button
                key={index}
                onClick={(e) => {
                  e.stopPropagation();
                  setCurrentImage(index);
                }}
                className={`w-2 h-2 rounded-full transition-all ${
                  index === currentImage
                    ? 'bg-white w-4'
                    : 'bg-white/50 hover:bg-white/75'
                }`}
              />
            ))}
          </div>
        )}

        {/* Gradient Overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

        {/* Badges */}
        <div className="absolute top-4 left-4 flex gap-2">
          {featured && (
            <OklaBadge variant="premium">
              <Star className="w-3 h-3 mr-1" fill="currentColor" />
              Destacado
            </OklaBadge>
          )}
          {isNew && <OklaBadge variant="success">Nuevo</OklaBadge>}
          {verified && (
            <OklaBadge variant="info">
              <Check className="w-3 h-3 mr-1" />
              Verificado
            </OklaBadge>
          )}
        </div>

        {/* Favorite Button */}
        <motion.button
          className={`absolute top-4 right-4 p-2.5 rounded-full shadow-md transition-all ${
            isFavorite
              ? 'bg-red-500 text-white'
              : 'bg-white/90 text-okla-navy hover:bg-white'
          }`}
          whileHover={{ scale: 1.1 }}
          whileTap={{ scale: 0.9 }}
          onClick={handleFavoriteClick}
        >
          <Heart className={`w-5 h-5 ${isFavorite ? 'fill-current' : ''}`} />
        </motion.button>

        {/* Quick Stats on Hover */}
        <motion.div
          className="absolute bottom-4 left-4 right-4 flex items-center justify-between"
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: isHovered ? 1 : 0, y: isHovered ? 0 : 10 }}
          transition={{ duration: 0.2 }}
        >
          {views && (
            <div className="flex items-center gap-1 text-white text-sm">
              <Eye className="w-4 h-4" />
              {views.toLocaleString()} vistas
            </div>
          )}
          {date && (
            <div className="flex items-center gap-1 text-white text-sm">
              <Calendar className="w-4 h-4" />
              {date}
            </div>
          )}
        </motion.div>
      </div>

      {/* Content */}
      <div className="p-5">
        <div className="flex items-center justify-between mb-2">
          <span className="text-okla-gold text-sm font-medium">{category}</span>
          {rating && (
            <div className="flex items-center gap-1 text-sm">
              <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
              <span className="text-okla-navy font-medium">{rating.toFixed(1)}</span>
              {reviews && (
                <span className="text-okla-slate">({reviews})</span>
              )}
            </div>
          )}
        </div>

        <h3 className="text-lg font-semibold text-okla-navy group-hover:text-okla-gold transition-colors line-clamp-2">
          {title}
        </h3>

        <div className="flex items-center gap-1.5 mt-2 text-okla-slate text-sm">
          <MapPin className="w-4 h-4 flex-shrink-0" />
          <span className="truncate">{location}</span>
        </div>

        {/* Specs Preview */}
        {specs.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-4">
            {specs.slice(0, 3).map((spec, index) => (
              <span
                key={index}
                className="px-2 py-1 bg-okla-cream rounded text-xs text-okla-charcoal"
              >
                {spec.value}
              </span>
            ))}
          </div>
        )}

        {/* Price */}
        <div className="flex items-end justify-between mt-4 pt-4 border-t border-okla-cream">
          <div>
            <span className="text-2xl font-bold text-okla-navy">
              {formatPrice(price, currency)}
            </span>
          </div>
          <motion.span
            className="text-okla-gold text-sm font-medium flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity"
            whileHover={{ x: 5 }}
          >
            Ver detalles →
          </motion.span>
        </div>
      </div>
    </motion.article>
  );
};

export default OklaListingCard;
