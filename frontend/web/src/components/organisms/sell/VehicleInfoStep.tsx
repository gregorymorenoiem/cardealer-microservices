import { useState, useEffect, useCallback } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '@/components/atoms/Input';
import Button from '@/components/atoms/Button';
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
import { FiLoader, FiCheck, FiInfo, FiGrid, FiList, FiAlertCircle } from 'react-icons/fi';

// ============================================================
// FALLBACK DATA (used when backend is not available)
// ============================================================

const fallbackMakes = [
  'Toyota', 'Honda', 'Ford', 'Chevrolet', 'Nissan', 'Hyundai', 'Kia',
  'BMW', 'Mercedes-Benz', 'Audi', 'Volkswagen', 'Mazda', 'Subaru',
  'Lexus', 'Jeep', 'Tesla', 'GMC', 'Ram', 'Dodge', 'Cadillac'
];

const transmissions = ['Automatic', 'Manual', 'CVT', 'Semi-Automatic', 'Dual-Clutch'];
const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'Plug-in Hybrid'];
const bodyTypes = ['Sedan', 'SUV', 'Truck', 'Coupe', 'Hatchback', 'Van', 'Convertible', 'Wagon', 'Crossover', 'Minivan'];
const drivetrains = ['FWD', 'RWD', 'AWD', '4WD'];
const conditions = ['New', 'Used', 'Certified Pre-Owned'];

const commonFeatures = [
  'Bluetooth', 'Backup Camera', 'Navigation System', 'Sunroof', 'Leather Seats',
  'Heated Seats', 'Ventilated Seats', 'Remote Start', 'Keyless Entry', 'Apple CarPlay',
  'Android Auto', 'Parking Sensors', 'Blind Spot Monitor', 'Lane Departure Warning',
  'Adaptive Cruise Control', 'Premium Audio', '360° Camera', 'Head-Up Display',
];

// ============================================================
// VALIDATION SCHEMA
// ============================================================

const vehicleInfoSchema = z.object({
  make: z.string().min(1, 'Make is required'),
  model: z.string().min(1, 'Model is required'),
  year: z.number().min(1900, 'Invalid year').max(new Date().getFullYear() + 1, 'Invalid year'),
  trim: z.string().optional(),
  mileage: z.number().min(0, 'Mileage must be positive'),
  vin: z.string().min(1, 'VIN is required').max(17, 'VIN must be at most 17 characters'),
  transmission: z.string().min(1, 'Transmission is required'),
  fuelType: z.string().min(1, 'Fuel type is required'),
  bodyType: z.string().min(1, 'Body type is required'),
  drivetrain: z.string().min(1, 'Drivetrain is required'),
  engine: z.string().min(1, 'Engine is required'),
  horsepower: z.string().optional(),
  mpg: z.string().optional(),
  exteriorColor: z.string().min(1, 'Exterior color is required'),
  interiorColor: z.string().min(1, 'Interior color is required'),
  condition: z.string().min(1, 'Condition is required'),
  features: z.array(z.string()),
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

  // ========================================
  // FORM
  // ========================================
  const {
    register,
    handleSubmit,
    watch,
    setValue,
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
      mpg: data.mpg || '',
      exteriorColor: data.exteriorColor || '',
      interiorColor: data.interiorColor || '',
      condition: data.condition || '',
      features: data.features || [],
    },
  });

  const selectedFeatures = watch('features');
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
  const watchMpg = watch('mpg');

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
          setMakes(fallbackMakes.map(name => ({
            id: createSlug(name),
            name,
            slug: createSlug(name),
            isPopular: true,
          })));
          setUsingFallback(true);
        }
      } catch (error) {
        console.error('Error loading makes:', error);
        // Use fallback
        setMakes(fallbackMakes.map(name => ({
          id: createSlug(name),
          name,
          slug: createSlug(name),
          isPopular: true,
        })));
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

      const make = makes.find(m => m.name === watchMake || m.slug === watchMake);
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

      const model = models.find(m => m.name === watchModel || m.slug === watchModel);
      setSelectedModel(model || null);

      if (!model || usingFallback) {
        // Generate default years
        setAvailableYears(Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse());
        return;
      }

      setLoadingYears(true);
      try {
        const yearsData = await getAvailableYears(model.id);
        setAvailableYears(yearsData.length > 0 ? yearsData : 
          Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse());
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
  const handleTrimSelect = useCallback((trimName: string) => {
    const trim = trims.find(t => t.name === trimName);
    
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
  }, [trims, setValue, selectedModel, watch]);

  useEffect(() => {
    if (watchTrim) {
      handleTrimSelect(watchTrim);
    }
  }, [watchTrim, handleTrimSelect]);

  // ========================================
  // HELPERS
  // ========================================
  function mapFuelType(value: string): string {
    const mapping: Record<string, string> = {
      'Gasoline': 'Gasoline',
      'Diesel': 'Diesel', 
      'Electric': 'Electric',
      'Hybrid': 'Hybrid',
      'PlugInHybrid': 'Plug-in Hybrid',
    };
    return mapping[value] || value;
  }

  function mapTransmission(value: string): string {
    const mapping: Record<string, string> = {
      'Automatic': 'Automatic',
      'Manual': 'Manual',
      'CVT': 'CVT',
      'DualClutch': 'Dual-Clutch',
      'Automated': 'Semi-Automatic',
    };
    return mapping[value] || value;
  }

  function mapDriveType(value: string): string {
    const mapping: Record<string, string> = {
      'FWD': 'FWD',
      'RWD': 'RWD',
      'AWD': 'AWD',
      'FourWD': '4WD',
    };
    return mapping[value] || value;
  }

  function mapBodyStyle(value: string): string {
    // Map model body styles to form body types
    const mapping: Record<string, string> = {
      'Sedan': 'Sedan',
      'SUV': 'SUV',
      'Crossover': 'Crossover',
      'Truck': 'Truck',
      'Pickup': 'Truck',
      'Coupe': 'Coupe',
      'Hatchback': 'Hatchback',
      'Van': 'Van',
      'Minivan': 'Minivan',
      'Wagon': 'Wagon',
      'Convertible': 'Convertible',
      'Sports': 'Coupe',
      'SportsCar': 'Coupe',
    };
    return mapping[value] || value;
  }

  const toggleFeature = (feature: string) => {
    const current = selectedFeatures || [];
    if (current.includes(feature)) {
      setValue('features', current.filter((f) => f !== feature));
    } else {
      setValue('features', [...current, feature]);
    }
  };

  const onSubmit = (formData: VehicleInfoFormData) => {
    console.log('Form submitted:', formData);
    console.log('Validation errors:', errors);
    
    // Add IDs from catalog
    const enrichedData = {
      ...formData,
      makeId: selectedMake?.id,
      modelId: selectedModel?.id,
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
          {(watchMake && watchModel) && (
            <div className="bg-gradient-to-r from-primary to-primary-dark p-3 rounded-lg text-white">
              <p className="font-semibold text-sm truncate">
                {watchYear} {watchMake} {watchModel} {watchTrim && `• ${watchTrim}`}
              </p>
              {selectedTrim?.baseMSRP && (
                <p className="text-xs text-white/80">MSRP: ${selectedTrim.baseMSRP.toLocaleString()}</p>
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
              Select make, model, year and trim to auto-fill specifications.
            </p>
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
                <option key={make.id} value={make.name}>{make.name}</option>
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
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Model <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            {models.length > 0 ? (
              <select
                {...register('model')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                disabled={loadingModels || !watchMake}
              >
                <option value="">Select model</option>
                {models.map((model) => (
                  <option key={model.id} value={model.name}>{model.name}</option>
                ))}
              </select>
            ) : (
              <Input
                {...register('model')}
                placeholder="e.g., Camry, Civic, F-150"
                disabled={!watchMake}
              />
            )}
            {loadingModels && (
              <FiLoader className="absolute right-8 top-3 animate-spin text-gray-400" />
            )}
          </div>
          {errors.model && <p className="text-red-500 text-sm mt-1">{errors.model.message}</p>}
        </div>

        {/* Year */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Year <span className="text-red-500">*</span>
          </label>
          <div className="relative">
            <select
              {...register('year', { valueAsNumber: true })}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
              disabled={loadingYears}
            >
              {(availableYears.length > 0 ? availableYears : 
                Array.from({ length: 11 }, (_, i) => currentYear - 10 + i + 1).reverse()
              ).map((year) => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
            {loadingYears && (
              <FiLoader className="absolute right-8 top-3 animate-spin text-gray-400" />
            )}
          </div>
          {errors.year && <p className="text-red-500 text-sm mt-1">{errors.year.message}</p>}
        </div>

        {/* Trim (optional - for auto-fill) */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Trim {trims.length > 0 && <span className="text-xs text-green-600">(auto-fill)</span>}
          </label>
          <div className="relative">
            {/* Dropdown fallback when no trims or using compact mode */}
            {trims.length === 0 ? (
              <Input
                {...register('trim')}
                placeholder="e.g., LE, Sport, Limited"
              />
            ) : trimViewMode === 'dropdown' ? (
              <select
                {...register('trim')}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
                disabled={loadingTrims}
              >
                <option value="">Select trim</option>
                {trims.map((trim) => (
                  <option key={trim.id} value={trim.name}>
                    {trim.name} {trim.baseMSRP && `($${trim.baseMSRP.toLocaleString()})`}
                  </option>
                ))}
              </select>
            ) : (
              <div className="text-sm text-gray-500 italic">
                {selectedTrim ? selectedTrim.name : 'Select from cards below ↓'}
              </div>
            )}
            {loadingTrims && (
              <FiLoader className="absolute right-8 top-3 animate-spin text-gray-400" />
            )}
          </div>
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

          {/* Mileage, VIN, Condition */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 md:gap-4">
        <Input
          {...register('mileage', { valueAsNumber: true })}
          type="number"
          label="Mileage"
          placeholder="e.g., 25000"
          error={errors.mileage?.message}
          required
        />

        <Input
          {...register('vin')}
          label="VIN"
          placeholder="17-character VIN"
          error={errors.vin?.message}
          maxLength={17}
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
              <option key={condition} value={condition}>{condition}</option>
            ))}
          </select>
          {errors.condition && <p className="text-red-500 text-sm mt-1">{errors.condition.message}</p>}
        </div>
      </div>

          {/* Technical Specs */}
          <div className="border-t pt-4 md:pt-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-2">
              <h3 className="text-base md:text-lg font-semibold text-gray-900">Technical Specifications</h3>
              {selectedTrim && (
                <span className="inline-flex items-center gap-1 text-xs bg-green-100 text-green-700 px-2 py-1 rounded-full w-fit">
                  <FiCheck size={12} />
                  Auto-filled from {selectedTrim.name}
                </span>
              )}
            </div>
            <p className="text-xs md:text-sm text-gray-500 mb-3 md:mb-4">
              {selectedTrim 
                ? 'Auto-filled from trim. You can modify as needed.'
                : trims.length > 0 
                  ? 'Select a trim to auto-fill specs.'
                  : 'Enter specifications manually.'}
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3 md:gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Transmission <span className="text-red-500">*</span>
              {selectedTrim?.transmission && <span className="ml-1 text-xs text-green-600">✓</span>}
            </label>
            <select
              {...register('transmission')}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent ${
                selectedTrim?.transmission ? 'border-green-300 bg-green-50' : 'border-gray-300'
              }`}
            >
              <option value="">Select transmission</option>
              {transmissions.map((trans) => (
                <option key={trans} value={trans}>{trans}</option>
              ))}
            </select>
            {errors.transmission && <p className="text-red-500 text-sm mt-1">{errors.transmission.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Fuel Type <span className="text-red-500">*</span>
              {selectedTrim?.fuelType && <span className="ml-1 text-xs text-green-600">✓</span>}
            </label>
            <select
              {...register('fuelType')}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent ${
                selectedTrim?.fuelType ? 'border-green-300 bg-green-50' : 'border-gray-300'
              }`}
            >
              <option value="">Select fuel type</option>
              {fuelTypes.map((fuel) => (
                <option key={fuel} value={fuel}>{fuel}</option>
              ))}
            </select>
            {errors.fuelType && <p className="text-red-500 text-sm mt-1">{errors.fuelType.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Body Type <span className="text-red-500">*</span>
            </label>
            <select
              {...register('bodyType')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
            >
              <option value="">Select body type</option>
              {bodyTypes.map((body) => (
                <option key={body} value={body}>{body}</option>
              ))}
            </select>
            {errors.bodyType && <p className="text-red-500 text-sm mt-1">{errors.bodyType.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Drivetrain <span className="text-red-500">*</span>
              {selectedTrim?.driveType && <span className="ml-1 text-xs text-green-600">✓</span>}
            </label>
            <select
              {...register('drivetrain')}
              className={`w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent ${
                selectedTrim?.driveType ? 'border-green-300 bg-green-50' : 'border-gray-300'
              }`}
            >
              <option value="">Select drivetrain</option>
              {drivetrains.map((drive) => (
                <option key={drive} value={drive}>{drive}</option>
              ))}
            </select>
            {errors.drivetrain && <p className="text-red-500 text-sm mt-1">{errors.drivetrain.message}</p>}
          </div>

          <Input
            {...register('engine')}
            label="Engine"
            placeholder="e.g., 2.5L I4"
            error={errors.engine?.message}
            required
          />

          <Input
            {...register('horsepower')}
            label="Horsepower"
            placeholder="e.g., 203 hp"
            error={errors.horsepower?.message}
          />

          <Input
            {...register('mpg')}
            label="MPG"
            placeholder="e.g., 28 city / 39 hwy"
            error={errors.mpg?.message}
          />
        </div>
      </div>

          {/* Colors */}
          <div className="border-t pt-4 md:pt-6">
            <h3 className="text-base md:text-lg font-semibold text-gray-900 mb-3 md:mb-4">Colors</h3>
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

          {/* Features */}
          <div className="border-t pt-4 md:pt-6">
            <h3 className="text-base md:text-lg font-semibold text-gray-900 mb-2 md:mb-4">Features</h3>
            <p className="text-xs md:text-sm text-gray-600 mb-3 md:mb-4">
              Select all features that apply
            </p>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-2 md:gap-3">
              {commonFeatures.map((feature) => (
                <button
                  key={feature}
                  type="button"
                  onClick={() => toggleFeature(feature)}
                  className={`
                    px-2 md:px-4 py-1.5 md:py-2 rounded-lg text-xs md:text-sm font-medium transition-all border-2
                    ${
                  selectedFeatures?.includes(feature)
                    ? 'bg-primary text-white border-primary'
                    : 'bg-white text-gray-700 border-gray-300 hover:border-primary'
                }
              `}
            >
              {feature}
            </button>
          ))}
        </div>
      </div>

          {/* Actions */}
          <div className="flex flex-col-reverse sm:flex-row sm:justify-between items-stretch sm:items-center gap-3 pt-4 md:pt-6 border-t">
            <div className="text-xs md:text-sm text-gray-500 text-center sm:text-left">
              {selectedTrim && selectedTrim.baseMSRP && (
                <span>Base MSRP: <strong>${selectedTrim.baseMSRP.toLocaleString()}</strong></span>
              )}
            </div>
            <Button type="submit" variant="primary" size="lg" className="w-full sm:w-auto">
            Next: Upload Photos
          </Button>
        </div>
        </div>{/* End Main Form Column */}

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
                mpg: watchMpg,
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
      </div>{/* End Main Layout */}
    </form>
  );
}
