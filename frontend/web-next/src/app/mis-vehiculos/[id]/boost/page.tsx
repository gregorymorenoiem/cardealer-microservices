/**
 * Boost Vehicle Page
 *
 * Promote/boost a vehicle listing for more visibility
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  ArrowLeft,
  Sparkles,
  Check,
  Eye,
  TrendingUp,
  Clock,
  Zap,
  Star,
  Crown,
  ChevronRight,
  Shield,
  CreditCard,
} from 'lucide-react';
import Link from 'next/link';

const boostPlans = [
  {
    id: 'basic',
    name: 'Boost Básico',
    duration: '3 días',
    price: 500,
    savings: null,
    features: ['3x más vistas', 'Aparece primero en búsquedas', 'Badge "Destacado"'],
    icon: Zap,
    color: 'blue',
    popular: false,
  },
  {
    id: 'pro',
    name: 'Boost Pro',
    duration: '7 días',
    price: 900,
    savings: 260,
    features: [
      '5x más vistas',
      'Aparece primero en búsquedas',
      'Badge "Destacado"',
      'Rotación en homepage',
      'Estadísticas avanzadas',
    ],
    icon: Star,
    color: 'amber',
    popular: true,
  },
  {
    id: 'premium',
    name: 'Boost Premium',
    duration: '14 días',
    price: 1500,
    savings: 500,
    features: [
      '10x más vistas',
      'Aparece primero en búsquedas',
      'Badge "Premium"',
      'Rotación en homepage',
      'Estadísticas avanzadas',
      'Soporte prioritario',
      'Compartir en redes sociales',
    ],
    icon: Crown,
    color: 'purple',
    popular: false,
  },
];

const testimonials = [
  {
    name: 'Carlos Martínez',
    text: 'Vendí mi Toyota en 3 días después de usar el boost. ¡Increíble!',
    vehicle: 'Toyota Corolla 2021',
  },
  {
    name: 'María García',
    text: 'Las vistas aumentaron 8 veces. Recibí muchas más llamadas.',
    vehicle: 'Honda CR-V 2020',
  },
  {
    name: 'Juan Pérez',
    text: 'Mejor inversión que pude hacer. Mi carro se vendió rápido.',
    vehicle: 'Hyundai Tucson 2022',
  },
];

export default function BoostVehiclePage() {
  const [selectedPlan, setSelectedPlan] = useState('pro');
  const [isProcessing, setIsProcessing] = useState(false);

  const handlePurchase = () => {
    setIsProcessing(true);
    setTimeout(() => {
      window.location.href = '/mis-vehiculos';
    }, 2000);
  };

  const getColorClasses = (color: string, isSelected: boolean) => {
    if (!isSelected) return 'border-gray-200 hover:border-gray-300';
    switch (color) {
      case 'blue':
        return 'border-blue-500 bg-blue-50';
      case 'amber':
        return 'border-amber-500 bg-amber-50';
      case 'purple':
        return 'border-purple-500 bg-purple-50';
      default:
        return 'border-emerald-500 bg-emerald-50';
    }
  };

  const getIconColor = (color: string) => {
    switch (color) {
      case 'blue':
        return 'text-blue-500';
      case 'amber':
        return 'text-amber-500';
      case 'purple':
        return 'text-purple-500';
      default:
        return 'text-emerald-500';
    }
  };

  const selectedPlanData = boostPlans.find(p => p.id === selectedPlan);

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-5xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 flex items-center gap-4">
          <Link href="/mis-vehiculos">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Destacar Vehículo</h1>
            <p className="text-gray-600">Toyota Corolla XLE 2022</p>
          </div>
        </div>

        {/* Hero Stats */}
        <Card className="mb-8 border-0 bg-gradient-to-r from-emerald-500 to-teal-500 text-white">
          <CardContent className="py-8">
            <div className="mb-6 text-center">
              <Sparkles className="mx-auto mb-4 h-12 w-12" />
              <h2 className="mb-2 text-2xl font-bold">Vende hasta 10x más rápido</h2>
              <p className="text-emerald-100">
                Los vehículos destacados reciben en promedio 10 veces más vistas
              </p>
            </div>
            <div className="mx-auto grid max-w-lg grid-cols-3 gap-4">
              <div className="text-center">
                <div className="flex items-center justify-center gap-1">
                  <Eye className="h-5 w-5" />
                  <span className="text-2xl font-bold">10x</span>
                </div>
                <p className="text-sm text-emerald-100">Más vistas</p>
              </div>
              <div className="text-center">
                <div className="flex items-center justify-center gap-1">
                  <TrendingUp className="h-5 w-5" />
                  <span className="text-2xl font-bold">85%</span>
                </div>
                <p className="text-sm text-emerald-100">Venden más rápido</p>
              </div>
              <div className="text-center">
                <div className="flex items-center justify-center gap-1">
                  <Clock className="h-5 w-5" />
                  <span className="text-2xl font-bold">7 días</span>
                </div>
                <p className="text-sm text-emerald-100">Tiempo promedio</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Plans */}
          <div className="space-y-4 lg:col-span-2">
            <h3 className="text-lg font-semibold text-gray-900">Selecciona tu plan</h3>

            {boostPlans.map(plan => {
              const Icon = plan.icon;
              const isSelected = selectedPlan === plan.id;

              return (
                <Card
                  key={plan.id}
                  className={`cursor-pointer transition-all ${getColorClasses(plan.color, isSelected)}`}
                  onClick={() => setSelectedPlan(plan.id)}
                >
                  <CardContent className="p-6">
                    <div className="flex items-start justify-between">
                      <div className="flex items-start gap-4">
                        <div
                          className={`rounded-xl p-3 ${
                            plan.color === 'blue'
                              ? 'bg-blue-100'
                              : plan.color === 'amber'
                                ? 'bg-amber-100'
                                : 'bg-purple-100'
                          }`}
                        >
                          <Icon className={`h-6 w-6 ${getIconColor(plan.color)}`} />
                        </div>
                        <div>
                          <div className="flex items-center gap-2">
                            <h4 className="font-semibold text-gray-900">{plan.name}</h4>
                            {plan.popular && <Badge className="bg-amber-500">Más Popular</Badge>}
                          </div>
                          <p className="mb-3 text-sm text-gray-500">{plan.duration}</p>
                          <ul className="space-y-1">
                            {plan.features.map((feature, i) => (
                              <li key={i} className="flex items-center gap-2 text-sm text-gray-600">
                                <Check className={`h-4 w-4 ${getIconColor(plan.color)}`} />
                                {feature}
                              </li>
                            ))}
                          </ul>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-2xl font-bold text-gray-900">
                          RD$ {plan.price.toLocaleString()}
                        </p>
                        {plan.savings && (
                          <p className="text-sm text-emerald-600">Ahorras RD$ {plan.savings}</p>
                        )}
                        <div
                          className={`mt-2 ml-auto h-5 w-5 rounded-full border-2 ${
                            isSelected
                              ? plan.color === 'blue'
                                ? 'border-blue-500 bg-blue-500'
                                : plan.color === 'amber'
                                  ? 'border-amber-500 bg-amber-500'
                                  : 'border-purple-500 bg-purple-500'
                              : 'border-gray-300'
                          } flex items-center justify-center`}
                        >
                          {isSelected && <Check className="h-3 w-3 text-white" />}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>

          {/* Checkout Sidebar */}
          <div className="space-y-6">
            <Card className="sticky top-6">
              <CardHeader>
                <CardTitle>Resumen de Compra</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex justify-between border-b py-2">
                  <span className="text-gray-600">Plan</span>
                  <span className="font-medium">{selectedPlanData?.name}</span>
                </div>
                <div className="flex justify-between border-b py-2">
                  <span className="text-gray-600">Duración</span>
                  <span className="font-medium">{selectedPlanData?.duration}</span>
                </div>
                <div className="flex justify-between border-b py-2">
                  <span className="text-gray-600">Vehículo</span>
                  <span className="text-right text-sm font-medium">Toyota Corolla 2022</span>
                </div>
                <div className="flex justify-between py-3 text-lg">
                  <span className="font-semibold">Total</span>
                  <span className="font-bold text-emerald-600">
                    RD$ {selectedPlanData?.price.toLocaleString()}
                  </span>
                </div>

                <Button
                  className="w-full bg-emerald-600 hover:bg-emerald-700"
                  size="lg"
                  onClick={handlePurchase}
                  disabled={isProcessing}
                >
                  {isProcessing ? (
                    <>
                      <div className="mr-2 h-4 w-4 animate-spin rounded-full border-b-2 border-white" />
                      Procesando...
                    </>
                  ) : (
                    <>
                      <CreditCard className="mr-2 h-4 w-4" />
                      Pagar Ahora
                    </>
                  )}
                </Button>

                <div className="flex items-center justify-center gap-2 text-xs text-gray-500">
                  <Shield className="h-4 w-4" />
                  Pago seguro con SSL
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Testimonials */}
        <div className="mt-12">
          <h3 className="mb-6 text-center text-lg font-semibold text-gray-900">
            Lo que dicen nuestros vendedores
          </h3>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {testimonials.map((testimonial, i) => (
              <Card key={i}>
                <CardContent className="p-6">
                  <div className="mb-3 flex gap-1">
                    {[...Array(5)].map((_, j) => (
                      <Star key={j} className="h-4 w-4 fill-amber-400 text-amber-400" />
                    ))}
                  </div>
                  <p className="mb-4 text-sm text-gray-600">"{testimonial.text}"</p>
                  <div>
                    <p className="text-sm font-medium">{testimonial.name}</p>
                    <p className="text-xs text-gray-500">{testimonial.vehicle}</p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>

        {/* FAQ */}
        <Card className="mt-12">
          <CardHeader>
            <CardTitle>Preguntas Frecuentes</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {[
              {
                q: '¿Cuándo empieza a funcionar el boost?',
                a: 'El boost se activa inmediatamente después del pago y dura por el período seleccionado.',
              },
              {
                q: '¿Puedo cancelar el boost?',
                a: 'El boost no es reembolsable una vez activado, pero puedes contactar soporte si tienes problemas.',
              },
              {
                q: '¿Qué pasa si vendo el vehículo antes?',
                a: 'Si vendes tu vehículo, puedes contactarnos para transferir el boost restante a otra publicación.',
              },
            ].map((faq, i) => (
              <div key={i} className="rounded-lg bg-gray-50 p-4">
                <p className="mb-1 font-medium text-gray-900">{faq.q}</p>
                <p className="text-sm text-gray-600">{faq.a}</p>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
