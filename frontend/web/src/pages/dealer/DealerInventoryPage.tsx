/**
 * DealerInventoryPage - Gestión de Inventario de Vehículos
 *
 * Lista todos los vehículos del dealer con acciones de gestión
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiPlusCircle,
  FiSearch,
  FiFilter,
  FiMoreVertical,
  FiEdit,
  FiEye,
  FiTrash2,
  FiPause,
  FiPlay,
  FiDownload,
  FiUpload,
  FiGrid,
  FiList,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';

// Mock data para demo
const mockVehicles = [
  {
    id: '1',
    title: 'Toyota Corolla 2022',
    price: 28000,
    status: 'active',
    views: 245,
    inquiries: 12,
    image: 'https://images.unsplash.com/photo-1623869675781-80aa31012a5a?w=300',
    createdAt: '2026-01-05',
  },
  {
    id: '2',
    title: 'Honda Civic 2021',
    price: 25000,
    status: 'active',
    views: 180,
    inquiries: 8,
    image: 'https://images.unsplash.com/photo-1606611013016-969c19ba27bb?w=300',
    createdAt: '2026-01-03',
  },
  {
    id: '3',
    title: 'BMW X5 2023',
    price: 65000,
    status: 'paused',
    views: 320,
    inquiries: 15,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=300',
    createdAt: '2026-01-01',
  },
];

export default function DealerInventoryPage() {
  const [vehicles] = useState(mockVehicles);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'paused' | 'sold'>('all');

  const filteredVehicles = vehicles.filter((v) => {
    const matchesSearch = v.title.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === 'all' || v.status === filterStatus;
    return matchesSearch && matchesStatus;
  });

  const getStatusBadge = (status: string) => {
    const badges: Record<string, { bg: string; text: string; label: string }> = {
      active: { bg: 'bg-green-100', text: 'text-green-700', label: 'Activo' },
      paused: { bg: 'bg-yellow-100', text: 'text-yellow-700', label: 'Pausado' },
      sold: { bg: 'bg-blue-100', text: 'text-blue-700', label: 'Vendido' },
    };
    const badge = badges[status] || badges.active;
    return (
      <span className={`${badge.bg} ${badge.text} px-2 py-1 rounded-full text-xs font-medium`}>
        {badge.label}
      </span>
    );
  };

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Inventario</h1>
            <p className="text-gray-500 mt-1">Gestiona tu inventario de vehículos</p>
          </div>
          <div className="flex items-center gap-3">
            <button className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50">
              <FiUpload className="w-4 h-4" />
              <span>Importar CSV</span>
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
                <option value="active">Activos</option>
                <option value="paused">Pausados</option>
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
              <div
                key={vehicle.id}
                className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-lg transition-all group"
              >
                <div className="relative">
                  <img
                    src={vehicle.image}
                    alt={vehicle.title}
                    className="w-full h-48 object-cover group-hover:scale-105 transition-transform duration-300"
                  />
                  <div className="absolute top-3 left-3">{getStatusBadge(vehicle.status)}</div>
                  <button className="absolute top-3 right-3 p-2 bg-white/90 rounded-full hover:bg-white shadow-sm">
                    <FiMoreVertical className="w-4 h-4 text-gray-600" />
                  </button>
                </div>
                <div className="p-5">
                  <h3 className="font-bold text-gray-900 mb-1">{vehicle.title}</h3>
                  <p className="text-2xl font-bold text-blue-600 mb-4">
                    ${vehicle.price.toLocaleString()}
                  </p>
                  <div className="flex items-center gap-4 text-sm text-gray-500 mb-4">
                    <span className="flex items-center gap-1">
                      <FiEye className="w-4 h-4" /> {vehicle.views} vistas
                    </span>
                    <span className="flex items-center gap-1">
                      💬 {vehicle.inquiries} consultas
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Link
                      to={`/dealer/inventory/${vehicle.id}/edit`}
                      className="flex-1 flex items-center justify-center gap-2 py-2 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
                    >
                      <FiEdit className="w-4 h-4" /> Editar
                    </Link>
                    <Link
                      to={`/vehicles/${vehicle.id}`}
                      className="flex-1 flex items-center justify-center gap-2 py-2 bg-blue-600 text-white rounded-xl hover:bg-blue-700"
                    >
                      <FiEye className="w-4 h-4" /> Ver
                    </Link>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          // List View
          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
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
                  <tr key={vehicle.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <img
                          src={vehicle.image}
                          alt=""
                          className="w-12 h-12 rounded-lg object-cover"
                        />
                        <span className="font-medium text-gray-900">{vehicle.title}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 font-bold text-gray-900">
                      ${vehicle.price.toLocaleString()}
                    </td>
                    <td className="px-6 py-4">{getStatusBadge(vehicle.status)}</td>
                    <td className="px-6 py-4 text-gray-600">{vehicle.views}</td>
                    <td className="px-6 py-4 text-gray-600">{vehicle.inquiries}</td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button className="p-2 hover:bg-gray-100 rounded-lg">
                          <FiEdit className="w-4 h-4 text-gray-500" />
                        </button>
                        <button className="p-2 hover:bg-gray-100 rounded-lg">
                          <FiEye className="w-4 h-4 text-gray-500" />
                        </button>
                        <button className="p-2 hover:bg-gray-100 rounded-lg">
                          <FiTrash2 className="w-4 h-4 text-red-500" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {filteredVehicles.length === 0 && (
          <div className="text-center py-16 bg-white rounded-2xl">
            <FaCar className="w-16 h-16 mx-auto text-gray-300 mb-4" />
            <h3 className="text-xl font-bold text-gray-900 mb-2">No hay vehículos</h3>
            <p className="text-gray-500 mb-6">
              Comienza agregando tu primer vehículo al inventario
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
