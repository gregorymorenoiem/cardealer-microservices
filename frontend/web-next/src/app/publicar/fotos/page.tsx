/**
 * Publicar Fotos Page
 *
 * Photo upload step for vehicle listing (edit mode)
 * Connected to real APIs - February 2026
 */

'use client';

import { useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Camera,
  Upload,
  X,
  Check,
  ChevronRight,
  ChevronLeft,
  Info,
  GripVertical,
  Star,
  Trash2,
  ImagePlus,
  Loader2,
} from 'lucide-react';
import Link from 'next/link';
import Image from 'next/image';
import { toast } from 'sonner';
import { uploadImages, type UploadProgress } from '@/services/media';
import { useVehicle, useUpdateVehicle } from '@/hooks/use-vehicles';

// =============================================================================
// TYPES
// =============================================================================

interface UploadedPhoto {
  id: string;
  url: string;
  category: string;
  isPrimary: boolean;
}

const photoCategories = [
  { id: 'exterior', label: 'Exterior', required: 4, description: 'Frente, atrás, laterales' },
  { id: 'interior', label: 'Interior', required: 2, description: 'Asientos delanteros y traseros' },
  { id: 'dashboard', label: 'Tablero', required: 1, description: 'Panel de instrumentos' },
  { id: 'engine', label: 'Motor', required: 1, description: 'Compartimento del motor' },
  { id: 'wheels', label: 'Llantas', required: 1, description: 'Estado de las llantas' },
  { id: 'details', label: 'Detalles', required: 0, description: 'Opcionales: detalles especiales' },
];

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function PublicarFotosPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const vehicleId = searchParams.get('vehicleId');

  const { data: vehicle, isLoading: vehicleLoading } = useVehicle(vehicleId || '');
  const updateVehicleMutation = useUpdateVehicle();

  const [photos, setPhotos] = useState<UploadedPhoto[]>([]);
  const [selectedCategory, setSelectedCategory] = useState('exterior');
  const [isDragging, setIsDragging] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgressMap, setUploadProgressMap] = useState<Record<string, number>>({});

  // Initialize photos from existing vehicle data
  useState(() => {
    if (vehicle?.images) {
      setPhotos(vehicle.images.map((img: any, index: number) => ({
        id: `existing-${index}`,
        url: img.url,
        category: img.category || 'general',
        isPrimary: img.isPrimary || index === 0,
      })));
    }
  });

  const totalRequired = photoCategories.reduce((acc, cat) => acc + cat.required, 0);
  const uploadedCount = photos.length;
  const uploadProgress = Math.min(Math.round((uploadedCount / totalRequired) * 100), 100);

  const handleDrop = async (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const files = e.dataTransfer.files;
    if (files.length > 0) {
      await handlePhotoUpload(files, selectedCategory);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handlePhotoUpload = async (files: FileList, category: string) => {
    setIsUploading(true);
    try {
      const results = await uploadImages(
        Array.from(files),
        'vehicles',
        (index, progress) => {
          setUploadProgressMap(prev => ({
            ...prev,
            [`upload-${index}`]: progress.percentage,
          }));
        }
      );

      const newPhotos: UploadedPhoto[] = results.map((result, index) => ({
        id: `${category}-${Date.now()}-${index}`,
        url: result.url,
        category,
        isPrimary: photos.length === 0 && index === 0,
      }));

      setPhotos(prev => [...prev, ...newPhotos]);
      toast.success(`${results.length} fotos subidas`);
    } catch {
      toast.error('Error al subir fotos');
    } finally {
      setIsUploading(false);
      setUploadProgressMap({});
    }
  };

  const handleRemovePhoto = (id: string) => {
    setPhotos(prev => {
      const updated = prev.filter(p => p.id !== id);
      // If we removed the primary photo, make the first remaining one primary
      if (updated.length > 0 && !updated.some(p => p.isPrimary)) {
        updated[0].isPrimary = true;
      }
      return updated;
    });
  };

  const handleSetPrimary = (id: string) => {
    setPhotos(prev => prev.map(p => ({
      ...p,
      isPrimary: p.id === id,
    })));
  };

  const handleSave = async () => {
    if (!vehicleId) {
      toast.error('No se encontró el vehículo');
      return;
    }

    if (photos.length < 3) {
      toast.error('Se requieren al menos 3 fotos');
      return;
    }

    try {
      await updateVehicleMutation.mutateAsync({
        id: vehicleId,
        data: {
          images: photos.map((p, index) => ({
            url: p.url,
            category: p.category,
            isPrimary: p.isPrimary,
            order: index,
          })),
        },
      });
      toast.success('Fotos guardadas exitosamente');
      router.push(`/publicar/preview?vehicleId=${vehicleId}`);
    } catch {
      toast.error('Error al guardar las fotos');
    }
  };

  if (vehicleLoading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="mx-auto max-w-5xl px-4 py-8">
          <Skeleton className="h-12 w-64" />
          <Skeleton className="mt-8 h-96" />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-5xl px-4 py-8">
        {/* Header */}
        <div className="mb-8 flex items-center justify-between">
          <div>
            <h1 className="mb-2 text-3xl font-bold text-gray-900">Fotos del Vehículo</h1>
            <p className="text-gray-600">
              Las fotos de calidad aumentan tus posibilidades de venta
            </p>
          </div>
          <div className="text-right">
            <p className="text-sm text-gray-500">Progreso</p>
            <p className="text-2xl font-bold text-emerald-600">
              {uploadedCount}/{totalRequired}
            </p>
          </div>
        </div>

        {/* Progress */}
        <div className="mb-6">
          <Progress value={uploadProgress} className="h-2" />
          <div className="mt-2 flex justify-between text-sm text-gray-500">
            <span>{uploadedCount} fotos subidas</span>
            <span>Mínimo {totalRequired} requeridas</span>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
          {/* Categories Sidebar */}
          <div className="lg:col-span-1">
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Categorías</CardTitle>
              </CardHeader>
              <CardContent className="p-0">
                {photoCategories.map(cat => {
                  const catPhotos = photos.filter(p => p.category === cat.id).length;
                  const isComplete = catPhotos >= cat.required;
                  const isActive = selectedCategory === cat.id;

                  return (
                    <button
                      key={cat.id}
                      onClick={() => setSelectedCategory(cat.id)}
                      className={`flex w-full items-center justify-between border-b px-4 py-3 transition-colors last:border-b-0 ${
                        isActive
                          ? 'border-l-4 border-l-emerald-500 bg-emerald-50'
                          : 'hover:bg-gray-50'
                      }`}
                    >
                      <div className="text-left">
                        <p
                          className={`font-medium ${isActive ? 'text-emerald-700' : 'text-gray-900'}`}
                        >
                          {cat.label}
                        </p>
                        <p className="text-xs text-gray-500">{cat.description}</p>
                      </div>
                      <div className="flex items-center gap-2">
                        {isComplete ? (
                          <Check className="h-4 w-4 text-emerald-500" />
                        ) : cat.required > 0 ? (
                          <Badge variant="outline" className="text-xs">
                            {catPhotos}/{cat.required}
                          </Badge>
                        ) : (
                          <Badge variant="outline" className="text-xs text-gray-400">
                            Opcional
                          </Badge>
                        )}
                      </div>
                    </button>
                  );
                })}
              </CardContent>
            </Card>

            {/* Tips */}
            <Card className="mt-4 border-blue-200 bg-blue-50">
              <CardContent className="p-4">
                <div className="flex gap-2">
                  <Info className="h-5 w-5 flex-shrink-0 text-blue-600" />
                  <div className="text-sm text-blue-800">
                    <p className="mb-1 font-medium">Tips para mejores fotos</p>
                    <ul className="space-y-1 text-xs">
                      <li>• Usa luz natural</li>
                      <li>• Mantén el vehículo limpio</li>
                      <li>• Evita reflejos</li>
                      <li>• Muestra todos los ángulos</li>
                    </ul>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Upload Area */}
          <div className="space-y-6 lg:col-span-3">
            {/* Drag & Drop Zone */}
            <Card
              className={`border-2 border-dashed transition-colors ${
                isDragging ? 'border-emerald-500 bg-emerald-50' : 'border-gray-300'
              }`}
              onDrop={handleDrop}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
            >
              <CardContent className="py-12 text-center">
                {isUploading ? (
                  <div className="flex flex-col items-center gap-3">
                    <Loader2 className="h-12 w-12 animate-spin text-emerald-500" />
                    <p className="text-lg font-medium text-gray-900">Subiendo fotos...</p>
                    <Progress 
                      value={Object.values(uploadProgressMap).reduce((a, b) => a + b, 0) / Object.keys(uploadProgressMap).length || 0} 
                      className="h-2 w-48"
                    />
                  </div>
                ) : (
                  <>
                    <Upload
                      className={`mx-auto mb-4 h-12 w-12 ${isDragging ? 'text-emerald-500' : 'text-gray-400'}`}
                    />
                    <h3 className="mb-2 text-lg font-medium text-gray-900">
                      Arrastra las fotos de{' '}
                      {photoCategories.find(c => c.id === selectedCategory)?.label}
                    </h3>
                    <p className="mb-4 text-gray-500">
                      Formatos: JPG, PNG, WEBP (máximo 10MB por foto)
                    </p>
                    <input
                      type="file"
                      multiple
                      accept="image/*"
                      onChange={e => e.target.files && handlePhotoUpload(e.target.files, selectedCategory)}
                      className="hidden"
                      id="photo-upload"
                    />
                    <label htmlFor="photo-upload">
                      <Button className="cursor-pointer bg-emerald-600 hover:bg-emerald-700" asChild>
                        <span>
                          <ImagePlus className="mr-2 h-4 w-4" />
                          Seleccionar Archivos
                        </span>
                      </Button>
                    </label>
                  </>
                )}
              </CardContent>
            </Card>

            {/* Uploaded Photos Grid */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center justify-between text-base">
                  <span>Fotos Subidas ({photos.length})</span>
                  <span className="text-sm font-normal text-gray-500">
                    Click en una foto para hacerla principal
                  </span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                {photos.length > 0 ? (
                  <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
                    {photos.map((photo) => (
                      <div
                        key={photo.id}
                        className={`group relative aspect-square cursor-pointer overflow-hidden rounded-lg border-2 ${
                          photo.isPrimary ? 'border-emerald-500' : 'border-gray-200'
                        }`}
                        onClick={() => handleSetPrimary(photo.id)}
                      >
                        <Image
                          src={photo.url}
                          alt="Foto del vehículo"
                          fill
                          className="object-cover"
                        />

                        {/* Category badge */}
                        <Badge
                          variant="secondary"
                          className="absolute left-2 top-2 text-xs"
                        >
                          {photoCategories.find(c => c.id === photo.category)?.label || 'General'}
                        </Badge>

                        {/* Primary badge */}
                        {photo.isPrimary && (
                          <Badge className="absolute right-2 top-2 bg-emerald-500">
                            <Star className="mr-1 h-3 w-3" />
                            Principal
                          </Badge>
                        )}

                        {/* Delete button */}
                        <Button
                          variant="destructive"
                          size="icon"
                          className="absolute bottom-2 right-2 h-8 w-8 opacity-0 transition-opacity group-hover:opacity-100"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleRemovePhoto(photo.id);
                          }}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    ))}

                    {/* Add more button */}
                    <label
                      htmlFor="photo-upload"
                      className="flex aspect-square cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 text-gray-400 transition-colors hover:border-emerald-500 hover:bg-emerald-50 hover:text-emerald-600"
                    >
                      <ImagePlus className="mb-2 h-8 w-8" />
                      <span className="text-sm">Agregar</span>
                    </label>
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-500">
                    <Camera className="mb-4 h-12 w-12" />
                    <p>Aún no has subido fotos</p>
                    <p className="text-sm">Arrastra y suelta o usa el botón de arriba</p>
                  </div>
                )}

                {photos.length > 0 && (
                  <p className="mt-4 text-xs text-gray-500">
                    <Star className="mr-1 inline h-3 w-3" />
                    La foto marcada como principal será la imagen de portada de tu anuncio
                  </p>
                )}
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Navigation */}
        <div className="mt-8 flex justify-between">
          <Link href={vehicleId ? `/publicar?edit=${vehicleId}` : '/publicar'}>
            <Button variant="outline">
              <ChevronLeft className="mr-2 h-4 w-4" />
              Información Básica
            </Button>
          </Link>
          <Button
            onClick={handleSave}
            className="bg-emerald-600 hover:bg-emerald-700"
            disabled={photos.length < 3 || updateVehicleMutation.isPending}
          >
            {updateVehicleMutation.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Guardando...
              </>
            ) : (
              <>
                Vista Previa
                <ChevronRight className="ml-2 h-4 w-4" />
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
