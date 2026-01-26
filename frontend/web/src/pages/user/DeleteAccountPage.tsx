/**
 * DeleteAccountPage - Solicitud de eliminación de cuenta
 *
 * Permite al usuario solicitar la eliminación completa de su cuenta
 * según el derecho de Cancelación de la Ley 172-13.
 *
 * @module pages/user/DeleteAccountPage
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  FiTrash2,
  FiArrowLeft,
  FiAlertTriangle,
  FiCheck,
  FiX,
  FiLoader,
  FiLock,
  FiMail,
  FiClock,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

type DeletionReason =
  | 'privacy_concerns'
  | 'no_longer_needed'
  | 'found_alternative'
  | 'bad_experience'
  | 'too_many_emails'
  | 'other';

const DeleteAccountPage = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState<'reason' | 'confirm' | 'submitted'>('reason');
  const [selectedReason, setSelectedReason] = useState<DeletionReason | null>(null);
  const [otherReason, setOtherReason] = useState('');
  const [password, setPassword] = useState('');
  const [confirmText, setConfirmText] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  const reasons: { id: DeletionReason; label: string }[] = [
    { id: 'privacy_concerns', label: 'Preocupaciones de privacidad' },
    { id: 'no_longer_needed', label: 'Ya no necesito el servicio' },
    { id: 'found_alternative', label: 'Encontré una alternativa' },
    { id: 'bad_experience', label: 'Tuve una mala experiencia' },
    { id: 'too_many_emails', label: 'Recibo demasiados emails' },
    { id: 'other', label: 'Otra razón' },
  ];

  const consequences = [
    'Tu perfil y todos tus datos personales serán eliminados permanentemente',
    'Tus anuncios de vehículos serán desactivados y eliminados',
    'Tus favoritos, alertas y búsquedas guardadas se perderán',
    'El historial de mensajes con vendedores será eliminado',
    'No podrás recuperar tu cuenta una vez eliminada',
    'Si eres dealer, tu suscripción será cancelada sin reembolso',
  ];

  const handleNextStep = () => {
    if (!selectedReason) {
      setError('Por favor selecciona una razón');
      return;
    }
    if (selectedReason === 'other' && !otherReason.trim()) {
      setError('Por favor especifica tu razón');
      return;
    }
    setError('');
    setStep('confirm');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!password) {
      setError('Por favor ingresa tu contraseña');
      return;
    }
    if (confirmText.toLowerCase() !== 'eliminar mi cuenta') {
      setError('Por favor escribe "ELIMINAR MI CUENTA" correctamente');
      return;
    }

    setIsSubmitting(true);

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 2000));

    setIsSubmitting(false);
    setStep('submitted');
  };

  if (step === 'submitted') {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 py-12">
          <div className="max-w-lg mx-auto px-4">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
              <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-6">
                <FiClock className="w-8 h-8 text-yellow-600" />
              </div>
              <h1 className="text-2xl font-bold text-gray-900 mb-2">
                Solicitud de eliminación recibida
              </h1>
              <p className="text-gray-600 mb-6">
                Tu solicitud ha sido registrada y será procesada en un plazo máximo de 10 días
                hábiles, según lo establecido en la Ley 172-13.
              </p>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6 text-left">
                <div className="flex">
                  <FiMail className="w-5 h-5 text-blue-600 mr-3 flex-shrink-0 mt-0.5" />
                  <div className="text-sm text-blue-800">
                    <p className="font-medium">¿Cambiaste de opinión?</p>
                    <p>
                      Tienes 7 días para cancelar esta solicitud. Después de ese período, la
                      eliminación será irreversible. Revisa tu email para más instrucciones.
                    </p>
                  </div>
                </div>
              </div>

              <div className="bg-gray-50 rounded-lg p-4 mb-6 text-left">
                <h3 className="font-medium text-gray-900 mb-2">Próximos pasos:</h3>
                <ol className="text-sm text-gray-600 space-y-2">
                  <li className="flex items-start">
                    <span className="flex-shrink-0 w-5 h-5 flex items-center justify-center rounded-full bg-gray-200 text-xs font-medium mr-2">
                      1
                    </span>
                    Recibirás un email de confirmación
                  </li>
                  <li className="flex items-start">
                    <span className="flex-shrink-0 w-5 h-5 flex items-center justify-center rounded-full bg-gray-200 text-xs font-medium mr-2">
                      2
                    </span>
                    Período de gracia de 7 días para cancelar
                  </li>
                  <li className="flex items-start">
                    <span className="flex-shrink-0 w-5 h-5 flex items-center justify-center rounded-full bg-gray-200 text-xs font-medium mr-2">
                      3
                    </span>
                    Eliminación completa en máximo 10 días hábiles
                  </li>
                  <li className="flex items-start">
                    <span className="flex-shrink-0 w-5 h-5 flex items-center justify-center rounded-full bg-gray-200 text-xs font-medium mr-2">
                      4
                    </span>
                    Email final confirmando la eliminación
                  </li>
                </ol>
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
                  to="/"
                  className="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                >
                  Ir al inicio
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
              <div className="p-3 bg-red-100 rounded-lg mr-4">
                <FiTrash2 className="w-6 h-6 text-red-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Eliminar mi cuenta</h1>
                <p className="text-gray-600">Solicitar eliminación permanente de datos</p>
              </div>
            </div>
          </div>

          {/* Warning Banner */}
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
            <div className="flex">
              <FiAlertTriangle className="w-5 h-5 text-red-600 mr-3 flex-shrink-0 mt-0.5" />
              <div className="text-sm text-red-800">
                <p className="font-medium mb-1">⚠️ Esta acción es irreversible</p>
                <p>
                  Una vez que tu cuenta sea eliminada, no podrás recuperar tus datos, anuncios,
                  favoritos ni historial de mensajes. Por favor, considera descargar tus datos antes
                  de continuar.
                </p>
              </div>
            </div>
          </div>

          {/* Legal Notice */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
            <div className="flex">
              <FiLock className="w-5 h-5 text-blue-600 mr-3 flex-shrink-0 mt-0.5" />
              <div className="text-sm text-blue-800">
                <p className="font-medium mb-1">Derecho de Cancelación - Ley 172-13</p>
                <p>
                  De acuerdo con la Ley 172-13, tienes derecho a solicitar la eliminación de tus
                  datos personales. La solicitud será procesada en un plazo máximo de 10 días
                  hábiles.
                </p>
              </div>
            </div>
          </div>

          {/* Step Indicator */}
          <div className="flex items-center mb-8">
            <div
              className={`flex items-center justify-center w-8 h-8 rounded-full ${
                step === 'reason' ? 'bg-red-600 text-white' : 'bg-green-500 text-white'
              }`}
            >
              {step === 'reason' ? '1' : <FiCheck />}
            </div>
            <div className="flex-1 h-1 mx-4 bg-gray-200">
              <div
                className={`h-full transition-all duration-300 ${
                  step === 'confirm' ? 'bg-red-600 w-full' : 'bg-gray-200 w-0'
                }`}
              />
            </div>
            <div
              className={`flex items-center justify-center w-8 h-8 rounded-full ${
                step === 'confirm' ? 'bg-red-600 text-white' : 'bg-gray-200 text-gray-500'
              }`}
            >
              2
            </div>
          </div>

          {step === 'reason' && (
            <div className="space-y-6">
              {/* Reason Selection */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  ¿Por qué deseas eliminar tu cuenta?
                </h2>
                <div className="space-y-3">
                  {reasons.map((reason) => (
                    <label
                      key={reason.id}
                      className={`flex items-center p-4 rounded-lg border cursor-pointer transition-colors ${
                        selectedReason === reason.id
                          ? 'border-red-500 bg-red-50'
                          : 'border-gray-200 hover:bg-gray-50'
                      }`}
                    >
                      <input
                        type="radio"
                        name="reason"
                        value={reason.id}
                        checked={selectedReason === reason.id}
                        onChange={() => setSelectedReason(reason.id)}
                        className="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-300"
                      />
                      <span className="ml-3 text-sm font-medium text-gray-900">{reason.label}</span>
                    </label>
                  ))}
                </div>

                {selectedReason === 'other' && (
                  <textarea
                    value={otherReason}
                    onChange={(e) => setOtherReason(e.target.value)}
                    placeholder="Por favor cuéntanos más..."
                    className="mt-4 w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500"
                    rows={3}
                  />
                )}

                {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
              </div>

              {/* Consequences */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  Lo que sucederá cuando elimines tu cuenta
                </h2>
                <ul className="space-y-3">
                  {consequences.map((consequence, index) => (
                    <li key={index} className="flex items-start">
                      <FiX className="w-5 h-5 text-red-500 mr-3 flex-shrink-0 mt-0.5" />
                      <span className="text-sm text-gray-600">{consequence}</span>
                    </li>
                  ))}
                </ul>
              </div>

              {/* Download Data Suggestion */}
              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex items-center justify-between">
                  <div className="flex items-start">
                    <FiAlertTriangle className="w-5 h-5 text-yellow-600 mr-3 flex-shrink-0 mt-0.5" />
                    <div className="text-sm text-yellow-800">
                      <p className="font-medium">Antes de continuar</p>
                      <p>Te recomendamos descargar tus datos antes de eliminar tu cuenta.</p>
                    </div>
                  </div>
                  <Link
                    to="/settings/privacy/download-my-data"
                    className="ml-4 inline-flex items-center px-3 py-1.5 border border-yellow-600 text-xs font-medium rounded-md text-yellow-700 bg-white hover:bg-yellow-50"
                  >
                    Descargar datos
                  </Link>
                </div>
              </div>

              {/* Actions */}
              <div className="flex flex-col sm:flex-row gap-3 justify-end">
                <Link
                  to="/privacy-center"
                  className="inline-flex items-center justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  Cancelar
                </Link>
                <button
                  onClick={handleNextStep}
                  className="inline-flex items-center justify-center px-6 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                >
                  Continuar
                </button>
              </div>
            </div>
          )}

          {step === 'confirm' && (
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Confirmation Form */}
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Confirmar eliminación</h2>
                <p className="text-sm text-gray-600 mb-6">
                  Para confirmar que deseas eliminar permanentemente tu cuenta, ingresa tu
                  contraseña y escribe "ELIMINAR MI CUENTA" en el campo de confirmación.
                </p>

                <div className="space-y-4">
                  {/* Password */}
                  <div>
                    <label
                      htmlFor="password"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Contraseña actual
                    </label>
                    <input
                      type="password"
                      id="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      placeholder="Ingresa tu contraseña"
                      className="w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500"
                    />
                  </div>

                  {/* Confirmation Text */}
                  <div>
                    <label
                      htmlFor="confirmText"
                      className="block text-sm font-medium text-gray-700 mb-1"
                    >
                      Escribe "ELIMINAR MI CUENTA" para confirmar
                    </label>
                    <input
                      type="text"
                      id="confirmText"
                      value={confirmText}
                      onChange={(e) => setConfirmText(e.target.value)}
                      placeholder="ELIMINAR MI CUENTA"
                      className="w-full border-gray-300 rounded-md shadow-sm focus:ring-red-500 focus:border-red-500"
                    />
                  </div>
                </div>

                {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
              </div>

              {/* Actions */}
              <div className="flex flex-col sm:flex-row gap-3 justify-end">
                <button
                  type="button"
                  onClick={() => setStep('reason')}
                  className="inline-flex items-center justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <FiArrowLeft className="mr-2" />
                  Volver
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="inline-flex items-center justify-center px-6 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSubmitting ? (
                    <>
                      <FiLoader className="animate-spin mr-2" />
                      Procesando...
                    </>
                  ) : (
                    <>
                      <FiTrash2 className="mr-2" />
                      Eliminar mi cuenta permanentemente
                    </>
                  )}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default DeleteAccountPage;
