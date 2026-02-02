/**
 * Publicar Vehículo Page
 *
 * Multi-step wizard for creating a vehicle listing
 * Connected to real APIs - February 2026
 */

'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Car,
  Camera,
  DollarSign,
  FileText,
  Check,
  ChevronRight,
  ChevronLeft,
  Info,
  AlertCircle,
  Loader2,
  Upload,
  X,
  GripVertical,
  Trash2,
} from 'lucide-react';
import Link from 'next/link';
import Image from 'next/image';
import { toast } from 'sonner';
import { useMakes, useModelsByMake, useCreateVehicle } from '@/hooks/use-vehicles';
import { uploadImage, uploadImages, type UploadProgress } from '@/services/media';

// =============================================================================
// TYPES
// =============================================================================

interface VehicleFormData {
  makeId: string;
  modelId: string;
  year: string;
  version: string;
  mileage: string;
  fuelType: string;
  transmission: string;
  color: string;
  condition: string;
  doors: string;
  seats: string;
  description: string;
  price: string;
  currency: string;
  negotiable: boolean;
  acceptTrades: boolean;
  province: string;
  city: string;
  features: string[];
  photos: UploadedPhoto[];
}

interface UploadedPhoto {
  id: string;
  url: string;
  category: string;
  isPrimary: boolean;
}

const steps = [
  { id: 1, title: 'Información Básica', icon: Car },
  { id: 2, title: 'Fotos', icon: Camera },
  { id: 3, title: 'Precio y Detalles', icon: DollarSign },
  { id: 4, title: 'Revisión', icon: FileText },
];

const fuelTypes = ['Gasolina', 'Diesel', 'Híbrido', 'Eléctrico', 'GLP'];
const transmissions = ['Automática', 'Manual', 'CVT', 'Dual Clutch'];
const colors = [
  'Blanco',
  'Negro',
  'Gris',
  'Plata',
  'Rojo',
  'Azul',
  'Verde',
  'Marrón',
  'Dorado',
  'Otro',
];
const conditions = ['Nuevo', 'Usado - Excelente', 'Usado - Bueno', 'Usado - Regular'];
const years = Array.from({ length: 30 }, (_, i) => 2025 - i);
const provinces = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'La Vega',
  'San Cristóbal',
  'Puerto Plata',
  'La Romana',
  'San Pedro de Macorís',
  'Duarte',
  'La Altagracia',
];

const features = [
  'Aire Acondicionado',
  'Vidrios Eléctricos',
  'Cierre Central',
  'Dirección Hidráulica',
  'Airbags',
  'ABS',
  'Control de Crucero',
  'Bluetooth',
  'Cámara de Reversa',
  'Sensores de Parqueo',
  'Techo Solar',
  'Asientos de Cuero',
  'Sistema de Navegación',
  'Sistema Keyless',
  'Apple CarPlay',
  'Android Auto',
];

const photoCategories = [
  { id: 'exterior', label: 'Exterior', required: true },
  { id: 'interior', label: 'Interior', required: true },
  { id: 'dashboard', label: 'Tablero', required: false },
  { id: 'engine', label: 'Motor', required: false },
  { id: 'wheels', label: 'Llantas', required: false },
  { id: 'details', label: 'Detalles', required: false },
];

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function PublicarPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState<VehicleFormData>({
    makeId: '',
    modelId: '',
    year: '',
    version: '',
    mileage: '',
    fuelType: '',
    transmission: '',
    color: '',
    condition: '',
    doors: '4',
    seats: '5',
    description: '',
    price: '',
    currency: 'DOP',
    negotiable: false,
    acceptTrades: false,
    province: '',
    city: '',
    features: [],
    photos: [],
  });
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<Record<string, number>>({});

  const { data: makes, isLoading: makesLoading } = useMakes();
  const { data: models, isLoading: modelsLoading } = useModelsByMake(formData.makeId);
  const createVehicleMutation = useCreateVehicle();

  const progress = (currentStep / steps.length) * 100;

  const handleChange = (field: keyof VehicleFormData, value: string | boolean | string[]) => {
    setFormData(prev => ({ ...prev, [field]: value }));

    // Reset model when make changes
    if (field === 'makeId') {
      setFormData(prev => ({ ...prev, modelId: '' }));
    }
  };

  const handleFeatureToggle = (feature: string) => {
    setFormData(prev => ({
      ...prev,
      features: prev.features.includes(feature)
        ? prev.features.filter(f => f !== feature)
        : [...prev.features, feature],
    }));
  };

  const handlePhotoUpload = async (files: FileList, category: string) => {
    setIsUploading(true);
    try {
      const results = await uploadImages(Array.from(files), 'vehicles', (index, progress) => {
        setUploadProgress(prev => ({
          ...prev,
          [`${category}-${index}`]: progress.percentage,
        }));
      });

      const newPhotos: UploadedPhoto[] = results.map((result, index) => ({
        id: `${category}-${Date.now()}-${index}`,
        url: result.url,
        category,
        isPrimary: formData.photos.length === 0 && index === 0,
      }));

      setFormData(prev => ({
        ...prev,
        photos: [...prev.photos, ...newPhotos],
      }));
      toast.success(`${results.length} fotos subidas`);
    } catch {
      toast.error('Error al subir fotos');
    } finally {
      setIsUploading(false);
      setUploadProgress({});
    }
  };

  const handleRemovePhoto = (id: string) => {
    setFormData(prev => ({
      ...prev,
      photos: prev.photos.filter(p => p.id !== id),
    }));
  };

  const handleSetPrimary = (id: string) => {
    setFormData(prev => ({
      ...prev,
      photos: prev.photos.map(p => ({ ...p, isPrimary: p.id === id })),
    }));
  };

  const validateStep = (step: number): boolean => {
    switch (step) {
      case 1:
        return !!(
          formData.makeId &&
          formData.modelId &&
          formData.year &&
          formData.fuelType &&
          formData.transmission
        );
      case 2:
        return formData.photos.length >= 3;
      case 3:
        return !!(formData.price && formData.province);
      default:
        return true;
    }
  };

  const nextStep = () => {
    if (!validateStep(currentStep)) {
      toast.error('Por favor completa todos los campos requeridos');
      return;
    }
    if (currentStep < steps.length) {
      setCurrentStep(prev => prev + 1);
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(prev => prev - 1);
    }
  };

  const handleSubmit = async () => {
    if (!validateStep(1) || !validateStep(2) || !validateStep(3)) {
      toast.error('Por favor completa todos los campos requeridos');
      return;
    }

    try {
      const vehicleData = {
        makeId: formData.makeId,
        modelId: formData.modelId,
        year: parseInt(formData.year),
        version: formData.version,
        mileage: parseInt(formData.mileage) || 0,
        fuelType: formData.fuelType,
        transmission: formData.transmission,
        color: formData.color,
        condition: formData.condition,
        doors: parseInt(formData.doors),
        seats: parseInt(formData.seats),
        description: formData.description,
        price: parseFloat(formData.price),
        currency: formData.currency,
        negotiable: formData.negotiable,
        acceptTrades: formData.acceptTrades,
        location: {
          province: formData.province,
          city: formData.city,
        },
        features: formData.features,
        images: formData.photos.map(p => ({
          url: p.url,
          category: p.category,
          isPrimary: p.isPrimary,
        })),
      };

      const result = await createVehicleMutation.mutateAsync(vehicleData as any);
      toast.success('¡Vehículo publicado exitosamente!');
      router.push(`/vehiculos/${result.id}`);
    } catch {
      toast.error('Error al publicar el vehículo');
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-4xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 text-center">
          <h1 className="mb-2 text-3xl font-bold text-gray-900">Publicar Vehículo</h1>
          <p className="text-gray-600">Completa la información de tu vehículo para publicarlo</p>
        </div>

        {/* Progress Bar */}
        <div className="mb-8">
          <Progress value={progress} className="h-2" />
          <div className="mt-4 flex justify-between">
            {steps.map(step => {
              const Icon = step.icon;
              const isActive = step.id === currentStep;
              const isComplete = step.id < currentStep;
              return (
                <div
                  key={step.id}
                  className={`flex flex-col items-center ${
                    isActive
                      ? 'text-emerald-600'
                      : isComplete
                        ? 'text-emerald-500'
                        : 'text-gray-400'
                  }`}
                >
                  <div
                    className={`mb-2 flex h-10 w-10 items-center justify-center rounded-full ${
                      isComplete
                        ? 'bg-emerald-500 text-white'
                        : isActive
                          ? 'border-2 border-emerald-500 bg-emerald-100 text-emerald-600'
                          : 'bg-gray-100 text-gray-400'
                    }`}
                  >
                    {isComplete ? <Check className="h-5 w-5" /> : <Icon className="h-5 w-5" />}
                  </div>
                  <span className="hidden text-xs font-medium sm:block">{step.title}</span>
                </div>
              );
            })}
          </div>
        </div>

        {/* Step 1: Basic Info */}
        {currentStep === 1 && (
          <Card>
            <CardHeader>
              <CardTitle>Información del Vehículo</CardTitle>
              <CardDescription>Ingresa los datos básicos de tu vehículo</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                {/* Make */}
                <div className="space-y-2">
                  <Label>Marca *</Label>
                  {makesLoading ? (
                    <Skeleton className="h-10 w-full" />
                  ) : (
                    <Select value={formData.makeId} onValueChange={v => handleChange('makeId', v)}>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecciona marca" />
                      </SelectTrigger>
                      <SelectContent>
                        {makes?.map((make: any) => (
                          <SelectItem key={make.id} value={make.id}>
                            {make.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                </div>

                {/* Model */}
                <div className="space-y-2">
                  <Label>Modelo *</Label>
                  {modelsLoading ? (
                    <Skeleton className="h-10 w-full" />
                  ) : (
                    <Select
                      value={formData.modelId}
                      onValueChange={v => handleChange('modelId', v)}
                      disabled={!formData.makeId}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Selecciona modelo" />
                      </SelectTrigger>
                      <SelectContent>
                        {models?.map((model: any) => (
                          <SelectItem key={model.id} value={model.id}>
                            {model.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                </div>

                {/* Year */}
                <div className="space-y-2">
                  <Label>Año *</Label>
                  <Select value={formData.year} onValueChange={v => handleChange('year', v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona año" />
                    </SelectTrigger>
                    <SelectContent>
                      {years.map(year => (
                        <SelectItem key={year} value={year.toString()}>
                          {year}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Version */}
                <div className="space-y-2">
                  <Label>Versión (Opcional)</Label>
                  <Input
                    placeholder="Ej: LX, EX, Sport"
                    value={formData.version}
                    onChange={e => handleChange('version', e.target.value)}
                  />
                </div>

                {/* Mileage */}
                <div className="space-y-2">
                  <Label>Kilometraje</Label>
                  <Input
                    type="number"
                    placeholder="0"
                    value={formData.mileage}
                    onChange={e => handleChange('mileage', e.target.value)}
                  />
                </div>

                {/* Fuel Type */}
                <div className="space-y-2">
                  <Label>Combustible *</Label>
                  <Select
                    value={formData.fuelType}
                    onValueChange={v => handleChange('fuelType', v)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona tipo" />
                    </SelectTrigger>
                    <SelectContent>
                      {fuelTypes.map(type => (
                        <SelectItem key={type} value={type}>
                          {type}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Transmission */}
                <div className="space-y-2">
                  <Label>Transmisión *</Label>
                  <Select
                    value={formData.transmission}
                    onValueChange={v => handleChange('transmission', v)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona tipo" />
                    </SelectTrigger>
                    <SelectContent>
                      {transmissions.map(type => (
                        <SelectItem key={type} value={type}>
                          {type}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Color */}
                <div className="space-y-2">
                  <Label>Color</Label>
                  <Select value={formData.color} onValueChange={v => handleChange('color', v)}>
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona color" />
                    </SelectTrigger>
                    <SelectContent>
                      {colors.map(color => (
                        <SelectItem key={color} value={color}>
                          {color}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Condition */}
                <div className="space-y-2">
                  <Label>Condición</Label>
                  <Select
                    value={formData.condition}
                    onValueChange={v => handleChange('condition', v)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona condición" />
                    </SelectTrigger>
                    <SelectContent>
                      {conditions.map(cond => (
                        <SelectItem key={cond} value={cond}>
                          {cond}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Doors */}
                <div className="space-y-2">
                  <Label>Puertas</Label>
                  <Select value={formData.doors} onValueChange={v => handleChange('doors', v)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {['2', '3', '4', '5'].map(num => (
                        <SelectItem key={num} value={num}>
                          {num} puertas
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              {/* Features */}
              <div className="space-y-3">
                <Label>Características</Label>
                <div className="grid grid-cols-2 gap-2 md:grid-cols-4">
                  {features.map(feature => (
                    <div key={feature} className="flex items-center space-x-2">
                      <Checkbox
                        id={feature}
                        checked={formData.features.includes(feature)}
                        onCheckedChange={() => handleFeatureToggle(feature)}
                      />
                      <label
                        htmlFor={feature}
                        className="text-sm leading-none font-medium peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                      >
                        {feature}
                      </label>
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Step 2: Photos */}
        {currentStep === 2 && (
          <Card>
            <CardHeader>
              <CardTitle>Fotos del Vehículo</CardTitle>
              <CardDescription>
                Sube al menos 3 fotos de tu vehículo. La primera será la foto principal.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Upload Area */}
              <div
                className={`flex min-h-48 flex-col items-center justify-center rounded-lg border-2 border-dashed p-8 transition-colors ${
                  isUploading
                    ? 'border-emerald-300 bg-emerald-50'
                    : 'border-gray-300 hover:border-emerald-400 hover:bg-gray-50'
                }`}
              >
                {isUploading ? (
                  <div className="flex flex-col items-center gap-2">
                    <Loader2 className="h-8 w-8 animate-spin text-emerald-600" />
                    <p className="text-sm text-gray-600">Subiendo fotos...</p>
                  </div>
                ) : (
                  <>
                    <Upload className="mb-4 h-12 w-12 text-gray-400" />
                    <p className="mb-2 text-lg font-medium text-gray-700">
                      Arrastra y suelta tus fotos aquí
                    </p>
                    <p className="mb-4 text-sm text-gray-500">o</p>
                    <input
                      type="file"
                      multiple
                      accept="image/*"
                      onChange={e => e.target.files && handlePhotoUpload(e.target.files, 'general')}
                      className="hidden"
                      id="photo-upload"
                    />
                    <label htmlFor="photo-upload">
                      <Button variant="outline" className="cursor-pointer" asChild>
                        <span>
                          <Camera className="mr-2 h-4 w-4" />
                          Seleccionar fotos
                        </span>
                      </Button>
                    </label>
                    <p className="mt-4 text-xs text-gray-400">
                      JPG, PNG o WEBP. Máximo 10MB por imagen.
                    </p>
                  </>
                )}
              </div>

              {/* Uploaded Photos */}
              {formData.photos.length > 0 && (
                <div>
                  <p className="mb-3 text-sm font-medium text-gray-700">
                    Fotos subidas ({formData.photos.length})
                  </p>
                  <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
                    {formData.photos.map((photo, index) => (
                      <div
                        key={photo.id}
                        className={`relative aspect-[4/3] overflow-hidden rounded-lg border-2 ${
                          photo.isPrimary ? 'border-emerald-500' : 'border-gray-200'
                        }`}
                      >
                        <Image
                          src={photo.url}
                          alt={`Foto ${index + 1}`}
                          fill
                          className="object-cover"
                        />
                        {photo.isPrimary && (
                          <Badge className="absolute top-2 left-2 bg-emerald-500">Principal</Badge>
                        )}
                        <div className="absolute right-2 bottom-2 flex gap-1">
                          {!photo.isPrimary && (
                            <Button
                              variant="secondary"
                              size="sm"
                              onClick={() => handleSetPrimary(photo.id)}
                              className="h-7 px-2 text-xs"
                            >
                              Principal
                            </Button>
                          )}
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => handleRemovePhoto(photo.id)}
                            className="h-7 w-7 p-0"
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {formData.photos.length < 3 && (
                <div className="flex items-center gap-2 rounded-lg bg-amber-50 p-4 text-amber-800">
                  <AlertCircle className="h-5 w-5" />
                  <p className="text-sm">Se requieren al menos 3 fotos para publicar</p>
                </div>
              )}
            </CardContent>
          </Card>
        )}

        {/* Step 3: Price & Details */}
        {currentStep === 3 && (
          <Card>
            <CardHeader>
              <CardTitle>Precio y Ubicación</CardTitle>
              <CardDescription>Define el precio y la ubicación del vehículo</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                {/* Price */}
                <div className="space-y-2">
                  <Label>Precio *</Label>
                  <div className="flex gap-2">
                    <Select
                      value={formData.currency}
                      onValueChange={v => handleChange('currency', v)}
                    >
                      <SelectTrigger className="w-24">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="DOP">RD$</SelectItem>
                        <SelectItem value="USD">US$</SelectItem>
                      </SelectContent>
                    </Select>
                    <Input
                      type="number"
                      placeholder="0.00"
                      value={formData.price}
                      onChange={e => handleChange('price', e.target.value)}
                      className="flex-1"
                    />
                  </div>
                </div>

                {/* Province */}
                <div className="space-y-2">
                  <Label>Provincia *</Label>
                  <Select
                    value={formData.province}
                    onValueChange={v => handleChange('province', v)}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Selecciona provincia" />
                    </SelectTrigger>
                    <SelectContent>
                      {provinces.map(prov => (
                        <SelectItem key={prov} value={prov}>
                          {prov}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* City */}
                <div className="space-y-2">
                  <Label>Ciudad/Sector</Label>
                  <Input
                    placeholder="Ej: Piantini, Naco"
                    value={formData.city}
                    onChange={e => handleChange('city', e.target.value)}
                  />
                </div>
              </div>

              {/* Options */}
              <div className="space-y-4">
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="negotiable"
                    checked={formData.negotiable}
                    onCheckedChange={checked => handleChange('negotiable', checked as boolean)}
                  />
                  <label htmlFor="negotiable" className="text-sm font-medium">
                    Precio negociable
                  </label>
                </div>
                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="acceptTrades"
                    checked={formData.acceptTrades}
                    onCheckedChange={checked => handleChange('acceptTrades', checked as boolean)}
                  />
                  <label htmlFor="acceptTrades" className="text-sm font-medium">
                    Acepto vehículos en parte de pago
                  </label>
                </div>
              </div>

              {/* Description */}
              <div className="space-y-2">
                <Label>Descripción</Label>
                <Textarea
                  placeholder="Describe tu vehículo: estado, mantenimiento, razón de venta, etc."
                  value={formData.description}
                  onChange={e => handleChange('description', e.target.value)}
                  rows={5}
                />
                <p className="text-xs text-gray-500">
                  {formData.description.length}/2000 caracteres
                </p>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Step 4: Review */}
        {currentStep === 4 && (
          <Card>
            <CardHeader>
              <CardTitle>Revisión Final</CardTitle>
              <CardDescription>Revisa los datos antes de publicar</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Photo Preview */}
              {formData.photos.length > 0 && (
                <div className="relative aspect-video overflow-hidden rounded-lg">
                  <Image
                    src={formData.photos.find(p => p.isPrimary)?.url || formData.photos[0].url}
                    alt="Foto principal"
                    fill
                    className="object-cover"
                  />
                  <div className="absolute right-4 bottom-4">
                    <Badge variant="secondary">{formData.photos.length} fotos</Badge>
                  </div>
                </div>
              )}

              {/* Vehicle Details */}
              <div className="rounded-lg border p-4">
                <h3 className="mb-4 text-lg font-semibold">
                  {makes?.find((m: any) => m.id === formData.makeId)?.name}{' '}
                  {models?.find((m: any) => m.id === formData.modelId)?.name} {formData.year}
                  {formData.version && ` ${formData.version}`}
                </h3>
                <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-3">
                  <div>
                    <span className="text-gray-500">Precio:</span>
                    <p className="font-bold text-emerald-600">
                      {formData.currency === 'USD' ? 'US$' : 'RD$'}{' '}
                      {parseFloat(formData.price || '0').toLocaleString()}
                    </p>
                  </div>
                  <div>
                    <span className="text-gray-500">Kilometraje:</span>
                    <p>{parseInt(formData.mileage || '0').toLocaleString()} km</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Combustible:</span>
                    <p>{formData.fuelType}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Transmisión:</span>
                    <p>{formData.transmission}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Color:</span>
                    <p>{formData.color || 'No especificado'}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Ubicación:</span>
                    <p>
                      {formData.province}
                      {formData.city && `, ${formData.city}`}
                    </p>
                  </div>
                </div>
              </div>

              {/* Features */}
              {formData.features.length > 0 && (
                <div>
                  <h4 className="mb-2 font-medium">Características</h4>
                  <div className="flex flex-wrap gap-2">
                    {formData.features.map(feature => (
                      <Badge key={feature} variant="secondary">
                        {feature}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}

              {/* Description */}
              {formData.description && (
                <div>
                  <h4 className="mb-2 font-medium">Descripción</h4>
                  <p className="text-sm text-gray-600">{formData.description}</p>
                </div>
              )}

              {/* Options */}
              <div className="flex flex-wrap gap-2">
                {formData.negotiable && <Badge variant="outline">Precio negociable</Badge>}
                {formData.acceptTrades && (
                  <Badge variant="outline">Acepta vehículos en parte de pago</Badge>
                )}
              </div>

              {/* Submit Info */}
              <div className="rounded-lg bg-emerald-50 p-4">
                <div className="flex items-start gap-3">
                  <Info className="mt-0.5 h-5 w-5 text-emerald-600" />
                  <div className="text-sm">
                    <p className="font-medium text-emerald-800">Información importante</p>
                    <p className="text-emerald-700">
                      Tu publicación será revisada por nuestro equipo antes de ser publicada. El
                      proceso de revisión toma aproximadamente 24 horas hábiles.
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Navigation Buttons */}
        <div className="mt-8 flex justify-between">
          <Button variant="outline" onClick={prevStep} disabled={currentStep === 1}>
            <ChevronLeft className="mr-2 h-4 w-4" />
            Anterior
          </Button>

          {currentStep < steps.length ? (
            <Button onClick={nextStep} className="bg-emerald-600 hover:bg-emerald-700">
              Siguiente
              <ChevronRight className="ml-2 h-4 w-4" />
            </Button>
          ) : (
            <Button
              onClick={handleSubmit}
              className="bg-emerald-600 hover:bg-emerald-700"
              disabled={createVehicleMutation.isPending}
            >
              {createVehicleMutation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Publicando...
                </>
              ) : (
                <>
                  <Check className="mr-2 h-4 w-4" />
                  Publicar Vehículo
                </>
              )}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}
