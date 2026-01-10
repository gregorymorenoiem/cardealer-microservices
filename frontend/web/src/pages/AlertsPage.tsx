import { useState, useEffect } from 'react';
import { FiBell, FiSearch, FiTrash2, FiEdit3, FiClock } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import EmptyState from '@/components/organisms/EmptyState';

interface PriceAlert {
  id: string;
  vehicleId: string;
  vehicleTitle: string;
  currentPrice: number;
  targetPrice: number;
  isActive: boolean;
  createdAt: string;
}

interface SavedSearch {
  id: string;
  name: string;
  filters: {
    make?: string;
    model?: string;
    minYear?: number;
    maxYear?: number;
    minPrice?: number;
    maxPrice?: number;
  };
  isActive: boolean;
  createdAt: string;
}

export function AlertsPage() {
  const [activeTab, setActiveTab] = useState<'price' | 'search'>('price');
  const [priceAlerts, setPriceAlerts] = useState<PriceAlert[]>([]);
  const [savedSearches, setSavedSearches] = useState<SavedSearch[]>([]);
  const [loading, setLoading] = useState(true);
  const [freeDaysLeft, setFreeDaysLeft] = useState<number>(0);

  useEffect(() => {
    loadAlerts();
  }, []);

  const loadAlerts = async () => {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      // ProtectedRoute ya se encarga de la redirección, no necesitamos hacerlo aquí
      setLoading(false);
      return;
    }

    try {
      const [priceResponse, searchResponse, daysResponse] = await Promise.all([
        fetch('https://api.okla.com.do/api/alerts/price-alerts', {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch('https://api.okla.com.do/api/alerts/saved-searches', {
          headers: { Authorization: `Bearer ${token}` },
        }),
        fetch('https://api.okla.com.do/api/alerts/free-days-left', {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);

      const priceData = await priceResponse.json();
      const searchData = await searchResponse.json();
      const daysData = await daysResponse.json();

      setPriceAlerts(priceData);
      setSavedSearches(searchData);
      setFreeDaysLeft(daysData.daysLeft);
    } catch (error) {
      console.error('Failed to load alerts:', error);
    } finally {
      setLoading(false);
    }
  };

  const togglePriceAlert = async (alertId: string, isActive: boolean) => {
    const token = localStorage.getItem('accessToken');
    try {
      const response = await fetch(
        `https://api.okla.com.do/api/alerts/price-alerts/${alertId}/toggle`,
        {
          method: 'PUT',
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ isActive: !isActive }),
        }
      );

      if (response.ok) {
        setPriceAlerts(
          priceAlerts.map((a) => (a.id === alertId ? { ...a, isActive: !isActive } : a))
        );
      }
    } catch (error) {
      console.error('Failed to toggle alert:', error);
    }
  };

  const deletePriceAlert = async (alertId: string) => {
    if (!confirm('¿Eliminar esta alerta?')) return;

    const token = localStorage.getItem('accessToken');
    try {
      const response = await fetch(`https://api.okla.com.do/api/alerts/price-alerts/${alertId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (response.ok) {
        setPriceAlerts(priceAlerts.filter((a) => a.id !== alertId));
      }
    } catch (error) {
      console.error('Failed to delete alert:', error);
    }
  };

  const toggleSavedSearch = async (searchId: string, isActive: boolean) => {
    const token = localStorage.getItem('accessToken');
    try {
      const response = await fetch(
        `https://api.okla.com.do/api/alerts/saved-searches/${searchId}/toggle`,
        {
          method: 'PUT',
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ isActive: !isActive }),
        }
      );

      if (response.ok) {
        setSavedSearches(
          savedSearches.map((s) => (s.id === searchId ? { ...s, isActive: !isActive } : s))
        );
      }
    } catch (error) {
      console.error('Failed to toggle search:', error);
    }
  };

  const deleteSavedSearch = async (searchId: string) => {
    if (!confirm('¿Eliminar esta búsqueda guardada?')) return;

    const token = localStorage.getItem('accessToken');
    try {
      const response = await fetch(
        `https://api.okla.com.do/api/alerts/saved-searches/${searchId}`,
        {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      if (response.ok) {
        setSavedSearches(savedSearches.filter((s) => s.id !== searchId));
      }
    } catch (error) {
      console.error('Failed to delete search:', error);
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="text-center py-12">
            <div className="animate-spin h-12 w-12 border-4 border-blue-500 border-t-transparent rounded-full mx-auto"></div>
            <p className="mt-4 text-gray-600">Cargando alertas...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold">Mis Alertas</h1>
            <p className="text-gray-600 mt-2">
              Recibe notificaciones cuando encuentres lo que buscas
            </p>
          </div>
          {freeDaysLeft > 0 && (
            <div className="bg-blue-50 px-4 py-2 rounded-lg border border-blue-200">
              <div className="flex items-center gap-2 text-blue-700">
                <FiClock className="w-5 h-5" />
                <span className="font-semibold">{freeDaysLeft} días gratis restantes</span>
              </div>
            </div>
          )}
        </div>

        {/* Tabs */}
        <div className="border-b border-gray-200 mb-8">
          <div className="flex gap-4">
            <button
              onClick={() => setActiveTab('price')}
              className={`pb-4 px-2 font-semibold border-b-2 transition-colors ${
                activeTab === 'price'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <div className="flex items-center gap-2">
                <FiBell className="w-5 h-5" />
                <span>Alertas de Precio</span>
                <span className="bg-blue-100 text-blue-700 px-2 py-1 rounded-full text-sm">
                  {priceAlerts.length}
                </span>
              </div>
            </button>
            <button
              onClick={() => setActiveTab('search')}
              className={`pb-4 px-2 font-semibold border-b-2 transition-colors ${
                activeTab === 'search'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <div className="flex items-center gap-2">
                <FiSearch className="w-5 h-5" />
                <span>Búsquedas Guardadas</span>
                <span className="bg-blue-100 text-blue-700 px-2 py-1 rounded-full text-sm">
                  {savedSearches.length}
                </span>
              </div>
            </button>
          </div>
        </div>

        {/* Price Alerts Tab */}
        {activeTab === 'price' && (
          <div className="space-y-4">
            {priceAlerts.length === 0 ? (
              <EmptyState
                icon={FiBell}
                title="No tienes alertas de precio"
                description="Crea alertas para recibir notificaciones cuando el precio baje"
                actionLabel="Buscar Vehículos"
                onAction={() => (window.location.href = '/search')}
              />
            ) : (
              priceAlerts.map((alert) => (
                <div
                  key={alert.id}
                  className="border rounded-lg p-6 hover:shadow-md transition-shadow"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <h3 className="text-xl font-bold mb-2">{alert.vehicleTitle}</h3>
                      <div className="flex gap-4 text-sm text-gray-600">
                        <span>
                          Precio actual: <strong>{formatPrice(alert.currentPrice)}</strong>
                        </span>
                        <span>
                          Precio objetivo:{' '}
                          <strong className="text-green-600">
                            {formatPrice(alert.targetPrice)}
                          </strong>
                        </span>
                      </div>
                      <div
                        className={`mt-2 inline-block px-3 py-1 rounded-full text-sm ${
                          alert.isActive
                            ? 'bg-green-100 text-green-700'
                            : 'bg-gray-100 text-gray-700'
                        }`}
                      >
                        {alert.isActive ? 'Activa' : 'Pausada'}
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        onClick={() => togglePriceAlert(alert.id, alert.isActive)}
                      >
                        {alert.isActive ? 'Pausar' : 'Activar'}
                      </Button>
                      <Button
                        variant="outline"
                        onClick={() => deletePriceAlert(alert.id)}
                        leftIcon={<FiTrash2 />}
                      >
                        Eliminar
                      </Button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        )}

        {/* Saved Searches Tab */}
        {activeTab === 'search' && (
          <div className="space-y-4">
            {savedSearches.length === 0 ? (
              <EmptyState
                icon={FiSearch}
                title="No tienes búsquedas guardadas"
                description="Guarda tus búsquedas para recibir notificaciones de nuevos vehículos"
                actionLabel="Buscar Vehículos"
                onAction={() => (window.location.href = '/search')}
              />
            ) : (
              savedSearches.map((search) => (
                <div
                  key={search.id}
                  className="border rounded-lg p-6 hover:shadow-md transition-shadow"
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <h3 className="text-xl font-bold mb-2">{search.name}</h3>
                      <div className="flex gap-2 flex-wrap">
                        {search.filters.make && (
                          <span className="bg-gray-100 px-3 py-1 rounded-full text-sm">
                            Marca: {search.filters.make}
                          </span>
                        )}
                        {search.filters.model && (
                          <span className="bg-gray-100 px-3 py-1 rounded-full text-sm">
                            Modelo: {search.filters.model}
                          </span>
                        )}
                        {search.filters.minYear && search.filters.maxYear && (
                          <span className="bg-gray-100 px-3 py-1 rounded-full text-sm">
                            Año: {search.filters.minYear}-{search.filters.maxYear}
                          </span>
                        )}
                      </div>
                      <div
                        className={`mt-2 inline-block px-3 py-1 rounded-full text-sm ${
                          search.isActive
                            ? 'bg-green-100 text-green-700'
                            : 'bg-gray-100 text-gray-700'
                        }`}
                      >
                        {search.isActive ? 'Activa' : 'Pausada'}
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        onClick={() => toggleSavedSearch(search.id, search.isActive)}
                      >
                        {search.isActive ? 'Pausar' : 'Activar'}
                      </Button>
                      <Button
                        variant="outline"
                        onClick={() => deleteSavedSearch(search.id)}
                        leftIcon={<FiTrash2 />}
                      >
                        Eliminar
                      </Button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
}
