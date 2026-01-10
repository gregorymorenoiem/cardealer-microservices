import { useEffect, useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '@/components/atoms/Input';
import Button from '@/components/atoms/Button';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';
import vehicleIntelligenceService from '@/services/vehicleIntelligenceService';
import type { PriceAnalysisDto } from '@/services/vehicleIntelligenceService';

const pricingSchema = z.object({
  price: z.number().min(1, 'Price is required').max(10000000, 'Price is too high'),
  description: z
    .string()
    .min(50, 'Description must be at least 50 characters')
    .max(2000, 'Description must be less than 2000 characters'),
  location: z.string().min(1, 'Location is required'),
  sellerName: z.string().min(1, 'Your name is required'),
  sellerPhone: z.string().min(10, 'Valid phone number is required'),
  sellerEmail: z.string().email('Valid email is required'),
  sellerType: z.enum(['private', 'dealer']),
});

type PricingFormData = z.infer<typeof pricingSchema>;

interface PricingStepProps {
  data: Partial<VehicleFormData>;
  onNext: (data: Partial<VehicleFormData>) => void;
  onBack: () => void;
}

export default function PricingStep({ data, onNext, onBack }: PricingStepProps) {
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<PricingFormData>({
    resolver: zodResolver(pricingSchema),
    defaultValues: {
      price: data.price || 0,
      description: data.description || '',
      location: data.location || '',
      sellerName: data.sellerName || '',
      sellerPhone: data.sellerPhone || '',
      sellerEmail: data.sellerEmail || '',
      sellerType: data.sellerType || 'private',
    },
  });

  const description = watch('description');
  const sellerType = watch('sellerType');
  const askingPrice = watch('price');
  const location = watch('location');

  const [suggestion, setSuggestion] = useState<PriceAnalysisDto | null>(null);
  const [suggestionLoading, setSuggestionLoading] = useState(false);
  const [suggestionError, setSuggestionError] = useState<string | null>(null);

  const requestBase = useMemo(() => {
    return {
      make: data.make,
      model: data.model,
      year: data.year,
      mileage: data.mileage,
      bodyType: data.bodyType,
    };
  }, [data.bodyType, data.make, data.mileage, data.model, data.year]);

  const hasAuth = typeof window !== 'undefined' && Boolean(localStorage.getItem('accessToken'));
  const canRequestSuggestion =
    hasAuth &&
    Boolean(requestBase.make) &&
    Boolean(requestBase.model) &&
    typeof requestBase.year === 'number' &&
    requestBase.year > 1900 &&
    typeof requestBase.mileage === 'number' &&
    requestBase.mileage >= 0 &&
    typeof askingPrice === 'number' &&
    askingPrice > 0 &&
    Boolean(location);

  useEffect(() => {
    if (!hasAuth) {
      setSuggestion(null);
      setSuggestionError(null);
      return;
    }

    if (!canRequestSuggestion) {
      setSuggestion(null);
      setSuggestionError(null);
      return;
    }

    let isCancelled = false;
    const timeoutId = window.setTimeout(async () => {
      setSuggestionLoading(true);
      setSuggestionError(null);
      try {
        const result = await vehicleIntelligenceService.analyzePricing({
          vehicleId: crypto.randomUUID(),
          make: requestBase.make as string,
          model: requestBase.model as string,
          year: requestBase.year as number,
          mileage: requestBase.mileage as number,
          condition: requestBase.condition || 'Good',
          fuelType: requestBase.fuelType || 'Gasoline',
          transmission: requestBase.transmission || 'Automatic',
          currentPrice: askingPrice,
          photoCount: 0,
          viewCount: 0,
          daysListed: 0,
        });
        if (!isCancelled) {
          setSuggestion(result);
        }
      } catch (error) {
        if (!isCancelled) {
          setSuggestion(null);
          setSuggestionError(
            error instanceof Error ? error.message : 'No se pudo calcular la sugerencia de precio'
          );
        }
      } finally {
        if (!isCancelled) {
          setSuggestionLoading(false);
        }
      }
    }, 500);

    return () => {
      isCancelled = true;
      window.clearTimeout(timeoutId);
    };
  }, [askingPrice, canRequestSuggestion, hasAuth, location, requestBase]);

  const onSubmit = (formData: PricingFormData) => {
    onNext(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold font-heading text-gray-900 mb-2">Pricing & Details</h2>
        <p className="text-gray-600">
          Set your asking price and provide detailed information about your vehicle
        </p>
      </div>

      {/* Price */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Asking Price <span className="text-red-500">*</span>
        </label>
        <div className="relative">
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-lg font-semibold">
            $
          </span>
          <input
            {...register('price', { valueAsNumber: true })}
            type="number"
            placeholder="50,000"
            className="w-full pl-8 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent text-lg"
          />
        </div>
        {errors.price && <p className="text-red-500 text-sm mt-1">{errors.price.message}</p>}

        {/* Intelligent Pricing (Sprint 18) */}
        <div className="mt-3 space-y-3">
          {!hasAuth && (
            <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
              <p className="text-xs text-gray-700">
                Inicia sesión para ver sugerencias de precio inteligentes.
              </p>
            </div>
          )}

          {hasAuth && !canRequestSuggestion && (
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
              <p className="text-xs text-blue-800">
                Completa <strong>Marca</strong>, <strong>Modelo</strong>, <strong>Año</strong>,{' '}
                <strong>Millaje</strong>, <strong>Precio</strong> y <strong>Ubicación</strong> para
                calcular tu precio sugerido.
              </p>
            </div>
          )}

          {suggestionLoading && (
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
              <p className="text-xs text-blue-800">Calculando sugerencia de precio…</p>
            </div>
          )}

          {suggestionError && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3">
              <p className="text-xs text-red-700">{suggestionError}</p>
            </div>
          )}

          {suggestion && (
            <div className="bg-white border border-gray-200 rounded-xl p-4">
              <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3">
                <div>
                  <p className="text-sm font-medium text-gray-700">Precio sugerido</p>
                  <p className="mt-1 text-2xl font-bold text-gray-900">
                    ${suggestion.suggestedPrice.toLocaleString()}
                  </p>
                  <p className="mt-1 text-xs text-gray-500">
                    Rango: ${suggestion.suggestedPriceMin.toLocaleString()} - $
                    {suggestion.suggestedPriceMax.toLocaleString()}
                    {' • '}Mercado promedio: ${suggestion.marketAvgPrice.toLocaleString()}
                  </p>
                </div>

                <div className="bg-gray-50 border border-gray-200 rounded-lg p-3">
                  <p className="text-xs text-gray-700">Tiempo estimado de venta</p>
                  <p className="text-lg font-semibold text-gray-900">
                    ~{suggestion.predictedDaysToSaleAtCurrentPrice} días
                  </p>
                  <p className="text-xs text-gray-500">
                    Con precio sugerido: {suggestion.predictedDaysToSaleAtSuggestedPrice} días
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    Confianza: {Math.round(suggestion.confidenceScore)}%
                  </p>
                </div>
              </div>

              <div className="mt-3 flex flex-wrap gap-2">
                <span
                  className={`text-xs font-medium px-2.5 py-1 rounded-full border ${
                    suggestion.pricePosition === 'Above Market'
                      ? 'bg-red-50 text-red-700 border-red-200'
                      : suggestion.pricePosition === 'Below Market'
                        ? 'bg-green-50 text-green-700 border-green-200'
                        : 'bg-amber-50 text-amber-700 border-amber-200'
                  }`}
                >
                  {suggestion.pricePosition === 'Above Market'
                    ? `Tu precio está ${Math.abs(Math.round(suggestion.priceVsMarket * 100))}% por encima del mercado`
                    : suggestion.pricePosition === 'Below Market'
                      ? `Tu precio está ${Math.abs(Math.round(suggestion.priceVsMarket * 100))}% por debajo del mercado`
                      : 'Tu precio está dentro del mercado'}
                </span>
              </div>

              {suggestion.recommendations && suggestion.recommendations.length > 0 && (
                <div className="mt-3">
                  <p className="text-sm font-semibold text-gray-900">Recomendaciones IA</p>
                  <ul className="mt-2 space-y-1">
                    {suggestion.recommendations.slice(0, 3).map((rec, idx) => (
                      <li key={idx} className="text-xs text-gray-700 flex items-start">
                        <span className="mr-1.5 text-blue-500">•</span>
                        <span>
                          <strong>{rec.type}:</strong> {rec.reason}
                        </span>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}

          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <p className="text-xs text-blue-800">
              <strong>Pricing Tips:</strong> Research similar vehicles in your area. Price
              competitively to attract buyers. Consider highlighting any recent maintenance or
              upgrades.
            </p>
          </div>
        </div>
      </div>

      {/* Description */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Vehicle Description <span className="text-red-500">*</span>
        </label>
        <textarea
          {...register('description')}
          rows={8}
          placeholder="Describe your vehicle in detail. Include information about its condition, service history, modifications, reason for selling, etc."
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent resize-none"
        />
        <div className="flex justify-between items-center mt-1">
          <div>
            {errors.description && (
              <p className="text-red-500 text-sm">{errors.description.message}</p>
            )}
          </div>
          <p className={`text-sm ${description.length < 50 ? 'text-red-500' : 'text-gray-500'}`}>
            {description.length} / 2000 characters{' '}
            {description.length < 50 && `(${50 - description.length} more needed)`}
          </p>
        </div>

        <div className="mt-2 bg-blue-50 border border-blue-200 rounded-lg p-3">
          <p className="text-xs text-blue-800 mb-2">
            <strong>Description Tips:</strong> A detailed description helps buyers make informed
            decisions.
          </p>
          <ul className="text-xs text-blue-800 space-y-1">
            <li>• Mention service history and maintenance records</li>
            <li>• Describe any recent repairs or upgrades</li>
            <li>• Be honest about any issues or wear</li>
            <li>• Highlight unique features or options</li>
            <li>• Explain why you're selling the vehicle</li>
          </ul>
        </div>
      </div>

      {/* Location */}
      <div>
        <Input
          {...register('location')}
          label="Location"
          placeholder="e.g., Los Angeles, CA"
          error={errors.location?.message}
          required
        />
        <p className="text-xs text-gray-500 mt-1">City and state where the vehicle is located</p>
      </div>

      {/* Seller Information */}
      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Contact Information</h3>

        {/* Seller Type */}
        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            I am a <span className="text-red-500">*</span>
          </label>
          <div className="flex gap-4">
            <label className="flex items-center cursor-pointer">
              <input {...register('sellerType')} type="radio" value="private" className="mr-2" />
              <span
                className={`text-sm ${sellerType === 'private' ? 'font-semibold text-primary' : 'text-gray-700'}`}
              >
                Private Seller
              </span>
            </label>
            <label className="flex items-center cursor-pointer">
              <input {...register('sellerType')} type="radio" value="dealer" className="mr-2" />
              <span
                className={`text-sm ${sellerType === 'dealer' ? 'font-semibold text-primary' : 'text-gray-700'}`}
              >
                Dealer
              </span>
            </label>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            {...register('sellerName')}
            label={sellerType === 'dealer' ? 'Dealership Name' : 'Your Name'}
            placeholder={sellerType === 'dealer' ? 'ABC Motors' : 'John Doe'}
            error={errors.sellerName?.message}
            required
          />

          <Input
            {...register('sellerPhone')}
            type="tel"
            label="Phone Number"
            placeholder="+1 (555) 123-4567"
            error={errors.sellerPhone?.message}
            required
          />

          <Input
            {...register('sellerEmail')}
            type="email"
            label="Email Address"
            placeholder="seller@example.com"
            error={errors.sellerEmail?.message}
            required
          />
        </div>

        <div className="mt-3 bg-gray-50 border border-gray-200 rounded-lg p-3">
          <p className="text-xs text-gray-600">
            Your contact information will be visible to interested buyers. We recommend using a
            phone number and email you check regularly.
          </p>
        </div>
      </div>

      {/* Actions */}
      <div className="flex justify-between pt-6 border-t">
        <Button type="button" variant="outline" size="lg" onClick={onBack}>
          Back
        </Button>
        <Button type="submit" variant="primary" size="lg">
          Next: Review Listing
        </Button>
      </div>
    </form>
  );
}
