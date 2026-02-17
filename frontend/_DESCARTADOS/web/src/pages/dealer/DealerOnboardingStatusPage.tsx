/**
 * Dealer Onboarding Status Page
 *
 * Step 5 of dealer onboarding - shows current status and allows activation
 */

import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Check,
  Clock,
  AlertCircle,
  XCircle,
  Mail,
  FileText,
  CreditCard,
  Shield,
  ArrowRight,
  RefreshCw,
  Building2,
  Phone,
  ExternalLink,
} from 'lucide-react';
import {
  useActivateDealer,
  useOnboardingProgress,
  useDealerOnboardingDetails,
} from '@/hooks/useDealerOnboarding';
import {
  OnboardingStatus,
  getStatusLabel,
  getStatusColor,
} from '@/services/dealerOnboardingService';

interface StepProps {
  number: number;
  title: string;
  description: string;
  status: 'completed' | 'current' | 'pending' | 'error';
  icon: React.ElementType;
}

const Step: React.FC<StepProps> = ({ number, title, description, status, icon: Icon }) => {
  const statusStyles = {
    completed: {
      circle: 'bg-green-500 text-white',
      line: 'bg-green-500',
      title: 'text-gray-900',
      description: 'text-gray-500',
    },
    current: {
      circle: 'bg-blue-500 text-white animate-pulse',
      line: 'bg-gray-300',
      title: 'text-blue-600 font-semibold',
      description: 'text-blue-600',
    },
    pending: {
      circle: 'bg-gray-300 text-gray-500',
      line: 'bg-gray-300',
      title: 'text-gray-400',
      description: 'text-gray-400',
    },
    error: {
      circle: 'bg-red-500 text-white',
      line: 'bg-red-500',
      title: 'text-red-600',
      description: 'text-red-500',
    },
  };

  const styles = statusStyles[status];

  return (
    <div className="flex gap-4">
      <div className="flex flex-col items-center">
        <div className={`w-10 h-10 rounded-full flex items-center justify-center ${styles.circle}`}>
          {status === 'completed' ? (
            <Check className="h-5 w-5" />
          ) : status === 'error' ? (
            <XCircle className="h-5 w-5" />
          ) : (
            <Icon className="h-5 w-5" />
          )}
        </div>
        <div className={`w-0.5 h-full min-h-[40px] ${styles.line}`} />
      </div>
      <div className="pb-8">
        <h4 className={`font-medium ${styles.title}`}>{title}</h4>
        <p className={`text-sm ${styles.description}`}>{description}</p>
      </div>
    </div>
  );
};

export const DealerOnboardingStatusPage: React.FC = () => {
  const navigate = useNavigate();
  const { dealerId, status, hasOnboardingInProgress } = useOnboardingProgress();
  const { data: details, refetch } = useDealerOnboardingDetails(dealerId || undefined);
  const activateMutation = useActivateDealer();

  // Redirect if no onboarding in progress
  useEffect(() => {
    if (!hasOnboardingInProgress) {
      navigate('/dealer/onboarding');
    }
  }, [hasOnboardingInProgress, navigate]);

  // Redirect based on status
  useEffect(() => {
    if (status) {
      if (!status.isEmailVerified) {
        navigate('/dealer/onboarding/verify-email');
      } else if (!status.documentsSubmitted) {
        navigate('/dealer/onboarding/documents');
      } else if (!status.azulConfigured) {
        navigate('/dealer/onboarding/payment-setup');
      }
    }
  }, [status, navigate]);

  const getStepStatus = (step: number): 'completed' | 'current' | 'pending' | 'error' => {
    if (!status) return 'pending';

    if (status.status === OnboardingStatus.Rejected) {
      return step <= status.currentStep ? 'error' : 'pending';
    }

    if (step < status.currentStep) return 'completed';
    if (step === status.currentStep) return 'current';
    return 'pending';
  };

  const handleActivate = async () => {
    if (!dealerId) return;

    try {
      await activateMutation.mutateAsync(dealerId);
      navigate('/dealer/dashboard');
    } catch (error) {
      // Error handled by mutation
    }
  };

  const handleRefresh = () => {
    refetch();
  };

  if (!status) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-600 border-t-transparent" />
      </div>
    );
  }

  const isPendingApproval = status.status === OnboardingStatus.PendingApproval;
  const isApproved = status.status === OnboardingStatus.Approved;
  const isRejected = status.status === OnboardingStatus.Rejected;
  const isActive = status.isActive;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-2xl mx-auto px-4 py-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Estado de tu Solicitud</h1>
              <p className="text-gray-500">{details?.businessName || 'Dealer'}</p>
            </div>
            <button
              onClick={handleRefresh}
              className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
              title="Actualizar"
            >
              <RefreshCw className="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-2xl mx-auto px-4 py-8">
        {/* Status Card */}
        <div
          className={`rounded-xl p-6 mb-8 ${
            isActive
              ? 'bg-green-50 border border-green-200'
              : isRejected
                ? 'bg-red-50 border border-red-200'
                : isApproved
                  ? 'bg-green-50 border border-green-200'
                  : 'bg-amber-50 border border-amber-200'
          }`}
        >
          <div className="flex items-start gap-4">
            <div
              className={`w-12 h-12 rounded-full flex items-center justify-center ${
                isActive
                  ? 'bg-green-100'
                  : isRejected
                    ? 'bg-red-100'
                    : isApproved
                      ? 'bg-green-100'
                      : 'bg-amber-100'
              }`}
            >
              {isActive ? (
                <Check className="h-6 w-6 text-green-600" />
              ) : isRejected ? (
                <XCircle className="h-6 w-6 text-red-600" />
              ) : isApproved ? (
                <Check className="h-6 w-6 text-green-600" />
              ) : (
                <Clock className="h-6 w-6 text-amber-600" />
              )}
            </div>
            <div className="flex-1">
              <h2
                className={`text-lg font-semibold ${
                  isActive
                    ? 'text-green-800'
                    : isRejected
                      ? 'text-red-800'
                      : isApproved
                        ? 'text-green-800'
                        : 'text-amber-800'
                }`}
              >
                {isActive
                  ? '¡Tu cuenta está activa!'
                  : isRejected
                    ? 'Solicitud Rechazada'
                    : isApproved
                      ? '¡Aprobado! Lista para Activar'
                      : 'Pendiente de Aprobación'}
              </h2>
              <p
                className={`text-sm mt-1 ${
                  isActive
                    ? 'text-green-700'
                    : isRejected
                      ? 'text-red-700'
                      : isApproved
                        ? 'text-green-700'
                        : 'text-amber-700'
                }`}
              >
                {isActive
                  ? 'Ya puedes empezar a publicar vehículos y recibir consultas.'
                  : isRejected
                    ? status.rejectionReason || 'Tu solicitud no fue aprobada.'
                    : isApproved
                      ? 'Tu solicitud fue aprobada. Activa tu cuenta para comenzar.'
                      : 'Estamos revisando tu documentación. Te notificaremos por email cuando esté lista.'}
              </p>
            </div>
          </div>

          {/* Action buttons */}
          {isApproved && !isActive && (
            <div className="mt-6">
              <button
                onClick={handleActivate}
                disabled={activateMutation.isPending}
                className="w-full py-3 bg-green-600 hover:bg-green-700 disabled:bg-green-300 
                  text-white font-medium rounded-lg transition-colors 
                  inline-flex items-center justify-center gap-2"
              >
                {activateMutation.isPending ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                    Activando...
                  </>
                ) : (
                  <>
                    Activar mi Cuenta
                    <ArrowRight className="h-5 w-5" />
                  </>
                )}
              </button>
            </div>
          )}

          {isActive && (
            <div className="mt-6">
              <button
                onClick={() => navigate('/dealer/dashboard')}
                className="w-full py-3 bg-blue-600 hover:bg-blue-700 
                  text-white font-medium rounded-lg transition-colors 
                  inline-flex items-center justify-center gap-2"
              >
                Ir al Dashboard
                <ArrowRight className="h-5 w-5" />
              </button>
            </div>
          )}

          {isRejected && (
            <div className="mt-6 space-y-3">
              <button
                onClick={() => navigate('/dealer/onboarding/documents')}
                className="w-full py-3 bg-blue-600 hover:bg-blue-700 
                  text-white font-medium rounded-lg transition-colors 
                  inline-flex items-center justify-center gap-2"
              >
                Corregir Documentos
                <ArrowRight className="h-5 w-5" />
              </button>
              <a
                href="mailto:soporte@okla.com.do"
                className="w-full py-3 border border-gray-300 
                  text-gray-700 font-medium rounded-lg transition-colors 
                  inline-flex items-center justify-center gap-2 hover:bg-gray-50"
              >
                Contactar Soporte
                <ExternalLink className="h-5 w-5" />
              </a>
            </div>
          )}
        </div>

        {/* Progress Steps */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-8">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">Progreso del Registro</h3>

          <div className="space-y-0">
            <Step
              number={1}
              title="Registro Completado"
              description="Datos del negocio registrados"
              status={getStepStatus(1)}
              icon={Building2}
            />
            <Step
              number={2}
              title="Email Verificado"
              description={
                status.isEmailVerified ? 'Verificado correctamente' : 'Pendiente de verificación'
              }
              status={getStepStatus(2)}
              icon={Mail}
            />
            <Step
              number={3}
              title="Documentos Enviados"
              description={
                status.documentsSubmitted ? 'Documentos en revisión' : 'Pendiente de subida'
              }
              status={getStepStatus(3)}
              icon={FileText}
            />
            <Step
              number={4}
              title="Pagos Configurados"
              description={status.azulConfigured ? 'Azul conectado' : 'Pendiente de configuración'}
              status={getStepStatus(4)}
              icon={CreditCard}
            />
            <Step
              number={5}
              title="Cuenta Activa"
              description={
                isActive
                  ? '¡Lista para usar!'
                  : isApproved
                    ? 'Aprobada, pendiente de activación'
                    : 'Pendiente de aprobación'
              }
              status={getStepStatus(5)}
              icon={Shield}
            />
          </div>
        </div>

        {/* Contact Info */}
        {isPendingApproval && (
          <div className="bg-gray-50 rounded-xl p-6">
            <h4 className="font-medium text-gray-900 mb-3">¿Tienes preguntas?</h4>
            <div className="space-y-2 text-sm text-gray-600">
              <div className="flex items-center gap-2">
                <Mail className="h-4 w-4" />
                <a href="mailto:dealers@okla.com.do" className="hover:underline">
                  dealers@okla.com.do
                </a>
              </div>
              <div className="flex items-center gap-2">
                <Phone className="h-4 w-4" />
                <a href="tel:+18095551234" className="hover:underline">
                  (809) 555-1234
                </a>
              </div>
            </div>
            <p className="text-xs text-gray-500 mt-4">
              Tiempo promedio de aprobación: 24-48 horas hábiles
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default DealerOnboardingStatusPage;
