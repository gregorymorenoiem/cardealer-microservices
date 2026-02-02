/**
 * Sell Vehicle Wizard - Multi-step form to publish a vehicle
 *
 * Steps:
 * 1. Vehicle Info - Basic details, VIN decode
 * 2. Photos - Upload images
 * 3. Features - Select vehicle features
 * 4. Pricing - Set price and contact info
 * 5. Review - Final review and publish
 *
 * Route: /vender/publicar
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  Car,
  Camera,
  ListChecks,
  DollarSign,
  Eye,
  ArrowLeft,
  ArrowRight,
  Check,
  Save,
  Loader2,
  AlertCircle,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { cn } from '@/lib/utils';
import { toast } from 'sonner';

// Hooks for API integration
import {
  useMakes,
  useBodyTypes,
  useFuelTypes,
  useTransmissions,
  useColors,
  useProvinces,
  useFeatures,
  useCreateVehicle,
} from '@/hooks/use-vehicles';
import { useUploadImages, validateImageFile, type UploadProgress } from '@/hooks/use-media';
import type { CreateVehicleRequest, CreateVehicleImage } from '@/services/vehicles';

// =============================================================================
// TYPES
// =============================================================================

interface VehicleFormData {
  // Step 1: Vehicle Info
  make: string;
  model: string;
  year: number | null;
  trim: string;
  mileage: number | null;
  vin: string;
  transmission: string;
  fuelType: string;
  bodyType: string;
  exteriorColor: string;
  interiorColor: string;
  condition: string;

  // Step 2: Photos
  images: File[];

  // Step 3: Features
  features: string[];

  // Step 4: Pricing
  price: number | null;
  description: string;
  location: string;
  sellerName: string;
  sellerPhone: string;
  sellerEmail: string;
}

interface Step {
  id: number;
  name: string;
  description: string;
  icon: React.ElementType;
}

const steps: Step[] = [
  { id: 1, name: 'Información', description: 'Datos del vehículo', icon: Car },
  { id: 2, name: 'Fotos', description: 'Imágenes del vehículo', icon: Camera },
  { id: 3, name: 'Características', description: 'Equipamiento', icon: ListChecks },
  { id: 4, name: 'Precio', description: 'Precio y contacto', icon: DollarSign },
  { id: 5, name: 'Revisar', description: 'Publicar anuncio', icon: Eye },
];

const initialFormData: VehicleFormData = {
  make: '',
  model: '',
  year: null,
  trim: '',
  mileage: null,
  vin: '',
  transmission: '',
  fuelType: '',
  bodyType: '',
  exteriorColor: '',
  interiorColor: '',
  condition: 'used',
  images: [],
  features: [],
  price: null,
  description: '',
  location: '',
  sellerName: '',
  sellerPhone: '',
  sellerEmail: '',
};

// Static conditions (doesn't change often)
const conditions = ['Nuevo', 'Usado', 'Certificado'];

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function PublicarVehiculoPage() {
  const router = useRouter();

  // Form state
  const [currentStep, setCurrentStep] = React.useState(1);
  const [formData, setFormData] = React.useState<VehicleFormData>(initialFormData);
  const [imagePreviews, setImagePreviews] = React.useState<string[]>([]);
  const [uploadProgress, setUploadProgress] = React.useState<number[]>([]);
  const [submitError, setSubmitError] = React.useState<string | null>(null);

  // API Hooks for catalog data
  const { data: makesData, isLoading: makesLoading } = useMakes();
  const { data: bodyTypesData } = useBodyTypes();
  const { data: fuelTypesData } = useFuelTypes();
  const { data: transmissionsData } = useTransmissions();
  const { data: colorsData } = useColors();
  const { data: provincesData } = useProvinces();
  const { data: featuresData } = useFeatures();

  // Mutations
  const uploadImagesMutation = useUploadImages();
  const createVehicleMutation = useCreateVehicle();

  // Derived state
  const isSubmitting = uploadImagesMutation.isPending || createVehicleMutation.isPending;

  // Convert catalog data to arrays for selects
  const makes = makesData?.map(m => m.name) || [];
  const bodyTypes = bodyTypesData?.map(b => b.label) || [];
  const fuelTypes = fuelTypesData?.map(f => f.label) || [];
  const transmissions = transmissionsData?.map(t => t.label) || [];
  const colors = colorsData?.map(c => c.label) || [];
  const provinces = provincesData?.map(p => p.label) || [];
  const featureCategories = featuresData || {};

  // Load draft from localStorage on mount
  React.useEffect(() => {
    const draft = localStorage.getItem('sell-vehicle-draft');
    if (draft) {
      try {
        const parsed = JSON.parse(draft);
        // Don't restore files from localStorage
        setFormData(prev => ({ ...prev, ...parsed, images: [] }));
      } catch {
        // Invalid draft, ignore
      }
    }
  }, []);

  // Auto-save draft on form changes (debounced)
  React.useEffect(() => {
    const timeout = setTimeout(() => {
      const toSave = { ...formData, images: [] }; // Don't save files
      localStorage.setItem('sell-vehicle-draft', JSON.stringify(toSave));
    }, 1000);

    return () => clearTimeout(timeout);
  }, [formData]);

  const updateFormData = (data: Partial<VehicleFormData>) => {
    setFormData(prev => ({ ...prev, ...data }));
  };

  const nextStep = () => {
    if (currentStep < steps.length) {
      setCurrentStep(prev => prev + 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(prev => prev - 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    // Limit to 10 images
    const newImages = [...formData.images, ...files].slice(0, 10);
    updateFormData({ images: newImages });

    // Generate previews
    const newPreviews = newImages.map(file => URL.createObjectURL(file));
    setImagePreviews(newPreviews);
  };

  const removeImage = (index: number) => {
    const newImages = formData.images.filter((_, i) => i !== index);
    const newPreviews = imagePreviews.filter((_, i) => i !== index);
    updateFormData({ images: newImages });
    setImagePreviews(newPreviews);
  };

  const toggleFeature = (feature: string) => {
    const features = formData.features.includes(feature)
      ? formData.features.filter(f => f !== feature)
      : [...formData.features, feature];
    updateFormData({ features });
  };

  const handleSubmit = async () => {
    setSubmitError(null);

    try {
      // Step 1: Validate required fields
      if (!formData.make || !formData.model || !formData.year || !formData.price) {
        setSubmitError('Por favor completa todos los campos obligatorios.');
        return;
      }

      if (formData.images.length === 0) {
        setSubmitError('Debes subir al menos una foto del vehículo.');
        return;
      }

      // Step 2: Upload images to MediaService
      toast.info('Subiendo imágenes...');
      const uploadedImages = await uploadImagesMutation.mutateAsync({
        files: formData.images,
        folder: 'vehicles',
        onProgress: (index, progress) => {
          setUploadProgress(prev => {
            const updated = [...prev];
            updated[index] = progress.percentage;
            return updated;
          });
        },
      });

      // Step 3: Build the vehicle images array
      const vehicleImages: CreateVehicleImage[] = uploadedImages.map((img, index) => ({
        url: img.url,
        order: index,
        isPrimary: index === 0,
      }));

      // Step 4: Create vehicle via API
      toast.info('Creando publicación...');
      const vehicleData: CreateVehicleRequest = {
        make: formData.make,
        model: formData.model,
        year: formData.year,
        trim: formData.trim || undefined,
        mileage: formData.mileage || 0,
        vin: formData.vin || undefined,
        transmission: formData.transmission || 'automatic',
        fuelType: formData.fuelType || 'gasoline',
        bodyType: formData.bodyType || 'sedan',
        exteriorColor: formData.exteriorColor || undefined,
        interiorColor: formData.interiorColor || undefined,
        condition: formData.condition || 'used',
        price: formData.price,
        currency: 'DOP',
        description: formData.description || undefined,
        features: formData.features,
        images: vehicleImages,
        city: formData.location.split(',')[0]?.trim() || formData.location,
        province: formData.location,
        sellerName: formData.sellerName || undefined,
        sellerPhone: formData.sellerPhone || undefined,
        sellerEmail: formData.sellerEmail || undefined,
        isNegotiable: true,
      };

      const result = await createVehicleMutation.mutateAsync(vehicleData);

      // Success!
      toast.success('¡Vehículo publicado exitosamente!');

      // Clear draft
      localStorage.removeItem('sell-vehicle-draft');

      // Redirect to the vehicle detail or user's vehicles
      router.push(`/vehiculos/${result.slug}`);
    } catch (error) {
      console.error('Error submitting vehicle:', error);
      const message = error instanceof Error ? error.message : 'Error al publicar el vehículo';
      setSubmitError(message);
      toast.error(message);
    }
  };

  const clearDraft = () => {
    localStorage.removeItem('sell-vehicle-draft');
    setFormData(initialFormData);
    setImagePreviews([]);
    setCurrentStep(1);
  };

  // =============================================================================
  // RENDER STEP CONTENT
  // =============================================================================

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <div className="space-y-6">
            {/* VIN Input */}
            <div className="space-y-2">
              <Label htmlFor="vin">Número de VIN (opcional)</Label>
              <div className="flex gap-2">
                <Input
                  id="vin"
                  value={formData.vin}
                  onChange={e => updateFormData({ vin: e.target.value.toUpperCase() })}
                  placeholder="Ej: 1HGBH41JXMN109186"
                  maxLength={17}
                  className="font-mono"
                />
                <Button variant="outline" type="button">
                  Decodificar
                </Button>
              </div>
              <p className="text-xs text-gray-500">
                El VIN permite autocompletar los datos del vehículo automáticamente
              </p>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              {/* Make */}
              <div className="space-y-2">
                <Label>Marca *</Label>
                <Select
                  value={formData.make}
                  onValueChange={value => updateFormData({ make: value, model: '' })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar marca" />
                  </SelectTrigger>
                  <SelectContent>
                    {makes.map(make => (
                      <SelectItem key={make} value={make}>
                        {make}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Model */}
              <div className="space-y-2">
                <Label>Modelo *</Label>
                <Input
                  value={formData.model}
                  onChange={e => updateFormData({ model: e.target.value })}
                  placeholder="Ej: Corolla"
                />
              </div>

              {/* Year */}
              <div className="space-y-2">
                <Label>Año *</Label>
                <Select
                  value={formData.year?.toString() || ''}
                  onValueChange={value => updateFormData({ year: parseInt(value) })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar año" />
                  </SelectTrigger>
                  <SelectContent>
                    {Array.from({ length: 30 }, (_, i) => new Date().getFullYear() + 1 - i).map(
                      year => (
                        <SelectItem key={year} value={year.toString()}>
                          {year}
                        </SelectItem>
                      )
                    )}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              {/* Trim */}
              <div className="space-y-2">
                <Label>Versión/Trim</Label>
                <Input
                  value={formData.trim}
                  onChange={e => updateFormData({ trim: e.target.value })}
                  placeholder="Ej: LE, SE, XLE"
                />
              </div>

              {/* Mileage */}
              <div className="space-y-2">
                <Label>Kilometraje *</Label>
                <Input
                  type="number"
                  value={formData.mileage || ''}
                  onChange={e => updateFormData({ mileage: parseInt(e.target.value) || null })}
                  placeholder="Ej: 50000"
                />
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              {/* Transmission */}
              <div className="space-y-2">
                <Label>Transmisión</Label>
                <Select
                  value={formData.transmission}
                  onValueChange={value => updateFormData({ transmission: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {transmissions.map(t => (
                      <SelectItem key={t} value={t}>
                        {t}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Fuel Type */}
              <div className="space-y-2">
                <Label>Combustible</Label>
                <Select
                  value={formData.fuelType}
                  onValueChange={value => updateFormData({ fuelType: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {fuelTypes.map(f => (
                      <SelectItem key={f} value={f}>
                        {f}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Body Type */}
              <div className="space-y-2">
                <Label>Tipo de carrocería</Label>
                <Select
                  value={formData.bodyType}
                  onValueChange={value => updateFormData({ bodyType: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {bodyTypes.map(b => (
                      <SelectItem key={b} value={b}>
                        {b}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              {/* Condition */}
              <div className="space-y-2">
                <Label>Condición *</Label>
                <Select
                  value={formData.condition}
                  onValueChange={value => updateFormData({ condition: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {conditions.map(c => (
                      <SelectItem key={c} value={c}>
                        {c}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Exterior Color */}
              <div className="space-y-2">
                <Label>Color exterior *</Label>
                <Select
                  value={formData.exteriorColor}
                  onValueChange={value => updateFormData({ exteriorColor: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {colors.map(c => (
                      <SelectItem key={c} value={c}>
                        {c}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Interior Color */}
              <div className="space-y-2">
                <Label>Color interior</Label>
                <Select
                  value={formData.interiorColor}
                  onValueChange={value => updateFormData({ interiorColor: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Seleccionar" />
                  </SelectTrigger>
                  <SelectContent>
                    {colors.map(c => (
                      <SelectItem key={c} value={c}>
                        {c}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>
        );

      case 2:
        return (
          <div className="space-y-6">
            {/* Upload Zone */}
            <div
              className={cn(
                'relative rounded-lg border-2 border-dashed p-8 text-center transition-colors',
                formData.images.length >= 10
                  ? 'border-gray-200 bg-gray-50'
                  : 'cursor-pointer border-gray-300 hover:border-[#00A870] hover:bg-[#00A870]/5'
              )}
              onClick={() => {
                if (formData.images.length < 10) {
                  document.getElementById('image-upload')?.click();
                }
              }}
            >
              <input
                id="image-upload"
                type="file"
                accept="image/*"
                multiple
                className="hidden"
                onChange={handleImageUpload}
                disabled={formData.images.length >= 10}
              />
              <Camera className="mx-auto h-12 w-12 text-gray-400" />
              <p className="mt-2 text-sm font-medium text-gray-700">
                {formData.images.length >= 10
                  ? 'Límite de 10 fotos alcanzado'
                  : 'Haz clic o arrastra fotos aquí'}
              </p>
              <p className="mt-1 text-xs text-gray-500">
                Máximo 10 fotos • Formatos: JPG, PNG • La primera será la principal
              </p>
            </div>

            {/* Image Previews */}
            {imagePreviews.length > 0 && (
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-5">
                {imagePreviews.map((preview, index) => (
                  <div
                    key={index}
                    className="group relative aspect-[4/3] overflow-hidden rounded-lg bg-gray-100"
                  >
                    <img
                      src={preview}
                      alt={`Foto ${index + 1}`}
                      className="h-full w-full object-cover"
                    />
                    {index === 0 && (
                      <span className="absolute top-1 left-1 rounded bg-[#00A870] px-1.5 py-0.5 text-xs font-medium text-white">
                        Principal
                      </span>
                    )}
                    <button
                      onClick={e => {
                        e.stopPropagation();
                        removeImage(index);
                      }}
                      className="absolute top-1 right-1 flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-white opacity-0 transition-opacity group-hover:opacity-100"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}

            <p className="text-center text-sm text-gray-500">
              {formData.images.length} de 10 fotos
            </p>
          </div>
        );

      case 3:
        return (
          <div className="space-y-6">
            {Object.entries(featureCategories).map(([category, features]) => (
              <div key={category}>
                <h3 className="mb-3 text-sm font-semibold text-gray-700 capitalize">
                  {category === 'seguridad' && '🛡️ Seguridad'}
                  {category === 'confort' && '❄️ Confort'}
                  {category === 'entretenimiento' && '🎵 Entretenimiento'}
                  {category === 'rendimiento' && '🚗 Rendimiento'}
                </h3>
                <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 md:grid-cols-4">
                  {features.map(feature => (
                    <label
                      key={feature}
                      className={cn(
                        'flex cursor-pointer items-center gap-2 rounded-lg border p-3 text-sm transition-colors',
                        formData.features.includes(feature)
                          ? 'border-[#00A870] bg-[#00A870]/5 text-[#00A870]'
                          : 'border-gray-200 hover:border-gray-300'
                      )}
                    >
                      <input
                        type="checkbox"
                        checked={formData.features.includes(feature)}
                        onChange={() => toggleFeature(feature)}
                        className="sr-only"
                      />
                      <div
                        className={cn(
                          'flex h-4 w-4 items-center justify-center rounded border',
                          formData.features.includes(feature)
                            ? 'border-[#00A870] bg-[#00A870]'
                            : 'border-gray-300'
                        )}
                      >
                        {formData.features.includes(feature) && (
                          <Check className="h-3 w-3 text-white" />
                        )}
                      </div>
                      {feature}
                    </label>
                  ))}
                </div>
              </div>
            ))}

            <p className="text-center text-sm text-gray-500">
              {formData.features.length} características seleccionadas
            </p>
          </div>
        );

      case 4:
        return (
          <div className="space-y-6">
            {/* Price */}
            <div className="space-y-2">
              <Label htmlFor="price">Precio (RD$) *</Label>
              <Input
                id="price"
                type="number"
                value={formData.price || ''}
                onChange={e => updateFormData({ price: parseInt(e.target.value) || null })}
                placeholder="Ej: 1500000"
                className="text-lg font-semibold"
              />
              <p className="text-xs text-gray-500">Ingresa el precio sin separadores de miles</p>
            </div>

            {/* Description */}
            <div className="space-y-2">
              <Label htmlFor="description">Descripción *</Label>
              <textarea
                id="description"
                value={formData.description}
                onChange={e => updateFormData({ description: e.target.value })}
                placeholder="Describe el estado del vehículo, historial de mantenimiento, razón de venta, etc."
                rows={5}
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-[#00A870] focus:ring-1 focus:ring-[#00A870] focus:outline-none"
              />
              <p className="text-xs text-gray-500">
                {formData.description.length}/50 caracteres mínimo
              </p>
            </div>

            {/* Location */}
            <div className="space-y-2">
              <Label>Ubicación *</Label>
              <Select
                value={formData.location}
                onValueChange={value => updateFormData({ location: value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Seleccionar provincia" />
                </SelectTrigger>
                <SelectContent>
                  {provinces.map(p => (
                    <SelectItem key={p} value={p}>
                      {p}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Seller Info */}
            <div className="border-t pt-6">
              <h3 className="mb-4 text-sm font-semibold text-gray-700">Información de contacto</h3>
              <div className="grid gap-4 md:grid-cols-3">
                <div className="space-y-2">
                  <Label>Nombre *</Label>
                  <Input
                    value={formData.sellerName}
                    onChange={e => updateFormData({ sellerName: e.target.value })}
                    placeholder="Tu nombre"
                  />
                </div>
                <div className="space-y-2">
                  <Label>Teléfono *</Label>
                  <Input
                    value={formData.sellerPhone}
                    onChange={e => updateFormData({ sellerPhone: e.target.value })}
                    placeholder="809-000-0000"
                  />
                </div>
                <div className="space-y-2">
                  <Label>Email *</Label>
                  <Input
                    type="email"
                    value={formData.sellerEmail}
                    onChange={e => updateFormData({ sellerEmail: e.target.value })}
                    placeholder="tu@email.com"
                  />
                </div>
              </div>
            </div>
          </div>
        );

      case 5:
        return (
          <div className="space-y-6">
            {/* Vehicle Summary */}
            <Card>
              <CardContent className="p-6">
                <div className="flex gap-6">
                  {/* Main Image */}
                  <div className="w-48 flex-shrink-0">
                    {imagePreviews[0] ? (
                      <img
                        src={imagePreviews[0]}
                        alt="Vehículo"
                        className="aspect-[4/3] w-full rounded-lg object-cover"
                      />
                    ) : (
                      <div className="flex aspect-[4/3] w-full items-center justify-center rounded-lg bg-gray-100">
                        <Camera className="h-8 w-8 text-gray-400" />
                      </div>
                    )}
                  </div>

                  {/* Info */}
                  <div className="flex-1">
                    <h2 className="text-xl font-bold text-gray-900">
                      {formData.year} {formData.make} {formData.model} {formData.trim}
                    </h2>
                    <p className="mt-1 text-2xl font-bold text-[#00A870]">
                      RD$ {formData.price?.toLocaleString() || '—'}
                    </p>
                    <div className="mt-3 flex flex-wrap gap-2 text-sm text-gray-600">
                      {formData.mileage && <span>{formData.mileage.toLocaleString()} km</span>}
                      {formData.transmission && <span>• {formData.transmission}</span>}
                      {formData.fuelType && <span>• {formData.fuelType}</span>}
                      {formData.location && <span>• {formData.location}</span>}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Checklist */}
            <div className="space-y-3">
              <h3 className="font-semibold text-gray-900">Verificación del anuncio</h3>
              <div className="space-y-2">
                <ChecklistItem
                  label="Información básica completa"
                  checked={!!(formData.make && formData.model && formData.year)}
                />
                <ChecklistItem label="Al menos 1 foto" checked={formData.images.length > 0} />
                <ChecklistItem label="Precio establecido" checked={!!formData.price} />
                <ChecklistItem
                  label="Descripción mínima (50 caracteres)"
                  checked={formData.description.length >= 50}
                />
                <ChecklistItem
                  label="Información de contacto"
                  checked={!!(formData.sellerName && formData.sellerPhone && formData.sellerEmail)}
                />
              </div>
            </div>

            {/* Features Summary */}
            {formData.features.length > 0 && (
              <div>
                <h3 className="mb-2 font-semibold text-gray-900">
                  Características ({formData.features.length})
                </h3>
                <div className="flex flex-wrap gap-2">
                  {formData.features.map(f => (
                    <span
                      key={f}
                      className="rounded-full bg-gray-100 px-3 py-1 text-sm text-gray-700"
                    >
                      {f}
                    </span>
                  ))}
                </div>
              </div>
            )}
          </div>
        );

      default:
        return null;
    }
  };

  // =============================================================================
  // RENDER
  // =============================================================================

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="container mx-auto max-w-3xl px-4">
        {/* Header */}
        <div className="mb-8">
          <Link
            href="/vender"
            className="inline-flex items-center gap-1 text-sm text-gray-600 hover:text-gray-900"
          >
            <ArrowLeft className="h-4 w-4" />
            Volver
          </Link>
          <h1 className="mt-4 text-2xl font-bold text-gray-900">Publicar vehículo</h1>
          <p className="text-gray-600">Completa los datos de tu vehículo</p>
        </div>

        {/* Stepper */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            {steps.map((step, index) => {
              const Icon = step.icon;
              const isCompleted = currentStep > step.id;
              const isCurrent = currentStep === step.id;

              return (
                <React.Fragment key={step.id}>
                  <div className="flex flex-col items-center">
                    <div
                      className={cn(
                        'flex h-10 w-10 items-center justify-center rounded-full border-2 transition-colors',
                        isCompleted
                          ? 'border-[#00A870] bg-[#00A870] text-white'
                          : isCurrent
                            ? 'border-[#00A870] bg-white text-[#00A870]'
                            : 'border-gray-300 bg-white text-gray-400'
                      )}
                    >
                      {isCompleted ? <Check className="h-5 w-5" /> : <Icon className="h-5 w-5" />}
                    </div>
                    <span
                      className={cn(
                        'mt-2 text-xs font-medium',
                        isCurrent ? 'text-[#00A870]' : 'text-gray-500'
                      )}
                    >
                      {step.name}
                    </span>
                  </div>

                  {/* Connector Line */}
                  {index < steps.length - 1 && (
                    <div
                      className={cn(
                        'h-0.5 flex-1 transition-colors',
                        currentStep > step.id ? 'bg-[#00A870]' : 'bg-gray-200'
                      )}
                    />
                  )}
                </React.Fragment>
              );
            })}
          </div>
        </div>

        {/* Step Content */}
        <Card className="mb-6">
          <CardContent className="p-6">{renderStepContent()}</CardContent>
        </Card>

        {/* Navigation Buttons */}
        <div className="flex items-center justify-between">
          <div className="flex gap-2">
            {currentStep > 1 && (
              <Button variant="outline" onClick={prevStep} className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Anterior
              </Button>
            )}
            <Button variant="ghost" onClick={clearDraft} className="gap-2 text-gray-500">
              <Save className="h-4 w-4" />
              Limpiar borrador
            </Button>
          </div>

          {currentStep < steps.length ? (
            <Button onClick={nextStep} className="gap-2 bg-[#00A870] hover:bg-[#009663]">
              Siguiente
              <ArrowRight className="h-4 w-4" />
            </Button>
          ) : (
            <Button
              onClick={handleSubmit}
              disabled={isSubmitting}
              className="gap-2 bg-[#00A870] hover:bg-[#009663]"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Publicando...
                </>
              ) : (
                <>
                  Publicar anuncio
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </Button>
          )}
        </div>

        {/* Step Progress */}
        <p className="mt-4 text-center text-sm text-gray-500">
          Paso {currentStep} de {steps.length}
        </p>
      </div>
    </div>
  );
}

// =============================================================================
// HELPER COMPONENTS
// =============================================================================

function ChecklistItem({ label, checked }: { label: string; checked: boolean }) {
  return (
    <div className="flex items-center gap-2">
      <div
        className={cn(
          'flex h-5 w-5 items-center justify-center rounded-full',
          checked ? 'bg-[#00A870]' : 'bg-gray-200'
        )}
      >
        {checked && <Check className="h-3 w-3 text-white" />}
      </div>
      <span className={cn('text-sm', checked ? 'text-gray-900' : 'text-gray-500')}>{label}</span>
    </div>
  );
}
