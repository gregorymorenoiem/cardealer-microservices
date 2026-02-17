import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import {
  FiUser,
  FiFileText,
  FiUpload,
  FiCheck,
  FiAlertCircle,
  FiCamera,
  FiHome,
  FiShield,
  FiChevronRight,
  FiX,
  FiLoader,
} from 'react-icons/fi';
import { kycService, KYCStatus, DocumentType } from '@/services/kycService';
import type { KYCProfile, CreateKYCProfileRequest, KYCDocument } from '@/services/kycService';
import { userProfileService } from '@/services/userProfileService';
import { DocumentCapture } from '@/components/kyc/DocumentCapture';
import type { DocumentSide } from '@/components/kyc/DocumentCapture';
import { LivenessChallenge } from '@/components/kyc/LivenessChallenge';
import type { LivenessData } from '@/services/identityVerificationService';

// Steps for the KYC wizard (Simplified)
const STEPS = [
  { id: 'personal', label: 'Información Personal', icon: FiUser },
  { id: 'address', label: 'Dirección', icon: FiHome },
  { id: 'verification', label: 'Verificación', icon: FiCamera },
  { id: 'review', label: 'Revisión', icon: FiShield },
];

// Threshold for requiring Utility Bill (according to Ley 155-17 de Prevención de Lavado)
const HIGH_TRANSACTION_THRESHOLD = 50000; // DOP 50,000

// Dominican provinces
const PROVINCES = [
  'Distrito Nacional',
  'Santo Domingo',
  'Santiago',
  'La Vega',
  'San Cristóbal',
  'La Altagracia',
  'Puerto Plata',
  'Duarte',
  'San Pedro de Macorís',
  'La Romana',
  'Espaillat',
  'Azua',
  'Peravia',
  'Valverde',
  'Monseñor Nouel',
];

export default function KYCVerificationPage() {
  // Translation hook available for future use
  useTranslation('kyc');
  const navigate = useNavigate();
  const { user } = useAuth();

  const [currentStep, setCurrentStep] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [profile, setProfile] = useState<KYCProfile | null>(null);

  // Form data
  const [formData, setFormData] = useState<CreateKYCProfileRequest>({
    userId: '',
    firstName: '',
    lastName: '',
    documentNumber: '',
    documentType: DocumentType.Cedula,
    dateOfBirth: '',
    nationality: 'Dominicana',
    address: '',
    city: '',
    province: 'Distrito Nacional',
    phoneNumber: '',
    sourceOfFunds: '',
    occupation: '',
    expectedMonthlyTransaction: undefined,
  });

  // Documents
  const [uploadedDocs, setUploadedDocs] = useState<Record<DocumentType, KYCDocument | null>>(
    {} as any
  );
  const [uploadingDoc, setUploadingDoc] = useState<DocumentType | null>(null);

  // Camera capture states
  const [isCapturingCamera, setIsCapturingCamera] = useState(false);
  const [currentDocumentType, setCurrentDocumentType] = useState<DocumentType | null>(null);
  const [currentSide, setCurrentSide] = useState<'Front' | 'Back' | null>(null);
  const [capturedImages, setCapturedImages] = useState<
    Record<string, { front?: string; back?: string }>
  >({}); // documentType -> sides

  // Liveness challenge states
  const [showLivenessChallenge, setShowLivenessChallenge] = useState(false);
  const [livenessData, setLivenessData] = useState<LivenessData | null>(null);
  const [selfieBlob, setSelfieBlob] = useState<Blob | null>(null);

  // Log liveness data whenever it changes (for debugging/future facial validation)
  useEffect(() => {
    if (livenessData) {
      console.log('✅ Liveness verification completed:', {
        challengesCompleted: livenessData.challenges?.length || 0,
        videoFrames: livenessData.videoFrames?.length || 0,
        hasGyroscope: !!livenessData.deviceGyroscope,
      });
    }
  }, [livenessData]);

  // Pre-fill form with user data when available (from UserService)
  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!user?.id || profile) return;

      try {
        // Fetch complete user profile from UserService
        const userProfile = await userProfileService.getUserProfile(user.id);

        let firstName = '';
        let lastName = '';
        let phoneNumber = '';

        if (userProfile) {
          // Use data from UserService
          firstName = userProfile.firstName || '';
          lastName = userProfile.lastName || '';
          phoneNumber = userProfile.phoneNumber || '';
        } else {
          // Fallback to auth user data
          firstName = user.firstName || '';
          lastName = user.lastName || '';
          phoneNumber = user.phone || '';

          // If firstName/lastName not available but name is, split it
          if (!firstName && !lastName && user.name) {
            const nameParts = user.name.trim().split(' ');
            firstName = nameParts[0] || '';
            lastName = nameParts.slice(1).join(' ') || '';
          }
        }

        setFormData((prev) => ({
          ...prev,
          userId: user.id,
          firstName: prev.firstName || firstName,
          lastName: prev.lastName || lastName,
          phoneNumber: prev.phoneNumber || phoneNumber,
        }));
      } catch {
        // Silently use auth user data as fallback
        setFormData((prev) => ({
          ...prev,
          userId: user.id,
        }));
      }
    };

    fetchUserProfile();
  }, [user, profile]);

  // Check if user has existing profile
  useEffect(() => {
    const fetchProfile = async () => {
      if (!user?.id) return;

      try {
        setIsLoading(true);
        const existingProfile = await kycService.getProfileByUserId(user.id);

        if (existingProfile) {
          setProfile(existingProfile);
          setFormData({
            userId: existingProfile.userId,
            firstName: existingProfile.firstName,
            lastName: existingProfile.lastName,
            documentNumber: existingProfile.documentNumber,
            documentType: existingProfile.documentType,
            dateOfBirth: existingProfile.dateOfBirth.split('T')[0],
            nationality: existingProfile.nationality,
            address: existingProfile.address,
            city: existingProfile.city,
            province: existingProfile.province,
            phoneNumber: existingProfile.phoneNumber,
            sourceOfFunds: existingProfile.sourceOfFunds,
            occupation: existingProfile.occupation,
            expectedMonthlyTransaction: existingProfile.expectedMonthlyTransaction,
          });

          // Map documents
          const docs: Record<DocumentType, KYCDocument | null> = {} as any;
          existingProfile.documents?.forEach((doc) => {
            docs[doc.documentType] = doc;
          });
          setUploadedDocs(docs);

          // Navigate based on status
          if (existingProfile.status === KYCStatus.Approved) {
            navigate('/kyc/status');
            return;
          } else if (
            existingProfile.status === KYCStatus.PendingReview ||
            existingProfile.status === KYCStatus.UnderReview
          ) {
            setCurrentStep(4); // Go to review step
          } else if (existingProfile.documents?.length > 0) {
            setCurrentStep(2); // Go to documents step
          }
        }
      } catch (err) {
        // Error already logged in kycService - no need to duplicate
        // This catch handles unexpected errors only
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [user?.id, navigate]);

  // Handle input change
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  // Save personal info
  const handleSavePersonalInfo = async () => {
    if (!user?.id) return;

    try {
      setIsSaving(true);
      setError(null);

      const data = { ...formData, userId: user.id };

      if (profile) {
        // Update existing profile
        const updated = await kycService.updateProfile(profile.id, data);
        setProfile(updated);
      } else {
        // Create new profile
        const created = await kycService.createProfile(data);
        setProfile(created);
      }

      setCurrentStep(1);
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Error al guardar la información');
      }
    } finally {
      setIsSaving(false);
    }
  };

  // Save address info
  const handleSaveAddressInfo = async () => {
    if (!profile) return;

    try {
      setIsSaving(true);
      setError(null);

      await kycService.updateProfile(profile.id, {
        address: formData.address,
        city: formData.city,
        province: formData.province,
      });

      setCurrentStep(2);
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Error al guardar la dirección');
      }
    } finally {
      setIsSaving(false);
    }
  };

  // Handle document upload
  const handleDocumentUpload = async (type: DocumentType, file: File, side?: 'Front' | 'Back') => {
    if (!profile) return;

    try {
      setUploadingDoc(type);
      setError(null);

      const doc = await kycService.uploadDocument({
        profileId: profile.id,
        documentType: type,
        file,
        side,
      });

      setUploadedDocs((prev) => ({ ...prev, [type]: doc }));
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Error al subir el documento');
      }
    } finally {
      setUploadingDoc(null);
    }
  };

  // Handle submit for review - called when user clicks "Enviar para Revisión"
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmitForReview = async () => {
    if (!profile) return;

    try {
      setIsSubmitting(true);
      setError(null);

      // Submit profile for review
      const updatedProfile = await kycService.submitForReview(profile.id);
      setProfile(updatedProfile);

      // Move to review step
      setCurrentStep(3);
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Error al enviar la verificación');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // Check if all required documents are uploaded
  const requiredDocs = kycService.getRequiredDocuments(
    user?.accountType === 'dealer' ? 'dealer' : 'seller'
  );

  if (isLoading) {
    return (
      <MainLayout>
        <div className="max-w-4xl mx-auto px-4 py-12">
          <div className="flex items-center justify-center">
            <FiLoader className="animate-spin text-primary" size={40} />
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900 flex items-center justify-center gap-3">
            <FiShield className="text-primary" />
            Verificación de Identidad (KYC)
          </h1>
          <p className="text-gray-600 mt-2">
            Completa tu verificación para acceder a todas las funcionalidades
          </p>
        </div>

        {/* Progress Steps */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            {STEPS.map((step, index) => (
              <div
                key={step.id}
                className={`flex items-center ${index < STEPS.length - 1 ? 'flex-1' : ''}`}
              >
                <div
                  className={`flex items-center justify-center w-10 h-10 rounded-full border-2 
                    ${
                      index < currentStep
                        ? 'bg-green-500 border-green-500 text-white'
                        : index === currentStep
                          ? 'bg-primary border-primary text-white'
                          : 'bg-white border-gray-300 text-gray-500'
                    }`}
                >
                  {index < currentStep ? <FiCheck size={20} /> : <step.icon size={20} />}
                </div>
                {index < STEPS.length - 1 && (
                  <div
                    className={`flex-1 h-1 mx-2 ${
                      index < currentStep ? 'bg-green-500' : 'bg-gray-200'
                    }`}
                  />
                )}
              </div>
            ))}
          </div>
          <div className="flex justify-between text-sm text-gray-600">
            {STEPS.map((step) => (
              <span key={step.id} className="text-center w-20">
                {step.label}
              </span>
            ))}
          </div>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-start gap-3">
            <FiAlertCircle className="text-red-600 flex-shrink-0 mt-0.5" size={20} />
            <div className="flex-1">
              <p className="text-sm text-red-800">{error}</p>
            </div>
            <button onClick={() => setError(null)}>
              <FiX className="text-red-600" />
            </button>
          </div>
        )}

        {/* Step Content */}
        <div className="bg-white rounded-xl shadow-sm border p-6">
          {/* Step 1: Personal Info */}
          {currentStep === 0 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold flex items-center gap-2">
                <FiUser className="text-primary" />
                Información Personal
              </h2>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  name="firstName"
                  label="Nombre"
                  placeholder="Tu nombre"
                  value={formData.firstName}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
                <Input
                  name="lastName"
                  label="Apellido"
                  placeholder="Tu apellido"
                  value={formData.lastName}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tipo de Documento
                  </label>
                  <select
                    name="documentType"
                    value={formData.documentType}
                    onChange={handleInputChange}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary"
                  >
                    <option value={DocumentType.Cedula}>Cédula</option>
                    <option value={DocumentType.Passport}>Pasaporte</option>
                  </select>
                </div>
                <Input
                  name="documentNumber"
                  label="Número de Documento"
                  placeholder="000-0000000-0"
                  value={formData.documentNumber}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  name="dateOfBirth"
                  label="Fecha de Nacimiento"
                  type="date"
                  value={formData.dateOfBirth}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
                <Input
                  name="phoneNumber"
                  label="Teléfono"
                  placeholder="+1 809-555-1234"
                  value={formData.phoneNumber}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  name="occupation"
                  label="Ocupación"
                  placeholder="Ej: Ingeniero, Médico, etc."
                  value={formData.occupation}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Fuente de Ingresos
                  </label>
                  <select
                    name="sourceOfFunds"
                    value={formData.sourceOfFunds}
                    onChange={handleInputChange}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary"
                  >
                    <option value="">Seleccionar...</option>
                    <option value="Salary">Salario</option>
                    <option value="Business">Negocio Propio</option>
                    <option value="Investments">Inversiones</option>
                    <option value="Inheritance">Herencia</option>
                    <option value="Savings">Ahorros</option>
                    <option value="Other">Otro</option>
                  </select>
                </div>
              </div>

              <div className="flex justify-end">
                <Button
                  variant="primary"
                  onClick={handleSavePersonalInfo}
                  isLoading={isSaving}
                  disabled={!formData.firstName || !formData.lastName || !formData.documentNumber}
                >
                  Continuar <FiChevronRight className="ml-2" />
                </Button>
              </div>
            </div>
          )}

          {/* Step 2: Address */}
          {currentStep === 1 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold flex items-center gap-2">
                <FiHome className="text-primary" />
                Dirección
              </h2>

              <Input
                name="address"
                label="Dirección"
                placeholder="Calle, número, sector"
                value={formData.address}
                onChange={handleInputChange}
                required
                fullWidth
              />

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  name="city"
                  label="Ciudad"
                  placeholder="Tu ciudad"
                  value={formData.city}
                  onChange={handleInputChange}
                  required
                  fullWidth
                />
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Provincia</label>
                  <select
                    name="province"
                    value={formData.province}
                    onChange={handleInputChange}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary/20 focus:border-primary"
                  >
                    {PROVINCES.map((p) => (
                      <option key={p} value={p}>
                        {p}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="flex justify-between">
                <Button variant="outline" onClick={() => setCurrentStep(0)}>
                  Atrás
                </Button>
                <Button
                  variant="primary"
                  onClick={handleSaveAddressInfo}
                  isLoading={isSaving}
                  disabled={!formData.address || !formData.city}
                >
                  Continuar <FiChevronRight className="ml-2" />
                </Button>
              </div>
            </div>
          )}

          {/* Step 3: Verification (Cedula + Selfie + Optional Utility Bill) */}
          {currentStep === 2 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold flex items-center gap-2">
                <FiCamera className="text-primary" />
                Verificación de Identidad
              </h2>

              <p className="text-gray-600">
                Captura ambos lados de tu cédula con la cámara y completa la verificación facial.
                {formData.expectedMonthlyTransaction &&
                  formData.expectedMonthlyTransaction >= HIGH_TRANSACTION_THRESHOLD && (
                    <span className="block mt-2 text-sm text-blue-600">
                      ⚠️ Según la Ley 155-17, se requiere factura eléctrica para transacciones
                      mayores a RD${HIGH_TRANSACTION_THRESHOLD.toLocaleString()}
                    </span>
                  )}
              </p>

              {!isCapturingCamera ? (
                <div className="space-y-4">
                  {/* For Cedula - capture both sides */}
                  {requiredDocs.includes(DocumentType.Cedula) && (
                    <div className="bg-white border border-gray-200 rounded-lg p-4">
                      <h3 className="font-medium text-gray-900 mb-2">
                        {kycService.getDocumentTypeLabel(DocumentType.Cedula)}
                      </h3>
                      <p className="text-sm text-gray-600 mb-4">
                        Captura ambos lados de tu cédula con la cámara
                      </p>

                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {/* Front side */}
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Lado Frontal
                          </label>
                          {capturedImages[DocumentType.Cedula]?.front ? (
                            <div className="relative">
                              <img
                                src={capturedImages[DocumentType.Cedula].front}
                                alt="Cédula frontal"
                                className="w-full h-48 object-cover rounded-lg"
                              />
                              <button
                                onClick={() => {
                                  const updated = { ...capturedImages };
                                  if (updated[DocumentType.Cedula]) {
                                    delete updated[DocumentType.Cedula].front;
                                  }
                                  setCapturedImages(updated);
                                }}
                                className="absolute top-2 right-2 p-2 bg-red-500 text-white rounded-full hover:bg-red-600"
                              >
                                <FiX />
                              </button>
                              {uploadedDocs[DocumentType.Cedula] && (
                                <div className="absolute top-2 left-2 p-2 bg-green-500 text-white rounded-full">
                                  <FiCheck />
                                </div>
                              )}
                            </div>
                          ) : (
                            <Button
                              variant="outline"
                              onClick={() => {
                                setCurrentDocumentType(DocumentType.Cedula);
                                setCurrentSide('Front');
                                setIsCapturingCamera(true);
                              }}
                              className="w-full"
                            >
                              <FiCamera className="mr-2" /> Capturar Frontal
                            </Button>
                          )}
                        </div>

                        {/* Back side */}
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Lado Posterior
                          </label>
                          {capturedImages[DocumentType.Cedula]?.back ? (
                            <div className="relative">
                              <img
                                src={capturedImages[DocumentType.Cedula].back}
                                alt="Cédula posterior"
                                className="w-full h-48 object-cover rounded-lg"
                              />
                              <button
                                onClick={() => {
                                  const updated = { ...capturedImages };
                                  if (updated[DocumentType.Cedula]) {
                                    delete updated[DocumentType.Cedula].back;
                                  }
                                  setCapturedImages(updated);
                                }}
                                className="absolute top-2 right-2 p-2 bg-red-500 text-white rounded-full hover:bg-red-600"
                              >
                                <FiX />
                              </button>
                            </div>
                          ) : (
                            <Button
                              variant="outline"
                              onClick={() => {
                                setCurrentDocumentType(DocumentType.Cedula);
                                setCurrentSide('Back');
                                setIsCapturingCamera(true);
                              }}
                              className="w-full"
                              disabled={!capturedImages[DocumentType.Cedula]?.front}
                            >
                              <FiCamera className="mr-2" /> Capturar Posterior
                            </Button>
                          )}
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Utility Bill - Required for high transactions (Ley 155-17) */}
                  {formData.expectedMonthlyTransaction &&
                    formData.expectedMonthlyTransaction >= HIGH_TRANSACTION_THRESHOLD && (
                      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                        <div className="flex items-start gap-3 mb-3">
                          <FiAlertCircle className="text-blue-600 flex-shrink-0 mt-1" size={20} />
                          <div>
                            <h3 className="font-medium text-blue-900">
                              Factura Eléctrica Requerida
                            </h3>
                            <p className="text-sm text-blue-700 mt-1">
                              Según la Ley 155-17 de Prevención de Lavado de Activos, se requiere
                              comprobante de domicilio para transacciones superiores a RD$50,000.
                            </p>
                          </div>
                        </div>
                        <DocumentUploadCard
                          documentType={DocumentType.UtilityBill}
                          document={uploadedDocs[DocumentType.UtilityBill]}
                          isUploading={uploadingDoc === DocumentType.UtilityBill}
                          onUpload={(file) => handleDocumentUpload(DocumentType.UtilityBill, file)}
                          label="Factura Eléctrica o Servicio"
                          description="Factura reciente (últimos 3 meses) a tu nombre"
                        />
                      </div>
                    )}
                </div>
              ) : (
                <div>
                  <DocumentCapture
                    side={currentSide as DocumentSide}
                    documentType={
                      currentDocumentType === DocumentType.Cedula ? 'Cedula' : undefined
                    }
                    onCapture={async (file: File, side: DocumentSide) => {
                      // Store captured image as data URL for preview
                      const reader = new FileReader();
                      reader.onload = (e) => {
                        const imageDataUrl = e.target?.result as string;
                        setCapturedImages((prev) => ({
                          ...prev,
                          [currentDocumentType!]: {
                            ...prev[currentDocumentType!],
                            [side.toLowerCase()]: imageDataUrl,
                          },
                        }));
                      };
                      reader.readAsDataURL(file);

                      // Upload the file
                      try {
                        await handleDocumentUpload(currentDocumentType!, file, side);
                      } catch (err) {
                        console.error('Error uploading document:', err);
                        setError('Error al subir el documento');
                      }

                      setIsCapturingCamera(false);
                      setCurrentDocumentType(null);
                      setCurrentSide(null);
                    }}
                    onError={(errorMsg: string) => {
                      setError(errorMsg);
                    }}
                    isProcessing={uploadingDoc === currentDocumentType}
                    capturedImage={null}
                  />
                  <div className="mt-4">
                    <Button
                      variant="outline"
                      onClick={() => {
                        setIsCapturingCamera(false);
                        setCurrentDocumentType(null);
                        setCurrentSide(null);
                      }}
                    >
                      Cancelar
                    </Button>
                  </div>
                </div>
              )}

              {/* Only show tips and verification button if selfie is not yet taken */}
              {!isCapturingCamera && !selfieBlob && (
                <>
                  <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <h4 className="font-medium text-blue-900 mb-2">Consejos para captura:</h4>
                    <ul className="text-sm text-blue-800 space-y-1">
                      <li>• Usa buena iluminación natural</li>
                      <li>• Coloca el documento sobre una superficie plana</li>
                      <li>• Asegúrate de que todo el documento sea visible</li>
                      <li>• Evita reflejos y sombras</li>
                    </ul>
                  </div>

                  <div className="flex justify-between">
                    <Button variant="outline" onClick={() => setCurrentStep(1)}>
                      Atrás
                    </Button>
                    <Button
                      variant="primary"
                      onClick={() => {
                        // Check if utility bill is required and uploaded
                        const requiresUtilityBill =
                          formData.expectedMonthlyTransaction &&
                          formData.expectedMonthlyTransaction >= HIGH_TRANSACTION_THRESHOLD;
                        const hasUtilityBill = uploadedDocs[DocumentType.UtilityBill];

                        if (requiresUtilityBill && !hasUtilityBill) {
                          setError(
                            'Debes subir tu factura eléctrica para transacciones mayores a RD$50,000 (Ley 155-17)'
                          );
                          return;
                        }

                        setShowLivenessChallenge(true);
                      }}
                      disabled={
                        !capturedImages[DocumentType.Cedula]?.front ||
                        !capturedImages[DocumentType.Cedula]?.back
                      }
                    >
                      Continuar a Verificación Facial <FiChevronRight className="ml-2" />
                    </Button>
                  </div>
                </>
              )}
            </div>
          )}

          {/* Step 4: Selfie Verification with Liveness (shown when showLivenessChallenge is true) */}
          {showLivenessChallenge && currentStep === 2 && (
            <div className="fixed inset-0 z-50 bg-black bg-opacity-75 flex items-center justify-center p-4">
              <div className="bg-white rounded-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto p-6">
                <LivenessChallenge
                  requiredChallenges={['Blink', 'Smile', 'TurnLeft']}
                  onComplete={async (selfie: Blob, liveness: LivenessData) => {
                    setSelfieBlob(selfie);
                    setLivenessData(liveness);
                    setShowLivenessChallenge(false);

                    // Upload selfie
                    try {
                      const file = new File([selfie], 'selfie_with_document.jpg', {
                        type: 'image/jpeg',
                      });
                      await handleDocumentUpload(DocumentType.SelfieWithDocument, file);

                      // Log liveness data for future facial comparison
                      console.log('Liveness data captured:', liveness);
                      // TODO: Send liveness data to backend for facial comparison
                    } catch (err) {
                      console.error('Error uploading selfie:', err);
                      setError('Error al subir la selfie');
                    }
                  }}
                  onError={(error: string) => {
                    console.error('Liveness challenge error:', error);
                    setError(error);
                    setShowLivenessChallenge(false);
                  }}
                  isProcessing={uploadingDoc === DocumentType.SelfieWithDocument}
                />
                <div className="mt-4 text-center">
                  <Button variant="outline" onClick={() => setShowLivenessChallenge(false)}>
                    Cancelar
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* Show selfie result and navigation after liveness */}
          {currentStep === 2 && !isCapturingCamera && !showLivenessChallenge && selfieBlob && (
            <div className="mt-6 p-6 bg-green-50 border border-green-200 rounded-lg">
              <h3 className="font-medium text-green-900 mb-4 flex items-center gap-2">
                <FiCheck className="text-green-600" /> Verificación Facial Completada
              </h3>

              {/* Show upload status */}
              {uploadingDoc === DocumentType.SelfieWithDocument && (
                <div className="mb-4 flex items-center gap-2 text-blue-600">
                  <FiLoader className="animate-spin" />
                  <span>Subiendo selfie...</span>
                </div>
              )}

              {uploadedDocs[DocumentType.SelfieWithDocument] && (
                <div className="mb-4 flex items-center gap-2 text-green-600">
                  <FiCheck />
                  <span>Selfie subida correctamente</span>
                </div>
              )}

              <div className="flex justify-between items-center">
                <Button
                  variant="outline"
                  onClick={() => {
                    setShowLivenessChallenge(false);
                    setSelfieBlob(null);
                    setLivenessData(null);
                  }}
                >
                  Tomar Selfie Nuevamente
                </Button>
                <Button
                  variant="primary"
                  onClick={handleSubmitForReview}
                  disabled={
                    !selfieBlob || uploadingDoc === DocumentType.SelfieWithDocument || isSubmitting
                  }
                >
                  {isSubmitting ? (
                    <>
                      <FiLoader className="animate-spin mr-2" /> Enviando...
                    </>
                  ) : uploadingDoc === DocumentType.SelfieWithDocument ? (
                    <>
                      <FiLoader className="animate-spin mr-2" /> Subiendo...
                    </>
                  ) : (
                    <>
                      Enviar para Revisión <FiChevronRight className="ml-2" />
                    </>
                  )}
                </Button>
              </div>
            </div>
          )}

          {/* Step 4: Review */}
          {currentStep === 3 && (
            <div className="space-y-6 text-center">
              <div className="py-8">
                <div className="w-20 h-20 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FiShield className="text-yellow-600" size={40} />
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-2">¡Verificación Enviada!</h2>
                <p className="text-gray-600 max-w-md mx-auto">
                  Tu información ha sido enviada para revisión. Nuestro equipo de compliance
                  revisará tus documentos en un plazo de 24-48 horas.
                </p>
              </div>

              <div className="bg-gray-50 rounded-lg p-4 max-w-md mx-auto">
                <h4 className="font-medium text-gray-900 mb-2">Estado Actual</h4>
                <div className="flex items-center justify-center gap-2">
                  <span className="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm font-medium">
                    {profile ? kycService.getStatusLabel(profile.status) : 'Pendiente de Revisión'}
                  </span>
                </div>
              </div>

              <div className="flex justify-center gap-4">
                <Button variant="outline" onClick={() => navigate('/dashboard')}>
                  Ir al Dashboard
                </Button>
                <Button variant="primary" onClick={() => navigate('/kyc/status')}>
                  Ver Estado de Verificación
                </Button>
              </div>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
}

// Document Upload Card Component
interface DocumentUploadCardProps {
  documentType: DocumentType;
  document: KYCDocument | null;
  isUploading: boolean;
  onUpload: (file: File) => void;
  label: string;
  description?: string;
}

function DocumentUploadCard({
  documentType: _documentType,
  document,
  isUploading,
  onUpload,
  label,
  description,
}: DocumentUploadCardProps) {
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onUpload(file);
    }
  };

  return (
    <div
      className={`border-2 border-dashed rounded-lg p-4 transition-colors ${
        document ? 'border-green-300 bg-green-50' : 'border-gray-300 hover:border-primary'
      }`}
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          {document ? (
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <FiCheck className="text-green-600" size={20} />
            </div>
          ) : (
            <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
              <FiFileText className="text-gray-500" size={20} />
            </div>
          )}
          <div>
            <h4 className="font-medium text-gray-900">{label}</h4>
            {description && <p className="text-sm text-gray-500">{description}</p>}
            {document && (
              <p className="text-sm text-green-600">{document.fileName} - Subido correctamente</p>
            )}
          </div>
        </div>

        {isUploading ? (
          <div className="flex items-center gap-2 text-primary">
            <FiLoader className="animate-spin" />
            <span className="text-sm">Subiendo...</span>
          </div>
        ) : (
          <label className="cursor-pointer">
            <input
              type="file"
              accept="image/*,.pdf"
              onChange={handleFileChange}
              className="hidden"
            />
            <div className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors">
              <FiUpload size={16} />
              {document ? 'Cambiar' : 'Subir'}
            </div>
          </label>
        )}
      </div>
    </div>
  );
}
