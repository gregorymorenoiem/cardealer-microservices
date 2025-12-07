import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import Input from '@/components/atoms/Input';
import Button from '@/components/atoms/Button';
import type { VehicleFormData } from '@/pages/SellYourCarPage';

const makes = ['Tesla', 'BMW', 'Toyota', 'Ford', 'Honda', 'Audi', 'Mercedes-Benz', 'Chevrolet', 'Mazda', 'Volkswagen', 'Nissan', 'Hyundai', 'Kia', 'Jeep', 'Subaru'];
const transmissions = ['Automatic', 'Manual', 'CVT', 'Semi-Automatic'];
const fuelTypes = ['Gasoline', 'Diesel', 'Electric', 'Hybrid', 'Plug-in Hybrid'];
const bodyTypes = ['Sedan', 'SUV', 'Truck', 'Coupe', 'Hatchback', 'Van', 'Convertible', 'Wagon'];
const drivetrains = ['FWD', 'RWD', 'AWD', '4WD'];
const conditions = ['New', 'Used', 'Certified Pre-Owned'];

const commonFeatures = [
  'Bluetooth', 'Backup Camera', 'Navigation System', 'Sunroof', 'Leather Seats',
  'Heated Seats', 'Ventilated Seats', 'Remote Start', 'Keyless Entry', 'Apple CarPlay',
  'Android Auto', 'Parking Sensors', 'Blind Spot Monitor', 'Lane Departure Warning',
  'Adaptive Cruise Control', 'Premium Audio', '360° Camera', 'Head-Up Display',
];

const vehicleInfoSchema = z.object({
  make: z.string().min(1, 'Make is required'),
  model: z.string().min(1, 'Model is required'),
  year: z.number().min(1900, 'Invalid year').max(new Date().getFullYear() + 1, 'Invalid year'),
  mileage: z.number().min(0, 'Mileage must be positive'),
  vin: z.string().length(17, 'VIN must be 17 characters'),
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
});

type VehicleInfoFormData = z.infer<typeof vehicleInfoSchema>;

interface VehicleInfoStepProps {
  data: Partial<VehicleFormData>;
  onNext: (data: Partial<VehicleFormData>) => void;
  onBack: () => void;
}

export default function VehicleInfoStep({ data, onNext }: VehicleInfoStepProps) {
  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 30 }, (_, i) => currentYear + 1 - i);

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

  const toggleFeature = (feature: string) => {
    const current = selectedFeatures || [];
    if (current.includes(feature)) {
      setValue('features', current.filter((f) => f !== feature));
    } else {
      setValue('features', [...current, feature]);
    }
  };

  const onSubmit = (formData: VehicleInfoFormData) => {
    onNext(formData);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold font-heading text-gray-900 mb-2">
          Vehicle Information
        </h2>
        <p className="text-gray-600">
          Tell us about your vehicle's basic details
        </p>
      </div>

      {/* Basic Info */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Make <span className="text-red-500">*</span>
          </label>
          <select
            {...register('make')}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
          >
            <option value="">Select make</option>
            {makes.map((make) => (
              <option key={make} value={make}>{make}</option>
            ))}
          </select>
          {errors.make && <p className="text-red-500 text-sm mt-1">{errors.make.message}</p>}
        </div>

        <Input
          {...register('model')}
          label="Model"
          placeholder="e.g., Model 3, 3 Series, Camry"
          error={errors.model?.message}
          required
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Year <span className="text-red-500">*</span>
          </label>
          <select
            {...register('year', { valueAsNumber: true })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
          >
            {years.map((year) => (
              <option key={year} value={year}>{year}</option>
            ))}
          </select>
          {errors.year && <p className="text-red-500 text-sm mt-1">{errors.year.message}</p>}
        </div>

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
      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Technical Specifications</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Transmission <span className="text-red-500">*</span>
            </label>
            <select
              {...register('transmission')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
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
            </label>
            <select
              {...register('fuelType')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
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
            </label>
            <select
              {...register('drivetrain')}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
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
            placeholder="e.g., 2.0L Turbo I4"
            error={errors.engine?.message}
            required
          />

          <Input
            {...register('horsepower')}
            label="Horsepower"
            placeholder="e.g., 250 hp"
            error={errors.horsepower?.message}
          />

          <Input
            {...register('mpg')}
            label="MPG"
            placeholder="e.g., 28 city / 35 hwy"
            error={errors.mpg?.message}
          />
        </div>
      </div>

      {/* Colors */}
      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Colors</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Features</h3>
        <p className="text-sm text-gray-600 mb-4">
          Select all features that apply to your vehicle
        </p>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
          {commonFeatures.map((feature) => (
            <button
              key={feature}
              type="button"
              onClick={() => toggleFeature(feature)}
              className={`
                px-4 py-2 rounded-lg text-sm font-medium transition-all border-2
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
      <div className="flex justify-between pt-6 border-t">
        <div></div>
        <Button type="submit" variant="primary" size="lg">
          Next: Upload Photos
        </Button>
      </div>
    </form>
  );
}
