/**
 * Vehicle Search Filters Component
 *
 * Renders filter controls for vehicle search
 * Can be used in sidebar or sheet/modal on mobile
 */

'use client';

import * as React from 'react';
import { X, ChevronDown, RotateCcw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Slider } from '@/components/ui/slider';
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { cn } from '@/lib/utils';
import type { VehicleSearchFilters } from '@/hooks/use-vehicle-search';

// =============================================================================
// TYPES
// =============================================================================

export interface VehicleFiltersProps {
  filters: VehicleSearchFilters;
  onChange: (filters: Partial<VehicleSearchFilters>) => void;
  onClear: () => void;
  activeCount: number;
  facets?: {
    makes: { value: string; count: number }[];
    bodyTypes: { value: string; count: number }[];
    provinces: { value: string; count: number }[];
    fuelTypes: { value: string; count: number }[];
    transmissions: { value: string; count: number }[];
  };
  className?: string;
}

// =============================================================================
// DATA
// =============================================================================

const currentYear = new Date().getFullYear();
const years = Array.from({ length: 30 }, (_, i) => currentYear - i);

const defaultMakes = [
  { value: 'Toyota', count: 0 },
  { value: 'Honda', count: 0 },
  { value: 'Hyundai', count: 0 },
  { value: 'Nissan', count: 0 },
  { value: 'Kia', count: 0 },
  { value: 'Mazda', count: 0 },
  { value: 'Ford', count: 0 },
  { value: 'Chevrolet', count: 0 },
  { value: 'Mercedes-Benz', count: 0 },
  { value: 'BMW', count: 0 },
];

const defaultBodyTypes = [
  { value: 'Sedán', count: 0 },
  { value: 'SUV', count: 0 },
  { value: 'Pickup', count: 0 },
  { value: 'Hatchback', count: 0 },
  { value: 'Coupé', count: 0 },
  { value: 'Convertible', count: 0 },
  { value: 'Van', count: 0 },
  { value: 'Wagon', count: 0 },
];

const defaultProvinces = [
  { value: 'Santo Domingo', count: 0 },
  { value: 'Distrito Nacional', count: 0 },
  { value: 'Santiago', count: 0 },
  { value: 'La Romana', count: 0 },
  { value: 'Puerto Plata', count: 0 },
  { value: 'San Pedro de Macorís', count: 0 },
  { value: 'La Vega', count: 0 },
  { value: 'San Cristóbal', count: 0 },
];

const fuelTypes = [
  { value: 'gasolina', label: 'Gasolina' },
  { value: 'diesel', label: 'Diesel' },
  { value: 'hibrido', label: 'Híbrido' },
  { value: 'electrico', label: 'Eléctrico' },
  { value: 'glp', label: 'GLP' },
];

const transmissions = [
  { value: 'automatica', label: 'Automática' },
  { value: 'manual', label: 'Manual' },
  { value: 'cvt', label: 'CVT' },
];

const dealRatings = [
  { value: 'great', label: 'Excelente precio', color: 'bg-green-500' },
  { value: 'good', label: 'Buen precio', color: 'bg-blue-500' },
  { value: 'fair', label: 'Precio justo', color: 'bg-yellow-500' },
];

// =============================================================================
// FILTER SECTION COMPONENT
// =============================================================================

function FilterSection({
  title,
  children,
  defaultOpen = true,
}: {
  title: string;
  children: React.ReactNode;
  defaultOpen?: boolean;
}) {
  return (
    <AccordionItem value={title.toLowerCase().replace(/\s/g, '-')} className="border-b">
      <AccordionTrigger className="py-3 text-sm font-medium">{title}</AccordionTrigger>
      <AccordionContent className="pb-4">{children}</AccordionContent>
    </AccordionItem>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export function VehicleFilters({
  filters,
  onChange,
  onClear,
  activeCount,
  facets,
  className,
}: VehicleFiltersProps) {
  const makes = facets?.makes?.length ? facets.makes : defaultMakes;
  const bodyTypes = facets?.bodyTypes?.length ? facets.bodyTypes : defaultBodyTypes;
  const provinces = facets?.provinces?.length ? facets.provinces : defaultProvinces;

  // Price range state
  const [priceRange, setPriceRange] = React.useState<[number, number]>([
    filters.priceMin || 0,
    filters.priceMax || 5000000,
  ]);

  // Mileage state
  const [mileageMax, setMileageMax] = React.useState(filters.mileageMax || 200000);

  // Debounce price/mileage changes
  React.useEffect(() => {
    const timer = setTimeout(() => {
      onChange({
        priceMin: priceRange[0] > 0 ? priceRange[0] : undefined,
        priceMax: priceRange[1] < 5000000 ? priceRange[1] : undefined,
      });
    }, 500);
    return () => clearTimeout(timer);
  }, [priceRange, onChange]);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      onChange({ mileageMax: mileageMax < 200000 ? mileageMax : undefined });
    }, 500);
    return () => clearTimeout(timer);
  }, [mileageMax, onChange]);

  return (
    <div className={cn('space-y-4', className)}>
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-foreground">Filtros</h2>
        {activeCount > 0 && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onClear}
            className="gap-1 text-sm text-muted-foreground hover:text-foreground"
          >
            <RotateCcw className="h-3.5 w-3.5" />
            Limpiar ({activeCount})
          </Button>
        )}
      </div>

      <Separator />

      {/* Filters */}
      <Accordion
        type="multiple"
        defaultValue={[
          'marca',
          'precio',
          'año',
          'carroceria',
          'ubicacion',
          'combustible',
          'transmision',
          'deal-rating',
        ]}
        className="w-full"
      >
        {/* Make Filter */}
        <FilterSection title="Marca">
          <div className="max-h-48 space-y-2 overflow-y-auto">
            {makes.map(make => (
              <label key={make.value} className="flex cursor-pointer items-center gap-2 text-sm">
                <Checkbox
                  checked={filters.make === make.value}
                  onCheckedChange={checked => {
                    onChange({ make: checked ? make.value : undefined, model: undefined });
                  }}
                />
                <span className="flex-1">{make.value}</span>
                {make.count > 0 && <span className="text-xs text-muted-foreground">({make.count})</span>}
              </label>
            ))}
          </div>
        </FilterSection>

        {/* Price Filter */}
        <FilterSection title="Precio">
          <div className="space-y-4">
            <Slider
              value={priceRange}
              onValueChange={value => setPriceRange(value as [number, number])}
              min={0}
              max={5000000}
              step={50000}
              className="w-full"
            />
            <div className="flex items-center gap-2 text-sm">
              <span className="text-muted-foreground">RD$</span>
              <Input
                type="number"
                value={priceRange[0]}
                onChange={e => setPriceRange([parseInt(e.target.value) || 0, priceRange[1]])}
                className="h-8"
                placeholder="Min"
              />
              <span className="text-muted-foreground">—</span>
              <Input
                type="number"
                value={priceRange[1]}
                onChange={e => setPriceRange([priceRange[0], parseInt(e.target.value) || 5000000])}
                className="h-8"
                placeholder="Max"
              />
            </div>
          </div>
        </FilterSection>

        {/* Year Filter */}
        <FilterSection title="Año">
          <div className="flex items-center gap-2">
            <select
              value={filters.yearMin || ''}
              onChange={e =>
                onChange({ yearMin: e.target.value ? parseInt(e.target.value) : undefined })
              }
              className="border-input bg-background flex h-9 w-full rounded-md border px-3 py-1 text-sm"
            >
              <option value="">Desde</option>
              {years.map(year => (
                <option key={year} value={year}>
                  {year}
                </option>
              ))}
            </select>
            <span className="text-muted-foreground">—</span>
            <select
              value={filters.yearMax || ''}
              onChange={e =>
                onChange({ yearMax: e.target.value ? parseInt(e.target.value) : undefined })
              }
              className="border-input bg-background flex h-9 w-full rounded-md border px-3 py-1 text-sm"
            >
              <option value="">Hasta</option>
              {years.map(year => (
                <option key={year} value={year}>
                  {year}
                </option>
              ))}
            </select>
          </div>
        </FilterSection>

        {/* Body Type Filter */}
        <FilterSection title="Carrocería">
          <div className="flex flex-wrap gap-2">
            {bodyTypes.map(type => (
              <Badge
                key={type.value}
                variant={filters.bodyType === type.value ? 'default' : 'outline'}
                className={cn(
                  'cursor-pointer',
                  filters.bodyType === type.value && 'bg-[#00A870] hover:bg-[#009663]'
                )}
                onClick={() =>
                  onChange({ bodyType: filters.bodyType === type.value ? undefined : type.value })
                }
              >
                {type.value}
              </Badge>
            ))}
          </div>
        </FilterSection>

        {/* Location Filter */}
        <FilterSection title="Ubicación">
          <select
            value={filters.province || ''}
            onChange={e => onChange({ province: e.target.value || undefined })}
            className="border-input bg-background flex h-9 w-full rounded-md border px-3 py-1 text-sm"
          >
            <option value="">Todas las provincias</option>
            {provinces.map(prov => (
              <option key={prov.value} value={prov.value}>
                {prov.value} {prov.count > 0 && `(${prov.count})`}
              </option>
            ))}
          </select>
        </FilterSection>

        {/* Mileage Filter */}
        <FilterSection title="Kilometraje">
          <div className="space-y-4">
            <Slider
              value={[mileageMax]}
              onValueChange={value => setMileageMax(value[0])}
              min={0}
              max={200000}
              step={5000}
              className="w-full"
            />
            <div className="flex items-center justify-between text-sm text-muted-foreground">
              <span>0 km</span>
              <span className="font-medium text-foreground">{mileageMax.toLocaleString()} km</span>
            </div>
          </div>
        </FilterSection>

        {/* Fuel Type Filter */}
        <FilterSection title="Combustible">
          <div className="space-y-2">
            {fuelTypes.map(fuel => (
              <label key={fuel.value} className="flex cursor-pointer items-center gap-2 text-sm">
                <Checkbox
                  checked={filters.fuelType === fuel.value}
                  onCheckedChange={checked => {
                    onChange({
                      fuelType: checked
                        ? (fuel.value as VehicleSearchFilters['fuelType'])
                        : undefined,
                    });
                  }}
                />
                <span>{fuel.label}</span>
              </label>
            ))}
          </div>
        </FilterSection>

        {/* Transmission Filter */}
        <FilterSection title="Transmisión">
          <div className="space-y-2">
            {transmissions.map(trans => (
              <label key={trans.value} className="flex cursor-pointer items-center gap-2 text-sm">
                <Checkbox
                  checked={filters.transmission === trans.value}
                  onCheckedChange={checked => {
                    onChange({
                      transmission: checked
                        ? (trans.value as VehicleSearchFilters['transmission'])
                        : undefined,
                    });
                  }}
                />
                <span>{trans.label}</span>
              </label>
            ))}
          </div>
        </FilterSection>

        {/* Deal Rating Filter */}
        <FilterSection title="Valoración de precio">
          <div className="space-y-2">
            {dealRatings.map(rating => (
              <label key={rating.value} className="flex cursor-pointer items-center gap-2 text-sm">
                <Checkbox
                  checked={filters.dealRating === rating.value}
                  onCheckedChange={checked => {
                    onChange({
                      dealRating: checked
                        ? (rating.value as VehicleSearchFilters['dealRating'])
                        : undefined,
                    });
                  }}
                />
                <span className={cn('h-2 w-2 rounded-full', rating.color)} />
                <span>{rating.label}</span>
              </label>
            ))}
          </div>
        </FilterSection>

        {/* Seller Type Filter */}
        <FilterSection title="Tipo de vendedor">
          <div className="space-y-2">
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <Checkbox
                checked={filters.sellerType === 'dealer'}
                onCheckedChange={checked => {
                  onChange({ sellerType: checked ? 'dealer' : undefined });
                }}
              />
              <span>Dealers</span>
            </label>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <Checkbox
                checked={filters.sellerType === 'particular'}
                onCheckedChange={checked => {
                  onChange({ sellerType: checked ? 'particular' : undefined });
                }}
              />
              <span>Particulares</span>
            </label>
          </div>
        </FilterSection>
      </Accordion>
    </div>
  );
}

export default VehicleFilters;
