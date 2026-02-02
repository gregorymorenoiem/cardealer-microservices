/**
 * Dealer Subscription Page
 *
 * Manage subscription plan and upgrade/downgrade with real API integration
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Crown, Check, Zap, Star, Car, AlertCircle, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { useCurrentDealer, useEarlyBird, dealerService } from '@/hooks/use-dealers';
import {
  useSubscription,
  useChangePlan,
  useCancelSubscription,
  usePlans,
  useUsageMetrics,
} from '@/hooks/use-dealer-billing';

// =============================================================================
// HELPERS
// =============================================================================

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(price);
};

// Plan features
const PLAN_FEATURES: Record<string, string[]> = {
  starter: [
    '15 vehículos activos',
    'Panel de control básico',
    'Estadísticas básicas',
    'Soporte por email',
  ],
  pro: [
    '50 vehículos activos',
    'Panel de control completo',
    'Estadísticas avanzadas',
    '5 publicaciones destacadas/mes',
    'CRM integrado',
    'Soporte prioritario',
  ],
  enterprise: [
    'Vehículos ilimitados',
    'Panel de control completo',
    'Estadísticas avanzadas + API',
    '15 publicaciones destacadas/mes',
    'CRM integrado + WhatsApp',
    'Múltiples ubicaciones',
    'Empleados ilimitados',
    'Soporte 24/7 + Manager dedicado',
  ],
};

// =============================================================================
// SKELETON COMPONENTS
// =============================================================================

function SubscriptionSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <Skeleton className="mb-2 h-8 w-48" />
          <Skeleton className="h-5 w-64" />
        </div>
      </div>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <Skeleton className="h-14 w-14 rounded-xl" />
            <div>
              <Skeleton className="mb-2 h-8 w-48" />
              <Skeleton className="h-5 w-64" />
            </div>
          </div>
        </CardContent>
      </Card>
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardContent className="p-6">
            <Skeleton className="mb-2 h-10 w-24" />
            <Skeleton className="h-3 w-full rounded-full" />
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-6">
            <Skeleton className="mb-2 h-10 w-24" />
            <Skeleton className="h-3 w-full rounded-full" />
          </CardContent>
        </Card>
      </div>
      <div className="grid gap-6 md:grid-cols-3">
        {[1, 2, 3].map(i => (
          <Card key={i}>
            <CardHeader>
              <Skeleton className="h-6 w-24" />
              <Skeleton className="h-4 w-40" />
              <Skeleton className="mt-4 h-10 w-32" />
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[1, 2, 3, 4].map(j => (
                  <Skeleton key={j} className="h-4 w-full" />
                ))}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE COMPONENT
// =============================================================================

export default function DealerSubscriptionPage() {
  const { data: dealer, isLoading: isDealerLoading } = useCurrentDealer();
  const dealerId = dealer?.id ?? '';
  const { data: subscription, isLoading: isSubscriptionLoading } = useSubscription(dealerId);
  const { data: usage } = useUsageMetrics(dealerId);
  const { data: plans } = usePlans();
  const changePlanMutation = useChangePlan(dealerId);
  const cancelMutation = useCancelSubscription(dealerId);
  const earlyBird = useEarlyBird();

  const [cancelConfirm, setCancelConfirm] = useState(false);

  const isLoading = isDealerLoading || isSubscriptionLoading;

  // Get static plan info
  const dealerPlans = dealerService.DEALER_PLANS;

  const handleUpgrade = async (planId: string) => {
    try {
      await changePlanMutation.mutateAsync(planId);
      toast.success('Plan actualizado correctamente');
    } catch {
      toast.error('Error al actualizar el plan');
    }
  };

  const handleCancel = async () => {
    if (!cancelConfirm) {
      setCancelConfirm(true);
      return;
    }

    try {
      await cancelMutation.mutateAsync(false); // false = cancel at end of period
      toast.success('Suscripción cancelada');
      setCancelConfirm(false);
    } catch {
      toast.error('Error al cancelar la suscripción');
    }
  };

  if (isLoading) {
    return <SubscriptionSkeleton />;
  }

  if (!dealer) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Card className="w-full max-w-md p-6 text-center">
          <CardContent>
            <AlertCircle className="mx-auto mb-4 h-12 w-12 text-amber-500" />
            <h2 className="mb-2 text-xl font-semibold">No se encontró el dealer</h2>
            <p className="text-gray-600">Por favor, inicia sesión como dealer.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const currentPlanInfo = dealerPlans.find(p => p.plan === dealer.plan);
  const vehicleUsagePercent =
    dealer.maxActiveListings === -1
      ? 10
      : Math.min(100, (dealer.currentActiveListings / dealer.maxActiveListings) * 100);
  const vehiclesRemaining =
    dealer.maxActiveListings === -1 ? '∞' : dealer.maxActiveListings - dealer.currentActiveListings;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Suscripción</h1>
          <p className="text-gray-600">Gestiona tu plan y uso</p>
        </div>
      </div>

      {/* Early Bird Banner */}
      {earlyBird.isActive && dealer.plan === 'starter' && (
        <Card className="border-amber-200 bg-gradient-to-r from-amber-50 to-orange-50">
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <Star className="h-6 w-6 text-amber-500" />
              <div className="flex-1">
                <p className="font-medium text-amber-800">
                  🎉 Early Bird: ¡{earlyBird.daysRemaining} días restantes!
                </p>
                <p className="text-sm text-amber-600">
                  Upgrade ahora y obtén 20% de descuento de por vida + 3 meses gratis
                </p>
              </div>
              <Button
                className="bg-amber-500 hover:bg-amber-600"
                onClick={() => handleUpgrade('pro')}
              >
                Upgrade con Early Bird
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Current Plan */}
      <Card className="border-emerald-200 bg-gradient-to-r from-emerald-50 to-white">
        <CardContent className="p-6">
          <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
            <div className="flex items-center gap-4">
              <div className="rounded-xl bg-emerald-600 p-3">
                <Crown className="h-8 w-8 text-white" />
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <h2 className="text-2xl font-bold">
                    Plan {currentPlanInfo?.name || dealer.plan}
                  </h2>
                  <Badge className={dealer.isSubscriptionActive ? 'bg-emerald-600' : 'bg-gray-400'}>
                    {dealer.isSubscriptionActive ? 'Activo' : 'Inactivo'}
                  </Badge>
                </div>
                <p className="text-gray-600">
                  {currentPlanInfo && formatPrice(currentPlanInfo.price)}/mes
                  {subscription?.currentPeriodEnd &&
                    ` • Renueva el ${new Date(subscription.currentPeriodEnd).toLocaleDateString('es-DO')}`}
                </p>
              </div>
            </div>
            <div className="flex gap-3">
              {cancelConfirm ? (
                <>
                  <Button variant="outline" onClick={() => setCancelConfirm(false)}>
                    No, mantener
                  </Button>
                  <Button
                    variant="destructive"
                    onClick={handleCancel}
                    disabled={cancelMutation.isPending}
                  >
                    {cancelMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Sí, cancelar
                  </Button>
                </>
              ) : (
                <>
                  <Button variant="outline" onClick={handleCancel}>
                    Cancelar Plan
                  </Button>
                  {dealer.plan !== 'enterprise' && (
                    <Button
                      className="bg-emerald-600 hover:bg-emerald-700"
                      onClick={() => handleUpgrade('enterprise')}
                      disabled={changePlanMutation.isPending}
                    >
                      {changePlanMutation.isPending ? (
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      ) : (
                        <Zap className="mr-2 h-4 w-4" />
                      )}
                      Upgrade a Enterprise
                    </Button>
                  )}
                </>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Usage */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Car className="h-5 w-5" />
              Vehículos Activos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="mb-2 flex items-end justify-between">
              <span className="text-3xl font-bold">{dealer.currentActiveListings}</span>
              <span className="text-gray-500">
                de {dealer.maxActiveListings === -1 ? '∞' : dealer.maxActiveListings}
              </span>
            </div>
            <div className="h-3 overflow-hidden rounded-full bg-gray-100">
              <div
                className="h-full rounded-full bg-emerald-500"
                style={{ width: `${vehicleUsagePercent}%` }}
              />
            </div>
            <p className="mt-2 text-sm text-gray-500">
              Te quedan {vehiclesRemaining} espacios disponibles
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-lg">
              <Star className="h-5 w-5" />
              Destacados del Mes
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="mb-2 flex items-end justify-between">
              <span className="text-3xl font-bold">{usage?.featuredListings || 0}</span>
              <span className="text-gray-500">
                de{' '}
                {usage?.maxFeatured ||
                  (dealer.plan === 'starter' ? 0 : dealer.plan === 'pro' ? 5 : 15)}
              </span>
            </div>
            <div className="h-3 overflow-hidden rounded-full bg-gray-100">
              <div
                className="h-full rounded-full bg-amber-500"
                style={{
                  width: `${Math.min(100, ((usage?.featuredListings || 0) / (usage?.maxFeatured || 1)) * 100)}%`,
                }}
              />
            </div>
            <p className="mt-2 text-sm text-gray-500">
              {(usage?.maxFeatured || 0) - (usage?.featuredListings || 0)} destacados disponibles
              este mes
            </p>
          </CardContent>
        </Card>
      </div>

      {/* All Plans */}
      <div>
        <h2 className="mb-4 text-xl font-bold">Todos los Planes</h2>
        <div className="grid gap-6 md:grid-cols-3">
          {dealerPlans.map(plan => {
            const isCurrent = plan.plan === dealer.plan;
            const features = PLAN_FEATURES[plan.plan] || [];
            const earlyBirdPrice = earlyBird.isActive
              ? earlyBird.getDiscountedPrice(plan.price)
              : plan.price;

            return (
              <Card
                key={plan.plan}
                className={`relative ${
                  plan.isPopular ? 'border-emerald-500 shadow-lg' : ''
                } ${isCurrent ? 'ring-2 ring-emerald-500' : ''}`}
              >
                {plan.isPopular && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <Badge className="bg-emerald-600">Más Popular</Badge>
                  </div>
                )}
                {isCurrent && (
                  <div className="absolute -top-3 right-4">
                    <Badge variant="outline" className="bg-white">
                      Tu Plan
                    </Badge>
                  </div>
                )}
                <CardHeader>
                  <CardTitle>{plan.name}</CardTitle>
                  <CardDescription>
                    {plan.maxListings === -1 ? 'Ilimitado' : `Hasta ${plan.maxListings} vehículos`}
                  </CardDescription>
                  <div className="pt-4">
                    {earlyBird.isActive && !isCurrent ? (
                      <>
                        <span className="text-lg text-gray-400 line-through">
                          {formatPrice(plan.price)}
                        </span>
                        <span className="ml-2 text-3xl font-bold text-emerald-600">
                          {formatPrice(earlyBirdPrice)}
                        </span>
                      </>
                    ) : (
                      <span className="text-3xl font-bold">{formatPrice(plan.price)}</span>
                    )}
                    <span className="text-gray-500">/mes</span>
                  </div>
                </CardHeader>
                <CardContent>
                  <ul className="mb-6 space-y-3">
                    {features.map((feature, idx) => (
                      <li key={idx} className="flex items-center gap-2 text-sm">
                        <Check className="h-4 w-4 flex-shrink-0 text-emerald-500" />
                        {feature}
                      </li>
                    ))}
                  </ul>
                  {isCurrent ? (
                    <Button variant="outline" className="w-full" disabled>
                      Plan Actual
                    </Button>
                  ) : plan.plan === 'enterprise' ? (
                    <Button
                      className="w-full bg-emerald-600 hover:bg-emerald-700"
                      onClick={() => handleUpgrade(plan.plan)}
                      disabled={changePlanMutation.isPending}
                    >
                      {changePlanMutation.isPending && (
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      )}
                      Upgrade
                    </Button>
                  ) : plan.plan === 'starter' && dealer.plan !== 'starter' ? (
                    <Button
                      variant="outline"
                      className="w-full text-gray-500"
                      onClick={() => handleUpgrade(plan.plan)}
                      disabled={changePlanMutation.isPending}
                    >
                      Downgrade
                    </Button>
                  ) : (
                    <Button
                      variant="outline"
                      className="w-full"
                      onClick={() => handleUpgrade(plan.plan)}
                      disabled={changePlanMutation.isPending}
                    >
                      Seleccionar
                    </Button>
                  )}
                </CardContent>
              </Card>
            );
          })}
        </div>
      </div>

      {/* Features Comparison */}
      <Card>
        <CardHeader>
          <CardTitle>Comparar Características</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b">
                  <th className="p-3 text-left">Característica</th>
                  <th className="p-3 text-center">Starter</th>
                  <th className="bg-emerald-50 p-3 text-center">Pro</th>
                  <th className="p-3 text-center">Enterprise</th>
                </tr>
              </thead>
              <tbody>
                {[
                  {
                    feature: 'Vehículos activos',
                    starter: '15',
                    pro: '50',
                    enterprise: 'Ilimitado',
                  },
                  { feature: 'Fotos por vehículo', starter: '10', pro: '20', enterprise: '30' },
                  { feature: 'Destacados/mes', starter: '0', pro: '5', enterprise: '15' },
                  {
                    feature: 'Estadísticas',
                    starter: 'Básicas',
                    pro: 'Avanzadas',
                    enterprise: 'Avanzadas + API',
                  },
                  { feature: 'CRM', starter: false, pro: true, enterprise: true },
                  {
                    feature: 'Múltiples ubicaciones',
                    starter: false,
                    pro: false,
                    enterprise: true,
                  },
                  { feature: 'Empleados', starter: '1', pro: '5', enterprise: 'Ilimitado' },
                  {
                    feature: 'Soporte',
                    starter: 'Email',
                    pro: 'Prioritario',
                    enterprise: '24/7 + Manager',
                  },
                ].map(row => (
                  <tr key={row.feature} className="border-b">
                    <td className="p-3 font-medium">{row.feature}</td>
                    <td className="p-3 text-center">
                      {typeof row.starter === 'boolean' ? (
                        row.starter ? (
                          <Check className="mx-auto h-4 w-4 text-emerald-500" />
                        ) : (
                          '—'
                        )
                      ) : (
                        row.starter
                      )}
                    </td>
                    <td className="bg-emerald-50 p-3 text-center">
                      {typeof row.pro === 'boolean' ? (
                        row.pro ? (
                          <Check className="mx-auto h-4 w-4 text-emerald-500" />
                        ) : (
                          '—'
                        )
                      ) : (
                        row.pro
                      )}
                    </td>
                    <td className="p-3 text-center">
                      {typeof row.enterprise === 'boolean' ? (
                        row.enterprise ? (
                          <Check className="mx-auto h-4 w-4 text-emerald-500" />
                        ) : (
                          '—'
                        )
                      ) : (
                        row.enterprise
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* FAQ */}
      <Card>
        <CardHeader>
          <CardTitle>Preguntas Frecuentes</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="font-medium">¿Puedo cambiar de plan en cualquier momento?</h4>
            <p className="mt-1 text-sm text-gray-600">
              Sí, puedes hacer upgrade o downgrade en cualquier momento. Los cambios se aplican
              inmediatamente y se prorratea la diferencia.
            </p>
          </div>
          <div>
            <h4 className="font-medium">¿Qué pasa si excedo el límite de vehículos?</h4>
            <p className="mt-1 text-sm text-gray-600">
              No podrás publicar nuevos vehículos hasta que elimines alguno o hagas upgrade a un
              plan superior.
            </p>
          </div>
          <div>
            <h4 className="font-medium">¿Hay contrato mínimo?</h4>
            <p className="mt-1 text-sm text-gray-600">
              No, todos los planes son mes a mes. Puedes cancelar en cualquier momento sin
              penalidad.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
