import { motion } from 'framer-motion';
import { X } from 'lucide-react';

interface ActiveFilter {
  id: string;
  sectionId: string;
  label: string;
  value: string;
}

interface OklaActiveFiltersProps {
  filters: ActiveFilter[];
  onRemove: (sectionId: string, value: string) => void;
  onClearAll: () => void;
}

export const OklaActiveFilters = ({
  filters,
  onRemove,
  onClearAll,
}: OklaActiveFiltersProps) => {
  if (filters.length === 0) return null;

  return (
    <div className="flex flex-wrap items-center gap-2 mb-6">
      <span className="text-sm text-okla-slate font-medium">Filtros activos:</span>
      
      {filters.map((filter) => (
        <motion.button
          key={filter.id}
          onClick={() => onRemove(filter.sectionId, filter.value)}
          className="inline-flex items-center gap-2 px-3 py-1.5 bg-okla-navy/10 text-okla-navy rounded-full text-sm hover:bg-okla-navy hover:text-white transition-colors group"
          initial={{ scale: 0, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0, opacity: 0 }}
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.98 }}
        >
          <span className="text-okla-slate text-xs group-hover:text-white/70">
            {filter.label}:
          </span>
          <span className="font-medium">{filter.value}</span>
          <X className="w-3.5 h-3.5 opacity-60 group-hover:opacity-100" />
        </motion.button>
      ))}

      {filters.length > 1 && (
        <motion.button
          onClick={onClearAll}
          className="text-sm text-okla-gold hover:underline ml-2"
          whileTap={{ scale: 0.95 }}
        >
          Limpiar todos
        </motion.button>
      )}
    </div>
  );
};

export default OklaActiveFilters;
