import { useAuthStore } from '../../store/authStore';
import { useDealerFeatures } from '../../hooks/useDealerFeatures';
import { UpgradePrompt } from '../../components/dealer/UpgradePrompt';

/**
 * Página de testing para Analytics Dashboard
 * 
 * - FREE: Muestra UpgradePrompt (bloqueado)
 * - BASIC+: Muestra analytics básicos
 * - PRO+: Muestra análisis avanzado de precios
 */
export const DealerAnalyticsTestPage = () => {
  const user = useAuthStore((state) => state.user);
  const { canAccess, currentPlan, limits } = useDealerFeatures(user?.subscription);

  // Si no tiene acceso a analytics, mostrar upgrade prompt
  if (!canAccess('analyticsAccess')) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <h1 className="text-3xl font-bold mb-2">Analytics Dashboard</h1>
        <p className="text-gray-600 mb-6">
          Get insights into your inventory performance, market trends, and ROI
        </p>
        <UpgradePrompt 
          feature="analyticsAccess" 
          currentPlan={currentPlan}
          onUpgrade={() => {
            alert('Upgrade flow would start here');
          }}
        />
      </div>
    );
  }

  // Tiene acceso básico a analytics
  return (
    <div className="max-w-7xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Analytics Dashboard</h1>
          <p className="text-gray-600 mt-1">
            Track your performance and market insights
          </p>
        </div>
        <span className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm font-medium">
          {currentPlan.toUpperCase()} Plan
        </span>
      </div>

      {/* Stats básicos (BASIC+) */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <p className="text-sm text-gray-600 mb-1">Total Views</p>
          <p className="text-3xl font-bold text-gray-900">12,458</p>
          <p className="text-xs text-green-600 mt-2">↑ 12.3% vs last month</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <p className="text-sm text-gray-600 mb-1">Leads Generated</p>
          <p className="text-3xl font-bold text-gray-900">342</p>
          <p className="text-xs text-green-600 mt-2">↑ 8.7% vs last month</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <p className="text-sm text-gray-600 mb-1">Conversions</p>
          <p className="text-3xl font-bold text-gray-900">28</p>
          <p className="text-xs text-red-600 mt-2">↓ 3.2% vs last month</p>
        </div>
        <div className="bg-white p-6 rounded-lg shadow border border-gray-200">
          <p className="text-sm text-gray-600 mb-1">Avg. Days to Sell</p>
          <p className="text-3xl font-bold text-gray-900">14.5</p>
          <p className="text-xs text-green-600 mt-2">↓ 2.1 days faster</p>
        </div>
      </div>

      {/* Market Price Analysis - Solo PRO+ */}
      {!canAccess('marketPriceAnalysis') ? (
        <div className="mb-8">
          <h2 className="text-xl font-semibold mb-4">Market Price Analysis</h2>
          <UpgradePrompt 
            feature="marketPriceAnalysis" 
            currentPlan={currentPlan}
            onUpgrade={() => alert('Upgrade to PRO')}
          />
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-8">
          <h2 className="text-xl font-semibold mb-4">Market Price Analysis</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <p className="text-sm text-gray-600 mb-2">Market Average</p>
              <p className="text-2xl font-bold text-gray-900">$28,450</p>
              <p className="text-xs text-gray-500 mt-1">For similar vehicles</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-2">Your Average</p>
              <p className="text-2xl font-bold text-blue-600">$27,200</p>
              <p className="text-xs text-gray-500 mt-1">4.4% below market</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-2">Recommended Range</p>
              <p className="text-2xl font-bold text-green-600">$27,800 - $29,200</p>
              <p className="text-xs text-gray-500 mt-1">Optimal pricing</p>
            </div>
          </div>
        </div>
      )}

      {/* Top Performing Listings */}
      <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
        <h2 className="text-xl font-semibold mb-4">Top Performing Listings</h2>
        <div className="space-y-3">
          {[
            { name: '2023 Toyota Camry', views: 1240, leads: 45, price: '$28,500' },
            { name: '2022 Honda Accord', views: 980, leads: 38, price: '$26,900' },
            { name: '2024 Mazda CX-5', views: 870, leads: 32, price: '$32,400' },
          ].map((listing, idx) => (
            <div key={idx} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">{listing.name}</p>
                <p className="text-sm text-gray-600">{listing.price}</p>
              </div>
              <div className="flex gap-6 text-sm">
                <div>
                  <span className="text-gray-600">Views:</span>
                  <span className="ml-1 font-semibold text-gray-900">{listing.views}</span>
                </div>
                <div>
                  <span className="text-gray-600">Leads:</span>
                  <span className="ml-1 font-semibold text-gray-900">{listing.leads}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Feature Limits Info */}
      <div className="mt-6 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <p className="text-sm text-blue-900 font-medium mb-2">Your Plan Features:</p>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-xs">
          <div>
            <span className="text-blue-700">Max Listings:</span>
            <span className="ml-1 font-semibold text-blue-900">
              {limits.maxListings === 999999 ? '∞' : limits.maxListings}
            </span>
          </div>
          <div>
            <span className="text-blue-700">Analytics:</span>
            <span className="ml-1 font-semibold text-blue-900">
              {limits.analyticsAccess ? '✓' : '✗'}
            </span>
          </div>
          <div>
            <span className="text-blue-700">Market Analysis:</span>
            <span className="ml-1 font-semibold text-blue-900">
              {limits.marketPriceAnalysis ? '✓' : '✗'}
            </span>
          </div>
          <div>
            <span className="text-blue-700">API Access:</span>
            <span className="ml-1 font-semibold text-blue-900">
              {limits.apiAccess ? '✓' : '✗'}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};
