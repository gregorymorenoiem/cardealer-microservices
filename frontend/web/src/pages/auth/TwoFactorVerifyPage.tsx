import { useState, useRef, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/store/authStore';
import Button from '@/components/atoms/Button';
import {
  FiShield,
  FiAlertCircle,
  FiKey,
  FiSmartphone,
  FiRefreshCw,
  FiUnlock,
  FiCopy,
  FiCheck,
  FiDownload,
} from 'react-icons/fi';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const TWOFACTOR_API_URL = `${API_BASE_URL}/api/twofactor`;

// 2FA types matching backend enum
enum TwoFactorType {
  Authenticator = 1,
  SMS = 2,
  Email = 3,
}

interface TwoFactorState {
  sessionToken: string;
  email: string;
  twoFactorType?: number; // 1=Authenticator, 2=SMS, 3=Email
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

  // Full recovery mode (10 codes) - SEC-2FA-006 Extended
  const [useFullRecovery, setUseFullRecovery] = useState(false);
  const [allRecoveryCodes, setAllRecoveryCodes] = useState<string[]>(Array(10).fill(''));
  const [fullRecoverySuccess, setFullRecoverySuccess] = useState(false);
  const [newQrCodeUrl, setNewQrCodeUrl] = useState<string | null>(null);
  const [newAuthSecret, setNewAuthSecret] = useState<string | null>(null);
  const [newRecoveryCodes, setNewRecoveryCodes] = useState<string[]>([]);
  const [copiedCodes, setCopiedCodes] = useState(false);

  // SMS 2FA state
  const [twoFactorType, setTwoFactorType] = useState<number>(TwoFactorType.Authenticator);
  const [smsSent, setSmsSent] = useState(false);
  const [isSendingSms, setIsSendingSms] = useState(false);
  const [maskedPhone, setMaskedPhone] = useState<string | null>(null);
  const [canResendSms, setCanResendSms] = useState(true);
  const [resendCountdown, setResendCountdown] = useState(0);

  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Redirect if no session token and detect 2FA type
  useEffect(() => {
    // Try to get from sessionStorage (set by LoginPage)
    const storedToken = sessionStorage.getItem('twoFactorSessionToken');
    const storedType = sessionStorage.getItem('twoFactorType');

    if (!state?.sessionToken && !storedToken) {
      navigate('/login', {
        state: { error: 'Session expired. Please login again.' },
      });
      return;
    }

    // Detect 2FA type from state or sessionStorage
    if (state?.twoFactorType) {
      setTwoFactorType(state.twoFactorType);
    } else if (storedType) {
      // Map string to enum
      const typeMap: Record<string, number> = {
        totp: TwoFactorType.Authenticator,
        authenticator: TwoFactorType.Authenticator,
        sms: TwoFactorType.SMS,
        email: TwoFactorType.Email,
      };
      setTwoFactorType(typeMap[storedType.toLowerCase()] || TwoFactorType.Authenticator);
    }
  }, [state, navigate]);

  // Auto-send SMS code if SMS 2FA
  useEffect(() => {
    if (twoFactorType === TwoFactorType.SMS && !smsSent && !isSendingSms) {
      handleSendSmsCode();
    }
  }, [twoFactorType]);

  // Countdown timer for SMS resend
  useEffect(() => {
    if (resendCountdown > 0) {
      const timer = setTimeout(() => setResendCountdown(resendCountdown - 1), 1000);
      return () => clearTimeout(timer);
    } else if (resendCountdown === 0 && !canResendSms) {
      setCanResendSms(true);
    }
  }, [resendCountdown, canResendSms]);

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

  // Send SMS code
  const handleSendSmsCode = async () => {
    const token = state?.sessionToken || sessionStorage.getItem('twoFactorSessionToken');
    if (!token) return;

    setIsSendingSms(true);
    setApiError(null);

    try {
      const response = await axios.post(`${TWOFACTOR_API_URL}/send-sms-code`, {
        tempToken: token,
      });

      const { data } = response.data;
      setSmsSent(true);
      setMaskedPhone(data?.maskedPhoneNumber || '***-***-**XX');
      setCanResendSms(false);
      setResendCountdown(60); // 60 seconds before can resend
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        setApiError(error.response.data?.error || 'Failed to send SMS code');
      } else {
        setApiError('Failed to send SMS code. Please try again.');
      }
    } finally {
      setIsSendingSms(false);
    }
  };

  // Get the correct submit endpoint based on 2FA type
  const getSubmitEndpoint = () => {
    if (twoFactorType === TwoFactorType.SMS) {
      return `${TWOFACTOR_API_URL}/verify-sms-code`;
    }
    return `${TWOFACTOR_API_URL}/login`;
  };

  const handleSubmitCode = async (codeString: string) => {
    const token = state?.sessionToken || sessionStorage.getItem('twoFactorSessionToken');
    if (!token) return;

    setIsSubmitting(true);
    setApiError(null);

    try {
      // Use correct endpoint and payload based on 2FA type
      const endpoint = getSubmitEndpoint();
      const payload =
        twoFactorType === TwoFactorType.SMS
          ? { tempToken: token, code: codeString }
          : { tempToken: token, twoFactorCode: codeString };

      const response = await axios.post(endpoint, payload);

      const { data } = response.data;

      if (data?.accessToken) {
        // Store tokens
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userId', data.userId);

        // Get email from state or data
        const userEmail = state?.email || data.email || 'user@example.com';

        // Update store
        storeLogin({
          user: {
            id: data.userId,
            email: userEmail,
            name: userEmail.split('@')[0],
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
    const token = state?.sessionToken || sessionStorage.getItem('twoFactorSessionToken');
    if (!token || !recoveryCode.trim()) return;

    setIsSubmitting(true);
    setApiError(null);

    try {
      // Use the correct endpoint: login-with-recovery (not verify-recovery-code)
      const response = await axios.post(`${TWOFACTOR_API_URL}/login-with-recovery`, {
        tempToken: token,
        recoveryCode: recoveryCode.trim().toUpperCase(),
      });

      const { data } = response.data;

      if (data?.accessToken) {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userId', data.userId);

        // Get email from state or data
        const userEmail = state?.email || data.email || 'user@example.com';

        storeLogin({
          user: {
            id: data.userId,
            email: userEmail,
            name: userEmail.split('@')[0],
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

  // Handle full account recovery with all 10 codes (SEC-2FA-006 Extended)
  const handleFullRecoverySubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const token = state?.sessionToken || sessionStorage.getItem('twoFactorSessionToken');

    // Validate all 10 codes are provided
    const codes = allRecoveryCodes.map((c) => c.trim().toUpperCase()).filter((c) => c.length >= 6);
    if (codes.length !== 10) {
      setApiError('Please enter all 10 recovery codes');
      return;
    }

    if (!token) {
      setApiError('Session expired. Please login again.');
      return;
    }

    setIsSubmitting(true);
    setApiError(null);

    try {
      const response = await axios.post(`${TWOFACTOR_API_URL}/recover-with-all-codes`, {
        tempToken: token,
        recoveryCodes: codes,
      });

      const { data } = response.data;

      if (data?.accessToken) {
        // Store tokens
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('userId', data.userId);

        // Get email from state or data
        const userEmail = state?.email || data.email || 'user@example.com';

        // Generate QR code URL from the new secret
        const otpauthUrl = `otpauth://totp/OKLA:${encodeURIComponent(userEmail)}?secret=${data.newAuthenticatorSecret}&issuer=OKLA`;
        const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(otpauthUrl)}`;

        setNewQrCodeUrl(qrUrl);
        setNewAuthSecret(data.newAuthenticatorSecret);
        setNewRecoveryCodes(data.newRecoveryCodes || []);
        setFullRecoverySuccess(true);

        // Store login in state but don't navigate yet
        storeLogin({
          user: {
            id: data.userId,
            email: userEmail,
            name: userEmail.split('@')[0],
            accountType: 'individual',
            emailVerified: true,
            createdAt: new Date().toISOString(),
          },
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
        });
      }
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        const message =
          error.response.data?.error ||
          error.response.data?.message ||
          'Recovery failed. Please verify all 10 codes are correct.';
        setApiError(message);
      } else {
        setApiError('Failed to recover account. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle updating a single recovery code input
  const handleRecoveryCodeChange = (index: number, value: string) => {
    const newCodes = [...allRecoveryCodes];
    newCodes[index] = value
      .toUpperCase()
      .replace(/[^A-Z0-9]/g, '')
      .slice(0, 8);
    setAllRecoveryCodes(newCodes);
  };

  // Copy new recovery codes to clipboard
  const handleCopyNewCodes = async () => {
    const codesText = newRecoveryCodes.join('\n');
    try {
      await navigator.clipboard.writeText(codesText);
      setCopiedCodes(true);
      setTimeout(() => setCopiedCodes(false), 3000);
    } catch {
      // Fallback for older browsers
      const textArea = document.createElement('textarea');
      textArea.value = codesText;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand('copy');
      document.body.removeChild(textArea);
      setCopiedCodes(true);
      setTimeout(() => setCopiedCodes(false), 3000);
    }
  };

  // Download new recovery codes as text file
  const handleDownloadCodes = () => {
    const codesText = `OKLA - Recovery Codes\n${'='.repeat(30)}\nGenerated: ${new Date().toLocaleString()}\n\nSave these codes in a safe place.\nEach code can only be used once.\n\n${newRecoveryCodes.map((c, i) => `${i + 1}. ${c}`).join('\n')}\n`;
    const blob = new Blob([codesText], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'okla-recovery-codes.txt';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  // Continue to dashboard after saving new codes
  const handleContinueToDashboard = () => {
    navigate('/dashboard', {
      replace: true,
      state: {
        success:
          'Account recovered successfully! Your 2FA has been reset with a new authenticator.',
      },
    });
  };

  if (!state?.sessionToken) {
    return null;
  }

  // Get title and subtitle based on 2FA type
  const get2FATitle = () => {
    if (fullRecoverySuccess) return t('twoFactor.recoverySuccessTitle', 'Account Recovered!');
    if (useFullRecovery) return t('twoFactor.fullRecoveryTitle', 'Full Account Recovery');
    if (useRecoveryCode) return t('twoFactor.recoveryTitle', 'Use Recovery Code');
    if (twoFactorType === TwoFactorType.SMS) return t('twoFactor.smsTitle', 'SMS Verification');
    return t('twoFactor.title', 'Two-Factor Authentication');
  };

  const get2FASubtitle = () => {
    if (fullRecoverySuccess)
      return t(
        'twoFactor.recoverySuccessSubtitle',
        'Your 2FA has been reset. Set up your new authenticator below.'
      );
    if (useFullRecovery)
      return t(
        'twoFactor.fullRecoverySubtitle',
        'Enter ALL 10 original recovery codes to prove account ownership'
      );
    if (useRecoveryCode) return t('twoFactor.recoverySubtitle', 'Enter one of your recovery codes');
    if (twoFactorType === TwoFactorType.SMS) {
      return smsSent
        ? t('twoFactor.smsSubtitle', `Enter the 6-digit code sent to ${maskedPhone}`)
        : t('twoFactor.smsSending', 'Sending verification code to your phone...');
    }
    return t('twoFactor.subtitle', 'Enter the 6-digit code from your authenticator app');
  };

  const get2FAIcon = () => {
    if (fullRecoverySuccess) return <FiCheck className="text-green-600" size={32} />;
    if (useFullRecovery) return <FiUnlock className="text-amber-600" size={32} />;
    if (useRecoveryCode) return <FiKey className="text-purple-600" size={32} />;
    if (twoFactorType === TwoFactorType.SMS)
      return <FiSmartphone className="text-green-600" size={32} />;
    return <FiShield className="text-blue-600" size={32} />;
  };

  const getIconBgColor = () => {
    if (fullRecoverySuccess) return 'bg-green-100';
    if (useFullRecovery) return 'bg-amber-100';
    if (useRecoveryCode) return 'bg-purple-100';
    if (twoFactorType === TwoFactorType.SMS) return 'bg-green-100';
    return 'bg-blue-100';
  };

  return (
    <div className="w-full max-w-md mx-auto">
      {/* Header */}
      <div className="text-center mb-8">
        <div
          className={`mx-auto w-16 h-16 rounded-full flex items-center justify-center mb-6 ${getIconBgColor()}`}
        >
          {get2FAIcon()}
        </div>
        <h1 className="text-2xl font-bold text-gray-900 mb-2">{get2FATitle()}</h1>
        <p className="text-gray-600">{get2FASubtitle()}</p>
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

      {/* Full Recovery Success - Show new QR and codes */}
      {fullRecoverySuccess && newQrCodeUrl && (
        <div className="space-y-6">
          {/* Success Message */}
          <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
            <div className="flex items-start gap-3">
              <FiCheck className="text-green-600 flex-shrink-0 mt-0.5" size={20} />
              <div>
                <h3 className="font-semibold text-green-800">Account Successfully Recovered</h3>
                <p className="text-sm text-green-700 mt-1">
                  Your 2FA has been reset. Please set up your authenticator app with the new QR code
                  below.
                </p>
              </div>
            </div>
          </div>

          {/* New QR Code */}
          <div className="text-center">
            <p className="text-sm text-gray-600 mb-4">
              Scan this QR code with your authenticator app
            </p>
            <div className="inline-block p-4 bg-white border-2 rounded-xl shadow-sm">
              <img src={newQrCodeUrl} alt="New 2FA QR Code" className="w-48 h-48 mx-auto" />
            </div>

            {newAuthSecret && (
              <div className="mt-4 p-3 bg-gray-100 rounded-lg max-w-sm mx-auto">
                <p className="text-xs text-gray-500 mb-2">Manual entry code:</p>
                <code className="text-sm font-mono font-bold text-gray-800 break-all">
                  {newAuthSecret}
                </code>
              </div>
            )}
          </div>

          {/* New Recovery Codes */}
          {newRecoveryCodes.length > 0 && (
            <div className="space-y-4">
              <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                <div className="flex items-start gap-3">
                  <FiKey className="text-yellow-600 flex-shrink-0 mt-0.5" size={20} />
                  <div>
                    <h3 className="font-semibold text-yellow-800">New Recovery Codes</h3>
                    <p className="text-sm text-yellow-700 mt-1">
                      Save these codes in a safe place. They can be used if you lose access to your
                      authenticator.
                    </p>
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-2 p-4 bg-gray-100 rounded-lg font-mono text-sm">
                {newRecoveryCodes.map((code, index) => (
                  <div
                    key={index}
                    className="p-2 bg-white rounded text-center font-bold text-gray-800"
                  >
                    {code}
                  </div>
                ))}
              </div>

              <div className="flex gap-2">
                <Button variant="outline" size="sm" onClick={handleCopyNewCodes} className="flex-1">
                  {copiedCodes ? (
                    <>
                      <FiCheck className="mr-1" size={14} />
                      Copied!
                    </>
                  ) : (
                    <>
                      <FiCopy className="mr-1" size={14} />
                      Copy Codes
                    </>
                  )}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleDownloadCodes}
                  className="flex-1"
                >
                  <FiDownload className="mr-1" size={14} />
                  Download
                </Button>
              </div>
            </div>
          )}

          {/* Continue Button */}
          <Button variant="primary" size="lg" fullWidth onClick={handleContinueToDashboard}>
            I've Saved My Codes - Continue
          </Button>
        </div>
      )}

      {/* Full Recovery Mode - Enter 10 codes */}
      {useFullRecovery && !fullRecoverySuccess && (
        <form onSubmit={handleFullRecoverySubmit} className="space-y-4">
          <div className="p-4 bg-amber-50 border border-amber-200 rounded-lg mb-4">
            <div className="flex items-start gap-3">
              <FiAlertCircle className="text-amber-600 flex-shrink-0 mt-0.5" size={20} />
              <div>
                <h3 className="font-semibold text-amber-800">Emergency Recovery</h3>
                <p className="text-sm text-amber-700 mt-1">
                  Enter ALL 10 recovery codes you received when you enabled 2FA. Order doesn't
                  matter, but all codes must be correct.
                </p>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-2">
            {allRecoveryCodes.map((code, index) => (
              <input
                key={index}
                type="text"
                value={code}
                onChange={(e) => handleRecoveryCodeChange(index, e.target.value)}
                placeholder={`Code ${index + 1}`}
                maxLength={8}
                className="px-3 py-2 text-center text-sm font-mono tracking-wider border-2 border-gray-300 rounded-lg 
                         focus:border-amber-500 focus:ring-2 focus:ring-amber-500/20 outline-none uppercase"
                disabled={isSubmitting}
              />
            ))}
          </div>

          <p className="text-xs text-gray-500 text-center">
            {allRecoveryCodes.filter((c) => c.length >= 6).length} of 10 codes entered
          </p>

          <Button
            type="submit"
            variant="primary"
            size="lg"
            fullWidth
            isLoading={isSubmitting}
            disabled={allRecoveryCodes.filter((c) => c.length >= 6).length !== 10}
          >
            Recover Account
          </Button>

          <div className="text-center">
            <button
              type="button"
              onClick={() => {
                setUseFullRecovery(false);
                setAllRecoveryCodes(Array(10).fill(''));
                setApiError(null);
              }}
              className="text-sm text-gray-600 hover:text-primary"
            >
              ← Back to recovery code
            </button>
          </div>
        </form>
      )}

      {/* Normal 2FA and single recovery code modes */}
      {!useFullRecovery && !fullRecoverySuccess && !useRecoveryCode && (
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
            disabled={
              code.some((d) => d === '') || (twoFactorType === TwoFactorType.SMS && !smsSent)
            }
          >
            {t('twoFactor.verify', 'Verify')}
          </Button>

          {/* SMS Resend Option */}
          {twoFactorType === TwoFactorType.SMS && smsSent && (
            <div className="mt-4 text-center">
              {canResendSms ? (
                <button
                  type="button"
                  onClick={handleSendSmsCode}
                  disabled={isSendingSms}
                  className="text-sm text-primary hover:text-primary-600 flex items-center justify-center gap-2 mx-auto"
                >
                  <FiRefreshCw size={16} className={isSendingSms ? 'animate-spin' : ''} />
                  {isSendingSms ? 'Sending...' : t('twoFactor.resendSms', 'Resend SMS code')}
                </button>
              ) : (
                <p className="text-sm text-gray-500">
                  {t('twoFactor.resendIn', `Resend code in ${resendCountdown}s`)}
                </p>
              )}
            </div>
          )}

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
      )}

      {/* Single Recovery Code Mode */}
      {!useFullRecovery && !fullRecoverySuccess && useRecoveryCode && (
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

          {/* Full Recovery Option */}
          <div className="mt-4 p-3 bg-gray-50 border border-gray-200 rounded-lg">
            <p className="text-xs text-gray-600 text-center mb-2">Lost your recovery codes too?</p>
            <button
              type="button"
              onClick={() => {
                setUseRecoveryCode(false);
                setUseFullRecovery(true);
                setRecoveryCode('');
                setApiError(null);
              }}
              className="text-sm text-amber-600 hover:text-amber-700 font-medium flex items-center justify-center gap-2 mx-auto"
            >
              <FiUnlock size={14} />
              Recover with all 10 original codes
            </button>
          </div>

          {/* Back to TOTP */}
          <div className="mt-4 text-center">
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
      {!fullRecoverySuccess && (
        <p className="mt-8 text-center text-sm text-gray-600">
          <Link
            to="/login"
            className="font-medium text-primary hover:text-primary-600 transition-colors"
          >
            {t('twoFactor.backToLogin', '← Back to Login')}
          </Link>
        </p>
      )}
    </div>
  );
}
