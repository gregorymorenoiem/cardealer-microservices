/**
 * Dealer Email Verification Page
 *
 * Step 2 of dealer onboarding - verify email with 6-digit code
 */

import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Mail, ArrowRight, RefreshCw, CheckCircle } from 'lucide-react';
import {
  useVerifyDealerEmail,
  useResendVerificationEmail,
  useOnboardingProgress,
} from '@/hooks/useDealerOnboarding';
import toast from 'react-hot-toast';

export const DealerEmailVerificationPage: React.FC = () => {
  const navigate = useNavigate();
  const { email, dealerId, status, hasOnboardingInProgress } = useOnboardingProgress();
  const verifyMutation = useVerifyDealerEmail();
  const resendMutation = useResendVerificationEmail();

  const [code, setCode] = useState(['', '', '', '', '', '']);
  const [isVerified, setIsVerified] = useState(false);
  const [resendCooldown, setResendCooldown] = useState(0);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Redirect if no onboarding in progress
  useEffect(() => {
    if (!hasOnboardingInProgress) {
      navigate('/dealer/onboarding');
    }
  }, [hasOnboardingInProgress, navigate]);

  // Redirect if already verified
  useEffect(() => {
    if (status?.isEmailVerified) {
      setIsVerified(true);
      setTimeout(() => {
        navigate('/dealer/onboarding/documents');
      }, 2000);
    }
  }, [status, navigate]);

  // Resend cooldown timer
  useEffect(() => {
    if (resendCooldown > 0) {
      const timer = setTimeout(() => setResendCooldown(resendCooldown - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [resendCooldown]);

  const handleInputChange = (index: number, value: string) => {
    // Only allow numbers
    if (value && !/^\d+$/.test(value)) return;

    const newCode = [...code];

    // Handle paste
    if (value.length > 1) {
      const pastedCode = value.slice(0, 6).split('');
      pastedCode.forEach((digit, i) => {
        if (index + i < 6) {
          newCode[index + i] = digit;
        }
      });
      setCode(newCode);

      // Focus on the next empty input or the last one
      const nextIndex = Math.min(index + pastedCode.length, 5);
      inputRefs.current[nextIndex]?.focus();

      // Auto-submit if all filled
      if (newCode.every((d) => d !== '')) {
        handleVerify(newCode.join(''));
      }
      return;
    }

    newCode[index] = value;
    setCode(newCode);

    // Move to next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }

    // Auto-submit if all filled
    if (newCode.every((d) => d !== '')) {
      handleVerify(newCode.join(''));
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Backspace' && !code[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleVerify = async (verificationCode: string) => {
    if (!email) return;

    try {
      await verifyMutation.mutateAsync({
        email,
        verificationCode,
      });
      setIsVerified(true);
      setTimeout(() => {
        navigate('/dealer/onboarding/documents');
      }, 2000);
    } catch (error) {
      // Clear code on error
      setCode(['', '', '', '', '', '']);
      inputRefs.current[0]?.focus();
    }
  };

  const handleResend = async () => {
    if (!email || resendCooldown > 0) return;

    try {
      await resendMutation.mutateAsync(email);
      setResendCooldown(60); // 60 second cooldown
    } catch (error) {
      // Error handled by mutation
    }
  };

  if (isVerified) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 flex items-center justify-center">
        <div className="text-center">
          <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-8">
            <CheckCircle className="h-12 w-12 text-green-600" />
          </div>
          <h2 className="text-3xl font-bold text-gray-900 mb-4">¡Email Verificado!</h2>
          <p className="text-xl text-gray-600 mb-4">Redirigiendo a la subida de documentos...</p>
          <div className="animate-spin rounded-full h-8 w-8 border-4 border-blue-600 border-t-transparent mx-auto" />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Progress indicator */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-md mx-auto px-4 py-4">
          <div className="flex items-center justify-between text-sm text-gray-500">
            <span className="text-blue-600 font-medium">Paso 2 de 5</span>
            <span>Verificación de Email</span>
          </div>
          <div className="mt-2 h-2 bg-gray-200 rounded-full">
            <div className="h-2 bg-blue-600 rounded-full w-2/5" />
          </div>
        </div>
      </div>

      <div className="max-w-md mx-auto px-4 py-12">
        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <Mail className="h-10 w-10 text-blue-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Verifica tu Email</h1>
          <p className="text-gray-600">Ingresa el código de 6 dígitos que enviamos a</p>
          <p className="font-semibold text-gray-900 mt-1">{email}</p>
        </div>

        {/* Code Input */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8 mb-6">
          <div className="flex justify-center gap-3 mb-6">
            {code.map((digit, index) => (
              <input
                key={index}
                ref={(el) => (inputRefs.current[index] = el)}
                type="text"
                inputMode="numeric"
                maxLength={6}
                value={digit}
                onChange={(e) => handleInputChange(index, e.target.value)}
                onKeyDown={(e) => handleKeyDown(index, e)}
                className={`w-12 h-14 text-center text-2xl font-bold border-2 rounded-lg
                  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500
                  ${verifyMutation.isError ? 'border-red-500' : 'border-gray-300'}
                  ${digit ? 'bg-blue-50' : 'bg-white'}
                `}
                disabled={verifyMutation.isPending}
              />
            ))}
          </div>

          {verifyMutation.isError && (
            <p className="text-center text-red-500 text-sm mb-4">
              Código inválido. Por favor intenta de nuevo.
            </p>
          )}

          <button
            onClick={() => handleVerify(code.join(''))}
            disabled={code.some((d) => !d) || verifyMutation.isPending}
            className="w-full py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 
              text-white font-medium rounded-lg transition-colors 
              inline-flex items-center justify-center gap-2"
          >
            {verifyMutation.isPending ? (
              <>
                <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                Verificando...
              </>
            ) : (
              <>
                Verificar
                <ArrowRight className="h-5 w-5" />
              </>
            )}
          </button>
        </div>

        {/* Resend */}
        <div className="text-center">
          <p className="text-gray-500 text-sm mb-2">¿No recibiste el código?</p>
          <button
            onClick={handleResend}
            disabled={resendCooldown > 0 || resendMutation.isPending}
            className="text-blue-600 hover:text-blue-700 disabled:text-gray-400 
              font-medium inline-flex items-center gap-2"
          >
            {resendMutation.isPending ? (
              <>
                <RefreshCw className="h-4 w-4 animate-spin" />
                Reenviando...
              </>
            ) : resendCooldown > 0 ? (
              <>Reenviar en {resendCooldown}s</>
            ) : (
              <>
                <RefreshCw className="h-4 w-4" />
                Reenviar código
              </>
            )}
          </button>
        </div>

        {/* Help text */}
        <div className="mt-8 p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-500">
            <strong>¿Problemas?</strong> Revisa tu carpeta de spam o correo no deseado. Si no lo
            encuentras, puedes solicitar un nuevo código.
          </p>
        </div>
      </div>
    </div>
  );
};

export default DealerEmailVerificationPage;
