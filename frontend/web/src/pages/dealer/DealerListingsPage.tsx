/**
 * DealerListingsPage - Publicaciones Activas
 *
 * Muestra SOLO los vehículos publicados (status: Active/approved)
 * Mismo estilo visual que DealerInventoryPage
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiSearch,
  FiEdit,
  FiEye,
  FiGrid,
  FiList,
  FiRefreshCw,
  FiAlertCircle,
  FiCheckCircle,
  FiExternalLink,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { getDealerVehicles } from '@/services/vehicleService';
import type { DealerVehicle } from '@/services/vehicleService';
import { useAuth } from '@/hooks/useAuth';
import { generateListingUrl } from '@/utils/seoSlug';

export default function DealerListingsPage() {
  const { user } = useAuth();
  const [publishedVehicles, setPublishedVehicles] = useState<DealerVehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [searchTerm, setSearchTerm] = useState('');

  const dealerId = user?.dealerId || user?.id || 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e';

  // Fetch inventory and filter to published only
  useEffect(() => {
    const fetchInventory = async () => {
      if (!dealerId) {
        setError('No dealer account found');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);
        const data = await getDealerVehicles(dealerId);
        // Filter ONLY published (approved/active) vehicles
        const published = data.vehicles.filter((v) => v.status === 'approved');
        setPublishedVehicles(published);
      } catch (err) {
        console.error('Error fetching inventory:', err);
        setError('Error al cargar las publicaciones');
      } finally {
        setIsLoading(false);
      }
    };

    fetchInventory();
  }, [dealerId]);

  // Filter by search
  const filteredVehicles = publishedVehicles.filter((v) => {
    const matchesSearch =
      v.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      v.make.toLowerCase().includes(searchTerm.toLowerCase()) ||
      v.model.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesSearch;
  });

  // Stats for published vehicles only
  const stats = {
    total: publishedVehicles.length,
    totalViews: publishedVehicles.reduce((sum, v) => sum + (v.viewCount || 0), 0),
    totalInquiries: publishedVehicles.reduce((sum, v) => sum + (v.inquiryCount || 0), 0),
    totalValue: publishedVehicles.reduce((sum, v) => sum + v.price, 0),
  };

  const handleRefresh = () => {
    setIsLoading(true);
    setError(null);
    getDealerVehicles(dealerId)
      .then((data) => {
        const published = data.vehicles.filter((v) => v.status === 'approved');
        setPublishedVehicles(published);
      })
      .catch((err) => {
        console.error('Error refreshing:', err);
        setError('Error al actualizar');
      })
      .finally(() => setIsLoading(false));
  };

  const formatPrice = (price: number) =>
    new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      maximumFractionDigits: 0,
    }).format(price);

  // Loading State
  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="p-6 lg:p-8">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent mx-auto mb-4"></div>
              <p className="text-gray-500">Cargando publicaciones...</p>
            </div>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  // Error State
  if (error) {
    return (
      <DealerPortalLayout>
        <div className="p-6 lg:p-8">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <FiAlertCircle className="w-12 h-12 mx-auto text-red-500 mb-4" />
              <h3 className="text-xl font-bold text-gray-900 mb-2">Error</h3>
              <p className="text-gray-500 mb-4">{error}</p>
              <button
                onClick={handleRefresh}
                className="px-4 py-2 bg-blue-600 text-white rounded-xl hover:bg-blue-700"
              >
                Reintentar
              </button>
            </div>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Publicaciones Activas</h1>
            <p className="text-gray-500 mt-1">
              {stats.total} vehículos publicados y visibles al público
            </p>
          </div>
          <div className="flex items-center gap-3">
            <button
              onClick={handleRefresh}
              className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
            >
              <FiRefreshCw className="w-4 h-4" />
              <span>Actualizar</span>
            </button>
            <Link
              to="/dealer/inventory"
              className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
            >
              <span>Ver Inventario Completo</span>
            </Link>
          </div>
        </div>

        {/* Info Banner */}
        <div className="bg-green-50 border border-green-200 rounded-xl p-4 flex items-start gap-3">
          <FiCheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-medium text-green-800">Solo Vehículos Publicados</p>
            <p className="text-sm text-green-600 mt-0.5">
              Esta vista muestra únicamente los vehículos aprobados y visibles al público. Para
              gestionar borradores, pendientes y vendidos, ve a{' '}
              <Link to="/dealer/inventory" className="underline font-medium">
                Inventario
              </Link>
              .
            </p>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Publicados</p>
            <p className="text-2xl font-bold text-green-600">{stats.total}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Vistas Totales</p>
            <p className="text-2xl font-bold text-blue-600">{stats.totalViews.toLocaleString()}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Consultas</p>
            <p className="text-2xl font-bold text-purple-600">{stats.totalInquiries}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Valor Total</p>
            <p className="text-2xl font-bold text-gray-900">{formatPrice(stats.totalValue)}</p>
          </div>
        </div>

        {/* Search Bar */}
        <div className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100">
          <div className="flex flex-col lg:flex-row lg:items-center gap-4">
            {/* Search */}
            <div className="relative flex-1">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar en publicaciones activas..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400"
              />
            </div>

            {/* View Mode */}
            <div className="flex items-center gap-1 p-1 bg-gray-100 rounded-xl">
              <button
                onClick={() => setViewMode('grid')}
                className={`p-2 rounded-lg ${viewMode === 'grid' ? 'bg-white shadow-sm' : ''}`}
              >
                <FiGrid className="w-5 h-5 text-gray-600" />
              </button>
              <button
                onClick={() => setViewMode('list')}
                className={`p-2 rounded-lg ${viewMode === 'list' ? 'bg-white shadow-sm' : ''}`}
              >
                <FiList className="w-5 h-5 text-gray-600" />
              </button>
            </div>
          </div>
        </div>

        {/* Vehicles Grid */}
        {viewMode === 'grid' ? (
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredVehicles.map((vehicle) => (
              <VehicleGridCard key={vehicle.id} vehicle={vehicle} formatPrice={formatPrice} />
            ))}
          </div>
        ) : (
          // List View
          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 border-b border-gray-100">
                  <tr>
                    <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Vehículo
                    </th>
                    <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Precio
                    </th>
                    <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Estado
                    </th>
                    <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Vistas
                    </th>
                    <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Consultas
                    </th>
                    <th className="text-right px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {filteredVehicles.map((vehicle) => (
                    <VehicleTableRow key={vehicle.id} vehicle={vehicle} formatPrice={formatPrice} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {filteredVehicles.length === 0 && !isLoading && (
          <div className="text-center py-16 bg-white rounded-2xl">
            <FaCar className="w-16 h-16 mx-auto text-gray-300 mb-4" />
            <h3 className="text-xl font-bold text-gray-900 mb-2">No hay publicaciones activas</h3>
            <p className="text-gray-500 mb-6">
              {searchTerm
                ? 'No se encontraron vehículos con ese término de búsqueda'
                : 'Los vehículos aparecerán aquí cuando sean aprobados por los administradores'}
            </p>
            <Link
              to="/dealer/inventory"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-xl font-semibold"
            >
              Ir a Inventario
            </Link>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}

// Grid Card Component
function VehicleGridCard({
  vehicle,
  formatPrice,
}: {
  vehicle: DealerVehicle;
  formatPrice: (price: number) => string;
}) {
  const listingUrl = generateListingUrl(vehicle.id, vehicle.title);

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden group hover:shadow-md transition-shadow">
      {/* Image */}
      <div className="relative aspect-[4/3] bg-gray-100">
        {vehicle.images && vehicle.images.length > 0 ? (
          <img src={vehicle.images[0]} alt={vehicle.title} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <FaCar className="w-12 h-12 text-gray-300" />
          </div>
        )}
        {/* Status Badge */}
        <div className="absolute top-3 left-3">
          <span className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-green-500 text-white text-xs font-semibold rounded-full shadow-sm">
            <FiCheckCircle className="w-3.5 h-3.5" />
            Publicado
          </span>
        </div>
      </div>

      {/* Content */}
      <div className="p-4 space-y-3">
        <div>
          <h3 className="font-bold text-gray-900 truncate">{vehicle.title}</h3>
          <p className="text-sm text-gray-500">
            {vehicle.year} • {vehicle.mileage?.toLocaleString()} km
          </p>
        </div>

        <p className="text-xl font-bold text-blue-600">{formatPrice(vehicle.price)}</p>

        {/* Stats */}
        <div className="flex items-center gap-4 text-sm text-gray-500">
          <span className="flex items-center gap-1">
            <FiEye className="w-4 h-4" />
            {vehicle.viewCount || 0} vistas
          </span>
          <span className="flex items-center gap-1">💬 {vehicle.inquiryCount || 0} consultas</span>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2 pt-2 border-t border-gray-100">
          <Link
            to={`/dealer/inventory/${vehicle.id}/edit`}
            className="flex-1 flex items-center justify-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
          >
            <FiEdit className="w-4 h-4" />
            Editar
          </Link>
          <Link
            to={listingUrl}
            target="_blank"
            className="flex-1 flex items-center justify-center gap-2 px-3 py-2 text-sm font-medium text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100"
          >
            <FiExternalLink className="w-4 h-4" />
            Ver
          </Link>
        </div>
      </div>
    </div>
  );
}

// Table Row Component
function VehicleTableRow({
  vehicle,
  formatPrice,
}: {
  vehicle: DealerVehicle;
  formatPrice: (price: number) => string;
}) {
  const listingUrl = generateListingUrl(vehicle.id, vehicle.title);

  return (
    <tr className="hover:bg-gray-50">
      {/* Vehicle */}
      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="w-16 h-12 bg-gray-100 rounded-lg overflow-hidden flex-shrink-0">
            {vehicle.images && vehicle.images.length > 0 ? (
              <img
                src={vehicle.images[0]}
                alt={vehicle.title}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                <FaCar className="w-6 h-6 text-gray-300" />
              </div>
            )}
          </div>
          <div>
            <p className="font-semibold text-gray-900">{vehicle.title}</p>
            <p className="text-sm text-gray-500">
              {vehicle.year} • {vehicle.mileage?.toLocaleString()} km
            </p>
          </div>
        </div>
      </td>

      {/* Price */}
      <td className="px-6 py-4">
        <span className="font-semibold text-gray-900">{formatPrice(vehicle.price)}</span>
      </td>

      {/* Status */}
      <td className="px-6 py-4">
        <span className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-green-100 text-green-700 text-xs font-semibold rounded-full">
          <FiCheckCircle className="w-3.5 h-3.5" />
          Publicado
        </span>
      </td>

      {/* Views */}
      <td className="px-6 py-4">
        <span className="text-gray-600">{vehicle.viewCount || 0}</span>
      </td>

      {/* Inquiries */}
      <td className="px-6 py-4">
        <span className="text-gray-600">{vehicle.inquiryCount || 0}</span>
      </td>

      {/* Actions */}
      <td className="px-6 py-4 text-right">
        <div className="flex items-center justify-end gap-2">
          <Link
            to={`/dealer/inventory/${vehicle.id}/edit`}
            className="p-2 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg"
            title="Editar"
          >
            <FiEdit className="w-4 h-4" />
          </Link>
          <Link
            to={listingUrl}
            target="_blank"
            className="p-2 text-gray-500 hover:text-green-600 hover:bg-green-50 rounded-lg"
            title="Ver publicación"
          >
            <FiExternalLink className="w-4 h-4" />
          </Link>
        </div>
      </td>
    </tr>
  );
}
