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
import { FiMail, FiLock, FiAlertCircle } from 'react-icons/fi';

// Validation schema
const loginSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  rememberMe: z.boolean().optional(),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const location = useLocation();
  const storeLogin = useAuthStore((state) => state.login);
  const [apiError, setApiError] = useState<string | null>(null);

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
      
      // Call backend auth service
      const response = await authService.login({
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe,
      });
      
      // Update auth store with response
      storeLogin(response);
      
      // Redirect based on account type
      let defaultPath = '/dashboard';
      if (response.user.accountType === 'admin') {
        defaultPath = '/admin';
      } else if (response.user.accountType === 'dealer' || response.user.accountType === 'dealer_employee') {
        defaultPath = '/dealer';
      }
      
      // Redirect to the page they tried to visit or default
      const from = (location.state as { from?: { pathname: string } })?.from?.pathname || defaultPath;
      navigate(from, { replace: true });
    } catch (error: unknown) {
      if (error instanceof Error) {
        setApiError(error.message || 'Invalid email or password. Please try again.');
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

  return (
    <div className="w-full">
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
          {t('login.title')}
        </h1>
        <p className="text-gray-600">
          {t('login.subtitle')}
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

      {/* Login Form */}
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
        <Button
          type="submit"
          variant="primary"
          size="lg"
          fullWidth
          isLoading={isSubmitting}
        >
          {t('login.signIn')}
        </Button>
      </form>

      {/* OAuth Buttons */}
      <OAuthButtons
        onGoogleClick={handleGoogleLogin}
        onMicrosoftClick={handleMicrosoftLogin}
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
    </div>
  );
}
