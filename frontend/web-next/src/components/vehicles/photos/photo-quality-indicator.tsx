'use client';

import { CheckCircle2, AlertTriangle, XCircle, Info } from 'lucide-react';

// ============================================================
// TYPES
// ============================================================

interface QualityIndicatorProps {
  score: number;
  isTooSmall?: boolean;
  warnings?: string[];
  suggestions?: string[];
  compact?: boolean;
}

// ============================================================
// COMPONENT
// ============================================================

export function PhotoQualityIndicator({
  score,
  isTooSmall = false,
  warnings = [],
  suggestions = [],
  compact = false,
}: QualityIndicatorProps) {
  const level = score >= 80 ? 'excellent' : score >= 60 ? 'good' : score >= 40 ? 'fair' : 'poor';

  const config = {
    excellent: {
      icon: CheckCircle2,
      color: 'text-emerald-600',
      bg: 'bg-emerald-50',
      border: 'border-emerald-200',
      bar: 'bg-emerald-500',
      label: 'Excelente',
    },
    good: {
      icon: CheckCircle2,
      color: 'text-blue-600',
      bg: 'bg-blue-50',
      border: 'border-blue-200',
      bar: 'bg-blue-500',
      label: 'Buena',
    },
    fair: {
      icon: AlertTriangle,
      color: 'text-amber-600',
      bg: 'bg-amber-50',
      border: 'border-amber-200',
      bar: 'bg-amber-500',
      label: 'Regular',
    },
    poor: {
      icon: XCircle,
      color: 'text-red-600',
      bg: 'bg-red-50',
      border: 'border-red-200',
      bar: 'bg-red-500',
      label: 'Baja',
    },
  }[level];

  const Icon = config.icon;

  if (compact) {
    return (
      <div
        className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${config.bg} ${config.color}`}
      >
        <Icon className="h-3 w-3" />
        {score}
      </div>
    );
  }

  return (
    <div className={`rounded-lg border p-3 ${config.bg} ${config.border}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Icon className={`h-5 w-5 ${config.color}`} />
          <span className={`text-sm font-semibold ${config.color}`}>
            Calidad: {config.label} ({score}/100)
          </span>
        </div>
      </div>

      {/* Progress bar */}
      <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-gray-200">
        <div
          className={`h-full rounded-full transition-all duration-500 ${config.bar}`}
          style={{ width: `${score}%` }}
        />
      </div>

      {/* Too small warning */}
      {isTooSmall && (
        <p className="mt-2 flex items-center gap-1 text-xs text-red-600">
          <XCircle className="h-3 w-3 flex-shrink-0" />
          Resolución muy baja. Mínimo recomendado: 640×480px
        </p>
      )}

      {/* Warnings */}
      {warnings.length > 0 && (
        <div className="mt-2 space-y-1">
          {warnings.map((w, i) => (
            <p key={i} className="flex items-start gap-1 text-xs text-amber-700">
              <AlertTriangle className="mt-0.5 h-3 w-3 flex-shrink-0" />
              {w}
            </p>
          ))}
        </div>
      )}

      {/* Suggestions */}
      {suggestions.length > 0 && (
        <div className="mt-2 space-y-1">
          {suggestions.map((s, i) => (
            <p key={i} className="flex items-start gap-1 text-xs text-gray-600">
              <Info className="mt-0.5 h-3 w-3 flex-shrink-0" />
              {s}
            </p>
          ))}
        </div>
      )}
    </div>
  );
}
