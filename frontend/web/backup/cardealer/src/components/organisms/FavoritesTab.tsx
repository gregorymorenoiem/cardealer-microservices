import { useFavorites } from '@/hooks/useFavorites';
import { mockVehicles } from '@/data/mockVehicles';
import VehicleCard from '@/components/organisms/VehicleCard';
import { FiHeart } from 'react-icons/fi';

export default function FavoritesTab() {
  const { favorites, removeFavorite } = useFavorites();

  const favoriteVehicles = mockVehicles.filter((vehicle) =>
    favorites.includes(vehicle.id)
  );

  if (favoriteVehicles.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-card p-12 text-center">
        <div className="flex justify-center mb-4">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
            <FiHeart size={32} className="text-gray-400" />
          </div>
        </div>
        <h3 className="text-xl font-bold font-heading text-gray-900 mb-2">
          No favorites yet
        </h3>
        <p className="text-gray-600 mb-6 max-w-md mx-auto">
          Start adding vehicles to your favorites by clicking the heart icon on any vehicle listing.
        </p>
        <a
          href="/browse"
          className="inline-block px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium"
        >
          Browse Vehicles
        </a>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold font-heading text-gray-900">
            My Favorites
          </h2>
          <p className="text-gray-600 mt-1">
            {favoriteVehicles.length} {favoriteVehicles.length === 1 ? 'vehicle' : 'vehicles'} saved
          </p>
        </div>
        {favoriteVehicles.length > 0 && (
          <button
            onClick={() => {
              if (window.confirm('Remove all favorites?')) {
                favorites.forEach((id) => removeFavorite(id));
              }
            }}
            className="text-sm text-red-600 hover:text-red-700 font-medium transition-colors duration-200"
          >
            Clear All
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {favoriteVehicles.map((vehicle) => (
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
    </div>
  );
}
