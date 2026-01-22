import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import {
  FiShield,
  FiLock,
  FiSmartphone,
  FiMonitor,
  FiMapPin,
  FiClock,
  FiAlertCircle,
  FiCheckCircle,
  FiTrash2,
  FiKey,
} from 'react-icons/fi';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const AUTH_API_URL = `${API_BASE_URL}/api/auth`;

interface ActiveSession {
  id: string;
  device: string;
  browser: string;
  location: string;
  ipAddress: string;
  lastActive: string;
  isCurrent: boolean;
}

interface LoginHistoryItem {
  id: string;
  device: string;
  browser: string;
  location: string;
  ipAddress: string;
  loginTime: string;
  success: boolean;
}

interface SecuritySettings {
  twoFactorEnabled: boolean;
  twoFactorType: string | null;
  lastPasswordChange: string;
  activeSessions: ActiveSession[];
  recentLogins: LoginHistoryItem[];
}

export default function SecuritySettingsPage() {
  const { t } = useTranslation('auth');

  const [settings, setSettings] = useState<SecuritySettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Change password state
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [showPasswords, setShowPasswords] = useState(false);

  // 2FA state
  const [show2FASetup, setShow2FASetup] = useState(false);
  const [qrCode, setQrCode] = useState<string | null>(null);
  const [manualKey, setManualKey] = useState<string | null>(null);
  const [verificationCode, setVerificationCode] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([]);
  const [is2FALoading, setIs2FALoading] = useState(false);
  const [twoFAError, setTwoFAError] = useState<string | null>(null);

  // Fetch security settings
  useEffect(() => {
    fetchSecuritySettings();
  }, []);

  const fetchSecuritySettings = async () => {
    try {
      setIsLoading(true);
      const token = localStorage.getItem('accessToken');
      const response = await axios.get<SecuritySettings>(`${AUTH_API_URL}/auth/security`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setSettings(response.data);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch security settings:', err);
      setError('Failed to load security settings');
    } finally {
      setIsLoading(false);
    }
  };

  // Change password handler
  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setPasswordError(null);
    setPasswordSuccess(false);

    if (newPassword !== confirmPassword) {
      setPasswordError('Passwords do not match');
      return;
    }

    if (newPassword.length < 8) {
      setPasswordError('Password must be at least 8 characters');
      return;
    }

    try {
      setIsChangingPassword(true);
      const token = localStorage.getItem('accessToken');
      await axios.post(
        `${AUTH_API_URL}/auth/security/change-password`,
        {
          currentPassword,
          newPassword,
          confirmPassword,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      setPasswordSuccess(true);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
      setShowChangePassword(false);
      fetchSecuritySettings();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setPasswordError(err.response.data.error || 'Failed to change password');
      } else {
        setPasswordError('Failed to change password');
      }
    } finally {
      setIsChangingPassword(false);
    }
  };

  // Enable 2FA
  const handleEnable2FA = async () => {
    try {
      setIs2FALoading(true);
      setTwoFAError(null);
      const token = localStorage.getItem('accessToken');
      const response = await axios.post(
        `${AUTH_API_URL}/twofactor/enable`,
        {
          type: 'Totp',
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      const { data } = response.data;
      setQrCode(data.qrCodeUrl);
      setManualKey(data.manualEntryKey);
      setShow2FASetup(true);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setTwoFAError(err.response.data.error || 'Failed to enable 2FA');
      } else {
        setTwoFAError('Failed to enable 2FA');
      }
    } finally {
      setIs2FALoading(false);
    }
  };

  // Verify 2FA setup
  const handleVerify2FA = async () => {
    try {
      setIs2FALoading(true);
      setTwoFAError(null);
      const token = localStorage.getItem('accessToken');
      const response = await axios.post(
        `${AUTH_API_URL}/twofactor/verify`,
        {
          code: verificationCode,
          type: 'Totp',
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      const { data } = response.data;
      setRecoveryCodes(data.recoveryCodes || []);
      fetchSecuritySettings();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setTwoFAError(err.response.data.error || 'Invalid verification code');
      } else {
        setTwoFAError('Failed to verify code');
      }
    } finally {
      setIs2FALoading(false);
    }
  };

  // Disable 2FA
  const handleDisable2FA = async () => {
    const password = prompt('Enter your password to disable 2FA:');
    if (!password) return;

    try {
      setIs2FALoading(true);
      const token = localStorage.getItem('accessToken');
      await axios.post(
        `${AUTH_API_URL}/twofactor/disable`,
        {
          password,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      fetchSecuritySettings();
      setShow2FASetup(false);
      setRecoveryCodes([]);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        alert(err.response.data.error || 'Failed to disable 2FA');
      } else {
        alert('Failed to disable 2FA');
      }
    } finally {
      setIs2FALoading(false);
    }
  };

  // Revoke session
  const handleRevokeSession = async (sessionId: string) => {
    try {
      const token = localStorage.getItem('accessToken');
      await axios.delete(`${AUTH_API_URL}/auth/security/sessions/${sessionId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchSecuritySettings();
    } catch (err) {
      console.error('Failed to revoke session:', err);
    }
  };

  // Revoke all sessions
  const handleRevokeAllSessions = async () => {
    if (!confirm('This will log you out from all other devices. Continue?')) return;

    try {
      const token = localStorage.getItem('accessToken');
      await axios.post(
        `${AUTH_API_URL}/auth/security/sessions/revoke-all`,
        {},
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      fetchSecuritySettings();
    } catch (err) {
      console.error('Failed to revoke all sessions:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getDeviceIcon = (device: string) => {
    if (device.toLowerCase().includes('mobile')) return <FiSmartphone size={20} />;
    return <FiMonitor size={20} />;
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-4xl mx-auto px-4 py-8">
          <div className="animate-pulse space-y-4">
            <div className="h-8 bg-gray-200 rounded w-1/3"></div>
            <div className="h-64 bg-gray-200 rounded"></div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <FiShield className="text-primary" />
            {t('security.title', 'Security Settings')}
          </h1>
          <p className="text-gray-600 mt-2">
            {t('security.subtitle', 'Manage your account security and active sessions')}
          </p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
            <FiAlertCircle className="text-red-600" size={20} />
            <p className="text-red-800">{error}</p>
          </div>
        )}

        {passwordSuccess && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
            <FiCheckCircle className="text-green-600" size={20} />
            <p className="text-green-800">Password changed successfully!</p>
          </div>
        )}

        <div className="space-y-6">
          {/* Password Section */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-blue-100 rounded-lg">
                  <FiLock className="text-blue-600" size={24} />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">
                    {t('security.password', 'Password')}
                  </h2>
                  <p className="text-sm text-gray-500">
                    {settings?.lastPasswordChange
                      ? `Last changed: ${formatDate(settings.lastPasswordChange)}`
                      : 'Never changed'}
                  </p>
                </div>
              </div>
              <Button variant="outline" onClick={() => setShowChangePassword(!showChangePassword)}>
                {showChangePassword ? 'Cancel' : 'Change Password'}
              </Button>
            </div>

            {showChangePassword && (
              <form onSubmit={handleChangePassword} className="mt-4 space-y-4 pt-4 border-t">
                {passwordError && (
                  <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800">
                    {passwordError}
                  </div>
                )}

                <div className="relative">
                  <Input
                    type={showPasswords ? 'text' : 'password'}
                    label="Current Password"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    required
                    fullWidth
                  />
                </div>

                <Input
                  type={showPasswords ? 'text' : 'password'}
                  label="New Password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  required
                  fullWidth
                />

                <Input
                  type={showPasswords ? 'text' : 'password'}
                  label="Confirm New Password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                  fullWidth
                />

                <label className="flex items-center gap-2 text-sm text-gray-600">
                  <input
                    type="checkbox"
                    checked={showPasswords}
                    onChange={(e) => setShowPasswords(e.target.checked)}
                    className="rounded border-gray-300"
                  />
                  Show passwords
                </label>

                <Button type="submit" variant="primary" isLoading={isChangingPassword}>
                  Change Password
                </Button>
              </form>
            )}
          </div>

          {/* Two-Factor Authentication Section */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <div
                  className={`p-3 rounded-lg ${settings?.twoFactorEnabled ? 'bg-green-100' : 'bg-gray-100'}`}
                >
                  <FiShield
                    className={settings?.twoFactorEnabled ? 'text-green-600' : 'text-gray-600'}
                    size={24}
                  />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">
                    {t('security.twoFactor', 'Two-Factor Authentication')}
                  </h2>
                  <p className="text-sm text-gray-500">
                    {settings?.twoFactorEnabled
                      ? `Enabled via ${settings.twoFactorType || 'Authenticator App'}`
                      : 'Add an extra layer of security'}
                  </p>
                </div>
              </div>
              <Button
                variant={settings?.twoFactorEnabled ? 'ghost' : 'primary'}
                onClick={settings?.twoFactorEnabled ? handleDisable2FA : handleEnable2FA}
                isLoading={is2FALoading}
              >
                {settings?.twoFactorEnabled ? 'Disable' : 'Enable'}
              </Button>
            </div>

            {twoFAError && (
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800 mb-4">
                {twoFAError}
              </div>
            )}

            {/* 2FA Setup Flow */}
            {show2FASetup && qrCode && !recoveryCodes.length && (
              <div className="mt-4 pt-4 border-t space-y-4">
                <div className="text-center">
                  <p className="text-sm text-gray-600 mb-4">
                    Scan this QR code with your authenticator app (Google Authenticator, Authy,
                    etc.)
                  </p>
                  <img src={qrCode} alt="2FA QR Code" className="mx-auto w-48 h-48" />

                  {manualKey && (
                    <div className="mt-4 p-3 bg-gray-100 rounded-lg">
                      <p className="text-xs text-gray-500 mb-1">Or enter this code manually:</p>
                      <code className="text-sm font-mono">{manualKey}</code>
                    </div>
                  )}
                </div>

                <div className="max-w-xs mx-auto">
                  <Input
                    type="text"
                    label="Verification Code"
                    placeholder="Enter 6-digit code"
                    value={verificationCode}
                    onChange={(e) =>
                      setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))
                    }
                    maxLength={6}
                    fullWidth
                  />
                  <Button
                    variant="primary"
                    fullWidth
                    className="mt-3"
                    onClick={handleVerify2FA}
                    isLoading={is2FALoading}
                    disabled={verificationCode.length !== 6}
                  >
                    Verify & Enable
                  </Button>
                </div>
              </div>
            )}

            {/* Recovery Codes Display */}
            {recoveryCodes.length > 0 && (
              <div className="mt-4 pt-4 border-t">
                <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg mb-4">
                  <div className="flex items-start gap-3">
                    <FiKey className="text-yellow-600 flex-shrink-0 mt-0.5" size={20} />
                    <div>
                      <h3 className="font-semibold text-yellow-800">Save Your Recovery Codes</h3>
                      <p className="text-sm text-yellow-700 mt-1">
                        These codes can be used to access your account if you lose your
                        authenticator. Store them in a safe place. Each code can only be used once.
                      </p>
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-2 p-4 bg-gray-100 rounded-lg font-mono text-sm">
                  {recoveryCodes.map((code, index) => (
                    <div key={index} className="p-2 bg-white rounded">
                      {code}
                    </div>
                  ))}
                </div>

                <Button
                  variant="outline"
                  className="mt-4"
                  onClick={() => {
                    navigator.clipboard.writeText(recoveryCodes.join('\n'));
                    alert('Recovery codes copied to clipboard!');
                  }}
                >
                  Copy All Codes
                </Button>

                <Button
                  variant="ghost"
                  className="mt-2 ml-2"
                  onClick={() => {
                    setRecoveryCodes([]);
                    setShow2FASetup(false);
                  }}
                >
                  I've Saved These Codes
                </Button>
              </div>
            )}
          </div>

          {/* Active Sessions Section */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-purple-100 rounded-lg">
                  <FiMonitor className="text-purple-600" size={24} />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">
                    {t('security.sessions', 'Active Sessions')}
                  </h2>
                  <p className="text-sm text-gray-500">
                    {settings?.activeSessions.length || 0} active device(s)
                  </p>
                </div>
              </div>
              {(settings?.activeSessions.length || 0) > 1 && (
                <Button
                  variant="ghost"
                  className="text-red-600 hover:text-red-700"
                  onClick={handleRevokeAllSessions}
                >
                  Log out all other devices
                </Button>
              )}
            </div>

            <div className="space-y-3">
              {settings?.activeSessions.map((session) => (
                <div
                  key={session.id}
                  className={`flex items-center justify-between p-4 rounded-lg border ${
                    session.isCurrent ? 'bg-green-50 border-green-200' : 'bg-gray-50'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div className="p-2 bg-white rounded-lg">{getDeviceIcon(session.device)}</div>
                    <div>
                      <div className="font-medium text-gray-900 flex items-center gap-2">
                        {session.device} - {session.browser}
                        {session.isCurrent && (
                          <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded-full">
                            Current
                          </span>
                        )}
                      </div>
                      <div className="text-sm text-gray-500 flex items-center gap-3">
                        <span className="flex items-center gap-1">
                          <FiMapPin size={12} /> {session.location}
                        </span>
                        <span className="flex items-center gap-1">
                          <FiClock size={12} /> {formatDate(session.lastActive)}
                        </span>
                      </div>
                    </div>
                  </div>
                  {!session.isCurrent && (
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-red-600"
                      onClick={() => handleRevokeSession(session.id)}
                    >
                      <FiTrash2 size={16} />
                    </Button>
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Login History Section */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="p-3 bg-orange-100 rounded-lg">
                <FiClock className="text-orange-600" size={24} />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  {t('security.loginHistory', 'Recent Login Activity')}
                </h2>
                <p className="text-sm text-gray-500">Your last 10 login attempts</p>
              </div>
            </div>

            <div className="space-y-2">
              {settings?.recentLogins.map((login) => (
                <div
                  key={login.id}
                  className={`flex items-center justify-between p-3 rounded-lg ${
                    login.success ? 'bg-gray-50' : 'bg-red-50'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div className={`p-2 rounded-lg ${login.success ? 'bg-white' : 'bg-red-100'}`}>
                      {login.success ? (
                        <FiCheckCircle className="text-green-600" size={18} />
                      ) : (
                        <FiAlertCircle className="text-red-600" size={18} />
                      )}
                    </div>
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {login.device} - {login.browser}
                      </div>
                      <div className="text-xs text-gray-500 flex items-center gap-2">
                        <span>{login.location}</span>
                        <span>•</span>
                        <span>{login.ipAddress}</span>
                      </div>
                    </div>
                  </div>
                  <div className="text-xs text-gray-500">{formatDate(login.loginTime)}</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
