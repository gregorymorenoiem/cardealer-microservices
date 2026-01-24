import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  FiAlertTriangle,
  FiLoader,
  FiMail,
  FiLock,
  FiCheck,
  FiX,
  FiAlertCircle,
  FiLogOut,
} from 'react-icons/fi';
import { authService } from '@/services/authService';

interface UnlinkActiveProviderModalProps {
  isOpen: boolean;
  onClose: () => void;
  provider: string;
  onSuccess?: () => void;
}

type Step =
  | 'validating'
  | 'needs_password'
  | 'confirm'
  | 'enter_code'
  | 'processing'
  | 'success'
  | 'error';

/**
 * UnlinkActiveProviderModal (AUTH-EXT-008)
 *
 * Multi-step modal for unlinking OAuth providers with proper security:
 * 1. Validates if user can unlink (checks password, active provider)
 * 2. If no password → prompts to set password via email
 * 3. If active provider → requires email verification code
 * 4. Executes unlink and revokes all sessions
 */
const UnlinkActiveProviderModal: React.FC<UnlinkActiveProviderModalProps> = ({
  isOpen,
  onClose,
  provider,
  onSuccess,
}) => {
  const navigate = useNavigate();

  // State
  const [step, setStep] = useState<Step>('validating');
  const [error, setError] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [maskedEmail, setMaskedEmail] = useState('');
  const [expiresIn, setExpiresIn] = useState(600); // 10 minutes in seconds
  const [sessionsRevoked, setSessionsRevoked] = useState(0);
  const [passwordSetupRequested, setPasswordSetupRequested] = useState(false);

  // Memoized validateUnlink function
  const validateUnlink = React.useCallback(async () => {
    setStep('validating');
    setError('');

    try {
      const result = await authService.validateUnlinkAccount(provider);

      if (!result.hasPassword) {
        // User needs to set password first
        setStep('needs_password');
      } else if (result.requiresEmailVerification) {
        // Active provider - needs verification
        setStep('confirm');
      } else if (result.canUnlink) {
        // Can use simple unlink (AUTH-EXT-006)
        // This shouldn't happen if using this modal for active provider only
        setStep('confirm');
      } else {
        setError(result.message);
        setStep('error');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to validate unlink request');
      setStep('error');
    }
  }, [provider]);

  // Validate on open
  useEffect(() => {
    if (isOpen && provider) {
      validateUnlink();
    } else {
      // Reset state when closed
      setStep('validating');
      setError('');
      setVerificationCode('');
      setMaskedEmail('');
      setExpiresIn(600);
      setPasswordSetupRequested(false);
    }
  }, [isOpen, provider, validateUnlink]);

  // Countdown timer for code expiration
  useEffect(() => {
    let interval: NodeJS.Timeout | undefined;
    if (step === 'enter_code' && expiresIn > 0) {
      interval = setInterval(() => {
        setExpiresIn((prev) => {
          if (prev <= 1) {
            if (interval) clearInterval(interval);
            setError('Verification code expired. Please request a new one.');
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    return () => {
      if (interval) clearInterval(interval);
    };
  }, [step, expiresIn]);

  const handleRequestPasswordSetup = async () => {
    try {
      setPasswordSetupRequested(true);
      await authService.requestPasswordSetup();
      // The email has been sent - user will get a link
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send password setup email');
      setPasswordSetupRequested(false);
    }
  };

  const handleRequestCode = async () => {
    setStep('processing');
    setError('');

    try {
      const result = await authService.requestUnlinkCode(provider);
      setMaskedEmail(result.maskedEmail);
      setExpiresIn(result.expiresInMinutes * 60);
      setStep('enter_code');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send verification code');
      setStep('confirm');
    }
  };

  const handleVerifyAndUnlink = async () => {
    if (verificationCode.length !== 6) {
      setError('Please enter the 6-digit code');
      return;
    }

    setStep('processing');
    setError('');

    try {
      const result = await authService.unlinkActiveProvider(provider, verificationCode);

      if (result.success) {
        setSessionsRevoked(result.sessionsRevoked);
        setStep('success');

        // Notify parent component of success
        onSuccess?.();

        // Clear local auth state and redirect after 3 seconds
        setTimeout(() => {
          authService.logout();
          navigate('/login', {
            state: {
              message: `${provider} account unlinked. Please log in with your email and password.`,
            },
          });
        }, 3000);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to unlink account');
      setStep('enter_code'); // Stay on enter_code step to allow retry
    }
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onClick={onClose} />

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6 transform transition-all">
          {/* Close button */}
          <button
            onClick={onClose}
            className="absolute top-4 right-4 text-gray-400 hover:text-gray-600"
          >
            <FiX size={20} />
          </button>

          {/* Validating step */}
          {step === 'validating' && (
            <div className="text-center py-8">
              <FiLoader className="animate-spin h-12 w-12 text-blue-600 mx-auto mb-4" />
              <p className="text-gray-600">Validating your request...</p>
            </div>
          )}

          {/* Needs Password step */}
          {step === 'needs_password' && (
            <div className="space-y-4">
              <div className="text-center">
                <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FiLock className="h-8 w-8 text-yellow-600" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-2">Password Required</h3>
                <p className="text-gray-600 text-sm">
                  You need to set a password before unlinking your {provider} account. This ensures
                  you won't lose access to your account.
                </p>
              </div>

              {!passwordSetupRequested ? (
                <>
                  <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <div className="flex items-start gap-3">
                      <FiMail className="h-5 w-5 text-blue-600 flex-shrink-0 mt-0.5" />
                      <div className="text-sm text-blue-800">
                        <p className="font-medium mb-1">We'll send you an email</p>
                        <p>
                          Click the link in the email to set your password. The link expires in 1
                          hour.
                        </p>
                      </div>
                    </div>
                  </div>

                  <div className="flex gap-3">
                    <button
                      onClick={onClose}
                      className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleRequestPasswordSetup}
                      className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center justify-center gap-2"
                    >
                      <FiMail size={16} />
                      Send Email
                    </button>
                  </div>
                </>
              ) : (
                <div className="bg-green-50 border border-green-200 rounded-lg p-4 text-center">
                  <FiCheck className="h-8 w-8 text-green-600 mx-auto mb-2" />
                  <p className="text-green-800 font-medium">Email sent!</p>
                  <p className="text-green-700 text-sm mt-1">
                    Check your inbox and click the link to set your password.
                  </p>
                  <button
                    onClick={onClose}
                    className="mt-4 px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                  >
                    Got it
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Confirm step */}
          {step === 'confirm' && (
            <div className="space-y-4">
              <div className="text-center">
                <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FiAlertTriangle className="h-8 w-8 text-red-600" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-2">Unlink {provider} Account?</h3>
              </div>

              <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <p className="text-sm font-medium text-yellow-800 mb-2">⚠️ This action will:</p>
                <ul className="text-sm text-yellow-700 space-y-1 list-disc list-inside">
                  <li>Disconnect your {provider} account</li>
                  <li>Log you out from ALL devices</li>
                  <li>Require you to log in with email + password</li>
                </ul>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <div className="flex items-start gap-3">
                  <FiMail className="h-5 w-5 text-blue-600 flex-shrink-0 mt-0.5" />
                  <div className="text-sm text-blue-800">
                    <p className="font-medium">Verification required</p>
                    <p>We'll send a 6-digit code to your email for security.</p>
                  </div>
                </div>
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-3 flex items-center gap-2">
                  <FiAlertCircle className="h-5 w-5 text-red-600 flex-shrink-0" />
                  <p className="text-sm text-red-700">{error}</p>
                </div>
              )}

              <div className="flex gap-3">
                <button
                  onClick={onClose}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                  Keep Connected
                </button>
                <button
                  onClick={handleRequestCode}
                  className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                >
                  Send Code
                </button>
              </div>
            </div>
          )}

          {/* Enter Code step */}
          {step === 'enter_code' && (
            <div className="space-y-4">
              <div className="text-center">
                <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FiMail className="h-8 w-8 text-blue-600" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 mb-2">Enter Verification Code</h3>
                <p className="text-gray-600 text-sm">
                  We sent a 6-digit code to <span className="font-medium">{maskedEmail}</span>
                </p>
              </div>

              {/* Code input */}
              <div>
                <input
                  type="text"
                  inputMode="numeric"
                  pattern="[0-9]*"
                  maxLength={6}
                  value={verificationCode}
                  onChange={(e) => {
                    const value = e.target.value.replace(/\D/g, '');
                    setVerificationCode(value);
                    setError('');
                  }}
                  placeholder="000000"
                  className="w-full text-center text-3xl tracking-[0.5em] font-mono px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>

              {/* Timer */}
              <div className="text-center">
                {expiresIn > 0 ? (
                  <p className="text-sm text-gray-500">
                    Code expires in{' '}
                    <span className="font-mono font-medium">{formatTime(expiresIn)}</span>
                  </p>
                ) : (
                  <button
                    onClick={handleRequestCode}
                    className="text-sm text-blue-600 hover:text-blue-800"
                  >
                    Resend code
                  </button>
                )}
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-3 flex items-center gap-2">
                  <FiAlertCircle className="h-5 w-5 text-red-600 flex-shrink-0" />
                  <p className="text-sm text-red-700">{error}</p>
                </div>
              )}

              <div className="flex gap-3">
                <button
                  onClick={() => setStep('confirm')}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                  Back
                </button>
                <button
                  onClick={handleVerifyAndUnlink}
                  disabled={verificationCode.length !== 6}
                  className={`flex-1 px-4 py-2 rounded-lg ${
                    verificationCode.length === 6
                      ? 'bg-red-600 text-white hover:bg-red-700'
                      : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  }`}
                >
                  Verify & Unlink
                </button>
              </div>
            </div>
          )}

          {/* Processing step */}
          {step === 'processing' && (
            <div className="text-center py-8">
              <FiLoader className="animate-spin h-12 w-12 text-blue-600 mx-auto mb-4" />
              <p className="text-gray-600">Processing your request...</p>
            </div>
          )}

          {/* Success step */}
          {step === 'success' && (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto">
                <FiCheck className="h-8 w-8 text-green-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-900">Account Unlinked</h3>
              <p className="text-gray-600 text-sm">
                Your {provider} account has been disconnected.
              </p>
              {sessionsRevoked > 0 && (
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
                  <div className="flex items-center justify-center gap-2 text-blue-800">
                    <FiLogOut size={16} />
                    <span className="text-sm font-medium">
                      {sessionsRevoked} session{sessionsRevoked > 1 ? 's' : ''} logged out
                    </span>
                  </div>
                </div>
              )}
              <p className="text-sm text-gray-500">Redirecting to login page...</p>
            </div>
          )}

          {/* Error step */}
          {step === 'error' && (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto">
                <FiAlertCircle className="h-8 w-8 text-red-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-900">Something Went Wrong</h3>
              <p className="text-gray-600 text-sm">{error}</p>
              <div className="flex gap-3">
                <button
                  onClick={onClose}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50"
                >
                  Close
                </button>
                <button
                  onClick={validateUnlink}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Try Again
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default UnlinkActiveProviderModal;
