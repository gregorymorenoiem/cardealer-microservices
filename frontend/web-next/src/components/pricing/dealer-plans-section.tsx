/**
 * Dealer Plans Section (Client Component)
 *
 * Renders dealer pricing plans using dynamic pricing from ConfigurationService.
 * Used in the dealers landing page (Server Component) as a client island.
 */

'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { CheckCircle } from 'lucide-react';
import { usePlatformPricing } from '@/hooks/use-platform-pricing';

export function DealerPlansSection() {
  const { pricing, formatPrice, isLoading } = usePlatformPricing();

  const plans = [
    {
      name: 'Starter',
      price: formatPrice(pricing.dealerStarter),
      period: '/mes',
      description: 'Para dealers pequeños',
      features: [
        `Hasta ${pricing.starterMaxVehicles} vehículos`,
        '1 ubicación',
        'Analytics básico',
        'Soporte por email',
      ],
      cta: 'Comenzar',
      highlighted: false,
    },
    {
      name: 'Profesional',
      price: formatPrice(pricing.dealerPro),
      period: '/mes',
      description: 'El más popular',
      features: [
        `Hasta ${pricing.proMaxVehicles} vehículos`,
        '3 ubicaciones',
        'Analytics Pro + CRM',
        'Boost de listados',
        'Soporte prioritario',
      ],
      cta: 'Elegir Plan',
      highlighted: true,
    },
    {
      name: 'Enterprise',
      price: formatPrice(pricing.dealerEnterprise),
      period: '/mes',
      description: 'Para grandes dealers',
      features: [
        'Vehículos ilimitados',
        'Ubicaciones ilimitadas',
        'Analytics + API',
        'Manager dedicado',
        'White label',
      ],
      cta: 'Contactar',
      highlighted: false,
    },
  ];

  if (isLoading) {
    return (
      <div className="grid gap-8 md:grid-cols-3">
        {[1, 2, 3].map(i => (
          <Card key={i} className="border-border animate-pulse">
            <CardContent className="p-8">
              <div className="mb-4 h-6 w-24 rounded bg-gray-200" />
              <div className="mb-6 h-10 w-32 rounded bg-gray-200" />
              <div className="space-y-3">
                {[1, 2, 3, 4].map(j => (
                  <div key={j} className="h-4 w-full rounded bg-gray-200" />
                ))}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  return (
    <div className="grid gap-8 md:grid-cols-3">
      {plans.map((plan, index) => (
        <Card
          key={index}
          className={`relative ${
            plan.highlighted ? 'border-2 border-[#00A870] shadow-lg' : 'border-border'
          }`}
        >
          {plan.highlighted && (
            <div className="absolute -top-3 left-1/2 -translate-x-1/2">
              <span className="rounded-full bg-[#00A870] px-3 py-1 text-xs font-semibold text-white">
                MÁS POPULAR
              </span>
            </div>
          )}
          <CardContent className="p-8">
            <div className="mb-4">
              <h3 className="text-foreground text-xl font-bold">{plan.name}</h3>
              <p className="text-muted-foreground text-sm">{plan.description}</p>
            </div>
            <div className="mb-6">
              <span className="text-foreground text-3xl font-bold">{plan.price}</span>
              <span className="text-muted-foreground text-sm">{plan.period}</span>
            </div>
            <ul className="mb-8 space-y-3">
              {plan.features.map((feature, featureIndex) => (
                <li
                  key={featureIndex}
                  className="text-muted-foreground flex items-center gap-2 text-sm"
                >
                  <CheckCircle className="h-4 w-4 text-[#00A870]" />
                  {feature}
                </li>
              ))}
            </ul>
            <Button
              asChild
              className={`w-full ${
                plan.highlighted
                  ? 'bg-[#00A870] hover:bg-[#009663]'
                  : 'bg-secondary text-secondary-foreground hover:bg-secondary/80'
              }`}
            >
              <Link href="/dealers/registro">{plan.cta}</Link>
            </Button>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
