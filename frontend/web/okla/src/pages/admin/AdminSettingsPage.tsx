import { useState, useEffect } from 'react';
import { FiSave, FiAlertCircle, FiCheck } from 'react-icons/fi';
import { getSystemSettings, updateSystemSettings, type SystemSettings } from '@/services/adminService';

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState<SystemSettings>({
    maintenanceMode: false,
    allowRegistrations: true,
    requireEmailVerification: true,
    requireListingApproval: true,
    maxListingsPerUser: 10,
    maxImagesPerListing: 10,
    featuredListingPrice: 49.99,
    commissionRate: 5,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  useEffect(() => {
    loadSettings();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadSettings = async () => {
    try {
      setIsLoading(true);
      const data = await getSystemSettings();
      setSettings(data);
    } catch {
      showMessage('error', 'Failed to load settings');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setIsSaving(true);
      await updateSystemSettings(settings);
      showMessage('success', 'Settings saved successfully');
    } catch {
      showMessage('error', 'Failed to save settings');
    } finally {
      setIsSaving(false);
    }
  };

  const showMessage = (type: 'success' | 'error', text: string) => {
    setMessage({ type, text });
    setTimeout(() => setMessage(null), 5000);
  };

  const handleToggle = (key: keyof SystemSettings) => {
    setSettings((prev) => ({
      ...prev,
      [key]: !prev[key],
    }));
  };

  const handleNumberChange = (key: keyof SystemSettings, value: number) => {
    setSettings((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
            System Settings
          </h1>
          <p className="text-gray-600">Configure platform-wide settings</p>
        </div>
        <button
          onClick={handleSave}
          disabled={isSaving}
          className="px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors disabled:opacity-50 flex items-center gap-2"
        >
          <FiSave size={18} />
          {isSaving ? 'Saving...' : 'Save Changes'}
        </button>
      </div>

      {/* Message */}
      {message && (
        <div
          className={`mb-6 p-4 rounded-lg flex items-center gap-2 ${
            message.type === 'success'
              ? 'bg-green-50 border border-green-200 text-green-800'
              : 'bg-red-50 border border-red-200 text-red-800'
          }`}
        >
          {message.type === 'success' ? <FiCheck /> : <FiAlertCircle />}
          <span>{message.text}</span>
        </div>
      )}

      {/* Settings Sections */}
      <div className="space-y-6">
        {/* General Settings */}
        <div className="bg-white rounded-xl shadow-card p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4">General Settings</h2>
          <div className="space-y-4">
            <label className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
              <div>
                <div className="font-medium text-gray-900">Maintenance Mode</div>
                <div className="text-sm text-gray-600">
                  Disable public access to the platform for maintenance
                </div>
              </div>
              <button
                type="button"
                onClick={() => handleToggle('maintenanceMode')}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  settings.maintenanceMode ? 'bg-red-600' : 'bg-gray-300'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    settings.maintenanceMode ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </label>

            <label className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
              <div>
                <div className="font-medium text-gray-900">Allow Registrations</div>
                <div className="text-sm text-gray-600">
                  Allow new users to register on the platform
                </div>
              </div>
              <button
                type="button"
                onClick={() => handleToggle('allowRegistrations')}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  settings.allowRegistrations ? 'bg-primary' : 'bg-gray-300'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    settings.allowRegistrations ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </label>

            <label className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
              <div>
                <div className="font-medium text-gray-900">Require Email Verification</div>
                <div className="text-sm text-gray-600">
                  Users must verify their email before accessing full features
                </div>
              </div>
              <button
                type="button"
                onClick={() => handleToggle('requireEmailVerification')}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  settings.requireEmailVerification ? 'bg-primary' : 'bg-gray-300'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    settings.requireEmailVerification ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </label>
          </div>
        </div>

        {/* Listing Settings */}
        <div className="bg-white rounded-xl shadow-card p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Listing Settings</h2>
          <div className="space-y-4">
            <label className="flex items-center justify-between p-4 border border-gray-200 rounded-lg cursor-pointer hover:bg-gray-50">
              <div>
                <div className="font-medium text-gray-900">Require Listing Approval</div>
                <div className="text-sm text-gray-600">
                  New listings must be approved by admin before going live
                </div>
              </div>
              <button
                type="button"
                onClick={() => handleToggle('requireListingApproval')}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                  settings.requireListingApproval ? 'bg-primary' : 'bg-gray-300'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                    settings.requireListingApproval ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </label>

            <div className="p-4 border border-gray-200 rounded-lg">
              <label className="block">
                <div className="font-medium text-gray-900 mb-2">Max Listings Per User</div>
                <input
                  type="number"
                  min="1"
                  max="100"
                  value={settings.maxListingsPerUser}
                  onChange={(e) => handleNumberChange('maxListingsPerUser', parseInt(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                />
                <p className="text-sm text-gray-600 mt-1">
                  Maximum number of active listings a user can have
                </p>
              </label>
            </div>

            <div className="p-4 border border-gray-200 rounded-lg">
              <label className="block">
                <div className="font-medium text-gray-900 mb-2">Max Images Per Listing</div>
                <input
                  type="number"
                  min="1"
                  max="20"
                  value={settings.maxImagesPerListing}
                  onChange={(e) => handleNumberChange('maxImagesPerListing', parseInt(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                />
                <p className="text-sm text-gray-600 mt-1">
                  Maximum number of images per vehicle listing
                </p>
              </label>
            </div>
          </div>
        </div>

        {/* Pricing Settings */}
        <div className="bg-white rounded-xl shadow-card p-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Pricing Settings</h2>
          <div className="space-y-4">
            <div className="p-4 border border-gray-200 rounded-lg">
              <label className="block">
                <div className="font-medium text-gray-900 mb-2">Featured Listing Price (USD)</div>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={settings.featuredListingPrice}
                  onChange={(e) => handleNumberChange('featuredListingPrice', parseFloat(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                />
                <p className="text-sm text-gray-600 mt-1">
                  Price to make a listing featured for 30 days
                </p>
              </label>
            </div>

            <div className="p-4 border border-gray-200 rounded-lg">
              <label className="block">
                <div className="font-medium text-gray-900 mb-2">Commission Rate (%)</div>
                <input
                  type="number"
                  min="0"
                  max="100"
                  step="0.1"
                  value={settings.commissionRate}
                  onChange={(e) => handleNumberChange('commissionRate', parseFloat(e.target.value))}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                />
                <p className="text-sm text-gray-600 mt-1">
                  Platform commission on successful sales
                </p>
              </label>
            </div>
          </div>
        </div>

        {/* Danger Zone */}
        <div className="bg-red-50 border border-red-200 rounded-xl p-6">
          <h2 className="text-xl font-bold text-red-900 mb-4">Danger Zone</h2>
          <div className="space-y-4">
            <div className="flex items-start justify-between">
              <div>
                <div className="font-medium text-red-900">Clear All Cache</div>
                <div className="text-sm text-red-700">
                  Clear all cached data. This may temporarily slow down the platform.
                </div>
              </div>
              <button className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors">
                Clear Cache
              </button>
            </div>
            <div className="flex items-start justify-between">
              <div>
                <div className="font-medium text-red-900">Reset All Statistics</div>
                <div className="text-sm text-red-700">
                  Reset all platform statistics. This action cannot be undone.
                </div>
              </div>
              <button className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors">
                Reset Stats
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
