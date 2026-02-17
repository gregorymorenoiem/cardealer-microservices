import { useState, useEffect } from 'react';
import { useAuthStore } from '@/store/authStore';
import { authService } from '@/services/authService';
import uploadService from '@/services/uploadService';
import { type NotificationPreferences } from '@/services/notificationService';
import { useNotificationPreferences, useUpdatePreferences } from '@/hooks/useNotifications';
import { FiUser, FiBell, FiLock, FiShield, FiMail, FiCamera, FiCheck, FiX } from 'react-icons/fi';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

// Profile update schema
const profileSchema = z.object({
  firstName: z.string().min(2, 'First name must be at least 2 characters'),
  lastName: z.string().min(2, 'Last name must be at least 2 characters'),
  phone: z.string().optional(),
  email: z.string().email('Invalid email address'),
});

// Password change schema
const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Passwords don't match",
  path: ['confirmPassword'],
});

type ProfileFormData = z.infer<typeof profileSchema>;
type PasswordFormData = z.infer<typeof passwordSchema>;

type SettingsSection = 'profile' | 'notifications' | 'security' | 'privacy';

export default function SettingsTab() {
  const { user, updateUser } = useAuthStore();
  const [activeSection, setActiveSection] = useState<SettingsSection>('profile');
  const [isLoading, setIsLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [avatarPreview, setAvatarPreview] = useState<string | undefined>(user?.avatar);
  const [uploadProgress, setUploadProgress] = useState(0);
  // TanStack Query hooks for notification preferences
  const { data: fetchedPreferences, isLoading: prefsLoading } = useNotificationPreferences();
  const updatePreferences = useUpdatePreferences();

  const [notificationPrefs, setNotificationPrefs] = useState<NotificationPreferences>({
    emailNotifications: true,
    pushNotifications: false,
    smsNotifications: false,
    notifyOnMessage: true,
    notifyOnVehicleApproval: true,
    notifyOnVehicleSold: true,
    notifyOnPriceChange: false,
    notifyOnNewFavorite: false,
    notifyOnSystemUpdates: true,
  });

  const profileForm = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      phone: user?.phone || '',
      email: user?.email || '',
    },
  });

  const passwordForm = useForm<PasswordFormData>({
    resolver: zodResolver(passwordSchema),
  });

  // Sync notification preferences from API
  useEffect(() => {
    if (fetchedPreferences) {
      setNotificationPrefs(fetchedPreferences);
    }
  }, [fetchedPreferences]);

  const showMessage = (type: 'success' | 'error', message: string) => {
    if (type === 'success') {
      setSuccessMessage(message);
      setErrorMessage('');
    } else {
      setErrorMessage(message);
      setSuccessMessage('');
    }
    setTimeout(() => {
      setSuccessMessage('');
      setErrorMessage('');
    }, 5000);
  };

  const handleProfileUpdate = async (data: ProfileFormData) => {
    setIsLoading(true);
    try {
      const updatedUser = await authService.updateProfile(data);
      updateUser(updatedUser);
      showMessage('success', 'Profile updated successfully');
    } catch (error) {
      showMessage('error', error instanceof Error ? error.message : 'Failed to update profile');
    } finally {
      setIsLoading(false);
    }
  };

  const handlePasswordChange = async (data: PasswordFormData) => {
    setIsLoading(true);
    try {
      await authService.changePassword(data.currentPassword, data.newPassword);
      passwordForm.reset();
      showMessage('success', 'Password changed successfully');
    } catch (error) {
      showMessage('error', error instanceof Error ? error.message : 'Failed to change password');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAvatarChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file
    if (!uploadService.isValidImageFile(file)) {
      showMessage('error', 'Invalid file type. Only JPEG, PNG, and WebP are allowed.');
      return;
    }

    if (!uploadService.isValidFileSize(file, 5)) {
      showMessage('error', 'File size exceeds 5MB limit.');
      return;
    }

    setIsLoading(true);
    try {
      // Show preview
      const preview = await uploadService.createThumbnailPreview(file);
      setAvatarPreview(preview);

      // Upload
      const result = await uploadService.uploadProfilePicture(file, (progress) => {
        setUploadProgress(progress.percentage);
      });

      // Update profile
      const updatedUser = await authService.updateProfile({ avatar: result.url });
      updateUser(updatedUser);
      setUploadProgress(0);
      showMessage('success', 'Profile picture updated successfully');
    } catch {
      showMessage('error', 'Failed to upload profile picture');
      setAvatarPreview(user?.avatar);
    } finally {
      setIsLoading(false);
    }
  };

  const handleNotificationToggle = async (key: keyof NotificationPreferences) => {
    const newValue = !notificationPrefs[key];
    setNotificationPrefs((prev) => ({ ...prev, [key]: newValue }));

    updatePreferences.mutate(
      { [key]: newValue },
      {
        onSuccess: () => {
          showMessage('success', 'Notification preferences updated');
        },
        onError: () => {
          // Revert on error
          setNotificationPrefs((prev) => ({ ...prev, [key]: !newValue }));
          showMessage('error', 'Failed to update preferences');
        },
      }
    );
  };

  const handleDeleteAccount = async () => {
    if (!window.confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      return;
    }

    const confirmation = prompt('Type "DELETE" to confirm:');
    if (confirmation !== 'DELETE') return;

    setIsLoading(true);
    try {
      // Call delete account API
      showMessage('success', 'Account deletion request submitted');
      // Logout after delay
      setTimeout(() => {
        authService.logout();
        window.location.href = '/';
      }, 2000);
    } catch {
      showMessage('error', 'Failed to delete account');
    } finally {
      setIsLoading(false);
    }
  };

  const sections = [
    { id: 'profile' as const, label: 'Profile', icon: FiUser },
    { id: 'notifications' as const, label: 'Notifications', icon: FiBell },
    { id: 'security' as const, label: 'Security', icon: FiLock },
    { id: 'privacy' as const, label: 'Privacy', icon: FiShield },
  ];

  return (
    <div className="bg-white rounded-xl shadow-card">
      <div className="grid md:grid-cols-4 gap-6">
        {/* Sidebar */}
        <div className="md:col-span-1 border-r border-gray-200 p-6">
          <h2 className="text-xl font-bold font-heading text-gray-900 mb-4">Settings</h2>
          <nav className="space-y-1">
            {sections.map((section) => (
              <button
                key={section.id}
                onClick={() => setActiveSection(section.id)}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors ${
                  activeSection === section.id
                    ? 'bg-primary text-white'
                    : 'text-gray-700 hover:bg-gray-100'
                }`}
              >
                <section.icon size={18} />
                {section.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        <div className="md:col-span-3 p-6">
          {/* Messages */}
          {successMessage && (
            <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-2 text-green-800">
              <FiCheck className="flex-shrink-0" />
              <span>{successMessage}</span>
            </div>
          )}
          {errorMessage && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-800">
              <FiX className="flex-shrink-0" />
              <span>{errorMessage}</span>
            </div>
          )}

          {/* Profile Section */}
          {activeSection === 'profile' && (
            <div>
              <h3 className="text-2xl font-bold font-heading text-gray-900 mb-6">Profile Settings</h3>

              {/* Avatar Upload */}
              <div className="mb-8">
                <label className="block text-sm font-medium text-gray-700 mb-3">Profile Picture</label>
                <div className="flex items-center gap-4">
                  <div className="relative">
                    <img
                      src={avatarPreview || `https://ui-avatars.com/api/?name=${user?.firstName}+${user?.lastName}&background=4F46E5&color=fff&size=128`}
                      alt="Profile"
                      className="w-24 h-24 rounded-full object-cover border-2 border-gray-200"
                    />
                    <label className="absolute bottom-0 right-0 bg-primary text-white p-2 rounded-full cursor-pointer hover:bg-primary-600 transition-colors">
                      <FiCamera size={16} />
                      <input
                        type="file"
                        accept="image/*"
                        className="hidden"
                        onChange={handleAvatarChange}
                        disabled={isLoading}
                      />
                    </label>
                  </div>
                  {uploadProgress > 0 && uploadProgress < 100 && (
                    <div className="flex-1">
                      <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-primary transition-all duration-300"
                          style={{ width: `${uploadProgress}%` }}
                        />
                      </div>
                      <p className="text-sm text-gray-600 mt-1">{uploadProgress}% uploaded</p>
                    </div>
                  )}
                </div>
              </div>

              {/* Profile Form */}
              <form onSubmit={profileForm.handleSubmit(handleProfileUpdate)} className="space-y-4">
                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                    <input
                      type="text"
                      {...profileForm.register('firstName')}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                    />
                    {profileForm.formState.errors.firstName && (
                      <p className="text-sm text-red-600 mt-1">{profileForm.formState.errors.firstName.message}</p>
                    )}
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                    <input
                      type="text"
                      {...profileForm.register('lastName')}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                    />
                    {profileForm.formState.errors.lastName && (
                      <p className="text-sm text-red-600 mt-1">{profileForm.formState.errors.lastName.message}</p>
                    )}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                  <div className="relative">
                    <FiMail className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                    <input
                      type="email"
                      {...profileForm.register('email')}
                      className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                    />
                  </div>
                  {profileForm.formState.errors.email && (
                    <p className="text-sm text-red-600 mt-1">{profileForm.formState.errors.email.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Phone (optional)</label>
                  <input
                    type="tel"
                    {...profileForm.register('phone')}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors disabled:opacity-50"
                >
                  {isLoading ? 'Saving...' : 'Save Changes'}
                </button>
              </form>
            </div>
          )}

          {/* Notifications Section */}
          {activeSection === 'notifications' && (
            <div>
              <h3 className="text-2xl font-bold font-heading text-gray-900 mb-6">Notification Preferences</h3>

              <div className="space-y-6">
                <div>
                  <h4 className="font-semibold text-gray-900 mb-3">Notification Channels</h4>
                  <div className="space-y-3">
                    {[
                      { key: 'emailNotifications' as const, label: 'Email Notifications', desc: 'Receive notifications via email' },
                      { key: 'pushNotifications' as const, label: 'Push Notifications', desc: 'Receive push notifications in browser' },
                      { key: 'smsNotifications' as const, label: 'SMS Notifications', desc: 'Receive notifications via SMS' },
                    ].map((item) => (
                      <label key={item.key} className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
                        <div>
                          <div className="font-medium text-gray-900">{item.label}</div>
                          <div className="text-sm text-gray-600">{item.desc}</div>
                        </div>
                        <button
                          type="button"
                          onClick={() => handleNotificationToggle(item.key)}
                          className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                            notificationPrefs[item.key] ? 'bg-primary' : 'bg-gray-300'
                          }`}
                        >
                          <span
                            className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                              notificationPrefs[item.key] ? 'translate-x-6' : 'translate-x-1'
                            }`}
                          />
                        </button>
                      </label>
                    ))}
                  </div>
                </div>

                <div>
                  <h4 className="font-semibold text-gray-900 mb-3">Notification Types</h4>
                  <div className="space-y-3">
                    {[
                      { key: 'notifyOnMessage' as const, label: 'New Messages', desc: 'When someone sends you a message' },
                      { key: 'notifyOnVehicleApproval' as const, label: 'Listing Status', desc: 'When your listing is approved or rejected' },
                      { key: 'notifyOnVehicleSold' as const, label: 'Vehicle Sold', desc: 'When your vehicle is marked as sold' },
                      { key: 'notifyOnPriceChange' as const, label: 'Price Changes', desc: 'When favorited vehicles change price' },
                      { key: 'notifyOnNewFavorite' as const, label: 'New Favorites', desc: 'When someone favorites your listing' },
                      { key: 'notifyOnSystemUpdates' as const, label: 'System Updates', desc: 'Important platform updates and announcements' },
                    ].map((item) => (
                      <label key={item.key} className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
                        <div>
                          <div className="font-medium text-gray-900">{item.label}</div>
                          <div className="text-sm text-gray-600">{item.desc}</div>
                        </div>
                        <button
                          type="button"
                          onClick={() => handleNotificationToggle(item.key)}
                          className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                            notificationPrefs[item.key] ? 'bg-primary' : 'bg-gray-300'
                          }`}
                        >
                          <span
                            className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                              notificationPrefs[item.key] ? 'translate-x-6' : 'translate-x-1'
                            }`}
                          />
                        </button>
                      </label>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Security Section */}
          {activeSection === 'security' && (
            <div>
              <h3 className="text-2xl font-bold font-heading text-gray-900 mb-6">Security Settings</h3>

              <form onSubmit={passwordForm.handleSubmit(handlePasswordChange)} className="space-y-4 max-w-md">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Current Password</label>
                  <input
                    type="password"
                    {...passwordForm.register('currentPassword')}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                  {passwordForm.formState.errors.currentPassword && (
                    <p className="text-sm text-red-600 mt-1">{passwordForm.formState.errors.currentPassword.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">New Password</label>
                  <input
                    type="password"
                    {...passwordForm.register('newPassword')}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                  {passwordForm.formState.errors.newPassword && (
                    <p className="text-sm text-red-600 mt-1">{passwordForm.formState.errors.newPassword.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Confirm New Password</label>
                  <input
                    type="password"
                    {...passwordForm.register('confirmPassword')}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  />
                  {passwordForm.formState.errors.confirmPassword && (
                    <p className="text-sm text-red-600 mt-1">{passwordForm.formState.errors.confirmPassword.message}</p>
                  )}
                </div>

                <button
                  type="submit"
                  disabled={isLoading}
                  className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors disabled:opacity-50"
                >
                  {isLoading ? 'Changing...' : 'Change Password'}
                </button>
              </form>
            </div>
          )}

          {/* Privacy Section */}
          {activeSection === 'privacy' && (
            <div>
              <h3 className="text-2xl font-bold font-heading text-gray-900 mb-6">Privacy & Data</h3>

              <div className="space-y-6">
                <div className="border border-gray-200 rounded-lg p-6">
                  <h4 className="font-semibold text-gray-900 mb-2">Download Your Data</h4>
                  <p className="text-gray-600 mb-4">
                    Request a copy of all your personal data stored on CarDealer.
                  </p>
                  <button className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors">
                    Request Data Export
                  </button>
                </div>

                <div className="border border-red-200 rounded-lg p-6 bg-red-50">
                  <h4 className="font-semibold text-red-900 mb-2">Delete Account</h4>
                  <p className="text-red-700 mb-4">
                    Permanently delete your account and all associated data. This action cannot be undone.
                  </p>
                  <button
                    onClick={handleDeleteAccount}
                    disabled={isLoading}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50"
                  >
                    Delete Account
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
