import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '@/hooks/useAuth';
import { authService } from '@/services/authService';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import { FiMail, FiLock, FiUser, FiAlertCircle, FiCheckCircle } from 'react-icons/fi';

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
const registerSchema = z.object({
  username: z.string()
    .min(3, 'Username must be at least 3 characters')
    .max(20, 'Username must be less than 20 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  email: z.string().email('Please enter a valid email address'),
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/\d/, 'Password must contain at least one number'),
  confirmPassword: z.string(),
  terms: z.boolean().refine(val => val === true, {
    message: 'You must accept the terms and conditions',
  }),
}).refine(data => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const { t } = useTranslation('auth');
  const navigate = useNavigate();
  const { login } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);
  const [passwordStrength, setPasswordStrength] = useState({ score: 0, label: '', color: '' });

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      terms: false,
    },
  });

  const password = watch('password');

  // Update password strength on password change
  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    if (value) {
      setPasswordStrength(getPasswordStrength(value));
    } else {
      setPasswordStrength({ score: 0, label: '', color: '' });
    }
  };

  const onSubmit = async (data: RegisterFormData) => {
    try {
      setApiError(null);
      
      // Parse name from username
      const nameParts = data.username.split(' ');
      const firstName = nameParts[0] || data.username;
      const lastName = nameParts.slice(1).join(' ') || '';
      
      // Call mock register service
      const response = await authService.register({
        email: data.email,
        password: data.password,
        firstName,
        lastName,
      });
      
      // Update auth store
      login(response);
      
      // Show success message and redirect
      navigate('/dashboard', { 
        replace: true,
        state: { message: 'Account created successfully! Welcome to CarDealer.' }
      });
    } catch (error: unknown) {
      if (error instanceof Error) {
        setApiError(error.message || 'Registration failed. Please try again.');
      } else {
        setApiError('An unexpected error occurred. Please try again.');
      }
    }
  };

  return (
    <div className="w-full">
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
          {t('register.title')}
        </h1>
        <p className="text-gray-600">
          {t('register.subtitle')}
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

      {/* Register Form */}
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        {/* Username Field */}
        <Input
          {...register('username')}
          type="text"
          label={t('register.username')}
          placeholder={t('register.usernamePlaceholder')}
          error={errors.username?.message}
          leftIcon={<FiUser size={18} />}
          required
          fullWidth
        />

        {/* Email Field */}
        <Input
          {...register('email')}
          type="email"
          label={t('register.email')}
          placeholder={t('register.emailPlaceholder')}
          error={errors.email?.message}
          leftIcon={<FiMail size={18} />}
          required
          fullWidth
        />

        {/* Password Field with Strength Indicator */}
        <div>
          <Input
            {...register('password', {
              onChange: handlePasswordChange,
            })}
            type="password"
            label={t('register.password')}
            placeholder={t('register.passwordPlaceholder')}
            error={errors.password?.message}
            leftIcon={<FiLock size={18} />}
            required
            fullWidth
          />
          
          {/* Password Strength Indicator */}
          {password && (
            <div className="mt-2">
              <div className="flex items-center justify-between mb-1">
                <span className="text-xs text-gray-600">{t('register.passwordStrength')}:</span>
                <span className={`text-xs font-medium ${
                  passwordStrength.label === 'Weak' ? 'text-red-600' :
                  passwordStrength.label === 'Medium' ? 'text-yellow-600' :
                  'text-green-600'
                }`}>
                  {t(`register.strength.${passwordStrength.label.toLowerCase()}`)}
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
        </div>

        {/* Confirm Password Field */}
        <Input
          {...register('confirmPassword')}
          type="password"
          label={t('register.confirmPassword')}
          placeholder={t('register.confirmPasswordPlaceholder')}
          error={errors.confirmPassword?.message}
          leftIcon={<FiLock size={18} />}
          required
          fullWidth
        />

        {/* Terms & Conditions */}
        <div>
          <label className="flex items-start gap-3 cursor-pointer group">
            <input
              {...register('terms')}
              type="checkbox"
              className="w-4 h-4 mt-0.5 text-primary border-gray-300 rounded focus:ring-primary-500"
            />
            <span className="text-sm text-gray-700 group-hover:text-gray-900 transition-colors">
              {t('register.agreeToTerms')}{' '}
              <Link to="/terms" className="text-primary hover:text-primary-600 font-medium">
                {t('register.termsOfService')}
              </Link>
              {' '}{t('register.and')}{' '}
              <Link to="/privacy" className="text-primary hover:text-primary-600 font-medium">
                {t('register.privacyPolicy')}
              </Link>
            </span>
          </label>
          {errors.terms && (
            <p className="mt-1 text-xs text-red-600">{errors.terms.message}</p>
          )}
        </div>

        {/* Submit Button */}
        <Button
          type="submit"
          variant="primary"
          size="lg"
          fullWidth
          isLoading={isSubmitting}
        >
          {t('register.createAccount')}
        </Button>
      </form>

      {/* Email Verification Notice */}
      <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg flex items-start gap-3">
        <FiCheckCircle className="text-blue-600 flex-shrink-0 mt-0.5" size={20} />
        <div className="flex-1">
          <p className="text-sm text-blue-800">
            {t('register.verificationNotice')}
          </p>
        </div>
      </div>

      {/* Login Link */}
      <p className="mt-6 text-center text-sm text-gray-600">
        {t('register.haveAccount')}{' '}
        <Link
          to="/login"
          className="font-medium text-primary hover:text-primary-600 transition-colors"
        >
          {t('register.signIn')}
        </Link>
      </p>
    </div>
  );
}
