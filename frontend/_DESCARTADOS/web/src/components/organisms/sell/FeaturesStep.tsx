import { useState, useEffect } from 'react';
import Button from '@/components/atoms/Button';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';
import { FiPlus, FiX, FiCheck } from 'react-icons/fi';

interface FeaturesStepProps {
  data: Partial<VehicleFormData>;
  onNext: (data: Partial<VehicleFormData>) => void;
  onBack: () => void;
}

const featureCategories = {
  comfort: {
    title: '🪑 Comfort',
    features: [
      'Leather Seats',
      'Heated Seats',
      'Ventilated Seats',
      'Power Seats',
      'Memory Seats',
      'Lumbar Support',
      'Massage Seats',
      'Climate Control',
    ],
  },
  entertainment: {
    title: '🎵 Entertainment',
    features: [
      'Premium Sound System',
      'Navigation System',
      'Apple CarPlay',
      'Android Auto',
      'DVD Player',
      'WiFi Hotspot',
      'Wireless Charging',
      'Rear Entertainment',
    ],
  },
  safety: {
    title: '🛡️ Safety',
    features: [
      'Backup Camera',
      '360° Camera',
      'Blind Spot Monitor',
      'Lane Departure Warning',
      'Lane Keep Assist',
      'Adaptive Cruise Control',
      'Automatic Emergency Braking',
      'Front/Rear Parking Sensors',
      'Head-Up Display',
      'Night Vision',
    ],
  },
  convenience: {
    title: '🔧 Convenience',
    features: [
      'Keyless Entry',
      'Remote Start',
      'Power Liftgate',
      'Sunroof/Moonroof',
      'Panoramic Roof',
      'Rain Sensing Wipers',
      'Auto-Dimming Mirrors',
      'Power Folding Mirrors',
      'Heated Steering Wheel',
      'Cooled Glove Box',
    ],
  },
};

export default function FeaturesStep({ data, onNext, onBack }: FeaturesStepProps) {
  // Combine existing features with VIN-detected safety features
  const initialFeatures = [...(data.features || []), ...(data.vinSafetyFeatures || [])].filter(
    (v, i, a) => a.indexOf(v) === i
  ); // Remove duplicates

  const [selectedFeatures, setSelectedFeatures] = useState<string[]>(initialFeatures);
  const [customFeature, setCustomFeature] = useState('');
  const [customFeatures, setCustomFeatures] = useState<string[]>([]);
  const vinFeaturesCount = data.vinSafetyFeatures?.length || 0;

  // Auto-select VIN safety features on mount
  useEffect(() => {
    if (data.vinSafetyFeatures && data.vinSafetyFeatures.length > 0) {
      setSelectedFeatures((prev) => {
        const combined = [...prev, ...data.vinSafetyFeatures!];
        return combined.filter((v, i, a) => a.indexOf(v) === i);
      });
    }
  }, [data.vinSafetyFeatures]);

  const handleFeatureToggle = (feature: string) => {
    setSelectedFeatures((prev) =>
      prev.includes(feature) ? prev.filter((f) => f !== feature) : [...prev, feature]
    );
  };

  const handleAddCustomFeature = () => {
    if (customFeature.trim() && !customFeatures.includes(customFeature.trim())) {
      const newFeature = customFeature.trim();
      setCustomFeatures((prev) => [...prev, newFeature]);
      setSelectedFeatures((prev) => [...prev, newFeature]);
      setCustomFeature('');
    }
  };

  const handleRemoveCustomFeature = (feature: string) => {
    setCustomFeatures((prev) => prev.filter((f) => f !== feature));
    setSelectedFeatures((prev) => prev.filter((f) => f !== feature));
  };

  const handleNext = () => {
    onNext({ features: selectedFeatures });
  };

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-8">
        <h2 className="text-3xl font-bold text-gray-900 mb-2">Features & Options</h2>
        <p className="text-gray-600">
          Select all features that your vehicle has. This helps buyers find exactly what they're
          looking for.
        </p>
      </div>

      {/* VIN auto-detected features notice */}
      {vinFeaturesCount > 0 && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg">
          <div className="flex items-center gap-2">
            <FiCheck className="w-5 h-5 text-green-600" />
            <p className="text-green-800 font-medium">
              {vinFeaturesCount} feature{vinFeaturesCount !== 1 ? 's' : ''} auto-selected from VIN
            </p>
          </div>
          <p className="text-sm text-green-700 mt-1">
            Safety features detected: {data.vinSafetyFeatures?.join(', ')}
          </p>
        </div>
      )}

      {/* Selected count */}
      <div className="mb-6 p-4 bg-primary-50 rounded-lg">
        <p className="text-primary-900 font-medium">
          {selectedFeatures.length} feature{selectedFeatures.length !== 1 ? 's' : ''} selected
        </p>
      </div>

      {/* Feature categories */}
      <div className="space-y-8 mb-8">
        {Object.entries(featureCategories).map(([key, category]) => (
          <div key={key} className="bg-white rounded-lg border border-gray-200 p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">{category.title}</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {category.features.map((feature) => (
                <label
                  key={feature}
                  className="flex items-center p-3 rounded-lg border border-gray-200 hover:border-primary-300 hover:bg-primary-50 cursor-pointer transition-colors"
                >
                  <input
                    type="checkbox"
                    checked={selectedFeatures.includes(feature)}
                    onChange={() => handleFeatureToggle(feature)}
                    className="w-5 h-5 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="ml-3 text-gray-900">{feature}</span>
                </label>
              ))}
            </div>
          </div>
        ))}

        {/* Custom features section */}
        <div className="bg-white rounded-lg border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">✨ Custom Features</h3>
          <p className="text-sm text-gray-600 mb-4">Add any additional features not listed above</p>

          {/* Add custom feature input */}
          <div className="flex gap-2 mb-4">
            <input
              type="text"
              value={customFeature}
              onChange={(e) => setCustomFeature(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleAddCustomFeature()}
              placeholder="e.g., Tow Package, Sport Exhaust..."
              className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
            <Button
              type="button"
              variant="secondary"
              onClick={handleAddCustomFeature}
              disabled={!customFeature.trim()}
            >
              <FiPlus className="mr-2" />
              Add
            </Button>
          </div>

          {/* Custom features list */}
          {customFeatures.length > 0 && (
            <div className="space-y-2">
              {customFeatures.map((feature) => (
                <div
                  key={feature}
                  className="flex items-center justify-between p-3 bg-primary-50 rounded-lg"
                >
                  <span className="text-gray-900">{feature}</span>
                  <button
                    type="button"
                    onClick={() => handleRemoveCustomFeature(feature)}
                    className="p-1 text-red-600 hover:bg-red-100 rounded transition-colors"
                    aria-label={`Remove ${feature}`}
                  >
                    <FiX size={20} />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Navigation */}
      <div className="flex justify-between pt-6 border-t">
        <Button type="button" variant="secondary" size="lg" onClick={onBack}>
          Back
        </Button>
        <Button type="button" variant="primary" size="lg" onClick={handleNext}>
          Next: Pricing & Details
        </Button>
      </div>
    </div>
  );
}
