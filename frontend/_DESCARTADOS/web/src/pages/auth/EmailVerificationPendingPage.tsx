import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FiMail, FiCheckCircle, FiRefreshCw, FiArrowLeft } from 'react-icons/fi';
import { authService } from '@/services/authService';
import Button from '@/components/atoms/Button';

export default function EmailVerificationPendingPage() {
  const { t } = useTranslation('auth');
  const location = useLocation();
  const [isResending, setIsResending] = useState(false);
  const [resendSuccess, setResendSuccess] = useState(false);
  const [resendError, setResendError] = useState<string | null>(null);
  const [countdown, setCountdown] = useState(0);

  // Get email from navigation state
  const email = location.state?.email || 'your email';

  // Countdown timer for resend button
  useEffect(() => {
    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [countdown]);

  const handleResendEmail = async () => {
    if (countdown > 0) return;

    try {
      setIsResending(true);
      setResendError(null);
      await authService.resendVerificationEmail();
      setResendSuccess(true);
      setCountdown(60); // 60 second cooldown

      // Reset success message after 5 seconds
      setTimeout(() => setResendSuccess(false), 5000);
    } catch (error) {
      console.error('Failed to resend verification email:', error);
      setResendError(
        t('verifyEmail.resendError', 'Failed to send verification email. Please try again.')
      );
    } finally {
      setIsResending(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 px-4">
      <div className="max-w-md w-full">
        {/* Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8 text-center">
          {/* Email Icon */}
          <div className="mx-auto w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mb-6">
            <FiMail className="w-10 h-10 text-blue-600" />
          </div>

          {/* Title */}
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            {t('verifyEmail.pendingTitle', 'Check Your Email')}
          </h1>

          {/* Description */}
          <p className="text-gray-600 mb-6">
            {t('verifyEmail.pendingDescription', "We've sent a verification link to")}
            <span className="block font-semibold text-gray-900 mt-1">{email}</span>
          </p>

          {/* Instructions */}
          <div className="bg-gray-50 rounded-lg p-4 mb-6 text-left">
            <p className="text-sm text-gray-600 mb-3">
              {t('verifyEmail.instructions', 'Please follow these steps:')}
            </p>
            <ol className="text-sm text-gray-600 space-y-2">
              <li className="flex items-start gap-2">
                <span className="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-xs font-bold">
                  1
                </span>
                <span>{t('verifyEmail.step1', 'Open your email inbox')}</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-xs font-bold">
                  2
                </span>
                <span>{t('verifyEmail.step2', 'Look for an email from OKLA')}</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="flex-shrink-0 w-5 h-5 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-xs font-bold">
                  3
                </span>
                <span>{t('verifyEmail.step3', 'Click the verification link')}</span>
              </li>
            </ol>
          </div>

          {/* Success Message */}
          {resendSuccess && (
            <div className="flex items-center gap-2 p-3 bg-green-50 text-green-700 rounded-lg mb-4">
              <FiCheckCircle className="w-5 h-5" />
              <span className="text-sm">
                {t('verifyEmail.resendSuccess', 'Verification email sent!')}
              </span>
            </div>
          )}

          {/* Error Message */}
          {resendError && (
            <div className="p-3 bg-red-50 text-red-700 rounded-lg mb-4 text-sm">{resendError}</div>
          )}

          {/* Resend Button */}
          <Button
            onClick={handleResendEmail}
            disabled={isResending || countdown > 0}
            variant="secondary"
            className="w-full mb-4"
          >
            {isResending ? (
              <>
                <FiRefreshCw className="w-4 h-4 animate-spin mr-2" />
                {t('verifyEmail.sending', 'Sending...')}
              </>
            ) : countdown > 0 ? (
              t('verifyEmail.resendCountdown', `Resend in ${countdown}s`)
            ) : (
              <>
                <FiRefreshCw className="w-4 h-4 mr-2" />
                {t('verifyEmail.resendButton', 'Resend Verification Email')}
              </>
            )}
          </Button>

          {/* Spam Notice */}
          <p className="text-xs text-gray-500 mb-6">
            {t('verifyEmail.spamNotice', "Didn't receive the email? Check your spam folder.")}
          </p>

          {/* Divider */}
          <div className="border-t border-gray-200 pt-6">
            <Link
              to="/login"
              className="inline-flex items-center gap-2 text-blue-600 hover:text-blue-700 font-medium"
            >
              <FiArrowLeft className="w-4 h-4" />
              {t('verifyEmail.backToLogin', 'Back to Login')}
            </Link>
          </div>
        </div>

        {/* Footer Note */}
        <p className="text-center text-sm text-gray-500 mt-6">
          {t('verifyEmail.wrongEmail', 'Wrong email?')}{' '}
          <Link to="/register" className="text-blue-600 hover:underline">
            {t('verifyEmail.registerAgain', 'Register with a different email')}
          </Link>
        </p>
      </div>
    </div>
  );
}
