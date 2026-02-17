/**
 * Payment Tokenization Callback Page
 *
 * Handles the return from payment provider tokenization pages
 * Processes the callback parameters and completes the tokenization
 */

'use client';

import * as React from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { userBillingService, type PaymentGateway } from '@/services/user-billing';

type CallbackStatus = 'processing' | 'success' | 'error' | 'cancelled';

interface StoredSession {
  sessionId: string;
  gateway: PaymentGateway;
  setAsDefault: boolean;
}

export default function PaymentCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = React.useState<CallbackStatus>('processing');
  const [message, setMessage] = React.useState('Procesando respuesta del proveedor...');
  const [cardInfo, setCardInfo] = React.useState<string | null>(null);

  React.useEffect(() => {
    const processCallback = async () => {
      try {
        // SEGURIDAD: Usar sessionStorage (más seguro que localStorage)
        const storedSessionStr = sessionStorage.getItem('tokenization_session');
        if (!storedSessionStr) {
          setStatus('error');
          setMessage('No se encontró una sesión de tokenización activa');
          return;
        }

        const storedSession: StoredSession = JSON.parse(storedSessionStr);

        // SEGURIDAD: Limpiar inmediatamente después de leer
        sessionStorage.removeItem('tokenization_session');

        // Get callback parameters based on provider
        const providerResponse: Record<string, string> = {};

        // Common parameters
        searchParams.forEach((value, key) => {
          providerResponse[key] = value;
        });

        // Check for cancellation
        if (
          searchParams.get('cancelled') === 'true' ||
          searchParams.get('status') === 'cancelled'
        ) {
          setStatus('cancelled');
          setMessage('Has cancelado el proceso de registro de tarjeta');
          return;
        }

        // Check for errors
        const errorParam = searchParams.get('error') || searchParams.get('errorCode');
        if (errorParam) {
          setStatus('error');
          setMessage(
            searchParams.get('errorMessage') ||
              searchParams.get('error_description') ||
              'Error en el proceso de tokenización'
          );
          return;
        }

        // Extract provider-specific tokens
        const tokenRequest: {
          sessionId: string;
          gateway: PaymentGateway;
          setAsDefault: boolean;
          providerToken?: string;
          providerResponse: Record<string, string>;
          azulDataVaultToken?: string;
          azulOrderId?: string;
          cardNetToken?: string;
          pixelPayToken?: string;
          fygaroToken?: string;
          payPalVaultId?: string;
          braintreeNonce?: string;
        } = {
          sessionId: storedSession.sessionId,
          gateway: storedSession.gateway,
          setAsDefault: storedSession.setAsDefault,
          providerResponse,
        };

        // Extract token based on gateway
        switch (storedSession.gateway) {
          case 'Azul':
            tokenRequest.azulDataVaultToken =
              searchParams.get('DataVaultToken') || searchParams.get('token') || '';
            tokenRequest.azulOrderId =
              searchParams.get('OrderId') || searchParams.get('orderId') || '';
            providerResponse.cardBrand =
              searchParams.get('CardBrand') || searchParams.get('cardBrand') || '';
            providerResponse.cardLast4 =
              searchParams.get('CardNumber')?.slice(-4) || searchParams.get('last4') || '';
            providerResponse.expMonth =
              searchParams.get('ExpirationMonth') || searchParams.get('expMonth') || '';
            providerResponse.expYear =
              searchParams.get('ExpirationYear') || searchParams.get('expYear') || '';
            break;

          case 'CardNET':
            tokenRequest.cardNetToken =
              searchParams.get('token') || searchParams.get('cardToken') || '';
            providerResponse.cardBrand = searchParams.get('brand') || '';
            providerResponse.cardLast4 =
              searchParams.get('last4') || searchParams.get('maskedCard')?.slice(-4) || '';
            providerResponse.expMonth = searchParams.get('expMonth') || '';
            providerResponse.expYear = searchParams.get('expYear') || '';
            break;

          case 'PixelPay':
            tokenRequest.pixelPayToken =
              searchParams.get('token') || searchParams.get('card_token') || '';
            providerResponse.cardBrand = searchParams.get('card_brand') || '';
            providerResponse.cardLast4 = searchParams.get('card_last_four') || '';
            providerResponse.expMonth = searchParams.get('exp_month') || '';
            providerResponse.expYear = searchParams.get('exp_year') || '';
            break;

          case 'Fygaro':
            tokenRequest.fygaroToken =
              searchParams.get('token') || searchParams.get('vaultToken') || '';
            providerResponse.cardBrand = searchParams.get('cardBrand') || '';
            providerResponse.cardLast4 = searchParams.get('last4') || '';
            providerResponse.expMonth = searchParams.get('expMonth') || '';
            providerResponse.expYear = searchParams.get('expYear') || '';
            break;

          case 'PayPal':
            tokenRequest.payPalVaultId =
              searchParams.get('vault_id') || searchParams.get('vaultId') || '';
            tokenRequest.braintreeNonce =
              searchParams.get('nonce') || searchParams.get('payment_method_nonce') || '';
            providerResponse.cardBrand = searchParams.get('card_type') || 'PayPal';
            providerResponse.cardLast4 = searchParams.get('last_four') || '';
            break;
        }

        // Verify we have a token
        const hasToken =
          tokenRequest.azulDataVaultToken ||
          tokenRequest.cardNetToken ||
          tokenRequest.pixelPayToken ||
          tokenRequest.fygaroToken ||
          tokenRequest.payPalVaultId ||
          tokenRequest.braintreeNonce;

        if (!hasToken) {
          setStatus('error');
          setMessage('No se recibió un token válido del proveedor');
          return;
        }

        // Complete tokenization
        const paymentMethod = await userBillingService.completeTokenization(tokenRequest);

        setStatus('success');
        setCardInfo(
          `${paymentMethod.card?.brand || 'Tarjeta'} ****${paymentMethod.card?.last4 || '****'}`
        );
        setMessage('Tu tarjeta ha sido agregada exitosamente');

        // Redirect after a short delay
        setTimeout(() => {
          router.push('/cuenta/pagos?added=true');
        }, 2000);
      } catch (error) {
        // SEGURIDAD: No exponer errores detallados en producción
        if (process.env.NODE_ENV === 'development') {
          console.error('Error processing callback:', error);
        }
        setStatus('error');
        const err = error as Error & { response?: { data?: { message?: string } } };
        setMessage(
          err.response?.data?.message ||
            err.message ||
            'Error al procesar la respuesta del proveedor'
        );
      }
    };

    processCallback();
  }, [searchParams, router]);

  const handleRetry = () => {
    router.push('/cuenta/pagos');
  };

  return (
    <div className="container mx-auto max-w-lg px-4 py-12">
      <Card>
        <CardHeader className="text-center">
          <div className="mx-auto mb-4">
            {status === 'processing' && (
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-blue-100">
                <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
              </div>
            )}
            {status === 'success' && (
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
                <CheckCircle className="h-8 w-8 text-green-600" />
              </div>
            )}
            {status === 'error' && (
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
                <XCircle className="h-8 w-8 text-red-600" />
              </div>
            )}
            {status === 'cancelled' && (
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-yellow-100">
                <AlertCircle className="h-8 w-8 text-yellow-600" />
              </div>
            )}
          </div>
          <CardTitle>
            {status === 'processing' && 'Procesando...'}
            {status === 'success' && '¡Tarjeta Agregada!'}
            {status === 'error' && 'Error'}
            {status === 'cancelled' && 'Cancelado'}
          </CardTitle>
          <CardDescription className="text-base">{message}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4 text-center">
          {status === 'success' && cardInfo && (
            <div className="inline-block rounded-lg bg-muted/50 px-6 py-4">
              <p className="text-lg font-medium">{cardInfo}</p>
              <p className="text-sm text-muted-foreground">Agregada a tus métodos de pago</p>
            </div>
          )}

          {(status === 'error' || status === 'cancelled') && (
            <Button onClick={handleRetry} className="mt-4">
              Volver a intentar
            </Button>
          )}

          {status === 'processing' && (
            <p className="text-sm text-muted-foreground">
              Por favor espera mientras procesamos tu solicitud...
            </p>
          )}

          {status === 'success' && (
            <p className="text-sm text-muted-foreground">Redirigiendo a tus métodos de pago...</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
