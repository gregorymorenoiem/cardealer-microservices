/**
 * Sell Your Car Landing Page
 *
 * Features:
 * - Hero section with value proposition
 * - How it works steps
 * - Benefits for sellers
 * - Pricing information
 * - CTA to start listing
 *
 * Route: /vender
 */

'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  Camera,
  DollarSign,
  Users,
  Shield,
  Clock,
  CheckCircle,
  ArrowRight,
  TrendingUp,
  Star,
  Zap,
} from 'lucide-react';

// =============================================================================
// STEPS DATA
// =============================================================================

const steps = [
  {
    number: '1',
    title: 'Crea tu anuncio',
    description: 'Ingresa los detalles de tu vehículo y sube fotos de calidad',
    icon: Camera,
  },
  {
    number: '2',
    title: 'Define tu precio',
    description: 'Te ayudamos con sugerencias basadas en el mercado dominicano',
    icon: DollarSign,
  },
  {
    number: '3',
    title: 'Recibe ofertas',
    description: 'Compradores interesados te contactarán directamente',
    icon: Users,
  },
  {
    number: '4',
    title: 'Cierra la venta',
    description: 'Coordina la entrega y recibe tu pago de forma segura',
    icon: Shield,
  },
];

// =============================================================================
// BENEFITS DATA
// =============================================================================

const benefits = [
  {
    title: 'Sin comisiones',
    description: 'Vende tu vehículo sin pagar comisión sobre la venta',
    icon: DollarSign,
  },
  {
    title: 'Rápido y fácil',
    description: 'Publica tu anuncio en menos de 5 minutos',
    icon: Zap,
  },
  {
    title: 'Máxima visibilidad',
    description: 'Tu anuncio llegará a miles de compradores en toda RD',
    icon: TrendingUp,
  },
  {
    title: 'Soporte 24/7',
    description: 'Nuestro equipo te ayuda en todo el proceso',
    icon: Star,
  },
];

// =============================================================================
// STATS DATA
// =============================================================================

const stats = [
  { value: '15K+', label: 'Vehículos vendidos' },
  { value: '7 días', label: 'Tiempo promedio de venta' },
  { value: '98%', label: 'Clientes satisfechos' },
  { value: 'RD$500M+', label: 'Valor transado' },
];

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function VenderPage() {
  return (
    <div className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-br from-[#00A870] via-[#009663] to-[#007d52]">
        {/* Background pattern */}
        <div className="absolute inset-0 opacity-10">
          <div
            className="absolute inset-0"
            style={{
              backgroundImage: 'radial-gradient(circle at 1px 1px, white 1px, transparent 0)',
              backgroundSize: '40px 40px',
            }}
          />
        </div>

        <div className="relative container mx-auto px-4 py-16 lg:py-24">
          <div className="mx-auto max-w-3xl text-center">
            {/* Badge */}
            <div className="mb-6 inline-flex items-center gap-2 rounded-full bg-white/20 px-4 py-2 text-sm font-medium text-white backdrop-blur-sm">
              <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
              La forma más fácil de vender tu carro en RD
            </div>

            {/* Title */}
            <h1 className="mb-6 text-4xl font-bold tracking-tight text-white md:text-5xl lg:text-6xl">
              Vende tu vehículo
              <br />
              <span className="text-yellow-300">al mejor precio</span>
            </h1>

            {/* Subtitle */}
            <p className="mx-auto mb-8 max-w-2xl text-lg text-white/90 md:text-xl">
              Conecta con miles de compradores en República Dominicana. Sin comisiones ocultas, sin
              intermediarios. Tú tienes el control.
            </p>

            {/* CTAs */}
            <div className="flex flex-col items-center justify-center gap-4 sm:flex-row">
              <Button
                asChild
                size="lg"
                className="gap-2 bg-white px-8 text-[#00A870] hover:bg-gray-100"
              >
                <Link href="/vender/publicar">
                  Publicar mi vehículo
                  <ArrowRight className="h-5 w-5" />
                </Link>
              </Button>
              <Button
                asChild
                variant="outline"
                size="lg"
                className="gap-2 border-white/30 bg-white/10 text-white hover:bg-white/20"
              >
                <Link href="#como-funciona">Ver cómo funciona</Link>
              </Button>
            </div>

            {/* Trust indicators */}
            <div className="mt-10 flex flex-wrap items-center justify-center gap-6 text-sm text-white/80">
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-yellow-300" />
                <span>100% Gratis</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-yellow-300" />
                <span>Sin comisiones</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-yellow-300" />
                <span>Soporte local</span>
              </div>
            </div>
          </div>
        </div>

        {/* Wave divider */}
        <div className="absolute right-0 bottom-0 left-0">
          <svg
            viewBox="0 0 1440 120"
            className="h-16 w-full text-white md:h-24"
            preserveAspectRatio="none"
          >
            <path fill="currentColor" d="M0,64L1440,32L1440,120L0,120Z" />
          </svg>
        </div>
      </section>

      {/* Stats Section */}
      <section className="border-b bg-white py-12">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
            {stats.map((stat, index) => (
              <div key={index} className="text-center">
                <div className="text-3xl font-bold text-[#00A870] md:text-4xl">{stat.value}</div>
                <div className="mt-1 text-sm text-gray-600">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How It Works Section */}
      <section id="como-funciona" className="bg-gray-50 py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mb-12 text-center">
            <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">¿Cómo funciona?</h2>
            <p className="mx-auto max-w-2xl text-lg text-gray-600">
              Vender tu vehículo en OKLA es rápido y sencillo. Sigue estos 4 pasos y conecta con
              compradores reales.
            </p>
          </div>

          <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
            {steps.map((step, index) => {
              const Icon = step.icon;
              return (
                <div key={index} className="relative">
                  {/* Connector line (hidden on last item and mobile) */}
                  {index < steps.length - 1 && (
                    <div className="absolute top-12 left-1/2 hidden h-0.5 w-full bg-[#00A870]/20 lg:block" />
                  )}

                  <Card className="relative bg-white">
                    <CardContent className="p-6 text-center">
                      {/* Step number badge */}
                      <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-[#00A870]/10">
                        <Icon className="h-8 w-8 text-[#00A870]" />
                      </div>

                      {/* Step number */}
                      <div className="mb-2 text-sm font-semibold text-[#00A870]">
                        Paso {step.number}
                      </div>

                      {/* Title */}
                      <h3 className="mb-2 text-lg font-bold text-gray-900">{step.title}</h3>

                      {/* Description */}
                      <p className="text-sm text-gray-600">{step.description}</p>
                    </CardContent>
                  </Card>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="bg-white py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mb-12 text-center">
            <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">
              ¿Por qué vender en OKLA?
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-gray-600">
              Somos la plataforma líder para compra y venta de vehículos en República Dominicana.
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
            {benefits.map((benefit, index) => {
              const Icon = benefit.icon;
              return (
                <Card key={index} className="border-none bg-gray-50 shadow-none">
                  <CardContent className="p-6">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-[#00A870]/10">
                      <Icon className="h-6 w-6 text-[#00A870]" />
                    </div>
                    <h3 className="mb-2 text-lg font-bold text-gray-900">{benefit.title}</h3>
                    <p className="text-sm text-gray-600">{benefit.description}</p>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* Pricing Section */}
      <section className="bg-gray-50 py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mx-auto max-w-3xl">
            <div className="mb-12 text-center">
              <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">
                Planes de publicación
              </h2>
              <p className="mx-auto max-w-2xl text-lg text-gray-600">
                Elige el plan que mejor se adapte a tus necesidades.
              </p>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              {/* Free Plan */}
              <Card className="border-2 border-gray-200">
                <CardContent className="p-6">
                  <div className="mb-4">
                    <h3 className="text-xl font-bold text-gray-900">Gratuito</h3>
                    <p className="text-sm text-gray-600">Para vendedores ocasionales</p>
                  </div>
                  <div className="mb-6">
                    <span className="text-4xl font-bold text-gray-900">RD$ 0</span>
                  </div>
                  <ul className="mb-6 space-y-3">
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />1 publicación activa
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Hasta 10 fotos
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Duración: 30 días
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Contacto por WhatsApp
                    </li>
                  </ul>
                  <Button asChild variant="outline" className="w-full">
                    <Link href="/vender/publicar">Comenzar gratis</Link>
                  </Button>
                </CardContent>
              </Card>

              {/* Premium Plan */}
              <Card className="relative border-2 border-[#00A870]">
                {/* Popular badge */}
                <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                  <span className="rounded-full bg-[#00A870] px-3 py-1 text-xs font-semibold text-white">
                    MÁS POPULAR
                  </span>
                </div>

                <CardContent className="p-6">
                  <div className="mb-4">
                    <h3 className="text-xl font-bold text-gray-900">Premium</h3>
                    <p className="text-sm text-gray-600">Vende más rápido</p>
                  </div>
                  <div className="mb-6">
                    <span className="text-4xl font-bold text-gray-900">RD$ 999</span>
                    <span className="text-sm text-gray-600">/mes</span>
                  </div>
                  <ul className="mb-6 space-y-3">
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Hasta 5 publicaciones activas
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Fotos ilimitadas
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Prioridad en búsquedas
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Badge de vendedor verificado
                    </li>
                    <li className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle className="h-4 w-4 text-[#00A870]" />
                      Estadísticas detalladas
                    </li>
                  </ul>
                  <Button asChild className="w-full bg-[#00A870] hover:bg-[#009663]">
                    <Link href="/vender/publicar?plan=premium">Elegir Premium</Link>
                  </Button>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </section>

      {/* Final CTA Section */}
      <section className="bg-[#00A870] py-16 lg:py-20">
        <div className="container mx-auto px-4">
          <div className="mx-auto max-w-3xl text-center">
            <h2 className="mb-4 text-3xl font-bold text-white md:text-4xl">
              ¿Listo para vender tu vehículo?
            </h2>
            <p className="mx-auto mb-8 max-w-xl text-lg text-white/90">
              Publica tu anuncio hoy y conecta con compradores interesados en toda República
              Dominicana.
            </p>
            <Button
              asChild
              size="lg"
              className="gap-2 bg-white px-8 text-[#00A870] hover:bg-gray-100"
            >
              <Link href="/vender/publicar">
                Publicar ahora
                <ArrowRight className="h-5 w-5" />
              </Link>
            </Button>

            <p className="mt-6 text-sm text-white/70">
              <Clock className="mr-1 inline h-4 w-4" />
              Publica en menos de 5 minutos
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
