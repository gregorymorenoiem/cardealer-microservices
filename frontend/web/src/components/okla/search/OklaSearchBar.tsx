import { motion, AnimatePresence } from 'framer-motion';
import { 
  Search, 
  SlidersHorizontal, 
  X, 
  ChevronDown,
  MapPin,
  DollarSign
} from 'lucide-react';
import { useState, useEffect, useCallback } from 'react';
import { OklaButton } from '../../atoms/okla/OklaButton';
import { OklaInput } from '../../atoms/okla/OklaInput';

interface FilterOption {
  id: string;
  label: string;
  count?: number;
}

interface FilterSection {
  id: string;
  label: string;
  type: 'checkbox' | 'radio' | 'range' | 'select';
  options?: FilterOption[];
  min?: number;
  max?: number;
  step?: number;
  prefix?: string;
}

interface OklaSearchBarProps {
  value: string;
  onChange: (value: string) => void;
  onSearch: () => void;
  placeholder?: string;
  showFilters?: boolean;
  filterSections?: FilterSection[];
  activeFilters?: Record<string, string | string[] | [number, number]>;
  onFilterChange?: (filters: Record<string, string | string[] | [number, number]>) => void;
  onClearFilters?: () => void;
  suggestions?: string[];
}

export const OklaSearchBar = ({
  value,
  onChange,
  onSearch,
  placeholder = 'Buscar...',
  showFilters = true,
  filterSections = [],
  activeFilters = {},
  onFilterChange,
  onClearFilters,
  suggestions = [],
}: OklaSearchBarProps) => {
  const [isFiltersOpen, setIsFiltersOpen] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [localFilters, setLocalFilters] = useState(activeFilters);

  useEffect(() => {
    setLocalFilters(activeFilters);
  }, [activeFilters]);

  const handleFilterChange = useCallback((sectionId: string, value: string | string[] | [number, number]) => {
    const newFilters = { ...localFilters, [sectionId]: value };
    setLocalFilters(newFilters);
    onFilterChange?.(newFilters);
  }, [localFilters, onFilterChange]);

  const activeFilterCount = Object.values(localFilters).filter((v) => {
    if (Array.isArray(v)) {
      return v.length > 0;
    }
    return v !== '' && v !== undefined;
  }).length;

  return (
    <div className="relative">
      {/* Main Search Bar */}
      <div className="bg-white rounded-2xl shadow-lg border border-okla-cream overflow-hidden">
        <div className="flex items-center">
          {/* Search Input */}
          <div className="flex-1 relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-okla-slate" />
            <input
              type="text"
              value={value}
              onChange={(e) => onChange(e.target.value)}
              onFocus={() => setShowSuggestions(true)}
              onBlur={() => setTimeout(() => setShowSuggestions(false), 200)}
              placeholder={placeholder}
              className="w-full pl-12 pr-4 py-4 text-okla-charcoal placeholder-okla-slate/60 focus:outline-none text-lg"
              onKeyDown={(e) => e.key === 'Enter' && onSearch()}
            />
          </div>

          {/* Filter Toggle */}
          {showFilters && (
            <motion.button
              onClick={() => setIsFiltersOpen(!isFiltersOpen)}
              className={`flex items-center gap-2 px-4 py-4 border-l border-okla-cream transition-colors ${
                isFiltersOpen ? 'bg-okla-navy text-white' : 'text-okla-charcoal hover:bg-okla-cream'
              }`}
              whileTap={{ scale: 0.95 }}
            >
              <SlidersHorizontal className="w-5 h-5" />
              <span className="hidden sm:inline font-medium">Filtros</span>
              {activeFilterCount > 0 && (
                <span className="w-5 h-5 rounded-full bg-okla-gold text-white text-xs flex items-center justify-center">
                  {activeFilterCount}
                </span>
              )}
            </motion.button>
          )}

          {/* Search Button */}
          <OklaButton
            variant="primary"
            onClick={onSearch}
            className="rounded-none px-8 h-full"
          >
            Buscar
          </OklaButton>
        </div>

        {/* Suggestions Dropdown */}
        <AnimatePresence>
          {showSuggestions && suggestions.length > 0 && value.length > 0 && (
            <motion.div
              initial={{ opacity: 0, y: -10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-xl border border-okla-cream z-50"
            >
              {suggestions
                .filter((s) => s.toLowerCase().includes(value.toLowerCase()))
                .slice(0, 5)
                .map((suggestion, index) => (
                  <button
                    key={index}
                    onClick={() => {
                      onChange(suggestion);
                      setShowSuggestions(false);
                    }}
                    className="w-full px-4 py-3 text-left hover:bg-okla-cream transition-colors text-okla-charcoal first:rounded-t-xl last:rounded-b-xl"
                  >
                    <Search className="w-4 h-4 inline-block mr-3 text-okla-slate" />
                    {suggestion}
                  </button>
                ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Filters Panel */}
      <AnimatePresence>
        {isFiltersOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            className="mt-4 bg-white rounded-2xl shadow-lg border border-okla-cream overflow-hidden"
          >
            <div className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-lg font-semibold text-okla-navy">Filtros Avanzados</h3>
                {activeFilterCount > 0 && (
                  <button
                    onClick={onClearFilters}
                    className="text-sm text-okla-gold hover:underline flex items-center gap-1"
                  >
                    <X className="w-4 h-4" />
                    Limpiar filtros
                  </button>
                )}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {filterSections.map((section) => (
                  <FilterSectionComponent
                    key={section.id}
                    section={section}
                    value={localFilters[section.id]}
                    onChange={(v) => handleFilterChange(section.id, v)}
                  />
                ))}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// Filter Section Component
interface FilterSectionComponentProps {
  section: FilterSection;
  value: string | string[] | [number, number] | undefined;
  onChange: (value: string | string[] | [number, number]) => void;
}

const FilterSectionComponent = ({ section, value, onChange }: FilterSectionComponentProps) => {
  const [isOpen, setIsOpen] = useState(true);

  if (section.type === 'range') {
    const rangeValue = (value as [number, number]) || [section.min || 0, section.max || 100];
    return (
      <div>
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="flex items-center justify-between w-full text-left mb-3"
        >
          <span className="font-medium text-okla-navy">{section.label}</span>
          <ChevronDown className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
        </button>
        {isOpen && (
          <div className="space-y-4">
            <div className="flex gap-4">
              <div className="flex-1">
                <label className="text-xs text-okla-slate mb-1 block">Mínimo</label>
                <div className="relative">
                  {section.prefix && (
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-okla-slate">
                      {section.prefix}
                    </span>
                  )}
                  <input
                    type="number"
                    value={rangeValue[0]}
                    onChange={(e) => onChange([Number(e.target.value), rangeValue[1]])}
                    className={`w-full px-3 py-2 border border-okla-cream rounded-lg focus:outline-none focus:ring-2 focus:ring-okla-gold/50 ${section.prefix ? 'pl-7' : ''}`}
                    min={section.min}
                    max={rangeValue[1]}
                  />
                </div>
              </div>
              <div className="flex-1">
                <label className="text-xs text-okla-slate mb-1 block">Máximo</label>
                <div className="relative">
                  {section.prefix && (
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-okla-slate">
                      {section.prefix}
                    </span>
                  )}
                  <input
                    type="number"
                    value={rangeValue[1]}
                    onChange={(e) => onChange([rangeValue[0], Number(e.target.value)])}
                    className={`w-full px-3 py-2 border border-okla-cream rounded-lg focus:outline-none focus:ring-2 focus:ring-okla-gold/50 ${section.prefix ? 'pl-7' : ''}`}
                    min={rangeValue[0]}
                    max={section.max}
                  />
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  }

  if (section.type === 'checkbox') {
    const selectedValues = (value as string[]) || [];
    return (
      <div>
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="flex items-center justify-between w-full text-left mb-3"
        >
          <span className="font-medium text-okla-navy">{section.label}</span>
          <ChevronDown className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
        </button>
        {isOpen && (
          <div className="space-y-2 max-h-48 overflow-y-auto">
            {section.options?.map((option) => (
              <label key={option.id} className="flex items-center gap-3 cursor-pointer group">
                <input
                  type="checkbox"
                  checked={selectedValues.includes(option.id)}
                  onChange={(e) => {
                    if (e.target.checked) {
                      onChange([...selectedValues, option.id]);
                    } else {
                      onChange(selectedValues.filter((v) => v !== option.id));
                    }
                  }}
                  className="w-4 h-4 text-okla-gold border-okla-cream rounded focus:ring-okla-gold"
                />
                <span className="text-okla-charcoal group-hover:text-okla-navy transition-colors">
                  {option.label}
                </span>
                {option.count !== undefined && (
                  <span className="text-xs text-okla-slate ml-auto">({option.count})</span>
                )}
              </label>
            ))}
          </div>
        )}
      </div>
    );
  }

  if (section.type === 'select') {
    return (
      <div>
        <label className="font-medium text-okla-navy block mb-2">{section.label}</label>
        <select
          value={(value as string) || ''}
          onChange={(e) => onChange(e.target.value)}
          className="w-full px-4 py-3 border border-okla-cream rounded-xl focus:outline-none focus:ring-2 focus:ring-okla-gold/50 bg-white text-okla-charcoal"
        >
          <option value="">Todos</option>
          {section.options?.map((option) => (
            <option key={option.id} value={option.id}>
              {option.label}
            </option>
          ))}
        </select>
      </div>
    );
  }

  // Radio
  return (
    <div>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center justify-between w-full text-left mb-3"
      >
        <span className="font-medium text-okla-navy">{section.label}</span>
        <ChevronDown className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
      </button>
      {isOpen && (
        <div className="space-y-2">
          {section.options?.map((option) => (
            <label key={option.id} className="flex items-center gap-3 cursor-pointer group">
              <input
                type="radio"
                name={section.id}
                checked={(value as string) === option.id}
                onChange={() => onChange(option.id)}
                className="w-4 h-4 text-okla-gold border-okla-cream focus:ring-okla-gold"
              />
              <span className="text-okla-charcoal group-hover:text-okla-navy transition-colors">
                {option.label}
              </span>
            </label>
          ))}
        </div>
      )}
    </div>
  );
};

export default OklaSearchBar;
