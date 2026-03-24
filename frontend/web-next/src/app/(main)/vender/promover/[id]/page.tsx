/**
 * Promote Vehicle Page — v2
 *
 * Shows seller or dealer upgrade plans aligned with the admin portal catalog.
 * Uses PayPal payment (same flow as /cuenta/upgrade) — no dependency on
 * /api/checkout/sessions which may not be configured for boost products.
 *
 * For sellers: shows Libre / Estándar ($9.99/listing) / Verificado ($34.99/mes)
 * For dealers: shows plan upgrade options with live pricing from AdminService
 */

'use client';

import * as React from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import Image from 'next/image';
import {
  ArrowLeft,
  ArrowRight,
  Star,
  Eye,
  TrendingUp,
  Shield,
  ShieldCheck,
  Check,
  X,
  Loader2,
  AlertTriangle,
  CreditCard,
  Car,
  Lock,
  AlertCircle,
  PartyPopper,
  Sparkles,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Separator } from '@/components/ui/separator';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { toast } from 'sonner';
import { vehicleService } from '@/services/vehicles';
import { usePlanAccess } from '@/hooks/use-plan-access';
import { usePlatformPricing } from '@/hooks/use-platform-pricing';
import { PayPalPaymentButton } from '@/components/checkout/PayPalPaymentButton';
import { serverCreatePayPalOrder, serverCapturePayPalOrder } from '@/actions/checkout';
import type { Vehicle } from '@/types';
import { formatPrice } from '@/lib/format';
import { cn } from '@/lib/utils';
import { SellerPlan, SELLER_PLAN_LIMITS } from '@/lib/plan-config';

// =============================================================================
// TYPES
// =============================================================================

type PaymentMethod = 'paypal' | 'azul' | 'fygaro';

interface PlanCard {
  key: string;
  name: string;
  price: number;
  billingLabel: string;
  badge: string | null;
  badgeColor?: string;
  features: string[];
}

// =============================================================================
// SELLER PLAN CARDS
// =============================================================================

function buildSellerCards(
  estandarPrice: number,
  verificadoPrice: number
): PlanCard[] {
  return [
    {
      key: 'libre_seller',
      name: 'Libre',
      price: 0,
      billingLabel: 'gratis',
      badge: null,
      features: [
        '1 publicación activa',
        'Hasta 5 fotos por vehículo',
        'Duración: 30 días',
        '⬇ Posición al fondo en búsquedas',
        '⚪ Sin badge de verificación',
        'KYC: solo email',
      ],
    },
    {
      key: 'estandar',
      name: 'Estándar',
      price: estandarPrice,
      billingLabel: 'por listing',
      badge: null,
      features: [
        '1 publicación por pago',
        'Hasta 10 fotos por vehículo',
        'Duración: 60 días',
        '⬆ Posición media (bajo dealers)',
        '🔵 Badge Vendedor OKLA',
        'KYC: email + teléfono verificados',
        'Renovación de listing: incluida',
        '1 valoración PricingAgent IA por listing',
      ],
    },
    {
      key: 'verificado',
      name: 'Verificado',
      price: verificadoPrice,
      billingLabel: '/mes',
      badge: 'MÁS POPULAR',
      badgeColor: 'emerald',
      features: [
        '3 publicaciones simultáneas',
        'Hasta 12 fotos por vehículo',
        'Duración: 90 días',
        '📈 Alta posición (bajo dealers VISIBLE)',
        '✅ Badge Vendedor Verificado',
        'KYC completo: cédula + selfie + teléfono',
        'Renovación de listing incluida',
        '2 valoraciones PricingAgent IA/mes',
        'Analytics básico de tus publicaciones',
      ],
    },
  ];
}

// =============================================================================
// DEALER PLAN CARDS (upgrade options)
// =============================================================================

function buildDealerCards(
  visiblePrice: number,
  starterPrice: number,
  proPrice: number,
  elitePrice: number
): PlanCard[] {
  return [
    {
      key: 'visible',
      name: 'Visible',
      price: visiblePrice,
      billingLabel: '/mes',
      badge: null,
      features: [
        'Publicaciones ilimitadas',
        'Hasta 10 fotos por vehículo',
        'Prioridad media en búsquedas',
        '3 publicaciones destacadas/mes',
        '🔵 Badge Dealer Verificado OKLA',
        '$15 OKLA Coins/mes',
        '5 valoraciones PricingAgent IA/mes',
        'Dashboard Analytics básico',
        '✅ Garantía: 10 consultas en 30 días o mes 2 gratis',
      ],
    },
    {
      key: 'starter',
      name: 'Starter',
      price: starterPrice,
      billingLabel: '/mes',
      badge: null,
      features: [
        'Publicaciones ilimitadas',
        'Hasta 12 fotos por vehículo',
        'Alta prioridad en búsquedas',
        '5 publicaciones destacadas/mes',
        '🔵 Badge Verificado+',
        '$30 OKLA Coins/mes',
        'ChatAgent Web 100 conv/mes',
        'ChatAgent WhatsApp 100 conv/mes',
        '10 valoraciones PricingAgent IA/mes',
      ],
    },
    {
      key: 'pro',
      name: 'Pro',
      price: proPrice,
      billingLabel: '/mes',
      badge: 'MÁS POPULAR',
      badgeColor: 'emerald',
      features: [
        'Publicaciones ilimitadas',
        'Hasta 15 fotos por vehículo',
        'Alta prioridad en búsquedas',
        '10 publicaciones destacadas/mes',
        '🥇 Badge Verificado Dorado',
        '$45 OKLA Coins/mes',
        'ChatAgent Web 300 conv/mes',
        'ChatAgent WhatsApp 300 conv/mes',
        'Agendamiento automático',
        'PricingAgent IA ilimitado',
        'Dashboard Analytics avanzado',
      ],
    },
    {
      key: 'elite',
      name: 'Élite',
      price: elitePrice,
      billingLabel: '/mes',
      badge: 'RECOMENDADO',
      badgeColor: 'amber',
      features: [
        'Publicaciones ilimitadas',
        'Hasta 20 fotos + video tour',
        'Top prioridad en búsquedas',
        '25 publicaciones destacadas/mes',
        '💎 Badge Verificado Premium',
        '$120 OKLA Coins/mes',
        'ChatAgent Web 5,000 conv/mes',
        'ChatAgent WhatsApp 5,000 conv/mes',
        'PricingAgent IA ilimitado + PDF',
        'Dashboard Analytics completo + exportar',
        'Gerente de cuenta dedicado',
      ],
    },
  ];
}

// =============================================================================
// COMPARISON TABLE DATA
// =============================================================================

interface ComparisonRow {
  label: string;
  libre: string | boolean;
  estandar: string | boolean;
  verificado: string | boolean;
}

function buildSellerComparison(): ComparisonRow[] {
  const libre = SELLER_PLAN_LIMITS[SellerPlan.LIBRE];
  const estandar = SELLER_PLAN_LIMITS[SellerPlan.ESTANDAR];
  const verificado = SELLER_PLAN_LIMITS[SellerPlan.VERIFICADO];
  return [
    {
      label: 'Publicaciones activas',
      libre: String(libre.maxListings),
      estandar: '1 por pago',
      verificado: String(verificado.maxListings),
    },
    {
      label: 'Duración del listing',
      libre: `${libre.listingDuration} días`,
      estandar: `${estandar.listingDuration} días`,
      verificado: `${verificado.listingDuration} días`,
    },
    {
      label: 'Fotos por vehículo',
      libre: `Hasta ${libre.maxImages}`,
      estandar: `Hasta ${estandar.maxImages}`,
      verificado: `Hasta ${verificado.maxImages}`,
    },
    {
      label: 'Posición búsqueda',
      libre: '⬇ Fondo absoluto',
      estandar: '⬆ Media (bajo dealers)',
      verificado: '📈 Alta (bajo dealers VISIBLE)',
    },
    {
      label: 'Badge de confianza',
      libre: '⚪ Sin verificar',
      estandar: '🔵 Vendedor OKLA',
      verificado: '✅ Vendedor Verificado',
    },
    {
      label: 'Verificación KYC',
      libre: 'Solo email',
      estandar: 'Email + teléfono',
      verificado: 'Cédula + selfie + tel.',
    },
    {
      label: 'Renovación listing',
      libre: '—',
      estandar: 'Incluida',
      verificado: 'Incluida',
    },
    {
      label: 'PricingAgent IA',
      libre: '—',
      estandar: '1/listing',
      verificado: '2/mes',
    },
    {
      label: 'Analytics',
      libre: false,
      estandar: false,
      verificado: true,
    },
  ];
}

// =============================================================================
// PAGE COMPONENT
// =============================================================================

export default function PromoteVehiclePage() {
  const params = useParams();
  const router = useRouter();
  const vehicleId = params.id as string;

  const { isSeller, isDealer, currentPlan, isLoading: planLoading } = usePlanAccess();
  const { pricing } = usePlatformPricing();

  const [vehicle, setVehicle] = React.useState<Vehicle | null>(null);
  const [vehicleLoading, setVehicleLoading] = React.useState(true);
  const [vehicleError, setVehicleError] = React.useState<string | null>(null);

  const [selectedPlanKey, setSelectedPlanKey] = React.useState<string>('');
  const [paymentMethod, setPaymentMethod] = React.useState<PaymentMethod>('paypal');
  const [acceptTerms, setAcceptTerms] = React.useState(false);
  const [isProcessing, setIsProcessing] = React.useState(false);
  const [showSuccess, setShowSuccess] = React.useState(false);

  // Load vehicle
  React.useEffect(() => {
    async function load() {
      try {
        const v = await vehicleService.getById(vehicleId);
        setVehicle(v);
      } catch {
        setVehicleError('No se pudo cargar el vehículo.');
      } finally {
        setVehicleLoading(false);
      }
    }
    load();
  }, [vehicleId]);

  // Build plan cards based on user type
  const planCards = React.useMemo<PlanCard[]>(() => {
    if (isDealer) {
      return buildDealerCards(
        pricing.dealerVisible,
        pricing.dealerStarter,
        pricing.dealerPro,
        pricing.dealerElite
      );
    }
    // Seller (default)
    return buildSellerCards(pricing.sellerEstandar, pricing.sellerVerificado);
  }, [isDealer, pricing]);

  // Initialize selected plan to first non-current paid plan
  React.useEffect(() => {
    if (!selectedPlanKey && planCards.length > 0) {
      const firstUpgrade = planCards.find(p => p.price > 0 && p.key !== currentPlan);
      setSelectedPlanKey(firstUpgrade?.key ?? planCards[planCards.length - 1].key);
    }
  }, [planCards, currentPlan, selectedPlanKey]);

  const selectedCard = planCards.find(p => p.key === selectedPlanKey);
  const paypalAmount =
    selectedCard && selectedCard.price > 0 ? (selectedCard.price / 58).toFixed(2) : '0';

  // Payment method options
  const PAYMENT_METHODS: { id: PaymentMethod; name: string; description: string; icon: string; color: string; recommended?: boolean }[] = [
    { id: 'paypal', name: 'PayPal', description: 'Paga con tu cuenta PayPal o tarjeta', icon: 'PP', color: 'bg-[#003087]', recommended: true },
    { id: 'azul', name: 'Azul', description: 'Tarjeta de crédito/débito (RD)', icon: 'AZ', color: 'bg-blue-600' },
    { id: 'fygaro', name: 'Fygaro', description: 'Pago recurrente automático', icon: 'FY', color: 'bg-teal-600' },
  ];

  const handleAzulOrFygaro = () => {
    setIsProcessing(true);
    toast.info(`Redirigiendo a ${paymentMethod === 'azul' ? 'AZUL' : 'Fygaro'}...`);
    setTimeout(() => {
      setIsProcessing(false);
      toast.error(`${paymentMethod === 'azul' ? 'AZUL' : 'Fygaro'} no está configurado todavía. Usa PayPal.`);
    }, 2000);
  };

  const renderCellValue = (value: string | boolean) => {
    if (typeof value === 'boolean') {
      return value ? (
        <Check className="mx-auto h-5 w-5 text-emerald-500" />
      ) : (
        <X className="mx-auto h-5 w-5 text-gray-300" />
      );
    }
    return <span className={cn('font-semibold', value === '—' && 'text-gray-400')}>{value}</span>;
  };

  // ─── Loading state ───────────────────────────────────────────────────────
  if (vehicleLoading || planLoading) {
    return (
      <div className="mx-auto max-w-4xl space-y-6 p-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-24 w-full" />
        <div className="grid gap-4 sm:grid-cols-3">
          <Skeleton className="h-72" />
          <Skeleton className="h-72" />
          <Skeleton className="h-72" />
        </div>
      </div>
    );
  }

  // ─── Vehicle error ────────────────────────────────────────────────────────
  if (vehicleError || !vehicle) {
    return (
      <div className="mx-auto max-w-3xl p-6">
        <Card>
          <CardContent className="py-12 text-center">
            <AlertTriangle className="mx-auto mb-4 h-12 w-12 text-red-500" />
            <h2 className="mb-2 text-xl font-semibold">Error</h2>
            <p className="text-muted-foreground mb-4">
              {vehicleError || 'No se encontró el vehículo'}
            </p>
            <Button asChild variant="outline">
              <Link href="/cuenta/mis-vehiculos">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Volver
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // ─── Vehicle not active ───────────────────────────────────────────────────
  if (vehicle.status !== 'active') {
    return (
      <div className="mx-auto max-w-3xl p-6">
        <Card>
          <CardContent className="py-12 text-center">
            <AlertTriangle className="mx-auto mb-4 h-12 w-12 text-yellow-500" />
            <h2 className="mb-2 text-xl font-semibold">Vehículo no activo</h2>
            <p className="text-muted-foreground mb-4">
              Solo puedes promocionar vehículos activos y publicados. Estado actual:{' '}
              <strong>{vehicle.status}</strong>
            </p>
            <Button asChild variant="outline">
              <Link href="/cuenta/mis-vehiculos">
                <ArrowLeft className="mr-2 h-4 w-4" />
                Volver
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  // ─── Main render ──────────────────────────────────────────────────────────
  return (
    <div className="mx-auto max-w-4xl space-y-8 px-4 py-8 sm:px-6">
      {/* Back */}
      <Link
        href="/cuenta/mis-vehiculos"
        className="text-muted-foreground hover:text-foreground inline-flex items-center gap-1 text-sm"
      >
        <ArrowLeft className="h-4 w-4" />
        Volver a mis vehículos
      </Link>

      {/* Header */}
      <div className="text-center">
        <div className="bg-primary/10 mx-auto mb-4 inline-flex rounded-full p-3">
          <Sparkles className="text-primary h-8 w-8" />
        </div>
        <h1 className="text-3xl font-bold">Promociona tu vehículo</h1>
        <p className="text-muted-foreground mt-2">
          Actualiza tu plan para ganar más visibilidad y vender más rápido
        </p>
      </div>

      {/* Vehicle Preview */}
      <Card>
        <CardContent className="flex items-center gap-4 p-4">
          <div className="bg-muted relative h-20 w-28 flex-shrink-0 overflow-hidden rounded-lg">
            {vehicle.images?.[0] ? (
              <Image
                src={vehicle.images[0].url}
                alt={`${vehicle.make} ${vehicle.model}`}
                fill
                className="object-cover"
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center">
                <Car className="text-muted-foreground h-8 w-8" />
              </div>
            )}
          </div>
          <div>
            <h3 className="font-semibold">
              {vehicle.year} {vehicle.make} {vehicle.model}
            </h3>
            <p className="text-primary text-lg font-bold">
              {formatPrice(vehicle.price, vehicle.currency)}
            </p>
            <div className="text-muted-foreground flex items-center gap-3 text-sm">
              <span className="flex items-center gap-1">
                <Eye className="h-3.5 w-3.5" />
                {vehicle.viewCount || 0} vistas
              </span>
              {vehicle.isFeatured && (
                <Badge variant="secondary" className="text-xs">
                  <Star className="mr-1 h-3 w-3" />
                  Destacado
                </Badge>
              )}
            </div>
          </div>
          {currentPlan && (
            <div className="ml-auto">
              <Badge variant="outline" className="text-xs">
                Plan actual: <strong className="ml-1">{currentPlan}</strong>
              </Badge>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Benefits */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5" />
            ¿Por qué mejorar tu plan?
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="flex items-start gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Eye className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="font-medium">Más vistas</p>
                <p className="text-muted-foreground text-sm">
                  Hasta 3× más visibilidad en búsquedas
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <Star className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="font-medium">Badge verificado</p>
                <p className="text-muted-foreground text-sm">
                  Genera más confianza y credibilidad
                </p>
              </div>
            </div>
            <div className="flex items-start gap-3">
              <div className="rounded-lg bg-green-100 p-2">
                <Shield className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="font-medium">Mayor duración</p>
                <p className="text-muted-foreground text-sm">
                  Listings activos por más tiempo
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Plan Cards */}
      <div>
        <h2 className="mb-4 text-xl font-semibold">
          {isDealer ? 'Planes para concesionarios' : 'Planes para vendedores'}
        </h2>
        <div className={cn(
          'grid gap-4',
          isSeller ? 'sm:grid-cols-3' : 'sm:grid-cols-2 lg:grid-cols-4'
        )}>
          {planCards.map(plan => {
            const isCurrent = plan.key === currentPlan;
            const isSelected = plan.key === selectedPlanKey && !isCurrent;
            const isFree = plan.price === 0;

            return (
              <Card
                key={plan.key}
                onClick={() => {
                  if (!isCurrent && !isFree) setSelectedPlanKey(plan.key);
                }}
                className={cn(
                  'relative border-2 transition-all',
                  isCurrent
                    ? 'border-gray-200 opacity-70'
                    : isFree
                      ? 'border-gray-100 opacity-60 cursor-not-allowed'
                      : isSelected
                        ? 'border-primary cursor-pointer shadow-md'
                        : 'cursor-pointer border-gray-200 hover:border-gray-400'
                )}
              >
                {isCurrent && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full bg-gray-500 px-3 py-1 text-xs font-bold whitespace-nowrap text-white">
                    TU PLAN
                  </div>
                )}
                {isSelected && (
                  <div className="bg-primary absolute -top-3 left-1/2 -translate-x-1/2 rounded-full px-3 py-1 text-xs font-bold whitespace-nowrap text-white">
                    SELECCIONADO
                  </div>
                )}
                {!isCurrent && !isSelected && plan.badge && (
                  <div
                    className={cn(
                      'absolute -top-3 left-1/2 -translate-x-1/2 rounded-full px-3 py-1 text-xs font-bold whitespace-nowrap text-white',
                      plan.badgeColor === 'emerald' ? 'bg-emerald-500' : 'bg-amber-500'
                    )}
                  >
                    {plan.badge}
                  </div>
                )}
                <CardHeader className="pt-6 text-center">
                  <CardTitle className="text-lg">{plan.name}</CardTitle>
                  <CardDescription className="text-base font-semibold">
                    {plan.price === 0
                      ? 'Gratis'
                      : `${formatPrice(plan.price, 'DOP')}${plan.billingLabel}`}
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <ul className="space-y-1.5">
                    {plan.features.map((f, i) => (
                      <li key={i} className="flex items-start gap-2 text-sm">
                        <Check
                          className={cn(
                            'mt-0.5 h-4 w-4 shrink-0',
                            isCurrent ? 'text-gray-400' : 'text-emerald-500'
                          )}
                        />
                        <span className={isCurrent ? 'text-muted-foreground' : 'font-medium'}>
                          {f}
                        </span>
                      </li>
                    ))}
                  </ul>
                </CardContent>
              </Card>
            );
          })}
        </div>
      </div>

      {/* Seller comparison table */}
      {isSeller && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Comparación detallada — Planes Vendedor</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-muted/50 border-b">
                    <th className="px-4 py-3 text-left font-medium">CARACTERÍSTICA</th>
                    <th className="px-4 py-3 text-center font-medium">LIBRE</th>
                    <th className="px-4 py-3 text-center font-medium">ESTÁNDAR</th>
                    <th className="bg-primary/5 px-4 py-3 text-center font-medium">VERIFICADO</th>
                  </tr>
                </thead>
                <tbody>
                  {buildSellerComparison().map((row, i) => (
                    <tr key={i} className="border-b last:border-0">
                      <td className="px-4 py-3">{row.label}</td>
                      <td className="px-4 py-3 text-center">{renderCellValue(row.libre)}</td>
                      <td className="px-4 py-3 text-center">{renderCellValue(row.estandar)}</td>
                      <td className="bg-primary/5 px-4 py-3 text-center">
                        {renderCellValue(row.verificado)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Guarantee banner for dealer Visible plan */}
      {isDealer && selectedPlanKey === 'visible' && (
        <Card className="border-2 border-emerald-200 bg-emerald-50">
          <CardContent className="flex items-start gap-4 pt-6">
            <div className="rounded-full bg-emerald-100 p-3">
              <ShieldCheck className="h-6 w-6 text-emerald-600" />
            </div>
            <div className="space-y-1">
              <h3 className="text-lg font-bold text-emerald-800">Garantía de Resultados OKLA</h3>
              <p className="text-sm text-emerald-700">
                Si no recibes <strong>al menos 10 consultas</strong> en tus primeros{' '}
                <strong>30 días</strong> con el plan Visible,{' '}
                <strong>tu segundo mes es completamente gratis</strong>.
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Payment Method Selection */}
      {selectedCard && selectedCard.price > 0 && (
        <>
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-lg">
                <CreditCard className="h-5 w-5" />
                Método de pago
              </CardTitle>
              <CardDescription>
                <span className="flex items-center gap-1 text-xs text-emerald-600">
                  <Lock className="h-3 w-3" />
                  Conexión segura con cifrado SSL de 256 bits
                </span>
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-3 sm:grid-cols-3">
                {PAYMENT_METHODS.map(method => (
                  <button
                    key={method.id}
                    onClick={() => setPaymentMethod(method.id)}
                    disabled={isProcessing}
                    className={cn(
                      'relative flex flex-col items-center rounded-xl border-2 p-4 transition-all',
                      paymentMethod === method.id
                        ? 'border-primary bg-primary/5 shadow-sm'
                        : 'border-gray-200 hover:border-gray-300',
                      isProcessing && 'cursor-not-allowed opacity-50'
                    )}
                  >
                    {method.recommended && (
                      <Badge className="absolute -top-2 right-2 bg-emerald-500 text-[10px]">
                        Recomendado
                      </Badge>
                    )}
                    <div
                      className={cn(
                        'mb-2 flex h-10 w-10 items-center justify-center rounded-lg text-sm font-bold text-white',
                        method.color
                      )}
                    >
                      {method.icon}
                    </div>
                    <span className="text-sm font-semibold">{method.name}</span>
                    <span className="text-muted-foreground mt-0.5 text-center text-[11px] leading-tight">
                      {method.description}
                    </span>
                  </button>
                ))}
              </div>

              <Separator />

              {/* Security badges */}
              <div className="flex flex-wrap items-center justify-center gap-4 rounded-lg bg-gray-50 p-3">
                <div className="flex items-center gap-1.5 text-xs text-gray-600">
                  <Shield className="h-4 w-4 text-emerald-500" />
                  <span>Pago 100% seguro</span>
                </div>
                <div className="flex items-center gap-1.5 text-xs text-gray-600">
                  <Lock className="h-4 w-4 text-emerald-500" />
                  <span>Datos encriptados</span>
                </div>
                <div className="flex items-center gap-1.5 text-xs text-gray-600">
                  <ShieldCheck className="h-4 w-4 text-emerald-500" />
                  <span>Protección al comprador</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Order summary + payment CTA */}
          <Card>
            <CardContent className="space-y-4 pt-6">
              {/* Terms */}
              <label className="flex cursor-pointer items-start gap-3">
                <input
                  type="checkbox"
                  checked={acceptTerms}
                  onChange={e => setAcceptTerms(e.target.checked)}
                  className="mt-1 h-4 w-4 rounded border-gray-300"
                />
                <span className="text-sm">
                  Acepto los{' '}
                  <Link href="/terminos" className="text-primary underline">
                    términos y condiciones
                  </Link>{' '}
                  y la{' '}
                  <Link href="/privacidad" className="text-primary underline">
                    política de privacidad
                  </Link>
                  . Entiendo que se me cobrará{' '}
                  <strong>{formatPrice(selectedCard.price, 'DOP')}</strong>
                  {selectedCard.billingLabel}.
                </span>
              </label>

              <Separator />

              {/* Total */}
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-lg font-bold">
                    Total: {formatPrice(selectedCard.price, 'DOP')}
                    <span className="text-muted-foreground text-sm font-normal">
                      {selectedCard.billingLabel}
                    </span>
                  </p>
                  <p className="text-muted-foreground text-xs">
                    ≈ ${paypalAmount} USD — Plan {selectedCard.name}
                  </p>
                </div>
              </div>

              {/* PayPal */}
              {paymentMethod === 'paypal' && acceptTerms && (
                <PayPalPaymentButton
                  clientId={process.env.NEXT_PUBLIC_PAYPAL_CLIENT_ID || ''}
                  amount={paypalAmount}
                  currency="USD"
                  onCreateOrder={async () => {
                    const description = `OKLA Plan ${selectedCard.name} — ${vehicle.year} ${vehicle.make} ${vehicle.model}`;
                    const result = await serverCreatePayPalOrder(
                      parseFloat(paypalAmount),
                      'USD',
                      description,
                      vehicleId,
                      `${window.location.origin}/cuenta/mis-vehiculos?promoted=true`,
                      `${window.location.origin}/vender/promover/${vehicleId}`
                    );
                    if (!result.success || !result.data?.orderId) {
                      throw new Error(result.error || 'Error al crear orden PayPal');
                    }
                    return result.data.orderId;
                  }}
                  onApprove={async orderId => {
                    setIsProcessing(true);
                    const result = await serverCapturePayPalOrder(orderId, vehicleId);
                    setIsProcessing(false);
                    if (result.success && result.data?.status === 'COMPLETED') {
                      setShowSuccess(true);
                    } else {
                      toast.error(result.error || 'Error al procesar el pago de PayPal');
                    }
                  }}
                  onError={message => {
                    setIsProcessing(false);
                    toast.error(message);
                  }}
                  onCancel={() => toast.info('Pago cancelado')}
                  disabled={isProcessing}
                />
              )}

              {/* Azul / Fygaro */}
              {(paymentMethod === 'azul' || paymentMethod === 'fygaro') && (
                <Button
                  size="lg"
                  disabled={!acceptTerms || isProcessing}
                  onClick={handleAzulOrFygaro}
                  className={cn(
                    'w-full gap-2',
                    paymentMethod === 'azul' && 'bg-blue-600 hover:bg-blue-700'
                  )}
                >
                  {isProcessing ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Redirigiendo...
                    </>
                  ) : (
                    <>
                      <CreditCard className="h-4 w-4" />
                      Pagar con {paymentMethod === 'azul' ? 'AZUL' : 'Fygaro'} —{' '}
                      {formatPrice(selectedCard.price, 'DOP')}
                    </>
                  )}
                </Button>
              )}

              {/* Info when terms not accepted */}
              {paymentMethod === 'paypal' && !acceptTerms && (
                <div className="flex items-center gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
                  <AlertCircle className="h-4 w-4 shrink-0" />
                  <span>Acepta los términos y condiciones para continuar.</span>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Trust footer */}
          <div className="flex flex-col items-center gap-2 text-center">
            <div className="flex items-center gap-3">
              <img
                src="https://www.paypalobjects.com/webstatic/mktg/Logo/pp-logo-100px.png"
                alt="PayPal"
                className="h-5 opacity-50"
              />
              <span className="text-xs text-gray-300">|</span>
              <span className="text-xs font-medium text-gray-400">FYGARO</span>
              <span className="text-xs text-gray-300">|</span>
              <span className="text-xs font-medium text-blue-400">AZUL</span>
            </div>
            <p className="text-muted-foreground max-w-md text-xs">
              Tus datos financieros nunca son almacenados en nuestros servidores. Todos los pagos
              son procesados de forma segura por proveedores certificados PCI DSS.
            </p>
          </div>
        </>
      )}

      {/* Success Dialog */}
      <Dialog open={showSuccess} onOpenChange={setShowSuccess}>
        <DialogContent className="text-center sm:max-w-md">
          <DialogHeader>
            <div className="mx-auto mb-4 rounded-full bg-emerald-100 p-4">
              <PartyPopper className="h-10 w-10 text-emerald-600" />
            </div>
            <DialogTitle className="text-2xl">¡Promoción activada!</DialogTitle>
          </DialogHeader>
          <div className="space-y-3">
            <p className="text-muted-foreground">
              Tu plan ha sido actualizado a <strong>{selectedCard?.name}</strong>. Tu vehículo{' '}
              <strong>
                {vehicle.year} {vehicle.make} {vehicle.model}
              </strong>{' '}
              ahora cuenta con mayor visibilidad en OKLA.
            </p>
            <div className="rounded-lg bg-emerald-50 p-4">
              <p className="text-sm font-semibold text-emerald-800">
                Se ha enviado un recibo de confirmación a tu correo electrónico.
              </p>
            </div>
          </div>
          <DialogFooter className="mt-4 sm:justify-center">
            <Button
              onClick={() => {
                setShowSuccess(false);
                router.push('/cuenta/mis-vehiculos?promoted=true');
              }}
              className="gap-2"
            >
              Ver mis vehículos
              <ArrowRight className="h-4 w-4" />
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
