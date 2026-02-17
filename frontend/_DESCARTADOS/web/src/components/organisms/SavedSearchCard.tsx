import { FiPlay, FiBell, FiBellOff, FiTrash2, FiClock, FiFilter, FiChevronRight } from 'react-icons/fi';
import { formatFilters, type SavedSearch } from '@/services/savedSearchService';

interface SavedSearchCardProps {
  search: SavedSearch;
  onRun: (search: SavedSearch) => void;
  onToggleNotifications: (id: string, enabled: boolean) => void;
  onDelete: (id: string) => void;
  isDeleting?: boolean;
  isToggling?: boolean;
  showPreview?: boolean;
  onTogglePreview?: () => void;
  previewContent?: React.ReactNode;
}

export default function SavedSearchCard({
  search,
  onRun,
  onToggleNotifications,
  onDelete,
  isDeleting = false,
  isToggling = false,
  showPreview = false,
  onTogglePreview,
  previewContent,
}: SavedSearchCardProps) {
  const handleDelete = () => {
    if (confirm('¿Eliminar esta búsqueda guardada?')) {
      onDelete(search.id);
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow">
      <div className="p-4 sm:p-5">
        <div className="flex items-start gap-4">
          {/* Main Content */}
          <div className="flex-1 min-w-0">
            {/* Title Row */}
            <div className="flex items-center gap-2 mb-2">
              <h3 className="font-semibold text-gray-900 truncate">
                {search.name}
              </h3>
              {search.notificationsEnabled && (
                <span className="flex-shrink-0 inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                  <FiBell className="h-3 w-3" />
                  Alertas
                </span>
              )}
              {search.resultsCount > 0 && (
                <span className="flex-shrink-0 px-2 py-0.5 bg-primary/10 text-primary text-xs rounded-full font-medium">
                  {search.resultsCount} resultado{search.resultsCount !== 1 ? 's' : ''}
                </span>
              )}
            </div>

            {/* Filters */}
            <div className="flex items-center gap-2 mb-3">
              <FiFilter className="h-4 w-4 text-gray-400 flex-shrink-0" />
              <p className="text-sm text-gray-600 truncate">
                {formatFilters(search.filters)}
              </p>
            </div>

            {/* Metadata */}
            <div className="flex items-center gap-4 text-xs text-gray-500">
              <span className="flex items-center gap-1">
                <FiClock className="h-3 w-3" />
                {new Date(search.createdAt).toLocaleDateString('es-ES', {
                  day: 'numeric',
                  month: 'short',
                  year: 'numeric',
                })}
              </span>
              {search.lastChecked && (
                <span>
                  Verificado: {new Date(search.lastChecked).toLocaleDateString('es-ES', {
                    day: 'numeric',
                    month: 'short',
                  })}
                </span>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-1 flex-shrink-0">
            <button
              onClick={() => onRun(search)}
              className="p-2 text-primary hover:bg-primary/10 rounded-lg transition-colors"
              title="Ejecutar búsqueda"
            >
              <FiPlay className="h-5 w-5" />
            </button>
            
            <button
              onClick={() => onToggleNotifications(search.id, !search.notificationsEnabled)}
              disabled={isToggling}
              className={`p-2 rounded-lg transition-colors ${
                search.notificationsEnabled
                  ? 'text-green-600 hover:bg-green-50'
                  : 'text-gray-400 hover:bg-gray-100'
              }`}
              title={search.notificationsEnabled ? 'Desactivar alertas' : 'Activar alertas'}
            >
              {search.notificationsEnabled ? (
                <FiBell className="h-5 w-5" />
              ) : (
                <FiBellOff className="h-5 w-5" />
              )}
            </button>
            
            <button
              onClick={handleDelete}
              disabled={isDeleting}
              className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
              title="Eliminar"
            >
              <FiTrash2 className="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>

      {/* Preview Toggle */}
      {onTogglePreview && (
        <>
          <button
            onClick={onTogglePreview}
            className="w-full py-2.5 text-sm text-gray-500 hover:bg-gray-50 border-t border-gray-100 transition-colors flex items-center justify-center gap-1"
          >
            {showPreview ? 'Ocultar vista previa' : 'Ver vista previa'}
            <FiChevronRight className={`h-4 w-4 transition-transform ${showPreview ? 'rotate-90' : ''}`} />
          </button>

          {/* Preview Content */}
          {showPreview && previewContent && (
            <div className="px-5 pb-5 pt-2 border-t border-gray-100 bg-gray-50">
              {previewContent}
            </div>
          )}
        </>
      )}
    </div>
  );
}
