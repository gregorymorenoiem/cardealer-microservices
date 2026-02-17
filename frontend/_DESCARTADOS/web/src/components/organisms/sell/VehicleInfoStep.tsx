import { useState, useEffect, useCallback } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '@/components/atoms/Input';
import Button from '@/components/atoms/Button';
import Combobox from '@/components/atoms/Combobox';
import TrimSelectionCard from '@/components/molecules/TrimSelectionCard';
import VehiclePreviewCard from '@/components/molecules/VehiclePreviewCard';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';
import {
  getAllMakes,
  getModelsByMake,
  getAvailableYears,
  getTrimsByModelAndYear,
  type VehicleMake,
  type VehicleModel,
  type VehicleTrim,
  createSlug,
} from '@/services/vehicleCatalogService';
import { FiLoader, FiCheck, FiInfo, FiGrid, FiList, FiAlertCircle, FiSearch } from 'react-icons/fi';
import { decodeVIN, validateVINFormat, type VinDecodeResult } from '@/services/vinDecoderService';

// ============================================================
// FALLBACK DATA (used when backend is not available)
// ============================================================

const fallbackMakes = [
  'Toyota',
  'Honda',
  'Ford',
  'Chevrolet',
  'Nissan',
  'Hyundai',
  'Kia',
  'BMW',
  'Mercedes-Benz',
  'Audi',
  'Volkswagen',
  'Mazda',
  'Subaru',
  'Lexus',
  'Jeep',
  'Tesla',
  'GMC',
  'Ram',
  'Dodge',
  'Cadillac',
];

const transmissions = ['Automatic', 'Manual', 'CVT', 'Semi-Automatic', 'Dual-Clutch'];
const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'Plug-in Hybrid'];
const bodyTypes = [
  'Sedan',
  'SUV',
  'Truck',
  'Coupe',
  'Hatchback',
  'Van',
  'Convertible',
  'Wagon',
  'Crossover',
  'Minivan',
];
const drivetrains = ['FWD', 'RWD', 'AWD', '4WD'];
const conditions = ['New', 'Used', 'Certified Pre-Owned'];
const doorOptions = [2, 3, 4, 5];
const seatOptions = [2, 4, 5, 6, 7, 8];

// ============================================================
// VALIDATION SCHEMA
// ============================================================

const vehicleInfoSchema = z.object({
  make: z.string().min(1, 'Make is required'),
  model: z.string().min(1, 'Model is required'),
  year: z
    .number()
    .min(1900, 'Invalid year')
    .max(new Date().getFullYear() + 1, 'Invalid year'),
  trim: z.string().optional(),
  mileage: z.number().min(0, 'Mileage must be positive'),
  vin: z.string().min(1, 'VIN is required').max(17, 'VIN must be at most 17 characters'),
  transmission: z.string().optional(), // Opcional si VIN no tiene datos
  fuelType: z.string().optional(), // Opcional si VIN no tiene datos
  bodyType: z.string().optional(), // Opcional - VIN puede no tener datos compatibles
  drivetrain: z.string().optional(), // Opcional si VIN no tiene datos
  engine: z.string().optional(), // Opcional si VIN no tiene datos
  horsepower: z.string().optional(),
  doors: z.number().optional(),
  seats: z.number().optional(),
  exteriorColor: z.string().min(1, 'Exterior color is required'),
  interiorColor: z.string().min(1, 'Interior color is required'),
  condition: z.string().min(1, 'Condition is required'),
  // Hidden fields from catalog
  makeId: z.string().optional(),
  modelId: z.string().optional(),
  trimId: z.string().optional(),
  baseMSRP: z.number().optional(),
});

type VehicleInfoFormData = z.infer<typeof vehicleInfoSchema>;

interface VehicleInfoStepProps {
  data: Partial<VehicleFormData>;
  onNext: (data: Partial<VehicleFormData>) => void;
  onBack: () => void;
}

// ============================================================
// COMPONENT
// ============================================================

export default function VehicleInfoStep({ data, onNext }: VehicleInfoStepProps) {
  const currentYear = new Date().getFullYear();

  // ========================================
  // STATE - Catalog Data
  // ========================================
  const [makes, setMakes] = useState<VehicleMake[]>([]);
  const [models, setModels] = useState<VehicleModel[]>([]);
  const [availableYears, setAvailableYears] = useState<number[]>([]);
  const [trims, setTrims] = useState<VehicleTrim[]>([]);

  // Loading states
  const [loadingMakes, setLoadingMakes] = useState(true);
  const [loadingModels, setLoadingModels] = useState(false);
  const [loadingYears, setLoadingYears] = useState(false);
  const [loadingTrims, setLoadingTrims] = useState(false);

  // Selected items
  const [selectedMake, setSelectedMake] = useState<VehicleMake | null>(null);
  const [selectedModel, setSelectedModel] = useState<VehicleModel | null>(null);
  const [selectedTrim, setSelectedTrim] = useState<VehicleTrim | null>(null);

  // Auto-fill notification
  const [showAutoFillNotice, setShowAutoFillNotice] = useState(false);

  // Using fallback data
  const [usingFallback, setUsingFallback] = useState(false);

  // UI preferences
  const [trimViewMode, setTrimViewMode] = useState<'grid' | 'dropdown'>('grid');

  // VIN Decoder state
  const [vinDecoding, setVinDecoding] = useState(false);
  const [vinDecodeResult, setVinDecodeResult] = useState<VinDecodeResult | null>(null);
  const [showVinSuccess, setShowVinSuccess] = useState(false);

  // ========================================
  // FORM
  // ========================================
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    control,
    formState: { errors },
  } = useForm<VehicleInfoFormData>({
    resolver: zodResolver(vehicleInfoSchema),
    defaultValues: {
      make: data.make || '',
      model: data.model || '',
      year: data.year || currentYear,
      trim: (data as VehicleInfoFormData).trim || '',
      mileage: data.mileage || 0,
      vin: data.vin || '',
      transmission: data.transmission || '',
      fuelType: data.fuelType || '',
      bodyType: data.bodyType || '',
      drivetrain: data.drivetrain || '',
      engine: data.engine || '',
      horsepower: data.horsepower || '',
      doors: data.doors || 4,
      seats: data.seats || 5,
      exteriorColor: data.exteriorColor || '',
      interiorColor: data.interiorColor || '',
      condition: data.condition || '',
    },
  });

  const watchMake = watch('make');
  const watchModel = watch('model');
  const watchYear = watch('year');
  const watchTrim = watch('trim');
  const watchExteriorColor = watch('exteriorColor');
  const watchMileage = watch('mileage');
  const watchCondition = watch('condition');
  const watchTransmission = watch('transmission');
  const watchFuelType = watch('fuelType');
  const watchEngine = watch('engine');
  const watchHorsepower = watch('horsepower');
  const watchDoors = watch('doors');
  const watchSeats = watch('seats');
  const watchVin = watch('vin');

  // ========================================
  // LOAD MAKES ON MOUNT
  // ========================================
  useEffect(() => {
    async function loadMakes() {
      setLoadingMakes(true);
      try {
        const makesData = await getAllMakes();
        if (makesData.length > 0) {
          setMakes(makesData);
          setUsingFallback(false);
        } else {
          // Use fallback
          setMakes(
            fallbackMakes.map((name) => ({
              id: createSlug(name),
              name,
              slug: createSlug(name),
              isPopular: true,
            }))
          );
          setUsingFallback(true);
        }
      } catch (error) {
        console.error('Error loading makes:', error);
        // Use fallback
        setMakes(
          fallbackMakes.map((name) => ({
            id: createSlug(name),
            name,
            slug: createSlug(name),
            isPopular: true,
          }))
        );
        setUsingFallback(true);
      }
      setLoadingMakes(false);
    }
    loadMakes();
  }, []);

  // ========================================
  // LOAD MODELS WHEN MAKE CHANGES
  // ========================================
  useEffect(() => {
    async function loadModels() {
      if (!watchMake) {
        setModels([]);
        setSelectedMake(null);
        return;
      }

      const make = makes.find((m) => m.name === watchMake || m.slug === watchMake);
      setSelectedMake(make || null);

      if (!make || usingFallback) {
        setModels([]);
        return;
      }

      setLoadingModels(true);
      try {
        const modelsData = await getModelsByMake(make.slug);
        setModels(modelsData);
      } catch (error) {
        console.error('Error loading models:', error);
        setModels([]);
      }
      setLoadingModels(false);
    }
    loadModels();
  }, [watchMake, makes, usingFallback]);

  // ========================================
  // LOAD YEARS WHEN MODEL CHANGES
  // ========================================
  useEffect(() => {
    async function loadYears() {
      if (!watchModel || !selectedMake) {
        setAvailableYears([]);
        setSelectedModel(null);
        return;
      }

      const model = models.find((m) => m.name === watchModel || m.slug === watchModel);
      setSelectedModel(model || null);

      if (!model || usingFallback) {
        // Generate default years
        setAvailableYears(Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse());
        return;
      }

      setLoadingYears(true);
      try {
        const yearsData = await getAvailableYears(model.id);
        setAvailableYears(
          yearsData.length > 0
            ? yearsData
            : Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse()
        );
      } catch (error) {
        console.error('Error loading years:', error);
        setAvailableYears(Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse());
      }
      setLoadingYears(false);
    }
    loadYears();
  }, [watchModel, models, selectedMake, currentYear, usingFallback]);

  // ========================================
  // LOAD TRIMS WHEN YEAR CHANGES
  // ========================================
  useEffect(() => {
    async function loadTrims() {
      if (!selectedModel || !watchYear || usingFallback) {
        setTrims([]);
        return;
      }

      setLoadingTrims(true);
      try {
        const trimsData = await getTrimsByModelAndYear(selectedModel.id, watchYear);
        setTrims(trimsData);
      } catch (error) {
        console.error('Error loading trims:', error);
        setTrims([]);
      }
      setLoadingTrims(false);
    }
    loadTrims();
  }, [selectedModel, watchYear, usingFallback]);

  // ========================================
  // AUTO-FILL WHEN TRIM IS SELECTED
  // ========================================
  const handleTrimSelect = useCallback(
    (trimName: string) => {
      const trim = trims.find((t) => t.name === trimName);

      if (!trim) {
        setSelectedTrim(null);
        return;
      }

      setSelectedTrim(trim);
      setShowAutoFillNotice(true);

      // Auto-fill form with trim specs (dealer can still modify)
      if (trim.engineSize) {
        setValue('engine', trim.engineSize);
      }
      if (trim.horsepower) {
        setValue('horsepower', `${trim.horsepower} hp`);
      }
      if (trim.fuelType) {
        setValue('fuelType', mapFuelType(trim.fuelType));
      }
      if (trim.transmission) {
        setValue('transmission', mapTransmission(trim.transmission));
      }
      if (trim.driveType) {
        setValue('drivetrain', mapDriveType(trim.driveType));
      }
      if (trim.mpgCity && trim.mpgHighway) {
        setValue('mpg', `${trim.mpgCity} city / ${trim.mpgHighway} hwy`);
      }

      // Auto-fill body type from selected model
      if (selectedModel?.bodyStyle && !watch('bodyType')) {
        const mappedBodyType = mapBodyStyle(selectedModel.bodyStyle);
        if (mappedBodyType) {
          setValue('bodyType', mappedBodyType);
        }
      }

      // Store hidden fields
      setValue('trimId', trim.id);
      setValue('trim', trim.name);
      if (trim.baseMSRP) setValue('baseMSRP', trim.baseMSRP);

      // Hide notice after 5 seconds
      setTimeout(() => setShowAutoFillNotice(false), 5000);
    },
    [trims, setValue, selectedModel, watch]
  );

  useEffect(() => {
    if (watchTrim) {
      handleTrimSelect(watchTrim);
    }
  }, [watchTrim, handleTrimSelect]);

  // ========================================
  // VIN DECODER
  // ========================================
  const handleVinDecode = async () => {
    const validation = validateVINFormat(watchVin || '');
    if (!validation.valid) {
      setVinDecodeResult({
        success: false,
        vin: watchVin || '',
        errorCode: 'INVALID_FORMAT',
        errorMessage: validation.message,
      });
      return;
    }

    setVinDecoding(true);
    setVinDecodeResult(null);

    try {
      const result = await decodeVIN(watchVin || '');
      setVinDecodeResult(result);

      if (result.success) {
        // Options to force react-hook-form to update the DOM
        const setOpts = { shouldValidate: true, shouldDirty: true };

        // Clear VIN-related fields first to avoid stale data from previous decodes
        setValue('make', '', setOpts);
        setValue('model', '', setOpts);
        setValue('trim', '', setOpts);
        setValue('bodyType', '', setOpts);
        setValue('drivetrain', '', setOpts);
        setValue('fuelType', '', setOpts);
        setValue('transmission', '', setOpts);
        setValue('engine', '', setOpts);
        setValue('horsepower', '', setOpts);
        setValue('doors', 4, setOpts);
        setValue('seats', 5, setOpts);

        // If VIN has a year, ensure it's in the available years list
        if (result.year) {
          setAvailableYears((prev) => {
            if (prev.includes(result.year!)) return prev;
            // Add the year and sort descending
            return [...prev, result.year!].sort((a, b) => b - a);
          });
        }

        // Now auto-fill form fields from VIN data
        if (result.make) setValue('make', result.make, setOpts);
        if (result.model) setValue('model', result.model, setOpts);
        if (result.year) {
          console.log('Setting year to:', result.year);
          setValue('year', result.year, setOpts);
        }
        if (result.trim) setValue('trim', result.trim, setOpts);
        if (result.bodyClass) setValue('bodyType', result.bodyClass, setOpts);
        if (result.driveType) setValue('drivetrain', result.driveType, setOpts);
        if (result.fuelType) setValue('fuelType', result.fuelType, setOpts);
        if (result.transmission) setValue('transmission', result.transmission, setOpts);
        if (result.engineSize) setValue('engine', result.engineSize, setOpts);
        if (result.horsepower) setValue('horsepower', `${result.horsepower} hp`, setOpts);
        if (result.doors) setValue('doors', result.doors, setOpts);
        if (result.seats) setValue('seats', result.seats, setOpts);

        // Store VIN data for other steps (FeaturesStep, PricingStep)
        // These are stored in vinDecodeResult state and passed to onNext

        setShowVinSuccess(true);
        setTimeout(() => setShowVinSuccess(false), 5000);
      }
    } catch (error) {
      console.error('VIN decode error:', error);
      setVinDecodeResult({
        success: false,
        vin: watchVin || '',
        errorCode: 'UNKNOWN',
        errorMessage: 'Error al decodificar el VIN.',
      });
    } finally {
      setVinDecoding(false);
    }
  };

  // ========================================
  // HELPERS
  // ========================================
  function mapFuelType(value: string): string {
    const mapping: Record<string, string> = {
      Gasoline: 'Gasoline',
      Diesel: 'Diesel',
      Electric: 'Electric',
      Hybrid: 'Hybrid',
      PlugInHybrid: 'Plug-in Hybrid',
    };
    return mapping[value] || value;
  }

  function mapTransmission(value: string): string {
    const mapping: Record<string, string> = {
      Automatic: 'Automatic',
      Manual: 'Manual',
      CVT: 'CVT',
      DualClutch: 'Dual-Clutch',
      Automated: 'Semi-Automatic',
    };
    return mapping[value] || value;
  }

  function mapDriveType(value: string): string {
    const mapping: Record<string, string> = {
      FWD: 'FWD',
      RWD: 'RWD',
      AWD: 'AWD',
      FourWD: '4WD',
    };
    return mapping[value] || value;
  }

  function mapBodyStyle(value: string): string {
    // Map model body styles to form body types
    const mapping: Record<string, string> = {
      Sedan: 'Sedan',
      SUV: 'SUV',
      Crossover: 'Crossover',
      Truck: 'Truck',
      Pickup: 'Truck',
      Coupe: 'Coupe',
      Hatchback: 'Hatchback',
      Van: 'Van',
      Minivan: 'Minivan',
      Wagon: 'Wagon',
      Convertible: 'Convertible',
      Sports: 'Coupe',
      SportsCar: 'Coupe',
    };
    return mapping[value] || value;
  }

  const onSubmit = (formData: VehicleInfoFormData) => {
    console.log('Form submitted:', formData);
    console.log('Validation errors:', errors);

    // Add IDs from catalog and VIN-decoded data
    const enrichedData = {
      ...formData,
      makeId: selectedMake?.id,
      modelId: selectedModel?.id,
      // Pass VIN-decoded data to other steps
      doors: vinDecodeResult?.doors,
      seats: vinDecodeResult?.seats,
      vinBasePrice: vinDecodeResult?.basePrice,
      vinSafetyFeatures: vinDecodeResult?.safetyFeatures || [],
    };
    onNext(enrichedData as Partial<VehicleFormData>);
  };

  // ========================================
  // RENDER
  // ========================================
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="w-full">
      {/* Validation Errors Summary */}
      {Object.keys(errors).length > 0 && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
          <div className="flex items-start gap-2">
            <FiAlertCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
            <div>
              <h4 className="font-semibold text-red-900 mb-1">Please fix the following errors:</h4>
              <ul className="text-sm text-red-700 list-disc list-inside space-y-1">
                {Object.entries(errors).map(([field, error]) => (
                  <li key={field}>
                    <strong>{field}:</strong> {error?.message?.toString()}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}

      {/* Main Layout: Form + Preview Sidebar */}
      <div className="flex flex-col xl:flex-row gap-4 lg:gap-6">
        {/* Sidebar: Vehicle Preview Card - Show first on mobile, sticky on desktop */}
        <div className="xl:hidden mb-4">
          {/* Collapsed preview for mobile */}
          {watchMake && watchModel && (
            <div className="bg-gradient-to-r from-primary to-primary-dark p-3 rounded-lg text-white">
              <p className="font-semibold text-sm truncate">
                {watchYear} {watchMake} {watchModel} {watchTrim && `• ${watchTrim}`}
              </p>
              {selectedTrim?.baseMSRP && (
                <p className="text-xs text-white/80">
                  MSRP: ${selectedTrim.baseMSRP.toLocaleString()}
                </p>
              )}
            </div>
          )}
        </div>

        {/* Main Form Column */}
        <div className="flex-1 min-w-0 space-y-4 lg:space-y-6">
          <div>
            <h2 className="text-xl md:text-2xl font-bold font-heading text-gray-900 mb-1 md:mb-2">
              Vehicle Information
            </h2>
            <p className="text-sm md:text-base text-gray-600">
              Enter the VIN to auto-fill vehicle details, or enter manually.
            </p>
          </div>

          {/* VIN DECODER - Quick Start */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  VIN <span className="text-xs text-blue-600">(auto-fill)</span>
                </label>
                <Input
                  {...register('vin')}
                  placeholder="17-character VIN"
                  maxLength={17}
                  className="font-mono uppercase"
                  error={errors.vin?.message}
                />
              </div>
              <div className="sm:pt-6">
                <Button
                  type="button"
                  onClick={handleVinDecode}
                  disabled={vinDecoding || !watchVin || watchVin.length < 17}
                  className="w-full sm:w-auto"
                >
                  {vinDecoding ? (
                    <FiLoader className="w-4 h-4 animate-spin" />
                  ) : (
                    <FiSearch className="w-4 h-4" />
                  )}
                  <span className="ml-2">{vinDecoding ? 'Loading...' : 'Decode'}</span>
                </Button>
              </div>
            </div>

            {/* Success message */}
            {showVinSuccess && vinDecodeResult?.success && (
              <div className="mt-3 p-3 bg-green-50 border border-green-200 rounded-lg">
                <p className="text-sm text-green-800 font-medium flex items-center gap-1">
                  <FiCheck className="w-4 h-4" />
                  {vinDecodeResult.year} {vinDecodeResult.make} {vinDecodeResult.model} loaded!
                </p>
                {/* Show what fields were auto-filled */}
                <div className="mt-2 text-xs text-green-700">
                  <p className="font-medium mb-1">Auto-filled fields:</p>
                  <div className="grid grid-cols-2 sm:grid-cols-3 gap-x-3 gap-y-1">
                    {vinDecodeResult.make && <span>✓ Make</span>}
                    {vinDecodeResult.model && <span>✓ Model</span>}
                    {vinDecodeResult.year && <span>✓ Year</span>}
                    {vinDecodeResult.trim && <span>✓ Trim</span>}
                    {vinDecodeResult.bodyClass && <span>✓ Body Type</span>}
                    {vinDecodeResult.transmission && <span>✓ Transmission</span>}
                    {vinDecodeResult.fuelType && <span>✓ Fuel Type</span>}
                    {vinDecodeResult.driveType && <span>✓ Drivetrain</span>}
                    {vinDecodeResult.engineSize && <span>✓ Engine</span>}
                    {vinDecodeResult.horsepower && <span>✓ Horsepower</span>}
                    {vinDecodeResult.doors && <span>✓ Doors</span>}
                    {vinDecodeResult.seats && <span>✓ Seats</span>}
                  </div>
                  {vinDecodeResult.safetyFeatures && vinDecodeResult.safetyFeatures.length > 0 && (
                    <p className="mt-1">
                      🛡️ {vinDecodeResult.safetyFeatures.length} safety features (Step 3)
                    </p>
                  )}
                  {vinDecodeResult.basePrice && <p className="mt-1">💰 MSRP suggestion (Step 4)</p>}
                </div>
              </div>
            )}

            {/* Error message */}
            {vinDecodeResult && !vinDecodeResult.success && (
              <p className="mt-2 text-sm text-red-600 flex items-center gap-1">
                <FiAlertCircle className="w-4 h-4" />
                {vinDecodeResult.errorMessage}
              </p>
            )}
          </div>

          {/* Backend Status Badge */}
          {usingFallback && (
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3 flex items-start gap-2">
              <FiInfo className="text-yellow-600 flex-shrink-0 mt-0.5" />
              <span className="text-xs md:text-sm text-yellow-800">
                Vehicle catalog is not available. Please enter specifications manually.
              </span>
            </div>
          )}

          {/* Auto-fill Notice */}
          {showAutoFillNotice && selectedTrim && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-start gap-2">
              <FiCheck className="text-green-600 flex-shrink-0 mt-0.5" />
              <span className="text-xs md:text-sm text-green-800">
                <strong>{selectedTrim.name}</strong> specs loaded!
                {selectedTrim.baseMSRP && ` ($${selectedTrim.baseMSRP.toLocaleString()})`}
              </span>
            </div>
          )}

          {/* Basic Info - Make / Model / Year / Trim */}
          <div className="grid grid-cols-2 md:grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
            {/* Make */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Make <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <select
                  {...register('make')}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                  disabled={loadingMakes}
                >
                  <option value="">Select make</option>
                  {makes.map((make) => (
                    <option key={make.id} value={make.name}>
                      {make.name}
                    </option>
                  ))}
                </select>
                {loadingMakes && (
                  <FiLoader className="absolute right-8 top-3 animate-spin text-gray-400" />
                )}
              </div>
              {errors.make && <p className="text-red-500 text-sm mt-1">{errors.make.message}</p>}
            </div>

            {/* Model */}
            <div>
              <Combobox
                label="Model"
                required
                options={models.map((model) => ({
                  value: model.name,
                  label: model.name,
                  sublabel: model.bodyStyle,
                }))}
                value={watchModel}
                onChange={(value) => setValue('model', value)}
                placeholder="Select or type model"
                disabled={loadingModels || !watchMake}
                loading={loadingModels}
                error={errors.model?.message}
                allowCustom={true}
              />
            </div>

            {/* Year */}
            <div>
              <Controller
                name="year"
                control={control}
                render={({ field }) => (
                  <Combobox
                    label="Year"
                    required
                    options={(availableYears.length > 0
                      ? availableYears
                      : Array.from({ length: 30 }, (_, i) => currentYear - 28 + i + 1).reverse()
                    ).map((year) => ({
                      value: String(year),
                      label: String(year),
                    }))}
                    value={String(field.value)}
                    onChange={(value) => field.onChange(Number(value))}
                    placeholder="Select or type year"
                    disabled={loadingYears}
                    loading={loadingYears}
                    error={errors.year?.message}
                    allowCustom={true}
                  />
                )}
              />
            </div>

            {/* Trim (optional - for auto-fill) */}
            <div>
              <Combobox
                label={trims.length > 0 ? 'Trim (auto-fill)' : 'Trim'}
                options={trims.map((trim) => ({
                  value: trim.name,
                  label: trim.name,
                  sublabel: trim.baseMSRP ? `$${trim.baseMSRP.toLocaleString()}` : undefined,
                }))}
                value={watchTrim}
                onChange={(value) => {
                  setValue('trim', value);
                  handleTrimSelect(value);
                }}
                placeholder="Select or type trim"
                disabled={loadingTrims}
                loading={loadingTrims}
                allowCustom={true}
              />
              {trims.length > 0 && trimViewMode === 'grid' && (
                <p className="text-xs text-gray-500 mt-1">Or select from cards below ↓</p>
              )}
            </div>
          </div>

          {/* Trim Selection Cards - Visual Grid */}
          {trims.length > 0 && (
            <div className="border border-blue-100 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg md:rounded-xl p-3 md:p-4">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-3 md:mb-4">
                <div>
                  <h3 className="text-base md:text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <FiInfo className="text-blue-600 w-4 h-4 md:w-5 md:h-5" />
                    Select a Trim Level
                  </h3>
                  <p className="text-xs md:text-sm text-gray-600 mt-0.5 md:mt-1">
                    Click to auto-fill specs
                  </p>
                </div>
                <div className="flex items-center gap-1">
                  <button
                    type="button"
                    onClick={() => setTrimViewMode('grid')}
                    className={`p-1.5 md:p-2 rounded-lg transition-colors ${
                      trimViewMode === 'grid'
                        ? 'bg-blue-600 text-white'
                        : 'bg-white text-gray-600 hover:bg-gray-100'
                    }`}
                    title="Grid view"
                  >
                    <FiGrid size={16} />
                  </button>
                  <button
                    type="button"
                    onClick={() => setTrimViewMode('dropdown')}
                    className={`p-1.5 md:p-2 rounded-lg transition-colors ${
                      trimViewMode === 'dropdown'
                        ? 'bg-blue-600 text-white'
                        : 'bg-white text-gray-600 hover:bg-gray-100'
                    }`}
                    title="Dropdown view"
                  >
                    <FiList size={16} />
                  </button>
                </div>
              </div>

              {trimViewMode === 'grid' && (
                <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-2 gap-2 md:gap-3 max-h-[400px] overflow-y-auto">
                  {trims.map((trim) => (
                    <TrimSelectionCard
                      key={trim.id}
                      trim={trim}
                      isSelected={selectedTrim?.id === trim.id}
                      onSelect={() => handleTrimSelect(trim.name)}
                    />
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Mileage and Condition */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 md:gap-4">
            <Input
              {...register('mileage', { valueAsNumber: true })}
              type="number"
              label="Mileage"
              placeholder="e.g., 25000"
              error={errors.mileage?.message}
              required
            />

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Condition <span className="text-red-500">*</span>
              </label>
              <select
                {...register('condition')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
              >
                <option value="">Select condition</option>
                {conditions.map((condition) => (
                  <option key={condition} value={condition}>
                    {condition}
                  </option>
                ))}
              </select>
              {errors.condition && (
                <p className="text-red-500 text-sm mt-1">{errors.condition.message}</p>
              )}
            </div>
          </div>

          {/* Technical Specs */}
          <div className="border-t pt-4 md:pt-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-2">
              <h3 className="text-base md:text-lg font-semibold text-gray-900">
                Technical Specifications
              </h3>
              {selectedTrim && (
                <span className="inline-flex items-center gap-1 text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full w-fit">
                  <FiCheck size={12} />
                  Auto-filled from {selectedTrim.name}
                </span>
              )}
            </div>
            <p className="text-xs md:text-sm text-gray-500 mb-3 md:mb-4">
              {selectedTrim
                ? 'Auto-filled from trim. You can modify or leave empty if data is not available.'
                : trims.length > 0
                  ? 'Select a trim to auto-fill specs, or enter manually.'
                  : 'Enter specifications manually or leave empty if not available. You can type custom values.'}
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 md:gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Transmission
                  {selectedTrim?.transmission && (
                    <span className="ml-1 text-xs text-green-600">✓</span>
                  )}
                </label>
                <Controller
                  name="transmission"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value}
                      onChange={field.onChange}
                      options={transmissions}
                      placeholder="Select or type transmission"
                      className={selectedTrim?.transmission ? 'border-green-300 bg-green-50' : ''}
                    />
                  )}
                />
                {errors.transmission && (
                  <p className="text-red-500 text-sm mt-1">{errors.transmission.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fuel Type
                  {selectedTrim?.fuelType && <span className="ml-1 text-xs text-green-600">✓</span>}
                </label>
                <Controller
                  name="fuelType"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value}
                      onChange={field.onChange}
                      options={fuelTypes}
                      placeholder="Select or type fuel type"
                      className={selectedTrim?.fuelType ? 'border-green-300 bg-green-50' : ''}
                    />
                  )}
                />
                {errors.fuelType && (
                  <p className="text-red-500 text-sm mt-1">{errors.fuelType.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Body Type</label>
                <Controller
                  name="bodyType"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value}
                      onChange={field.onChange}
                      options={bodyTypes}
                      placeholder="Select or type body type"
                    />
                  )}
                />
                {errors.bodyType && (
                  <p className="text-red-500 text-sm mt-1">{errors.bodyType.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Drivetrain
                  {selectedTrim?.driveType && (
                    <span className="ml-1 text-xs text-green-600">✓</span>
                  )}
                </label>
                <Controller
                  name="drivetrain"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value}
                      onChange={field.onChange}
                      options={drivetrains}
                      placeholder="Select or type drivetrain"
                      className={selectedTrim?.driveType ? 'border-green-300 bg-green-50' : ''}
                    />
                  )}
                />
                {errors.drivetrain && (
                  <p className="text-red-500 text-sm mt-1">{errors.drivetrain.message}</p>
                )}
              </div>

              <Input
                {...register('engine')}
                label="Engine"
                placeholder="e.g., 2.5L I4"
                error={errors.engine?.message}
              />

              <Input
                {...register('horsepower')}
                label="Horsepower"
                placeholder="e.g., 203 hp"
                error={errors.horsepower?.message}
              />

              {/* Doors */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Doors</label>
                <Controller
                  name="doors"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value?.toString() || ''}
                      onChange={(value) => {
                        const num = parseInt(value);
                        field.onChange(isNaN(num) ? undefined : num);
                      }}
                      options={doorOptions.map((d) => d.toString())}
                      placeholder="Select doors"
                    />
                  )}
                />
              </div>

              {/* Seats */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Seats</label>
                <Controller
                  name="seats"
                  control={control}
                  render={({ field }) => (
                    <Combobox
                      value={field.value?.toString() || ''}
                      onChange={(value) => {
                        const num = parseInt(value);
                        field.onChange(isNaN(num) ? undefined : num);
                      }}
                      options={seatOptions.map((s) => s.toString())}
                      placeholder="Select seats"
                    />
                  )}
                />
              </div>
            </div>
          </div>

          {/* Colors */}
          <div className="border-t pt-4 md:pt-6">
            <h3 className="text-base md:text-lg font-semibold text-gray-900 mb-3 md:mb-4">
              Colors
            </h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 md:gap-4">
              <Input
                {...register('exteriorColor')}
                label="Exterior Color"
                placeholder="e.g., Pearl White"
                error={errors.exteriorColor?.message}
                required
              />

              <Input
                {...register('interiorColor')}
                label="Interior Color"
                placeholder="e.g., Black Leather"
                error={errors.interiorColor?.message}
                required
              />
            </div>
          </div>

          {/* Actions */}
          <div className="flex flex-col-reverse sm:flex-row sm:justify-between items-stretch sm:items-center gap-3 pt-4 md:pt-6 border-t">
            <div className="text-xs md:text-sm text-gray-500 text-center sm:text-left">
              {selectedTrim && selectedTrim.baseMSRP && (
                <span>
                  Base MSRP: <strong>${selectedTrim.baseMSRP.toLocaleString()}</strong>
                </span>
              )}
            </div>
            <Button type="submit" variant="primary" size="lg" className="w-full sm:w-auto">
              Next: Upload Photos
            </Button>
          </div>
        </div>
        {/* End Main Form Column */}

        {/* Sidebar: Vehicle Preview Card (sticky on desktop, hidden on mobile) */}
        <div className="hidden xl:block xl:w-72 2xl:w-80 flex-shrink-0">
          <div className="sticky top-4 space-y-4">
            <VehiclePreviewCard
              data={{
                make: watchMake,
                model: watchModel,
                year: watchYear,
                trim: watchTrim,
                exteriorColor: watchExteriorColor,
                mileage: watchMileage,
                condition: watchCondition,
                transmission: watchTransmission,
                fuelType: watchFuelType,
                engine: watchEngine,
                horsepower: watchHorsepower,
                doors: watchDoors,
                seats: watchSeats,
                baseMSRP: selectedTrim?.baseMSRP,
              }}
            />

            {/* Quick tips */}
            <div className="bg-gray-50 rounded-lg p-3 border border-gray-200">
              <h4 className="font-medium text-gray-900 mb-2 flex items-center gap-2 text-sm">
                <FiAlertCircle className="text-blue-600 w-4 h-4" />
                Quick Tips
              </h4>
              <ul className="text-xs text-gray-600 space-y-1">
                <li>• Select a trim to auto-fill specs</li>
                <li>• All fields can be modified</li>
                <li>• Complete required fields to proceed</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      {/* End Main Layout */}
    </form>
  );
}
