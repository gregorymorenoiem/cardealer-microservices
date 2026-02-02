/**
 * Dealer Documents Page
 *
 * Manage dealer verification documents with real API integration
 */

'use client';

import { useState, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  FileText,
  Upload,
  Download,
  Eye,
  Trash2,
  CheckCircle,
  Clock,
  XCircle,
  AlertCircle,
  Shield,
  Calendar,
  File,
  Loader2,
} from 'lucide-react';
import { toast } from 'sonner';
import {
  useCurrentDealer,
  useDealerDocuments,
  useUploadDealerDocument,
  useDeleteDealerDocument,
  useDocumentVerificationStats,
  DOCUMENT_TYPE_LABELS,
  REQUIRED_DOCUMENT_TYPES,
  OPTIONAL_DOCUMENT_TYPES,
  type DealerDocumentDto,
} from '@/hooks/use-dealers';
import type { DocumentType, DocumentVerificationStatus } from '@/services/dealers';

// =============================================================================
// HELPERS
// =============================================================================

function getStatusBadge(status: DocumentVerificationStatus) {
  switch (status) {
    case 'Approved':
      return (
        <Badge className="bg-emerald-100 text-emerald-700">
          <CheckCircle className="mr-1 h-3 w-3" />
          Verificado
        </Badge>
      );
    case 'Pending':
      return (
        <Badge className="bg-gray-100 text-gray-700">
          <Clock className="mr-1 h-3 w-3" />
          Pendiente
        </Badge>
      );
    case 'UnderReview':
      return (
        <Badge className="bg-blue-100 text-blue-700">
          <Clock className="mr-1 h-3 w-3" />
          En Revisión
        </Badge>
      );
    case 'Rejected':
      return (
        <Badge className="bg-red-100 text-red-700">
          <XCircle className="mr-1 h-3 w-3" />
          Rechazado
        </Badge>
      );
    case 'Expired':
      return (
        <Badge className="bg-amber-100 text-amber-700">
          <AlertCircle className="mr-1 h-3 w-3" />
          Expirado
        </Badge>
      );
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
}

function isExpiringSoon(expiryDate?: string): boolean {
  if (!expiryDate) return false;
  const expiry = new Date(expiryDate);
  const thirtyDaysFromNow = new Date();
  thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
  return expiry <= thirtyDaysFromNow && expiry > new Date();
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

// =============================================================================
// SKELETON COMPONENTS
// =============================================================================

function VerificationSkeleton() {
  return (
    <Card className="border-gray-200">
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Skeleton className="h-14 w-14 rounded-xl" />
            <div>
              <Skeleton className="mb-2 h-6 w-48" />
              <Skeleton className="h-4 w-64" />
            </div>
          </div>
          <div className="flex gap-2">
            {[1, 2, 3, 4].map(i => (
              <Skeleton key={i} className="h-3 w-3 rounded-full" />
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

function DocumentsSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3, 4].map(i => (
        <div key={i} className="flex items-center gap-4 rounded-lg border p-4">
          <Skeleton className="h-12 w-12 rounded-lg" />
          <div className="flex-1">
            <Skeleton className="mb-2 h-5 w-48" />
            <Skeleton className="h-4 w-32" />
          </div>
          <Skeleton className="h-6 w-24" />
          <Skeleton className="h-9 w-24" />
        </div>
      ))}
    </div>
  );
}

// =============================================================================
// DOCUMENT ROW COMPONENT
// =============================================================================

interface DocumentRowProps {
  document?: DealerDocumentDto;
  type: DocumentType;
  dealerId: string;
  isRequired: boolean;
  onUpload: (type: DocumentType, file: File) => void;
  isUploading: boolean;
}

function DocumentRow({
  document,
  type,
  dealerId,
  isRequired,
  onUpload,
  isUploading,
}: DocumentRowProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const deleteMutation = useDeleteDealerDocument(dealerId);

  const name = DOCUMENT_TYPE_LABELS[type];
  const hasDocument = !!document;
  const status = document?.verificationStatus || 'Pending';
  const expiringSoon = document ? isExpiringSoon(document.expiryDate) : false;

  // Suppress unused variable warning
  void isRequired;

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onUpload(type, file);
    }
    // Reset input
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDelete = () => {
    if (document && confirm('¿Estás seguro de eliminar este documento?')) {
      deleteMutation.mutate(document.id, {
        onSuccess: () => {
          toast.success('Documento eliminado');
        },
        onError: () => {
          toast.error('Error al eliminar documento');
        },
      });
    }
  };

  const handleDownload = () => {
    if (document?.fileUrl) {
      window.open(document.fileUrl, '_blank');
    }
  };

  const handleView = () => {
    if (document?.fileUrl) {
      window.open(document.fileUrl, '_blank');
    }
  };

  return (
    <div className="flex flex-col gap-4 rounded-lg border p-4 sm:flex-row sm:items-center">
      <div className="flex flex-1 items-center gap-4">
        <div
          className={`rounded-lg p-3 ${
            status === 'Approved'
              ? 'bg-emerald-100'
              : expiringSoon || status === 'Expired'
                ? 'bg-amber-100'
                : status === 'Rejected'
                  ? 'bg-red-100'
                  : 'bg-gray-100'
          }`}
        >
          <FileText
            className={`h-6 w-6 ${
              status === 'Approved'
                ? 'text-emerald-600'
                : expiringSoon || status === 'Expired'
                  ? 'text-amber-600'
                  : status === 'Rejected'
                    ? 'text-red-600'
                    : 'text-gray-600'
            }`}
          />
        </div>
        <div className="flex-1">
          <h4 className="font-medium">{name}</h4>
          {hasDocument ? (
            <>
              <p className="flex items-center gap-2 text-sm text-gray-500">
                <File className="h-3 w-3" />
                {document.fileName}
                <span className="text-gray-400">({formatFileSize(document.fileSizeBytes)})</span>
              </p>
              {document.expiryDate && (
                <p
                  className={`flex items-center gap-1 text-sm ${
                    expiringSoon || status === 'Expired' ? 'text-amber-600' : 'text-gray-500'
                  }`}
                >
                  <Calendar className="h-3 w-3" />
                  Vence: {new Date(document.expiryDate).toLocaleDateString('es-DO')}
                </p>
              )}
              {document.rejectionReason && (
                <p className="mt-1 text-sm text-red-600">Razón: {document.rejectionReason}</p>
              )}
            </>
          ) : (
            <p className="text-sm text-gray-400">Sin documento</p>
          )}
        </div>
      </div>

      <div className="flex items-center gap-3">
        {hasDocument && getStatusBadge(status)}
        {expiringSoon && status !== 'Expired' && (
          <Badge className="bg-amber-100 text-amber-700">
            <AlertCircle className="mr-1 h-3 w-3" />
            Por Vencer
          </Badge>
        )}

        {hasDocument && (
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={handleView} title="Ver documento">
              <Eye className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" onClick={handleDownload} title="Descargar">
              <Download className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              onClick={handleDelete}
              disabled={deleteMutation.isPending}
              title="Eliminar"
            >
              {deleteMutation.isPending ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Trash2 className="h-4 w-4 text-red-500" />
              )}
            </Button>
          </div>
        )}

        <input
          ref={fileInputRef}
          type="file"
          accept=".pdf,.jpg,.jpeg,.png"
          onChange={handleFileSelect}
          className="hidden"
        />

        <Button
          variant="outline"
          size="sm"
          onClick={() => fileInputRef.current?.click()}
          disabled={isUploading}
          className={expiringSoon ? 'border-amber-300' : ''}
        >
          {isUploading ? (
            <Loader2 className="mr-1 h-4 w-4 animate-spin" />
          ) : (
            <Upload className="mr-1 h-4 w-4" />
          )}
          {hasDocument ? 'Actualizar' : 'Subir'}
        </Button>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE COMPONENT
// =============================================================================

export default function DealerDocumentsPage() {
  const { data: dealer, isLoading: isDealerLoading } = useCurrentDealer();
  const { data: documents, isLoading: isDocumentsLoading } = useDealerDocuments(dealer?.id);
  const uploadMutation = useUploadDealerDocument(dealer?.id || '');
  const [uploadingType, setUploadingType] = useState<DocumentType | null>(null);

  const stats = useDocumentVerificationStats(documents);
  const isLoading = isDealerLoading || isDocumentsLoading;

  // Get document by type
  const getDocumentByType = (type: DocumentType): DealerDocumentDto | undefined => {
    return documents?.find(d => d.type === type);
  };

  // Handle file upload
  const handleUpload = async (type: DocumentType, file: File) => {
    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      toast.error('El archivo es demasiado grande. Máximo 10MB.');
      return;
    }

    // Validate file type
    const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png'];
    if (!allowedTypes.includes(file.type)) {
      toast.error('Tipo de archivo no permitido. Solo PDF, JPG, PNG.');
      return;
    }

    setUploadingType(type);
    try {
      await uploadMutation.mutateAsync({
        type,
        file,
      });
      toast.success('Documento subido correctamente');
    } catch {
      toast.error('Error al subir el documento');
    } finally {
      setUploadingType(null);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col justify-between gap-4 sm:flex-row">
          <div>
            <Skeleton className="mb-2 h-8 w-48" />
            <Skeleton className="h-5 w-64" />
          </div>
          <Skeleton className="h-10 w-40" />
        </div>
        <VerificationSkeleton />
        <Card>
          <CardHeader>
            <Skeleton className="mb-2 h-6 w-48" />
            <Skeleton className="h-4 w-80" />
          </CardHeader>
          <CardContent>
            <DocumentsSkeleton />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!dealer) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <Card className="w-full max-w-md p-6 text-center">
          <CardContent>
            <AlertCircle className="mx-auto mb-4 h-12 w-12 text-amber-500" />
            <h2 className="mb-2 text-xl font-semibold">No se encontró el dealer</h2>
            <p className="text-gray-600">Por favor, inicia sesión como dealer.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Documentos</h1>
          <p className="text-gray-600">Gestiona tus documentos de verificación</p>
        </div>
      </div>

      {/* Verification Status */}
      <Card
        className={
          stats.isFullyVerified
            ? 'border-emerald-200 bg-emerald-50'
            : stats.rejectedCount > 0
              ? 'border-red-200 bg-red-50'
              : 'border-amber-200 bg-amber-50'
        }
      >
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <div
                className={`rounded-xl p-3 ${
                  stats.isFullyVerified
                    ? 'bg-emerald-100'
                    : stats.rejectedCount > 0
                      ? 'bg-red-100'
                      : 'bg-amber-100'
                }`}
              >
                <Shield
                  className={`h-8 w-8 ${
                    stats.isFullyVerified
                      ? 'text-emerald-600'
                      : stats.rejectedCount > 0
                        ? 'text-red-600'
                        : 'text-amber-600'
                  }`}
                />
              </div>
              <div>
                <h2 className="text-xl font-bold">
                  {stats.isFullyVerified
                    ? 'Cuenta Verificada'
                    : stats.rejectedCount > 0
                      ? 'Documentos Rechazados'
                      : 'Verificación Pendiente'}
                </h2>
                <p
                  className={
                    stats.isFullyVerified
                      ? 'text-emerald-600'
                      : stats.rejectedCount > 0
                        ? 'text-red-600'
                        : 'text-amber-600'
                  }
                >
                  {stats.verifiedCount} de {stats.totalRequired} documentos requeridos verificados
                  {stats.pendingCount > 0 && ` • ${stats.pendingCount} en revisión`}
                  {stats.expiringCount > 0 && ` • ${stats.expiringCount} por vencer`}
                </p>
              </div>
            </div>
            <div className="hidden sm:block">
              <div className="flex items-center gap-2">
                {[...Array(stats.totalRequired)].map((_, i) => (
                  <div
                    key={i}
                    className={`h-3 w-3 rounded-full ${
                      i < stats.verifiedCount ? 'bg-emerald-500' : 'bg-gray-300'
                    }`}
                  />
                ))}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Required Documents */}
      <Card>
        <CardHeader>
          <CardTitle>Documentos Requeridos</CardTitle>
          <CardDescription>
            Estos documentos son necesarios para operar como dealer verificado
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {REQUIRED_DOCUMENT_TYPES.map(type => (
              <DocumentRow
                key={type}
                document={getDocumentByType(type)}
                type={type}
                dealerId={dealer.id}
                isRequired={true}
                onUpload={handleUpload}
                isUploading={uploadingType === type}
              />
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Optional Documents */}
      <Card>
        <CardHeader>
          <CardTitle>Documentos Opcionales</CardTitle>
          <CardDescription>Documentos adicionales que mejoran tu perfil</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {OPTIONAL_DOCUMENT_TYPES.map(type => (
              <DocumentRow
                key={type}
                document={getDocumentByType(type)}
                type={type}
                dealerId={dealer.id}
                isRequired={false}
                onUpload={handleUpload}
                isUploading={uploadingType === type}
              />
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Tips */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card className="border-blue-200 bg-blue-50">
          <CardContent className="p-4">
            <h4 className="mb-2 font-medium text-blue-800">📄 Formatos Aceptados</h4>
            <p className="text-sm text-blue-700">
              PDF, JPG, PNG. Tamaño máximo: 10MB por archivo. Asegúrate de que los documentos sean
              legibles y estén actualizados.
            </p>
          </CardContent>
        </Card>
        <Card className="border-amber-200 bg-amber-50">
          <CardContent className="p-4">
            <h4 className="mb-2 font-medium text-amber-800">⏰ Tiempo de Verificación</h4>
            <p className="text-sm text-amber-700">
              Los documentos son revisados en 24-48 horas hábiles. Recibirás una notificación cuando
              tu cuenta esté verificada.
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
