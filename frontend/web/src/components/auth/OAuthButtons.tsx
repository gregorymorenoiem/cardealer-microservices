import { FcGoogle } from 'react-icons/fc';
import { FaMicrosoft, FaFacebookF, FaApple } from 'react-icons/fa';
import Button from '@/components/atoms/Button';

interface OAuthButtonsProps {
  onGoogleClick?: () => void;
  onMicrosoftClick?: () => void;
  onFacebookClick?: () => void;
  onAppleClick?: () => void;
  disabled?: boolean;
  showLabels?: boolean;
  // Allow showing providers without env vars (backend handles credentials)
  showGoogle?: boolean;
  showMicrosoft?: boolean;
  showFacebook?: boolean;
  showApple?: boolean;
}

// Legacy env var check - only used as fallback
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const microsoftClientId = import.meta.env.VITE_MICROSOFT_CLIENT_ID;
const facebookAppId = import.meta.env.VITE_FACEBOOK_APP_ID;
const appleClientId = import.meta.env.VITE_APPLE_CLIENT_ID;

export default function OAuthButtons({
  onGoogleClick,
  onMicrosoftClick,
  onFacebookClick,
  onAppleClick,
  disabled = false,
  showLabels = true,
  // By default, show all providers (backend handles OAuth URLs)
  showGoogle = true,
  showMicrosoft = true,
  showFacebook = true,
  showApple = true,
}: OAuthButtonsProps) {
  // Determine which providers to show based on props OR env vars
  const displayGoogle = (showGoogle || !!googleClientId) && onGoogleClick;
  const displayMicrosoft = (showMicrosoft || !!microsoftClientId) && onMicrosoftClick;
  const displayFacebook = (showFacebook || !!facebookAppId) && onFacebookClick;
  const displayApple = (showApple || !!appleClientId) && onAppleClick;

  const hasAnyProvider = displayGoogle || displayMicrosoft || displayFacebook || displayApple;

  return (
    <div className="space-y-3">
      {/* Divider */}
      <div className="relative my-6">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-gray-300"></div>
        </div>
        <div className="relative flex justify-center text-sm">
          <span className="px-4 bg-white text-gray-500">O continúa con</span>
        </div>
      </div>

      {/* Google Login */}
      {displayGoogle && (
        <Button
          variant="outline"
          fullWidth
          onClick={onGoogleClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-gray-50"
        >
          <FcGoogle size={20} />
          {showLabels && <span>Continuar con Google</span>}
        </Button>
      )}

      {/* Facebook Login */}
      {displayFacebook && (
        <Button
          variant="outline"
          fullWidth
          onClick={onFacebookClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-[#1877F2] hover:text-white hover:border-[#1877F2] group"
        >
          <FaFacebookF size={18} className="text-[#1877F2] group-hover:text-white" />
          {showLabels && <span>Continuar con Facebook</span>}
        </Button>
      )}

      {/* Apple Login */}
      {displayApple && (
        <Button
          variant="outline"
          fullWidth
          onClick={onAppleClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-black hover:text-white hover:border-black group"
        >
          <FaApple size={20} className="group-hover:text-white" />
          {showLabels && <span>Continuar con Apple</span>}
        </Button>
      )}

      {/* Microsoft Login */}
      {displayMicrosoft && (
        <Button
          variant="outline"
          fullWidth
          onClick={onMicrosoftClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-gray-50"
        >
          <FaMicrosoft size={18} className="text-[#00A4EF]" />
          {showLabels && <span>Continuar con Microsoft</span>}
        </Button>
      )}

      {/* No OAuth providers available */}
      {!hasAnyProvider && (
        <p className="text-center text-sm text-gray-500">Inicio de sesión social no disponible</p>
      )}
    </div>
  );
}
