/**
 * Dealer Payment Setup Page
 *
 * Step 4 of dealer onboarding - Pay subscription to OKLA
 *
 * IMPORTANT: Dealers PAY OKLA for advertising services.
 * This is NOT for dealers to receive payments.
 * Vehicle transactions between buyers/sellers are EXTERNAL to the platform.
 *
 * Flow:
 * 1. Show selected plan with pricing
 * 2. Apply Early Bird discount if eligible (20% off)
 * 3. Collect card details
 * 4. Create subscription via AzulPaymentService
 * 5. OKLA receives the payment, dealer gets activated
 */

import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  CreditCard,
  ArrowRight,
  ArrowLeft,
  AlertCircle,
  CheckCircle,
  Shield,
  Sparkles,
  Check,
  Info,
} from 'lucide-react';
import {
  useOnboardingProgress,
  useDealerOnboardingStatus,
  useUpdateAzulIds,
} from '@/hooks/useDealerOnboarding';
import { useDealerSubscription } from '@/hooks/useAzulPayment';
import {
  DEALER_PLANS,
  getPlanById,
  formatDOPAmount,
  calculateEarlyBirdPrice,
  validateCardNumber,
  getCardBrand,
} from '@/services/azulPaymentService';

export const DealerPaymentSetupPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const planId = searchParams.get('plan') || 'professional';

  const { dealerId, status, hasOnboardingInProgress } = useOnboardingProgress();
  const { data: onboardingData } = useDealerOnboardingStatus(dealerId);
  const subscriptionMutation = useDealerSubscription();
  const updateAzulIdsMutation = useUpdateAzulIds();

  // Form state
  const [cardNumber, setCardNumber] = useState('');
  const [cardExpiry, setCardExpiry] = useState('');
  const [cardCVV, setCardCVV] = useState('');
  const [cardholderName, setCardholderName] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isProcessing, setIsProcessing] = useState(false);

  // Get plan info
  const selectedPlan = getPlanById(planId) || DEALER_PLANS[1];
  const isEarlyBird = onboardingData?.isEarlyBirdEligible || false;
  const finalPrice = isEarlyBird
    ? calculateEarlyBirdPrice(selectedPlan)
    : selectedPlan.monthlyPrice;
  const savings = isEarlyBird ? selectedPlan.monthlyPrice - finalPrice : 0;

  // Detect card brand
  const cardBrand = getCardBrand(cardNumber.replace(/\s/g, ''));

  // Redirect if no onboarding in progress
  useEffect(() => {
    if (!hasOnboardingInProgress) {
      navigate('/dealer/onboarding');
    }
  }, [hasOnboardingInProgress, navigate]);

  // Redirect based on status
  useEffect(() => {
    if (status) {
      if (!status.isEmailVerified) {
        navigate('/dealer/onboarding/verify-email');
      } else if (!status.documentsSubmitted) {
        navigate('/dealer/onboarding/documents');
      } else if (status.azulConfigured) {
        navigate('/dealer/onboarding/status');
      }
    }
  }, [status, navigate]);

  // Format card number with spaces
  const formatCardNumber = (value: string): string => {
    const cleaned = value.replace(/\D/g, '').slice(0, 16);
    const groups = cleaned.match(/.{1,4}/g);
    return groups ? groups.join(' ') : cleaned;
  };

  // Format expiry as MM/YY
  const formatExpiry = (value: string): string => {
    const cleaned = value.replace(/\D/g, '').slice(0, 4);
    if (cleaned.length >= 3) {
      return cleaned.slice(0, 2) + '/' + cleaned.slice(2);
    }
    return cleaned;
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    const cleanCardNumber = cardNumber.replace(/\s/g, '');

    if (!cleanCardNumber) {
      newErrors.cardNumber = 'El número de tarjeta es requerido';
    } else if (!validateCardNumber(cleanCardNumber)) {
      newErrors.cardNumber = 'Número de tarjeta inválido';
    }

    if (!cardExpiry) {
      newErrors.cardExpiry = 'La fecha de expiración es requerida';
    } else {
      const [month, year] = cardExpiry.split('/');
      const expMonth = parseInt(month, 10);
      const expYear = parseInt('20' + year, 10);
      const now = new Date();
      const currentMonth = now.getMonth() + 1;
      const currentYear = now.getFullYear();

      if (expMonth < 1 || expMonth > 12) {
        newErrors.cardExpiry = 'Mes inválido';
      } else if (expYear < currentYear || (expYear === currentYear && expMonth < currentMonth)) {
        newErrors.cardExpiry = 'Tarjeta expirada';
      }
    }

    if (!cardCVV) {
      newErrors.cardCVV = 'El CVV es requerido';
    } else if (!/^\d{3,4}$/.test(cardCVV)) {
      newErrors.cardCVV = 'CVV inválido';
    }

    if (!cardholderName.trim()) {
      newErrors.cardholderName = 'El nombre del titular es requerido';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!dealerId || !validateForm()) return;

    setIsProcessing(true);

    try {
      const [month, year] = cardExpiry.split('/');

      // Step 1: Create subscription in AzulPaymentService
      // This is where the dealer PAYS OKLA for advertising services
      const subscriptionResult = await subscriptionMutation.mutateAsync({
        dealerId,
        planId: selectedPlan.id,
        planName: selectedPlan.name,
        amount: finalPrice,
        isEarlyBird,
        cardNumber: cardNumber.replace(/\s/g, ''),
        cardExpiryMonth: month,
        cardExpiryYear: '20' + year,
        cardCVV,
        cardholderName: cardholderName.trim(),
        customerEmail: onboardingData?.email,
        customerPhone: onboardingData?.phone,
      });

      // Step 2: Update dealer onboarding with Azul IDs
      // The dealer is a CUSTOMER (payer), not a merchant
      await updateAzulIdsMutation.mutateAsync({
        dealerId,
        data: {
          azulCustomerId: subscriptionResult.subscriptionId, // Use subscription ID as customer reference
          azulSubscriptionId: subscriptionResult.azulSubscriptionId,
          enrollEarlyBird: isEarlyBird,
        },
      });

      navigate('/dealer/onboarding/status');
    } catch (error) {
      // Error handled by mutation
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Progress indicator */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-2xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between text-sm text-gray-500">
            <span className="text-blue-600 font-medium">Paso 4 de 5</span>
            <span>Pago de Suscripción</span>
          </div>
          <div className="mt-2 h-2 bg-gray-200 rounded-full">
            <div className="h-2 bg-blue-600 rounded-full w-4/5" />
          </div>
        </div>
      </div>

      <div className="max-w-2xl mx-auto px-4 py-12">
        <button
          onClick={() => navigate('/dealer/onboarding/documents')}
          className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
          Volver
        </button>

        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <CreditCard className="h-10 w-10 text-blue-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Paga tu Suscripción</h1>
          <p className="text-gray-600">
            Activa tu cuenta de dealer y comienza a publicar vehículos
          </p>
        </div>

        {/* Info Banner */}
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-6">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
            <div className="text-sm text-blue-800">
              <p className="font-medium mb-1">¿Qué incluye mi suscripción?</p>
              <p>
                Tu suscripción mensual te permite publicar y gestionar tus vehículos en OKLA. Las
                transacciones de venta de vehículos son directas entre tú y el comprador.
              </p>
            </div>
          </div>
        </div>

        {/* Plan Summary */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Plan {selectedPlan.name}</h2>
            {isEarlyBird && (
              <span className="bg-gradient-to-r from-amber-100 to-orange-100 text-amber-800 text-xs font-semibold px-3 py-1 rounded-full flex items-center gap-1">
                <Sparkles className="h-3 w-3" />
                Early Bird
              </span>
            )}
          </div>

          <div className="flex items-baseline gap-2 mb-4">
            <span className="text-3xl font-bold text-gray-900">{formatDOPAmount(finalPrice)}</span>
            <span className="text-gray-500">/mes</span>
            {isEarlyBird && (
              <>
                <span className="text-lg text-gray-400 line-through ml-2">
                  {formatDOPAmount(selectedPlan.monthlyPrice)}
                </span>
                <span className="text-green-600 text-sm font-medium">
                  (Ahorras {formatDOPAmount(savings)})
                </span>
              </>
            )}
          </div>

          {isEarlyBird && (
            <div className="bg-amber-50 rounded-lg p-3 mb-4">
              <p className="text-sm text-amber-800">
                <strong>🎁 Early Bird:</strong> 3 meses GRATIS + 20% descuento de por vida + Badge
                Fundador
              </p>
            </div>
          )}

          <ul className="space-y-2">
            {selectedPlan.features.slice(0, 5).map((feature, idx) => (
              <li key={idx} className="flex items-center gap-2 text-sm text-gray-600">
                <Check className="h-4 w-4 text-green-500 flex-shrink-0" />
                {feature}
              </li>
            ))}
          </ul>
        </div>

        {/* Payment Form */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-6">Datos de Pago</h2>

          <div className="space-y-4">
            {/* Card Number */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Número de Tarjeta *
              </label>
              <div className="relative">
                <input
                  type="text"
                  value={cardNumber}
                  onChange={(e) => {
                    setCardNumber(formatCardNumber(e.target.value));
                    if (errors.cardNumber) {
                      setErrors((prev) => ({ ...prev, cardNumber: '' }));
                    }
                  }}
                  placeholder="1234 5678 9012 3456"
                  className={`w-full px-4 py-3 rounded-lg border ${
                    errors.cardNumber ? 'border-red-500' : 'border-gray-300'
                  } focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono pr-12`}
                />
                {cardBrand && (
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm font-medium text-gray-500">
                    {cardBrand}
                  </span>
                )}
              </div>
              {errors.cardNumber && (
                <p className="mt-1 text-sm text-red-500">{errors.cardNumber}</p>
              )}
            </div>

            {/* Expiry and CVV */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Fecha de Expiración *
                </label>
                <input
                  type="text"
                  value={cardExpiry}
                  onChange={(e) => {
                    setCardExpiry(formatExpiry(e.target.value));
                    if (errors.cardExpiry) {
                      setErrors((prev) => ({ ...prev, cardExpiry: '' }));
                    }
                  }}
                  placeholder="MM/YY"
                  className={`w-full px-4 py-3 rounded-lg border ${
                    errors.cardExpiry ? 'border-red-500' : 'border-gray-300'
                  } focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono`}
                />
                {errors.cardExpiry && (
                  <p className="mt-1 text-sm text-red-500">{errors.cardExpiry}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">CVV *</label>
                <input
                  type="text"
                  value={cardCVV}
                  onChange={(e) => {
                    const cleaned = e.target.value.replace(/\D/g, '').slice(0, 4);
                    setCardCVV(cleaned);
                    if (errors.cardCVV) {
                      setErrors((prev) => ({ ...prev, cardCVV: '' }));
                    }
                  }}
                  placeholder="123"
                  className={`w-full px-4 py-3 rounded-lg border ${
                    errors.cardCVV ? 'border-red-500' : 'border-gray-300'
                  } focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono`}
                />
                {errors.cardCVV && <p className="mt-1 text-sm text-red-500">{errors.cardCVV}</p>}
              </div>
            </div>

            {/* Cardholder Name */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Nombre del Titular *
              </label>
              <input
                type="text"
                value={cardholderName}
                onChange={(e) => {
                  setCardholderName(e.target.value.toUpperCase());
                  if (errors.cardholderName) {
                    setErrors((prev) => ({ ...prev, cardholderName: '' }));
                  }
                }}
                placeholder="COMO APARECE EN LA TARJETA"
                className={`w-full px-4 py-3 rounded-lg border ${
                  errors.cardholderName ? 'border-red-500' : 'border-gray-300'
                } focus:ring-2 focus:ring-blue-500 focus:border-transparent uppercase`}
              />
              {errors.cardholderName && (
                <p className="mt-1 text-sm text-red-500">{errors.cardholderName}</p>
              )}
            </div>
          </div>
        </div>

        {/* Error Display */}
        {subscriptionMutation.isError && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-4 mb-6">
            <div className="flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-red-600 mt-0.5" />
              <div>
                <p className="text-sm text-red-800 font-medium">Error al procesar el pago</p>
                <p className="text-sm text-red-700">
                  {(subscriptionMutation.error as Error)?.message ||
                    'Por favor verifica los datos de tu tarjeta e intenta nuevamente.'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Submit Button */}
        <button
          onClick={handleSubmit}
          disabled={isProcessing}
          className="w-full px-8 py-4 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 
            text-white font-medium rounded-xl transition-colors 
            flex items-center justify-center gap-2"
        >
          {isProcessing ? (
            <>
              <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
              Procesando pago...
            </>
          ) : (
            <>
              <CheckCircle className="h-5 w-5" />
              Pagar {formatDOPAmount(finalPrice)}
              {isEarlyBird ? ' (Primer cobro en 90 días)' : ''}
              <ArrowRight className="h-5 w-5" />
            </>
          )}
        </button>

        {/* Security Info */}
        <div className="bg-green-50 border border-green-200 rounded-xl p-4 mt-6">
          <div className="flex items-start gap-3">
            <Shield className="h-5 w-5 text-green-600 mt-0.5" />
            <div>
              <p className="text-sm text-green-800 font-medium mb-1">Pago 100% seguro</p>
              <p className="text-sm text-green-700">
                Procesado por Azul (Banco Popular) con encriptación de nivel bancario. Puedes
                cancelar tu suscripción en cualquier momento.
              </p>
            </div>
          </div>
        </div>

        {/* Accepted Cards */}
        <div className="flex items-center justify-center gap-4 mt-6">
          <span className="text-xs text-gray-500">Aceptamos:</span>
          <div className="flex items-center gap-2">
            <div className="bg-white border border-gray-200 rounded px-2 py-1 text-xs font-medium text-blue-700">
              VISA
            </div>
            <div className="bg-white border border-gray-200 rounded px-2 py-1 text-xs font-medium text-orange-600">
              Mastercard
            </div>
          </div>
          <span className="text-xs text-gray-400">Procesado por Azul</span>
        </div>
      </div>
    </div>
  );
};

export default DealerPaymentSetupPage;
