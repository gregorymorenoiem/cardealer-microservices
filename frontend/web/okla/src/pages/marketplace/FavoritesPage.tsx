/**
 * MarketplaceFavoritesPage - User's saved/favorite listings
 */

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import type { Listing, MarketplaceCategorySelection } from '@/types/marketplace';
import {
  HeartIcon,
  TrashIcon,
  FunnelIcon,
  BellIcon,
  BellSlashIcon,
} from '@heroicons/react/24/outline';
import { HeartIcon as HeartSolidIcon } from '@heroicons/react/24/solid';

// Mock favorites data
const mockFavorites: Listing[] = [
  {
    id: 'fav-v1',
    vertical: 'vehicles',
    title: '2023 BMW X5 xDrive40i',
    description: 'SUV de lujo con todas las opciones.',
    price: 1250000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/1e40af/white?text=BMW+X5', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/1e40af/white?text=BMW+X5',
    location: { address: '', city: 'CDMX', state: 'CDMX', zipCode: '', country: 'México' },
    seller: { id: '1', name: 'BMW Premium', isVerified: true, isDealership: true },
    viewCount: 1245,
    favoriteCount: 89,
    inquiryCount: 23,
    createdAt: '2024-01-10T00:00:00Z',
    updatedAt: '2024-01-18T00:00:00Z',
  } as Listing,
  {
    id: 'fav-p1',
    vertical: 'real-estate',
    title: 'Penthouse en Santa Fe',
    description: 'Increíble penthouse con vista panorámica.',
    price: 18500000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/059669/white?text=Penthouse', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/059669/white?text=Penthouse',
    location: { address: '', city: 'CDMX', state: 'CDMX', zipCode: '', country: 'México' },
    seller: { id: '2', name: 'Inmobiliaria Premier', isVerified: true, isDealership: true },
    viewCount: 892,
    favoriteCount: 67,
    inquiryCount: 15,
    createdAt: '2024-01-05T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
  } as Listing,
  {
    id: 'fav-v2',
    vertical: 'vehicles',
    title: '2022 Mercedes-Benz Clase C',
    description: 'Sedán elegante, excelente estado.',
    price: 875000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/374151/white?text=Mercedes', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/374151/white?text=Mercedes',
    location: { address: '', city: 'Guadalajara', state: 'Jalisco', zipCode: '', country: 'México' },
    seller: { id: '3', name: 'Auto Premium', isVerified: true, isDealership: true },
    viewCount: 567,
    favoriteCount: 34,
    inquiryCount: 8,
    createdAt: '2024-01-12T00:00:00Z',
    updatedAt: '2024-01-20T00:00:00Z',
  } as Listing,
  {
    id: 'fav-p2',
    vertical: 'real-estate',
    title: 'Casa en Coyoacán',
    description: 'Hermosa casa con jardín amplio.',
    price: 8900000,
    currency: 'MXN',
    status: 'sold',
    isFeatured: false,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/84cc16/white?text=Casa', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/84cc16/white?text=Casa',
    location: { address: '', city: 'CDMX', state: 'CDMX', zipCode: '', country: 'México' },
    seller: { id: '4', name: 'Bienes Raíces Select', isVerified: true, isDealership: false },
    viewCount: 423,
    favoriteCount: 45,
    inquiryCount: 12,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-22T00:00:00Z',
  } as Listing,
];

const MarketplaceFavoritesPage: React.FC = () => {
  const [favorites, setFavorites] = useState<Listing[]>(mockFavorites);
  const [selectedCategory, setSelectedCategory] = useState<MarketplaceCategorySelection>('all');
  const [notifications, setNotifications] = useState<Record<string, boolean>>({});
  const [showClearConfirm, setShowClearConfirm] = useState(false);

  // Filter favorites by category
  const filteredFavorites = favorites.filter((listing) => {
    if (selectedCategory === 'all') return true;
    return listing.vertical === selectedCategory;
  });

  // Counts by category
  const vehicleCount = favorites.filter((l) => l.vertical === 'vehicles').length;
  const propertyCount = favorites.filter((l) => l.vertical === 'real-estate').length;
  const activeCount = favorites.filter((l) => l.status === 'active').length;
  const soldCount = favorites.filter((l) => l.status === 'sold' || l.status === 'rented').length;

  // Remove from favorites
  const removeFavorite = (id: string) => {
    setFavorites((prev) => prev.filter((f) => f.id !== id));
  };

  // Toggle price alerts
  const toggleNotification = (id: string) => {
    setNotifications((prev) => ({
      ...prev,
      [id]: !prev[id],
    }));
  };

  // Clear all favorites
  const clearAllFavorites = () => {
    setFavorites([]);
    setShowClearConfirm(false);
  };

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-pink-100 rounded-xl">
              <HeartSolidIcon className="h-6 w-6 text-pink-600" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                Mis Favoritos
              </h1>
              <p className="text-gray-500">
                {favorites.length} {favorites.length === 1 ? 'guardado' : 'guardados'}
              </p>
            </div>
          </div>

          {favorites.length > 0 && (
            <button
              onClick={() => setShowClearConfirm(true)}
              className="text-sm text-red-600 hover:text-red-700 flex items-center gap-1"
            >
              <TrashIcon className="h-4 w-4" />
              Limpiar todo
            </button>
          )}
        </div>

        {/* Stats Cards */}
        {favorites.length > 0 && (
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
            <StatCard
              label="Vehículos"
              value={vehicleCount}
              icon="🚗"
              color="blue"
            />
            <StatCard
              label="Propiedades"
              value={propertyCount}
              icon="🏠"
              color="green"
            />
            <StatCard
              label="Disponibles"
              value={activeCount}
              icon="✅"
              color="emerald"
            />
            <StatCard
              label="Vendidos"
              value={soldCount}
              icon="🏷️"
              color="amber"
            />
          </div>
        )}

        {/* Filters */}
        {favorites.length > 0 && (
          <div className="flex items-center gap-2 mb-6 overflow-x-auto pb-2">
            <div className="flex items-center gap-1 text-gray-500 mr-2">
              <FunnelIcon className="h-4 w-4" />
              <span className="text-sm">Filtrar:</span>
            </div>
            <FilterButton
              label="Todos"
              count={favorites.length}
              isActive={selectedCategory === 'all'}
              onClick={() => setSelectedCategory('all')}
            />
            <FilterButton
              label="Vehículos"
              count={vehicleCount}
              isActive={selectedCategory === 'vehicles'}
              onClick={() => setSelectedCategory('vehicles')}
            />
            <FilterButton
              label="Propiedades"
              count={propertyCount}
              isActive={selectedCategory === 'real-estate'}
              onClick={() => setSelectedCategory('real-estate')}
            />
          </div>
        )}

        {/* Favorites List */}
        {filteredFavorites.length > 0 ? (
          <div className="space-y-4">
            {filteredFavorites.map((listing) => (
              <FavoriteCard
                key={listing.id}
                listing={listing}
                hasNotification={notifications[listing.id] || false}
                onToggleNotification={() => toggleNotification(listing.id)}
                onRemove={() => removeFavorite(listing.id)}
              />
            ))}
          </div>
        ) : favorites.length > 0 ? (
          // Filtered empty
          <EmptyFilterState onClearFilter={() => setSelectedCategory('all')} />
        ) : (
          // No favorites
          <EmptyState />
        )}

        {/* Clear Confirmation Modal */}
        <AnimatePresence>
          {showClearConfirm && (
            <ClearConfirmModal
              count={favorites.length}
              onConfirm={clearAllFavorites}
              onCancel={() => setShowClearConfirm(false)}
            />
          )}
        </AnimatePresence>
      </div>
    </MainLayout>
  );
};

// Stat Card
interface StatCardProps {
  label: string;
  value: number;
  icon: string;
  color: 'blue' | 'green' | 'emerald' | 'amber';
}

const StatCard: React.FC<StatCardProps> = ({ label, value, icon, color }) => {
  const colorClasses = {
    blue: 'bg-blue-50',
    green: 'bg-green-50',
    emerald: 'bg-emerald-50',
    amber: 'bg-amber-50',
  };

  return (
    <div className={`p-4 rounded-xl ${colorClasses[color]}`}>
      <div className="text-2xl mb-1">{icon}</div>
      <div className="text-2xl font-bold text-gray-900">{value}</div>
      <div className="text-sm text-gray-500">{label}</div>
    </div>
  );
};

// Filter Button
interface FilterButtonProps {
  label: string;
  count: number;
  isActive: boolean;
  onClick: () => void;
}

const FilterButton: React.FC<FilterButtonProps> = ({ label, count, isActive, onClick }) => (
  <button
    onClick={onClick}
    className={`flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap transition-colors ${
      isActive
        ? 'bg-gray-900 text-white'
        : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
    }`}
  >
    {label}
    <span className={`px-1.5 py-0.5 rounded-full text-xs ${
      isActive
        ? 'bg-white/20 text-white'
        : 'bg-gray-100 text-gray-600'
    }`}>
      {count}
    </span>
  </button>
);

// Favorite Card with actions
interface FavoriteCardProps {
  listing: Listing;
  hasNotification: boolean;
  onToggleNotification: () => void;
  onRemove: () => void;
}

const FavoriteCard: React.FC<FavoriteCardProps> = ({
  listing,
  hasNotification,
  onToggleNotification,
  onRemove,
}) => {
  const isSold = listing.status === 'sold' || listing.status === 'rented';
  const detailUrl = listing.vertical === 'vehicles' 
    ? `/marketplace/vehicles/${listing.id}`
    : `/properties/${listing.id}`;

  return (
    <motion.div
      layout
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, x: -100 }}
      className={`bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden ${
        isSold ? 'opacity-60' : ''
      }`}
    >
      <div className="flex flex-col md:flex-row">
        {/* Image */}
        <Link to={detailUrl} className="relative md:w-64 aspect-video md:aspect-auto flex-shrink-0">
          <img
            src={listing.primaryImageUrl || 'https://placehold.co/400x300/gray/white?text=Sin+imagen'}
            alt={listing.title}
            className="w-full h-full object-cover"
          />
          {isSold && (
            <div className="absolute inset-0 bg-black/50 flex items-center justify-center">
              <span className="px-4 py-2 bg-red-600 text-white font-semibold rounded-lg">
                {listing.status === 'sold' ? 'Vendido' : 'Rentado'}
              </span>
            </div>
          )}
          {listing.isFeatured && !isSold && (
            <span className="absolute top-2 left-2 px-2 py-1 bg-amber-500 text-white text-xs font-medium rounded">
              Destacado
            </span>
          )}
          <span className={`absolute top-2 right-2 px-2 py-1 text-xs font-medium rounded ${
            listing.vertical === 'vehicles'
              ? 'bg-blue-600 text-white'
              : 'bg-emerald-600 text-white'
          }`}>
            {listing.vertical === 'vehicles' ? '🚗 Vehículo' : '🏠 Propiedad'}
          </span>
        </Link>

        {/* Content */}
        <div className="flex-1 p-4 md:p-6">
          <div className="flex flex-col md:flex-row md:items-start justify-between gap-4">
            <div className="flex-1 min-w-0">
              <Link 
                to={detailUrl}
                className="text-lg font-semibold text-gray-900 hover:text-blue-600 line-clamp-1"
              >
                <LocalizedContent content={listing.title} showBadge={false} />
              </Link>
              <p className="text-gray-500 text-sm mt-1 line-clamp-2">
                <LocalizedContent content={listing.description} showBadge={false} />
              </p>
              <div className="flex items-center gap-2 mt-2 text-sm text-gray-500">
                <span>📍 {listing.location.city}, {listing.location.state}</span>
              </div>
              <div className="text-xl font-bold text-blue-600 mt-3">
                {new Intl.NumberFormat('es-MX', {
                  style: 'currency',
                  currency: listing.currency,
                  maximumFractionDigits: 0,
                }).format(listing.price)}
              </div>
            </div>

            {/* Actions */}
            <div className="flex md:flex-col items-center gap-2">
              <button
                onClick={onToggleNotification}
                className={`p-2 rounded-lg transition-colors ${
                  hasNotification
                    ? 'bg-blue-100 text-blue-600'
                    : 'bg-gray-100 text-gray-500 hover:bg-gray-200'
                }`}
                title={hasNotification ? 'Desactivar alertas' : 'Activar alertas de precio'}
              >
                {hasNotification ? (
                  <BellIcon className="h-5 w-5" />
                ) : (
                  <BellSlashIcon className="h-5 w-5" />
                )}
              </button>
              <button
                onClick={onRemove}
                className="p-2 bg-gray-100 text-gray-500 rounded-lg hover:bg-red-100 hover:text-red-600 transition-colors"
                title="Quitar de favoritos"
              >
                <TrashIcon className="h-5 w-5" />
              </button>
            </div>
          </div>

          {/* Quick stats */}
          <div className="flex items-center gap-4 mt-4 pt-4 border-t border-gray-100 text-sm text-gray-500">
            <span>👁️ {listing.viewCount} vistas</span>
            <span>❤️ {listing.favoriteCount} favoritos</span>
            <span>💬 {listing.inquiryCount} consultas</span>
            <span className="ml-auto text-xs">
              Guardado: {new Date(listing.createdAt).toLocaleDateString('es-MX')}
            </span>
          </div>
        </div>
      </div>
    </motion.div>
  );
};

// Empty State
const EmptyState: React.FC = () => (
  <div className="text-center py-16">
    <div className="w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-6">
      <HeartIcon className="h-12 w-12 text-gray-400" />
    </div>
    <h2 className="text-xl font-semibold text-gray-900 mb-2">
      No tienes favoritos aún
    </h2>
    <p className="text-gray-500 mb-6 max-w-md mx-auto">
      Explora el marketplace y guarda los listados que te interesen para verlos después.
    </p>
    <Link
      to="/marketplace/browse"
      className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
    >
      Explorar marketplace
    </Link>
  </div>
);

// Empty Filter State
const EmptyFilterState: React.FC<{ onClearFilter: () => void }> = ({ onClearFilter }) => (
  <div className="text-center py-12">
    <p className="text-gray-500 mb-4">
      No tienes favoritos en esta categoría
    </p>
    <button
      onClick={onClearFilter}
      className="text-blue-600 hover:text-blue-700"
    >
      Ver todos los favoritos
    </button>
  </div>
);

// Clear Confirmation Modal
interface ClearConfirmModalProps {
  count: number;
  onConfirm: () => void;
  onCancel: () => void;
}

const ClearConfirmModal: React.FC<ClearConfirmModalProps> = ({ count, onConfirm, onCancel }) => (
  <>
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="fixed inset-0 bg-black/50 z-40"
      onClick={onCancel}
    />
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 0.95 }}
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
    >
      <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="p-3 bg-red-100 rounded-full">
            <TrashIcon className="h-6 w-6 text-red-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900">
            ¿Limpiar favoritos?
          </h3>
        </div>
        <p className="text-gray-500 mb-6">
          Esto eliminará {count} {count === 1 ? 'elemento' : 'elementos'} de tus favoritos.
          Esta acción no se puede deshacer.
        </p>
        <div className="flex items-center gap-3 justify-end">
          <button
            onClick={onCancel}
            className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          >
            Cancelar
          </button>
          <button
            onClick={onConfirm}
            className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
          >
            Sí, limpiar todo
          </button>
        </div>
      </div>
    </motion.div>
  </>
);

export default MarketplaceFavoritesPage;
