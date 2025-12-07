import { useDealerFeatures } from '../../hooks/useDealerFeatures';
import { UpgradePrompt, LimitReachedBanner } from '../../components/dealer/UpgradePrompt';
import { useAuthStore } from '../../store/authStore';
import { BarChart3, TrendingUp, DollarSign } from 'lucide-react';

/**
 * Ejemplo: Página de Analytics del Dealer
 * 
 * Muestra diferentes estados según el plan:
 * - FREE: Muestra UpgradePrompt (bloqueado)
 * - BASIC+: Muestra analytics
 */
export const DealerAnalyticsPage = () => {
  const user = useAuthStore((state) => state.user);
  const { canAccess, currentPlan } = useDealerFeatures(user?.subscription);

  // Si no tiene acceso a analytics, mostrar upgrade prompt
  if (!canAccess('analyticsAccess')) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <h1 className="text-3xl font-bold mb-6">Analytics Dashboard</h1>
        <UpgradePrompt 
          feature="analyticsAccess" 
          currentPlan={currentPlan}
          onUpgrade={() => {
            // TODO: Redirigir a página de upgrade
            window.location.href = '/dealer/billing/upgrade';
          }}
        />
      </div>
    );
  }

  // Si tiene acceso, mostrar analytics
  return (
    <div className="max-w-7xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-3xl font-bold">Analytics Dashboard</h1>
        <span className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium">
          {currentPlan.toUpperCase()} Plan
        </span>
      </div>

      {/* Market Price Analysis - Solo PRO+ */}
      {!canAccess('marketPriceAnalysis') ? (
        <div className="mb-6">
          <UpgradePrompt 
            feature="marketPriceAnalysis" 
            currentPlan={currentPlan}
            onUpgrade={() => window.location.href = '/dealer/billing/upgrade'}
          />
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white p-6 rounded-lg shadow">
            <div className="flex items-center gap-3 mb-2">
              <BarChart3 className="w-6 h-6 text-blue-600" />
              <h3 className="font-semibold">Market Price</h3>
            </div>
            <p className="text-3xl font-bold">$28,450</p>
            <p className="text-sm text-gray-600">Average for sedans</p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow">
            <div className="flex items-center gap-3 mb-2">
              <TrendingUp className="w-6 h-6 text-green-600" />
              <h3 className="font-semibold">Trend</h3>
            </div>
            <p className="text-3xl font-bold text-green-600">+8.3%</p>
            <p className="text-sm text-gray-600">vs last month</p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow">
            <div className="flex items-center gap-3 mb-2">
              <DollarSign className="w-6 h-6 text-purple-600" />
              <h3 className="font-semibold">Your Avg</h3>
            </div>
            <p className="text-3xl font-bold">$27,200</p>
            <p className="text-sm text-gray-600">4.4% below market</p>
          </div>
        </div>
      )}

      {/* Analytics básicos (todos los planes con analytics) */}
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-xl font-semibold mb-4">Performance Overview</h2>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div>
            <p className="text-gray-600 text-sm">Total Views</p>
            <p className="text-2xl font-bold">12,458</p>
          </div>
          <div>
            <p className="text-gray-600 text-sm">Leads</p>
            <p className="text-2xl font-bold">342</p>
          </div>
          <div>
            <p className="text-gray-600 text-sm">Conversions</p>
            <p className="text-2xl font-bold">28</p>
          </div>
          <div>
            <p className="text-gray-600 text-sm">Conversion Rate</p>
            <p className="text-2xl font-bold">8.2%</p>
          </div>
        </div>
      </div>
    </div>
  );
};

/**
 * Ejemplo: Página de crear listing
 * Verifica límites antes de permitir publicar
 */
export const CreateListingPage = () => {
  const user = useAuthStore((state) => state.user);
  const { hasReachedLimit, getUsageProgress, currentPlan, usage, limits } = useDealerFeatures(user?.subscription);

  const isAtLimit = hasReachedLimit('listings');
  const progress = getUsageProgress('listings');

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Create New Listing</h1>

      {/* Banner si alcanzó el límite */}
      {isAtLimit && (
        <div className="mb-6">
          <LimitReachedBanner 
            type="listings"
            current={usage.currentListings}
            max={limits.maxListings}
            currentPlan={currentPlan}
          />
        </div>
      )}

      {/* Progress bar de uso */}
      {!isAtLimit && (
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-6">
          <div className="flex items-center justify-between mb-2">
            <p className="text-sm font-medium text-gray-700">
              Listings used: {usage.currentListings} of {limits.maxListings}
            </p>
            <span className="text-sm text-gray-600">{Math.round(progress)}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div 
              className={`h-2 rounded-full transition-all ${
                progress > 90 ? 'bg-red-500' : progress > 70 ? 'bg-yellow-500' : 'bg-blue-500'
              }`}
              style={{ width: `${progress}%` }}
            />
          </div>
          {progress > 80 && (
            <p className="text-xs text-gray-600 mt-2">
              You're running out of listing slots. 
              <a href="/dealer/billing/upgrade" className="text-blue-600 hover:underline ml-1">
                Upgrade your plan
              </a>
            </p>
          )}
        </div>
      )}

      {/* Formulario de listing (disabled si está en el límite) */}
      <form className={isAtLimit ? 'opacity-50 pointer-events-none' : ''}>
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Vehicle Information</h2>
          {/* Campos del formulario... */}
          <p className="text-gray-600 text-sm">Form fields here...</p>
        </div>

        <div className="mt-6 flex gap-4">
          <button
            type="submit"
            disabled={isAtLimit}
            className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            Publish Listing
          </button>
          <button
            type="button"
            className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
          >
            Save as Draft
          </button>
        </div>
      </form>
    </div>
  );
};
