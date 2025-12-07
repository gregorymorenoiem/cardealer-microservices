/**
 * GlobalSearch - Enhanced global search with category filters
 * Sprint 5.2: Moved from SearchSection to Navbar for space optimization
 * Searches across all verticals (vehicles, rentals, properties, lodging)
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
  FiLoader,
  FiFilter,
  FiDollarSign
} from 'react-icons/fi';
import { FaCar, FaBed, FaKey } from 'react-icons/fa';

type CategoryType = 'all' | 'vehicles' | 'vehicle-rental' | 'properties' | 'lodging';

interface SearchResult {
  id: string;
  type: 'vehicle' | 'vehicle-rental' | 'property' | 'lodging';
  title: string;
  subtitle: string;
  price: number;
  image?: string;
  url: string;
}

const searchCategories = [
  { id: 'all' as CategoryType, label: 'Todo', icon: FiSearch },
  { id: 'vehicles' as CategoryType, label: 'Vehículos', icon: FaCar },
  { id: 'vehicle-rental' as CategoryType, label: 'Renta', icon: FaKey },
  { id: 'properties' as CategoryType, label: 'Propiedades', icon: FiHome },
  { id: 'lodging' as CategoryType, label: 'Hospedaje', icon: FaBed },
];

const quickFilters = {
  vehicles: ['Toyota', 'Honda', 'BMW', 'Tesla', 'Menor $30K'],
  'vehicle-rental': ['SUV', 'Sedán', 'Deportivo', 'Por día', 'Por semana'],
  properties: ['Casa', 'Apartamento', 'Venta', 'Renta', 'Menor $500K'],
  lodging: ['Hotel', 'Cabaña', 'Villa', 'Por noche', 'Por semana'],
};

// Mock search results - in production this would come from an API
const mockSearch = async (query: string, category: CategoryType = 'all'): Promise<SearchResult[]> => {
  await new Promise(resolve => setTimeout(resolve, 300));
  
  if (!query.trim()) return [];
  
  const q = query.toLowerCase();
  
  const results: SearchResult[] = [];
  
  // Vehicle results (for sale)
  if ((category === 'all' || category === 'vehicles') && 
      (q.includes('toyota') || q.includes('auto') || q.includes('sedan') || q.includes('camioneta') || q.includes('bmw') || q.includes('tesla'))) {
    results.push(
      { id: 'v1', type: 'vehicle', title: 'Toyota Camry 2024', subtitle: 'Sedán • Automático • 5,000 km', price: 450000, url: '/vehicles/v1' },
      { id: 'v2', type: 'vehicle', title: 'Toyota RAV4 2023', subtitle: 'SUV • Híbrido • 15,000 km', price: 620000, url: '/vehicles/v2' },
    );
  }
  
  if ((category === 'all' || category === 'vehicles') && (q.includes('honda') || q.includes('civic'))) {
    results.push(
      { id: 'v3', type: 'vehicle', title: 'Honda Civic 2024', subtitle: 'Sedán • Manual • Nuevo', price: 380000, url: '/vehicles/v3' },
    );
  }
  
  if ((category === 'all' || category === 'vehicles') && (q.includes('bmw') || q.includes('tesla'))) {
    results.push(
      { id: 'v4', type: 'vehicle', title: 'BMW Serie 3 2024', subtitle: 'Sedán • Automático • 8,000 km', price: 720000, url: '/vehicles/v4' },
      { id: 'v5', type: 'vehicle', title: 'Tesla Model 3 2024', subtitle: 'Sedán • Eléctrico • Nuevo', price: 890000, url: '/vehicles/v5' },
    );
  }
  
  // Vehicle rental results
  if ((category === 'all' || category === 'vehicle-rental') && 
      (q.includes('renta') || q.includes('alquiler') || q.includes('suv') || q.includes('deportivo'))) {
    results.push(
      { id: 'vr1', type: 'vehicle-rental', title: 'BMW X5 - Renta Diaria', subtitle: 'SUV de lujo • Automático', price: 150, url: '/vehicle-rental/vr1' },
      { id: 'vr2', type: 'vehicle-rental', title: 'Porsche 911 - Renta Weekend', subtitle: 'Deportivo • Manual', price: 350, url: '/vehicle-rental/vr2' },
    );
  }
  
  // Property results
  if ((category === 'all' || category === 'properties') && 
      (q.includes('casa') || q.includes('house') || q.includes('venta') || q.includes('apartamento'))) {
    results.push(
      { id: 'p1', type: 'property', title: 'Casa en Polanco', subtitle: '4 rec • 3 baños • 350 m²', price: 12500000, url: '/properties/p1' },
      { id: 'p2', type: 'property', title: 'Casa en Condesa', subtitle: '3 rec • 2 baños • 220 m²', price: 8900000, url: '/properties/p2' },
    );
  }
  
  if ((category === 'all' || category === 'properties') && (q.includes('depa') || q.includes('apartamento') || q.includes('renta'))) {
    results.push(
      { id: 'p3', type: 'property', title: 'Departamento en Roma Norte', subtitle: '2 rec • 2 baños • 95 m²', price: 25000, url: '/properties/p3' },
      { id: 'p4', type: 'property', title: 'Penthouse en Santa Fe', subtitle: '3 rec • 3 baños • 180 m²', price: 45000, url: '/properties/p4' },
    );
  }
  
  // Lodging results
  if ((category === 'all' || category === 'lodging') && 
      (q.includes('hotel') || q.includes('hospedaje') || q.includes('cabaña') || q.includes('villa'))) {
    results.push(
      { id: 'l1', type: 'lodging', title: 'Villa en Tulum', subtitle: '4 habitaciones • Vista al mar', price: 450, url: '/lodging/l1' },
      { id: 'l2', type: 'lodging', title: 'Cabaña en Valle de Bravo', subtitle: '2 habitaciones • Bosque', price: 180, url: '/lodging/l2' },
    );
  }
  
  // If no specific matches, show some generic results based on category
  if (results.length === 0 && q.length >= 2) {
    if (category === 'all') {
      results.push(
        { id: 'v1', type: 'vehicle', title: 'Toyota Camry 2024', subtitle: 'Sedán • Automático • 5,000 km', price: 450000, url: '/vehicles/v1' },
        { id: 'p1', type: 'property', title: 'Casa en Polanco', subtitle: '4 rec • 3 baños • 350 m²', price: 12500000, url: '/properties/p1' },
      );
    }
  }
  
  return results;
};

interface GlobalSearchProps {
  className?: string;
  placeholder?: string;
  showOnMobile?: boolean;
  compact?: boolean; // For navbar vs expanded view
}

const GlobalSearch: React.FC<GlobalSearchProps> = ({ 
  className = '',
  placeholder = 'Buscar vehículos, propiedades...',
  showOnMobile = false,
  compact = true
}) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<CategoryType>('all');
  const [showFilters, setShowFilters] = useState(false);
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
        const searchResults = await mockSearch(query, selectedCategory);
        setResults(searchResults);
        setIsLoading(false);
      } else {
        setResults([]);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query, selectedCategory]);

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

  const handleQuickFilter = (filter: string) => {
    setQuery(filter);
    inputRef.current?.focus();
  };

  const vehicleResults = results.filter(r => r.type === 'vehicle');
  const rentalResults = results.filter(r => r.type === 'vehicle-rental');
  const propertyResults = results.filter(r => r.type === 'property');
  const lodgingResults = results.filter(r => r.type === 'lodging');

  return (
    <div 
      ref={containerRef}
      className={`relative ${showOnMobile ? '' : 'hidden md:block'} ${className}`}
    >
      {/* Search Input with Category Filter */}
      <div className="relative flex items-center gap-2">
        {/* Category Selector (only in expanded mode) */}
        {!compact && (
          <div className="flex gap-1 border-r pr-2">
            {searchCategories.map((cat) => {
              const Icon = cat.icon;
              return (
                <button
                  key={cat.id}
                  onClick={() => setSelectedCategory(cat.id)}
                  className={`flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                    selectedCategory === cat.id
                      ? 'bg-blue-50 text-blue-600'
                      : 'text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  <Icon className="w-4 h-4" />
                  <span className="hidden lg:inline">{cat.label}</span>
                </button>
              );
            })}
          </div>
        )}
        
        <div className="relative flex-1">
          <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onFocus={() => setIsOpen(true)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            className="w-full pl-10 pr-20 py-2.5 bg-gray-100 border border-transparent focus:border-primary focus:bg-white rounded-xl text-gray-900 placeholder-gray-500 outline-none transition-all"
          />
          <div className="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1">
            {/* Filter Toggle (compact mode) */}
            {compact && (
              <button
                onClick={() => setShowFilters(!showFilters)}
                className={`p-1.5 rounded-lg transition-colors ${
                  showFilters ? 'bg-blue-100 text-blue-600' : 'hover:bg-gray-200 text-gray-500'
                }`}
                title="Filtros"
              >
                <FiFilter className="w-4 h-4" />
              </button>
            )}
            {query && (
              <button
                onClick={clearSearch}
                className="p-1.5 hover:bg-gray-200 rounded-lg transition-colors"
              >
                <FiX className="w-4 h-4 text-gray-500" />
              </button>
            )}
            {isLoading && (
              <FiLoader className="w-5 h-5 text-gray-400 animate-spin" />
            )}
          </div>
        </div>
      </div>

      {/* Dropdown Results */}
      <AnimatePresence>
        {isOpen && (query.length >= 2 || recentSearches.length > 0 || showFilters) && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            transition={{ duration: 0.15 }}
            className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-2xl border border-gray-200 overflow-hidden z-50 max-h-[70vh] overflow-y-auto"
          >
            {/* Quick Filters (in compact mode when filters shown) */}
            {compact && showFilters && selectedCategory !== 'all' && (
              <div className="p-4 border-b border-gray-100 bg-gray-50">
                <div className="flex items-center gap-2 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                  <FiFilter className="w-3 h-3" />
                  Filtros rápidos
                </div>
                <div className="flex flex-wrap gap-2">
                  {quickFilters[selectedCategory]?.map((filter) => (
                    <button
                      key={filter}
                      onClick={() => handleQuickFilter(filter)}
                      className="px-3 py-1.5 text-sm bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:text-blue-600 transition-colors"
                    >
                      {filter}
                    </button>
                  ))}
                </div>
              </div>
            )}
            
            {/* Category Tabs (compact mode) */}
            {compact && (query.length >= 2 || showFilters) && (
              <div className="flex gap-1 p-2 border-b border-gray-100 overflow-x-auto">
                {searchCategories.map((cat) => {
                  const Icon = cat.icon;
                  return (
                    <button
                      key={cat.id}
                      onClick={() => setSelectedCategory(cat.id)}
                      className={`flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium whitespace-nowrap transition-colors ${
                        selectedCategory === cat.id
                          ? 'bg-blue-50 text-blue-600'
                          : 'text-gray-600 hover:bg-gray-100'
                      }`}
                    >
                      <Icon className="w-4 h-4" />
                      {cat.label}
                    </button>
                  );
                })}
              </div>
            )}
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
                {/* Vehicle Rentals Section */}
                {rentalResults.length > 0 && (
                  <div className="p-4 border-b border-gray-100">
                    <div className="flex items-center gap-2 text-xs font-semibold text-amber-600 uppercase tracking-wider mb-3">
                      <FaKey className="w-3 h-3" />
                      Renta de Vehículos
                    </div>
                    <div className="space-y-1">
                      {rentalResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          <div className="w-12 h-12 bg-amber-50 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FaKey className="w-5 h-5 text-amber-500" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="font-medium text-gray-900 truncate group-hover:text-amber-600">
                              {result.title}
                            </div>
                            <div className="text-sm text-gray-500 truncate">{result.subtitle}</div>
                          </div>
                          <div className="text-right flex-shrink-0">
                            <div className="font-semibold text-gray-900">
                              {formatPrice(result.price)}<span className="text-sm text-gray-500">/día</span>
                            </div>
                          </div>
                        </Link>
                      ))}
                    </div>
                  </div>
                )}

                {/* Vehicles Section */}
                {vehicleResults.length > 0 && (
                  <div className="p-4 border-b border-gray-100">
                    <div className="flex items-center gap-2 text-xs font-semibold text-blue-600 uppercase tracking-wider mb-3">
                      <FiTruck className="w-3 h-3" />
                      Vehículos en Venta
                    </div>
                    <div className="space-y-1">
                      {vehicleResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          <div className="w-12 h-12 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FiTruck className="w-5 h-5 text-blue-500" />
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
                          <div className="w-12 h-12 bg-emerald-50 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FiHome className="w-5 h-5 text-emerald-500" />
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
                
                {/* Lodging Section */}
                {lodgingResults.length > 0 && (
                  <div className="p-4 border-b border-gray-100">
                    <div className="flex items-center gap-2 text-xs font-semibold text-purple-600 uppercase tracking-wider mb-3">
                      <FaBed className="w-3 h-3" />
                      Hospedaje
                    </div>
                    <div className="space-y-1">
                      {lodgingResults.map((result) => (
                        <Link
                          key={result.id}
                          to={result.url}
                          onClick={() => handleResultClick(result)}
                          className="flex items-center gap-3 p-2 hover:bg-gray-100 rounded-lg transition-colors group"
                        >
                          <div className="w-12 h-12 bg-purple-50 rounded-lg flex items-center justify-center flex-shrink-0">
                            <FaBed className="w-5 h-5 text-purple-500" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="font-medium text-gray-900 truncate group-hover:text-purple-600">
                              {result.title}
                            </div>
                            <div className="text-sm text-gray-500 truncate">{result.subtitle}</div>
                          </div>
                          <div className="text-right flex-shrink-0">
                            <div className="font-semibold text-gray-900">
                              {formatPrice(result.price)}<span className="text-sm text-gray-500">/noche</span>
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
