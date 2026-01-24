/**
 * Dealer Onboarding Page V2
 *
 * Multi-step wizard for new dealer registration using Azul (Banco Popular).
 *
 * Flow:
 * 1. Welcome → Type Selection → Business Form → Submit
 * 2. Redirect to Email Verification
 * 3. Document Upload
 * 4. Azul Configuration
 * 5. Wait for Admin Approval
 * 6. Activate Account
 */

import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Building2,
  ArrowRight,
  ArrowLeft,
  Check,
  Store,
  Users,
  Shield,
  CreditCard,
  ChevronRight,
  Zap,
  Globe,
  Mail,
  FileText,
  CheckCircle,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { useRegisterDealer, useOnboardingProgress } from '@/hooks/useDealerOnboarding';
import { DealerType, getStatusLabel } from '@/services/dealerOnboardingService';
import type { RegisterDealerRequest } from '@/services/dealerOnboardingService';
import toast from 'react-hot-toast';

type Step = 'welcome' | 'type' | 'form' | 'review' | 'registered';

interface FormData {
  businessName: string;
  email: string;
  phone: string;
  rnc: string;
  dealerType: DealerType;
  address: string;
  city: string;
  province: string;
  website: string;
  selectedPlan: string;
}

const initialFormData: FormData = {
  businessName: '',
  email: '',
  phone: '',
  rnc: '',
  dealerType: DealerType.INDIVIDUAL,
  address: '',
  city: '',
  province: '',
  website: '',
  selectedPlan: 'pro',
};

// Dominican Republic provinces
const provinces = [
  'Santo Domingo',
  'Distrito Nacional',
  'Santiago',
  'La Vega',
  'Puerto Plata',
  'San Cristóbal',
  'Duarte',
  'La Romana',
  'San Pedro de Macorís',
  'Espaillat',
  'La Altagracia',
  'Peravia',
  'Azua',
  'Barahona',
  'Monte Cristi',
  'Valverde',
  'Sánchez Ramírez',
  'Monseñor Nouel',
  'Monte Plata',
  'Samaná',
  'Hermanas Mirabal',
  'María Trinidad Sánchez',
  'Hato Mayor',
  'El Seibo',
  'San José de Ocoa',
  'Independencia',
  'Pedernales',
  'Bahoruco',
  'Elías Piña',
  'San Juan',
  'Dajabón',
  'Santiago Rodríguez',
];

export const DealerOnboardingPageV2: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { user } = useAuth();
  const registerMutation = useRegisterDealer();
  const { hasOnboardingInProgress, dealerId, status } = useOnboardingProgress();

  const [currentStep, setCurrentStep] = useState<Step>('welcome');
  const [formData, setFormData] = useState<FormData>({
    ...initialFormData,
    email: user?.email || '',
    selectedPlan: searchParams.get('plan') || 'pro',
  });
  const [errors, setErrors] = useState<Partial<Record<keyof FormData, string>>>({});

  // Check if user already has onboarding in progress
  useEffect(() => {
    if (hasOnboardingInProgress && status) {
      // Redirect to appropriate step based on status
      if (!status.isEmailVerified) {
        navigate('/dealer/onboarding/verify-email');
      } else if (!status.documentsSubmitted) {
        navigate('/dealer/onboarding/documents');
      } else if (!status.azulConfigured) {
        navigate('/dealer/onboarding/payment-setup');
      } else {
        navigate('/dealer/onboarding/status');
      }
    }
  }, [hasOnboardingInProgress, status, navigate]);

  const dealerTypes = [
    {
      type: DealerType.INDIVIDUAL,
      icon: Store,
      label: 'Dealer Individual',
      description: 'Negocio independiente de venta de vehículos usados y nuevos',
      features: ['1-2 ubicaciones', 'Vehículos usados y nuevos', 'Gestión simplificada'],
    },
    {
      type: DealerType.MULTI_STORE,
      icon: Building2,
      label: 'Multi-Tienda',
      description: 'Red de concesionarios con múltiples sucursales',
      features: ['3+ ubicaciones', 'Gestión centralizada', 'Reportes consolidados'],
    },
    {
      type: DealerType.FRANCHISE,
      icon: Users,
      label: 'Franquicia',
      description: 'Representante oficial de marca con showroom autorizado',
      features: ['Representación oficial', 'Garantía de fábrica', 'Servicio certificado'],
    },
  ];

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof FormData, string>> = {};

    if (!formData.businessName.trim()) {
      newErrors.businessName = 'El nombre del negocio es requerido';
    }
    if (!formData.email.trim()) {
      newErrors.email = 'El email es requerido';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'Email inválido';
    }
    if (!formData.phone.trim()) {
      newErrors.phone = 'El teléfono es requerido';
    } else if (!/^(\+1)?8[024]9\d{7}$/.test(formData.phone.replace(/[\s-]/g, ''))) {
      newErrors.phone = 'Formato: 809-XXX-XXXX, 829-XXX-XXXX, o 849-XXX-XXXX';
    }
    if (!formData.rnc.trim()) {
      newErrors.rnc = 'El RNC es requerido';
    } else if (!/^\d{9,11}$/.test(formData.rnc.replace(/[-\s]/g, ''))) {
      newErrors.rnc = 'RNC debe tener 9-11 dígitos';
    }
    if (!formData.address.trim()) {
      newErrors.address = 'La dirección es requerida';
    }
    if (!formData.city.trim()) {
      newErrors.city = 'La ciudad es requerida';
    }
    if (!formData.province) {
      newErrors.province = 'La provincia es requerida';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (field: keyof FormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSubmit = async () => {
    try {
      const request: RegisterDealerRequest = {
        businessName: formData.businessName,
        email: formData.email,
        phone: formData.phone.replace(/[\s-]/g, ''),
        rnc: formData.rnc.replace(/[-\s]/g, ''),
        dealerType: formData.dealerType,
        address: formData.address,
        city: formData.city,
        province: formData.province,
        country: 'República Dominicana',
        website: formData.website || undefined,
        selectedPlan: formData.selectedPlan,
      };

      await registerMutation.mutateAsync(request);
      setCurrentStep('registered');
    } catch (error) {
      // Error handled by mutation
    }
  };

  const steps = [
    { id: 'type', label: 'Tipo', icon: Store },
    { id: 'form', label: 'Datos', icon: FileText },
    { id: 'review', label: 'Revisar', icon: CheckCircle },
  ];

  const stepIndex = steps.findIndex((s) => s.id === currentStep);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Progress Bar */}
      {!['welcome', 'registered'].includes(currentStep) && (
        <div className="bg-white border-b border-gray-200 sticky top-0 z-10">
          <div className="max-w-4xl mx-auto px-4 py-4">
            <div className="flex items-center justify-between">
              {steps.map((step, index) => {
                const isActive = stepIndex >= index;
                const isCurrent = step.id === currentStep;
                const Icon = step.icon;

                return (
                  <div key={step.id} className="flex items-center flex-1">
                    <div className="flex items-center gap-2">
                      <div
                        className={`w-10 h-10 rounded-full flex items-center justify-center font-medium transition-colors ${
                          isActive ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-500'
                        }`}
                      >
                        {isActive && !isCurrent ? (
                          <Check className="h-5 w-5" />
                        ) : (
                          <Icon className="h-5 w-5" />
                        )}
                      </div>
                      <span
                        className={`text-sm font-medium hidden sm:block ${
                          isCurrent ? 'text-blue-600' : 'text-gray-500'
                        }`}
                      >
                        {step.label}
                      </span>
                    </div>
                    {index < steps.length - 1 && (
                      <div
                        className={`flex-1 h-1 mx-4 rounded transition-colors ${
                          stepIndex > index ? 'bg-blue-600' : 'bg-gray-200'
                        }`}
                      />
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      <div className="max-w-4xl mx-auto px-4 py-12">
        {/* Welcome Step */}
        {currentStep === 'welcome' && (
          <div className="text-center">
            <div className="w-24 h-24 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-8">
              <Building2 className="h-12 w-12 text-blue-600" />
            </div>

            <h1 className="text-4xl font-bold text-gray-900 mb-4">Bienvenido a OKLA</h1>
            <p className="text-xl text-gray-600 mb-12 max-w-2xl mx-auto">
              Únete a los dealers que confían en nuestra plataforma para vender vehículos en
              República Dominicana. Configura tu perfil en minutos.
            </p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <Globe className="h-10 w-10 text-blue-600 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Alcanza Más Compradores</h3>
                <p className="text-gray-500 text-sm">
                  Accede a miles de compradores buscando vehículos
                </p>
              </div>
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <Zap className="h-10 w-10 text-amber-500 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Vende Más Rápido</h3>
                <p className="text-gray-500 text-sm">
                  Herramientas avanzadas para cerrar ventas rápidamente
                </p>
              </div>
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <CreditCard className="h-10 w-10 text-green-600 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Pagos con Azul</h3>
                <p className="text-gray-500 text-sm">
                  Recibe pagos con tarjetas locales vía Banco Popular
                </p>
              </div>
            </div>

            <button
              onClick={() => setCurrentStep('type')}
              className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white text-lg font-medium rounded-xl transition-colors inline-flex items-center gap-2"
            >
              Comenzar Registro
              <ArrowRight className="h-5 w-5" />
            </button>
          </div>
        )}

        {/* Dealer Type Selection */}
        {currentStep === 'type' && (
          <div>
            <button
              onClick={() => setCurrentStep('welcome')}
              className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
            >
              <ArrowLeft className="h-5 w-5" />
              Volver
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-4 text-center">
              ¿Qué tipo de dealer eres?
            </h2>
            <p className="text-gray-600 mb-8 text-center">
              Selecciona la opción que mejor describe tu negocio
            </p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
              {dealerTypes.map(({ type, icon: Icon, label, description, features }) => (
                <button
                  key={type}
                  onClick={() => setFormData((prev) => ({ ...prev, dealerType: type }))}
                  className={`p-6 rounded-xl border-2 text-left transition-all ${
                    formData.dealerType === type
                      ? 'border-blue-600 bg-blue-50 shadow-lg'
                      : 'border-gray-200 bg-white hover:border-gray-300'
                  }`}
                >
                  <div className="flex flex-col h-full">
                    <div
                      className={`p-3 rounded-lg w-fit mb-4 ${
                        formData.dealerType === type
                          ? 'bg-blue-600 text-white'
                          : 'bg-gray-100 text-gray-600'
                      }`}
                    >
                      <Icon className="h-6 w-6" />
                    </div>
                    <div className="flex items-center justify-between mb-2">
                      <h3 className="font-semibold text-gray-900">{label}</h3>
                      {formData.dealerType === type && <Check className="h-5 w-5 text-blue-600" />}
                    </div>
                    <p className="text-sm text-gray-500 mb-4">{description}</p>
                    <div className="mt-auto flex flex-wrap gap-2">
                      {features.map((feature, idx) => (
                        <span
                          key={idx}
                          className={`text-xs px-2 py-1 rounded ${
                            formData.dealerType === type
                              ? 'bg-blue-100 text-blue-700'
                              : 'bg-gray-100 text-gray-600'
                          }`}
                        >
                          {feature}
                        </span>
                      ))}
                    </div>
                  </div>
                </button>
              ))}
            </div>

            <div className="flex justify-center">
              <button
                onClick={() => setCurrentStep('form')}
                className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
              >
                Continuar
                <ArrowRight className="h-5 w-5" />
              </button>
            </div>
          </div>
        )}

        {/* Form Step */}
        {currentStep === 'form' && (
          <div>
            <button
              onClick={() => setCurrentStep('type')}
              className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
            >
              <ArrowLeft className="h-5 w-5" />
              Volver
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-2 text-center">
              Datos de tu Negocio
            </h2>
            <p className="text-gray-600 mb-8 text-center">
              Completa la información de tu concesionario
            </p>

            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Business Name */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Nombre del Negocio *
                  </label>
                  <input
                    type="text"
                    value={formData.businessName}
                    onChange={(e) => handleInputChange('businessName', e.target.value)}
                    placeholder="Ej: Auto Import RD"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.businessName ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.businessName && (
                    <p className="mt-1 text-sm text-red-500">{errors.businessName}</p>
                  )}
                </div>

                {/* Email */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Email del Negocio *
                  </label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={(e) => handleInputChange('email', e.target.value)}
                    placeholder="ventas@tunegocio.com"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.email ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.email && <p className="mt-1 text-sm text-red-500">{errors.email}</p>}
                </div>

                {/* Phone */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Teléfono *</label>
                  <input
                    type="tel"
                    value={formData.phone}
                    onChange={(e) => handleInputChange('phone', e.target.value)}
                    placeholder="809-555-1234"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.phone ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.phone && <p className="mt-1 text-sm text-red-500">{errors.phone}</p>}
                </div>

                {/* RNC */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    RNC (Registro Nacional de Contribuyentes) *
                  </label>
                  <input
                    type="text"
                    value={formData.rnc}
                    onChange={(e) => handleInputChange('rnc', e.target.value)}
                    placeholder="123456789"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.rnc ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.rnc && <p className="mt-1 text-sm text-red-500">{errors.rnc}</p>}
                </div>

                {/* Website */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Sitio Web (opcional)
                  </label>
                  <input
                    type="url"
                    value={formData.website}
                    onChange={(e) => handleInputChange('website', e.target.value)}
                    placeholder="https://www.tunegocio.com"
                    className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                {/* Address */}
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Dirección *
                  </label>
                  <input
                    type="text"
                    value={formData.address}
                    onChange={(e) => handleInputChange('address', e.target.value)}
                    placeholder="Calle Principal #123, Sector Las Mercedes"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.address ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.address && <p className="mt-1 text-sm text-red-500">{errors.address}</p>}
                </div>

                {/* City */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Ciudad *</label>
                  <input
                    type="text"
                    value={formData.city}
                    onChange={(e) => handleInputChange('city', e.target.value)}
                    placeholder="Santo Domingo"
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.city ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  />
                  {errors.city && <p className="mt-1 text-sm text-red-500">{errors.city}</p>}
                </div>

                {/* Province */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Provincia *
                  </label>
                  <select
                    value={formData.province}
                    onChange={(e) => handleInputChange('province', e.target.value)}
                    className={`w-full px-4 py-3 rounded-lg border ${
                      errors.province ? 'border-red-500' : 'border-gray-300'
                    } focus:ring-2 focus:ring-blue-500 focus:border-transparent`}
                  >
                    <option value="">Seleccionar provincia</option>
                    {provinces.map((province) => (
                      <option key={province} value={province}>
                        {province}
                      </option>
                    ))}
                  </select>
                  {errors.province && (
                    <p className="mt-1 text-sm text-red-500">{errors.province}</p>
                  )}
                </div>
              </div>

              <div className="mt-8 flex justify-end">
                <button
                  onClick={() => {
                    if (validateForm()) {
                      setCurrentStep('review');
                    }
                  }}
                  className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
                >
                  Revisar Información
                  <ArrowRight className="h-5 w-5" />
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Review Step */}
        {currentStep === 'review' && (
          <div>
            <button
              onClick={() => setCurrentStep('form')}
              className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
            >
              <ArrowLeft className="h-5 w-5" />
              Volver a Editar
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-8 text-center">
              Revisa tu Información
            </h2>

            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden mb-8">
              {/* Header */}
              <div className="bg-gradient-to-r from-blue-600 to-blue-700 p-6 text-white">
                <div className="flex items-center gap-4">
                  <div className="w-16 h-16 bg-white rounded-lg flex items-center justify-center">
                    <Building2 className="h-8 w-8 text-blue-600" />
                  </div>
                  <div>
                    <h3 className="text-xl font-bold">{formData.businessName}</h3>
                    <p className="text-blue-100">
                      {formData.dealerType === DealerType.INDIVIDUAL && 'Dealer Individual'}
                      {formData.dealerType === DealerType.MULTI_STORE && 'Multi-Tienda'}
                      {formData.dealerType === DealerType.FRANCHISE && 'Franquicia'}
                    </p>
                  </div>
                </div>
              </div>

              {/* Details */}
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">
                      Información de Contacto
                    </h4>
                    <div className="space-y-2">
                      <p className="text-gray-900">{formData.email}</p>
                      <p className="text-gray-900">{formData.phone}</p>
                      {formData.website && <p className="text-blue-600">{formData.website}</p>}
                    </div>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">Ubicación</h4>
                    <div className="space-y-1">
                      <p className="text-gray-900">{formData.address}</p>
                      <p className="text-gray-900">
                        {formData.city}, {formData.province}
                      </p>
                      <p className="text-gray-900">República Dominicana</p>
                    </div>
                  </div>
                </div>

                <div>
                  <h4 className="text-sm font-medium text-gray-500 mb-2">Información Fiscal</h4>
                  <p className="text-gray-900">RNC: {formData.rnc}</p>
                </div>
              </div>
            </div>

            {/* Info box about next steps */}
            <div className="bg-blue-50 border border-blue-200 rounded-xl p-6 mb-8">
              <div className="flex items-start gap-3">
                <Mail className="h-6 w-6 text-blue-600 mt-0.5" />
                <div>
                  <h4 className="font-semibold text-blue-800">Próximos Pasos</h4>
                  <p className="text-sm text-blue-700 mt-1">
                    Después de registrarte, te enviaremos un código de verificación a tu email.
                    Luego podrás subir los documentos requeridos y configurar tus credenciales de
                    Azul (Banco Popular) para recibir pagos.
                  </p>
                </div>
              </div>
            </div>

            <div className="flex justify-center gap-4">
              <button
                onClick={() => setCurrentStep('form')}
                className="px-6 py-3 border border-gray-300 rounded-xl text-gray-700 font-medium hover:bg-gray-50 transition-colors"
              >
                Editar Datos
              </button>
              <button
                onClick={handleSubmit}
                disabled={registerMutation.isPending}
                className="px-8 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
              >
                {registerMutation.isPending ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                    Registrando...
                  </>
                ) : (
                  <>
                    Registrar Dealer
                    <ChevronRight className="h-5 w-5" />
                  </>
                )}
              </button>
            </div>
          </div>
        )}

        {/* Registered Success Step */}
        {currentStep === 'registered' && (
          <div className="text-center">
            <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-8">
              <Check className="h-12 w-12 text-green-600" />
            </div>

            <h2 className="text-3xl font-bold text-gray-900 mb-4">¡Registro Exitoso! 🎉</h2>
            <p className="text-xl text-gray-600 mb-8 max-w-lg mx-auto">
              Te hemos enviado un código de verificación a{' '}
              <span className="font-semibold">{formData.email}</span>
            </p>

            <div className="bg-amber-50 border border-amber-200 rounded-xl p-6 mb-8 max-w-lg mx-auto">
              <div className="flex items-start gap-3">
                <Mail className="h-6 w-6 text-amber-600 mt-0.5" />
                <div className="text-left">
                  <h4 className="font-semibold text-amber-800">Verifica tu Email</h4>
                  <p className="text-sm text-amber-700">
                    Revisa tu bandeja de entrada y la carpeta de spam. Ingresa el código de 6
                    dígitos para continuar con el proceso.
                  </p>
                </div>
              </div>
            </div>

            <button
              onClick={() => navigate('/dealer/onboarding/verify-email')}
              className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
            >
              Verificar Email
              <ArrowRight className="h-5 w-5" />
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default DealerOnboardingPageV2;
