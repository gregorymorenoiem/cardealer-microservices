import { motion, AnimatePresence } from 'framer-motion';
import { 
  X, 
  ChevronDown, 
  ChevronUp,
  RotateCcw
} from 'lucide-react';
import { useState } from 'react';
import { OklaButton } from '../../atoms/okla/OklaButton';

export interface FilterOption {
  id: string;
  label: string;
  count?: number;
}

export interface FilterSection {
  id: string;
  label: string;
  type: 'checkbox' | 'radio' | 'range' | 'select' | 'color' | 'buttons';
  options?: FilterOption[];
  min?: number;
  max?: number;
  step?: number;
  prefix?: string;
  suffix?: string;
  defaultOpen?: boolean;
}

interface OklaFilterSidebarProps {
  sections: FilterSection[];
  activeFilters: Record<string, string | string[] | [number, number]>;
  onFilterChange: (sectionId: string, value: string | string[] | [number, number]) => void;
  onClearFilters: () => void;
  onApply?: () => void;
  title?: string;
  showApplyButton?: boolean;
  isOpen?: boolean;
  onClose?: () => void;
  variant?: 'sidebar' | 'modal';
}

export const OklaFilterSidebar = ({
  sections,
  activeFilters,
  onFilterChange,
  onClearFilters,
  onApply,
  title = 'Filtros',
  showApplyButton = false,
  isOpen = true,
  onClose,
  variant = 'sidebar',
}: OklaFilterSidebarProps) => {
  const activeFilterCount = Object.values(activeFilters).filter((v) => {
    if (Array.isArray(v)) {
      if (typeof v[0] === 'number') return true; // range always counts
      return v.length > 0;
    }
    return v !== '' && v !== undefined;
  }).length;

  const content = (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between p-6 border-b border-okla-cream">
        <div className="flex items-center gap-3">
          <h2 className="text-xl font-playfair font-bold text-okla-navy">{title}</h2>
          {activeFilterCount > 0 && (
            <span className="px-2 py-1 bg-okla-gold text-white text-xs rounded-full font-medium">
              {activeFilterCount}
            </span>
          )}
        </div>
        <div className="flex items-center gap-2">
          {activeFilterCount > 0 && (
            <button
              onClick={onClearFilters}
              className="text-sm text-okla-slate hover:text-okla-navy flex items-center gap-1 transition-colors"
            >
              <RotateCcw className="w-4 h-4" />
              Limpiar
            </button>
          )}
          {variant === 'modal' && onClose && (
            <button
              onClick={onClose}
              className="p-2 hover:bg-okla-cream rounded-lg transition-colors"
            >
              <X className="w-5 h-5 text-okla-charcoal" />
            </button>
          )}
        </div>
      </div>

      {/* Filter Sections */}
      <div className="flex-1 overflow-y-auto p-6 space-y-6">
        {sections.map((section) => (
          <FilterSection
            key={section.id}
            section={section}
            value={activeFilters[section.id]}
            onChange={(value) => onFilterChange(section.id, value)}
          />
        ))}
      </div>

      {/* Apply Button */}
      {showApplyButton && (
        <div className="p-6 border-t border-okla-cream">
          <OklaButton variant="primary" className="w-full" onClick={onApply}>
            Aplicar Filtros
          </OklaButton>
        </div>
      )}
    </div>
  );

  if (variant === 'modal') {
    return (
      <AnimatePresence>
        {isOpen && (
          <>
            {/* Backdrop */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 bg-black/50 z-40"
              onClick={onClose}
            />
            {/* Modal */}
            <motion.div
              initial={{ x: '-100%' }}
              animate={{ x: 0 }}
              exit={{ x: '-100%' }}
              transition={{ type: 'spring', damping: 25, stiffness: 200 }}
              className="fixed left-0 top-0 bottom-0 w-full max-w-md bg-white z-50 shadow-2xl"
            >
              {content}
            </motion.div>
          </>
        )}
      </AnimatePresence>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-okla-cream overflow-hidden">
      {content}
    </div>
  );
};

// Individual Filter Section
interface FilterSectionProps {
  section: FilterSection;
  value: string | string[] | [number, number] | undefined;
  onChange: (value: string | string[] | [number, number]) => void;
}

const FilterSection = ({ section, value, onChange }: FilterSectionProps) => {
  const [isOpen, setIsOpen] = useState(section.defaultOpen !== false);

  const hasValue = () => {
    if (!value) return false;
    if (Array.isArray(value)) {
      if (typeof value[0] === 'number') return true;
      return value.length > 0;
    }
    return value !== '';
  };

  return (
    <div className="border-b border-okla-cream pb-6 last:border-0 last:pb-0">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center justify-between w-full text-left mb-4 group"
      >
        <div className="flex items-center gap-2">
          <span className="font-semibold text-okla-navy group-hover:text-okla-gold transition-colors">
            {section.label}
          </span>
          {hasValue() && (
            <span className="w-2 h-2 rounded-full bg-okla-gold" />
          )}
        </div>
        {isOpen ? (
          <ChevronUp className="w-5 h-5 text-okla-slate" />
        ) : (
          <ChevronDown className="w-5 h-5 text-okla-slate" />
        )}
      </button>

      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.2 }}
          >
            {section.type === 'range' && (
              <RangeFilter
                section={section}
                value={value as [number, number]}
                onChange={onChange}
              />
            )}
            {section.type === 'checkbox' && (
              <CheckboxFilter
                section={section}
                value={value as string[]}
                onChange={onChange}
              />
            )}
            {section.type === 'radio' && (
              <RadioFilter
                section={section}
                value={value as string}
                onChange={onChange}
              />
            )}
            {section.type === 'select' && (
              <SelectFilter
                section={section}
                value={value as string}
                onChange={onChange}
              />
            )}
            {section.type === 'buttons' && (
              <ButtonsFilter
                section={section}
                value={value as string}
                onChange={onChange}
              />
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// Range Filter
const RangeFilter = ({
  section,
  value,
  onChange,
}: {
  section: FilterSection;
  value: [number, number] | undefined;
  onChange: (value: [number, number]) => void;
}) => {
  const min = section.min || 0;
  const max = section.max || 1000000;
  const rangeValue = value || [min, max];

  return (
    <div className="space-y-4">
      <div className="flex gap-4">
        <div className="flex-1">
          <label className="text-xs text-okla-slate mb-1 block">Desde</label>
          <div className="relative">
            {section.prefix && (
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-okla-slate text-sm">
                {section.prefix}
              </span>
            )}
            <input
              type="number"
              value={rangeValue[0]}
              onChange={(e) => onChange([Number(e.target.value), rangeValue[1]])}
              className={`w-full px-3 py-2.5 border border-okla-cream rounded-xl focus:outline-none focus:ring-2 focus:ring-okla-gold/50 text-okla-charcoal ${
                section.prefix ? 'pl-8' : ''
              }`}
              min={min}
              max={rangeValue[1]}
            />
          </div>
        </div>
        <div className="flex-1">
          <label className="text-xs text-okla-slate mb-1 block">Hasta</label>
          <div className="relative">
            {section.prefix && (
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-okla-slate text-sm">
                {section.prefix}
              </span>
            )}
            <input
              type="number"
              value={rangeValue[1]}
              onChange={(e) => onChange([rangeValue[0], Number(e.target.value)])}
              className={`w-full px-3 py-2.5 border border-okla-cream rounded-xl focus:outline-none focus:ring-2 focus:ring-okla-gold/50 text-okla-charcoal ${
                section.prefix ? 'pl-8' : ''
              }`}
              min={rangeValue[0]}
              max={max}
            />
          </div>
        </div>
      </div>
      {/* Range Slider Visual */}
      <div className="relative h-2 bg-okla-cream rounded-full">
        <div
          className="absolute h-full bg-okla-gold rounded-full"
          style={{
            left: `${((rangeValue[0] - min) / (max - min)) * 100}%`,
            right: `${100 - ((rangeValue[1] - min) / (max - min)) * 100}%`,
          }}
        />
      </div>
    </div>
  );
};

// Checkbox Filter
const CheckboxFilter = ({
  section,
  value,
  onChange,
}: {
  section: FilterSection;
  value: string[] | undefined;
  onChange: (value: string[]) => void;
}) => {
  const selectedValues = value || [];
  const [showAll, setShowAll] = useState(false);
  const displayOptions = showAll ? section.options : section.options?.slice(0, 5);

  return (
    <div className="space-y-3">
      {displayOptions?.map((option) => (
        <label
          key={option.id}
          className="flex items-center gap-3 cursor-pointer group"
        >
          <motion.div
            className={`w-5 h-5 rounded border-2 flex items-center justify-center transition-colors ${
              selectedValues.includes(option.id)
                ? 'bg-okla-gold border-okla-gold'
                : 'border-okla-slate/30 group-hover:border-okla-gold'
            }`}
            whileTap={{ scale: 0.9 }}
          >
            {selectedValues.includes(option.id) && (
              <motion.svg
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="w-3 h-3 text-white"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={3}
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </motion.svg>
            )}
          </motion.div>
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
            className="sr-only"
          />
          <span className="flex-1 text-okla-charcoal group-hover:text-okla-navy transition-colors">
            {option.label}
          </span>
          {option.count !== undefined && (
            <span className="text-sm text-okla-slate">({option.count})</span>
          )}
        </label>
      ))}
      {section.options && section.options.length > 5 && (
        <button
          onClick={() => setShowAll(!showAll)}
          className="text-sm text-okla-gold hover:underline mt-2"
        >
          {showAll ? 'Ver menos' : `Ver todos (${section.options.length})`}
        </button>
      )}
    </div>
  );
};

// Radio Filter
const RadioFilter = ({
  section,
  value,
  onChange,
}: {
  section: FilterSection;
  value: string | undefined;
  onChange: (value: string) => void;
}) => {
  return (
    <div className="space-y-3">
      {section.options?.map((option) => (
        <label
          key={option.id}
          className="flex items-center gap-3 cursor-pointer group"
        >
          <motion.div
            className={`w-5 h-5 rounded-full border-2 flex items-center justify-center transition-colors ${
              value === option.id
                ? 'border-okla-gold'
                : 'border-okla-slate/30 group-hover:border-okla-gold'
            }`}
            whileTap={{ scale: 0.9 }}
          >
            {value === option.id && (
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                className="w-2.5 h-2.5 rounded-full bg-okla-gold"
              />
            )}
          </motion.div>
          <input
            type="radio"
            name={section.id}
            checked={value === option.id}
            onChange={() => onChange(option.id)}
            className="sr-only"
          />
          <span className="text-okla-charcoal group-hover:text-okla-navy transition-colors">
            {option.label}
          </span>
        </label>
      ))}
    </div>
  );
};

// Select Filter
const SelectFilter = ({
  section,
  value,
  onChange,
}: {
  section: FilterSection;
  value: string | undefined;
  onChange: (value: string) => void;
}) => {
  return (
    <select
      value={value || ''}
      onChange={(e) => onChange(e.target.value)}
      className="w-full px-4 py-3 border border-okla-cream rounded-xl focus:outline-none focus:ring-2 focus:ring-okla-gold/50 bg-white text-okla-charcoal appearance-none bg-[url('data:image/svg+xml;charset=US-ASCII,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%2224%22%20height%3D%2224%22%20viewBox%3D%220%200%2024%2024%22%20fill%3D%22none%22%20stroke%3D%22%2364748b%22%20stroke-width%3D%222%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%3E%3Cpolyline%20points%3D%226%209%2012%2015%2018%209%22%3E%3C%2Fpolyline%3E%3C%2Fsvg%3E')] bg-no-repeat bg-[right_1rem_center] bg-[length:1rem]"
    >
      <option value="">Todos</option>
      {section.options?.map((option) => (
        <option key={option.id} value={option.id}>
          {option.label}
        </option>
      ))}
    </select>
  );
};

// Buttons Filter
const ButtonsFilter = ({
  section,
  value,
  onChange,
}: {
  section: FilterSection;
  value: string | undefined;
  onChange: (value: string) => void;
}) => {
  return (
    <div className="flex flex-wrap gap-2">
      {section.options?.map((option) => (
        <motion.button
          key={option.id}
          onClick={() => onChange(value === option.id ? '' : option.id)}
          className={`px-4 py-2 rounded-xl text-sm font-medium transition-colors ${
            value === option.id
              ? 'bg-okla-gold text-white'
              : 'bg-okla-cream text-okla-charcoal hover:bg-okla-navy hover:text-white'
          }`}
          whileTap={{ scale: 0.95 }}
        >
          {option.label}
        </motion.button>
      ))}
    </div>
  );
};

export default OklaFilterSidebar;
