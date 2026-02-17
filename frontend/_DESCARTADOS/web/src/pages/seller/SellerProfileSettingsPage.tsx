/**
 * Seller Profile Settings Page
 * Página de configuración del perfil de vendedor (SELLER-002, SELLER-003)
 */

import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import {
  FiCamera,
  FiSave,
  FiEye,
  FiPhone,
  FiMail,
  FiMessageCircle,
  FiClock,
  FiShield,
  FiTrendingUp,
  FiUsers,
  FiAlertCircle,
  FiCheckCircle,
  FiSettings,
  FiUser,
  FiStar,
} from 'react-icons/fi';
import { FaWhatsapp } from 'react-icons/fa';
import sellerProfileService, {
  SellerType,
  SellerVerificationStatus,
  BADGE_INFO,
} from '@/services/sellerProfileService';
import type {
  SellerProfile,
  SellerMyStats,
  ContactPreferences,
} from '@/services/sellerProfileService';

type TabType = 'profile' | 'contact' | 'stats';

export default function SellerProfileSettingsPage() {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const coverInputRef = useRef<HTMLInputElement>(null);

  const [activeTab, setActiveTab] = useState<TabType>('profile');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [profile, setProfile] = useState<SellerProfile | null>(null);
  const [stats, setStats] = useState<SellerMyStats | null>(null);
  const [, setContactPrefs] = useState<ContactPreferences | null>(null);

  // Profile form state
  const [displayName, setDisplayName] = useState('');
  const [bio, setBio] = useState('');
  const [city, setCity] = useState('');
  const [province, setProvince] = useState('');

  // Contact preferences form state
  const [allowPhoneCalls, setAllowPhoneCalls] = useState(false);
  const [allowWhatsApp, setAllowWhatsApp] = useState(false);
  const [allowInAppChat, setAllowInAppChat] = useState(false);
  const [allowEmail, setAllowEmail] = useState(false);
  const [showPhoneNumber, setShowPhoneNumber] = useState(false);
  const [showWhatsAppNumber, setShowWhatsAppNumber] = useState(false);
  const [showEmail, setShowEmail] = useState(false);
  const [contactHoursStart, setContactHoursStart] = useState('09:00');
  const [contactHoursEnd, setContactHoursEnd] = useState('18:00');
  const [contactDays, setContactDays] = useState<string[]>([]);
  const [autoReplyEnabled, setAutoReplyEnabled] = useState(false);
  const [autoReplyMessage, setAutoReplyMessage] = useState('');
  const [requireVerifiedBuyers, setRequireVerifiedBuyers] = useState(false);
  const [blockNewAccounts, setBlockNewAccounts] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    setError(null);

    try {
      const [profileData, statsData] = await Promise.all([
        sellerProfileService.getMyProfile(),
        sellerProfileService.getMyStats(),
      ]);

      setProfile(profileData);
      setStats(statsData);

      // Initialize profile form
      setDisplayName(profileData.displayName || profileData.fullName || '');
      setBio(profileData.bio || '');
      setCity(profileData.city || '');
      setProvince(profileData.province || profileData.state || '');

      // Load contact preferences if exists
      if (profileData.contactPreferences) {
        const prefs = profileData.contactPreferences;
        setContactPrefs(prefs);
        setAllowPhoneCalls(prefs.allowPhoneCalls);
        setAllowWhatsApp(prefs.allowWhatsApp);
        setAllowInAppChat(prefs.allowInAppChat);
        setAllowEmail(prefs.allowEmail);
        setShowPhoneNumber(prefs.showPhoneNumber);
        setShowWhatsAppNumber(prefs.showWhatsAppNumber);
        setShowEmail(prefs.showEmail);
        setContactHoursStart(prefs.contactHoursStart);
        setContactHoursEnd(prefs.contactHoursEnd);
        setContactDays(prefs.contactDays);
        setAutoReplyEnabled(prefs.autoReplyEnabled || false);
        setAutoReplyMessage(prefs.autoReplyMessage || '');
        setRequireVerifiedBuyers(prefs.requireVerifiedBuyers);
        setBlockNewAccounts(prefs.blockNewAccounts || false);
      }
    } catch (err) {
      setError('Error al cargar el perfil');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleSaveProfile = async () => {
    if (!profile) return;

    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      await sellerProfileService.updateMyProfile({
        displayName,
        bio,
        city,
        province,
      });
      setSuccess('Perfil actualizado correctamente');
    } catch (err) {
      setError('Error al actualizar el perfil');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleSaveContactPreferences = async () => {
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      await sellerProfileService.updateContactPreferences({
        allowPhoneCalls,
        allowWhatsApp,
        allowInAppChat,
        allowEmail,
        showPhoneNumber,
        showWhatsAppNumber,
        showEmail,
        contactHoursStart,
        contactHoursEnd,
        contactDays,
        autoReplyEnabled,
        autoReplyMessage,
        requireVerifiedBuyers,
        blockNewAccounts,
      });
      setSuccess('Preferencias de contacto actualizadas');
    } catch (err) {
      setError('Error al actualizar preferencias');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handlePhotoUpload = async (
    event: React.ChangeEvent<HTMLInputElement>,
    type: 'profile' | 'cover'
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setSaving(true);
    setError(null);

    try {
      await sellerProfileService.updateProfilePhoto({
        photoType: type,
        file,
      });
      setSuccess(`Foto de ${type === 'profile' ? 'perfil' : 'portada'} actualizada`);
      loadData(); // Reload to get new photo URL
    } catch (err) {
      setError('Error al subir la foto');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const toggleContactDay = (day: string) => {
    if (contactDays.includes(day)) {
      setContactDays(contactDays.filter((d) => d !== day));
    } else {
      setContactDays([...contactDays, day]);
    }
  };

  const getSellerTypeLabel = (type: SellerType) => {
    switch (type) {
      case SellerType.Individual:
        return 'Vendedor Individual';
      case SellerType.Dealer:
        return 'Dealer';
      case SellerType.PremiumDealer:
        return 'Dealer Premium';
      default:
        return 'Vendedor';
    }
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (!profile) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Perfil no encontrado</h2>
            <p className="text-gray-600 mb-4">Primero debes crear tu perfil de vendedor</p>
            <button
              onClick={() => navigate('/sell')}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Crear perfil de vendedor
            </button>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">Configuración del Perfil</h1>
            <p className="text-gray-600 mt-1">
              Administra cómo te ven los compradores y cómo pueden contactarte
            </p>
          </div>

          {/* Alerts */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
              <FiAlertCircle className="w-5 h-5 text-red-500" />
              <span className="text-red-700">{error}</span>
            </div>
          )}

          {success && (
            <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
              <FiCheckCircle className="w-5 h-5 text-green-500" />
              <span className="text-green-700">{success}</span>
            </div>
          )}

          {/* Tabs */}
          <div className="bg-white rounded-xl shadow-sm mb-6">
            <div className="border-b border-gray-200">
              <nav className="flex -mb-px">
                <button
                  onClick={() => setActiveTab('profile')}
                  className={`flex-1 py-4 px-4 text-center font-medium text-sm border-b-2 transition-colors ${
                    activeTab === 'profile'
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <FiUser className="inline-block mr-2" />
                  Perfil
                </button>
                <button
                  onClick={() => setActiveTab('contact')}
                  className={`flex-1 py-4 px-4 text-center font-medium text-sm border-b-2 transition-colors ${
                    activeTab === 'contact'
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <FiSettings className="inline-block mr-2" />
                  Contacto
                </button>
                <button
                  onClick={() => setActiveTab('stats')}
                  className={`flex-1 py-4 px-4 text-center font-medium text-sm border-b-2 transition-colors ${
                    activeTab === 'stats'
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  <FiTrendingUp className="inline-block mr-2" />
                  Estadísticas
                </button>
              </nav>
            </div>

            <div className="p-6">
              {/* Profile Tab */}
              {activeTab === 'profile' && (
                <div className="space-y-6">
                  {/* Photos */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Fotos</h3>

                    {/* Cover Photo */}
                    <div className="relative mb-6">
                      <div className="h-32 rounded-lg bg-gradient-to-r from-blue-500 to-blue-600 overflow-hidden">
                        {profile.coverPhotoUrl && (
                          <img
                            src={profile.coverPhotoUrl}
                            alt="Cover"
                            className="w-full h-full object-cover"
                          />
                        )}
                      </div>
                      <button
                        onClick={() => coverInputRef.current?.click()}
                        className="absolute bottom-2 right-2 p-2 bg-white/90 rounded-lg shadow hover:bg-white transition-colors"
                      >
                        <FiCamera className="w-5 h-5" />
                      </button>
                      <input
                        ref={coverInputRef}
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={(e) => handlePhotoUpload(e, 'cover')}
                      />
                    </div>

                    {/* Profile Photo */}
                    <div className="flex items-center gap-4">
                      <div className="relative">
                        <div className="w-24 h-24 rounded-full bg-gray-200 overflow-hidden">
                          {profile.profilePhotoUrl ? (
                            <img
                              src={profile.profilePhotoUrl}
                              alt="Profile"
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            <div className="w-full h-full flex items-center justify-center text-gray-400 text-2xl font-bold">
                              {displayName.charAt(0) || 'V'}
                            </div>
                          )}
                        </div>
                        <button
                          onClick={() => fileInputRef.current?.click()}
                          className="absolute bottom-0 right-0 p-1.5 bg-blue-600 text-white rounded-full shadow"
                        >
                          <FiCamera className="w-4 h-4" />
                        </button>
                        <input
                          ref={fileInputRef}
                          type="file"
                          accept="image/*"
                          className="hidden"
                          onChange={(e) => handlePhotoUpload(e, 'profile')}
                        />
                      </div>
                      <div>
                        <p className="font-medium text-gray-900">Foto de perfil</p>
                        <p className="text-sm text-gray-500">JPG, PNG. Máximo 5MB.</p>
                      </div>
                    </div>
                  </div>

                  {/* Basic Info */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Información básica</h3>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Nombre a mostrar
                        </label>
                        <input
                          type="text"
                          value={displayName}
                          onChange={(e) => setDisplayName(e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          placeholder="Ej: Juan Autos RD"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Tipo de vendedor
                        </label>
                        <input
                          type="text"
                          value={getSellerTypeLabel(profile.sellerType)}
                          disabled
                          className="w-full px-4 py-2 border border-gray-200 rounded-lg bg-gray-50 text-gray-500"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Ciudad
                        </label>
                        <input
                          type="text"
                          value={city}
                          onChange={(e) => setCity(e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          placeholder="Ej: Santo Domingo"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Provincia
                        </label>
                        <input
                          type="text"
                          value={province}
                          onChange={(e) => setProvince(e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          placeholder="Ej: Distrito Nacional"
                        />
                      </div>

                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Descripción / Bio
                        </label>
                        <textarea
                          value={bio}
                          onChange={(e) => setBio(e.target.value)}
                          rows={4}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                          placeholder="Cuéntales a los compradores sobre ti o tu negocio..."
                        />
                      </div>
                    </div>
                  </div>

                  {/* Badges */}
                  {profile.badges && profile.badges.length > 0 && (
                    <div>
                      <h3 className="text-lg font-medium text-gray-900 mb-4">Insignias</h3>
                      <div className="flex flex-wrap gap-2">
                        {profile.badges.map((badge) => {
                          const badgeInfo = BADGE_INFO[badge.name] || {
                            icon: '🏷️',
                            description: '',
                          };
                          return (
                            <div
                              key={badge.name}
                              className="flex items-center gap-2 px-3 py-2 bg-gray-100 rounded-lg"
                              title={badgeInfo.description}
                            >
                              <span className="text-xl">{badgeInfo.icon}</span>
                              <div>
                                <p className="font-medium text-sm text-gray-900">
                                  {badge.name.replace(/([A-Z])/g, ' $1').trim()}
                                </p>
                                <p className="text-xs text-gray-500">
                                  Desde {new Date(badge.earnedAt).toLocaleDateString()}
                                </p>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  )}

                  {/* Save Button */}
                  <div className="flex justify-end gap-4 pt-4 border-t">
                    <button
                      onClick={() => navigate(`/sellers/${profile.id}`)}
                      className="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 flex items-center gap-2"
                    >
                      <FiEye className="w-4 h-4" />
                      Ver perfil público
                    </button>
                    <button
                      onClick={handleSaveProfile}
                      disabled={saving}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
                    >
                      <FiSave className="w-4 h-4" />
                      {saving ? 'Guardando...' : 'Guardar cambios'}
                    </button>
                  </div>
                </div>
              )}

              {/* Contact Tab */}
              {activeTab === 'contact' && (
                <div className="space-y-6">
                  {/* Contact Channels */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Canales de contacto</h3>
                    <p className="text-sm text-gray-600 mb-4">
                      Selecciona cómo pueden contactarte los compradores
                    </p>

                    <div className="space-y-3">
                      <label className="flex items-center justify-between p-4 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <div className="flex items-center gap-3">
                          <FaWhatsapp className="w-5 h-5 text-green-500" />
                          <div>
                            <span className="font-medium">WhatsApp</span>
                            <p className="text-sm text-gray-500">Recibir mensajes por WhatsApp</p>
                          </div>
                        </div>
                        <input
                          type="checkbox"
                          checked={allowWhatsApp}
                          onChange={(e) => setAllowWhatsApp(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                      </label>

                      {allowWhatsApp && (
                        <label className="flex items-center gap-3 ml-8 text-sm">
                          <input
                            type="checkbox"
                            checked={showWhatsAppNumber}
                            onChange={(e) => setShowWhatsAppNumber(e.target.checked)}
                            className="w-4 h-4 text-blue-600 rounded"
                          />
                          Mostrar número de WhatsApp públicamente
                        </label>
                      )}

                      <label className="flex items-center justify-between p-4 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <div className="flex items-center gap-3">
                          <FiPhone className="w-5 h-5 text-blue-500" />
                          <div>
                            <span className="font-medium">Llamadas telefónicas</span>
                            <p className="text-sm text-gray-500">Recibir llamadas directas</p>
                          </div>
                        </div>
                        <input
                          type="checkbox"
                          checked={allowPhoneCalls}
                          onChange={(e) => setAllowPhoneCalls(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                      </label>

                      {allowPhoneCalls && (
                        <label className="flex items-center gap-3 ml-8 text-sm">
                          <input
                            type="checkbox"
                            checked={showPhoneNumber}
                            onChange={(e) => setShowPhoneNumber(e.target.checked)}
                            className="w-4 h-4 text-blue-600 rounded"
                          />
                          Mostrar número de teléfono públicamente
                        </label>
                      )}

                      <label className="flex items-center justify-between p-4 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <div className="flex items-center gap-3">
                          <FiMessageCircle className="w-5 h-5 text-purple-500" />
                          <div>
                            <span className="font-medium">Chat en la aplicación</span>
                            <p className="text-sm text-gray-500">Mensajes dentro de OKLA</p>
                          </div>
                        </div>
                        <input
                          type="checkbox"
                          checked={allowInAppChat}
                          onChange={(e) => setAllowInAppChat(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                      </label>

                      <label className="flex items-center justify-between p-4 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <div className="flex items-center gap-3">
                          <FiMail className="w-5 h-5 text-orange-500" />
                          <div>
                            <span className="font-medium">Email</span>
                            <p className="text-sm text-gray-500">Recibir emails de compradores</p>
                          </div>
                        </div>
                        <input
                          type="checkbox"
                          checked={allowEmail}
                          onChange={(e) => setAllowEmail(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                      </label>

                      {allowEmail && (
                        <label className="flex items-center gap-3 ml-8 text-sm">
                          <input
                            type="checkbox"
                            checked={showEmail}
                            onChange={(e) => setShowEmail(e.target.checked)}
                            className="w-4 h-4 text-blue-600 rounded"
                          />
                          Mostrar email públicamente
                        </label>
                      )}
                    </div>
                  </div>

                  {/* Hours */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Horario de atención</h3>

                    <div className="grid grid-cols-2 gap-4 mb-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Desde
                        </label>
                        <input
                          type="time"
                          value={contactHoursStart}
                          onChange={(e) => setContactHoursStart(e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Hasta
                        </label>
                        <input
                          type="time"
                          value={contactHoursEnd}
                          onChange={(e) => setContactHoursEnd(e.target.value)}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Días de atención
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {[
                          'Lunes',
                          'Martes',
                          'Miércoles',
                          'Jueves',
                          'Viernes',
                          'Sábado',
                          'Domingo',
                        ].map((day) => (
                          <button
                            key={day}
                            onClick={() => toggleContactDay(day)}
                            className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${
                              contactDays.includes(day)
                                ? 'bg-blue-600 text-white'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {day}
                          </button>
                        ))}
                      </div>
                    </div>
                  </div>

                  {/* Auto Reply */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Respuesta automática</h3>

                    <label className="flex items-center gap-3 mb-4">
                      <input
                        type="checkbox"
                        checked={autoReplyEnabled}
                        onChange={(e) => setAutoReplyEnabled(e.target.checked)}
                        className="w-5 h-5 text-blue-600 rounded"
                      />
                      <span className="font-medium">Habilitar respuesta automática</span>
                    </label>

                    {autoReplyEnabled && (
                      <textarea
                        value={autoReplyMessage}
                        onChange={(e) => setAutoReplyMessage(e.target.value)}
                        rows={3}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                        placeholder="Ej: Gracias por tu interés. Te responderé lo antes posible..."
                      />
                    )}
                  </div>

                  {/* Privacy */}
                  <div>
                    <h3 className="text-lg font-medium text-gray-900 mb-4">Privacidad</h3>

                    <div className="space-y-3">
                      <label className="flex items-center gap-3">
                        <input
                          type="checkbox"
                          checked={requireVerifiedBuyers}
                          onChange={(e) => setRequireVerifiedBuyers(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                        <div>
                          <span className="font-medium">Solo compradores verificados</span>
                          <p className="text-sm text-gray-500">
                            Solo usuarios con identidad verificada pueden contactarte
                          </p>
                        </div>
                      </label>

                      <label className="flex items-center gap-3">
                        <input
                          type="checkbox"
                          checked={blockNewAccounts}
                          onChange={(e) => setBlockNewAccounts(e.target.checked)}
                          className="w-5 h-5 text-blue-600 rounded"
                        />
                        <div>
                          <span className="font-medium">Bloquear cuentas nuevas</span>
                          <p className="text-sm text-gray-500">
                            Bloquear mensajes de cuentas creadas hace menos de 7 días
                          </p>
                        </div>
                      </label>
                    </div>
                  </div>

                  {/* Save Button */}
                  <div className="flex justify-end pt-4 border-t">
                    <button
                      onClick={handleSaveContactPreferences}
                      disabled={saving}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
                    >
                      <FiSave className="w-4 h-4" />
                      {saving ? 'Guardando...' : 'Guardar preferencias'}
                    </button>
                  </div>
                </div>
              )}

              {/* Stats Tab */}
              {activeTab === 'stats' && stats && (
                <div className="space-y-6">
                  {/* Quick Stats */}
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <div className="p-4 bg-blue-50 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <FiEye className="w-5 h-5 text-blue-600" />
                        <span className="text-sm text-gray-600">Vistas este mes</span>
                      </div>
                      <p className="text-2xl font-bold text-gray-900">{stats.viewsThisMonth}</p>
                      {stats.viewsChange !== undefined && (
                        <p
                          className={`text-sm ${stats.viewsChange >= 0 ? 'text-green-600' : 'text-red-600'}`}
                        >
                          {stats.viewsChange >= 0 ? '+' : ''}
                          {stats.viewsChange}% vs mes anterior
                        </p>
                      )}
                    </div>

                    <div className="p-4 bg-green-50 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <FiUsers className="w-5 h-5 text-green-600" />
                        <span className="text-sm text-gray-600">Leads este mes</span>
                      </div>
                      <p className="text-2xl font-bold text-gray-900">{stats.leadsThisMonth}</p>
                      {stats.leadsChange !== undefined && (
                        <p
                          className={`text-sm ${stats.leadsChange >= 0 ? 'text-green-600' : 'text-red-600'}`}
                        >
                          {stats.leadsChange >= 0 ? '+' : ''}
                          {stats.leadsChange}% vs mes anterior
                        </p>
                      )}
                    </div>

                    <div className="p-4 bg-purple-50 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <FiClock className="w-5 h-5 text-purple-600" />
                        <span className="text-sm text-gray-600">Tasa de respuesta</span>
                      </div>
                      <p className="text-2xl font-bold text-gray-900">{stats.responseRate}%</p>
                      <p className="text-sm text-gray-500">
                        Tiempo promedio: {stats.averageResponseTime}
                      </p>
                    </div>

                    <div className="p-4 bg-yellow-50 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <FiTrendingUp className="w-5 h-5 text-yellow-600" />
                        <span className="text-sm text-gray-600">Conversión</span>
                      </div>
                      <p className="text-2xl font-bold text-gray-900">{stats.conversionRate}%</p>
                      <p className="text-sm text-gray-500">
                        {stats.soldCount} de {stats.totalListings} vendidos
                      </p>
                    </div>
                  </div>

                  {/* Detailed Stats */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="bg-gray-50 rounded-lg p-4">
                      <h4 className="font-medium text-gray-900 mb-3">Publicaciones</h4>
                      <div className="space-y-2">
                        <div className="flex justify-between">
                          <span className="text-gray-600">Activas</span>
                          <span className="font-medium">{stats.activeListings}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-600">Total publicadas</span>
                          <span className="font-medium">{stats.totalListings}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-600">Vendidas</span>
                          <span className="font-medium">{stats.soldCount}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-600">Pendientes respuesta</span>
                          <span className="font-medium">{stats.pendingResponses}</span>
                        </div>
                      </div>
                    </div>

                    <div className="bg-gray-50 rounded-lg p-4">
                      <h4 className="font-medium text-gray-900 mb-3">Reputación</h4>
                      <div className="space-y-2">
                        <div className="flex justify-between items-center">
                          <span className="text-gray-600">Calificación promedio</span>
                          <div className="flex items-center gap-1">
                            <FiStar className="w-4 h-4 text-yellow-500 fill-yellow-500" />
                            <span className="font-medium">{stats.averageRating.toFixed(1)}</span>
                          </div>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-600">Total reseñas</span>
                          <span className="font-medium">{stats.reviewCount}</span>
                        </div>
                        <div className="flex justify-between">
                          <span className="text-gray-600">Estado de verificación</span>
                          <span className="font-medium">
                            {profile.verificationStatus === SellerVerificationStatus.Verified ? (
                              <span className="text-green-600 flex items-center gap-1">
                                <FiCheckCircle className="w-4 h-4" /> Verificado
                              </span>
                            ) : (
                              <span className="text-yellow-600">{profile.verificationStatus}</span>
                            )}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Tips */}
                  <div className="bg-blue-50 rounded-lg p-4">
                    <div className="flex items-start gap-3">
                      <FiShield className="w-6 h-6 text-blue-600 flex-shrink-0 mt-0.5" />
                      <div>
                        <h4 className="font-medium text-gray-900">Mejora tu perfil</h4>
                        <ul className="mt-2 text-sm text-gray-600 space-y-1">
                          <li>• Responde rápido para obtener el badge de "Respuesta Rápida"</li>
                          <li>• Mantén una calificación de 4.5+ para ser "Vendedor Confiable"</li>
                          <li>
                            • Verifica tu identidad para ganar la confianza de los compradores
                          </li>
                          <li>• Actualiza tus publicaciones regularmente para más visibilidad</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
