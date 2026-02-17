import { useState } from 'react';
import MainLayout from '@/layouts/MainLayout';
import { PricingWidget } from '@/components/pricing/PricingWidget';
import { DemandPredictor } from '@/components/pricing/DemandPredictor';
import { FiDollarSign, FiTrendingUp } from 'react-icons/fi';

export default function PricingIntelligencePage() {
  const [activeTab, setActiveTab] = useState<'pricing' | 'demand'>('pricing');

  // Demo form state
  const [formData, setFormData] = useState({
    make: 'Toyota',
    model: 'Corolla',
    year: 2021,
    mileage: 35000,
    condition: 'Good',
    fuelType: 'Gasoline',
    transmission: 'Automatic',
    currentPrice: 28000,
    photoCount: 10,
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: ['year', 'mileage', 'currentPrice', 'photoCount'].includes(name)
        ? Number(value)
        : value,
    }));
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900 mb-2">🤖 Pricing Intelligence IA</h1>
            <p className="text-gray-600">
              Analiza precios y demanda de vehículos con inteligencia artificial
            </p>
          </div>

          {/* Tabs */}
          <div className="flex space-x-1 mb-6 border-b border-gray-200">
            <button
              onClick={() => setActiveTab('pricing')}
              className={`px-6 py-3 font-medium text-sm transition-colors border-b-2 ${
                activeTab === 'pricing'
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <FiDollarSign className="inline mr-2" />
              Análisis de Precio
            </button>
            <button
              onClick={() => setActiveTab('demand')}
              className={`px-6 py-3 font-medium text-sm transition-colors border-b-2 ${
                activeTab === 'demand'
                  ? 'border-purple-600 text-purple-600'
                  : 'border-transparent text-gray-600 hover:text-gray-900'
              }`}
            >
              <FiTrendingUp className="inline mr-2" />
              Predicción de Demanda
            </button>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Form Column */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 space-y-4 sticky top-4">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Datos del Vehículo</h3>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Marca</label>
                  <select
                    name="make"
                    value={formData.make}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="Toyota">Toyota</option>
                    <option value="Honda">Honda</option>
                    <option value="Nissan">Nissan</option>
                    <option value="Hyundai">Hyundai</option>
                    <option value="Kia">Kia</option>
                    <option value="Ford">Ford</option>
                    <option value="Chevrolet">Chevrolet</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Modelo</label>
                  <input
                    type="text"
                    name="model"
                    value={formData.model}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Año</label>
                  <input
                    type="number"
                    name="year"
                    value={formData.year}
                    onChange={handleInputChange}
                    min="2000"
                    max="2025"
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Kilometraje
                  </label>
                  <input
                    type="number"
                    name="mileage"
                    value={formData.mileage}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Condición</label>
                  <select
                    name="condition"
                    value={formData.condition}
                    onChange={handleInputChange}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="Excellent">Excelente</option>
                    <option value="Good">Bueno</option>
                    <option value="Fair">Regular</option>
                    <option value="Poor">Pobre</option>
                  </select>
                </div>

                {activeTab === 'pricing' && (
                  <>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Precio Actual (RD$)
                      </label>
                      <input
                        type="number"
                        name="currentPrice"
                        value={formData.currentPrice}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Número de Fotos
                      </label>
                      <input
                        type="number"
                        name="photoCount"
                        value={formData.photoCount}
                        onChange={handleInputChange}
                        className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      />
                    </div>
                  </>
                )}
              </div>
            </div>

            {/* Results Column */}
            <div className="lg:col-span-2">
              {activeTab === 'pricing' && (
                <PricingWidget
                  make={formData.make}
                  model={formData.model}
                  year={formData.year}
                  mileage={formData.mileage}
                  condition={formData.condition}
                  fuelType={formData.fuelType}
                  transmission={formData.transmission}
                  currentPrice={formData.currentPrice}
                  photoCount={formData.photoCount}
                  viewCount={0}
                  daysListed={0}
                  onPriceChange={(suggestedPrice) => {
                    setFormData((prev) => ({ ...prev, currentPrice: suggestedPrice }));
                  }}
                />
              )}

              {activeTab === 'demand' && (
                <DemandPredictor
                  make={formData.make}
                  model={formData.model}
                  year={formData.year}
                  fuelType={formData.fuelType}
                  transmission={formData.transmission}
                />
              )}
            </div>
          </div>

          {/* Info Cards */}
          <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h4 className="text-lg font-semibold text-gray-900 mb-2">🎯 Alta Precisión</h4>
              <p className="text-sm text-gray-600">
                Nuestro modelo de ML analiza miles de vehículos para darte el precio más preciso del
                mercado.
              </p>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h4 className="text-lg font-semibold text-gray-900 mb-2">📈 Predicción de Demanda</h4>
              <p className="text-sm text-gray-600">
                Predice qué vehículos se venderán más rápido para optimizar tu inventario.
              </p>
            </div>

            <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
              <h4 className="text-lg font-semibold text-gray-900 mb-2">
                💡 Recomendaciones Inteligentes
              </h4>
              <p className="text-sm text-gray-600">
                Recibe sugerencias automáticas para vender más rápido y al mejor precio.
              </p>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
