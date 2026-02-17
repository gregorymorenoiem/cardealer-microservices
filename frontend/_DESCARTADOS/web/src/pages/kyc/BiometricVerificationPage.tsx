import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  CheckCircle,
  XCircle,
  AlertTriangle,
  Loader2,
  ArrowLeft,
  ArrowRight,
  Shield,
  CreditCard,
  Camera,
  User,
  FileCheck,
  HelpCircle,
  Phone,
  Mail,
  MessageCircle,
} from 'lucide-react';
import MainLayout from '../../layouts/MainLayout';
import { DocumentCapture, type DocumentSide } from '../../components/kyc/DocumentCapture';
import { LivenessChallenge } from '../../components/kyc/LivenessChallenge';
import identityVerificationService, {
  DocumentType,
  type StartVerificationResponse,
  type DocumentProcessedResponse,
  type VerificationCompletedResponse,
  type VerificationFailedResponse,
  type LivenessData,
  type CanStartVerificationResult,
  type VerificationSessionDetails,
} from '../../services/identityVerificationService';

// ===== Types =====
type VerificationStep =
  | 'intro'
  | 'document-front'
  | 'document-back'
  | 'liveness'
  | 'processing'
  | 'success'
  | 'failed';

interface StepInfo {
  step: number;
  title: string;
  subtitle: string;
  icon: React.FC<{ className?: string }>;
}

// ===== Constants =====
const STEPS: Record<VerificationStep, StepInfo> = {
  intro: { step: 0, title: 'Iniciar verificación', subtitle: 'Prepara tu documento', icon: Shield },
  'document-front': {
    step: 1,
    title: 'Frente del documento',
    subtitle: 'Captura el frente de tu cédula',
    icon: CreditCard,
  },
  'document-back': {
    step: 2,
    title: 'Reverso del documento',
    subtitle: 'Captura el reverso de tu cédula',
    icon: CreditCard,
  },
  liveness: {
    step: 3,
    title: 'Verificación de vida',
    subtitle: 'Verifica que eres una persona real',
    icon: Camera,
  },
  processing: { step: 4, title: 'Procesando', subtitle: 'Verificando tu identidad', icon: Loader2 },
  success: {
    step: 5,
    title: '¡Verificación exitosa!',
    subtitle: 'Tu identidad ha sido verificada',
    icon: CheckCircle,
  },
  failed: {
    step: 5,
    title: 'Verificación fallida',
    subtitle: 'No pudimos verificar tu identidad',
    icon: XCircle,
  },
};

// ===== Component =====
export function BiometricVerificationPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // State
  const [currentStep, setCurrentStep] = useState<VerificationStep>('intro');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [canStartResult, setCanStartResult] = useState<CanStartVerificationResult | null>(null);
  const [session, setSession] = useState<StartVerificationResponse | null>(null);
  const [documentType] = useState<DocumentType>(DocumentType.Cedula);
  const [documentFrontResult, setDocumentFrontResult] = useState<DocumentProcessedResponse | null>(
    null
  );
  const [documentBackResult, setDocumentBackResult] = useState<DocumentProcessedResponse | null>(
    null
  );
  const [verificationResult, setVerificationResult] =
    useState<VerificationCompletedResponse | null>(null);
  const [failedResult, setFailedResult] = useState<VerificationFailedResponse | null>(null);

  // Check if user can start verification
  useEffect(() => {
    checkCanStart();
  }, []);

  const checkCanStart = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await identityVerificationService.canStartVerification();
      setCanStartResult(result);

      if (result.hasActiveSession && result.activeSessionId) {
        // Resume existing session
        const activeSession = await identityVerificationService.getSessionStatus(
          result.activeSessionId
        );
        // Map session status to step
        if (activeSession.documentFrontCaptured && !activeSession.documentBackCaptured) {
          setCurrentStep('document-back');
        } else if (activeSession.documentBackCaptured && !activeSession.selfieCapture) {
          setCurrentStep('liveness');
        }
        // Set session info for resuming
        setSession({
          sessionId: result.activeSessionId,
          status: activeSession.status,
          documentType: 'Cedula',
          expiresAt: activeSession.expiresAt,
          expiresInSeconds: 0,
          nextStep: activeSession.currentStep,
          instructions: { title: '', steps: [], tips: [] },
          requiredChallenges: ['Blink', 'Smile', 'TurnLeft'],
        });
      } else if (result.hasApprovedKYC) {
        // Already verified
        navigate('/kyc/status');
        return;
      }
    } catch (err: any) {
      console.error('Check can start error:', err);
      setError(err.response?.data?.message || 'Error al verificar el estado');
    } finally {
      setIsLoading(false);
    }
  };

  // Start verification session
  const startVerification = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const deviceInfo = identityVerificationService.getDeviceInfo();
      const location = await identityVerificationService.getLocation();

      const response = await identityVerificationService.startVerification({
        documentType,
        deviceInfo,
        location: location || undefined,
      });

      setSession(response);
      setCurrentStep('document-front');
    } catch (err: any) {
      console.error('Start verification error:', err);
      setError(err.response?.data?.message || 'Error al iniciar la verificación');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle document capture
  const handleDocumentCapture = useCallback(
    async (image: File, side: DocumentSide) => {
      if (!session) return;

      try {
        const response = await identityVerificationService.uploadDocument(
          session.sessionId,
          side,
          image
        );

        if (side === 'Front') {
          setDocumentFrontResult(response);

          // Check for errors
          if (!response.ocrResult?.success) {
            setError(response.ocrResult?.errors?.join(', ') || 'Error al procesar el documento');
            return;
          }

          setCurrentStep('document-back');
        } else {
          setDocumentBackResult(response);

          // Check for errors
          if (!response.ocrResult?.success) {
            setError(response.ocrResult?.errors?.join(', ') || 'Error al procesar el documento');
            return;
          }

          setCurrentStep('liveness');
        }
      } catch (err: any) {
        console.error('Document capture error:', err);
        setError(err.response?.data?.message || 'Error al procesar el documento');
      }
    },
    [session]
  );

  // Handle liveness completion
  const handleLivenessComplete = useCallback(
    async (selfie: Blob, livenessData: LivenessData) => {
      if (!session) return;

      setCurrentStep('processing');

      try {
        const response = await identityVerificationService.uploadSelfie(
          session.sessionId,
          selfie,
          livenessData
        );

        // Check if verification passed
        if ('result' in response && response.result.verified) {
          setVerificationResult(response as VerificationCompletedResponse);
          setCurrentStep('success');
        } else {
          setFailedResult(response as VerificationFailedResponse);
          setCurrentStep('failed');
        }
      } catch (err: any) {
        console.error('Liveness complete error:', err);
        setFailedResult({
          sessionId: session.sessionId,
          status: 'Failed',
          result: {
            verified: false,
            failureReason: 'UnknownError',
            failureDetails: err.response?.data?.message || 'Error desconocido',
            scores: {},
          },
          attemptsRemaining: 2,
          canRetry: true,
        });
        setCurrentStep('failed');
      }
    },
    [session]
  );

  // Handle retry
  const handleRetry = useCallback(async () => {
    if (!session) return;

    setIsLoading(true);
    setError(null);

    try {
      const response = await identityVerificationService.retryVerification(session.sessionId);
      setSession(response);
      setDocumentFrontResult(null);
      setDocumentBackResult(null);
      setVerificationResult(null);
      setFailedResult(null);
      setCurrentStep('document-front');
    } catch (err: any) {
      console.error('Retry error:', err);
      setError(err.response?.data?.message || 'Error al reintentar');
    } finally {
      setIsLoading(false);
    }
  }, [session]);

  // Navigate to dashboard
  const goToDashboard = () => {
    navigate('/dashboard');
  };

  // Get support contact
  const contactSupport = () => {
    window.open(
      'https://wa.me/18095551234?text=Necesito ayuda con la verificación de identidad',
      '_blank'
    );
  };

  // Render step indicator
  const renderStepIndicator = () => {
    const steps = [
      { label: 'Documento (frente)', step: 1 },
      { label: 'Documento (reverso)', step: 2 },
      { label: 'Selfie', step: 3 },
      { label: 'Resultado', step: 4 },
    ];

    const currentStepNumber = STEPS[currentStep].step;

    return (
      <div className="w-full max-w-2xl mx-auto mb-8">
        <div className="flex items-center justify-between">
          {steps.map((s, index) => (
            <React.Fragment key={s.step}>
              <div className="flex flex-col items-center">
                <div
                  className={`w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium transition-all ${
                    s.step < currentStepNumber
                      ? 'bg-green-500 text-white'
                      : s.step === currentStepNumber || (currentStep === 'intro' && s.step === 1)
                        ? 'bg-blue-600 text-white ring-4 ring-blue-100'
                        : 'bg-gray-200 text-gray-500'
                  }`}
                >
                  {s.step < currentStepNumber ? <CheckCircle className="w-5 h-5" /> : s.step}
                </div>
                <span className="text-xs mt-2 text-gray-600 hidden sm:block">{s.label}</span>
              </div>
              {index < steps.length - 1 && (
                <div
                  className={`flex-1 h-1 mx-2 ${
                    s.step < currentStepNumber ? 'bg-green-500' : 'bg-gray-200'
                  }`}
                />
              )}
            </React.Fragment>
          ))}
        </div>
      </div>
    );
  };

  // Render intro step
  const renderIntro = () => {
    if (isLoading) {
      return (
        <div className="flex flex-col items-center justify-center py-12">
          <Loader2 className="w-12 h-12 text-blue-600 animate-spin mb-4" />
          <p className="text-gray-600">Verificando estado...</p>
        </div>
      );
    }

    if (canStartResult && !canStartResult.canStart) {
      return (
        <div className="text-center py-8">
          <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <AlertTriangle className="w-10 h-10 text-yellow-500" />
          </div>
          <h3 className="text-xl font-semibold text-gray-900 mb-2">
            No puedes iniciar una verificación
          </h3>
          <p className="text-gray-600 mb-6">{canStartResult.reason}</p>
          {canStartResult.canRetryAfter && (
            <p className="text-sm text-gray-500 mb-6">
              Podrás intentar nuevamente después de:{' '}
              {new Date(canStartResult.canRetryAfter).toLocaleString()}
            </p>
          )}
          <button
            onClick={goToDashboard}
            className="px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors"
          >
            Volver al dashboard
          </button>
        </div>
      );
    }

    return (
      <div className="text-center py-8">
        {/* Icon */}
        <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <Shield className="w-12 h-12 text-blue-600" />
        </div>

        {/* Title */}
        <h2 className="text-2xl font-bold text-gray-900 mb-2">Verificación de Identidad</h2>
        <p className="text-gray-600 mb-8">
          Verifica tu identidad para acceder a todas las funciones de la plataforma
        </p>

        {/* Requirements */}
        <div className="max-w-md mx-auto mb-8">
          <h3 className="text-left text-sm font-medium text-gray-700 mb-3">Necesitarás:</h3>
          <div className="space-y-3">
            <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
              <CreditCard className="w-6 h-6 text-blue-600" />
              <div className="text-left">
                <p className="font-medium text-gray-900">Tu cédula de identidad</p>
                <p className="text-sm text-gray-500">Original, vigente y en buen estado</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
              <Camera className="w-6 h-6 text-blue-600" />
              <div className="text-left">
                <p className="font-medium text-gray-900">Cámara del dispositivo</p>
                <p className="text-sm text-gray-500">Para capturar documento y selfie</p>
              </div>
            </div>
            <div className="flex items-center gap-3 p-3 bg-gray-50 rounded-lg">
              <User className="w-6 h-6 text-blue-600" />
              <div className="text-left">
                <p className="font-medium text-gray-900">Buena iluminación</p>
                <p className="text-sm text-gray-500">Para mejores resultados</p>
              </div>
            </div>
          </div>
        </div>

        {/* Time estimate */}
        <p className="text-sm text-gray-500 mb-6">
          ⏱️ Este proceso toma aproximadamente 2-3 minutos
        </p>

        {/* Start button */}
        <button
          onClick={startVerification}
          disabled={isLoading}
          className="w-full max-w-md mx-auto flex items-center justify-center gap-2 px-6 py-4 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
        >
          Iniciar Verificación
          <ArrowRight className="w-5 h-5" />
        </button>

        {/* Help link */}
        <button
          onClick={() => {}}
          className="mt-4 text-sm text-blue-600 hover:text-blue-700 flex items-center gap-1 mx-auto"
        >
          <HelpCircle className="w-4 h-4" />
          ¿Tienes problemas?
        </button>
      </div>
    );
  };

  // Render success step
  const renderSuccess = () => {
    return (
      <div className="text-center py-8">
        {/* Success animation */}
        <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6 animate-pulse">
          <CheckCircle className="w-16 h-16 text-green-500" />
        </div>

        {/* Title */}
        <h2 className="text-2xl font-bold text-gray-900 mb-2">¡Verificación Exitosa!</h2>
        <p className="text-gray-600 mb-6">Tu identidad ha sido verificada correctamente</p>

        {/* Extracted data summary */}
        {verificationResult?.extractedProfile && (
          <div className="max-w-md mx-auto mb-8 p-4 bg-gray-50 rounded-xl text-left">
            <h4 className="text-sm font-medium text-gray-700 mb-3 flex items-center gap-2">
              <FileCheck className="w-4 h-4" />
              Datos verificados:
            </h4>
            <dl className="space-y-2">
              <div className="flex justify-between">
                <dt className="text-gray-500">Nombre:</dt>
                <dd className="font-medium text-gray-900">
                  {verificationResult.extractedProfile.fullName}
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500">Documento:</dt>
                <dd className="font-medium text-gray-900">
                  {verificationResult.extractedProfile.documentNumber}
                </dd>
              </div>
              {verificationResult.extractedProfile.dateOfBirth && (
                <div className="flex justify-between">
                  <dt className="text-gray-500">Fecha de nacimiento:</dt>
                  <dd className="font-medium text-gray-900">
                    {new Date(verificationResult.extractedProfile.dateOfBirth).toLocaleDateString()}
                  </dd>
                </div>
              )}
            </dl>
          </div>
        )}

        {/* Score badge */}
        {verificationResult?.result && (
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-green-100 text-green-700 rounded-full text-sm font-medium mb-6">
            <CheckCircle className="w-4 h-4" />
            Score de verificación: {Math.round(verificationResult.result.overallScore * 100)}%
          </div>
        )}

        {/* Action buttons */}
        <div className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
          <button
            onClick={goToDashboard}
            className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors"
          >
            Ir al Dashboard
          </button>
        </div>
      </div>
    );
  };

  // Render failed step
  const renderFailed = () => {
    const failureMessage = failedResult?.result?.failureReason
      ? identityVerificationService.getFailureMessage(failedResult.result.failureReason)
      : 'No pudimos verificar tu identidad';

    return (
      <div className="text-center py-8">
        {/* Failed icon */}
        <div className="w-24 h-24 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <XCircle className="w-16 h-16 text-red-500" />
        </div>

        {/* Title */}
        <h2 className="text-2xl font-bold text-gray-900 mb-2">Verificación Fallida</h2>
        <p className="text-gray-600 mb-4">{failureMessage}</p>

        {/* Detailed reason */}
        {failedResult?.result?.failureDetails && (
          <p className="text-sm text-gray-500 mb-6">{failedResult.result.failureDetails}</p>
        )}

        {/* Retry info */}
        {failedResult?.canRetry && (
          <div className="max-w-md mx-auto mb-8 p-4 bg-yellow-50 border border-yellow-200 rounded-lg text-left">
            <h4 className="font-medium text-yellow-800 mb-2">
              Sugerencias para el próximo intento:
            </h4>
            <ul className="text-sm text-yellow-700 space-y-1">
              {failedResult.retryInstructions?.suggestions?.map((suggestion, index) => (
                <li key={index} className="flex items-start gap-2">
                  <span>•</span> {suggestion}
                </li>
              )) || (
                <>
                  <li className="flex items-start gap-2">
                    <span>•</span> Asegúrate de tener buena iluminación
                  </li>
                  <li className="flex items-start gap-2">
                    <span>•</span> El documento debe estar completo en la imagen
                  </li>
                  <li className="flex items-start gap-2">
                    <span>•</span> Evita reflejos y sombras
                  </li>
                  <li className="flex items-start gap-2">
                    <span>•</span> Mira directamente a la cámara durante la selfie
                  </li>
                </>
              )}
            </ul>
            <p className="text-sm text-yellow-600 mt-3">
              Intentos restantes: {failedResult.attemptsRemaining}
            </p>
          </div>
        )}

        {/* Action buttons */}
        <div className="flex flex-col sm:flex-row gap-4 max-w-md mx-auto">
          {failedResult?.canRetry ? (
            <button
              onClick={handleRetry}
              disabled={isLoading}
              className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50"
            >
              Intentar de nuevo
            </button>
          ) : (
            <button
              onClick={contactSupport}
              className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors flex items-center justify-center gap-2"
            >
              <MessageCircle className="w-5 h-5" />
              Contactar soporte
            </button>
          )}
          <button
            onClick={goToDashboard}
            className="flex-1 px-6 py-3 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition-colors"
          >
            Volver al dashboard
          </button>
        </div>

        {/* Support contact */}
        <div className="mt-8 p-4 bg-gray-50 rounded-lg max-w-md mx-auto">
          <h4 className="text-sm font-medium text-gray-700 mb-3">¿Necesitas ayuda?</h4>
          <div className="flex flex-col sm:flex-row gap-4 justify-center text-sm">
            <a
              href="mailto:soporte@okla.com.do"
              className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
            >
              <Mail className="w-4 h-4" />
              soporte@okla.com.do
            </a>
            <a
              href="tel:+18095551234"
              className="flex items-center gap-2 text-blue-600 hover:text-blue-700"
            >
              <Phone className="w-4 h-4" />
              (809) 555-1234
            </a>
          </div>
        </div>
      </div>
    );
  };

  // Render processing step
  const renderProcessing = () => {
    return (
      <div className="text-center py-12">
        <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <Loader2 className="w-12 h-12 text-blue-600 animate-spin" />
        </div>
        <h2 className="text-xl font-bold text-gray-900 mb-2">Verificando tu identidad...</h2>
        <p className="text-gray-600 mb-8">Esto puede tomar unos segundos</p>

        <div className="max-w-xs mx-auto space-y-3">
          <div className="flex items-center gap-3 text-left">
            <div className="w-6 h-6 rounded-full bg-green-100 flex items-center justify-center">
              <CheckCircle className="w-4 h-4 text-green-500" />
            </div>
            <span className="text-sm text-gray-600">Documento capturado</span>
          </div>
          <div className="flex items-center gap-3 text-left">
            <div className="w-6 h-6 rounded-full bg-green-100 flex items-center justify-center">
              <CheckCircle className="w-4 h-4 text-green-500" />
            </div>
            <span className="text-sm text-gray-600">Selfie capturada</span>
          </div>
          <div className="flex items-center gap-3 text-left">
            <div className="w-6 h-6 rounded-full bg-blue-100 flex items-center justify-center animate-pulse">
              <Loader2 className="w-4 h-4 text-blue-500 animate-spin" />
            </div>
            <span className="text-sm text-gray-600">Comparando rostro...</span>
          </div>
        </div>
      </div>
    );
  };

  // Main render
  const stepInfo = STEPS[currentStep];

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8 px-4">
        <div className="max-w-4xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <button
              onClick={() => {
                if (
                  currentStep === 'intro' ||
                  currentStep === 'success' ||
                  currentStep === 'failed'
                ) {
                  navigate(-1);
                } else if (currentStep === 'document-front') {
                  setCurrentStep('intro');
                } else if (currentStep === 'document-back') {
                  setCurrentStep('document-front');
                } else if (currentStep === 'liveness') {
                  setCurrentStep('document-back');
                }
              }}
              className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4"
            >
              <ArrowLeft className="w-5 h-5" />
              Volver
            </button>
          </div>

          {/* Step indicator (except for intro, success, failed) */}
          {!['intro', 'success', 'failed'].includes(currentStep) && renderStepIndicator()}

          {/* Main content card */}
          <div className="bg-white rounded-2xl shadow-lg p-6 sm:p-8">
            {/* Error message */}
            {error && (
              <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
                <XCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
                <div>
                  <p className="text-sm text-red-700">{error}</p>
                  <button
                    onClick={() => setError(null)}
                    className="text-xs text-red-600 hover:text-red-700 mt-1"
                  >
                    Cerrar
                  </button>
                </div>
              </div>
            )}

            {/* Step content */}
            {currentStep === 'intro' && renderIntro()}

            {currentStep === 'document-front' && (
              <DocumentCapture
                side="Front"
                documentType="Cedula"
                onCapture={handleDocumentCapture}
                onError={(err) => setError(err)}
                isProcessing={isLoading}
              />
            )}

            {currentStep === 'document-back' && (
              <DocumentCapture
                side="Back"
                documentType="Cedula"
                onCapture={handleDocumentCapture}
                onError={(err) => setError(err)}
                isProcessing={isLoading}
              />
            )}

            {currentStep === 'liveness' && session && (
              <LivenessChallenge
                requiredChallenges={session.requiredChallenges || ['Blink', 'Smile', 'TurnLeft']}
                onComplete={handleLivenessComplete}
                onError={(err) => setError(err)}
                isProcessing={isLoading}
              />
            )}

            {currentStep === 'processing' && renderProcessing()}

            {currentStep === 'success' && renderSuccess()}

            {currentStep === 'failed' && renderFailed()}
          </div>

          {/* Footer info */}
          <div className="mt-6 text-center text-sm text-gray-500">
            <p className="flex items-center justify-center gap-1">
              <Shield className="w-4 h-4" />
              Tus datos están protegidos y encriptados
            </p>
            <p className="mt-1">Cumplimos con la Ley 155-17 sobre Lavado de Activos</p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}

export default BiometricVerificationPage;
