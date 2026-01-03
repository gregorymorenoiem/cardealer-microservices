import type { Vehicle } from '@/data/mockVehicles';
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
} from 'react-icons/fi';

interface VehicleSpecsProps {
  vehicle: Vehicle;
}

export default function VehicleSpecs({ vehicle }: VehicleSpecsProps) {
  const specs = [
    { icon: FiCalendar, label: 'Year', value: vehicle.year.toString() },
    { icon: FiActivity, label: 'Mileage', value: formatMileage(vehicle.mileage) },
    { icon: FiCreditCard, label: 'Price', value: formatPrice(vehicle.price), highlight: true },
    { icon: FiSettings, label: 'Transmission', value: vehicle.transmission },
    { icon: FiDroplet, label: 'Fuel Type', value: vehicle.fuelType },
    { icon: FiTruck, label: 'Body Type', value: vehicle.bodyType },
    { icon: FiDisc, label: 'Drivetrain', value: vehicle.drivetrain },
    { icon: FiZap, label: 'Engine', value: vehicle.engine },
    { icon: FiTrendingUp, label: 'Horsepower', value: `${vehicle.horsepower} HP` },
    { icon: FiActivity, label: 'MPG', value: `${vehicle.mpg.city} city / ${vehicle.mpg.highway} hwy` },
    { icon: FiCircle, label: 'Exterior Color', value: vehicle.color },
    { icon: FiCircle, label: 'Interior Color', value: vehicle.interiorColor },
    { icon: FiHash, label: 'VIN', value: vehicle.vin },
    { icon: FiAward, label: 'Condition', value: vehicle.condition },
  ];

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <h2 className="text-2xl font-bold font-heading text-gray-900 mb-6">
        Specifications
      </h2>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {specs.map((spec, index) => (
          <div
            key={index}
            className={`
              flex items-start gap-3 p-4 rounded-lg transition-colors duration-200
              ${spec.highlight ? 'bg-primary/5 border-2 border-primary' : 'bg-gray-50 hover:bg-gray-100'}
            `}
          >
            <div className={`
              p-2 rounded-lg flex-shrink-0
              ${spec.highlight ? 'bg-primary text-white' : 'bg-white text-primary'}
            `}>
              <spec.icon size={20} />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm text-gray-600 mb-1">{spec.label}</p>
              <p className={`
                font-semibold truncate
                ${spec.highlight ? 'text-primary text-lg' : 'text-gray-900'}
              `}>
                {spec.value}
              </p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
