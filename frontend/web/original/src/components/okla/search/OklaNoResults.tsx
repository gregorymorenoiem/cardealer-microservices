import { motion } from 'framer-motion';
import { SearchX, ArrowRight } from 'lucide-react';
import { OklaButton } from '../../atoms/okla/OklaButton';

interface OklaNoResultsProps {
  searchQuery?: string;
  onClearFilters?: () => void;
  onBrowseAll?: () => void;
}

export const OklaNoResults = ({
  searchQuery,
  onClearFilters,
  onBrowseAll,
}: OklaNoResultsProps) => {
  return (
    <motion.div
      className="flex flex-col items-center justify-center py-16 px-4 text-center"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
    >
      {/* Icon */}
      <motion.div
        className="w-24 h-24 bg-okla-navy/5 rounded-full flex items-center justify-center mb-6"
        initial={{ scale: 0 }}
        animate={{ scale: 1 }}
        transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
      >
        <SearchX className="w-10 h-10 text-okla-slate" />
      </motion.div>

      {/* Title */}
      <motion.h3
        className="text-2xl font-display font-bold text-okla-navy mb-3"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.3 }}
      >
        No encontramos resultados
      </motion.h3>

      {/* Description */}
      <motion.p
        className="text-okla-slate max-w-md mb-8"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.4 }}
      >
        {searchQuery ? (
          <>
            No hay publicaciones que coincidan con "<span className="font-semibold text-okla-navy">{searchQuery}</span>".
            Intenta con otros términos o ajusta los filtros.
          </>
        ) : (
          'No hay publicaciones que coincidan con los filtros seleccionados. Intenta ajustar los criterios de búsqueda.'
        )}
      </motion.p>

      {/* Suggestions */}
      <motion.div
        className="bg-okla-cream rounded-xl p-6 max-w-md w-full mb-8"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.5 }}
      >
        <h4 className="font-semibold text-okla-navy mb-4">Sugerencias:</h4>
        <ul className="text-left text-sm text-okla-slate space-y-2">
          <li className="flex items-start gap-2">
            <span className="text-okla-gold mt-0.5">•</span>
            Revisa la ortografía de las palabras
          </li>
          <li className="flex items-start gap-2">
            <span className="text-okla-gold mt-0.5">•</span>
            Usa términos más generales
          </li>
          <li className="flex items-start gap-2">
            <span className="text-okla-gold mt-0.5">•</span>
            Reduce la cantidad de filtros aplicados
          </li>
          <li className="flex items-start gap-2">
            <span className="text-okla-gold mt-0.5">•</span>
            Amplía el rango de precios
          </li>
        </ul>
      </motion.div>

      {/* Actions */}
      <motion.div
        className="flex flex-col sm:flex-row gap-3"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.6 }}
      >
        {onClearFilters && (
          <OklaButton variant="outline" onClick={onClearFilters}>
            Limpiar filtros
          </OklaButton>
        )}
        {onBrowseAll && (
          <OklaButton onClick={onBrowseAll}>
            Ver todas las publicaciones
            <ArrowRight className="w-4 h-4 ml-2" />
          </OklaButton>
        )}
      </motion.div>
    </motion.div>
  );
};

export default OklaNoResults;
