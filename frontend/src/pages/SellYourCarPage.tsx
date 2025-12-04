import { useState, useEffect } from 'react';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import VehicleInfoStep from '@/components/organisms/sell/VehicleInfoStep';
import PhotosStep from '@/components/organisms/sell/PhotosStep';
import FeaturesStep from '@/components/organisms/sell/FeaturesStep';
import PricingStep from '@/components/organisms/sell/PricingStep';
import ReviewStep from '@/components/organisms/sell/ReviewStep';
import { FiCheck } from 'react-icons/fi';

export interface VehicleFormData {
  // Step 1: Vehicle Info
  make: string;
  model: string;
  year: number;
  mileage: number;
  vin: string;
  transmission: string;
  fuelType: string;
  bodyType: string;
  drivetrain: string;
  engine: string;
  horsepower: string;
  mpg: string;
  exteriorColor: string;
  interiorColor: string;
  condition: string;
  features: string[];
  
  // Step 2: Photos
  images: File[];
  
  // Step 3: Pricing
  price: number;
  description: string;
  location: string;
  sellerName: string;
  sellerPhone: string;
  sellerEmail: string;
  sellerType: 'private' | 'dealer';
}

const steps = [
  { id: 1, name: 'Vehicle Info', description: 'Basic details about your vehicle' },
  { id: 2, name: 'Photos', description: 'Upload images of your vehicle' },
  { id: 3, name: 'Features & Options', description: 'Select vehicle features' },
  { id: 4, name: 'Pricing & Details', description: 'Set price and contact info' },
  { id: 5, name: 'Review', description: 'Review and publish your listing' },
];

export default function SellYourCarPage() {
  const [currentStep, setCurrentStep] = useState(1);
  const [showDraftModal, setShowDraftModal] = useState(false);
  const [formData, setFormData] = useState<Partial<VehicleFormData>>(() => {
    // Load from localStorage on mount
    const saved = localStorage.getItem('sell-vehicle-draft');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch {
        return {
          features: [],
          images: [],
          sellerType: 'private',
        };
      }
    }
    return {
      features: [],
      images: [],
      sellerType: 'private',
    };
  });

  // Check for saved draft on mount
  useEffect(() => {
    const saved = localStorage.getItem('sell-vehicle-draft');
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
    localStorage.setItem('sell-vehicle-draft', JSON.stringify(formData));
  }, [formData]);

  const updateFormData = (data: Partial<VehicleFormData>) => {
    setFormData((prev) => ({ ...prev, ...data }));
  };

  const saveDraft = () => {
    localStorage.setItem('sell-vehicle-draft', JSON.stringify(formData));
    alert('Draft saved successfully!');
  };

  const clearDraft = () => {
    if (confirm('Are you sure you want to clear your draft? This cannot be undone.')) {
      localStorage.removeItem('sell-vehicle-draft');
      setFormData({
        features: [],
        images: [],
        sellerType: 'private',
      });
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
    try {
      // TODO: Submit to API
      console.log('Submitting listing:', formData);
      // Clear draft after successful submission
      localStorage.removeItem('sell-vehicle-draft');
      alert('Listing created successfully!');
    } catch (error) {
      console.error('Error creating listing:', error);
      alert('Error creating listing. Please try again.');
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
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900 mb-2">
              Sell Your Car
            </h1>
            <p className="text-gray-600">
              List your vehicle in just a few steps and reach thousands of potential buyers
            </p>
          </div>

          {/* Stepper */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              {steps.map((step, index) => (
                <div key={step.id} className="flex items-center flex-1">
                  <div className="flex flex-col items-center flex-1">
                    {/* Circle */}
                    <div
                      className={`
                        w-10 h-10 rounded-full flex items-center justify-center font-semibold text-sm transition-all
                        ${
                          currentStep > step.id
                            ? 'bg-green-500 text-white'
                            : currentStep === step.id
                            ? 'bg-primary text-white'
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
                          text-sm font-medium
                          ${currentStep >= step.id ? 'text-gray-900' : 'text-gray-500'}
                        `}
                      >
                        {step.name}
                      </p>
                      <p className="text-xs text-gray-500 mt-0.5 hidden sm:block">
                        {step.description}
                      </p>
                    </div>
                  </div>

                  {/* Line */}
                  {index < steps.length - 1 && (
                    <div
                      className={`
                        h-0.5 flex-1 mx-2 transition-all
                        ${currentStep > step.id ? 'bg-green-500' : 'bg-gray-200'}
                      `}
                      style={{ marginTop: '-40px' }}
                    />
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Step Content */}
          <div className="bg-white rounded-xl shadow-card p-6 sm:p-8">
            {renderStep()}
          </div>

          {/* Progress Info */}
          <div className="mt-6 text-center text-sm text-gray-600">
            Step {currentStep} of {steps.length}
          </div>
        </div>
      </div>

      {/* Draft Resume Modal */}
      {showDraftModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6">
            <h3 className="text-xl font-bold text-gray-900 mb-2">
              Continue Your Draft?
            </h3>
            <p className="text-gray-600 mb-6">
              We found a saved draft of your vehicle listing. Would you like to continue where you left off?
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
                Start Fresh
              </Button>
              <Button
                variant="primary"
                size="lg"
                onClick={() => setShowDraftModal(false)}
                className="flex-1"
              >
                Continue Draft
              </Button>
            </div>
          </div>
        </div>
      )}
    </MainLayout>
  );
}
