'use client';

import { useMemo, useCallback } from 'react';
import {
  useMakes,
  useModelsByMake,
  useBodyTypes,
  useFuelTypes,
  useTransmissions,
  useColors,
  useProvinces,
  useFeatures,
} from '@/hooks/use-vehicles';
import type { VehicleFormData } from './smart-publish-wizard';
import { sanitizeText, sanitizeMileage, sanitizeYear } from '@/lib/security/sanitize';
import { Check, ChevronDown, Sparkles } from 'lucide-react';

// ============================================================
// Static Data
// ============================================================

const CONDITIONS = [
  { value: 'new', label: 'Nuevo', description: '0 km, sin uso previo' },
  { value: 'like-new', label: 'Como Nuevo', description: 'Excelente estado, mínimo uso' },
  { value: 'excellent', label: 'Excelente', description: 'Muy bien cuidado, sin detalles' },
  { value: 'good', label: 'Bueno', description: 'Buen estado general, detalles menores' },
  { value: 'fair', label: 'Regular', description: 'Funcional, necesita algunos arreglos' },
];

const MILEAGE_UNITS = [
  { value: 'km', label: 'Kilómetros' },
  { value: 'mi', label: 'Millas' },
];

const CURRENT_YEAR = new Date().getFullYear();
const YEARS = Array.from({ length: CURRENT_YEAR - 1900 + 2 }, (_, i) => CURRENT_YEAR + 1 - i);

// ============================================================
// Sub-components
// ============================================================

function SelectField({
  label,
  value,
  onChange,
  options,
  placeholder = 'Seleccionar...',
  isAutoFilled = false,
  isLoading = false,
  disabled = false,
  required = false,
}: {
  label: string;
  value: string;
  onChange: (val: string) => void;
  options: { value: string; label: string }[];
  placeholder?: string;
  isAutoFilled?: boolean;
  isLoading?: boolean;
  disabled?: boolean;
  required?: boolean;
}) {
  return (
    <div>
      <label className="mb-1.5 flex items-center gap-1.5 text-sm font-medium text-gray-700">
        {label}
        {required && <span className="text-red-500">*</span>}
        {isAutoFilled && (
          <span className="inline-flex items-center gap-0.5 rounded-full bg-emerald-100 px-1.5 py-0.5 text-[10px] font-medium text-emerald-700">
            <Sparkles className="h-2.5 w-2.5" />
            VIN
          </span>
        )}
      </label>
      <div className="relative">
        <select
          value={value}
          onChange={e => onChange(e.target.value)}
          disabled={disabled || isLoading}
          className={`w-full appearance-none rounded-lg border px-3 py-2.5 text-sm transition-colors focus:ring-2 focus:ring-emerald-500 focus:outline-none ${
            isAutoFilled ? 'border-emerald-300 bg-emerald-50/50' : 'border-gray-300 bg-white'
          } ${disabled ? 'cursor-not-allowed opacity-60' : ''}`}
        >
          <option value="">{isLoading ? 'Cargando...' : placeholder}</option>
          {options.map(opt => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
        <ChevronDown className="pointer-events-none absolute top-1/2 right-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
      </div>
    </div>
  );
}

function TextField({
  label,
  value,
  onChange,
  placeholder,
  type = 'text',
  isAutoFilled = false,
  required = false,
  maxLength,
}: {
  label: string;
  value: string | number;
  onChange: (val: string) => void;
  placeholder?: string;
  type?: 'text' | 'number';
  isAutoFilled?: boolean;
  required?: boolean;
  maxLength?: number;
}) {
  return (
    <div>
      <label className="mb-1.5 flex items-center gap-1.5 text-sm font-medium text-gray-700">
        {label}
        {required && <span className="text-red-500">*</span>}
        {isAutoFilled && (
          <span className="inline-flex items-center gap-0.5 rounded-full bg-emerald-100 px-1.5 py-0.5 text-[10px] font-medium text-emerald-700">
            <Sparkles className="h-2.5 w-2.5" />
            VIN
          </span>
        )}
      </label>
      <input
        type={type}
        value={value}
        onChange={e => onChange(e.target.value)}
        placeholder={placeholder}
        maxLength={maxLength}
        className={`w-full rounded-lg border px-3 py-2.5 text-sm transition-colors focus:ring-2 focus:ring-emerald-500 focus:outline-none ${
          isAutoFilled ? 'border-emerald-300 bg-emerald-50/50' : 'border-gray-300 bg-white'
        }`}
      />
    </div>
  );
}

// ============================================================
// Main Component
// ============================================================

interface VehicleInfoFormProps {
  data: VehicleFormData;
  onChange: (updates: Partial<VehicleFormData>) => void;
  autoFilledFields?: Set<string>;
}

export function VehicleInfoForm({
  data,
  onChange,
  autoFilledFields = new Set(),
}: VehicleInfoFormProps) {
  const isAuto = (field: string) => autoFilledFields.has(field);

  // Catalog data
  const { data: makes = [], isLoading: makesLoading } = useMakes();
  const { data: models = [], isLoading: modelsLoading } = useModelsByMake(data.makeId || data.make);
  const { data: bodyTypes = [], isLoading: bodyLoading } = useBodyTypes();
  const { data: fuelTypes = [], isLoading: fuelLoading } = useFuelTypes();
  const { data: transmissions = [], isLoading: transLoading } = useTransmissions();
  const { data: colors = [], isLoading: colorsLoading } = useColors();
  const { data: provinces = [], isLoading: provLoading } = useProvinces();
  const { data: featuresByCategory = {} } = useFeatures();

  // Derived options
  const makeOptions = useMemo(
    () =>
      makes.map((m: { id?: string; name: string; slug?: string }) => ({
        value: m.name,
        label: m.name,
      })),
    [makes]
  );

  const modelOptions = useMemo(
    () =>
      models.map((m: { id?: string; name: string; slug?: string }) => ({
        value: m.name,
        label: m.name,
      })),
    [models]
  );

  const bodyOptions = useMemo(
    () =>
      bodyTypes.map((b: { value: string; label: string }) => ({ value: b.value, label: b.label })),
    [bodyTypes]
  );

  const fuelOptions = useMemo(
    () =>
      fuelTypes.map((f: { value: string; label: string }) => ({ value: f.value, label: f.label })),
    [fuelTypes]
  );

  const transOptions = useMemo(
    () =>
      transmissions.map((t: { value: string; label: string }) => ({
        value: t.value,
        label: t.label,
      })),
    [transmissions]
  );

  const colorOptions = useMemo(
    () => colors.map((c: { value: string; label: string }) => ({ value: c.value, label: c.label })),
    [colors]
  );

  const provOptions = useMemo(
    () =>
      provinces.map((p: { value: string; label: string }) => ({ value: p.value, label: p.label })),
    [provinces]
  );

  const yearOptions = useMemo(
    () => YEARS.map(y => ({ value: y.toString(), label: y.toString() })),
    []
  );

  // Handlers
  const handleMakeChange = useCallback(
    (val: string) => {
      const make = makes.find((m: { id?: string; name: string }) => m.name === val);
      onChange({ make: val, makeId: make?.id || '', model: '', modelId: '', trim: '', trimId: '' });
    },
    [makes, onChange]
  );

  const handleModelChange = useCallback(
    (val: string) => {
      const model = models.find((m: { id?: string; name: string }) => m.name === val);
      onChange({ model: val, modelId: model?.id || '', trim: '', trimId: '' });
    },
    [models, onChange]
  );

  const handleFeatureToggle = useCallback(
    (feature: string) => {
      const features = data.features.includes(feature)
        ? data.features.filter(f => f !== feature)
        : [...data.features, feature];
      onChange({ features });
    },
    [data.features, onChange]
  );

  return (
    <div className="space-y-8">
      <div className="text-center">
        <h2 className="text-xl font-bold text-gray-900">Información del Vehículo</h2>
        <p className="mt-1 text-sm text-gray-500">
          {autoFilledFields.size > 0
            ? `${autoFilledFields.size} campos auto-completados desde el VIN`
            : 'Completa la información de tu vehículo'}
        </p>
      </div>

      {/* ── Section: Basic Info ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Información Básica
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <SelectField
            label="Marca"
            value={data.make}
            onChange={handleMakeChange}
            options={makeOptions}
            isAutoFilled={isAuto('make')}
            isLoading={makesLoading}
            required
          />
          <SelectField
            label="Modelo"
            value={data.model}
            onChange={handleModelChange}
            options={modelOptions}
            placeholder={data.make ? 'Seleccionar modelo...' : 'Selecciona marca primero'}
            isAutoFilled={isAuto('model')}
            isLoading={modelsLoading}
            disabled={!data.make}
            required
          />
          <SelectField
            label="Año"
            value={data.year?.toString() || ''}
            onChange={val => onChange({ year: sanitizeYear(parseInt(val)) })}
            options={yearOptions}
            isAutoFilled={isAuto('year')}
            required
          />
          <TextField
            label="Trim / Versión"
            value={data.trim}
            onChange={val => onChange({ trim: sanitizeText(val, { maxLength: 50 }) })}
            placeholder="Ej: LX, EX, Sport..."
            isAutoFilled={isAuto('trim')}
          />
          <SelectField
            label="Tipo de Carrocería"
            value={data.bodyStyle}
            onChange={val => onChange({ bodyStyle: val })}
            options={bodyOptions}
            isAutoFilled={isAuto('bodyStyle')}
            isLoading={bodyLoading}
            required
          />
          <TextField
            label="Motor"
            value={data.engineSize}
            onChange={val => onChange({ engineSize: sanitizeText(val, { maxLength: 20 }) })}
            placeholder="Ej: 2.0L Turbo"
            isAutoFilled={isAuto('engineSize')}
          />
        </div>
      </section>

      {/* ── Section: Specs ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Especificaciones
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <SelectField
            label="Combustible"
            value={data.fuelType}
            onChange={val => onChange({ fuelType: val })}
            options={fuelOptions}
            isAutoFilled={isAuto('fuelType')}
            isLoading={fuelLoading}
            required
          />
          <SelectField
            label="Transmisión"
            value={data.transmission}
            onChange={val => onChange({ transmission: val })}
            options={transOptions}
            isAutoFilled={isAuto('transmission')}
            isLoading={transLoading}
            required
          />
          <TextField
            label="Tracción"
            value={data.driveType}
            onChange={val => onChange({ driveType: sanitizeText(val, { maxLength: 20 }) })}
            placeholder="Ej: FWD, AWD, RWD"
            isAutoFilled={isAuto('driveType')}
          />
          <div className="flex gap-3">
            <div className="flex-1">
              <TextField
                label="Kilometraje"
                value={data.mileage || ''}
                onChange={val => onChange({ mileage: sanitizeMileage(parseInt(val) || 0) })}
                type="number"
                placeholder="0"
                required
              />
            </div>
            <div className="w-32">
              <SelectField
                label="Unidad"
                value={data.mileageUnit}
                onChange={val => onChange({ mileageUnit: val as 'km' | 'mi' })}
                options={MILEAGE_UNITS}
              />
            </div>
          </div>
        </div>
      </section>

      {/* ── Section: Condition & Colors ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Condición y Colores
        </h3>

        {/* Condition selector */}
        <div className="mb-4">
          <label className="mb-2 block text-sm font-medium text-gray-700">
            Condición <span className="text-red-500">*</span>
          </label>
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-2 lg:grid-cols-3">
            {CONDITIONS.map(cond => (
              <button
                key={cond.value}
                type="button"
                onClick={() => onChange({ condition: cond.value })}
                className={`flex items-start gap-3 rounded-lg border-2 p-3 text-left transition-colors ${
                  data.condition === cond.value
                    ? 'border-emerald-500 bg-emerald-50'
                    : 'border-gray-200 hover:border-gray-300'
                }`}
              >
                <div
                  className={`mt-0.5 flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full border-2 ${
                    data.condition === cond.value
                      ? 'border-emerald-500 bg-emerald-500'
                      : 'border-gray-300'
                  }`}
                >
                  {data.condition === cond.value && <Check className="h-3 w-3 text-white" />}
                </div>
                <div>
                  <p className="text-sm font-medium text-gray-900">{cond.label}</p>
                  <p className="text-xs text-gray-500">{cond.description}</p>
                </div>
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <SelectField
            label="Color Exterior"
            value={data.exteriorColor}
            onChange={val => onChange({ exteriorColor: val })}
            options={colorOptions}
            isAutoFilled={isAuto('exteriorColor')}
            isLoading={colorsLoading}
          />
          <SelectField
            label="Color Interior"
            value={data.interiorColor}
            onChange={val => onChange({ interiorColor: val })}
            options={colorOptions}
            isLoading={colorsLoading}
          />
        </div>
      </section>

      {/* ── Section: Features ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Características y Equipamiento
        </h3>
        <div className="space-y-4">
          {Object.entries(featuresByCategory).map(([category, features]) => (
            <div key={category}>
              <p className="mb-2 text-sm font-medium text-gray-700 capitalize">{category}</p>
              <div className="flex flex-wrap gap-2">
                {(features as string[]).map(feature => {
                  const isSelected = data.features.includes(feature);
                  return (
                    <button
                      key={feature}
                      type="button"
                      onClick={() => handleFeatureToggle(feature)}
                      className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1.5 text-xs font-medium transition-colors ${
                        isSelected
                          ? 'border-emerald-500 bg-emerald-50 text-emerald-700'
                          : 'border-gray-200 bg-white text-gray-600 hover:border-gray-300'
                      }`}
                    >
                      {isSelected && <Check className="h-3 w-3" />}
                      {feature}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* ── Section: Location ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Ubicación
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <SelectField
            label="Provincia"
            value={data.province}
            onChange={val => onChange({ province: val, city: '' })}
            options={provOptions}
            isLoading={provLoading}
            required
          />
          <TextField
            label="Ciudad / Sector"
            value={data.city}
            onChange={val => onChange({ city: sanitizeText(val, { maxLength: 100 }) })}
            placeholder="Ej: Piantini, Naco..."
            required
          />
        </div>
      </section>

      {/* ── Section: Contact ── */}
      <section>
        <h3 className="mb-4 text-sm font-semibold tracking-wider text-gray-500 uppercase">
          Información de Contacto
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <TextField
            label="Nombre"
            value={data.sellerName}
            onChange={val => onChange({ sellerName: sanitizeText(val, { maxLength: 100 }) })}
            placeholder="Tu nombre"
          />
          <TextField
            label="Teléfono"
            value={data.sellerPhone}
            onChange={val =>
              onChange({ sellerPhone: val.replace(/[^\d\-+() ]/g, '').slice(0, 15) })
            }
            placeholder="809-000-0000"
          />
          <TextField
            label="Email"
            value={data.sellerEmail}
            onChange={val => onChange({ sellerEmail: val.trim().toLowerCase() })}
            placeholder="correo@ejemplo.com"
          />
          <TextField
            label="WhatsApp"
            value={data.sellerWhatsApp}
            onChange={val =>
              onChange({ sellerWhatsApp: val.replace(/[^\d\-+() ]/g, '').slice(0, 15) })
            }
            placeholder="809-000-0000"
          />
        </div>
      </section>
    </div>
  );
}
