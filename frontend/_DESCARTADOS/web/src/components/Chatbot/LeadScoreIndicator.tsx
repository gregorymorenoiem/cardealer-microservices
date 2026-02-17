import React from 'react';
import { chatbotService, LeadTemperature } from '@/services/chatbotService';
import { FiThermometer } from 'react-icons/fi';

interface LeadScoreIndicatorProps {
  score: number;
  temperature: LeadTemperature;
  showLabel?: boolean;
  size?: 'sm' | 'md' | 'lg';
}

/**
 * Badge que muestra el score y temperatura del lead
 * Solo visible para dealers (no para compradores)
 */
export const LeadScoreIndicator: React.FC<LeadScoreIndicatorProps> = ({
  score,
  temperature,
  showLabel = true,
  size = 'md',
}) => {
  const colorClass = getColorClass(temperature);
  const sizeClass = getSizeClass(size);
  const label = chatbotService.getTemperatureLabel(temperature);

  return (
    <div className={`inline-flex items-center space-x-2 ${sizeClass}`}>
      {/* Thermometer Icon */}
      <div className={`flex items-center justify-center ${colorClass} rounded-full p-1`}>
        <FiThermometer className="text-white" />
      </div>

      {/* Score Badge */}
      <div className={`${colorClass} text-white font-bold rounded-full px-3 py-1 text-center`}>
        {score}
      </div>

      {/* Temperature Label */}
      {showLabel && (
        <span className={`font-semibold ${getTextColorClass(temperature)}`}>{label}</span>
      )}
    </div>
  );
};

// Helper functions
function getColorClass(temperature: LeadTemperature): string {
  switch (temperature) {
    case LeadTemperature.Hot:
      return 'bg-red-600';
    case LeadTemperature.WarmHot:
      return 'bg-orange-500';
    case LeadTemperature.Warm:
      return 'bg-yellow-500';
    case LeadTemperature.Cold:
      return 'bg-blue-500';
    default:
      return 'bg-gray-500';
  }
}

function getTextColorClass(temperature: LeadTemperature): string {
  switch (temperature) {
    case LeadTemperature.Hot:
      return 'text-red-600';
    case LeadTemperature.WarmHot:
      return 'text-orange-500';
    case LeadTemperature.Warm:
      return 'text-yellow-600';
    case LeadTemperature.Cold:
      return 'text-blue-600';
    default:
      return 'text-gray-600';
  }
}

function getSizeClass(size: 'sm' | 'md' | 'lg'): string {
  switch (size) {
    case 'sm':
      return 'text-xs';
    case 'md':
      return 'text-sm';
    case 'lg':
      return 'text-base';
    default:
      return 'text-sm';
  }
}

export default LeadScoreIndicator;
