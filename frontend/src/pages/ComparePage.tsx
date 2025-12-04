import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import ComparisonTable from '@/components/organisms/ComparisonTable';
import { useCompare } from '@/hooks/useCompare';
import { mockVehicles } from '@/data/mockVehicles';
import { FiX, FiPlus } from 'react-icons/fi';

export default function ComparePage() {
  const navigate = useNavigate();
  const { compareItems, removeFromCompare, clearCompare, canAddMore, addToCompare, maxItems } = useCompare();
  const [showSelector, setShowSelector] = useState(false);

  // Get vehicles to compare
  const vehiclesToCompare = compareItems
    .map((id) => mockVehicles.find((v) => v.id === id))
    .filter((v) => v !== undefined);

  // Get available vehicles (not in compare)
  const availableVehicles = mockVehicles.filter((v) => !compareItems.includes(v.id));

  const handleAddVehicle = (vehicleId: string) => {
    addToCompare(vehicleId);
    setShowSelector(false);
  };

  if (compareItems.length === 0) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4">
          <div className="max-w-md text-center">
            <div className="w-20 h-20 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-6">
              <FiPlus size={40} className="text-gray-400" />
            </div>
            <h1 className="text-3xl font-bold font-heading text-gray-900 mb-4">
              No Vehicles to Compare
            </h1>
            <p className="text-gray-600 mb-8">
              Start adding vehicles to compare their specs, features, and pricing side by side.
            </p>
            <Button
              variant="primary"
              size="lg"
              onClick={() => navigate('/browse')}
            >
              Browse Vehicles
            </Button>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
                  Compare Vehicles
                </h1>
                <p className="text-gray-600">
                  Comparing {vehiclesToCompare.length} vehicle{vehiclesToCompare.length !== 1 ? 's' : ''}
                </p>
              </div>
              <div className="flex gap-3">
                {canAddMore() && (
                  <Button
                    variant="outline"
                    onClick={() => setShowSelector(!showSelector)}
                  >
                    <FiPlus className="mr-2" />
                    Add Vehicle
                  </Button>
                )}
                <Button
                  variant="outline"
                  onClick={() => {
                    if (confirm('Remove all vehicles from comparison?')) {
                      clearCompare();
                    }
                  }}
                >
                  Clear All
                </Button>
              </div>
            </div>

            {/* Vehicle Selector */}
            {showSelector && canAddMore() && (
              <div className="bg-white rounded-xl shadow-card p-6 mb-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">
                    Select a Vehicle to Add
                  </h3>
                  <button
                    onClick={() => setShowSelector(false)}
                    className="text-gray-400 hover:text-gray-600"
                  >
                    <FiX size={24} />
                  </button>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 max-h-96 overflow-y-auto">
                  {availableVehicles.slice(0, 12).map((vehicle) => (
                    <button
                      key={vehicle.id}
                      onClick={() => handleAddVehicle(vehicle.id)}
                      className="flex items-center gap-3 p-3 border border-gray-200 rounded-lg hover:border-primary hover:bg-primary/5 transition-all text-left"
                    >
                      <img
                        src={vehicle.images[0]}
                        alt={`${vehicle.make} ${vehicle.model}`}
                        className="w-16 h-16 object-cover rounded"
                      />
                      <div className="flex-1 min-w-0">
                        <p className="font-semibold text-gray-900 truncate">
                          {vehicle.year} {vehicle.make} {vehicle.model}
                        </p>
                        <p className="text-sm text-primary font-semibold">
                          ${vehicle.price.toLocaleString()}
                        </p>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Vehicle Cards Preview */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            {vehiclesToCompare.map((vehicle) => (
              <div
                key={vehicle.id}
                className="bg-white rounded-xl shadow-card overflow-hidden"
              >
                <div className="relative">
                  <img
                    src={vehicle.images[0]}
                    alt={`${vehicle.make} ${vehicle.model}`}
                    className="w-full h-48 object-cover"
                  />
                  <button
                    onClick={() => removeFromCompare(vehicle.id)}
                    className="absolute top-3 right-3 p-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors"
                    title="Remove from comparison"
                  >
                    <FiX size={20} />
                  </button>
                </div>
                <div className="p-4">
                  <h3 className="text-lg font-bold text-gray-900 mb-1">
                    {vehicle.year} {vehicle.make} {vehicle.model}
                  </h3>
                  <p className="text-2xl font-bold text-primary mb-2">
                    ${vehicle.price.toLocaleString()}
                  </p>
                  <div className="flex items-center gap-4 text-sm text-gray-600">
                    <span>{vehicle.mileage.toLocaleString()} mi</span>
                    <span>•</span>
                    <span>{vehicle.transmission}</span>
                  </div>
                </div>
              </div>
            ))}

            {/* Add More Card */}
            {canAddMore() && (
              <button
                onClick={() => setShowSelector(true)}
                className="bg-white rounded-xl shadow-card border-2 border-dashed border-gray-300 hover:border-primary hover:bg-primary/5 transition-all flex flex-col items-center justify-center min-h-[300px] gap-3"
              >
                <FiPlus size={48} className="text-gray-400" />
                <p className="text-gray-600 font-medium">Add Vehicle</p>
                <p className="text-sm text-gray-500">
                  Compare up to {maxItems} vehicles
                </p>
              </button>
            )}
          </div>

          {/* Comparison Table */}
          {vehiclesToCompare.length > 1 && (
            <>
              <ComparisonTable vehicles={vehiclesToCompare} />

              {/* Quick Actions */}
              <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6">
                {vehiclesToCompare.map((vehicle) => (
                  <div
                    key={vehicle.id}
                    className="bg-white rounded-xl shadow-card p-6"
                  >
                    <h3 className="font-bold text-gray-900 mb-4">
                      {vehicle.year} {vehicle.make} {vehicle.model}
                    </h3>
                    <div className="space-y-3">
                      <Link
                        to={`/vehicles/${vehicle.id}`}
                        className="block w-full px-4 py-2 bg-primary text-white text-center rounded-lg hover:bg-primary-600 transition-colors font-medium text-sm"
                      >
                        View Full Details
                      </Link>
                      <button
                        onClick={() => {
                          // Simulate contact action
                          alert(`Contact seller: ${vehicle.seller.name}\nPhone: ${vehicle.seller.phone}`);
                        }}
                        className="block w-full px-4 py-2 bg-gray-100 text-gray-900 text-center rounded-lg hover:bg-gray-200 transition-colors font-medium text-sm"
                      >
                        Contact Seller
                      </button>
                      <button
                        onClick={() => removeFromCompare(vehicle.id)}
                        className="block w-full px-4 py-2 bg-white border border-gray-300 text-gray-700 text-center rounded-lg hover:bg-gray-50 transition-colors font-medium text-sm"
                      >
                        Remove from Compare
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              {/* Comparison Insights */}
              <div className="mt-8 bg-gradient-to-br from-blue-50 to-indigo-50 rounded-xl p-6 border border-blue-200">
                <h3 className="text-lg font-bold text-gray-900 mb-4">Comparison Insights</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Price Range */}
                  <div className="bg-white rounded-lg p-4">
                    <p className="text-sm text-gray-600 mb-1">Price Range</p>
                    <p className="text-2xl font-bold text-gray-900">
                      ${Math.min(...vehiclesToCompare.map(v => v.price)).toLocaleString()} - ${Math.max(...vehiclesToCompare.map(v => v.price)).toLocaleString()}
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      Difference: ${(Math.max(...vehiclesToCompare.map(v => v.price)) - Math.min(...vehiclesToCompare.map(v => v.price))).toLocaleString()}
                    </p>
                  </div>

                  {/* Mileage Range */}
                  <div className="bg-white rounded-lg p-4">
                    <p className="text-sm text-gray-600 mb-1">Mileage Range</p>
                    <p className="text-2xl font-bold text-gray-900">
                      {Math.min(...vehiclesToCompare.map(v => v.mileage)).toLocaleString()} - {Math.max(...vehiclesToCompare.map(v => v.mileage)).toLocaleString()} mi
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      Difference: {(Math.max(...vehiclesToCompare.map(v => v.mileage)) - Math.min(...vehiclesToCompare.map(v => v.mileage))).toLocaleString()} mi
                    </p>
                  </div>

                  {/* Average Price */}
                  <div className="bg-white rounded-lg p-4">
                    <p className="text-sm text-gray-600 mb-1">Average Price</p>
                    <p className="text-2xl font-bold text-gray-900">
                      ${Math.round(vehiclesToCompare.reduce((sum, v) => sum + v.price, 0) / vehiclesToCompare.length).toLocaleString()}
                    </p>
                  </div>

                  {/* Best Value */}
                  <div className="bg-white rounded-lg p-4">
                    <p className="text-sm text-gray-600 mb-1">Best Overall Value</p>
                    <p className="text-lg font-bold text-green-600">
                      {(() => {
                        const lowestPrice = vehiclesToCompare.reduce((min, v) => v.price < min.price ? v : min);
                        return `${lowestPrice.year} ${lowestPrice.make} ${lowestPrice.model}`;
                      })()}
                    </p>
                    <p className="text-xs text-gray-500 mt-1">Lowest price</p>
                  </div>
                </div>
              </div>

              {/* Tips */}
              <div className="mt-8 bg-yellow-50 border border-yellow-200 rounded-xl p-6">
                <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
                  💡 Comparison Tips
                </h4>
                <ul className="space-y-2 text-sm text-gray-700">
                  <li>• Look for values highlighted in <span className="text-green-600 font-semibold">green</span> - these are the best values in each category</li>
                  <li>• The blue dot (●) indicates where vehicles differ - pay special attention to these</li>
                  <li>• Consider not just price, but also mileage, features, and condition</li>
                  <li>• Contact sellers to verify all information and schedule test drives</li>
                  <li>• Get a vehicle history report before making a final decision</li>
                </ul>
              </div>
            </>
          )}

          {vehiclesToCompare.length === 1 && (
            <div className="bg-blue-50 border border-blue-200 rounded-xl p-6 text-center">
              <p className="text-blue-900 font-medium mb-2">
                Add at least one more vehicle to start comparing
              </p>
              <p className="text-blue-700 text-sm">
                You can compare up to {maxItems} vehicles side by side
              </p>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
}
