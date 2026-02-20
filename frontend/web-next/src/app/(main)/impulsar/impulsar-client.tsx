'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import type { PricingEstimate } from '@/types/advertising';

interface ImpulsarClientProps {
  pricing: {
    featured: PricingEstimate | null;
    premium: PricingEstimate | null;
  };
}

const benefits = [
  {
    icon: '🚀',
    title: 'Más Visibilidad',
    description: 'Tu vehículo aparece en las posiciones destacadas de la página principal.',
  },
  {
    icon: '📊',
    title: 'Estadísticas en Tiempo Real',
    description: 'Ve cuántas personas ven tu anuncio, hacen clic y muestran interés.',
  },
  {
    icon: '🎯',
    title: 'Rotación Inteligente',
    description: 'Nuestro algoritmo optimiza la visibilidad según calidad y demanda.',
  },
  {
    icon: '💰',
    title: 'Paga por Resultados',
    description: 'Elige entre pago por vista, por clic o tarifa fija. Tú decides.',
  },
  {
    icon: '⏱️',
    title: 'Flexibilidad Total',
    description: 'Pausa, reanuda o cancela tu campaña en cualquier momento.',
  },
  {
    icon: '📈',
    title: 'Vende Más Rápido',
    description: 'Los vehículos promocionados se venden hasta 3x más rápido.',
  },
];

export default function ImpulsarClient({ pricing }: ImpulsarClientProps) {
  const [selectedPlan, setSelectedPlan] = useState<'FeaturedSpot' | 'PremiumSpot'>('FeaturedSpot');

  return (
    <div className="min-h-screen">
      {/* Hero */}
      <section className="from-primary/10 via-background to-primary/5 bg-gradient-to-br py-16 lg:py-24">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <Badge variant="secondary" className="mb-4 text-sm">
            🚀 Nuevo
          </Badge>
          <h1 className="text-foreground text-4xl font-bold tracking-tight lg:text-5xl">
            Impulsa tu Vehículo
          </h1>
          <p className="text-muted-foreground mx-auto mt-4 max-w-2xl text-lg">
            Destaca tu vehículo en la página principal de OKLA y llega a miles de compradores
            potenciales en República Dominicana.
          </p>
          <div className="mt-8 flex justify-center gap-4">
            <Link href="/publicar">
              <Button size="lg">Crear Campaña</Button>
            </Link>
            <Link href="#precios">
              <Button size="lg" variant="outline">
                Ver Precios
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Benefits */}
      <section className="bg-muted/30 py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="mb-12 text-center text-2xl font-bold">¿Por qué impulsar tu vehículo?</h2>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {benefits.map(b => (
              <Card key={b.title} className="border-0 shadow-sm">
                <CardContent className="pt-6">
                  <span className="text-3xl">{b.icon}</span>
                  <h3 className="mt-3 text-lg font-semibold">{b.title}</h3>
                  <p className="text-muted-foreground mt-2 text-sm">{b.description}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Pricing */}
      <section id="precios" className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="mb-4 text-center text-2xl font-bold">Planes de Promoción</h2>
          <p className="text-muted-foreground mx-auto mb-12 max-w-xl text-center">
            Elige el plan que mejor se adapte a tu presupuesto y objetivos.
          </p>

          <div className="mx-auto grid max-w-4xl grid-cols-1 gap-8 md:grid-cols-2">
            {/* Featured Spot */}
            <Card
              className={`cursor-pointer transition-all ${selectedPlan === 'FeaturedSpot' ? 'ring-primary ring-2' : ''}`}
              onClick={() => setSelectedPlan('FeaturedSpot')}
            >
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  ⭐ Destacado
                  <Badge variant="secondary">Popular</Badge>
                </CardTitle>
                <CardDescription>
                  Tu vehículo aparece en la sección de destacados de la página principal.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-primary mb-4 text-3xl font-bold">
                  RD$0.50 <span className="text-muted-foreground text-sm font-normal">/ vista</span>
                </div>
                <ul className="space-y-2 text-sm">
                  <li className="flex items-center gap-2">✅ Posición destacada en homepage</li>
                  <li className="flex items-center gap-2">✅ Rotación inteligente</li>
                  <li className="flex items-center gap-2">✅ Estadísticas básicas</li>
                  <li className="flex items-center gap-2">✅ Pausa/Reanuda cuando quieras</li>
                </ul>
                {pricing.featured && (
                  <p className="text-muted-foreground mt-4 text-xs">
                    ~{pricing.featured.pricingModels?.[0]?.estimatedDailyViews || 500} vistas
                    estimadas/día
                  </p>
                )}
              </CardContent>
            </Card>

            {/* Premium Spot */}
            <Card
              className={`cursor-pointer transition-all ${selectedPlan === 'PremiumSpot' ? 'ring-primary ring-2' : ''}`}
              onClick={() => setSelectedPlan('PremiumSpot')}
            >
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  💎 Premium
                  <Badge className="bg-gradient-to-r from-amber-500 to-orange-500 text-white">
                    Pro
                  </Badge>
                </CardTitle>
                <CardDescription>
                  Máxima visibilidad. Posición premium con badge especial y prioridad.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-primary mb-4 text-3xl font-bold">
                  RD$1.00 <span className="text-muted-foreground text-sm font-normal">/ vista</span>
                </div>
                <ul className="space-y-2 text-sm">
                  <li className="flex items-center gap-2">✅ Todo lo de Destacado</li>
                  <li className="flex items-center gap-2">✅ Posición premium (primeras filas)</li>
                  <li className="flex items-center gap-2">✅ Badge &quot;Premium&quot; visible</li>
                  <li className="flex items-center gap-2">✅ Reportes avanzados</li>
                  <li className="flex items-center gap-2">✅ Prioridad en rotación</li>
                </ul>
                {pricing.premium && (
                  <p className="text-muted-foreground mt-4 text-xs">
                    ~{pricing.premium.pricingModels?.[0]?.estimatedDailyViews || 1000} vistas
                    estimadas/día
                  </p>
                )}
              </CardContent>
            </Card>
          </div>

          <div className="mt-8 text-center">
            <Link href="/publicar">
              <Button size="lg" className="px-8">
                Impulsar Mi Vehículo →
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* FAQ */}
      <section className="bg-muted/30 py-16">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <h2 className="mb-8 text-center text-2xl font-bold">Preguntas Frecuentes</h2>
          <div className="space-y-6">
            <div>
              <h3 className="font-semibold">¿Cuánto cuesta promocionar un vehículo?</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                Desde RD$0.50 por vista en el plan Destacado. Tú estableces tu presupuesto total y
                la campaña se detiene automáticamente cuando se agota.
              </p>
            </div>
            <div>
              <h3 className="font-semibold">¿Puedo cancelar en cualquier momento?</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                Sí. Puedes pausar, reanudar o cancelar tu campaña cuando quieras. Solo pagas por las
                vistas/clics que ya se realizaron.
              </p>
            </div>
            <div>
              <h3 className="font-semibold">¿Cómo funciona la rotación?</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                Nuestro algoritmo rota los vehículos destacados cada 30 minutos, asegurando
                visibilidad equitativa basada en calidad y presupuesto.
              </p>
            </div>
            <div>
              <h3 className="font-semibold">¿Qué métricas puedo ver?</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                Vistas totales, clics, tasa de clic (CTR), presupuesto restante y datos diarios con
                gráficas detalladas.
              </p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
