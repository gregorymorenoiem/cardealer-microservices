'use client';

import { useState, useCallback, useMemo } from 'react';
import { useDecodeVinBatch } from '@/hooks/use-vehicles';
import { sanitizeVIN } from '@/lib/security/sanitize';
import { CsvPreviewTable } from './csv-preview-table';
import {
  Upload,
  FileSpreadsheet,
  Download,
  Check,
  Loader2,
  ChevronRight,
  Info,
} from 'lucide-react';
import { toast } from 'sonner';

// ============================================================
// Types
// ============================================================

export interface CsvVehicleRow {
  vin: string;
  price?: number;
  mileage?: number;
  condition?: string;
  description?: string;
  province?: string;
  city?: string;
  // Decoded data (filled after batch decode)
  make?: string;
  model?: string;
  year?: number;
  trim?: string;
  transmission?: string;
  fuelType?: string;
  bodyType?: string;
  status?: 'pending' | 'decoded' | 'error' | 'duplicate';
  errorMessage?: string;
}

// ============================================================
// Helpers
// ============================================================

function parseCsvText(text: string): CsvVehicleRow[] {
  const lines = text.trim().split('\n');
  if (lines.length < 2) return [];

  const headers = lines[0]
    .toLowerCase()
    .split(',')
    .map(h => h.trim().replace(/"/g, ''));
  const rows: CsvVehicleRow[] = [];

  for (let i = 1; i < lines.length; i++) {
    const values = lines[i].split(',').map(v => v.trim().replace(/"/g, ''));
    const row: Record<string, string> = {};
    headers.forEach((h, idx) => {
      row[h] = values[idx] || '';
    });

    const vin = sanitizeVIN(row['vin'] || '');
    if (!vin) continue;

    rows.push({
      vin,
      price: row['price'] || row['precio'] ? parseFloat(row['price'] || row['precio']) : undefined,
      mileage:
        row['mileage'] || row['kilometraje']
          ? parseInt(row['mileage'] || row['kilometraje'])
          : undefined,
      condition: row['condition'] || row['condicion'] || undefined,
      description: row['description'] || row['descripcion'] || undefined,
      province: row['province'] || row['provincia'] || undefined,
      city: row['city'] || row['ciudad'] || undefined,
      status: 'pending',
    });
  }

  return rows;
}

const CSV_TEMPLATE = `vin,price,mileage,condition,province,city,description
1HGCV1F32LA000001,1500000,25000,excellent,santo-domingo,Piantini,Vehículo en excelente estado
2T1BURHE5JC000002,850000,60000,good,santiago,,
`;

// ============================================================
// Component
// ============================================================

interface CsvImportWizardProps {
  onImportComplete: (rows: CsvVehicleRow[]) => void;
  onCancel: () => void;
}

export function CsvImportWizard({ onImportComplete, onCancel }: CsvImportWizardProps) {
  const [step, setStep] = useState<'upload' | 'preview' | 'decoding' | 'results'>('upload');
  const [rows, setRows] = useState<CsvVehicleRow[]>([]);
  const [decodeProgress, setDecodeProgress] = useState(0);
  const decodeBatch = useDecodeVinBatch();

  // Upload handler
  const handleFileUpload = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.name.endsWith('.csv') && file.type !== 'text/csv') {
      toast.error('Solo se aceptan archivos CSV');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const text = reader.result as string;
      const parsed = parseCsvText(text);
      if (parsed.length === 0) {
        toast.error('No se encontraron VINs válidos en el archivo');
        return;
      }
      if (parsed.length > 50) {
        toast.error('Máximo 50 vehículos por importación');
        return;
      }
      setRows(parsed);
      setStep('preview');
      toast.success(
        `${parsed.length} vehículo${parsed.length > 1 ? 's' : ''} encontrado${parsed.length > 1 ? 's' : ''}`
      );
    };
    reader.readAsText(file);
    e.target.value = '';
  }, []);

  // Download template
  const handleDownloadTemplate = useCallback(() => {
    const blob = new Blob([CSV_TEMPLATE], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'okla-import-template.csv';
    a.click();
    URL.revokeObjectURL(url);
  }, []);

  // Batch decode
  const handleStartDecode = useCallback(async () => {
    setStep('decoding');
    setDecodeProgress(0);

    const vins = rows.map(r => r.vin);

    try {
      const result = await decodeBatch.mutateAsync(vins);

      const updatedRows = rows.map(row => {
        const decoded = result.results?.find((r: { vin: string }) => r.vin === row.vin);
        if (!decoded)
          return { ...row, status: 'error' as const, errorMessage: 'No se pudo decodificar' };
        if (decoded.isDuplicate)
          return { ...row, status: 'duplicate' as const, errorMessage: 'VIN ya publicado' };

        return {
          ...row,
          status: 'decoded' as const,
          make: decoded.autoFill?.make || row.make,
          model: decoded.autoFill?.model || row.model,
          year: decoded.autoFill?.year || row.year,
          trim: decoded.autoFill?.trim || row.trim,
          transmission: decoded.autoFill?.transmission || row.transmission,
          fuelType: decoded.autoFill?.fuelType || row.fuelType,
          bodyType: decoded.autoFill?.bodyStyle || row.bodyType,
        };
      });

      setRows(updatedRows);
      setDecodeProgress(100);
      setStep('results');

      const decoded = updatedRows.filter(r => r.status === 'decoded').length;
      const errors = updatedRows.filter(r => r.status === 'error').length;
      const dupes = updatedRows.filter(r => r.status === 'duplicate').length;
      toast.success(`${decoded} decodificados, ${dupes} duplicados, ${errors} errores`);
    } catch {
      toast.error('Error al decodificar VINs. Intenta de nuevo.');
      setStep('preview');
    }
  }, [rows, decodeBatch]);

  // Row editing
  const handleRowUpdate = useCallback((index: number, updates: Partial<CsvVehicleRow>) => {
    setRows(prev => prev.map((row, i) => (i === index ? { ...row, ...updates } : row)));
  }, []);

  const handleRowRemove = useCallback((index: number) => {
    setRows(prev => prev.filter((_, i) => i !== index));
  }, []);

  // Stats
  const stats = useMemo(
    () => ({
      total: rows.length,
      decoded: rows.filter(r => r.status === 'decoded').length,
      errors: rows.filter(r => r.status === 'error').length,
      duplicates: rows.filter(r => r.status === 'duplicate').length,
      pending: rows.filter(r => r.status === 'pending').length,
    }),
    [rows]
  );

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-xl font-bold text-gray-900">Importación Masiva CSV</h2>
        <p className="mt-1 text-sm text-gray-500">
          Importa múltiples vehículos desde un archivo CSV con VINs
        </p>
      </div>

      {/* ── Step: Upload ── */}
      {step === 'upload' && (
        <div className="space-y-4">
          {/* Drop Zone */}
          <label className="flex cursor-pointer flex-col items-center gap-4 rounded-xl border-2 border-dashed border-gray-300 p-12 text-center transition-colors hover:border-emerald-400 hover:bg-gray-50">
            <input
              type="file"
              accept=".csv,text/csv"
              className="hidden"
              onChange={handleFileUpload}
            />
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
              <FileSpreadsheet className="h-8 w-8 text-gray-400" />
            </div>
            <div>
              <p className="text-sm font-medium text-gray-700">
                Arrastra un CSV aquí o <span className="text-emerald-600">selecciona archivo</span>
              </p>
              <p className="mt-1 text-xs text-gray-500">Máximo 50 vehículos por importación</p>
            </div>
          </label>

          {/* Template download */}
          <div className="rounded-lg border border-blue-200 bg-blue-50 p-4">
            <div className="flex items-start gap-3">
              <Info className="mt-0.5 h-5 w-5 flex-shrink-0 text-blue-500" />
              <div>
                <p className="text-sm font-medium text-blue-800">Formato del archivo CSV</p>
                <p className="mt-1 text-xs text-blue-700">
                  El archivo debe tener una columna &quot;vin&quot; obligatoria. Columnas
                  opcionales: price, mileage, condition, province, city, description.
                </p>
                <button
                  onClick={handleDownloadTemplate}
                  className="mt-2 inline-flex items-center gap-1.5 text-sm font-medium text-blue-700 underline hover:text-blue-800"
                >
                  <Download className="h-3.5 w-3.5" />
                  Descargar plantilla CSV
                </button>
              </div>
            </div>
          </div>

          <div className="text-center">
            <button onClick={onCancel} className="text-sm text-gray-500 hover:text-gray-700">
              Cancelar
            </button>
          </div>
        </div>
      )}

      {/* ── Step: Preview ── */}
      {step === 'preview' && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <p className="text-sm text-gray-600">
              <span className="font-semibold">{rows.length}</span> vehículo
              {rows.length > 1 ? 's' : ''} encontrado{rows.length > 1 ? 's' : ''}
            </p>
            <label className="flex cursor-pointer items-center gap-2 text-sm text-emerald-600 hover:text-emerald-700">
              <Upload className="h-4 w-4" />
              Cambiar archivo
              <input type="file" accept=".csv" className="hidden" onChange={handleFileUpload} />
            </label>
          </div>

          <CsvPreviewTable
            rows={rows}
            onRowUpdate={handleRowUpdate}
            onRowRemove={handleRowRemove}
          />

          <div className="flex gap-3">
            <button
              onClick={() => {
                setRows([]);
                setStep('upload');
              }}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              onClick={handleStartDecode}
              disabled={rows.length === 0}
              className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-3 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            >
              Decodificar {rows.length} VIN{rows.length > 1 ? 's' : ''}
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>
        </div>
      )}

      {/* ── Step: Decoding ── */}
      {step === 'decoding' && (
        <div className="flex flex-col items-center gap-4 py-12">
          <Loader2 className="h-12 w-12 animate-spin text-emerald-500" />
          <div className="text-center">
            <p className="text-sm font-medium text-gray-700">Decodificando VINs...</p>
            <p className="mt-1 text-xs text-gray-500">
              {stats.total} vehículo{stats.total > 1 ? 's' : ''} en proceso
            </p>
          </div>
          <div className="h-2 w-64 overflow-hidden rounded-full bg-gray-200">
            <div
              className="h-full rounded-full bg-emerald-500 transition-all"
              style={{ width: `${decodeProgress}%` }}
            />
          </div>
        </div>
      )}

      {/* ── Step: Results ── */}
      {step === 'results' && (
        <div className="space-y-4">
          {/* Summary */}
          <div className="grid grid-cols-4 gap-3">
            <div className="rounded-lg bg-gray-50 p-3 text-center">
              <p className="text-lg font-bold text-gray-900">{stats.total}</p>
              <p className="text-xs text-gray-500">Total</p>
            </div>
            <div className="rounded-lg bg-emerald-50 p-3 text-center">
              <p className="text-lg font-bold text-emerald-600">{stats.decoded}</p>
              <p className="text-xs text-emerald-600">Exitosos</p>
            </div>
            <div className="rounded-lg bg-amber-50 p-3 text-center">
              <p className="text-lg font-bold text-amber-600">{stats.duplicates}</p>
              <p className="text-xs text-amber-600">Duplicados</p>
            </div>
            <div className="rounded-lg bg-red-50 p-3 text-center">
              <p className="text-lg font-bold text-red-600">{stats.errors}</p>
              <p className="text-xs text-red-600">Errores</p>
            </div>
          </div>

          <CsvPreviewTable
            rows={rows}
            onRowUpdate={handleRowUpdate}
            onRowRemove={handleRowRemove}
            showStatus
          />

          <div className="flex gap-3">
            <button
              onClick={onCancel}
              className="flex-1 rounded-lg border border-gray-300 px-4 py-3 text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              onClick={() => onImportComplete(rows.filter(r => r.status === 'decoded'))}
              disabled={stats.decoded === 0}
              className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-3 text-sm font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
            >
              <Check className="h-4 w-4" />
              Importar {stats.decoded} vehículo{stats.decoded !== 1 ? 's' : ''}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
