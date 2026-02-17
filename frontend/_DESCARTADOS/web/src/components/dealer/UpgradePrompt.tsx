import { Lock, TrendingUp, Zap } from 'lucide-react';
import { DealerPlan, DEALER_PLAN_LIMITS } from '@/shared/types';
import { FEATURE_NAMES, DEALER_PLAN_PRICING } from '@/hooks/useDealerFeatures';
import type { DealerPlanFeatures } from '@/shared/types';

interface UpgradePromptProps {
  feature: keyof DealerPlanFeatures;
  currentPlan: DealerPlan;
  onUpgrade?: () => void;
}

/**
 * Componente para mostrar cuando un dealer intenta acceder a una feature bloqueada
 */
export const UpgradePrompt = ({ feature, currentPlan: _currentPlan, onUpgrade }: UpgradePromptProps) => {
  const featureName = String(FEATURE_NAMES[feature as keyof typeof FEATURE_NAMES] || feature);
  
  // Encontrar el primer plan que tenga esta feature
  const availablePlans = Object.entries(DEALER_PLAN_LIMITS)
    .filter(([_, limits]) => {
      const value = limits[feature];
      return typeof value === 'boolean' ? value : (value as number) > 0;
    })
    .map(([plan]) => plan as DealerPlan);

  const recommendedPlan = availablePlans[0];
  const pricing = recommendedPlan ? DEALER_PLAN_PRICING[recommendedPlan as keyof typeof DEALER_PLAN_PRICING] : null;
  const isAvailable = pricing?.available;

  return (
    <div className="bg-gradient-to-br from-blue-50 to-indigo-50 border border-blue-200 rounded-lg p-6 text-center">
      <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-100 rounded-full mb-4">
        <Lock className="w-8 h-8 text-blue-600" />
      </div>
      
      <h3 className="text-xl font-semibold text-gray-900 mb-2">
        {featureName} is locked
      </h3>
      
      <p className="text-gray-600 mb-4">
        Upgrade to <span className="font-semibold text-blue-600">{recommendedPlan.toUpperCase()}</span> plan to unlock this feature
      </p>

      {!isAvailable && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-4">
          <div className="flex items-center gap-2 text-yellow-800 text-sm">
            <Zap className="w-4 h-4" />
            <p className="font-medium">
              Paid plans launching soon! You'll be the first to know.
            </p>
          </div>
        </div>
      )}

      <div className="flex flex-col sm:flex-row gap-3 justify-center items-center">
        {isAvailable ? (
          <>
            <button
              onClick={onUpgrade}
              className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
            >
              Upgrade to {recommendedPlan.charAt(0).toUpperCase() + recommendedPlan.slice(1)}
            </button>
            <a
              href="/pricing"
              className="px-6 py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Compare Plans
            </a>
          </>
        ) : (
          <>
            <button
              onClick={() => {
                // TODO: Implementar waitlist
                alert('We\'ll notify you when paid plans are available!');
              }}
              className="px-6 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
            >
              Join Waitlist
            </button>
            <a
              href="/pricing"
              className="text-blue-600 hover:text-blue-700 font-medium text-sm"
            >
              Learn more about upcoming plans →
            </a>
          </>
        )}
      </div>

      <div className="mt-6 pt-6 border-t border-blue-200">
        <p className="text-sm text-gray-600 mb-3">
          What you'll get with {recommendedPlan.toUpperCase()}:
        </p>
        <div className="flex flex-wrap gap-2 justify-center">
          {Object.entries(DEALER_PLAN_LIMITS[recommendedPlan])
            .filter(([_, value]) => value === true || (typeof value === 'number' && value > 0))
            .slice(0, 4)
            .map(([key]) => (
              <span
                key={key}
                className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-xs font-medium"
              >
                <TrendingUp className="w-3 h-3" />
                {FEATURE_NAMES[key as keyof DealerPlanFeatures]}
              </span>
            ))}
        </div>
      </div>
    </div>
  );
};

/**
 * Componente pequeño para mostrar en botones/links bloqueados
 */
export const FeatureLock = ({ feature }: { feature: keyof DealerPlanFeatures }) => {
  return (
    <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-yellow-100 text-yellow-800 rounded text-xs font-medium">
      <Lock className="w-3 h-3" />
      {FEATURE_NAMES[feature]}
    </span>
  );
};

/**
 * Banner de límite alcanzado
 */
interface LimitReachedBannerProps {
  type: 'listings' | 'featured';
  current: number;
  max: number;
  currentPlan: DealerPlan;
}

export const LimitReachedBanner = ({ type, current: _current, max, currentPlan }: LimitReachedBannerProps) => {
  const message = type === 'listings' 
    ? `You've reached your limit of ${max} vehicle listings` 
    : `You've used all ${max} featured listing slots`;

  const nextPlan = currentPlan === DealerPlan.FREE ? DealerPlan.BASIC : DealerPlan.PRO;
  const nextLimit = type === 'listings' 
    ? DEALER_PLAN_LIMITS[nextPlan].maxListings 
    : DEALER_PLAN_LIMITS[nextPlan].maxFeaturedListings;

  return (
    <div className="bg-orange-50 border border-orange-200 rounded-lg p-4">
      <div className="flex items-start gap-3">
        <Lock className="w-5 h-5 text-orange-600 mt-0.5 flex-shrink-0" />
        <div className="flex-1">
          <p className="font-medium text-orange-900">{message}</p>
          <p className="text-sm text-orange-700 mt-1">
            Upgrade to <span className="font-semibold">{nextPlan.toUpperCase()}</span> for up to {nextLimit === 999999 ? 'unlimited' : nextLimit} {type}
          </p>
        </div>
        <a
          href="/dealer/billing/upgrade"
          className="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 transition-colors text-sm font-medium whitespace-nowrap"
        >
          Upgrade Plan
        </a>
      </div>
    </div>
  );
};
