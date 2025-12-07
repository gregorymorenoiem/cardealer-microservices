/**
 * MapViewPage - Professional dealer map view for vehicles
 * Shows dealers with their inventory on an interactive map
 */

import { useState, useCallback, useMemo, useEffect, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { GoogleMap, useJsApiLoader, OverlayView } from '@react-google-maps/api';
import MainLayout from '@/layouts/MainLayout';
import { mockVehicleDealers } from '@/data/mockDealers';
import { 
  FiX, FiPhone, FiNavigation, FiShare2, FiStar, 
  FiMapPin, FiFilter, FiChevronLeft, FiCheckCircle,
  FiMessageCircle, FiCopy, FiExternalLink
} from 'react-icons/fi';
import { motion, AnimatePresence } from 'framer-motion';

const GOOGLE_MAPS_API_KEY = 'AIzaSyDKVgKqLUzWFaEMcXjkZUUTOFDNa4V0AFI';

const mapContainerStyle = {
  width: '100%',
  height: '100vh',
};

interface DealerFilters {
  verified: boolean;
  maxDistance: number; // in km, 0 = show all
}

export default function MapViewPage() {
  const navigate = useNavigate();
  const location = useLocation();
  
  // Restore state from URL params
  const urlParams = new URLSearchParams(location.search);
  const dealerIdFromUrl = urlParams.get('dealer');
  const pageFromUrl = urlParams.get('page');
  
  const [selectedDealer, setSelectedDealer] = useState<string | null>(dealerIdFromUrl);
  const [hoveredDealer, setHoveredDealer] = useState<string | null>(null);
  const [showFilters, setShowFilters] = useState(false);
  const [showShareMenu, setShowShareMenu] = useState(false);
  const [imageStartIndex, setImageStartIndex] = useState(pageFromUrl ? (parseInt(pageFromUrl) - 1) * 3 : 0);
  const [filters, setFilters] = useState<DealerFilters>({
    verified: false,
    maxDistance: 0, // 0 = show all
  });
  const [userLocation] = useState({ lat: 18.4861, lng: -69.9312 });
  const filtersRef = useRef<HTMLDivElement>(null);

  const { isLoaded, loadError } = useJsApiLoader({
    googleMapsApiKey: GOOGLE_MAPS_API_KEY,
    id: 'google-map-script',
  });

  // Close filters when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (showFilters && filtersRef.current && !filtersRef.current.contains(event.target as Node)) {
        setShowFilters(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [showFilters]);

  // Update URL when dealer selection changes
  useEffect(() => {
    if (selectedDealer) {
      const params = new URLSearchParams();
      params.set('dealer', selectedDealer);
      const currentPage = Math.floor(imageStartIndex / 3) + 1;
      params.set('page', currentPage.toString());
      navigate(`?${params.toString()}`, { replace: true });
    }
  }, [selectedDealer, imageStartIndex, navigate]);

  // Calculate distance from user location
  const getDistance = useCallback((dealer: typeof mockVehicleDealers[0]) => {
    const R = 6371;
    const dLat = (dealer.latitude - userLocation.lat) * Math.PI / 180;
    const dLon = (dealer.longitude - userLocation.lng) * Math.PI / 180;
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(userLocation.lat * Math.PI / 180) * Math.cos(dealer.latitude * Math.PI / 180) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    const distance = R * c;
    return distance < 1 ? `${Math.round(distance * 1000)}m` : `${distance.toFixed(1)} km`;
  }, [userLocation]);

  // Filter dealers
  const filteredDealers = useMemo(() => {
    return mockVehicleDealers.filter(dealer => {
      if (filters.verified && !dealer.isVerified) return false;
      
      // Distance filter
      if (filters.maxDistance > 0) {
        const distance = parseFloat(getDistance(dealer).replace(' km', ''));
        if (distance > filters.maxDistance) return false;
      }
      
      return true;
    });
  }, [filters, getDistance]);

  // Calculate map center
  const center = useMemo(() => {
    if (filteredDealers.length === 0) return userLocation;
    return {
      lat: filteredDealers.reduce((sum, d) => sum + d.latitude, 0) / filteredDealers.length,
      lng: filteredDealers.reduce((sum, d) => sum + d.longitude, 0) / filteredDealers.length,
    };
  }, [filteredDealers, userLocation]);

  const selectedDealerData = useMemo(() => {
    const dealer = filteredDealers.find(d => d.id === selectedDealer);
    if (dealer) {
      setImageStartIndex(0); // Reset cuando cambia el dealer
    }
    return dealer;
  }, [selectedDealer, filteredDealers]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const handleShare = useCallback((dealer: typeof mockVehicleDealers[0], method: 'copy' | 'whatsapp') => {
    const url = `${window.location.origin}/vehicles/map?dealer=${dealer.id}`;
    const message = `🚗 ${dealer.name}\n📍 ${dealer.address}\n⭐ ${dealer.rating}/5 (${dealer.reviewCount} reseñas)\n🔗 ${url}`;

    if (method === 'copy') {
      navigator.clipboard.writeText(url);
      alert('¡Enlace copiado al portapapeles!');
    } else if (method === 'whatsapp') {
      window.open(`https://wa.me/?text=${encodeURIComponent(message)}`, '_blank');
    }
    setShowShareMenu(false);
  }, []);

  if (loadError) {
    return (
      <MainLayout>
        <div className="h-screen flex items-center justify-center">
          <div className="text-center">
            <p className="text-red-600 font-semibold mb-2">Error al cargar el mapa</p>
            <p className="text-gray-600">Por favor, intenta nuevamente más tarde</p>
            <button
              onClick={() => navigate('/vehicles')}
              className="mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Volver a la lista
            </button>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!isLoaded) {
    return (
      <MainLayout>
        <div className="h-screen flex items-center justify-center bg-gray-50">
          <div className="text-center">
            <div className="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600 font-medium">Cargando mapa de dealers...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="relative h-screen">
        {/* Top Navigation Bar */}
        <div className="absolute top-0 left-0 right-0 z-10 bg-white/95 backdrop-blur-sm shadow-md">
          <div className="max-w-7xl mx-auto px-3 py-2 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <button
                onClick={() => navigate('/vehicles')}
                className="flex items-center gap-1 text-gray-700 hover:text-gray-900 font-medium text-sm"
              >
                <FiChevronLeft size={18} />
                <span>Volver</span>
              </button>
              <div className="h-5 w-px bg-gray-300"></div>
              <div>
                <h1 className="text-base font-bold text-gray-900">
                  {filteredDealers.length} dealers
                </h1>
              </div>
            </div>

            <div className="flex items-center gap-2">
              {/* Filter Button */}
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors shadow-sm ${
                  showFilters ? 'bg-gray-900 text-white' : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
                }`}
              >
                <FiFilter size={16} />
                <span className="hidden sm:inline">Filtros</span>
              </button>
            </div>
          </div>

          {/* Filters Panel */}
          <AnimatePresence>
            {showFilters && (
              <motion.div
                ref={filtersRef}
                initial={{ height: 0, opacity: 0 }}
                animate={{ height: 'auto', opacity: 1 }}
                exit={{ height: 0, opacity: 0 }}
                className="border-t border-gray-200 overflow-hidden bg-white/95"
              >
                <div className="max-w-7xl mx-auto px-4 py-4">
                  <div className="max-w-md space-y-4">
                    {/* Distance slider */}
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <label className="text-sm font-medium text-gray-700">Radio de búsqueda</label>
                        <span className="text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">
                          {filters.maxDistance === 0 ? 'Todos' : `${filters.maxDistance} km`}
                        </span>
                      </div>
                      <input
                        type="range"
                        min="0"
                        max="50"
                        step="5"
                        value={filters.maxDistance}
                        onChange={(e) => setFilters({ ...filters, maxDistance: parseInt(e.target.value) })}
                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-blue-600"
                      />
                      <div className="flex justify-between text-xs text-gray-500 mt-1">
                        <span>Todos</span>
                        <span>5km</span>
                        <span>10km</span>
                        <span>25km</span>
                        <span>50km</span>
                      </div>
                    </div>

                    {/* Verified toggle */}
                    <div className="flex items-center justify-between py-2">
                      <label className="text-sm font-medium text-gray-700 flex items-center gap-2">
                        <FiCheckCircle className="w-4 h-4 text-blue-500" />
                        Solo dealers verificados
                      </label>
                      <button
                        onClick={() => setFilters({ ...filters, verified: !filters.verified })}
                        className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                          filters.verified ? 'bg-blue-600' : 'bg-gray-300'
                        }`}
                      >
                        <span
                          className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                            filters.verified ? 'translate-x-6' : 'translate-x-1'
                          }`}
                        />
                      </button>
                    </div>

                    {/* Clear filters button */}
                    <button
                      onClick={() => setFilters({ verified: false, maxDistance: 0 })}
                      className="text-sm text-blue-600 hover:text-blue-700 font-medium hover:underline"
                    >
                      Limpiar filtros
                    </button>
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Google Map */}
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
          {filteredDealers.map((dealer) => {
            const isSelected = selectedDealer === dealer.id;
            const isHovered = hoveredDealer === dealer.id;
            const zIndex = isHovered ? 1000 : (isSelected ? 100 : 10);
            
            return (
              <OverlayView
                key={dealer.id}
                position={{ lat: dealer.latitude, lng: dealer.longitude }}
                mapPaneName={OverlayView.OVERLAY_MOUSE_TARGET}
              >
                <div
                  onClick={() => {
                    setSelectedDealer(dealer.id);
                    setImageStartIndex(0);
                  }}
                  onMouseEnter={() => setHoveredDealer(dealer.id)}
                  onMouseLeave={() => setHoveredDealer(null)}
                  className="cursor-pointer transform -translate-x-1/2 -translate-y-1/2 group relative"
                  style={{ position: 'absolute', zIndex }}
                >
                  {/* Animated pulse ring for verified dealers */}
                  {dealer.isVerified && !isSelected && (
                    <div className="absolute inset-0 rounded-full animate-ping opacity-20 bg-blue-500" style={{ animationDuration: '2s' }}></div>
                  )}
                  
                  {/* Enhanced tooltip with glassmorphism */}
                  <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-3 opacity-0 group-hover:opacity-100 transition-all duration-300 pointer-events-none scale-95 group-hover:scale-100">
                    <div className="backdrop-blur-md bg-white/90 border border-white/20 shadow-2xl px-4 py-2.5 rounded-xl">
                      <div className="flex items-center gap-2 mb-1">
                        <p className="text-sm font-bold text-gray-900 whitespace-nowrap">{dealer.name}</p>
                        {dealer.isVerified && (
                          <FiCheckCircle className="w-4 h-4 text-blue-500 flex-shrink-0" />
                        )}
                      </div>
                      <div className="flex items-center gap-3 text-xs text-gray-600">
                        <span className="flex items-center gap-1">
                          <FiStar className="w-3 h-3 fill-amber-400 text-amber-400" />
                          <span className="font-semibold text-gray-900">{dealer.rating}</span>
                          <span className="text-gray-500">({dealer.reviewCount})</span>
                        </span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <FiNavigation className="w-3 h-3" />
                          <span>{getDistance(dealer)}</span>
                        </span>
                      </div>
                      {/* Arrow */}
                      <div className="absolute top-full left-1/2 transform -translate-x-1/2 -mt-1">
                        <div className="border-8 border-transparent border-t-white/90"></div>
                      </div>
                    </div>
                  </div>
                  
                  {/* Dealer thumbnail with enhanced styling */}
                  <div
                    className={`relative transition-all duration-200 ${
                      isSelected 
                        ? 'w-16 h-16 ring-4 ring-blue-500 ring-offset-2' 
                        : 'w-12 h-12 ring-3 ring-white hover:w-14 hover:h-14 hover:ring-4 hover:ring-blue-400'
                    } rounded-full overflow-hidden shadow-lg hover:shadow-2xl`}
                  >
                    <img
                      src={dealer.featuredListings[0]?.image || 'https://via.placeholder.com/150'}
                      alt={dealer.name}
                      className="w-full h-full object-cover"
                    />
                    
                    {/* Badge with listing count */}
                    <div className="absolute -top-1 -right-1 bg-gradient-to-br from-emerald-500 to-emerald-600 text-white text-[10px] font-bold rounded-full w-5 h-5 flex items-center justify-center shadow-lg ring-2 ring-white">
                      {dealer.activeListings}
                    </div>
                    
                    {/* Verified badge */}
                    {dealer.isVerified && (
                      <div className="absolute -bottom-1 -right-1 bg-blue-500 rounded-full p-0.5 ring-2 ring-white shadow-md">
                        <FiCheckCircle className="w-3 h-3 text-white" />
                      </div>
                    )}
                  </div>
                </div>
              </OverlayView>
            );
          })}
        </GoogleMap>

        {/* Selected Dealer Card */}
        <AnimatePresence>
          {selectedDealerData && (
            <motion.div
              initial={{ opacity: 0, y: 50 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 50 }}
              className="absolute bottom-16 left-1/2 transform -translate-x-1/2 w-full max-w-xl px-3 z-20"
            >
              <div className="bg-white rounded-xl shadow-2xl overflow-hidden border-2 border-gray-900">
                {/* Header */}
                <div className="bg-gray-900 px-3 py-2 text-white">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="text-base font-bold">{selectedDealerData.name}</h3>
                        {selectedDealerData.isVerified && (
                          <FiCheckCircle className="w-4 h-4" />
                        )}
                      </div>
                      <div className="flex items-center gap-2 text-xs opacity-90">
                        <div className="flex items-center gap-1">
                          <FiStar className="fill-current w-3 h-3" />
                          <span>{selectedDealerData.rating}</span>
                          <span>({selectedDealerData.reviewCount})</span>
                        </div>
                        <span>•</span>
                        <span>{getDistance(selectedDealerData)}</span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <FiMapPin className="w-3 h-3" />
                          {selectedDealerData.city}
                        </span>
                      </div>
                    </div>
                    <button
                      onClick={() => setSelectedDealer(null)}
                      className="p-1 hover:bg-white/10 rounded transition-colors"
                    >
                      <FiX size={20} />
                    </button>
                  </div>
                </div>

                {/* Featured Vehicles */}
                <div className="p-3">
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="text-sm font-semibold text-gray-700">
                      {selectedDealerData.activeListings} vehículos
                    </h4>
                    {selectedDealerData.featuredListings.length > 3 && (
                      <div className="flex items-center gap-1">
                        <button
                          onClick={() => setImageStartIndex(Math.max(0, imageStartIndex - 3))}
                          disabled={imageStartIndex === 0}
                          className="p-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                          </svg>
                        </button>
                        <span className="text-xs text-gray-600">
                          {Math.floor(imageStartIndex / 3) + 1}/{Math.ceil(selectedDealerData.featuredListings.length / 3)}
                        </span>
                        <button
                          onClick={() => setImageStartIndex(Math.min(selectedDealerData.featuredListings.length - 3, imageStartIndex + 3))}
                          disabled={imageStartIndex + 3 >= selectedDealerData.featuredListings.length}
                          className="p-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-30 disabled:cursor-not-allowed"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                          </svg>
                        </button>
                      </div>
                    )}
                  </div>
                  <div className="grid grid-cols-3 gap-2 mb-3">
                    {selectedDealerData.featuredListings.slice(imageStartIndex, imageStartIndex + 3).map((listing) => (
                      <div 
                        key={listing.id} 
                        className="group cursor-pointer bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow"
                        onDoubleClick={() => navigate(`/vehicles/${listing.id}`)}
                      >
                        <div className="relative aspect-[4/3] overflow-hidden">
                          <img
                            src={listing.image}
                            alt={listing.title}
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-200"
                          />
                        </div>
                        <div className="p-1.5">
                          <p className="text-xs font-bold text-gray-900 line-clamp-1">{listing.title}</p>
                          <p className="text-[11px] font-semibold text-emerald-600">{formatPrice(listing.price)}</p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Action Buttons */}
                  <div className="grid grid-cols-4 gap-1.5">
                    <a
                      href={`tel:${selectedDealerData.phone}`}
                      className="flex flex-col items-center justify-center gap-0.5 px-1 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors"
                    >
                      <FiPhone className="w-4 h-4" />
                      <span className="text-[10px] font-medium">Llamar</span>
                    </a>
                    <a
                      href={`https://wa.me/${selectedDealerData.phone.replace(/\D/g, '')}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex flex-col items-center justify-center gap-0.5 px-1 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                    >
                      <FiMessageCircle className="w-4 h-4" />
                      <span className="text-[10px] font-medium">WhatsApp</span>
                    </a>
                    <a
                      href={`https://www.google.com/maps/dir/?api=1&destination=${selectedDealerData.latitude},${selectedDealerData.longitude}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex flex-col items-center justify-center gap-0.5 px-1 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                    >
                      <FiNavigation className="w-4 h-4" />
                      <span className="text-[10px] font-medium">Ir</span>
                    </a>
                    <div className="relative">
                      <button
                        onClick={() => setShowShareMenu(!showShareMenu)}
                        className="w-full flex flex-col items-center justify-center gap-0.5 px-1 py-2 bg-gray-700 text-white rounded-lg hover:bg-gray-800 transition-colors h-full"
                      >
                        <FiShare2 className="w-4 h-4" />
                        <span className="text-[10px] font-medium">Compartir</span>
                      </button>
                      {showShareMenu && (
                        <div className="absolute bottom-full mb-1 right-0 bg-white rounded-lg shadow-xl py-1 w-36 border border-gray-200 z-50">
                          <button
                            onClick={() => handleShare(selectedDealerData, 'copy')}
                            className="w-full flex items-center gap-2 px-2 py-1.5 hover:bg-gray-100 text-xs text-left text-gray-700"
                          >
                            <FiCopy className="w-3 h-3" />
                            Copiar
                          </button>
                          <button
                            onClick={() => handleShare(selectedDealerData, 'whatsapp')}
                            className="w-full flex items-center gap-2 px-2 py-1.5 hover:bg-gray-100 text-xs text-left text-gray-700"
                          >
                            <FiMessageCircle className="w-3 h-3" />
                            WhatsApp
                          </button>
                        </div>
                      )}
                    </div>
                  </div>

                  {selectedDealerData.activeListings > 3 && (
                    <button
                      onClick={() => navigate(`/dealers/${selectedDealerData.id}`)}
                      className="w-full mt-2 flex items-center justify-center gap-1 px-3 py-1.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 text-xs font-medium transition-colors"
                    >
                      Ver todos ({selectedDealerData.activeListings})
                      <FiExternalLink className="w-3 h-3" />
                    </button>
                  )}
                </div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </MainLayout>
  );
}
