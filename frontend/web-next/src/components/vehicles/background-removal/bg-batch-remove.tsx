'use client';

import { useState, useCallback } from 'react';
import { CheckCircle2, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';
import { BgRemoveButton } from './bg-remove-button';
import { useBatchRemoveBackground } from '@/hooks/use-background-removal';

// ============================================================
// TYPES
// ============================================================

interface PhotoForRemoval {
  id: string;
  url: string;
  name: string;
}

interface BgBatchRemoveProps {
  photos: PhotoForRemoval[];
  onComplete: (results: { photoId: string; resultUrl: string }[]) => void;
  className?: string;
}

// ============================================================
// COMPONENT
// ============================================================

export function BgBatchRemove({
  photos,
  className = '',
}: BgBatchRemoveProps) {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isProcessing, setIsProcessing] = useState(false);
  const [results] = useState<Map<string, { url?: string; error?: string }>>(new Map());

  const batchMutation = useBatchRemoveBackground();

  const toggleSelect = useCallback((id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const selectAll = useCallback(() => {
    if (selectedIds.size === photos.length) {
      setSelectedIds(new Set());
    } else {
      setSelectedIds(new Set(photos.map(p => p.id)));
    }
  }, [selectedIds.size, photos]);

  const handleProcess = useCallback(async () => {
    if (selectedIds.size === 0) {
      toast.warning('Selecciona al menos una foto');
      return;
    }

    setIsProcessing(true);
    const urls = photos
      .filter(p => selectedIds.has(p.id))
      .map(p => p.url);

    try {
      await batchMutation.mutateAsync(urls, {
        onSuccess: () => {
          // The batch returns a job - we'd poll for results
          toast.success(`Procesando ${selectedIds.size} fotos...`);
        },
      });
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Error al procesar';
      toast.error(msg);
    } finally {
      setIsProcessing(false);
    }
  }, [selectedIds, photos, batchMutation]);

  return (
    <div className={`space-y-4 ${className}`}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h4 className="text-sm font-semibold text-gray-900">
            Eliminar fondo en lote
          </h4>
          <p className="text-xs text-gray-500">
            Selecciona las fotos a las que deseas eliminar el fondo
          </p>
        </div>
        <button
          type="button"
          onClick={selectAll}
          className="text-sm font-medium text-emerald-600 hover:text-emerald-700"
        >
          {selectedIds.size === photos.length ? 'Deseleccionar todas' : 'Seleccionar todas'}
        </button>
      </div>

      {/* Photo grid */}
      <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
        {photos.map(photo => {
          const isSelected = selectedIds.has(photo.id);
          const result = results.get(photo.id);

          return (
            <button
              key={photo.id}
              type="button"
              onClick={() => toggleSelect(photo.id)}
              disabled={isProcessing}
              className={`relative aspect-[4/3] overflow-hidden rounded-lg border-2 transition-all ${
                isSelected
                  ? 'border-purple-500 ring-2 ring-purple-200'
                  : 'border-transparent hover:border-gray-300'
              } ${isProcessing ? 'opacity-70' : ''}`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={photo.url}
                alt={photo.name}
                className="h-full w-full object-cover"
              />

              {/* Selection indicator */}
              {isSelected && (
                <div className="absolute top-1 right-1 flex h-5 w-5 items-center justify-center rounded-full bg-purple-500">
                  <CheckCircle2 className="h-3 w-3 text-white" />
                </div>
              )}

              {/* Result indicator */}
              {result?.url && (
                <div className="absolute inset-0 flex items-center justify-center bg-emerald-500/20">
                  <CheckCircle2 className="h-6 w-6 text-emerald-600" />
                </div>
              )}
              {result?.error && (
                <div className="absolute inset-0 flex items-center justify-center bg-red-500/20">
                  <AlertTriangle className="h-6 w-6 text-red-600" />
                </div>
              )}
            </button>
          );
        })}
      </div>

      {/* Actions */}
      <div className="flex items-center justify-between rounded-lg bg-gray-50 px-4 py-3">
        <p className="text-sm text-gray-600">
          {selectedIds.size} de {photos.length} fotos seleccionadas
        </p>
        <BgRemoveButton
          onClick={handleProcess}
          isProcessing={isProcessing}
          isDisabled={selectedIds.size === 0}
          variant="primary"
        />
      </div>
    </div>
  );
}
