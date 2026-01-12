import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  FiArrowLeft,
  FiCreditCard,
  FiPlus,
  FiTrash2,
  FiStar,
  FiCheck,
  FiAlertCircle,
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import {
  usePaymentMethods,
  useSetDefaultPaymentMethod,
  useRemovePaymentMethod,
} from '@/hooks/useBilling';
import { useAuthStore } from '@/store/authStore';
import { mockPaymentMethods } from '@/mocks/billingData';
import type { PaymentMethodInfo } from '@/types/billing';

export default function PaymentMethodsPage() {
  const location = useLocation();

  // Determine if we're in dealer context
  const isDealerContext = location.pathname.startsWith('/dealer');
  const routePrefix = isDealerContext ? '/dealer' : '/billing';

  // Get user info
  const { user } = useAuthStore();
  const dealerId = user?.dealerId || user?.id || '';

  // Use TanStack Query hooks
  const { data: fetchedPaymentMethods } = usePaymentMethods(dealerId);
  const setDefaultMutation = useSetDefaultPaymentMethod();
  const removeMutation = useRemovePaymentMethod();

  // Use fetched data or fallback to mocks
  const initialMethods = fetchedPaymentMethods?.length ? fetchedPaymentMethods : mockPaymentMethods;

  const [paymentMethods, setPaymentMethods] = useState<PaymentMethodInfo[]>(initialMethods);
  const [showAddModal, setShowAddModal] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);

  // Update local state when API data changes
  useEffect(() => {
    if (fetchedPaymentMethods?.length) {
      setPaymentMethods(fetchedPaymentMethods);
    }
  }, [fetchedPaymentMethods]);

  const handleSetDefault = async (id: string) => {
    try {
      await setDefaultMutation.mutateAsync({ dealerId, paymentMethodId: id });
    } catch {
      // Fallback to local state update
      setPaymentMethods((methods) =>
        methods.map((m) => ({
          ...m,
          isDefault: m.id === id,
        }))
      );
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await removeMutation.mutateAsync({ dealerId, paymentMethodId: id });
    } catch {
      // Fallback to local state update
      setPaymentMethods((methods) => methods.filter((m) => m.id !== id));
    }
    setDeleteConfirm(null);
  };

  const getCardBrandIcon = (brand: string) => {
    // In a real app, these would be actual brand logos
    const brandColors: Record<string, string> = {
      visa: 'bg-blue-600',
      mastercard: 'bg-orange-500',
      amex: 'bg-blue-500',
      discover: 'bg-orange-400',
    };
    return brandColors[brand.toLowerCase()] || 'bg-gray-600';
  };

  const isCardExpiringSoon = (month: number, year: number) => {
    const now = new Date();
    const expiry = new Date(year, month - 1);
    const threeMonthsFromNow = new Date();
    threeMonthsFromNow.setMonth(threeMonthsFromNow.getMonth() + 3);
    return expiry <= threeMonthsFromNow && expiry >= now;
  };

  const isCardExpired = (month: number, year: number) => {
    const now = new Date();
    const expiry = new Date(year, month);
    return expiry < now;
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <Link
              to={isDealerContext ? '/dealer/billing' : '/billing'}
              className="inline-flex items-center text-gray-600 hover:text-gray-900 mb-4"
            >
              <FiArrowLeft className="w-4 h-4 mr-2" />
              Back to Billing
            </Link>
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-3xl font-bold text-gray-900">Payment Methods</h1>
                <p className="text-gray-600 mt-1">Manage your payment methods for subscriptions</p>
              </div>
              <Button onClick={() => setShowAddModal(true)}>
                <FiPlus className="w-4 h-4 mr-2" />
                Add Method
              </Button>
            </div>
          </div>

          {/* Payment Methods List */}
          <div className="space-y-4">
            {paymentMethods.map((method, index) => (
              <motion.div
                key={method.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
                className={`bg-white rounded-xl shadow-sm p-6 border-2 ${
                  method.isDefault ? 'border-primary-500' : 'border-transparent'
                }`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-4">
                    {method.type === 'card' && method.card && (
                      <>
                        <div
                          className={`w-12 h-8 rounded ${getCardBrandIcon(method.card.brand)} flex items-center justify-center text-white text-xs font-bold`}
                        >
                          {method.card.brand.substring(0, 4).toUpperCase()}
                        </div>
                        <div>
                          <div className="flex items-center gap-2">
                            <p className="font-medium text-gray-900">
                              {method.card.brand} •••• {method.card.last4}
                            </p>
                            {method.isDefault && (
                              <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium bg-primary-100 text-primary-800 rounded-full">
                                <FiStar className="w-3 h-3 mr-1" />
                                Default
                              </span>
                            )}
                          </div>
                          <div className="flex items-center gap-2 mt-1">
                            <p className="text-sm text-gray-500">
                              Expires {method.card.expMonth}/{method.card.expYear}
                            </p>
                            {isCardExpired(method.card.expMonth, method.card.expYear) && (
                              <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium bg-red-100 text-red-800 rounded-full">
                                <FiAlertCircle className="w-3 h-3 mr-1" />
                                Expired
                              </span>
                            )}
                            {!isCardExpired(method.card.expMonth, method.card.expYear) &&
                              isCardExpiringSoon(method.card.expMonth, method.card.expYear) && (
                                <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium bg-yellow-100 text-yellow-800 rounded-full">
                                  <FiAlertCircle className="w-3 h-3 mr-1" />
                                  Expiring Soon
                                </span>
                              )}
                          </div>
                        </div>
                      </>
                    )}
                    {method.type === 'bank_account' && method.bankAccount && (
                      <>
                        <div className="w-12 h-8 rounded bg-gray-600 flex items-center justify-center text-white text-xs font-bold">
                          BANK
                        </div>
                        <div>
                          <div className="flex items-center gap-2">
                            <p className="font-medium text-gray-900">
                              {method.bankAccount.bankName} •••• {method.bankAccount.last4}
                            </p>
                            {method.isDefault && (
                              <span className="inline-flex items-center px-2 py-0.5 text-xs font-medium bg-primary-100 text-primary-800 rounded-full">
                                <FiStar className="w-3 h-3 mr-1" />
                                Default
                              </span>
                            )}
                          </div>
                          <p className="text-sm text-gray-500 capitalize mt-1">
                            {method.bankAccount.accountType} Account
                          </p>
                        </div>
                      </>
                    )}
                  </div>

                  <div className="flex items-center gap-2">
                    {!method.isDefault && (
                      <Button variant="ghost" size="sm" onClick={() => handleSetDefault(method.id)}>
                        <FiCheck className="w-4 h-4 mr-1" />
                        Set Default
                      </Button>
                    )}
                    {deleteConfirm === method.id ? (
                      <div className="flex items-center gap-2">
                        <Button variant="danger" size="sm" onClick={() => handleDelete(method.id)}>
                          Confirm
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => setDeleteConfirm(null)}>
                          Cancel
                        </Button>
                      </div>
                    ) : (
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setDeleteConfirm(method.id)}
                        disabled={method.isDefault && paymentMethods.length > 1}
                        className="text-gray-400 hover:text-red-600"
                      >
                        <FiTrash2 className="w-4 h-4" />
                      </Button>
                    )}
                  </div>
                </div>
              </motion.div>
            ))}
          </div>

          {/* Empty State */}
          {paymentMethods.length === 0 && (
            <div className="text-center py-12 bg-white rounded-xl shadow-sm">
              <FiCreditCard className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900">No payment methods</h3>
              <p className="text-gray-500 mt-1 mb-4">
                Add a payment method to continue your subscription
              </p>
              <Button onClick={() => setShowAddModal(true)}>
                <FiPlus className="w-4 h-4 mr-2" />
                Add Payment Method
              </Button>
            </div>
          )}

          {/* Add Payment Method Modal */}
          {showAddModal && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
              <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6"
              >
                <h2 className="text-xl font-bold text-gray-900 mb-4">Add Payment Method</h2>

                <form
                  onSubmit={(e) => {
                    e.preventDefault();
                    // Mock adding a new card
                    const newMethod: PaymentMethodInfo = {
                      id: `pm_${Date.now()}`,
                      type: 'card',
                      isDefault: paymentMethods.length === 0,
                      card: {
                        brand: 'Visa',
                        last4: '1234',
                        expMonth: 12,
                        expYear: 2028,
                      },
                      createdAt: new Date().toISOString(),
                    };
                    setPaymentMethods([...paymentMethods, newMethod]);
                    setShowAddModal(false);
                  }}
                >
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Card Number
                      </label>
                      <input
                        type="text"
                        placeholder="4242 4242 4242 4242"
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        required
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Expiry Date
                        </label>
                        <input
                          type="text"
                          placeholder="MM/YY"
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          required
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">CVC</label>
                        <input
                          type="text"
                          placeholder="123"
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          required
                        />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Name on Card
                      </label>
                      <input
                        type="text"
                        placeholder="John Doe"
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        required
                      />
                    </div>
                  </div>

                  <div className="flex gap-3 mt-6">
                    <Button
                      type="button"
                      variant="outline"
                      className="flex-1"
                      onClick={() => setShowAddModal(false)}
                    >
                      Cancel
                    </Button>
                    <Button type="submit" className="flex-1">
                      Add Card
                    </Button>
                  </div>
                </form>

                <p className="text-xs text-gray-500 text-center mt-4">
                  Your card information is encrypted and secure.
                </p>
              </motion.div>
            </div>
          )}

          {/* Security Note */}
          <div className="mt-8 p-4 bg-blue-50 rounded-xl">
            <div className="flex items-start gap-3">
              <FiAlertCircle className="w-5 h-5 text-blue-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-blue-900">Secure Payment Processing</p>
                <p className="text-sm text-blue-700 mt-1">
                  All payment information is encrypted using industry-standard 256-bit SSL
                  encryption. We never store your full card number on our servers.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
