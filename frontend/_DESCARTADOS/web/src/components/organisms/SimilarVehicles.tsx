import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import VehicleCard from '@/components/organisms/VehicleCard';
import { getSimilarVehicles, type Vehicle } from '@/services/vehicleService';
import { FiArrowRight } from 'react-icons/fi';

interface SimilarVehiclesProps {
  currentVehicle: {
    id: string;
    make?: string;
    model?: string;
    bodyType?: string;
    year?: number;
    price?: number;
    transmission?: string;
    fuelType?: string;
  };
  maxItems?: number;
}

export default function SimilarVehicles({ currentVehicle, maxItems = 4 }: SimilarVehiclesProps) {
  // Fetch similar vehicles from API only (no mock data fallback)
  const {
    data: similarVehicles = [],
    isLoading,
    isError,
  } = useQuery({
    queryKey: ['similar-vehicles', currentVehicle.id, maxItems],
    queryFn: () => getSimilarVehicles(currentVehicle.id, maxItems),
    staleTime: 5 * 60 * 1000,
    retry: 1,
    select: (vehicles) => vehicles.filter((v) => v.id !== currentVehicle.id).slice(0, maxItems),
  });

  if (similarVehicles.length === 0) {
    return null;
  }

  return (
    <div className="bg-white rounded-xl shadow-card p-6 sm:p-8">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold font-heading text-gray-900">Similar Vehicles</h2>
        <Link
          to="/browse"
          className="flex items-center gap-2 text-primary hover:text-primary-600 font-medium transition-colors duration-200"
        >
          View All
          <FiArrowRight size={18} />
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
        {similarVehicles.map((vehicle) => (
          <VehicleCard
            key={vehicle.id}
            id={vehicle.id}
            make={vehicle.make}
            model={vehicle.model}
            year={vehicle.year}
            price={vehicle.price}
            mileage={vehicle.mileage}
            location={vehicle.location}
            imageUrl={vehicle.images[0]}
            isFeatured={vehicle.isFeatured}
            isNew={vehicle.isNew}
            transmission={vehicle.transmission}
            fuelType={vehicle.fuelType}
          />
        ))}
      </div>

      {similarVehicles.length < 3 && (
        <div className="mt-6 text-center">
          <p className="text-gray-600 mb-4">Looking for more options?</p>
          <Link to="/browse">
            <button className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium">
              Browse All Vehicles
            </button>
          </Link>
        </div>
      )}
    </div>
  );
}
