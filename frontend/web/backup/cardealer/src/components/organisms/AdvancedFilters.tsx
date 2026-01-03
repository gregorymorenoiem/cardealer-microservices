import { useState, useCallback, useEffect, useRef, useMemo } from 'react';
import { FiSliders, FiX, FiChevronDown, FiChevronUp } from 'react-icons/fi';

interface AdvancedFiltersProps {
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
  features?: string[];
  minHorsepower?: number;
  drivetrain?: string;
}

export type SortOption = 'price-asc' | 'price-desc' | 'year-desc' | 'year-asc' | 'mileage-asc' | 'mileage-desc' | 'horsepower-desc';

const makes = ['Tesla', 'BMW', 'Toyota', 'Ford', 'Honda', 'Audi', 'Mercedes-Benz', 'Chevrolet', 'Mazda', 'Volkswagen'];
const transmissions = ['Automatic', 'Manual', 'CVT'];
const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'Plug-in Hybrid'];
const bodyTypes = ['Sedan', 'SUV', 'Truck', 'Coupe', 'Hatchback', 'Van', 'Convertible', 'Wagon'];
const conditions = ['New', 'Used', 'Certified Pre-Owned'];
const drivetrains = ['FWD', 'RWD', 'AWD', '4WD'];
const commonFeatures = [
  'Leather Seats',
  'Sunroof',
  'Navigation System',
  'Backup Camera',
  'Bluetooth',
  'Heated Seats',
  'Apple CarPlay',
  'Android Auto',
  'Adaptive Cruise Control',
  'Lane Departure Warning',
  'Blind Spot Monitoring',
  'Parking Sensors',
  'Keyless Entry',
  'Remote Start',
];

function AdvancedFilters({ onFilterChange, onSortChange, currentFilters, currentSort }: AdvancedFiltersProps) {
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(['basic', 'price']));
  const [localFilters, setLocalFilters] = useState<VehicleFilters>(currentFilters);
  const isFirstRenderRef = useRef(true);
  const onFilterChangeRef = useRef(onFilterChange);
  const debounceTimerRef = useRef<number | undefined>(undefined);
  
  // Keep ref updated
  useEffect(() => {
    onFilterChangeRef.current = onFilterChange;
  }, [onFilterChange]);

  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 30 }, (_, i) => currentYear - i);

  const toggleSection = (section: string) => {
    const newExpanded = new Set(expandedSections);
    if (newExpanded.has(section)) {
      newExpanded.delete(section);
    } else {
      newExpanded.add(section);
    }
    setExpandedSections(newExpanded);
  };

  const handleFilterChange = useCallback((key: keyof VehicleFilters, value: string | number | string[] | undefined) => {
    setLocalFilters(prev => ({ ...prev, [key]: value || undefined }));
  }, []);

  // Debounced effect to call parent when localFilters changes
  useEffect(() => {
    if (isFirstRenderRef.current) {
      isFirstRenderRef.current = false;
      return;
    }
    
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }
    
    debounceTimerRef.current = setTimeout(() => {
      onFilterChangeRef.current(localFilters);
    }, 300);
    
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, [localFilters]);

  const handlePriceSliderChange = useCallback((e: React.ChangeEvent<HTMLInputElement>, type: 'min' | 'max') => {
    const value = Number(e.target.value);
    if (type === 'min') {
      handleFilterChange('minPrice', value);
    } else {
      handleFilterChange('maxPrice', value);
    }
  }, [handleFilterChange]);

  const handleFeatureToggle = useCallback((feature: string) => {
    setLocalFilters(prev => {
      const currentFeatures = prev.features || [];
      const newFeatures = currentFeatures.includes(feature)
        ? currentFeatures.filter(f => f !== feature)
        : [...currentFeatures, feature];
      return { ...prev, features: newFeatures.length > 0 ? newFeatures : undefined };
    });
  }, []);

  const handleClearFilters = useCallback(() => {
    setLocalFilters({});
  }, []);

  // Memoized handlers for specific inputs to prevent re-renders
  const handleMakeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('make', e.target.value);
  }, [handleFilterChange]);

  const handleConditionChange = useCallback((condition: string) => {
    handleFilterChange('condition', condition);
  }, [handleFilterChange]);

  const handleMinPriceInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    handleFilterChange('minPrice', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMaxPriceInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    handleFilterChange('maxPrice', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMinYearChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('minYear', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMaxYearChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('maxYear', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMinMileageChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    handleFilterChange('minMileage', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMaxMileageChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    handleFilterChange('maxMileage', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleMinHorsepowerChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    handleFilterChange('minHorsepower', e.target.value ? Number(e.target.value) : undefined);
  }, [handleFilterChange]);

  const handleTransmissionChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('transmission', e.target.value);
  }, [handleFilterChange]);

  const handleFuelTypeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('fuelType', e.target.value);
  }, [handleFilterChange]);

  const handleBodyTypeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('bodyType', e.target.value);
  }, [handleFilterChange]);

  const handleDrivetrainChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    handleFilterChange('drivetrain', e.target.value);
  }, [handleFilterChange]);

  const activeFilterCount = Object.values(localFilters).filter(v => 
    v !== undefined && (Array.isArray(v) ? v.length > 0 : true)
  ).length;

  const filterContent = useMemo(() => (
    <div className="space-y-4">
      {/* Sort By */}
      <div className="bg-white rounded-lg border border-gray-200 p-4">
        <label className="block text-sm font-semibold text-gray-900 mb-3">
          Sort By
        </label>
        <select
          value={currentSort}
          onChange={(e) => onSortChange(e.target.value as SortOption)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
        >
          <option value="year-desc">Year: Newest First</option>
          <option value="year-asc">Year: Oldest First</option>
          <option value="price-asc">Price: Low to High</option>
          <option value="price-desc">Price: High to Low</option>
          <option value="mileage-asc">Mileage: Low to High</option>
          <option value="mileage-desc">Mileage: High to Low</option>
          <option value="horsepower-desc">Horsepower: High to Low</option>
        </select>
      </div>

      {/* Basic Filters */}
      <div className="bg-white rounded-lg border border-gray-200">
        <button
          onClick={() => toggleSection('basic')}
          className="w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
        >
          <span className="text-sm font-semibold text-gray-900">Basic Filters</span>
          {expandedSections.has('basic') ? <FiChevronUp /> : <FiChevronDown />}
        </button>
        
        {expandedSections.has('basic') && (
          <div className="p-4 pt-0 space-y-4">
            {/* Make */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Make</label>
              <select
                value={localFilters.make || ''}
                onChange={handleMakeChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">All Makes</option>
                {makes.map((make) => (
                  <option key={make} value={make}>{make}</option>
                ))}
              </select>
            </div>

            {/* Model */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Model</label>
              <input
                type="text"
                value={localFilters.model || ''}
                onChange={(e) => handleFilterChange('model', e.target.value)}
                placeholder="Enter model name..."
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>

            {/* Condition */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Condition</label>
              <div className="space-y-2">
                {conditions.map((condition) => (
                  <label key={condition} className="flex items-center">
                    <input
                      type="radio"
                      name="condition"
                      checked={localFilters.condition === condition}
                      onChange={() => handleConditionChange(condition)}
                      className="mr-2"
                    />
                    <span className="text-sm text-gray-700">{condition}</span>
                  </label>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Price Range with Slider */}
      <div className="bg-white rounded-lg border border-gray-200">
        <button
          onClick={() => toggleSection('price')}
          className="w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
        >
          <span className="text-sm font-semibold text-gray-900">Price Range</span>
          {expandedSections.has('price') ? <FiChevronUp /> : <FiChevronDown />}
        </button>
        
        {expandedSections.has('price') && (
          <div className="p-4 pt-0 space-y-4">
            <div className="space-y-3">
              <div className="flex justify-between text-sm text-gray-600">
                <span>${(currentFilters.minPrice || 0).toLocaleString()}</span>
                <span>${(currentFilters.maxPrice || 100000).toLocaleString()}</span>
              </div>
              
              {/* Min Price Slider */}
              <div>
                <label className="block text-xs text-gray-600 mb-1">Minimum Price</label>
                <input
                  type="range"
                  min="0"
                  max="100000"
                  step="1000"
                  value={localFilters.minPrice || 0}
                  onChange={(e) => handlePriceSliderChange(e, 'min')}
                  className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-primary"
                />
              </div>

              {/* Max Price Slider */}
              <div>
                <label className="block text-xs text-gray-600 mb-1">Maximum Price</label>
                <input
                  type="range"
                  min="0"
                  max="100000"
                  step="1000"
                  value={localFilters.maxPrice || 100000}
                  onChange={(e) => handlePriceSliderChange(e, 'max')}
                  className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-primary"
                />
              </div>

              {/* Manual Input */}
              <div className="grid grid-cols-2 gap-3">
                <input
                  type="number"
                  value={localFilters.minPrice || ''}
                  onChange={handleMinPriceInputChange}
                  placeholder="Min"
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                />
                <input
                  type="number"
                  value={localFilters.maxPrice || ''}
                  onChange={handleMaxPriceInputChange}
                  placeholder="Max"
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                />
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Year & Mileage */}
      <div className="bg-white rounded-lg border border-gray-200">
        <button
          onClick={() => toggleSection('specs')}
          className="w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
        >
          <span className="text-sm font-semibold text-gray-900">Year & Mileage</span>
          {expandedSections.has('specs') ? <FiChevronUp /> : <FiChevronDown />}
        </button>
        
        {expandedSections.has('specs') && (
          <div className="p-4 pt-0 space-y-4">
            {/* Year Range */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Year Range</label>
              <div className="grid grid-cols-2 gap-3">
                <select
                  value={localFilters.minYear || ''}
                  onChange={handleMinYearChange}
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                >
                  <option value="">Min Year</option>
                  {years.map((year) => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </select>
                <select
                  value={localFilters.maxYear || ''}
                  onChange={handleMaxYearChange}
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                >
                  <option value="">Max Year</option>
                  {years.map((year) => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* Mileage Range */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Mileage Range</label>
              <div className="grid grid-cols-2 gap-3">
                <input
                  type="number"
                  value={localFilters.minMileage || ''}
                  onChange={handleMinMileageChange}
                  placeholder="Min Miles"
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                />
                <input
                  type="number"
                  value={localFilters.maxMileage || ''}
                  onChange={handleMaxMileageChange}
                  placeholder="Max Miles"
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
                />
              </div>
            </div>

            {/* Horsepower */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Minimum Horsepower</label>
              <input
                type="number"
                value={localFilters.minHorsepower || ''}
                onChange={handleMinHorsepowerChange}
                placeholder="Min HP"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-sm"
              />
            </div>
          </div>
        )}
      </div>

      {/* Vehicle Type */}
      <div className="bg-white rounded-lg border border-gray-200">
        <button
          onClick={() => toggleSection('type')}
          className="w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
        >
          <span className="text-sm font-semibold text-gray-900">Vehicle Type</span>
          {expandedSections.has('type') ? <FiChevronUp /> : <FiChevronDown />}
        </button>
        
        {expandedSections.has('type') && (
          <div className="p-4 pt-0 space-y-4">
            {/* Body Type */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Body Type</label>
              <select
                value={localFilters.bodyType || ''}
                onChange={handleBodyTypeChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">All Types</option>
                {bodyTypes.map((type) => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </div>

            {/* Transmission */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Transmission</label>
              <select
                value={localFilters.transmission || ''}
                onChange={handleTransmissionChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">All Transmissions</option>
                {transmissions.map((trans) => (
                  <option key={trans} value={trans}>{trans}</option>
                ))}
              </select>
            </div>

            {/* Fuel Type */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Fuel Type</label>
              <select
                value={localFilters.fuelType || ''}
                onChange={handleFuelTypeChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">All Fuel Types</option>
                {fuelTypes.map((fuel) => (
                  <option key={fuel} value={fuel}>{fuel}</option>
                ))}
              </select>
            </div>

            {/* Drivetrain */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Drivetrain</label>
              <select
                value={localFilters.drivetrain || ''}
                onChange={handleDrivetrainChange}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">All Drivetrains</option>
                {drivetrains.map((dt) => (
                  <option key={dt} value={dt}>{dt}</option>
                ))}
              </select>
            </div>
          </div>
        )}
      </div>

      {/* Features */}
      <div className="bg-white rounded-lg border border-gray-200">
        <button
          onClick={() => toggleSection('features')}
          className="w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors"
        >
          <span className="text-sm font-semibold text-gray-900">
            Features {localFilters.features && localFilters.features.length > 0 && `(${localFilters.features.length})`}
          </span>
          {expandedSections.has('features') ? <FiChevronUp /> : <FiChevronDown />}
        </button>
        
        {expandedSections.has('features') && (
          <div className="p-4 pt-0">
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {commonFeatures.map((feature) => (
                <label key={feature} className="flex items-center hover:bg-gray-50 p-2 rounded cursor-pointer">
                  <input
                    type="checkbox"
                    checked={localFilters.features?.includes(feature) || false}
                    onChange={() => handleFeatureToggle(feature)}
                    className="mr-3 h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                  />
                  <span className="text-sm text-gray-700">{feature}</span>
                </label>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Clear Filters Button */}
      {activeFilterCount > 0 && (
        <button
          onClick={handleClearFilters}
          className="w-full py-3 px-4 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition-colors font-semibold flex items-center justify-center gap-2"
        >
          <FiX />
          Clear All Filters ({activeFilterCount})
        </button>
      )}
    </div>
  // eslint-disable-next-line react-hooks/exhaustive-deps
  ), [localFilters, currentSort, expandedSections]);

  return (
    <>
      {/* Mobile Filter Button */}
      <div className="lg:hidden mb-4">
        <button
          onClick={() => setMobileFiltersOpen(true)}
          className="w-full flex items-center justify-center gap-2 px-4 py-3 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-semibold"
        >
          <FiSliders />
          Filters {activeFilterCount > 0 && `(${activeFilterCount})`}
        </button>
      </div>

      {/* Desktop Filters */}
      <div className="hidden lg:block">
        <div className="sticky top-4">
          {filterContent}
        </div>
      </div>

      {/* Mobile Filters Modal */}
      {mobileFiltersOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 lg:hidden">
          <div className="fixed inset-y-0 right-0 w-full max-w-sm bg-gray-50 shadow-xl overflow-y-auto">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between z-10">
              <h2 className="text-xl font-bold text-gray-900">Filters</h2>
              <button
                onClick={() => setMobileFiltersOpen(false)}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <FiX className="text-xl" />
              </button>
            </div>
            <div className="p-4">
              {filterContent}
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default AdvancedFilters;


