/**
 * DealerAddVehiclePage - Agregar Nuevo Vehículo al Inventario del Dealer
 *
 * Formulario paso a paso para publicar un nuevo vehículo.
 * Reutiliza los componentes de SellYourCarPage pero con el layout del dealer.
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import Button from '@/components/atoms/Button';
import VehicleInfoStep from '@/components/organisms/sell/VehicleInfoStep';
import PhotosStep from '@/components/organisms/sell/PhotosStep';
import FeaturesStep from '@/components/organisms/sell/FeaturesStep';
import PricingStep from '@/components/organisms/sell/PricingStep';
import ReviewStep from '@/components/organisms/sell/ReviewStep';
import { FiCheck, FiArrowLeft, FiSave, FiTrash2 } from 'react-icons/fi';
import { createVehicle } from '@/services/vehicleService';
import { uploadVehicleImages } from '@/services/mediaService';
import { useAuth } from '@/hooks/useAuth';

export interface VehicleFormData {
  // Step 1: Vehicle Info
  make: string;
  model: string;
  trim?: string; // LE, SE, XLE, Sport (from VIN decode)
  year: number;
  mileage: number;
  vin: string;
  transmission?: string; // Opcional si VIN no tiene datos
  fuelType?: string; // Opcional si VIN no tiene datos
  bodyType?: string; // Opcional - VIN no siempre tiene datos
  drivetrain?: string; // Opcional si VIN no tiene datos
  engine?: string; // Opcional si VIN no tiene datos
  horsepower?: string;
  doors?: number;
  seats?: number;
  exteriorColor: string;
  interiorColor: string;
  condition: string;

  // Step 3: Features (from FeaturesStep)
  features: string[];

  // VIN-decoded data (auto-filled)
  vinBasePrice?: number; // MSRP from VIN (for price suggestion)
  vinSafetyFeatures?: string[]; // Safety features from VIN (for auto-select in FeaturesStep)

  // Step 2: Photos
  images: File[];

  // Step 4: Pricing
  price: number;
  description: string;
  location: string;
  sellerName: string;
  sellerPhone: string;
  sellerEmail: string;
}

const steps = [
  { id: 1, name: 'Información', description: 'Datos básicos del vehículo' },
  { id: 2, name: 'Fotos', description: 'Imágenes del vehículo' },
  { id: 3, name: 'Características', description: 'Equipamiento y extras' },
  { id: 4, name: 'Precio', description: 'Precio y contacto' },
  { id: 5, name: 'Revisar', description: 'Revisar y publicar' },
];

export default function DealerAddVehiclePage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [currentStep, setCurrentStep] = useState(1);
  const [showDraftModal, setShowDraftModal] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Dealer-specific defaults
  const dealerDefaults = {
    features: [],
    images: [],
    sellerName: user?.name || user?.firstName || '',
    sellerEmail: user?.email || '',
    sellerPhone: user?.phone || '',
  };

  const [formData, setFormData] = useState<Partial<VehicleFormData>>(() => {
    // Load from localStorage on mount (dealer-specific key)
    const saved = localStorage.getItem('dealer-vehicle-draft');
    if (saved) {
      try {
        return { ...dealerDefaults, ...JSON.parse(saved) };
      } catch {
        return dealerDefaults;
      }
    }
    return dealerDefaults;
  });

  // Check for saved draft on mount
  useEffect(() => {
    const saved = localStorage.getItem('dealer-vehicle-draft');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        // Check if draft has meaningful data
        if (parsed.make || parsed.model || parsed.year || parsed.vin || parsed.price) {
          setShowDraftModal(true);
        }
      } catch {
        // Ignore parsing errors
      }
    }
  }, []);

  // Save to localStorage whenever formData changes
  useEffect(() => {
    localStorage.setItem('dealer-vehicle-draft', JSON.stringify(formData));
  }, [formData]);

  const updateFormData = (data: Partial<VehicleFormData>) => {
    setFormData((prev) => ({ ...prev, ...data }));
  };

  const saveDraft = () => {
    localStorage.setItem('dealer-vehicle-draft', JSON.stringify(formData));
    alert('✅ Borrador guardado correctamente');
  };

  const clearDraft = () => {
    if (confirm('¿Estás seguro de eliminar el borrador? Esta acción no se puede deshacer.')) {
      localStorage.removeItem('dealer-vehicle-draft');
      setFormData(dealerDefaults);
      setCurrentStep(1);
    }
  };

  const nextStep = () => {
    if (currentStep < steps.length) {
      setCurrentStep(currentStep + 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const handleSubmit = async () => {
    if (isSubmitting) return;

    try {
      setIsSubmitting(true);

      // Validate required fields
      if (!formData.make || !formData.model || !formData.year || !formData.price) {
        alert('⚠️ Por favor completa los campos requeridos (Marca, Modelo, Año, Precio)');
        return;
      }

      console.log('Creating vehicle listing...', formData);

      // STEP 1: Upload images to MediaService first
      let uploadedImageUrls: string[] = [];

      if (formData.images && formData.images.length > 0) {
        console.log(`📸 Uploading ${formData.images.length} images to MediaService...`);

        try {
          const uploadResults = await uploadVehicleImages(
            formData.images,
            (current, total, progress) => {
              console.log(`Uploading image ${current}/${total} - ${progress}%`);
            }
          );

          uploadedImageUrls = uploadResults.map((result) => result.url);

          console.log(
            `✅ Successfully uploaded ${uploadedImageUrls.length} images:`,
            uploadedImageUrls
          );

          if (uploadedImageUrls.length === 0) {
            alert('⚠️ No se pudieron subir las imágenes. El vehículo se creará sin fotos.');
          }
        } catch (uploadError) {
          console.error('Error uploading images:', uploadError);
          const continueWithoutImages = confirm(
            '⚠️ Algunas imágenes no se pudieron subir. ¿Deseas continuar sin fotos?\n\n' +
              'Puedes agregar fotos después editando el vehículo.'
          );

          if (!continueWithoutImages) {
            return;
          }
        }
      }

      // STEP 2: Create vehicle with uploaded image URLs
      const vehiclePayload = {
        ...formData,
        images: uploadedImageUrls, // Replace File[] with string URLs
        dealerId: user?.dealerId, // Associate with dealer
      };

      const createdVehicle = await createVehicle(vehiclePayload as any);

      console.log('Vehicle created successfully:', createdVehicle);

      // Clear draft after successful submission
      localStorage.removeItem('dealer-vehicle-draft');

      // Show success message
      const imageCount = uploadedImageUrls.length;
      alert(
        `✅ ¡Vehículo publicado exitosamente!\n\n` +
          `Título: ${createdVehicle.title}\n` +
          `Imágenes: ${imageCount}\n\n` +
          `Redirigiendo a tu inventario...`
      );

      // Redirect to inventory
      setTimeout(() => {
        navigate('/dealer/inventory');
      }, 500);
    } catch (error) {
      console.error('Error creating vehicle listing:', error);
      const errorMessage = error instanceof Error ? error.message : 'Error desconocido';
      alert(`❌ Error al crear el vehículo:\n\n${errorMessage}\n\nPor favor intenta de nuevo.`);
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderStep = () => {
    switch (currentStep) {
      case 1:
        return (
          <VehicleInfoStep
            data={formData}
            onNext={(data) => {
              updateFormData(data);
              nextStep();
            }}
            onBack={prevStep}
          />
        );
      case 2:
        return (
          <PhotosStep
            data={formData}
            onNext={(data: Partial<VehicleFormData>) => {
              updateFormData(data);
              nextStep();
            }}
            onBack={prevStep}
          />
        );
      case 3:
        return (
          <FeaturesStep
            data={formData}
            onNext={(data: Partial<VehicleFormData>) => {
              updateFormData(data);
              nextStep();
            }}
            onBack={prevStep}
          />
        );
      case 4:
        return (
          <PricingStep
            data={formData}
            onNext={(data: Partial<VehicleFormData>) => {
              updateFormData(data);
              nextStep();
            }}
            onBack={prevStep}
          />
        );
      case 5:
        return (
          <ReviewStep
            data={formData as VehicleFormData}
            onSubmit={handleSubmit}
            onBack={prevStep}
            onSaveDraft={saveDraft}
          />
        );
      default:
        return null;
    }
  };

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-4">
            <button
              onClick={() => navigate('/dealer/inventory')}
              className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <FiArrowLeft className="w-5 h-5 text-gray-600" />
            </button>
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Agregar Vehículo</h1>
              <p className="text-gray-500 text-sm mt-1">
                Completa los pasos para publicar tu vehículo
              </p>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="hidden sm:flex items-center gap-2">
            <button
              onClick={saveDraft}
              className="flex items-center gap-2 px-4 py-2 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50"
            >
              <FiSave className="w-4 h-4" />
              Guardar Borrador
            </button>
            <button
              onClick={clearDraft}
              className="flex items-center gap-2 px-4 py-2 border border-red-200 rounded-xl text-red-600 hover:bg-red-50"
            >
              <FiTrash2 className="w-4 h-4" />
              Limpiar
            </button>
          </div>
        </div>

        {/* Stepper */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 mb-6">
          <div className="flex items-center justify-between overflow-x-auto">
            {steps.map((step, index) => (
              <div key={step.id} className="flex items-center flex-1 min-w-0">
                <div className="flex flex-col items-center flex-1">
                  {/* Circle */}
                  <div
                    className={`
                      w-10 h-10 rounded-full flex items-center justify-center font-semibold text-sm transition-all shrink-0
                      ${
                        currentStep > step.id
                          ? 'bg-green-500 text-white'
                          : currentStep === step.id
                            ? 'bg-blue-600 text-white'
                            : 'bg-gray-200 text-gray-600'
                      }
                    `}
                  >
                    {currentStep > step.id ? <FiCheck size={20} /> : step.id}
                  </div>

                  {/* Label */}
                  <div className="mt-2 text-center">
                    <p
                      className={`
                        text-sm font-medium whitespace-nowrap
                        ${currentStep >= step.id ? 'text-gray-900' : 'text-gray-500'}
                      `}
                    >
                      {step.name}
                    </p>
                    <p className="text-xs text-gray-500 mt-0.5 hidden lg:block">
                      {step.description}
                    </p>
                  </div>
                </div>

                {/* Line */}
                {index < steps.length - 1 && (
                  <div
                    className={`
                      h-0.5 flex-1 mx-2 transition-all hidden sm:block
                      ${currentStep > step.id ? 'bg-green-500' : 'bg-gray-200'}
                    `}
                    style={{ marginTop: '-40px' }}
                  />
                )}
              </div>
            ))}
          </div>

          {/* Mobile: Progress indicator */}
          <div className="sm:hidden mt-4 flex items-center justify-center gap-2 text-sm text-gray-600">
            <span className="font-medium text-blue-600">Paso {currentStep}</span>
            <span>de</span>
            <span>{steps.length}</span>
          </div>
        </div>

        {/* Step Content */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 lg:p-8">
          {renderStep()}
        </div>

        {/* Mobile: Quick Actions */}
        <div className="sm:hidden mt-4 flex gap-2">
          <button
            onClick={saveDraft}
            className="flex-1 flex items-center justify-center gap-2 py-3 border border-gray-200 rounded-xl text-gray-700"
          >
            <FiSave className="w-4 h-4" />
            Guardar
          </button>
          <button
            onClick={clearDraft}
            className="flex-1 flex items-center justify-center gap-2 py-3 border border-red-200 rounded-xl text-red-600"
          >
            <FiTrash2 className="w-4 h-4" />
            Limpiar
          </button>
        </div>
      </div>

      {/* Draft Resume Modal */}
      {showDraftModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6">
            <h3 className="text-xl font-bold text-gray-900 mb-2">¿Continuar con el borrador?</h3>
            <p className="text-gray-600 mb-6">
              Encontramos un borrador guardado. ¿Deseas continuar donde lo dejaste?
            </p>
            <div className="flex gap-3">
              <Button
                variant="outline"
                size="lg"
                onClick={() => {
                  clearDraft();
                  setShowDraftModal(false);
                }}
                className="flex-1"
              >
                Empezar de Nuevo
              </Button>
              <Button
                variant="primary"
                size="lg"
                onClick={() => setShowDraftModal(false)}
                className="flex-1"
              >
                Continuar
              </Button>
            </div>
          </div>
        </div>
      )}
    </DealerPortalLayout>
  );
}
