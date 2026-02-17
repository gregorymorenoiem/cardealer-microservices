/**
 * Dealer Documents Upload Page
 *
 * Step 3 of dealer onboarding - upload required documents for Dominican Republic
 */

import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { FileText, Upload, Check, X, ArrowRight, ArrowLeft, AlertCircle, Eye } from 'lucide-react';
import { useUpdateDealerDocuments, useOnboardingProgress } from '@/hooks/useDealerOnboarding';
import {
  getRequiredDocuments,
  type UpdateDocumentsRequest,
} from '@/services/dealerOnboardingService';

interface UploadedFile {
  file: File;
  preview?: string;
  uploading?: boolean;
  error?: string;
}

type DocumentKey = keyof UpdateDocumentsRequest;

export const DealerDocumentsPage: React.FC = () => {
  const navigate = useNavigate();
  const { dealerId, status, hasOnboardingInProgress } = useOnboardingProgress();
  const updateDocumentsMutation = useUpdateDealerDocuments();

  const [uploadedFiles, setUploadedFiles] = useState<Record<DocumentKey, UploadedFile | null>>({
    rncDocument: null,
    businessLicense: null,
    identificationCard: null,
    proofOfAddress: null,
    bankCertificate: null,
  });
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const requiredDocuments = getRequiredDocuments();

  // Redirect if no onboarding in progress
  useEffect(() => {
    if (!hasOnboardingInProgress) {
      navigate('/dealer/onboarding');
    }
  }, [hasOnboardingInProgress, navigate]);

  // Redirect if not email verified
  useEffect(() => {
    if (status && !status.isEmailVerified) {
      navigate('/dealer/onboarding/verify-email');
    }
  }, [status, navigate]);

  // Redirect if documents already submitted
  useEffect(() => {
    if (status?.documentsSubmitted) {
      navigate('/dealer/onboarding/payment-setup');
    }
  }, [status, navigate]);

  const handleFileChange = (key: DocumentKey, files: FileList | null) => {
    if (!files || files.length === 0) return;

    const file = files[0];

    // Validate file type
    const validTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      setUploadedFiles((prev) => ({
        ...prev,
        [key]: {
          file,
          error: 'Formato no válido. Usa PDF, JPG o PNG.',
        },
      }));
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      setUploadedFiles((prev) => ({
        ...prev,
        [key]: {
          file,
          error: 'Archivo muy grande. Máximo 5MB.',
        },
      }));
      return;
    }

    // Create preview for images
    let preview: string | undefined;
    if (file.type.startsWith('image/')) {
      preview = URL.createObjectURL(file);
    }

    setUploadedFiles((prev) => ({
      ...prev,
      [key]: {
        file,
        preview,
      },
    }));
  };

  const handleRemoveFile = (key: DocumentKey) => {
    const file = uploadedFiles[key];
    if (file?.preview) {
      URL.revokeObjectURL(file.preview);
    }
    setUploadedFiles((prev) => ({
      ...prev,
      [key]: null,
    }));
  };

  const isFormValid = () => {
    // Check all required documents are uploaded
    return requiredDocuments
      .filter((doc) => doc.required)
      .every((doc) => uploadedFiles[doc.key] && !uploadedFiles[doc.key]?.error);
  };

  const handleSubmit = async () => {
    if (!dealerId) return;

    const documents: UpdateDocumentsRequest = {};
    Object.entries(uploadedFiles).forEach(([key, value]) => {
      if (value?.file && !value.error) {
        documents[key as DocumentKey] = value.file;
      }
    });

    try {
      await updateDocumentsMutation.mutateAsync({
        dealerId,
        documents,
      });
      navigate('/dealer/onboarding/payment-setup');
    } catch (error) {
      // Error handled by mutation
    }
  };

  const getFileIcon = (file: UploadedFile | null) => {
    if (!file) return null;
    if (file.error) return <AlertCircle className="h-5 w-5 text-red-500" />;
    if (file.file.type === 'application/pdf') {
      return <FileText className="h-5 w-5 text-red-600" />;
    }
    return null;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Progress indicator */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-2xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between text-sm text-gray-500">
            <span className="text-blue-600 font-medium">Paso 3 de 5</span>
            <span>Documentos Requeridos</span>
          </div>
          <div className="mt-2 h-2 bg-gray-200 rounded-full">
            <div className="h-2 bg-blue-600 rounded-full w-3/5" />
          </div>
        </div>
      </div>

      <div className="max-w-2xl mx-auto px-4 py-12">
        <button
          onClick={() => navigate('/dealer/onboarding')}
          className="flex items-center gap-2 text-gray-500 hover:text-gray-700 mb-8 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
          Volver
        </button>

        <div className="text-center mb-8">
          <div className="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <FileText className="h-10 w-10 text-blue-600" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Documentos Requeridos</h1>
          <p className="text-gray-600">Sube los documentos necesarios para verificar tu negocio</p>
        </div>

        {/* Documents List */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
          <div className="space-y-6">
            {requiredDocuments.map(({ key, label, required }) => {
              const file = uploadedFiles[key];
              const hasFile = file && !file.error;

              return (
                <div
                  key={key}
                  className={`border-2 rounded-lg p-4 transition-colors ${
                    hasFile
                      ? 'border-green-300 bg-green-50'
                      : file?.error
                        ? 'border-red-300 bg-red-50'
                        : 'border-dashed border-gray-300 hover:border-gray-400'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      {hasFile ? (
                        <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                          {file.preview ? (
                            <img
                              src={file.preview}
                              alt={label}
                              className="w-10 h-10 rounded-lg object-cover"
                            />
                          ) : (
                            getFileIcon(file) || <Check className="h-5 w-5 text-green-600" />
                          )}
                        </div>
                      ) : (
                        <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                          {file?.error ? (
                            <AlertCircle className="h-5 w-5 text-red-500" />
                          ) : (
                            <FileText className="h-5 w-5 text-gray-400" />
                          )}
                        </div>
                      )}
                      <div>
                        <p className="font-medium text-gray-900">
                          {label}
                          {required && <span className="text-red-500 ml-1">*</span>}
                        </p>
                        {file ? (
                          <p className={`text-sm ${file.error ? 'text-red-500' : 'text-gray-500'}`}>
                            {file.error || file.file.name}
                          </p>
                        ) : (
                          <p className="text-sm text-gray-400">PDF, JPG o PNG (máx. 5MB)</p>
                        )}
                      </div>
                    </div>

                    <div className="flex items-center gap-2">
                      {hasFile && file.preview && (
                        <button
                          onClick={() => setPreviewUrl(file.preview!)}
                          className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
                          title="Ver documento"
                        >
                          <Eye className="h-5 w-5" />
                        </button>
                      )}
                      {file && (
                        <button
                          onClick={() => handleRemoveFile(key)}
                          className="p-2 text-red-500 hover:text-red-700 hover:bg-red-50 rounded-lg"
                          title="Eliminar"
                        >
                          <X className="h-5 w-5" />
                        </button>
                      )}
                      <label className="cursor-pointer">
                        <input
                          type="file"
                          accept=".pdf,.jpg,.jpeg,.png,.webp"
                          onChange={(e) => handleFileChange(key, e.target.files)}
                          className="hidden"
                        />
                        <span
                          className={`px-4 py-2 rounded-lg font-medium inline-flex items-center gap-2 ${
                            hasFile
                              ? 'bg-green-100 text-green-700 hover:bg-green-200'
                              : 'bg-blue-100 text-blue-700 hover:bg-blue-200'
                          }`}
                        >
                          <Upload className="h-4 w-4" />
                          {hasFile ? 'Cambiar' : 'Subir'}
                        </span>
                      </label>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Info box */}
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 mb-8">
          <div className="flex items-start gap-3">
            <AlertCircle className="h-5 w-5 text-blue-600 mt-0.5" />
            <div>
              <p className="text-sm text-blue-700">
                <strong>Importante:</strong> Todos los documentos deben estar legibles y
                actualizados. La certificación del RNC debe tener menos de 30 días de emitida.
              </p>
            </div>
          </div>
        </div>

        {/* Submit button */}
        <div className="flex justify-end">
          <button
            onClick={handleSubmit}
            disabled={!isFormValid() || updateDocumentsMutation.isPending}
            className="px-8 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 
              text-white font-medium rounded-xl transition-colors 
              inline-flex items-center gap-2"
          >
            {updateDocumentsMutation.isPending ? (
              <>
                <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
                Subiendo...
              </>
            ) : (
              <>
                Continuar
                <ArrowRight className="h-5 w-5" />
              </>
            )}
          </button>
        </div>
      </div>

      {/* Image Preview Modal */}
      {previewUrl && (
        <div
          className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4"
          onClick={() => setPreviewUrl(null)}
        >
          <div className="relative max-w-4xl max-h-[90vh]">
            <button
              onClick={() => setPreviewUrl(null)}
              className="absolute -top-10 right-0 text-white hover:text-gray-300"
            >
              <X className="h-8 w-8" />
            </button>
            <img
              src={previewUrl}
              alt="Preview"
              className="max-h-[85vh] rounded-lg"
              onClick={(e) => e.stopPropagation()}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default DealerDocumentsPage;
