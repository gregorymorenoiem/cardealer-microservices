import { useState, useEffect, useMemo } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  FiShield,
  FiSearch,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiEye,
  FiRefreshCw,
  FiUsers,
  FiKey,
  FiCheck,
  FiX,
  FiAlertCircle,
} from 'react-icons/fi';
import roleService from '../../services/roleService';
import type { Role, CreateRoleRequest } from '../../services/roleService';

type FilterStatus = 'all' | 'active' | 'inactive' | 'system';

const RolesManagementPage = () => {
  const navigate = useNavigate();
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<FilterStatus>('all');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  // New role form state
  const [newRole, setNewRole] = useState<CreateRoleRequest>({
    name: '',
    displayName: '',
    description: '',
    isActive: true,
  });

  // Fetch roles
  const fetchRoles = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await roleService.getRoles({ pageSize: 100 });
      setRoles(result.items);
    } catch (err) {
      setError('Error cargando roles. Por favor, intente nuevamente.');
      console.error('Error fetching roles:', err);
      // Fallback to mock data for demo
      setRoles(getMockRoles());
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchRoles();
  }, []);

  // Filter roles
  const filteredRoles = useMemo(() => {
    return roles.filter((role) => {
      // Search filter
      const matchesSearch =
        role.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        role.displayName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (role.description || '').toLowerCase().includes(searchTerm.toLowerCase());

      // Status filter
      let matchesStatus = true;
      if (filterStatus === 'active') {
        matchesStatus = role.isActive;
      } else if (filterStatus === 'inactive') {
        matchesStatus = !role.isActive;
      } else if (filterStatus === 'system') {
        matchesStatus = role.isSystem;
      }

      return matchesSearch && matchesStatus;
    });
  }, [roles, searchTerm, filterStatus]);

  // Create role handler
  const handleCreateRole = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsCreating(true);
    try {
      await roleService.createRole(newRole);
      setShowCreateModal(false);
      setNewRole({ name: '', displayName: '', description: '', isActive: true });
      fetchRoles();
    } catch (err) {
      console.error('Error creating role:', err);
      alert('Error al crear el rol. Por favor, intente nuevamente.');
    } finally {
      setIsCreating(false);
    }
  };

  // Delete role handler
  const handleDeleteRole = async (roleId: string, roleName: string) => {
    if (
      !confirm(`¿Está seguro de eliminar el rol "${roleName}"? Esta acción no se puede deshacer.`)
    ) {
      return;
    }
    try {
      await roleService.deleteRole(roleId);
      fetchRoles();
    } catch (err) {
      console.error('Error deleting role:', err);
      alert('Error al eliminar el rol. Es posible que tenga usuarios asignados.');
    }
  };

  // Stats
  const stats = useMemo(() => {
    return {
      total: roles.length,
      active: roles.filter((r) => r.isActive).length,
      system: roles.filter((r) => r.isSystem).length,
      custom: roles.filter((r) => !r.isSystem).length,
    };
  }, [roles]);

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-purple-100 rounded-lg">
                <FiShield className="h-8 w-8 text-purple-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Gestión de Roles</h1>
                <p className="text-sm text-gray-500">Administra los roles del sistema RBAC</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <button
                onClick={fetchRoles}
                className="px-4 py-2 text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
              >
                <FiRefreshCw className={isLoading ? 'animate-spin' : ''} />
                Actualizar
              </button>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors flex items-center gap-2"
              >
                <FiPlus />
                Nuevo Rol
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Total Roles</p>
                <p className="text-3xl font-bold text-gray-900 mt-1">{stats.total}</p>
              </div>
              <div className="p-3 bg-purple-100 rounded-lg">
                <FiShield className="h-6 w-6 text-purple-600" />
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Activos</p>
                <p className="text-3xl font-bold text-green-600 mt-1">{stats.active}</p>
              </div>
              <div className="p-3 bg-green-100 rounded-lg">
                <FiCheck className="h-6 w-6 text-green-600" />
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Del Sistema</p>
                <p className="text-3xl font-bold text-blue-600 mt-1">{stats.system}</p>
              </div>
              <div className="p-3 bg-blue-100 rounded-lg">
                <FiKey className="h-6 w-6 text-blue-600" />
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Personalizados</p>
                <p className="text-3xl font-bold text-orange-600 mt-1">{stats.custom}</p>
              </div>
              <div className="p-3 bg-orange-100 rounded-lg">
                <FiEdit2 className="h-6 w-6 text-orange-600" />
              </div>
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 mb-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1 relative">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar roles por nombre o descripción..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <div className="flex gap-2">
              {(['all', 'active', 'inactive', 'system'] as FilterStatus[]).map((status) => (
                <button
                  key={status}
                  onClick={() => setFilterStatus(status)}
                  className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                    filterStatus === status
                      ? 'bg-purple-600 text-white'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  {status === 'all' && 'Todos'}
                  {status === 'active' && 'Activos'}
                  {status === 'inactive' && 'Inactivos'}
                  {status === 'system' && 'Sistema'}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6 flex items-center gap-2">
            <FiAlertCircle />
            {error}
          </div>
        )}

        {/* Roles Table */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          {isLoading ? (
            <div className="p-12 text-center">
              <FiRefreshCw className="h-8 w-8 text-purple-600 animate-spin mx-auto mb-4" />
              <p className="text-gray-500">Cargando roles...</p>
            </div>
          ) : filteredRoles.length === 0 ? (
            <div className="p-12 text-center">
              <FiShield className="h-12 w-12 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No se encontraron roles</h3>
              <p className="text-gray-500">Intente con otros filtros o cree un nuevo rol.</p>
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Estado
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tipo
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Usuarios
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Permisos
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredRoles.map((role) => (
                  <tr key={role.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="flex-shrink-0 h-10 w-10 rounded-lg bg-purple-100 flex items-center justify-center">
                          <FiShield className="h-5 w-5 text-purple-600" />
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900">
                            {role.displayName}
                          </div>
                          <div className="text-sm text-gray-500">{role.name}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {role.isActive ? (
                        <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                          <FiCheck className="mr-1" /> Activo
                        </span>
                      ) : (
                        <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-100 text-gray-600">
                          <FiX className="mr-1" /> Inactivo
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      {role.isSystem ? (
                        <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800">
                          Sistema
                        </span>
                      ) : (
                        <span className="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-orange-100 text-orange-800">
                          Personalizado
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center text-sm text-gray-900">
                        <FiUsers className="mr-2 text-gray-400" />
                        {role.userCount ?? 0}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center text-sm text-gray-900">
                        <FiKey className="mr-2 text-gray-400" />
                        {role.permissionCount ?? 0}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex items-center justify-end gap-2">
                        <Link
                          to={`/admin/roles/${role.id}`}
                          className="p-2 text-purple-600 hover:bg-purple-50 rounded-lg transition-colors"
                          title="Ver detalles"
                        >
                          <FiEye className="h-4 w-4" />
                        </Link>
                        {!role.isSystem && (
                          <>
                            <button
                              onClick={() => navigate(`/admin/roles/${role.id}/edit`)}
                              className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                              title="Editar"
                            >
                              <FiEdit2 className="h-4 w-4" />
                            </button>
                            <button
                              onClick={() => handleDeleteRole(role.id, role.displayName)}
                              className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                              title="Eliminar"
                            >
                              <FiTrash2 className="h-4 w-4" />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Link to Permissions */}
        <div className="mt-8 bg-gradient-to-r from-purple-600 to-indigo-600 rounded-xl p-6 text-white">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-semibold mb-1">¿Necesitas gestionar permisos?</h3>
              <p className="text-purple-100">
                Crea nuevos permisos o revisa los existentes para asignarlos a los roles.
              </p>
            </div>
            <Link
              to="/admin/permissions"
              className="px-6 py-3 bg-white text-purple-600 rounded-lg font-medium hover:bg-purple-50 transition-colors flex items-center gap-2"
            >
              <FiKey />
              Gestionar Permisos
            </Link>
          </div>
        </div>
      </div>

      {/* Create Role Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full mx-4">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-lg font-semibold text-gray-900">Crear Nuevo Rol</h2>
            </div>
            <form onSubmit={handleCreateRole} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Rol (interno)
                </label>
                <input
                  type="text"
                  required
                  placeholder="ej: DealerManager"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  value={newRole.name}
                  onChange={(e) =>
                    setNewRole({ ...newRole, name: e.target.value.replace(/\s/g, '') })
                  }
                />
                <p className="text-xs text-gray-500 mt-1">Sin espacios, solo letras y números</p>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de Visualización
                </label>
                <input
                  type="text"
                  required
                  placeholder="ej: Gerente de Concesionario"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  value={newRole.displayName}
                  onChange={(e) => setNewRole({ ...newRole, displayName: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Descripción (opcional)
                </label>
                <textarea
                  placeholder="Describe las responsabilidades de este rol..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  rows={3}
                  value={newRole.description}
                  onChange={(e) => setNewRole({ ...newRole, description: e.target.value })}
                />
              </div>
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={newRole.isActive}
                  onChange={(e) => setNewRole({ ...newRole, isActive: e.target.checked })}
                  className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"
                />
                <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                  Rol activo
                </label>
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={isCreating}
                  className="flex-1 px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {isCreating ? (
                    <>
                      <FiRefreshCw className="animate-spin" />
                      Creando...
                    </>
                  ) : (
                    <>
                      <FiPlus />
                      Crear Rol
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

// Mock data for demo
function getMockRoles(): Role[] {
  return [
    {
      id: '1',
      name: 'SuperAdmin',
      displayName: 'Super Administrador',
      description: 'Control total del sistema',
      isActive: true,
      isSystem: true,
      userCount: 2,
      permissionCount: 50,
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: '2',
      name: 'Admin',
      displayName: 'Administrador',
      description: 'Gestión de la plataforma',
      isActive: true,
      isSystem: true,
      userCount: 5,
      permissionCount: 40,
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: '3',
      name: 'DealerOwner',
      displayName: 'Dueño de Concesionario',
      description: 'Acceso completo a su dealer',
      isActive: true,
      isSystem: true,
      userCount: 12,
      permissionCount: 25,
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: '4',
      name: 'Seller',
      displayName: 'Vendedor Individual',
      description: 'Venta de vehículos personales',
      isActive: true,
      isSystem: true,
      userCount: 150,
      permissionCount: 10,
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: '5',
      name: 'Buyer',
      displayName: 'Comprador',
      description: 'Búsqueda y compra de vehículos',
      isActive: true,
      isSystem: true,
      userCount: 500,
      permissionCount: 5,
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: '6',
      name: 'ComplianceOfficer',
      displayName: 'Oficial de Compliance',
      description: 'Reportes AML/KYC',
      isActive: true,
      isSystem: true,
      userCount: 3,
      permissionCount: 15,
      createdAt: '2026-01-01T00:00:00Z',
    },
  ];
}

export default RolesManagementPage;
