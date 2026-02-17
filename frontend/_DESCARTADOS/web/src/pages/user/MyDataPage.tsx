/**
 * MyDataPage - Ver todos mis datos personales
 *
 * Muestra un resumen de todos los datos personales que OKLA tiene sobre el usuario
 * según el derecho de Acceso de la Ley 172-13.
 *
 * @module pages/user/MyDataPage
 * @version 1.0.0
 * @since Enero 26, 2026
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  FiArrowLeft,
  FiUser,
  FiActivity,
  FiDollarSign,
  FiShield,
  FiLoader,
  FiDownload,
  FiRefreshCw,
  FiAlertCircle,
  FiEye,
  FiHeart,
  FiBell,
  FiMessageCircle,
  FiCalendar,
  FiMail,
  FiPhone,
  FiMapPin,
  FiCheck,
  FiX,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import { privacyService } from '../../services/privacyService';
import type { UserDataSummary } from '../../services/privacyService';

const MyDataPage = () => {
  const [data, setData] = useState<UserDataSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      const summary = await privacyService.getMyDataSummary();
      setData(summary);
    } catch (err) {
      console.error('Error loading data summary:', err);
      setError('Error al cargar tus datos. Por favor, inténtalo de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <FiLoader className="w-12 h-12 animate-spin text-blue-600 mx-auto mb-4" />
            <p className="text-gray-600">Cargando tus datos...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 py-12">
          <div className="max-w-lg mx-auto px-4">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
              <FiAlertCircle className="w-12 h-12 text-red-500 mx-auto mb-4" />
              <h2 className="text-lg font-semibold text-gray-900 mb-2">Error</h2>
              <p className="text-gray-600 mb-4">{error}</p>
              <button
                onClick={loadData}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              >
                <FiRefreshCw className="mr-2" />
                Reintentar
              </button>
            </div>
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
          <div className="mb-6">
            <Link
              to="/privacy-center"
              className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-4"
            >
              <FiArrowLeft className="mr-1" />
              Volver al Centro de Privacidad
            </Link>
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <div className="p-3 bg-blue-100 rounded-lg mr-4">
                  <FiEye className="w-6 h-6 text-blue-600" />
                </div>
                <div>
                  <h1 className="text-2xl font-bold text-gray-900">Mis Datos Personales</h1>
                  <p className="text-gray-600">
                    Generado el {data && new Date(data.generatedAt).toLocaleString('es-DO')}
                  </p>
                </div>
              </div>
              <Link
                to="/settings/privacy/download-my-data"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                <FiDownload className="mr-2" />
                Descargar todo
              </Link>
            </div>
          </div>

          {/* Profile Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center mb-4">
              <div className="p-2 bg-green-100 rounded-lg mr-3">
                <FiUser className="w-5 h-5 text-green-600" />
              </div>
              <h2 className="text-lg font-semibold text-gray-900">Datos de Perfil</h2>
            </div>
            {data && (
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-3">
                  <div className="flex items-start">
                    <FiUser className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Nombre completo</p>
                      <p className="text-sm font-medium text-gray-900">{data.profile.fullName}</p>
                    </div>
                  </div>
                  <div className="flex items-start">
                    <FiMail className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Email</p>
                      <p className="text-sm font-medium text-gray-900">
                        {data.profile.email}
                        {data.profile.emailVerified && (
                          <span className="ml-2 inline-flex items-center text-green-600">
                            <FiCheck className="w-3 h-3" />
                          </span>
                        )}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start">
                    <FiPhone className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Teléfono</p>
                      <p className="text-sm font-medium text-gray-900">
                        {data.profile.phone || 'No registrado'}
                      </p>
                    </div>
                  </div>
                </div>
                <div className="space-y-3">
                  <div className="flex items-start">
                    <FiMapPin className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Ubicación</p>
                      <p className="text-sm font-medium text-gray-900">
                        {data.profile.city && data.profile.province
                          ? `${data.profile.city}, ${data.profile.province}`
                          : 'No especificada'}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start">
                    <FiShield className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Tipo de cuenta</p>
                      <p className="text-sm font-medium text-gray-900">
                        {data.profile.accountType}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start">
                    <FiCalendar className="w-4 h-4 text-gray-400 mt-1 mr-3" />
                    <div>
                      <p className="text-xs text-gray-500">Miembro desde</p>
                      <p className="text-sm font-medium text-gray-900">
                        {new Date(data.profile.memberSince).toLocaleDateString('es-DO')}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Activity Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center mb-4">
              <div className="p-2 bg-purple-100 rounded-lg mr-3">
                <FiActivity className="w-5 h-5 text-purple-600" />
              </div>
              <h2 className="text-lg font-semibold text-gray-900">Actividad</h2>
            </div>
            {data && (
              <div className="grid gap-4 grid-cols-2 md:grid-cols-3 lg:grid-cols-5">
                <div className="bg-gray-50 rounded-lg p-4 text-center">
                  <FiEye className="w-6 h-6 text-blue-500 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">
                    {data.activity.totalVehicleViews}
                  </p>
                  <p className="text-xs text-gray-500">Vehículos vistos</p>
                </div>
                <div className="bg-gray-50 rounded-lg p-4 text-center">
                  <FiHeart className="w-6 h-6 text-red-500 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{data.activity.totalFavorites}</p>
                  <p className="text-xs text-gray-500">Favoritos</p>
                </div>
                <div className="bg-gray-50 rounded-lg p-4 text-center">
                  <FiBell className="w-6 h-6 text-yellow-500 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{data.activity.totalAlerts}</p>
                  <p className="text-xs text-gray-500">Alertas</p>
                </div>
                <div className="bg-gray-50 rounded-lg p-4 text-center">
                  <FiMessageCircle className="w-6 h-6 text-green-500 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{data.activity.totalMessages}</p>
                  <p className="text-xs text-gray-500">Mensajes</p>
                </div>
                <div className="bg-gray-50 rounded-lg p-4 text-center">
                  <FiActivity className="w-6 h-6 text-purple-500 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{data.activity.totalSearches}</p>
                  <p className="text-xs text-gray-500">Búsquedas</p>
                </div>
              </div>
            )}
            {data?.activity.lastActivity && (
              <p className="text-xs text-gray-500 mt-4 text-center">
                Última actividad: {new Date(data.activity.lastActivity).toLocaleString('es-DO')}
              </p>
            )}
          </div>

          {/* Transactions Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center mb-4">
              <div className="p-2 bg-yellow-100 rounded-lg mr-3">
                <FiDollarSign className="w-5 h-5 text-yellow-600" />
              </div>
              <h2 className="text-lg font-semibold text-gray-900">Transacciones</h2>
            </div>
            {data && (
              <div className="grid gap-4 md:grid-cols-4">
                <div className="text-center">
                  <p className="text-2xl font-bold text-gray-900">
                    {data.transactions.totalPayments}
                  </p>
                  <p className="text-sm text-gray-500">Pagos realizados</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-gray-900">
                    ${data.transactions.totalSpent.toLocaleString()}
                  </p>
                  <p className="text-sm text-gray-500">Total gastado</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-gray-900">
                    {data.transactions.totalInvoices}
                  </p>
                  <p className="text-sm text-gray-500">Facturas</p>
                </div>
                <div className="text-center">
                  <p className="text-2xl font-bold text-gray-900">
                    {data.transactions.activeSubscription || 'Ninguna'}
                  </p>
                  <p className="text-sm text-gray-500">Suscripción activa</p>
                </div>
              </div>
            )}
          </div>

          {/* Privacy Settings Section */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
            <div className="flex items-center mb-4">
              <div className="p-2 bg-blue-100 rounded-lg mr-3">
                <FiShield className="w-5 h-5 text-blue-600" />
              </div>
              <h2 className="text-lg font-semibold text-gray-900">Configuración de Privacidad</h2>
            </div>
            {data && (
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-700">Marketing por email</span>
                  {data.privacy.marketingOptIn ? (
                    <span className="inline-flex items-center text-green-600">
                      <FiCheck className="w-4 h-4 mr-1" /> Activado
                    </span>
                  ) : (
                    <span className="inline-flex items-center text-gray-500">
                      <FiX className="w-4 h-4 mr-1" /> Desactivado
                    </span>
                  )}
                </div>
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-700">Analytics</span>
                  {data.privacy.analyticsOptIn ? (
                    <span className="inline-flex items-center text-green-600">
                      <FiCheck className="w-4 h-4 mr-1" /> Activado
                    </span>
                  ) : (
                    <span className="inline-flex items-center text-gray-500">
                      <FiX className="w-4 h-4 mr-1" /> Desactivado
                    </span>
                  )}
                </div>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-700">Compartir con terceros</span>
                  {data.privacy.thirdPartyOptIn ? (
                    <span className="inline-flex items-center text-green-600">
                      <FiCheck className="w-4 h-4 mr-1" /> Activado
                    </span>
                  ) : (
                    <span className="inline-flex items-center text-gray-500">
                      <FiX className="w-4 h-4 mr-1" /> Desactivado
                    </span>
                  )}
                </div>
              </div>
            )}
            <p className="text-xs text-gray-500 mt-4">
              Última actualización:{' '}
              {data && new Date(data.privacy.lastUpdated).toLocaleDateString('es-DO')}
            </p>
          </div>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/settings/privacy/download-my-data"
              className="inline-flex items-center justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
            >
              <FiDownload className="mr-2" />
              Descargar todos mis datos
            </Link>
            <Link
              to="/profile"
              className="inline-flex items-center justify-center px-6 py-3 border border-gray-300 text-base font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
            >
              Editar mi perfil
            </Link>
          </div>

          {/* Legal Note */}
          <div className="mt-8 text-center text-xs text-gray-500">
            <p>
              Esta información se proporciona en cumplimiento con la Ley 172-13 de Protección de
              Datos Personales de República Dominicana.
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default MyDataPage;
