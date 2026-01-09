import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import featureStoreService, { UserFeature, VehicleFeature, FeatureDefinition } from '../services/featureStoreService';

export default function FeatureStoreDashboard() {
  const { entityId, entityType } = useParams<{ entityId?: string; entityType?: 'user' | 'vehicle' }>();
  const [userFeatures, setUserFeatures] = useState<UserFeature[]>([]);
  const [vehicleFeatures, setVehicleFeatures] = useState<VehicleFeature[]>([]);
  const [definitions, setDefinitions] = useState<FeatureDefinition[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  useEffect(() => {
    loadData();
  }, [entityId, entityType]);

  const loadData = async () => {
    try {
      setLoading(true);
      
      if (entityId && entityType === 'user') {
        const features = await featureStoreService.getUserFeatures(entityId);
        setUserFeatures(features);
      } else if (entityId && entityType === 'vehicle') {
        const features = await featureStoreService.getVehicleFeatures(entityId);
        setVehicleFeatures(features);
      }

      const defs = await featureStoreService.getFeatureDefinitions();
      setDefinitions(defs);
      
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Error al cargar features');
    } finally {
      setLoading(false);
    }
  };

  const filteredDefinitions = selectedCategory === 'all' 
    ? definitions 
    : definitions.filter(d => d.category === selectedCategory);

  const categories = ['all', ...Array.from(new Set(definitions.map(d => d.category)))];

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

  const displayFeatures = entityType === 'user' ? userFeatures : vehicleFeatures;

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Feature Store</h1>
          <p className="text-gray-600 mt-2">
            {entityId 
              ? `Features para ${entityType === 'user' ? 'Usuario' : 'Vehículo'}: ${entityId}`
              : 'Definiciones de Features para Machine Learning'}
          </p>
        </div>

        {/* Entity Features */}
        {entityId && displayFeatures.length > 0 && (
          <div className="bg-white rounded-lg shadow mb-8 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-xl font-bold text-gray-900">Features Activos ({displayFeatures.length})</h2>
            </div>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Feature</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Valor</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tipo</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Version</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Computado</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Expira</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {displayFeatures.map((feature) => (
                    <tr key={feature.id}>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {feature.featureName}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-900">
                        {feature.featureValue}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-2 py-1 text-xs font-medium rounded-full bg-${featureStoreService.getFeatureTypeColor(feature.featureType)}-100 text-${featureStoreService.getFeatureTypeColor(feature.featureType)}-800`}>
                          {feature.featureType}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        v{feature.version}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {new Date(feature.computedAt).toLocaleString('es-DO')}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {feature.expiresAt 
                          ? new Date(feature.expiresAt).toLocaleString('es-DO')
                          : 'Never'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Feature Definitions */}
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-bold text-gray-900">Definiciones de Features ({filteredDefinitions.length})</h2>
              <div className="flex gap-2">
                {categories.map((cat) => (
                  <button
                    key={cat}
                    onClick={() => setSelectedCategory(cat)}
                    className={`px-3 py-1 text-sm font-medium rounded ${
                      selectedCategory === cat
                        ? 'bg-blue-600 text-white'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    {cat === 'all' ? 'Todas' : cat}
                  </button>
                ))}
              </div>
            </div>
          </div>

          <div className="divide-y divide-gray-200">
            {filteredDefinitions.map((def) => (
              <div key={def.id} className="px-6 py-4">
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">{def.featureName}</h3>
                    <p className="text-sm text-gray-600 mt-1">{def.description}</p>
                  </div>
                  <div className="flex gap-2 items-center">
                    <span className={`px-2 py-1 text-xs font-medium rounded-full bg-${featureStoreService.getFeatureTypeColor(def.featureType)}-100 text-${featureStoreService.getFeatureTypeColor(def.featureType)}-800`}>
                      {def.featureType}
                    </span>
                    {def.isActive ? (
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                        Active
                      </span>
                    ) : (
                      <span className="px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800">
                        Inactive
                      </span>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-3 gap-4 text-sm mt-3">
                  <div>
                    <span className="text-gray-500">Category:</span>
                    <span className="ml-2 font-medium text-gray-900">{def.category}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Refresh:</span>
                    <span className="ml-2 font-medium text-gray-900">{def.refreshIntervalHours}h</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Updated:</span>
                    <span className="ml-2 font-medium text-gray-900">
                      {new Date(def.updatedAt).toLocaleDateString('es-DO')}
                    </span>
                  </div>
                </div>
                {def.computationLogic && (
                  <div className="mt-3 p-3 bg-gray-50 rounded text-xs font-mono text-gray-700">
                    {def.computationLogic}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
