/**
 * Dealer Vehicle Boost Page
 *
 * Promote/boost a vehicle in dealer inventory
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  ArrowLeft,
  Zap,
  TrendingUp,
  Eye,
  Star,
  Check,
  Clock,
  BarChart3,
  Car,
  Sparkles,
  Crown,
} from 'lucide-react';
import Link from 'next/link';

const vehicle = {
  id: 'INV-001',
  title: 'Toyota Camry 2023',
  price: 2150000,
  currentViews: 245,
  image: null,
};

const boostPlans = [
  {
    id: 'basic',
    name: 'Básico',
    price: 500,
    duration: '3 días',
    multiplier: '2x',
    features: ['Aparece más arriba en búsquedas', 'Badge "Destacado"', 'Prioridad en categoría'],
    color: 'blue',
  },
  {
    id: 'pro',
    name: 'Profesional',
    price: 1200,
    duration: '7 días',
    multiplier: '5x',
    features: [
      'Todo lo del plan Básico',
      'Aparece en homepage',
      'Prioridad en resultados',
      'Estadísticas detalladas',
    ],
    recommended: true,
    color: 'emerald',
  },
  {
    id: 'premium',
    name: 'Premium',
    price: 2500,
    duration: '14 días',
    multiplier: '10x',
    features: [
      'Todo lo del plan Pro',
      'Posición #1 garantizada',
      'Banner destacado',
      'Notificación a compradores',
      'Soporte prioritario',
    ],
    color: 'yellow',
  },
];

const results = [
  { metric: 'Vistas promedio', before: '250', after: '1,200', increase: '+380%' },
  { metric: 'Contactos', before: '8', after: '35', increase: '+337%' },
  { metric: 'Tiempo de venta', before: '45 días', after: '12 días', increase: '-73%' },
];

export default function DealerVehicleBoostPage() {
  const [selectedPlan, setSelectedPlan] = useState<string>('pro');
  const [isProcessing, setIsProcessing] = useState(false);

  const handleBoost = () => {
    setIsProcessing(true);
    setTimeout(() => setIsProcessing(false), 1500);
  };

  const selected = boostPlans.find(p => p.id === selectedPlan);

  return (
    <div className="min-h-screen bg-slate-900 p-6">
      {/* Header */}
      <div className="mb-6 flex items-center gap-4">
        <Link href={`/dealer/inventario/${vehicle.id}`}>
          <Button
            variant="ghost"
            size="icon"
            className="text-slate-400 hover:bg-slate-800 hover:text-white"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-white">Promocionar Vehículo</h1>
          <p className="text-slate-400">Aumenta la visibilidad de tu publicación</p>
        </div>
      </div>

      {/* Vehicle Card */}
      <Card className="mb-6 border-slate-700 bg-slate-800">
        <CardContent className="p-4">
          <div className="flex items-center gap-4">
            <div className="flex h-18 w-24 items-center justify-center rounded-lg bg-slate-700">
              <Car className="h-8 w-8 text-slate-500" />
            </div>
            <div className="flex-1">
              <h3 className="font-semibold text-white">{vehicle.title}</h3>
              <p className="font-bold text-emerald-400">RD$ {vehicle.price.toLocaleString()}</p>
              <div className="mt-1 flex items-center gap-4 text-sm text-slate-400">
                <span className="flex items-center gap-1">
                  <Eye className="h-4 w-4" />
                  {vehicle.currentViews} vistas
                </span>
                <span className="flex items-center gap-1">
                  <Clock className="h-4 w-4" />
                  16 días publicado
                </span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Results Preview */}
      <Card className="mb-8 border-emerald-700 bg-gradient-to-r from-emerald-900/40 to-teal-900/40">
        <CardContent className="p-6">
          <div className="mb-4 flex items-center gap-3">
            <Sparkles className="h-6 w-6 text-emerald-400" />
            <h3 className="text-lg font-semibold text-white">Resultados promedio con Boost</h3>
          </div>
          <div className="grid grid-cols-3 gap-6">
            {results.map((r, i) => (
              <div key={i} className="text-center">
                <p className="mb-2 text-sm text-slate-400">{r.metric}</p>
                <div className="flex items-center justify-center gap-2">
                  <span className="text-slate-500 line-through">{r.before}</span>
                  <TrendingUp className="h-4 w-4 text-emerald-400" />
                  <span className="font-bold text-white">{r.after}</span>
                </div>
                <Badge className="mt-2 bg-emerald-600">{r.increase}</Badge>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Plans */}
        <div className="lg:col-span-2">
          <h2 className="mb-4 text-lg font-semibold text-white">Selecciona un plan</h2>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {boostPlans.map(plan => (
              <button
                key={plan.id}
                onClick={() => setSelectedPlan(plan.id)}
                className={`relative rounded-xl p-6 text-left transition-all ${
                  selectedPlan === plan.id
                    ? 'bg-slate-700 ring-2 ring-emerald-500'
                    : 'border border-slate-700 bg-slate-800 hover:border-slate-600'
                }`}
              >
                {plan.recommended && (
                  <Badge className="absolute -top-2 left-1/2 -translate-x-1/2 bg-emerald-600">
                    Recomendado
                  </Badge>
                )}

                <div
                  className={`mb-4 flex h-12 w-12 items-center justify-center rounded-lg ${
                    plan.color === 'blue'
                      ? 'bg-blue-600/20'
                      : plan.color === 'emerald'
                        ? 'bg-emerald-600/20'
                        : 'bg-yellow-600/20'
                  }`}
                >
                  {plan.color === 'yellow' ? (
                    <Crown className={`h-6 w-6 text-yellow-400`} />
                  ) : (
                    <Zap
                      className={`h-6 w-6 ${
                        plan.color === 'blue' ? 'text-blue-400' : 'text-emerald-400'
                      }`}
                    />
                  )}
                </div>

                <h3 className="mb-1 font-semibold text-white">{plan.name}</h3>
                <p className="mb-1 text-2xl font-bold text-white">
                  RD$ {plan.price.toLocaleString()}
                </p>
                <p className="mb-4 text-sm text-slate-400">{plan.duration}</p>

                <div className="mb-4 flex items-center gap-2">
                  <TrendingUp className="h-4 w-4 text-emerald-400" />
                  <span className="font-medium text-emerald-400">{plan.multiplier} más vistas</span>
                </div>

                <ul className="space-y-2">
                  {plan.features.map((feature, i) => (
                    <li key={i} className="flex items-start gap-2 text-sm text-slate-300">
                      <Check className="mt-0.5 h-4 w-4 shrink-0 text-emerald-400" />
                      {feature}
                    </li>
                  ))}
                </ul>
              </button>
            ))}
          </div>
        </div>

        {/* Checkout */}
        <div>
          <Card className="sticky top-6 border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Resumen</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="rounded-lg bg-slate-900 p-4">
                <p className="text-sm text-slate-400">Plan seleccionado</p>
                <p className="text-lg font-semibold text-white">{selected?.name}</p>
                <p className="text-sm text-slate-400">{selected?.duration}</p>
              </div>

              <div className="rounded-lg bg-slate-900 p-4">
                <p className="text-sm text-slate-400">Vehículo</p>
                <p className="text-white">{vehicle.title}</p>
              </div>

              <div className="border-t border-slate-700 pt-4">
                <div className="mb-2 flex justify-between">
                  <span className="text-slate-400">Subtotal</span>
                  <span className="text-white">RD$ {selected?.price.toLocaleString()}</span>
                </div>
                <div className="mb-2 flex justify-between">
                  <span className="text-slate-400">ITBIS (18%)</span>
                  <span className="text-white">
                    RD$ {Math.round((selected?.price || 0) * 0.18).toLocaleString()}
                  </span>
                </div>
                <div className="mt-2 flex justify-between border-t border-slate-700 pt-2 text-lg font-semibold">
                  <span className="text-white">Total</span>
                  <span className="text-emerald-400">
                    RD$ {Math.round((selected?.price || 0) * 1.18).toLocaleString()}
                  </span>
                </div>
              </div>

              <Button
                className="w-full bg-emerald-600 hover:bg-emerald-700"
                onClick={handleBoost}
                disabled={isProcessing}
              >
                {isProcessing ? (
                  <div className="h-4 w-4 animate-spin rounded-full border-b-2 border-white" />
                ) : (
                  <>
                    <Zap className="mr-2 h-4 w-4" />
                    Activar Boost
                  </>
                )}
              </Button>

              <p className="text-center text-xs text-slate-400">
                El boost se activa inmediatamente después del pago
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* FAQ */}
      <Card className="border-slate-700 bg-slate-800">
        <CardHeader>
          <CardTitle className="text-white">Preguntas Frecuentes</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h4 className="mb-2 font-medium text-white">¿Cuándo se activa el boost?</h4>
            <p className="text-sm text-slate-400">
              El boost se activa inmediatamente después de confirmar el pago. Verás tu vehículo
              destacado en pocos minutos.
            </p>
          </div>
          <div>
            <h4 className="mb-2 font-medium text-white">¿Puedo cancelar el boost?</h4>
            <p className="text-sm text-slate-400">
              Los boosts no son reembolsables una vez activados. Te recomendamos seleccionar el plan
              que mejor se adapte a tus necesidades.
            </p>
          </div>
          <div>
            <h4 className="mb-2 font-medium text-white">¿Qué pasa cuando termina el boost?</h4>
            <p className="text-sm text-slate-400">
              Tu publicación vuelve a su posición normal en los resultados de búsqueda. Puedes
              renovar el boost en cualquier momento.
            </p>
          </div>
          <div>
            <h4 className="mb-2 font-medium text-white">¿El boost garantiza la venta?</h4>
            <p className="text-sm text-slate-400">
              El boost aumenta significativamente la visibilidad de tu publicación, pero la venta
              depende de factores como precio, condición y demanda del mercado.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
