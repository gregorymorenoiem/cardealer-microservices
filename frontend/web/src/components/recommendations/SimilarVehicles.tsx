import { useEffect, useState } from 'react';
import { FiRefreshCw } from 'react-icons/fi';
import { Link } from 'react-router-dom';
import {
  recommendationService,
  Recommendation,
} from '@/services/recommendationService';
import { vehicleService, Vehicle } from '@/services/vehicleService';

interface SimilarVehiclesProps {
  vehicleId: string;
  limit?: number;
}

export const SimilarVehicles = ({ vehicleId, limit = 6 }: SimilarVehiclesProps) => {
  const [recommendations, setRecommendations] = useState<Recommendation[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSimilarVehicles();
  }, [vehicleId]);

  const loadSimilarVehicles = async () => {
    try {
      setLoading(true);
      const recs = await recommendationService.getSimilarVehicles(vehicleId, limit);
      setRecommendations(recs);

      const vehicleIds = recs.map((r) => r.vehicleId);
      const vehiclesData = await Promise.all(
        vehicleIds.map((id) => vehicleService.getVehicleById(id))
      );
      setVehicles(vehiclesData.filter((v) => v !== null) as Vehicle[]);
    } catch (err) {
      console.error('Error loading similar vehicles:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="py-8">
        <h3 className="text-xl font-bold mb-4">Vehículos similares</h3>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="animate-pulse">
              <div className="bg-gray-200 h-32 rounded-lg mb-2"></div>
              <div className="bg-gray-200 h-3 rounded w-3/4 mb-1"></div>
              <div className="bg-gray-200 h-3 rounded w-1/2"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (recommendations.length === 0) {
    return null;
  }

  return (
    <div className="py-8">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-xl font-bold">Vehículos similares</h3>
        <button
          onClick={loadSimilarVehicles}
          className="p-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
          title="Actualizar"
        >
          <FiRefreshCw />
        </button>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        {vehicles.map((vehicle, index) => {
          const recommendation = recommendations[index];
          return (
            <Link
              key={vehicle.id}
              to={`/vehicles/${vehicle.slug || vehicle.id}`}
              onClick={() => recommendationService.markRecommendationClicked(recommendation.id)}
              className="group bg-white rounded-lg shadow hover:shadow-lg transition overflow-hidden"
            >
              <img
                src={vehicle.images?.[0] || '/placeholder-vehicle.jpg'}
                alt={`${vehicle.make} ${vehicle.model}`}
                className="w-full h-32 object-cover group-hover:scale-105 transition-transform duration-300"
              />
              <div className="p-2">
                <p className="font-semibold text-sm truncate">
                  {vehicle.make} {vehicle.model}
                </p>
                <p className="text-xs text-gray-600">{vehicle.year}</p>
                <p className="text-sm font-bold text-blue-600 mt-1">
                  ${vehicle.price?.toLocaleString()}
                </p>
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
};
