/**
 * PropertyMap - Interactive map component for property listings
 * Uses OpenStreetMap with Leaflet for free map display
 */

import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

interface PropertyMapProps {
  latitude?: number;
  longitude?: number;
  address: string;
  city: string;
  state: string;
  neighborhood?: string;
  className?: string;
  height?: string;
  showFullscreenButton?: boolean;
  showNearbyPOIs?: boolean;
}

interface NearbyPOI {
  id: string;
  name: string;
  type: 'school' | 'hospital' | 'supermarket' | 'park' | 'restaurant' | 'gym' | 'bank' | 'pharmacy';
  distance: string;
  icon: string;
}

// Mock POIs for demo - in production this would come from Google Places API or similar
const generateMockPOIs = (_lat: number, _lng: number): NearbyPOI[] => [
  { id: '1', name: 'Escuela Primaria Juárez', type: 'school', distance: '300m', icon: '🏫' },
  { id: '2', name: 'Hospital General', type: 'hospital', distance: '1.2km', icon: '🏥' },
  { id: '3', name: 'Superama', type: 'supermarket', distance: '450m', icon: '🛒' },
  { id: '4', name: 'Parque Lincoln', type: 'park', distance: '600m', icon: '🌳' },
  { id: '5', name: 'Sport City', type: 'gym', distance: '800m', icon: '💪' },
  { id: '6', name: 'BBVA', type: 'bank', distance: '200m', icon: '🏦' },
];

const PropertyMap: React.FC<PropertyMapProps> = ({
  latitude,
  longitude,
  address,
  city,
  state,
  neighborhood,
  className = '',
  height = '400px',
  showFullscreenButton = true,
  showNearbyPOIs = true,
}) => {
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [activeTab, setActiveTab] = useState<'map' | 'satellite' | 'street'>('map');
  const [showPOIs, setShowPOIs] = useState(true);

  // Default to Mexico City if no coordinates
  const lat = latitude || 19.4326;
  const lng = longitude || -99.1332;
  const hasCoordinates = latitude !== undefined && longitude !== undefined;

  const nearbyPOIs = hasCoordinates ? generateMockPOIs(lat, lng) : [];

  // Google Maps link for directions
  const googleMapsUrl = `https://www.google.com/maps/search/?api=1&query=${lat},${lng}`;
  const googleMapsDirectionsUrl = `https://www.google.com/maps/dir/?api=1&destination=${lat},${lng}`;

  const fullAddress = [address, neighborhood, city, state].filter(Boolean).join(', ');

  const handleFullscreen = () => {
    setIsFullscreen(!isFullscreen);
  };

  const handleOpenInGoogleMaps = () => {
    window.open(googleMapsUrl, '_blank');
  };

  const handleGetDirections = () => {
    window.open(googleMapsDirectionsUrl, '_blank');
  };

  // Close fullscreen on escape key
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isFullscreen) {
        setIsFullscreen(false);
      }
    };
    window.addEventListener('keydown', handleEscape);
    return () => window.removeEventListener('keydown', handleEscape);
  }, [isFullscreen]);

  return (
    <>
      <div className={`relative rounded-xl overflow-hidden ${className}`} style={{ height }}>
        {/* Map Image/Iframe */}
        <div className="absolute inset-0">
          {hasCoordinates ? (
            <iframe
              title="Property Location Map"
              width="100%"
              height="100%"
              frameBorder="0"
              scrolling="no"
              src={`https://www.openstreetmap.org/export/embed.html?bbox=${lng - 0.01},${lat - 0.008},${lng + 0.01},${lat + 0.008}&layer=mapnik&marker=${lat},${lng}`}
              className="w-full h-full"
            />
          ) : (
            <div className="w-full h-full bg-gray-200 flex items-center justify-center">
              <div className="text-center text-gray-500">
                <svg className="w-16 h-16 mx-auto mb-4 opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <p className="text-sm font-medium">Ubicación aproximada</p>
                <p className="text-xs mt-1">{fullAddress}</p>
              </div>
            </div>
          )}
        </div>

        {/* Overlay Controls */}
        <div className="absolute top-4 right-4 flex flex-col gap-2 z-10">
          {showFullscreenButton && (
            <button
              onClick={handleFullscreen}
              className="p-2 bg-white rounded-lg shadow-lg hover:bg-gray-50 transition-colors"
              title="Pantalla completa"
            >
              <svg className="w-5 h-5 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
              </svg>
            </button>
          )}
          <button
            onClick={handleOpenInGoogleMaps}
            className="p-2 bg-white rounded-lg shadow-lg hover:bg-gray-50 transition-colors"
            title="Abrir en Google Maps"
          >
            <svg className="w-5 h-5 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
            </svg>
          </button>
        </div>

        {/* Bottom Info Bar */}
        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/70 to-transparent p-4">
          <div className="flex items-end justify-between">
            <div className="text-white">
              <p className="font-medium text-sm">{neighborhood || city}</p>
              <p className="text-xs text-white/80">{city}, {state}</p>
            </div>
            <button
              onClick={handleGetDirections}
              className="px-4 py-2 bg-white/20 backdrop-blur-sm hover:bg-white/30 text-white text-sm font-medium rounded-lg transition-colors flex items-center gap-2"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
              </svg>
              Cómo llegar
            </button>
          </div>
        </div>
      </div>

      {/* Nearby POIs Section */}
      {showNearbyPOIs && nearbyPOIs.length > 0 && (
        <div className="mt-4">
          <div className="flex items-center justify-between mb-3">
            <h3 className="text-sm font-semibold text-gray-700">
              Lugares cercanos
            </h3>
            <button
              onClick={() => setShowPOIs(!showPOIs)}
              className="text-xs text-emerald-600 hover:underline"
            >
              {showPOIs ? 'Ocultar' : 'Mostrar'}
            </button>
          </div>
          <AnimatePresence>
            {showPOIs && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                className="grid grid-cols-2 md:grid-cols-3 gap-2"
              >
                {nearbyPOIs.map((poi) => (
                  <div
                    key={poi.id}
                    className="flex items-center gap-2 p-2 bg-gray-50 rounded-lg text-sm"
                  >
                    <span className="text-lg">{poi.icon}</span>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-gray-800 truncate">
                        {poi.name}
                      </p>
                      <p className="text-xs text-gray-500">{poi.distance}</p>
                    </div>
                  </div>
                ))}
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      )}

      {/* Fullscreen Modal */}
      <AnimatePresence>
        {isFullscreen && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 bg-black"
          >
            {/* Close button */}
            <button
              onClick={handleFullscreen}
              className="absolute top-4 right-4 z-50 p-3 bg-white/10 backdrop-blur-sm rounded-full hover:bg-white/20 transition-colors"
            >
              <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            {/* Map style tabs */}
            <div className="absolute top-4 left-4 z-50 flex gap-2">
              {(['map', 'satellite', 'street'] as const).map((style) => (
                <button
                  key={style}
                  onClick={() => setActiveTab(style)}
                  className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                    activeTab === style
                      ? 'bg-white text-gray-900'
                      : 'bg-white/20 text-white hover:bg-white/30'
                  }`}
                >
                  {style === 'map' && 'Mapa'}
                  {style === 'satellite' && 'Satélite'}
                  {style === 'street' && 'Calles'}
                </button>
              ))}
            </div>

            {/* Full map iframe */}
            <iframe
              title="Property Location Map Fullscreen"
              width="100%"
              height="100%"
              frameBorder="0"
              scrolling="no"
              src={`https://www.openstreetmap.org/export/embed.html?bbox=${lng - 0.02},${lat - 0.015},${lng + 0.02},${lat + 0.015}&layer=${activeTab === 'satellite' ? 'cyclemap' : 'mapnik'}&marker=${lat},${lng}`}
              className="w-full h-full"
            />

            {/* Bottom info panel */}
            <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-6">
              <div className="max-w-4xl mx-auto flex items-end justify-between">
                <div className="text-white">
                  <h2 className="text-xl font-bold mb-1">{neighborhood || address}</h2>
                  <p className="text-white/80">{fullAddress}</p>
                </div>
                <div className="flex gap-3">
                  <button
                    onClick={handleOpenInGoogleMaps}
                    className="px-5 py-2.5 bg-white/20 backdrop-blur-sm hover:bg-white/30 text-white font-medium rounded-xl transition-colors flex items-center gap-2"
                  >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                    </svg>
                    Ver en Google Maps
                  </button>
                  <button
                    onClick={handleGetDirections}
                    className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-xl transition-colors flex items-center gap-2"
                  >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
                    </svg>
                    Cómo llegar
                  </button>
                </div>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
};

export default PropertyMap;
