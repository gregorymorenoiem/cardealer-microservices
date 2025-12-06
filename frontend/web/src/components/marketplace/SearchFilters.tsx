import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
  FunnelIcon,
  XMarkIcon,
  ChevronDownIcon,
  ChevronUpIcon,
} from '@heroicons/react/24/outline';
import { type MarketplaceVertical } from '../../types/marketplace';

// Separate interfaces for each filter type
interface VehicleFilterValues {
  make: string;
  model: string;
  minYear: string;
  maxYear: string;
  minPrice: string;
  maxPrice: string;
  minMileage: string;
  maxMileage: string;
  fuelType: string;
  transmission: string;
  condition: string;
  bodyType: string;
}

interface PropertyFilterValues {
  propertyType: string;
  listingType: string;
  minPrice: string;
  maxPrice: string;
  minBedrooms: string;
  maxBedrooms: string;
  minBathrooms: string;
  maxBathrooms: string;
  minArea: string;
  maxArea: string;
  hasParking: boolean;
  hasPool: boolean;
  hasGarden: boolean;
  isFurnished: boolean;
}

interface SearchFiltersProps {
  vertical: MarketplaceVertical;
  onSearch: (filters: Record<string, string | number | boolean>) => void;
  isLoading?: boolean;
}

const initialVehicleFilters: VehicleFilterValues = {
  make: '',
  model: '',
  minYear: '',
  maxYear: '',
  minPrice: '',
  maxPrice: '',
  minMileage: '',
  maxMileage: '',
  fuelType: '',
  transmission: '',
  condition: '',
  bodyType: '',
};

const initialPropertyFilters: PropertyFilterValues = {
  propertyType: '',
  listingType: '',
  minPrice: '',
  maxPrice: '',
  minBedrooms: '',
  maxBedrooms: '',
  minBathrooms: '',
  maxBathrooms: '',
  minArea: '',
  maxArea: '',
  hasParking: false,
  hasPool: false,
  hasGarden: false,
  isFurnished: false,
};

// Vehicle Makes
const vehicleMakes = [
  'Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz',
  'Audi', 'Volkswagen', 'Nissan', 'Hyundai', 'Kia', 'Mazda',
];

// Vehicle Body Types
const bodyTypes = [
  'Sedan', 'SUV', 'Hatchback', 'Coupe', 'Truck', 'Van', 'Wagon', 'Convertible',
];

// Property Types
const propertyTypes = [
  'Casa', 'Apartamento', 'Terreno', 'Local Comercial', 'Oficina', 'Bodega',
];

// Listing Types
const listingTypes = [
  { value: 'sale', label: 'Venta' },
  { value: 'rent', label: 'Alquiler' },
];

// Filter Input Component
interface FilterInputProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: 'text' | 'number';
  placeholder?: string;
}

const FilterInput: React.FC<FilterInputProps> = ({
  label,
  value,
  onChange,
  type = 'text',
  placeholder = '',
}) => (
  <div>
    <label className="block text-sm font-medium text-gray-700 mb-1">
      {label}
    </label>
    <input
      type={type}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={placeholder}
      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
    />
  </div>
);

// Filter Select Component
interface FilterSelectProps {
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: Array<{ value: string; label: string }> | string[];
  placeholder?: string;
}

const FilterSelect: React.FC<FilterSelectProps> = ({
  label,
  value,
  onChange,
  options,
  placeholder = 'Seleccionar...',
}) => (
  <div>
    <label className="block text-sm font-medium text-gray-700 mb-1">
      {label}
    </label>
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
    >
      <option value="">{placeholder}</option>
      {options.map((option) => {
        if (typeof option === 'string') {
          return (
            <option key={option} value={option}>
              {option}
            </option>
          );
        }
        return (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        );
      })}
    </select>
  </div>
);

// Filter Checkbox Component
interface FilterCheckboxProps {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
}

const FilterCheckbox: React.FC<FilterCheckboxProps> = ({
  label,
  checked,
  onChange,
}) => (
  <label className="flex items-center space-x-2 cursor-pointer">
    <input
      type="checkbox"
      checked={checked}
      onChange={(e) => onChange(e.target.checked)}
      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
    />
    <span className="text-sm text-gray-700">{label}</span>
  </label>
);

// Vehicle Filters Component
interface VehicleFiltersComponentProps {
  filters: VehicleFilterValues;
  onChange: (key: keyof VehicleFilterValues, value: string) => void;
}

const VehicleFiltersComponent: React.FC<VehicleFiltersComponentProps> = ({
  filters,
  onChange,
}) => {
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <FilterSelect
          label="Marca"
          value={filters.make}
          onChange={(v) => onChange('make', v)}
          options={vehicleMakes}
          placeholder="Todas las marcas"
        />
        <FilterInput
          label="Modelo"
          value={filters.model}
          onChange={(v) => onChange('model', v)}
          placeholder="Ej: Corolla, Civic..."
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Año mínimo"
          value={filters.minYear}
          onChange={(v) => onChange('minYear', v)}
          type="number"
          placeholder="2015"
        />
        <FilterInput
          label="Año máximo"
          value={filters.maxYear}
          onChange={(v) => onChange('maxYear', v)}
          type="number"
          placeholder="2024"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Precio mínimo"
          value={filters.minPrice}
          onChange={(v) => onChange('minPrice', v)}
          type="number"
          placeholder="$0"
        />
        <FilterInput
          label="Precio máximo"
          value={filters.maxPrice}
          onChange={(v) => onChange('maxPrice', v)}
          type="number"
          placeholder="$100,000"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Kilometraje mínimo"
          value={filters.minMileage}
          onChange={(v) => onChange('minMileage', v)}
          type="number"
          placeholder="0"
        />
        <FilterInput
          label="Kilometraje máximo"
          value={filters.maxMileage}
          onChange={(v) => onChange('maxMileage', v)}
          type="number"
          placeholder="200,000"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterSelect
          label="Combustible"
          value={filters.fuelType}
          onChange={(v) => onChange('fuelType', v)}
          options={[
            { value: 'gasoline', label: 'Gasolina' },
            { value: 'diesel', label: 'Diesel' },
            { value: 'hybrid', label: 'Híbrido' },
            { value: 'electric', label: 'Eléctrico' },
          ]}
        />
        <FilterSelect
          label="Transmisión"
          value={filters.transmission}
          onChange={(v) => onChange('transmission', v)}
          options={[
            { value: 'automatic', label: 'Automática' },
            { value: 'manual', label: 'Manual' },
          ]}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterSelect
          label="Condición"
          value={filters.condition}
          onChange={(v) => onChange('condition', v)}
          options={[
            { value: 'new', label: 'Nuevo' },
            { value: 'used', label: 'Usado' },
            { value: 'certified', label: 'Certificado' },
          ]}
        />
        <FilterSelect
          label="Tipo de carrocería"
          value={filters.bodyType}
          onChange={(v) => onChange('bodyType', v)}
          options={bodyTypes}
        />
      </div>
    </div>
  );
};

// Property Filters Component
interface PropertyFiltersComponentProps {
  filters: PropertyFilterValues;
  onStringChange: (key: keyof Omit<PropertyFilterValues, 'hasParking' | 'hasPool' | 'hasGarden' | 'isFurnished'>, value: string) => void;
  onBooleanChange: (key: 'hasParking' | 'hasPool' | 'hasGarden' | 'isFurnished', value: boolean) => void;
}

const PropertyFiltersComponent: React.FC<PropertyFiltersComponentProps> = ({
  filters,
  onStringChange,
  onBooleanChange,
}) => {
  return (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <FilterSelect
          label="Tipo de propiedad"
          value={filters.propertyType}
          onChange={(v) => onStringChange('propertyType', v)}
          options={propertyTypes}
          placeholder="Todos los tipos"
        />
        <FilterSelect
          label="Tipo de listado"
          value={filters.listingType}
          onChange={(v) => onStringChange('listingType', v)}
          options={listingTypes}
          placeholder="Venta o Alquiler"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Precio mínimo"
          value={filters.minPrice}
          onChange={(v) => onStringChange('minPrice', v)}
          type="number"
          placeholder="$0"
        />
        <FilterInput
          label="Precio máximo"
          value={filters.maxPrice}
          onChange={(v) => onStringChange('maxPrice', v)}
          type="number"
          placeholder="$1,000,000"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Habitaciones mín"
          value={filters.minBedrooms}
          onChange={(v) => onStringChange('minBedrooms', v)}
          type="number"
          placeholder="1"
        />
        <FilterInput
          label="Habitaciones máx"
          value={filters.maxBedrooms}
          onChange={(v) => onStringChange('maxBedrooms', v)}
          type="number"
          placeholder="10"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Baños mín"
          value={filters.minBathrooms}
          onChange={(v) => onStringChange('minBathrooms', v)}
          type="number"
          placeholder="1"
        />
        <FilterInput
          label="Baños máx"
          value={filters.maxBathrooms}
          onChange={(v) => onStringChange('maxBathrooms', v)}
          type="number"
          placeholder="6"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FilterInput
          label="Área mín (m²)"
          value={filters.minArea}
          onChange={(v) => onStringChange('minArea', v)}
          type="number"
          placeholder="50"
        />
        <FilterInput
          label="Área máx (m²)"
          value={filters.maxArea}
          onChange={(v) => onStringChange('maxArea', v)}
          type="number"
          placeholder="500"
        />
      </div>

      <div className="border-t pt-4">
        <p className="text-sm font-medium text-gray-700 mb-3">Amenidades</p>
        <div className="grid grid-cols-2 gap-3">
          <FilterCheckbox
            label="Estacionamiento"
            checked={filters.hasParking}
            onChange={(v) => onBooleanChange('hasParking', v)}
          />
          <FilterCheckbox
            label="Piscina"
            checked={filters.hasPool}
            onChange={(v) => onBooleanChange('hasPool', v)}
          />
          <FilterCheckbox
            label="Jardín"
            checked={filters.hasGarden}
            onChange={(v) => onBooleanChange('hasGarden', v)}
          />
          <FilterCheckbox
            label="Amueblado"
            checked={filters.isFurnished}
            onChange={(v) => onBooleanChange('isFurnished', v)}
          />
        </div>
      </div>
    </div>
  );
};

// Main SearchFilters Component
export const SearchFilters: React.FC<SearchFiltersProps> = ({
  vertical,
  onSearch,
  isLoading = false,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isExpanded, setIsExpanded] = useState(true);
  const [vehicleFilters, setVehicleFilters] = useState<VehicleFilterValues>(initialVehicleFilters);
  const [propertyFilters, setPropertyFilters] = useState<PropertyFilterValues>(initialPropertyFilters);

  const handleVehicleFilterChange = (key: keyof VehicleFilterValues, value: string) => {
    setVehicleFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handlePropertyStringChange = (
    key: keyof Omit<PropertyFilterValues, 'hasParking' | 'hasPool' | 'hasGarden' | 'isFurnished'>,
    value: string
  ) => {
    setPropertyFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handlePropertyBooleanChange = (
    key: 'hasParking' | 'hasPool' | 'hasGarden' | 'isFurnished',
    value: boolean
  ) => {
    setPropertyFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleSearch = () => {
    const filters: Record<string, string | number | boolean> = {};
    
    if (vertical === 'vehicles') {
      Object.entries(vehicleFilters).forEach(([key, value]) => {
        if (value !== '') {
          filters[key] = value;
        }
      });
    } else {
      Object.entries(propertyFilters).forEach(([key, value]) => {
        if (value !== '' && value !== false) {
          filters[key] = value;
        }
      });
    }
    
    onSearch(filters);
  };

  const handleReset = () => {
    if (vertical === 'vehicles') {
      setVehicleFilters(initialVehicleFilters);
    } else {
      setPropertyFilters(initialPropertyFilters);
    }
  };

  const getActiveFiltersCount = (): number => {
    if (vertical === 'vehicles') {
      return Object.values(vehicleFilters).filter((v) => v !== '').length;
    }
    return Object.values(propertyFilters).filter((v) => v !== '' && v !== false).length;
  };

  const activeCount = getActiveFiltersCount();

  return (
    <>
      {/* Mobile Filter Toggle */}
      <div className="lg:hidden mb-4">
        <button
          onClick={() => setIsOpen(true)}
          className="w-full flex items-center justify-center space-x-2 px-4 py-3 bg-white border border-gray-300 rounded-lg shadow-sm hover:bg-gray-50 transition-colors"
        >
          <FunnelIcon className="h-5 w-5 text-gray-600" />
          <span className="font-medium text-gray-700">Filtros</span>
          {activeCount > 0 && (
            <span className="bg-blue-600 text-white text-xs font-bold px-2 py-0.5 rounded-full">
              {activeCount}
            </span>
          )}
        </button>
      </div>

      {/* Mobile Filter Drawer */}
      <AnimatePresence>
        {isOpen && (
          <>
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-40 lg:hidden"
              onClick={() => setIsOpen(false)}
            />
            <motion.div
              initial={{ x: '-100%' }}
              animate={{ x: 0 }}
              exit={{ x: '-100%' }}
              transition={{ type: 'tween', duration: 0.3 }}
              className="fixed inset-y-0 left-0 w-80 bg-white z-50 lg:hidden overflow-y-auto"
            >
              <div className="sticky top-0 bg-white border-b px-4 py-3 flex items-center justify-between">
                <h3 className="text-lg font-semibold">Filtros</h3>
                <button
                  onClick={() => setIsOpen(false)}
                  className="p-2 hover:bg-gray-100 rounded-full"
                >
                  <XMarkIcon className="h-5 w-5" />
                </button>
              </div>
              <div className="p-4">
                {vertical === 'vehicles' ? (
                  <VehicleFiltersComponent
                    filters={vehicleFilters}
                    onChange={handleVehicleFilterChange}
                  />
                ) : (
                  <PropertyFiltersComponent
                    filters={propertyFilters}
                    onStringChange={handlePropertyStringChange}
                    onBooleanChange={handlePropertyBooleanChange}
                  />
                )}
                <div className="mt-6 space-y-3">
                  <button
                    onClick={() => {
                      handleSearch();
                      setIsOpen(false);
                    }}
                    disabled={isLoading}
                    className="w-full py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                  >
                    {isLoading ? 'Buscando...' : 'Aplicar filtros'}
                  </button>
                  <button
                    onClick={handleReset}
                    className="w-full py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Limpiar filtros
                  </button>
                </div>
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>

      {/* Desktop Sidebar Filters */}
      <div className="hidden lg:block bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="w-full flex items-center justify-between px-4 py-3 bg-gray-50 border-b border-gray-200 hover:bg-gray-100 transition-colors"
        >
          <div className="flex items-center space-x-2">
            <FunnelIcon className="h-5 w-5 text-gray-600" />
            <span className="font-semibold text-gray-800">Filtros</span>
            {activeCount > 0 && (
              <span className="bg-blue-600 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                {activeCount}
              </span>
            )}
          </div>
          {isExpanded ? (
            <ChevronUpIcon className="h-5 w-5 text-gray-500" />
          ) : (
            <ChevronDownIcon className="h-5 w-5 text-gray-500" />
          )}
        </button>

        <AnimatePresence>
          {isExpanded && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="overflow-hidden"
            >
              <div className="p-4">
                {vertical === 'vehicles' ? (
                  <VehicleFiltersComponent
                    filters={vehicleFilters}
                    onChange={handleVehicleFilterChange}
                  />
                ) : (
                  <PropertyFiltersComponent
                    filters={propertyFilters}
                    onStringChange={handlePropertyStringChange}
                    onBooleanChange={handlePropertyBooleanChange}
                  />
                )}
                <div className="mt-6 space-y-3">
                  <button
                    onClick={handleSearch}
                    disabled={isLoading}
                    className="w-full py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                  >
                    {isLoading ? 'Buscando...' : 'Buscar'}
                  </button>
                  <button
                    onClick={handleReset}
                    className="w-full py-3 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Limpiar
                  </button>
                </div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </>
  );
};

export default SearchFilters;
