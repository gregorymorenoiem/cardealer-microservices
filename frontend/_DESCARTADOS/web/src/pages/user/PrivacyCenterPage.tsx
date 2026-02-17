/**
 * PrivacyCenterPage - Centro de Privacidad ARCO (Ley 172-13)
 *
 * Derechos ARCO:
 * - Acceso: Ver todos mis datos personales
 * - Rectificación: Corregir datos incorrectos
 * - Cancelación: Solicitar eliminación de cuenta
 * - Oposición: Oponerse a tratamiento de datos
 *
 * @module pages/user/PrivacyCenterPage
 * @version 2.0.0
 * @since Enero 26, 2026
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  FiShield,
  FiDownload,
  FiEdit3,
  FiTrash2,
  FiAlertCircle,
  FiCheck,
  FiClock,
  FiFileText,
  FiLock,
  FiMail,
  FiEye,
  FiLoader,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import {
  privacyService,
  requestStatusLabels,
  requestStatusColors,
} from '../../services/privacyService';
import type {
  CommunicationPreferences,
  PrivacyRequestHistory,
} from '../../services/privacyService';

const PrivacyCenterPage = () => {
  const [activeTab, setActiveTab] = useState<'overview' | 'history'>('overview');
  const [preferences, setPreferences] = useState<CommunicationPreferences | null>(null);
  const [requestHistory, setRequestHistory] = useState<PrivacyRequestHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Cargar preferencias y historial
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        const [prefs, history] = await Promise.all([
          privacyService.getCommunicationPreferences(),
          privacyService.getPrivacyRequestHistory(),
        ]);
        setPreferences(prefs);
        setRequestHistory(history.requests);
      } catch (err) {
        console.error('Error loading privacy data:', err);
        setError('Error al cargar los datos de privacidad');
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  // Actualizar preferencia individual
  const updatePreference = async (key: string, value: boolean) => {
    if (!preferences) return;
    setSaving(true);
    try {
      const updated = await privacyService.updateCommunicationPreferences({
        [key]: value,
      });
      setPreferences(updated);
    } catch (err) {
      console.error('Error updating preference:', err);
      setError('Error al actualizar preferencia');
    } finally {
      setSaving(false);
    }
  };

  const arcoRights = [
    {
      id: 'access',
      title: 'Acceso',
      description: 'Conocer qué datos personales tenemos sobre ti y cómo los utilizamos.',
      icon: FiEye,
      color: 'bg-blue-500',
      action: 'Ver mis datos',
      link: '/settings/privacy/my-data',
    },
    {
      id: 'rectification',
      title: 'Rectificación',
      description: 'Corregir datos personales incorrectos o desactualizados.',
      icon: FiEdit3,
      color: 'bg-green-500',
      action: 'Editar perfil',
      link: '/profile',
    },
    {
      id: 'cancellation',
      title: 'Cancelación',
      description: 'Solicitar la eliminación de tus datos personales y cuenta.',
      icon: FiTrash2,
      color: 'bg-red-500',
      action: 'Eliminar cuenta',
      link: '/settings/privacy/delete-account',
    },
    {
      id: 'opposition',
      title: 'Oposición',
      description: 'Oponerte al tratamiento de tus datos para ciertos fines.',
      icon: FiAlertCircle,
      color: 'bg-yellow-500',
      action: 'Gestionar consentimientos',
      link: '#consents',
    },
  ];

  const dataCategories = [
    { name: 'Datos de identidad', items: ['Nombre', 'Email', 'Teléfono', 'Cédula'] },
    { name: 'Datos de ubicación', items: ['Dirección', 'Ciudad', 'Provincia'] },
    { name: 'Datos de actividad', items: ['Búsquedas', 'Favoritos', 'Historial de navegación'] },
    { name: 'Datos transaccionales', items: ['Compras', 'Ventas', 'Pagos'] },
    { name: 'Preferencias', items: ['Configuración', 'Notificaciones', 'Alertas'] },
  ];

  const getStatusBadge = (status: string) => {
    const statusKey = status as keyof typeof requestStatusLabels;
    const colorClass = requestStatusColors[statusKey] || 'bg-gray-100 text-gray-800';
    const label = requestStatusLabels[statusKey] || status;

    return (
      <span
        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${colorClass}`}
      >
        {status === 'Completed' && <FiCheck className="mr-1" />}
        {status === 'Processing' && <FiClock className="mr-1" />}
        {label}
      </span>
    );
  };

  const getTypeLabel = (type: string) => {
    const labels: Record<string, string> = {
      Access: 'Acceso',
      Rectification: 'Rectificación',
      Cancellation: 'Cancelación',
      Opposition: 'Oposición',
      Portability: 'Portabilidad',
    };
    return labels[type] || type;
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <FiLoader className="w-8 h-8 animate-spin text-blue-600" />
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
            <div className="flex items-center mb-4">
              <div className="p-3 bg-blue-100 rounded-lg mr-4">
                <FiShield className="w-8 h-8 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Centro de Privacidad</h1>
                <p className="text-gray-600">Gestiona tus datos personales según la Ley 172-13</p>
              </div>
            </div>

            {/* Legal Notice */}
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mt-4">
              <div className="flex">
                <FiFileText className="w-5 h-5 text-blue-600 mr-3 flex-shrink-0 mt-0.5" />
                <div className="text-sm text-blue-800">
                  <p className="font-medium mb-1">Derechos ARCO - Ley 172-13</p>
                  <p>
                    En cumplimiento con la Ley 172-13 de Protección de Datos Personales de República
                    Dominicana, tienes derecho a Acceder, Rectificar, Cancelar y Oponerte al
                    tratamiento de tus datos personales. Las solicitudes se procesarán en un plazo
                    máximo de 10 días hábiles.
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-200 mb-6">
            <nav className="-mb-px flex space-x-8">
              <button
                onClick={() => setActiveTab('overview')}
                className={`py-4 px-1 border-b-2 font-medium text-sm ${
                  activeTab === 'overview'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                Mis Derechos
              </button>
              <button
                onClick={() => setActiveTab('history')}
                className={`py-4 px-1 border-b-2 font-medium text-sm ${
                  activeTab === 'history'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                Historial de Solicitudes
              </button>
            </nav>
          </div>

          {activeTab === 'overview' && (
            <>
              {/* ARCO Rights Cards */}
              <div className="grid gap-4 md:grid-cols-2 mb-8">
                {arcoRights.map((right) => (
                  <div
                    key={right.id}
                    className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow"
                  >
                    <div className="flex items-start">
                      <div className={`p-2 rounded-lg ${right.color} text-white mr-4`}>
                        <right.icon className="w-5 h-5" />
                      </div>
                      <div className="flex-1">
                        <h3 className="text-lg font-semibold text-gray-900 mb-1">{right.title}</h3>
                        <p className="text-sm text-gray-600 mb-4">{right.description}</p>
                        <Link
                          to={right.link}
                          className="inline-flex items-center text-sm font-medium text-blue-600 hover:text-blue-800"
                        >
                          {right.action}
                          <span className="ml-1">→</span>
                        </Link>
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {/* Data Download Section */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-8">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="p-3 bg-purple-100 rounded-lg mr-4">
                      <FiDownload className="w-6 h-6 text-purple-600" />
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900">Descargar mis datos</h3>
                      <p className="text-sm text-gray-600">
                        Obtén una copia de todos tus datos personales en formato JSON o PDF
                      </p>
                    </div>
                  </div>
                  <Link
                    to="/settings/privacy/download-my-data"
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                  >
                    <FiDownload className="mr-2" />
                    Solicitar descarga
                  </Link>
                </div>
              </div>

              {/* Consent Management Section */}
              <div
                id="consents"
                className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-8"
              >
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <FiLock className="mr-2 text-gray-600" />
                  Gestión de Consentimientos
                  {saving && <FiLoader className="ml-2 w-4 h-4 animate-spin text-blue-500" />}
                </h3>
                {error && (
                  <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
                    {error}
                  </div>
                )}
                <div className="space-y-4">
                  {/* Marketing Consent */}
                  <div className="flex items-center justify-between py-3 border-b border-gray-100">
                    <div>
                      <p className="font-medium text-gray-900">Comunicaciones de marketing</p>
                      <p className="text-sm text-gray-500">
                        Recibir ofertas, promociones y novedades por email
                      </p>
                    </div>
                    <button
                      onClick={() =>
                        updatePreference('emailPromotions', !preferences?.email.promotions)
                      }
                      disabled={saving}
                      className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                        preferences?.email.promotions ? 'bg-blue-600' : 'bg-gray-200'
                      } ${saving ? 'opacity-50' : ''}`}
                    >
                      <span
                        className={`inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                          preferences?.email.promotions ? 'translate-x-5' : 'translate-x-0'
                        }`}
                      />
                    </button>
                  </div>

                  {/* Analytics Consent */}
                  <div className="flex items-center justify-between py-3 border-b border-gray-100">
                    <div>
                      <p className="font-medium text-gray-900">Analytics y mejora del servicio</p>
                      <p className="text-sm text-gray-500">
                        Ayudarnos a mejorar OKLA con datos de uso anónimos
                      </p>
                    </div>
                    <button
                      onClick={() =>
                        updatePreference('allowAnalytics', !preferences?.privacy.allowAnalytics)
                      }
                      disabled={saving}
                      className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                        preferences?.privacy.allowAnalytics ? 'bg-blue-600' : 'bg-gray-200'
                      } ${saving ? 'opacity-50' : ''}`}
                    >
                      <span
                        className={`inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                          preferences?.privacy.allowAnalytics ? 'translate-x-5' : 'translate-x-0'
                        }`}
                      />
                    </button>
                  </div>

                  {/* Third Party Consent */}
                  <div className="flex items-center justify-between py-3">
                    <div>
                      <p className="font-medium text-gray-900">Compartir con terceros</p>
                      <p className="text-sm text-gray-500">
                        Compartir datos con socios para ofertas personalizadas
                      </p>
                    </div>
                    <button
                      onClick={() =>
                        updatePreference(
                          'allowThirdPartySharing',
                          !preferences?.privacy.allowThirdPartySharing
                        )
                      }
                      disabled={saving}
                      className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 ${
                        preferences?.privacy.allowThirdPartySharing ? 'bg-blue-600' : 'bg-gray-200'
                      } ${saving ? 'opacity-50' : ''}`}
                    >
                      <span
                        className={`inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out ${
                          preferences?.privacy.allowThirdPartySharing
                            ? 'translate-x-5'
                            : 'translate-x-0'
                        }`}
                      />
                    </button>
                  </div>
                </div>
              </div>

              {/* Data Categories */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
                  <FiFileText className="mr-2 text-gray-600" />
                  Datos que recopilamos
                </h3>
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {dataCategories.map((category, index) => (
                    <div key={index} className="bg-gray-50 rounded-lg p-4">
                      <h4 className="font-medium text-gray-900 mb-2">{category.name}</h4>
                      <ul className="space-y-1">
                        {category.items.map((item, itemIndex) => (
                          <li key={itemIndex} className="text-sm text-gray-600 flex items-center">
                            <span className="w-1.5 h-1.5 bg-gray-400 rounded-full mr-2"></span>
                            {item}
                          </li>
                        ))}
                      </ul>
                    </div>
                  ))}
                </div>
              </div>
            </>
          )}

          {activeTab === 'history' && (
            <div className="bg-white rounded-lg shadow-sm border border-gray-200">
              <div className="px-6 py-4 border-b border-gray-200">
                <h3 className="text-lg font-semibold text-gray-900">
                  Historial de Solicitudes ARCO
                </h3>
              </div>
              {requestHistory.length > 0 ? (
                <div className="divide-y divide-gray-200">
                  {requestHistory.map((request) => (
                    <div key={request.id} className="px-6 py-4 flex items-center justify-between">
                      <div>
                        <p className="font-medium text-gray-900">{request.description}</p>
                        <div className="flex items-center mt-1 space-x-4 text-sm text-gray-500">
                          <span>Tipo: {getTypeLabel(request.type)}</span>
                          <span>
                            Fecha: {new Date(request.createdAt).toLocaleDateString('es-DO')}
                          </span>
                        </div>
                      </div>
                      {getStatusBadge(request.status)}
                    </div>
                  ))}
                </div>
              ) : (
                <div className="px-6 py-12 text-center text-gray-500">
                  <FiFileText className="w-12 h-12 mx-auto text-gray-300 mb-4" />
                  <p>No tienes solicitudes anteriores</p>
                </div>
              )}
            </div>
          )}

          {/* Contact Info */}
          <div className="mt-8 text-center text-sm text-gray-500">
            <p>¿Necesitas ayuda? Contáctanos en:</p>
            <a
              href="mailto:privacidad@okla.com.do"
              className="text-blue-600 hover:text-blue-800 flex items-center justify-center mt-1"
            >
              <FiMail className="mr-1" />
              privacidad@okla.com.do
            </a>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default PrivacyCenterPage;
