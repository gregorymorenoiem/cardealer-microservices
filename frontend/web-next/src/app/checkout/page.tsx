/**
 * Checkout Page
 *
 * Main checkout flow for premium services
 *
 * Route: /checkout?product=boost-basic
 */

'use client';

import * as React from 'react';
import { Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  CreditCard,
  Shield,
  Lock,
  Check,
  ArrowLeft,
  Info,
  AlertCircle,
  RefreshCw,
  Loader2,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { toast } from 'sonner';
import { checkoutService, type Product, type CreateCheckoutRequest } from '@/services/checkout';

// =============================================================================
// LOADING STATE
// =============================================================================

function CheckoutLoading() {
  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="container mx-auto max-w-4xl px-4">
        <Skeleton className="mb-8 h-8 w-32" />
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="space-y-6 lg:col-span-2">
            <Skeleton className="h-48 w-full rounded-lg" />
            <Skeleton className="h-64 w-full rounded-lg" />
          </div>
          <Skeleton className="h-96 w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// ERROR STATE
// =============================================================================

function CheckoutError({ message, onRetry }: { message: string; onRetry: () => void }) {
  const router = useRouter();

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="container mx-auto max-w-4xl px-4 py-16">
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <AlertCircle className="mb-4 h-16 w-16 text-red-400" />
            <h2 className="mb-2 text-xl font-semibold">Error en el checkout</h2>
            <p className="mb-6 text-gray-500">{message}</p>
            <div className="flex gap-3">
              <Button variant="outline" onClick={() => router.back()}>
                <ArrowLeft className="mr-2 h-4 w-4" />
                Volver
              </Button>
              <Button onClick={onRetry}>
                <RefreshCw className="mr-2 h-4 w-4" />
                Reintentar
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

// =============================================================================
// CHECKOUT CONTENT
// =============================================================================

function CheckoutContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const productId = searchParams.get('product') || 'boost-basic';
  const vehicleId = searchParams.get('vehicleId') || undefined;
  const dealerId = searchParams.get('dealerId') || undefined;

  // Fetch product data
  const {
    data: product,
    isLoading: productLoading,
    error: productError,
    refetch: refetchProduct,
  } = useQuery({
    queryKey: ['checkout-product', productId],
    queryFn: () => checkoutService.getProduct(productId),
  });

  // State
  const [paymentMethod, setPaymentMethod] = React.useState<'card' | 'azul'>('card');
  const [promoCode, setPromoCode] = React.useState('');
  const [promoApplied, setPromoApplied] = React.useState<{
    discountAmount: number;
    discountType: 'percentage' | 'fixed';
    discountValue: number;
  } | null>(null);
  const [cardData, setCardData] = React.useState({
    number: '',
    name: '',
    expiry: '',
    cvv: '',
  });

  // Validate promo code mutation
  const promoMutation = useMutation({
    mutationFn: (code: string) => checkoutService.validatePromoCode(code, productId),
    onSuccess: result => {
      if (result.valid) {
        setPromoApplied({
          discountAmount: result.discountAmount,
          discountType: result.discountType,
          discountValue: result.discountValue,
        });
        toast.success('Código promocional aplicado');
      } else {
        toast.error(result.errorMessage || 'Código no válido');
      }
    },
    onError: () => {
      toast.error('Error al validar el código');
    },
  });

  // Process payment mutation
  const paymentMutation = useMutation({
    mutationFn: async () => {
      // First create checkout session
      const request: CreateCheckoutRequest = {
        productId,
        vehicleId,
        dealerId,
        promoCode: promoApplied ? promoCode : undefined,
        paymentMethod,
        returnUrl: `${window.location.origin}/checkout/exito`,
        cancelUrl: `${window.location.origin}/checkout?product=${productId}`,
      };

      const session = await checkoutService.createCheckoutSession(request);

      // If payment URL provided, redirect (for Azul)
      if (session.paymentUrl) {
        window.location.href = session.paymentUrl;
        return { redirected: true };
      }

      // Process payment directly
      const result = await checkoutService.processPayment({
        sessionId: session.sessionId,
        cardToken: btoa(JSON.stringify(cardData)), // Simple encoding for demo
      });

      return result;
    },
    onSuccess: result => {
      if (result && 'redirected' in result && result.redirected) return;
      if (result && 'success' in result && result.success && result.orderId) {
        router.push(`/checkout/exito?orderId=${result.orderId}`);
      } else if (result && 'error' in result) {
        toast.error(result.error || 'Error al procesar el pago');
      }
    },
    onError: () => {
      toast.error('Error al procesar el pago');
    },
  });

  // Calculate prices
  const subtotal = product?.price ?? 0;
  const discount = promoApplied?.discountAmount ?? 0;
  const subtotalAfterDiscount = subtotal - discount;
  const tax = checkoutService.calculateTax(subtotalAfterDiscount);
  const total = subtotalAfterDiscount + tax;

  const handleApplyPromo = () => {
    if (promoCode.trim()) {
      promoMutation.mutate(promoCode.trim());
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Basic validation
    if (paymentMethod === 'card') {
      if (!cardData.number || !cardData.name || !cardData.expiry || !cardData.cvv) {
        toast.error('Por favor completa todos los campos de la tarjeta');
        return;
      }
    }

    paymentMutation.mutate();
  };

  // Loading state
  if (productLoading) {
    return <CheckoutLoading />;
  }

  // Error state
  if (productError || !product) {
    return (
      <CheckoutError
        message="No se pudo cargar la información del producto"
        onRetry={() => refetchProduct()}
      />
    );
  }

  const isProcessing = paymentMutation.isPending;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="border-b bg-white py-4">
        <div className="container mx-auto px-4">
          <div className="flex items-center justify-between">
            <Button variant="ghost" size="sm" onClick={() => router.back()}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Volver
            </Button>
            <div className="flex items-center gap-2 text-emerald-600">
              <Shield className="h-5 w-5" />
              <span className="text-sm font-medium">Pago Seguro</span>
            </div>
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-4xl">
          <h1 className="mb-8 text-2xl font-bold">Checkout</h1>

          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            {/* Payment Form */}
            <div className="space-y-6 lg:col-span-2">
              {/* Payment Method */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <CreditCard className="h-5 w-5" />
                    Método de Pago
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <RadioGroup
                    value={paymentMethod}
                    onValueChange={v => setPaymentMethod(v as 'card' | 'azul')}
                    className="grid grid-cols-2 gap-4"
                  >
                    <div>
                      <RadioGroupItem value="card" id="card" className="peer sr-only" />
                      <Label
                        htmlFor="card"
                        className={cn(
                          'flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 p-4',
                          'transition-colors hover:bg-gray-50',
                          paymentMethod === 'card'
                            ? 'border-emerald-500 bg-emerald-50'
                            : 'border-gray-200'
                        )}
                      >
                        <CreditCard className="mb-2 h-8 w-8 text-gray-600" />
                        <span className="font-medium">Tarjeta</span>
                        <span className="text-xs text-gray-500">Visa, Mastercard</span>
                      </Label>
                    </div>

                    <div>
                      <RadioGroupItem value="azul" id="azul" className="peer sr-only" />
                      <Label
                        htmlFor="azul"
                        className={cn(
                          'flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 p-4',
                          'transition-colors hover:bg-gray-50',
                          paymentMethod === 'azul'
                            ? 'border-emerald-500 bg-emerald-50'
                            : 'border-gray-200'
                        )}
                      >
                        <div className="mb-2 flex h-8 w-8 items-center justify-center rounded bg-blue-600 text-xs font-bold text-white">
                          AZUL
                        </div>
                        <span className="font-medium">AZUL</span>
                        <span className="text-xs text-gray-500">Banco Popular</span>
                      </Label>
                    </div>
                  </RadioGroup>
                </CardContent>
              </Card>

              {/* Card Details */}
              {paymentMethod === 'card' && (
                <Card>
                  <CardHeader>
                    <CardTitle>Datos de la Tarjeta</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4">
                      <div>
                        <Label htmlFor="cardNumber">Número de Tarjeta</Label>
                        <Input
                          id="cardNumber"
                          placeholder="1234 5678 9012 3456"
                          value={cardData.number}
                          onChange={e => setCardData({ ...cardData, number: e.target.value })}
                          required
                        />
                      </div>

                      <div>
                        <Label htmlFor="cardName">Nombre en la Tarjeta</Label>
                        <Input
                          id="cardName"
                          placeholder="JUAN PEREZ"
                          value={cardData.name}
                          onChange={e => setCardData({ ...cardData, name: e.target.value })}
                          required
                        />
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <Label htmlFor="expiry">Vencimiento</Label>
                          <Input
                            id="expiry"
                            placeholder="MM/AA"
                            value={cardData.expiry}
                            onChange={e => setCardData({ ...cardData, expiry: e.target.value })}
                            required
                          />
                        </div>
                        <div>
                          <Label htmlFor="cvv">CVV</Label>
                          <Input
                            id="cvv"
                            type="password"
                            placeholder="123"
                            maxLength={4}
                            value={cardData.cvv}
                            onChange={e => setCardData({ ...cardData, cvv: e.target.value })}
                            required
                          />
                        </div>
                      </div>

                      <Button
                        type="submit"
                        className="h-12 w-full bg-emerald-600 text-lg hover:bg-emerald-700"
                        disabled={isProcessing}
                      >
                        {isProcessing ? (
                          <>
                            <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                            Procesando...
                          </>
                        ) : (
                          <>
                            <Lock className="mr-2 h-5 w-5" />
                            Pagar {checkoutService.formatCurrency(total, product.currency)}
                          </>
                        )}
                      </Button>
                    </form>
                  </CardContent>
                </Card>
              )}

              {/* AZUL Payment */}
              {paymentMethod === 'azul' && (
                <Card>
                  <CardHeader>
                    <CardTitle>Pago con AZUL</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <p className="mb-4 text-gray-600">
                      Serás redirigido a la página segura de AZUL para completar tu pago.
                    </p>
                    <Button
                      onClick={handleSubmit}
                      className="h-12 w-full bg-blue-600 text-lg hover:bg-blue-700"
                      disabled={isProcessing}
                    >
                      {isProcessing ? (
                        <>
                          <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                          Redirigiendo...
                        </>
                      ) : (
                        <>
                          <Lock className="mr-2 h-5 w-5" />
                          Pagar con AZUL {checkoutService.formatCurrency(total, product.currency)}
                        </>
                      )}
                    </Button>
                  </CardContent>
                </Card>
              )}

              {/* Security Notice */}
              <div className="flex items-start gap-3 text-sm text-gray-500">
                <Shield className="h-5 w-5 shrink-0 text-emerald-600" />
                <p>
                  Tus datos de pago están protegidos con encriptación SSL de 256 bits. Nunca
                  almacenamos los datos completos de tu tarjeta.
                </p>
              </div>
            </div>

            {/* Order Summary */}
            <div>
              <Card className="sticky top-4">
                <CardHeader>
                  <CardTitle>Resumen del Pedido</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Product */}
                  <div>
                    <h3 className="font-semibold">{product.name}</h3>
                    <p className="text-sm text-gray-600">{product.description}</p>
                    {product.originalPrice && (
                      <Badge variant="secondary" className="mt-2 bg-emerald-100 text-emerald-700">
                        Ahorra{' '}
                        {checkoutService.formatCurrency(
                          product.originalPrice - product.price,
                          product.currency
                        )}
                      </Badge>
                    )}
                  </div>

                  {/* Features */}
                  <ul className="space-y-1.5">
                    {product.features.map(feature => (
                      <li key={feature} className="flex items-start gap-2 text-sm">
                        <Check className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600" />
                        {feature}
                      </li>
                    ))}
                  </ul>

                  <Separator />

                  {/* Pricing */}
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Subtotal</span>
                      <span>
                        {product.originalPrice && (
                          <span className="mr-2 text-gray-400 line-through">
                            {checkoutService.formatCurrency(
                              product.originalPrice,
                              product.currency
                            )}
                          </span>
                        )}
                        {checkoutService.formatCurrency(subtotal, product.currency)}
                      </span>
                    </div>
                    {promoApplied && (
                      <div className="flex justify-between text-emerald-600">
                        <span>Descuento ({promoCode})</span>
                        <span>-{checkoutService.formatCurrency(discount, product.currency)}</span>
                      </div>
                    )}
                    <div className="flex justify-between">
                      <span className="text-gray-600">ITBIS (18%)</span>
                      <span>{checkoutService.formatCurrency(tax, product.currency)}</span>
                    </div>
                    <Separator />
                    <div className="flex justify-between text-lg font-bold">
                      <span>Total</span>
                      <span className="text-emerald-600">
                        {checkoutService.formatCurrency(total, product.currency)}
                      </span>
                    </div>
                  </div>

                  {/* Promo Code */}
                  <div className="flex gap-2">
                    <Input
                      placeholder="Código promocional"
                      className="flex-1"
                      value={promoCode}
                      onChange={e => setPromoCode(e.target.value)}
                      disabled={!!promoApplied}
                    />
                    <Button
                      variant="outline"
                      onClick={handleApplyPromo}
                      disabled={promoMutation.isPending || !!promoApplied}
                    >
                      {promoMutation.isPending ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : promoApplied ? (
                        <Check className="h-4 w-4" />
                      ) : (
                        'Aplicar'
                      )}
                    </Button>
                  </div>

                  {/* Info */}
                  <div className="flex items-start gap-2 rounded-lg bg-gray-50 p-3 text-xs text-gray-500">
                    <Info className="mt-0.5 h-4 w-4 shrink-0" />
                    <p>
                      Al completar la compra, aceptas nuestros{' '}
                      <Link href="/terminos" className="text-emerald-600 hover:underline">
                        términos y condiciones
                      </Link>
                      .
                    </p>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// PAGE EXPORT
// =============================================================================

export default function CheckoutPage() {
  return (
    <Suspense fallback={<CheckoutLoading />}>
      <CheckoutContent />
    </Suspense>
  );
}
