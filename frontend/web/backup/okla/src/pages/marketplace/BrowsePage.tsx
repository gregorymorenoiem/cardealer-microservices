/**
 * MarketplaceBrowsePage - Unified browse page for all marketplace verticals
 * Supports filtering by vertical, price, location, and specific attributes
 */

import React, { useState, useMemo } from 'react';
import { useSearchParams, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { 
  CategorySelector, 
  SearchBar, 
  SearchFilters, 
  ListingGrid 
} from '@/components/marketplace';
import { 
  useCategories, 
  useListings,
} from '@/hooks/useMarketplace';
import type { 
  MarketplaceVertical,
  MarketplaceCategorySelection, 
  MarketplaceSearchParams 
} from '@/types/marketplace';

const MarketplaceBrowsePage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const location = useLocation();
  const [filtersOpen, setFiltersOpen] = useState(false);
  
  // Detect vertical from route path first, then fall back to URL params
  const getVerticalFromPath = (): MarketplaceCategorySelection => {
    if (location.pathname.startsWith('/vehicles')) return 'vehicles';
    if (location.pathname.startsWith('/properties')) return 'real-estate';
    return 'all';
  };
  
  // Parse URL params - use the specific category selection type
  const verticalParam = searchParams.get('vertical');
  const pathVertical = getVerticalFromPath();
  
  // Path takes precedence, then URL param
  const selectedCategory: MarketplaceCategorySelection = 
    pathVertical !== 'all' 
      ? pathVertical
      : verticalParam === 'vehicles' || verticalParam === 'real-estate' 
        ? verticalParam 
        : 'all';
      
  const query = searchParams.get('q') || '';
  const page = Number(searchParams.get('page')) || 1;
  const isFeatured = searchParams.get('featured') === 'true';

  // Get the actual vertical for filters (undefined if 'all')
  const currentVertical: MarketplaceVertical | undefined = 
    selectedCategory !== 'all' ? selectedCategory : undefined;

  // Build search params
  const searchParamsObj = useMemo<MarketplaceSearchParams>(() => {
    const params: MarketplaceSearchParams = {
      page,
      pageSize: 12,
    };

    if (currentVertical) {
      params.vertical = currentVertical;
    }
    if (query) {
      params.query = query;
    }
    if (isFeatured) {
      params.isFeatured = true;
    }

    // Parse other filters from URL
    const minPrice = searchParams.get('minPrice');
    const maxPrice = searchParams.get('maxPrice');
    const city = searchParams.get('city');

    if (minPrice) params.minPrice = Number(minPrice);
    if (maxPrice) params.maxPrice = Number(maxPrice);
    if (city) params.city = city;

    return params;
  }, [currentVertical, query, page, isFeatured, searchParams]);

  // Data hooks
  const { data: categories = [] } = useCategories();
  const { data: listingsData, isLoading } = useListings(searchParamsObj);

  const listings = listingsData?.listings || [];
  const totalCount = listingsData?.totalCount || 0;
  const totalPages = listingsData?.totalPages || 1;

  // Handlers
  const handleCategoryChange = (category: MarketplaceCategorySelection) => {
    const newParams = new URLSearchParams(searchParams);
    if (category === 'all') {
      newParams.delete('vertical');
    } else {
      newParams.set('vertical', category);
    }
    newParams.delete('page'); // Reset page on category change
    setSearchParams(newParams);
  };

  const handleSearch = (searchQuery: string, vertical: MarketplaceCategorySelection) => {
    const newParams = new URLSearchParams(searchParams);
    if (searchQuery) {
      newParams.set('q', searchQuery);
    } else {
      newParams.delete('q');
    }
    if (vertical !== 'all') {
      newParams.set('vertical', vertical);
    } else {
      newParams.delete('vertical');
    }
    newParams.delete('page');
    setSearchParams(newParams);
  };

  const handleFiltersChange = (filters: Record<string, string | number | boolean>) => {
    const newParams = new URLSearchParams();
    
    // Preserve existing params
    if (currentVertical) newParams.set('vertical', currentVertical);
    if (query) newParams.set('q', query);
    
    // Add new filters
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== '' && value !== false) {
        newParams.set(key, String(value));
      }
    });

    setSearchParams(newParams);
    setFiltersOpen(false);
  };

  const handlePageChange = (newPage: number) => {
    const newParams = new URLSearchParams(searchParams);
    newParams.set('page', String(newPage));
    setSearchParams(newParams);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const clearFilters = () => {
    setSearchParams({});
  };

  // Count active filters
  const activeFiltersCount = useMemo(() => {
    let count = 0;
    searchParams.forEach((value, key) => {
      if (!['page', 'vertical'].includes(key) && value) {
        count++;
      }
    });
    return count;
  }, [searchParams]);

  // For SearchFilters, we need an actual vertical, default to vehicles
  const filterVertical: MarketplaceVertical = currentVertical || 'vehicles';

  return (
    <MainLayout>
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          {/* Search and category row */}
          <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center">
            <div className="flex-1 w-full lg:max-w-xl">
              <SearchBar
                vertical={selectedCategory}
                onSearch={handleSearch}
                placeholder="Buscar..."
                showVerticalSelector={false}
                size="sm"
              />
            </div>

            <CategorySelector
              categories={categories}
              selectedCategory={selectedCategory}
              onCategoryChange={handleCategoryChange}
              variant="minimal"
            />
          </div>
        </div>
      </div>

      {/* Main content area with light gray background like /browse */}
      <div className="bg-gray-50 min-h-screen">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex flex-col lg:flex-row gap-8">
            {/* Sidebar Filters (Desktop) */}
          <aside className="hidden lg:block w-72 flex-shrink-0">
            <div className="sticky top-32">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-gray-900">Filtros</h3>
                {activeFiltersCount > 0 && (
                  <button
                    onClick={clearFilters}
                    className="text-sm text-blue-600 hover:text-blue-700"
                  >
                    Limpiar ({activeFiltersCount})
                  </button>
                )}
              </div>
              <SearchFilters
                vertical={filterVertical}
                onSearch={handleFiltersChange}
              />
            </div>
          </aside>

          {/* Main Content */}
          <main className="flex-1 min-w-0">
            {/* Results header */}
            <div className="flex items-center justify-between mb-6">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  {isFeatured ? 'Destacados' : 
                   selectedCategory === 'vehicles' ? 'Vehículos' :
                   selectedCategory === 'real-estate' ? 'Propiedades' :
                   'Todos los listados'}
                </h1>
                <p className="text-gray-600 mt-1">
                  {totalCount.toLocaleString()} {totalCount === 1 ? 'resultado' : 'resultados'}
                  {query && ` para "${query}"`}
                </p>
              </div>

              {/* Mobile filter button */}
              <button
                onClick={() => setFiltersOpen(!filtersOpen)}
                className="lg:hidden flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-lg hover:bg-gray-50"
              >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
                </svg>
                Filtros
                {activeFiltersCount > 0 && (
                  <span className="bg-blue-600 text-white text-xs px-2 py-0.5 rounded-full">
                    {activeFiltersCount}
                  </span>
                )}
              </button>
            </div>

            {/* Mobile filters drawer - shown when filtersOpen */}
            {filtersOpen && (
              <div className="lg:hidden mb-6 bg-white rounded-xl shadow-lg p-4 border border-gray-200">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="font-semibold">Filtros</h3>
                  <button
                    onClick={() => setFiltersOpen(false)}
                    className="text-gray-500 hover:text-gray-700"
                  >
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <SearchFilters
                  vertical={filterVertical}
                  onSearch={handleFiltersChange}
                />
              </div>
            )}

            {/* Active filters chips */}
            {activeFiltersCount > 0 && (
              <div className="flex flex-wrap gap-2 mb-6">
                {query && (
                  <FilterChip
                    label={`Búsqueda: ${query}`}
                    onRemove={() => {
                      const newParams = new URLSearchParams(searchParams);
                      newParams.delete('q');
                      setSearchParams(newParams);
                    }}
                  />
                )}
                {searchParams.get('minPrice') && (
                  <FilterChip
                    label={`Precio mín: $${searchParams.get('minPrice')}`}
                    onRemove={() => {
                      const newParams = new URLSearchParams(searchParams);
                      newParams.delete('minPrice');
                      setSearchParams(newParams);
                    }}
                  />
                )}
                {searchParams.get('maxPrice') && (
                  <FilterChip
                    label={`Precio máx: $${searchParams.get('maxPrice')}`}
                    onRemove={() => {
                      const newParams = new URLSearchParams(searchParams);
                      newParams.delete('maxPrice');
                      setSearchParams(newParams);
                    }}
                  />
                )}
                {searchParams.get('city') && (
                  <FilterChip
                    label={`Ciudad: ${searchParams.get('city')}`}
                    onRemove={() => {
                      const newParams = new URLSearchParams(searchParams);
                      newParams.delete('city');
                      setSearchParams(newParams);
                    }}
                  />
                )}
              </div>
            )}

            {/* Listings Grid */}
            <ListingGrid
              listings={listings}
              isLoading={isLoading}
              columns={3}
              emptyMessage="No se encontraron resultados. Intenta ajustar los filtros."
            />

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="mt-12 flex justify-center">
                <Pagination
                  currentPage={page}
                  totalPages={totalPages}
                  onPageChange={handlePageChange}
                />
              </div>
            )}
          </main>
        </div>
      </div>
      </div>
    </MainLayout>
  );
};

// Filter chip component
const FilterChip: React.FC<{ label: string; onRemove: () => void }> = ({ label, onRemove }) => (
  <motion.button
    initial={{ opacity: 0, scale: 0.9 }}
    animate={{ opacity: 1, scale: 1 }}
    exit={{ opacity: 0, scale: 0.9 }}
    onClick={onRemove}
    className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-blue-50 text-blue-700 text-sm rounded-full hover:bg-blue-100 transition-colors"
  >
    {label}
    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
    </svg>
  </motion.button>
);

// Pagination component
const Pagination: React.FC<{
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}> = ({ currentPage, totalPages, onPageChange }) => {
  const pages = useMemo(() => {
    const result: (number | 'ellipsis')[] = [];
    
    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) result.push(i);
    } else {
      result.push(1);
      
      if (currentPage > 3) result.push('ellipsis');
      
      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);
      
      for (let i = start; i <= end; i++) result.push(i);
      
      if (currentPage < totalPages - 2) result.push('ellipsis');
      
      result.push(totalPages);
    }
    
    return result;
  }, [currentPage, totalPages]);

  return (
    <nav className="flex items-center gap-1">
      <button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
      </button>

      {pages.map((pageNum, i) => (
        pageNum === 'ellipsis' ? (
          <span key={`ellipsis-${i}`} className="px-3 py-2 text-gray-400">...</span>
        ) : (
          <button
            key={pageNum}
            onClick={() => onPageChange(pageNum)}
            className={`w-10 h-10 rounded-lg font-medium transition-colors ${
              pageNum === currentPage
                ? 'bg-blue-600 text-white'
                : 'hover:bg-gray-100 text-gray-700'
            }`}
          >
            {pageNum}
          </button>
        )
      ))}

      <button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="p-2 rounded-lg hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </button>
    </nav>
  );
};

export default MarketplaceBrowsePage;
