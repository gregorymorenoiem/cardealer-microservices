import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { authService } from '@/services/authService';
import Button from '@/components/atoms/Button';
import { FiAlertCircle, FiCheckCircle, FiMail, FiLoader } from 'react-icons/fi';

type VerificationStatus = 'verifying' | 'success' | 'error' | 'expired' | 'already_verified';

export default function VerifyEmailPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [status, setStatus] = useState<VerificationStatus>('verifying');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isResending, setIsResending] = useState(false);
  const [resendSuccess, setResendSuccess] = useState(false);

  useEffect(() => {
    const verifyEmail = async () => {
      if (!token) {
        setStatus('error');
        setErrorMessage('Verification token is missing. Please check your email link.');
        return;
      }

      try {
        await authService.verifyEmail(token);
        setStatus('success');
      } catch (error: unknown) {
        if (error instanceof Error) {
          const message = error.message.toLowerCase();

          if (message.includes('expired')) {
            setStatus('expired');
            setErrorMessage('This verification link has expired. Please request a new one.');
          } else if (
            message.includes('already verified') ||
            message.includes('already confirmed')
          ) {
            setStatus('already_verified');
          } else {
            setStatus('error');
            setErrorMessage(error.message);
          }
        } else {
          setStatus('error');
          setErrorMessage('Failed to verify email. Please try again.');
        }
      }
    };

    verifyEmail();
  }, [token]);

  const handleResendVerification = async () => {
    setIsResending(true);
    setResendSuccess(false);

    try {
      await authService.resendVerificationEmail();
      setResendSuccess(true);
    } catch (error) {
      console.error('Failed to resend verification email:', error);
    } finally {
      setIsResending(false);
    }
  };

  const renderContent = () => {
    switch (status) {
      case 'verifying':
        return (
          <div className="text-center">
            <div className="mx-auto w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mb-6">
              <FiLoader className="text-blue-600 animate-spin" size={32} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {t('verifyEmail.verifyingTitle', 'Verifying Your Email')}
            </h1>
            <p className="text-gray-600">
              {t(
                'verifyEmail.verifyingMessage',
                'Please wait while we verify your email address...'
              )}
            </p>
          </div>
        );

      case 'success':
        return (
          <div className="text-center">
            <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mb-6">
              <FiCheckCircle className="text-green-600" size={32} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {t('verifyEmail.successTitle', 'Email Verified!')}
            </h1>
            <p className="text-gray-600 mb-6">
              {t(
                'verifyEmail.successMessage',
                'Your email has been successfully verified. You can now sign in to your account.'
              )}
            </p>
            <div className="space-y-3">
              <Button
                variant="primary"
                fullWidth
                onClick={() =>
                  navigate('/login', { state: { message: 'Email verified! Please sign in.' } })
                }
              >
                {t('verifyEmail.signIn', 'Sign In')}
              </Button>
              <Button variant="outline" fullWidth onClick={() => navigate('/vehicles')}>
                {t('verifyEmail.browseVehicles', 'Browse Vehicles')}
              </Button>
            </div>
          </div>
        );

      case 'already_verified':
        return (
          <div className="text-center">
            <div className="mx-auto w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mb-6">
              <FiCheckCircle className="text-blue-600" size={32} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {t('verifyEmail.alreadyVerifiedTitle', 'Already Verified')}
            </h1>
            <p className="text-gray-600 mb-6">
              {t(
                'verifyEmail.alreadyVerifiedMessage',
                'This email address has already been verified.'
              )}
            </p>
            <Button variant="primary" fullWidth onClick={() => navigate('/login')}>
              {t('verifyEmail.signIn', 'Sign In')}
            </Button>
          </div>
        );

      case 'expired':
        return (
          <div className="text-center">
            <div className="mx-auto w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mb-6">
              <FiMail className="text-yellow-600" size={32} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {t('verifyEmail.expiredTitle', 'Link Expired')}
            </h1>
            <p className="text-gray-600 mb-6">
              {errorMessage ||
                t(
                  'verifyEmail.expiredMessage',
                  'This verification link has expired. Please request a new one.'
                )}
            </p>

            {resendSuccess ? (
              <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg">
                <p className="text-sm text-green-800">
                  {t(
                    'verifyEmail.resendSuccess',
                    'A new verification email has been sent. Please check your inbox.'
                  )}
                </p>
              </div>
            ) : (
              <Button
                variant="primary"
                fullWidth
                onClick={handleResendVerification}
                isLoading={isResending}
              >
                {t('verifyEmail.resendEmail', 'Resend Verification Email')}
              </Button>
            )}

            <p className="mt-4 text-sm text-gray-500">
              {t('verifyEmail.checkSpam', "Don't forget to check your spam folder!")}
            </p>
          </div>
        );

      case 'error':
      default:
        return (
          <div className="text-center">
            <div className="mx-auto w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mb-6">
              <FiAlertCircle className="text-red-600" size={32} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              {t('verifyEmail.errorTitle', 'Verification Failed')}
            </h1>
            <p className="text-gray-600 mb-6">
              {errorMessage ||
                t(
                  'verifyEmail.errorMessage',
                  'We could not verify your email. Please try again or request a new verification link.'
                )}
            </p>

            <div className="space-y-3">
              {resendSuccess ? (
                <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg">
                  <p className="text-sm text-green-800">
                    {t(
                      'verifyEmail.resendSuccess',
                      'A new verification email has been sent. Please check your inbox.'
                    )}
                  </p>
                </div>
              ) : (
                <Button
                  variant="primary"
                  fullWidth
                  onClick={handleResendVerification}
                  isLoading={isResending}
                >
                  {t('verifyEmail.resendEmail', 'Resend Verification Email')}
                </Button>
              )}

              <Button variant="outline" fullWidth onClick={() => navigate('/login')}>
                {t('verifyEmail.backToLogin', 'Back to Login')}
              </Button>
            </div>
          </div>
        );
    }
  };

  return (
    <div className="w-full max-w-md mx-auto">
      {renderContent()}

      {/* Help Link */}
      {(status === 'error' || status === 'expired') && (
        <p className="mt-8 text-center text-sm text-gray-500">
          {t('verifyEmail.needHelp', 'Need help?')}{' '}
          <Link
            to="/contact"
            className="font-medium text-primary hover:text-primary-600 transition-colors"
          >
            {t('verifyEmail.contactSupport', 'Contact Support')}
          </Link>
        </p>
      )}
    </div>
  );
}
