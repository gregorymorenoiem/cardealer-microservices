import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { authService } from '@/services/authService';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import { FiLock, FiAlertCircle, FiCheckCircle } from 'react-icons/fi';

// Password strength checker
const getPasswordStrength = (password: string): { score: number; label: string; color: string } => {
  let score = 0;
  if (password.length >= 8) score++;
  if (password.length >= 12) score++;
  if (/[a-z]/.test(password)) score++;
  if (/[A-Z]/.test(password)) score++;
  if (/\d/.test(password)) score++;
  if (/[^a-zA-Z0-9]/.test(password)) score++;

  if (score <= 2) return { score, label: 'Weak', color: 'bg-red-500' };
  if (score <= 4) return { score, label: 'Medium', color: 'bg-yellow-500' };
  return { score, label: 'Strong', color: 'bg-green-500' };
};

// Validation schema
const resetPasswordSchema = z
  .object({
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
      .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .regex(/\d/, 'Password must contain at least one number'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

export default function ResetPasswordPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [apiError, setApiError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);
  const [passwordStrength, setPasswordStrength] = useState({ score: 0, label: '', color: '' });

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const password = watch('password');

  // Redirect if no token
  useEffect(() => {
    if (!token) {
      navigate('/forgot-password', {
        state: { error: 'Invalid or missing reset token. Please request a new password reset.' },
      });
    }
  }, [token, navigate]);

  // Update password strength on password change
  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (value) {
      setPasswordStrength(getPasswordStrength(value));
    } else {
      setPasswordStrength({ score: 0, label: '', color: '' });
    }
  };

  const onSubmit = async (data: ResetPasswordFormData) => {
    try {
      setApiError(null);

      if (!token) {
        throw new Error('Reset token is missing');
      }

      await authService.resetPassword(token, data.password);
      setIsSuccess(true);

      // Redirect to login after 3 seconds
      setTimeout(() => {
        navigate('/login', {
          state: { message: 'Password reset successfully. Please login with your new password.' },
        });
      }, 3000);
    } catch (error: unknown) {
      if (error instanceof Error) {
        setApiError(error.message);
      } else {
        setApiError('Failed to reset password. The link may have expired.');
      }
    }
  };

  if (!token) {
    return null;
  }

  if (isSuccess) {
    return (
      <div className="w-full max-w-md mx-auto">
        <div className="text-center">
          <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mb-6">
            <FiCheckCircle className="text-green-600" size={32} />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-4">
            {t('resetPassword.successTitle', 'Password Reset Successfully')}
          </h1>
          <p className="text-gray-600 mb-6">
            {t(
              'resetPassword.successMessage',
              'Your password has been changed. You will be redirected to the login page shortly.'
            )}
          </p>
          <Link to="/login" className="text-primary hover:text-primary-600 font-medium">
            {t('resetPassword.backToLogin', 'Go to Login')}
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md mx-auto">
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
          {t('resetPassword.title', 'Reset Your Password')}
        </h1>
        <p className="text-gray-600">
          {t('resetPassword.subtitle', 'Enter your new password below')}
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

      {/* Reset Password Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        {/* New Password Field */}
        <div>
          <Input
            {...register('password', {
              onChange: handlePasswordChange,
            })}
            type="password"
            label={t('resetPassword.newPassword', 'New Password')}
            placeholder={t('resetPassword.newPasswordPlaceholder', 'Enter your new password')}
            error={errors.password?.message}
            leftIcon={<FiLock size={18} />}
            required
            fullWidth
          />

          {/* Password Strength Indicator */}
          {password && (
            <div className="mt-2">
              <div className="flex items-center justify-between mb-1">
                <span className="text-xs text-gray-600">Password strength:</span>
                <span
                  className={`text-xs font-medium ${
                    passwordStrength.label === 'Weak'
                      ? 'text-red-600'
                      : passwordStrength.label === 'Medium'
                        ? 'text-yellow-600'
                        : 'text-green-600'
                  }`}
                >
                  {passwordStrength.label}
                </span>
              </div>
              <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                  className={`h-full transition-all duration-300 ${passwordStrength.color}`}
                  style={{ width: `${(passwordStrength.score / 6) * 100}%` }}
                />
              </div>
            </div>
          )}

          {/* Password Requirements */}
          <ul className="mt-3 text-xs text-gray-500 space-y-1">
            <li className={password && password.length >= 8 ? 'text-green-600' : ''}>
              • At least 8 characters
            </li>
            <li className={password && /[a-z]/.test(password) ? 'text-green-600' : ''}>
              • One lowercase letter
            </li>
            <li className={password && /[A-Z]/.test(password) ? 'text-green-600' : ''}>
              • One uppercase letter
            </li>
            <li className={password && /\d/.test(password) ? 'text-green-600' : ''}>
              • One number
            </li>
          </ul>
        </div>

        {/* Confirm Password Field */}
        <Input
          {...register('confirmPassword')}
          type="password"
          label={t('resetPassword.confirmPassword', 'Confirm Password')}
          placeholder={t('resetPassword.confirmPasswordPlaceholder', 'Confirm your new password')}
          error={errors.confirmPassword?.message}
          leftIcon={<FiLock size={18} />}
          required
          fullWidth
        />

        {/* Submit Button */}
        <Button type="submit" variant="primary" size="lg" fullWidth isLoading={isSubmitting}>
          {t('resetPassword.submit', 'Reset Password')}
        </Button>
      </form>

      {/* Back to Login */}
      <p className="mt-8 text-center text-sm text-gray-600">
        {t('resetPassword.rememberPassword', 'Remember your password?')}{' '}
        <Link
          to="/login"
          className="font-medium text-primary hover:text-primary-600 transition-colors"
        >
          {t('resetPassword.signIn', 'Sign in')}
        </Link>
      </p>
    </div>
  );
}
