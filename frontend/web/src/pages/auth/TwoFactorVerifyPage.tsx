import { useState, useRef, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/store/authStore';
import Button from '@/components/atoms/Button';
import { FiShield, FiAlertCircle, FiKey } from 'react-icons/fi';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const AUTH_API_URL = `${API_BASE_URL}/api/auth`;

interface TwoFactorState {
  sessionToken: string;
  email: string;
}

export default function TwoFactorVerifyPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const location = useLocation();
  const storeLogin = useAuthStore((state) => state.login);

  const state = location.state as TwoFactorState | null;

  const [code, setCode] = useState(['', '', '', '', '', '']);
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [useRecoveryCode, setUseRecoveryCode] = useState(false);
  const [recoveryCode, setRecoveryCode] = useState('');

  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Redirect if no session token
  useEffect(() => {
    if (!state?.sessionToken) {
      navigate('/login', {
        state: { error: 'Session expired. Please login again.' },
      });
    }
  }, [state, navigate]);

  // Handle digit input
  const handleDigitChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;

    const newCode = [...code];
    newCode[index] = value.slice(-1);
    setCode(newCode);

    // Auto-focus next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }

    // Auto-submit when all digits entered
    if (newCode.every((d) => d !== '') && newCode.length === 6) {
      handleSubmitCode(newCode.join(''));
    }
  };

  // Handle backspace
  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !code[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  // Handle paste
  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6);

    if (pastedData.length === 6) {
      const newCode = pastedData.split('');
      setCode(newCode);
      handleSubmitCode(pastedData);
    }
  };

  const handleSubmitCode = async (codeString: string) => {
    if (!state?.sessionToken) return;

    setIsSubmitting(true);
    setApiError(null);

    try {
      const response = await axios.post(`${AUTH_API_URL}/twofactor/login`, {
        tempToken: state.sessionToken,
        twoFactorCode: codeString,
      });

      const { data } = response.data;

      if (data?.accessToken) {
        // Store tokens
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userId', data.userId);

        // Update store
        storeLogin({
          user: {
            id: data.userId,
            email: state.email,
            name: state.email.split('@')[0],
            accountType: 'individual',
            emailVerified: true,
            createdAt: new Date().toISOString(),
          },
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
        });

        navigate('/dashboard', { replace: true });
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Invalid verification code';
        setApiError(message);
      } else {
        setApiError('Failed to verify code. Please try again.');
      }
      // Clear code on error
      setCode(['', '', '', '', '', '']);
      inputRefs.current[0]?.focus();
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRecoveryCodeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!state?.sessionToken || !recoveryCode.trim()) return;

    setIsSubmitting(true);
    setApiError(null);

    try {
      const response = await axios.post(`${AUTH_API_URL}/twofactor/verify-recovery-code`, {
        tempToken: state.sessionToken,
        recoveryCode: recoveryCode.trim().toUpperCase(),
      });

      const { data } = response.data;

      if (data?.accessToken) {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userId', data.userId);

        storeLogin({
          user: {
            id: data.userId,
            email: state.email,
            name: state.email.split('@')[0],
            accountType: 'individual',
            emailVerified: true,
            createdAt: new Date().toISOString(),
          },
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
        });

        // Show warning about remaining codes
        if (data.remainingCodes !== undefined && data.remainingCodes <= 3) {
          navigate('/dashboard', {
            replace: true,
            state: {
              warning: `You have only ${data.remainingCodes} recovery codes left. Please generate new ones in Security Settings.`,
            },
          });
        } else {
          navigate('/dashboard', { replace: true });
        }
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message = error.response.data?.error || 'Invalid recovery code';
        setApiError(message);
      } else {
        setApiError('Failed to verify recovery code. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!state?.sessionToken) {
    return null;
  }

  return (
    <div className="w-full max-w-md mx-auto">
      {/* Header */}
      <div className="text-center mb-8">
        <div className="mx-auto w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mb-6">
          <FiShield className="text-blue-600" size={32} />
        </div>
        <h1 className="text-2xl font-bold text-gray-900 mb-2">
          {useRecoveryCode
            ? t('twoFactor.recoveryTitle', 'Use Recovery Code')
            : t('twoFactor.title', 'Two-Factor Authentication')}
        </h1>
        <p className="text-gray-600">
          {useRecoveryCode
            ? t('twoFactor.recoverySubtitle', 'Enter one of your recovery codes')
            : t('twoFactor.subtitle', 'Enter the 6-digit code from your authenticator app')}
        </p>
      </div>

      {/* API Error Alert */}
      {apiError && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
          <FiAlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={20} />
          <div className="flex-1">
            <p className="text-sm text-red-800">{apiError}</p>
          </div>
        </div>
      )}

      {!useRecoveryCode ? (
        <>
          {/* 6-Digit Code Input */}
          <div className="flex justify-center gap-2 mb-6" onPaste={handlePaste}>
            {code.map((digit, index) => (
              <input
                key={index}
                ref={(el) => {
                  if (inputRefs.current) {
                    inputRefs.current[index] = el;
                  }
                }}
                type="text"
                inputMode="numeric"
                maxLength={1}
                value={digit}
                onChange={(e) => handleDigitChange(index, e.target.value)}
                onKeyDown={(e) => handleKeyDown(index, e)}
                className="w-12 h-14 text-center text-2xl font-bold border-2 border-gray-300 rounded-lg 
                         focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none
                         disabled:bg-gray-100 disabled:cursor-not-allowed"
                disabled={isSubmitting}
                autoFocus={index === 0}
              />
            ))}
          </div>

          {/* Submit Button */}
          <Button
            variant="primary"
            size="lg"
            fullWidth
            onClick={() => handleSubmitCode(code.join(''))}
            isLoading={isSubmitting}
            disabled={code.some((d) => d === '')}
          >
            {t('twoFactor.verify', 'Verify')}
          </Button>

          {/* Recovery Code Option */}
          <div className="mt-6 text-center">
            <button
              type="button"
              onClick={() => setUseRecoveryCode(true)}
              className="text-sm text-gray-600 hover:text-primary flex items-center justify-center gap-2 mx-auto"
            >
              <FiKey size={16} />
              {t(
                'twoFactor.useRecoveryCode',
                "Can't access your authenticator? Use a recovery code"
              )}
            </button>
          </div>
        </>
      ) : (
        <>
          {/* Recovery Code Form */}
          <form onSubmit={handleRecoveryCodeSubmit} className="space-y-4">
            <input
              type="text"
              value={recoveryCode}
              onChange={(e) => setRecoveryCode(e.target.value.toUpperCase())}
              placeholder="XXXXXXXX"
              maxLength={10}
              className="w-full px-4 py-3 text-center text-xl font-mono tracking-widest border-2 border-gray-300 rounded-lg 
                       focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none uppercase"
              disabled={isSubmitting}
              autoFocus
            />

            <Button
              type="submit"
              variant="primary"
              size="lg"
              fullWidth
              isLoading={isSubmitting}
              disabled={recoveryCode.trim().length < 6}
            >
              {t('twoFactor.verifyRecovery', 'Verify Recovery Code')}
            </Button>
          </form>

          {/* Back to TOTP */}
          <div className="mt-6 text-center">
            <button
              type="button"
              onClick={() => {
                setUseRecoveryCode(false);
                setRecoveryCode('');
                setApiError(null);
              }}
              className="text-sm text-gray-600 hover:text-primary"
            >
              {t('twoFactor.backToCode', 'Back to authenticator code')}
            </button>
          </div>
        </>
      )}

      {/* Back to Login */}
      <p className="mt-8 text-center text-sm text-gray-600">
        <Link
          to="/login"
          className="font-medium text-primary hover:text-primary-600 transition-colors"
        >
          {t('twoFactor.backToLogin', '← Back to Login')}
        </Link>
      </p>
    </div>
  );
}
