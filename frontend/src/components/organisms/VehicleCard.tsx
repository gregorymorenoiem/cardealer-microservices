import { Link } from 'react-router-dom';
import { FiMapPin, FiActivity, FiHeart } from 'react-icons/fi';
import { formatPrice, formatMileage } from '@/utils/formatters';

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
  const defaultImage = 'https://via.placeholder.com/400x300?text=No+Image';

  return (
    <div className="card overflow-hidden group">
      {/* Image Container */}
      <div className="relative h-48 overflow-hidden bg-gray-200">
        <img
          src={imageUrl || defaultImage}
          alt={`${year} ${make} ${model}`}
          className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
        />
        
        {/* Badges */}
        <div className="absolute top-3 left-3 flex gap-2">
          {isFeatured && (
            <span className="px-3 py-1 bg-accent text-white text-xs font-semibold rounded-full">
              Featured
            </span>
          )}
          {isNew && (
            <span className="px-3 py-1 bg-green-500 text-white text-xs font-semibold rounded-full">
              New
            </span>
          )}
        </div>

        {/* Favorite Button */}
        <button
          className="absolute top-3 right-3 w-9 h-9 bg-white/90 hover:bg-white rounded-full flex items-center justify-center transition-colors shadow-sm"
          onClick={(e) => {
            e.preventDefault();
            // TODO: Add to favorites
          }}
        >
          <FiHeart className="text-gray-700" size={18} />
        </button>
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Title */}
        <Link to={`/vehicles/${id}`}>
          <h3 className="text-lg font-semibold text-gray-900 mb-2 hover:text-primary transition-colors">
            {year} {make} {model}
          </h3>
        </Link>

        {/* Price */}
        <p className="text-2xl font-bold text-primary mb-3">
          {formatPrice(price)}
        </p>

        {/* Details */}
        <div className="flex items-center gap-4 text-sm text-gray-600 mb-3">
          <div className="flex items-center gap-1.5">
            <FiActivity size={16} />
            <span>{formatMileage(mileage)}</span>
          </div>
          <div className="flex items-center gap-1.5">
            <FiMapPin size={16} />
            <span>{location}</span>
          </div>
        </div>

        {/* Additional Info */}
        {(transmission || fuelType) && (
          <div className="flex gap-2 mb-3">
            {transmission && (
              <span className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                {transmission}
              </span>
            )}
            {fuelType && (
              <span className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">
                {fuelType}
              </span>
            )}
          </div>
        )}

        {/* View Details Link */}
        <Link
          to={`/vehicles/${id}`}
          className="block text-center px-4 py-2 bg-gray-100 hover:bg-primary hover:text-white text-gray-900 rounded-lg transition-colors font-medium text-sm"
        >
          View Details
        </Link>
      </div>
    </div>
  );
}
