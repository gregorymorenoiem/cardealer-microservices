/**
 * DataDownloadPage - Descargar mis datos personales
 *
 * Permite al usuario solicitar una copia de todos sus datos personales
 * según el derecho de Acceso de la Ley 172-13.
 *
 * @module pages/user/DataDownloadPage
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import {
  FiDownload,
  FiArrowLeft,
  FiCheck,
  FiClock,
  FiFile,
  FiFileText,
  FiLock,
  FiMail,
  FiAlertCircle,
  FiLoader,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

type DownloadFormat = 'json' | 'pdf';
type RequestStatus = 'idle' | 'submitting' | 'pending' | 'ready' | 'expired';

interface PreviousDownload {
  id: string;
  format: DownloadFormat;
  requestedAt: string;
  readyAt?: string;
  expiresAt?: string;
  status: RequestStatus;
  fileSize?: string;
}

const DataDownloadPage = () => {
  const [selectedFormat, setSelectedFormat] = useState<DownloadFormat>('json');
  const [includeActivity, setIncludeActivity] = useState(true);
  const [includeMessages, setIncludeMessages] = useState(true);
  const [includeFavorites, setIncludeFavorites] = useState(true);
  const [includeTransactions, setIncludeTransactions] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  // Mock previous downloads
  const previousDownloads: PreviousDownload[] = [
    {
      id: '1',
      format: 'json',
      requestedAt: '2026-01-15T10:30:00',
      readyAt: '2026-01-15T10:35:00',
      expiresAt: '2026-01-22T10:35:00',
      status: 'expired',
      fileSize: '2.4 MB',
    },
  ];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 2000));

    setIsSubmitting(false);
    setSubmitted(true);
  };

  const dataCategories = [
    {
      id: 'profile',
      label: 'Datos de perfil',
      description: 'Nombre, email, teléfono, dirección, foto',
      required: true,
      checked: true,
    },
    {
      id: 'activity',
      label: 'Historial de actividad',
      description: 'Búsquedas realizadas, vehículos vistos, clics',
      required: false,
      checked: includeActivity,
      onChange: setIncludeActivity,
    },
    {
      id: 'messages',
      label: 'Mensajes y consultas',
      description: 'Conversaciones con vendedores y dealers',
      required: false,
      checked: includeMessages,
      onChange: setIncludeMessages,
    },
    {
      id: 'favorites',
      label: 'Favoritos y alertas',
      description: 'Vehículos guardados, alertas de precio',
      required: false,
      checked: includeFavorites,
      onChange: setIncludeFavorites,
    },
    {
      id: 'transactions',
      label: 'Transacciones',
      description: 'Historial de pagos y facturas (si aplica)',
      required: false,
      checked: includeTransactions,
      onChange: setIncludeTransactions,
    },
  ];

  if (submitted) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 py-12">
          <div className="max-w-lg mx-auto px-4">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiCheck className="w-8 h-8 text-green-600" />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">Solicitud recibida</h1>
              <p className="text-gray-600 mb-6">
                Estamos preparando tu archivo de datos. Recibirás un email cuando esté listo para
                descargar (generalmente en menos de 24 horas).
              </p>
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6 text-left">
                <div className="flex">
                  <FiMail className="w-5 h-5 text-blue-600 mr-3 flex-shrink-0 mt-0.5" />
                  <div className="text-sm text-blue-800">
                    <p className="font-medium">Te notificaremos por email</p>
                    <p>El enlace de descarga estará disponible por 7 días.</p>
                  </div>
                </div>
              </div>
              <div className="flex flex-col sm:flex-row gap-3 justify-center">
                <Link
                  to="/privacy-center"
                  className="inline-flex items-center justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <FiArrowLeft className="mr-2" />
                  Volver al Centro de Privacidad
                </Link>
                <Link
                  to="/dashboard"
                  className="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                >
                  Ir al Dashboard
                </Link>
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-6">
            <Link
              to="/privacy-center"
              className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-4"
            >
              <FiArrowLeft className="mr-1" />
              Volver al Centro de Privacidad
            </Link>
            <div className="flex items-center">
              <div className="p-3 bg-purple-100 rounded-lg mr-4">
                <FiDownload className="w-6 h-6 text-purple-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Descargar mis datos</h1>
                <p className="text-gray-600">Obtén una copia de tu información personal</p>
              </div>
            </div>
          </div>

          {/* Legal Notice */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <div className="flex">
              <FiLock className="w-5 h-5 text-blue-600 mr-3 flex-shrink-0 mt-0.5" />
              <div className="text-sm text-blue-800">
                <p className="font-medium mb-1">Derecho de Acceso - Ley 172-13</p>
                <p>
                  Tienes derecho a recibir una copia de tus datos personales en un formato
                  estructurado y de uso común. El archivo estará protegido y disponible por 7 días
                  después de su generación.
                </p>
              </div>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            {/* Format Selection */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Formato de descarga</h2>
              <div className="grid grid-cols-2 gap-4">
                <label
                  className={`relative flex cursor-pointer rounded-lg border p-4 focus:outline-none ${
                    selectedFormat === 'json'
                      ? 'border-purple-500 ring-2 ring-purple-500'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name="format"
                    value="json"
                    checked={selectedFormat === 'json'}
                    onChange={() => setSelectedFormat('json')}
                    className="sr-only"
                  />
                  <div className="flex items-center">
                    <FiFile className="w-8 h-8 text-purple-500 mr-3" />
                    <div>
                      <span className="block text-sm font-medium text-gray-900">JSON</span>
                      <span className="block text-xs text-gray-500">Formato técnico, completo</span>
                    </div>
                  </div>
                </label>
                <label
                  className={`relative flex cursor-pointer rounded-lg border p-4 focus:outline-none ${
                    selectedFormat === 'pdf'
                      ? 'border-purple-500 ring-2 ring-purple-500'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name="format"
                    value="pdf"
                    checked={selectedFormat === 'pdf'}
                    onChange={() => setSelectedFormat('pdf')}
                    className="sr-only"
                  />
                  <div className="flex items-center">
                    <FiFileText className="w-8 h-8 text-red-500 mr-3" />
                    <div>
                      <span className="block text-sm font-medium text-gray-900">PDF</span>
                      <span className="block text-xs text-gray-500">Fácil de leer</span>
                    </div>
                  </div>
                </label>
              </div>
            </div>

            {/* Data Selection */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 mb-6">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Datos a incluir</h2>
              <div className="space-y-4">
                {dataCategories.map((category) => (
                  <label
                    key={category.id}
                    className={`flex items-start p-4 rounded-lg border ${
                      category.required
                        ? 'bg-gray-50 border-gray-200'
                        : 'border-gray-200 hover:bg-gray-50'
                    } ${!category.required ? 'cursor-pointer' : ''}`}
                  >
                    <input
                      type="checkbox"
                      checked={category.checked}
                      onChange={(e) => category.onChange?.(e.target.checked)}
                      disabled={category.required}
                      className="h-4 w-4 text-purple-600 focus:ring-purple-500 border-gray-300 rounded mt-1"
                    />
                    <div className="ml-3">
                      <span className="text-sm font-medium text-gray-900">
                        {category.label}
                        {category.required && (
                          <span className="ml-2 text-xs text-gray-500">(obligatorio)</span>
                        )}
                      </span>
                      <p className="text-sm text-gray-500">{category.description}</p>
                    </div>
                  </label>
                ))}
              </div>
            </div>

            {/* Submit Button */}
            <div className="flex flex-col sm:flex-row gap-3 justify-end">
              <Link
                to="/privacy-center"
                className="inline-flex items-center justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
              >
                Cancelar
              </Link>
              <button
                type="submit"
                disabled={isSubmitting}
                className="inline-flex items-center justify-center px-6 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isSubmitting ? (
                  <>
                    <FiLoader className="animate-spin mr-2" />
                    Procesando...
                  </>
                ) : (
                  <>
                    <FiDownload className="mr-2" />
                    Solicitar descarga
                  </>
                )}
              </button>
            </div>
          </form>

          {/* Previous Downloads */}
          {previousDownloads.length > 0 && (
            <div className="mt-8">
              <h2 className="text-lg font-semibold text-gray-900 mb-4">Descargas anteriores</h2>
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 divide-y divide-gray-200">
                {previousDownloads.map((download) => (
                  <div key={download.id} className="p-4 flex items-center justify-between">
                    <div className="flex items-center">
                      {download.format === 'json' ? (
                        <FiFile className="w-8 h-8 text-purple-500 mr-3" />
                      ) : (
                        <FiFileText className="w-8 h-8 text-red-500 mr-3" />
                      )}
                      <div>
                        <p className="text-sm font-medium text-gray-900">
                          datos-personales.{download.format}
                        </p>
                        <p className="text-xs text-gray-500">
                          Solicitado: {new Date(download.requestedAt).toLocaleDateString('es-DO')}
                          {download.fileSize && ` • ${download.fileSize}`}
                        </p>
                      </div>
                    </div>
                    {download.status === 'expired' ? (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600">
                        <FiClock className="mr-1" /> Expirado
                      </span>
                    ) : download.status === 'ready' ? (
                      <button className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-purple-600 hover:bg-purple-700">
                        <FiDownload className="mr-1" /> Descargar
                      </button>
                    ) : (
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                        <FiClock className="mr-1" /> Procesando
                      </span>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Info */}
          <div className="mt-8 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <div className="flex">
              <FiAlertCircle className="w-5 h-5 text-yellow-600 mr-3 flex-shrink-0 mt-0.5" />
              <div className="text-sm text-yellow-800">
                <p className="font-medium mb-1">Información importante</p>
                <ul className="list-disc list-inside space-y-1">
                  <li>El archivo estará protegido con contraseña (la enviaremos por email)</li>
                  <li>El enlace de descarga expira después de 7 días</li>
                  <li>Solo se permite una solicitud cada 30 días</li>
                  <li>El proceso puede tomar hasta 24 horas para cuentas con mucha actividad</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default DataDownloadPage;
