import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import {
  FiKey,
  FiSearch,
  FiPlus,
  FiRefreshCw,
  FiFilter,
  FiAlertCircle,
  FiLayers,
  FiShield,
  FiPackage,
} from 'react-icons/fi';
import roleService from '../../services/roleService';
import type { Permission, CreatePermissionRequest } from '../../services/roleService';

const PermissionsManagementPage = () => {
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedModule, setSelectedModule] = useState<string>('all');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [isCreating, setIsCreating] = useState(false);

  // New permission form state
  const [newPermission, setNewPermission] = useState<CreatePermissionRequest>({
    name: '',
    displayName: '',
    description: '',
    module: '',
    resource: '',
    action: '',
  });

  const modules = roleService.getModules();
  const actions = roleService.getActions();

  // Fetch permissions
  const fetchPermissions = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await roleService.getPermissions();
      setPermissions(result);
    } catch (err) {
      setError('Error cargando permisos. Por favor, intente nuevamente.');
      console.error('Error fetching permissions:', err);
      // Fallback to mock data
      setPermissions(getMockPermissions());
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchPermissions();
  }, []);

  // Filter permissions
  const filteredPermissions = useMemo(() => {
    return permissions.filter((perm) => {
      const matchesSearch =
        perm.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        perm.displayName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (perm.description || '').toLowerCase().includes(searchTerm.toLowerCase());

      const matchesModule = selectedModule === 'all' || perm.module === selectedModule;

      return matchesSearch && matchesModule;
    });
  }, [permissions, searchTerm, selectedModule]);

  // Group permissions by module
  const groupedPermissions = useMemo(() => {
    const groups: { [key: string]: Permission[] } = {};
    filteredPermissions.forEach((perm) => {
      if (!groups[perm.module]) {
        groups[perm.module] = [];
      }
      groups[perm.module].push(perm);
    });
    return groups;
  }, [filteredPermissions]);

  // Update permission name based on resource:action
  useEffect(() => {
    if (newPermission.resource && newPermission.action) {
      setNewPermission((prev) => ({
        ...prev,
        name: `${prev.resource}:${prev.action}`,
      }));
    }
  }, [newPermission.resource, newPermission.action]);

  // Create permission handler
  const handleCreatePermission = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsCreating(true);
    try {
      await roleService.createPermission(newPermission);
      setShowCreateModal(false);
      setNewPermission({
        name: '',
        displayName: '',
        description: '',
        module: '',
        resource: '',
        action: '',
      });
      fetchPermissions();
    } catch (err) {
      console.error('Error creating permission:', err);
      alert('Error al crear el permiso. Por favor, intente nuevamente.');
    } finally {
      setIsCreating(false);
    }
  };

  // Stats
  const stats = useMemo(() => {
    const moduleCount: { [key: string]: number } = {};
    permissions.forEach((p) => {
      moduleCount[p.module] = (moduleCount[p.module] || 0) + 1;
    });
    return {
      total: permissions.length,
      modules: Object.keys(moduleCount).length,
      topModules: Object.entries(moduleCount)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 3),
    };
  }, [permissions]);

  // Module icon color
  const getModuleColor = (module: string) => {
    const colors: { [key: string]: string } = {
      vehicles: 'bg-blue-100 text-blue-600',
      users: 'bg-green-100 text-green-600',
      dealers: 'bg-purple-100 text-purple-600',
      billing: 'bg-yellow-100 text-yellow-600',
      admin: 'bg-red-100 text-red-600',
      kyc: 'bg-indigo-100 text-indigo-600',
      aml: 'bg-pink-100 text-pink-600',
      reports: 'bg-cyan-100 text-cyan-600',
    };
    return colors[module] || 'bg-gray-100 text-gray-600';
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-indigo-100 rounded-lg">
                <FiKey className="h-8 w-8 text-indigo-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Gestión de Permisos</h1>
                <p className="text-sm text-gray-500">Administra los permisos del sistema RBAC</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Link
                to="/admin/roles"
                className="px-4 py-2 text-purple-600 bg-purple-50 border border-purple-200 rounded-lg hover:bg-purple-100 transition-colors flex items-center gap-2"
              >
                <FiShield />
                Ver Roles
              </Link>
              <button
                onClick={fetchPermissions}
                className="px-4 py-2 text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
              >
                <FiRefreshCw className={isLoading ? 'animate-spin' : ''} />
                Actualizar
              </button>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors flex items-center gap-2"
              >
                <FiPlus />
                Nuevo Permiso
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Total Permisos</p>
                <p className="text-3xl font-bold text-gray-900 mt-1">{stats.total}</p>
              </div>
              <div className="p-3 bg-indigo-100 rounded-lg">
                <FiKey className="h-6 w-6 text-indigo-600" />
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-500">Módulos</p>
                <p className="text-3xl font-bold text-blue-600 mt-1">{stats.modules}</p>
              </div>
              <div className="p-3 bg-blue-100 rounded-lg">
                <FiLayers className="h-6 w-6 text-blue-600" />
              </div>
            </div>
          </div>
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <div>
              <p className="text-sm font-medium text-gray-500 mb-2">Top Módulos</p>
              <div className="flex gap-2 flex-wrap">
                {stats.topModules.map(([module, count]) => (
                  <span
                    key={module}
                    className={`px-2 py-1 text-xs font-medium rounded-full ${getModuleColor(module)}`}
                  >
                    {module} ({count})
                  </span>
                ))}
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
                placeholder="Buscar permisos..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
            <div className="flex items-center gap-2">
              <FiFilter className="text-gray-400" />
              <select
                value={selectedModule}
                onChange={(e) => setSelectedModule(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              >
                <option value="all">Todos los módulos</option>
                {modules.map((mod) => (
                  <option key={mod} value={mod}>
                    {mod.charAt(0).toUpperCase() + mod.slice(1)}
                  </option>
                ))}
              </select>
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

        {/* Permissions by Module */}
        {isLoading ? (
          <div className="bg-white rounded-xl shadow-sm p-12 text-center">
            <FiRefreshCw className="h-8 w-8 text-indigo-600 animate-spin mx-auto mb-4" />
            <p className="text-gray-500">Cargando permisos...</p>
          </div>
        ) : filteredPermissions.length === 0 ? (
          <div className="bg-white rounded-xl shadow-sm p-12 text-center">
            <FiKey className="h-12 w-12 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No se encontraron permisos</h3>
            <p className="text-gray-500">Intente con otros filtros o cree un nuevo permiso.</p>
          </div>
        ) : (
          <div className="space-y-6">
            {Object.entries(groupedPermissions).map(([module, perms]) => (
              <div
                key={module}
                className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
              >
                <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-lg ${getModuleColor(module)}`}>
                      <FiPackage className="h-5 w-5" />
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900 capitalize">{module}</h3>
                      <p className="text-sm text-gray-500">{perms.length} permisos</p>
                    </div>
                  </div>
                </div>
                <div className="divide-y divide-gray-100">
                  {perms.map((perm) => (
                    <div key={perm.id} className="px-6 py-4 hover:bg-gray-50">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-4">
                          <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-indigo-100 flex items-center justify-center">
                            <FiKey className="h-5 w-5 text-indigo-600" />
                          </div>
                          <div>
                            <div className="text-sm font-medium text-gray-900">
                              {perm.displayName}
                            </div>
                            <div className="flex items-center gap-2 mt-1">
                              <code className="text-xs bg-gray-100 px-2 py-1 rounded text-gray-600">
                                {perm.name}
                              </code>
                              {perm.description && (
                                <span className="text-xs text-gray-500">• {perm.description}</span>
                              )}
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="px-2 py-1 text-xs font-medium rounded-full bg-blue-100 text-blue-700">
                            {perm.resource}
                          </span>
                          <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-700">
                            {perm.action}
                          </span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Info box */}
        <div className="mt-8 bg-blue-50 border border-blue-200 rounded-xl p-6">
          <div className="flex items-start gap-4">
            <div className="p-2 bg-blue-100 rounded-lg">
              <FiAlertCircle className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">Convención de Permisos</h4>
              <p className="text-blue-800 text-sm">
                Los permisos siguen el formato{' '}
                <code className="bg-blue-100 px-1 rounded">resource:action</code>. Por ejemplo:{' '}
                <code className="bg-blue-100 px-1 rounded">vehicles:create</code> permite crear
                vehículos. Los permisos se asignan a roles, y los roles se asignan a usuarios.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Create Permission Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-lg w-full mx-4 max-h-[90vh] overflow-y-auto">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-lg font-semibold text-gray-900">Crear Nuevo Permiso</h2>
            </div>
            <form onSubmit={handleCreatePermission} className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Módulo</label>
                  <select
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    value={newPermission.module}
                    onChange={(e) => setNewPermission({ ...newPermission, module: e.target.value })}
                  >
                    <option value="">Seleccionar...</option>
                    {modules.map((mod) => (
                      <option key={mod} value={mod}>
                        {mod.charAt(0).toUpperCase() + mod.slice(1)}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Recurso</label>
                  <input
                    type="text"
                    required
                    placeholder="ej: vehicles"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    value={newPermission.resource}
                    onChange={(e) =>
                      setNewPermission({ ...newPermission, resource: e.target.value.toLowerCase() })
                    }
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Acción</label>
                <select
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  value={newPermission.action}
                  onChange={(e) => setNewPermission({ ...newPermission, action: e.target.value })}
                >
                  <option value="">Seleccionar...</option>
                  {actions.map((act) => (
                    <option key={act} value={act}>
                      {act}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Permiso (automático)
                </label>
                <input
                  type="text"
                  readOnly
                  className="w-full px-4 py-2 border border-gray-200 rounded-lg bg-gray-50 text-gray-500"
                  value={newPermission.name}
                  placeholder="resource:action"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de Visualización
                </label>
                <input
                  type="text"
                  required
                  placeholder="ej: Crear Vehículos"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  value={newPermission.displayName}
                  onChange={(e) =>
                    setNewPermission({ ...newPermission, displayName: e.target.value })
                  }
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Descripción (opcional)
                </label>
                <textarea
                  placeholder="Describe qué permite este permiso..."
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  rows={2}
                  value={newPermission.description}
                  onChange={(e) =>
                    setNewPermission({ ...newPermission, description: e.target.value })
                  }
                />
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
                  disabled={isCreating || !newPermission.name}
                  className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
                >
                  {isCreating ? (
                    <>
                      <FiRefreshCw className="animate-spin" />
                      Creando...
                    </>
                  ) : (
                    <>
                      <FiPlus />
                      Crear Permiso
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
function getMockPermissions(): Permission[] {
  const modules = ['vehicles', 'users', 'dealers', 'billing', 'admin', 'kyc'];
  const actions = ['create', 'read', 'update', 'delete'];
  const permissions: Permission[] = [];
  let id = 1;

  modules.forEach((module) => {
    actions.forEach((action) => {
      permissions.push({
        id: String(id++),
        name: `${module}:${action}`,
        displayName: `${capitalize(action)} ${capitalize(module)}`,
        description: `Permite ${action} en ${module}`,
        module,
        resource: module,
        action,
      });
    });
  });

  // Add some extra permissions
  permissions.push({
    id: String(id++),
    name: 'vehicles:publish',
    displayName: 'Publicar Vehículos',
    description: 'Permite publicar y despublicar vehículos',
    module: 'vehicles',
    resource: 'vehicles',
    action: 'publish',
  });
  permissions.push({
    id: String(id++),
    name: 'vehicles:feature',
    displayName: 'Destacar Vehículos',
    description: 'Permite marcar vehículos como destacados',
    module: 'vehicles',
    resource: 'vehicles',
    action: 'feature',
  });
  permissions.push({
    id: String(id++),
    name: 'admin:manage-roles',
    displayName: 'Gestionar Roles',
    description: 'Permite crear, editar y eliminar roles',
    module: 'admin',
    resource: 'admin',
    action: 'manage-roles',
  });
  permissions.push({
    id: String(id++),
    name: 'admin:manage-permissions',
    displayName: 'Gestionar Permisos',
    description: 'Permite crear permisos y asignarlos a roles',
    module: 'admin',
    resource: 'admin',
    action: 'manage-permissions',
  });

  return permissions;
}

function capitalize(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1);
}

export default PermissionsManagementPage;
