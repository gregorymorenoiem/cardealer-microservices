/**
 * PropertyDetailPage - Detailed view for a real estate listing
 */

import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import { useListing, useSimilarListings } from '@/hooks/useMarketplace';
import { ListingGrid, PropertyMap } from '@/components/marketplace';
import type { PropertyListing } from '@/types/marketplace';
import { isPropertyListing } from '@/types/marketplace';

const PropertyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  
  const { data: listing, isLoading, error } = useListing(id || '');
  const { data: similarListings = [] } = useSimilarListings(listing || null, 4);

  // Check if it's a property
  const property = listing && isPropertyListing(listing) ? listing as PropertyListing : null;

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <PropertyDetailSkeleton />
        </div>
      </MainLayout>
    );
  }

  if (error || !property) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 text-center">
          <div className="w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <span className="text-4xl">🏠</span>
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-4">
            Propiedad no encontrada
          </h1>
          <p className="text-gray-500 mb-8">
            La propiedad que buscas no existe o ha sido removida.
          </p>
          <Link
            to="/marketplace?vertical=real-estate"
            className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-full transition-colors"
          >
            Ver todas las propiedades
          </Link>
        </div>
      </MainLayout>
    );
  }

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: property.currency,
      maximumFractionDigits: 0,
    }).format(price);
  };

  const propertyTypeLabels: Record<string, string> = {
    house: 'Casa',
    apartment: 'Departamento',
    condo: 'Condominio',
    townhouse: 'Casa Adosada',
    land: 'Terreno',
    commercial: 'Comercial',
    office: 'Oficina',
    warehouse: 'Bodega',
    building: 'Edificio',
  };

  return (
    <MainLayout>
      {/* Image Gallery */}
      <section className="bg-gray-100">
        <div className="max-w-7xl mx-auto">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-2">
            {/* Main image */}
            <div className="aspect-[4/3] lg:aspect-auto lg:h-[500px]">
              <img
                src={property.primaryImageUrl || property.images[0]?.url || '/placeholder.jpg'}
                alt={property.title}
                className="w-full h-full object-cover"
              />
            </div>
            {/* Secondary images */}
            <div className="hidden lg:grid grid-cols-2 gap-2 h-[500px]">
              {property.images.slice(1, 5).map((img, i) => (
                <div key={img.id} className="relative">
                  <img
                    src={img.url}
                    alt={`${property.title} - ${i + 2}`}
                    className="w-full h-full object-cover"
                  />
                  {i === 3 && property.images.length > 5 && (
                    <button className="absolute inset-0 bg-black/50 flex items-center justify-center text-white font-semibold hover:bg-black/60 transition-colors">
                      +{property.images.length - 5} fotos
                    </button>
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="lg:grid lg:grid-cols-3 lg:gap-8">
          {/* Main content */}
          <div className="lg:col-span-2">
            {/* Breadcrumb */}
            <nav className="flex items-center gap-2 text-sm text-gray-500 mb-4">
              <Link to="/marketplace" className="hover:text-gray-700">
                Marketplace
              </Link>
              <span>/</span>
              <Link to="/marketplace?vertical=real-estate" className="hover:text-gray-700">
                Propiedades
              </Link>
              <span>/</span>
              <span className="text-gray-900">
                <LocalizedContent content={property.title} showBadge={false} />
              </span>
            </nav>

            {/* Title and badges */}
            <div className="mb-6">
              <div className="flex flex-wrap gap-2 mb-3">
                <span className={`px-3 py-1 text-sm font-medium rounded-full ${
                  property.listingType === 'sale'
                    ? 'bg-blue-100 text-blue-800'
                    : 'bg-purple-100 text-purple-800'
                }`}>
                  {property.listingType === 'sale' ? 'En Venta' : 'En Renta'}
                </span>
                <span className="px-3 py-1 bg-gray-100 text-gray-700 text-sm font-medium rounded-full">
                  {propertyTypeLabels[property.propertyType] || property.propertyType}
                </span>
                {property.isFeatured && (
                  <span className="px-3 py-1 bg-amber-100 text-amber-800 text-sm font-medium rounded-full">
                    ⭐ Destacado
                  </span>
                )}
              </div>
              <h1 className="text-3xl font-bold text-gray-900 mb-2">
                <LocalizedContent content={property.title} showBadge={false} />
              </h1>
              <div className="flex items-center gap-2 text-gray-500">
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                {property.location.address}, {property.location.neighborhood}, {property.location.city}, {property.location.state}
              </div>
            </div>

            {/* Key features */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 p-4 bg-gray-50 rounded-2xl mb-8">
              {property.bedrooms > 0 && (
                <div className="text-center">
                  <div className="text-2xl font-bold text-gray-900">
                    {property.bedrooms}
                  </div>
                  <div className="text-sm text-gray-500">Recámaras</div>
                </div>
              )}
              {property.bathrooms > 0 && (
                <div className="text-center">
                  <div className="text-2xl font-bold text-gray-900">
                    {property.bathrooms}
                    {property.halfBathrooms ? `.${property.halfBathrooms}` : ''}
                  </div>
                  <div className="text-sm text-gray-500">Baños</div>
                </div>
              )}
              <div className="text-center">
                <div className="text-2xl font-bold text-gray-900">
                  {property.totalArea}
                </div>
                <div className="text-sm text-gray-500">
                  m² {property.builtArea && 'construidos'}
                </div>
              </div>
              {property.parkingSpaces && property.parkingSpaces > 0 && (
                <div className="text-center">
                  <div className="text-2xl font-bold text-gray-900">
                    {property.parkingSpaces}
                  </div>
                  <div className="text-sm text-gray-500">Estacionamientos</div>
                </div>
              )}
            </div>

            {/* Description */}
            <div className="mb-8">
              <h2 className="text-xl font-bold text-gray-900 mb-4">
                Descripción
              </h2>
              <div className="prose max-w-none text-gray-600">
                <p className="whitespace-pre-line">{property.description}</p>
              </div>
            </div>

            {/* Amenities */}
            {property.amenities.length > 0 && (
              <div className="mb-8">
                <h2 className="text-xl font-bold text-gray-900 mb-4">
                  Amenidades y características
                </h2>
                <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                  {property.amenities.map((amenity, i) => (
                    <div
                      key={i}
                      className="flex items-center gap-2 p-3 bg-gray-50 rounded-lg"
                    >
                      <span className="text-xl">{amenity.icon || '✓'}</span>
                      <span className="text-gray-700">{amenity.name}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Quick feature badges */}
            <div className="flex flex-wrap gap-2 mb-8">
              {property.hasPool && (
                <span className="px-3 py-1.5 bg-blue-50 text-blue-700 rounded-full text-sm">
                  🏊 Alberca
                </span>
              )}
              {property.hasGarden && (
                <span className="px-3 py-1.5 bg-green-50 text-green-700 rounded-full text-sm">
                  🌳 Jardín
                </span>
              )}
              {property.hasGym && (
                <span className="px-3 py-1.5 bg-orange-50 text-orange-700 rounded-full text-sm">
                  💪 Gimnasio
                </span>
              )}
              {property.hasSecurity && (
                <span className="px-3 py-1.5 bg-gray-100 text-gray-700 rounded-full text-sm">
                  🔒 Seguridad 24/7
                </span>
              )}
              {property.hasElevator && (
                <span className="px-3 py-1.5 bg-gray-100 text-gray-700 rounded-full text-sm">
                  🛗 Elevador
                </span>
              )}
              {property.isFurnished && (
                <span className="px-3 py-1.5 bg-amber-50 text-amber-700 rounded-full text-sm">
                  🛋️ Amueblado
                </span>
              )}
              {property.allowsPets && (
                <span className="px-3 py-1.5 bg-pink-50 text-pink-700 rounded-full text-sm">
                  🐕 Acepta mascotas
                </span>
              )}
            </div>

            {/* Property details */}
            <div className="mb-8">
              <h2 className="text-xl font-bold text-gray-900 mb-4">
                Detalles de la propiedad
              </h2>
              <div className="grid grid-cols-2 gap-4">
                <DetailRow label="Tipo" value={propertyTypeLabels[property.propertyType]} />
                <DetailRow label="Operación" value={property.listingType === 'sale' ? 'Venta' : 'Renta'} />
                <DetailRow label="Área total" value={`${property.totalArea} m²`} />
                {property.builtArea && <DetailRow label="Área construida" value={`${property.builtArea} m²`} />}
                {property.lotArea && <DetailRow label="Área de terreno" value={`${property.lotArea} m²`} />}
                {property.yearBuilt && <DetailRow label="Año de construcción" value={String(property.yearBuilt)} />}
                {property.floor && <DetailRow label="Piso" value={`${property.floor} de ${property.totalFloors}`} />}
                {property.maintenanceFee && (
                  <DetailRow label="Mantenimiento" value={formatPrice(property.maintenanceFee) + '/mes'} />
                )}
                {property.pricePerSqMeter && (
                  <DetailRow label="Precio por m²" value={formatPrice(property.pricePerSqMeter)} />
                )}
              </div>
            </div>

            {/* Location Map */}
            <div className="mt-8">
              <h2 className="text-xl font-bold text-gray-900 mb-4 flex items-center gap-2">
                <svg className="w-6 h-6 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                Ubicación
              </h2>
              <PropertyMap
                latitude={property.location.latitude}
                longitude={property.location.longitude}
                address={property.location.address}
                city={property.location.city}
                state={property.location.state}
                neighborhood={property.location.neighborhood}
              />
            </div>
          </div>

          {/* Sidebar */}
          <div className="lg:col-span-1">
            <div className="sticky top-24">
              {/* Price card */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-white rounded-2xl shadow-lg p-6 mb-6"
              >
                <div className="mb-4">
                  <div className="text-3xl font-bold text-gray-900">
                    {formatPrice(property.price)}
                    {property.listingType === 'rent' && (
                      <span className="text-lg font-normal text-gray-500">/mes</span>
                    )}
                  </div>
                  {property.isNegotiable && (
                    <span className="text-sm text-emerald-600">
                      Precio negociable
                    </span>
                  )}
                </div>

                <div className="space-y-3 mb-6">
                  <button className="w-full px-6 py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-xl transition-colors">
                    Contactar vendedor
                  </button>
                  <button className="w-full px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-900 font-medium rounded-xl transition-colors flex items-center justify-center gap-2">
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                    </svg>
                    Guardar
                  </button>
                </div>

                {/* Seller info */}
                <div className="border-t border-gray-200 pt-4">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-gradient-to-br from-emerald-400 to-teal-500 rounded-full flex items-center justify-center text-white font-bold text-lg">
                      {property.seller.name.charAt(0)}
                    </div>
                    <div className="flex-1">
                      <div className="font-semibold text-gray-900 flex items-center gap-1">
                        {property.seller.name}
                        {property.seller.isVerified && (
                          <svg className="w-4 h-4 text-blue-500" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                          </svg>
                        )}
                      </div>
                      {property.seller.rating && (
                        <div className="text-sm text-gray-500 flex items-center gap-1">
                          <svg className="w-4 h-4 text-amber-500" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                          </svg>
                          {property.seller.rating.toFixed(1)} ({property.seller.reviewCount} reseñas)
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </motion.div>

              {/* Stats */}
              <div className="bg-white rounded-2xl shadow-sm p-4">
                <div className="flex items-center justify-around text-center">
                  <div>
                    <div className="text-lg font-semibold text-gray-900">
                      {property.viewCount}
                    </div>
                    <div className="text-xs text-gray-500">Vistas</div>
                  </div>
                  <div className="w-px h-8 bg-gray-200" />
                  <div>
                    <div className="text-lg font-semibold text-gray-900">
                      {property.favoriteCount}
                    </div>
                    <div className="text-xs text-gray-500">Favoritos</div>
                  </div>
                  <div className="w-px h-8 bg-gray-200" />
                  <div>
                    <div className="text-lg font-semibold text-gray-900">
                      {property.inquiryCount}
                    </div>
                    <div className="text-xs text-gray-500">Consultas</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Similar properties */}
        {similarListings.length > 0 && (
          <section className="mt-16">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Propiedades similares
            </h2>
            <ListingGrid listings={similarListings} columns={4} />
          </section>
        )}
      </div>
    </MainLayout>
  );
};

// Detail row component
const DetailRow: React.FC<{ label: string; value: string }> = ({ label, value }) => (
  <div className="flex justify-between py-2 border-b border-gray-100">
    <span className="text-gray-500">{label}</span>
    <span className="font-medium text-gray-900">{value}</span>
  </div>
);

// Loading skeleton
const PropertyDetailSkeleton: React.FC = () => (
  <div className="animate-pulse">
    <div className="aspect-video bg-gray-200 rounded-xl mb-8" />
    <div className="h-8 bg-gray-200 rounded w-3/4 mb-4" />
    <div className="h-6 bg-gray-200 rounded w-1/2 mb-8" />
    <div className="grid grid-cols-4 gap-4 mb-8">
      {Array.from({ length: 4 }).map((_, i) => (
        <div key={i} className="h-16 bg-gray-200 rounded" />
      ))}
    </div>
    <div className="space-y-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className="h-4 bg-gray-200 rounded w-full" />
      ))}
    </div>
  </div>
);

export default PropertyDetailPage;
