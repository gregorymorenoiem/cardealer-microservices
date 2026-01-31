import { useState, useEffect } from 'react';
import { FiHeart, FiTrash2, FiBell, FiEdit3, FiExternalLink } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import VehicleCard from '@/components/organisms/VehicleCard';
import VehicleCardSkeleton from '@/components/organisms/VehicleCardSkeleton';
import EmptyState from '@/components/organisms/EmptyState';
import {
  getFavorites,
  removeFavorite as removeFavoriteApi,
  updateFavorite,
  type FavoriteVehicle,
} from '@/services/favoritesService';

export function FavoritesPage() {
  const [favorites, setFavorites] = useState<FavoriteVehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editingNote, setEditingNote] = useState<string | null>(null);
  const [noteText, setNoteText] = useState('');

  useEffect(() => {
    loadFavorites();
  }, []);

  const loadFavorites = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getFavorites();
      setFavorites(data);
    } catch (err) {
      console.error('Failed to load favorites:', err);
      setError('Error al cargar favoritos. Por favor, intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveFavorite = async (vehicleId: string) => {
    if (!confirm('¿Estás seguro de eliminar este favorito?')) return;

    try {
      await removeFavoriteApi(vehicleId);
      setFavorites(favorites.filter((f) => f.id !== vehicleId));
    } catch (err) {
      console.error('Failed to remove favorite:', err);
      alert('Error al eliminar favorito. Por favor, intenta de nuevo.');
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="text-center py-12">
            <div className="animate-spin h-12 w-12 border-4 border-blue-500 border-t-transparent rounded-full mx-auto"></div>
            <p className="mt-4 text-gray-600">Cargando favoritos...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="text-center py-12">
            <p className="text-red-600">{error}</p>
            <Button onClick={loadFavorites} className="mt-4">
              Reintentar
            </Button>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold">Mis Favoritos</h1>
            <p className="text-gray-600 mt-2">
              {favorites.length}{' '}
              {favorites.length === 1 ? 'vehículo guardado' : 'vehículos guardados'}
            </p>
          </div>
        </div>

        {favorites.length === 0 ? (
          <EmptyState
            icon={<FiHeart className="text-gray-400" size={64} />}
            title="No tienes favoritos aún"
            message="Marca los vehículos que te gusten para verlos aquí más tarde"
            actionLabel="Buscar Vehículos"
            onAction={() => (window.location.href = '/search')}
          />
        ) : (
          <div className="space-y-4">
            {favorites.map((vehicle) => (
              <div
                key={vehicle.id}
                className="border rounded-lg overflow-hidden hover:shadow-md transition-shadow"
              >
                <div className="flex flex-col md:flex-row">
                  {/* Image */}
                  <div className="md:w-1/3">
                    <img
                      src={vehicle.primaryImageUrl || '/placeholder-vehicle.jpg'}
                      alt={vehicle.title}
                      className="w-full h-48 md:h-full object-cover"
                    />
                  </div>

                  {/* Content */}
                  <div className="flex-1 p-6">
                    <div className="flex justify-between items-start mb-4">
                      <div>
                        <h3 className="text-2xl font-bold mb-2">{vehicle.title}</h3>
                        <p className="text-gray-600">
                          {vehicle.year} • {vehicle.make} {vehicle.model} •{' '}
                          {vehicle.mileage?.toLocaleString() || 'N/A'} km
                        </p>
                      </div>
                      <p className="text-3xl font-bold text-blue-600">
                        {formatPrice(vehicle.price)}
                      </p>
                    </div>

                    {/* Vehicle Info */}
                    <div className="mb-4 flex gap-4 text-sm text-gray-600">
                      <span>{vehicle.fuelType}</span>
                      <span>•</span>
                      <span>{vehicle.transmission}</span>
                      <span>•</span>
                      <span>{vehicle.bodyStyle}</span>
                    </div>

                    {/* Actions */}
                    <div className="flex gap-2">
                      <Button
                        onClick={() => (window.location.href = `/vehicles/${vehicle.id}`)}
                        leftIcon={<FiExternalLink />}
                      >
                        Ver Detalles
                      </Button>
                      <Button
                        variant="outline"
                        onClick={() => handleRemoveFavorite(vehicle.id)}
                        leftIcon={<FiTrash2 />}
                      >
                        Eliminar
                      </Button>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
}
