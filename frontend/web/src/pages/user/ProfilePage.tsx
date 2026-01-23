import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import { FiUser, FiMail, FiSave, FiAlertCircle, FiCheckCircle } from 'react-icons/fi';

// Validation schema
const profileSchema = z.object({
  username: z
    .string()
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must be less than 50 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  email: z.string().email('Please enter a valid email address'),
  firstName: z.string().max(100, 'First name must be less than 100 characters').optional(),
  lastName: z.string().max(100, 'Last name must be less than 100 characters').optional(),
  phone: z
    .string()
    .max(20, 'Phone number must be less than 20 characters')
    .regex(
      /^[\d\s\-\+\(\)]*$/,
      'Phone number can only contain digits, spaces, and symbols like +, -, (, )'
    )
    .optional()
    .or(z.literal('')),
});

type ProfileFormData = z.infer<typeof profileSchema>;

export default function ProfilePage() {
  const { t } = useTranslation('user');
  const { user, updateUser } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      username: user?.name || '',
      email: user?.email || '',
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      phone: user?.phone || '',
    },
  });

  const onSubmit = async (data: ProfileFormData) => {
    try {
      setSuccessMessage(null);
      setErrorMessage(null);

      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // Update user in store
      updateUser({
        ...user!,
        ...data,
      });

      setSuccessMessage(t('profile.updated'));
      setIsEditing(false);

      // Clear success message after 3 seconds
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || t('profile.updateFailed'));
      } else {
        setErrorMessage(t('profile.unexpectedError'));
      }
    }
  };

  const handleCancel = () => {
    reset();
    setIsEditing(false);
    setErrorMessage(null);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold font-heading text-gray-900">Mi Perfil</h1>
        </div>

        {/* Success Message */}
        {successMessage && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3">
            <FiCheckCircle className="text-green-600 flex-shrink-0 mt-0.5" size={20} />
            <div className="flex-1">
              <p className="text-sm text-green-800">{successMessage}</p>
            </div>
          </div>
        )}

        {/* Error Message */}
        {errorMessage && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
            <FiAlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={20} />
            <div className="flex-1">
              <p className="text-sm text-red-800">{errorMessage}</p>
            </div>
          </div>
        )}

        {/* Profile Card */}
        <div className="bg-white rounded-lg shadow-card p-6 sm:p-8">
          {/* Avatar Section */}
          <div className="flex items-center gap-6 mb-8 pb-8 border-b border-gray-200">
            <div className="w-20 h-20 bg-primary-100 rounded-full flex items-center justify-center">
              <FiUser className="text-primary" size={32} />
            </div>
            <div className="flex-1">
              <h2 className="text-xl font-semibold text-gray-900 mb-1">{user?.name || 'User'}</h2>
              <p className="text-gray-600 text-sm">{user?.email}</p>
            </div>
            {!isEditing && (
              <Button variant="outline" size="md" onClick={() => setIsEditing(true)}>
                {t('profile.edit')}
              </Button>
            )}
          </div>

          {/* Profile Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
              {/* Username */}
              <Input
                {...register('username')}
                type="text"
                label={t('profile.fields.username')}
                placeholder="johndoe"
                error={errors.username?.message}
                leftIcon={<FiUser size={18} />}
                disabled={!isEditing}
                required
                fullWidth
              />

              {/* Email */}
              <Input
                {...register('email')}
                type="email"
                label={t('profile.fields.email')}
                placeholder="you@example.com"
                error={errors.email?.message}
                leftIcon={<FiMail size={18} />}
                disabled={!isEditing}
                required
                fullWidth
              />

              {/* First Name */}
              <Input
                {...register('firstName')}
                type="text"
                label={t('profile.fields.firstName')}
                placeholder="John"
                error={errors.firstName?.message}
                disabled={!isEditing}
                fullWidth
              />

              {/* Last Name */}
              <Input
                {...register('lastName')}
                type="text"
                label={t('profile.fields.lastName')}
                placeholder="Doe"
                error={errors.lastName?.message}
                disabled={!isEditing}
                fullWidth
              />

              {/* Phone */}
              <Input
                {...register('phone')}
                type="tel"
                label={t('profile.fields.phone')}
                placeholder="(555) 123-4567"
                error={errors.phone?.message}
                disabled={!isEditing}
                fullWidth
              />
            </div>

            {/* Action Buttons */}
            {isEditing && (
              <div className="flex gap-3 pt-4">
                <Button
                  type="submit"
                  variant="primary"
                  size="md"
                  isLoading={isSubmitting}
                  leftIcon={<FiSave size={18} />}
                >
                  {t('profile.save')}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="md"
                  onClick={handleCancel}
                  disabled={isSubmitting}
                >
                  {t('profile.cancel')}
                </Button>
              </div>
            )}
          </form>
        </div>

        {/* Account Stats Card (Optional) */}
        <div className="mt-6 bg-white rounded-lg shadow-card p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">{t('profile.accountInfo')}</h3>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-gray-600 mb-1">Member Since</p>
              <p className="text-lg font-semibold text-gray-900">N/A</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Listings</p>
              <p className="text-lg font-semibold text-gray-900">0</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Account Status</p>
              <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
                Active
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
