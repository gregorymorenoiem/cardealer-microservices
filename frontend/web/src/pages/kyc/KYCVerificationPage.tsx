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

// Steps for the KYC wizard
const STEPS = [
  { id: 'personal', label: 'Información Personal', icon: FiUser },
  { id: 'address', label: 'Dirección', icon: FiHome },
  { id: 'documents', label: 'Documentos', icon: FiFileText },
  { id: 'selfie', label: 'Verificación', icon: FiCamera },
  { id: 'review', label: 'Revisión', icon: FiShield },
];

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
        console.error('Error fetching KYC profile:', err);
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

  // Check if all required documents are uploaded
  const requiredDocs = kycService.getRequiredDocuments(
    user?.accountType === 'dealer' ? 'dealer' : 'seller'
  );
  const allDocsUploaded = requiredDocs.every((type) => uploadedDocs[type]);

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

          {/* Step 3: Documents */}
          {currentStep === 2 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold flex items-center gap-2">
                <FiFileText className="text-primary" />
                Documentos Requeridos
              </h2>

              <div className="space-y-4">
                {requiredDocs.map((docType) => (
                  <DocumentUploadCard
                    key={docType}
                    documentType={docType}
                    document={uploadedDocs[docType]}
                    isUploading={uploadingDoc === docType}
                    onUpload={(file) => handleDocumentUpload(docType, file)}
                    label={kycService.getDocumentTypeLabel(docType)}
                  />
                ))}
              </div>

              <div className="flex justify-between">
                <Button variant="outline" onClick={() => setCurrentStep(1)}>
                  Atrás
                </Button>
                <Button
                  variant="primary"
                  onClick={() => setCurrentStep(3)}
                  disabled={!allDocsUploaded}
                >
                  Continuar <FiChevronRight className="ml-2" />
                </Button>
              </div>
            </div>
          )}

          {/* Step 4: Selfie Verification */}
          {currentStep === 3 && (
            <div className="space-y-6">
              <h2 className="text-xl font-semibold flex items-center gap-2">
                <FiCamera className="text-primary" />
                Verificación con Selfie
              </h2>

              <div className="text-center py-8">
                <DocumentUploadCard
                  documentType={DocumentType.SelfieWithDocument}
                  document={uploadedDocs[DocumentType.SelfieWithDocument]}
                  isUploading={uploadingDoc === DocumentType.SelfieWithDocument}
                  onUpload={(file) => handleDocumentUpload(DocumentType.SelfieWithDocument, file)}
                  label="Selfie sosteniendo tu documento de identidad"
                  description="Toma una foto clara de tu rostro sosteniendo tu cédula o pasaporte"
                />
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h4 className="font-medium text-blue-900 mb-2">Consejos para una buena foto:</h4>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>• Buena iluminación (evita contraluz)</li>
                  <li>• Documento claramente visible</li>
                  <li>• Tu rostro debe ser visible completo</li>
                  <li>• No uses filtros ni edites la foto</li>
                </ul>
              </div>

              <div className="flex justify-between">
                <Button variant="outline" onClick={() => setCurrentStep(2)}>
                  Atrás
                </Button>
                <Button
                  variant="primary"
                  onClick={() => setCurrentStep(4)}
                  disabled={!uploadedDocs[DocumentType.SelfieWithDocument]}
                >
                  Enviar para Revisión <FiChevronRight className="ml-2" />
                </Button>
              </div>
            </div>
          )}

          {/* Step 5: Review */}
          {currentStep === 4 && (
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
