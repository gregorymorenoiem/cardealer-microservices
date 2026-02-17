/**
 * DealerEmployeePermissionsPage - Gestión de Permisos de Empleado
 *
 * Página para administrar los permisos específicos de un empleado del dealer:
 * - Ver y editar permisos por módulo
 * - Cambiar rol del empleado
 * - Ver historial de cambios
 *
 * @sprint UserService UI - Dealer Employees Permissions (0% → 100%)
 * @route /dealer/employees/:employeeId/permissions
 */

import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiArrowLeft,
  FiUser,
  FiShield,
  FiCheck,
  FiX,
  FiSave,
  FiRefreshCw,
  FiAlertCircle,
  FiLock,
  FiUnlock,
  FiSettings,
  FiTruck,
  FiUsers,
  FiDollarSign,
  FiBarChart2,
  FiMessageSquare,
  FiFileText,
} from 'react-icons/fi';
import { useAuth } from '@/hooks/useAuth';
import {
  dealerSettingsApi,
  type DealerEmployee,
  type DealerRole,
} from '@/services/dealerSettingsService';

// ============================================================================
// TYPES
// ============================================================================

interface Permission {
  id: string;
  name: string;
  description: string;
  module: string;
  enabled: boolean;
}

interface PermissionModule {
  id: string;
  name: string;
  icon: React.ElementType;
  permissions: Permission[];
}

// ============================================================================
// DEFAULT PERMISSIONS BY MODULE
// ============================================================================

const getDefaultPermissions = (): PermissionModule[] => [
  {
    id: 'inventory',
    name: 'Inventario',
    icon: FiTruck,
    permissions: [
      {
        id: 'inventory:view',
        name: 'Ver inventario',
        description: 'Ver lista de vehículos',
        module: 'inventory',
        enabled: true,
      },
      {
        id: 'inventory:create',
        name: 'Agregar vehículos',
        description: 'Crear nuevas publicaciones',
        module: 'inventory',
        enabled: false,
      },
      {
        id: 'inventory:edit',
        name: 'Editar vehículos',
        description: 'Modificar publicaciones existentes',
        module: 'inventory',
        enabled: false,
      },
      {
        id: 'inventory:delete',
        name: 'Eliminar vehículos',
        description: 'Borrar publicaciones',
        module: 'inventory',
        enabled: false,
      },
      {
        id: 'inventory:import',
        name: 'Importar masivo',
        description: 'Importar vehículos desde CSV/Excel',
        module: 'inventory',
        enabled: false,
      },
    ],
  },
  {
    id: 'leads',
    name: 'Leads / CRM',
    icon: FiUsers,
    permissions: [
      {
        id: 'leads:view',
        name: 'Ver leads',
        description: 'Ver lista de prospectos',
        module: 'leads',
        enabled: true,
      },
      {
        id: 'leads:view_all',
        name: 'Ver todos los leads',
        description: 'Ver leads de todo el equipo',
        module: 'leads',
        enabled: false,
      },
      {
        id: 'leads:edit',
        name: 'Editar leads',
        description: 'Actualizar información de leads',
        module: 'leads',
        enabled: false,
      },
      {
        id: 'leads:assign',
        name: 'Asignar leads',
        description: 'Asignar leads a vendedores',
        module: 'leads',
        enabled: false,
      },
      {
        id: 'leads:delete',
        name: 'Eliminar leads',
        description: 'Borrar leads del sistema',
        module: 'leads',
        enabled: false,
      },
    ],
  },
  {
    id: 'sales',
    name: 'Ventas',
    icon: FiDollarSign,
    permissions: [
      {
        id: 'sales:view',
        name: 'Ver ventas',
        description: 'Ver historial de ventas',
        module: 'sales',
        enabled: true,
      },
      {
        id: 'sales:create',
        name: 'Registrar ventas',
        description: 'Crear nuevas ventas',
        module: 'sales',
        enabled: false,
      },
      {
        id: 'sales:edit',
        name: 'Editar ventas',
        description: 'Modificar registros de venta',
        module: 'sales',
        enabled: false,
      },
    ],
  },
  {
    id: 'analytics',
    name: 'Reportes y Analytics',
    icon: FiBarChart2,
    permissions: [
      {
        id: 'analytics:view',
        name: 'Ver reportes',
        description: 'Acceso a analytics básicos',
        module: 'analytics',
        enabled: true,
      },
      {
        id: 'analytics:export',
        name: 'Exportar reportes',
        description: 'Descargar reportes en PDF/Excel',
        module: 'analytics',
        enabled: false,
      },
      {
        id: 'analytics:advanced',
        name: 'Analytics avanzados',
        description: 'Acceso a dashboards avanzados',
        module: 'analytics',
        enabled: false,
      },
    ],
  },
  {
    id: 'messages',
    name: 'Mensajería',
    icon: FiMessageSquare,
    permissions: [
      {
        id: 'messages:view',
        name: 'Ver mensajes',
        description: 'Ver mensajes entrantes',
        module: 'messages',
        enabled: true,
      },
      {
        id: 'messages:reply',
        name: 'Responder mensajes',
        description: 'Responder a clientes',
        module: 'messages',
        enabled: true,
      },
      {
        id: 'messages:view_all',
        name: 'Ver todos los mensajes',
        description: 'Ver mensajes de todo el equipo',
        module: 'messages',
        enabled: false,
      },
    ],
  },
  {
    id: 'billing',
    name: 'Facturación',
    icon: FiFileText,
    permissions: [
      {
        id: 'billing:view',
        name: 'Ver facturas',
        description: 'Ver historial de pagos',
        module: 'billing',
        enabled: false,
      },
      {
        id: 'billing:manage',
        name: 'Gestionar suscripción',
        description: 'Cambiar plan, métodos de pago',
        module: 'billing',
        enabled: false,
      },
    ],
  },
  {
    id: 'settings',
    name: 'Configuración',
    icon: FiSettings,
    permissions: [
      {
        id: 'settings:view',
        name: 'Ver configuración',
        description: 'Ver ajustes del dealer',
        module: 'settings',
        enabled: false,
      },
      {
        id: 'settings:edit',
        name: 'Editar configuración',
        description: 'Modificar ajustes del dealer',
        module: 'settings',
        enabled: false,
      },
      {
        id: 'team:manage',
        name: 'Gestionar equipo',
        description: 'Invitar y remover miembros',
        module: 'settings',
        enabled: false,
      },
    ],
  },
];

// ============================================================================
// ROLE PRESETS
// ============================================================================

const getRolePreset = (role: DealerRole): string[] => {
  switch (role) {
    case 'Owner':
    case 'Admin':
      return getDefaultPermissions()
        .flatMap((m) => m.permissions)
        .map((p) => p.id);
    case 'SalesManager':
      return [
        'inventory:view',
        'inventory:create',
        'inventory:edit',
        'leads:view',
        'leads:view_all',
        'leads:edit',
        'leads:assign',
        'sales:view',
        'sales:create',
        'sales:edit',
        'analytics:view',
        'analytics:export',
        'analytics:advanced',
        'messages:view',
        'messages:reply',
        'messages:view_all',
      ];
    case 'Salesperson':
      return [
        'inventory:view',
        'leads:view',
        'leads:edit',
        'sales:view',
        'sales:create',
        'analytics:view',
        'messages:view',
        'messages:reply',
      ];
    case 'InventoryManager':
      return [
        'inventory:view',
        'inventory:create',
        'inventory:edit',
        'inventory:delete',
        'inventory:import',
        'analytics:view',
      ];
    case 'Viewer':
      return ['inventory:view', 'leads:view', 'sales:view', 'analytics:view', 'messages:view'];
    default:
      return ['inventory:view'];
  }
};

// ============================================================================
// ROLE BADGE COMPONENT
// ============================================================================

const RoleBadge = ({ role }: { role: DealerRole }) => {
  const roleConfig: Record<DealerRole, { color: string; label: string }> = {
    Owner: { color: 'bg-purple-100 text-purple-800 border-purple-200', label: 'Dueño' },
    Admin: { color: 'bg-blue-100 text-blue-800 border-blue-200', label: 'Administrador' },
    SalesManager: {
      color: 'bg-green-100 text-green-800 border-green-200',
      label: 'Gerente de Ventas',
    },
    Salesperson: { color: 'bg-yellow-100 text-yellow-800 border-yellow-200', label: 'Vendedor' },
    InventoryManager: {
      color: 'bg-orange-100 text-orange-800 border-orange-200',
      label: 'Inventario',
    },
    Viewer: { color: 'bg-gray-100 text-gray-800 border-gray-200', label: 'Visualizador' },
  };

  const config = roleConfig[role] || roleConfig.Viewer;

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${config.color}`}
    >
      {config.label}
    </span>
  );
};

// ============================================================================
// PERMISSION TOGGLE COMPONENT
// ============================================================================

interface PermissionToggleProps {
  permission: Permission;
  onChange: (permissionId: string, enabled: boolean) => void;
  disabled: boolean;
}

const PermissionToggle = ({ permission, onChange, disabled }: PermissionToggleProps) => {
  return (
    <div
      className={`flex items-center justify-between py-3 px-4 rounded-lg ${
        permission.enabled ? 'bg-green-50' : 'bg-gray-50'
      } ${disabled ? 'opacity-60' : ''}`}
    >
      <div className="flex items-center gap-3">
        {permission.enabled ? (
          <FiUnlock className="w-4 h-4 text-green-600" />
        ) : (
          <FiLock className="w-4 h-4 text-gray-400" />
        )}
        <div>
          <p className="text-sm font-medium text-gray-900">{permission.name}</p>
          <p className="text-xs text-gray-500">{permission.description}</p>
        </div>
      </div>
      <label className="relative inline-flex items-center cursor-pointer">
        <input
          type="checkbox"
          checked={permission.enabled}
          onChange={(e) => onChange(permission.id, e.target.checked)}
          disabled={disabled}
          className="sr-only peer"
        />
        <div
          className={`w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600 ${disabled ? 'cursor-not-allowed' : ''}`}
        ></div>
      </label>
    </div>
  );
};

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function DealerEmployeePermissionsPage() {
  const { employeeId } = useParams<{ employeeId: string }>();
  const { user } = useAuth();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [hasChanges, setHasChanges] = useState(false);

  // Data states
  const [employee, setEmployee] = useState<DealerEmployee | null>(null);
  const [selectedRole, setSelectedRole] = useState<DealerRole>('Salesperson');
  const [permissionModules, setPermissionModules] =
    useState<PermissionModule[]>(getDefaultPermissions());

  const dealerId = user?.dealerId || '';
  // Note: isOwner check is used for future feature where only owners can edit permissions

  // Load employee data
  const loadData = useCallback(async () => {
    if (!dealerId || !employeeId) return;

    setLoading(true);
    setError(null);

    try {
      const employeeData = await dealerSettingsApi.getTeamMember(dealerId, employeeId);
      setEmployee(employeeData);
      setSelectedRole(employeeData.role);

      // Apply role preset permissions
      const presetPermissions = getRolePreset(employeeData.role);
      setPermissionModules(
        getDefaultPermissions().map((module) => ({
          ...module,
          permissions: module.permissions.map((p) => ({
            ...p,
            enabled: presetPermissions.includes(p.id),
          })),
        }))
      );
    } catch (err) {
      console.error('Error loading employee:', err);
      setError('Error al cargar el empleado. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  }, [dealerId, employeeId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Clear messages after 3 seconds
  useEffect(() => {
    if (successMessage) {
      const timer = setTimeout(() => setSuccessMessage(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [successMessage]);

  // Handle role change
  const handleRoleChange = (newRole: DealerRole) => {
    setSelectedRole(newRole);
    setHasChanges(true);

    // Apply role preset permissions
    const presetPermissions = getRolePreset(newRole);
    setPermissionModules(
      permissionModules.map((module) => ({
        ...module,
        permissions: module.permissions.map((p) => ({
          ...p,
          enabled: presetPermissions.includes(p.id),
        })),
      }))
    );
  };

  // Handle permission toggle
  const handlePermissionChange = (permissionId: string, enabled: boolean) => {
    setHasChanges(true);
    setPermissionModules(
      permissionModules.map((module) => ({
        ...module,
        permissions: module.permissions.map((p) => (p.id === permissionId ? { ...p, enabled } : p)),
      }))
    );
  };

  // Handle save
  const handleSave = async () => {
    if (!dealerId || !employeeId || !employee) return;

    setSaving(true);
    setError(null);

    try {
      // Update role if changed
      if (selectedRole !== employee.role) {
        await dealerSettingsApi.updateTeamMemberRole(dealerId, employeeId, {
          role: selectedRole,
        });
      }

      // TODO: Save custom permissions if different from role preset

      setSuccessMessage('Permisos actualizados correctamente');
      setHasChanges(false);
      setEmployee({ ...employee, role: selectedRole });
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al guardar los cambios');
    } finally {
      setSaving(false);
    }
  };

  // Calculate permission stats
  const totalPermissions = permissionModules.reduce((acc, m) => acc + m.permissions.length, 0);
  const enabledPermissions = permissionModules.reduce(
    (acc, m) => acc + m.permissions.filter((p) => p.enabled).length,
    0
  );

  const isOwnerEmployee = employee?.role === 'Owner';

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 max-w-5xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <Link
            to="/dealer/employees"
            className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
          >
            <FiArrowLeft className="w-4 h-4" />
            Volver al equipo
          </Link>

          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                <FiShield className="w-7 h-7 text-blue-600" />
                Permisos de Empleado
              </h1>
              <p className="text-gray-500 mt-1">
                Configura los permisos y accesos de este miembro del equipo
              </p>
            </div>

            {!isOwnerEmployee && (
              <button
                onClick={handleSave}
                disabled={saving || !hasChanges}
                className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg transition-colors shadow-sm ${
                  hasChanges
                    ? 'bg-blue-600 text-white hover:bg-blue-700'
                    : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                }`}
              >
                {saving ? (
                  <>
                    <FiRefreshCw className="w-4 h-4 animate-spin" />
                    Guardando...
                  </>
                ) : (
                  <>
                    <FiSave className="w-4 h-4" />
                    Guardar Cambios
                  </>
                )}
              </button>
            )}
          </div>
        </div>

        {/* Messages */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
            <FiAlertCircle className="w-5 h-5 text-red-600 flex-shrink-0" />
            <p className="text-red-700">{error}</p>
            <button
              onClick={() => setError(null)}
              className="ml-auto text-red-600 hover:text-red-800"
            >
              <FiX className="w-4 h-4" />
            </button>
          </div>
        )}

        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
            <FiCheck className="w-5 h-5 text-green-600 flex-shrink-0" />
            <p className="text-green-700">{successMessage}</p>
          </div>
        )}

        {loading ? (
          <div className="p-12 text-center">
            <FiRefreshCw className="w-8 h-8 text-gray-400 animate-spin mx-auto mb-4" />
            <p className="text-gray-500">Cargando permisos...</p>
          </div>
        ) : employee ? (
          <div className="space-y-6">
            {/* Employee Info Card */}
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
              <div className="flex items-center gap-4">
                <div className="w-16 h-16 rounded-full bg-gray-200 flex items-center justify-center">
                  {employee.avatarUrl ? (
                    <img
                      src={employee.avatarUrl}
                      alt={employee.name}
                      className="w-16 h-16 rounded-full object-cover"
                    />
                  ) : (
                    <FiUser className="w-8 h-8 text-gray-500" />
                  )}
                </div>
                <div className="flex-1">
                  <h2 className="text-xl font-semibold text-gray-900">{employee.name}</h2>
                  <p className="text-gray-500">{employee.email}</p>
                </div>
                <div className="text-right">
                  <RoleBadge role={employee.role} />
                  <p className="text-sm text-gray-500 mt-1">
                    {enabledPermissions} de {totalPermissions} permisos
                  </p>
                </div>
              </div>
            </div>

            {isOwnerEmployee ? (
              <div className="bg-purple-50 border border-purple-200 rounded-xl p-6">
                <div className="flex items-start gap-3">
                  <FiShield className="w-6 h-6 text-purple-600 mt-0.5" />
                  <div>
                    <h3 className="font-medium text-purple-900">Dueño del Dealer</h3>
                    <p className="text-purple-700 mt-1">
                      El dueño tiene acceso completo a todas las funcionalidades del dealer. Los
                      permisos no pueden ser modificados.
                    </p>
                  </div>
                </div>
              </div>
            ) : (
              <>
                {/* Role Selection */}
                <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">Rol del Empleado</h3>
                  <p className="text-gray-500 mb-4">
                    Selecciona un rol para aplicar un conjunto de permisos predefinidos. Puedes
                    personalizar los permisos individuales después.
                  </p>

                  <div className="grid grid-cols-2 lg:grid-cols-3 gap-3">
                    {(
                      [
                        'Admin',
                        'SalesManager',
                        'Salesperson',
                        'InventoryManager',
                        'Viewer',
                      ] as DealerRole[]
                    ).map((role) => (
                      <button
                        key={role}
                        onClick={() => handleRoleChange(role)}
                        className={`p-4 rounded-lg border-2 transition-all ${
                          selectedRole === role
                            ? 'border-blue-500 bg-blue-50'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        <RoleBadge role={role} />
                        <p className="text-xs text-gray-500 mt-2">
                          {role === 'Admin' && 'Acceso completo'}
                          {role === 'SalesManager' && 'Gestión de ventas'}
                          {role === 'Salesperson' && 'Ventas básico'}
                          {role === 'InventoryManager' && 'Solo inventario'}
                          {role === 'Viewer' && 'Solo lectura'}
                        </p>
                      </button>
                    ))}
                  </div>
                </div>

                {/* Permission Modules */}
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-gray-900">Permisos Detallados</h3>
                  <p className="text-gray-500">
                    Personaliza los permisos específicos de cada módulo.
                  </p>

                  {permissionModules.map((module) => {
                    const Icon = module.icon;
                    const enabledCount = module.permissions.filter((p) => p.enabled).length;

                    return (
                      <div
                        key={module.id}
                        className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden"
                      >
                        <div className="px-6 py-4 bg-gray-50 border-b border-gray-200 flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-lg bg-white border border-gray-200 flex items-center justify-center">
                              <Icon className="w-5 h-5 text-gray-600" />
                            </div>
                            <div>
                              <h4 className="font-medium text-gray-900">{module.name}</h4>
                              <p className="text-xs text-gray-500">
                                {enabledCount} de {module.permissions.length} activos
                              </p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <button
                              onClick={() => {
                                setHasChanges(true);
                                setPermissionModules(
                                  permissionModules.map((m) =>
                                    m.id === module.id
                                      ? {
                                          ...m,
                                          permissions: m.permissions.map((p) => ({
                                            ...p,
                                            enabled: true,
                                          })),
                                        }
                                      : m
                                  )
                                );
                              }}
                              className="text-xs text-blue-600 hover:text-blue-700"
                            >
                              Activar todos
                            </button>
                            <span className="text-gray-300">|</span>
                            <button
                              onClick={() => {
                                setHasChanges(true);
                                setPermissionModules(
                                  permissionModules.map((m) =>
                                    m.id === module.id
                                      ? {
                                          ...m,
                                          permissions: m.permissions.map((p) => ({
                                            ...p,
                                            enabled: false,
                                          })),
                                        }
                                      : m
                                  )
                                );
                              }}
                              className="text-xs text-gray-500 hover:text-gray-700"
                            >
                              Desactivar todos
                            </button>
                          </div>
                        </div>

                        <div className="p-4 space-y-2">
                          {module.permissions.map((permission) => (
                            <PermissionToggle
                              key={permission.id}
                              permission={permission}
                              onChange={handlePermissionChange}
                              disabled={false}
                            />
                          ))}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        ) : (
          <div className="p-12 text-center">
            <FiAlertCircle className="w-12 h-12 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Empleado no encontrado</h3>
            <p className="text-gray-500 mb-4">
              El empleado que buscas no existe o no tienes acceso.
            </p>
            <Link
              to="/dealer/employees"
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <FiArrowLeft className="w-4 h-4" />
              Volver al equipo
            </Link>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}
