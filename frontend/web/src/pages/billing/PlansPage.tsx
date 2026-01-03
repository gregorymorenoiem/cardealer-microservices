import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  FiCheck, 
  FiX, 
  FiArrowLeft, 
  FiZap,
  FiStar,
  FiShield,
  FiHeadphones
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import { usePlans, useSubscription } from '@/hooks/useBilling';
import { useAuthStore } from '@/store/authStore';
import { plans as mockPlans, formatCurrency } from '@/mocks/billingData';
import type { BillingCycle, PlanConfig } from '@/types/billing';

export default function PlansPage() {
  const navigate = useNavigate();
  const [billingCycle, setBillingCycle] = useState<BillingCycle>('monthly');
  
  // Get user info for subscription lookup
  const { user } = useAuthStore();
  const dealerId = user?.dealerId || user?.id || '';
  
  // Use TanStack Query hooks
  const { data: fetchedPlans, isLoading: isLoadingPlans } = usePlans();
  const { data: currentSubscription, isLoading: isLoadingSub } = useSubscription(dealerId);
  
  // Use fetched plans if available, fallback to mocks
  const plans = fetchedPlans || mockPlans;
  const isLoading = isLoadingPlans || isLoadingSub;

  const getPrice = (plan: PlanConfig) => {
    return plan.prices[billingCycle];
  };

  const getSavings = (plan: PlanConfig) => {
    if (billingCycle === 'monthly') return 0;
    const monthlyTotal = plan.prices.monthly * (billingCycle === 'yearly' ? 12 : 3);
    const actualPrice = plan.prices[billingCycle];
    return Math.round(((monthlyTotal - actualPrice) / monthlyTotal) * 100);
  };

  const handleSelectPlan = (planId: string) => {
    // In real app, this would initiate checkout or upgrade flow
    navigate('/billing/checkout', { state: { planId, billingCycle } });
  };

  const isCurrentPlan = (planId: string) => {
    if (!currentSubscription) return false;
    return currentSubscription.plan === planId && currentSubscription.cycle === billingCycle;
  };

  const isUpgrade = (planId: string) => {
    const planOrder = ['free', 'basic', 'professional', 'enterprise'];
    const currentPlan = currentSubscription?.plan || 'free';
    const currentIndex = planOrder.indexOf(currentPlan);
    const planIndex = planOrder.indexOf(planId);
    return planIndex > currentIndex;
  };

  const featureLabels: Record<string, string> = {
    listings: 'Active Listings',
    users: 'Team Members',
    storage: 'Storage',
    analytics: 'Analytics Dashboard',
    api: 'API Access',
    customBranding: 'Custom Branding',
    prioritySupport: 'Priority Support',
    dedicatedManager: 'Dedicated Account Manager',
    bulkUpload: 'Bulk Upload',
    marketplace: 'Marketplace Access',
    realEstate: 'Real Estate Vertical',
    crm: 'CRM Features',
    reporting: 'Advanced Reporting',
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <Link to="/billing" className="inline-flex items-center text-gray-600 hover:text-gray-900 mb-4">
              <FiArrowLeft className="w-4 h-4 mr-2" />
              Back to Billing
            </Link>
            <h1 className="text-3xl font-bold text-gray-900">Choose Your Plan</h1>
            <p className="text-gray-600 mt-2">
              Select the perfect plan for your dealership. Upgrade or downgrade anytime.
            </p>
          </div>

          {/* Billing Cycle Toggle */}
          <div className="flex justify-center mb-12">
            <div className="bg-gray-100 p-1 rounded-lg inline-flex">
              {(['monthly', 'quarterly', 'yearly'] as BillingCycle[]).map((cycle) => (
                <button
                  key={cycle}
                  onClick={() => setBillingCycle(cycle)}
                  className={`px-6 py-2 rounded-md text-sm font-medium transition-all ${
                    billingCycle === cycle
                      ? 'bg-white text-gray-900 shadow-sm'
                      : 'text-gray-600 hover:text-gray-900'
                  }`}
                >
                  {cycle.charAt(0).toUpperCase() + cycle.slice(1)}
                  {cycle === 'yearly' && (
                    <span className="ml-2 text-xs text-green-600 font-semibold">Save 20%</span>
                  )}
                </button>
              ))}
            </div>
          </div>

          {/* Plans Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
            {plans.map((plan, index) => (
              <motion.div
                key={plan.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
                className={`relative bg-white rounded-2xl shadow-sm border-2 transition-all hover:shadow-lg ${
                  plan.popular 
                    ? 'border-primary-500 ring-4 ring-primary-100' 
                    : isCurrentPlan(plan.id)
                    ? 'border-green-500'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                {plan.popular && (
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                    <span className="bg-primary-500 text-white text-xs font-bold px-4 py-1 rounded-full flex items-center gap-1">
                      <FiStar className="w-3 h-3" />
                      Most Popular
                    </span>
                  </div>
                )}
                
                {isCurrentPlan(plan.id) && (
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2">
                    <span className="bg-green-500 text-white text-xs font-bold px-4 py-1 rounded-full">
                      Current Plan
                    </span>
                  </div>
                )}

                <div className="p-6">
                  <h3 className="text-xl font-bold text-gray-900">{plan.name}</h3>
                  <p className="text-sm text-gray-500 mt-1">{plan.description}</p>

                  <div className="mt-6">
                    <div className="flex items-baseline">
                      <span className="text-4xl font-bold text-gray-900">
                        {formatCurrency(getPrice(plan))}
                      </span>
                      {plan.id !== 'free' && (
                        <span className="text-gray-500 ml-2">/{billingCycle}</span>
                      )}
                    </div>
                    {getSavings(plan) > 0 && (
                      <p className="text-sm text-green-600 mt-1">
                        Save {getSavings(plan)}% vs monthly
                      </p>
                    )}
                  </div>

                  <Button
                    variant={plan.popular ? 'primary' : 'outline'}
                    className="w-full mt-6"
                    onClick={() => handleSelectPlan(plan.id)}
                    disabled={isCurrentPlan(plan.id)}
                  >
                    {isCurrentPlan(plan.id) 
                      ? 'Current Plan' 
                      : isUpgrade(plan.id) 
                      ? 'Upgrade' 
                      : plan.id === 'free' 
                      ? 'Get Started' 
                      : 'Select Plan'}
                  </Button>

                  <div className="mt-6 pt-6 border-t border-gray-100">
                    <p className="text-sm font-medium text-gray-900 mb-4">Features included:</p>
                    <ul className="space-y-3">
                      <li className="flex items-center text-sm">
                        <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                        <span>
                          {plan.features.listings === 'unlimited' 
                            ? 'Unlimited listings' 
                            : `${plan.features.listings} listings`}
                        </span>
                      </li>
                      <li className="flex items-center text-sm">
                        <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                        <span>
                          {plan.features.users === 'unlimited'
                            ? 'Unlimited users'
                            : `${plan.features.users} team members`}
                        </span>
                      </li>
                      <li className="flex items-center text-sm">
                        <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                        <span>{plan.features.storage} storage</span>
                      </li>
                      {plan.features.analytics && (
                        <li className="flex items-center text-sm">
                          <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                          <span>Analytics dashboard</span>
                        </li>
                      )}
                      {plan.features.api && (
                        <li className="flex items-center text-sm">
                          <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                          <span>API access</span>
                        </li>
                      )}
                      {plan.features.marketplace && (
                        <li className="flex items-center text-sm">
                          <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                          <span>Marketplace access</span>
                        </li>
                      )}
                      {plan.features.prioritySupport && (
                        <li className="flex items-center text-sm">
                          <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                          <span>Priority support</span>
                        </li>
                      )}
                      {plan.features.dedicatedManager && (
                        <li className="flex items-center text-sm">
                          <FiCheck className="w-4 h-4 text-green-500 mr-3 flex-shrink-0" />
                          <span>Dedicated manager</span>
                        </li>
                      )}
                    </ul>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>

          {/* Feature Comparison Table */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5 }}
            className="bg-white rounded-2xl shadow-sm overflow-hidden"
          >
            <div className="p-6 border-b border-gray-100">
              <h2 className="text-xl font-bold text-gray-900">Compare All Features</h2>
            </div>
            
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gray-50">
                    <th className="text-left py-4 px-6 text-sm font-medium text-gray-500">Feature</th>
                    {plans.map((plan) => (
                      <th key={plan.id} className="text-center py-4 px-6 text-sm font-medium text-gray-900">
                        {plan.name}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {Object.entries(featureLabels).map(([key, label]) => (
                    <tr key={key} className="hover:bg-gray-50">
                      <td className="py-4 px-6 text-sm text-gray-600">{label}</td>
                      {plans.map((plan) => {
                        const value = plan.features[key as keyof typeof plan.features];
                        return (
                          <td key={plan.id} className="text-center py-4 px-6">
                            {typeof value === 'boolean' ? (
                              value ? (
                                <FiCheck className="w-5 h-5 text-green-500 mx-auto" />
                              ) : (
                                <FiX className="w-5 h-5 text-gray-300 mx-auto" />
                              )
                            ) : (
                              <span className="text-sm font-medium text-gray-900">
                                {value === 'unlimited' ? '∞' : value}
                              </span>
                            )}
                          </td>
                        );
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </motion.div>

          {/* Enterprise CTA */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.6 }}
            className="mt-12 bg-gradient-to-r from-gray-800 to-gray-900 rounded-2xl p-8 text-white"
          >
            <div className="flex flex-col md:flex-row items-center justify-between gap-6">
              <div>
                <h3 className="text-2xl font-bold">Need a Custom Solution?</h3>
                <p className="text-gray-300 mt-2">
                  Contact our sales team for custom pricing, dedicated support, and enterprise features.
                </p>
              </div>
              <div className="flex gap-4">
                <Button variant="secondary" size="lg">
                  <FiHeadphones className="w-5 h-5 mr-2" />
                  Contact Sales
                </Button>
              </div>
            </div>
          </motion.div>

          {/* Trust Badges */}
          <div className="mt-12 flex flex-wrap justify-center items-center gap-8 text-gray-400">
            <div className="flex items-center gap-2">
              <FiShield className="w-5 h-5" />
              <span className="text-sm">256-bit SSL Encryption</span>
            </div>
            <div className="flex items-center gap-2">
              <FiZap className="w-5 h-5" />
              <span className="text-sm">99.9% Uptime SLA</span>
            </div>
            <div className="flex items-center gap-2">
              <FiHeadphones className="w-5 h-5" />
              <span className="text-sm">24/7 Support</span>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
