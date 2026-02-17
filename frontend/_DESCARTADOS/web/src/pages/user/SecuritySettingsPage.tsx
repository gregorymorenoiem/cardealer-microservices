import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams, useNavigate } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import { ConfirmDialog } from '@/components/common';
import UnlinkActiveProviderModal from '@/components/modals/UnlinkActiveProviderModal';
import { kycService, type KYCProfile, KYCStatus } from '@/services/kycService';
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
  FiPhone,
  FiLink,
  FiLink2,
  FiAlertTriangle,
  FiLogOut,
  FiGlobe,
  FiTablet,
  FiUser,
  FiCamera,
  FiFileText,
} from 'react-icons/fi';
import {
  FaGoogle,
  FaFacebook,
  FaApple,
  FaMicrosoft,
  FaChrome,
  FaFirefox,
  FaSafari,
  FaEdge,
} from 'react-icons/fa';
import axios from 'axios';
import { authService } from '@/services/authService';
import {
  securitySessionService,
  type ActiveSessionDto,
  type RevokeSessionResponse,
  type RevokeAllSessionsResponse,
} from '@/services/securitySessionService';
import type { LinkedAccount } from '@/services/authService';
import { useAuthStore } from '@/store/authStore';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
const AUTH_API_URL = `${API_BASE_URL}/api/auth`;
const TWOFACTOR_API_URL = `${API_BASE_URL}/api/twofactor`;
const PHONE_API_URL = `${API_BASE_URL}/api/phoneverification`;

// 2FA Types matching backend enum
enum TwoFactorType {
  Authenticator = 1,
  SMS = 2,
  Email = 3,
}

interface SecuritySettings {
  twoFactorEnabled: boolean;
  twoFactorType: number | null;
  phoneNumber: string | null;
  phoneNumberConfirmed: boolean;
  lastPasswordChange: string;
  recentLogins: LoginHistoryItem[];
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

export default function SecuritySettingsPage() {
  const { t } = useTranslation('auth');
  const [searchParams, setSearchParams] = useSearchParams();

  const [settings, setSettings] = useState<SecuritySettings | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Active sessions state (using new service)
  const [activeSessions, setActiveSessions] = useState<ActiveSessionDto[]>([]);
  const [sessionsLoading, setSessionsLoading] = useState(false);
  const [sessionsError, setSessionsError] = useState<string | null>(null);
  const [sessionSuccess, setSessionSuccess] = useState<string | null>(null);
  const [revokingSessionId, setRevokingSessionId] = useState<string | null>(null);
  const [isRevokingAll, setIsRevokingAll] = useState(false);

  // Session revocation with code verification
  const [showRevokeModal, setShowRevokeModal] = useState(false);
  const [revokeTargetSession, setRevokeTargetSession] = useState<ActiveSessionDto | null>(null);
  const [revokeStep, setRevokeStep] = useState<'confirm' | 'code' | 'success'>('confirm');
  const [verificationCode, setVerificationCode] = useState('');
  const [codeExpiresAt, setCodeExpiresAt] = useState<Date | null>(null);
  const [remainingAttempts, setRemainingAttempts] = useState<number>(3);
  const [isRequestingCode, setIsRequestingCode] = useState(false);
  const [isVerifyingCode, setIsVerifyingCode] = useState(false);

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
  const [show2FAOptions, setShow2FAOptions] = useState(false);
  const [show2FASetup, setShow2FASetup] = useState(false);
  const [qrCodeUrl, setQrCodeUrl] = useState<string | null>(null);
  const [manualKey, setManualKey] = useState<string | null>(null);
  const [twoFACode, setTwoFACode] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([]);
  const [is2FALoading, setIs2FALoading] = useState(false);
  const [twoFAError, setTwoFAError] = useState<string | null>(null);
  const [twoFASuccess, setTwoFASuccess] = useState<string | null>(null);

  // Phone verification state
  const [showPhoneVerification, setShowPhoneVerification] = useState(false);
  const [phoneNumber, setPhoneNumber] = useState('');
  const [phoneCode, setPhoneCode] = useState('');
  const [isPhoneLoading, setIsPhoneLoading] = useState(false);
  const [phoneSent, setPhoneSent] = useState(false);
  const [phoneError, setPhoneError] = useState<string | null>(null);

  // Linked accounts state
  const [linkedAccounts, setLinkedAccounts] = useState<LinkedAccount[]>([]);
  const [isLinkedAccountsLoading, setIsLinkedAccountsLoading] = useState(false);
  const [linkedAccountsError, setLinkedAccountsError] = useState<string | null>(null);

  // KYC/Identity Verification state
  const [kycProfile, setKycProfile] = useState<KYCProfile | null>(null);
  const [isKycLoading, setIsKycLoading] = useState(false);
  const navigate = useNavigate();

  // Get user from auth store
  const user = useAuthStore((state) => state.user);

  // Fetch security settings and sessions
  useEffect(() => {
    fetchSecuritySettings();
    fetchActiveSessions();
    fetchLinkedAccounts();
  }, []);

  // Fetch KYC status when user changes
  useEffect(() => {
    fetchKycStatus();
  }, [user]);

  // Fetch active sessions using new service
  const fetchActiveSessions = async () => {
    try {
      setSessionsLoading(true);
      setSessionsError(null);
      const response = await securitySessionService.getActiveSessions();
      setActiveSessions(response.sessions || []);
    } catch (err) {
      // Error handled silently - UI shows error state
      setSessionsError(err instanceof Error ? err.message : 'Failed to load sessions');
      setActiveSessions([]);
    } finally {
      setSessionsLoading(false);
    }
  };

  // Fetch linked accounts
  const fetchLinkedAccounts = async () => {
    try {
      setIsLinkedAccountsLoading(true);
      const accounts = await authService.getLinkedAccounts();
      setLinkedAccounts(accounts);
      setLinkedAccountsError(null);
    } catch (err) {
      // Error handled silently - UI shows error state
      setLinkedAccountsError('Failed to load linked accounts');
    } finally {
      setIsLinkedAccountsLoading(false);
    }
  };

  // Fetch KYC/Identity Verification status
  const fetchKycStatus = async () => {
    try {
      setIsKycLoading(true);
      if (user?.id) {
        const profile = await kycService.getProfileByUserId(user.id);
        setKycProfile(profile);
      } else {
        setKycProfile(null);
      }
    } catch {
      // 404 means no KYC profile yet - that's OK
      setKycProfile(null);
    } finally {
      setIsKycLoading(false);
    }
  };

  // Get KYC status display info
  const getKycStatusInfo = () => {
    if (!kycProfile) {
      return {
        status: 'not_started',
        label: 'No verificado',
        description: 'Completa la verificación de identidad para acceder a todas las funciones',
        color: 'gray',
        icon: FiAlertTriangle,
        action: 'Iniciar Verificación',
        actionRoute: '/kyc/biometric-verify',
      };
    }

    switch (kycProfile.status) {
      case KYCStatus.Approved:
        return {
          status: 'verified',
          label: '✅ Verificado',
          description: `Verificado el ${new Date(kycProfile.approvedAt || '').toLocaleDateString()}`,
          color: 'green',
          icon: FiCheckCircle,
          action: 'Ver Estado',
          actionRoute: '/kyc/status',
        };
      case KYCStatus.InProgress:
      case KYCStatus.PendingReview:
      case KYCStatus.UnderReview:
        return {
          status: 'pending',
          label: '⏳ En Revisión',
          description: 'Tu verificación está siendo procesada',
          color: 'yellow',
          icon: FiClock,
          action: 'Ver Estado',
          actionRoute: '/kyc/status',
        };
      case KYCStatus.Rejected:
        return {
          status: 'rejected',
          label: '❌ Rechazado',
          description:
            kycProfile.rejectionReason ||
            'Tu verificación fue rechazada. Por favor intenta de nuevo.',
          color: 'red',
          icon: FiAlertCircle,
          action: 'Reintentar',
          actionRoute: '/kyc/biometric-verify',
        };
      case KYCStatus.Expired:
        return {
          status: 'expired',
          label: '⏰ Expirado',
          description: 'Tu verificación ha expirado. Por favor verifica de nuevo.',
          color: 'orange',
          icon: FiClock,
          action: 'Renovar',
          actionRoute: '/kyc/biometric-verify',
        };
      case KYCStatus.Suspended:
        return {
          status: 'suspended',
          label: '⛔ Suspendido',
          description: 'Tu verificación está suspendida. Contacta soporte para más información.',
          color: 'red',
          icon: FiAlertCircle,
          action: 'Contactar Soporte',
          actionRoute: '/support',
        };
      default:
        return {
          status: 'incomplete',
          label: 'Incompleto',
          description: 'Continúa tu proceso de verificación',
          color: 'orange',
          icon: FiAlertTriangle,
          action: 'Continuar',
          actionRoute: '/kyc/biometric-verify',
        };
    }
  };

  // State for linked accounts success message
  const [linkedAccountsSuccess, setLinkedAccountsSuccess] = useState<string | null>(null);
  const [unlinkingProvider, setUnlinkingProvider] = useState<string | null>(null);
  const [linkingProvider, setLinkingProvider] = useState<string | null>(null);

  // Unlink confirmation modal state (simple unlink - AUTH-EXT-006)
  const [showUnlinkModal, setShowUnlinkModal] = useState(false);
  const [providerToUnlink, setProviderToUnlink] = useState<string | null>(null);

  // Active provider unlink modal state (AUTH-EXT-008)
  const [showActiveProviderUnlinkModal, setShowActiveProviderUnlinkModal] = useState(false);
  const [activeProviderToUnlink, setActiveProviderToUnlink] = useState<string | null>(null);

  // Handle URL params from OAuth callback (success/error messages)
  useEffect(() => {
    const linked = searchParams.get('linked');
    const linkError = searchParams.get('linkError');

    if (linked) {
      const providerName = linked.charAt(0).toUpperCase() + linked.slice(1);
      setLinkedAccountsSuccess(
        `✓ ${providerName} account linked successfully! You can now use it to sign in faster.`
      );
      fetchLinkedAccounts(); // Refresh the list

      // Clear the URL param
      setSearchParams({}, { replace: true });

      // Auto-clear success message after 8 seconds
      setTimeout(() => setLinkedAccountsSuccess(null), 8000);
    }

    if (linkError) {
      setLinkedAccountsError(decodeURIComponent(linkError));

      // Clear the URL param
      setSearchParams({}, { replace: true });

      // Auto-clear error after 10 seconds
      setTimeout(() => setLinkedAccountsError(null), 10000);
    }
  }, [searchParams, setSearchParams]);

  // Open unlink confirmation modal - validates first to determine which flow to use
  const openUnlinkModal = async (provider: string) => {
    try {
      setLinkedAccountsError(null);
      setUnlinkingProvider(provider); // Show loading state on button

      // Validate unlink request (AUTH-EXT-008)
      const validation = await authService.validateUnlinkAccount(provider);

      if (!validation.canUnlink && !validation.requiresEmailVerification) {
        // Cannot unlink at all (e.g., only one provider and no password)
        setLinkedAccountsError(validation.message);
        return;
      }

      if (!validation.hasPassword || validation.requiresEmailVerification) {
        // Use active provider flow with verification
        setActiveProviderToUnlink(provider);
        setShowActiveProviderUnlinkModal(true);
      } else {
        // Simple unlink flow (has password, not active provider)
        setProviderToUnlink(provider);
        setShowUnlinkModal(true);
      }
    } catch (err) {
      setLinkedAccountsError(
        err instanceof Error ? err.message : 'Failed to validate unlink request'
      );
    } finally {
      setUnlinkingProvider(null);
    }
  };

  // Close unlink confirmation modal (simple flow)
  const closeUnlinkModal = () => {
    setShowUnlinkModal(false);
    setProviderToUnlink(null);
  };

  // Close active provider unlink modal
  const closeActiveProviderUnlinkModal = () => {
    setShowActiveProviderUnlinkModal(false);
    setActiveProviderToUnlink(null);
  };

  // Handle successful unlink from active provider modal
  const handleActiveProviderUnlinkSuccess = () => {
    closeActiveProviderUnlinkModal();
    fetchLinkedAccounts();
    setLinkedAccountsSuccess(
      'Account unlinked successfully. You have been logged out of all sessions.'
    );
  };

  // Handle unlinking an external account (AUTH-EXT-006)
  const handleUnlinkAccount = async () => {
    if (!providerToUnlink) return;

    const provider = providerToUnlink;
    closeUnlinkModal();

    try {
      setUnlinkingProvider(provider);
      setLinkedAccountsError(null);
      setLinkedAccountsSuccess(null);

      const result = await authService.unlinkExternalAccount(provider);

      setLinkedAccountsSuccess(
        `✓ ${result.provider} account unlinked successfully.\n` +
          `You can now only sign in with your email and password.`
      );

      await fetchLinkedAccounts();
    } catch (err) {
      if (err instanceof Error) {
        setLinkedAccountsError(err.message);
      } else {
        setLinkedAccountsError('Failed to unlink account. Please try again.');
      }
    } finally {
      setUnlinkingProvider(null);
    }
  };

  // Handle linking a new external account (AUTH-EXT-005)
  const handleLinkAccount = async (provider: 'google' | 'microsoft' | 'facebook' | 'apple') => {
    try {
      setLinkingProvider(provider);
      setLinkedAccountsError(null);
      setLinkedAccountsSuccess(null);

      // This will redirect to OAuth provider
      await authService.startLinkAccount(provider);

      // Note: The actual linking happens in the callback page
      // Success/error will be shown when user returns
    } catch (err) {
      if (err instanceof Error) {
        setLinkedAccountsError(err.message);
      } else {
        setLinkedAccountsError(`Failed to initiate ${provider} linking. Please try again.`);
      }
      setLinkingProvider(null);
    }
  };

  // Get provider icon
  const getProviderIcon = (provider: string) => {
    switch (provider.toLowerCase()) {
      case 'google':
        return <FaGoogle className="text-red-500" size={20} />;
      case 'facebook':
        return <FaFacebook className="text-blue-600" size={20} />;
      case 'apple':
        return <FaApple className="text-gray-900" size={20} />;
      case 'microsoft':
        return <FaMicrosoft className="text-blue-500" size={20} />;
      default:
        return <FiLink className="text-gray-600" size={20} />;
    }
  };

  // Check if a provider is already linked
  const isProviderLinked = (provider: string) => {
    return linkedAccounts.some(
      (account) => account.provider.toLowerCase() === provider.toLowerCase()
    );
  };

  const fetchSecuritySettings = async () => {
    try {
      setIsLoading(true);
      const token = localStorage.getItem('accessToken');

      // Fetch security settings only (phone verification service not implemented yet)
      const securityResponse = await axios
        .get(`${AUTH_API_URL}/security`, {
          headers: { Authorization: `Bearer ${token}` },
        })
        .catch(() => ({ data: null }));

      setSettings({
        twoFactorEnabled: securityResponse.data?.twoFactorEnabled || false,
        twoFactorType: securityResponse.data?.twoFactorType || null,
        phoneNumber: null, // Phone verification not implemented yet
        phoneNumberConfirmed: false,
        lastPasswordChange: securityResponse.data?.lastPasswordChange || '',
        recentLogins: securityResponse.data?.recentLogins || [],
      });
      setError(null);
    } catch (err) {
      // Error handled silently - set default values if API fails
      setSettings({
        twoFactorEnabled: false,
        twoFactorType: null,
        phoneNumber: null,
        phoneNumberConfirmed: false,
        lastPasswordChange: '',
        recentLogins: [],
      });
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
        `${AUTH_API_URL}/security/change-password`,
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

  // Send phone verification code
  const handleSendPhoneCode = async () => {
    if (!phoneNumber.trim()) {
      setPhoneError('Please enter a phone number');
      return;
    }

    try {
      setIsPhoneLoading(true);
      setPhoneError(null);
      const token = localStorage.getItem('accessToken');

      await axios.post(
        `${PHONE_API_URL}/send`,
        { phoneNumber: phoneNumber.startsWith('+') ? phoneNumber : `+1${phoneNumber}` },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setPhoneSent(true);
      setTwoFASuccess('Verification code sent to your phone!');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setPhoneError(err.response.data.error || 'Failed to send verification code');
      } else {
        setPhoneError('Failed to send verification code');
      }
    } finally {
      setIsPhoneLoading(false);
    }
  };

  // Verify phone code
  const handleVerifyPhone = async () => {
    if (!phoneCode.trim()) {
      setPhoneError('Please enter the verification code');
      return;
    }

    try {
      setIsPhoneLoading(true);
      setPhoneError(null);
      const token = localStorage.getItem('accessToken');

      await axios.post(
        `${PHONE_API_URL}/verify`,
        { code: phoneCode },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setTwoFASuccess('Phone verified successfully! Now enabling SMS 2FA...');
      setShowPhoneVerification(false);
      setPhoneSent(false);
      setPhoneCode('');
      setPhoneNumber('');

      // Refresh settings and proceed to enable SMS 2FA
      await fetchSecuritySettings();

      // Now enable SMS 2FA
      handleEnable2FA(TwoFactorType.SMS);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setPhoneError(err.response.data.error || 'Invalid verification code');
      } else {
        setPhoneError('Failed to verify code');
      }
    } finally {
      setIsPhoneLoading(false);
    }
  };

  // Handle 2FA type selection
  const handle2FATypeSelect = (type: TwoFactorType) => {
    setTwoFAError(null);
    setTwoFASuccess(null);

    if (type === TwoFactorType.SMS && !settings?.phoneNumberConfirmed) {
      // Need to verify phone first
      setShowPhoneVerification(true);
      setShow2FAOptions(false);
    } else {
      // Can proceed to enable
      handleEnable2FA(type);
    }
  };

  // Enable 2FA
  const handleEnable2FA = async (type: TwoFactorType) => {
    try {
      setIs2FALoading(true);
      setTwoFAError(null);
      const token = localStorage.getItem('accessToken');
      const response = await axios.post(
        `${TWOFACTOR_API_URL}/enable`,
        { type },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      const { data } = response.data;

      if (type === TwoFactorType.Authenticator) {
        // For Authenticator, generate QR code URL using external service
        const secret = data.secret;
        const email = localStorage.getItem('userEmail') || 'user@okla.com.do';
        const otpauthUrl = `otpauth://totp/OKLA:${encodeURIComponent(email)}?secret=${secret}&issuer=OKLA`;
        const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(otpauthUrl)}`;

        setQrCodeUrl(qrUrl);
        setManualKey(secret);
        setShow2FASetup(true);
        setShow2FAOptions(false);
      } else if (type === TwoFactorType.SMS) {
        // For SMS, 2FA is enabled immediately (code will be sent on login)
        setTwoFASuccess(
          'SMS 2FA enabled successfully! You will receive a code via SMS when logging in.'
        );
        setShow2FAOptions(false);
        fetchSecuritySettings();
      }

      // Store recovery codes if provided
      if (data.recoveryCodes && data.recoveryCodes.length > 0) {
        setRecoveryCodes(data.recoveryCodes);
      }
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

  // Verify 2FA setup (for Authenticator)
  const handleVerify2FA = async () => {
    try {
      setIs2FALoading(true);
      setTwoFAError(null);
      const token = localStorage.getItem('accessToken');
      const response = await axios.post(
        `${TWOFACTOR_API_URL}/verify`,
        {
          code: twoFACode,
          type: TwoFactorType.Authenticator,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      const { data } = response.data;
      // If verify returns new recovery codes, update them
      if (data?.recoveryCodes && data.recoveryCodes.length > 0) {
        setRecoveryCodes(data.recoveryCodes);
      }
      setTwoFASuccess('Google Authenticator 2FA enabled successfully!');
      setShow2FASetup(false);
      setTwoFACode('');
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
        `${TWOFACTOR_API_URL}/disable`,
        {
          password,
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      setTwoFASuccess('2FA has been disabled.');
      fetchSecuritySettings();
      setShow2FASetup(false);
      setRecoveryCodes([]);
      setQrCodeUrl(null);
      setManualKey(null);
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

  // Regenerate recovery codes
  const handleRegenerateRecoveryCodes = async () => {
    const password = prompt('Enter your password to generate new recovery codes:');
    if (!password) return;

    try {
      setIs2FALoading(true);
      setTwoFAError(null);
      const token = localStorage.getItem('accessToken');
      const response = await axios.post(
        `${TWOFACTOR_API_URL}/generate-recovery-codes`,
        { password },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      const { data } = response.data;
      if (data?.recoveryCodes && data.recoveryCodes.length > 0) {
        setRecoveryCodes(data.recoveryCodes);
        setTwoFASuccess('New recovery codes generated! Please save them securely.');
      }
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        setTwoFAError(err.response.data.error || 'Failed to generate recovery codes');
      } else {
        setTwoFAError('Failed to generate recovery codes');
      }
    } finally {
      setIs2FALoading(false);
    }
  };

  // Open revoke session modal - AUTH-SEC-003
  const handleOpenRevokeModal = (session: ActiveSessionDto) => {
    if (session.isCurrent) {
      setSessionsError('No puedes terminar tu sesión actual. Usa el botón de cerrar sesión.');
      return;
    }
    setRevokeTargetSession(session);
    setRevokeStep('confirm');
    setVerificationCode('');
    setCodeExpiresAt(null);
    setRemainingAttempts(3);
    setSessionsError(null);
    setShowRevokeModal(true);
  };

  // Request verification code for session revocation
  const handleRequestRevocationCode = async () => {
    if (!revokeTargetSession) return;

    try {
      setIsRequestingCode(true);
      setSessionsError(null);

      const response = await securitySessionService.requestSessionRevocation(
        revokeTargetSession.id
      );

      if (response.success) {
        setRevokeStep('code');
        if (response.codeExpiresAt) {
          setCodeExpiresAt(new Date(response.codeExpiresAt));
        }
        if (response.remainingAttempts) {
          setRemainingAttempts(response.remainingAttempts);
        }
        setSessionSuccess('Se ha enviado un código de verificación a tu correo electrónico.');
      } else {
        setSessionsError(response.message);
      }
    } catch (err) {
      setSessionsError(
        err instanceof Error ? err.message : 'Error al solicitar código de verificación'
      );
    } finally {
      setIsRequestingCode(false);
    }
  };

  // Verify code and revoke session
  const handleVerifyAndRevoke = async () => {
    if (!revokeTargetSession || !verificationCode) return;

    if (verificationCode.length !== 6 || !/^\d+$/.test(verificationCode)) {
      setSessionsError('El código debe ser de 6 dígitos numéricos.');
      return;
    }

    try {
      setIsVerifyingCode(true);
      setSessionsError(null);

      const response = await securitySessionService.revokeSession(
        revokeTargetSession.id,
        verificationCode
      );

      if (response.success) {
        setRevokeStep('success');
        setSessionSuccess('Sesión terminada exitosamente. El dispositivo ha sido desconectado.');

        // Refresh the sessions list after a short delay
        setTimeout(async () => {
          await fetchActiveSessions();
          setShowRevokeModal(false);
          setRevokeTargetSession(null);
        }, 2000);
      } else {
        if (response.remainingAttempts !== undefined) {
          setRemainingAttempts(response.remainingAttempts);
        }
        setSessionsError(response.message);
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al verificar código';
      setSessionsError(errorMessage);

      // Check if it mentions remaining attempts
      if (errorMessage.includes('Invalid verification code')) {
        setRemainingAttempts((prev) => Math.max(0, prev - 1));
      }
    } finally {
      setIsVerifyingCode(false);
    }
  };

  // Close revoke modal
  const handleCloseRevokeModal = () => {
    setShowRevokeModal(false);
    setRevokeTargetSession(null);
    setVerificationCode('');
    setCodeExpiresAt(null);
    setRevokeStep('confirm');
  };

  // Legacy handler (keeping for backwards compatibility with UI that might call it directly)
  const handleRevokeSession = async (sessionId: string) => {
    const session = activeSessions.find((s) => s.id === sessionId);
    if (session) {
      handleOpenRevokeModal(session);
    }
  };

  // Revoke all sessions - AUTH-SEC-004
  const handleRevokeAllSessions = async (keepCurrent: boolean = true) => {
    const message = keepCurrent
      ? 'This will log you out from all other devices. Continue?'
      : 'This will log you out from ALL devices including this one. You will need to log in again. Continue?';

    if (!confirm(message)) return;

    try {
      setIsRevokingAll(true);
      setSessionsError(null);
      setSessionSuccess(null);

      const response = await securitySessionService.revokeAllSessions(keepCurrent);

      if (response.success) {
        const successMsg = [
          `✓ ${response.sessionsRevoked} session${response.sessionsRevoked !== 1 ? 's' : ''} terminated`,
          response.refreshTokensRevoked > 0
            ? `✓ ${response.refreshTokensRevoked} refresh token${response.refreshTokensRevoked !== 1 ? 's' : ''} revoked`
            : '',
          response.securityAlertSent ? '✓ Security alert email sent' : '',
          response.currentSessionKept ? '✓ Current session kept active' : '',
        ]
          .filter(Boolean)
          .join('\n');

        setSessionSuccess(successMsg);

        // Refresh the sessions list
        await fetchActiveSessions();

        // If current session was NOT kept, log out
        if (!response.currentSessionKept) {
          setTimeout(() => {
            authService.logout();
            window.location.href = '/auth/login';
          }, 3000);
        }
      }
    } catch (err) {
      // Error handled via toast notification
      setSessionsError(err instanceof Error ? err.message : 'Failed to revoke all sessions');
    } finally {
      setIsRevokingAll(false);
    }
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return 'Never';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) return 'Recently';
      return date.toLocaleString();
    } catch {
      return 'Recently';
    }
  };

  // Get device icon based on device string
  const getDeviceIcon = (device: string) => {
    const deviceLower = device?.toLowerCase() || '';
    if (deviceLower.includes('mobile') || deviceLower.includes('phone')) {
      return <FiSmartphone size={20} className="text-green-600" />;
    }
    if (deviceLower.includes('tablet') || deviceLower.includes('ipad')) {
      return <FiTablet size={20} className="text-blue-600" />;
    }
    return <FiMonitor size={20} className="text-purple-600" />;
  };

  // Get browser icon
  const getBrowserIcon = (browser: string) => {
    const browserLower = browser?.toLowerCase() || '';
    if (browserLower.includes('chrome')) return <FaChrome size={16} className="text-yellow-500" />;
    if (browserLower.includes('firefox'))
      return <FaFirefox size={16} className="text-orange-500" />;
    if (browserLower.includes('safari')) return <FaSafari size={16} className="text-blue-500" />;
    if (browserLower.includes('edge')) return <FaEdge size={16} className="text-blue-600" />;
    return <FiGlobe size={16} className="text-gray-500" />;
  };

  const get2FATypeName = (type: number | null) => {
    switch (type) {
      case TwoFactorType.Authenticator:
        return 'Google Authenticator';
      case TwoFactorType.SMS:
        return 'SMS';
      case TwoFactorType.Email:
        return 'Email';
      default:
        return 'Unknown';
    }
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

        {twoFASuccess && (
          <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
            <FiCheckCircle className="text-green-600" size={20} />
            <p className="text-green-800">{twoFASuccess}</p>
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
                      ? `✅ Enabled via ${get2FATypeName(settings.twoFactorType)}`
                      : 'Add an extra layer of security'}
                  </p>
                </div>
              </div>
              {settings?.twoFactorEnabled ? (
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleRegenerateRecoveryCodes}
                    isLoading={is2FALoading}
                    className="text-amber-600 border-amber-300 hover:bg-amber-50"
                  >
                    <FiKey className="mr-1" size={14} />
                    New Recovery Codes
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-600"
                    onClick={handleDisable2FA}
                    isLoading={is2FALoading}
                  >
                    Disable 2FA
                  </Button>
                </div>
              ) : (
                <Button
                  variant="primary"
                  onClick={() => {
                    setShow2FAOptions(true);
                    setTwoFAError(null);
                    setTwoFASuccess(null);
                  }}
                  isLoading={is2FALoading}
                >
                  Enable 2FA
                </Button>
              )}
            </div>

            {twoFAError && (
              <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800 mb-4">
                {twoFAError}
              </div>
            )}

            {/* 2FA Method Selection */}
            {show2FAOptions && !settings?.twoFactorEnabled && (
              <div className="mt-4 pt-4 border-t">
                <h3 className="font-medium text-gray-900 mb-4">Choose your 2FA method:</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Google Authenticator Option */}
                  <button
                    onClick={() => handle2FATypeSelect(TwoFactorType.Authenticator)}
                    className="p-4 border-2 rounded-xl hover:border-primary hover:bg-primary/5 transition-all text-left"
                  >
                    <div className="flex items-center gap-3 mb-2">
                      <div className="p-2 bg-blue-100 rounded-lg">
                        <FiKey className="text-blue-600" size={24} />
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-900">Google Authenticator</h4>
                        <span className="text-xs text-green-600 font-medium">Recommended</span>
                      </div>
                    </div>
                    <p className="text-sm text-gray-600">
                      Use an authenticator app (Google Authenticator, Authy, 1Password) to generate
                      time-based codes. More secure and works offline.
                    </p>
                  </button>

                  {/* SMS Option */}
                  <button
                    onClick={() => handle2FATypeSelect(TwoFactorType.SMS)}
                    className="p-4 border-2 rounded-xl hover:border-primary hover:bg-primary/5 transition-all text-left"
                  >
                    <div className="flex items-center gap-3 mb-2">
                      <div className="p-2 bg-green-100 rounded-lg">
                        <FiSmartphone className="text-green-600" size={24} />
                      </div>
                      <div>
                        <h4 className="font-semibold text-gray-900">SMS</h4>
                        {settings?.phoneNumberConfirmed ? (
                          <span className="text-xs text-green-600 font-medium">
                            ✅ Phone verified: {settings.phoneNumber}
                          </span>
                        ) : (
                          <span className="text-xs text-amber-600 font-medium">
                            ⚠️ Phone verification required
                          </span>
                        )}
                      </div>
                    </div>
                    <p className="text-sm text-gray-600">
                      Receive a code via SMS to your phone number. Requires phone verification
                      first.
                    </p>
                  </button>
                </div>

                <Button variant="ghost" className="mt-4" onClick={() => setShow2FAOptions(false)}>
                  Cancel
                </Button>
              </div>
            )}

            {/* Phone Verification Flow */}
            {showPhoneVerification && (
              <div className="mt-4 pt-4 border-t">
                <div className="p-4 bg-amber-50 border border-amber-200 rounded-lg mb-4">
                  <div className="flex items-start gap-3">
                    <FiPhone className="text-amber-600 flex-shrink-0 mt-0.5" size={20} />
                    <div>
                      <h3 className="font-semibold text-amber-800">Phone Verification Required</h3>
                      <p className="text-sm text-amber-700 mt-1">
                        To use SMS 2FA, you must first verify your phone number.
                      </p>
                    </div>
                  </div>
                </div>

                {phoneError && (
                  <div className="p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-800 mb-4">
                    {phoneError}
                  </div>
                )}

                {!phoneSent ? (
                  <div className="space-y-4">
                    <Input
                      type="tel"
                      label="Phone Number"
                      placeholder="+1 (829) 830-2434"
                      value={phoneNumber}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                      fullWidth
                    />
                    <p className="text-xs text-gray-500">
                      Enter your phone number with country code (e.g., +18298302434)
                    </p>
                    <div className="flex gap-2">
                      <Button
                        variant="primary"
                        onClick={handleSendPhoneCode}
                        isLoading={isPhoneLoading}
                      >
                        Send Verification Code
                      </Button>
                      <Button
                        variant="ghost"
                        onClick={() => {
                          setShowPhoneVerification(false);
                          setShow2FAOptions(true);
                        }}
                      >
                        Back
                      </Button>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-4">
                    <p className="text-sm text-gray-600">
                      Enter the 6-digit code sent to <strong>{phoneNumber}</strong>
                    </p>
                    <Input
                      type="text"
                      label="Verification Code"
                      placeholder="Enter 6-digit code"
                      value={phoneCode}
                      onChange={(e) => setPhoneCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                      maxLength={6}
                      fullWidth
                    />
                    <div className="flex gap-2">
                      <Button
                        variant="primary"
                        onClick={handleVerifyPhone}
                        isLoading={isPhoneLoading}
                        disabled={phoneCode.length !== 6}
                      >
                        Verify Phone & Enable SMS 2FA
                      </Button>
                      <Button
                        variant="ghost"
                        onClick={() => {
                          setPhoneSent(false);
                          setPhoneCode('');
                        }}
                      >
                        Resend Code
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Authenticator Setup Flow */}
            {show2FASetup && qrCodeUrl && !recoveryCodes.length && (
              <div className="mt-4 pt-4 border-t space-y-4">
                <div className="text-center">
                  <p className="text-sm text-gray-600 mb-4">
                    Scan this QR code with your authenticator app (Google Authenticator, Authy,
                    1Password, etc.)
                  </p>
                  <div className="inline-block p-4 bg-white border-2 rounded-xl">
                    <img
                      src={qrCodeUrl}
                      alt="2FA QR Code"
                      className="w-48 h-48 mx-auto"
                      onError={(e) => {
                        // Fallback if QR service fails
                        e.currentTarget.style.display = 'none';
                      }}
                    />
                  </div>

                  {manualKey && (
                    <div className="mt-4 p-4 bg-gray-100 rounded-lg max-w-md mx-auto">
                      <p className="text-xs text-gray-500 mb-2">
                        Can't scan? Enter this code manually:
                      </p>
                      <code className="text-sm font-mono font-bold text-gray-800 break-all">
                        {manualKey}
                      </code>
                      <button
                        onClick={() => {
                          navigator.clipboard.writeText(manualKey);
                          alert('Secret key copied to clipboard!');
                        }}
                        className="mt-2 text-xs text-primary hover:underline block mx-auto"
                      >
                        Copy to clipboard
                      </button>
                    </div>
                  )}
                </div>

                <div className="max-w-xs mx-auto">
                  <Input
                    type="text"
                    label="Enter 6-digit code from your app"
                    placeholder="000000"
                    value={twoFACode}
                    onChange={(e) => setTwoFACode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    maxLength={6}
                    fullWidth
                  />
                  <Button
                    variant="primary"
                    fullWidth
                    className="mt-3"
                    onClick={handleVerify2FA}
                    isLoading={is2FALoading}
                    disabled={twoFACode.length !== 6}
                  >
                    Verify & Enable
                  </Button>
                  <Button
                    variant="ghost"
                    fullWidth
                    className="mt-2"
                    onClick={() => {
                      setShow2FASetup(false);
                      setShow2FAOptions(true);
                      setQrCodeUrl(null);
                      setManualKey(null);
                    }}
                  >
                    Cancel
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
                    <div key={index} className="p-2 bg-white rounded text-center font-bold">
                      {code}
                    </div>
                  ))}
                </div>

                <div className="flex gap-2 mt-4">
                  <Button
                    variant="outline"
                    onClick={() => {
                      navigator.clipboard.writeText(recoveryCodes.join('\n'));
                      alert('Recovery codes copied to clipboard!');
                    }}
                  >
                    Copy All Codes
                  </Button>

                  <Button
                    variant="primary"
                    onClick={() => {
                      setRecoveryCodes([]);
                      setShow2FASetup(false);
                    }}
                  >
                    I've Saved These Codes
                  </Button>
                </div>
              </div>
            )}
          </div>

          {/* Identity Verification Section (KYC) */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-3">
                <div
                  className={`p-3 rounded-lg ${
                    getKycStatusInfo().color === 'green'
                      ? 'bg-green-100'
                      : getKycStatusInfo().color === 'yellow'
                        ? 'bg-yellow-100'
                        : getKycStatusInfo().color === 'red'
                          ? 'bg-red-100'
                          : getKycStatusInfo().color === 'orange'
                            ? 'bg-orange-100'
                            : 'bg-gray-100'
                  }`}
                >
                  <FiUser
                    className={`${
                      getKycStatusInfo().color === 'green'
                        ? 'text-green-600'
                        : getKycStatusInfo().color === 'yellow'
                          ? 'text-yellow-600'
                          : getKycStatusInfo().color === 'red'
                            ? 'text-red-600'
                            : getKycStatusInfo().color === 'orange'
                              ? 'text-orange-600'
                              : 'text-gray-600'
                    }`}
                    size={24}
                  />
                </div>
                <div>
                  <h2 className="text-lg font-semibold text-gray-900">
                    {t('security.identityVerification', 'Verificación de Identidad')}
                  </h2>
                  <p className="text-sm text-gray-500">
                    {isKycLoading ? 'Cargando...' : getKycStatusInfo().label}
                  </p>
                </div>
              </div>
              <Button
                variant={getKycStatusInfo().status === 'verified' ? 'outline' : 'primary'}
                onClick={() => navigate(getKycStatusInfo().actionRoute)}
                isLoading={isKycLoading}
              >
                {getKycStatusInfo().status === 'verified' ? (
                  <>
                    <FiFileText className="mr-1" size={16} />
                    {getKycStatusInfo().action}
                  </>
                ) : (
                  <>
                    <FiCamera className="mr-1" size={16} />
                    {getKycStatusInfo().action}
                  </>
                )}
              </Button>
            </div>

            {/* KYC Status Details */}
            <div
              className={`p-4 rounded-lg ${
                getKycStatusInfo().color === 'green'
                  ? 'bg-green-50 border border-green-200'
                  : getKycStatusInfo().color === 'yellow'
                    ? 'bg-yellow-50 border border-yellow-200'
                    : getKycStatusInfo().color === 'red'
                      ? 'bg-red-50 border border-red-200'
                      : getKycStatusInfo().color === 'orange'
                        ? 'bg-orange-50 border border-orange-200'
                        : 'bg-gray-50 border border-gray-200'
              }`}
            >
              <div className="flex items-start gap-3">
                {(() => {
                  const StatusIcon = getKycStatusInfo().icon;
                  return (
                    <StatusIcon
                      className={`flex-shrink-0 mt-0.5 ${
                        getKycStatusInfo().color === 'green'
                          ? 'text-green-600'
                          : getKycStatusInfo().color === 'yellow'
                            ? 'text-yellow-600'
                            : getKycStatusInfo().color === 'red'
                              ? 'text-red-600'
                              : getKycStatusInfo().color === 'orange'
                                ? 'text-orange-600'
                                : 'text-gray-600'
                      }`}
                      size={20}
                    />
                  );
                })()}
                <div>
                  <p
                    className={`text-sm ${
                      getKycStatusInfo().color === 'green'
                        ? 'text-green-800'
                        : getKycStatusInfo().color === 'yellow'
                          ? 'text-yellow-800'
                          : getKycStatusInfo().color === 'red'
                            ? 'text-red-800'
                            : getKycStatusInfo().color === 'orange'
                              ? 'text-orange-800'
                              : 'text-gray-700'
                    }`}
                  >
                    {getKycStatusInfo().description}
                  </p>
                  {getKycStatusInfo().status === 'not_started' && (
                    <div className="mt-3 space-y-2">
                      <p className="text-xs text-gray-600 font-medium">
                        Para verificar tu identidad necesitas:
                      </p>
                      <ul className="text-xs text-gray-500 space-y-1">
                        <li className="flex items-center gap-2">
                          <FiFileText size={12} className="text-gray-400" />
                          Cédula dominicana (frente y reverso)
                        </li>
                        <li className="flex items-center gap-2">
                          <FiCamera size={12} className="text-gray-400" />
                          Una selfie clara con buena iluminación
                        </li>
                        <li className="flex items-center gap-2">
                          <FiSmartphone size={12} className="text-gray-400" />
                          Cámara web o teléfono móvil
                        </li>
                      </ul>
                    </div>
                  )}
                  {kycProfile && getKycStatusInfo().status === 'verified' && (
                    <div className="mt-3 text-xs text-green-600">
                      <p>✓ Documento de identidad verificado</p>
                      <p>✓ Verificación biométrica completada</p>
                      <p>✓ Validación con JCE aprobada</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Active Sessions Section - Enhanced */}
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
                    {sessionsLoading ? 'Loading...' : `${activeSessions.length} active device(s)`}
                  </p>
                </div>
              </div>
              {activeSessions.length > 1 && (
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    className="text-amber-600 border-amber-300 hover:bg-amber-50"
                    onClick={() => handleRevokeAllSessions(true)}
                    isLoading={isRevokingAll}
                    disabled={isRevokingAll}
                  >
                    <FiLogOut className="mr-1" size={14} />
                    Log out other devices
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-600 hover:bg-red-50"
                    onClick={() => handleRevokeAllSessions(false)}
                    isLoading={isRevokingAll}
                    disabled={isRevokingAll}
                  >
                    Log out ALL
                  </Button>
                </div>
              )}
            </div>

            {/* Session Success Message */}
            {sessionSuccess && (
              <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-800 flex items-start gap-2">
                <FiCheckCircle className="mt-0.5 flex-shrink-0" size={16} />
                <pre className="whitespace-pre-wrap font-sans">{sessionSuccess}</pre>
              </div>
            )}

            {/* Session Error Message */}
            {sessionsError && (
              <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-600 flex items-center gap-2">
                <FiAlertCircle size={16} />
                {sessionsError}
              </div>
            )}

            {/* Sessions List */}
            <div className="space-y-3">
              {sessionsLoading ? (
                <div className="flex justify-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600"></div>
                </div>
              ) : activeSessions.length > 0 ? (
                activeSessions.map((session) => (
                  <div
                    key={session.id}
                    className={`flex items-center justify-between p-4 rounded-lg border transition-all ${
                      session.isCurrent
                        ? 'bg-green-50 border-green-200 ring-2 ring-green-100'
                        : session.isExpiringSoon
                          ? 'bg-amber-50 border-amber-200'
                          : 'bg-gray-50 hover:bg-gray-100'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      <div
                        className={`p-2 rounded-lg ${session.isCurrent ? 'bg-green-100' : 'bg-white'}`}
                      >
                        {getDeviceIcon(session.device)}
                      </div>
                      <div>
                        <div className="font-medium text-gray-900 flex items-center gap-2 flex-wrap">
                          <span className="flex items-center gap-1">
                            {session.device}
                            {getBrowserIcon(session.browser)}
                            <span className="text-gray-600 text-sm">{session.browser}</span>
                          </span>
                          {session.isCurrent && (
                            <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded-full font-semibold">
                              ✓ This device
                            </span>
                          )}
                          {session.isExpiringSoon && !session.isCurrent && (
                            <span className="px-2 py-0.5 text-xs bg-amber-100 text-amber-700 rounded-full flex items-center gap-1">
                              <FiAlertTriangle size={10} />
                              Expiring soon
                            </span>
                          )}
                        </div>
                        <div className="text-sm text-gray-500 flex items-center gap-3 mt-1 flex-wrap">
                          <span className="flex items-center gap-1">
                            <FiMapPin size={12} /> {session.location}
                          </span>
                          <span className="flex items-center gap-1 text-gray-400">
                            IP: {session.ipAddress}
                          </span>
                          <span className="flex items-center gap-1">
                            <FiClock size={12} />
                            {securitySessionService.formatRelativeTime(session.lastActive)}
                          </span>
                        </div>
                        {session.operatingSystem && (
                          <div className="text-xs text-gray-400 mt-1">
                            OS: {session.operatingSystem}
                          </div>
                        )}
                      </div>
                    </div>
                    {!session.isCurrent && (
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-red-600 hover:bg-red-50"
                        onClick={() => handleRevokeSession(session.id)}
                        isLoading={revokingSessionId === session.id}
                        disabled={revokingSessionId !== null}
                      >
                        <FiTrash2 size={16} />
                        <span className="ml-1 hidden sm:inline">Terminate</span>
                      </Button>
                    )}
                  </div>
                ))
              ) : (
                <div className="text-center py-8">
                  <FiMonitor className="mx-auto text-gray-300 mb-2" size={48} />
                  <p className="text-gray-500">No active sessions found</p>
                  <Button
                    variant="outline"
                    size="sm"
                    className="mt-3"
                    onClick={fetchActiveSessions}
                  >
                    Refresh
                  </Button>
                </div>
              )}
            </div>

            {/* Session Security Tips */}
            {activeSessions.length > 0 && (
              <div className="mt-4 pt-4 border-t">
                <div className="flex items-start gap-2 text-xs text-gray-500">
                  <FiShield className="flex-shrink-0 mt-0.5" size={14} />
                  <p>
                    <strong>Security tip:</strong> If you don't recognize a session, terminate it
                    immediately and consider changing your password. IP addresses are partially
                    masked for your privacy.
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Session Revocation Modal */}
          {showRevokeModal && revokeTargetSession && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
              <div className="bg-white rounded-xl shadow-2xl max-w-md w-full p-6 animate-in zoom-in-95 duration-200">
                {revokeStep === 'confirm' && (
                  <>
                    <div className="flex items-center gap-3 mb-4">
                      <div className="p-3 bg-red-100 rounded-full">
                        <FiAlertTriangle className="text-red-600" size={24} />
                      </div>
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">Terminar Sesión</h3>
                        <p className="text-sm text-gray-500">Se requiere verificación</p>
                      </div>
                    </div>

                    <div className="bg-gray-50 rounded-lg p-4 mb-4">
                      <p className="text-sm text-gray-700 mb-2">
                        <strong>Dispositivo a desconectar:</strong>
                      </p>
                      <div className="text-sm text-gray-600 space-y-1">
                        <p>📱 {revokeTargetSession.device}</p>
                        <p>
                          🌐 {revokeTargetSession.browser} • {revokeTargetSession.operatingSystem}
                        </p>
                        <p>📍 {revokeTargetSession.location}</p>
                        <p>
                          🕐 Última actividad:{' '}
                          {securitySessionService.formatRelativeTime(
                            revokeTargetSession.lastActive
                          )}
                        </p>
                      </div>
                    </div>

                    <p className="text-sm text-gray-600 mb-4">
                      Para mayor seguridad, enviaremos un código de verificación a tu correo
                      electrónico. Ingresa el código para confirmar la terminación de esta sesión.
                    </p>

                    <div className="flex gap-3">
                      <Button variant="outline" onClick={handleCloseRevokeModal} className="flex-1">
                        Cancelar
                      </Button>
                      <Button
                        variant="primary"
                        onClick={handleRequestRevocationCode}
                        isLoading={isRequestingCode}
                        className="flex-1 bg-red-600 hover:bg-red-700"
                      >
                        Enviar Código
                      </Button>
                    </div>
                  </>
                )}

                {revokeStep === 'code' && (
                  <>
                    <div className="flex items-center gap-3 mb-4">
                      <div className="p-3 bg-blue-100 rounded-full">
                        <FiKey className="text-blue-600" size={24} />
                      </div>
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900">Verificar Código</h3>
                        <p className="text-sm text-gray-500">Revisa tu correo electrónico</p>
                      </div>
                    </div>

                    <p className="text-sm text-gray-600 mb-4">
                      Hemos enviado un código de 6 dígitos a tu correo. Ingrésalo a continuación
                      para confirmar.
                    </p>

                    {codeExpiresAt && (
                      <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4 text-sm">
                        <p className="text-amber-800">⏱️ El código expira en 5 minutos</p>
                      </div>
                    )}

                    <div className="mb-4">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Código de verificación
                      </label>
                      <Input
                        type="text"
                        value={verificationCode}
                        onChange={(e) =>
                          setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))
                        }
                        placeholder="123456"
                        className="text-center text-2xl tracking-widest font-mono"
                        maxLength={6}
                        autoFocus
                      />
                      {remainingAttempts < 3 && (
                        <p className="text-sm text-red-600 mt-2">
                          ⚠️ Intentos restantes: {remainingAttempts}
                        </p>
                      )}
                    </div>

                    {sessionsError && (
                      <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4 text-sm text-red-700">
                        {sessionsError}
                      </div>
                    )}

                    <div className="flex gap-3">
                      <Button variant="outline" onClick={handleCloseRevokeModal} className="flex-1">
                        Cancelar
                      </Button>
                      <Button
                        variant="primary"
                        onClick={handleVerifyAndRevoke}
                        isLoading={isVerifyingCode}
                        disabled={verificationCode.length !== 6}
                        className="flex-1 bg-red-600 hover:bg-red-700"
                      >
                        Terminar Sesión
                      </Button>
                    </div>

                    <button
                      onClick={handleRequestRevocationCode}
                      disabled={isRequestingCode}
                      className="w-full mt-3 text-sm text-blue-600 hover:text-blue-800 disabled:opacity-50"
                    >
                      ¿No recibiste el código? Enviar de nuevo
                    </button>
                  </>
                )}

                {revokeStep === 'success' && (
                  <div className="text-center py-4">
                    <div className="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mb-4">
                      <FiCheckCircle className="text-green-600" size={32} />
                    </div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-2">¡Sesión Terminada!</h3>
                    <p className="text-sm text-gray-600">
                      El dispositivo ha sido desconectado exitosamente. Se ha enviado una
                      notificación a tu correo.
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* Linked Accounts Section */}
          <div className="bg-white rounded-xl shadow-sm border p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="p-3 bg-indigo-100 rounded-lg">
                <FiLink2 className="text-indigo-600" size={24} />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  {t('security.linkedAccounts', 'Linked Accounts')}
                </h2>
                <p className="text-sm text-gray-500">
                  Connect your account with external providers for easier login
                </p>
              </div>
            </div>

            {/* Success Message */}
            {linkedAccountsSuccess && (
              <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg flex items-start gap-3 animate-in slide-in-from-top-2 duration-300">
                <div className="p-1.5 bg-green-100 rounded-full">
                  <FiCheckCircle className="text-green-600" size={18} />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-green-800 mb-1">
                    Account Linked Successfully
                  </p>
                  <p className="text-sm text-green-700">{linkedAccountsSuccess}</p>
                </div>
                <button
                  onClick={() => setLinkedAccountsSuccess(null)}
                  className="text-green-600 hover:text-green-800 p-1"
                  aria-label="Dismiss"
                >
                  ×
                </button>
              </div>
            )}

            {/* Error Message */}
            {linkedAccountsError && (
              <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3 animate-in slide-in-from-top-2 duration-300">
                <div className="p-1.5 bg-red-100 rounded-full">
                  <FiAlertCircle className="text-red-600" size={18} />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-red-800 mb-1">Failed to Link Account</p>
                  <p className="text-sm text-red-700">{linkedAccountsError}</p>
                </div>
                <button
                  onClick={() => setLinkedAccountsError(null)}
                  className="text-red-600 hover:text-red-800 p-1"
                  aria-label="Dismiss"
                >
                  ×
                </button>
              </div>
            )}

            {isLinkedAccountsLoading ? (
              <div className="flex justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
              </div>
            ) : (
              <div className="space-y-4">
                {/* Currently linked accounts */}
                {linkedAccounts.length > 0 && (
                  <div className="space-y-3 mb-6">
                    <h3 className="text-sm font-medium text-gray-700">Connected Accounts</h3>
                    {linkedAccounts.map((account) => (
                      <div
                        key={account.provider}
                        className={`flex items-center justify-between p-4 rounded-lg border transition-all ${
                          unlinkingProvider === account.provider
                            ? 'bg-red-50 border-red-200'
                            : 'bg-green-50 border-green-200 hover:bg-green-100'
                        }`}
                      >
                        <div className="flex items-center gap-3">
                          <div className="p-2 bg-white rounded-lg shadow-sm">
                            {getProviderIcon(account.provider)}
                          </div>
                          <div>
                            <div className="font-medium text-gray-900 capitalize flex items-center gap-2">
                              {account.provider}
                              <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded-full">
                                ✓ Connected
                              </span>
                            </div>
                            <div className="text-sm text-gray-500">{account.email}</div>
                            <div className="text-xs text-gray-400">
                              Linked on {new Date(account.linkedAt).toLocaleDateString()}
                            </div>
                          </div>
                        </div>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                          onClick={() => openUnlinkModal(account.provider)}
                          disabled={unlinkingProvider !== null}
                          isLoading={unlinkingProvider === account.provider}
                        >
                          {unlinkingProvider !== account.provider && (
                            <FiTrash2 size={16} className="mr-1" />
                          )}
                          {unlinkingProvider === account.provider ? 'Unlinking...' : 'Unlink'}
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Unlink Account Confirmation Modal (Simple flow - AUTH-EXT-006) */}
      <ConfirmDialog
        isOpen={showUnlinkModal}
        onClose={closeUnlinkModal}
        onConfirm={handleUnlinkAccount}
        title={`Unlink ${providerToUnlink ? providerToUnlink.charAt(0).toUpperCase() + providerToUnlink.slice(1) : ''} Account`}
        message={
          <div className="space-y-3">
            <p className="text-sm text-gray-600">
              Are you sure you want to unlink your{' '}
              <strong className="capitalize">{providerToUnlink}</strong> account?
            </p>
            <div className="flex items-start gap-2 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
              <FiAlertTriangle className="text-yellow-600 flex-shrink-0 mt-0.5" size={16} />
              <div className="text-xs text-yellow-800">
                <p className="font-medium mb-1">Before you proceed:</p>
                <ul className="list-disc list-inside space-y-1 text-yellow-700">
                  <li>You will no longer be able to sign in with {providerToUnlink}</li>
                  <li>Make sure you have set a password for your account</li>
                  <li>This action cannot be undone automatically</li>
                </ul>
              </div>
            </div>
          </div>
        }
        confirmText="Unlink Account"
        cancelText="Keep Connected"
        variant="danger"
        isLoading={unlinkingProvider !== null}
        infoText="You can always link this account again from this settings page."
      />

      {/* Active Provider Unlink Modal (AUTH-EXT-008) */}
      <UnlinkActiveProviderModal
        isOpen={showActiveProviderUnlinkModal}
        onClose={closeActiveProviderUnlinkModal}
        provider={activeProviderToUnlink || ''}
        onSuccess={handleActiveProviderUnlinkSuccess}
      />
    </MainLayout>
  );
}
