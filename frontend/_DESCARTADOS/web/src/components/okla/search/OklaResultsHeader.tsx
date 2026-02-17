import { motion } from 'framer-motion';
import { Grid, List, ArrowUpDown, Check } from 'lucide-react';
import { useState } from 'react';

interface SortOption {
  id: string;
  label: string;
}

interface OklaResultsHeaderProps {
  totalResults: number;
  currentPage?: number;
  itemsPerPage?: number;
  viewMode: 'grid' | 'list';
  onViewModeChange: (mode: 'grid' | 'list') => void;
  sortBy: string;
  sortOptions: SortOption[];
  onSortChange: (sortId: string) => void;
  resultsLabel?: string;
}

export const OklaResultsHeader = ({
  totalResults,
  currentPage = 1,
  itemsPerPage = 12,
  viewMode,
  onViewModeChange,
  sortBy,
  sortOptions,
  onSortChange,
  resultsLabel = 'resultados',
}: OklaResultsHeaderProps) => {
  const [isSortOpen, setIsSortOpen] = useState(false);

  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, totalResults);

  const currentSort = sortOptions.find((opt) => opt.id === sortBy);

  return (
    <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-6">
      {/* Results Count */}
      <div className="text-okla-charcoal">
        <span className="text-okla-slate">Mostrando </span>
        <span className="font-semibold text-okla-navy">{startItem}-{endItem}</span>
        <span className="text-okla-slate"> de </span>
        <span className="font-semibold text-okla-navy">{totalResults.toLocaleString()}</span>
        <span className="text-okla-slate"> {resultsLabel}</span>
      </div>

      <div className="flex items-center gap-4">
        {/* Sort Dropdown */}
        <div className="relative">
          <button
            onClick={() => setIsSortOpen(!isSortOpen)}
            className="flex items-center gap-2 px-4 py-2 bg-white border border-okla-cream rounded-xl hover:border-okla-gold transition-colors"
          >
            <ArrowUpDown className="w-4 h-4 text-okla-slate" />
            <span className="text-okla-charcoal">{currentSort?.label || 'Ordenar'}</span>
          </button>

          {isSortOpen && (
            <>
              <div
                className="fixed inset-0 z-10"
                onClick={() => setIsSortOpen(false)}
              />
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                className="absolute right-0 top-full mt-2 w-56 bg-white rounded-xl shadow-xl border border-okla-cream z-20 overflow-hidden"
              >
                {sortOptions.map((option) => (
                  <button
                    key={option.id}
                    onClick={() => {
                      onSortChange(option.id);
                      setIsSortOpen(false);
                    }}
                    className={`w-full px-4 py-3 text-left flex items-center justify-between hover:bg-okla-cream transition-colors ${
                      sortBy === option.id ? 'bg-okla-gold/10' : ''
                    }`}
                  >
                    <span className={sortBy === option.id ? 'text-okla-gold font-medium' : 'text-okla-charcoal'}>
                      {option.label}
                    </span>
                    {sortBy === option.id && (
                      <Check className="w-4 h-4 text-okla-gold" />
                    )}
                  </button>
                ))}
              </motion.div>
            </>
          )}
        </div>

        {/* View Mode Toggle */}
        <div className="flex items-center bg-white border border-okla-cream rounded-xl overflow-hidden">
          <button
            onClick={() => onViewModeChange('grid')}
            className={`p-2.5 transition-colors ${
              viewMode === 'grid'
                ? 'bg-okla-navy text-white'
                : 'text-okla-slate hover:bg-okla-cream'
            }`}
            title="Vista de cuadrícula"
          >
            <Grid className="w-5 h-5" />
          </button>
          <button
            onClick={() => onViewModeChange('list')}
            className={`p-2.5 transition-colors ${
              viewMode === 'list'
                ? 'bg-okla-navy text-white'
                : 'text-okla-slate hover:bg-okla-cream'
            }`}
            title="Vista de lista"
          >
            <List className="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>
  );
};

export default OklaResultsHeader;
