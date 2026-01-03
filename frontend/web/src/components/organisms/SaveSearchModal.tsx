import { useState, useEffect } from 'react';
import { FiX, FiBell, FiBookmark, FiCheck } from 'react-icons/fi';
import type { VehicleFilters } from '@/components/organisms/AdvancedFilters';
import { formatFilters } from '@/services/savedSearchService';

export type AlertFrequency = 'immediate' | 'daily' | 'weekly';

interface SaveSearchModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: SaveSearchData) => void;
  filters: VehicleFilters;
  isSaving?: boolean;
}

export interface SaveSearchData {
  name: string;
  filters: VehicleFilters;
  notificationsEnabled: boolean;
  alertFrequency: AlertFrequency;
}

export default function SaveSearchModal({
  isOpen,
  onClose,
  onSave,
  filters,
  isSaving = false,
}: SaveSearchModalProps) {
  const [name, setName] = useState('');
  const [notificationsEnabled, setNotificationsEnabled] = useState(true);
  const [alertFrequency, setAlertFrequency] = useState<AlertFrequency>('daily');

  // Generate default name from filters
  useEffect(() => {
    if (isOpen) {
      const parts: string[] = [];
      if (filters.make) parts.push(filters.make);
      if (filters.model) parts.push(filters.model);
      if (filters.minYear || filters.maxYear) {
        parts.push(`${filters.minYear || ''}${filters.maxYear ? '-' + filters.maxYear : '+'}`);
      }
      if (filters.minPrice || filters.maxPrice) {
        const min = filters.minPrice ? `$${(filters.minPrice / 1000).toFixed(0)}k` : '';
        const max = filters.maxPrice ? `$${(filters.maxPrice / 1000).toFixed(0)}k` : '';
        if (min && max) parts.push(`${min}-${max}`);
        else if (min) parts.push(`desde ${min}`);
        else if (max) parts.push(`hasta ${max}`);
      }
      
      setName(parts.length > 0 ? parts.join(' ') : 'Mi búsqueda');
    }
  }, [isOpen, filters]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    onSave({
      name: name.trim(),
      filters,
      notificationsEnabled,
      alertFrequency,
    });
  };

  if (!isOpen) return null;

  const filterSummary = formatFilters(filters as Record<string, unknown>);
  const hasFilters = Object.keys(filters).some(key => filters[key as keyof VehicleFilters]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/50 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-100">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-primary/10 rounded-lg">
              <FiBookmark className="h-5 w-5 text-primary" />
            </div>
            <h2 className="text-xl font-semibold text-gray-900">
              Guardar búsqueda
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <FiX className="h-5 w-5" />
          </button>
        </div>

        {/* Content */}
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {/* Name Input */}
          <div>
            <label htmlFor="search-name" className="block text-sm font-medium text-gray-700 mb-2">
              Nombre de la búsqueda
            </label>
            <input
              id="search-name"
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ej: SUV familiar 2020-2024"
              className="w-full px-4 py-3 border border-gray-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
              required
              autoFocus
            />
          </div>

          {/* Filters Summary */}
          {hasFilters && (
            <div className="p-4 bg-gray-50 rounded-xl">
              <p className="text-sm font-medium text-gray-700 mb-2">Filtros aplicados:</p>
              <p className="text-sm text-gray-600">{filterSummary}</p>
            </div>
          )}

          {/* Notifications Toggle */}
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <FiBell className="h-5 w-5 text-gray-400" />
                <div>
                  <p className="font-medium text-gray-900">Recibir alertas</p>
                  <p className="text-sm text-gray-500">Te notificaremos cuando haya nuevos resultados</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setNotificationsEnabled(!notificationsEnabled)}
                className={`relative w-12 h-6 rounded-full transition-colors ${
                  notificationsEnabled ? 'bg-primary' : 'bg-gray-300'
                }`}
              >
                <span
                  className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full shadow transition-transform ${
                    notificationsEnabled ? 'translate-x-6' : ''
                  }`}
                />
              </button>
            </div>

            {/* Alert Frequency */}
            {notificationsEnabled && (
              <div className="ml-8 space-y-2">
                <p className="text-sm font-medium text-gray-700">Frecuencia de alertas:</p>
                <div className="flex gap-2">
                  {[
                    { value: 'immediate', label: 'Inmediato' },
                    { value: 'daily', label: 'Diario' },
                    { value: 'weekly', label: 'Semanal' },
                  ].map((option) => (
                    <button
                      key={option.value}
                      type="button"
                      onClick={() => setAlertFrequency(option.value as AlertFrequency)}
                      className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                        alertFrequency === option.value
                          ? 'bg-primary text-white'
                          : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                      }`}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-3 border border-gray-200 text-gray-700 rounded-xl font-medium hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSaving || !name.trim()}
              className="flex-1 px-4 py-3 bg-primary text-white rounded-xl font-medium hover:bg-primary-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {isSaving ? (
                <>
                  <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                  Guardando...
                </>
              ) : (
                <>
                  <FiCheck className="h-4 w-4" />
                  Guardar
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
