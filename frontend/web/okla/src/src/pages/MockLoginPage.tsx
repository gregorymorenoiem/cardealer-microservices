import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';
import { MOCK_USERS, AVAILABLE_MOCK_USERS } from '../mocks/mockUsers';
import { User, Lock, ChevronDown } from 'lucide-react';

/**
 * Página de Login con Mock Users para testing de planes
 */
export const MockLoginPage = () => {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const [selectedEmail, setSelectedEmail] = useState('dealer.free@cardealer.com');
  const [showDropdown, setShowDropdown] = useState(false);

  const handleLogin = () => {
    const user = Object.values(MOCK_USERS).find((u) => u.email === selectedEmail);
    
    if (user) {
      login({
        user,
        accessToken: 'mock-access-token-' + Date.now(),
        refreshToken: 'mock-refresh-token-' + Date.now(),
      });

      // Redirigir según el tipo de usuario
      if (user.accountType === 'admin') {
        navigate('/admin');
      } else if (user.accountType === 'dealer' || user.accountType === 'dealer_employee') {
        navigate('/dealer');
      } else {
        navigate('/dashboard');
      }
    }
  };

  const selectedMockUser = AVAILABLE_MOCK_USERS.find((u) => u.email === selectedEmail);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 flex items-center justify-center p-4">
      <div className="max-w-md w-full">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-600 rounded-full mb-4">
            <Lock className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            Mock Login - Testing Plans
          </h1>
          <p className="text-gray-600">
            Select a user to test different subscription plans
          </p>
        </div>

        {/* Demo Credentials */}
        <div className="bg-white rounded-xl shadow-lg p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            🔑 Demo Credentials by Plan:
          </h2>
          
          {/* FREE Plan Users */}
          <div className="mb-4 pb-4 border-b border-gray-100">
            <div className="flex items-center gap-2 mb-2">
              <span className="px-2 py-0.5 bg-gray-100 text-gray-700 rounded text-xs font-medium">
                FREE
              </span>
              <h3 className="text-sm font-semibold text-gray-700">Free Plan Users</h3>
            </div>
            <div className="space-y-1.5 text-sm ml-4">
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.free@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-gray-500">→ 2/3 listings (normal usage)</p>
              </div>
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.freenearlimit@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-red-600 font-medium">→ 3/3 AT LIMIT (test limit banner)</p>
              </div>
            </div>
          </div>

          {/* BASIC Plan Users */}
          <div className="mb-4 pb-4 border-b border-gray-100">
            <div className="flex items-center gap-2 mb-2">
              <span className="px-2 py-0.5 bg-blue-100 text-blue-700 rounded text-xs font-medium">
                BASIC
              </span>
              <h3 className="text-sm font-semibold text-gray-700">Basic Plan Users</h3>
            </div>
            <div className="space-y-1.5 text-sm ml-4">
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.basic@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-gray-500">→ 35/50 listings (normal usage)</p>
              </div>
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.basicnearlimit@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-yellow-600 font-medium">→ 48/50 NEAR LIMIT (test warnings)</p>
              </div>
            </div>
          </div>

          {/* PRO Plan User */}
          <div className="mb-4 pb-4 border-b border-gray-100">
            <div className="flex items-center gap-2 mb-2">
              <span className="px-2 py-0.5 bg-purple-100 text-purple-700 rounded text-xs font-medium">
                PRO
              </span>
              <h3 className="text-sm font-semibold text-gray-700">Pro Plan User</h3>
            </div>
            <div className="space-y-1.5 text-sm ml-4">
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.pro@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-gray-500">→ 120/200 listings (normal usage)</p>
              </div>
            </div>
          </div>

          {/* ENTERPRISE Plan User */}
          <div className="mb-4 pb-4 border-b border-gray-100">
            <div className="flex items-center gap-2 mb-2">
              <span className="px-2 py-0.5 bg-indigo-100 text-indigo-700 rounded text-xs font-medium">
                ENTERPRISE
              </span>
              <h3 className="text-sm font-semibold text-gray-700">Enterprise Plan User</h3>
            </div>
            <div className="space-y-1.5 text-sm ml-4">
              <div>
                <p className="font-mono text-xs">
                  <strong>dealer.enterprise@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-gray-500">→ 550 listings (unlimited)</p>
              </div>
            </div>
          </div>

          {/* Other Users */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Other User Types:</h3>
            <div className="space-y-1.5 text-sm ml-4">
              <div>
                <p className="font-mono text-xs">
                  <strong>individual@cardealer.com</strong> / password123
                </p>
                <p className="text-xs text-gray-500">→ Individual Seller (no dealer features)</p>
              </div>
              <div>
                <p className="font-mono text-xs">
                  <strong>admin@cardealer.com</strong> / admin123
                </p>
                <p className="text-xs text-gray-500">→ Platform Admin (full access)</p>
              </div>
            </div>
          </div>

          <div className="mt-4 pt-4 border-t border-gray-100 flex items-start gap-2">
            <span className="text-lg">💡</span>
            <p className="text-xs text-gray-600">
              <strong>Password:</strong> All dealers use "password123", admin uses "admin123"
            </p>
          </div>
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-xl shadow-lg p-8">
          {/* User Selector */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Select Test User
            </label>
            <div className="relative">
              <button
                onClick={() => setShowDropdown(!showDropdown)}
                className="w-full flex items-center justify-between px-4 py-3 border border-gray-300 rounded-lg hover:border-blue-500 transition-colors bg-white"
              >
                <div className="flex items-center gap-3">
                  <User className="w-5 h-5 text-gray-400" />
                  <div className="text-left">
                    <p className="text-sm font-medium text-gray-900">
                      {selectedMockUser?.email}
                    </p>
                    <p className="text-xs text-gray-500">
                      {selectedMockUser?.plan} - {selectedMockUser?.usage}
                    </p>
                  </div>
                </div>
                <ChevronDown
                  className={`w-5 h-5 text-gray-400 transition-transform ${
                    showDropdown ? 'rotate-180' : ''
                  }`}
                />
              </button>

              {/* Dropdown */}
              {showDropdown && (
                <div className="absolute z-10 w-full mt-2 bg-white border border-gray-200 rounded-lg shadow-xl max-h-96 overflow-y-auto">
                  {AVAILABLE_MOCK_USERS.map((mockUser) => (
                    <button
                      key={mockUser.email}
                      onClick={() => {
                        setSelectedEmail(mockUser.email);
                        setShowDropdown(false);
                      }}
                      className={`w-full px-4 py-3 text-left hover:bg-blue-50 transition-colors border-b border-gray-100 last:border-b-0 ${
                        selectedEmail === mockUser.email ? 'bg-blue-50' : ''
                      }`}
                    >
                      <p className="text-sm font-medium text-gray-900">
                        {mockUser.email}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        <span
                          className={`inline-block px-2 py-0.5 rounded text-xs font-medium ${
                            mockUser.plan === 'FREE'
                              ? 'bg-gray-100 text-gray-700'
                              : mockUser.plan === 'BASIC'
                              ? 'bg-blue-100 text-blue-700'
                              : mockUser.plan === 'PRO'
                              ? 'bg-purple-100 text-purple-700'
                              : mockUser.plan === 'ENTERPRISE'
                              ? 'bg-indigo-100 text-indigo-700'
                              : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {mockUser.plan}
                        </span>
                        <span className="text-xs text-gray-500">{mockUser.usage}</span>
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* User Info Preview */}
          {selectedEmail && (
            <div className="mb-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
              <p className="text-xs font-medium text-gray-700 mb-2">User Details:</p>
              <div className="space-y-1 text-xs text-gray-600">
                <p>
                  <span className="font-medium">Email:</span> {selectedEmail}
                </p>
                <p>
                  <span className="font-medium">Plan:</span> {selectedMockUser?.plan}
                </p>
                <p>
                  <span className="font-medium">Usage:</span> {selectedMockUser?.usage}
                </p>
              </div>
            </div>
          )}

          {/* Login Button */}
          <button
            onClick={handleLogin}
            className="w-full px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium shadow-md hover:shadow-lg"
          >
            Login as {selectedMockUser?.plan} User
          </button>

          {/* Info */}
          <div className="mt-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <p className="text-xs text-yellow-800">
              <span className="font-semibold">Note:</span> This is a mock login for
              testing. No actual authentication is performed. Use this to test different
              subscription plans and feature access.
            </p>
          </div>
        </div>

        {/* Available Plans Info */}
        <div className="mt-6 bg-white rounded-xl shadow-lg p-6">
          <h3 className="text-sm font-semibold text-gray-900 mb-4">
            Available Test Scenarios:
          </h3>
          <ul className="space-y-2 text-xs text-gray-600">
            <li className="flex items-start gap-2">
              <span className="inline-block w-2 h-2 bg-gray-400 rounded-full mt-1 flex-shrink-0"></span>
              <span>
                <span className="font-medium">FREE Plan:</span> Limited to 3 listings, no
                analytics, basic features only
              </span>
            </li>
            <li className="flex items-start gap-2">
              <span className="inline-block w-2 h-2 bg-blue-400 rounded-full mt-1 flex-shrink-0"></span>
              <span>
                <span className="font-medium">BASIC Plan:</span> 50 listings, analytics
                access, bulk upload
              </span>
            </li>
            <li className="flex items-start gap-2">
              <span className="inline-block w-2 h-2 bg-purple-400 rounded-full mt-1 flex-shrink-0"></span>
              <span>
                <span className="font-medium">PRO Plan:</span> 200 listings, market
                analysis, email automation
              </span>
            </li>
            <li className="flex items-start gap-2">
              <span className="inline-block w-2 h-2 bg-indigo-400 rounded-full mt-1 flex-shrink-0"></span>
              <span>
                <span className="font-medium">ENTERPRISE Plan:</span> Unlimited listings,
                API access, priority support
              </span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
};
