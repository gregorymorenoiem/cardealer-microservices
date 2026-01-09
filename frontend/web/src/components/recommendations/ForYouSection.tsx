import { useEffect, useState, useCallback } from 'react';
import { FiTrendingUp, FiStar } from 'react-icons/fi';
import { Link } from 'react-router-dom';
import { recommendationService, RecommendationType } from '@/services/recommendationService';
import type { Recommendation } from '@/services/recommendationService';
import vehicleService from '@/services/vehicleService';
import type { Vehicle } from '@/services/vehicleService';

export const ForYouSection = () => {
  const [recommendations, setRecommendations] = useState<Recommendation[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRecommendations = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      // Obtener recomendaciones
      const recs = await recommendationService.getForYouRecommendations(10);
      setRecommendations(recs);

      // Cargar datos de los vehículos
      const vehicleIds = recs.map((r) => r.vehicleId);
      const vehiclesData = await Promise.all(
        vehicleIds.map((id) => vehicleService.getVehicleById(id))
      );
      setVehicles(vehiclesData.filter((v: Vehicle | null) => v !== null) as Vehicle[]);
    } catch (err: any) {
      console.error('Error loading recommendations:', err);
      setError(err.response?.data?.message || 'Error al cargar recomendaciones');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRecommendations();
  }, [loadRecommendations]);

  const handleRecommendationClick = async (recommendationId: string) => {
    try {
      await recommendationService.markRecommendationClicked(recommendationId);
    } catch (err) {
      console.error('Error marking recommendation as clicked:', err);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h2 className="text-2xl font-bold mb-6">Para ti</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {[...Array(8)].map((_, i) => (
            <div key={i} className="animate-pulse">
              <div className="bg-gray-200 h-48 rounded-lg mb-4"></div>
              <div className="bg-gray-200 h-4 rounded w-3/4 mb-2"></div>
              <div className="bg-gray-200 h-4 rounded w-1/2"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
          <p className="font-semibold">Error</p>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  if (recommendations.length === 0) {
    return (
      <div className="container mx-auto px-4 py-8">
        <h2 className="text-2xl font-bold mb-6">Para ti</h2>
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-8 text-center">
          <FiStar className="mx-auto text-gray-400 text-5xl mb-4" />
          <p className="text-gray-600 text-lg">Aún no tenemos recomendaciones para ti</p>
          <p className="text-gray-500 text-sm mt-2">
            Explora vehículos para que podamos personalizar tu experiencia
          </p>
          <Link
            to="/vehicles"
            className="inline-block mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Ver todos los vehículos
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <FiStar className="text-yellow-500" />
            Para ti
          </h2>
          <p className="text-gray-600 text-sm mt-1">Basado en tus preferencias y búsquedas</p>
        </div>
        <button
          onClick={loadRecommendations}
          className="px-4 py-2 text-blue-600 hover:bg-blue-50 rounded-lg transition"
        >
          Actualizar
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {vehicles.map((vehicle, index) => {
          const recommendation = recommendations[index];
          return (
            <Link
              key={vehicle.id}
              to={`/vehicles/${vehicle.id}`}
              onClick={() => handleRecommendationClick(recommendation.id)}
              className="group bg-white rounded-lg shadow-md hover:shadow-xl transition-shadow overflow-hidden"
            >
              {/* Badge de tipo de recomendación */}
              <div className="relative">
                <img
                  src={vehicle.images?.[0] || '/placeholder-vehicle.jpg'}
                  alt={`${vehicle.make} ${vehicle.model}`}
                  className="w-full h-48 object-cover group-hover:scale-105 transition-transform duration-300"
                />
                <div className="absolute top-2 left-2">
                  <span className="px-2 py-1 bg-blue-600 text-white text-xs rounded-full flex items-center gap-1">
                    {recommendation.type === 'ForYou' && <FiStar />}
                    {recommendation.type === 'Trending' && <FiTrendingUp />}
                    {recommendationService.formatRecommendationType(
                      recommendation.type as RecommendationType
                    )}
                  </span>
                </div>
                {vehicle.isFeatured && (
                  <div className="absolute top-2 right-2">
                    <span className="px-2 py-1 bg-yellow-500 text-white text-xs rounded-full">
                      Destacado
                    </span>
                  </div>
                )}
              </div>

              {/* Info del vehículo */}
              <div className="p-4">
                <h3 className="font-bold text-lg mb-1 group-hover:text-blue-600 transition">
                  {vehicle.make} {vehicle.model}
                </h3>
                <p className="text-gray-600 text-sm mb-2">{vehicle.year}</p>

                <div className="flex items-center justify-between mb-3">
                  <p className="text-xl font-bold text-blue-600">
                    ${vehicle.price?.toLocaleString()}
                  </p>
                  <div className="flex items-center gap-1 text-yellow-500">
                    <FiStar className="fill-current" />
                    <span className="text-sm font-medium">
                      {recommendationService.formatScore(recommendation.score)}
                    </span>
                  </div>
                </div>

                {/* Reason */}
                <p className="text-gray-500 text-xs italic mb-3">{recommendation.reason}</p>

                {/* Specs rápidas */}
                <div className="flex flex-wrap gap-2 text-xs text-gray-600">
                  {vehicle.mileage && (
                    <span className="px-2 py-1 bg-gray-100 rounded">
                      {vehicle.mileage.toLocaleString()} km
                    </span>
                  )}
                  {vehicle.transmission && (
                    <span className="px-2 py-1 bg-gray-100 rounded capitalize">
                      {vehicle.transmission}
                    </span>
                  )}
                  {vehicle.fuelType && (
                    <span className="px-2 py-1 bg-gray-100 rounded capitalize">
                      {vehicle.fuelType}
                    </span>
                  )}
                </div>
              </div>
            </Link>
          );
        })}
      </div>

      {/* Call to action */}
      <div className="mt-8 text-center">
        <Link
          to="/vehicles"
          className="inline-block px-6 py-3 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition"
        >
          Ver todos los vehículos
        </Link>
      </div>
    </div>
  );
};
