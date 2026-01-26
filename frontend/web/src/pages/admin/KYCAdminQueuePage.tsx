import React, { useState, useEffect } from 'react';
import {
  FiSearch,
  FiFilter,
  FiEye,
  FiCheckCircle,
  FiXCircle,
  FiClock,
  FiAlertTriangle,
  FiUser,
  FiFileText,
  FiRefreshCw,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import kycService, { KYCProfile, KYCStatus } from '../../services/kycService';

interface KYCQueueItem {
  profile: KYCProfile;
  priority: 'low' | 'medium' | 'high';
  waitingTime: string;
}

const KYCAdminQueuePage: React.FC = () => {
  const [profiles, setProfiles] = useState<KYCProfile[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<number | ''>('');
  const [selectedProfile, setSelectedProfile] = useState<KYCProfile | null>(null);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [processing, setProcessing] = useState(false);

  const [stats, setStats] = useState({
    pending: 0,
    inProgress: 0,
    approved: 0,
    rejected: 0,
    total: 0,
  });

  useEffect(() => {
    loadProfiles();
    loadStats();
  }, [selectedStatus]);

  const loadProfiles = async () => {
    try {
      setLoading(true);
      const status = selectedStatus === '' ? undefined : (selectedStatus as KYCStatus);
      const result = await kycService.getProfiles({
        page: 1,
        pageSize: 50,
        status,
      });
      setProfiles(result.items || []);
    } catch (error) {
      console.error('Error loading profiles:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      const statsData = await kycService.getStatistics();
      setStats({
        pending: statsData.pendingProfiles || 0,
        inProgress: statsData.inProgressProfiles || 0,
        approved: statsData.approvedProfiles || 0,
        rejected: statsData.rejectedProfiles || 0,
        total: statsData.totalProfiles || 0,
      });
    } catch (error) {
      console.error('Error loading stats:', error);
    }
  };

  const handleApprove = async (profileId: string) => {
    try {
      setProcessing(true);
      await kycService.approveProfile(profileId, {
        approvedByName: 'Admin User',
        notes: 'Aprobado desde panel de administración',
        validityDays: 365,
      });
      await loadProfiles();
      await loadStats();
      setSelectedProfile(null);
    } catch (error) {
      console.error('Error approving profile:', error);
      alert('Error al aprobar el perfil');
    } finally {
      setProcessing(false);
    }
  };

  const handleReject = async () => {
    if (!selectedProfile || !rejectionReason) return;

    try {
      setProcessing(true);
      await kycService.rejectProfile(selectedProfile.id, {
        rejectedByName: 'Admin User',
        rejectionReason: rejectionReason,
      });
      await loadProfiles();
      await loadStats();
      setSelectedProfile(null);
      setShowRejectModal(false);
      setRejectionReason('');
    } catch (error) {
      console.error('Error rejecting profile:', error);
      alert('Error al rechazar el perfil');
    } finally {
      setProcessing(false);
    }
  };

  const getStatusBadge = (status: number) => {
    const statusMap: Record<number, { label: string; color: string }> = {
      1: { label: 'Pendiente', color: 'bg-yellow-100 text-yellow-800' },
      2: { label: 'En Progreso', color: 'bg-blue-100 text-blue-800' },
      3: { label: 'Docs Requeridos', color: 'bg-orange-100 text-orange-800' },
      4: { label: 'En Revisión', color: 'bg-purple-100 text-purple-800' },
      5: { label: 'Aprobado', color: 'bg-green-100 text-green-800' },
      6: { label: 'Rechazado', color: 'bg-red-100 text-red-800' },
      7: { label: 'Expirado', color: 'bg-gray-100 text-gray-800' },
      8: { label: 'Suspendido', color: 'bg-red-100 text-red-800' },
    };
    const s = statusMap[status] || { label: 'Desconocido', color: 'bg-gray-100 text-gray-800' };
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${s.color}`}>{s.label}</span>
    );
  };

  const getRiskBadge = (riskLevel: number) => {
    const riskMap: Record<number, { label: string; color: string }> = {
      0: { label: 'Bajo', color: 'bg-green-100 text-green-800' },
      1: { label: 'Medio', color: 'bg-yellow-100 text-yellow-800' },
      2: { label: 'Alto', color: 'bg-orange-100 text-orange-800' },
      3: { label: 'Crítico', color: 'bg-red-100 text-red-800' },
    };
    const r = riskMap[riskLevel] || { label: 'Bajo', color: 'bg-green-100 text-green-800' };
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${r.color}`}>{r.label}</span>
    );
  };

  const filteredProfiles = profiles.filter(
    (p) =>
      p.fullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.primaryDocumentNumber?.includes(searchTerm)
  );

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="flex justify-between items-center mb-8">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Cola de Verificación KYC</h1>
              <p className="text-gray-600 mt-1">
                Revisa y aprueba los perfiles de verificación de identidad
              </p>
            </div>
            <button
              onClick={() => {
                loadProfiles();
                loadStats();
              }}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              <FiRefreshCw size={18} />
              Actualizar
            </button>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-8">
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Total</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                </div>
                <FiUser className="text-blue-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Pendientes</p>
                  <p className="text-2xl font-bold text-yellow-600">{stats.pending}</p>
                </div>
                <FiClock className="text-yellow-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">En Progreso</p>
                  <p className="text-2xl font-bold text-blue-600">{stats.inProgress}</p>
                </div>
                <FiFileText className="text-blue-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Aprobados</p>
                  <p className="text-2xl font-bold text-green-600">{stats.approved}</p>
                </div>
                <FiCheckCircle className="text-green-500" size={32} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Rechazados</p>
                  <p className="text-2xl font-bold text-red-600">{stats.rejected}</p>
                </div>
                <FiXCircle className="text-red-500" size={32} />
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-lg shadow p-4 mb-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1 relative">
                <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar por nombre o documento..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
              <div className="flex items-center gap-2">
                <FiFilter className="text-gray-400" />
                <select
                  value={selectedStatus}
                  onChange={(e) =>
                    setSelectedStatus(e.target.value === '' ? '' : Number(e.target.value))
                  }
                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Todos los estados</option>
                  <option value="1">Pendiente</option>
                  <option value="2">En Progreso</option>
                  <option value="3">Docs Requeridos</option>
                  <option value="4">En Revisión</option>
                  <option value="5">Aprobado</option>
                  <option value="6">Rechazado</option>
                  <option value="7">Expirado</option>
                </select>
              </div>
            </div>
          </div>

          {/* Profiles Table */}
          <div className="bg-white rounded-lg shadow overflow-hidden">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-3 text-gray-600">Cargando perfiles...</span>
              </div>
            ) : filteredProfiles.length === 0 ? (
              <div className="text-center py-12">
                <FiUser className="mx-auto text-gray-400 mb-4" size={48} />
                <p className="text-gray-500">No hay perfiles para mostrar</p>
              </div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Usuario
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Documento
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estado
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Riesgo
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      PEP
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Fecha
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredProfiles.map((profile) => (
                    <tr key={profile.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10 bg-gray-200 rounded-full flex items-center justify-center">
                            <FiUser className="text-gray-500" />
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {profile.fullName}
                            </div>
                            <div className="text-sm text-gray-500">{profile.email}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900">
                          {profile.primaryDocumentNumber || '-'}
                        </div>
                        <div className="text-sm text-gray-500">
                          {profile.primaryDocumentType === 1
                            ? 'Cédula'
                            : profile.primaryDocumentType === 2
                              ? 'Pasaporte'
                              : profile.primaryDocumentType === 3
                                ? 'Licencia'
                                : 'Otro'}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getStatusBadge(profile.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getRiskBadge(profile.riskLevel)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {profile.isPEP ? (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                            <FiAlertTriangle className="mr-1" size={12} />
                            PEP
                          </span>
                        ) : (
                          <span className="text-gray-400">No</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {new Date(profile.createdAt).toLocaleDateString('es-DO')}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => setSelectedProfile(profile)}
                            className="text-blue-600 hover:text-blue-900 p-2"
                            title="Ver detalles"
                          >
                            <FiEye size={18} />
                          </button>
                          {(profile.status === 4 ||
                            profile.status === 1 ||
                            profile.status === 2) && (
                            <>
                              <button
                                onClick={() => handleApprove(profile.id)}
                                className="text-green-600 hover:text-green-900 p-2"
                                title="Aprobar"
                                disabled={processing}
                              >
                                <FiCheckCircle size={18} />
                              </button>
                              <button
                                onClick={() => {
                                  setSelectedProfile(profile);
                                  setShowRejectModal(true);
                                }}
                                className="text-red-600 hover:text-red-900 p-2"
                                title="Rechazar"
                                disabled={processing}
                              >
                                <FiXCircle size={18} />
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Profile Detail Modal */}
          {selectedProfile && !showRejectModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                <div className="p-6 border-b">
                  <div className="flex justify-between items-start">
                    <div>
                      <h2 className="text-xl font-bold text-gray-900">Detalles del Perfil</h2>
                      <p className="text-gray-500">{selectedProfile.fullName}</p>
                    </div>
                    <button
                      onClick={() => setSelectedProfile(null)}
                      className="text-gray-400 hover:text-gray-600"
                    >
                      <FiXCircle size={24} />
                    </button>
                  </div>
                </div>
                <div className="p-6">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium text-gray-500">Nombre Completo</label>
                      <p className="text-gray-900">{selectedProfile.fullName}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Email</label>
                      <p className="text-gray-900">{selectedProfile.email || '-'}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Documento</label>
                      <p className="text-gray-900">
                        {selectedProfile.primaryDocumentNumber || '-'}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Fecha de Nacimiento
                      </label>
                      <p className="text-gray-900">
                        {selectedProfile.dateOfBirth
                          ? new Date(selectedProfile.dateOfBirth).toLocaleDateString('es-DO')
                          : '-'}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Dirección</label>
                      <p className="text-gray-900">{selectedProfile.address || '-'}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">
                        Ciudad / Provincia
                      </label>
                      <p className="text-gray-900">
                        {selectedProfile.city}, {selectedProfile.province}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Estado</label>
                      <p>{getStatusBadge(selectedProfile.status)}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Nivel de Riesgo</label>
                      <p>{getRiskBadge(selectedProfile.riskLevel)}</p>
                    </div>
                    {selectedProfile.isPEP && (
                      <div className="col-span-2">
                        <label className="text-sm font-medium text-gray-500">Posición PEP</label>
                        <p className="text-gray-900">
                          {selectedProfile.pepPosition || 'No especificado'}
                        </p>
                      </div>
                    )}
                    {selectedProfile.riskFactors && selectedProfile.riskFactors.length > 0 && (
                      <div className="col-span-2">
                        <label className="text-sm font-medium text-gray-500">
                          Factores de Riesgo
                        </label>
                        <div className="flex flex-wrap gap-2 mt-1">
                          {selectedProfile.riskFactors.map((factor, idx) => (
                            <span
                              key={idx}
                              className="px-2 py-1 bg-red-100 text-red-700 rounded text-sm"
                            >
                              {factor}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => setSelectedProfile(null)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cerrar
                  </button>
                  {(selectedProfile.status === 4 ||
                    selectedProfile.status === 1 ||
                    selectedProfile.status === 2) && (
                    <>
                      <button
                        onClick={() => setShowRejectModal(true)}
                        className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                        disabled={processing}
                      >
                        Rechazar
                      </button>
                      <button
                        onClick={() => handleApprove(selectedProfile.id)}
                        className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                        disabled={processing}
                      >
                        {processing ? 'Procesando...' : 'Aprobar'}
                      </button>
                    </>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Reject Modal */}
          {showRejectModal && selectedProfile && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
                <div className="p-6 border-b">
                  <h2 className="text-xl font-bold text-gray-900">Rechazar Perfil KYC</h2>
                  <p className="text-gray-500 mt-1">
                    ¿Estás seguro de rechazar el perfil de {selectedProfile.fullName}?
                  </p>
                </div>
                <div className="p-6">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Razón del rechazo *
                  </label>
                  <textarea
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                    rows={4}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-red-500"
                    placeholder="Describe la razón del rechazo..."
                  />
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => {
                      setShowRejectModal(false);
                      setRejectionReason('');
                    }}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleReject}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
                    disabled={!rejectionReason || processing}
                  >
                    {processing ? 'Procesando...' : 'Confirmar Rechazo'}
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default KYCAdminQueuePage;
