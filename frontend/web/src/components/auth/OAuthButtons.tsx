import { FcGoogle } from 'react-icons/fc';
import { FaMicrosoft } from 'react-icons/fa';
import Button from '@/components/atoms/Button';

interface OAuthButtonsProps {
  onGoogleClick: () => void;
  onMicrosoftClick: () => void;
  disabled?: boolean;
}

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
const microsoftClientId = import.meta.env.VITE_MICROSOFT_CLIENT_ID;

export default function OAuthButtons({ onGoogleClick, onMicrosoftClick, disabled = false }: OAuthButtonsProps) {
  return (
    <div className="space-y-3">
      {/* Divider */}
      <div className="relative my-6">
        <div className="absolute inset-0 flex items-center">
          <div className="w-full border-t border-gray-300"></div>
        </div>
        <div className="relative flex justify-center text-sm">
          <span className="px-4 bg-white text-gray-500">Or continue with</span>
        </div>
      </div>

      {/* Google Login */}
      {googleClientId && (
        <Button
          variant="outline"
          fullWidth
          onClick={onGoogleClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-gray-50"
        >
          <FcGoogle size={20} />
          <span>Continue with Google</span>
        </Button>
      )}

      {/* Microsoft Login */}
      {microsoftClientId && (
        <Button
          variant="outline"
          fullWidth
          onClick={onMicrosoftClick}
          disabled={disabled}
          className="flex items-center justify-center gap-3 border-gray-300 hover:bg-gray-50"
        >
          <FaMicrosoft size={18} className="text-[#00A4EF]" />
          <span>Continue with Microsoft</span>
        </Button>
      )}
      
      {/* No OAuth providers configured */}
      {!googleClientId && !microsoftClientId && (
        <p className="text-center text-sm text-gray-500">
          OAuth login not available
        </p>
      )}
    </div>
  );
}
