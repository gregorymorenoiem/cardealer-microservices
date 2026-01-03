import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  FiArrowLeft, 
  FiCreditCard, 
  FiCheck,
  FiShield,
  FiLock,
  FiAlertCircle
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import { plans, formatCurrency, mockPaymentMethods } from '@/mocks/billingData';
import type { BillingCycle } from '@/types/billing';

interface LocationState {
  planId: string;
  billingCycle: BillingCycle;
}

export default function CheckoutPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState | null;

  const planId = state?.planId || 'professional';
  const billingCycle = state?.billingCycle || 'monthly';
  
  const plan = plans.find(p => p.id === planId) || plans[2]; // Default to professional
  
  const [selectedPaymentMethod, setSelectedPaymentMethod] = useState(
    mockPaymentMethods.find(m => m.isDefault)?.id || mockPaymentMethods[0]?.id
  );
  const [isProcessing, setIsProcessing] = useState(false);
  const [promoCode, setPromoCode] = useState('');
  const [promoApplied, setPromoApplied] = useState(false);
  const [agreeToTerms, setAgreeToTerms] = useState(false);

  const basePrice = plan.prices[billingCycle];
  const discount = promoApplied ? basePrice * 0.1 : 0; // 10% discount if promo applied
  const taxRate = 0.08; // 8% tax
  const subtotal = basePrice - discount;
  const tax = subtotal * taxRate;
  const total = subtotal + tax;

  const handleApplyPromo = () => {
    if (promoCode.toLowerCase() === 'save10') {
      setPromoApplied(true);
    }
  };

  const handleCheckout = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!agreeToTerms) return;

    setIsProcessing(true);
    
    // Simulate payment processing
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Navigate to success page or billing dashboard
    navigate('/billing', { 
      state: { 
        success: true, 
        message: `Successfully subscribed to ${plan.name} plan!` 
      } 
    });
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <Link to="/billing/plans" className="inline-flex items-center text-gray-600 hover:text-gray-900 mb-4">
              <FiArrowLeft className="w-4 h-4 mr-2" />
              Back to Plans
            </Link>
            <h1 className="text-3xl font-bold text-gray-900">Checkout</h1>
            <p className="text-gray-600 mt-1">
              Complete your subscription to {plan.name}
            </p>
          </div>

          <form onSubmit={handleCheckout}>
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Left Column - Payment Form */}
              <div className="lg:col-span-2 space-y-6">
                {/* Order Summary (Mobile) */}
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="lg:hidden bg-white rounded-xl shadow-sm p-6"
                >
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">Order Summary</h2>
                  <div className="flex items-center justify-between pb-4 border-b border-gray-100">
                    <div>
                      <p className="font-medium text-gray-900">{plan.name} Plan</p>
                      <p className="text-sm text-gray-500 capitalize">{billingCycle} billing</p>
                    </div>
                    <p className="text-lg font-semibold">{formatCurrency(basePrice)}</p>
                  </div>
                  <div className="pt-4 flex justify-between text-lg font-bold">
                    <span>Total</span>
                    <span className="text-primary-600">{formatCurrency(total)}</span>
                  </div>
                </motion.div>

                {/* Payment Method Selection */}
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.1 }}
                  className="bg-white rounded-xl shadow-sm p-6"
                >
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">Payment Method</h2>
                  
                  <div className="space-y-3">
                    {mockPaymentMethods.map((method) => (
                      <label
                        key={method.id}
                        className={`flex items-center p-4 border-2 rounded-lg cursor-pointer transition-all ${
                          selectedPaymentMethod === method.id
                            ? 'border-primary-500 bg-primary-50'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        <input
                          type="radio"
                          name="paymentMethod"
                          value={method.id}
                          checked={selectedPaymentMethod === method.id}
                          onChange={(e) => setSelectedPaymentMethod(e.target.value)}
                          className="sr-only"
                        />
                        <div className="flex items-center flex-1">
                          <div className="w-10 h-6 bg-gray-200 rounded flex items-center justify-center text-xs font-bold mr-4">
                            {method.card?.brand.substring(0, 4).toUpperCase()}
                          </div>
                          <div>
                            <p className="font-medium text-gray-900">
                              •••• •••• •••• {method.card?.last4}
                            </p>
                            <p className="text-sm text-gray-500">
                              Expires {method.card?.expMonth}/{method.card?.expYear}
                            </p>
                          </div>
                        </div>
                        {selectedPaymentMethod === method.id && (
                          <FiCheck className="w-5 h-5 text-primary-600" />
                        )}
                      </label>
                    ))}
                    
                    {/* Add new card option */}
                    <Link
                      to="/billing/payment-methods"
                      className="flex items-center p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-all"
                    >
                      <FiCreditCard className="w-5 h-5 text-gray-400 mr-3" />
                      <span className="text-gray-600">Add new payment method</span>
                    </Link>
                  </div>
                </motion.div>

                {/* Promo Code */}
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.2 }}
                  className="bg-white rounded-xl shadow-sm p-6"
                >
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">Promo Code</h2>
                  <div className="flex gap-3">
                    <input
                      type="text"
                      placeholder="Enter promo code"
                      value={promoCode}
                      onChange={(e) => setPromoCode(e.target.value)}
                      disabled={promoApplied}
                      className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent disabled:bg-gray-100"
                    />
                    <Button
                      type="button"
                      variant="outline"
                      onClick={handleApplyPromo}
                      disabled={promoApplied || !promoCode}
                    >
                      {promoApplied ? 'Applied!' : 'Apply'}
                    </Button>
                  </div>
                  {promoApplied && (
                    <p className="text-sm text-green-600 mt-2 flex items-center gap-1">
                      <FiCheck className="w-4 h-4" />
                      Promo code applied! You saved {formatCurrency(discount)}
                    </p>
                  )}
                  <p className="text-xs text-gray-500 mt-2">
                    Try: SAVE10 for 10% off
                  </p>
                </motion.div>

                {/* Terms and Conditions */}
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.3 }}
                  className="bg-white rounded-xl shadow-sm p-6"
                >
                  <label className="flex items-start gap-3 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={agreeToTerms}
                      onChange={(e) => setAgreeToTerms(e.target.checked)}
                      className="mt-1 w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                    />
                    <span className="text-sm text-gray-600">
                      I agree to the{' '}
                      <Link to="/terms" className="text-primary-600 hover:underline">
                        Terms of Service
                      </Link>{' '}
                      and{' '}
                      <Link to="/privacy" className="text-primary-600 hover:underline">
                        Privacy Policy
                      </Link>
                      . I understand that my subscription will automatically renew and I can cancel anytime.
                    </span>
                  </label>
                </motion.div>

                {/* Submit Button (Mobile) */}
                <div className="lg:hidden">
                  <Button
                    type="submit"
                    size="lg"
                    className="w-full"
                    disabled={!agreeToTerms || isProcessing}
                  >
                    {isProcessing ? (
                      <>
                        <FiLock className="w-5 h-5 mr-2 animate-pulse" />
                        Processing...
                      </>
                    ) : (
                      <>
                        <FiLock className="w-5 h-5 mr-2" />
                        Pay {formatCurrency(total)}
                      </>
                    )}
                  </Button>
                </div>
              </div>

              {/* Right Column - Order Summary (Desktop) */}
              <div className="hidden lg:block">
                <motion.div
                  initial={{ opacity: 0, x: 20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.2 }}
                  className="bg-white rounded-xl shadow-sm p-6 sticky top-8"
                >
                  <h2 className="text-lg font-semibold text-gray-900 mb-4">Order Summary</h2>
                  
                  {/* Plan Details */}
                  <div className="pb-4 mb-4 border-b border-gray-100">
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-medium text-gray-900">{plan.name} Plan</span>
                      <span className="font-medium">{formatCurrency(basePrice)}</span>
                    </div>
                    <p className="text-sm text-gray-500 capitalize">{billingCycle} billing</p>
                  </div>

                  {/* Plan Features Preview */}
                  <div className="pb-4 mb-4 border-b border-gray-100">
                    <p className="text-sm font-medium text-gray-700 mb-2">Includes:</p>
                    <ul className="space-y-1.5 text-sm text-gray-600">
                      <li className="flex items-center gap-2">
                        <FiCheck className="w-4 h-4 text-green-500" />
                        {plan.features.listings === 'unlimited' 
                          ? 'Unlimited listings' 
                          : `${plan.features.listings} listings`}
                      </li>
                      <li className="flex items-center gap-2">
                        <FiCheck className="w-4 h-4 text-green-500" />
                        {plan.features.users === 'unlimited'
                          ? 'Unlimited users'
                          : `${plan.features.users} team members`}
                      </li>
                      <li className="flex items-center gap-2">
                        <FiCheck className="w-4 h-4 text-green-500" />
                        {plan.features.storage} storage
                      </li>
                    </ul>
                  </div>

                  {/* Price Breakdown */}
                  <div className="space-y-2 pb-4 mb-4 border-b border-gray-100">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Subtotal</span>
                      <span>{formatCurrency(basePrice)}</span>
                    </div>
                    {promoApplied && (
                      <div className="flex justify-between text-sm text-green-600">
                        <span>Promo Discount</span>
                        <span>-{formatCurrency(discount)}</span>
                      </div>
                    )}
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Tax ({(taxRate * 100).toFixed(0)}%)</span>
                      <span>{formatCurrency(tax)}</span>
                    </div>
                  </div>

                  {/* Total */}
                  <div className="flex justify-between text-lg font-bold mb-6">
                    <span>Total</span>
                    <span className="text-primary-600">{formatCurrency(total)}</span>
                  </div>

                  {/* Submit Button */}
                  <Button
                    type="submit"
                    size="lg"
                    className="w-full"
                    disabled={!agreeToTerms || isProcessing}
                  >
                    {isProcessing ? (
                      <>
                        <FiLock className="w-5 h-5 mr-2 animate-pulse" />
                        Processing...
                      </>
                    ) : (
                      <>
                        <FiLock className="w-5 h-5 mr-2" />
                        Pay {formatCurrency(total)}
                      </>
                    )}
                  </Button>

                  {/* Security Note */}
                  <div className="mt-4 flex items-center justify-center gap-2 text-xs text-gray-500">
                    <FiShield className="w-4 h-4" />
                    <span>Secure 256-bit SSL encryption</span>
                  </div>
                </motion.div>
              </div>
            </div>
          </form>

          {/* Trust Badges */}
          <div className="mt-12 flex flex-wrap justify-center items-center gap-8 text-gray-400">
            <div className="flex items-center gap-2">
              <FiShield className="w-5 h-5" />
              <span className="text-sm">Secure Payment</span>
            </div>
            <div className="flex items-center gap-2">
              <FiAlertCircle className="w-5 h-5" />
              <span className="text-sm">Cancel Anytime</span>
            </div>
            <div className="flex items-center gap-2">
              <FiLock className="w-5 h-5" />
              <span className="text-sm">Money-Back Guarantee</span>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
