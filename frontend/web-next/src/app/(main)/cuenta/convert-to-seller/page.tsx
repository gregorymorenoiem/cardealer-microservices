/**
 * Convert to Seller Page
 * 
 * Allows buyers to convert their account to a seller account.
 * Requires authentication. Dealers/DealerEmployees are rejected.
 */

'use client';

import * as React from 'react';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  Store,
  ArrowRight,
  CheckCircle,
  AlertCircle,
  Loader2,
  ArrowLeft,
  ShieldCheck,
  DollarSign,
  BarChart3,
} from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { convertToSeller, type ConvertToSellerRequest } from '@/services/sellers';
import { sanitizeText, sanitizePhone } from '@/lib/security/sanitize';

type ConversionStep = 'info' | 'form' | 'processing' | 'success' | 'error';

export default function ConvertToSellerPage() {
  const router = useRouter();
  const [step, setStep] = useState<ConversionStep>('info');
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    businessName: '',
    description: '',
    phone: '',
    location: '',
    acceptTerms: false,
  });

  const handleInputChange = (field: string, value: string | boolean) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.acceptTerms) {
      setError('Debes aceptar los términos y condiciones para continuar.');
      return;
    }

    if (!formData.businessName.trim()) {
      setError('El nombre del negocio es requerido.');
      return;
    }

    setStep('processing');

    try {
      const sanitizedData: ConvertToSellerRequest = {
        businessName: sanitizeText(formData.businessName.trim(), { maxLength: 150 }),
        description: formData.description ? sanitizeText(formData.description.trim(), { maxLength: 2000 }) : undefined,
        phone: formData.phone ? sanitizePhone(formData.phone) : undefined,
        location: formData.location ? sanitizeText(formData.location.trim(), { maxLength: 200 }) : undefined,
        acceptTerms: formData.acceptTerms,
      };

      // Generate idempotency key to prevent duplicate conversions
      const idempotencyKey = `convert-seller-${Date.now()}-${Math.random().toString(36).slice(2)}`;

      const result = await convertToSeller(sanitizedData, idempotencyKey);

      if (result.pendingVerification) {
        setStep('success');
      } else {
        setStep('success');
      }
    } catch (err: unknown) {
      const error = err as { response?: { data?: { detail?: string; extensions?: { errorCode?: string } } } };
      const errorCode = error?.response?.data?.extensions?.errorCode;
      
      if (errorCode === 'CONVERSION_NOT_ALLOWED') {
        setError('Tu tipo de cuenta no permite esta conversión. Si eres un Dealer, ya tienes acceso a las funciones de venta.');
      } else if (errorCode === 'FEATURE_DISABLED') {
        setError('La conversión a vendedor no está disponible en este momento. Intenta más tarde.');
      } else {
        setError(error?.response?.data?.detail || 'Ocurrió un error al procesar tu solicitud. Intenta de nuevo.');
      }
      setStep('error');
    }
  };

  // ─── Step: Information ────────────────────────────────────────────────
  if (step === 'info') {
    return (
      <div className="container mx-auto max-w-3xl px-4 py-8">
        <Link href="/cuenta" className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" />
          Volver a mi cuenta
        </Link>

        <div className="mb-8">
          <h1 className="text-3xl font-bold">Conviértete en Vendedor</h1>
          <p className="mt-2 text-muted-foreground">
            Empieza a vender tus vehículos en OKLA. La conversión es rápida y sencilla.
          </p>
        </div>

        {/* Benefits */}
        <div className="mb-8 grid gap-4 sm:grid-cols-3">
          <Card>
            <CardContent className="flex flex-col items-center p-6 text-center">
              <DollarSign className="mb-3 h-10 w-10 text-green-600" />
              <h3 className="font-semibold">Publica y Vende</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Lista tus vehículos y alcanza miles de compradores.
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="flex flex-col items-center p-6 text-center">
              <BarChart3 className="mb-3 h-10 w-10 text-blue-600" />
              <h3 className="font-semibold">Estadísticas</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Accede a métricas de tus publicaciones y visitas.
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="flex flex-col items-center p-6 text-center">
              <ShieldCheck className="mb-3 h-10 w-10 text-purple-600" />
              <h3 className="font-semibold">Perfil Verificado</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Obtén la insignia de vendedor verificado.
              </p>
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardContent className="p-6">
            <h3 className="mb-4 text-lg font-semibold">¿Qué incluye?</h3>
            <ul className="space-y-3">
              {[
                'Perfil de vendedor público con tu información de contacto',
                'Publicación de vehículos con fotos y descripciones detalladas',
                'Panel de estadísticas con vistas, consultas y favoritos',
                'Acceso a mensajería directa con compradores',
                'Precio: $29 por publicación (pay-per-listing)',
              ].map((item, i) => (
                <li key={i} className="flex items-start gap-3">
                  <CheckCircle className="mt-0.5 h-5 w-5 shrink-0 text-green-600" />
                  <span className="text-sm">{item}</span>
                </li>
              ))}
            </ul>

            <div className="mt-6 rounded-lg border border-amber-200 bg-amber-50 p-4">
              <p className="text-sm text-amber-800">
                <strong>¿Tienes un negocio de vehículos?</strong> Si eres un dealer o concesionario,
                te recomendamos{' '}
                <Link href="/dealer/register" className="font-medium underline">
                  registrarte como Dealer
                </Link>{' '}
                para acceder a funciones avanzadas como inventario masivo, CRM y analytics.
              </p>
            </div>
          </CardContent>
        </Card>

        <div className="mt-6 flex justify-end">
          <Button size="lg" onClick={() => setStep('form')}>
            Continuar
            <ArrowRight className="ml-2 h-4 w-4" />
          </Button>
        </div>
      </div>
    );
  }

  // ─── Step: Form ───────────────────────────────────────────────────────
  if (step === 'form') {
    return (
      <div className="container mx-auto max-w-2xl px-4 py-8">
        <button onClick={() => setStep('info')} className="mb-6 inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" />
          Volver
        </button>

        <h1 className="mb-6 text-2xl font-bold">Información del Vendedor</h1>

        <form onSubmit={handleSubmit}>
          <Card>
            <CardContent className="space-y-5 p-6">
              <div>
                <Label htmlFor="businessName">Nombre del negocio *</Label>
                <Input
                  id="businessName"
                  placeholder="Ej: Vehículos Juan Pérez"
                  value={formData.businessName}
                  onChange={(e) => handleInputChange('businessName', e.target.value)}
                  maxLength={150}
                  required
                />
                <p className="mt-1 text-xs text-muted-foreground">
                  Este nombre se mostrará en tu perfil de vendedor.
                </p>
              </div>

              <div>
                <Label htmlFor="description">Descripción (opcional)</Label>
                <Textarea
                  id="description"
                  placeholder="Cuéntanos sobre ti como vendedor..."
                  value={formData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  maxLength={2000}
                  rows={4}
                />
              </div>

              <div>
                <Label htmlFor="phone">Teléfono de contacto (opcional)</Label>
                <Input
                  id="phone"
                  type="tel"
                  placeholder="809-555-1234"
                  value={formData.phone}
                  onChange={(e) => handleInputChange('phone', e.target.value)}
                  maxLength={20}
                />
              </div>

              <div>
                <Label htmlFor="location">Ubicación (opcional)</Label>
                <Input
                  id="location"
                  placeholder="Ej: Santo Domingo, DN"
                  value={formData.location}
                  onChange={(e) => handleInputChange('location', e.target.value)}
                  maxLength={200}
                />
              </div>

              <div className="flex items-start gap-3 rounded-lg border p-4">
                <Checkbox
                  id="acceptTerms"
                  checked={formData.acceptTerms}
                  onCheckedChange={(checked) => handleInputChange('acceptTerms', !!checked)}
                />
                <label htmlFor="acceptTerms" className="cursor-pointer text-sm leading-snug">
                  Acepto los{' '}
                  <Link href="/terminos" className="font-medium text-primary underline">
                    Términos y Condiciones
                  </Link>{' '}
                  para vendedores individuales en OKLA, incluyendo el precio de $29 por publicación.
                </label>
              </div>

              {error && (
                <div className="flex items-start gap-3 rounded-lg border border-red-200 bg-red-50 p-4">
                  <AlertCircle className="mt-0.5 h-5 w-5 shrink-0 text-red-600" />
                  <p className="text-sm text-red-700">{error}</p>
                </div>
              )}
            </CardContent>
          </Card>

          <div className="mt-6 flex justify-between">
            <Button variant="outline" type="button" onClick={() => setStep('info')}>
              Atrás
            </Button>
            <Button type="submit" disabled={!formData.acceptTerms || !formData.businessName.trim()}>
              <Store className="mr-2 h-4 w-4" />
              Convertirme en Vendedor
            </Button>
          </div>
        </form>
      </div>
    );
  }

  // ─── Step: Processing ─────────────────────────────────────────────────
  if (step === 'processing') {
    return (
      <div className="container mx-auto flex max-w-md flex-col items-center px-4 py-20 text-center">
        <Loader2 className="mb-6 h-12 w-12 animate-spin text-primary" />
        <h2 className="text-xl font-semibold">Procesando tu solicitud...</h2>
        <p className="mt-2 text-muted-foreground">
          Esto tomará solo unos segundos.
        </p>
      </div>
    );
  }

  // ─── Step: Success ────────────────────────────────────────────────────
  if (step === 'success') {
    return (
      <div className="container mx-auto flex max-w-md flex-col items-center px-4 py-20 text-center">
        <div className="mb-6 rounded-full bg-green-100 p-4">
          <CheckCircle className="h-12 w-12 text-green-600" />
        </div>
        <h2 className="text-2xl font-bold">¡Solicitud Enviada!</h2>
        <p className="mt-3 text-muted-foreground">
          Tu solicitud de conversión a vendedor ha sido recibida. Puede requerir verificación antes de activarse.
        </p>
        <div className="mt-8 flex gap-3">
          <Button variant="outline" onClick={() => router.push('/cuenta')}>
            Ir a mi cuenta
          </Button>
          <Button onClick={() => router.push('/publicar')}>
            Publicar vehículo
          </Button>
        </div>
      </div>
    );
  }

  // ─── Step: Error ──────────────────────────────────────────────────────
  return (
    <div className="container mx-auto flex max-w-md flex-col items-center px-4 py-20 text-center">
      <div className="mb-6 rounded-full bg-red-100 p-4">
        <AlertCircle className="h-12 w-12 text-red-600" />
      </div>
      <h2 className="text-2xl font-bold">Error en la Conversión</h2>
      <p className="mt-3 text-muted-foreground">
        {error || 'Ocurrió un error inesperado. Por favor intenta de nuevo.'}
      </p>
      <div className="mt-8 flex gap-3">
        <Button variant="outline" onClick={() => router.push('/cuenta')}>
          Volver a mi cuenta
        </Button>
        <Button onClick={() => { setError(null); setStep('info'); }}>
          Intentar de nuevo
        </Button>
      </div>
    </div>
  );
}
