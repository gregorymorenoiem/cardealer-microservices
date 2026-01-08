import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Building2, TrendingUp, Eye, MessageSquare, Car, AlertCircle, Loader2 } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';
import { dealerManagementService, Dealer } from '@/services/dealerManagementService';

export default function DealerDashboard() {
  const navigate = useNavigate();
  const [dealer, setDealer] = useState<Dealer | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDealerData();
  }, []);

  const loadDealerData = async () => {
    try {
      const userId = localStorage.getItem('userId');
      if (!userId) {
        navigate('/login?redirect=/dealer/dashboard');
        return;
      }

      const dealerData = await dealerManagementService.getDealerByUserId(userId);
      setDealer(dealerData);
    } catch (err: any) {
      if (err.response?.status === 404) {
        // No dealer account, redirect to registration
        navigate('/dealer/register');
      } else {
        setError(err.message || 'Error al cargar datos del dealer');
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center min-h-screen">
          <Loader2 className="w-12 h-12 animate-spin text-blue-600" />
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-12">
          <div className="bg-red-50 border border-red-200 text-red-700 px-6 py-4 rounded-lg">
            {error}
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!dealer) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-12">
          <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 px-6 py-4 rounded-lg">
            No tienes una cuenta de dealer.{' '}
            <button
              onClick={() => navigate('/dealer/register')}
              className="underline font-semibold"
            >
              Regístrate aquí
            </button>
          </div>
        </div>
      </MainLayout>
    );
  }

  const getStatusBadge = (status: string) => {
    const badges = {
      Pending: { bg: 'bg-yellow-100', text: 'text-yellow-800', label: 'Pendiente' },
      UnderReview: { bg: 'bg-blue-100', text: 'text-blue-800', label: 'En Revisión' },
      Active: { bg: 'bg-green-100', text: 'text-green-800', label: 'Activo' },
      Suspended: { bg: 'bg-red-100', text: 'text-red-800', label: 'Suspendido' },
      Rejected: { bg: 'bg-red-100', text: 'text-red-800', label: 'Rechazado' },
    };
    const badge = badges[status as keyof typeof badges] || badges.Pending;
    return (
      <span className={`${badge.bg} ${badge.text} px-3 py-1 rounded-full text-sm font-semibold`}>
        {badge.label}
      </span>
    );
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="container mx-auto px-4">
          {/* Header */}
          <div className="bg-white rounded-lg shadow-md p-6 mb-8">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-4">
                <div className="bg-blue-100 w-16 h-16 rounded-full flex items-center justify-center">
                  <Building2 className="w-8 h-8 text-blue-600" />
                </div>
                <div>
                  <h1 className="text-3xl font-bold">{dealer.businessName}</h1>
                  <p className="text-gray-600">RNC: {dealer.rnc}</p>
                </div>
              </div>
              <div className="text-right">
                {getStatusBadge(dealer.status)}
                <div className="mt-2 text-sm text-gray-600">
                  Plan: <span className="font-semibold">{dealer.currentPlan}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Verification Alert */}
          {dealer.verificationStatus !== 'Verified' && (
            <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 px-6 py-4 rounded-lg mb-8 flex gap-3">
              <AlertCircle className="w-6 h-6 flex-shrink-0" />
              <div>
                <p className="font-semibold mb-1">Verificación Pendiente</p>
                <p className="text-sm">
                  Tu cuenta está en proceso de verificación. Necesitas subir los documentos
                  requeridos.
                </p>
                <button className="mt-2 text-sm underline font-semibold">Subir Documentos →</button>
              </div>
            </div>
          )}

          {/* Stats Grid */}
          <div className="grid md:grid-cols-4 gap-6 mb-8">
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between mb-4">
                <Car className="w-8 h-8 text-blue-600" />
                <TrendingUp className="w-5 h-5 text-green-500" />
              </div>
              <div className="text-3xl font-bold mb-1">{dealer.currentActiveListings}</div>
              <div className="text-gray-600 text-sm">Vehículos Activos</div>
              <div className="text-xs text-gray-500 mt-2">
                {dealer.remainingListings} restantes de {dealer.maxActiveListings}
              </div>
            </div>

            <div className="bg-white rounded-lg shadow-md p-6">
              <Eye className="w-8 h-8 text-purple-600 mb-4" />
              <div className="text-3xl font-bold mb-1">2,450</div>
              <div className="text-gray-600 text-sm">Vistas Este Mes</div>
              <div className="text-xs text-green-600 mt-2">+15% vs mes anterior</div>
            </div>

            <div className="bg-white rounded-lg shadow-md p-6">
              <MessageSquare className="w-8 h-8 text-orange-600 mb-4" />
              <div className="text-3xl font-bold mb-1">48</div>
              <div className="text-gray-600 text-sm">Consultas</div>
              <div className="text-xs text-gray-500 mt-2">12 sin responder</div>
            </div>

            <div className="bg-white rounded-lg shadow-md p-6">
              <TrendingUp className="w-8 h-8 text-green-600 mb-4" />
              <div className="text-3xl font-bold mb-1">$125K</div>
              <div className="text-gray-600 text-sm">Valor Inventario</div>
              <div className="text-xs text-gray-500 mt-2">Promedio: $8,333/vehículo</div>
            </div>
          </div>

          {/* Main Content Grid */}
          <div className="grid md:grid-cols-3 gap-8">
            {/* Recent Activity */}
            <div className="md:col-span-2 bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-bold mb-4">Actividad Reciente</h2>
              <div className="space-y-4">
                <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-lg">
                  <Eye className="w-5 h-5 text-gray-400" />
                  <div className="flex-1">
                    <p className="font-semibold">Nueva vista en "Toyota Corolla 2022"</p>
                    <p className="text-sm text-gray-600">hace 5 minutos</p>
                  </div>
                </div>
                <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-lg">
                  <MessageSquare className="w-5 h-5 text-blue-500" />
                  <div className="flex-1">
                    <p className="font-semibold">Nueva consulta sobre "Honda Civic 2021"</p>
                    <p className="text-sm text-gray-600">hace 1 hora</p>
                  </div>
                </div>
                <div className="flex items-center gap-4 p-4 bg-gray-50 rounded-lg">
                  <TrendingUp className="w-5 h-5 text-green-500" />
                  <div className="flex-1">
                    <p className="font-semibold">Tu listing alcanzó 100 vistas</p>
                    <p className="text-sm text-gray-600">hace 3 horas</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Quick Actions */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-bold mb-4">Acciones Rápidas</h2>
              <div className="space-y-3">
                <button
                  onClick={() => navigate('/vehicles/create')}
                  className="w-full px-4 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors"
                >
                  + Publicar Vehículo
                </button>
                <button
                  onClick={() => navigate('/dealer/profile/edit')}
                  className="w-full px-4 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors"
                >
                  ✏️ Editar Perfil Público
                </button>
                <button
                  onClick={() => navigate('/dealer/analytics')}
                  className="w-full px-4 py-3 bg-gradient-to-r from-purple-600 to-indigo-600 text-white rounded-lg font-semibold hover:from-purple-700 hover:to-indigo-700 transition-colors shadow-md"
                >
                  📊 Ver Analytics & Métricas
                </button>
                <button
                  onClick={() => navigate('/dealer/inventory')}
                  className="w-full px-4 py-3 bg-gray-100 text-gray-900 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                >
                  Ver Inventario
                </button>
                <button
                  onClick={() => navigate('/dealer/inventory/import')}
                  className="w-full px-4 py-3 bg-gray-100 text-gray-900 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                >
                  Importar CSV
                </button>
                <button
                  onClick={() => navigate('/inquiries/received')}
                  className="w-full px-4 py-3 bg-gray-100 text-gray-900 rounded-lg font-semibold hover:bg-gray-200 transition-colors"
                >
                  Ver Consultas
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
