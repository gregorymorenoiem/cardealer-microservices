import { useState, useEffect, useMemo, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  FiShield,
  FiKey,
  FiArrowLeft,
  FiEdit2,
  FiSave,
  FiX,
  FiPlus,
  FiTrash2,
  FiRefreshCw,
  FiUsers,
  FiCheck,
  FiAlertCircle,
  FiSearch,
  FiClock,
} from 'react-icons/fi';
import roleService from '../../services/roleService';
import type { RoleDetails, Permission, UpdateRoleRequest } from '../../services/roleService';

const RoleDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [role, setRole] = useState<RoleDetails | null>(null);
  const [allPermissions, setAllPermissions] = useState<Permission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);

  // Edit form state
  const [editForm, setEditForm] = useState<UpdateRoleRequest>({
    displayName: '',
    description: '',
    isActive: true,
  });

  // Selected permissions for adding
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  // Fetch role details
  const fetchRole = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    setError(null);
    try {
      const [roleData, permsData] = await Promise.all([
        roleService.getRoleById(id),
        roleService.getPermissions(),
      ]);
      setRole(roleData);
      setAllPermissions(permsData);
      setEditForm({
        displayName: roleData.displayName,
        description: roleData.description || '',
        isActive: roleData.isActive,
      });
    } catch (err) {
      setError('Error cargando el rol. Por favor, intente nuevamente.');
      console.error('Error fetching role:', err);
      // Fallback to mock data
      setRole(getMockRole());
      setAllPermissions(getMockPermissions());
      setEditForm({
        displayName: 'Administrador',
        description: 'Gestión de la plataforma',
        isActive: true,
      });
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchRole();
  }, [fetchRole]);

  // Available permissions (not already assigned)
  const availablePermissions = useMemo(() => {
    if (!role) return [];
    const assignedIds = new Set(role.permissions.map((p) => p.id));
    return allPermissions.filter((p) => !assignedIds.has(p.id));
  }, [role, allPermissions]);

  // Filtered available permissions for modal
  const filteredAvailablePermissions = useMemo(() => {
    return availablePermissions.filter(
      (p) =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.displayName.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [availablePermissions, searchTerm]);

  // Group permissions by module
  const groupedPermissions = useMemo(() => {
    if (!role) return {};
    const groups: { [key: string]: Permission[] } = {};
    role.permissions.forEach((perm) => {
      if (!groups[perm.module]) {
        groups[perm.module] = [];
      }
      groups[perm.module].push(perm);
    });
    return groups;
  }, [role]);

  // Save role changes
  const handleSave = async () => {
    if (!id || !role) return;
    setIsSaving(true);
    try {
      await roleService.updateRole(id, editForm);
      setIsEditing(false);
      fetchRole();
    } catch (err) {
      console.error('Error updating role:', err);
      alert('Error al actualizar el rol. Por favor, intente nuevamente.');
    } finally {
      setIsSaving(false);
    }
  };

  // Remove permission from role
  const handleRemovePermission = async (permissionId: string) => {
    if (!id) return;
    if (!confirm('¿Está seguro de remover este permiso del rol?')) return;
    try {
      await roleService.removePermission({ roleId: id, permissionId });
      fetchRole();
    } catch (err) {
      console.error('Error removing permission:', err);
      alert('Error al remover el permiso.');
    }
  };

  // Bulk assign permissions
  const handleBulkAssign = async () => {
    if (!id || selectedPermissions.length === 0) return;
    try {
      await Promise.all(
        selectedPermissions.map((permId) =>
          roleService.assignPermission({ roleId: id, permissionId: permId })
        )
      );
      setSelectedPermissions([]);
      setShowAddModal(false);
      fetchRole();
    } catch (err) {
      console.error('Error assigning permissions:', err);
      alert('Error al asignar los permisos.');
    }
  };

  // Module color
  const getModuleColor = (module: string) => {
    const colors: { [key: string]: string } = {
      vehicles: 'bg-blue-100 text-blue-600 border-blue-200',
      users: 'bg-green-100 text-green-600 border-green-200',
      dealers: 'bg-purple-100 text-purple-600 border-purple-200',
      billing: 'bg-yellow-100 text-yellow-600 border-yellow-200',
      admin: 'bg-red-100 text-red-600 border-red-200',
      kyc: 'bg-indigo-100 text-indigo-600 border-indigo-200',
      aml: 'bg-pink-100 text-pink-600 border-pink-200',
      reports: 'bg-cyan-100 text-cyan-600 border-cyan-200',
    };
    return colors[module] || 'bg-gray-100 text-gray-600 border-gray-200';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <FiRefreshCw className="h-8 w-8 text-purple-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-500">Cargando rol...</p>
        </div>
      </div>
    );
  }

  if (error && !role) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <FiAlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
          <h2 className="text-lg font-medium text-gray-900 mb-2">Error</h2>
          <p className="text-gray-500 mb-4">{error}</p>
          <button
            onClick={() => navigate('/admin/roles')}
            className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700"
          >
            Volver a Roles
          </button>
        </div>
      </div>
    );
  }

  if (!role) return null;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                to="/admin/roles"
                className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <FiArrowLeft className="h-5 w-5" />
              </Link>
              <div className="flex items-center gap-3">
                <div className="p-3 bg-purple-100 rounded-lg">
                  <FiShield className="h-8 w-8 text-purple-600" />
                </div>
                <div>
                  {isEditing ? (
                    <input
                      type="text"
                      className="text-2xl font-bold text-gray-900 border-b-2 border-purple-500 focus:outline-none bg-transparent"
                      value={editForm.displayName}
                      onChange={(e) => setEditForm({ ...editForm, displayName: e.target.value })}
                    />
                  ) : (
                    <h1 className="text-2xl font-bold text-gray-900">{role.displayName}</h1>
                  )}
                  <div className="flex items-center gap-2 mt-1">
                    <code className="text-sm text-gray-500 bg-gray-100 px-2 py-0.5 rounded">
                      {role.name}
                    </code>
                    {role.isSystem && (
                      <span className="px-2 py-0.5 text-xs font-medium bg-blue-100 text-blue-700 rounded-full">
                        Sistema
                      </span>
                    )}
                    {role.isActive ? (
                      <span className="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-700 rounded-full">
                        Activo
                      </span>
                    ) : (
                      <span className="px-2 py-0.5 text-xs font-medium bg-gray-100 text-gray-600 rounded-full">
                        Inactivo
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>
            <div className="flex items-center gap-3">
              {isEditing ? (
                <>
                  <button
                    onClick={() => setIsEditing(false)}
                    className="px-4 py-2 text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
                  >
                    <FiX />
                    Cancelar
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={isSaving}
                    className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors flex items-center gap-2 disabled:opacity-50"
                  >
                    {isSaving ? <FiRefreshCw className="animate-spin" /> : <FiSave />}
                    Guardar
                  </button>
                </>
              ) : (
                !role.isSystem && (
                  <button
                    onClick={() => setIsEditing(true)}
                    className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors flex items-center gap-2"
                  >
                    <FiEdit2 />
                    Editar Rol
                  </button>
                )
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column - Role Info */}
          <div className="lg:col-span-1 space-y-6">
            {/* Description Card */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Información del Rol</h3>
              {isEditing ? (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Descripción
                    </label>
                    <textarea
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                      rows={3}
                      value={editForm.description}
                      onChange={(e) => setEditForm({ ...editForm, description: e.target.value })}
                    />
                  </div>
                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="isActive"
                      checked={editForm.isActive}
                      onChange={(e) => setEditForm({ ...editForm, isActive: e.target.checked })}
                      className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"
                    />
                    <label htmlFor="isActive" className="ml-2 block text-sm text-gray-900">
                      Rol activo
                    </label>
                  </div>
                </div>
              ) : (
                <p className="text-gray-600">{role.description || 'Sin descripción'}</p>
              )}
            </div>

            {/* Stats Card */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Estadísticas</h3>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-gray-600">
                    <FiUsers className="h-5 w-5" />
                    <span>Usuarios asignados</span>
                  </div>
                  <span className="text-2xl font-bold text-gray-900">{role.userCount ?? 0}</span>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-gray-600">
                    <FiKey className="h-5 w-5" />
                    <span>Permisos</span>
                  </div>
                  <span className="text-2xl font-bold text-gray-900">
                    {role.permissions.length}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-gray-600">
                    <FiClock className="h-5 w-5" />
                    <span>Creado</span>
                  </div>
                  <span className="text-sm text-gray-500">
                    {new Date(role.createdAt).toLocaleDateString('es-DO', {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                    })}
                  </span>
                </div>
              </div>
            </div>

            {/* Warning for system roles */}
            {role.isSystem && (
              <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-4">
                <div className="flex items-start gap-3">
                  <FiAlertCircle className="h-5 w-5 text-yellow-600 mt-0.5" />
                  <div>
                    <h4 className="font-medium text-yellow-800">Rol del Sistema</h4>
                    <p className="text-sm text-yellow-700 mt-1">
                      Este rol es parte del sistema y no puede ser modificado ni eliminado.
                    </p>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Right Column - Permissions */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
              <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Permisos Asignados</h3>
                  <p className="text-sm text-gray-500">{role.permissions.length} permisos</p>
                </div>
                {!role.isSystem && (
                  <button
                    onClick={() => setShowAddModal(true)}
                    className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors flex items-center gap-2"
                  >
                    <FiPlus />
                    Agregar Permisos
                  </button>
                )}
              </div>

              {role.permissions.length === 0 ? (
                <div className="p-12 text-center">
                  <FiKey className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-gray-900 mb-2">Sin permisos asignados</h3>
                  <p className="text-gray-500 mb-4">
                    Este rol no tiene permisos. Agregue permisos para definir qué acciones pueden
                    realizar los usuarios.
                  </p>
                  {!role.isSystem && (
                    <button
                      onClick={() => setShowAddModal(true)}
                      className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700"
                    >
                      Agregar Permisos
                    </button>
                  )}
                </div>
              ) : (
                <div className="divide-y divide-gray-100">
                  {Object.entries(groupedPermissions).map(([module, perms]) => (
                    <div key={module} className="p-4">
                      <div className="flex items-center gap-2 mb-3">
                        <span
                          className={`px-3 py-1 text-sm font-medium rounded-full capitalize ${getModuleColor(
                            module
                          )}`}
                        >
                          {module}
                        </span>
                        <span className="text-sm text-gray-500">{perms.length} permisos</span>
                      </div>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                        {perms.map((perm) => (
                          <div
                            key={perm.id}
                            className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 group"
                          >
                            <div className="flex items-center gap-2">
                              <FiCheck className="h-4 w-4 text-green-500" />
                              <div>
                                <div className="text-sm font-medium text-gray-900">
                                  {perm.displayName}
                                </div>
                                <code className="text-xs text-gray-500">{perm.name}</code>
                              </div>
                            </div>
                            {!role.isSystem && (
                              <button
                                onClick={() => handleRemovePermission(perm.id)}
                                className="p-1 text-gray-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-all"
                                title="Remover permiso"
                              >
                                <FiTrash2 className="h-4 w-4" />
                              </button>
                            )}
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Add Permissions Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-2xl w-full mx-4 max-h-[80vh] flex flex-col">
            <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
              <div>
                <h2 className="text-lg font-semibold text-gray-900">Agregar Permisos</h2>
                <p className="text-sm text-gray-500">
                  Seleccione los permisos a agregar al rol {role.displayName}
                </p>
              </div>
              <button
                onClick={() => {
                  setShowAddModal(false);
                  setSelectedPermissions([]);
                  setSearchTerm('');
                }}
                className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg"
              >
                <FiX className="h-5 w-5" />
              </button>
            </div>

            {/* Search */}
            <div className="px-6 py-3 border-b border-gray-100">
              <div className="relative">
                <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar permisos..."
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </div>

            {/* Permissions List */}
            <div className="flex-1 overflow-y-auto p-6">
              {filteredAvailablePermissions.length === 0 ? (
                <div className="text-center py-8">
                  <FiKey className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500">No hay permisos disponibles para agregar.</p>
                </div>
              ) : (
                <div className="space-y-2">
                  {filteredAvailablePermissions.map((perm) => {
                    const isSelected = selectedPermissions.includes(perm.id);
                    return (
                      <label
                        key={perm.id}
                        className={`flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-colors ${
                          isSelected
                            ? 'bg-purple-50 border-2 border-purple-300'
                            : 'bg-gray-50 hover:bg-gray-100 border-2 border-transparent'
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => {
                            if (isSelected) {
                              setSelectedPermissions(
                                selectedPermissions.filter((id) => id !== perm.id)
                              );
                            } else {
                              setSelectedPermissions([...selectedPermissions, perm.id]);
                            }
                          }}
                          className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded"
                        />
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <span className="text-sm font-medium text-gray-900">
                              {perm.displayName}
                            </span>
                            <span
                              className={`px-2 py-0.5 text-xs font-medium rounded-full capitalize ${getModuleColor(
                                perm.module
                              )}`}
                            >
                              {perm.module}
                            </span>
                          </div>
                          <code className="text-xs text-gray-500">{perm.name}</code>
                        </div>
                      </label>
                    );
                  })}
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
              <span className="text-sm text-gray-500">
                {selectedPermissions.length} permisos seleccionados
              </span>
              <div className="flex gap-3">
                <button
                  onClick={() => {
                    setShowAddModal(false);
                    setSelectedPermissions([]);
                    setSearchTerm('');
                  }}
                  className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  onClick={handleBulkAssign}
                  disabled={selectedPermissions.length === 0}
                  className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors disabled:opacity-50 flex items-center gap-2"
                >
                  <FiPlus />
                  Agregar ({selectedPermissions.length})
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Mock data for demo
function getMockRole(): RoleDetails {
  return {
    id: '2',
    name: 'Admin',
    displayName: 'Administrador',
    description: 'Gestión de la plataforma',
    isActive: true,
    isSystem: true,
    userCount: 5,
    permissionCount: 40,
    createdAt: '2026-01-01T00:00:00Z',
    permissions: [
      {
        id: '1',
        name: 'vehicles:create',
        displayName: 'Crear Vehículos',
        module: 'vehicles',
        resource: 'vehicles',
        action: 'create',
      },
      {
        id: '2',
        name: 'vehicles:read',
        displayName: 'Ver Vehículos',
        module: 'vehicles',
        resource: 'vehicles',
        action: 'read',
      },
      {
        id: '3',
        name: 'vehicles:update',
        displayName: 'Actualizar Vehículos',
        module: 'vehicles',
        resource: 'vehicles',
        action: 'update',
      },
      {
        id: '4',
        name: 'vehicles:delete',
        displayName: 'Eliminar Vehículos',
        module: 'vehicles',
        resource: 'vehicles',
        action: 'delete',
      },
      {
        id: '5',
        name: 'users:create',
        displayName: 'Crear Usuarios',
        module: 'users',
        resource: 'users',
        action: 'create',
      },
      {
        id: '6',
        name: 'users:read',
        displayName: 'Ver Usuarios',
        module: 'users',
        resource: 'users',
        action: 'read',
      },
      {
        id: '7',
        name: 'users:update',
        displayName: 'Actualizar Usuarios',
        module: 'users',
        resource: 'users',
        action: 'update',
      },
      {
        id: '8',
        name: 'admin:access',
        displayName: 'Acceso Admin',
        module: 'admin',
        resource: 'admin',
        action: 'access',
      },
      {
        id: '9',
        name: 'admin:manage-roles',
        displayName: 'Gestionar Roles',
        module: 'admin',
        resource: 'admin',
        action: 'manage-roles',
      },
    ],
  };
}

function getMockPermissions(): Permission[] {
  return [
    {
      id: '10',
      name: 'vehicles:publish',
      displayName: 'Publicar Vehículos',
      module: 'vehicles',
      resource: 'vehicles',
      action: 'publish',
    },
    {
      id: '11',
      name: 'vehicles:feature',
      displayName: 'Destacar Vehículos',
      module: 'vehicles',
      resource: 'vehicles',
      action: 'feature',
    },
    {
      id: '12',
      name: 'users:ban',
      displayName: 'Banear Usuarios',
      module: 'users',
      resource: 'users',
      action: 'ban',
    },
    {
      id: '13',
      name: 'dealers:create',
      displayName: 'Crear Dealers',
      module: 'dealers',
      resource: 'dealers',
      action: 'create',
    },
    {
      id: '14',
      name: 'dealers:verify',
      displayName: 'Verificar Dealers',
      module: 'dealers',
      resource: 'dealers',
      action: 'verify',
    },
    {
      id: '15',
      name: 'billing:read',
      displayName: 'Ver Facturación',
      module: 'billing',
      resource: 'billing',
      action: 'read',
    },
    {
      id: '16',
      name: 'billing:refund',
      displayName: 'Reembolsos',
      module: 'billing',
      resource: 'billing',
      action: 'refund',
    },
    {
      id: '17',
      name: 'kyc:approve',
      displayName: 'Aprobar KYC',
      module: 'kyc',
      resource: 'kyc',
      action: 'approve',
    },
    {
      id: '18',
      name: 'kyc:reject',
      displayName: 'Rechazar KYC',
      module: 'kyc',
      resource: 'kyc',
      action: 'reject',
    },
    {
      id: '19',
      name: 'reports:create',
      displayName: 'Crear Reportes',
      module: 'reports',
      resource: 'reports',
      action: 'create',
    },
    {
      id: '20',
      name: 'reports:export',
      displayName: 'Exportar Reportes',
      module: 'reports',
      resource: 'reports',
      action: 'export',
    },
  ];
}

export default RoleDetailPage;
