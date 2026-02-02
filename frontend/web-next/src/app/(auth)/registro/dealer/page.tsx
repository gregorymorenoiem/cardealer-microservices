/**
 * Dealer Registration Page
 *
 * Registration form for dealers
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Progress } from '@/components/ui/progress';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Car,
  Building2,
  User,
  MapPin,
  ArrowLeft,
  ArrowRight,
  Check,
  Upload,
  Shield,
  Star,
  TrendingUp,
} from 'lucide-react';
import Link from 'next/link';

const steps = [
  { id: 1, title: 'Tipo de Cuenta', icon: Building2 },
  { id: 2, title: 'Información del Negocio', icon: User },
  { id: 3, title: 'Ubicación', icon: MapPin },
  { id: 4, title: 'Verificación', icon: Shield },
];

const benefits = [
  'Panel de control profesional',
  'Publicar múltiples vehículos',
  'Estadísticas avanzadas',
  'Importar inventario CSV',
  'CRM de leads incluido',
  'Badge de verificación',
];

export default function DealerRegistrationPage() {
  const [currentStep, setCurrentStep] = useState(1);
  const [accountType, setAccountType] = useState<'individual' | 'company'>('company');
  const [formData, setFormData] = useState({
    // Step 1
    dealerType: '',
    // Step 2
    businessName: '',
    rnc: '',
    contactName: '',
    email: '',
    phone: '',
    website: '',
    // Step 3
    address: '',
    city: '',
    province: '',
    // Step 4
    agreeTerms: false,
    agreeVerification: false,
  });

  const handleChange = (field: string, value: string | boolean) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const nextStep = () => {
    if (currentStep < 4) setCurrentStep(currentStep + 1);
  };

  const prevStep = () => {
    if (currentStep > 1) setCurrentStep(currentStep - 1);
  };

  const progress = (currentStep / 4) * 100;

  return (
    <div className="min-h-screen bg-gradient-to-br from-emerald-600 to-teal-700">
      {/* Header */}
      <div className="bg-white/10 backdrop-blur-sm">
        <div className="mx-auto max-w-6xl px-4 py-4">
          <Link href="/" className="inline-flex items-center gap-2 text-white">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-white">
              <Car className="h-6 w-6 text-emerald-600" />
            </div>
            <span className="text-2xl font-bold">OKLA</span>
          </Link>
        </div>
      </div>

      <div className="mx-auto max-w-6xl px-4 py-12">
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          {/* Left: Benefits */}
          <div className="hidden lg:block">
            <div className="sticky top-8">
              <h2 className="mb-6 text-2xl font-bold text-white">Únete como Dealer</h2>
              <p className="mb-8 text-emerald-100">
                Accede a herramientas profesionales para vender más vehículos.
              </p>

              <div className="space-y-4">
                {benefits.map((benefit, i) => (
                  <div key={i} className="flex items-center gap-3 text-white">
                    <div className="flex h-6 w-6 items-center justify-center rounded-full bg-emerald-500">
                      <Check className="h-4 w-4" />
                    </div>
                    <span>{benefit}</span>
                  </div>
                ))}
              </div>

              {/* Testimonial */}
              <div className="mt-12 rounded-xl bg-white/10 p-6 backdrop-blur-sm">
                <div className="mb-3 flex items-center gap-1">
                  {[1, 2, 3, 4, 5].map(i => (
                    <Star key={i} className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                  ))}
                </div>
                <p className="mb-4 text-white italic">
                  "Desde que me uní a OKLA, mis ventas aumentaron un 40%. El panel es muy fácil de
                  usar."
                </p>
                <p className="font-medium text-emerald-200">— Carlos, Auto Premium RD</p>
              </div>
            </div>
          </div>

          {/* Right: Form */}
          <div className="lg:col-span-2">
            <Card className="shadow-xl">
              <CardHeader>
                <div className="mb-6 flex items-center justify-between">
                  <CardTitle>Registro de Dealer</CardTitle>
                  <span className="text-sm text-gray-500">Paso {currentStep} de 4</span>
                </div>
                <Progress value={progress} className="h-2" />

                {/* Steps indicator */}
                <div className="mt-4 flex justify-between">
                  {steps.map(step => {
                    const Icon = step.icon;
                    const isActive = currentStep === step.id;
                    const isCompleted = currentStep > step.id;
                    return (
                      <div key={step.id} className="flex flex-col items-center">
                        <div
                          className={`flex h-10 w-10 items-center justify-center rounded-full ${
                            isCompleted
                              ? 'bg-emerald-600 text-white'
                              : isActive
                                ? 'border-2 border-emerald-600 bg-emerald-100 text-emerald-600'
                                : 'bg-gray-100 text-gray-400'
                          }`}
                        >
                          {isCompleted ? (
                            <Check className="h-5 w-5" />
                          ) : (
                            <Icon className="h-5 w-5" />
                          )}
                        </div>
                        <span
                          className={`mt-2 text-xs ${isActive || isCompleted ? 'text-gray-900' : 'text-gray-400'}`}
                        >
                          {step.title}
                        </span>
                      </div>
                    );
                  })}
                </div>
              </CardHeader>

              <CardContent className="space-y-6">
                {currentStep === 1 && (
                  <div className="space-y-6">
                    <div>
                      <h3 className="mb-4 text-lg font-semibold">¿Qué tipo de dealer eres?</h3>
                      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                        <button
                          onClick={() => handleChange('dealerType', 'independent')}
                          className={`rounded-xl border-2 p-6 text-left transition-colors ${
                            formData.dealerType === 'independent'
                              ? 'border-emerald-600 bg-emerald-50'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <Building2 className="mb-3 h-8 w-8 text-emerald-600" />
                          <h4 className="font-semibold">Dealer Independiente</h4>
                          <p className="mt-1 text-sm text-gray-500">
                            Un solo local, inventario propio
                          </p>
                        </button>
                        <button
                          onClick={() => handleChange('dealerType', 'chain')}
                          className={`rounded-xl border-2 p-6 text-left transition-colors ${
                            formData.dealerType === 'chain'
                              ? 'border-emerald-600 bg-emerald-50'
                              : 'border-gray-200 hover:border-gray-300'
                          }`}
                        >
                          <TrendingUp className="mb-3 h-8 w-8 text-emerald-600" />
                          <h4 className="font-semibold">Cadena / Multi-sucursal</h4>
                          <p className="mt-1 text-sm text-gray-500">
                            Múltiples locales o franquicia
                          </p>
                        </button>
                      </div>
                    </div>

                    <div className="pt-4">
                      <h4 className="mb-3 font-medium">¿Eres persona física o jurídica?</h4>
                      <div className="flex gap-4">
                        <button
                          onClick={() => setAccountType('individual')}
                          className={`flex-1 rounded-lg border-2 px-4 py-3 ${
                            accountType === 'individual'
                              ? 'border-emerald-600 bg-emerald-50'
                              : 'border-gray-200'
                          }`}
                        >
                          Persona Física
                        </button>
                        <button
                          onClick={() => setAccountType('company')}
                          className={`flex-1 rounded-lg border-2 px-4 py-3 ${
                            accountType === 'company'
                              ? 'border-emerald-600 bg-emerald-50'
                              : 'border-gray-200'
                          }`}
                        >
                          Empresa (RNC)
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                {currentStep === 2 && (
                  <div className="space-y-4">
                    <h3 className="text-lg font-semibold">Información del Negocio</h3>

                    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                      <div className="md:col-span-2">
                        <label className="mb-2 block text-sm font-medium">
                          Nombre del Negocio *
                        </label>
                        <Input
                          placeholder="Ej: Auto Premium RD"
                          value={formData.businessName}
                          onChange={e => handleChange('businessName', e.target.value)}
                        />
                      </div>

                      {accountType === 'company' && (
                        <div className="md:col-span-2">
                          <label className="mb-2 block text-sm font-medium">RNC *</label>
                          <Input
                            placeholder="000-00000-0"
                            value={formData.rnc}
                            onChange={e => handleChange('rnc', e.target.value)}
                          />
                        </div>
                      )}

                      <div>
                        <label className="mb-2 block text-sm font-medium">
                          Nombre del Contacto *
                        </label>
                        <Input
                          placeholder="Tu nombre"
                          value={formData.contactName}
                          onChange={e => handleChange('contactName', e.target.value)}
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm font-medium">
                          Correo Electrónico *
                        </label>
                        <Input
                          type="email"
                          placeholder="email@tudealer.com"
                          value={formData.email}
                          onChange={e => handleChange('email', e.target.value)}
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm font-medium">Teléfono *</label>
                        <Input
                          type="tel"
                          placeholder="809-555-0123"
                          value={formData.phone}
                          onChange={e => handleChange('phone', e.target.value)}
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm font-medium">
                          Sitio Web (opcional)
                        </label>
                        <Input
                          placeholder="https://tudealer.com"
                          value={formData.website}
                          onChange={e => handleChange('website', e.target.value)}
                        />
                      </div>
                    </div>
                  </div>
                )}

                {currentStep === 3 && (
                  <div className="space-y-4">
                    <h3 className="text-lg font-semibold">Ubicación del Negocio</h3>

                    <div>
                      <label className="mb-2 block text-sm font-medium">Dirección *</label>
                      <Textarea
                        placeholder="Av. Principal #123, Local 4..."
                        value={formData.address}
                        onChange={e => handleChange('address', e.target.value)}
                      />
                    </div>

                    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                      <div>
                        <label className="mb-2 block text-sm font-medium">Ciudad *</label>
                        <Input
                          placeholder="Santo Domingo"
                          value={formData.city}
                          onChange={e => handleChange('city', e.target.value)}
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm font-medium">Provincia *</label>
                        <Select
                          value={formData.province}
                          onValueChange={v => handleChange('province', v)}
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Seleccionar provincia" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="dn">Distrito Nacional</SelectItem>
                            <SelectItem value="sd">Santo Domingo</SelectItem>
                            <SelectItem value="stgo">Santiago</SelectItem>
                            <SelectItem value="pn">Puerto Plata</SelectItem>
                            <SelectItem value="lv">La Vega</SelectItem>
                            <SelectItem value="spm">San Pedro de Macorís</SelectItem>
                            <SelectItem value="lr">La Romana</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                    </div>

                    <div className="rounded-lg bg-gray-50 p-4">
                      <p className="flex items-center gap-2 text-sm text-gray-600">
                        <MapPin className="h-4 w-4" />
                        Podrás agregar múltiples ubicaciones después de completar el registro.
                      </p>
                    </div>
                  </div>
                )}

                {currentStep === 4 && (
                  <div className="space-y-6">
                    <h3 className="text-lg font-semibold">Verificación</h3>

                    <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-6">
                      <Shield className="mb-4 h-10 w-10 text-emerald-600" />
                      <h4 className="mb-2 text-lg font-semibold">Verificación de Dealer</h4>
                      <p className="mb-4 text-sm text-gray-600">
                        Para garantizar la confianza de los compradores, verificamos todos los
                        dealers. Después de registrarte, deberás subir:
                      </p>
                      <ul className="space-y-2 text-sm text-gray-600">
                        <li className="flex items-center gap-2">
                          <Check className="h-4 w-4 text-emerald-600" />
                          Cédula del representante legal
                        </li>
                        <li className="flex items-center gap-2">
                          <Check className="h-4 w-4 text-emerald-600" />
                          RNC y documentos de la empresa
                        </li>
                        <li className="flex items-center gap-2">
                          <Check className="h-4 w-4 text-emerald-600" />
                          Fotos del local/establecimiento
                        </li>
                      </ul>
                    </div>

                    <div className="space-y-4">
                      <div className="flex items-start gap-3">
                        <Checkbox
                          id="terms"
                          checked={formData.agreeTerms as boolean}
                          onCheckedChange={checked =>
                            handleChange('agreeTerms', checked as boolean)
                          }
                        />
                        <label htmlFor="terms" className="text-sm text-gray-600">
                          Acepto los{' '}
                          <Link href="/terminos" className="text-emerald-600 hover:underline">
                            Términos y Condiciones
                          </Link>{' '}
                          y la{' '}
                          <Link href="/privacidad" className="text-emerald-600 hover:underline">
                            Política de Privacidad
                          </Link>
                        </label>
                      </div>

                      <div className="flex items-start gap-3">
                        <Checkbox
                          id="verification"
                          checked={formData.agreeVerification as boolean}
                          onCheckedChange={checked =>
                            handleChange('agreeVerification', checked as boolean)
                          }
                        />
                        <label htmlFor="verification" className="text-sm text-gray-600">
                          Entiendo que debo completar la verificación de documentos para activar mi
                          cuenta
                        </label>
                      </div>
                    </div>
                  </div>
                )}

                {/* Navigation */}
                <div className="flex justify-between border-t pt-6">
                  {currentStep > 1 ? (
                    <Button variant="outline" onClick={prevStep}>
                      <ArrowLeft className="mr-2 h-4 w-4" />
                      Anterior
                    </Button>
                  ) : (
                    <Link href="/auth/registro">
                      <Button variant="outline">
                        <ArrowLeft className="mr-2 h-4 w-4" />
                        Volver
                      </Button>
                    </Link>
                  )}

                  {currentStep < 4 ? (
                    <Button className="bg-emerald-600 hover:bg-emerald-700" onClick={nextStep}>
                      Continuar
                      <ArrowRight className="ml-2 h-4 w-4" />
                    </Button>
                  ) : (
                    <Link href="/dealer">
                      <Button
                        className="bg-emerald-600 hover:bg-emerald-700"
                        disabled={!formData.agreeTerms || !formData.agreeVerification}
                      >
                        Crear Cuenta de Dealer
                        <ArrowRight className="ml-2 h-4 w-4" />
                      </Button>
                    </Link>
                  )}
                </div>
              </CardContent>
            </Card>

            <p className="mt-6 text-center text-sm text-emerald-100">
              ¿Ya tienes cuenta de dealer?{' '}
              <Link href="/auth/login" className="font-medium text-white hover:underline">
                Inicia sesión
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
