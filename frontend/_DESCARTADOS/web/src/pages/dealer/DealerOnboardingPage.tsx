/**
 * Dealer Onboarding Page
 * Multi-step wizard for new dealer registration
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Building2,
  ArrowRight,
  ArrowLeft,
  Check,
  Store,
  Users,
  Sparkles,
  Shield,
  CreditCard,
  Truck,
  Car,
  ChevronRight,
  Zap,
  Globe,
} from 'lucide-react';
import { useCreateDealer } from '@/hooks/useDealer';
import { useAuth } from '@/hooks/useAuth';
import { DealerForm } from '@/components/dealer/DealerForm';
import { DealerType, DealerTypeLabels } from '@/types/dealer';
import type { CreateDealerRequest } from '@/types/dealer';
import toast from 'react-hot-toast';

type Step = 'welcome' | 'type' | 'form' | 'review' | 'success';

export const DealerOnboardingPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const createDealerMutation = useCreateDealer();
  const [currentStep, setCurrentStep] = useState<Step>('welcome');
  const [selectedType, setSelectedType] = useState<DealerType>(DealerType.INDEPENDENT);
  const [formData, setFormData] = useState<CreateDealerRequest | null>(null);

  const dealerTypes = [
    {
      type: DealerType.INDEPENDENT,
      icon: Store,
      label: 'Concesionario',
      description: 'Venta de vehículos usados y nuevos sin representación exclusiva de marca',
      features: ['Múltiples marcas', 'Vehículos usados y nuevos', 'Precios competitivos'],
    },
    {
      type: DealerType.FRANCHISE,
      icon: Building2,
      label: 'Dealer Certificado',
      description: 'Representante oficial de marca con showroom y servicio autorizado',
      features: ['Representación oficial', 'Garantía de fábrica', 'Servicio técnico certificado'],
    },
  ];

  const handleFormSubmit = async (data: CreateDealerRequest) => {
    if (!user?.id) return;
    setFormData({ ...data, dealerType: selectedType, ownerUserId: user.id });
    setCurrentStep('review');
  };

  const handleRegister = async () => {
    if (!formData || !user?.id) return;
    
    try {
      await createDealerMutation.mutateAsync(formData);
      
      setCurrentStep('success');
      toast.success('Dealer profile created successfully!');
    } catch (error) {
      toast.error('Failed to create dealer profile');
    }
  };

  const steps = [
    { id: 'welcome', label: 'Welcome' },
    { id: 'type', label: 'Dealer Type' },
    { id: 'form', label: 'Details' },
    { id: 'review', label: 'Review' },
    { id: 'success', label: 'Complete' },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Progress Bar */}
      {currentStep !== 'welcome' && currentStep !== 'success' && (
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-4xl mx-auto px-4 py-4">
            <div className="flex items-center justify-between">
              {steps.slice(1, -1).map((step, index) => {
                const isActive = steps.findIndex(s => s.id === currentStep) >= index + 1;
                const isCurrent = step.id === currentStep;
                
                return (
                  <div key={step.id} className="flex items-center flex-1">
                    <div className="flex items-center gap-2">
                      <div className={`w-8 h-8 rounded-full flex items-center justify-center font-medium ${
                        isActive 
                          ? 'bg-blue-600 text-white' 
                          : 'bg-gray-200 text-gray-500'
                      }`}>
                        {isActive && !isCurrent ? (
                          <Check className="h-5 w-5" />
                        ) : (
                          index + 1
                        )}
                      </div>
                      <span className={`text-sm font-medium hidden sm:block ${
                        isCurrent ? 'text-blue-600' : 'text-gray-500'
                      }`}>
                        {step.label}
                      </span>
                    </div>
                    {index < steps.length - 3 && (
                      <div className={`flex-1 h-1 mx-4 rounded ${
                        steps.findIndex(s => s.id === currentStep) > index + 1
                          ? 'bg-blue-600'
                          : 'bg-gray-200'
                      }`} />
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
            
            <h1 className="text-4xl font-bold text-gray-900 mb-4">
              Welcome to CarDealer
            </h1>
            <p className="text-xl text-gray-600 mb-12 max-w-2xl mx-auto">
              Join thousands of dealers who trust our platform to sell vehicles.
              Set up your dealership profile in just a few minutes.
            </p>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <Globe className="h-10 w-10 text-blue-600 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Reach More Buyers</h3>
                <p className="text-gray-500 text-sm">
                  Access millions of potential customers searching for vehicles
                </p>
              </div>
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <Zap className="h-10 w-10 text-amber-500 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Sell Faster</h3>
                <p className="text-gray-500 text-sm">
                  Advanced tools to help you close deals quickly
                </p>
              </div>
              <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
                <Shield className="h-10 w-10 text-green-600 mb-4" />
                <h3 className="font-semibold text-gray-900 mb-2">Trusted Platform</h3>
                <p className="text-gray-500 text-sm">
                  Verified dealers build trust with buyers
                </p>
              </div>
            </div>

            <button
              onClick={() => setCurrentStep('type')}
              className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white text-lg font-medium rounded-xl transition-colors inline-flex items-center gap-2"
            >
              Get Started
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
              Back
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-4 text-center">
              What type of dealer are you?
            </h2>
            <p className="text-gray-600 mb-8 text-center">
              Select the option that best describes your business
            </p>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
              {dealerTypes.map(({ type, icon: Icon, label, description, features }) => (
                <button
                  key={type}
                  onClick={() => setSelectedType(type)}
                  className={`p-6 rounded-xl border-2 text-left transition-all ${
                    selectedType === type
                      ? 'border-blue-600 bg-blue-50 shadow-lg'
                      : 'border-gray-200 bg-white hover:border-gray-300'
                  }`}
                >
                  <div className="flex items-start gap-4">
                    <div className={`p-3 rounded-lg ${
                      selectedType === type ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'
                    }`}>
                      <Icon className="h-6 w-6" />
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center justify-between">
                        <h3 className="font-semibold text-gray-900">{label}</h3>
                        {selectedType === type && (
                          <Check className="h-5 w-5 text-blue-600" />
                        )}
                      </div>
                      <p className="text-sm text-gray-500 mt-1 mb-3">{description}</p>
                      <div className="flex flex-wrap gap-2">
                        {features.map((feature, idx) => (
                          <span
                            key={idx}
                            className={`text-xs px-2 py-1 rounded ${
                              selectedType === type
                                ? 'bg-blue-100 text-blue-700'
                                : 'bg-gray-100 text-gray-600'
                            }`}
                          >
                            {feature}
                          </span>
                        ))}
                      </div>
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
                Continue
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
              Back to Dealer Type
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-2 text-center">
              Tell us about your dealership
            </h2>
            <p className="text-gray-600 mb-8 text-center">
              Fill in the details below to create your dealer profile
            </p>

            <DealerForm
              ownerUserId={user?.id}
              onSubmit={handleFormSubmit}
              mode="create"
            />
          </div>
        )}

        {/* Review Step */}
        {currentStep === 'review' && formData && (
          <div>
            <button
              onClick={() => setCurrentStep('form')}
              className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
            >
              <ArrowLeft className="h-5 w-5" />
              Back to Edit
            </button>

            <h2 className="text-3xl font-bold text-gray-900 mb-8 text-center">
              Review Your Information
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
                    <p className="text-blue-100">{DealerTypeLabels[selectedType]}</p>
                  </div>
                </div>
              </div>

              {/* Details */}
              <div className="p-6 space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">Contact Information</h4>
                    <div className="space-y-2">
                      <p className="text-gray-900">{formData.email}</p>
                      <p className="text-gray-900">{formData.phone}</p>
                      {formData.website && (
                        <p className="text-blue-600">{formData.website}</p>
                      )}
                    </div>
                  </div>
                  <div>
                    <h4 className="text-sm font-medium text-gray-500 mb-2">Location</h4>
                    <div className="space-y-1">
                      <p className="text-gray-900">{formData.address}</p>
                      <p className="text-gray-900">
                        {formData.city}, {formData.state} {formData.zipCode}
                      </p>
                      <p className="text-gray-900">{formData.country}</p>
                    </div>
                  </div>
                </div>

                {/* Services */}
                <div>
                  <h4 className="text-sm font-medium text-gray-500 mb-3">Services Offered</h4>
                  <div className="flex flex-wrap gap-3">
                    {formData.acceptsFinancing && (
                      <span className="flex items-center gap-2 px-3 py-1.5 bg-green-50 text-green-700 rounded-lg text-sm">
                        <CreditCard className="h-4 w-4" />
                        Financing
                      </span>
                    )}
                    {formData.acceptsTradeIn && (
                      <span className="flex items-center gap-2 px-3 py-1.5 bg-blue-50 text-blue-700 rounded-lg text-sm">
                        <Car className="h-4 w-4" />
                        Trade-In
                      </span>
                    )}
                    {formData.offersWarranty && (
                      <span className="flex items-center gap-2 px-3 py-1.5 bg-purple-50 text-purple-700 rounded-lg text-sm">
                        <Shield className="h-4 w-4" />
                        Warranty
                      </span>
                    )}
                    {formData.homeDelivery && (
                      <span className="flex items-center gap-2 px-3 py-1.5 bg-amber-50 text-amber-700 rounded-lg text-sm">
                        <Truck className="h-4 w-4" />
                        Home Delivery
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="flex justify-center gap-4">
              <button
                onClick={() => setCurrentStep('form')}
                className="px-6 py-3 border border-gray-300 rounded-xl text-gray-700 font-medium hover:bg-gray-50 transition-colors"
              >
                Edit Details
              </button>
              <button
                onClick={handleRegister}
                disabled={createDealerMutation.isPending}
                className="px-8 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
              >
                {createDealerMutation.isPending ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                    Creating...
                  </>
                ) : (
                  <>
                    Create Dealer Profile
                    <ChevronRight className="h-5 w-5" />
                  </>
                )}
              </button>
            </div>
          </div>
        )}

        {/* Success Step */}
        {currentStep === 'success' && (
          <div className="text-center">
            <div className="w-24 h-24 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-8">
              <Check className="h-12 w-12 text-green-600" />
            </div>
            
            <h2 className="text-3xl font-bold text-gray-900 mb-4">
              Welcome Aboard! 🎉
            </h2>
            <p className="text-xl text-gray-600 mb-8 max-w-lg mx-auto">
              Your dealer profile has been created successfully. 
              Start listing your vehicles and reach millions of buyers.
            </p>

            <div className="bg-amber-50 border border-amber-200 rounded-xl p-6 mb-8 max-w-lg mx-auto">
              <div className="flex items-start gap-3">
                <Shield className="h-6 w-6 text-amber-600 mt-0.5" />
                <div className="text-left">
                  <h4 className="font-semibold text-amber-800">Verification Pending</h4>
                  <p className="text-sm text-amber-700">
                    Your account is pending verification. You can start adding listings, 
                    but they won't be visible until your account is verified.
                  </p>
                </div>
              </div>
            </div>

            <div className="flex justify-center gap-4">
              <button
                onClick={() => navigate('/profile')}
                className="px-6 py-3 border border-gray-300 rounded-xl text-gray-700 font-medium hover:bg-gray-50 transition-colors"
              >
                View Profile
              </button>
              <button
                onClick={() => navigate('/dealer/listings/new')}
                className="px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-xl transition-colors inline-flex items-center gap-2"
              >
                Add Your First Listing
                <ArrowRight className="h-5 w-5" />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default DealerOnboardingPage;
