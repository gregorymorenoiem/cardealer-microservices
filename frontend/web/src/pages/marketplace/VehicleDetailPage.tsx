/**
 * VehicleDetailPage - Detailed view for a vehicle listing
 */

import React, { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import { useListing, useSimilarListings } from '@/hooks/useMarketplace';
import { ListingGrid } from '@/components/marketplace';
import type { VehicleListing } from '@/types/marketplace';
import { isVehicleListing } from '@/types/marketplace';
import {
  ArrowLeftIcon,
  HeartIcon,
  ShareIcon,
  MapPinIcon,
  CheckBadgeIcon,
  PhoneIcon,
  EnvelopeIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  SparklesIcon,
} from '@heroicons/react/24/outline';
import { HeartIcon as HeartSolidIcon } from '@heroicons/react/24/solid';

const VehicleDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  
  const { data: listing, isLoading, error } = useListing(id || '');
  const { data: similarListings = [] } = useSimilarListings(listing || null, 4);

  // Check if it's a vehicle
  const vehicle = listing && isVehicleListing(listing) ? listing as VehicleListing : null;

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <VehicleDetailSkeleton />
        </div>
      </MainLayout>
    );
  }

  if (error || !vehicle) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              Vehículo no encontrado
            </h1>
            <p className="text-gray-500 mb-8">
              El vehículo que buscas no existe o ha sido removido.
            </p>
            <Link
              to="/marketplace/browse?vertical=vehicles"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <ArrowLeftIcon className="h-5 w-5" />
              Ver vehículos disponibles
            </Link>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      {/* Breadcrumb */}
      <div className="bg-gray-50 border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3">
          <nav className="flex items-center text-sm text-gray-500">
            <Link to="/marketplace" className="hover:text-blue-600">Marketplace</Link>
            <span className="mx-2">/</span>
            <Link to="/marketplace/browse?vertical=vehicles" className="hover:text-blue-600">Vehículos</Link>
            <span className="mx-2">/</span>
            <span className="text-gray-900">{vehicle.make} {vehicle.model}</span>
          </nav>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-8">
            {/* Gallery */}
            <ImageGallery images={vehicle.images} />

            {/* Vehicle Details */}
            <VehicleInfo vehicle={vehicle} />

            {/* Features */}
            <VehicleFeatures features={vehicle.features} />

            {/* Description */}
            {vehicle.description && (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-white rounded-xl p-6 shadow-sm border border-gray-200"
              >
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  Descripción
                </h2>
                <p className="text-gray-600 whitespace-pre-line">
                  <LocalizedContent content={vehicle.description} showBadge={false} />
                </p>
              </motion.div>
            )}

            {/* Location Map */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-xl p-6 shadow-sm border border-gray-200"
            >
              <h2 className="text-lg font-semibold text-gray-900 mb-4">
                Ubicación
              </h2>
              <div className="flex items-center gap-2 text-gray-600">
                <MapPinIcon className="h-5 w-5 text-gray-400" />
                <span>{vehicle.location.city}, {vehicle.location.state}</span>
              </div>
              {/* Map placeholder */}
              <div className="mt-4 h-48 bg-gray-100 rounded-lg flex items-center justify-center text-gray-400">
                <span>Mapa de ubicación</span>
              </div>
            </motion.div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Price & Actions */}
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              className="bg-white rounded-xl p-6 shadow-sm border border-gray-200 sticky top-24"
            >
              <div className="flex items-center justify-between mb-4">
                <div>
                  <div className="text-3xl font-bold text-gray-900">
                    {new Intl.NumberFormat('es-MX', {
                      style: 'currency',
                      currency: vehicle.currency,
                      maximumFractionDigits: 0,
                    }).format(vehicle.price)}
                  </div>
                  <div className="text-sm text-gray-500">
                    Precio final
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <FavoriteButton listingId={vehicle.id} />
                  <ShareButton />
                </div>
              </div>

              {/* Quick Stats */}
              <div className="grid grid-cols-3 gap-4 py-4 border-t border-b border-gray-200 mb-4">
                <div className="text-center">
                  <div className="text-lg font-semibold text-gray-900">
                    {vehicle.year}
                  </div>
                  <div className="text-xs text-gray-500">Año</div>
                </div>
                <div className="text-center">
                  <div className="text-lg font-semibold text-gray-900">
                    {vehicle.mileage.toLocaleString()}
                  </div>
                  <div className="text-xs text-gray-500">{vehicle.mileageUnit}</div>
                </div>
                <div className="text-center">
                  <div className="text-lg font-semibold text-gray-900 capitalize">
                    {vehicle.transmission === 'automatic' ? 'Auto' : vehicle.transmission}
                  </div>
                  <div className="text-xs text-gray-500">Transmisión</div>
                </div>
              </div>

              {/* Seller Info */}
              <SellerCard seller={vehicle.seller} />

              {/* Contact Buttons */}
              <div className="space-y-3 mt-6">
                <button className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
                  <PhoneIcon className="h-5 w-5" />
                  Llamar
                </button>
                <button className="w-full flex items-center justify-center gap-2 px-4 py-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                  <EnvelopeIcon className="h-5 w-5" />
                  Enviar mensaje
                </button>
                <button className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors">
                  <svg className="h-5 w-5" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z"/>
                  </svg>
                  WhatsApp
                </button>
              </div>
            </motion.div>
          </div>
        </div>

        {/* Similar Vehicles */}
        {similarListings.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="mt-12"
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Vehículos similares
            </h2>
            <ListingGrid
              listings={similarListings}
              columns={4}
              variant="compact"
            />
          </motion.div>
        )}
      </div>
    </MainLayout>
  );
};

// Image Gallery Component
const ImageGallery: React.FC<{ images: VehicleListing['images'] }> = ({ images }) => {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [showFullscreen, setShowFullscreen] = useState(false);

  if (!images.length) {
    return (
      <div className="aspect-video bg-gray-100 rounded-xl flex items-center justify-center">
        <span className="text-gray-400">Sin imágenes</span>
      </div>
    );
  }

  const mainImage = images[currentIndex];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="space-y-4"
    >
      {/* Main Image */}
      <div
        className="relative aspect-[4/3] rounded-xl overflow-hidden bg-gray-100 cursor-pointer group"
        onClick={() => setShowFullscreen(true)}
      >
        <img
          src={mainImage.url}
          alt={mainImage.caption || 'Vehicle image'}
          className="w-full h-full object-cover transition-transform group-hover:scale-105"
        />
        
        {/* Navigation arrows */}
        {images.length > 1 && (
          <>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setCurrentIndex((prev) => (prev - 1 + images.length) % images.length);
              }}
              className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 rounded-full flex items-center justify-center shadow-lg hover:bg-white transition-colors"
            >
              <ChevronLeftIcon className="h-5 w-5 text-gray-800" />
            </button>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setCurrentIndex((prev) => (prev + 1) % images.length);
              }}
              className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 bg-white/90 rounded-full flex items-center justify-center shadow-lg hover:bg-white transition-colors"
            >
              <ChevronRightIcon className="h-5 w-5 text-gray-800" />
            </button>
          </>
        )}

        {/* Image counter */}
        <div className="absolute bottom-4 right-4 px-3 py-1 bg-black/60 text-white text-sm rounded-full">
          {currentIndex + 1} / {images.length}
        </div>
      </div>

      {/* Thumbnails */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((image, index) => (
            <button
              key={image.id}
              onClick={() => setCurrentIndex(index)}
              className={`relative flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden transition-all ${
                index === currentIndex
                  ? 'ring-2 ring-blue-600 ring-offset-2'
                  : 'opacity-60 hover:opacity-100'
              }`}
            >
              <img
                src={image.thumbnailUrl || image.url}
                alt={image.caption || `Thumbnail ${index + 1}`}
                className="w-full h-full object-cover"
              />
            </button>
          ))}
        </div>
      )}

      {/* Fullscreen Modal */}
      <AnimatePresence>
        {showFullscreen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 bg-black flex items-center justify-center"
            onClick={() => setShowFullscreen(false)}
          >
            <button
              onClick={() => setShowFullscreen(false)}
              className="absolute top-4 right-4 text-white hover:text-gray-300"
            >
              <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
            <img
              src={mainImage.url}
              alt={mainImage.caption || 'Full size image'}
              className="max-w-full max-h-full object-contain"
              onClick={(e) => e.stopPropagation()}
            />
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
};

// Vehicle Info Component
const VehicleInfo: React.FC<{ vehicle: VehicleListing }> = ({ vehicle }) => {
  const specs = [
    { label: 'Marca', value: vehicle.make },
    { label: 'Modelo', value: vehicle.model },
    { label: 'Año', value: vehicle.year.toString() },
    { label: 'Kilometraje', value: `${vehicle.mileage.toLocaleString()} ${vehicle.mileageUnit}` },
    { label: 'Transmisión', value: vehicle.transmission === 'automatic' ? 'Automática' : 'Manual' },
    { label: 'Combustible', value: formatFuelType(vehicle.fuelType) },
    { label: 'Carrocería', value: formatBodyType(vehicle.bodyType) },
    { label: 'Tracción', value: formatDrivetrain(vehicle.drivetrain) },
    { label: 'Color exterior', value: vehicle.exteriorColor },
    ...(vehicle.interiorColor ? [{ label: 'Color interior', value: vehicle.interiorColor }] : []),
    ...(vehicle.engine ? [{ label: 'Motor', value: vehicle.engine }] : []),
    ...(vehicle.horsepower ? [{ label: 'Potencia', value: `${vehicle.horsepower} HP` }] : []),
    { label: 'Condición', value: formatCondition(vehicle.condition) },
  ];

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-xl p-6 shadow-sm border border-gray-200"
    >
      <div className="flex items-start justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {vehicle.year} {vehicle.make} {vehicle.model}
          </h1>
          <div className="flex items-center gap-2 mt-2 text-gray-500">
            <MapPinIcon className="h-4 w-4" />
            <span>{vehicle.location.city}, {vehicle.location.state}</span>
          </div>
        </div>
        {vehicle.isFeatured && (
          <span className="inline-flex items-center gap-1 px-3 py-1 bg-amber-100 text-amber-800 text-sm font-medium rounded-full">
            <SparklesIcon className="h-4 w-4" />
            Destacado
          </span>
        )}
      </div>

      <h2 className="text-lg font-semibold text-gray-900 mb-4">
        Especificaciones
      </h2>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
        {specs.map((spec, index) => (
          <div key={index} className="border-b border-gray-100 pb-2">
            <div className="text-xs text-gray-500">{spec.label}</div>
            <div className="font-medium text-gray-900">{spec.value}</div>
          </div>
        ))}
      </div>
    </motion.div>
  );
};

// Vehicle Features Component
const VehicleFeatures: React.FC<{ features: string[] }> = ({ features }) => {
  if (!features.length) return null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-xl p-6 shadow-sm border border-gray-200"
    >
      <h2 className="text-lg font-semibold text-gray-900 mb-4">
        Equipamiento
      </h2>
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
        {features.map((feature, index) => (
          <div
            key={index}
            className="flex items-center gap-2 text-gray-600"
          >
            <CheckBadgeIcon className="h-5 w-5 text-green-500 flex-shrink-0" />
            <span className="text-sm">{feature}</span>
          </div>
        ))}
      </div>
    </motion.div>
  );
};

// Seller Card Component
const SellerCard: React.FC<{ seller: VehicleListing['seller'] }> = ({ seller }) => {
  return (
    <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-lg">
      <div className="w-12 h-12 rounded-full bg-gray-200 flex items-center justify-center overflow-hidden">
        {seller.avatar ? (
          <img src={seller.avatar} alt={seller.name} className="w-full h-full object-cover" />
        ) : (
          <span className="text-lg font-semibold text-gray-500">
            {seller.name.charAt(0).toUpperCase()}
          </span>
        )}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-semibold text-gray-900 truncate">
            {seller.name}
          </span>
          {seller.isVerified && (
            <CheckBadgeIcon className="h-5 w-5 text-blue-500 flex-shrink-0" />
          )}
        </div>
        {seller.rating !== undefined && (
          <div className="flex items-center gap-1 text-sm text-gray-500">
            <span className="text-yellow-500">★</span>
            <span>{seller.rating.toFixed(1)}</span>
            {seller.reviewCount !== undefined && (
              <span>({seller.reviewCount} reseñas)</span>
            )}
          </div>
        )}
        {seller.isDealership && (
          <span className="text-xs text-blue-600">Concesionario</span>
        )}
      </div>
    </div>
  );
};

// Favorite Button Component
const FavoriteButton: React.FC<{ listingId: string }> = ({ listingId: _listingId }) => {
  const [isFavorite, setIsFavorite] = useState(false);
  // TODO: Use _listingId to save/remove from favorites API
  
  return (
    <button
      onClick={() => setIsFavorite(!isFavorite)}
      className="p-2 rounded-full hover:bg-gray-100 transition-colors"
      aria-label={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
    >
      {isFavorite ? (
        <HeartSolidIcon className="h-6 w-6 text-red-500" />
      ) : (
        <HeartIcon className="h-6 w-6 text-gray-400" />
      )}
    </button>
  );
};

// Share Button Component
const ShareButton: React.FC = () => {
  const handleShare = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: document.title,
          url: window.location.href,
        });
      } catch (err) {
        console.error('Error sharing:', err);
      }
    } else {
      navigator.clipboard.writeText(window.location.href);
    }
  };

  return (
    <button
      onClick={handleShare}
      className="p-2 rounded-full hover:bg-gray-100 transition-colors"
      aria-label="Share"
    >
      <ShareIcon className="h-6 w-6 text-gray-400" />
    </button>
  );
};

// Skeleton Component
const VehicleDetailSkeleton: React.FC = () => {
  return (
    <div className="animate-pulse">
      <div className="aspect-[4/3] bg-gray-200 rounded-xl mb-8" />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-8">
          <div className="bg-gray-200 rounded-xl h-64" />
          <div className="bg-gray-200 rounded-xl h-48" />
        </div>
        <div>
          <div className="bg-gray-200 rounded-xl h-96" />
        </div>
      </div>
    </div>
  );
};

// Helper functions
function formatFuelType(type: string): string {
  const types: Record<string, string> = {
    gasoline: 'Gasolina',
    diesel: 'Diésel',
    electric: 'Eléctrico',
    hybrid: 'Híbrido',
    'plug-in-hybrid': 'Híbrido enchufable',
  };
  return types[type] || type;
}

function formatBodyType(type: string): string {
  const types: Record<string, string> = {
    sedan: 'Sedán',
    suv: 'SUV',
    truck: 'Pickup',
    coupe: 'Coupé',
    hatchback: 'Hatchback',
    van: 'Van',
    convertible: 'Convertible',
    wagon: 'Familiar',
  };
  return types[type] || type;
}

function formatDrivetrain(type: string): string {
  const types: Record<string, string> = {
    fwd: 'Tracción delantera',
    rwd: 'Tracción trasera',
    awd: 'AWD',
    '4wd': '4x4',
  };
  return types[type] || type;
}

function formatCondition(condition: string): string {
  const conditions: Record<string, string> = {
    new: 'Nuevo',
    used: 'Usado',
    'certified-pre-owned': 'Semi-nuevo certificado',
  };
  return conditions[condition] || condition;
}

export default VehicleDetailPage;
