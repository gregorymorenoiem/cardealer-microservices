'use client';

import type { CsvVehicleRow } from './csv-import-wizard';
import { Trash2, Check, AlertTriangle, X, Clock } from 'lucide-react';

// ============================================================
// Component
// ============================================================

interface CsvPreviewTableProps {
  rows: CsvVehicleRow[];
  onRowUpdate: (index: number, updates: Partial<CsvVehicleRow>) => void;
  onRowRemove: (index: number) => void;
  showStatus?: boolean;
}

const STATUS_CONFIG = {
  pending: { icon: Clock, label: 'Pendiente', color: 'text-gray-500', bg: 'bg-gray-100' },
  decoded: { icon: Check, label: 'Decodificado', color: 'text-emerald-600', bg: 'bg-emerald-100' },
  error: { icon: X, label: 'Error', color: 'text-red-600', bg: 'bg-red-100' },
  duplicate: {
    icon: AlertTriangle,
    label: 'Duplicado',
    color: 'text-amber-600',
    bg: 'bg-amber-100',
  },
};

export function CsvPreviewTable({
  rows,
  onRowUpdate,
  onRowRemove,
  showStatus = false,
}: CsvPreviewTableProps) {
  if (rows.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 p-8 text-center text-sm text-gray-500">
        No hay vehículos para mostrar
      </div>
    );
  }

  return (
    <div className="overflow-x-auto rounded-xl border border-gray-200">
      <table className="w-full text-left text-sm">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
              #
            </th>
            <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
              VIN
            </th>
            {showStatus && (
              <>
                <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
                  Marca
                </th>
                <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
                  Modelo
                </th>
                <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
                  Año
                </th>
              </>
            )}
            <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
              Precio
            </th>
            <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
              Km
            </th>
            {showStatus && (
              <th className="px-3 py-2.5 text-xs font-semibold tracking-wider text-gray-600 uppercase">
                Estado
              </th>
            )}
            <th className="w-10 px-3 py-2.5" />
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {rows.map((row, index) => {
            const statusCfg = row.status ? STATUS_CONFIG[row.status] : STATUS_CONFIG.pending;
            const StatusIcon = statusCfg.icon;

            return (
              <tr
                key={index}
                className={`${
                  row.status === 'error' || row.status === 'duplicate' ? 'bg-red-50/50' : ''
                }`}
              >
                <td className="px-3 py-2.5 text-xs text-gray-400">{index + 1}</td>
                <td className="px-3 py-2.5">
                  <span className="font-mono text-xs tracking-wider text-gray-900">{row.vin}</span>
                </td>
                {showStatus && (
                  <>
                    <td className="px-3 py-2.5 text-gray-700">{row.make || '—'}</td>
                    <td className="px-3 py-2.5 text-gray-700">{row.model || '—'}</td>
                    <td className="px-3 py-2.5 text-gray-700">{row.year || '—'}</td>
                  </>
                )}
                <td className="px-3 py-2.5">
                  <input
                    type="number"
                    value={row.price || ''}
                    onChange={e =>
                      onRowUpdate(index, { price: parseFloat(e.target.value) || undefined })
                    }
                    placeholder="—"
                    className="w-24 rounded border border-gray-200 px-2 py-1 text-xs focus:ring-1 focus:ring-emerald-500 focus:outline-none"
                  />
                </td>
                <td className="px-3 py-2.5">
                  <input
                    type="number"
                    value={row.mileage || ''}
                    onChange={e =>
                      onRowUpdate(index, { mileage: parseInt(e.target.value) || undefined })
                    }
                    placeholder="—"
                    className="w-20 rounded border border-gray-200 px-2 py-1 text-xs focus:ring-1 focus:ring-emerald-500 focus:outline-none"
                  />
                </td>
                {showStatus && (
                  <td className="px-3 py-2.5">
                    <span
                      className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${statusCfg.bg} ${statusCfg.color}`}
                    >
                      <StatusIcon className="h-3 w-3" />
                      {statusCfg.label}
                    </span>
                    {row.errorMessage && (
                      <p className="mt-0.5 text-[10px] text-red-500">{row.errorMessage}</p>
                    )}
                  </td>
                )}
                <td className="px-3 py-2.5">
                  <button
                    onClick={() => onRowRemove(index)}
                    className="rounded p-1 text-gray-400 hover:bg-red-50 hover:text-red-500"
                    title="Eliminar fila"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
