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
  FiLoader,
  FiGlobe,
  FiMapPin,
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import {
  usePaymentMethods,
  useAddPaymentMethod,
  useSetDefaultPaymentMethod,
  useRemovePaymentMethod,
} from '@/hooks/useBilling';
import {
  availableGateways,
  type PaymentGateway,
  type AddPaymentMethodRequest,
} from '@/services/billingService';
import { useAuthStore } from '@/store/authStore';
import { mockPaymentMethods } from '@/mocks/billingData';
import type { PaymentMethodInfo } from '@/types/billing';

// Form state interface
interface CardFormData {
  cardNumber: string;
  expiry: string;
  cvv: string;
  cardHolderName: string;
  gateway: PaymentGateway;
  setAsDefault: boolean;
}

const initialFormData: CardFormData = {
  cardNumber: '',
  expiry: '',
  cvv: '',
  cardHolderName: '',
  gateway: 'Azul',
  setAsDefault: true,
};

export default function PaymentMethodsPage() {
  const location = useLocation();

  // Determine if we're in dealer context or cuenta context
  const isDealerContext = location.pathname.startsWith('/dealer');
  const isCuentaContext = location.pathname.startsWith('/cuenta');
  const routePrefix = isDealerContext ? '/dealer' : isCuentaContext ? '/cuenta' : '/billing';

  // Get user info
  const { user } = useAuthStore();
  const dealerId = user?.dealerId || user?.id || '';

  // Use TanStack Query hooks
  const { data: fetchedPaymentMethods, isLoading, refetch } = usePaymentMethods(dealerId);
  const addPaymentMethodMutation = useAddPaymentMethod({
    onSuccess: () => {
      setShowAddModal(false);
      setFormData(initialFormData);
      setFormError('');
      refetch();
    },
    onError: (error) => {
      setFormError(error.message || 'Error al agregar la tarjeta');
    },
  });
  const setDefaultMutation = useSetDefaultPaymentMethod();
  const removeMutation = useRemovePaymentMethod();

  // Use fetched data or fallback to mocks
  const initialMethods = fetchedPaymentMethods?.length ? fetchedPaymentMethods : mockPaymentMethods;

  const [paymentMethods, setPaymentMethods] = useState<PaymentMethodInfo[]>(initialMethods);
  const [showAddModal, setShowAddModal] = useState(false);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const [formData, setFormData] = useState<CardFormData>(initialFormData);
  const [formError, setFormError] = useState<string>('');

  // Update local state when API data changes
  useEffect(() => {
    if (fetchedPaymentMethods?.length) {
      setPaymentMethods(fetchedPaymentMethods);
    }
  }, [fetchedPaymentMethods]);

  // Parse expiry MM/YY to month and year
  const parseExpiry = (expiry: string): { month: number; year: number } | null => {
    const match = expiry.match(/^(\d{2})\/(\d{2})$/);
    if (!match) return null;
    const month = parseInt(match[1], 10);
    const year = 2000 + parseInt(match[2], 10);
    if (month < 1 || month > 12) return null;
    return { month, year };
  };

  // Handle form submission
  const handleAddCard = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    // Validate card number (basic: 13-19 digits)
    const cleanCardNumber = formData.cardNumber.replace(/\s+/g, '');
    if (!/^\d{13,19}$/.test(cleanCardNumber)) {
      setFormError('Número de tarjeta inválido');
      return;
    }

    // Validate expiry
    const expiry = parseExpiry(formData.expiry);
    if (!expiry) {
      setFormError('Fecha de vencimiento inválida (use MM/YY)');
      return;
    }

    // Validate CVV (3-4 digits)
    if (!/^\d{3,4}$/.test(formData.cvv)) {
      setFormError('CVV inválido');
      return;
    }

    // Validate cardholder name
    if (formData.cardHolderName.trim().length < 2) {
      setFormError('Nombre del titular requerido');
      return;
    }

    const request: AddPaymentMethodRequest = {
      gateway: formData.gateway,
      card: {
        number: cleanCardNumber,
        expMonth: expiry.month,
        expYear: expiry.year,
        cvv: formData.cvv,
        cardHolderName: formData.cardHolderName.toUpperCase(),
      },
      setAsDefault: formData.setAsDefault,
    };

    addPaymentMethodMutation.mutate(request);
  };

  // Format card number with spaces
  const formatCardNumber = (value: string) => {
    const v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
    const matches = v.match(/\d{4,16}/g);
    const match = (matches && matches[0]) || '';
    const parts = [];
    for (let i = 0, len = match.length; i < len; i += 4) {
      parts.push(match.substring(i, i + 4));
    }
    return parts.length ? parts.join(' ') : value;
  };

  // Format expiry date
  const formatExpiry = (value: string) => {
    const v = value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
    if (v.length >= 2) {
      return v.substring(0, 2) + '/' + v.substring(2, 4);
    }
    return v;
  };

  const handleSetDefault = async (id: string) => {
    try {
      await setDefaultMutation.mutateAsync(id);
      refetch();
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
      await removeMutation.mutateAsync(id);
      refetch();
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

          {/* Loading State */}
          {isLoading && (
            <div className="flex items-center justify-center py-12">
              <FiLoader className="w-8 h-8 animate-spin text-primary-500" />
            </div>
          )}

          {/* Add Payment Method Modal */}
          {showAddModal && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4 overflow-y-auto">
              <motion.div
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                className="bg-white rounded-2xl shadow-xl max-w-lg w-full p-6 my-8"
              >
                <h2 className="text-xl font-bold text-gray-900 mb-2">Agregar Método de Pago</h2>
                <p className="text-gray-600 text-sm mb-6">
                  Ingresa los datos de tu tarjeta de crédito o débito
                </p>

                {formError && (
                  <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-700">
                    <FiAlertCircle className="w-4 h-4 flex-shrink-0" />
                    <span className="text-sm">{formError}</span>
                  </div>
                )}

                <form onSubmit={handleAddCard}>
                  {/* Gateway Selection */}
                  <div className="mb-6">
                    <label className="block text-sm font-medium text-gray-700 mb-3">
                      Selecciona la pasarela de pago
                    </label>
                    <div className="grid grid-cols-1 gap-2">
                      {availableGateways.map((gateway) => (
                        <label
                          key={gateway.id}
                          className={`relative flex items-center p-4 border-2 rounded-xl cursor-pointer transition-all ${
                            formData.gateway === gateway.id
                              ? 'border-primary-500 bg-primary-50'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <input
                            type="radio"
                            name="gateway"
                            value={gateway.id}
                            checked={formData.gateway === gateway.id}
                            onChange={(e) =>
                              setFormData({
                                ...formData,
                                gateway: e.target.value as PaymentGateway,
                              })
                            }
                            className="sr-only"
                          />
                          <div className="flex items-center gap-3 flex-1">
                            <div
                              className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                                formData.gateway === gateway.id
                                  ? 'bg-primary-500 text-white'
                                  : 'bg-gray-100 text-gray-600'
                              }`}
                            >
                              <FiCreditCard className="w-5 h-5" />
                            </div>
                            <div className="flex-1">
                              <div className="flex items-center gap-2">
                                <span className="font-medium text-gray-900">{gateway.name}</span>
                                {gateway.recommended && (
                                  <span className="px-2 py-0.5 text-xs font-medium bg-green-100 text-green-800 rounded-full">
                                    Recomendado
                                  </span>
                                )}
                                {gateway.local ? (
                                  <FiMapPin
                                    className="w-3.5 h-3.5 text-blue-500"
                                    title="Local RD"
                                  />
                                ) : (
                                  <FiGlobe
                                    className="w-3.5 h-3.5 text-purple-500"
                                    title="Internacional"
                                  />
                                )}
                              </div>
                              <span className="text-sm text-gray-500">{gateway.description}</span>
                            </div>
                          </div>
                          {formData.gateway === gateway.id && (
                            <FiCheck className="w-5 h-5 text-primary-500" />
                          )}
                        </label>
                      ))}
                    </div>
                  </div>

                  {/* Card Details */}
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Número de Tarjeta
                      </label>
                      <input
                        type="text"
                        placeholder="4242 4242 4242 4242"
                        value={formData.cardNumber}
                        onChange={(e) =>
                          setFormData({ ...formData, cardNumber: formatCardNumber(e.target.value) })
                        }
                        maxLength={19}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-lg tracking-wider"
                        required
                      />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Fecha de Vencimiento
                        </label>
                        <input
                          type="text"
                          placeholder="MM/YY"
                          value={formData.expiry}
                          onChange={(e) =>
                            setFormData({ ...formData, expiry: formatExpiry(e.target.value) })
                          }
                          maxLength={5}
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          required
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">CVV</label>
                        <input
                          type="text"
                          placeholder="123"
                          value={formData.cvv}
                          onChange={(e) =>
                            setFormData({
                              ...formData,
                              cvv: e.target.value.replace(/\D/g, '').slice(0, 4),
                            })
                          }
                          maxLength={4}
                          className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          required
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Nombre del Titular
                      </label>
                      <input
                        type="text"
                        placeholder="JOHN DOE"
                        value={formData.cardHolderName}
                        onChange={(e) =>
                          setFormData({ ...formData, cardHolderName: e.target.value.toUpperCase() })
                        }
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent uppercase"
                        required
                      />
                    </div>

                    {/* Set as Default */}
                    <label className="flex items-center gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.setAsDefault}
                        onChange={(e) =>
                          setFormData({ ...formData, setAsDefault: e.target.checked })
                        }
                        className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                      <span className="text-sm text-gray-700">
                        Establecer como método de pago predeterminado
                      </span>
                    </label>
                  </div>

                  <div className="flex gap-3 mt-6">
                    <Button
                      type="button"
                      variant="outline"
                      className="flex-1"
                      onClick={() => {
                        setShowAddModal(false);
                        setFormData(initialFormData);
                        setFormError('');
                      }}
                      disabled={addPaymentMethodMutation.isPending}
                    >
                      Cancelar
                    </Button>
                    <Button
                      type="submit"
                      className="flex-1"
                      disabled={addPaymentMethodMutation.isPending}
                    >
                      {addPaymentMethodMutation.isPending ? (
                        <>
                          <FiLoader className="w-4 h-4 mr-2 animate-spin" />
                          Procesando...
                        </>
                      ) : (
                        <>
                          <FiPlus className="w-4 h-4 mr-2" />
                          Agregar Tarjeta
                        </>
                      )}
                    </Button>
                  </div>
                </form>

                <p className="text-xs text-gray-500 text-center mt-4">
                  🔒 Tu información está protegida con encriptación SSL de 256 bits.
                  <br />
                  Nunca almacenamos el número completo de tu tarjeta.
                </p>
              </motion.div>
            </div>
          )}

          {/* Security Note */}
          <div className="mt-8 p-4 bg-blue-50 rounded-xl">
            <div className="flex items-start gap-3">
              <FiAlertCircle className="w-5 h-5 text-blue-600 mt-0.5" />
              <div>
                <p className="text-sm font-medium text-blue-900">Procesamiento Seguro de Pagos</p>
                <p className="text-sm text-blue-700 mt-1">
                  Toda la información de pago está encriptada con SSL de 256 bits estándar de la
                  industria. Nunca almacenamos el número completo de tu tarjeta en nuestros
                  servidores.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
