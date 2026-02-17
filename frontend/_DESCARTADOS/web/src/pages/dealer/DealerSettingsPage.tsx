/**
 * DealerSettingsPage - Configuración del Dealer (Connected to Backend)
 */

import { useState, useEffect, useCallback } from 'react';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiCamera,
  FiSave,
  FiShield,
  FiBell,
  FiLock,
  FiUsers,
  FiCreditCard,
  FiPlus,
  FiTrash2,
  FiEdit2,
  FiMail,
  FiMonitor,
  FiLoader,
  FiCheck,
  FiX,
  FiAlertCircle,
} from 'react-icons/fi';
import { Building2 } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import {
  dealerSettingsApi,
  type DealerProfile,
  type DealerEmployee,
  type NotificationPreference,
  type PaymentMethod,
  type RoleDefinition,
  type SecuritySettings,
} from '@/services/dealerSettingsService';

// ============================================================================
// TYPES
// ============================================================================

type TabId = 'profile' | 'users' | 'notifications' | 'security' | 'billing';

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function DealerSettingsPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabId>('profile');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Data states
  const [profile, setProfile] = useState<DealerProfile | null>(null);
  const [employees, setEmployees] = useState<DealerEmployee[]>([]);
  const [notificationPrefs, setNotificationPrefs] = useState<NotificationPreference[]>([]);
  const [securitySettings, setSecuritySettings] = useState<SecuritySettings | null>(null);
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([]);
  const [availableRoles, setAvailableRoles] = useState<RoleDefinition[]>([]);
  const [billingInfo, setBillingInfo] = useState<{
    currentPlan: string;
    monthlyPrice: number;
    nextBillingDate?: string;
  } | null>(null);

  // Modal states
  const [showInviteModal, setShowInviteModal] = useState(false);
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState('Salesperson');

  const dealerId = user?.dealerId || '';

  const tabs = [
    { id: 'profile' as TabId, label: 'Perfil', icon: Building2 },
    { id: 'users' as TabId, label: 'Usuarios', icon: FiUsers },
    { id: 'notifications' as TabId, label: 'Notificaciones', icon: FiBell },
    { id: 'security' as TabId, label: 'Seguridad', icon: FiShield },
    { id: 'billing' as TabId, label: 'Facturación', icon: FiCreditCard },
  ];

  // Load data based on active tab
  const loadData = useCallback(async () => {
    if (!dealerId) return;

    setLoading(true);
    setError(null);

    try {
      switch (activeTab) {
        case 'profile':
          const profileData = await dealerSettingsApi.getProfile(dealerId);
          setProfile(profileData);
          break;

        case 'users':
          const [employeesData, rolesData] = await Promise.all([
            dealerSettingsApi.getTeamMembers(dealerId),
            dealerSettingsApi.getAvailableRoles(),
          ]);
          setEmployees(employeesData);
          setAvailableRoles(rolesData);
          break;

        case 'notifications':
          const notifData = await dealerSettingsApi.getNotificationPreferences(dealerId);
          setNotificationPrefs(notifData);
          break;

        case 'security':
          if (user?.id) {
            const securityData = await dealerSettingsApi.getSecuritySettings(user.id);
            setSecuritySettings(securityData);
          }
          break;

        case 'billing':
          const [billingData, paymentMethodsData] = await Promise.all([
            dealerSettingsApi.getBillingInfo(dealerId),
            dealerSettingsApi.getPaymentMethods(dealerId),
          ]);
          setBillingInfo({
            currentPlan: billingData.planDisplayName,
            monthlyPrice: billingData.monthlyPrice,
            nextBillingDate: billingData.nextBillingDate,
          });
          setPaymentMethods(paymentMethodsData);
          break;
      }
    } catch (err) {
      console.error('Error loading data:', err);
      setError('Error al cargar los datos. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  }, [activeTab, dealerId, user?.id]);

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
  const handleSaveProfile = async () => {
    if (!profile || !dealerId) return;

    setSaving(true);
    try {
      await dealerSettingsApi.updateProfile(dealerId, {
        businessName: profile.businessName,
        email: profile.email,
        phone: profile.phone,
        address: profile.address,
        website: profile.website,
        description: profile.description,
      });
      setSuccessMessage('Perfil actualizado correctamente');
    } catch (err) {
      setError('Error al guardar el perfil');
    } finally {
      setSaving(false);
    }
  };

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !dealerId) return;

    try {
      const logoUrl = await dealerSettingsApi.uploadLogo(dealerId, file);
      if (profile) {
        setProfile({ ...profile, logoUrl });
      }
      setSuccessMessage('Logo actualizado correctamente');
    } catch (err) {
      setError('Error al subir el logo');
    }
  };

  const handleInviteEmployee = async () => {
    if (!inviteEmail || !dealerId || !user?.id) return;

    setSaving(true);
    try {
      await dealerSettingsApi.inviteTeamMember(dealerId, {
        email: inviteEmail,
        role: inviteRole as any,
        invitedBy: user.id,
      });
      setSuccessMessage(`Invitación enviada a ${inviteEmail}`);
      setShowInviteModal(false);
      setInviteEmail('');
      loadData();
    } catch (err: any) {
      setError(err.response?.data?.error || 'Error al enviar la invitación');
    } finally {
      setSaving(false);
    }
  };

  const handleRemoveEmployee = async (employeeId: string) => {
    if (!window.confirm('¿Estás seguro de remover a este miembro?')) return;

    try {
      await dealerSettingsApi.removeTeamMember(dealerId, employeeId);
      setEmployees(employees.filter((e) => e.id !== employeeId));
      setSuccessMessage('Miembro removido correctamente');
    } catch (err) {
      setError('Error al remover el miembro');
    }
  };

  const handleToggleNotification = async (pref: NotificationPreference) => {
    try {
      await dealerSettingsApi.updateNotificationPreference(dealerId, {
        type: pref.type,
        enabled: !pref.enabled,
      });
      setNotificationPrefs(
        notificationPrefs.map((p) => (p.id === pref.id ? { ...p, enabled: !p.enabled } : p))
      );
    } catch (err) {
      setError('Error al actualizar la preferencia');
    }
  };

  const handleRevokeAllSessions = async () => {
    if (!window.confirm('¿Cerrar todas las sesiones activas?')) return;

    try {
      await dealerSettingsApi.revokeAllSessions();
      setSuccessMessage('Todas las sesiones han sido cerradas');
      loadData();
    } catch (err) {
      setError('Error al cerrar las sesiones');
    }
  };

  const handleAddPaymentMethod = async () => {
    try {
      const { redirectUrl } = await dealerSettingsApi.initAzulPaymentPage(dealerId);
      window.location.href = redirectUrl;
    } catch (err) {
      setError('Error al iniciar el proceso de pago');
    }
  };

  // Render loading state
  if (loading && !profile && activeTab === 'profile') {
    return (
      <DealerPortalLayout>
        <div className="flex items-center justify-center min-h-[400px]">
          <FiLoader className="w-8 h-8 animate-spin text-blue-600" />
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Configuración</h1>
          <p className="text-gray-500 mt-1">Administra la configuración de tu cuenta de dealer</p>
        </div>

        {/* Messages */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3">
            <FiAlertCircle className="w-5 h-5 text-red-600" />
            <span className="text-red-700">{error}</span>
            <button onClick={() => setError(null)} className="ml-auto">
              <FiX className="w-5 h-5 text-red-600" />
            </button>
          </div>
        )}

        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-xl flex items-center gap-3">
            <FiCheck className="w-5 h-5 text-green-600" />
            <span className="text-green-700">{successMessage}</span>
          </div>
        )}

        <div className="flex flex-col lg:flex-row gap-8">
          {/* Tabs Sidebar */}
          <div className="lg:w-64 flex-shrink-0">
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-2">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-left transition-all ${
                    activeTab === tab.id
                      ? 'bg-blue-50 text-blue-700'
                      : 'text-gray-600 hover:bg-gray-50'
                  }`}
                >
                  <tab.icon className="w-5 h-5" />
                  <span className="font-medium">{tab.label}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Content */}
          <div className="flex-1">
            {/* PROFILE TAB */}
            {activeTab === 'profile' && profile && (
              <div className="space-y-6">
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                  <h2 className="text-lg font-bold text-gray-900 mb-6">Información del Negocio</h2>

                  {/* Logo Upload */}
                  <div className="flex items-center gap-6 mb-8 pb-8 border-b border-gray-100">
                    <div className="w-24 h-24 bg-gradient-to-br from-blue-600 to-emerald-500 rounded-2xl flex items-center justify-center text-white text-3xl font-bold overflow-hidden">
                      {profile.logoUrl ? (
                        <img
                          src={profile.logoUrl}
                          alt="Logo"
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        profile.businessName.charAt(0)
                      )}
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900 mb-1">Logo del Dealer</h3>
                      <p className="text-sm text-gray-500 mb-3">JPG, PNG o SVG. Máximo 2MB.</p>
                      <label className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50 cursor-pointer">
                        <FiCamera className="w-4 h-4" />
                        Cambiar Logo
                        <input
                          type="file"
                          accept="image/*"
                          onChange={handleLogoUpload}
                          className="hidden"
                        />
                      </label>
                    </div>
                  </div>

                  <div className="grid md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Nombre del Negocio
                      </label>
                      <input
                        type="text"
                        value={profile.businessName}
                        onChange={(e) => setProfile({ ...profile, businessName: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">RNC</label>
                      <input
                        type="text"
                        value={profile.rnc}
                        disabled
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl bg-gray-50 text-gray-500"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                      <input
                        type="email"
                        value={profile.email}
                        onChange={(e) => setProfile({ ...profile, email: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Teléfono
                      </label>
                      <input
                        type="tel"
                        value={profile.phone}
                        onChange={(e) => setProfile({ ...profile, phone: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Dirección
                      </label>
                      <input
                        type="text"
                        value={profile.address}
                        onChange={(e) => setProfile({ ...profile, address: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Sitio Web
                      </label>
                      <input
                        type="url"
                        value={profile.website || ''}
                        onChange={(e) => setProfile({ ...profile, website: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Horario
                      </label>
                      <input
                        type="text"
                        value={profile.workingHours || ''}
                        onChange={(e) => setProfile({ ...profile, workingHours: e.target.value })}
                        placeholder="Lun-Sab 9:00 AM - 6:00 PM"
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Descripción
                      </label>
                      <textarea
                        rows={4}
                        value={profile.description || ''}
                        onChange={(e) => setProfile({ ...profile, description: e.target.value })}
                        className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                      />
                    </div>
                  </div>

                  <div className="flex justify-end mt-6">
                    <button
                      onClick={handleSaveProfile}
                      disabled={saving}
                      className="flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-xl font-semibold hover:bg-blue-700 transition-colors disabled:opacity-50"
                    >
                      {saving ? (
                        <FiLoader className="w-5 h-5 animate-spin" />
                      ) : (
                        <FiSave className="w-5 h-5" />
                      )}
                      Guardar Cambios
                    </button>
                  </div>
                </div>
              </div>
            )}

            {/* USERS TAB */}
            {activeTab === 'users' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-lg font-bold text-gray-900">Usuarios del Equipo</h2>
                  <button
                    onClick={() => setShowInviteModal(true)}
                    className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700"
                  >
                    <FiPlus className="w-4 h-4" />
                    Invitar Usuario
                  </button>
                </div>

                {loading ? (
                  <div className="flex justify-center py-8">
                    <FiLoader className="w-6 h-6 animate-spin text-blue-600" />
                  </div>
                ) : employees.length === 0 ? (
                  <div className="text-center py-12 text-gray-500">
                    <FiUsers className="w-12 h-12 mx-auto mb-4 opacity-50" />
                    <p>No hay miembros en el equipo aún</p>
                    <button
                      onClick={() => setShowInviteModal(true)}
                      className="mt-4 text-blue-600 font-medium hover:underline"
                    >
                      Invitar al primer miembro
                    </button>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {employees.map((employee) => (
                      <div
                        key={employee.id}
                        className="flex items-center justify-between p-4 bg-gray-50 rounded-xl"
                      >
                        <div className="flex items-center gap-4">
                          <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center text-blue-600 font-bold overflow-hidden">
                            {employee.avatarUrl ? (
                              <img
                                src={employee.avatarUrl}
                                alt=""
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              employee.name.charAt(0)
                            )}
                          </div>
                          <div>
                            <p className="font-semibold text-gray-900">{employee.name}</p>
                            <p className="text-sm text-gray-500">{employee.email}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-3">
                          <span
                            className={`px-3 py-1 rounded-full text-sm font-medium ${
                              employee.role === 'Owner'
                                ? 'bg-purple-100 text-purple-700'
                                : employee.role === 'Admin'
                                  ? 'bg-blue-100 text-blue-700'
                                  : 'bg-gray-100 text-gray-700'
                            }`}
                          >
                            {availableRoles.find((r) => r.id === employee.role)?.name ||
                              employee.role}
                          </span>
                          <span
                            className={`px-2 py-1 rounded text-xs ${
                              employee.status === 'Active'
                                ? 'bg-green-100 text-green-700'
                                : employee.status === 'Pending'
                                  ? 'bg-yellow-100 text-yellow-700'
                                  : 'bg-red-100 text-red-700'
                            }`}
                          >
                            {employee.status}
                          </span>
                          {employee.role !== 'Owner' && (
                            <button
                              onClick={() => handleRemoveEmployee(employee.id)}
                              className="p-2 hover:bg-red-100 rounded-lg text-red-500"
                            >
                              <FiTrash2 className="w-4 h-4" />
                            </button>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* NOTIFICATIONS TAB */}
            {activeTab === 'notifications' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">
                  Preferencias de Notificaciones
                </h2>
                <div className="space-y-4">
                  {notificationPrefs.map((pref) => (
                    <div
                      key={pref.id}
                      className="flex items-center justify-between p-4 border border-gray-100 rounded-xl"
                    >
                      <div>
                        <p className="font-semibold text-gray-900">{pref.title}</p>
                        <p className="text-sm text-gray-500">{pref.description}</p>
                      </div>
                      <label className="relative inline-flex items-center cursor-pointer">
                        <input
                          type="checkbox"
                          checked={pref.enabled}
                          onChange={() => handleToggleNotification(pref)}
                          className="sr-only peer"
                        />
                        <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
                      </label>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* SECURITY TAB */}
            {activeTab === 'security' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">Seguridad de la Cuenta</h2>
                <div className="space-y-6">
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Cambiar Contraseña</p>
                        <p className="text-sm text-gray-500">
                          {securitySettings?.lastPasswordChange
                            ? `Última actualización: ${new Date(securitySettings.lastPasswordChange).toLocaleDateString()}`
                            : 'Nunca cambiada'}
                        </p>
                      </div>
                      <button
                        onClick={() => setShowPasswordModal(true)}
                        className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50"
                      >
                        Cambiar
                      </button>
                    </div>
                  </div>
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Autenticación de Dos Factores</p>
                        <p className="text-sm text-gray-500">Añade una capa extra de seguridad</p>
                      </div>
                      <button
                        className={`px-4 py-2 rounded-xl text-sm font-medium ${
                          securitySettings?.twoFactorEnabled
                            ? 'bg-green-100 text-green-700'
                            : 'bg-blue-600 text-white hover:bg-blue-700'
                        }`}
                      >
                        {securitySettings?.twoFactorEnabled ? 'Activado ✓' : 'Activar'}
                      </button>
                    </div>
                  </div>
                  <div className="p-4 border border-gray-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">Sesiones Activas</p>
                        <p className="text-sm text-gray-500">
                          {securitySettings?.activeSessions.length || 0} dispositivos conectados
                        </p>
                      </div>
                      <button
                        onClick={handleRevokeAllSessions}
                        className="px-4 py-2 text-red-600 border border-red-200 rounded-xl text-sm font-medium hover:bg-red-50"
                      >
                        Cerrar Todas
                      </button>
                    </div>
                    {securitySettings?.activeSessions &&
                      securitySettings.activeSessions.length > 0 && (
                        <div className="mt-4 space-y-2">
                          {securitySettings.activeSessions.map((session) => (
                            <div
                              key={session.id}
                              className="flex items-center justify-between p-2 bg-gray-50 rounded-lg text-sm"
                            >
                              <div className="flex items-center gap-2">
                                <FiMonitor className="w-4 h-4 text-gray-400" />
                                <span>
                                  {session.device} - {session.browser}
                                </span>
                                {session.isCurrent && (
                                  <span className="text-green-600 text-xs">(actual)</span>
                                )}
                              </div>
                              <span className="text-gray-500">{session.location}</span>
                            </div>
                          ))}
                        </div>
                      )}
                  </div>
                </div>
              </div>
            )}

            {/* BILLING TAB */}
            {activeTab === 'billing' && (
              <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
                <h2 className="text-lg font-bold text-gray-900 mb-6">Información de Facturación</h2>
                <div className="space-y-6">
                  {/* Current Plan */}
                  <div className="p-4 bg-gradient-to-r from-blue-50 to-emerald-50 border border-blue-100 rounded-xl">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-semibold text-gray-900">
                          Plan Actual: {billingInfo?.currentPlan || 'Gratis'}
                        </p>
                        <p className="text-sm text-gray-500">
                          ${billingInfo?.monthlyPrice || 0}/mes
                          {billingInfo?.nextBillingDate &&
                            ` • Renovación: ${new Date(billingInfo.nextBillingDate).toLocaleDateString()}`}
                        </p>
                      </div>
                      <button className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700">
                        Actualizar Plan
                      </button>
                    </div>
                  </div>

                  {/* Payment Methods */}
                  <div>
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="font-semibold text-gray-900">Métodos de Pago</h3>
                      <button
                        onClick={handleAddPaymentMethod}
                        className="flex items-center gap-2 text-blue-600 text-sm font-medium hover:underline"
                      >
                        <FiPlus className="w-4 h-4" />
                        Agregar Método
                      </button>
                    </div>

                    {paymentMethods.length === 0 ? (
                      <div className="p-8 border-2 border-dashed border-gray-200 rounded-xl text-center">
                        <FiCreditCard className="w-10 h-10 mx-auto mb-3 text-gray-400" />
                        <p className="text-gray-500 mb-4">No hay métodos de pago configurados</p>
                        <button
                          onClick={handleAddPaymentMethod}
                          className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700"
                        >
                          Agregar Tarjeta (Azul)
                        </button>
                      </div>
                    ) : (
                      <div className="space-y-3">
                        {paymentMethods.map((pm) => (
                          <div
                            key={pm.id}
                            className="flex items-center justify-between p-4 border border-gray-100 rounded-xl"
                          >
                            <div className="flex items-center gap-4">
                              <div className="p-3 bg-gray-100 rounded-xl">
                                <FiCreditCard className="w-6 h-6 text-gray-600" />
                              </div>
                              <div>
                                <p className="font-semibold text-gray-900">
                                  {pm.card?.brand} •••• {pm.card?.last4}
                                  {pm.isDefault && (
                                    <span className="ml-2 text-xs text-green-600">(Principal)</span>
                                  )}
                                </p>
                                <p className="text-sm text-gray-500">
                                  Expira {pm.card?.expMonth}/{pm.card?.expYear}
                                </p>
                              </div>
                            </div>
                            <div className="flex items-center gap-2">
                              <button className="p-2 hover:bg-gray-100 rounded-lg">
                                <FiEdit2 className="w-4 h-4 text-gray-500" />
                              </button>
                              <button className="p-2 hover:bg-red-50 rounded-lg">
                                <FiTrash2 className="w-4 h-4 text-red-500" />
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* Azul Integration Note */}
                  <div className="p-4 bg-blue-50 border border-blue-100 rounded-xl">
                    <div className="flex items-start gap-3">
                      <div className="p-2 bg-blue-100 rounded-lg">
                        <FiShield className="w-5 h-5 text-blue-600" />
                      </div>
                      <div>
                        <p className="font-medium text-blue-900">Pagos seguros con Azul</p>
                        <p className="text-sm text-blue-700">
                          Tus pagos son procesados de forma segura a través de Azul (Banco Popular),
                          la principal pasarela de pagos de República Dominicana.
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Invite Employee Modal */}
      {showInviteModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Invitar Usuario</h3>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                <div className="relative">
                  <FiMail className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="email"
                    value={inviteEmail}
                    onChange={(e) => setInviteEmail(e.target.value)}
                    placeholder="email@ejemplo.com"
                    className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Rol</label>
                <select
                  value={inviteRole}
                  onChange={(e) => setInviteRole(e.target.value)}
                  className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                >
                  {availableRoles
                    .filter((r) => r.id !== 'Owner')
                    .map((role) => (
                      <option key={role.id} value={role.id}>
                        {role.name}
                      </option>
                    ))}
                </select>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <button
                onClick={() => setShowInviteModal(false)}
                className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50"
              >
                Cancelar
              </button>
              <button
                onClick={handleInviteEmployee}
                disabled={!inviteEmail || saving}
                className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
              >
                {saving && <FiLoader className="w-4 h-4 animate-spin" />}
                Enviar Invitación
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Change Password Modal */}
      {showPasswordModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Cambiar Contraseña</h3>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Contraseña actual
                </label>
                <div className="relative">
                  <FiLock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="password"
                    placeholder="••••••••"
                    className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nueva contraseña
                </label>
                <div className="relative">
                  <FiLock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="password"
                    placeholder="••••••••"
                    className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Confirmar contraseña
                </label>
                <div className="relative">
                  <FiLock className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                  <input
                    type="password"
                    placeholder="••••••••"
                    className="w-full pl-10 pr-4 py-3 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
                  />
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <button
                onClick={() => setShowPasswordModal(false)}
                className="px-4 py-2 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50"
              >
                Cancelar
              </button>
              <button className="px-4 py-2 bg-blue-600 text-white rounded-xl text-sm font-medium hover:bg-blue-700">
                Cambiar Contraseña
              </button>
            </div>
          </div>
        </div>
      )}
    </DealerPortalLayout>
  );
}
