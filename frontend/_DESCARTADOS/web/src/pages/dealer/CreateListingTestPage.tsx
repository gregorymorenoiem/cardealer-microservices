import { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { useDealerFeatures } from '../../hooks/useDealerFeatures';
import { LimitReachedBanner } from '../../components/dealer/UpgradePrompt';

/**
 * Página de testing para crear listings con control de límites
 */
export const CreateListingTestPage = () => {
  const user = useAuthStore((state) => state.user);
  const { hasReachedLimit, getUsageProgress, currentPlan, usage, limits } = useDealerFeatures(user?.subscription);
  const [vehicleData, setVehicleData] = useState({
    make: '',
    model: '',
    year: '',
    price: '',
  });

  const isAtLimit = hasReachedLimit('listings');
  const progress = getUsageProgress('listings');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (isAtLimit) {
      alert('Cannot create listing - limit reached!');
      return;
    }
    alert('Listing would be created here (mock)');
  };

  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Create New Listing</h1>
        <p className="text-gray-600">Add a new vehicle to your inventory</p>
      </div>

      {/* Banner si alcanzó el límite */}
      {isAtLimit && (
        <div className="mb-6">
          <LimitReachedBanner 
            type="listings"
            current={usage.currentListings}
            max={limits.maxListings}
            currentPlan={currentPlan}
          />
        </div>
      )}

      {/* Progress bar de uso */}
      {!isAtLimit && (
        <div className="bg-white border border-gray-200 rounded-lg p-5 mb-6 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div>
              <p className="text-sm font-semibold text-gray-900">
                Listing Usage
              </p>
              <p className="text-xs text-gray-600 mt-0.5">
                {usage.currentListings} of {limits.maxListings === 999999 ? '∞' : limits.maxListings} listings used
              </p>
            </div>
            <span className={`text-2xl font-bold ${
              progress > 90 ? 'text-red-600' : 
              progress > 70 ? 'text-yellow-600' : 
              'text-blue-600'
            }`}>
              {Math.round(progress)}%
            </span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
            <div 
              className={`h-3 rounded-full transition-all duration-500 ${
                progress > 90 ? 'bg-red-500' : 
                progress > 70 ? 'bg-yellow-500' : 
                'bg-blue-500'
              }`}
              style={{ width: `${Math.min(progress, 100)}%` }}
            />
          </div>
          {progress > 80 && (
            <div className="mt-3 p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
              <p className="text-xs text-yellow-900">
                <span className="font-semibold">⚠️ Warning:</span> You're running out of listing slots. 
                <a href="/dealer/billing/upgrade" className="text-yellow-700 hover:underline ml-1 font-medium">
                  Upgrade your plan
                </a> to add more vehicles.
              </p>
            </div>
          )}
        </div>
      )}

      {/* Current Plan Info */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-lg p-5 mb-6">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium text-blue-900">Current Plan</p>
            <p className="text-2xl font-bold text-blue-700 mt-1">
              {currentPlan.toUpperCase()}
            </p>
          </div>
          <div className="text-right">
            <p className="text-sm text-blue-700">Max Images per Listing</p>
            <p className="text-2xl font-bold text-blue-900">{limits.maxImages}</p>
          </div>
        </div>
        <div className="mt-4 pt-4 border-t border-blue-200">
          <div className="grid grid-cols-3 gap-4 text-xs">
            <div>
              <p className="text-blue-700">Bulk Upload</p>
              <p className="font-semibold text-blue-900">{limits.bulkUpload ? '✓ Yes' : '✗ No'}</p>
            </div>
            <div>
              <p className="text-blue-700">Featured Listings</p>
              <p className="font-semibold text-blue-900">{limits.maxFeaturedListings} available</p>
            </div>
            <div>
              <p className="text-blue-700">Analytics</p>
              <p className="font-semibold text-blue-900">{limits.analyticsAccess ? '✓ Yes' : '✗ No'}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Formulario de listing */}
      <form 
        onSubmit={handleSubmit}
        className={`bg-white rounded-lg shadow-md border border-gray-200 p-6 ${
          isAtLimit ? 'opacity-50 pointer-events-none' : ''
        }`}
      >
        <h2 className="text-xl font-semibold mb-4">Vehicle Information</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Make *
            </label>
            <input
              type="text"
              value={vehicleData.make}
              onChange={(e) => setVehicleData({ ...vehicleData, make: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="e.g., Toyota"
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Model *
            </label>
            <input
              type="text"
              value={vehicleData.model}
              onChange={(e) => setVehicleData({ ...vehicleData, model: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="e.g., Camry"
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Year *
            </label>
            <input
              type="number"
              value={vehicleData.year}
              onChange={(e) => setVehicleData({ ...vehicleData, year: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="2024"
              min="1900"
              max="2025"
              required
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Price *
            </label>
            <input
              type="number"
              value={vehicleData.price}
              onChange={(e) => setVehicleData({ ...vehicleData, price: e.target.value })}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="25000"
              min="0"
              required
            />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Description
          </label>
          <textarea
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            rows={4}
            placeholder="Describe the vehicle condition, features, etc."
          />
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Images (Max {limits.maxImages})
          </label>
          <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
            <p className="text-gray-500">Click to upload or drag and drop</p>
            <p className="text-xs text-gray-400 mt-1">
              PNG, JPG up to 10MB each (max {limits.maxImages} images)
            </p>
          </div>
        </div>

        <div className="flex gap-4">
          <button
            type="submit"
            disabled={isAtLimit}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed font-medium shadow-md transition-all"
          >
            {isAtLimit ? 'Limit Reached' : 'Publish Listing'}
          </button>
          <button
            type="button"
            className="px-6 py-3 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium transition-all"
          >
            Save as Draft
          </button>
        </div>

        {isAtLimit && (
          <p className="mt-4 text-sm text-red-600 font-medium">
            ⚠️ You've reached your listing limit. Upgrade your plan to add more vehicles.
          </p>
        )}
      </form>
    </div>
  );
};
