/**
 * Dealer Layout
 * 
 * Layout for dealer panel pages with sidebar navigation
 * Shows plan-based features and usage in sidebar
 */

import type { ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { 
  LogOut,
  Crown,
  TrendingUp
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';
import { useAuthStore } from '@/store/authStore';
import { DealerSidebar } from '@/components/navigation';

interface DealerLayoutProps {
  children: ReactNode;
}

const DealerLayout = ({ children }: DealerLayoutProps) => {
  const navigate = useNavigate();
  const logout = useAuthStore((state) => state.logout);
  const { 
    user, 
    dealerPlan, 
    usage,
    limits,
    getUsagePercentage,
    hasReachedLimit
  } = usePermissions();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  const getPlanBadgeColor = () => {
    switch (dealerPlan) {
      case 'enterprise': return 'bg-purple-100 text-purple-700';
      case 'pro': return 'bg-blue-100 text-blue-700';
      case 'basic': return 'bg-green-100 text-green-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Top Bar */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex items-center justify-between px-6 py-4">
          <div className="flex items-center gap-4">
            <Link to="/" className="flex items-center gap-2">
              <div className="w-9 h-9 bg-gradient-to-br from-blue-600 to-emerald-500 rounded-xl flex items-center justify-center shadow-lg">
                <span className="text-white font-bold text-lg">O</span>
              </div>
              <span className="text-xl font-bold text-gray-900">Okla</span>
            </Link>
            <div className="h-6 w-px bg-gray-300" />
            <span className="text-sm text-gray-600 font-medium">Panel de Dealer</span>
          </div>

          <div className="flex items-center gap-4">
            <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full ${getPlanBadgeColor()}`}>
              <Crown className="h-4 w-4" />
              <span className="text-sm font-medium capitalize">{dealerPlan || 'Free'}</span>
            </div>
            <span className="text-sm text-gray-600">{user?.name}</span>
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <LogOut className="w-4 h-4" />
              Salir
            </button>
          </div>
        </div>
      </div>

      <div className="flex">
        {/* Sidebar */}
        <aside className="w-72 bg-white border-r border-gray-200 h-[calc(100vh-65px)] flex flex-col">
          <div className="p-4 flex-1 overflow-y-auto">
            <DealerSidebar />
          </div>

          {/* Usage Stats */}
          <div className="p-4 border-t border-gray-100">
            <div className="space-y-3">
              <div>
                <div className="flex justify-between text-xs text-gray-600 mb-1">
                  <span>Publicaciones</span>
                  <span>{usage.listings}/{limits.maxListings === Infinity ? '∞' : limits.maxListings}</span>
                </div>
                <div className="w-full h-1.5 bg-gray-100 rounded-full overflow-hidden">
                  <div 
                    className={`h-full rounded-full ${
                      hasReachedLimit('listings') ? 'bg-red-500' :
                      getUsagePercentage('listings') >= 80 ? 'bg-amber-500' : 'bg-blue-500'
                    }`}
                    style={{ 
                      width: limits.maxListings === Infinity 
                        ? '20%' 
                        : `${getUsagePercentage('listings')}%` 
                    }}
                  />
                </div>
              </div>
              <div>
                <div className="flex justify-between text-xs text-gray-600 mb-1">
                  <span>Destacados</span>
                  <span>{usage.featuredListings}/{limits.maxFeaturedListings}</span>
                </div>
                <div className="w-full h-1.5 bg-gray-100 rounded-full overflow-hidden">
                  <div 
                    className={`h-full rounded-full ${
                      hasReachedLimit('featured') ? 'bg-red-500' :
                      getUsagePercentage('featured') >= 80 ? 'bg-amber-500' : 'bg-blue-500'
                    }`}
                    style={{ width: `${getUsagePercentage('featured')}%` }}
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Upgrade CTA */}
          {(dealerPlan === 'free' || dealerPlan === 'basic') && (
            <div className="p-4">
              <Link
                to="/dealer/plans"
                className="block bg-gradient-to-r from-blue-600 to-purple-600 text-white rounded-lg p-4 text-center hover:opacity-90 transition-opacity"
              >
                <TrendingUp className="h-5 w-5 mx-auto mb-2" />
                <p className="text-sm font-semibold">Actualiza tu Plan</p>
                <p className="text-xs text-blue-100 mt-1">Desbloquea más funciones</p>
              </Link>
            </div>
          )}
        </aside>

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto">
          {children}
        </main>
      </div>
    </div>
  );
};

export default DealerLayout;
