import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { FiLock, FiCheck, FiAlertCircle, FiLoader, FiEye, FiEyeOff } from 'react-icons/fi';
import { authService } from '../../services/authService';

/**
 * SetPasswordPage (AUTH-PWD-001)
 *
 * This page is accessed via the email link sent to OAuth users
 * who need to set a password before unlinking their OAuth provider.
 *
 * URL: /auth/set-password?token=xxxxx
 */
const SetPasswordPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');

  // Token validation state
  const [isValidating, setIsValidating] = useState(true);
  const [tokenValid, setTokenValid] = useState(false);
  const [tokenInfo, setTokenInfo] = useState<{
    email?: string;
    provider?: string;
    expiresAt?: string;
  }>({});
  const [validationError, setValidationError] = useState('');

  // Password form state
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [success, setSuccess] = useState(false);

  // Password validation
  const passwordRequirements = {
    minLength: newPassword.length >= 8,
    hasUppercase: /[A-Z]/.test(newPassword),
    hasLowercase: /[a-z]/.test(newPassword),
    hasNumber: /[0-9]/.test(newPassword),
    hasSpecial: /[@$!%*?&]/.test(newPassword),
    matches: newPassword === confirmPassword && newPassword.length > 0,
  };

  const allRequirementsMet = Object.values(passwordRequirements).every(Boolean);

  // Validate token on page load
  useEffect(() => {
    const validateToken = async () => {
      if (!token) {
        setIsValidating(false);
        setValidationError('No token provided. Please use the link from your email.');
        return;
      }

      try {
        const result = await authService.validatePasswordSetupToken(token);

        if (result.isValid) {
          setTokenValid(true);
          setTokenInfo({
            email: result.email,
            provider: result.provider,
            expiresAt: result.expiresAt,
          });
        } else {
          setValidationError(result.message);
        }
      } catch (error) {
        setValidationError(
          error instanceof Error ? error.message : 'Failed to validate token. It may have expired.'
        );
      } finally {
        setIsValidating(false);
      }
    };

    validateToken();
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!allRequirementsMet || !token) return;

    setIsSubmitting(true);
    setSubmitError('');

    try {
      const result = await authService.completePasswordSetup(token, newPassword, confirmPassword);

      if (result.success) {
        setSuccess(true);
        // Redirect to security settings after 3 seconds
        setTimeout(() => {
          navigate('/user/settings/security', {
            state: {
              message: 'Password set successfully. You can now unlink your OAuth provider.',
            },
          });
        }, 3000);
      }
    } catch (error) {
      setSubmitError(
        error instanceof Error ? error.message : 'Failed to set password. Please try again.'
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  // Loading state
  if (isValidating) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <FiLoader className="animate-spin h-12 w-12 text-blue-600 mx-auto mb-4" />
          <p className="text-gray-600">Validating your link...</p>
        </div>
      </div>
    );
  }

  // Invalid token state
  if (!tokenValid) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8 text-center">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <FiAlertCircle className="h-8 w-8 text-red-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Link Expired or Invalid</h1>
          <p className="text-gray-600 mb-6">{validationError}</p>
          <button
            onClick={() => navigate('/user/settings/security')}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Go to Security Settings
          </button>
        </div>
      </div>
    );
  }

  // Success state
  if (success) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8 text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <FiCheck className="h-8 w-8 text-green-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Password Set Successfully!</h1>
          <p className="text-gray-600 mb-2">
            Your password has been configured. You can now unlink your {tokenInfo.provider} account
            if you wish.
          </p>
          <p className="text-sm text-gray-500">Redirecting to Security Settings...</p>
        </div>
      </div>
    );
  }

  // Main form
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
        {/* Header */}
        <div className="text-center mb-6">
          <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <FiLock className="h-8 w-8 text-blue-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900">Set Your Password</h1>
          <p className="text-gray-600 mt-2">
            Configure a password for your account ({tokenInfo.email})
          </p>
          {tokenInfo.provider && (
            <p className="text-sm text-gray-500 mt-1">
              Currently signed in with {tokenInfo.provider}
            </p>
          )}
        </div>

        {/* Error Alert */}
        {submitError && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
            <FiAlertCircle className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-red-700">{submitError}</p>
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* New Password */}
          <div>
            <label htmlFor="newPassword" className="block text-sm font-medium text-gray-700 mb-1">
              New Password
            </label>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                id="newPassword"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Enter your new password"
                required
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
              >
                {showPassword ? <FiEyeOff /> : <FiEye />}
              </button>
            </div>
          </div>

          {/* Confirm Password */}
          <div>
            <label
              htmlFor="confirmPassword"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Confirm Password
            </label>
            <div className="relative">
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                id="confirmPassword"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                placeholder="Confirm your new password"
                required
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-gray-700"
              >
                {showConfirmPassword ? <FiEyeOff /> : <FiEye />}
              </button>
            </div>
          </div>

          {/* Password Requirements */}
          <div className="bg-gray-50 rounded-lg p-4">
            <p className="text-sm font-medium text-gray-700 mb-2">Password Requirements:</p>
            <ul className="space-y-1">
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.minLength ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.minLength ? <FiCheck /> : <span className="w-4 h-4" />}
                At least 8 characters
              </li>
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.hasUppercase ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.hasUppercase ? <FiCheck /> : <span className="w-4 h-4" />}
                One uppercase letter
              </li>
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.hasLowercase ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.hasLowercase ? <FiCheck /> : <span className="w-4 h-4" />}
                One lowercase letter
              </li>
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.hasNumber ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.hasNumber ? <FiCheck /> : <span className="w-4 h-4" />}
                One number
              </li>
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.hasSpecial ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.hasSpecial ? <FiCheck /> : <span className="w-4 h-4" />}
                One special character (@$!%*?&)
              </li>
              <li
                className={`text-sm flex items-center gap-2 ${passwordRequirements.matches ? 'text-green-600' : 'text-gray-500'}`}
              >
                {passwordRequirements.matches ? <FiCheck /> : <span className="w-4 h-4" />}
                Passwords match
              </li>
            </ul>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={!allRequirementsMet || isSubmitting}
            className={`w-full py-3 rounded-lg font-medium transition-colors flex items-center justify-center gap-2
              ${
                allRequirementsMet && !isSubmitting
                  ? 'bg-blue-600 text-white hover:bg-blue-700'
                  : 'bg-gray-300 text-gray-500 cursor-not-allowed'
              }`}
          >
            {isSubmitting ? (
              <>
                <FiLoader className="animate-spin" />
                Setting Password...
              </>
            ) : (
              <>
                <FiLock />
                Set Password
              </>
            )}
          </button>
        </form>

        {/* Footer */}
        <p className="mt-4 text-center text-sm text-gray-500">
          After setting your password, you'll be able to log in with your email and password, or
          continue using your {tokenInfo.provider} account.
        </p>
      </div>
    </div>
  );
};

export default SetPasswordPage;
