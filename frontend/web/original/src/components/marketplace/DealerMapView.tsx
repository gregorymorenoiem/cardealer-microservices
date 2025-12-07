/**
 * DealerMapView - Interactive dealer locations with listing previews
 * Shows dealers on Google Maps with their active listings
 */

import React, { useState, useCallback } from 'react';
import { GoogleMap, useJsApiLoader, Marker, InfoWindow } from '@react-google-maps/api';
import { motion, AnimatePresence } from 'framer-motion';
import { FiMapPin, FiPhone, FiStar, FiX, FiNavigation, FiCheckCircle } from 'react-icons/fi';
import { Link } from 'react-router-dom';

interface DealerLocation {
  id: string;
  name: string;
  type: 'dealer' | 'individual';
  phone: string;
  rating: number;
  reviewCount: number;
  address: string;
  city: string;
  state: string;
  latitude: number;
  longitude: number;
  activeListings: number;
  featuredListings: DealerListing[];
  isVerified: boolean;
}

interface DealerListing {
  id: string;
  title: string;
  price: number;
  image: string;
  category: 'vehicle' | 'property';
  featured: boolean;
}

interface DealerMapViewProps {
  dealers: DealerLocation[];
  category: 'vehicles' | 'properties';
  userLocation?: { lat: number; lng: number };
}

const GOOGLE_MAPS_API_KEY = 'AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI';

const mapContainerStyle = {
  width: '100%',
  height: '600px',
};

const DealerMapView: React.FC<DealerMapViewProps> = ({
  dealers,
  category,
  userLocation,
}) => {
  const [selectedDealer, setSelectedDealer] = useState<DealerLocation | null>(null);
  const [hoveredDealer, setHoveredDealer] = useState<string | null>(null);
  const [infoWindowDealer, setInfoWindowDealer] = useState<string | null>(null);

  const { isLoaded, loadError } = useJsApiLoader({
    googleMapsApiKey: GOOGLE_MAPS_API_KEY,
    id: 'google-map-script',
  });

  // Center map on Santo Domingo, Dominican Republic
  const defaultCenter = {
    lat: userLocation?.lat || 18.4861,
    lng: userLocation?.lng || -69.9312,
  };

  // Calculate average center from all dealers for better view
  const center = dealers.length > 0 
    ? {
        lat: dealers.reduce((sum, d) => sum + d.latitude, 0) / dealers.length,
        lng: dealers.reduce((sum, d) => sum + d.longitude, 0) / dealers.length,
      }
    : defaultCenter;

  const onMarkerClick = useCallback((dealer: DealerLocation) => {
    setInfoWindowDealer(dealer.id);
  }, []);

  const onInfoWindowClose = useCallback(() => {
    setInfoWindowDealer(null);
  }, []);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const getDistanceToUser = (dealer: DealerLocation) => {
    if (!userLocation) return null;
    // Simple distance calculation (Haversine formula would be more accurate)
    const R = 6371; // Earth radius in km
    const dLat = (dealer.latitude - userLocation.lat) * Math.PI / 180;
    const dLon = (dealer.longitude - userLocation.lng) * Math.PI / 180;
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(userLocation.lat * Math.PI / 180) * Math.cos(dealer.latitude * Math.PI / 180) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c;
    return distance < 1 ? `${Math.round(distance * 1000)}m` : `${distance.toFixed(1)}km`;
  };

  if (loadError) {
    return (
      <div className="bg-gradient-to-br from-blue-50 to-emerald-50 rounded-2xl p-6 mb-8">
        <div className="bg-white rounded-xl shadow-lg p-8 text-center">
          <p className="text-red-600 font-semibold mb-2">Error al cargar Google Maps</p>
          <p className="text-gray-600 text-sm">Por favor, verifica tu conexión a internet e intenta nuevamente.</p>
        </div>
      </div>
    );
  }

  if (!isLoaded) {
    return (
      <div className="bg-gradient-to-br from-blue-50 to-emerald-50 rounded-2xl p-6 mb-8">
        <div className="bg-white rounded-xl shadow-lg h-[600px] flex items-center justify-center">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Cargando mapa...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gradient-to-br from-blue-50 to-emerald-50 rounded-2xl p-6 mb-8">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-900 mb-2 flex items-center gap-2">
          <FiMapPin className="text-emerald-600" />
          {category === 'vehicles' ? 'Dealers Cerca de Ti' : 'Inmobiliarias en Tu Zona'}
        </h2>
        <p className="text-gray-600">
          Encuentra {category === 'vehicles' ? 'dealers certificados' : 'inmobiliarias verificadas'} con publicaciones activas en tu área
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Map Section */}
        <div className="lg:col-span-2">
          <div className="bg-white rounded-xl shadow-lg overflow-hidden h-[600px] relative">
            <GoogleMap
              mapContainerStyle={mapContainerStyle}
              center={center}
              zoom={12}
              options={{
                zoomControl: true,
                streetViewControl: false,
                mapTypeControl: false,
                fullscreenControl: true,
                styles: [
                  {
                    featureType: 'poi',
                    elementType: 'labels',
                    stylers: [{ visibility: 'off' }]
                  }
                ]
              }}
            >
              {dealers.map((dealer, index) => {
                  const label = String.fromCharCode(65 + index); // A, B, C, etc.
                  const isHovered = hoveredDealer === dealer.id;
                  const isSelected = selectedDealer?.id === dealer.id;

                  // Create custom marker icon
                  const markerColor = isSelected ? '#3b82f6' : isHovered ? '#ef4444' : '#10b981';

                  return (
                    <React.Fragment key={dealer.id}>
                      <Marker
                        position={{ lat: dealer.latitude, lng: dealer.longitude }}
                        label={{
                          text: label,
                          color: 'white',
                          fontSize: '14px',
                          fontWeight: 'bold',
                        }}
                        icon={{
                          url: `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(`
                            <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 40 40">
                              <circle cx="20" cy="20" r="${isHovered || isSelected ? '18' : '16'}" fill="${markerColor}" stroke="white" stroke-width="3"/>
                              <text x="20" y="26" font-size="14" font-weight="bold" fill="white" text-anchor="middle">${label}</text>
                            </svg>
                          `)}`,
                          scaledSize: new window.google.maps.Size(40, 40),
                          anchor: new window.google.maps.Point(20, 20),
                        }}
                        onClick={() => onMarkerClick(dealer)}
                        onMouseOver={() => setHoveredDealer(dealer.id)}
                        onMouseOut={() => setHoveredDealer(null)}
                      />

                      {infoWindowDealer === dealer.id && (
                        <InfoWindow
                          position={{ lat: dealer.latitude, lng: dealer.longitude }}
                          onCloseClick={onInfoWindowClose}
                          options={{
                            pixelOffset: new window.google.maps.Size(0, -10),
                            maxWidth: 340,
                          }}
                        >
                          <div className="w-80">
                            {/* Header with verification badge */}
                            <div className="flex items-start justify-between mb-3">
                              <div className="flex-1">
                                <div className="flex items-center gap-2 mb-1">
                                  <h3 className="font-bold text-gray-900 text-base">{dealer.name}</h3>
                                  {dealer.isVerified && (
                                    <FiCheckCircle className="w-4 h-4 text-blue-600" />
                                  )}
                                </div>
                                <div className="flex items-center gap-2">
                                  <div className="flex items-center gap-1 text-amber-500">
                                    <FiStar className="fill-current w-4 h-4" />
                                    <span className="font-semibold text-sm">{dealer.rating}</span>
                                  </div>
                                  <span className="text-gray-400 text-xs">({dealer.reviewCount} reseñas)</span>
                                </div>
                              </div>
                            </div>

                            {/* Address */}
                            <div className="flex items-start gap-2 mb-3 text-gray-600 text-sm">
                              <FiMapPin className="w-4 h-4 mt-0.5 flex-shrink-0" />
                              <p>{dealer.address}, {dealer.city}</p>
                            </div>

                            {/* Active listings badge */}
                            <div className="bg-emerald-50 border border-emerald-200 rounded-lg px-3 py-2 mb-3">
                              <p className="text-sm text-emerald-700 font-medium">
                                📋 {dealer.activeListings} publicaciones activas
                              </p>
                            </div>

                            {/* Featured Listings Images */}
                            {dealer.featuredListings.length > 0 && (
                              <div className="mb-3">
                                <p className="text-xs font-semibold text-gray-500 uppercase mb-2">Destacados</p>
                                <div className="grid grid-cols-3 gap-2">
                                  {dealer.featuredListings.slice(0, 3).map((listing) => (
                                    <div key={listing.id} className="relative group">
                                      <img
                                        src={listing.image}
                                        alt={listing.title}
                                        className="w-full h-20 object-cover rounded-lg"
                                      />
                                      <div className="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-30 transition-all rounded-lg" />
                                    </div>
                                  ))}
                                </div>
                              </div>
                            )}

                            {/* Actions */}
                            <div className="flex gap-2">
                              <button
                                onClick={() => setSelectedDealer(dealer)}
                                className="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors"
                              >
                                Ver todo el catálogo
                              </button>
                              <a
                                href={`tel:${dealer.phone}`}
                                className="flex items-center justify-center px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                                title="Llamar"
                              >
                                <FiPhone className="w-4 h-4" />
                              </a>
                            </div>
                          </div>
                        </InfoWindow>
                      )}
                    </React.Fragment>
                  );
                })}
              </GoogleMap>
          </div>
        </div>

        {/* Dealers List Section */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-xl shadow-lg h-[600px] overflow-y-auto">
            <div className="sticky top-0 bg-white border-b border-gray-200 p-4 z-10">
              <h3 className="font-semibold text-gray-900">
                {dealers.length} {category === 'vehicles' ? 'Dealers' : 'Inmobiliarias'}
              </h3>
            </div>

            <div className="divide-y divide-gray-100">
              {dealers.map((dealer, index) => {
                const label = String.fromCharCode(65 + index);
                const distance = getDistanceToUser(dealer);
                
                return (
                  <motion.div
                    key={dealer.id}
                    className={`p-4 cursor-pointer transition-all ${
                      selectedDealer?.id === dealer.id
                        ? 'bg-blue-50 border-l-4 border-blue-600'
                        : hoveredDealer === dealer.id
                        ? 'bg-gray-50'
                        : 'hover:bg-gray-50'
                    }`}
                    onMouseEnter={() => setHoveredDealer(dealer.id)}
                    onMouseLeave={() => setHoveredDealer(null)}
                    onClick={() => setSelectedDealer(dealer)}
                    whileHover={{ scale: 1.01 }}
                  >
                    <div className="flex items-start gap-3">
                      {/* Letter Badge */}
                      <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm flex-shrink-0 ${
                        selectedDealer?.id === dealer.id
                          ? 'bg-blue-600 text-white'
                          : hoveredDealer === dealer.id
                          ? 'bg-red-500 text-white'
                          : 'bg-emerald-100 text-emerald-700'
                      }`}>
                        {label}
                      </div>

                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-2 mb-1">
                          <h4 className="font-semibold text-gray-900 text-sm truncate flex items-center gap-1">
                            {dealer.name}
                            {dealer.isVerified && (
                              <svg className="w-4 h-4 text-blue-500 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                              </svg>
                            )}
                          </h4>
                          {distance && (
                            <span className="text-xs text-gray-500 flex items-center gap-1 flex-shrink-0">
                              <FiNavigation className="w-3 h-3" />
                              {distance}
                            </span>
                          )}
                        </div>

                        <div className="flex items-center gap-2 text-xs text-gray-600 mb-2">
                          <div className="flex items-center gap-1">
                            <FiStar className="w-3 h-3 text-yellow-500 fill-yellow-500" />
                            <span className="font-medium">{dealer.rating}</span>
                            <span>({dealer.reviewCount})</span>
                          </div>
                          <span>•</span>
                          <span>{dealer.activeListings} activos</span>
                        </div>

                        <p className="text-xs text-gray-500 truncate">
                          {dealer.city}, {dealer.state}
                        </p>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </div>
        </div>
      </div>

      {/* Selected Dealer Modal/Detail */}
      <AnimatePresence>
        {selectedDealer && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
            onClick={() => setSelectedDealer(null)}
          >
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-white rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto shadow-2xl"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div className="sticky top-0 bg-gradient-to-r from-blue-600 to-emerald-600 text-white p-6 flex items-start justify-between">
                <div>
                  <h3 className="text-2xl font-bold mb-2 flex items-center gap-2">
                    {selectedDealer.name}
                    {selectedDealer.isVerified && (
                      <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M6.267 3.455a3.066 3.066 0 001.745-.723 3.066 3.066 0 013.976 0 3.066 3.066 0 001.745.723 3.066 3.066 0 012.812 2.812c.051.643.304 1.254.723 1.745a3.066 3.066 0 010 3.976 3.066 3.066 0 00-.723 1.745 3.066 3.066 0 01-2.812 2.812 3.066 3.066 0 00-1.745.723 3.066 3.066 0 01-3.976 0 3.066 3.066 0 00-1.745-.723 3.066 3.066 0 01-2.812-2.812 3.066 3.066 0 00-.723-1.745 3.066 3.066 0 010-3.976 3.066 3.066 0 00.723-1.745 3.066 3.066 0 012.812-2.812zm7.44 5.252a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                      </svg>
                    )}
                  </h3>
                  <div className="flex items-center gap-4 text-sm">
                    <div className="flex items-center gap-1">
                      <FiStar className="fill-yellow-400 text-yellow-400" />
                      <span className="font-semibold">{selectedDealer.rating}</span>
                      <span className="opacity-90">({selectedDealer.reviewCount} reseñas)</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <FiMapPin />
                      <span>{selectedDealer.city}, {selectedDealer.state}</span>
                    </div>
                  </div>
                </div>
                <button
                  onClick={() => setSelectedDealer(null)}
                  className="p-2 hover:bg-white/20 rounded-lg transition-colors"
                >
                  <FiX className="w-6 h-6" />
                </button>
              </div>

              {/* Content */}
              <div className="p-6">
                {/* Contact Info */}
                <div className="bg-gray-50 rounded-xl p-4 mb-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-gray-600 mb-1">Contacto</p>
                      <div className="flex items-center gap-2">
                        <FiPhone className="text-emerald-600" />
                        <a href={`tel:${selectedDealer.phone}`} className="font-semibold text-gray-900 hover:text-emerald-600">
                          {selectedDealer.phone}
                        </a>
                      </div>
                    </div>
                    <a
                      href={`https://www.google.com/maps/dir/?api=1&destination=${selectedDealer.latitude},${selectedDealer.longitude}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-sm font-medium transition-colors flex items-center gap-2"
                    >
                      <FiNavigation className="w-4 h-4" />
                      Cómo llegar
                    </a>
                  </div>
                </div>

                {/* Featured Listings */}
                <div>
                  <h4 className="text-lg font-bold text-gray-900 mb-4">
                    Publicaciones Destacadas ({selectedDealer.activeListings})
                  </h4>
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {selectedDealer.featuredListings.map((listing) => (
                      <Link
                        key={listing.id}
                        to={`/${listing.category === 'vehicle' ? 'vehicles' : 'properties'}/${listing.id}`}
                        className="group"
                      >
                        <div className="bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-lg transition-shadow">
                          <div className="aspect-[4/3] overflow-hidden">
                            <img
                              src={listing.image}
                              alt={listing.title}
                              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                            />
                          </div>
                          <div className="p-3">
                            <p className="text-sm font-medium text-gray-900 line-clamp-1 mb-2">
                              {listing.title}
                            </p>
                            <p className="text-lg font-bold text-emerald-600">
                              {formatPrice(listing.price)}
                            </p>
                          </div>
                        </div>
                      </Link>
                    ))}
                  </div>

                  {selectedDealer.activeListings > selectedDealer.featuredListings.length && (
                    <div className="mt-4 text-center">
                      <button className="px-6 py-3 border-2 border-emerald-600 text-emerald-600 hover:bg-emerald-600 hover:text-white rounded-lg font-medium transition-colors">
                        Ver todas las publicaciones ({selectedDealer.activeListings})
                      </button>
                    </div>
                  )}
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default DealerMapView;
