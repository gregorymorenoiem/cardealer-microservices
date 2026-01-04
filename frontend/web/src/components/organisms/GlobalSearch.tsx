/**
 * GlobalSearch - Global search component with dropdown results
 * Searches vehicles from the VehiclesSaleService API
 */

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  FiSearch, 
  FiX, 
  FiTruck, 
  FiClock,
  FiArrowRight,
  FiLoader
} from 'react-icons/fi';
import { generateVehicleUrl } from '@/utils/seoSlug';

interface SearchResult {
  id: string;
  type: 'vehicle' | 'property';
  title: string;
  subtitle: string;
  price: number;
  image?: string;
  url: string;
  year?: number;
  make?: string;
  model?: string;
}

// API URL for VehiclesSaleService
const VEHICLES_API_URL = import.meta.env.VITE_VEHICLES_SALE_SERVICE_URL || 'http://localhost:15070/api';

// Real search using VehiclesSaleService API
const searchVehicles = async (query: string): Promise<SearchResult[]> => {
  if (!query.trim() || query.length < 2) return [];
  
  try {
    const response = await fetch(
      `${VEHICLES_API_URL}/Vehicles?search=${encodeURIComponent(query)}&pageSize=8&page=1`
    );
    
    if (!response.ok) {
      console.error('Search API error:', response.status);
      return [];
    }
    
    const data = await response.json();
    
    if (!data.vehicles || data.vehicles.length === 0) {
      return [];
    }
    
    // Transform API response to SearchResult format
    return data.vehicles.map((vehicle: any) => {
      // Map bodyStyle enum to readable text
      const bodyStyleMap: Record<number, string> = {
        0: 'Sedán',
        1: 'Coupé',
        2: 'Hatchback',
        3: 'Wagon',
        4: 'SUV',
        5: 'Crossover',
        6: 'Pickup',
        7: 'Van',
        8: 'Minivan',
        9: 'Convertible',
        10: 'Deportivo'
      };
      
      // Map transmission enum
      const transmissionMap: Record<number, string> = {
        0: 'Automático',
        1: 'Manual',
        2: 'CVT'
      };
      
      const bodyStyle = bodyStyleMap[vehicle.bodyStyle] || '';
      const transmission = transmissionMap[vehicle.transmission] || '';
      const mileageText = vehicle.mileage ? `${vehicle.mileage.toLocaleString()} km` : 'Nuevo';
      
      // Build subtitle with available info
      const subtitleParts = [bodyStyle, transmission, mileageText].filter(Boolean);
      
      // Get primary image
      const primaryImage = vehicle.images?.find((img: any) => img.isPrimary)?.url 
        || vehicle.images?.[0]?.url 
        || '/placeholder-car.jpg';
      
      // Generate SEO-friendly URL
      const vehicleUrl = generateVehicleUrl({
        id: vehicle.id,
        year: vehicle.year,
        make: vehicle.make,
        model: vehicle.model
      });
      
      return {
        id: vehicle.id,
        type: 'vehicle' as const,
        title: vehicle.title || `${vehicle.make} ${vehicle.model} ${vehicle.year}`,
        subtitle: subtitleParts.join(' • '),
        price: vehicle.price,
        image: primaryImage,
        url: vehicleUrl,
        year: vehicle.year,
        make: vehicle.make,
        model: vehicle.model
      };
    });
  } catch (error) {
    console.error('Error searching vehicles:', error);
    return [];
  }
};

interface GlobalSearchProps {
  className?: string;
  placeholder?: string;
  showOnMobile?: boolean;
}

const GlobalSearch: React.FC<GlobalSearchProps> = ({ 
  className = '',
  placeholder = 'Buscar vehículos, propiedades...',
  showOnMobile = false
}) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  // Load recent searches from localStorage
  useEffect(() => {
    const saved = localStorage.getItem('recentSearches');
    if (saved) {
      try {
        setRecentSearches(JSON.parse(saved).slice(0, 5));
      } catch {
        // Ignore parse errors
      }
    }
  }, []);

  // Save recent search
  const saveRecentSearch = useCallback((searchQuery: string) => {
    if (!searchQuery.trim()) return;
    
    const updated = [searchQuery, ...recentSearches.filter(s => s !== searchQuery)].slice(0, 5);
    setRecentSearches(updated);
    localStorage.setItem('recentSearches', JSON.stringify(updated));
  }, [recentSearches]);

  // Search debounce - calls VehiclesSaleService API
  useEffect(() => {
    const timer = setTimeout(async () => {
      if (query.length >= 2) {
        setIsLoading(true);
        const searchResults = await searchVehicles(query);
        setResults(searchResults);
        setIsLoading(false);
      } else {
        setResults([]);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query]);

  // Click outside to close
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Handle keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      setIsOpen(false);
      inputRef.current?.blur();
    }
    if (e.key === 'Enter' && query.trim()) {
      saveRecentSearch(query);
      navigate(`/search?q=${encodeURIComponent(query)}`);
      setIsOpen(false);
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);
  };

  const handleResultClick = (_result: SearchResult) => {
    saveRecentSearch(query);
    setIsOpen(false);
    setQuery('');
  };

  const clearSearch = () => {
    setQuery('');
    setResults([]);
    inputRef.current?.focus();
  };

  // Only vehicle results (no properties in this vehicle-only app)
  const vehicleResults = results.filter(r => r.type === 'vehicle');

  return (
    <div 
      ref={containerRef}
      className={`relative ${showOnMobile ? '' : 'hidden md:block'} ${className}`}
    >
      {/* Search Input */}
      <div className="relative">
        <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setIsOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="w-full pl-10 pr-10 py-2.5 bg-gray-100 border border-transparent focus:border-primary focus:bg-white rounded-xl text-gray-900 placeholder-gray-500 outline-none transition-all"
        />
        {query && (
          <button
            onClick={clearSearch}
            className="absolute right-3 top-1/2 -translate-y-1/2 p-1 hover:bg-gray-200 rounded-full transition-colors"
          >
            <FiX className="w-4 h-4 text-gray-500" />
          </button>
        )}
        {isLoading && (
          <FiLoader className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400 animate-spin" />
        )}
      </div>

      {/* Dropdown Results */}
      <AnimatePresence>
        {isOpen && (query.length >= 2 || recentSearches.length > 0) && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.15 }}
            className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden z-50 max-h-[70vh] overflow-y-auto"
          >
            {/* Recent Searches (when no query) */}
            {query.length < 2 && recentSearches.length > 0 && (
              <div className="p-4">
                <div className="flex items-center gap-2 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                  <FiClock className="w-3 h-3" />
                  Búsquedas recientes
                </div>
                <div className="space-y-1">
                  {recentSearches.map((search, index) => (
                    <button
                      key={index}
                      onClick={() => {
                        setQuery(search);
                        inputRef.current?.focus();
                      }}
                      className="w-full flex items-center gap-3 px-3 py-2 text-left text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
                    >
                      <FiSearch className="w-4 h-4 text-gray-400" />
                      {search}
                    </button>
                  ))}
                </div>
              </div>
            )}

            {/* Search Results */}
            {query.length >= 2 && (
              <>
                {/* Vehicles Section */}
                {vehicleResults.length > 0 && (
                  <div className="p-4 border-b border-gray-100">
                    <div className="flex items-center gap-2 text-xs font-semibold text-blue-600 uppercase tracking-wider mb-3">
                      <FiTruck className="w-3 h-3" />
                      Vehículos ({vehicleResults.length} resultados)
                    </div>
                    <div className="space-y-1">
                      {vehicleResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          {result.image && result.image !== '/placeholder-car.jpg' ? (
                            <img 
                              src={result.image} 
                              alt={result.title}
                              className="w-12 h-12 object-cover rounded-lg flex-shrink-0"
                              onError={(e) => {
                                (e.target as HTMLImageElement).src = '/placeholder-car.jpg';
                              }}
                            />
                          ) : (
                            <div className="w-12 h-12 bg-gray-200 rounded-lg flex items-center justify-center flex-shrink-0">
                              <FiTruck className="w-6 h-6 text-gray-400" />
                            </div>
                          )}
                          <div className="flex-1 min-w-0">
                            <div className="font-medium text-gray-900 truncate group-hover:text-blue-600">
                              {result.title}
                            </div>
                            <div className="text-sm text-gray-500 truncate">{result.subtitle}</div>
                          </div>
                          <div className="text-right flex-shrink-0">
                            <div className="font-semibold text-blue-600">
                              {formatPrice(result.price)}
                            </div>
                          </div>
                        </Link>
                      ))}
                    </div>
                  </div>
                )}

                {/* No Results */}
                {results.length === 0 && !isLoading && (
                  <div className="p-8 text-center">
                    <FiSearch className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                    <p className="text-gray-500">No se encontraron vehículos para "{query}"</p>
                    <p className="text-sm text-gray-400 mt-1">Intenta con otra marca, modelo o palabra clave</p>
                  </div>
                )}

                {/* View All Results */}
                {results.length > 0 && (
                  <Link
                    to={`/browse?search=${encodeURIComponent(query)}`}
                    onClick={() => {
                      saveRecentSearch(query);
                      setIsOpen(false);
                    }}
                    className="flex items-center justify-center gap-2 p-4 text-primary font-medium hover:bg-gray-50 transition-colors"
                  >
                    Ver todos los vehículos
                    <FiArrowRight className="w-4 h-4" />
                  </Link>
                )}
              </>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default GlobalSearch;