import { useState, useCallback, useMemo } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import MainLayout from '@/layouts/MainLayout';
import VehicleCard from '@/components/organisms/VehicleCard';
import VehicleCardSkeleton from '@/components/organisms/VehicleCardSkeleton';
import EmptyState from '@/components/organisms/EmptyState';
import AdvancedFilters, { type VehicleFilters, type SortOption } from '@/components/organisms/AdvancedFilters';
import Pagination from '@/components/molecules/Pagination';
import SaveSearchModal, { type SaveSearchData } from '@/components/organisms/SaveSearchModal';
import { type VehicleFilters as ServiceFilters } from '@/services/vehicleService';
import { mockVehicles, filterVehicles, sortVehicles } from '@/data/mockVehicles';
import { useCompare } from '@/hooks/useCompare';
import { useSearchPage, useAddRecentSearch, useCreateSavedSearch } from '@/hooks/useSearch';
import { FiGrid, FiList, FiBarChart2, FiMap, FiWifi, FiWifiOff, FiBookmark, FiSave } from 'react-icons/fi';

const ITEMS_PER_PAGE = 12;

export default function BrowsePage() {
  const { t } = useTranslation('vehicles');
  const [searchParams, setSearchParams] = useSearchParams();
  const { count: compareCount } = useCompare();
  
  // Initialize filters from URL params
  const getInitialFilters = (): VehicleFilters => {
    const urlFilters: VehicleFilters = {};
    const make = searchParams.get('make');
    const model = searchParams.get('model');
    const minYear = searchParams.get('minYear');
    const maxYear = searchParams.get('maxYear');
    const minPrice = searchParams.get('minPrice');
    const maxPrice = searchParams.get('maxPrice');

    if (make) urlFilters.make = make;
    if (model) urlFilters.model = model;
    if (minYear) urlFilters.minYear = Number(minYear);
    if (maxYear) urlFilters.maxYear = Number(maxYear);
    if (minPrice) urlFilters.minPrice = Number(minPrice);
    if (maxPrice) urlFilters.maxPrice = Number(maxPrice);

    return urlFilters;
  };

  const [filters, setFilters] = useState<VehicleFilters>(getInitialFilters);
  const [sortBy, setSortBy] = useState<SortOption>((searchParams.get('sort') as SortOption) || 'year-desc');
  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [showSaveModal, setShowSaveModal] = useState(false);

  // Convert UI filters to service filters format
  const serviceFilters: ServiceFilters = useMemo(() => ({
    search: filters.make || filters.model ? `${filters.make || ''} ${filters.model || ''}`.trim() : undefined,
    minPrice: filters.minPrice,
    maxPrice: filters.maxPrice,
  }), [filters]);

  // Use the composite search hook
  const searchPage = useSearchPage(serviceFilters, currentPage, ITEMS_PER_PAGE);
  const addRecentSearch = useAddRecentSearch();
  const createSavedSearch = useCreateSavedSearch();

  // Handler for saving search
  const handleSaveSearch = useCallback((data: SaveSearchData) => {
    createSavedSearch.mutate({
      name: data.name,
      filters: data.filters as Record<string, unknown>,
      notificationsEnabled: data.notificationsEnabled,
    }, {
      onSuccess: () => {
        setShowSaveModal(false);
      },
    });
  }, [createSavedSearch]);

  // Use API data if available, fallback to mock data only on error
  const useApiData = !searchPage.isError;
  const isUsingMockData = !useApiData;
  
  // Process data - API first (if sufficient data), then mock as fallback
  const processedData = useMemo(() => {
    if (useApiData) {
      // Using real API data - sort on client side since backend may not support sort
      const sorted = sortVehicles(searchPage.vehicles as unknown as typeof mockVehicles, sortBy);
      return {
        vehicles: sorted,
        totalItems: searchPage.total,
        totalPages: searchPage.totalPages,
        isLoading: searchPage.isLoading,
        isError: false,
      };
    }
    
    // Fallback to mock data
    const filteredVehicles = filterVehicles(mockVehicles, filters);
    const sortedVehicles = sortVehicles(filteredVehicles, sortBy);
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
    const endIndex = startIndex + ITEMS_PER_PAGE;
    const paginatedVehicles = sortedVehicles.slice(startIndex, endIndex);
    
    return {
      vehicles: paginatedVehicles,
      totalItems: sortedVehicles.length,
      totalPages: Math.ceil(sortedVehicles.length / ITEMS_PER_PAGE),
      isLoading: false,
      isError: false,
    };
  }, [useApiData, searchPage, filters, sortBy, currentPage]);

  const { vehicles: paginatedVehicles, totalItems, totalPages, isLoading, isError } = processedData;

  // Log API status for debugging and track search
  if (searchPage.isError) {
    console.warn('ProductService API error, using mock data');
  }

  // Update URL params when filters change
  const updateURLParams = useCallback((filters: VehicleFilters, sort: SortOption, page: number) => {
    const params = new URLSearchParams();
    if (filters.make) params.set('make', filters.make);
    if (filters.model) params.set('model', filters.model);
    if (filters.minYear) params.set('minYear', filters.minYear.toString());
    if (filters.maxYear) params.set('maxYear', filters.maxYear.toString());
    if (filters.minPrice) params.set('minPrice', filters.minPrice.toString());
    if (filters.maxPrice) params.set('maxPrice', filters.maxPrice.toString());
    if (filters.minMileage) params.set('minMileage', filters.minMileage.toString());
    if (filters.maxMileage) params.set('maxMileage', filters.maxMileage.toString());
    if (filters.transmission) params.set('transmission', filters.transmission);
    if (filters.fuelType) params.set('fuelType', filters.fuelType);
    if (filters.bodyType) params.set('bodyType', filters.bodyType);
    if (filters.condition) params.set('condition', filters.condition);
    params.set('sort', sort);
    if (page > 1) params.set('page', page.toString());
    setSearchParams(params);
  }, [setSearchParams]);

  const handleFilterChange = useCallback((newFilters: VehicleFilters) => {
    setFilters(newFilters);
    setCurrentPage(1);
    updateURLParams(newFilters, sortBy, 1);
    
    // Track search in recent searches
    const searchQuery = newFilters.make || newFilters.model 
      ? `${newFilters.make || ''} ${newFilters.model || ''}`.trim() 
      : null;
    if (searchQuery) {
      addRecentSearch.mutate({
        query: searchQuery,
        filters: newFilters as Record<string, unknown>,
      });
    }
  }, [sortBy, updateURLParams, addRecentSearch]);

  const handleSortChange = useCallback((newSort: SortOption) => {
    setSortBy(newSort);
    setCurrentPage(1);
    updateURLParams(filters, newSort, 1);
  }, [filters, updateURLParams]);

  const handlePageChange = useCallback((page: number) => {
    setCurrentPage(page);
    updateURLParams(filters, sortBy, page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }, [filters, sortBy, updateURLParams]);

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
              {t('browse.title')}
            </h1>
            <p className="text-gray-600">
              {t('browse.subtitle')}
            </p>
          </div>

          <div className="flex flex-col lg:flex-row gap-6">
            {/* Filters Sidebar */}
            <aside className="lg:w-72 flex-shrink-0">
              <AdvancedFilters
                onFilterChange={handleFilterChange}
                onSortChange={handleSortChange}
                currentFilters={filters}
                currentSort={sortBy}
              />
            </aside>

            {/* Main Content */}
            <main className="flex-1">
              {/* Results Header */}
              <div className="bg-white rounded-xl shadow-card p-4 sm:p-6 mb-6">
                <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                  <div>
                    <div className="flex items-center gap-2">
                      <p className="text-lg font-semibold text-gray-900">
                        {t('browse.vehiclesFound', { count: totalItems })}
                      </p>
                      {/* Data source indicator */}
                      <span 
                        className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
                          isUsingMockData 
                            ? 'bg-amber-100 text-amber-700' 
                            : 'bg-green-100 text-green-700'
                        }`}
                        title={isUsingMockData ? 'Using demo data' : 'Live data from ProductService'}
                      >
                        {isUsingMockData ? <FiWifiOff size={12} /> : <FiWifi size={12} />}
                        {isUsingMockData ? 'Demo' : 'Live'}
                      </span>
                    </div>
                    {Object.keys(filters).some((key) => filters[key as keyof VehicleFilters]) && (
                      <p className="text-sm text-gray-600 mt-1">
                        {t('browse.filteredResults')}
                      </p>
                    )}
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-3">
                    {/* Saved Searches Button */}
                    <Link
                      to="/saved-searches"
                      className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors bg-gray-100 text-gray-700 hover:bg-gray-200"
                      title={`${searchPage.savedSearches.length} búsquedas guardadas`}
                    >
                      <FiBookmark size={18} />
                      <span className="hidden sm:inline">Guardadas</span>
                      {searchPage.savedSearches.length > 0 && (
                        <span className="ml-1 px-2 py-0.5 bg-primary/10 text-primary rounded-full text-sm">
                          {searchPage.savedSearches.length}
                        </span>
                      )}
                    </Link>

                    {/* Compare Button */}
                    <Link
                      to="/vehicles/compare"
                      className={`
                        flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors
                        ${compareCount > 0 
                          ? 'bg-blue-600 text-white hover:bg-blue-700' 
                          : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                        }
                      `}
                    >
                      <FiBarChart2 size={18} />
                      <span className="hidden sm:inline">Comparar</span>
                      {compareCount > 0 && (
                        <span className="ml-1 px-2 py-0.5 bg-white/20 rounded-full text-sm">
                          {compareCount}
                        </span>
                      )}
                    </Link>

                    {/* Map Button */}
                    <Link
                      to="/vehicles/map"
                      className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors bg-emerald-600 text-white hover:bg-emerald-700"
                    >
                      <FiMap size={18} />
                      <span className="hidden sm:inline">Mapa</span>
                    </Link>

                    {/* View Mode Toggle */}
                    <div className="flex items-center gap-2 bg-gray-100 rounded-lg p-1">
                    <button
                      onClick={() => setViewMode('grid')}
                      className={`
                        p-2 rounded-md transition-colors duration-200
                        ${viewMode === 'grid' ? 'bg-white shadow-sm text-primary' : 'text-gray-600 hover:text-gray-900'}
                      `}
                      aria-label={t('browse.gridView')}
                    >
                      <FiGrid size={20} />
                    </button>
                    <button
                      onClick={() => setViewMode('list')}
                      className={`
                        p-2 rounded-md transition-colors duration-200
                        ${viewMode === 'list' ? 'bg-white shadow-sm text-primary' : 'text-gray-600 hover:text-gray-900'}
                      `}
                      aria-label={t('browse.listView')}
                    >
                      <FiList size={20} />
                    </button>
                    </div>
                  </div>
                </div>
              </div>

              {/* Vehicle Grid/List */}
              {isLoading ? (
                <div
                  className={`
                    ${viewMode === 'grid'
                      ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5'
                      : 'flex flex-col gap-5'
                    }
                    mb-8
                  `}
                >
                  {Array.from({ length: 9 }).map((_, index) => (
                    <VehicleCardSkeleton key={index} />
                  ))}
                </div>
              ) : isError ? (
                <EmptyState
                  type="error"
                  onAction={() => window.location.reload()}
                />
              ) : paginatedVehicles.length > 0 ? (
                <>
                  <div
                    className={`
                      ${viewMode === 'grid'
                        ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5'
                        : 'flex flex-col gap-5'
                      }
                      mb-8
                    `}
                  >
                    {paginatedVehicles.map((vehicle) => (
                      <VehicleCard
                        key={vehicle.id}
                        id={vehicle.id}
                        make={vehicle.make}
                        model={vehicle.model}
                        year={vehicle.year}
                        price={vehicle.price}
                        mileage={vehicle.mileage}
                        location={vehicle.location}
                        imageUrl={vehicle.images[0]}
                        isFeatured={vehicle.isFeatured}
                        isNew={vehicle.isNew}
                        transmission={vehicle.transmission}
                        fuelType={vehicle.fuelType}
                      />
                    ))}
                  </div>

                  {/* Pagination */}
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalItems={totalItems}
                    itemsPerPage={ITEMS_PER_PAGE}
                    onPageChange={handlePageChange}
                  />
                </>
              ) : (
                <EmptyState
                  type="no-results"
                  onAction={() => handleFilterChange({})}
                />
              )}
            </main>
          </div>
        </div>
      </div>

      {/* Save Search Modal */}
      <SaveSearchModal
        isOpen={showSaveModal}
        onClose={() => setShowSaveModal(false)}
        onSave={handleSaveSearch}
        filters={filters}
        isSaving={createSavedSearch.isPending}
      />
    </MainLayout>
  );
}

