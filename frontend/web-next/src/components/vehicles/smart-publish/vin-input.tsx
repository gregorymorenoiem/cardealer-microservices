'use client';

import { useState, useEffect, useCallback, useRef } from 'react';
import { useDecodeVin, useCheckVinExists } from '@/hooks/use-vehicles';
import { sanitizeVIN } from '@/lib/security/sanitize';
import type { SmartVinDecodeResult } from '@/services/vehicles';
import { Search, Check, AlertTriangle, X, Loader2, ExternalLink } from 'lucide-react';

// ============================================================
// VIN Validation Utilities
// ============================================================

const VIN_REGEX = /^[A-HJ-NPR-Z0-9]{17}$/;

function isValidVinChar(char: string): boolean {
  return /^[A-HJ-NPR-Z0-9]$/i.test(char);
}

function validateVinFormat(vin: string): { valid: boolean; message?: string } {
  if (!vin) return { valid: false };
  if (vin.length < 17) return { valid: false, message: `${vin.length}/17 caracteres` };
  if (vin.length > 17) return { valid: false, message: 'VIN demasiado largo' };
  if (/[IOQ]/i.test(vin)) return { valid: false, message: 'VIN no puede contener I, O o Q' };
  if (!VIN_REGEX.test(vin)) return { valid: false, message: 'Formato inválido' };
  return { valid: true };
}

// ============================================================
// Component
// ============================================================

interface VinInputProps {
  value: string;
  onChange: (vin: string) => void;
  onDecoded: (result: SmartVinDecodeResult) => void;
}

export function VinInput({ value, onChange, onDecoded }: VinInputProps) {
  const [localVin, setLocalVin] = useState(value);
  const [shouldDecode, setShouldDecode] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const hasAutoDecoded = useRef(false);

  const isComplete = localVin.length === 17;
  const validation = validateVinFormat(localVin);
  const sanitizedVin = sanitizeVIN(localVin);

  // Check existence (debounced via React Query)
  const { data: existsData, isFetching: isCheckingExists } = useCheckVinExists(sanitizedVin, {
    enabled: validation.valid,
  });

  // Decode VIN
  const {
    data: decodeData,
    isFetching: isDecoding,
    error: decodeError,
  } = useDecodeVin(sanitizedVin, {
    enabled: shouldDecode && validation.valid,
  });

  // Auto-decode when VIN is complete and valid
  useEffect(() => {
    if (validation.valid && !hasAutoDecoded.current && !existsData?.exists && isComplete) {
      hasAutoDecoded.current = true;
      requestAnimationFrame(() => setShouldDecode(true));
    }
  }, [validation.valid, existsData?.exists, isComplete]);

  // Handle decode result
  useEffect(() => {
    if (decodeData && shouldDecode) {
      requestAnimationFrame(() => setShouldDecode(false));
      onDecoded(decodeData);
    }
  }, [decodeData, shouldDecode, onDecoded]);

  // Handle input change
  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const raw = e.target.value.toUpperCase();
      // Filter only valid VIN characters
      const cleaned = raw
        .split('')
        .filter(c => isValidVinChar(c))
        .join('')
        .slice(0, 17);
      setLocalVin(cleaned);
      onChange(cleaned);
      hasAutoDecoded.current = false;
      setShouldDecode(false);
    },
    [onChange]
  );

  const handleDecode = useCallback(() => {
    if (validation.valid) {
      setShouldDecode(true);
    }
  }, [validation.valid]);

  const handleClear = useCallback(() => {
    setLocalVin('');
    onChange('');
    hasAutoDecoded.current = false;
    setShouldDecode(false);
    inputRef.current?.focus();
  }, [onChange]);

  // Determine status
  type Status =
    | 'empty'
    | 'typing'
    | 'invalid'
    | 'checking'
    | 'duplicate'
    | 'valid'
    | 'decoding'
    | 'error';
  let status: Status = 'empty';
  if (localVin.length === 0) status = 'empty';
  else if (localVin.length < 17) status = 'typing';
  else if (!validation.valid) status = 'invalid';
  else if (isCheckingExists || isDecoding) status = isDecoding ? 'decoding' : 'checking';
  else if (existsData?.exists) status = 'duplicate';
  else if (decodeError) status = 'error';
  else status = 'valid';

  const statusConfig = {
    empty: { border: 'border-gray-300', bg: '', icon: null, text: '' },
    typing: {
      border: 'border-gray-300',
      bg: '',
      icon: null,
      text: `${localVin.length}/17 caracteres`,
    },
    invalid: {
      border: 'border-amber-500',
      bg: 'bg-amber-50',
      icon: <AlertTriangle className="h-4 w-4 text-amber-500" />,
      text: validation.message || 'Formato inválido',
    },
    checking: {
      border: 'border-blue-500',
      bg: 'bg-blue-50',
      icon: <Loader2 className="h-4 w-4 animate-spin text-blue-500" />,
      text: 'Verificando...',
    },
    decoding: {
      border: 'border-blue-500',
      bg: 'bg-blue-50',
      icon: <Loader2 className="h-4 w-4 animate-spin text-blue-500" />,
      text: 'Decodificando VIN...',
    },
    duplicate: {
      border: 'border-red-500',
      bg: 'bg-red-50',
      icon: <AlertTriangle className="h-4 w-4 text-red-500" />,
      text: 'Este VIN ya está publicado',
    },
    valid: {
      border: 'border-emerald-500',
      bg: 'bg-emerald-50',
      icon: <Check className="h-4 w-4 text-emerald-500" />,
      text: 'VIN válido y disponible',
    },
    error: {
      border: 'border-red-500',
      bg: 'bg-red-50',
      icon: <X className="h-4 w-4 text-red-500" />,
      text: 'Error al decodificar',
    },
  };

  const config = statusConfig[status];

  return (
    <div className="space-y-4">
      <div className="mb-6 text-center">
        <h2 className="text-xl font-bold text-gray-900">Ingresa el VIN de tu vehículo</h2>
        <p className="mt-1 text-sm text-gray-500">17 caracteres alfanuméricos (sin I, O, Q)</p>
      </div>

      {/* VIN Input */}
      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={localVin}
          onChange={handleChange}
          placeholder="Ej: 1HGCV1F32LA000001"
          maxLength={17}
          autoComplete="off"
          spellCheck={false}
          className={`w-full rounded-xl border-2 px-4 py-4 text-center font-mono text-2xl tracking-[0.2em] uppercase transition-colors focus:ring-2 focus:ring-offset-2 focus:outline-none ${config.border} ${config.bg} focus:ring-emerald-500`}
          aria-label="Número de Identificación del Vehículo (VIN)"
          aria-describedby="vin-status"
        />
        {localVin.length > 0 && (
          <button
            onClick={handleClear}
            className="absolute top-1/2 right-3 -translate-y-1/2 rounded-full p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
            aria-label="Limpiar VIN"
          >
            <X className="h-5 w-5" />
          </button>
        )}
      </div>

      {/* Status message */}
      <div
        id="vin-status"
        className="flex min-h-[24px] items-center justify-center gap-2 text-sm"
        role="status"
      >
        {config.icon}
        <span
          className={`${
            status === 'valid'
              ? 'text-emerald-600'
              : status === 'duplicate' || status === 'error'
                ? 'text-red-600'
                : status === 'invalid'
                  ? 'text-amber-600'
                  : status === 'checking' || status === 'decoding'
                    ? 'text-blue-600'
                    : 'text-gray-500'
          }`}
        >
          {config.text}
        </span>
      </div>

      {/* Duplicate warning */}
      {status === 'duplicate' && existsData && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4">
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 flex-shrink-0 text-red-600" />
            <div>
              <p className="text-sm font-medium text-red-800">Este VIN ya está publicado en OKLA</p>
              <p className="mt-1 text-sm text-red-700">
                Ya existe un vehículo con este VIN. Puedes ver el listado existente o usar un VIN
                diferente.
              </p>
              {existsData.slug && (
                <a
                  href={`/vehiculos/${existsData.slug}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-2 inline-flex items-center gap-1 text-sm font-medium text-red-700 underline hover:text-red-800"
                >
                  Ver vehículo existente <ExternalLink className="h-3.5 w-3.5" />
                </a>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Decode button */}
      <div className="flex justify-center">
        <button
          onClick={handleDecode}
          disabled={!validation.valid || isDecoding || status === 'duplicate'}
          className="flex items-center gap-2 rounded-lg bg-emerald-600 px-8 py-3 text-sm font-medium text-white transition-colors hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isDecoding ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Search className="h-4 w-4" />
          )}
          {isDecoding ? 'Decodificando...' : 'Decodificar VIN'}
        </button>
      </div>

      {/* Error display */}
      {decodeError && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-center">
          <p className="text-sm text-red-700">
            No se pudo decodificar el VIN. Verifica que sea correcto o intenta de nuevo.
          </p>
          <button
            onClick={handleDecode}
            className="mt-2 text-sm font-medium text-red-700 underline hover:text-red-800"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Character grid visualization */}
      <div className="flex justify-center">
        <div className="flex gap-0.5">
          {Array.from({ length: 17 }).map((_, i) => (
            <div
              key={i}
              className={`flex h-8 w-6 items-center justify-center rounded font-mono text-xs font-bold transition-colors ${
                i < localVin.length
                  ? status === 'valid'
                    ? 'bg-emerald-100 text-emerald-700'
                    : status === 'duplicate'
                      ? 'bg-red-100 text-red-700'
                      : 'bg-gray-200 text-gray-700'
                  : 'bg-gray-100 text-gray-300'
              }`}
            >
              {localVin[i] || '·'}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
