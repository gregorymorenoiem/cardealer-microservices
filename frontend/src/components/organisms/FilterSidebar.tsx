import { useState } from 'react';
import { FiSliders, FiX } from 'react-icons/fi';

interface FilterSidebarProps {
  onFilterChange: (filters: VehicleFilters) => void;
  onSortChange: (sort: SortOption) => void;
  currentFilters: VehicleFilters;
  currentSort: SortOption;
}

export interface VehicleFilters {
  make?: string;
  model?: string;
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  minMileage?: number;
  maxMileage?: number;
  transmission?: string;
  fuelType?: string;
  bodyType?: string;
  condition?: string;
}

export type SortOption = 'price-asc' | 'price-desc' | 'year-desc' | 'year-asc' | 'mileage-asc' | 'mileage-desc';

const makes = ['Tesla', 'BMW', 'Toyota', 'Ford', 'Honda', 'Audi', 'Mercedes-Benz', 'Chevrolet', 'Mazda', 'Volkswagen'];
const transmissions = ['Automatic', 'Manual', 'CVT'];
const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'Plug-in Hybrid'];
const bodyTypes = ['Sedan', 'SUV', 'Truck', 'Coupe', 'Hatchback', 'Van', 'Convertible', 'Wagon'];
const conditions = ['New', 'Used', 'Certified Pre-Owned'];

export default function FilterSidebar({ onFilterChange, onSortChange, currentFilters, currentSort }: FilterSidebarProps) {
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);
  const [filters, setFilters] = useState<VehicleFilters>(currentFilters);

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 30 }, (_, i) => currentYear - i);

  const handleFilterChange = (key: keyof VehicleFilters, value: string | number | undefined) => {
    const newFilters = { ...filters, [key]: value || undefined };
    setFilters(newFilters);
    onFilterChange(newFilters);
  };

  const handleClearFilters = () => {
    const emptyFilters: VehicleFilters = {};
    setFilters(emptyFilters);
    onFilterChange(emptyFilters);
  };

  const activeFilterCount = Object.values(filters).filter(Boolean).length;

  const FilterContent = () => (
    <div className="space-y-6">
      {/* Sort By */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Sort By
        </label>
        <select
          value={currentSort}
          onChange={(e) => onSortChange(e.target.value as SortOption)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="year-desc">Year: Newest First</option>
          <option value="year-asc">Year: Oldest First</option>
          <option value="price-asc">Price: Low to High</option>
          <option value="price-desc">Price: High to Low</option>
          <option value="mileage-asc">Mileage: Low to High</option>
          <option value="mileage-desc">Mileage: High to Low</option>
        </select>
      </div>

      {/* Make */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Make
        </label>
        <select
          value={filters.make || ''}
          onChange={(e) => handleFilterChange('make', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="">All Makes</option>
          {makes.map((make) => (
            <option key={make} value={make}>{make}</option>
          ))}
        </select>
      </div>

      {/* Model */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Model
        </label>
        <input
          type="text"
          value={filters.model || ''}
          onChange={(e) => handleFilterChange('model', e.target.value)}
          placeholder="Enter model name..."
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        />
      </div>

      {/* Year Range */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Year Range
        </label>
        <div className="grid grid-cols-2 gap-3">
          <select
            value={filters.minYear || ''}
            onChange={(e) => handleFilterChange('minYear', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          >
            <option value="">Min Year</option>
            {years.map((year) => (
              <option key={year} value={year}>{year}</option>
            ))}
          </select>
          <select
            value={filters.maxYear || ''}
            onChange={(e) => handleFilterChange('maxYear', e.target.value ? Number(e.target.value) : undefined)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          >
            <option value="">Max Year</option>
            {years.map((year) => (
              <option key={year} value={year}>{year}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Price Range */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Price Range
        </label>
        <div className="grid grid-cols-2 gap-3">
          <input
            type="number"
            value={filters.minPrice || ''}
            onChange={(e) => handleFilterChange('minPrice', e.target.value ? Number(e.target.value) : undefined)}
            placeholder="Min"
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          />
          <input
            type="number"
            value={filters.maxPrice || ''}
            onChange={(e) => handleFilterChange('maxPrice', e.target.value ? Number(e.target.value) : undefined)}
            placeholder="Max"
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          />
        </div>
      </div>

      {/* Mileage Range */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Mileage Range
        </label>
        <div className="grid grid-cols-2 gap-3">
          <input
            type="number"
            value={filters.minMileage || ''}
            onChange={(e) => handleFilterChange('minMileage', e.target.value ? Number(e.target.value) : undefined)}
            placeholder="Min"
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          />
          <input
            type="number"
            value={filters.maxMileage || ''}
            onChange={(e) => handleFilterChange('maxMileage', e.target.value ? Number(e.target.value) : undefined)}
            placeholder="Max"
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-sm"
          />
        </div>
      </div>

      {/* Transmission */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Transmission
        </label>
        <select
          value={filters.transmission || ''}
          onChange={(e) => handleFilterChange('transmission', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="">All Transmissions</option>
          {transmissions.map((trans) => (
            <option key={trans} value={trans}>{trans}</option>
          ))}
        </select>
      </div>

      {/* Fuel Type */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Fuel Type
        </label>
        <select
          value={filters.fuelType || ''}
          onChange={(e) => handleFilterChange('fuelType', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="">All Fuel Types</option>
          {fuelTypes.map((fuel) => (
            <option key={fuel} value={fuel}>{fuel}</option>
          ))}
        </select>
      </div>

      {/* Body Type */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Body Type
        </label>
        <select
          value={filters.bodyType || ''}
          onChange={(e) => handleFilterChange('bodyType', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="">All Body Types</option>
          {bodyTypes.map((body) => (
            <option key={body} value={body}>{body}</option>
          ))}
        </select>
      </div>

      {/* Condition */}
      <div>
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Condition
        </label>
        <select
          value={filters.condition || ''}
          onChange={(e) => handleFilterChange('condition', e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
        >
          <option value="">All Conditions</option>
          {conditions.map((cond) => (
            <option key={cond} value={cond}>{cond}</option>
          ))}
        </select>
      </div>

      {/* Clear Filters Button */}
      {activeFilterCount > 0 && (
        <button
          onClick={handleClearFilters}
          className="w-full py-2 px-4 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors duration-200 font-medium"
        >
          Clear All Filters ({activeFilterCount})
        </button>
      )}
    </div>
  );

  return (
    <>
      {/* Mobile Filter Button */}
      <div className="lg:hidden mb-4">
        <button
          onClick={() => setMobileFiltersOpen(true)}
          className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors duration-200"
        >
          <FiSliders size={20} />
          <span className="font-medium">Filters {activeFilterCount > 0 && `(${activeFilterCount})`}</span>
        </button>
      </div>

      {/* Desktop Sidebar */}
      <div className="hidden lg:block bg-white rounded-xl shadow-card p-6 sticky top-24">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-gray-900">Filters</h2>
          {activeFilterCount > 0 && (
            <span className="px-2 py-1 bg-primary text-white text-xs font-semibold rounded-full">
              {activeFilterCount}
            </span>
          )}
        </div>
        <FilterContent />
      </div>

      {/* Mobile Filter Modal */}
      {mobileFiltersOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="fixed inset-0 bg-black bg-opacity-50" onClick={() => setMobileFiltersOpen(false)} />
          <div className="fixed inset-y-0 right-0 w-full max-w-sm bg-white shadow-xl overflow-y-auto">
            <div className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-bold text-gray-900">Filters</h2>
                <button
                  onClick={() => setMobileFiltersOpen(false)}
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors duration-200"
                >
                  <FiX size={24} />
                </button>
              </div>
              <FilterContent />
              <button
                onClick={() => setMobileFiltersOpen(false)}
                className="w-full mt-6 py-3 px-4 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium"
              >
                Apply Filters
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
