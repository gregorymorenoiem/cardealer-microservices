import { useState } from 'react';
import Button from '@/components/atoms/Button';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';
import { FiCheck, FiEdit2, FiMapPin, FiDollarSign, FiPhone, FiMail, FiUser } from 'react-icons/fi';

interface ReviewStepProps {
  data: VehicleFormData;
  onSubmit: () => void;
  onBack: () => void;
  onSaveDraft?: () => void;
}

export default function ReviewStep({ data, onSubmit, onBack, onSaveDraft }: ReviewStepProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [agreed, setAgreed] = useState(false);

  const handleSubmit = async () => {
    if (!agreed) {
      alert('Please agree to the terms and conditions');
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit();
    } finally {
      setIsSubmitting(false);
    }
  };

  // Format price
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(price);
  };

  // Format mileage
  const formatMileage = (mileage: number) => {
    return new Intl.NumberFormat('en-US').format(mileage);
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold font-heading text-gray-900 mb-2">Review Your Listing</h2>
        <p className="text-gray-600">
          Please review all information before publishing your listing
        </p>
      </div>

      {/* Preview Card */}
      <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl p-6 border-2 border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <FiCheck className="text-green-500" />
          Listing Preview
        </h3>

        {/* Main Photo */}
        {data.images && data.images.length > 0 && (
          <div className="mb-6 rounded-xl overflow-hidden">
            <img
              src={URL.createObjectURL(data.images[0])}
              alt="Main vehicle"
              className="w-full h-64 object-cover"
            />
            {data.images.length > 1 && (
              <div className="grid grid-cols-4 gap-2 mt-2">
                {data.images.slice(1, 5).map((image: File, index: number) => (
                  <img
                    key={index}
                    src={URL.createObjectURL(image)}
                    alt={`Vehicle ${index + 2}`}
                    className="w-full h-20 object-cover rounded-lg"
                  />
                ))}
                {data.images.length > 5 && (
                  <div className="w-full h-20 bg-gray-800/70 rounded-lg flex items-center justify-center text-white text-sm font-semibold">
                    +{data.images.length - 5} more
                  </div>
                )}
              </div>
            )}
          </div>
        )}

        {/* Vehicle Title & Price */}
        <div className="mb-6">
          <h4 className="text-2xl font-bold text-gray-900 mb-2">
            {data.year} {data.make} {data.model}
          </h4>
          <p className="text-3xl font-bold text-primary mb-2">{formatPrice(data.price)}</p>
          <div className="flex items-center gap-4 text-gray-600">
            <span>{formatMileage(data.mileage)} miles</span>
            <span>•</span>
            <span>{data.condition}</span>
            <span>•</span>
            <span className="flex items-center gap-1">
              <FiMapPin size={16} />
              {data.location}
            </span>
          </div>
        </div>

        {/* Description */}
        <div className="mb-6">
          <h5 className="text-sm font-semibold text-gray-900 mb-2">Description</h5>
          <p className="text-gray-700 text-sm whitespace-pre-line line-clamp-4">
            {data.description}
          </p>
        </div>

        {/* Quick Specs Grid */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
          {data.transmission && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Transmission</p>
              <p className="text-sm font-semibold text-gray-900">{data.transmission}</p>
            </div>
          )}
          {data.fuelType && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Fuel Type</p>
              <p className="text-sm font-semibold text-gray-900">{data.fuelType}</p>
            </div>
          )}
          <div>
            <p className="text-xs text-gray-500 mb-1">Body Type</p>
            <p className="text-sm font-semibold text-gray-900">{data.bodyType}</p>
          </div>
          {data.drivetrain && (
            <div>
              <p className="text-xs text-gray-500 mb-1">Drivetrain</p>
              <p className="text-sm font-semibold text-gray-900">{data.drivetrain}</p>
            </div>
          )}
        </div>

        {/* Features */}
        {data.features && data.features.length > 0 && (
          <div className="mb-6">
            <h5 className="text-sm font-semibold text-gray-900 mb-2">Features</h5>
            <div className="flex flex-wrap gap-2">
              {data.features.slice(0, 8).map((feature: string) => (
                <span
                  key={feature}
                  className="px-3 py-1 bg-white border border-gray-300 rounded-full text-xs text-gray-700"
                >
                  {feature}
                </span>
              ))}
              {data.features.length > 8 && (
                <span className="px-3 py-1 bg-gray-200 border border-gray-300 rounded-full text-xs text-gray-700 font-semibold">
                  +{data.features.length - 8} more
                </span>
              )}
            </div>
          </div>
        )}

        {/* Seller Info */}
        <div className="border-t pt-4">
          <h5 className="text-sm font-semibold text-gray-900 mb-3">Seller Information</h5>
          <div className="space-y-2 text-sm">
            <div className="flex items-center gap-2 text-gray-700">
              <FiUser size={16} className="text-gray-400" />
              <span>{data.sellerName}</span>
            </div>
            <div className="flex items-center gap-2 text-gray-700">
              <FiPhone size={16} className="text-gray-400" />
              <span>{data.sellerPhone}</span>
            </div>
            <div className="flex items-center gap-2 text-gray-700">
              <FiMail size={16} className="text-gray-400" />
              <span>{data.sellerEmail}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Detailed Information Sections */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Vehicle Details */}
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-gray-900">Vehicle Details</h3>
            <FiEdit2 className="text-gray-400 cursor-pointer hover:text-primary" />
          </div>
          <div className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-600">VIN</span>
              <span className="font-medium text-gray-900">{data.vin}</span>
            </div>
            {data.engine && (
              <div className="flex justify-between">
                <span className="text-gray-600">Engine</span>
                <span className="font-medium text-gray-900">{data.engine}</span>
              </div>
            )}
            {data.horsepower && (
              <div className="flex justify-between">
                <span className="text-gray-600">Horsepower</span>
                <span className="font-medium text-gray-900">{data.horsepower}</span>
              </div>
            )}
            {data.doors && (
              <div className="flex justify-between">
                <span className="text-gray-600">Doors</span>
                <span className="font-medium text-gray-900">{data.doors}</span>
              </div>
            )}
            {data.seats && (
              <div className="flex justify-between">
                <span className="text-gray-600">Seats</span>
                <span className="font-medium text-gray-900">{data.seats}</span>
              </div>
            )}
            <div className="flex justify-between">
              <span className="text-gray-600">Exterior Color</span>
              <span className="font-medium text-gray-900">{data.exteriorColor}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Interior Color</span>
              <span className="font-medium text-gray-900">{data.interiorColor}</span>
            </div>
          </div>
        </div>

        {/* Listing Summary */}
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Listing Summary</h3>
          <div className="space-y-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-primary/10 rounded-lg flex items-center justify-center">
                <FiDollarSign className="text-primary" />
              </div>
              <div>
                <p className="text-xs text-gray-600">Asking Price</p>
                <p className="text-lg font-bold text-gray-900">{formatPrice(data.price)}</p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center text-blue-600 font-bold">
                {data.images?.length || 0}
              </div>
              <div>
                <p className="text-xs text-gray-600">Photos Uploaded</p>
                <p className="text-sm font-medium text-gray-900">
                  {data.images?.length || 0} image{data.images?.length !== 1 ? 's' : ''}
                </p>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center text-green-600 font-bold">
                {data.features?.length || 0}
              </div>
              <div>
                <p className="text-xs text-gray-600">Features Listed</p>
                <p className="text-sm font-medium text-gray-900">
                  {data.features?.length || 0} feature{data.features?.length !== 1 ? 's' : ''}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Terms and Conditions */}
      <div className="bg-gray-50 border border-gray-200 rounded-xl p-6">
        <label className="flex items-start gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={agreed}
            onChange={(e) => setAgreed(e.target.checked)}
            className="mt-1"
          />
          <div className="text-sm">
            <p className="text-gray-900 font-medium mb-1">I agree to the terms and conditions</p>
            <p className="text-gray-600">
              By publishing this listing, you confirm that all information provided is accurate and
              you have the right to sell this vehicle. You agree to our{' '}
              <a href="#" className="text-primary hover:underline">
                Terms of Service
              </a>{' '}
              and{' '}
              <a href="#" className="text-primary hover:underline">
                Privacy Policy
              </a>
              .
            </p>
          </div>
        </label>
      </div>

      {/* Actions */}
      <div className="flex justify-between items-center pt-6 border-t gap-4">
        <Button type="button" variant="outline" size="lg" onClick={onBack}>
          Back
        </Button>
        <div className="flex gap-3">
          {onSaveDraft && (
            <Button
              type="button"
              variant="secondary"
              size="lg"
              onClick={onSaveDraft}
              disabled={isSubmitting}
            >
              Save as Draft
            </Button>
          )}
          <Button
            type="button"
            variant="primary"
            size="lg"
            onClick={handleSubmit}
            disabled={!agreed || isSubmitting}
          >
            {isSubmitting ? 'Publishing...' : 'Publish Listing'}
          </Button>
        </div>
      </div>

      {/* Additional Info */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 text-sm text-blue-800">
        <p className="font-semibold mb-1">What happens next?</p>
        <ul className="space-y-1">
          <li>• Your listing will be reviewed (usually within 24 hours)</li>
          <li>• Once approved, it will be visible to thousands of buyers</li>
          <li>• You'll receive email notifications when buyers contact you</li>
          <li>• You can edit or remove your listing anytime from your dashboard</li>
        </ul>
      </div>
    </div>
  );
}
