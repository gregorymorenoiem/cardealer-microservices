/**
 * PropertyDetailPage - Detailed view for a real estate listing
 */

import React from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FaSwimmingPool, FaTree, FaDumbbell, FaShieldAlt, FaDog } from 'react-icons/fa';
import { MdElevator } from 'react-icons/md';
import { GiSofa } from 'react-icons/gi';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import { useListing, useSimilarListings } from '@/hooks/useMarketplace';
import { ListingGrid, PropertyMap } from '@/components/marketplace';
import ContactSellerForm from '@/components/organisms/ContactSellerForm';
import type { PropertyListing } from '@/types/marketplace';
import { isPropertyListing } from '@/types/marketplace';

// Extract the ID from SEO-friendly URL (e.g., "penthouse-de-lujo-con-vista-al-mar-p1" -> "p1")
const extractIdFromSlug = (slugWithId: string): string => {
  const parts = slugWithId.split('-');
  return parts[parts.length - 1] || slugWithId;
};

const PropertyDetailPage: React.FC = () => {
  const { t } = useTranslation(['properties', 'common']);
  const { id: slugWithId } = useParams<{ id: string }>();
  const id = slugWithId ? extractIdFromSlug(slugWithId) : undefined;
  
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
            {t('properties:detail.notFound')}
          </h1>
          <p className="text-gray-500 mb-8">
            {t('properties:detail.notFoundMessage')}
          </p>
          <Link
            to="/marketplace?vertical=real-estate"
            className="inline-flex items-center gap-2 px-6 py-3 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-full transition-colors"
          >
            {t('properties:detail.viewAll')}
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
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Breadcrumb */}
          <nav className="flex items-center gap-2 text-sm text-gray-600 mb-6">
            <Link to="/" className="hover:text-emerald-600 transition-colors duration-200">
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
            </Link>
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
            <Link to="/properties" className="hover:text-emerald-600 transition-colors duration-200">
              Propiedades
            </Link>
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
            <span className="text-gray-900 font-medium">
              <LocalizedContent content={property.title} showBadge={false} />
            </span>
          </nav>

          {/* Title & Price */}
          <div className="mb-8">
            <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 mb-4">
              <div className="flex-1">
                <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
                  <LocalizedContent content={property.title} showBadge={false} />
                </h1>
                <div className="flex items-center gap-4 text-gray-600">
                  <span className="flex items-center gap-1">
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    {property.location.city}, {property.location.state}
                  </span>
                  <span className={`px-3 py-1 text-sm font-medium rounded-full ${
                    property.listingType === 'sale'
                      ? 'bg-blue-100 text-blue-800'
                      : 'bg-purple-100 text-purple-800'
                  }`}>
                    {property.listingType === 'sale' ? 'En Venta' : 'En Renta'}
                  </span>
                </div>
              </div>
              <div className="flex flex-col sm:flex-row items-end gap-3">
                <div className="flex items-center gap-3 print:hidden">
                  <button className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-all border-2 border-gray-300 text-gray-700 hover:border-emerald-500 hover:text-emerald-500">
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                    </svg>
                    Guardar
                  </button>
                  <button className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-all border-2 border-gray-300 text-gray-700 hover:border-emerald-500 hover:text-emerald-500">
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                    </svg>
                    Compartir
                  </button>
                </div>
                <div className="text-right">
                  <p className="text-4xl font-bold text-emerald-600">
                    {formatPrice(property.price)}
                    {property.listingType === 'rent' && (
                      <span className="text-lg font-normal text-gray-500">/mes</span>
                    )}
                  </p>
                  {property.isNegotiable && (
                    <span className="text-sm text-emerald-600">Precio negociable</span>
                  )}
                </div>
              </div>
            </div>

            {/* Badges */}
            <div className="flex flex-wrap gap-2">
              <span className="px-3 py-1 bg-gray-100 text-gray-700 text-sm font-medium rounded-full">
                {propertyTypeLabels[property.propertyType] || property.propertyType}
              </span>
              {property.isFeatured && (
                <span className="px-3 py-1 bg-amber-100 text-amber-800 text-sm font-medium rounded-full">
                  ⭐ Destacado
                </span>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left Column - Images & Details */}
            <div className="lg:col-span-2 space-y-8">
              {/* Image Gallery */}
              <div className="bg-white rounded-xl shadow-card overflow-hidden">
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-2">
                  {/* Main image */}
                  <div className="aspect-[4/3] lg:col-span-2">
                    <img
                      src={property.primaryImageUrl || property.images[0]?.url || '/placeholder.jpg'}
                      alt={property.title}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  {/* Secondary images */}
                  {property.images.slice(1, 5).map((img, i) => (
                    <div key={img.id} className="relative aspect-[4/3]">
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

            {/* Key features */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 p-4 bg-white rounded-xl shadow-card">
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
            <div className="bg-white rounded-xl shadow-card p-6">
              <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                Descripción
              </h2>
              <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                {property.description}
              </p>
            </div>

            {/* Amenities */}
            {property.amenities.length > 0 && (
              <div className="bg-white rounded-xl shadow-card p-6">
                <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                  Amenidades y características
                </h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {property.amenities.map((amenity, i) => (
                    <div
                      key={i}
                      className="flex items-center gap-2 text-gray-700"
                    >
                      <div className="w-2 h-2 bg-emerald-600 rounded-full flex-shrink-0" />
                      <span>{amenity.name}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Quick feature badges */}
            {(property.hasPool || property.hasGarden || property.hasGym || property.hasSecurity || property.hasElevator || property.isFurnished || property.allowsPets) && (
              <div className="bg-white rounded-xl shadow-card p-6">
                <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                  Características adicionales
                </h2>
                <div className="flex flex-wrap gap-2">
                  {property.hasPool && (
                    <span className="px-3 py-1.5 bg-blue-50 text-blue-700 rounded-full text-sm flex items-center gap-2">
                      <FaSwimmingPool className="w-4 h-4" /> Alberca
                    </span>
                  )}
                  {property.hasGarden && (
                    <span className="px-3 py-1.5 bg-green-50 text-green-700 rounded-full text-sm flex items-center gap-2">
                      <FaTree className="w-4 h-4" /> Jardín
                    </span>
                  )}
                  {property.hasGym && (
                    <span className="px-3 py-1.5 bg-orange-50 text-orange-700 rounded-full text-sm flex items-center gap-2">
                      <FaDumbbell className="w-4 h-4" /> Gimnasio
                    </span>
                  )}
                  {property.hasSecurity && (
                    <span className="px-3 py-1.5 bg-gray-100 text-gray-700 rounded-full text-sm flex items-center gap-2">
                      <FaShieldAlt className="w-4 h-4" /> Seguridad 24/7
                    </span>
                  )}
                  {property.hasElevator && (
                    <span className="px-3 py-1.5 bg-gray-100 text-gray-700 rounded-full text-sm flex items-center gap-2">
                      <MdElevator className="w-4 h-4" /> Elevador
                    </span>
                  )}
                  {property.isFurnished && (
                    <span className="px-3 py-1.5 bg-amber-50 text-amber-700 rounded-full text-sm flex items-center gap-2">
                      <GiSofa className="w-4 h-4" /> Amueblado
                    </span>
                  )}
                  {property.allowsPets && (
                    <span className="px-3 py-1.5 bg-pink-50 text-pink-700 rounded-full text-sm flex items-center gap-2">
                      <FaDog className="w-4 h-4" /> Acepta mascotas
                    </span>
                  )}
                </div>
              </div>
            )}

            {/* Property details */}
            <div className="bg-white rounded-xl shadow-card p-6">
              <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
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
            <div className="bg-white rounded-xl shadow-card p-6">
              <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4 flex items-center gap-2">
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

          {/* Right Column - Seller Info & Contact */}
          <div className="space-y-6">
            {/* Seller Info */}
            <div className="bg-white rounded-xl shadow-card p-6">
              <h3 className="text-xl font-bold font-heading text-gray-900 mb-4">
                Información del vendedor
              </h3>
              
              <div className="space-y-4 mb-6">
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

                <div className="flex items-center gap-2 text-gray-700">
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                  </svg>
                  <a
                    href={`tel:${property.seller.phone}`}
                    className="hover:text-emerald-600 transition-colors duration-200"
                  >
                    {property.seller.phone}
                  </a>
                </div>
              </div>

              <Link to={`tel:${property.seller.phone}`}>
                <button className="w-full py-3 px-4 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors duration-200 font-medium mb-3">
                  Llamar al vendedor
                </button>
              </Link>

              <Link to="/properties">
                <button className="w-full py-3 px-4 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 font-medium">
                  Ver más propiedades
                </button>
              </Link>
            </div>

            {/* Contact Form */}
            <ContactSellerForm
              vehicleName={property.title || 'esta propiedad'}
              sellerName={property.seller.name}
              sellerPhone={property.seller.phone || ''}
            />

            {/* Stats */}
            <div className="bg-white rounded-xl shadow-card p-6">
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

        {/* Similar properties */}
        {similarListings.length > 0 && (
          <section className="mt-8 print:hidden">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Propiedades similares
            </h2>
            <ListingGrid listings={similarListings} columns={4} />
          </section>
        )}
        </div>
      </div>

      {/* Print Styles */}
      <style>{`
        @media print {
          nav, .print\\:hidden, button:not(.print\\:block) {
            display: none !important;
          }

          body {
            background: white !important;
          }

          .bg-gray-50 {
            background: white !important;
          }

          .shadow-card, .shadow-sm, .shadow-md, .shadow-lg {
            box-shadow: none !important;
            border: 1px solid #e5e7eb;
          }

          .max-w-7xl {
            max-width: 100% !important;
          }

          .grid-cols-1.lg\\:grid-cols-3 {
            grid-template-columns: 1fr !important;
          }

          .lg\\:col-span-2 {
            grid-column: span 1 !important;
          }

          img {
            max-width: 100%;
            page-break-inside: avoid;
          }
        }
      `}</style>
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

