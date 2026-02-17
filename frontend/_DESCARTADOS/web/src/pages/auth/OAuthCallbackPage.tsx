import { useEffect, useState, useRef } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { authService } from '@/services/authService';
import { useAuthStore } from '@/store/authStore';
import { FaGoogle, FaFacebook, FaApple, FaMicrosoft } from 'react-icons/fa';
import { FiCheckCircle, FiXCircle, FiLink2 } from 'react-icons/fi';

type OAuthProvider = 'google' | 'microsoft' | 'facebook' | 'apple';

export default function OAuthCallbackPage() {
  const navigate = useNavigate();
  const { provider } = useParams<{ provider: OAuthProvider }>();
  const [searchParams] = useSearchParams();
  const storeLogin = useAuthStore((state) => state.login);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isLinking, setIsLinking] = useState(false);
  const hasProcessed = useRef(false);

  // Get provider icon
  const getProviderIcon = (p: string | undefined) => {
    const iconProps = { size: 32, className: 'text-white' };
    switch (p) {
      case 'google':
        return <FaGoogle {...iconProps} />;
      case 'microsoft':
        return <FaMicrosoft {...iconProps} />;
      case 'facebook':
        return <FaFacebook {...iconProps} />;
      case 'apple':
        return <FaApple {...iconProps} />;
      default:
        return <FiLink2 {...iconProps} />;
    }
  };

  // Get provider background color
  const getProviderBg = (p: string | undefined) => {
    switch (p) {
      case 'google':
        return 'bg-red-500';
      case 'microsoft':
        return 'bg-blue-500';
      case 'facebook':
        return 'bg-blue-600';
      case 'apple':
        return 'bg-gray-900';
      default:
        return 'bg-indigo-600';
    }
  };

  useEffect(() => {
    // Prevent double-processing (React StrictMode or re-renders)
    if (hasProcessed.current) return;
    hasProcessed.current = true;

    const handleCallback = async () => {
      try {
        // Get authorization code from URL
        const code = searchParams.get('code');
        const idToken = searchParams.get('id_token'); // For Apple
        const errorParam = searchParams.get('error');
        const errorDescription = searchParams.get('error_description');

        if (errorParam) {
          throw new Error(errorDescription || `OAuth error: ${errorParam}`);
        }

        if (!code && !idToken) {
          throw new Error('No authorization code received from provider');
        }

        const validProviders: OAuthProvider[] = ['google', 'microsoft', 'facebook', 'apple'];
        if (!provider || !validProviders.includes(provider)) {
          throw new Error('Invalid OAuth provider');
        }

        // Check if this is a link operation (user is already logged in)
        const oauthMode = sessionStorage.getItem('oauth_mode');
        sessionStorage.removeItem('oauth_mode');

        if (oauthMode === 'link' && isAuthenticated) {
          setIsLinking(true);

          // Link the account to existing user
          const result = await authService.linkExternalAccount(provider, code || idToken || '');

          // Show success briefly, then redirect with success param
          setSuccess(`${getProviderName(provider)} account linked successfully!`);

          setTimeout(() => {
            navigate('/user/security?linked=' + provider, { replace: true });
          }, 1500);
        } else {
          // Normal login flow - exchange code for tokens
          const response = await authService.handleOAuthCallback(provider, code || '', idToken);

          // Update auth store
          storeLogin(response);

          // Redirect to dashboard
          navigate('/dashboard', { replace: true });
        }
      } catch (err) {
        console.error('OAuth callback error:', err);
        const errorMessage = err instanceof Error ? err.message : 'Authentication failed';
        setError(errorMessage);

        // Check if this was a link operation
        const wasLinking = sessionStorage.getItem('oauth_mode') === 'link' || isAuthenticated;
        sessionStorage.removeItem('oauth_mode');

        // Redirect based on context
        setTimeout(() => {
          if (wasLinking || isAuthenticated) {
            navigate('/user/security?linkError=' + encodeURIComponent(errorMessage), {
              replace: true,
            });
          } else {
            navigate('/login?error=' + encodeURIComponent(errorMessage), { replace: true });
          }
        }, 2500);
      }
    };

    handleCallback();
  }, [provider, searchParams, storeLogin, navigate, isAuthenticated]);

  const getProviderName = (p: string | undefined): string => {
    switch (p) {
      case 'google':
        return 'Google';
      case 'microsoft':
        return 'Microsoft';
      case 'facebook':
        return 'Facebook';
      case 'apple':
        return 'Apple';
      default:
        return 'OAuth';
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100">
      <div className="max-w-md w-full mx-4">
        <div className="bg-white rounded-2xl shadow-xl overflow-hidden">
          {/* Header with provider branding */}
          <div className={`${getProviderBg(provider)} p-6 text-center`}>
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-white/20 backdrop-blur-sm mb-3">
              {getProviderIcon(provider)}
            </div>
            <h2 className="text-xl font-bold text-white">{getProviderName(provider)}</h2>
          </div>

          {/* Content */}
          <div className="p-8 text-center">
            {/* Loading State */}
            {!error && !success && (
              <>
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-gray-200 border-t-indigo-600 mb-4"></div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {isLinking ? 'Linking your account...' : 'Signing you in...'}
                </h3>
                <p className="text-gray-500 text-sm">
                  {isLinking
                    ? `Connecting your ${getProviderName(provider)} account`
                    : `Please wait while we complete your ${getProviderName(provider)} authentication`}
                </p>
              </>
            )}

            {/* Success State */}
            {success && (
              <>
                <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-green-100 mb-4">
                  <FiCheckCircle className="w-8 h-8 text-green-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">Account Linked!</h3>
                <p className="text-gray-500 text-sm mb-4">{success}</p>
                <div className="flex items-center justify-center gap-2 text-sm text-gray-400">
                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-gray-300 border-t-indigo-600"></div>
                  Redirecting to settings...
                </div>
              </>
            )}

            {/* Error State */}
            {error && (
              <>
                <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-100 mb-4">
                  <FiXCircle className="w-8 h-8 text-red-600" />
                </div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  {isLinking ? 'Linking Failed' : 'Authentication Failed'}
                </h3>
                <p className="text-red-600 text-sm mb-4 bg-red-50 p-3 rounded-lg">{error}</p>
                <div className="flex items-center justify-center gap-2 text-sm text-gray-400">
                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-gray-300 border-t-indigo-600"></div>
                  Redirecting...
                </div>
              </>
            )}
          </div>

          {/* Footer hint */}
          <div className="px-8 pb-6 text-center">
            <p className="text-xs text-gray-400">
              {isLinking
                ? 'You will be able to use this account for faster login'
                : 'You will be redirected automatically'}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
