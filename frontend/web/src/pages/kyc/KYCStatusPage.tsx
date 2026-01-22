import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '@/hooks/useAuth';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import {
  FiShield,
  FiCheck,
  FiX,
  FiClock,
  FiAlertTriangle,
  FiFileText,
  FiRefreshCw,
  FiLoader,
  FiCalendar,
  FiUser,
  FiChevronRight,
} from 'react-icons/fi';
import { kycService, KYCStatus, RiskLevel } from '@/services/kycService';
import type { KYCProfile, KYCDocument } from '@/services/kycService';

export default function KYCStatusPage() {
  // Translation hook available for future use
  useTranslation('kyc');
  const navigate = useNavigate();
  const { user } = useAuth();

  const [isLoading, setIsLoading] = useState(true);
  const [profile, setProfile] = useState<KYCProfile | null>(null);
  const [, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchProfile = async () => {
      if (!user?.id) return;

      try {
        setIsLoading(true);
        const data = await kycService.getProfileByUserId(user.id);
        setProfile(data);
      } catch (err) {
        console.error('Error fetching KYC status:', err);
        setError('Error al cargar el estado de verificación');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [user?.id]);

  const getStatusIcon = (status: KYCStatus) => {
    switch (status) {
      case KYCStatus.Approved:
        return <FiCheck className="text-green-600" size={40} />;
      case KYCStatus.Rejected:
        return <FiX className="text-red-600" size={40} />;
      case KYCStatus.PendingReview:
      case KYCStatus.UnderReview:
        return <FiClock className="text-yellow-600" size={40} />;
      case KYCStatus.Expired:
        return <FiAlertTriangle className="text-orange-600" size={40} />;
      default:
        return <FiShield className="text-gray-400" size={40} />;
    }
  };

  const getStatusBgColor = (status: KYCStatus) => {
    switch (status) {
      case KYCStatus.Approved:
        return 'bg-green-100';
      case KYCStatus.Rejected:
        return 'bg-red-100';
      case KYCStatus.PendingReview:
      case KYCStatus.UnderReview:
        return 'bg-yellow-100';
      case KYCStatus.Expired:
        return 'bg-orange-100';
      default:
        return 'bg-gray-100';
    }
  };

  const getRiskLevelLabel = (level: RiskLevel) => {
    const labels: Record<RiskLevel, string> = {
      [RiskLevel.Low]: 'Bajo',
      [RiskLevel.Medium]: 'Medio',
      [RiskLevel.High]: 'Alto',
      [RiskLevel.Critical]: 'Crítico',
    };
    return labels[level];
  };

  const getRiskLevelColor = (level: RiskLevel) => {
    const colors: Record<RiskLevel, string> = {
      [RiskLevel.Low]: 'text-green-600 bg-green-100',
      [RiskLevel.Medium]: 'text-yellow-600 bg-yellow-100',
      [RiskLevel.High]: 'text-orange-600 bg-orange-100',
      [RiskLevel.Critical]: 'text-red-600 bg-red-100',
    };
    return colors[level];
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const getDaysUntilExpiry = (expiresAt: string) => {
    const expiry = new Date(expiresAt);
    const now = new Date();
    const diffTime = expiry.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

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

  // No profile exists
  if (!profile) {
    return (
      <MainLayout>
        <div className="max-w-2xl mx-auto px-4 py-12">
          <div className="bg-white rounded-xl shadow-sm border p-8 text-center">
            <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <FiShield className="text-gray-400" size={40} />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Verificación No Iniciada</h1>
            <p className="text-gray-600 mb-6">
              Aún no has iniciado el proceso de verificación de identidad. Completa tu KYC para
              acceder a todas las funcionalidades.
            </p>
            <Button variant="primary" onClick={() => navigate('/kyc/verify')}>
              Iniciar Verificación <FiChevronRight className="ml-2" />
            </Button>
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
          <h1 className="text-3xl font-bold text-gray-900">Estado de Verificación</h1>
        </div>

        {/* Status Card */}
        <div className="bg-white rounded-xl shadow-sm border overflow-hidden mb-6">
          <div className={`p-8 ${getStatusBgColor(profile.status)}`}>
            <div className="flex items-center justify-center">
              <div className="w-20 h-20 bg-white rounded-full flex items-center justify-center shadow-sm">
                {getStatusIcon(profile.status)}
              </div>
            </div>
            <h2 className="text-2xl font-bold text-center mt-4">
              {kycService.getStatusLabel(profile.status)}
            </h2>

            {profile.status === KYCStatus.Approved && profile.expiresAt && (
              <div className="text-center mt-2">
                <p className="text-green-700">Válido hasta: {formatDate(profile.expiresAt)}</p>
                {getDaysUntilExpiry(profile.expiresAt) <= 30 && (
                  <div className="mt-2 inline-block px-3 py-1 bg-yellow-200 text-yellow-800 rounded-full text-sm">
                    ⚠️ Expira en {getDaysUntilExpiry(profile.expiresAt)} días
                  </div>
                )}
              </div>
            )}

            {profile.status === KYCStatus.Rejected && profile.rejectionReason && (
              <div className="mt-4 p-4 bg-white/50 rounded-lg max-w-md mx-auto">
                <p className="text-red-800 font-medium">Razón del rechazo:</p>
                <p className="text-red-700">{profile.rejectionReason}</p>
              </div>
            )}
          </div>

          <div className="p-6">
            {/* Profile Info */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <div>
                <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <FiUser className="text-primary" />
                  Información Personal
                </h3>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-500">Nombre:</span>
                    <span className="font-medium">
                      {profile.firstName} {profile.lastName}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Documento:</span>
                    <span className="font-medium">{profile.documentNumber}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Nacionalidad:</span>
                    <span className="font-medium">{profile.nationality}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">Ocupación:</span>
                    <span className="font-medium">{profile.occupation}</span>
                  </div>
                </div>
              </div>

              <div>
                <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <FiShield className="text-primary" />
                  Nivel de Riesgo
                </h3>
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-gray-500">Categoría:</span>
                    <span
                      className={`px-3 py-1 rounded-full text-sm font-medium ${getRiskLevelColor(profile.riskLevel)}`}
                    >
                      {getRiskLevelLabel(profile.riskLevel)}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-gray-500">Score:</span>
                    <span className="font-medium">{profile.riskScore.toFixed(1)} / 100</span>
                  </div>
                  {profile.isPEP && (
                    <div className="p-2 bg-orange-50 border border-orange-200 rounded text-sm text-orange-800">
                      ⚠️ Persona Políticamente Expuesta (PEP)
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Documents */}
            <div>
              <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <FiFileText className="text-primary" />
                Documentos ({profile.documents?.length || 0})
              </h3>
              <div className="space-y-2">
                {profile.documents?.map((doc) => (
                  <DocumentRow key={doc.id} document={doc} />
                ))}
                {(!profile.documents || profile.documents.length === 0) && (
                  <p className="text-gray-500 text-sm">No hay documentos subidos</p>
                )}
              </div>
            </div>

            {/* Timeline */}
            <div className="mt-6 pt-6 border-t">
              <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <FiCalendar className="text-primary" />
                Historial
              </h3>
              <div className="space-y-3">
                <TimelineItem date={profile.createdAt} title="Perfil creado" status="completed" />
                {profile.approvedAt && (
                  <TimelineItem
                    date={profile.approvedAt}
                    title={`Aprobado por ${profile.approvedBy}`}
                    status="completed"
                  />
                )}
                {profile.rejectedAt && (
                  <TimelineItem date={profile.rejectedAt} title="Rechazado" status="rejected" />
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-center gap-4">
          {profile.status === KYCStatus.Rejected && (
            <Button variant="primary" onClick={() => navigate('/kyc/verify')}>
              <FiRefreshCw className="mr-2" />
              Reiniciar Verificación
            </Button>
          )}
          {profile.status === KYCStatus.Expired && (
            <Button variant="primary" onClick={() => navigate('/kyc/verify')}>
              <FiRefreshCw className="mr-2" />
              Renovar Verificación
            </Button>
          )}
          {profile.status === KYCStatus.InProgress && (
            <Button variant="primary" onClick={() => navigate('/kyc/verify')}>
              Continuar Verificación
              <FiChevronRight className="ml-2" />
            </Button>
          )}
          <Button variant="outline" onClick={() => navigate('/dashboard')}>
            Volver al Dashboard
          </Button>
        </div>
      </div>
    </MainLayout>
  );
}

// Document Row Component
function DocumentRow({ document }: { document: KYCDocument }) {
  const getVerificationBadge = () => {
    switch (document.verificationStatus) {
      case 'Verified':
        return (
          <span className="px-2 py-1 bg-green-100 text-green-700 rounded text-xs font-medium flex items-center gap-1">
            <FiCheck size={12} /> Verificado
          </span>
        );
      case 'Rejected':
        return (
          <span className="px-2 py-1 bg-red-100 text-red-700 rounded text-xs font-medium flex items-center gap-1">
            <FiX size={12} /> Rechazado
          </span>
        );
      default:
        return (
          <span className="px-2 py-1 bg-yellow-100 text-yellow-700 rounded text-xs font-medium flex items-center gap-1">
            <FiClock size={12} /> Pendiente
          </span>
        );
    }
  };

  return (
    <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
      <div className="flex items-center gap-3">
        <FiFileText className="text-gray-400" />
        <div>
          <p className="font-medium text-sm">
            {kycService.getDocumentTypeLabel(document.documentType)}
          </p>
          <p className="text-xs text-gray-500">{document.fileName}</p>
        </div>
      </div>
      {getVerificationBadge()}
    </div>
  );
}

// Timeline Item Component
function TimelineItem({
  date,
  title,
  status,
}: {
  date: string;
  title: string;
  status: 'completed' | 'rejected' | 'pending';
}) {
  return (
    <div className="flex items-start gap-3">
      <div
        className={`w-3 h-3 rounded-full mt-1 ${
          status === 'completed'
            ? 'bg-green-500'
            : status === 'rejected'
              ? 'bg-red-500'
              : 'bg-gray-300'
        }`}
      />
      <div>
        <p className="font-medium text-sm">{title}</p>
        <p className="text-xs text-gray-500">{new Date(date).toLocaleString('es-DO')}</p>
      </div>
    </div>
  );
}
