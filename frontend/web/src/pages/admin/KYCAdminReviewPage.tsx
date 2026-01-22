import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import {
  FiShield,
  FiCheck,
  FiX,
  FiAlertTriangle,
  FiFileText,
  FiUser,
  FiClock,
  FiLoader,
  FiChevronLeft,
  FiChevronRight,
  FiEye,
  FiAlertCircle,
} from 'react-icons/fi';
import { kycService, KYCStatus, RiskLevel } from '@/services/kycService';
import type { KYCProfile, KYCProfileSummary } from '@/services/kycService';

// Tab types
type TabType = 'pending' | 'all' | 'statistics';

export default function KYCAdminReviewPage() {
  // Translation hook available for future use
  useTranslation('kyc');
  const navigate = useNavigate();
  const { profileId } = useParams<{ profileId?: string }>();

  const [activeTab, setActiveTab] = useState<TabType>('pending');
  const [isLoading, setIsLoading] = useState(true);
  const [profiles, setProfiles] = useState<KYCProfileSummary[]>([]);
  const [selectedProfile, setSelectedProfile] = useState<KYCProfile | null>(null);
  const [pagination, setPagination] = useState({ page: 1, totalPages: 1, totalCount: 0 });
  const [error, setError] = useState<string | null>(null);

  // Approval/Rejection modals
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [comments, setComments] = useState('');
  const [rejectionReason, setRejectionReason] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Statistics
  const [stats, setStats] = useState<{
    totalProfiles: number;
    pendingReview: number;
    approved: number;
    rejected: number;
    expired: number;
    pepCount: number;
    avgApprovalTime: number;
  } | null>(null);

  // Fetch profiles
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        setError(null);

        if (activeTab === 'pending') {
          const result = await kycService.getPendingProfiles(pagination.page);
          setProfiles(result.items);
          setPagination({
            page: result.page,
            totalPages: result.totalPages,
            totalCount: result.totalCount,
          });
        } else if (activeTab === 'all') {
          const result = await kycService.getProfiles({ page: pagination.page });
          setProfiles(result.items);
          setPagination({
            page: result.page,
            totalPages: result.totalPages,
            totalCount: result.totalCount,
          });
        } else if (activeTab === 'statistics') {
          const statsData = await kycService.getStatistics();
          setStats(statsData);
        }
      } catch (err) {
        console.error('Error fetching KYC data:', err);
        setError('Error al cargar los datos');
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [activeTab, pagination.page]);

  // Fetch specific profile if ID in URL
  useEffect(() => {
    if (profileId) {
      const fetchProfile = async () => {
        try {
          setIsLoading(true);
          const profile = await kycService.getProfileById(profileId);
          setSelectedProfile(profile);
        } catch (err) {
          console.error('Error fetching profile:', err);
          setError('Error al cargar el perfil');
        } finally {
          setIsLoading(false);
        }
      };
      fetchProfile();
    }
  }, [profileId]);

  // Handle profile selection
  const handleSelectProfile = async (id: string) => {
    try {
      setIsLoading(true);
      const profile = await kycService.getProfileById(id);
      setSelectedProfile(profile);
      navigate(`/admin/kyc/${id}`);
    } catch (err) {
      console.error('Error fetching profile:', err);
      setError('Error al cargar el perfil');
    } finally {
      setIsLoading(false);
    }
  };

  // Handle approval
  const handleApprove = async () => {
    if (!selectedProfile) return;

    try {
      setIsSubmitting(true);
      const expiresAt = new Date();
      expiresAt.setFullYear(expiresAt.getFullYear() + 1); // Default 1 year

      await kycService.approveProfile(
        selectedProfile.id,
        'admin@okla.com.do', // TODO: Get from auth context
        comments,
        expiresAt.toISOString()
      );

      setShowApproveModal(false);
      setComments('');
      setSelectedProfile(null);
      navigate('/admin/kyc');
      // Refresh list
      setPagination((p) => ({ ...p, page: 1 }));
    } catch (err) {
      setError('Error al aprobar el perfil');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle rejection
  const handleReject = async () => {
    if (!selectedProfile || !rejectionReason) return;

    try {
      setIsSubmitting(true);
      await kycService.rejectProfile(
        selectedProfile.id,
        rejectionReason,
        comments,
        true // canRetry
      );

      setShowRejectModal(false);
      setRejectionReason('');
      setComments('');
      setSelectedProfile(null);
      navigate('/admin/kyc');
      // Refresh list
      setPagination((p) => ({ ...p, page: 1 }));
    } catch (err) {
      setError('Error al rechazar el perfil');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Verify document
  const handleVerifyDocument = async (docId: string) => {
    if (!selectedProfile) return;

    try {
      await kycService.verifyDocument(docId, 'admin@okla.com.do');
      // Refresh profile
      const updated = await kycService.getProfileById(selectedProfile.id);
      setSelectedProfile(updated);
    } catch (err) {
      setError('Error al verificar el documento');
    }
  };

  const getRiskBadge = (level: RiskLevel) => {
    const colors: Record<RiskLevel, string> = {
      [RiskLevel.Low]: 'bg-green-100 text-green-700',
      [RiskLevel.Medium]: 'bg-yellow-100 text-yellow-700',
      [RiskLevel.High]: 'bg-orange-100 text-orange-700',
      [RiskLevel.Critical]: 'bg-red-100 text-red-700',
    };
    const labels: Record<RiskLevel, string> = {
      [RiskLevel.Low]: 'Bajo',
      [RiskLevel.Medium]: 'Medio',
      [RiskLevel.High]: 'Alto',
      [RiskLevel.Critical]: 'Crítico',
    };
    return (
      <span className={`px-2 py-1 rounded text-xs font-medium ${colors[level]}`}>
        {labels[level]}
      </span>
    );
  };

  // Detail View
  if (selectedProfile) {
    return (
      <MainLayout>
        <div className="max-w-6xl mx-auto px-4 py-8">
          {/* Back Button */}
          <button
            onClick={() => {
              setSelectedProfile(null);
              navigate('/admin/kyc');
            }}
            className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
          >
            <FiChevronLeft /> Volver a la lista
          </button>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
              <FiAlertCircle className="text-red-600" />
              <p className="text-red-800">{error}</p>
            </div>
          )}

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Profile Info */}
            <div className="lg:col-span-2 space-y-6">
              {/* Header Card */}
              <div className="bg-white rounded-xl shadow-sm border p-6">
                <div className="flex items-start justify-between mb-6">
                  <div className="flex items-center gap-4">
                    <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                      <FiUser className="text-gray-400" size={32} />
                    </div>
                    <div>
                      <h1 className="text-2xl font-bold">
                        {selectedProfile.firstName} {selectedProfile.lastName}
                      </h1>
                      <p className="text-gray-500">{selectedProfile.documentNumber}</p>
                      <div className="flex items-center gap-2 mt-2">
                        <span
                          className={`px-3 py-1 rounded-full text-sm font-medium ${
                            selectedProfile.status === KYCStatus.PendingReview
                              ? 'bg-yellow-100 text-yellow-700'
                              : selectedProfile.status === KYCStatus.Approved
                                ? 'bg-green-100 text-green-700'
                                : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {kycService.getStatusLabel(selectedProfile.status)}
                        </span>
                        {getRiskBadge(selectedProfile.riskLevel)}
                        {selectedProfile.isPEP && (
                          <span className="px-2 py-1 bg-orange-100 text-orange-700 rounded text-xs font-medium flex items-center gap-1">
                            <FiAlertTriangle size={12} /> PEP
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Personal Details */}
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-gray-500">Fecha de Nacimiento:</span>
                    <p className="font-medium">
                      {new Date(selectedProfile.dateOfBirth).toLocaleDateString('es-DO')}
                    </p>
                  </div>
                  <div>
                    <span className="text-gray-500">Nacionalidad:</span>
                    <p className="font-medium">{selectedProfile.nationality}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Teléfono:</span>
                    <p className="font-medium">{selectedProfile.phoneNumber}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Ocupación:</span>
                    <p className="font-medium">{selectedProfile.occupation}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Fuente de Ingresos:</span>
                    <p className="font-medium">{selectedProfile.sourceOfFunds}</p>
                  </div>
                  <div>
                    <span className="text-gray-500">Transacciones Esperadas:</span>
                    <p className="font-medium">
                      {selectedProfile.expectedMonthlyTransaction
                        ? `$${selectedProfile.expectedMonthlyTransaction.toLocaleString()}/mes`
                        : 'No especificado'}
                    </p>
                  </div>
                </div>

                {/* Address */}
                <div className="mt-4 pt-4 border-t">
                  <span className="text-gray-500 text-sm">Dirección:</span>
                  <p className="font-medium">
                    {selectedProfile.address}, {selectedProfile.city}, {selectedProfile.province}
                  </p>
                </div>
              </div>

              {/* Documents */}
              <div className="bg-white rounded-xl shadow-sm border p-6">
                <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <FiFileText className="text-primary" />
                  Documentos ({selectedProfile.documents?.length || 0})
                </h2>
                <div className="space-y-3">
                  {selectedProfile.documents?.map((doc) => (
                    <div
                      key={doc.id}
                      className="flex items-center justify-between p-4 bg-gray-50 rounded-lg"
                    >
                      <div className="flex items-center gap-3">
                        <FiFileText className="text-gray-400" size={24} />
                        <div>
                          <p className="font-medium">
                            {kycService.getDocumentTypeLabel(doc.documentType)}
                          </p>
                          <p className="text-sm text-gray-500">{doc.fileName}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <span
                          className={`px-2 py-1 rounded text-xs font-medium ${
                            doc.verificationStatus === 'Verified'
                              ? 'bg-green-100 text-green-700'
                              : doc.verificationStatus === 'Rejected'
                                ? 'bg-red-100 text-red-700'
                                : 'bg-yellow-100 text-yellow-700'
                          }`}
                        >
                          {doc.verificationStatus === 'Verified'
                            ? 'Verificado'
                            : doc.verificationStatus === 'Rejected'
                              ? 'Rechazado'
                              : 'Pendiente'}
                        </span>
                        <Button variant="ghost" size="sm">
                          <FiEye />
                        </Button>
                        {doc.verificationStatus === 'Pending' && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleVerifyDocument(doc.id)}
                          >
                            <FiCheck className="mr-1" /> Verificar
                          </Button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Actions Sidebar */}
            <div className="space-y-6">
              {/* Risk Assessment */}
              <div className="bg-white rounded-xl shadow-sm border p-6">
                <h3 className="font-semibold mb-4 flex items-center gap-2">
                  <FiShield className="text-primary" />
                  Evaluación de Riesgo
                </h3>
                <div className="space-y-4">
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span>Risk Score</span>
                      <span className="font-medium">
                        {selectedProfile.riskScore.toFixed(1)} / 100
                      </span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full ${
                          selectedProfile.riskScore < 30
                            ? 'bg-green-500'
                            : selectedProfile.riskScore < 60
                              ? 'bg-yellow-500'
                              : 'bg-red-500'
                        }`}
                        style={{ width: `${selectedProfile.riskScore}%` }}
                      />
                    </div>
                  </div>

                  {selectedProfile.isPEP && (
                    <div className="p-3 bg-orange-50 border border-orange-200 rounded-lg">
                      <p className="text-sm font-medium text-orange-800">
                        ⚠️ Persona Políticamente Expuesta
                      </p>
                      {selectedProfile.pepPosition && (
                        <p className="text-sm text-orange-700 mt-1">
                          Posición: {selectedProfile.pepPosition}
                        </p>
                      )}
                    </div>
                  )}
                </div>
              </div>

              {/* Actions */}
              {selectedProfile.status === KYCStatus.PendingReview && (
                <div className="bg-white rounded-xl shadow-sm border p-6">
                  <h3 className="font-semibold mb-4">Acciones</h3>
                  <div className="space-y-3">
                    <Button variant="primary" fullWidth onClick={() => setShowApproveModal(true)}>
                      <FiCheck className="mr-2" /> Aprobar
                    </Button>
                    <Button
                      variant="ghost"
                      fullWidth
                      className="text-red-600 hover:bg-red-50"
                      onClick={() => setShowRejectModal(true)}
                    >
                      <FiX className="mr-2" /> Rechazar
                    </Button>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Approve Modal */}
          {showApproveModal && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
              <div className="bg-white rounded-xl p-6 max-w-md w-full mx-4">
                <h3 className="text-lg font-bold mb-4">Aprobar Verificación</h3>
                <p className="text-gray-600 mb-4">
                  ¿Estás seguro de que deseas aprobar la verificación KYC de{' '}
                  <strong>
                    {selectedProfile.firstName} {selectedProfile.lastName}
                  </strong>
                  ?
                </p>
                <textarea
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  placeholder="Comentarios (opcional)"
                  className="w-full p-3 border rounded-lg mb-4"
                  rows={3}
                />
                <div className="flex gap-3">
                  <Button variant="outline" onClick={() => setShowApproveModal(false)}>
                    Cancelar
                  </Button>
                  <Button variant="primary" onClick={handleApprove} isLoading={isSubmitting}>
                    Confirmar Aprobación
                  </Button>
                </div>
              </div>
            </div>
          )}

          {/* Reject Modal */}
          {showRejectModal && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
              <div className="bg-white rounded-xl p-6 max-w-md w-full mx-4">
                <h3 className="text-lg font-bold mb-4">Rechazar Verificación</h3>
                <div className="mb-4">
                  <label className="block text-sm font-medium mb-2">Razón del Rechazo *</label>
                  <select
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                    className="w-full p-3 border rounded-lg"
                  >
                    <option value="">Seleccionar razón...</option>
                    <option value="DocumentExpired">Documento expirado</option>
                    <option value="DocumentBlurry">Documento ilegible</option>
                    <option value="DataMismatch">Datos no coinciden</option>
                    <option value="SuspiciousActivity">Actividad sospechosa</option>
                    <option value="IncompleteDocs">Documentos incompletos</option>
                    <option value="Other">Otra razón</option>
                  </select>
                </div>
                <textarea
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  placeholder="Detalles adicionales para el usuario"
                  className="w-full p-3 border rounded-lg mb-4"
                  rows={3}
                />
                <div className="flex gap-3">
                  <Button variant="outline" onClick={() => setShowRejectModal(false)}>
                    Cancelar
                  </Button>
                  <Button
                    variant="primary"
                    className="bg-red-600 hover:bg-red-700"
                    onClick={handleReject}
                    isLoading={isSubmitting}
                    disabled={!rejectionReason}
                  >
                    Confirmar Rechazo
                  </Button>
                </div>
              </div>
            </div>
          )}
        </div>
      </MainLayout>
    );
  }

  // List View
  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <FiShield className="text-primary" />
            Revisión KYC
          </h1>
          <p className="text-gray-600 mt-2">Gestiona las verificaciones de identidad de usuarios</p>
        </div>

        {/* Tabs */}
        <div className="flex gap-4 mb-6 border-b">
          {(['pending', 'all', 'statistics'] as TabType[]).map((tab) => (
            <button
              key={tab}
              onClick={() => {
                setActiveTab(tab);
                setPagination((p) => ({ ...p, page: 1 }));
              }}
              className={`px-4 py-2 border-b-2 transition-colors ${
                activeTab === tab
                  ? 'border-primary text-primary font-medium'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {tab === 'pending' && 'Pendientes'}
              {tab === 'all' && 'Todos'}
              {tab === 'statistics' && 'Estadísticas'}
            </button>
          ))}
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex items-center gap-3">
            <FiAlertCircle className="text-red-600" />
            <p className="text-red-800">{error}</p>
          </div>
        )}

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <FiLoader className="animate-spin text-primary" size={40} />
          </div>
        ) : activeTab === 'statistics' ? (
          /* Statistics View */
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard title="Total Perfiles" value={stats?.totalProfiles || 0} icon={<FiUser />} />
            <StatCard
              title="Pendientes"
              value={stats?.pendingReview || 0}
              icon={<FiClock />}
              color="yellow"
            />
            <StatCard
              title="Aprobados"
              value={stats?.approved || 0}
              icon={<FiCheck />}
              color="green"
            />
            <StatCard title="Rechazados" value={stats?.rejected || 0} icon={<FiX />} color="red" />
            <StatCard
              title="Expirados"
              value={stats?.expired || 0}
              icon={<FiAlertTriangle />}
              color="orange"
            />
            <StatCard
              title="PEPs"
              value={stats?.pepCount || 0}
              icon={<FiAlertTriangle />}
              color="purple"
            />
            <StatCard
              title="Tiempo Promedio"
              value={`${stats?.avgApprovalTime || 0}h`}
              icon={<FiClock />}
            />
          </div>
        ) : (
          /* Profiles List */
          <div className="bg-white rounded-xl shadow-sm border overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50 border-b">
                <tr>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Usuario</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">
                    Documento
                  </th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Estado</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Riesgo</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Fecha</th>
                  <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {profiles.map((profile) => (
                  <tr key={profile.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center">
                          <FiUser className="text-gray-400" />
                        </div>
                        <div>
                          <p className="font-medium">{profile.fullName}</p>
                          {profile.isPEP && <span className="text-xs text-orange-600">PEP</span>}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm">{profile.documentNumber}</td>
                    <td className="px-6 py-4">
                      <span
                        className={`px-2 py-1 rounded text-xs font-medium ${
                          profile.status === KYCStatus.PendingReview
                            ? 'bg-yellow-100 text-yellow-700'
                            : profile.status === KYCStatus.Approved
                              ? 'bg-green-100 text-green-700'
                              : profile.status === KYCStatus.Rejected
                                ? 'bg-red-100 text-red-700'
                                : 'bg-gray-100 text-gray-700'
                        }`}
                      >
                        {kycService.getStatusLabel(profile.status)}
                      </span>
                    </td>
                    <td className="px-6 py-4">{getRiskBadge(profile.riskLevel)}</td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {new Date(profile.createdAt).toLocaleDateString('es-DO')}
                    </td>
                    <td className="px-6 py-4 text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleSelectProfile(profile.id)}
                      >
                        Revisar <FiChevronRight className="ml-1" />
                      </Button>
                    </td>
                  </tr>
                ))}
                {profiles.length === 0 && (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                      No hay perfiles para mostrar
                    </td>
                  </tr>
                )}
              </tbody>
            </table>

            {/* Pagination */}
            {pagination.totalPages > 1 && (
              <div className="flex items-center justify-between px-6 py-4 border-t">
                <p className="text-sm text-gray-500">
                  Mostrando página {pagination.page} de {pagination.totalPages} (
                  {pagination.totalCount} total)
                </p>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={pagination.page === 1}
                    onClick={() => setPagination((p) => ({ ...p, page: p.page - 1 }))}
                  >
                    Anterior
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={pagination.page === pagination.totalPages}
                    onClick={() => setPagination((p) => ({ ...p, page: p.page + 1 }))}
                  >
                    Siguiente
                  </Button>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
}

// Stat Card Component
function StatCard({
  title,
  value,
  icon,
  color = 'blue',
}: {
  title: string;
  value: number | string;
  icon: React.ReactNode;
  color?: 'blue' | 'green' | 'yellow' | 'red' | 'orange' | 'purple';
}) {
  const colorClasses = {
    blue: 'bg-blue-100 text-blue-600',
    green: 'bg-green-100 text-green-600',
    yellow: 'bg-yellow-100 text-yellow-600',
    red: 'bg-red-100 text-red-600',
    orange: 'bg-orange-100 text-orange-600',
    purple: 'bg-purple-100 text-purple-600',
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-500">{title}</p>
          <p className="text-2xl font-bold mt-1">{value}</p>
        </div>
        <div
          className={`w-12 h-12 rounded-lg flex items-center justify-center ${colorClasses[color]}`}
        >
          {icon}
        </div>
      </div>
    </div>
  );
}
