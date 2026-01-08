import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import inventoryManagementService, {
  InventoryItemDto,
  InventoryStatus,
  InventoryFilters,
  PagedResultDto,
} from '../services/inventoryManagementService';
import { FiSearch, FiFilter, FiPlus, FiEye, FiEdit, FiTrash2, FiCheckSquare } from 'react-icons/fi';

interface InventoryManagementPageProps {
  dealerId: string;
}

export default function InventoryManagementPage({ dealerId }: InventoryManagementPageProps) {
  const navigate = useNavigate();
  const [inventoryData, setInventoryData] = useState<PagedResultDto<InventoryItemDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  
  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<InventoryStatus | undefined>(undefined);
  const [sortBy, setSortBy] = useState('created');
  const [sortDescending, setSortDescending] = useState(true);
  const [page, setPage] = useState(1);
  const pageSize = 20;

  useEffect(() => {
    loadInventory();
  }, [dealerId, page, statusFilter, sortBy, sortDescending, searchTerm]);

  const loadInventory = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const filters: InventoryFilters = {
        dealerId,
        page,
        pageSize,
        status: statusFilter,
        searchTerm: searchTerm || undefined,
        sortBy,
        sortDescending,
      };

      const data = await inventoryManagementService.getInventoryItems(filters);
      setInventoryData(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar el inventario');
      console.error('Error loading inventory:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectItem = (id: string) => {
    const newSelected = new Set(selectedItems);
    if (newSelected.has(id)) {
      newSelected.delete(id);
    } else {
      newSelected.add(id);
    }
    setSelectedItems(newSelected);
  };

  const handleSelectAll = () => {
    if (selectedItems.size === inventoryData?.items.length) {
      setSelectedItems(new Set());
    } else {
      const allIds = inventoryData?.items.map(item => item.id) || [];
      setSelectedItems(new Set(allIds));
    }
  };

  const handleBulkAction = async (action: 'activate' | 'pause' | 'delete') => {
    if (selectedItems.size === 0) {
      alert('Selecciona al menos un ítem');
      return;
    }

    const confirmMessage = 
      action === 'delete' 
        ? '¿Estás seguro de eliminar los ítems seleccionados?' 
        : `¿Confirmar cambiar status a ${action === 'activate' ? 'Activo' : 'Pausado'}?`;

    if (!confirm(confirmMessage)) return;

    try {
      const itemIds = Array.from(selectedItems);
      
      if (action === 'delete') {
        // Delete items one by one
        for (const id of itemIds) {
          await inventoryManagementService.deleteInventoryItem(id);
        }
      } else {
        const newStatus = action === 'activate' ? InventoryStatus.Active : InventoryStatus.Paused;
        await inventoryManagementService.bulkUpdateStatus({ itemIds, status: newStatus });
      }

      setSelectedItems(new Set());
      loadInventory();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Error al ejecutar acción');
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadInventory();
  };

  if (loading && !inventoryData) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Gestión de Inventario</h1>
            <p className="text-gray-600 mt-1">
              {inventoryData?.totalCount || 0} vehículos en total
            </p>
          </div>
          <button
            onClick={() => navigate('/dealer/inventory/new')}
            className="flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition"
          >
            <FiPlus size={20} />
            <span>Nuevo Vehículo</span>
          </button>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow-sm p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Search */}
            <div className="md:col-span-2">
              <div className="relative">
                <input
                  type="text"
                  placeholder="Buscar por VIN, ubicación, notas..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <FiSearch className="absolute left-3 top-3 text-gray-400" size={20} />
              </div>
            </div>

            {/* Status Filter */}
            <div>
              <select
                value={statusFilter || ''}
                onChange={(e) => setStatusFilter(e.target.value ? e.target.value as InventoryStatus : undefined)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Todos los estados</option>
                <option value={InventoryStatus.Active}>Activo</option>
                <option value={InventoryStatus.Paused}>Pausado</option>
                <option value={InventoryStatus.Sold}>Vendido</option>
              </select>
            </div>

            {/* Sort */}
            <div>
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              >
                <option value="created">Fecha creación</option>
                <option value="price">Precio</option>
                <option value="days">Días en mercado</option>
                <option value="views">Vistas</option>
              </select>
            </div>
          </div>
        </div>

        {/* Bulk Actions */}
        {selectedItems.size > 0 && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <div className="flex items-center justify-between">
              <span className="text-blue-900 font-medium">
                {selectedItems.size} ítem(s) seleccionado(s)
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => handleBulkAction('activate')}
                  className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition"
                >
                  Activar
                </button>
                <button
                  onClick={() => handleBulkAction('pause')}
                  className="px-4 py-2 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 transition"
                >
                  Pausar
                </button>
                <button
                  onClick={() => handleBulkAction('delete')}
                  className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
                >
                  Eliminar
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 rounded-lg p-4 mb-6">
            {error}
          </div>
        )}

        {/* Inventory Table */}
        <div className="bg-white rounded-lg shadow-sm overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="px-4 py-3 text-left">
                    <input
                      type="checkbox"
                      checked={selectedItems.size === inventoryData?.items.length && inventoryData?.items.length > 0}
                      onChange={handleSelectAll}
                      className="rounded border-gray-300"
                    />
                  </th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Stock #</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">VIN</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Ubicación</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Precio Lista</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Estado</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Días</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Vistas</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Consultas</th>
                  <th className="px-4 py-3 text-right text-sm font-semibold text-gray-700">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {inventoryData?.items.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <input
                        type="checkbox"
                        checked={selectedItems.has(item.id)}
                        onChange={() => handleSelectItem(item.id)}
                        className="rounded border-gray-300"
                      />
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">{item.stockNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.vin || 'N/A'}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.location || 'N/A'}</td>
                    <td className="px-4 py-3 text-sm font-semibold text-gray-900">
                      {inventoryManagementService.formatCurrency(item.listPrice)}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${inventoryManagementService.getStatusColor(item.status)}`}>
                        {inventoryManagementService.getStatusLabel(item.status)}
                      </span>
                      {item.isHot && (
                        <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                          🔥 HOT
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">
                      {item.daysOnMarket}
                      {item.isOverdue && <span className="ml-1 text-red-600">⚠️</span>}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.viewCount}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.inquiryCount}</td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => navigate(`/dealer/inventory/${item.id}`)}
                          className="text-blue-600 hover:text-blue-800"
                          title="Ver"
                        >
                          <FiEye size={18} />
                        </button>
                        <button
                          onClick={() => navigate(`/dealer/inventory/${item.id}/edit`)}
                          className="text-gray-600 hover:text-gray-800"
                          title="Editar"
                        >
                          <FiEdit size={18} />
                        </button>
                        <button
                          onClick={async () => {
                            if (confirm('¿Eliminar este vehículo?')) {
                              await inventoryManagementService.deleteInventoryItem(item.id);
                              loadInventory();
                            }
                          }}
                          className="text-red-600 hover:text-red-800"
                          title="Eliminar"
                        >
                          <FiTrash2 size={18} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {inventoryData && inventoryData.totalPages > 1 && (
            <div className="px-4 py-3 bg-gray-50 border-t border-gray-200 flex items-center justify-between">
              <div className="text-sm text-gray-700">
                Mostrando {((page - 1) * pageSize) + 1} - {Math.min(page * pageSize, inventoryData.totalCount)} de {inventoryData.totalCount}
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="px-3 py-1 border border-gray-300 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                >
                  Anterior
                </button>
                <span className="px-3 py-1">
                  Página {page} de {inventoryData.totalPages}
                </span>
                <button
                  onClick={() => setPage(p => Math.min(inventoryData.totalPages, p + 1))}
                  disabled={page === inventoryData.totalPages}
                  className="px-3 py-1 border border-gray-300 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                >
                  Siguiente
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Empty State */}
        {inventoryData?.items.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg mb-4">No hay vehículos en el inventario</p>
            <button
              onClick={() => navigate('/dealer/inventory/new')}
              className="inline-flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition"
            >
              <FiPlus size={20} />
              <span>Agregar Primer Vehículo</span>
            </button>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
