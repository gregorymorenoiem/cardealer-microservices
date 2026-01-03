/**
 * GlobalSearch - Global search component with dropdown results
 * Searches across all verticals (vehicles and properties)
 */

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  FiSearch, 
  FiX, 
  FiTruck, 
  FiHome,
  FiClock,
  FiArrowRight,
  FiLoader
} from 'react-icons/fi';

interface SearchResult {
  id: string;
  type: 'vehicle' | 'property';
  title: string;
  subtitle: string;
  price: number;
  image?: string;
  url: string;
}

// Mock search results - in production this would come from an API
const mockSearch = async (query: string): Promise<SearchResult[]> => {
  await new Promise(resolve => setTimeout(resolve, 300));
  
  if (!query.trim()) return [];
  
  const q = query.toLowerCase();
  
  const results: SearchResult[] = [];
  
  // Vehicle results
  if (q.includes('toyota') || q.includes('auto') || q.includes('sedan') || q.includes('camioneta')) {
    results.push(
      { id: 'v1', type: 'vehicle', title: 'Toyota Camry 2024', subtitle: 'Sedán • Automático • 5,000 km', price: 450000, url: '/vehicles/v1' },
      { id: 'v2', type: 'vehicle', title: 'Toyota RAV4 2023', subtitle: 'SUV • Híbrido • 15,000 km', price: 620000, url: '/vehicles/v2' },
    );
  }
  
  if (q.includes('honda') || q.includes('civic')) {
    results.push(
      { id: 'v3', type: 'vehicle', title: 'Honda Civic 2024', subtitle: 'Sedán • Manual • Nuevo', price: 380000, url: '/vehicles/v3' },
    );
  }
  
  // Property results
  if (q.includes('casa') || q.includes('house') || q.includes('venta')) {
    results.push(
      { id: 'p1', type: 'property', title: 'Casa en Polanco', subtitle: '4 rec • 3 baños • 350 m²', price: 12500000, url: '/properties/p1' },
      { id: 'p2', type: 'property', title: 'Casa en Condesa', subtitle: '3 rec • 2 baños • 220 m²', price: 8900000, url: '/properties/p2' },
    );
  }
  
  if (q.includes('depa') || q.includes('apartamento') || q.includes('renta')) {
    results.push(
      { id: 'p3', type: 'property', title: 'Departamento en Roma Norte', subtitle: '2 rec • 2 baños • 95 m²', price: 25000, url: '/properties/p3' },
      { id: 'p4', type: 'property', title: 'Penthouse en Santa Fe', subtitle: '3 rec • 3 baños • 180 m²', price: 45000, url: '/properties/p4' },
    );
  }
  
  // If no specific matches, show some generic results
  if (results.length === 0 && q.length >= 2) {
    results.push(
      { id: 'v1', type: 'vehicle', title: 'Toyota Camry 2024', subtitle: 'Sedán • Automático • 5,000 km', price: 450000, url: '/vehicles/v1' },
      { id: 'p1', type: 'property', title: 'Casa en Polanco', subtitle: '4 rec • 3 baños • 350 m²', price: 12500000, url: '/properties/p1' },
    );
  }
  
  return results;
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

  // Search debounce
  useEffect(() => {
    const timer = setTimeout(async () => {
      if (query.length >= 2) {
        setIsLoading(true);
        const searchResults = await mockSearch(query);
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
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
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

  const vehicleResults = results.filter(r => r.type === 'vehicle');
  const propertyResults = results.filter(r => r.type === 'property');

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
                      Vehículos
                    </div>
                    <div className="space-y-1">
                      {vehicleResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          <div className="w-12 h-12 bg-gray-200 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FiTruck className="w-6 h-6 text-gray-400" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="font-medium text-gray-900 truncate group-hover:text-blue-600">
                              {result.title}
                            </div>
                            <div className="text-sm text-gray-500 truncate">{result.subtitle}</div>
                          </div>
                          <div className="text-right flex-shrink-0">
                            <div className="font-semibold text-gray-900">
                              {formatPrice(result.price)}
                            </div>
                          </div>
                        </Link>
                      ))}
                    </div>
                  </div>
                )}

                {/* Properties Section */}
                {propertyResults.length > 0 && (
                  <div className="p-4 border-b border-gray-100">
                    <div className="flex items-center gap-2 text-xs font-semibold text-emerald-600 uppercase tracking-wider mb-3">
                      <FiHome className="w-3 h-3" />
                      Inmuebles
                    </div>
                    <div className="space-y-1">
                      {propertyResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          <div className="w-12 h-12 bg-gray-200 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FiHome className="w-6 h-6 text-gray-400" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="font-medium text-gray-900 truncate group-hover:text-emerald-600">
                              {result.title}
                            </div>
                            <div className="text-sm text-gray-500 truncate">{result.subtitle}</div>
                          </div>
                          <div className="text-right flex-shrink-0">
                            <div className="font-semibold text-gray-900">
                              {formatPrice(result.price)}
                              {result.price < 100000 && <span className="text-sm text-gray-500">/mes</span>}
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
                    <p className="text-gray-500">No se encontraron resultados para "{query}"</p>
                    <p className="text-sm text-gray-400 mt-1">Intenta con otro término de búsqueda</p>
                  </div>
                )}

                {/* View All Results */}
                {results.length > 0 && (
                  <Link
                    to={`/search?q=${encodeURIComponent(query)}`}
                    onClick={() => {
                      saveRecentSearch(query);
                      setIsOpen(false);
                    }}
                    className="flex items-center justify-center gap-2 p-4 text-primary font-medium hover:bg-gray-50 transition-colors"
                  >
                    Ver todos los resultados
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
