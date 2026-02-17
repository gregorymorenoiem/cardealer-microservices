/**
 * TrimSelectionCard - Muestra un trim como una card seleccionable
 * con todas sus especificaciones visibles
 */
import { FiCheck, FiZap, FiDroplet, FiSettings, FiDollarSign } from 'react-icons/fi';
import type { VehicleTrim } from '@/services/vehicleCatalogService';

interface TrimSelectionCardProps {
  trim: VehicleTrim;
  isSelected: boolean;
  onSelect: (trim: VehicleTrim) => void;
}

export default function TrimSelectionCard({ trim, isSelected, onSelect }: TrimSelectionCardProps) {
  return (
    <button
      type="button"
      onClick={() => onSelect(trim)}
      className={`
        w-full text-left p-3 sm:p-4 rounded-xl border-2 transition-all duration-200
        ${isSelected 
          ? 'border-primary bg-primary/5 shadow-lg shadow-primary/20' 
          : 'border-gray-200 bg-white hover:border-gray-300 hover:shadow-md'
        }
      `}
    >
      {/* Header */}
      <div className="flex items-center justify-between mb-2 sm:mb-3 gap-2">
        <div className="min-w-0 flex-1">
          <h4 className={`font-semibold text-base sm:text-lg truncate ${isSelected ? 'text-primary' : 'text-gray-900'}`}>
            {trim.name}
          </h4>
          {trim.baseMSRP && (
            <p className="text-xs sm:text-sm text-gray-500 flex items-center gap-1">
              <FiDollarSign className="w-3 h-3 sm:w-3.5 sm:h-3.5 flex-shrink-0" />
              <span>${trim.baseMSRP.toLocaleString()}</span>
            </p>
          )}
        </div>
        {isSelected && (
          <div className="w-6 h-6 sm:w-8 sm:h-8 rounded-full bg-primary flex items-center justify-center flex-shrink-0">
            <FiCheck className="w-4 h-4 sm:w-5 sm:h-5 text-white" />
          </div>
        )}
      </div>

      {/* Specs Grid */}
      <div className="grid grid-cols-2 gap-1.5 sm:gap-2 text-xs sm:text-sm">
        {/* Engine */}
        {trim.engineSize && (
          <div className="flex items-center gap-1.5 sm:gap-2 text-gray-600">
            <FiSettings className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-gray-400 flex-shrink-0" />
            <span className="truncate">{trim.engineSize}</span>
          </div>
        )}

        {/* Power */}
        {trim.horsepower && (
          <div className="flex items-center gap-1.5 sm:gap-2 text-gray-600">
            <FiZap className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-yellow-500 flex-shrink-0" />
            <span>{trim.horsepower} HP</span>
          </div>
        )}

        {/* Transmission */}
        {trim.transmission && (
          <div className="flex items-center gap-1.5 sm:gap-2 text-gray-600">
            <span className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-xs font-bold text-gray-400 flex-shrink-0">⚙</span>
            <span className="truncate">{trim.transmission}</span>
          </div>
        )}

        {/* Drive Type */}
        {trim.driveType && (
          <div className="flex items-center gap-1.5 sm:gap-2 text-gray-600">
            <span className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-xs font-bold text-gray-400 flex-shrink-0">🚗</span>
            <span>{trim.driveType}</span>
          </div>
        )}

        {/* Fuel Economy */}
        {trim.mpgCombined && (
          <div className="flex items-center gap-1.5 sm:gap-2 text-gray-600 col-span-2">
            <FiDroplet className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-blue-500 flex-shrink-0" />
            <span>
              {trim.mpgCity}/{trim.mpgHighway} MPG 
              <span className="text-gray-400 text-xs ml-1 hidden sm:inline">(city/hwy)</span>
            </span>
          </div>
        )}
      </div>

      {/* Selection Hint */}
      {!isSelected && (
        <p className="text-xs text-gray-400 mt-2 sm:mt-3 text-center">
          Tap to select
        </p>
      )}
    </button>
  );
}
