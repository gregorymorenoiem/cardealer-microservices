import { useState, useEffect } from 'react';
import { FiHeart, FiTrash2, FiBell, FiEdit3, FiExternalLink } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import VehicleCard from '@/components/organisms/VehicleCard';
import VehicleCardSkeleton from '@/components/organisms/VehicleCardSkeleton';
import EmptyState from '@/components/organisms/EmptyState';

interface Favorite {
  id: string;
  vehicleId: string;
  vehicle: {
    id: string;
    title: string;
    make: string;
    model: string;
    year: number;
    price: number;
    mileage: number;
    imageUrl: string;
  };
  notes: string;
  notifyOnPriceChange: boolean;
  addedAt: string;
}

export function FavoritesPage() {
  const [favorites, setFavorites] = useState<Favorite[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingNote, setEditingNote] = useState<string | null>(null);
  const [noteText, setNoteText] = useState('');

  useEffect(() => {
    loadFavorites();
  }, []);

  const loadFavorites = async () => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      window.location.href = '/login?redirect=/favorites';
      return;
    }

    try {
      const response = await fetch('https://api.okla.com.do/api/favorites', {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      setFavorites(data);
    } catch (error) {
      console.error('Failed to load favorites:', error);
    } finally {
      setLoading(false);
    }
  };

  const removeFavorite = async (vehicleId: string) => {
    if (!confirm('¿Estás seguro de eliminar este favorito?')) return;

    const token = localStorage.getItem('authToken');
    try {
      const response = await fetch(`https://api.okla.com.do/api/favorites/${vehicleId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        setFavorites(favorites.filter((f) => f.vehicleId !== vehicleId));
      }
    } catch (error) {
      console.error('Failed to remove favorite:', error);
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
            icon={FiHeart}
            title="No tienes favoritos aún"
            description="Marca los vehículos que te gusten para verlos aquí más tarde"
            actionLabel="Buscar Vehículos"
            onAction={() => (window.location.href = '/search')}
          />
        ) : (
          <div className="space-y-4">
            {favorites.map((favorite) => (
              <div
                key={favorite.id}
                className="border rounded-lg overflow-hidden hover:shadow-md transition-shadow"
              >
                <div className="flex flex-col md:flex-row">
                  {/* Image */}
                  <div className="md:w-1/3">
                    <img
                      src={favorite.vehicle.imageUrl}
                      alt={favorite.vehicle.title}
                      className="w-full h-48 md:h-full object-cover"
                    />
                  </div>

                  {/* Content */}
                  <div className="flex-1 p-6">
                    <div className="flex justify-between items-start mb-4">
                      <div>
                        <h3 className="text-2xl font-bold mb-2">{favorite.vehicle.title}</h3>
                        <p className="text-gray-600">
                          {favorite.vehicle.year} • {favorite.vehicle.mileage.toLocaleString()} km
                        </p>
                      </div>
                      <p className="text-3xl font-bold text-blue-600">
                        {formatPrice(favorite.vehicle.price)}
                      </p>
                    </div>

                    {/* Notes Section */}
                    {favorite.notes && (
                      <div className="mb-4 bg-gray-50 p-3 rounded-md">
                        <p className="text-sm text-gray-700">{favorite.notes}</p>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="flex gap-2">
                      <Button
                        onClick={() => (window.location.href = `/vehicles/${favorite.vehicleId}`)}
                        leftIcon={<FiExternalLink />}
                      >
                        Ver Detalles
                      </Button>
                      <Button
                        variant="outline"
                        onClick={() => removeFavorite(favorite.vehicleId)}
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
