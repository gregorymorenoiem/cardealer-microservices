import { useEffect, useState, useCallback } from 'react';
import { FiUsers } from 'react-icons/fi';
import { Link } from 'react-router-dom';
import vehicleService from '@/services/vehicleService';
import type { Vehicle } from '@/services/vehicleService';

interface AlsoViewedProps {
  vehicleId: string;
  limit?: number;
}

export const AlsoViewed = ({ vehicleId: _vehicleId, limit = 4 }: AlsoViewedProps) => {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);

  const loadAlsoViewed = useCallback(async () => {
    try {
      setLoading(true);
      // En producción, esto vendría de un endpoint específico basado en _vehicleId
      // Por ahora, simulamos con vehículos populares
      const result = await vehicleService.getAllVehicles({}, 1, limit);
      setVehicles(result.vehicles.slice(0, limit));
    } catch (err) {
      console.error('Error loading also viewed vehicles:', err);
    } finally {
      setLoading(false);
    }
  }, [limit]);

  useEffect(() => {
    loadAlsoViewed();
  }, [loadAlsoViewed]);

  if (loading) {
    return (
      <div className="py-8 border-t">
        <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
          <FiUsers />
          Usuarios también vieron
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="animate-pulse">
              <div className="bg-gray-200 h-40 rounded-lg mb-2"></div>
              <div className="bg-gray-200 h-3 rounded w-3/4 mb-1"></div>
              <div className="bg-gray-200 h-3 rounded w-1/2"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (vehicles.length === 0) {
    return null;
  }

  return (
    <div className="py-8 border-t">
      <h3 className="text-lg font-bold mb-4 flex items-center gap-2">
        <FiUsers className="text-purple-600" />
        Usuarios también vieron
      </h3>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {vehicles.map((vehicle) => (
          <Link
            key={vehicle.id}
            to={`/vehicles/${vehicle.id}`}
            className="group bg-white rounded-lg shadow hover:shadow-lg transition overflow-hidden"
          >
            <div className="relative">
              <img
                src={vehicle.images?.[0] || '/placeholder-vehicle.jpg'}
                alt={`${vehicle.make} ${vehicle.model}`}
                className="w-full h-40 object-cover group-hover:scale-105 transition-transform duration-300"
              />
            </div>
            <div className="p-3">
              <h4 className="font-semibold text-sm mb-1 truncate">
                {vehicle.make} {vehicle.model}
              </h4>
              <p className="text-xs text-gray-600 mb-2">{vehicle.year}</p>
              <p className="text-base font-bold text-blue-600">
                ${vehicle.price?.toLocaleString()}
              </p>
              {vehicle.mileage && (
                <p className="text-xs text-gray-500 mt-1">{vehicle.mileage.toLocaleString()} km</p>
              )}
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};
