/**
 * ListingFormPage - Create or edit a marketplace listing
 */

import React, { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import type { 
  MarketplaceVertical, 
  VehicleBodyType,
  VehicleTransmission,
  VehicleFuelType,
  VehicleDrivetrain,
  VehicleCondition,
  PropertyType,
  PropertyListingType,
} from '@/types/marketplace';
import {
  ArrowLeftIcon,
  PhotoIcon,
  XMarkIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
} from '@heroicons/react/24/outline';

// Form step types
type FormStep = 'category' | 'details' | 'images' | 'pricing' | 'review';

const steps: { id: FormStep; label: string }[] = [
  { id: 'category', label: 'Categoría' },
  { id: 'details', label: 'Detalles' },
  { id: 'images', label: 'Imágenes' },
  { id: 'pricing', label: 'Precio' },
  { id: 'review', label: 'Revisar' },
];

// Vehicle form data
interface VehicleFormData {
  title: string;
  description: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  transmission: VehicleTransmission;
  fuelType: VehicleFuelType;
  bodyType: VehicleBodyType;
  drivetrain: VehicleDrivetrain;
  condition: VehicleCondition;
  exteriorColor: string;
  interiorColor: string;
  engine: string;
  horsepower: string;
  features: string[];
  price: number;
  currency: string;
  city: string;
  state: string;
}

// Property form data
interface PropertyFormData {
  title: string;
  description: string;
  propertyType: PropertyType;
  listingType: PropertyListingType;
  bedrooms: number;
  bathrooms: number;
  totalArea: number;
  parkingSpaces: number;
  yearBuilt: string;
  hasGarden: boolean;
  hasPool: boolean;
  hasGym: boolean;
  hasElevator: boolean;
  hasSecurity: boolean;
  isFurnished: boolean;
  allowsPets: boolean;
  price: number;
  maintenanceFee: string;
  currency: string;
  city: string;
  state: string;
  address: string;
}

const ListingFormPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditing = !!id;
  
  const [currentStep, setCurrentStep] = useState<FormStep>('category');
  const [selectedVertical, setSelectedVertical] = useState<MarketplaceVertical | null>(null);
  const [, setImages] = useState<File[]>([]);
  const [imagePreviews, setImagePreviews] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Vehicle form state
  const [vehicleData, setVehicleData] = useState<VehicleFormData>({
    title: '',
    description: '',
    make: '',
    model: '',
    year: new Date().getFullYear(),
    mileage: 0,
    transmission: 'automatic',
    fuelType: 'gasoline',
    bodyType: 'sedan',
    drivetrain: 'fwd',
    condition: 'used',
    exteriorColor: '',
    interiorColor: '',
    engine: '',
    horsepower: '',
    features: [],
    price: 0,
    currency: 'MXN',
    city: '',
    state: '',
  });

  // Property form state
  const [propertyData, setPropertyData] = useState<PropertyFormData>({
    title: '',
    description: '',
    propertyType: 'house',
    listingType: 'sale',
    bedrooms: 1,
    bathrooms: 1,
    totalArea: 0,
    parkingSpaces: 0,
    yearBuilt: '',
    hasGarden: false,
    hasPool: false,
    hasGym: false,
    hasElevator: false,
    hasSecurity: false,
    isFurnished: false,
    allowsPets: false,
    price: 0,
    maintenanceFee: '',
    currency: 'MXN',
    city: '',
    state: '',
    address: '',
  });

  // Handle image upload
  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files) return;

    const newFiles = Array.from(files);
    setImages((prev) => [...prev, ...newFiles]);

    // Create previews
    newFiles.forEach((file) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreviews((prev) => [...prev, reader.result as string]);
      };
      reader.readAsDataURL(file);
    });
  };

  // Remove image
  const removeImage = (index: number) => {
    setImages((prev) => prev.filter((_, i) => i !== index));
    setImagePreviews((prev) => prev.filter((_, i) => i !== index));
  };

  // Navigate steps
  const goToStep = (step: FormStep) => {
    setCurrentStep(step);
  };

  const nextStep = () => {
    const currentIndex = steps.findIndex((s) => s.id === currentStep);
    if (currentIndex < steps.length - 1) {
      setCurrentStep(steps[currentIndex + 1].id);
    }
  };

  const prevStep = () => {
    const currentIndex = steps.findIndex((s) => s.id === currentStep);
    if (currentIndex > 0) {
      setCurrentStep(steps[currentIndex - 1].id);
    }
  };

  // Handle submit
  const handleSubmit = async () => {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 2000));
      
      // Success - navigate to dashboard
      navigate('/marketplace/seller');
    } catch (error) {
      setSubmitError('Error al publicar. Por favor intenta de nuevo.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Get step content
  const renderStepContent = () => {
    switch (currentStep) {
      case 'category':
        return (
          <CategoryStep
            selectedVertical={selectedVertical}
            onSelect={(v) => {
              setSelectedVertical(v);
              nextStep();
            }}
          />
        );
      case 'details':
        return selectedVertical === 'vehicles' ? (
          <VehicleDetailsStep
            data={vehicleData}
            onChange={(data) => setVehicleData((prev) => ({ ...prev, ...data }))}
          />
        ) : (
          <PropertyDetailsStep
            data={propertyData}
            onChange={(data) => setPropertyData((prev) => ({ ...prev, ...data }))}
          />
        );
      case 'images':
        return (
          <ImagesStep
            previews={imagePreviews}
            onUpload={handleImageUpload}
            onRemove={removeImage}
          />
        );
      case 'pricing':
        return selectedVertical === 'vehicles' ? (
          <VehiclePricingStep
            data={vehicleData}
            onChange={(data) => setVehicleData((prev) => ({ ...prev, ...data }))}
          />
        ) : (
          <PropertyPricingStep
            data={propertyData}
            onChange={(data) => setPropertyData((prev) => ({ ...prev, ...data }))}
          />
        );
      case 'review':
        return (
          <ReviewStep
            vertical={selectedVertical!}
            vehicleData={vehicleData}
            propertyData={propertyData}
            images={imagePreviews}
          />
        );
      default:
        return null;
    }
  };

  const currentStepIndex = steps.findIndex((s) => s.id === currentStep);
  const isFirstStep = currentStepIndex === 0;
  const isLastStep = currentStepIndex === steps.length - 1;
  const canProceed = currentStep === 'category' ? selectedVertical !== null : true;

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex items-center gap-4 mb-8">
          <Link
            to="/marketplace/seller"
            className="p-2 rounded-lg hover:bg-gray-100"
          >
            <ArrowLeftIcon className="h-5 w-5 text-gray-600" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              {isEditing ? 'Editar publicación' : 'Nueva publicación'}
            </h1>
            <p className="text-gray-500">
              {isEditing ? 'Actualiza los detalles de tu publicación' : 'Crea una nueva publicación en el marketplace'}
            </p>
          </div>
        </div>

        {/* Progress Steps */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            {steps.map((step, index) => {
              const isActive = step.id === currentStep;
              const isCompleted = index < currentStepIndex;
              const isClickable = isCompleted || (index === currentStepIndex + 1 && canProceed);

              return (
                <React.Fragment key={step.id}>
                  <button
                    onClick={() => isClickable && goToStep(step.id)}
                    disabled={!isClickable && !isActive}
                    className={`flex items-center gap-2 ${isClickable || isActive ? 'cursor-pointer' : 'cursor-not-allowed opacity-50'}`}
                  >
                    <div
                      className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-colors ${
                        isActive
                          ? 'bg-blue-600 text-white'
                          : isCompleted
                          ? 'bg-green-500 text-white'
                          : 'bg-gray-200 text-gray-600'
                      }`}
                    >
                      {isCompleted ? (
                        <CheckCircleIcon className="h-5 w-5" />
                      ) : (
                        index + 1
                      )}
                    </div>
                    <span className={`hidden sm:block text-sm font-medium ${
                      isActive ? 'text-blue-600' : 'text-gray-500'
                    }`}>
                      {step.label}
                    </span>
                  </button>
                  {index < steps.length - 1 && (
                    <div className={`flex-1 h-0.5 mx-2 ${
                      isCompleted ? 'bg-green-500' : 'bg-gray-200'
                    }`} />
                  )}
                </React.Fragment>
              );
            })}
          </div>
        </div>

        {/* Form Content */}
        <motion.div
          key={currentStep}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          {renderStepContent()}
        </motion.div>

        {/* Error Message */}
        {submitError && (
          <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-2 text-red-700">
            <ExclamationCircleIcon className="h-5 w-5 flex-shrink-0" />
            {submitError}
          </div>
        )}

        {/* Navigation Buttons */}
        <div className="flex items-center justify-between mt-6">
          <button
            onClick={prevStep}
            disabled={isFirstStep}
            className={`px-4 py-2 rounded-lg transition-colors ${
              isFirstStep
                ? 'text-gray-400 cursor-not-allowed'
                : 'text-gray-600 hover:bg-gray-100'
            }`}
          >
            Anterior
          </button>

          {isLastStep ? (
            <button
              onClick={handleSubmit}
              disabled={isSubmitting}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
            >
              {isSubmitting ? (
                <>
                  <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Publicando...
                </>
              ) : (
                'Publicar'
              )}
            </button>
          ) : (
            <button
              onClick={nextStep}
              disabled={!canProceed}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Siguiente
            </button>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

// Category Selection Step
const CategoryStep: React.FC<{
  selectedVertical: MarketplaceVertical | null;
  onSelect: (vertical: MarketplaceVertical) => void;
}> = ({ selectedVertical, onSelect }) => {
  const categories = [
    {
      id: 'vehicles' as MarketplaceVertical,
      title: 'Vehículo',
      description: 'Autos, motos, camionetas y más',
      icon: '🚗',
      gradient: 'from-blue-500 to-cyan-500',
    },
    {
      id: 'real-estate' as MarketplaceVertical,
      title: 'Propiedad',
      description: 'Casas, apartamentos, terrenos',
      icon: '🏠',
      gradient: 'from-emerald-500 to-teal-500',
    },
  ];

  return (
    <div>
      <h2 className="text-lg font-semibold text-gray-900 mb-2">
        ¿Qué quieres publicar?
      </h2>
      <p className="text-gray-500 mb-6">
        Selecciona la categoría de tu publicación
      </p>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {categories.map((category) => (
          <motion.button
            key={category.id}
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            onClick={() => onSelect(category.id)}
            className={`relative p-6 rounded-xl border-2 transition-all text-left ${
              selectedVertical === category.id
                ? 'border-blue-600 bg-blue-50'
                : 'border-gray-200 hover:border-gray-300'
            }`}
          >
            <div className="text-4xl mb-4">{category.icon}</div>
            <h3 className="text-lg font-semibold text-gray-900 mb-1">
              {category.title}
            </h3>
            <p className="text-sm text-gray-500">
              {category.description}
            </p>
            {selectedVertical === category.id && (
              <div className="absolute top-4 right-4">
                <CheckCircleIcon className="h-6 w-6 text-blue-600" />
              </div>
            )}
          </motion.button>
        ))}
      </div>
    </div>
  );
};

// Vehicle Details Step
const VehicleDetailsStep: React.FC<{
  data: VehicleFormData;
  onChange: (data: Partial<VehicleFormData>) => void;
}> = ({ data, onChange }) => {
  const makes = ['Toyota', 'Honda', 'Ford', 'Chevrolet', 'BMW', 'Mercedes-Benz', 'Audi', 'Volkswagen', 'Nissan', 'Hyundai', 'Kia', 'Mazda'];
  const bodyTypes: { value: VehicleBodyType; label: string }[] = [
    { value: 'sedan', label: 'Sedán' },
    { value: 'suv', label: 'SUV' },
    { value: 'truck', label: 'Pickup' },
    { value: 'coupe', label: 'Coupé' },
    { value: 'hatchback', label: 'Hatchback' },
    { value: 'van', label: 'Van' },
    { value: 'convertible', label: 'Convertible' },
    { value: 'wagon', label: 'Wagon' },
  ];
  const fuelTypes: { value: VehicleFuelType; label: string }[] = [
    { value: 'gasoline', label: 'Gasolina' },
    { value: 'diesel', label: 'Diesel' },
    { value: 'electric', label: 'Eléctrico' },
    { value: 'hybrid', label: 'Híbrido' },
  ];
  const transmissions: { value: VehicleTransmission; label: string }[] = [
    { value: 'automatic', label: 'Automática' },
    { value: 'manual', label: 'Manual' },
    { value: 'cvt', label: 'CVT' },
  ];
  const conditions: { value: VehicleCondition; label: string }[] = [
    { value: 'new', label: 'Nuevo' },
    { value: 'used', label: 'Usado' },
    { value: 'certified-pre-owned', label: 'Semi-nuevo certificado' },
  ];

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Detalles del vehículo
      </h2>

      {/* Title */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Título de la publicación *
        </label>
        <input
          type="text"
          value={data.title}
          onChange={(e) => onChange({ title: e.target.value })}
          placeholder="Ej: 2022 Toyota RAV4 XLE Premium"
          className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        {/* Make */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Marca *
          </label>
          <select
            value={data.make}
            onChange={(e) => onChange({ make: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            <option value="">Seleccionar...</option>
            {makes.map((make) => (
              <option key={make} value={make}>{make}</option>
            ))}
          </select>
        </div>

        {/* Model */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Modelo *
          </label>
          <input
            type="text"
            value={data.model}
            onChange={(e) => onChange({ model: e.target.value })}
            placeholder="Ej: RAV4"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Year */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Año *
          </label>
          <input
            type="number"
            value={data.year}
            onChange={(e) => onChange({ year: parseInt(e.target.value) })}
            min="1900"
            max={new Date().getFullYear() + 1}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Mileage */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Kilometraje *
          </label>
          <input
            type="number"
            value={data.mileage}
            onChange={(e) => onChange({ mileage: parseInt(e.target.value) })}
            min="0"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Body Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Tipo de carrocería
          </label>
          <select
            value={data.bodyType}
            onChange={(e) => onChange({ bodyType: e.target.value as VehicleBodyType })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {bodyTypes.map((bt) => (
              <option key={bt.value} value={bt.value}>{bt.label}</option>
            ))}
          </select>
        </div>

        {/* Transmission */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Transmisión
          </label>
          <select
            value={data.transmission}
            onChange={(e) => onChange({ transmission: e.target.value as VehicleTransmission })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {transmissions.map((t) => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
        </div>

        {/* Fuel Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Combustible
          </label>
          <select
            value={data.fuelType}
            onChange={(e) => onChange({ fuelType: e.target.value as VehicleFuelType })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {fuelTypes.map((ft) => (
              <option key={ft.value} value={ft.value}>{ft.label}</option>
            ))}
          </select>
        </div>

        {/* Condition */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Condición
          </label>
          <select
            value={data.condition}
            onChange={(e) => onChange({ condition: e.target.value as VehicleCondition })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {conditions.map((c) => (
              <option key={c.value} value={c.value}>{c.label}</option>
            ))}
          </select>
        </div>

        {/* Exterior Color */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Color exterior
          </label>
          <input
            type="text"
            value={data.exteriorColor}
            onChange={(e) => onChange({ exteriorColor: e.target.value })}
            placeholder="Ej: Blanco perla"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Interior Color */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Color interior
          </label>
          <input
            type="text"
            value={data.interiorColor}
            onChange={(e) => onChange({ interiorColor: e.target.value })}
            placeholder="Ej: Negro"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Description */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Descripción
        </label>
        <textarea
          value={data.description}
          onChange={(e) => onChange({ description: e.target.value })}
          rows={4}
          placeholder="Describe las características, estado y cualquier detalle importante..."
          className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );
};

// Property Details Step (simplified)
const PropertyDetailsStep: React.FC<{
  data: PropertyFormData;
  onChange: (data: Partial<PropertyFormData>) => void;
}> = ({ data, onChange }) => {
  const propertyTypes: { value: PropertyType; label: string }[] = [
    { value: 'house', label: 'Casa' },
    { value: 'apartment', label: 'Apartamento' },
    { value: 'condo', label: 'Condominio' },
    { value: 'townhouse', label: 'Casa adosada' },
    { value: 'land', label: 'Terreno' },
    { value: 'commercial', label: 'Local comercial' },
    { value: 'office', label: 'Oficina' },
  ];
  const listingTypes: { value: PropertyListingType; label: string }[] = [
    { value: 'sale', label: 'Venta' },
    { value: 'rent', label: 'Renta' },
    { value: 'sale-or-rent', label: 'Venta o Renta' },
  ];

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Detalles de la propiedad
      </h2>

      {/* Title */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Título de la publicación *
        </label>
        <input
          type="text"
          value={data.title}
          onChange={(e) => onChange({ title: e.target.value })}
          placeholder="Ej: Casa moderna en zona residencial"
          className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        {/* Property Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Tipo de propiedad *
          </label>
          <select
            value={data.propertyType}
            onChange={(e) => onChange({ propertyType: e.target.value as PropertyType })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {propertyTypes.map((pt) => (
              <option key={pt.value} value={pt.value}>{pt.label}</option>
            ))}
          </select>
        </div>

        {/* Listing Type */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Tipo de listado *
          </label>
          <select
            value={data.listingType}
            onChange={(e) => onChange({ listingType: e.target.value as PropertyListingType })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            {listingTypes.map((lt) => (
              <option key={lt.value} value={lt.value}>{lt.label}</option>
            ))}
          </select>
        </div>

        {/* Bedrooms */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Recámaras
          </label>
          <input
            type="number"
            value={data.bedrooms}
            onChange={(e) => onChange({ bedrooms: parseInt(e.target.value) })}
            min="0"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Bathrooms */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Baños
          </label>
          <input
            type="number"
            value={data.bathrooms}
            onChange={(e) => onChange({ bathrooms: parseInt(e.target.value) })}
            min="0"
            step="0.5"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Total Area */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Área total (m²)
          </label>
          <input
            type="number"
            value={data.totalArea}
            onChange={(e) => onChange({ totalArea: parseFloat(e.target.value) })}
            min="0"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {/* Parking */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Estacionamientos
          </label>
          <input
            type="number"
            value={data.parkingSpaces}
            onChange={(e) => onChange({ parkingSpaces: parseInt(e.target.value) })}
            min="0"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Amenities */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-3">
          Amenidades
        </label>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {[
            { key: 'hasGarden', label: 'Jardín' },
            { key: 'hasPool', label: 'Piscina' },
            { key: 'hasGym', label: 'Gimnasio' },
            { key: 'hasElevator', label: 'Elevador' },
            { key: 'hasSecurity', label: 'Seguridad' },
            { key: 'isFurnished', label: 'Amueblado' },
            { key: 'allowsPets', label: 'Mascotas' },
          ].map((amenity) => (
            <label key={amenity.key} className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={data[amenity.key as keyof PropertyFormData] as boolean}
                onChange={(e) => onChange({ [amenity.key]: e.target.checked })}
                className="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">{amenity.label}</span>
            </label>
          ))}
        </div>
      </div>

      {/* Description */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Descripción
        </label>
        <textarea
          value={data.description}
          onChange={(e) => onChange({ description: e.target.value })}
          rows={4}
          placeholder="Describe las características de la propiedad..."
          className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );
};

// Images Step
const ImagesStep: React.FC<{
  previews: string[];
  onUpload: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onRemove: (index: number) => void;
}> = ({ previews, onUpload, onRemove }) => {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Imágenes
      </h2>
      <p className="text-gray-500">
        Sube al menos una imagen. La primera será la imagen principal.
      </p>

      {/* Upload area */}
      <label className="flex flex-col items-center justify-center w-full h-40 border-2 border-dashed border-gray-300 rounded-xl cursor-pointer hover:border-blue-500 transition-colors">
        <PhotoIcon className="h-12 w-12 text-gray-400 mb-2" />
        <span className="text-gray-500">
          Haz clic o arrastra imágenes aquí
        </span>
        <span className="text-sm text-gray-400">PNG, JPG hasta 10MB</span>
        <input
          type="file"
          multiple
          accept="image/*"
          onChange={onUpload}
          className="hidden"
        />
      </label>

      {/* Preview grid */}
      {previews.length > 0 && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {previews.map((preview, index) => (
            <div key={index} className="relative group">
              <img
                src={preview}
                alt={`Preview ${index + 1}`}
                className="w-full aspect-square object-cover rounded-lg"
              />
              {index === 0 && (
                <span className="absolute top-2 left-2 px-2 py-1 bg-blue-600 text-white text-xs font-medium rounded">
                  Principal
                </span>
              )}
              <button
                onClick={() => onRemove(index)}
                className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
              >
                <XMarkIcon className="h-4 w-4" />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

// Vehicle Pricing Step
const VehiclePricingStep: React.FC<{
  data: VehicleFormData;
  onChange: (data: Partial<VehicleFormData>) => void;
}> = ({ data, onChange }) => {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Precio y ubicación
      </h2>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Precio *
          </label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500">$</span>
            <input
              type="number"
              value={data.price}
              onChange={(e) => onChange({ price: parseInt(e.target.value) })}
              min="0"
              className="w-full pl-8 pr-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Moneda
          </label>
          <select
            value={data.currency}
            onChange={(e) => onChange({ currency: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            <option value="MXN">MXN (Peso mexicano)</option>
            <option value="USD">USD (Dólar)</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Ciudad *
          </label>
          <input
            type="text"
            value={data.city}
            onChange={(e) => onChange({ city: e.target.value })}
            placeholder="Ciudad de México"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Estado *
          </label>
          <input
            type="text"
            value={data.state}
            onChange={(e) => onChange({ state: e.target.value })}
            placeholder="CDMX"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>
    </div>
  );
};

// Property Pricing Step
const PropertyPricingStep: React.FC<{
  data: PropertyFormData;
  onChange: (data: Partial<PropertyFormData>) => void;
}> = ({ data, onChange }) => {
  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Precio y ubicación
      </h2>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Precio {data.listingType === 'rent' ? 'mensual' : ''} *
          </label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500">$</span>
            <input
              type="number"
              value={data.price}
              onChange={(e) => onChange({ price: parseInt(e.target.value) })}
              min="0"
              className="w-full pl-8 pr-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Moneda
          </label>
          <select
            value={data.currency}
            onChange={(e) => onChange({ currency: e.target.value })}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          >
            <option value="MXN">MXN (Peso mexicano)</option>
            <option value="USD">USD (Dólar)</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Cuota de mantenimiento
          </label>
          <input
            type="text"
            value={data.maintenanceFee}
            onChange={(e) => onChange({ maintenanceFee: e.target.value })}
            placeholder="Opcional"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="col-span-2">
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Dirección
          </label>
          <input
            type="text"
            value={data.address}
            onChange={(e) => onChange({ address: e.target.value })}
            placeholder="Calle y número (opcional)"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Ciudad *
          </label>
          <input
            type="text"
            value={data.city}
            onChange={(e) => onChange({ city: e.target.value })}
            placeholder="Ciudad de México"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Estado *
          </label>
          <input
            type="text"
            value={data.state}
            onChange={(e) => onChange({ state: e.target.value })}
            placeholder="CDMX"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>
    </div>
  );
};

// Review Step
const ReviewStep: React.FC<{
  vertical: MarketplaceVertical;
  vehicleData: VehicleFormData;
  propertyData: PropertyFormData;
  images: string[];
}> = ({ vertical, vehicleData, propertyData, images }) => {
  const data = vertical === 'vehicles' ? vehicleData : propertyData;

  return (
    <div className="space-y-6">
      <h2 className="text-lg font-semibold text-gray-900">
        Revisa tu publicación
      </h2>

      {/* Images preview */}
      {images.length > 0 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.slice(0, 4).map((img, i) => (
            <img key={i} src={img} alt="" className="w-20 h-20 object-cover rounded-lg flex-shrink-0" />
          ))}
          {images.length > 4 && (
            <div className="w-20 h-20 bg-gray-100 rounded-lg flex items-center justify-center flex-shrink-0">
              <span className="text-gray-500">+{images.length - 4}</span>
            </div>
          )}
        </div>
      )}

      {/* Summary */}
      <div className="bg-gray-50 rounded-lg p-4 space-y-3">
        <div className="flex justify-between">
          <span className="text-gray-500">Título</span>
          <span className="font-medium text-gray-900">{data.title || 'Sin título'}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-gray-500">Precio</span>
          <span className="font-bold text-blue-600">
            {new Intl.NumberFormat('es-MX', {
              style: 'currency',
              currency: data.currency,
              maximumFractionDigits: 0,
            }).format(data.price)}
          </span>
        </div>
        <div className="flex justify-between">
          <span className="text-gray-500">Ubicación</span>
          <span className="text-gray-900">{data.city}, {data.state}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-gray-500">Imágenes</span>
          <span className="text-gray-900">{images.length}</span>
        </div>
      </div>

      <div className="p-4 bg-blue-50 rounded-lg text-blue-700 text-sm">
        <p>
          Tu publicación será revisada antes de aparecer en el marketplace. 
          Esto usualmente toma menos de 24 horas.
        </p>
      </div>
    </div>
  );
};

export default ListingFormPage;
