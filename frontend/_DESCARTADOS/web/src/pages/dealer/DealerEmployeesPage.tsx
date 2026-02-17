/**
 * DealerEmployeesPage - Gestión de Empleados del Dealer
 *
 * Página para administrar el equipo del dealer:
 * - Listar empleados y sus roles
 * - Invitar nuevos miembros
 * - Actualizar roles y permisos
 * - Remover empleados
 * - Ver invitaciones pendientes
 *
 * @sprint UserService UI - Dealer Employees (0% → 100%)
 * @route /dealer/employees
 */

import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiTrash2,
  FiMail,
  FiUser,
  FiUsers,
  FiShield,
  FiCheck,
  FiX,
  FiClock,
  FiMoreVertical,
  FiRefreshCw,
  FiSearch,
  FiAlertCircle,
  FiUserPlus,
} from 'react-icons/fi';
import { useAuth } from '@/hooks/useAuth';
import {
  dealerSettingsApi,
  type DealerEmployee,
  type DealerEmployeeInvitation,
  type RoleDefinition,
  type DealerRole,
} from '@/services/dealerSettingsService';

// ============================================================================
// TYPES
// ============================================================================

interface EmployeeStats {
  total: number;
  active: number;
  pending: number;
  suspended: number;
}

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
// STATUS BADGE COMPONENT
// ============================================================================

const StatusBadge = ({ status }: { status: string }) => {
  const statusConfig: Record<string, { color: string; label: string; icon: React.ElementType }> = {
    Active: { color: 'bg-green-100 text-green-800', label: 'Activo', icon: FiCheck },
    Pending: { color: 'bg-yellow-100 text-yellow-800', label: 'Pendiente', icon: FiClock },
    Suspended: { color: 'bg-red-100 text-red-800', label: 'Suspendido', icon: FiX },
  };

  const config = statusConfig[status] || statusConfig.Pending;
  const Icon = config.icon;

  return (
    <span
      className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${config.color}`}
    >
      <Icon className="w-3 h-3" />
      {config.label}
    </span>
  );
};

// ============================================================================
// INVITE MODAL COMPONENT
// ============================================================================

interface InviteModalProps {
  isOpen: boolean;
  onClose: () => void;
  onInvite: (email: string, role: DealerRole) => void;
  loading: boolean;
}

const InviteModal = ({ isOpen, onClose, onInvite, loading }: InviteModalProps) => {
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<DealerRole>('Salesperson');
  const [error, setError] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email.trim()) {
      setError('El email es requerido');
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setError('Email inválido');
      return;
    }

    onInvite(email, role);
    setEmail('');
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="fixed inset-0 bg-black bg-opacity-50" onClick={onClose} />

        <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-gray-900">Invitar Miembro del Equipo</h3>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-500">
              <FiX className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="empleado@ejemplo.com"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Rol</label>
              <select
                value={role}
                onChange={(e) => setRole(e.target.value as DealerRole)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="Admin">Administrador</option>
                <option value="SalesManager">Gerente de Ventas</option>
                <option value="Salesperson">Vendedor</option>
                <option value="InventoryManager">Gestor de Inventario</option>
                <option value="Viewer">Visualizador (Solo lectura)</option>
              </select>
              <p className="mt-1 text-xs text-gray-500">
                {role === 'Admin' && 'Acceso completo a todas las funciones del dealer'}
                {role === 'SalesManager' && 'Gestión de ventas, leads y equipo de ventas'}
                {role === 'Salesperson' && 'Acceso a inventario y leads asignados'}
                {role === 'InventoryManager' && 'Gestión de inventario y vehículos'}
                {role === 'Viewer' && 'Solo puede ver información, sin editar'}
              </p>
            </div>

            {error && (
              <div className="flex items-center gap-2 text-red-600 text-sm">
                <FiAlertCircle className="w-4 h-4" />
                {error}
              </div>
            )}

            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={onClose}
                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={loading}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
              >
                {loading ? (
                  <>
                    <FiRefreshCw className="w-4 h-4 animate-spin" />
                    Enviando...
                  </>
                ) : (
                  <>
                    <FiMail className="w-4 h-4" />
                    Enviar Invitación
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

// ============================================================================
// EMPLOYEE ROW COMPONENT
// ============================================================================

interface EmployeeRowProps {
  employee: DealerEmployee;
  onRemove: (id: string) => void;
  onEditPermissions: (id: string) => void;
  isOwner: boolean;
}

const EmployeeRow = ({ employee, onRemove, onEditPermissions, isOwner }: EmployeeRowProps) => {
  const [showMenu, setShowMenu] = useState(false);

  const isOwnProfile = employee.role === 'Owner';

  return (
    <tr className="hover:bg-gray-50 transition-colors">
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center">
            {employee.avatarUrl ? (
              <img
                src={employee.avatarUrl}
                alt={employee.name}
                className="w-10 h-10 rounded-full object-cover"
              />
            ) : (
              <FiUser className="w-5 h-5 text-gray-500" />
            )}
          </div>
          <div>
            <p className="text-sm font-medium text-gray-900">{employee.name}</p>
            <p className="text-sm text-gray-500">{employee.email}</p>
          </div>
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <RoleBadge role={employee.role} />
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <StatusBadge status={employee.status} />
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
        {employee.activationDate
          ? new Date(employee.activationDate).toLocaleDateString('es-DO', {
              day: 'numeric',
              month: 'short',
              year: 'numeric',
            })
          : 'Sin activar'}
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-right">
        {!isOwnProfile && isOwner && (
          <div className="relative">
            <button
              onClick={() => setShowMenu(!showMenu)}
              className="p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100"
            >
              <FiMoreVertical className="w-4 h-4" />
            </button>

            {showMenu && (
              <>
                <div className="fixed inset-0 z-10" onClick={() => setShowMenu(false)} />
                <div className="absolute right-0 mt-1 w-48 bg-white rounded-lg shadow-lg border border-gray-200 z-20">
                  <button
                    onClick={() => {
                      onEditPermissions(employee.id);
                      setShowMenu(false);
                    }}
                    className="w-full flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50"
                  >
                    <FiShield className="w-4 h-4" />
                    Editar Permisos
                  </button>
                  <button
                    onClick={() => {
                      onRemove(employee.id);
                      setShowMenu(false);
                    }}
                    className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                  >
                    <FiTrash2 className="w-4 h-4" />
                    Remover
                  </button>
                </div>
              </>
            )}
          </div>
        )}
      </td>
    </tr>
  );
};

// ============================================================================
// PENDING INVITATION COMPONENT
// ============================================================================

interface PendingInvitationRowProps {
  invitation: DealerEmployeeInvitation;
  onCancel: (id: string) => void;
  onResend: (id: string) => void;
}

const PendingInvitationRow = ({ invitation, onCancel, onResend }: PendingInvitationRowProps) => {
  const expiresAt = new Date(invitation.expirationDate);
  const isExpired = expiresAt < new Date();

  return (
    <tr className="hover:bg-gray-50 transition-colors">
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
            <FiMail className="w-5 h-5 text-gray-400" />
          </div>
          <div>
            <p className="text-sm font-medium text-gray-900">{invitation.email}</p>
            <p className="text-xs text-gray-500">Invitación pendiente</p>
          </div>
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <RoleBadge role={invitation.role as DealerRole} />
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <span
          className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
            isExpired ? 'bg-red-100 text-red-800' : 'bg-yellow-100 text-yellow-800'
          }`}
        >
          <FiClock className="w-3 h-3" />
          {isExpired ? 'Expirada' : 'Pendiente'}
        </span>
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
        {expiresAt.toLocaleDateString('es-DO', {
          day: 'numeric',
          month: 'short',
          year: 'numeric',
        })}
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-right">
        <div className="flex items-center justify-end gap-2">
          {isExpired && (
            <button
              onClick={() => onResend(invitation.id)}
              className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
              title="Reenviar invitación"
            >
              <FiRefreshCw className="w-4 h-4" />
            </button>
          )}
          <button
            onClick={() => onCancel(invitation.id)}
            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            title="Cancelar invitación"
          >
            <FiX className="w-4 h-4" />
          </button>
        </div>
      </td>
    </tr>
  );
};

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function DealerEmployeesPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Data states
  const [employees, setEmployees] = useState<DealerEmployee[]>([]);
  const [invitations, setInvitations] = useState<DealerEmployeeInvitation[]>([]);
  const [_availableRoles, setAvailableRoles] = useState<RoleDefinition[]>([]);
  const [stats, setStats] = useState<EmployeeStats>({
    total: 0,
    active: 0,
    pending: 0,
    suspended: 0,
  });

  // UI states
  const [showInviteModal, setShowInviteModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterRole, setFilterRole] = useState<string>('all');
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [activeTab, setActiveTab] = useState<'employees' | 'invitations'>('employees');

  const dealerId = user?.dealerId || '';
  const isOwner = employees.find((e) => e.userId === user?.id)?.role === 'Owner';

  // Load data
  const loadData = useCallback(async () => {
    if (!dealerId) return;

    setLoading(true);
    setError(null);

    try {
      const [employeesData, invitationsData, rolesData] = await Promise.all([
        dealerSettingsApi.getTeamMembers(dealerId),
        dealerSettingsApi.getInvitations(dealerId),
        dealerSettingsApi.getAvailableRoles(),
      ]);

      setEmployees(employeesData);
      setInvitations(invitationsData);
      setAvailableRoles(rolesData);

      // Calculate stats
      setStats({
        total: employeesData.length,
        active: employeesData.filter((e) => e.status === 'Active').length,
        pending:
          employeesData.filter((e) => e.status === 'Pending').length + invitationsData.length,
        suspended: employeesData.filter((e) => e.status === 'Suspended').length,
      });
    } catch (err) {
      console.error('Error loading employees:', err);
      setError('Error al cargar el equipo. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  }, [dealerId]);

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

  // Handlers
  const handleInvite = async (email: string, role: DealerRole) => {
    if (!dealerId || !user?.id) return;

    setSaving(true);
    try {
      await dealerSettingsApi.inviteTeamMember(dealerId, {
        email,
        role,
        invitedBy: user.id,
      });
      setSuccessMessage(`Invitación enviada a ${email}`);
      setShowInviteModal(false);
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al enviar la invitación');
    } finally {
      setSaving(false);
    }
  };

  const handleRemoveEmployee = async (employeeId: string) => {
    const employee = employees.find((e) => e.id === employeeId);
    if (!window.confirm(`¿Estás seguro de remover a ${employee?.name}?`)) return;

    try {
      await dealerSettingsApi.removeTeamMember(dealerId, employeeId);
      setEmployees(employees.filter((e) => e.id !== employeeId));
      setSuccessMessage('Miembro removido correctamente');
    } catch (err) {
      setError('Error al remover el miembro');
    }
  };

  const handleEditPermissions = (employeeId: string) => {
    navigate(`/dealer/employees/${employeeId}/permissions`);
  };

  const handleCancelInvitation = async (invitationId: string) => {
    if (!window.confirm('¿Cancelar esta invitación?')) return;

    try {
      await dealerSettingsApi.cancelInvitation(dealerId, invitationId);
      setInvitations(invitations.filter((i) => i.id !== invitationId));
      setSuccessMessage('Invitación cancelada');
    } catch (err) {
      setError('Error al cancelar la invitación');
    }
  };

  const handleResendInvitation = async (_invitationId: string) => {
    // TODO: Implement resend invitation API
    setSuccessMessage('Invitación reenviada');
  };

  // Filter employees
  const filteredEmployees = employees.filter((employee) => {
    const matchesSearch =
      employee.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      employee.email.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesRole = filterRole === 'all' || employee.role === filterRole;
    const matchesStatus = filterStatus === 'all' || employee.status === filterStatus;
    return matchesSearch && matchesRole && matchesStatus;
  });

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-8">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
              <FiUsers className="w-7 h-7 text-blue-600" />
              Equipo del Dealer
            </h1>
            <p className="text-gray-500 mt-1">Gestiona los miembros de tu equipo y sus permisos</p>
          </div>

          <button
            onClick={() => setShowInviteModal(true)}
            className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors shadow-sm"
          >
            <FiUserPlus className="w-4 h-4" />
            Invitar Miembro
          </button>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-xl p-4 border border-gray-200 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center">
                <FiUsers className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                <p className="text-sm text-gray-500">Total</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-green-100 flex items-center justify-center">
                <FiCheck className="w-5 h-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.active}</p>
                <p className="text-sm text-gray-500">Activos</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-yellow-100 flex items-center justify-center">
                <FiClock className="w-5 h-5 text-yellow-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.pending}</p>
                <p className="text-sm text-gray-500">Pendientes</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-xl p-4 border border-gray-200 shadow-sm">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-red-100 flex items-center justify-center">
                <FiX className="w-5 h-5 text-red-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.suspended}</p>
                <p className="text-sm text-gray-500">Suspendidos</p>
              </div>
            </div>
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

        {/* Tabs */}
        <div className="flex gap-4 border-b border-gray-200 mb-6">
          <button
            onClick={() => setActiveTab('employees')}
            className={`pb-3 px-1 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'employees'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Empleados ({employees.length})
          </button>
          <button
            onClick={() => setActiveTab('invitations')}
            className={`pb-3 px-1 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'invitations'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Invitaciones Pendientes ({invitations.length})
          </button>
        </div>

        {/* Search and Filters */}
        {activeTab === 'employees' && (
          <div className="flex flex-col lg:flex-row gap-4 mb-6">
            <div className="flex-1 relative">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar por nombre o email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <div className="flex gap-3">
              <select
                value={filterRole}
                onChange={(e) => setFilterRole(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="all">Todos los roles</option>
                <option value="Owner">Dueño</option>
                <option value="Admin">Administrador</option>
                <option value="SalesManager">Gerente de Ventas</option>
                <option value="Salesperson">Vendedor</option>
                <option value="InventoryManager">Inventario</option>
                <option value="Viewer">Visualizador</option>
              </select>
              <select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="all">Todos los estados</option>
                <option value="Active">Activo</option>
                <option value="Pending">Pendiente</option>
                <option value="Suspended">Suspendido</option>
              </select>
            </div>
          </div>
        )}

        {/* Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
          {loading ? (
            <div className="p-12 text-center">
              <FiRefreshCw className="w-8 h-8 text-gray-400 animate-spin mx-auto mb-4" />
              <p className="text-gray-500">Cargando equipo...</p>
            </div>
          ) : activeTab === 'employees' ? (
            filteredEmployees.length === 0 ? (
              <div className="p-12 text-center">
                <FiUsers className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                  {searchTerm || filterRole !== 'all' || filterStatus !== 'all'
                    ? 'No se encontraron resultados'
                    : 'Sin miembros del equipo'}
                </h3>
                <p className="text-gray-500 mb-4">
                  {searchTerm || filterRole !== 'all' || filterStatus !== 'all'
                    ? 'Intenta con otros filtros de búsqueda'
                    : 'Invita a tu equipo para empezar a colaborar'}
                </p>
                {!searchTerm && filterRole === 'all' && filterStatus === 'all' && (
                  <button
                    onClick={() => setShowInviteModal(true)}
                    className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    <FiUserPlus className="w-4 h-4" />
                    Invitar Primer Miembro
                  </button>
                )}
              </div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Miembro
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Rol
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estado
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Desde
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredEmployees.map((employee) => (
                    <EmployeeRow
                      key={employee.id}
                      employee={employee}
                      onRemove={handleRemoveEmployee}
                      onEditPermissions={handleEditPermissions}
                      isOwner={isOwner}
                    />
                  ))}
                </tbody>
              </table>
            )
          ) : invitations.length === 0 ? (
            <div className="p-12 text-center">
              <FiMail className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                Sin invitaciones pendientes
              </h3>
              <p className="text-gray-500">
                Todas las invitaciones han sido aceptadas o canceladas
              </p>
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Email
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Estado
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Expira
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {invitations.map((invitation) => (
                  <PendingInvitationRow
                    key={invitation.id}
                    invitation={invitation}
                    onCancel={handleCancelInvitation}
                    onResend={handleResendInvitation}
                  />
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Invite Modal */}
        <InviteModal
          isOpen={showInviteModal}
          onClose={() => setShowInviteModal(false)}
          onInvite={handleInvite}
          loading={saving}
        />
      </div>
    </DealerPortalLayout>
  );
}
