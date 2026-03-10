'use client';

/**
 * Marketplace Import Page
 *
 * Allows dealers/sellers to import vehicle listings from Facebook Marketplace
 * or other platforms by pasting the listing text. Uses AI (Claude) to extract
 * structured vehicle data and pre-fill the creation form.
 */

import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { csrfFetch } from '@/lib/security/csrf';
import { formatPrice } from '@/lib/format';
import {
  Upload,
  Sparkles,
  AlertCircle,
  CheckCircle2,
  Loader2,
  Facebook,
  Globe,
  ClipboardPaste,
  ArrowRight,
  Car,
  Edit3,
  Layers,
  Plus,
  Trash2,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';

interface ExtractedVehicle {
  make: string;
  model: string;
  year: number;
  price: number;
  currency: string;
  mileage: number | null;
  transmission: string | null;
  fuelType: string | null;
  bodyType: string | null;
  condition: string;
  color: string | null;
  description: string;
  location: string | null;
  province: string | null;
  features: string[];
  engineSize: string | null;
  doors: number | null;
  driveType: string | null;
  vin: string | null;
  imageUrls: string[];
  confidence: number;
}

export default function ImportarPage() {
  const router = useRouter();
  const [listingText, setListingText] = useState('');
  const [listingUrl, setListingUrl] = useState('');
  const [isExtracting, setIsExtracting] = useState(false);
  const [extractedData, setExtractedData] = useState<ExtractedVehicle | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [hint, setHint] = useState<string | null>(null);
  // Bulk import state
  const [bulkListings, setBulkListings] = useState<Array<{ text: string; source: string }>>([
    { text: '', source: 'facebook' },
  ]);
  const [bulkResults, setBulkResults] = useState<
    Array<{
      index: number;
      success: boolean;
      data: ExtractedVehicle | null;
      error: string | null;
      source?: string;
    }>
  >([]);
  const [isBulkProcessing, setIsBulkProcessing] = useState(false);

  const handleExtract = useCallback(
    async (mode: 'text' | 'url') => {
      setIsExtracting(true);
      setError(null);
      setHint(null);
      setExtractedData(null);

      try {
        const token = document.cookie
          .split(';')
          .find(c => c.trim().startsWith('token='))
          ?.split('=')[1];

        const response = await csrfFetch('/api/import/marketplace', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
          },
          body: JSON.stringify(mode === 'text' ? { text: listingText } : { url: listingUrl }),
        });

        const result = await response.json();

        if (result.success) {
          setExtractedData(result.data);
        } else {
          setError(result.error || 'Error al procesar el anuncio');
          if (result.hint) setHint(result.hint);
        }
      } catch {
        setError('Error de conexión. Intenta de nuevo.');
      } finally {
        setIsExtracting(false);
      }
    },
    [listingText, listingUrl]
  );

  const handlePublish = useCallback(() => {
    if (!extractedData) return;
    sessionStorage.setItem('importedVehicle', JSON.stringify(extractedData));
    router.push('/publicar?from=import');
  }, [extractedData, router]);

  const handleBulkImport = useCallback(async () => {
    const validListings = bulkListings.filter(l => l.text.length >= 10);
    if (validListings.length === 0) return;

    setIsBulkProcessing(true);
    setError(null);
    setBulkResults([]);

    try {
      const token = document.cookie
        .split(';')
        .find(c => c.trim().startsWith('token='))
        ?.split('=')[1];

      const response = await csrfFetch('/api/import/bulk', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify({ listings: validListings }),
      });

      const result = await response.json();
      if (result.success) {
        // Enrich results with source from original listings
        const enrichedResults = (
          result.results as Array<{
            index: number;
            success: boolean;
            data: ExtractedVehicle | null;
            error: string | null;
          }>
        ).map(
          (r: {
            index: number;
            success: boolean;
            data: ExtractedVehicle | null;
            error: string | null;
          }) => ({
            ...r,
            source: validListings[r.index]?.source || 'other',
          })
        );
        setBulkResults(enrichedResults);
      } else {
        setError(result.error || 'Error al procesar importación masiva');
      }
    } catch {
      setError('Error de conexión. Intenta de nuevo.');
    } finally {
      setIsBulkProcessing(false);
    }
  }, [bulkListings]);

  const addBulkListing = () => {
    if (bulkListings.length < 50) {
      setBulkListings([...bulkListings, { text: '', source: 'facebook' }]);
    }
  };

  const removeBulkListing = (index: number) => {
    setBulkListings(bulkListings.filter((_, i) => i !== index));
  };

  const updateBulkListing = (index: number, field: 'text' | 'source', value: string) => {
    const updated = [...bulkListings];
    updated[index] = { ...updated[index], [field]: value };
    setBulkListings(updated);
  };

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-8 text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-purple-600">
          <Sparkles className="h-8 w-8 text-white" />
        </div>
        <h1 className="text-3xl font-bold tracking-tight">Importar desde Marketplace</h1>
        <p className="text-muted-foreground mt-2 text-lg">
          Usa inteligencia artificial para importar tu inventario de Facebook Marketplace u otra
          plataforma
        </p>
      </div>

      {/* How it works */}
      <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="border-blue-200 bg-blue-50/50 dark:border-blue-900 dark:bg-blue-950/30">
          <CardContent className="pt-6 text-center">
            <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900">
              <ClipboardPaste className="h-5 w-5 text-blue-600" />
            </div>
            <h3 className="font-semibold">1. Copia el anuncio</h3>
            <p className="text-muted-foreground mt-1 text-sm">
              Copia el texto de tu anuncio en Facebook Marketplace
            </p>
          </CardContent>
        </Card>
        <Card className="border-purple-200 bg-purple-50/50 dark:border-purple-900 dark:bg-purple-950/30">
          <CardContent className="pt-6 text-center">
            <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-purple-100 dark:bg-purple-900">
              <Sparkles className="h-5 w-5 text-purple-600" />
            </div>
            <h3 className="font-semibold">2. IA extrae los datos</h3>
            <p className="text-muted-foreground mt-1 text-sm">
              Claude analiza el texto y extrae marca, modelo, precio y más
            </p>
          </CardContent>
        </Card>
        <Card className="border-green-200 bg-green-50/50 dark:border-green-900 dark:bg-green-950/30">
          <CardContent className="pt-6 text-center">
            <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
              <Car className="h-5 w-5 text-green-600" />
            </div>
            <h3 className="font-semibold">3. Publica en OKLA</h3>
            <p className="text-muted-foreground mt-1 text-sm">
              Revisa los datos, ajusta si es necesario y publica
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Input area */}
      <Card className="mb-8">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Upload className="h-5 w-5" />
            Pega tu anuncio
          </CardTitle>
          <CardDescription>
            Copia el texto completo de tu anuncio de Facebook Marketplace y pégalo aquí
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="text">
            <TabsList className="mb-4">
              <TabsTrigger value="text" className="gap-2">
                <ClipboardPaste className="h-4 w-4" />
                Pegar texto
              </TabsTrigger>
              <TabsTrigger value="url" className="gap-2">
                <Globe className="h-4 w-4" />
                URL del anuncio
              </TabsTrigger>
              <TabsTrigger value="bulk" className="gap-2">
                <Layers className="h-4 w-4" />
                Importación masiva
              </TabsTrigger>
            </TabsList>

            <TabsContent value="text">
              <div className="space-y-4">
                <Textarea
                  placeholder={`Ejemplo:\n\nToyota Corolla 2020\nRD$ 850,000\n45,000 km\nAutomático, gasolina\nColor blanco, interior negro\nCámara de reversa, GPS, aros deportivos\nSanto Domingo Este\n\nExcelente condición, único dueño...`}
                  value={listingText}
                  onChange={e => setListingText(e.target.value)}
                  className="min-h-[200px] resize-y"
                />
                <div className="flex items-center justify-between">
                  <span className="text-muted-foreground text-sm">
                    {listingText.length > 0
                      ? `${listingText.length} caracteres`
                      : 'Pega el texto del anuncio'}
                  </span>
                  <Button
                    onClick={() => handleExtract('text')}
                    disabled={isExtracting || listingText.length < 10}
                    className="gap-2 bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:from-blue-700 hover:to-purple-700"
                  >
                    {isExtracting ? (
                      <>
                        <Loader2 className="h-4 w-4 animate-spin" />
                        Analizando con IA...
                      </>
                    ) : (
                      <>
                        <Sparkles className="h-4 w-4" />
                        Extraer datos con IA
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </TabsContent>

            <TabsContent value="url">
              <div className="space-y-4">
                <div>
                  <Label htmlFor="marketplace-url">URL del anuncio</Label>
                  <div className="mt-1.5 flex gap-2">
                    <div className="relative flex-1">
                      <Facebook className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
                      <Input
                        id="marketplace-url"
                        placeholder="https://www.facebook.com/marketplace/item/..."
                        value={listingUrl}
                        onChange={e => setListingUrl(e.target.value)}
                        className="pl-10"
                      />
                    </div>
                    <Button
                      onClick={() => handleExtract('url')}
                      disabled={isExtracting || !listingUrl.startsWith('http')}
                      className="gap-2"
                      variant="outline"
                    >
                      {isExtracting ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <Sparkles className="h-4 w-4" />
                      )}
                      Extraer
                    </Button>
                  </div>
                </div>
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertTitle>Nota sobre URLs</AlertTitle>
                  <AlertDescription>
                    Facebook Marketplace puede bloquear el acceso externo a los anuncios. Si la URL
                    no funciona, copia y pega el texto del anuncio directamente en la pestaña
                    &quot;Pegar texto&quot;.
                  </AlertDescription>
                </Alert>
              </div>
            </TabsContent>

            <TabsContent value="bulk">
              <div className="space-y-4">
                <p className="text-muted-foreground text-sm">
                  Importa múltiples vehículos a la vez. Pega el texto de cada anuncio por separado.
                  Soporta Facebook Marketplace, Corotos, SuperCarros, y MercadoLibre.
                </p>
                {bulkListings.map((listing, index) => (
                  <div key={index} className="rounded-lg border p-4">
                    <div className="mb-2 flex items-center justify-between">
                      <span className="text-sm font-medium">Anuncio #{index + 1}</span>
                      <div className="flex items-center gap-2">
                        <select
                          value={listing.source}
                          onChange={e => updateBulkListing(index, 'source', e.target.value)}
                          className="bg-background rounded-md border px-2 py-1 text-xs"
                        >
                          <option value="facebook">Facebook Marketplace</option>
                          <option value="corotos">Corotos.com.do</option>
                          <option value="supercarros">SuperCarros.com</option>
                          <option value="mercadolibre">MercadoLibre RD</option>
                          <option value="other">Otra plataforma</option>
                        </select>
                        {bulkListings.length > 1 && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => removeBulkListing(index)}
                            className="h-7 w-7 p-0 text-red-500 hover:text-red-700"
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </Button>
                        )}
                      </div>
                    </div>
                    <Textarea
                      placeholder="Pega aquí el texto del anuncio..."
                      value={listing.text}
                      onChange={e => updateBulkListing(index, 'text', e.target.value)}
                      className="min-h-[100px] resize-y text-sm"
                    />
                  </div>
                ))}
                <div className="flex items-center justify-between">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={addBulkListing}
                    disabled={bulkListings.length >= 50}
                    className="gap-1.5"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    Agregar otro anuncio ({bulkListings.length}/50)
                  </Button>
                  <Button
                    onClick={handleBulkImport}
                    disabled={isBulkProcessing || bulkListings.every(l => l.text.length < 10)}
                    className="gap-2 bg-gradient-to-r from-blue-600 to-purple-600 text-white"
                  >
                    {isBulkProcessing ? (
                      <>
                        <Loader2 className="h-4 w-4 animate-spin" />
                        Procesando...
                      </>
                    ) : (
                      <>
                        <Sparkles className="h-4 w-4" />
                        Importar {bulkListings.filter(l => l.text.length >= 10).length} anuncios
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Error state */}
      {error && (
        <Alert variant="destructive" className="mb-8">
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>
            {error}
            {hint && <p className="mt-1 font-medium">{hint}</p>}
          </AlertDescription>
        </Alert>
      )}

      {/* Bulk import results */}
      {bulkResults && bulkResults.length > 0 && (
        <Card className="mb-8 border-purple-200 dark:border-purple-900">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Layers className="h-5 w-5 text-purple-600" />
                <CardTitle>Resultados de importación masiva</CardTitle>
              </div>
              <div className="flex gap-2">
                <Badge className="bg-green-100 text-green-700">
                  {bulkResults.filter(r => r.success).length} exitosos
                </Badge>
                {bulkResults.filter(r => !r.success).length > 0 && (
                  <Badge className="bg-red-100 text-red-700">
                    {bulkResults.filter(r => !r.success).length} fallidos
                  </Badge>
                )}
              </div>
            </div>
            <CardDescription>
              Revisa cada vehículo extraído y publícalo individualmente
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {bulkResults.map((result, index) => (
                <div
                  key={index}
                  className={`rounded-lg border p-4 ${result.success ? 'border-green-200 bg-green-50/50 dark:border-green-900 dark:bg-green-950/20' : 'border-red-200 bg-red-50/50 dark:border-red-900 dark:bg-red-950/20'}`}
                >
                  <div className="mb-2 flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      {result.success ? (
                        <CheckCircle2 className="h-4 w-4 text-green-600" />
                      ) : (
                        <AlertCircle className="h-4 w-4 text-red-600" />
                      )}
                      <span className="text-sm font-medium">
                        Anuncio #{index + 1}
                        {result.source && (
                          <span className="text-muted-foreground ml-1 text-xs">
                            ({result.source})
                          </span>
                        )}
                      </span>
                    </div>
                    {result.success && result.data && (
                      <Badge variant="secondary" className="text-xs">
                        {result.data.confidence}% confianza
                      </Badge>
                    )}
                  </div>
                  {result.success && result.data ? (
                    <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
                      <DataField label="Marca" value={result.data.make} />
                      <DataField label="Modelo" value={result.data.model} />
                      <DataField label="Año" value={result.data.year?.toString()} />
                      <DataField
                        label="Precio"
                        value={
                          result.data.price
                            ? formatPrice(result.data.price, result.data.currency)
                            : null
                        }
                        highlight
                      />
                    </div>
                  ) : (
                    <p className="text-sm text-red-600">{result.error || 'Error desconocido'}</p>
                  )}
                  {result.success && result.data && (
                    <div className="mt-3 flex justify-end">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => {
                          setExtractedData(result.data!);
                          setBulkResults([]);
                        }}
                        className="gap-1.5 text-xs"
                      >
                        Ver detalles y publicar
                        <ArrowRight className="h-3 w-3" />
                      </Button>
                    </div>
                  )}
                </div>
              ))}
            </div>
            <div className="mt-4 flex justify-end border-t pt-4">
              <Button variant="outline" onClick={() => setBulkResults([])} className="gap-2">
                Limpiar resultados
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Extracted data preview */}
      {extractedData && (
        <Card className="mb-8 border-green-200 dark:border-green-900">
          <CardHeader>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="h-5 w-5 text-green-600" />
                <CardTitle>Datos extraídos</CardTitle>
              </div>
              <Badge
                variant={extractedData.confidence >= 70 ? 'default' : 'secondary'}
                className={
                  extractedData.confidence >= 70
                    ? 'bg-green-100 text-green-700'
                    : 'bg-yellow-100 text-yellow-700'
                }
              >
                {extractedData.confidence}% confianza
              </Badge>
            </div>
            <CardDescription>
              Revisa los datos extraídos y ajústalos antes de publicar
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              {/* Left column: Vehicle details */}
              <div className="space-y-4">
                <h3 className="flex items-center gap-2 font-semibold">
                  <Car className="h-4 w-4" />
                  Detalles del Vehículo
                </h3>

                <div className="grid grid-cols-2 gap-3">
                  <DataField label="Marca" value={extractedData.make} />
                  <DataField label="Modelo" value={extractedData.model} />
                  <DataField label="Año" value={extractedData.year?.toString()} />
                  <DataField
                    label="Precio"
                    value={
                      extractedData.price
                        ? formatPrice(extractedData.price, extractedData.currency)
                        : null
                    }
                    highlight
                  />
                  <DataField
                    label="Kilometraje"
                    value={
                      extractedData.mileage ? `${extractedData.mileage.toLocaleString()} km` : null
                    }
                  />
                  <DataField label="Transmisión" value={extractedData.transmission} />
                  <DataField label="Combustible" value={extractedData.fuelType} />
                  <DataField label="Carrocería" value={extractedData.bodyType} />
                  <DataField label="Color" value={extractedData.color} />
                  <DataField label="Condición" value={extractedData.condition} />
                  <DataField label="Motor" value={extractedData.engineSize} />
                  <DataField label="Tracción" value={extractedData.driveType} />
                  <DataField label="VIN" value={extractedData.vin} />
                </div>
              </div>

              {/* Right column: Location, description, features */}
              <div className="space-y-4">
                <div>
                  <h3 className="mb-2 font-semibold">📍 Ubicación</h3>
                  <p className="text-sm">
                    {extractedData.location || extractedData.province || 'No especificada'}
                  </p>
                </div>

                {extractedData.features.length > 0 && (
                  <div>
                    <h3 className="mb-2 font-semibold">✨ Características</h3>
                    <div className="flex flex-wrap gap-1.5">
                      {extractedData.features.map((feature, i) => (
                        <Badge key={i} variant="secondary" className="text-xs">
                          {feature}
                        </Badge>
                      ))}
                    </div>
                  </div>
                )}

                {extractedData.description && (
                  <div>
                    <h3 className="mb-2 font-semibold">📝 Descripción</h3>
                    <p className="text-muted-foreground line-clamp-4 text-sm">
                      {extractedData.description}
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="mt-6 flex flex-col gap-3 border-t pt-6 sm:flex-row sm:justify-end">
              <Button variant="outline" onClick={() => setExtractedData(null)} className="gap-2">
                <Edit3 className="h-4 w-4" />
                Modificar texto y re-extraer
              </Button>
              <Button
                onClick={handlePublish}
                className="gap-2 bg-gradient-to-r from-emerald-600 to-teal-600 text-white hover:from-emerald-700 hover:to-teal-700"
              >
                Continuar a publicar
                <ArrowRight className="h-4 w-4" />
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Supported platforms info */}
      <Card className="border-dashed">
        <CardContent className="pt-6">
          <h3 className="mb-3 text-center font-semibold">Plataformas soportadas</h3>
          <div className="flex flex-wrap justify-center gap-4">
            {[
              { name: 'Facebook Marketplace', icon: Facebook, color: 'text-blue-600' },
              { name: 'Corotos', icon: Globe, color: 'text-orange-600' },
              { name: 'SuperCarros', icon: Car, color: 'text-red-600' },
              { name: 'Otras plataformas', icon: Globe, color: 'text-gray-600' },
            ].map(platform => (
              <div
                key={platform.name}
                className="flex items-center gap-2 rounded-full border px-4 py-2 text-sm"
              >
                <platform.icon className={`h-4 w-4 ${platform.color}`} />
                {platform.name}
              </div>
            ))}
          </div>
          <p className="text-muted-foreground mt-3 text-center text-xs">
            Funciona con cualquier texto de anuncio de vehículos en español
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

function DataField({
  label,
  value,
  highlight,
}: {
  label: string;
  value: string | null | undefined;
  highlight?: boolean;
}) {
  return (
    <div>
      <span className="text-muted-foreground text-xs">{label}</span>
      <p
        className={`text-sm font-medium ${highlight ? 'text-emerald-600' : ''} ${!value ? 'text-muted-foreground italic' : ''}`}
      >
        {value || 'No detectado'}
      </p>
    </div>
  );
}
