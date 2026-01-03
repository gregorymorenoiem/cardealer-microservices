import { useState, useCallback, useMemo } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import Pagination from '@/components/molecules/Pagination';
import { mockProperties } from '@/data/mockProperties';
import { FiGrid, FiList, FiMapPin, FiHome, FiMaximize, FiChevronDown, FiX, FiSliders, FiBarChart2, FiMap } from 'react-icons/fi';
import { IoBedOutline } from 'react-icons/io5';
import { LuBath } from 'react-icons/lu';
import { BiCar } from 'react-icons/bi';
import type { PropertyListing } from '@/types/marketplace';

const ITEMS_PER_PAGE = 12;

// Property Filters Interface
export interface PropertyFilters {
  propertyType?: string;
  listingType?: string;
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
  maxBedrooms?: number;
  minBathrooms?: number;
  minArea?: number;
  maxArea?: number;
  city?: string;
  neighborhood?: string;
  hasPool?: boolean;
  hasGarden?: boolean;
  hasSecurity?: boolean;
  isFurnished?: boolean;
  allowsPets?: boolean;
}

export type PropertySortOption = 'price-asc' | 'price-desc' | 'area-desc' | 'area-asc' | 'newest' | 'bedrooms-desc';

// Filter options
const propertyTypes = [
  { value: '', label: 'Todos los tipos' },
  { value: 'house', label: 'Casa' },
  { value: 'apartment', label: 'Departamento' },
  { value: 'condo', label: 'Condominio' },
  { value: 'land', label: 'Terreno' },
  { value: 'commercial', label: 'Comercial' },
  { value: 'office', label: 'Oficina' },
];

const listingTypes = [
  { value: '', label: 'Venta y Renta' },
  { value: 'sale', label: 'Venta' },
  { value: 'rent', label: 'Renta' },
];

const bedroomOptions = [
  { value: '', label: 'Cualquiera' },
  { value: '1', label: '1+' },
  { value: '2', label: '2+' },
  { value: '3', label: '3+' },
  { value: '4', label: '4+' },
  { value: '5', label: '5+' },
];

const bathroomOptions = [
  { value: '', label: 'Cualquiera' },
  { value: '1', label: '1+' },
  { value: '2', label: '2+' },
  { value: '3', label: '3+' },
];

const cities = [
  { value: '', label: 'Todas las ciudades' },
  { value: 'Ciudad de México', label: 'Ciudad de México' },
  { value: 'Guadalajara', label: 'Guadalajara' },
  { value: 'Monterrey', label: 'Monterrey' },
  { value: 'Querétaro', label: 'Querétaro' },
  { value: 'Puebla', label: 'Puebla' },
];

const sortOptions: { value: PropertySortOption; label: string }[] = [
  { value: 'newest', label: 'Más recientes' },
  { value: 'price-asc', label: 'Precio: menor a mayor' },
  { value: 'price-desc', label: 'Precio: mayor a menor' },
  { value: 'area-desc', label: 'Área: mayor a menor' },
  { value: 'area-asc', label: 'Área: menor a mayor' },
  { value: 'bedrooms-desc', label: 'Más recámaras' },
];

// Property Card Component
function PropertyCard({ property }: { property: PropertyListing }) {
  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);
  };

  const propertyTypeLabels: Record<string, string> = {
    house: 'Casa',
    apartment: 'Departamento',
    condo: 'Condominio',
    land: 'Terreno',
    commercial: 'Comercial',
    office: 'Oficina',
  };

  return (
    <Link
      to={`/properties/${property.id}`}
      className="group bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-lg transition-all duration-300"
    >
      {/* Image */}
      <div className="relative aspect-[4/3] overflow-hidden">
        <img
          src={property.primaryImageUrl || property.images[0]?.url}
          alt={property.title}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
        {/* Badges */}
        <div className="absolute top-3 left-3 flex gap-2">
          {property.isFeatured && (
            <span className="px-2 py-1 bg-amber-500 text-white text-xs font-semibold rounded-full">
              Destacado
            </span>
          )}
          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
            property.listingType === 'sale' 
              ? 'bg-blue-600 text-white' 
              : 'bg-green-600 text-white'
          }`}>
            {property.listingType === 'sale' ? 'Venta' : 'Renta'}
          </span>
        </div>
        {/* Property Type */}
        <div className="absolute bottom-3 left-3">
          <span className="px-2 py-1 bg-white/90 backdrop-blur-sm text-gray-700 text-xs font-medium rounded-full">
            {propertyTypeLabels[property.propertyType] || property.propertyType}
          </span>
        </div>
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Price */}
        <div className="mb-2">
          <span className="text-xl font-bold text-gray-900">
            {formatPrice(property.price, property.currency)}
          </span>
          {property.listingType === 'rent' && (
            <span className="text-gray-500 text-sm">/mes</span>
          )}
        </div>

        {/* Title */}
        <h3 className="font-semibold text-gray-900 mb-2 line-clamp-2 group-hover:text-blue-600 transition-colors">
          <LocalizedContent content={property.title} showBadge={false} />
        </h3>

        {/* Location */}
        <div className="flex items-center gap-1 text-gray-500 text-sm mb-3">
          <FiMapPin className="w-4 h-4 flex-shrink-0" />
          <span className="truncate">
            {property.location.neighborhood}, {property.location.city}
          </span>
        </div>

        {/* Features */}
        <div className="flex items-center gap-4 text-sm text-gray-600 pt-3 border-t border-gray-100">
          {property.bedrooms !== undefined && (
            <div className="flex items-center gap-1">
              <IoBedOutline className="w-4 h-4" />
              <span>{property.bedrooms}</span>
            </div>
          )}
          {property.bathrooms !== undefined && (
            <div className="flex items-center gap-1">
              <LuBath className="w-4 h-4" />
              <span>{property.bathrooms}</span>
            </div>
          )}
          {property.parkingSpaces !== undefined && property.parkingSpaces > 0 && (
            <div className="flex items-center gap-1">
              <BiCar className="w-4 h-4" />
              <span>{property.parkingSpaces}</span>
            </div>
          )}
          {property.totalArea && (
            <div className="flex items-center gap-1">
              <FiMaximize className="w-4 h-4" />
              <span>{property.totalArea} m²</span>
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}

// Filters Component
function PropertyFiltersPanel({
  filters,
  onFilterChange,
  onClear,
}: {
  filters: PropertyFilters;
  onFilterChange: (key: keyof PropertyFilters, value: string | number | boolean | undefined) => void;
  onClear: () => void;
}) {
  const [expandedSections, setExpandedSections] = useState<Set<string>>(
    new Set(['type', 'price', 'rooms'])
  );

  const toggleSection = (section: string) => {
    const newExpanded = new Set(expandedSections);
    if (newExpanded.has(section)) {
      newExpanded.delete(section);
    } else {
      newExpanded.add(section);
    }
    setExpandedSections(newExpanded);
  };

  const activeFiltersCount = useMemo(() => {
    return Object.values(filters).filter((v) => v !== undefined && v !== '').length;
  }, [filters]);

  const FilterSection = ({
    title,
    id,
    children,
  }: {
    title: string;
    id: string;
    children: React.ReactNode;
  }) => (
    <div className="border-b border-gray-200 last:border-b-0">
      <button
        onClick={() => toggleSection(id)}
        className="w-full flex items-center justify-between py-4 text-left"
      >
        <span className="font-medium text-gray-900">{title}</span>
        {expandedSections.has(id) ? (
          <FiChevronDown className="w-5 h-5 text-gray-400 rotate-180" />
        ) : (
          <FiChevronDown className="w-5 h-5 text-gray-400" />
        )}
      </button>
      {expandedSections.has(id) && <div className="pb-4">{children}</div>}
    </div>
  );

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <FiSliders className="w-5 h-5 text-gray-600" />
          <h3 className="font-semibold text-gray-900">Filtros</h3>
          {activeFiltersCount > 0 && (
            <span className="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
              {activeFiltersCount}
            </span>
          )}
        </div>
        {activeFiltersCount > 0 && (
          <button
            onClick={onClear}
            className="text-sm text-blue-600 hover:text-blue-700 font-medium"
          >
            Limpiar todo
          </button>
        )}
      </div>

      {/* Filter Sections */}
      <FilterSection title="Tipo de propiedad" id="type">
        <div className="space-y-3">
          <select
            value={filters.propertyType || ''}
            onChange={(e) => onFilterChange('propertyType', e.target.value || undefined)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            {propertyTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
          <select
            value={filters.listingType || ''}
            onChange={(e) => onFilterChange('listingType', e.target.value || undefined)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            {listingTypes.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>
      </FilterSection>

      <FilterSection title="Precio" id="price">
        <div className="space-y-3">
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Precio mínimo</label>
            <input
              type="number"
              placeholder="0"
              value={filters.minPrice || ''}
              onChange={(e) =>
                onFilterChange('minPrice', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Precio máximo</label>
            <input
              type="number"
              placeholder="Sin límite"
              value={filters.maxPrice || ''}
              onChange={(e) =>
                onFilterChange('maxPrice', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
      </FilterSection>

      <FilterSection title="Habitaciones" id="rooms">
        <div className="space-y-3">
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Recámaras</label>
            <select
              value={filters.minBedrooms || ''}
              onChange={(e) =>
                onFilterChange('minBedrooms', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {bedroomOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Baños</label>
            <select
              value={filters.minBathrooms || ''}
              onChange={(e) =>
                onFilterChange('minBathrooms', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {bathroomOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        </div>
      </FilterSection>

      <FilterSection title="Área (m²)" id="area">
        <div className="space-y-3">
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Área mínima</label>
            <input
              type="number"
              placeholder="0"
              value={filters.minArea || ''}
              onChange={(e) =>
                onFilterChange('minArea', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label className="text-sm text-gray-600 mb-1 block">Área máxima</label>
            <input
              type="number"
              placeholder="Sin límite"
              value={filters.maxArea || ''}
              onChange={(e) =>
                onFilterChange('maxArea', e.target.value ? Number(e.target.value) : undefined)
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
      </FilterSection>

      <FilterSection title="Ubicación" id="location">
        <select
          value={filters.city || ''}
          onChange={(e) => onFilterChange('city', e.target.value || undefined)}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        >
          {cities.map((city) => (
            <option key={city.value} value={city.value}>
              {city.label}
            </option>
          ))}
        </select>
      </FilterSection>

      <FilterSection title="Características" id="features">
        <div className="space-y-2">
          {[
            { key: 'hasPool', label: 'Alberca' },
            { key: 'hasGarden', label: 'Jardín' },
            { key: 'hasSecurity', label: 'Seguridad' },
            { key: 'isFurnished', label: 'Amueblado' },
            { key: 'allowsPets', label: 'Acepta mascotas' },
          ].map(({ key, label }) => (
            <label key={key} className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={filters[key as keyof PropertyFilters] === true}
                onChange={(e) =>
                  onFilterChange(key as keyof PropertyFilters, e.target.checked || undefined)
                }
                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">{label}</span>
            </label>
          ))}
        </div>
      </FilterSection>
    </div>
  );
}

// Main Page Component
export default function PropertyBrowsePage() {
  const { t } = useTranslation(['properties', 'common']);
  const [searchParams, setSearchParams] = useSearchParams();

  // Initialize filters from URL params
  const getInitialFilters = (): PropertyFilters => {
    const urlFilters: PropertyFilters = {};
    const propertyType = searchParams.get('propertyType');
    const listingType = searchParams.get('listingType');
    const minPrice = searchParams.get('minPrice');
    const maxPrice = searchParams.get('maxPrice');
    const minBedrooms = searchParams.get('minBedrooms');
    const minBathrooms = searchParams.get('minBathrooms');
    const city = searchParams.get('city');

    if (propertyType) urlFilters.propertyType = propertyType;
    if (listingType) urlFilters.listingType = listingType;
    if (minPrice) urlFilters.minPrice = Number(minPrice);
    if (maxPrice) urlFilters.maxPrice = Number(maxPrice);
    if (minBedrooms) urlFilters.minBedrooms = Number(minBedrooms);
    if (minBathrooms) urlFilters.minBathrooms = Number(minBathrooms);
    if (city) urlFilters.city = city;

    return urlFilters;
  };

  const [filters, setFilters] = useState<PropertyFilters>(getInitialFilters);
  const [sortBy, setSortBy] = useState<PropertySortOption>(
    (searchParams.get('sort') as PropertySortOption) || 'newest'
  );
  const [currentPage, setCurrentPage] = useState(Number(searchParams.get('page')) || 1);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  // Filter and sort properties
  const filteredProperties = useMemo(() => {
    let result = [...mockProperties];

    // Apply filters
    if (filters.propertyType) {
      result = result.filter((p) => p.propertyType === filters.propertyType);
    }
    if (filters.listingType) {
      result = result.filter((p) => p.listingType === filters.listingType);
    }
    if (filters.minPrice) {
      result = result.filter((p) => p.price >= filters.minPrice!);
    }
    if (filters.maxPrice) {
      result = result.filter((p) => p.price <= filters.maxPrice!);
    }
    if (filters.minBedrooms) {
      result = result.filter((p) => (p.bedrooms || 0) >= filters.minBedrooms!);
    }
    if (filters.minBathrooms) {
      result = result.filter((p) => (p.bathrooms || 0) >= filters.minBathrooms!);
    }
    if (filters.minArea) {
      result = result.filter((p) => (p.totalArea || 0) >= filters.minArea!);
    }
    if (filters.maxArea) {
      result = result.filter((p) => (p.totalArea || 0) <= filters.maxArea!);
    }
    if (filters.city) {
      result = result.filter((p) => p.location.city === filters.city);
    }
    if (filters.hasPool) {
      result = result.filter((p) => p.hasPool === true);
    }
    if (filters.hasGarden) {
      result = result.filter((p) => p.hasGarden === true);
    }
    if (filters.hasSecurity) {
      result = result.filter((p) => p.hasSecurity === true);
    }
    if (filters.isFurnished) {
      result = result.filter((p) => p.isFurnished === true);
    }
    if (filters.allowsPets) {
      result = result.filter((p) => p.allowsPets === true);
    }

    // Apply sorting
    switch (sortBy) {
      case 'price-asc':
        result.sort((a, b) => a.price - b.price);
        break;
      case 'price-desc':
        result.sort((a, b) => b.price - a.price);
        break;
      case 'area-desc':
        result.sort((a, b) => (b.totalArea || 0) - (a.totalArea || 0));
        break;
      case 'area-asc':
        result.sort((a, b) => (a.totalArea || 0) - (b.totalArea || 0));
        break;
      case 'bedrooms-desc':
        result.sort((a, b) => (b.bedrooms || 0) - (a.bedrooms || 0));
        break;
      case 'newest':
      default:
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
    }

    return result;
  }, [filters, sortBy]);

  // Pagination
  const totalPages = Math.ceil(filteredProperties.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const paginatedProperties = filteredProperties.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  // Update URL params
  const updateURLParams = useCallback(
    (newFilters: PropertyFilters, sort: PropertySortOption, page: number) => {
      const params = new URLSearchParams();
      if (newFilters.propertyType) params.set('propertyType', newFilters.propertyType);
      if (newFilters.listingType) params.set('listingType', newFilters.listingType);
      if (newFilters.minPrice) params.set('minPrice', newFilters.minPrice.toString());
      if (newFilters.maxPrice) params.set('maxPrice', newFilters.maxPrice.toString());
      if (newFilters.minBedrooms) params.set('minBedrooms', newFilters.minBedrooms.toString());
      if (newFilters.minBathrooms) params.set('minBathrooms', newFilters.minBathrooms.toString());
      if (newFilters.city) params.set('city', newFilters.city);
      params.set('sort', sort);
      if (page > 1) params.set('page', page.toString());
      setSearchParams(params);
    },
    [setSearchParams]
  );

  const handleFilterChange = useCallback(
    (key: keyof PropertyFilters, value: string | number | boolean | undefined) => {
      const newFilters = { ...filters, [key]: value };
      setFilters(newFilters);
      setCurrentPage(1);
      updateURLParams(newFilters, sortBy, 1);
    },
    [filters, sortBy, updateURLParams]
  );

  const handleClearFilters = useCallback(() => {
    setFilters({});
    setCurrentPage(1);
    updateURLParams({}, sortBy, 1);
  }, [sortBy, updateURLParams]);

  const handleSortChange = useCallback(
    (newSort: PropertySortOption) => {
      setSortBy(newSort);
      setCurrentPage(1);
      updateURLParams(filters, newSort, 1);
    },
    [filters, updateURLParams]
  );

  const handlePageChange = useCallback(
    (page: number) => {
      setCurrentPage(page);
      updateURLParams(filters, sortBy, page);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    [filters, sortBy, updateURLParams]
  );

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
              {t('properties:browse.title')}
            </h1>
            <p className="text-gray-600">
              {t('properties:browse.subtitle')}
            </p>
          </div>

          <div className="flex flex-col lg:flex-row gap-6">
            {/* Filters Sidebar - Desktop */}
            <aside className="hidden lg:block lg:w-72 flex-shrink-0">
              <PropertyFiltersPanel
                filters={filters}
                onFilterChange={handleFilterChange}
                onClear={handleClearFilters}
              />
            </aside>

            {/* Mobile Filters Button */}
            <div className="lg:hidden mb-4">
              <button
                onClick={() => setMobileFiltersOpen(true)}
                className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-white border border-gray-200 rounded-lg shadow-sm"
              >
                <FiSliders className="w-5 h-5" />
                <span className="font-medium">Filtros</span>
                {Object.keys(filters).length > 0 && (
                  <span className="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded-full">
                    {Object.keys(filters).length}
                  </span>
                )}
              </button>
            </div>

            {/* Mobile Filters Modal */}
            {mobileFiltersOpen && (
              <div className="fixed inset-0 z-50 lg:hidden">
                <div
                  className="absolute inset-0 bg-black/50"
                  onClick={() => setMobileFiltersOpen(false)}
                />
                <div className="absolute right-0 top-0 h-full w-full max-w-sm bg-white overflow-y-auto">
                  <div className="sticky top-0 bg-white border-b border-gray-200 p-4 flex items-center justify-between">
                    <h2 className="text-lg font-semibold">Filtros</h2>
                    <button onClick={() => setMobileFiltersOpen(false)}>
                      <FiX className="w-6 h-6" />
                    </button>
                  </div>
                  <div className="p-4">
                    <PropertyFiltersPanel
                      filters={filters}
                      onFilterChange={(key, value) => {
                        handleFilterChange(key, value);
                      }}
                      onClear={() => {
                        handleClearFilters();
                        setMobileFiltersOpen(false);
                      }}
                    />
                  </div>
                </div>
              </div>
            )}

            {/* Main Content */}
            <main className="flex-1">
              {/* Results Header */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4 sm:p-6 mb-6">
                <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                  <div>
                    <p className="text-lg font-semibold text-gray-900">
                      {t('properties:browse.propertiesFound', { count: filteredProperties.length })}
                    </p>
                  </div>

                  <div className="flex items-center gap-4">
                    {/* Sort */}
                    <select
                      value={sortBy}
                      onChange={(e) => handleSortChange(e.target.value as PropertySortOption)}
                      className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    >
                      {sortOptions.map((opt) => (
                        <option key={opt.value} value={opt.value}>
                          {opt.label}
                        </option>
                      ))}
                    </select>

                    {/* Compare Button */}
                    <Link
                      to="/properties/compare"
                      className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors bg-emerald-600 text-white hover:bg-emerald-700"
                    >
                      <FiBarChart2 size={18} />
                      <span className="hidden sm:inline">Comparar</span>
                    </Link>

                    {/* Map Button */}
                    <Link
                      to="/properties/map"
                      className="flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors bg-blue-600 text-white hover:bg-blue-700"
                    >
                      <FiMap size={18} />
                      <span className="hidden sm:inline">Mapa</span>
                    </Link>

                    {/* View Mode Toggle */}
                    <div className="flex items-center gap-1 bg-gray-100 rounded-lg p-1">
                      <button
                        onClick={() => setViewMode('grid')}
                        className={`p-2 rounded-md transition-colors ${
                          viewMode === 'grid'
                            ? 'bg-white shadow-sm text-blue-600'
                            : 'text-gray-600 hover:text-gray-900'
                        }`}
                        aria-label={t('properties:browse.gridView')}
                      >
                        <FiGrid size={18} />
                      </button>
                      <button
                        onClick={() => setViewMode('list')}
                        className={`p-2 rounded-md transition-colors ${
                          viewMode === 'list'
                            ? 'bg-white shadow-sm text-blue-600'
                            : 'text-gray-600 hover:text-gray-900'
                        }`}
                        aria-label={t('properties:browse.listView')}
                      >
                        <FiList size={18} />
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              {/* Properties Grid */}
              {paginatedProperties.length > 0 ? (
                <>
                  <div
                    className={`${
                      viewMode === 'grid'
                        ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5'
                        : 'flex flex-col gap-5'
                    } mb-8`}
                  >
                    {paginatedProperties.map((property) => (
                      <PropertyCard key={property.id} property={property} />
                    ))}
                  </div>

                  {/* Pagination */}
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalItems={filteredProperties.length}
                    itemsPerPage={ITEMS_PER_PAGE}
                    onPageChange={handlePageChange}
                  />
                </>
              ) : (
                <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
                  <FiHome className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">
                    {t('properties:empty.title')}
                  </h3>
                  <p className="text-gray-500 mb-4">
                    {t('properties:empty.subtitle')}
                  </p>
                  <button
                    onClick={handleClearFilters}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    {t('properties:empty.clearFilters')}
                  </button>
                </div>
              )}
            </main>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
