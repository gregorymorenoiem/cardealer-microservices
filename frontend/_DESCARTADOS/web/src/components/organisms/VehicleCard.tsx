import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FiMapPin, FiActivity, FiHeart, FiBarChart2 } from 'react-icons/fi';
import { formatPrice, formatMileage } from '@/utils/formatters';
import { useFavorites } from '@/hooks/useFavorites';
import { useCompare } from '@/hooks/useCompare';
import { generateVehicleUrl } from '@/utils/seoSlug';

export interface VehicleCardProps {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  location: string;
  imageUrl?: string;
  isFeatured?: boolean;
  isNew?: boolean;
  transmission?: string;
  fuelType?: string;
}

export default function VehicleCard({
  id,
  make,
  model,
  year,
  price,
  mileage,
  location,
  imageUrl,
  isFeatured = false,
  isNew = false,
  transmission,
  fuelType,
}: VehicleCardProps) {
  const { t } = useTranslation(['vehicles', 'common']);
  const defaultImage = 'https://via.placeholder.com/400x300?text=No+Image';
  const { isFavorite, toggleFavorite } = useFavorites();
  const { isInCompare, addToCompare, removeFromCompare } = useCompare();
  const isLiked = isFavorite(id);
  const inCompare = isInCompare(id);
  
  // Generate SEO-friendly URL
  const vehicleUrl = generateVehicleUrl({ id, year, make, model });

  return (
    <Link
      to={vehicleUrl}
      className="block group bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-lg transition-all duration-300 cursor-pointer"
    >
      {/* Image Container */}
      <div className="relative aspect-[4/3] overflow-hidden bg-gray-200">
        <img
          src={imageUrl || defaultImage}
          alt={`${year} ${make} ${model}`}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
        />
        
        {/* Badges */}
        <div className="absolute top-3 left-3 flex gap-2">
          {isFeatured && (
            <span className="px-2 py-1 bg-amber-500 text-white text-xs font-semibold rounded-full">
              {t('vehicles:card.featured')}
            </span>
          )}
          {isNew && (
            <span className="px-2 py-1 bg-green-500 text-white text-xs font-semibold rounded-full">
              {t('vehicles:card.new')}
            </span>
          )}
        </div>

        {/* Action Buttons */}
        <div className="absolute top-3 right-3 flex gap-2 z-10">
          {/* Compare Button */}
          <button
            type="button"
            className={`
              w-8 h-8 rounded-full flex items-center justify-center transition-all shadow-sm
              ${inCompare ? 'bg-blue-500 hover:bg-blue-600 text-white' : 'bg-white/90 hover:bg-white text-gray-700'}
            `}
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              if (inCompare) {
                removeFromCompare(id);
              } else {
                addToCompare(id);
              }
            }}
            title={inCompare ? t('common:buttons.removeFromCompare') : t('common:buttons.addToCompare')}
          >
            <FiBarChart2 size={16} />
          </button>

          {/* Favorite Button */}
          <button
            type="button"
            className={`
              w-8 h-8 rounded-full flex items-center justify-center transition-all shadow-sm
              ${isLiked ? 'bg-red-500 hover:bg-red-600' : 'bg-white/90 hover:bg-white'}
            `}
            onClick={(e) => {
              e.preventDefault();
              e.stopPropagation();
              toggleFavorite(id);
            }}
            title={isLiked ? t('common:buttons.removeFromFavorites') : t('common:buttons.addToFavorites')}
          >
            <FiHeart 
              className={isLiked ? 'text-white fill-white' : 'text-gray-700'} 
              size={16} 
            />
          </button>
        </div>

        {/* Vehicle Type Badge */}
        {(transmission || fuelType) && (
          <div className="absolute bottom-3 left-3 flex gap-2">
            {transmission && (
              <span className="px-2 py-1 bg-white/90 backdrop-blur-sm text-gray-700 text-xs font-medium rounded-full">
                {transmission}
              </span>
            )}
          </div>
        )}
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Price */}
        <div className="mb-2">
          <span className="text-xl font-bold text-gray-900">
            {formatPrice(price)}
          </span>
        </div>

        {/* Title */}
        <h3 className="font-semibold text-gray-900 mb-2 line-clamp-1 group-hover:text-blue-600 transition-colors">
          {year} {make} {model}
        </h3>

        {/* Details */}
        <div className="flex items-center gap-1 text-gray-500 text-sm mb-3">
          <FiMapPin className="w-4 h-4 flex-shrink-0" />
          <span className="truncate">{location}</span>
        </div>

        {/* Features */}
        <div className="flex items-center gap-4 text-sm text-gray-600 pt-3 border-t border-gray-100">
          <div className="flex items-center gap-1">
            <FiActivity className="w-4 h-4" />
            <span>{formatMileage(mileage)}</span>
          </div>
          {fuelType && (
            <div className="flex items-center gap-1">
              <span className="text-xs bg-gray-100 px-2 py-0.5 rounded">{fuelType}</span>
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}
