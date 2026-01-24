import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuthStore } from '@/store/authStore';
import { authService } from '@/services/authService';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import OAuthButtons from '@/components/auth/OAuthButtons';
import { FiMail, FiLock, FiAlertCircle, FiInfo, FiCheckCircle, FiShield } from 'react-icons/fi';

// Validation schema
const loginSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  rememberMe: z.boolean().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

// Extended response type for 2FA and revoked device verification
interface LoginResponse {
  requiresTwoFactor?: boolean;
  sessionToken?: string;
  twoFactorType?: string;
  // AUTH-SEC-005: Revoked device verification
  requiresRevokedDeviceVerification?: boolean;
  deviceFingerprint?: string;
  // Standard login response
  accessToken?: string;
  refreshToken?: string;
  user?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    accountType: string;
  };
}

export default function LoginPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const location = useLocation();
  const storeLogin = useAuthStore((state) => state.login);
  const [apiError, setApiError] = useState<string | null>(null);
  const [showEmailVerification, setShowEmailVerification] = useState(false);

  // AUTH-SEC-005: Revoked device verification state
  const [showRevokedDeviceVerification, setShowRevokedDeviceVerification] = useState(false);
  const [revokedDeviceEmail, setRevokedDeviceEmail] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [isRequestingCode, setIsRequestingCode] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [codeSent, setCodeSent] = useState(false);
  const [remainingAttempts, setRemainingAttempts] = useState<number | undefined>();
  const [isLockedOut, setIsLockedOut] = useState(false);
  const [lockoutMinutes, setLockoutMinutes] = useState<number | undefined>();

  // Get success message from navigation state (e.g., after email verification)
  const successMessage = location.state?.message as string | undefined;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      setApiError(null);
      setShowEmailVerification(false);
      setShowRevokedDeviceVerification(false);

      // Call backend auth service
      const response: LoginResponse = await authService.login({
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe,
      });

      // AUTH-SEC-005: Check if revoked device verification is required
      if (response.requiresRevokedDeviceVerification) {
        setRevokedDeviceEmail(data.email);
        setShowRevokedDeviceVerification(true);
        setCodeSent(false);
        setVerificationCode('');
        setRemainingAttempts(undefined);
        setIsLockedOut(false);
        return;
      }

      // Check if 2FA is required
      if (response.requiresTwoFactor && response.sessionToken) {
        // Store session token and redirect to 2FA verification
        sessionStorage.setItem('twoFactorSessionToken', response.sessionToken);
        sessionStorage.setItem('twoFactorType', response.twoFactorType || 'authenticator');
        sessionStorage.setItem(
          'pendingRedirect',
          (location.state as { from?: { pathname: string } })?.from?.pathname || '/dashboard'
        );

        // Navigate with state for better UX
        navigate('/verify-2fa', {
          replace: true,
          state: {
            sessionToken: response.sessionToken,
            email: data.email,
            twoFactorType:
              response.twoFactorType === 'sms' ? 2 : response.twoFactorType === 'email' ? 3 : 1, // 1=Authenticator, 2=SMS, 3=Email
          },
        });
        return;
      }

      // Update auth store with response
      storeLogin(response as any);

      // Redirect based on account type
      let defaultPath = '/dashboard';
      if (response.user?.accountType === 'admin') {
        defaultPath = '/admin';
      } else if (
        response.user?.accountType === 'dealer' ||
        response.user?.accountType === 'dealer_employee'
      ) {
        defaultPath = '/dealer';
      }

      // Redirect to the page they tried to visit or default
      const from =
        (location.state as { from?: { pathname: string } })?.from?.pathname || defaultPath;
      navigate(from, { replace: true });
    } catch (error: unknown) {
      if (error instanceof Error) {
        const errorMessage = error.message || '';

        // Check for email verification required error
        if (
          errorMessage.toLowerCase().includes('email') &&
          errorMessage.toLowerCase().includes('verif')
        ) {
          setShowEmailVerification(true);
          setApiError('Your email address has not been verified. Please check your inbox.');
        } else {
          setApiError(errorMessage || 'Invalid email or password. Please try again.');
        }
      } else {
        setApiError('An unexpected error occurred. Please try again.');
      }
    }
  };

  const handleGoogleLogin = async () => {
    try {
      await authService.loginWithGoogle();
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    }
  };

  const handleMicrosoftLogin = async () => {
    try {
      await authService.loginWithMicrosoft();
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    }
  };

  const handleFacebookLogin = async () => {
    try {
      await authService.loginWithFacebook();
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    }
  };

  const handleAppleLogin = async () => {
    try {
      await authService.loginWithApple();
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    }
  };

  // AUTH-SEC-005: Request verification code for revoked device
  const handleRequestRevokedDeviceCode = async () => {
    try {
      setIsRequestingCode(true);
      setApiError(null);

      const pendingLogin = authService.getPendingRevokedDeviceLogin();
      if (!pendingLogin) {
        setApiError('Session expired. Please login again.');
        setShowRevokedDeviceVerification(false);
        return;
      }

      const response = await authService.requestRevokedDeviceCode({
        email: pendingLogin.email,
        deviceFingerprint: pendingLogin.deviceFingerprint,
      });

      if (response.isLockedOut) {
        setIsLockedOut(true);
        setLockoutMinutes(response.lockoutMinutesRemaining);
        setApiError(
          `Too many attempts. Please try again in ${response.lockoutMinutesRemaining} minutes.`
        );
        return;
      }

      if (response.success) {
        setCodeSent(true);
        setApiError(null);
      } else {
        setApiError(response.message || 'Failed to send verification code');
      }
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    } finally {
      setIsRequestingCode(false);
    }
  };

  // AUTH-SEC-005: Verify the code and complete login
  const handleVerifyRevokedDevice = async () => {
    try {
      setIsVerifying(true);
      setApiError(null);

      const pendingLogin = authService.getPendingRevokedDeviceLogin();
      if (!pendingLogin) {
        setApiError('Session expired. Please login again.');
        setShowRevokedDeviceVerification(false);
        return;
      }

      const response = await authService.verifyRevokedDevice({
        email: pendingLogin.email,
        deviceFingerprint: pendingLogin.deviceFingerprint,
        verificationCode: verificationCode.trim(),
        password: pendingLogin.password,
      });

      if (response.isLockedOut) {
        setIsLockedOut(true);
        setLockoutMinutes(response.lockoutMinutesRemaining);
        setApiError(
          `Account temporarily locked. Please try again in ${response.lockoutMinutesRemaining} minutes.`
        );
        return;
      }

      if (response.remainingAttempts !== undefined) {
        setRemainingAttempts(response.remainingAttempts);
        setApiError(`Invalid code. ${response.remainingAttempts} attempts remaining.`);
        return;
      }

      if (response.success && response.accessToken) {
        // Clear pending login data
        authService.clearPendingRevokedDeviceLogin();

        // Build user object from response
        const user = {
          id: response.userId || '',
          email: response.email || pendingLogin.email,
          name: response.email?.split('@')[0] || '',
          accountType: 'individual' as const,
          emailVerified: true,
          createdAt: new Date().toISOString(),
        };

        // Update auth store
        storeLogin({
          user,
          accessToken: response.accessToken,
          refreshToken: response.refreshToken || '',
        });

        // Navigate to dashboard
        const from =
          (location.state as { from?: { pathname: string } })?.from?.pathname || '/dashboard';
        navigate(from, { replace: true });
      } else {
        setApiError(response.message || 'Verification failed. Please try again.');
      }
    } catch (error) {
      if (error instanceof Error) {
        setApiError(error.message);
      }
    } finally {
      setIsVerifying(false);
    }
  };

  // AUTH-SEC-005: Cancel revoked device verification
  const handleCancelRevokedDeviceVerification = () => {
    setShowRevokedDeviceVerification(false);
    setCodeSent(false);
    setVerificationCode('');
    setRemainingAttempts(undefined);
    setIsLockedOut(false);
    setApiError(null);
    authService.clearPendingRevokedDeviceLogin();
  };

  return (
    <div className="w-full">
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">{t('login.title')}</h1>
        <p className="text-gray-600">{t('login.subtitle')}</p>
      </div>

      {/* Success Message (e.g., after email verification) */}
      {successMessage && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3">
          <FiCheckCircle className="text-green-600 flex-shrink-0 mt-0.5" size={20} />
          <p className="text-sm text-green-800">{successMessage}</p>
        </div>
      )}

      {/* AUTH-SEC-005: Revoked Device Verification UI */}
      {showRevokedDeviceVerification && (
        <div className="mb-6 p-6 bg-amber-50 border border-amber-200 rounded-lg">
          <div className="flex items-start gap-3 mb-4">
            <FiShield className="text-amber-600 flex-shrink-0 mt-0.5" size={24} />
            <div>
              <h3 className="font-semibold text-amber-800">Security Verification Required</h3>
              <p className="text-sm text-amber-700 mt-1">
                This device was previously logged out for security reasons. To continue, we need to
                verify it's really you.
              </p>
            </div>
          </div>

          {isLockedOut ? (
            <div className="text-center py-4">
              <p className="text-amber-800 font-medium">Account Temporarily Locked</p>
              <p className="text-sm text-amber-600 mt-1">
                Too many failed attempts. Please try again in {lockoutMinutes} minutes.
              </p>
              <Button
                type="button"
                variant="outline"
                size="md"
                className="mt-4"
                onClick={handleCancelRevokedDeviceVerification}
              >
                Back to Login
              </Button>
            </div>
          ) : !codeSent ? (
            <div className="space-y-4">
              <p className="text-sm text-gray-700">
                We'll send a verification code to <strong>{revokedDeviceEmail}</strong>
              </p>
              <div className="flex gap-3">
                <Button
                  type="button"
                  variant="primary"
                  size="md"
                  onClick={handleRequestRevokedDeviceCode}
                  isLoading={isRequestingCode}
                >
                  Send Verification Code
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="md"
                  onClick={handleCancelRevokedDeviceVerification}
                >
                  Cancel
                </Button>
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              <div className="flex items-center gap-2 text-green-700 text-sm">
                <FiCheckCircle size={16} />
                <span>Verification code sent to {revokedDeviceEmail}</span>
              </div>

              <Input
                type="text"
                label="Verification Code"
                placeholder="Enter 6-digit code"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                maxLength={6}
                leftIcon={<FiLock size={18} />}
                fullWidth
              />

              {remainingAttempts !== undefined && (
                <p className="text-sm text-amber-600">{remainingAttempts} attempts remaining</p>
              )}

              <div className="flex gap-3">
                <Button
                  type="button"
                  variant="primary"
                  size="md"
                  onClick={handleVerifyRevokedDevice}
                  isLoading={isVerifying}
                  disabled={verificationCode.length < 6}
                >
                  Verify & Login
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="md"
                  onClick={handleRequestRevokedDeviceCode}
                  isLoading={isRequestingCode}
                >
                  Resend Code
                </Button>
              </div>

              <button
                type="button"
                onClick={handleCancelRevokedDeviceVerification}
                className="text-sm text-gray-500 hover:text-gray-700 underline"
              >
                Cancel and go back
              </button>
            </div>
          )}

          {apiError && (
            <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded flex items-start gap-2">
              <FiAlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={16} />
              <p className="text-sm text-red-800">{apiError}</p>
            </div>
          )}
        </div>
      )}

      {/* API Error Alert - Only show when not in revoked device verification mode */}
      {apiError && !showRevokedDeviceVerification && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
          <FiAlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={20} />
          <div className="flex-1">
            <p className="text-sm text-red-800">{apiError}</p>
            {showEmailVerification && (
              <button
                type="button"
                onClick={() => authService.resendVerificationEmail()}
                className="text-sm text-red-700 underline mt-2 hover:text-red-900"
              >
                Resend verification email
              </button>
            )}
          </div>
        </div>
      )}

      {/* Login Form - Hide when revoked device verification is active */}
      {!showRevokedDeviceVerification && (
        <>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Email Field */}
            <Input
              {...register('email')}
              type="email"
              label={t('login.email')}
              placeholder={t('login.emailPlaceholder')}
              error={errors.email?.message}
              leftIcon={<FiMail size={18} />}
              required
              fullWidth
            />

            {/* Password Field */}
            <Input
              {...register('password')}
              type="password"
              label={t('login.password')}
              placeholder={t('login.passwordPlaceholder')}
              error={errors.password?.message}
              leftIcon={<FiLock size={18} />}
              required
              fullWidth
            />

            {/* Remember Me & Forgot Password */}
            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  {...register('rememberMe')}
                  type="checkbox"
                  className="w-4 h-4 text-primary border-gray-300 rounded focus:ring-primary-500"
                />
                <span className="text-sm text-gray-700">{t('login.rememberMe')}</span>
              </label>

              <Link
                to="/forgot-password"
                className="text-sm text-primary hover:text-primary-600 font-medium transition-colors"
              >
                {t('login.forgotPassword')}
              </Link>
            </div>

            {/* Submit Button */}
            <Button type="submit" variant="primary" size="lg" fullWidth isLoading={isSubmitting}>
              {t('login.signIn')}
            </Button>
          </form>

          {/* OAuth Buttons */}
          <OAuthButtons
            onGoogleClick={handleGoogleLogin}
            onMicrosoftClick={handleMicrosoftLogin}
            onFacebookClick={handleFacebookLogin}
            onAppleClick={handleAppleLogin}
            disabled={isSubmitting}
          />

          {/* Register Link */}
          <p className="mt-8 text-center text-sm text-gray-600">
            {t('login.noAccount')}{' '}
            <Link
              to="/register"
              className="font-medium text-primary hover:text-primary-600 transition-colors"
            >
              {t('login.signUpFree')}
            </Link>
          </p>
        </>
      )}
    </div>
  );
}
