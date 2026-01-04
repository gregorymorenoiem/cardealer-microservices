import { useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { authService } from '@/services/authService';
import { useAuthStore } from '@/store/authStore';

export default function OAuthCallbackPage() {
  const navigate = useNavigate();
  const { provider } = useParams<{ provider: 'google' | 'microsoft' }>();
  const [searchParams] = useSearchParams();
  const storeLogin = useAuthStore((state) => state.login);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handleCallback = async () => {
      try {
        // Get authorization code from URL
        const code = searchParams.get('code');
        const errorParam = searchParams.get('error');

        if (errorParam) {
          throw new Error(`OAuth error: ${errorParam}`);
        }

        if (!code) {
          throw new Error('No authorization code received');
        }

        if (!provider || (provider !== 'google' && provider !== 'microsoft')) {
          throw new Error('Invalid OAuth provider');
        }

        // Exchange code for tokens
        const response = await authService.handleOAuthCallback(provider, code);

        // Update auth store
        storeLogin(response);

        // Redirect to dashboard
        navigate('/dashboard', { replace: true });
      } catch (err) {
        console.error('OAuth callback error:', err);
        setError(err instanceof Error ? err.message : 'Authentication failed');
        
        // Redirect to login after 3 seconds
        setTimeout(() => {
          navigate('/login', { replace: true });
        }, 3000);
      }
    };

    handleCallback();
  }, [provider, searchParams, storeLogin, navigate]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full p-8 text-center">
        {!error ? (
          <>
            <div className="inline-block animate-spin rounded-full h-16 w-16 border-b-2 border-primary mb-4"></div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              Signing you in...
            </h2>
            <p className="text-gray-600">
              Please wait while we complete your {provider} authentication.
            </p>
          </>
        ) : (
          <>
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-100 mb-4">
              <svg
                className="w-8 h-8 text-red-600"
                fill="none"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">
              Authentication Failed
            </h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <p className="text-sm text-gray-500">
              Redirecting to login page...
            </p>
          </>
        )}
      </div>
    </div>
  );
}
