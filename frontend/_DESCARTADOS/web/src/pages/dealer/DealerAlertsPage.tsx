import React, { useState, useEffect } from 'react';
import {
  FiBell,
  FiSearch,
  FiDollarSign,
  FiToggleLeft,
  FiToggleRight,
  FiTrash2,
  FiTrendingDown,
  FiCheckCircle,
  FiClock,
  FiMail,
  FiFilter,
  FiExternalLink,
} from 'react-icons/fi';
import {
  getPriceAlerts,
  getSavedSearches,
  activatePriceAlert,
  deactivatePriceAlert,
  deletePriceAlert,
  activateSavedSearch,
  deactivateSavedSearch,
  deleteSavedSearch,
  formatCriteriaForDisplay,
  getFrequencyLabel,
  getConditionLabel,
  type PriceAlert,
  type SavedSearch,
} from '../../services/alertsService';
import api from '../../services/api';

const DealerAlertsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'price' | 'searches'>('price');
  const [priceAlerts, setPriceAlerts] = useState<PriceAlert[]>([]);
  const [savedSearches, setSavedSearches] = useState<SavedSearch[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [vehicleInfo, setVehicleInfo] = useState<
    Record<string, { title: string; price: number; imageUrl?: string }>
  >({});

  const loadVehicleInfo = async (vehicleIds: string[]) => {
    const info: Record<string, { title: string; price: number; imageUrl?: string }> = {};

    // Fetch vehicle info for each ID (in parallel)
    await Promise.all(
      vehicleIds.map(async (id) => {
        try {
          const response = await api.get(`/api/vehicles/${id}`);
          if (response.data) {
            info[id] = {
              title: response.data.title || 'Vehículo',
              price: response.data.price || 0,
              imageUrl: response.data.images?.[0]?.url || response.data.imageUrl,
            };
          }
        } catch {
          info[id] = { title: 'Vehículo no disponible', price: 0 };
        }
      })
    );

    setVehicleInfo(info);
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      setError(null);
      try {
        const [alertsData, searchesData] = await Promise.all([
          getPriceAlerts(),
          getSavedSearches(),
        ]);
        setPriceAlerts(alertsData);
        setSavedSearches(searchesData);

        // Load vehicle info for price alerts
        const vehicleIds = alertsData.map((a) => a.vehicleId);
        if (vehicleIds.length > 0) {
          await loadVehicleInfo(vehicleIds);
        }
      } catch (err) {
        console.error('Error loading alerts data:', err);
        setError('No se pudieron cargar las alertas. Intenta nuevamente.');
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const refreshPriceAlerts = async () => {
    try {
      const updated = await getPriceAlerts();
      setPriceAlerts(updated);
    } catch (err) {
      console.error('Error refreshing price alerts:', err);
    }
  };

  const refreshSavedSearches = async () => {
    try {
      const updated = await getSavedSearches();
      setSavedSearches(updated);
    } catch (err) {
      console.error('Error refreshing saved searches:', err);
    }
  };

  const handleTogglePriceAlert = async (alert: PriceAlert) => {
    try {
      if (alert.isActive) {
        await deactivatePriceAlert(alert.id);
      } else {
        await activatePriceAlert(alert.id);
      }
      // Reload alerts
      const updated = await getPriceAlerts();
      setPriceAlerts(updated);
    } catch (err) {
      console.error('Error toggling price alert:', err);
    }
  };

  const handleDeletePriceAlert = async (id: string) => {
    if (!confirm('¿Estás seguro de eliminar esta alerta de precio?')) return;

    try {
      await deletePriceAlert(id);
      setPriceAlerts((prev) => prev.filter((a) => a.id !== id));
    } catch (err) {
      console.error('Error deleting price alert:', err);
    }
  };

  const handleToggleSavedSearch = async (search: SavedSearch) => {
    try {
      if (search.isActive) {
        await deactivateSavedSearch(search.id);
      } else {
        await activateSavedSearch(search.id);
      }
      // Reload searches
      const updated = await getSavedSearches();
      setSavedSearches(updated);
    } catch (err) {
      console.error('Error toggling saved search:', err);
    }
  };

  const handleDeleteSavedSearch = async (id: string) => {
    if (!confirm('¿Estás seguro de eliminar esta búsqueda guardada?')) return;

    try {
      await deleteSavedSearch(id);
      setSavedSearches((prev) => prev.filter((s) => s.id !== id));
    } catch (err) {
      console.error('Error deleting saved search:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatPrice = (price: number) => {
    return `RD$${price.toLocaleString('es-DO')}`;
  };

  // Stats
  const activeAlerts = priceAlerts.filter((a) => a.isActive).length;
  const triggeredAlerts = priceAlerts.filter((a) => a.isTriggered).length;
  const activeSearches = savedSearches.filter((s) => s.isActive).length;
  const withNotifications = savedSearches.filter((s) => s.sendEmailNotifications).length;

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mis Alertas</h1>
          <p className="text-gray-600 mt-1">Gestiona tus alertas de precio y búsquedas guardadas</p>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-blue-50 rounded-lg">
              <FiBell className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{activeAlerts}</p>
              <p className="text-sm text-gray-500">Alertas Activas</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-green-50 rounded-lg">
              <FiCheckCircle className="w-5 h-5 text-green-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{triggeredAlerts}</p>
              <p className="text-sm text-gray-500">Alertas Disparadas</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-purple-50 rounded-lg">
              <FiSearch className="w-5 h-5 text-purple-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{activeSearches}</p>
              <p className="text-sm text-gray-500">Búsquedas Activas</p>
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-orange-50 rounded-lg">
              <FiMail className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{withNotifications}</p>
              <p className="text-sm text-gray-500">Con Notificaciones</p>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">{error}</div>
      )}

      {/* Tabs */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px">
            <button
              onClick={() => setActiveTab('price')}
              className={`px-6 py-4 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'price'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <FiDollarSign className="inline-block w-4 h-4 mr-2" />
              Alertas de Precio ({priceAlerts.length})
            </button>
            <button
              onClick={() => setActiveTab('searches')}
              className={`px-6 py-4 text-sm font-medium border-b-2 transition-colors ${
                activeTab === 'searches'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              <FiSearch className="inline-block w-4 h-4 mr-2" />
              Búsquedas Guardadas ({savedSearches.length})
            </button>
          </nav>
        </div>

        {/* Tab Content */}
        <div className="p-6">
          {activeTab === 'price' && (
            <div className="space-y-4">
              {priceAlerts.length === 0 ? (
                <div className="text-center py-12">
                  <FiTrendingDown className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    No tienes alertas de precio
                  </h3>
                  <p className="text-gray-500 mb-4">
                    Crea alertas para recibir notificaciones cuando los precios bajen.
                  </p>
                </div>
              ) : (
                priceAlerts.map((alert) => {
                  const vehicle = vehicleInfo[alert.vehicleId];
                  return (
                    <div
                      key={alert.id}
                      className={`border rounded-lg p-4 transition-all ${
                        alert.isActive
                          ? alert.isTriggered
                            ? 'border-green-200 bg-green-50'
                            : 'border-gray-200 bg-white'
                          : 'border-gray-100 bg-gray-50 opacity-75'
                      }`}
                    >
                      <div className="flex items-center gap-4">
                        {/* Vehicle Image */}
                        <div className="w-20 h-20 rounded-lg overflow-hidden bg-gray-100 flex-shrink-0">
                          {vehicle?.imageUrl ? (
                            <img
                              src={vehicle.imageUrl}
                              alt={vehicle.title}
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center">
                              <FiDollarSign className="w-8 h-8 text-gray-300" />
                            </div>
                          )}
                        </div>

                        {/* Alert Info */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <h4 className="font-semibold text-gray-900 truncate">
                              {vehicle?.title || 'Cargando...'}
                            </h4>
                            {alert.isTriggered && (
                              <span className="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-700 rounded-full">
                                ¡Disparada!
                              </span>
                            )}
                            {!alert.isActive && (
                              <span className="px-2 py-0.5 text-xs font-medium bg-gray-100 text-gray-500 rounded-full">
                                Pausada
                              </span>
                            )}
                          </div>

                          <div className="flex items-center gap-4 text-sm text-gray-600">
                            <span className="flex items-center gap-1">
                              <FiDollarSign className="w-4 h-4" />
                              Precio actual: {vehicle ? formatPrice(vehicle.price) : '...'}
                            </span>
                            <span className="flex items-center gap-1 text-blue-600 font-medium">
                              <FiTrendingDown className="w-4 h-4" />
                              {getConditionLabel(alert.condition)}: {formatPrice(alert.targetPrice)}
                            </span>
                          </div>

                          <div className="flex items-center gap-4 mt-2 text-xs text-gray-400">
                            <span className="flex items-center gap-1">
                              <FiClock className="w-3 h-3" />
                              Creada: {formatDate(alert.createdAt)}
                            </span>
                            {alert.triggeredAt && (
                              <span className="flex items-center gap-1 text-green-600">
                                <FiCheckCircle className="w-3 h-3" />
                                Disparada: {formatDate(alert.triggeredAt)}
                              </span>
                            )}
                          </div>
                        </div>

                        {/* Actions */}
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleTogglePriceAlert(alert)}
                            className={`p-2 rounded-lg transition-colors ${
                              alert.isActive
                                ? 'text-blue-600 hover:bg-blue-50'
                                : 'text-gray-400 hover:bg-gray-100'
                            }`}
                            title={alert.isActive ? 'Pausar alerta' : 'Activar alerta'}
                          >
                            {alert.isActive ? (
                              <FiToggleRight className="w-6 h-6" />
                            ) : (
                              <FiToggleLeft className="w-6 h-6" />
                            )}
                          </button>
                          <button
                            onClick={() => handleDeletePriceAlert(alert.id)}
                            className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                            title="Eliminar alerta"
                          >
                            <FiTrash2 className="w-5 h-5" />
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })
              )}
            </div>
          )}

          {activeTab === 'searches' && (
            <div className="space-y-4">
              {savedSearches.length === 0 ? (
                <div className="text-center py-12">
                  <FiSearch className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    No tienes búsquedas guardadas
                  </h3>
                  <p className="text-gray-500 mb-4">
                    Guarda búsquedas para recibir notificaciones de nuevos vehículos.
                  </p>
                </div>
              ) : (
                savedSearches.map((search) => (
                  <div
                    key={search.id}
                    className={`border rounded-lg p-4 transition-all ${
                      search.isActive
                        ? 'border-gray-200 bg-white'
                        : 'border-gray-100 bg-gray-50 opacity-75'
                    }`}
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <h4 className="font-semibold text-gray-900">{search.name}</h4>
                          {!search.isActive && (
                            <span className="px-2 py-0.5 text-xs font-medium bg-gray-100 text-gray-500 rounded-full">
                              Pausada
                            </span>
                          )}
                        </div>

                        {/* Filters */}
                        <div className="flex items-center gap-2 text-sm text-gray-600 mb-3">
                          <FiFilter className="w-4 h-4 text-gray-400" />
                          <span>{formatCriteriaForDisplay(search.parsedCriteria || {})}</span>
                        </div>

                        {/* Notification Settings */}
                        <div className="flex items-center gap-4 text-xs text-gray-500">
                          {search.sendEmailNotifications ? (
                            <span className="flex items-center gap-1 text-green-600">
                              <FiMail className="w-3 h-3" />
                              Notificaciones: {getFrequencyLabel(search.frequency)}
                            </span>
                          ) : (
                            <span className="flex items-center gap-1">
                              <FiMail className="w-3 h-3 opacity-50" />
                              Sin notificaciones
                            </span>
                          )}
                          <span className="flex items-center gap-1">
                            <FiClock className="w-3 h-3" />
                            Creada: {formatDate(search.createdAt)}
                          </span>
                          {search.lastNotificationSent && (
                            <span className="flex items-center gap-1">
                              Última notificación: {formatDate(search.lastNotificationSent)}
                            </span>
                          )}
                        </div>
                      </div>

                      {/* Actions */}
                      <div className="flex items-center gap-2 ml-4">
                        <a
                          href={`/search?${search.searchCriteria}`}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="p-2 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="Ver resultados"
                        >
                          <FiExternalLink className="w-5 h-5" />
                        </a>
                        <button
                          onClick={() => handleToggleSavedSearch(search)}
                          className={`p-2 rounded-lg transition-colors ${
                            search.isActive
                              ? 'text-blue-600 hover:bg-blue-50'
                              : 'text-gray-400 hover:bg-gray-100'
                          }`}
                          title={search.isActive ? 'Pausar búsqueda' : 'Activar búsqueda'}
                        >
                          {search.isActive ? (
                            <FiToggleRight className="w-6 h-6" />
                          ) : (
                            <FiToggleLeft className="w-6 h-6" />
                          )}
                        </button>
                        <button
                          onClick={() => handleDeleteSavedSearch(search.id)}
                          className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                          title="Eliminar búsqueda"
                        >
                          <FiTrash2 className="w-5 h-5" />
                        </button>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default DealerAlertsPage;
