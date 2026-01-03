import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import { 
  useSavedSearchesPage, 
  useSavedSearchResults,
  useRecentSearches
} from '@/hooks/useSearch';
import { formatFilters } from '@/services/savedSearchService';
import { 
  FiSearch, 
  FiTrash2, 
  FiBell, 
  FiBellOff, 
  FiPlay, 
  FiClock,
  FiFilter,
  FiPlus,
  FiAlertCircle,
  FiCheckCircle
} from 'react-icons/fi';
import type { SavedSearch } from '@/services/savedSearchService';

export default function SavedSearchesPage() {
  const navigate = useNavigate();
  const [selectedSearch, setSelectedSearch] = useState<string | null>(null);

  const { 
    searches: savedSearches, 
    isLoading, 
    isError, 
    deleteSearch, 
    toggleNotifications,
    isDeleting 
  } = useSavedSearchesPage();
  const { data: recentSearches = [] } = useRecentSearches();
  const { data: results, isLoading: resultsLoading } = useSavedSearchResults(selectedSearch || '');

  const handleRunSearch = (search: SavedSearch) => {
    // Navigate to browse page with saved filters
    const params = new URLSearchParams();
    if (search.filters) {
      Object.entries(search.filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.set(key, String(value));
        }
      });
    }
    navigate(`/vehicles?${params.toString()}`);
  };

  const handleDelete = (id: string) => {
    if (confirm('¿Eliminar esta búsqueda guardada?')) {
      deleteSearch(id);
    }
  };

  const handleToggleNotifications = (id: string, currentState: boolean) => {
    toggleNotifications(id, !currentState);
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-4xl mx-auto px-4 py-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/3"></div>
            <div className="h-4 bg-gray-200 rounded w-1/2"></div>
            <div className="space-y-3 mt-8">
              {[1, 2, 3].map((i) => (
                <div key={i} className="h-24 bg-gray-200 rounded-xl"></div>
              ))}
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Búsquedas Guardadas
          </h1>
          <p className="text-gray-600">
            Gestiona tus búsquedas guardadas y configura alertas de nuevos resultados
          </p>
        </div>

        {isError && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3 text-red-700">
            <FiAlertCircle className="h-5 w-5" />
            <span>Error al cargar las búsquedas guardadas</span>
          </div>
        )}

        {/* Saved Searches List */}
        <div className="space-y-4 mb-12">
          <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
            <FiSearch className="h-5 w-5" />
            Mis búsquedas ({savedSearches.length})
          </h2>

          {savedSearches.length === 0 ? (
            <div className="bg-gray-50 rounded-xl p-8 text-center">
              <FiSearch className="h-12 w-12 mx-auto text-gray-400 mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                No tienes búsquedas guardadas
              </h3>
              <p className="text-gray-600 mb-4">
                Guarda tus búsquedas favoritas para acceder rápidamente a ellas
              </p>
              <button
                onClick={() => navigate('/vehicles')}
                className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors"
              >
                <FiPlus className="h-4 w-4" />
                Buscar vehículos
              </button>
            </div>
          ) : (
            <div className="grid gap-4">
              {savedSearches.map((search) => (
                <div
                  key={search.id}
                  className={`bg-white rounded-xl shadow-sm border transition-all duration-200 ${
                    selectedSearch === search.id 
                      ? 'border-primary ring-2 ring-primary/20' 
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <div className="p-4">
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <h3 className="font-semibold text-gray-900">
                            {search.name}
                          </h3>
                          {search.notificationsEnabled && (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                              <FiCheckCircle className="h-3 w-3" />
                              Alertas activas
                            </span>
                          )}
                          {search.resultsCount > 0 && (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-primary text-white text-xs rounded-full">
                              {search.resultsCount} resultados
                            </span>
                          )}
                        </div>

                        {/* Filters Display */}
                        <div className="flex flex-wrap gap-2 mb-3">
                          <span className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-700 text-sm rounded-lg">
                            <FiFilter className="h-3 w-3" />
                            {formatFilters(search.filters)}
                          </span>
                        </div>

                        {/* Metadata */}
                        <div className="flex items-center gap-4 text-sm text-gray-500">
                          <span className="flex items-center gap-1">
                            <FiClock className="h-4 w-4" />
                            Creada: {new Date(search.createdAt).toLocaleDateString()}
                          </span>
                          {search.lastChecked && (
                            <span>
                              Última verificación: {new Date(search.lastChecked).toLocaleDateString()}
                            </span>
                          )}
                        </div>
                      </div>

                      {/* Actions */}
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleRunSearch(search)}
                          className="p-2 text-primary hover:bg-primary/10 rounded-lg transition-colors"
                          title="Ejecutar búsqueda"
                        >
                          <FiPlay className="h-5 w-5" />
                        </button>
                        <button
                          onClick={() => handleToggleNotifications(search.id, search.notificationsEnabled)}
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
                          onClick={() => handleDelete(search.id)}
                          className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                          title="Eliminar"
                          disabled={isDeleting}
                        >
                          <FiTrash2 className="h-5 w-5" />
                        </button>
                      </div>
                    </div>

                    {/* Preview Results */}
                    {selectedSearch === search.id && results && (
                      <div className="mt-4 pt-4 border-t border-gray-100">
                        <p className="text-sm text-gray-600 mb-2">
                          {results.total} vehículos encontrados
                        </p>
                        {resultsLoading ? (
                          <div className="flex gap-2">
                            {[1, 2, 3].map((i) => (
                              <div key={i} className="w-20 h-14 bg-gray-200 rounded animate-pulse"></div>
                            ))}
                          </div>
                        ) : results.vehicles.slice(0, 4).length > 0 ? (
                          <div className="flex gap-2 overflow-x-auto">
                            {results.vehicles.slice(0, 4).map((vehicle) => (
                              <button
                                key={vehicle.id}
                                onClick={() => navigate(`/vehicles/${vehicle.id}`)}
                                className="flex-shrink-0 w-24 group"
                              >
                                <div className="relative">
                                  <img
                                    src={vehicle.images[0] || '/placeholder-vehicle.jpg'}
                                    alt={`${vehicle.make} ${vehicle.model}`}
                                    className="w-24 h-16 object-cover rounded-lg group-hover:ring-2 ring-primary transition-all"
                                  />
                                </div>
                                <p className="text-xs text-gray-600 mt-1 truncate">
                                  {vehicle.make} {vehicle.model}
                                </p>
                              </button>
                            ))}
                          </div>
                        ) : null}
                      </div>
                    )}
                  </div>

                  {/* Expand/Collapse */}
                  <button
                    onClick={() => setSelectedSearch(selectedSearch === search.id ? null : search.id)}
                    className="w-full py-2 text-sm text-gray-500 hover:bg-gray-50 border-t border-gray-100 transition-colors"
                  >
                    {selectedSearch === search.id ? 'Ocultar vista previa' : 'Ver vista previa'}
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Recent Searches Section */}
        {recentSearches.length > 0 && (
          <div className="space-y-4">
            <h2 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
              <FiClock className="h-5 w-5" />
              Búsquedas recientes
            </h2>

            <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
              <div className="flex flex-wrap gap-2">
                {recentSearches.slice(0, 10).map((recent, index) => (
                  <button
                    key={index}
                    onClick={() => navigate(`/vehicles?search=${encodeURIComponent(recent.query)}`)}
                    className="inline-flex items-center gap-2 px-3 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg transition-colors"
                  >
                    <FiClock className="h-4 w-4" />
                    {recent.query}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
