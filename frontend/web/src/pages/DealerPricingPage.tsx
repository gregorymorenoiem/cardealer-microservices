import { Link } from 'react-router-dom';
import { Check, Star, Zap } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';
import { dealerManagementService } from '@/services/dealerManagementService';
import { useState, useEffect } from 'react';

// Format price in Dominican Pesos
const formatDOP = (amount: number): string => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
};

export default function DealerPricingPage() {
  const plans = dealerManagementService.getPlanInfo();
  const isEarlyBird = dealerManagementService.isEarlyBirdActive();
  const [daysRemaining, setDaysRemaining] = useState(0);

  useEffect(() => {
    setDaysRemaining(dealerManagementService.getEarlyBirdDaysRemaining());
  }, []);

  return (
    <MainLayout>
      <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white py-12">
        <div className="container mx-auto px-4">
          {/* Header */}
          <div className="text-center mb-12">
            <h1 className="text-5xl font-bold mb-4">Planes para Dealers</h1>
            <p className="text-xl text-gray-600 max-w-2xl mx-auto">
              Elige el plan perfecto para tu negocio. Sin contratos largos, cancela cuando quieras.
            </p>
          </div>

          {/* Early Bird Banner */}
          {isEarlyBird && (
            <div className="max-w-4xl mx-auto mb-12">
              <div className="bg-gradient-to-r from-yellow-400 via-orange-500 to-red-500 text-white p-8 rounded-2xl shadow-2xl">
                <div className="flex items-center justify-center gap-3 mb-4">
                  <Zap className="w-8 h-8" />
                  <h2 className="text-3xl font-bold">¡Oferta Early Bird Activa!</h2>
                </div>
                <p className="text-center text-xl mb-4">
                  <strong>3 MESES GRATIS</strong> + <strong>20% de DESCUENTO DE POR VIDA</strong>
                </p>
                <p className="text-center text-lg">
                  🏆 Además, recibes el badge <strong>"Miembro Fundador"</strong> permanente
                </p>
                <div className="text-center mt-6">
                  <span className="inline-block bg-white text-orange-600 px-6 py-3 rounded-full font-bold text-2xl shadow-lg">
                    ⏰ Quedan {daysRemaining} días
                  </span>
                </div>
              </div>
            </div>
          )}

          {/* Pricing Cards */}
          <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto mb-16">
            {plans.map((plan) => (
              <div
                key={plan.name}
                className={`bg-white rounded-2xl shadow-xl overflow-hidden transition-all hover:scale-105 ${
                  plan.recommended ? 'ring-4 ring-blue-500 relative' : ''
                }`}
              >
                {plan.recommended && (
                  <div className="absolute top-4 right-4 bg-blue-500 text-white px-4 py-1 rounded-full text-sm font-bold flex items-center gap-1">
                    <Star className="w-4 h-4" fill="currentColor" />
                    Recomendado
                  </div>
                )}

                <div className={`p-8 ${plan.recommended ? 'bg-blue-50' : 'bg-gray-50'}`}>
                  <h3 className="text-2xl font-bold mb-2">{plan.displayName}</h3>
                  <div className="mb-4">
                    {isEarlyBird ? (
                      <>
                        <div className="flex items-baseline gap-2">
                          <span className="text-3xl font-bold text-blue-600">
                            {formatDOP(plan.earlyBirdPrice)}
                          </span>
                          <span className="text-gray-500">/mes</span>
                        </div>
                        <div className="text-sm text-gray-500 line-through">
                          {formatDOP(plan.price)}/mes precio regular
                        </div>
                        <div className="text-sm text-green-600 font-semibold mt-1">
                          ¡Ahorras {formatDOP(plan.price - plan.earlyBirdPrice)}/mes de por vida!
                        </div>
                      </>
                    ) : (
                      <div className="flex items-baseline gap-2">
                        <span className="text-3xl font-bold text-blue-600">
                          {formatDOP(plan.price)}
                        </span>
                        <span className="text-gray-500">/mes</span>
                      </div>
                    )}
                  </div>
                  <div className="text-lg font-semibold mb-4">
                    {typeof plan.maxListings === 'number'
                      ? `Hasta ${plan.maxListings} vehículos`
                      : plan.maxListings}
                  </div>
                </div>

                <div className="p-8">
                  <ul className="space-y-4 mb-8">
                    {plan.features.map((feature, idx) => (
                      <li key={idx} className="flex gap-3">
                        <Check className="w-5 h-5 text-green-500 flex-shrink-0 mt-0.5" />
                        <span className="text-gray-700">{feature}</span>
                      </li>
                    ))}
                  </ul>

                  <Link
                    to={`/dealer/register?plan=${plan.name}`}
                    className={`block w-full py-4 rounded-lg font-semibold text-center transition-colors ${
                      plan.recommended
                        ? 'bg-blue-600 text-white hover:bg-blue-700'
                        : 'bg-gray-100 text-gray-900 hover:bg-gray-200'
                    }`}
                  >
                    {isEarlyBird ? '¡Aprovechar Oferta!' : 'Comenzar Ahora'}
                  </Link>
                </div>
              </div>
            ))}
          </div>

          {/* FAQ Section */}
          <div className="max-w-3xl mx-auto">
            <h2 className="text-3xl font-bold text-center mb-8">Preguntas Frecuentes</h2>

            <div className="space-y-6">
              <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-bold text-lg mb-2">¿Puedo cambiar de plan después?</h3>
                <p className="text-gray-600">
                  Sí, puedes actualizar o bajar de plan en cualquier momento. Los cambios se aplican
                  en el siguiente ciclo de facturación.
                </p>
              </div>

              <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-bold text-lg mb-2">
                  ¿Qué pasa con mis listings si cambio a un plan menor?
                </h3>
                <p className="text-gray-600">
                  Tus listings existentes permanecen activos, pero no podrás agregar más hasta estar
                  dentro del límite del nuevo plan.
                </p>
              </div>

              <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-bold text-lg mb-2">¿Cómo funciona la oferta Early Bird?</h3>
                <p className="text-gray-600">
                  Registrándote antes del 31 de enero 2026, obtienes 3 meses gratis y 20% de
                  descuento DE POR VIDA en tu suscripción. El descuento se mantiene mientras tu
                  suscripción esté activa.
                </p>
              </div>

              <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-bold text-lg mb-2">¿Qué métodos de pago aceptan?</h3>
                <p className="text-gray-600">
                  Aceptamos tarjetas de crédito/débito dominicanas (AZUL - Banco Popular) y tarjetas
                  internacionales (Stripe). Pagos 100% seguros.
                </p>
              </div>

              <div className="bg-white p-6 rounded-lg shadow-md">
                <h3 className="font-bold text-lg mb-2">¿Necesito verificación para ser dealer?</h3>
                <p className="text-gray-600">
                  Sí, para garantizar calidad en la plataforma, validamos documentos como RNC,
                  licencia comercial y cédula del propietario. El proceso toma 1-2 días hábiles.
                </p>
              </div>
            </div>
          </div>

          {/* Final CTA */}
          <div className="text-center mt-16">
            <p className="text-gray-600 mb-4">¿Tienes preguntas? Contáctanos</p>
            <div className="flex gap-4 justify-center text-sm">
              <a href="tel:8095442985" className="text-blue-600 hover:underline">
                📞 809-544-2985
              </a>
              <a href="mailto:gmoreno@okla.com.do" className="text-blue-600 hover:underline">
                ✉️ gmoreno@okla.com.do
              </a>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
