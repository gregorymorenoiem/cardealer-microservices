/**
 * VehiclePreviewCard - Muestra un preview en tiempo real del vehículo
 * que el usuario está configurando
 */
import { FiCalendar, FiActivity, FiDroplet, FiZap, FiSettings, FiUsers } from 'react-icons/fi';

interface VehiclePreviewData {
  make?: string;
  model?: string;
  year?: number;
  trim?: string;
  mileage?: number;
  exteriorColor?: string;
  transmission?: string;
  fuelType?: string;
  engine?: string;
  horsepower?: string;
  doors?: number;
  seats?: number;
  condition?: string;
  baseMSRP?: number;
}

interface VehiclePreviewCardProps {
  data: VehiclePreviewData;
  className?: string;
}

export default function VehiclePreviewCard({ data, className = '' }: VehiclePreviewCardProps) {
  const hasBasicInfo = data.make && data.model && data.year;

  if (!hasBasicInfo) {
    return (
      <div
        className={`bg-gray-50 border-2 border-dashed border-gray-200 rounded-xl p-6 text-center ${className}`}
      >
        <div className="text-gray-400">
          <FiCalendar className="w-12 h-12 mx-auto mb-3 opacity-50" />
          <p className="text-sm">Select make, model, and year to see your vehicle preview</p>
        </div>
      </div>
    );
  }

  const title = `${data.year} ${data.make} ${data.model}${data.trim ? ` ${data.trim}` : ''}`;

  return (
    <div
      className={`bg-white border border-gray-200 rounded-xl overflow-hidden shadow-sm ${className}`}
    >
      {/* Header with gradient */}
      <div className="bg-gradient-to-r from-primary to-primary-dark p-4 text-white">
        <h3 className="font-bold text-lg truncate">{title}</h3>
        {data.condition && (
          <span className="inline-block mt-1 px-2 py-0.5 bg-white/20 rounded text-xs font-medium">
            {data.condition}
          </span>
        )}
      </div>

      {/* Specs */}
      <div className="p-4 space-y-3">
        {/* Color */}
        {data.exteriorColor && (
          <div className="flex items-center gap-3 text-sm">
            <div
              className="w-5 h-5 rounded-full border-2 border-gray-200"
              style={{ backgroundColor: getColorHex(data.exteriorColor) }}
            />
            <span className="text-gray-700">{data.exteriorColor}</span>
          </div>
        )}

        {/* Mileage */}
        {data.mileage !== undefined && data.mileage > 0 && (
          <div className="flex items-center gap-3 text-sm">
            <FiActivity className="w-5 h-5 text-gray-400" />
            <span className="text-gray-700">{data.mileage.toLocaleString()} miles</span>
          </div>
        )}

        {/* Engine & Power */}
        {(data.engine || data.horsepower) && (
          <div className="flex items-center gap-3 text-sm">
            <FiZap className="w-5 h-5 text-yellow-500" />
            <span className="text-gray-700">
              {data.engine}
              {data.horsepower && ` • ${data.horsepower}`}
            </span>
          </div>
        )}

        {/* Transmission */}
        {data.transmission && (
          <div className="flex items-center gap-3 text-sm">
            <FiSettings className="w-5 h-5 text-gray-400" />
            <span className="text-gray-700">{data.transmission}</span>
          </div>
        )}

        {/* Fuel Type */}
        {data.fuelType && (
          <div className="flex items-center gap-3 text-sm">
            <FiDroplet className="w-5 h-5 text-blue-500" />
            <span className="text-gray-700">{data.fuelType}</span>
          </div>
        )}

        {/* Doors & Seats */}
        {(data.doors || data.seats) && (
          <div className="flex items-center gap-3 text-sm">
            <FiUsers className="w-5 h-5 text-gray-400" />
            <span className="text-gray-700">
              {data.doors && `${data.doors} doors`}
              {data.doors && data.seats && ' • '}
              {data.seats && `${data.seats} seats`}
            </span>
          </div>
        )}
      </div>

      {/* MSRP Footer */}
      {data.baseMSRP && (
        <div className="px-4 py-3 bg-gray-50 border-t border-gray-100">
          <div className="flex items-center justify-between">
            <span className="text-xs text-gray-500 uppercase tracking-wide">Original MSRP</span>
            <span className="font-bold text-gray-900">${data.baseMSRP.toLocaleString()}</span>
          </div>
        </div>
      )}

      {/* Completion Progress */}
      <div className="px-4 py-2 bg-gray-50 border-t border-gray-100">
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs text-gray-500">Form completion</span>
          <span className="text-xs font-medium text-primary">{calculateCompletion(data)}%</span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-1.5">
          <div
            className="bg-primary h-1.5 rounded-full transition-all duration-300"
            style={{ width: `${calculateCompletion(data)}%` }}
          />
        </div>
      </div>
    </div>
  );
}

function calculateCompletion(data: VehiclePreviewData): number {
  const fields = [
    'make',
    'model',
    'year',
    'trim',
    'mileage',
    'exteriorColor',
    'transmission',
    'fuelType',
    'engine',
    'condition',
  ];
  const filled = fields.filter((f) => {
    const value = data[f as keyof VehiclePreviewData];
    return value !== undefined && value !== '' && value !== 0;
  }).length;
  return Math.round((filled / fields.length) * 100);
}

function getColorHex(colorName: string): string {
  const colors: Record<string, string> = {
    white: '#FFFFFF',
    black: '#1a1a1a',
    silver: '#C0C0C0',
    gray: '#808080',
    grey: '#808080',
    red: '#DC2626',
    blue: '#2563EB',
    green: '#16A34A',
    yellow: '#EAB308',
    orange: '#EA580C',
    brown: '#78350F',
    beige: '#D4B896',
    gold: '#B8860B',
    pearl: '#F0EAD6',
    midnight: '#191970',
    navy: '#000080',
    burgundy: '#800020',
    champagne: '#F7E7CE',
    bronze: '#CD7F32',
    copper: '#B87333',
  };

  const lowerColor = colorName.toLowerCase();
  for (const [name, hex] of Object.entries(colors)) {
    if (lowerColor.includes(name)) return hex;
  }
  return '#94A3B8'; // Default gray
}
