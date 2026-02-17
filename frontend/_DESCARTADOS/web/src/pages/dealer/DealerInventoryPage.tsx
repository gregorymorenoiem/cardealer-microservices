/**
 * DealerInventoryPage - Gestión de Inventario de Vehículos
 *
 * Lista todos los vehículos del dealer con acciones de gestión
 * Conectado al backend VehiclesSaleService
 */

import { useState, useEffect, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiPlusCircle,
  FiSearch,
  FiEdit,
  FiEye,
  FiTrash2,
  FiGrid,
  FiList,
  FiRefreshCw,
  FiAlertCircle,
  FiCheckCircle,
  FiPauseCircle,
  FiDollarSign,
  FiPlay,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { getDealerVehicles, updateVehicleStatus } from '@/services/vehicleService';
import type { DealerVehicle, DealerInventoryResponse } from '@/services/vehicleService';
import { useAuth } from '@/hooks/useAuth';
import { generateListingUrl } from '@/utils/seoSlug';

// Vehicle status enum (matches backend VehicleStatus)
const VehicleStatus = {
  Draft: 0, // Borrador - No publicado
  PendingReview: 1, // En revisión
  Active: 2, // Publicado/Activo
  Reserved: 3, // Reservado
  Sold: 4, // Vendido
  Archived: 5, // Archivado
  Rejected: 6, // Rechazado
} as const;

export default function DealerInventoryPage() {
  const { user } = useAuth();
  const [inventory, setInventory] = useState<DealerInventoryResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'review' | 'draft' | 'sold'>(
    'all'
  );

  // For demo: use test dealerId if user doesn't have one
  // TODO: Remove this fallback in production
  const dealerId = user?.dealerId || 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e';

  // Fetch inventory on mount
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
        setInventory(data);
      } catch (err) {
        console.error('Error fetching inventory:', err);
        setError('Failed to load inventory');
      } finally {
        setIsLoading(false);
      }
    };

    fetchInventory();
  }, [dealerId]);

  // Filter vehicles
  const filteredVehicles =
    inventory?.vehicles.filter((v) => {
      const matchesSearch =
        v.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.make.toLowerCase().includes(searchTerm.toLowerCase()) ||
        v.model.toLowerCase().includes(searchTerm.toLowerCase());

      let matchesStatus = true;
      if (filterStatus === 'active') matchesStatus = v.status === 'approved';
      else if (filterStatus === 'review')
        matchesStatus = v.status === 'pending'; // pending = En Evaluación
      else if (filterStatus === 'draft')
        matchesStatus = v.status === 'rejected'; // rejected = Borrador
      else if (filterStatus === 'sold') matchesStatus = v.status === 'sold';

      return matchesSearch && matchesStatus;
    }) || [];

  const getStatusBadge = (status: string) => {
    const badges: Record<string, { bg: string; text: string; label: string; icon: ReactNode }> = {
      approved: {
        bg: 'bg-green-100',
        text: 'text-green-700',
        label: '✓ Publicado',
        icon: <FiCheckCircle className="w-3.5 h-3.5" />,
      },
      pending: {
        bg: 'bg-orange-100',
        text: 'text-orange-700',
        label: '⏳ En Evaluación',
        icon: <FiAlertCircle className="w-3.5 h-3.5" />,
      },
      sold: {
        bg: 'bg-blue-100',
        text: 'text-blue-700',
        label: '$ Vendido',
        icon: <FiDollarSign className="w-3.5 h-3.5" />,
      },
      rejected: {
        bg: 'bg-gray-200',
        text: 'text-gray-700',
        label: '○ Borrador',
        icon: null,
      },
    };
    const badge = badges[status] || badges.rejected;
    return (
      <span
        className={`${badge.bg} ${badge.text} px-3 py-1.5 rounded-full text-xs font-bold flex items-center gap-1.5 shadow-sm`}
      >
        {badge.icon}
        {badge.label}
      </span>
    );
  };

  // Handle status change
  const handleStatusChange = async (vehicleId: string, newStatus: number, statusName: string) => {
    const confirmMessages: Record<number, string> = {
      [VehicleStatus.PendingReview]:
        '¿Enviar a evaluación? Un administrador revisará tu publicación antes de hacerla visible.',
      [VehicleStatus.Draft]:
        '¿Regresar a borrador? El vehículo dejará de estar visible y podrás editarlo.',
      [VehicleStatus.Sold]: '¿Marcar como vendido? Esto archivará el vehículo.',
    };

    if (!confirm(confirmMessages[newStatus] || `¿Cambiar estado a ${statusName}?`)) {
      return;
    }

    try {
      await updateVehicleStatus(vehicleId, newStatus);
      // Refresh the list
      await handleRefresh();
    } catch (err) {
      console.error('Error changing status:', err);
      alert('Error al cambiar el estado del vehículo');
    }
  };

  const handleRefresh = async () => {
    if (!dealerId) return;
    setIsLoading(true);
    try {
      const data = await getDealerVehicles(dealerId);
      setInventory(data);
    } catch (err) {
      console.error('Error refreshing:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="p-6 lg:p-8">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <FiRefreshCw className="w-8 h-8 mx-auto text-blue-500 animate-spin mb-4" />
              <p className="text-gray-500">Cargando inventario...</p>
            </div>
          </div>
        </div>
      </DealerPortalLayout>
    );
  }

  // Error state
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
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Inventario</h1>
            <p className="text-gray-500 mt-1">
              {inventory?.total || 0} vehículos • {inventory?.activeCount || 0} activos
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
              to="/dealer/inventory/new"
              className="flex items-center gap-2 px-5 py-2.5 bg-gradient-to-r from-blue-600 to-blue-500 text-white rounded-xl font-semibold shadow-lg hover:shadow-xl transition-all"
            >
              <FiPlusCircle className="w-5 h-5" />
              <span>Agregar Vehículo</span>
            </Link>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Total</p>
            <p className="text-2xl font-bold text-gray-900">{inventory?.total || 0}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Publicados</p>
            <p className="text-2xl font-bold text-green-600">{inventory?.activeCount || 0}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">En Evaluación</p>
            <p className="text-2xl font-bold text-orange-600">{inventory?.pausedCount || 0}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Borradores</p>
            <p className="text-2xl font-bold text-gray-600">{inventory?.draftCount || 0}</p>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-100">
            <p className="text-sm text-gray-500">Vendidos</p>
            <p className="text-2xl font-bold text-blue-600">{inventory?.soldCount || 0}</p>
          </div>
        </div>

        {/* Filters Bar */}
        <div className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100">
          <div className="flex flex-col lg:flex-row lg:items-center gap-4">
            {/* Search */}
            <div className="relative flex-1">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar vehículos..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400"
              />
            </div>

            {/* Status Filter */}
            <div className="flex items-center gap-2">
              <span className="text-sm text-gray-500">Estado:</span>
              <select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value as any)}
                className="px-3 py-2 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              >
                <option value="all">Todos</option>
                <option value="active">Publicados</option>
                <option value="review">En Evaluación</option>
                <option value="draft">Borradores</option>
                <option value="sold">Vendidos</option>
              </select>
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
              <VehicleGridCard
                key={vehicle.id}
                vehicle={vehicle}
                getStatusBadge={getStatusBadge}
                onStatusChange={handleStatusChange}
              />
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
                    <VehicleTableRow
                      key={vehicle.id}
                      vehicle={vehicle}
                      getStatusBadge={getStatusBadge}
                      onStatusChange={handleStatusChange}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {filteredVehicles.length === 0 && !isLoading && (
          <div className="text-center py-16 bg-white rounded-2xl">
            <FaCar className="w-16 h-16 mx-auto text-gray-300 mb-4" />
            <h3 className="text-xl font-bold text-gray-900 mb-2">No hay vehículos</h3>
            <p className="text-gray-500 mb-6">
              {searchTerm || filterStatus !== 'all'
                ? 'No se encontraron vehículos con esos filtros'
                : 'Comienza agregando tu primer vehículo al inventario'}
            </p>
            <Link
              to="/dealer/inventory/new"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-xl font-semibold"
            >
              <FiPlusCircle className="w-5 h-5" />
              Agregar Vehículo
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
  getStatusBadge,
  onStatusChange,
}: {
  vehicle: DealerVehicle;
  getStatusBadge: (status: string) => ReactNode;
  onStatusChange: (vehicleId: string, newStatus: number, statusName: string) => void;
}) {
  // Determine which action buttons to show based on current status
  const isPublished = vehicle.status === 'approved'; // Active (2)
  const isInReview = vehicle.status === 'pending'; // PendingReview (1)
  const isDraft = vehicle.status === 'rejected'; // Draft (0) - mapped as 'rejected'
  const isSold = vehicle.status === 'sold'; // Sold (4)

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition-all group">
      <div className="relative">
        <img
          src={vehicle.images[0] || '/placeholder-car.jpg'}
          alt={vehicle.title}
          className="w-full h-48 object-cover group-hover:scale-105 transition-transform duration-300"
          onError={(e) => {
            (e.target as HTMLImageElement).src = '/placeholder-car.jpg';
          }}
        />
        <div className="absolute top-3 left-3">{getStatusBadge(vehicle.status)}</div>
      </div>
      <div className="p-5">
        <h3 className="font-bold text-gray-900 mb-1 truncate">{vehicle.title}</h3>
        <p className="text-2xl font-bold text-blue-600 mb-4">${vehicle.price.toLocaleString()}</p>
        <div className="flex items-center gap-4 text-sm text-gray-500 mb-4">
          <span className="flex items-center gap-1">
            <FiEye className="w-4 h-4" /> {vehicle.viewCount} vistas
          </span>
          <span className="flex items-center gap-1">💬 {vehicle.inquiryCount} consultas</span>
        </div>

        {/* Action Buttons based on status */}
        <div className="flex flex-col gap-2">
          {/* Edit and View - always available except for sold */}
          <div className="flex items-center gap-2">
            <Link
              to={`/dealer/inventory/${vehicle.id}/edit`}
              className="flex-1 flex items-center justify-center gap-2 py-2 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
            >
              <FiEdit className="w-4 h-4" /> Editar
            </Link>
            <Link
              to={generateListingUrl(vehicle.id, vehicle.title)}
              className="flex-1 flex items-center justify-center gap-2 py-2 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200"
            >
              <FiEye className="w-4 h-4" /> Ver
            </Link>
          </div>

          {/* Status Actions */}
          <div className="flex items-center gap-2">
            {/* BORRADOR: Show "Enviar a Evaluación" button */}
            {isDraft && (
              <button
                onClick={() => onStatusChange(vehicle.id, 1, 'En Evaluación')}
                className="flex-1 flex items-center justify-center gap-2 py-2.5 bg-green-600 text-white rounded-xl hover:bg-green-700 font-semibold transition-colors"
              >
                <FiPlay className="w-4 h-4" /> Enviar a Evaluación
              </button>
            )}

            {/* EN EVALUACIÓN: Show message that it's being reviewed */}
            {isInReview && (
              <div className="flex-1 flex items-center justify-center gap-2 py-2.5 bg-orange-100 text-orange-700 rounded-xl font-medium">
                <FiAlertCircle className="w-4 h-4" /> Pendiente de aprobación
              </div>
            )}

            {/* PUBLICADO: Show PAUSE and SOLD buttons */}
            {isPublished && (
              <>
                <button
                  onClick={() => onStatusChange(vehicle.id, 0, 'Borrador')}
                  className="flex-1 flex items-center justify-center gap-2 py-2.5 bg-yellow-500 text-white rounded-xl hover:bg-yellow-600 font-semibold transition-colors"
                >
                  <FiPauseCircle className="w-4 h-4" /> Pausar
                </button>
                <button
                  onClick={() => onStatusChange(vehicle.id, 4, 'Vendido')}
                  className="flex-1 flex items-center justify-center gap-2 py-2.5 bg-blue-600 text-white rounded-xl hover:bg-blue-700 font-semibold transition-colors"
                >
                  <FiDollarSign className="w-4 h-4" /> Vendido
                </button>
              </>
            )}

            {/* VENDIDO: No actions, just info */}
            {isSold && (
              <div className="flex-1 flex items-center justify-center gap-2 py-2.5 bg-gray-100 text-gray-500 rounded-xl">
                <FiCheckCircle className="w-4 h-4" /> Venta completada
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// Table Row Component
function VehicleTableRow({
  vehicle,
  getStatusBadge,
  onStatusChange,
}: {
  vehicle: DealerVehicle;
  getStatusBadge: (status: string) => ReactNode;
  onStatusChange: (vehicleId: string, newStatus: number, statusName: string) => void;
}) {
  const isPublished = vehicle.status === 'approved'; // Active (2)
  const isInReview = vehicle.status === 'pending'; // PendingReview (1)
  const isDraft = vehicle.status === 'rejected'; // Draft (0)
  const isSold = vehicle.status === 'sold'; // Sold (4)

  return (
    <tr className="hover:bg-gray-50">
      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <img
            src={vehicle.images[0] || '/placeholder-car.jpg'}
            alt=""
            className="w-12 h-12 rounded-lg object-cover"
            onError={(e) => {
              (e.target as HTMLImageElement).src = '/placeholder-car.jpg';
            }}
          />
          <div>
            <span className="font-medium text-gray-900 block">{vehicle.title}</span>
            <span className="text-sm text-gray-500">{vehicle.mileage.toLocaleString()} km</span>
          </div>
        </div>
      </td>
      <td className="px-6 py-4 font-bold text-gray-900">${vehicle.price.toLocaleString()}</td>
      <td className="px-6 py-4">{getStatusBadge(vehicle.status)}</td>
      <td className="px-6 py-4 text-gray-600">{vehicle.viewCount}</td>
      <td className="px-6 py-4 text-gray-600">{vehicle.inquiryCount}</td>
      <td className="px-6 py-4 text-right">
        <div className="flex items-center justify-end gap-2">
          {/* Status action buttons */}
          {isDraft && (
            <button
              onClick={() => onStatusChange(vehicle.id, 1, 'En Evaluación')}
              className="px-3 py-1.5 bg-green-600 text-white text-xs font-semibold rounded-lg hover:bg-green-700 flex items-center gap-1"
              title="Enviar a Evaluación"
            >
              <FiPlay className="w-3.5 h-3.5" /> Evaluar
            </button>
          )}
          {isInReview && (
            <span className="px-3 py-1.5 bg-orange-100 text-orange-700 text-xs font-medium rounded-lg flex items-center gap-1">
              <FiAlertCircle className="w-3.5 h-3.5" /> Pendiente
            </span>
          )}
          {isPublished && (
            <>
              <button
                onClick={() => onStatusChange(vehicle.id, 0, 'Borrador')}
                className="px-3 py-1.5 bg-yellow-500 text-white text-xs font-semibold rounded-lg hover:bg-yellow-600 flex items-center gap-1"
                title="Pausar"
              >
                <FiPauseCircle className="w-3.5 h-3.5" /> Pausar
              </button>
              <button
                onClick={() => onStatusChange(vehicle.id, 4, 'Vendido')}
                className="px-3 py-1.5 bg-blue-600 text-white text-xs font-semibold rounded-lg hover:bg-blue-700 flex items-center gap-1"
                title="Vendido"
              >
                <FiDollarSign className="w-3.5 h-3.5" /> Vendido
              </button>
            </>
          )}
          {isSold && (
            <span className="px-3 py-1.5 bg-gray-100 text-gray-500 text-xs rounded-lg">
              Completado
            </span>
          )}

          {/* Edit and View */}
          <Link
            to={`/dealer/inventory/${vehicle.id}/edit`}
            className="p-2 hover:bg-gray-100 rounded-lg"
            title="Editar"
          >
            <FiEdit className="w-4 h-4 text-gray-500" />
          </Link>
          <Link
            to={generateListingUrl(vehicle.id, vehicle.title)}
            className="p-2 hover:bg-gray-100 rounded-lg"
            title="Ver publicación"
          >
            <FiEye className="w-4 h-4 text-gray-500" />
          </Link>
          <button className="p-2 hover:bg-gray-100 rounded-lg" title="Eliminar">
            <FiTrash2 className="w-4 h-4 text-red-500" />
          </button>
        </div>
      </td>
    </tr>
  );
}
