import { motion } from 'framer-motion';
import { ChevronLeft, ChevronRight, MoreHorizontal } from 'lucide-react';

interface OklaPaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  showPageNumbers?: boolean;
  maxVisiblePages?: number;
  variant?: 'default' | 'simple' | 'compact';
}

export const OklaPagination = ({
  currentPage,
  totalPages,
  onPageChange,
  showPageNumbers = true,
  maxVisiblePages = 5,
  variant = 'default',
}: OklaPaginationProps) => {
  const getVisiblePages = () => {
    const pages: (number | 'ellipsis')[] = [];
    
    if (totalPages <= maxVisiblePages) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }

    // Always show first page
    pages.push(1);

    // Calculate start and end of visible range
    let start = Math.max(2, currentPage - 1);
    let end = Math.min(totalPages - 1, currentPage + 1);

    // Adjust if at the beginning
    if (currentPage <= 3) {
      start = 2;
      end = Math.min(maxVisiblePages - 1, totalPages - 1);
    }

    // Adjust if at the end
    if (currentPage >= totalPages - 2) {
      start = Math.max(2, totalPages - maxVisiblePages + 2);
      end = totalPages - 1;
    }

    // Add ellipsis after first page if needed
    if (start > 2) {
      pages.push('ellipsis');
    }

    // Add middle pages
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    // Add ellipsis before last page if needed
    if (end < totalPages - 1) {
      pages.push('ellipsis');
    }

    // Always show last page
    if (totalPages > 1) {
      pages.push(totalPages);
    }

    return pages;
  };

  if (totalPages <= 1) return null;

  if (variant === 'simple') {
    return (
      <div className="flex items-center justify-center gap-4">
        <motion.button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className="flex items-center gap-2 px-4 py-2 text-okla-charcoal hover:text-okla-gold disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          whileTap={{ scale: 0.95 }}
        >
          <ChevronLeft className="w-5 h-5" />
          Anterior
        </motion.button>
        <span className="text-okla-slate">
          Página <span className="font-semibold text-okla-navy">{currentPage}</span> de{' '}
          <span className="font-semibold text-okla-navy">{totalPages}</span>
        </span>
        <motion.button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          className="flex items-center gap-2 px-4 py-2 text-okla-charcoal hover:text-okla-gold disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          whileTap={{ scale: 0.95 }}
        >
          Siguiente
          <ChevronRight className="w-5 h-5" />
        </motion.button>
      </div>
    );
  }

  if (variant === 'compact') {
    return (
      <div className="flex items-center justify-center gap-2">
        <motion.button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className="p-2 text-okla-charcoal hover:bg-okla-cream rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          whileTap={{ scale: 0.95 }}
        >
          <ChevronLeft className="w-5 h-5" />
        </motion.button>
        <span className="px-4 py-2 bg-okla-cream rounded-lg text-sm font-medium text-okla-navy">
          {currentPage} / {totalPages}
        </span>
        <motion.button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          className="p-2 text-okla-charcoal hover:bg-okla-cream rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          whileTap={{ scale: 0.95 }}
        >
          <ChevronRight className="w-5 h-5" />
        </motion.button>
      </div>
    );
  }

  // Default variant
  const visiblePages = getVisiblePages();

  return (
    <nav className="flex items-center justify-center gap-2">
      {/* Previous Button */}
      <motion.button
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage === 1}
        className="flex items-center gap-1 px-4 py-2 text-okla-charcoal hover:bg-okla-cream rounded-xl disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        whileTap={{ scale: 0.95 }}
      >
        <ChevronLeft className="w-5 h-5" />
        <span className="hidden sm:inline">Anterior</span>
      </motion.button>

      {/* Page Numbers */}
      {showPageNumbers && (
        <div className="flex items-center gap-1">
          {visiblePages.map((page, index) =>
            page === 'ellipsis' ? (
              <span
                key={`ellipsis-${index}`}
                className="w-10 h-10 flex items-center justify-center text-okla-slate"
              >
                <MoreHorizontal className="w-5 h-5" />
              </span>
            ) : (
              <motion.button
                key={page}
                onClick={() => onPageChange(page)}
                className={`w-10 h-10 rounded-xl font-medium transition-all ${
                  currentPage === page
                    ? 'bg-okla-gold text-white shadow-md'
                    : 'text-okla-charcoal hover:bg-okla-cream'
                }`}
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
              >
                {page}
              </motion.button>
            )
          )}
        </div>
      )}

      {/* Next Button */}
      <motion.button
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage === totalPages}
        className="flex items-center gap-1 px-4 py-2 text-okla-charcoal hover:bg-okla-cream rounded-xl disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        whileTap={{ scale: 0.95 }}
      >
        <span className="hidden sm:inline">Siguiente</span>
        <ChevronRight className="w-5 h-5" />
      </motion.button>
    </nav>
  );
};

export default OklaPagination;
