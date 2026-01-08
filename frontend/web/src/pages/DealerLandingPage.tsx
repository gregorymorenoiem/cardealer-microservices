import { Link } from 'react-router-dom';
import { Building2, TrendingUp, Users, BarChart3, Zap, Shield } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';

export default function DealerLandingPage() {
  return (
    <MainLayout>
      <div className="min-h-screen bg-gradient-to-b from-blue-50 to-white">
        {/* Hero Section */}
        <section className="bg-gradient-to-r from-blue-600 to-blue-800 text-white py-20">
          <div className="container mx-auto px-4">
            <div className="max-w-4xl mx-auto text-center">
              <Building2 className="w-16 h-16 mx-auto mb-6" />
              <h1 className="text-5xl font-bold mb-6">Potencia tu Negocio de Vehículos</h1>
              <p className="text-xl mb-8 text-blue-100">
                Únete a los dealers líderes en República Dominicana. Gestiona tu inventario, alcanza
                miles de compradores y haz crecer tus ventas con OKLA.
              </p>
              <div className="flex gap-4 justify-center">
                <Link
                  to="/dealer/pricing"
                  className="bg-white text-blue-600 px-8 py-4 rounded-lg font-semibold hover:bg-blue-50 transition-colors text-lg shadow-lg"
                >
                  Ver Planes y Precios
                </Link>
                <Link
                  to="/dealer/register"
                  className="bg-blue-500 text-white px-8 py-4 rounded-lg font-semibold hover:bg-blue-400 transition-colors text-lg border-2 border-white"
                >
                  Registrar mi Dealer
                </Link>
              </div>

              {/* Early Bird Badge */}
              <div className="mt-8 inline-block bg-yellow-400 text-blue-900 px-6 py-3 rounded-full font-bold text-lg shadow-lg animate-pulse">
                🎁 ¡Oferta Early Bird! 3 MESES GRATIS + 20% Descuento de por Vida
              </div>
            </div>
          </div>
        </section>

        {/* Benefits Section */}
        <section className="py-20">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-bold text-center mb-4">¿Por qué elegir OKLA?</h2>
            <p className="text-xl text-gray-600 text-center mb-12 max-w-2xl mx-auto">
              La plataforma diseñada específicamente para dealers de vehículos en RD
            </p>

            <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
              {/* Benefit 1 */}
              <div className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
                <div className="bg-blue-100 w-16 h-16 rounded-full flex items-center justify-center mb-6">
                  <TrendingUp className="w-8 h-8 text-blue-600" />
                </div>
                <h3 className="text-2xl font-bold mb-4">Aumenta tus Ventas</h3>
                <p className="text-gray-600 leading-relaxed">
                  Alcanza miles de compradores activos buscando vehículos en RD. Mayor visibilidad =
                  más ventas.
                </p>
              </div>

              {/* Benefit 2 */}
              <div className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
                <div className="bg-green-100 w-16 h-16 rounded-full flex items-center justify-center mb-6">
                  <BarChart3 className="w-8 h-8 text-green-600" />
                </div>
                <h3 className="text-2xl font-bold mb-4">Panel de Control Profesional</h3>
                <p className="text-gray-600 leading-relaxed">
                  Gestiona todo tu inventario desde un solo lugar. Estadísticas en tiempo real de
                  vistas y contactos.
                </p>
              </div>

              {/* Benefit 3 */}
              <div className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
                <div className="bg-purple-100 w-16 h-16 rounded-full flex items-center justify-center mb-6">
                  <Zap className="w-8 h-8 text-purple-600" />
                </div>
                <h3 className="text-2xl font-bold mb-4">Importación Masiva</h3>
                <p className="text-gray-600 leading-relaxed">
                  Carga tu inventario completo en minutos con CSV/Excel. Edita múltiples vehículos
                  simultáneamente.
                </p>
              </div>
            </div>
          </div>
        </section>

        {/* Stats Section */}
        <section className="bg-blue-600 text-white py-16">
          <div className="container mx-auto px-4">
            <div className="grid md:grid-cols-4 gap-8 text-center">
              <div>
                <div className="text-5xl font-bold mb-2">10K+</div>
                <div className="text-blue-200">Visitantes Mensuales</div>
              </div>
              <div>
                <div className="text-5xl font-bold mb-2">500+</div>
                <div className="text-blue-200">Vehículos Publicados</div>
              </div>
              <div>
                <div className="text-5xl font-bold mb-2">50+</div>
                <div className="text-blue-200">Dealers Activos</div>
              </div>
              <div>
                <div className="text-5xl font-bold mb-2">95%</div>
                <div className="text-blue-200">Satisfacción</div>
              </div>
            </div>
          </div>
        </section>

        {/* Features Section */}
        <section className="py-20">
          <div className="container mx-auto px-4">
            <h2 className="text-4xl font-bold text-center mb-12">Funcionalidades Premium</h2>

            <div className="grid md:grid-cols-2 gap-6 max-w-5xl mx-auto">
              {[
                {
                  icon: Shield,
                  title: 'Badge Verificado',
                  desc: 'Gana confianza con el badge "Dealer Verificado"',
                },
                {
                  icon: Users,
                  title: 'Múltiples Sucursales',
                  desc: 'Gestiona todas tus locaciones desde una cuenta',
                },
                {
                  icon: BarChart3,
                  title: 'Estadísticas Detalladas',
                  desc: 'Analiza vistas, clics y conversiones',
                },
                {
                  icon: Zap,
                  title: 'Prioridad en Búsquedas',
                  desc: 'Aparece primero en resultados de búsqueda',
                },
              ].map((feature, idx) => (
                <div
                  key={idx}
                  className="flex gap-4 p-6 bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow"
                >
                  <div className="bg-blue-100 w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0">
                    <feature.icon className="w-6 h-6 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="font-bold text-lg mb-2">{feature.title}</h3>
                    <p className="text-gray-600">{feature.desc}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section className="bg-gradient-to-r from-blue-600 to-blue-800 text-white py-16">
          <div className="container mx-auto px-4 text-center">
            <h2 className="text-4xl font-bold mb-6">¿Listo para Empezar?</h2>
            <p className="text-xl mb-8 text-blue-100 max-w-2xl mx-auto">
              Únete hoy y comienza a vender más vehículos. Sin contratos largos, cancela cuando
              quieras.
            </p>
            <div className="flex gap-4 justify-center">
              <Link
                to="/dealer/pricing"
                className="bg-white text-blue-600 px-8 py-4 rounded-lg font-semibold hover:bg-blue-50 transition-colors text-lg shadow-lg"
              >
                Ver Planes
              </Link>
              <Link
                to="/dealer/register"
                className="bg-blue-500 text-white px-8 py-4 rounded-lg font-semibold hover:bg-blue-400 transition-colors text-lg border-2 border-white"
              >
                Registrarme Ahora
              </Link>
            </div>
          </div>
        </section>
      </div>
    </MainLayout>
  );
}
