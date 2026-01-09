import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import userBehaviorService, { UserBehaviorProfile, UserAction, UserBehaviorSummary } from '../services/userBehaviorService';

export default function UserBehaviorDashboard() {
  const { userId } = useParams<{ userId: string }>();
  const [profile, setProfile] = useState<UserBehaviorProfile | null>(null);
  const [actions, setActions] = useState<UserAction[]>([]);
  const [summary, setSummary] = useState<UserBehaviorSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (userId) {
      loadUserData();
    } else {
      loadSummary();
    }
  }, [userId]);

  const loadUserData = async () => {
    try {
      setLoading(true);
      const [profileData, actionsData] = await Promise.all([
        userBehaviorService.getUserProfile(userId!),
        userBehaviorService.getUserActions(userId!, 20)
      ]);
      setProfile(profileData);
      setActions(actionsData);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Error al cargar datos del usuario');
    } finally {
      setLoading(false);
    }
  };

  const loadSummary = async () => {
    try {
      setLoading(true);
      const summaryData = await userBehaviorService.getSummary();
      setSummary(summaryData);
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Error al cargar resumen');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="flex justify-center items-center h-96">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-800">{error}</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">
            {userId ? 'Perfil de Comportamiento' : 'Resumen de Comportamiento'}
          </h1>
          <p className="text-gray-600 mt-2">
            {userId ? 'Análisis detallado de acciones y preferencias del usuario' : 'Métricas agregadas de todos los usuarios'}
          </p>
        </div>

        {/* User Profile View */}
        {profile && (
          <>
            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
              <div className="bg-white rounded-lg shadow p-6">
                <div className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-${userBehaviorService.getSegmentColor(profile.userSegment)}-100 text-${userBehaviorService.getSegmentColor(profile.userSegment)}-800 mb-3`}>
                  {userBehaviorService.getSegmentLabel(profile.userSegment)}
                </div>
                <p className="text-2xl font-bold text-gray-900">{Math.round(profile.engagementScore)}</p>
                <p className="text-sm text-gray-600">Engagement Score</p>
              </div>

              <div className="bg-white rounded-lg shadow p-6">
                <div className="text-sm text-gray-600 mb-2">Intent de Compra</div>
                <div className="flex items-center">
                  <div className="flex-1 bg-gray-200 rounded-full h-2 mr-3">
                    <div
                      className={`h-2 rounded-full ${profile.purchaseIntentScore >= 70 ? 'bg-green-500' : profile.purchaseIntentScore >= 40 ? 'bg-yellow-500' : 'bg-gray-400'}`}
                      style={{ width: `${profile.purchaseIntentScore}%` }}
                    ></div>
                  </div>
                  <span className="text-lg font-bold text-gray-900">{Math.round(profile.purchaseIntentScore)}</span>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow p-6">
                <p className="text-sm text-gray-600 mb-2">Total de Acciones</p>
                <p className="text-2xl font-bold text-gray-900">
                  {profile.totalSearches + profile.totalVehicleViews + profile.totalContactRequests}
                </p>
                <p className="text-xs text-gray-500">Búsquedas, Vistas, Contactos</p>
              </div>

              <div className="bg-white rounded-lg shadow p-6">
                <p className="text-sm text-gray-600 mb-2">Última Actividad</p>
                <p className="text-lg font-bold text-gray-900">
                  {profile.lastActivityAt 
                    ? new Date(profile.lastActivityAt).toLocaleDateString('es-DO')
                    : 'N/A'}
                </p>
              </div>
            </div>

            {/* Preferences */}
            <div className="bg-white rounded-lg shadow mb-8 p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Preferencias Inferidas</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <p className="text-sm font-medium text-gray-700 mb-2">Marcas Preferidas</p>
                  <div className="flex flex-wrap gap-2">
                    {profile.preferredMakes.length > 0 ? (
                      profile.preferredMakes.map((make, i) => (
                        <span key={i} className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                          {make}
                        </span>
                      ))
                    ) : (
                      <span className="text-gray-400 text-sm">No detectado</span>
                    )}
                  </div>
                </div>

                <div>
                  <p className="text-sm font-medium text-gray-700 mb-2">Modelos Preferidos</p>
                  <div className="flex flex-wrap gap-2">
                    {profile.preferredModels.length > 0 ? (
                      profile.preferredModels.map((model, i) => (
                        <span key={i} className="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm">
                          {model}
                        </span>
                      ))
                    ) : (
                      <span className="text-gray-400 text-sm">No detectado</span>
                    )}
                  </div>
                </div>

                <div>
                  <p className="text-sm font-medium text-gray-700 mb-2">Rango de Precio</p>
                  {profile.preferredPriceMin && profile.preferredPriceMax ? (
                    <p className="text-lg font-semibold text-gray-900">
                      ${profile.preferredPriceMin.toLocaleString()} - ${profile.preferredPriceMax.toLocaleString()}
                    </p>
                  ) : (
                    <span className="text-gray-400 text-sm">No detectado</span>
                  )}
                </div>

                <div>
                  <p className="text-sm font-medium text-gray-700 mb-2">Tipos de Carrocería</p>
                  <div className="flex flex-wrap gap-2">
                    {profile.preferredBodyTypes.length > 0 ? (
                      profile.preferredBodyTypes.map((type, i) => (
                        <span key={i} className="px-3 py-1 bg-purple-100 text-purple-800 rounded-full text-sm">
                          {type}
                        </span>
                      ))
                    ) : (
                      <span className="text-gray-400 text-sm">No detectado</span>
                    )}
                  </div>
                </div>
              </div>
            </div>

            {/* Recent Actions */}
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Historial de Acciones (Últimas 20)</h2>
              <div className="space-y-3">
                {actions.length > 0 ? (
                  actions.map((action) => (
                    <div key={action.id} className="flex items-start border-b border-gray-200 pb-3 last:border-0">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs font-medium">
                            {action.actionType}
                          </span>
                          <span className="text-xs text-gray-500">
                            {new Date(action.timestamp).toLocaleString('es-DO')}
                          </span>
                        </div>
                        <p className="text-sm text-gray-900">{action.actionDetails}</p>
                        {action.searchQuery && (
                          <p className="text-xs text-gray-600 mt-1">Búsqueda: "{action.searchQuery}"</p>
                        )}
                      </div>
                      <span className="text-xs text-gray-500 ml-4">{action.deviceType}</span>
                    </div>
                  ))
                ) : (
                  <p className="text-gray-400 text-center py-8">No hay acciones registradas</p>
                )}
              </div>
            </div>
          </>
        )}

        {/* Summary View */}
        {summary && (
          <>
            {/* Overview Stats */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
              <div className="bg-white rounded-lg shadow p-6">
                <p className="text-sm text-gray-600 mb-2">Total Usuarios</p>
                <p className="text-3xl font-bold text-gray-900">{summary.totalUsers.toLocaleString()}</p>
              </div>
              <div className="bg-white rounded-lg shadow p-6">
                <p className="text-sm text-gray-600 mb-2">Activos (7 días)</p>
                <p className="text-3xl font-bold text-green-600">{summary.activeUsers7Days.toLocaleString()}</p>
              </div>
              <div className="bg-white rounded-lg shadow p-6">
                <p className="text-sm text-gray-600 mb-2">Activos (30 días)</p>
                <p className="text-3xl font-bold text-blue-600">{summary.activeUsers30Days.toLocaleString()}</p>
              </div>
            </div>

            {/* Segment Distribution */}
            <div className="bg-white rounded-lg shadow p-6 mb-8">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Distribución de Segmentos</h2>
              <div className="space-y-3">
                {Object.entries(summary.segmentDistribution).map(([segment, count]) => (
                  <div key={segment} className="flex items-center">
                    <div className="w-40">
                      <span className={`inline-block px-3 py-1 rounded-full text-sm font-medium bg-${userBehaviorService.getSegmentColor(segment)}-100 text-${userBehaviorService.getSegmentColor(segment)}-800`}>
                        {userBehaviorService.getSegmentLabel(segment)}
                      </span>
                    </div>
                    <div className="flex-1 mx-4">
                      <div className="bg-gray-200 rounded-full h-4">
                        <div
                          className={`h-4 rounded-full bg-${userBehaviorService.getSegmentColor(segment)}-500`}
                          style={{ width: `${(count / summary.totalUsers) * 100}%` }}
                        ></div>
                      </div>
                    </div>
                    <div className="w-20 text-right">
                      <span className="text-lg font-bold text-gray-900">{count}</span>
                      <span className="text-xs text-gray-500 ml-1">
                        ({Math.round((count / summary.totalUsers) * 100)}%)
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Top Makes & Models */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-xl font-bold text-gray-900 mb-4">Top Marcas Preferidas</h2>
                <div className="space-y-2">
                  {Object.entries(summary.topMakes).slice(0, 10).map(([make, count], i) => (
                    <div key={make} className="flex items-center justify-between">
                      <span className="text-sm text-gray-700">
                        {i + 1}. {make}
                      </span>
                      <span className="text-sm font-bold text-gray-900">{count}</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="bg-white rounded-lg shadow p-6">
                <h2 className="text-xl font-bold text-gray-900 mb-4">Top Modelos Preferidos</h2>
                <div className="space-y-2">
                  {Object.entries(summary.topModels).slice(0, 10).map(([model, count], i) => (
                    <div key={model} className="flex items-center justify-between">
                      <span className="text-sm text-gray-700">
                        {i + 1}. {model}
                      </span>
                      <span className="text-sm font-bold text-gray-900">{count}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </>
        )}
      </div>
    </MainLayout>
  );
}
