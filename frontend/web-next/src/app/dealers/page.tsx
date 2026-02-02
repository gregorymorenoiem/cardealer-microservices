/**
 * Dealers Landing Page
 *
 * Features:
 * - Hero section for dealer recruitment
 * - Stats and social proof
 * - Features overview
 * - Pricing plans preview
 * - Testimonials
 * - CTAs for registration
 *
 * Route: /dealers
 */

import { Metadata } from 'next';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  BarChart3,
  Camera,
  Users,
  Smartphone,
  Search,
  MessageSquare,
  Star,
  ArrowRight,
  CheckCircle,
  Building2,
  TrendingUp,
  Shield,
  Zap,
} from 'lucide-react';

// =============================================================================
// METADATA
// =============================================================================

export const metadata: Metadata = {
  title: 'Para Dealers | OKLA - Plataforma de Venta de Vehículos',
  description:
    'Únete a la plataforma líder de venta de vehículos en República Dominicana. +500 dealers confían en nosotros. Herramientas profesionales para concesionarios.',
  openGraph: {
    title: 'OKLA para Dealers - Vende más vehículos',
    description: 'Herramientas profesionales para concesionarios',
  },
};

// =============================================================================
// DATA
// =============================================================================

const stats = [
  { value: '500+', label: 'Dealers activos' },
  { value: '10K+', label: 'Ventas mensuales' },
  { value: '95%', label: 'Satisfacción' },
  { value: '24h', label: 'Tiempo de soporte' },
];

const features = [
  {
    icon: BarChart3,
    title: 'Dashboard Profesional',
    description: 'Gestiona tu inventario, leads y ventas desde un solo lugar',
  },
  {
    icon: Camera,
    title: 'Fotos 360° Profesionales',
    description: 'Muestra tus vehículos con tours virtuales interactivos',
  },
  {
    icon: TrendingUp,
    title: 'Analytics en Tiempo Real',
    description: 'Métricas detalladas de rendimiento y conversión',
  },
  {
    icon: MessageSquare,
    title: 'CRM Integrado',
    description: 'Gestiona todos tus leads y seguimientos',
  },
  {
    icon: Smartphone,
    title: 'App Móvil Incluida',
    description: 'Gestiona tu negocio desde cualquier lugar',
  },
  {
    icon: Search,
    title: 'SEO Optimizado',
    description: 'Tus vehículos aparecen primero en búsquedas',
  },
];

const plans = [
  {
    name: 'Starter',
    price: 'RD$ 2,999',
    period: '/mes',
    description: 'Para dealers pequeños',
    features: ['Hasta 15 vehículos', '1 ubicación', 'Analytics básico', 'Soporte por email'],
    cta: 'Comenzar',
    highlighted: false,
  },
  {
    name: 'Profesional',
    price: 'RD$ 5,999',
    period: '/mes',
    description: 'El más popular',
    features: [
      'Hasta 50 vehículos',
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
    price: 'RD$ 14,999',
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

const testimonials = [
  {
    quote: 'Aumentamos nuestras ventas un 40% en los primeros 3 meses. La plataforma es increíble.',
    author: 'Juan Pérez',
    company: 'AutoMax RD',
    rating: 5,
  },
  {
    quote: 'El CRM integrado nos ha ahorrado horas de trabajo. Ahora todo está centralizado.',
    author: 'María García',
    company: 'Caribbean Motors',
    rating: 5,
  },
  {
    quote: 'El soporte es excepcional. Siempre responden rápido y resuelven cualquier duda.',
    author: 'Carlos Martínez',
    company: 'Premium Auto',
    rating: 5,
  },
];

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function DealersPage() {
  return (
    <div className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="relative overflow-hidden bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900">
        {/* Background pattern */}
        <div className="absolute inset-0 opacity-5">
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
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-[#00A870]/30 bg-[#00A870]/10 px-4 py-2 text-sm font-medium text-[#00A870]">
              <Building2 className="h-4 w-4" />
              Plataforma #1 para Dealers en RD
            </div>

            {/* Title */}
            <h1 className="mb-6 text-4xl font-bold tracking-tight text-white md:text-5xl lg:text-6xl">
              Vende más vehículos
              <br />
              <span className="text-[#00A870]">con OKLA</span>
            </h1>

            {/* Subtitle */}
            <p className="mx-auto mb-8 max-w-2xl text-lg text-gray-400 md:text-xl">
              Herramientas profesionales para concesionarios. Dashboard, CRM, analytics y más. Únete
              a los +500 dealers que ya confían en nosotros.
            </p>

            {/* CTAs */}
            <div className="flex flex-col items-center justify-center gap-4 sm:flex-row">
              <Button asChild size="lg" className="gap-2 bg-[#00A870] px-8 hover:bg-[#009663]">
                <Link href="/dealers/registro">
                  Comenzar prueba gratis
                  <ArrowRight className="h-5 w-5" />
                </Link>
              </Button>
              <Button
                asChild
                variant="outline"
                size="lg"
                className="gap-2 border-gray-600 text-white hover:bg-gray-800"
              >
                <Link href="#planes">Ver planes</Link>
              </Button>
            </div>

            {/* Trust indicators */}
            <div className="mt-10 flex flex-wrap items-center justify-center gap-6 text-sm text-gray-500">
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-[#00A870]" />
                <span>14 días gratis</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-[#00A870]" />
                <span>Sin tarjeta de crédito</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5 text-[#00A870]" />
                <span>Cancela cuando quieras</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="border-b bg-white py-12">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
            {stats.map((stat, index) => (
              <div key={index} className="text-center">
                <div className="text-3xl font-bold text-slate-900 md:text-4xl">{stat.value}</div>
                <div className="mt-1 text-sm text-gray-600">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="bg-gray-50 py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mb-12 text-center">
            <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">
              Todo lo que necesitas para vender más
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-gray-600">
              Herramientas profesionales diseñadas para concesionarios en República Dominicana.
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {features.map((feature, index) => {
              const Icon = feature.icon;
              return (
                <Card key={index} className="border-none bg-white shadow-sm">
                  <CardContent className="p-6">
                    <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-[#00A870]/10">
                      <Icon className="h-6 w-6 text-[#00A870]" />
                    </div>
                    <h3 className="mb-2 text-lg font-bold text-gray-900">{feature.title}</h3>
                    <p className="text-sm text-gray-600">{feature.description}</p>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      </section>

      {/* Pricing Section */}
      <section id="planes" className="bg-white py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mb-12 text-center">
            <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">
              Planes para cada tipo de dealer
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-gray-600">
              Elige el plan que mejor se adapte a tu negocio. Todos incluyen 14 días de prueba
              gratis.
            </p>
          </div>

          <div className="mx-auto grid max-w-5xl gap-6 md:grid-cols-3">
            {plans.map((plan, index) => (
              <Card
                key={index}
                className={`relative ${
                  plan.highlighted ? 'border-2 border-[#00A870] shadow-lg' : 'border-gray-200'
                }`}
              >
                {plan.highlighted && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                    <span className="rounded-full bg-[#00A870] px-3 py-1 text-xs font-semibold text-white">
                      MÁS POPULAR
                    </span>
                  </div>
                )}

                <CardContent className="p-6">
                  <div className="mb-4">
                    <h3 className="text-xl font-bold text-gray-900">{plan.name}</h3>
                    <p className="text-sm text-gray-600">{plan.description}</p>
                  </div>

                  <div className="mb-6">
                    <span className="text-3xl font-bold text-gray-900">{plan.price}</span>
                    <span className="text-sm text-gray-600">{plan.period}</span>
                  </div>

                  <ul className="mb-6 space-y-3">
                    {plan.features.map((feature, featureIndex) => (
                      <li
                        key={featureIndex}
                        className="flex items-center gap-2 text-sm text-gray-600"
                      >
                        <CheckCircle className="h-4 w-4 flex-shrink-0 text-[#00A870]" />
                        {feature}
                      </li>
                    ))}
                  </ul>

                  <Button
                    asChild
                    className={`w-full ${
                      plan.highlighted
                        ? 'bg-[#00A870] hover:bg-[#009663]'
                        : 'bg-gray-900 hover:bg-gray-800'
                    }`}
                  >
                    <Link href="/dealers/registro">{plan.cta}</Link>
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Testimonials Section */}
      <section className="bg-gray-50 py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="mb-12 text-center">
            <h2 className="mb-4 text-3xl font-bold text-gray-900 md:text-4xl">
              Lo que dicen nuestros dealers
            </h2>
            <p className="mx-auto max-w-2xl text-lg text-gray-600">
              Únete a cientos de dealers satisfechos en toda República Dominicana.
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-3">
            {testimonials.map((testimonial, index) => (
              <Card key={index} className="bg-white">
                <CardContent className="p-6">
                  {/* Stars */}
                  <div className="mb-4 flex gap-1">
                    {Array.from({ length: testimonial.rating }).map((_, i) => (
                      <Star key={i} className="h-5 w-5 fill-yellow-400 text-yellow-400" />
                    ))}
                  </div>

                  {/* Quote */}
                  <p className="mb-4 text-gray-700">&ldquo;{testimonial.quote}&rdquo;</p>

                  {/* Author */}
                  <div>
                    <div className="font-semibold text-gray-900">{testimonial.author}</div>
                    <div className="text-sm text-gray-600">{testimonial.company}</div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Why Choose Us Section */}
      <section className="bg-white py-16 lg:py-24">
        <div className="container mx-auto px-4">
          <div className="grid items-center gap-12 lg:grid-cols-2">
            <div>
              <h2 className="mb-6 text-3xl font-bold text-gray-900 md:text-4xl">
                ¿Por qué elegir OKLA?
              </h2>

              <div className="space-y-6">
                <div className="flex gap-4">
                  <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Users className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900">Audiencia verificada</h3>
                    <p className="text-sm text-gray-600">
                      Miles de compradores activos buscando vehículos cada día en RD.
                    </p>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Zap className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900">Resultados rápidos</h3>
                    <p className="text-sm text-gray-600">
                      Nuestros dealers venden en promedio 3x más rápido que en otras plataformas.
                    </p>
                  </div>
                </div>

                <div className="flex gap-4">
                  <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-lg bg-[#00A870]/10">
                    <Shield className="h-5 w-5 text-[#00A870]" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900">Plataforma segura</h3>
                    <p className="text-sm text-gray-600">
                      Transacciones protegidas y verificación de compradores.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="rounded-2xl bg-gray-100 p-8 lg:p-12">
              <div className="text-center">
                <div className="mb-4 text-5xl font-bold text-[#00A870]">40%</div>
                <div className="mb-2 text-xl font-semibold text-gray-900">
                  Aumento promedio en ventas
                </div>
                <p className="text-gray-600">
                  Nuestros dealers reportan un incremento significativo en sus ventas dentro de los
                  primeros 3 meses.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Final CTA Section */}
      <section className="bg-slate-900 py-16 lg:py-20">
        <div className="container mx-auto px-4">
          <div className="mx-auto max-w-3xl text-center">
            <h2 className="mb-4 text-3xl font-bold text-white md:text-4xl">
              ¿Listo para vender más?
            </h2>
            <p className="mx-auto mb-8 max-w-xl text-lg text-gray-400">
              Únete a los +500 dealers que ya están vendiendo más vehículos con OKLA. Comienza tu
              prueba gratis hoy.
            </p>
            <Button asChild size="lg" className="gap-2 bg-[#00A870] px-8 hover:bg-[#009663]">
              <Link href="/dealers/registro">
                Comenzar prueba gratis
                <ArrowRight className="h-5 w-5" />
              </Link>
            </Button>

            <p className="mt-6 text-sm text-gray-500">
              14 días gratis • Sin tarjeta de crédito • Cancela cuando quieras
            </p>
          </div>
        </div>
      </section>
    </div>
  );
}
