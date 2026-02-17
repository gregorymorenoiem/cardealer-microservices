import { useAuthStore } from '../../store/authStore';
import { useDealerFeatures, FEATURE_NAMES, DEALER_PLAN_PRICING } from '../../hooks/useDealerFeatures';
import { DEALER_PLAN_LIMITS, DealerPlan } from '@/shared/types';
import type { DealerPlanFeatures } from '@/shared/types';

/**
 * Página de testing para ver comparación de planes y features
 */
export const PlansComparisonTestPage = () => {
  const user = useAuthStore((state) => state.user);
  const { currentPlan, canAccess } = useDealerFeatures(user?.subscription);

  const plans = [DealerPlan.FREE, DealerPlan.BASIC, DealerPlan.PRO, DealerPlan.ENTERPRISE];
  const allFeatures = Object.keys(FEATURE_NAMES) as (keyof DealerPlanFeatures)[];

  return (
    <div className="max-w-7xl mx-auto p-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Plans & Features Comparison</h1>
        <p className="text-gray-600">
          Compare all available plans and their features
        </p>
        {user && (
          <p className="mt-2 text-sm">
            Your current plan: <span className="font-bold text-blue-600">{currentPlan.toUpperCase()}</span>
          </p>
        )}
      </div>

      {/* Pricing Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-12">
        {plans.map((plan) => {
          const pricing = DEALER_PLAN_PRICING[plan];
          const limits = DEALER_PLAN_LIMITS[plan];
          const isCurrentPlan = plan === currentPlan;
          
          return (
            <div
              key={plan}
              className={`bg-white rounded-xl shadow-lg border-2 p-6 ${
                isCurrentPlan 
                  ? 'border-blue-500 ring-2 ring-blue-200' 
                  : 'border-gray-200'
              }`}
            >
              {isCurrentPlan && (
                <div className="inline-block px-3 py-1 bg-blue-500 text-white text-xs font-bold rounded-full mb-3">
                  CURRENT PLAN
                </div>
              )}
              <h3 className="text-2xl font-bold mb-2">{plan.toUpperCase()}</h3>
              <div className="mb-4">
                {pricing.price === 0 ? (
                  <p className="text-4xl font-bold">Free</p>
                ) : (
                  <>
                    <p className="text-4xl font-bold">
                      ${pricing.price}
                      <span className="text-lg text-gray-600">/mo</span>
                    </p>
                    {!pricing.available && (
                      <p className="text-xs text-yellow-600 mt-1 font-medium">
                        Coming soon
                      </p>
                    )}
                  </>
                )}
              </div>
              <ul className="space-y-2 text-sm mb-6">
                <li className="flex items-center gap-2">
                  <span className="text-green-600">✓</span>
                  <span>
                    {limits.maxListings === 999999 ? 'Unlimited' : limits.maxListings} listings
                  </span>
                </li>
                <li className="flex items-center gap-2">
                  <span className="text-green-600">✓</span>
                  <span>{limits.maxImages} images/vehicle</span>
                </li>
                <li className="flex items-center gap-2">
                  <span className={limits.analyticsAccess ? 'text-green-600' : 'text-gray-400'}>
                    {limits.analyticsAccess ? '✓' : '✗'}
                  </span>
                  <span className={!limits.analyticsAccess ? 'text-gray-400' : ''}>
                    Analytics
                  </span>
                </li>
                <li className="flex items-center gap-2">
                  <span className={limits.bulkUpload ? 'text-green-600' : 'text-gray-400'}>
                    {limits.bulkUpload ? '✓' : '✗'}
                  </span>
                  <span className={!limits.bulkUpload ? 'text-gray-400' : ''}>
                    Bulk upload
                  </span>
                </li>
              </ul>
              {!isCurrentPlan && (
                <button
                  className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium disabled:bg-gray-400 disabled:cursor-not-allowed"
                  disabled={!pricing.available && pricing.price > 0}
                >
                  {pricing.available || pricing.price === 0 ? 'Select Plan' : 'Join Waitlist'}
                </button>
              )}
            </div>
          );
        })}
      </div>

      {/* Detailed Feature Comparison Table */}
      <div className="bg-white rounded-xl shadow-lg border border-gray-200 overflow-hidden">
        <div className="px-6 py-4 bg-gray-50 border-b border-gray-200">
          <h2 className="text-xl font-bold">Detailed Feature Comparison</h2>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-200">
                <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                  Feature
                </th>
                {plans.map((plan) => (
                  <th key={plan} className="px-6 py-3 text-center text-sm font-semibold text-gray-900">
                    {plan.toUpperCase()}
                    {plan === currentPlan && (
                      <span className="ml-2 text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">
                        Your plan
                      </span>
                    )}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {allFeatures.map((feature) => (
                <tr key={feature} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm text-gray-900">
                    {FEATURE_NAMES[feature]}
                    {user && !canAccess(feature) && (
                      <span className="ml-2 text-xs bg-yellow-100 text-yellow-800 px-2 py-0.5 rounded">
                        Locked
                      </span>
                    )}
                  </td>
                  {plans.map((plan) => {
                    const limits = DEALER_PLAN_LIMITS[plan];
                    const value = limits[feature];
                    
                    return (
                      <td key={plan} className="px-6 py-4 text-center text-sm">
                        {typeof value === 'boolean' ? (
                          <span className={`inline-flex items-center justify-center w-6 h-6 rounded-full ${
                            value 
                              ? 'bg-green-100 text-green-600' 
                              : 'bg-gray-100 text-gray-400'
                          }`}>
                            {value ? '✓' : '✗'}
                          </span>
                        ) : typeof value === 'number' ? (
                          <span className="font-semibold text-gray-900">
                            {value === 999999 ? '∞' : value}
                          </span>
                        ) : (
                          <span className="text-gray-400">—</span>
                        )}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Your Current Access */}
      {user && (
        <div className="mt-8 bg-blue-50 border border-blue-200 rounded-xl p-6">
          <h3 className="text-lg font-bold text-blue-900 mb-4">
            Your Current Access ({currentPlan.toUpperCase()} Plan)
          </h3>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {allFeatures.map((feature) => {
              const hasAccess = canAccess(feature);
              return (
                <div
                  key={feature}
                  className={`p-3 rounded-lg border-2 ${
                    hasAccess 
                      ? 'bg-green-50 border-green-200' 
                      : 'bg-gray-50 border-gray-200'
                  }`}
                >
                  <div className="flex items-center gap-2 mb-1">
                    <span className={`text-lg ${hasAccess ? 'text-green-600' : 'text-gray-400'}`}>
                      {hasAccess ? '✓' : '✗'}
                    </span>
                    <span className={`text-xs font-medium ${
                      hasAccess ? 'text-green-900' : 'text-gray-500'
                    }`}>
                      {hasAccess ? 'Available' : 'Locked'}
                    </span>
                  </div>
                  <p className="text-xs text-gray-700">
                    {FEATURE_NAMES[feature]}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* FAQ */}
      <div className="mt-8 bg-white rounded-xl shadow-lg border border-gray-200 p-6">
        <h3 className="text-xl font-bold mb-4">Frequently Asked Questions</h3>
        <div className="space-y-4">
          <div>
            <p className="font-semibold text-gray-900 mb-1">
              Can I upgrade or downgrade my plan?
            </p>
            <p className="text-sm text-gray-600">
              Yes, you can change your plan at any time. Upgrades take effect immediately,
              and downgrades will take effect at the end of your current billing period.
            </p>
          </div>
          <div>
            <p className="font-semibold text-gray-900 mb-1">
              What happens when I reach my listing limit?
            </p>
            <p className="text-sm text-gray-600">
              You won't be able to create new listings until you either remove existing ones
              or upgrade to a plan with a higher limit.
            </p>
          </div>
          <div>
            <p className="font-semibold text-gray-900 mb-1">
              Is the FREE plan really free forever?
            </p>
            <p className="text-sm text-gray-600">
              Yes! The FREE plan is completely free with no time limits. It's perfect for
              getting started or for dealers with small inventories.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
