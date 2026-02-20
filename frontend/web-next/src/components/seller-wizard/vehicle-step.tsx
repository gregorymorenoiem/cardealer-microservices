/**
 * Seller Wizard - Step 3: Publish First Vehicle
 *
 * Multi-section form for creating the first vehicle listing.
 * Uses catalog data from VehiclesSaleService via hooks.
 * Images uploaded via PhotoUploader component.
 */

'use client';

import * as React from 'react';
import { Loader2, AlertCircle, Car, Camera, DollarSign, FileText, MapPin } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Separator } from '@/components/ui/separator';
import {
  useMakes,
  useBodyTypes,
  useFuelTypes,
  useTransmissions,
  useColors,
  useFeatures,
} from '@/hooks/use-vehicles';
import { PhotoUploader, type UploadedImage } from './photo-uploader';
import { RD_PROVINCES, type VehicleFormData } from '@/lib/validations/seller-onboarding';

// =============================================================================
// TYPES
// =============================================================================

interface VehicleStepProps {
  data: VehicleFormData;
  onChange: (data: Partial<VehicleFormData>) => void;
  images: UploadedImage[];
  onImagesChange: (images: UploadedImage[]) => void;
  onSubmit: () => void;
  onBack: () => void;
  isLoading: boolean;
  error: string | null;
}

// Year range
const currentYear = new Date().getFullYear();
const years = Array.from({ length: currentYear + 2 - 1990 + 1 }, (_, i) => currentYear + 2 - i);

// =============================================================================
// COMPONENT
// =============================================================================

export function VehicleStep({
  data,
  onChange,
  images,
  onImagesChange,
  onSubmit,
  onBack,
  isLoading,
  error,
}: VehicleStepProps) {
  // Catalog data from backend (with static fallbacks)
  const { data: makes = [] } = useMakes();
  const { data: bodyTypes = [] } = useBodyTypes();
  const { data: fuelTypes = [] } = useFuelTypes();
  const { data: transmissions = [] } = useTransmissions();
  const { data: colors = [] } = useColors();
  useFeatures(); // Pre-fetch for later use

  const doneImages = images.filter(i => i.status === 'done');
  const hasMinImages = doneImages.length >= 3;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit();
  };

  const handleNumberChange = (field: keyof VehicleFormData, value: string) => {
    const num = value === '' ? undefined : Number(value.replace(/[^0-9]/g, ''));
    onChange({ [field]: num } as Partial<VehicleFormData>);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-8">
      {/* ── Section: Vehicle Info ── */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Car className="h-5 w-5 text-[#00A870]" />
          <h3 className="text-lg font-semibold">Información del vehículo</h3>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {/* Make */}
          <div className="space-y-2">
            <Label>Marca *</Label>
            <Select value={data.make} onValueChange={v => onChange({ make: v, model: '' })}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar marca" />
              </SelectTrigger>
              <SelectContent>
                {(makes.length > 0 ? makes : DEFAULT_MAKES).map(m => (
                  <SelectItem
                    key={typeof m === 'string' ? m : m.name}
                    value={typeof m === 'string' ? m : m.name}
                  >
                    {typeof m === 'string' ? m : m.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Model */}
          <div className="space-y-2">
            <Label>Modelo *</Label>
            <Input
              value={data.model}
              onChange={e => onChange({ model: e.target.value })}
              placeholder="Ej: Corolla, Civic, RAV4..."
              required
              maxLength={100}
            />
          </div>

          {/* Year */}
          <div className="space-y-2">
            <Label>Año *</Label>
            <Select
              value={data.year?.toString() ?? ''}
              onValueChange={v => onChange({ year: Number(v) })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar año" />
              </SelectTrigger>
              <SelectContent>
                {years.map(y => (
                  <SelectItem key={y} value={y.toString()}>
                    {y}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Trim */}
          <div className="space-y-2">
            <Label>
              Versión / Trim <span className="text-muted-foreground">(opcional)</span>
            </Label>
            <Input
              value={data.trim ?? ''}
              onChange={e => onChange({ trim: e.target.value })}
              placeholder="Ej: LE, XLE, Sport..."
              maxLength={50}
            />
          </div>

          {/* Body Type */}
          <div className="space-y-2">
            <Label>Tipo de carrocería *</Label>
            <Select value={data.bodyType} onValueChange={v => onChange({ bodyType: v })}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar tipo" />
              </SelectTrigger>
              <SelectContent>
                {(bodyTypes.length > 0 ? bodyTypes : DEFAULT_BODY_TYPES).map(bt => (
                  <SelectItem key={bt.value} value={bt.value}>
                    {bt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Transmission */}
          <div className="space-y-2">
            <Label>Transmisión *</Label>
            <Select value={data.transmission} onValueChange={v => onChange({ transmission: v })}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar transmisión" />
              </SelectTrigger>
              <SelectContent>
                {(transmissions.length > 0 ? transmissions : DEFAULT_TRANSMISSIONS).map(t => (
                  <SelectItem key={t.value} value={t.value}>
                    {t.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Fuel Type */}
          <div className="space-y-2">
            <Label>Combustible *</Label>
            <Select value={data.fuelType} onValueChange={v => onChange({ fuelType: v })}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar combustible" />
              </SelectTrigger>
              <SelectContent>
                {(fuelTypes.length > 0 ? fuelTypes : DEFAULT_FUEL_TYPES).map(ft => (
                  <SelectItem key={ft.value} value={ft.value}>
                    {ft.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Condition */}
          <div className="space-y-2">
            <Label>Condición *</Label>
            <Select
              value={data.condition}
              onValueChange={v => onChange({ condition: v as 'new' | 'used' | 'certified' })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar condición" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="new">Nuevo</SelectItem>
                <SelectItem value="used">Usado</SelectItem>
                <SelectItem value="certified">Certificado</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Mileage */}
          <div className="space-y-2">
            <Label>Kilometraje *</Label>
            <Input
              type="text"
              inputMode="numeric"
              value={data.mileage?.toLocaleString('es-DO') ?? ''}
              onChange={e => handleNumberChange('mileage', e.target.value)}
              placeholder="Ej: 45,000"
              required
            />
            <p className="text-muted-foreground text-xs">En kilómetros</p>
          </div>

          {/* VIN */}
          <div className="space-y-2">
            <Label>
              VIN <span className="text-muted-foreground">(opcional)</span>
            </Label>
            <Input
              value={data.vin ?? ''}
              onChange={e => onChange({ vin: e.target.value.toUpperCase().slice(0, 17) })}
              placeholder="17 caracteres"
              maxLength={17}
              className="font-mono"
            />
          </div>

          {/* Exterior Color */}
          <div className="space-y-2">
            <Label>
              Color exterior <span className="text-muted-foreground">(opcional)</span>
            </Label>
            <Select
              value={data.exteriorColor ?? ''}
              onValueChange={v => onChange({ exteriorColor: v })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar color" />
              </SelectTrigger>
              <SelectContent>
                {(colors.length > 0 ? colors : DEFAULT_COLORS).map(c => (
                  <SelectItem key={c.value} value={c.value}>
                    {c.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </section>

      <Separator />

      {/* ── Section: Photos ── */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <Camera className="h-5 w-5 text-[#00A870]" />
          <h3 className="text-lg font-semibold">Fotos del vehículo</h3>
        </div>
        <p className="text-muted-foreground text-sm">
          Las buenas fotos venden más rápido. Incluye exterior, interior, motor y detalles.
        </p>
        <PhotoUploader images={images} onImagesChange={onImagesChange} disabled={isLoading} />
      </section>

      <Separator />

      {/* ── Section: Pricing ── */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <DollarSign className="h-5 w-5 text-[#00A870]" />
          <h3 className="text-lg font-semibold">Precio</h3>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {/* Price */}
          <div className="space-y-2">
            <Label>Precio *</Label>
            <div className="relative">
              <span className="text-muted-foreground absolute top-1/2 left-3 -translate-y-1/2 text-sm">
                {data.currency === 'USD' ? 'US$' : 'RD$'}
              </span>
              <Input
                type="text"
                inputMode="numeric"
                value={data.price?.toLocaleString('es-DO') ?? ''}
                onChange={e => handleNumberChange('price', e.target.value)}
                placeholder="0"
                required
                className="pl-12"
              />
            </div>
          </div>

          {/* Currency */}
          <div className="space-y-2">
            <Label>Moneda</Label>
            <Select
              value={data.currency}
              onValueChange={v => onChange({ currency: v as 'DOP' | 'USD' })}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="DOP">Pesos Dominicanos (RD$)</SelectItem>
                <SelectItem value="USD">Dólares (US$)</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Negotiable */}
        <div className="flex items-center gap-2">
          <Checkbox
            id="isNegotiable"
            checked={data.isNegotiable}
            onCheckedChange={checked => onChange({ isNegotiable: checked === true })}
          />
          <Label htmlFor="isNegotiable" className="cursor-pointer text-sm">
            Precio negociable
          </Label>
        </div>
      </section>

      <Separator />

      {/* ── Section: Description ── */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <FileText className="h-5 w-5 text-[#00A870]" />
          <h3 className="text-lg font-semibold">Descripción</h3>
        </div>

        <div className="space-y-2">
          <Textarea
            value={data.description}
            onChange={e => onChange({ description: e.target.value })}
            placeholder="Describe tu vehículo: estado general, historial de mantenimiento, razón de venta, extras incluidos..."
            rows={5}
            required
            minLength={20}
            maxLength={5000}
          />
          <p className="text-muted-foreground text-right text-xs">
            {data.description.length}/5000 (mínimo 20)
          </p>
        </div>
      </section>

      <Separator />

      {/* ── Section: Location ── */}
      <section className="space-y-4">
        <div className="flex items-center gap-2">
          <MapPin className="h-5 w-5 text-[#00A870]" />
          <h3 className="text-lg font-semibold">Ubicación</h3>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {/* Province */}
          <div className="space-y-2">
            <Label>Provincia *</Label>
            <Select value={data.province} onValueChange={v => onChange({ province: v })}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar provincia" />
              </SelectTrigger>
              <SelectContent>
                {RD_PROVINCES.map(p => (
                  <SelectItem key={p} value={p}>
                    {p}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* City */}
          <div className="space-y-2">
            <Label>Ciudad *</Label>
            <Input
              value={data.city}
              onChange={e => onChange({ city: e.target.value })}
              placeholder="Ej: Santo Domingo, Santiago..."
              required
              maxLength={100}
            />
          </div>
        </div>
      </section>

      <Separator />

      {/* ── Section: Contact ── */}
      <section className="space-y-4">
        <h3 className="text-lg font-semibold">Contacto para esta publicación</h3>
        <p className="text-muted-foreground text-sm">
          Opcional: si dejas vacío, usaremos los datos de tu perfil.
        </p>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <Label>Teléfono</Label>
            <Input
              value={data.sellerPhone ?? ''}
              onChange={e =>
                onChange({ sellerPhone: e.target.value.replace(/\D/g, '').slice(0, 10) })
              }
              placeholder="8091234567"
              maxLength={10}
            />
          </div>
          <div className="space-y-2">
            <Label>Email</Label>
            <Input
              type="email"
              value={data.sellerEmail ?? ''}
              onChange={e => onChange({ sellerEmail: e.target.value })}
              placeholder="tu@email.com"
              maxLength={254}
            />
          </div>
        </div>
      </section>

      {/* Error */}
      {error && (
        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-red-600" />
          <p className="text-sm text-red-700">{error}</p>
        </div>
      )}

      {/* Validation warnings */}
      {!hasMinImages && images.length > 0 && (
        <div className="flex items-start gap-2 rounded-lg border border-amber-200 bg-amber-50 p-3">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-amber-600" />
          <p className="text-sm text-amber-700">
            Necesitas al menos 3 fotos para publicar tu vehículo.
          </p>
        </div>
      )}

      {/* Actions */}
      <div className="flex gap-3">
        <Button type="button" variant="outline" onClick={onBack} disabled={isLoading}>
          Atrás
        </Button>
        <Button
          type="submit"
          className="flex-1 bg-[#00A870] hover:bg-[#009663]"
          disabled={
            isLoading ||
            !data.make ||
            !data.model ||
            !data.year ||
            !data.bodyType ||
            !data.transmission ||
            !data.fuelType ||
            !data.condition ||
            !data.mileage ||
            !data.price ||
            !data.description ||
            data.description.length < 20 ||
            !data.province ||
            !data.city ||
            !hasMinImages
          }
        >
          {isLoading ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Publicando vehículo...
            </>
          ) : (
            '🚗 Publicar vehículo'
          )}
        </Button>
      </div>
    </form>
  );
}

// =============================================================================
// STATIC FALLBACKS (used when catalog API is unavailable)
// =============================================================================

const DEFAULT_MAKES = [
  'Toyota',
  'Honda',
  'Hyundai',
  'Kia',
  'Nissan',
  'Mitsubishi',
  'Chevrolet',
  'Ford',
  'Jeep',
  'BMW',
  'Mercedes-Benz',
  'Audi',
  'Volkswagen',
  'Mazda',
  'Subaru',
  'Lexus',
  'Suzuki',
  'Isuzu',
  'Dodge',
  'Chrysler',
  'RAM',
  'GMC',
  'Cadillac',
  'Acura',
  'Infiniti',
  'Volvo',
  'Land Rover',
  'Porsche',
  'Mini',
  'Fiat',
];

const DEFAULT_BODY_TYPES = [
  { value: 'sedan', label: 'Sedán' },
  { value: 'suv', label: 'SUV / Yipeta' },
  { value: 'pickup', label: 'Pickup' },
  { value: 'hatchback', label: 'Hatchback' },
  { value: 'coupe', label: 'Coupé' },
  { value: 'convertible', label: 'Convertible' },
  { value: 'minivan', label: 'Minivan' },
  { value: 'wagon', label: 'Wagon' },
  { value: 'crossover', label: 'Crossover' },
  { value: 'sport', label: 'Deportivo' },
];

const DEFAULT_TRANSMISSIONS = [
  { value: 'automatic', label: 'Automática' },
  { value: 'manual', label: 'Manual' },
  { value: 'cvt', label: 'CVT' },
];

const DEFAULT_FUEL_TYPES = [
  { value: 'gasoline', label: 'Gasolina' },
  { value: 'diesel', label: 'Diésel' },
  { value: 'hybrid', label: 'Híbrido' },
  { value: 'electric', label: 'Eléctrico' },
  { value: 'lpg', label: 'GLP' },
];

const DEFAULT_COLORS = [
  { value: 'white', label: 'Blanco' },
  { value: 'black', label: 'Negro' },
  { value: 'gray', label: 'Gris' },
  { value: 'silver', label: 'Plata' },
  { value: 'red', label: 'Rojo' },
  { value: 'blue', label: 'Azul' },
  { value: 'green', label: 'Verde' },
  { value: 'brown', label: 'Marrón' },
  { value: 'beige', label: 'Beige' },
  { value: 'gold', label: 'Dorado' },
  { value: 'orange', label: 'Naranja' },
  { value: 'yellow', label: 'Amarillo' },
];
