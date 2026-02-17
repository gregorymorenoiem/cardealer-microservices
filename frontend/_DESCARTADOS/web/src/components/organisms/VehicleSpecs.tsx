import type { Vehicle } from '@/services/vehicleService';
import type { Vehicle as ApiVehicle } from '@/services/vehicleService';
import { formatPrice, formatMileage } from '@/utils/formatters';
import {
  FiCalendar,
  FiActivity,
  FiCreditCard,
  FiSettings,
  FiDroplet,
  FiTruck,
  FiDisc,
  FiZap,
  FiTrendingUp,
  FiCircle,
  FiHash,
  FiAward,
  FiUsers,
  FiLayers,
  FiTag,
} from 'react-icons/fi';

interface VehicleSpecsProps {
  vehicle: Vehicle | ApiVehicle;
}

// Helper function to check if a value is valid (not null, undefined, empty, or placeholder)
const isValidValue = (value: unknown): boolean => {
  if (value === null || value === undefined) return false;
  if (typeof value === 'string') {
    const trimmed = value.trim().toLowerCase();
    return trimmed !== '' && trimmed !== 'n/a' && trimmed !== 'unknown' && trimmed !== '0';
  }
  if (typeof value === 'number') return value > 0;
  return true;
};

// Helper function to format MPG if valid
const formatMpg = (mpg?: { city: number; highway: number }): string | null => {
  if (!mpg || (mpg.city === 0 && mpg.highway === 0)) return null;
  if (!mpg.city && !mpg.highway) return null;
  return `${mpg.city} city / ${mpg.highway} hwy`;
};

export default function VehicleSpecs({ vehicle }: VehicleSpecsProps) {
  // Build specs array only with valid values
  const allSpecs = [
    { icon: FiCalendar, label: 'Year', value: vehicle.year?.toString(), highlight: false },
    {
      icon: FiActivity,
      label: 'Mileage',
      value: vehicle.mileage ? formatMileage(vehicle.mileage) : null,
      highlight: false,
    },
    {
      icon: FiCreditCard,
      label: 'Price',
      value: vehicle.price ? formatPrice(vehicle.price) : null,
      highlight: true,
    },
    { icon: FiSettings, label: 'Transmission', value: vehicle.transmission, highlight: false },
    { icon: FiDroplet, label: 'Fuel Type', value: vehicle.fuelType, highlight: false },
    { icon: FiTruck, label: 'Body Type', value: vehicle.bodyType, highlight: false },
    { icon: FiDisc, label: 'Drivetrain', value: vehicle.drivetrain, highlight: false },
    { icon: FiZap, label: 'Engine', value: vehicle.engine, highlight: false },
    {
      icon: FiTrendingUp,
      label: 'Horsepower',
      value: vehicle.horsepower ? `${vehicle.horsepower} HP` : null,
      highlight: false,
    },
    { icon: FiActivity, label: 'MPG', value: formatMpg(vehicle.mpg), highlight: false },
    { icon: FiCircle, label: 'Exterior Color', value: vehicle.color, highlight: false },
    { icon: FiCircle, label: 'Interior Color', value: vehicle.interiorColor, highlight: false },
    { icon: FiTag, label: 'Trim', value: (vehicle as ApiVehicle).trim, highlight: false },
    {
      icon: FiLayers,
      label: 'Doors',
      value: (vehicle as ApiVehicle).doors ? `${(vehicle as ApiVehicle).doors} doors` : null,
      highlight: false,
    },
    {
      icon: FiUsers,
      label: 'Seats',
      value: (vehicle as ApiVehicle).seats ? `${(vehicle as ApiVehicle).seats} seats` : null,
      highlight: false,
    },
    { icon: FiHash, label: 'VIN', value: vehicle.vin, highlight: false },
    { icon: FiAward, label: 'Condition', value: vehicle.condition, highlight: false },
  ];

  // Filter out specs with invalid values
  const specs = allSpecs.filter((spec) => isValidValue(spec.value));

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <h2 className="text-2xl font-bold font-heading text-gray-900 mb-6">Specifications</h2>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {specs.map((spec, index) => (
          <div
            key={index}
            className={`
              flex items-start gap-3 p-4 rounded-lg transition-colors duration-200
              ${spec.highlight ? 'bg-primary/5 border-2 border-primary' : 'bg-gray-50 hover:bg-gray-100'}
            `}
          >
            <div
              className={`
              p-2 rounded-lg flex-shrink-0
              ${spec.highlight ? 'bg-primary text-white' : 'bg-white text-primary'}
            `}
            >
              <spec.icon size={20} />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm text-gray-600 mb-1">{spec.label}</p>
              <p
                className={`
                font-semibold truncate
                ${spec.highlight ? 'text-primary text-lg' : 'text-gray-900'}
              `}
              >
                {spec.value}
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
