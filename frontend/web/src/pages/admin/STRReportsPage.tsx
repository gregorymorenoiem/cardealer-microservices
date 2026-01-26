import React, { useState, useEffect } from 'react';
import {
  FiSearch,
  FiPlus,
  FiEye,
  FiCheckCircle,
  FiSend,
  FiClock,
  FiAlertTriangle,
  FiFileText,
  FiFilter,
  FiDownload,
  FiXCircle,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';
import kycService, { SuspiciousTransactionReport, STRStatus } from '../../services/kycService';

const STRReportsPage: React.FC = () => {
  const [reports, setReports] = useState<SuspiciousTransactionReport[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<STRStatus | ''>('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedReport, setSelectedReport] = useState<SuspiciousTransactionReport | null>(null);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showSendModal, setShowSendModal] = useState(false);
  const [uafNumber, setUafNumber] = useState('');
  const [processing, setProcessing] = useState(false);

  const [newReport, setNewReport] = useState({
    userId: '',
    suspiciousActivityType: '',
    description: '',
    amount: '',
    currency: 'DOP',
    redFlags: [] as string[],
  });

  const [stats, setStats] = useState({
    draft: 0,
    pending: 0,
    approved: 0,
    sent: 0,
    overdue: 0,
    total: 0,
  });

  const activityTypes = [
    'Estructuración de depósitos',
    'Transacciones sin justificación económica',
    'Movimientos inusuales de efectivo',
    'Transferencias a jurisdicciones de alto riesgo',
    'Cambio frecuente de billetes',
    'Uso de terceros para transacciones',
    'Inconsistencias en documentación',
    'Comportamiento evasivo del cliente',
    'Otro',
  ];

  const commonRedFlags = [
    'Structuring',
    'No justificación económica',
    'PEP involucrado',
    'País de alto riesgo',
    'Documentos inconsistentes',
    'Múltiples cuentas',
    'Transacciones rápidas',
    'Uso de efectivo inusual',
  ];

  useEffect(() => {
    loadReports();
    loadStats();
  }, [selectedStatus]);

  const loadReports = async () => {
    try {
      setLoading(true);
      const status = selectedStatus === '' ? undefined : selectedStatus;
      const result = await kycService.getSTRs({ page: 1, pageSize: 50, status });
      setReports(result.items || []);
    } catch (error) {
      console.error('Error loading STRs:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      const statsData = await kycService.getSTRStatistics();
      setStats({
        draft: statsData.draftCount || 0,
        pending: statsData.pendingCount || 0,
        approved: statsData.approvedCount || 0,
        sent: statsData.sentCount || 0,
        overdue: statsData.overdueCount || 0,
        total: statsData.totalCount || 0,
      });
    } catch (error) {
      console.error('Error loading stats:', error);
    }
  };

  const handleCreateReport = async () => {
    try {
      setProcessing(true);
      await kycService.createSTR({
        userId: newReport.userId,
        suspiciousActivityType: newReport.suspiciousActivityType,
        description: newReport.description,
        amount: parseFloat(newReport.amount),
        currency: newReport.currency,
        redFlags: newReport.redFlags,
        detectedAt: new Date().toISOString(),
      });
      setShowCreateModal(false);
      setNewReport({
        userId: '',
        suspiciousActivityType: '',
        description: '',
        amount: '',
        currency: 'DOP',
        redFlags: [],
      });
      loadReports();
      loadStats();
    } catch (error) {
      console.error('Error creating STR:', error);
      alert('Error al crear el reporte');
    } finally {
      setProcessing(false);
    }
  };

  const handleApprove = async () => {
    if (!selectedReport) return;
    try {
      setProcessing(true);
      await kycService.approveSTR(selectedReport.id);
      setShowApproveModal(false);
      setSelectedReport(null);
      loadReports();
      loadStats();
    } catch (error) {
      console.error('Error approving STR:', error);
      alert('Error al aprobar el reporte');
    } finally {
      setProcessing(false);
    }
  };

  const handleSendToUAF = async () => {
    if (!selectedReport || !uafNumber) return;
    try {
      setProcessing(true);
      await kycService.sendSTRToUAF(selectedReport.id, uafNumber);
      setShowSendModal(false);
      setSelectedReport(null);
      setUafNumber('');
      loadReports();
      loadStats();
    } catch (error) {
      console.error('Error sending to UAF:', error);
      alert('Error al enviar a la UAF');
    } finally {
      setProcessing(false);
    }
  };

  const getStatusBadge = (status: STRStatus) => {
    const statusMap: Record<STRStatus, { label: string; color: string; icon: React.ReactNode }> = {
      [STRStatus.Draft]: {
        label: 'Borrador',
        color: 'bg-gray-100 text-gray-800',
        icon: <FiFileText size={12} />,
      },
      [STRStatus.PendingReview]: {
        label: 'Pendiente Revisión',
        color: 'bg-yellow-100 text-yellow-800',
        icon: <FiClock size={12} />,
      },
      [STRStatus.Approved]: {
        label: 'Aprobado',
        color: 'bg-green-100 text-green-800',
        icon: <FiCheckCircle size={12} />,
      },
      [STRStatus.SentToUAF]: {
        label: 'Enviado a UAF',
        color: 'bg-blue-100 text-blue-800',
        icon: <FiSend size={12} />,
      },
      [STRStatus.Rejected]: {
        label: 'Rechazado',
        color: 'bg-red-100 text-red-800',
        icon: <FiXCircle size={12} />,
      },
      [STRStatus.Archived]: {
        label: 'Archivado',
        color: 'bg-gray-100 text-gray-800',
        icon: <FiFileText size={12} />,
      },
    };
    const s = statusMap[status] || {
      label: 'Desconocido',
      color: 'bg-gray-100 text-gray-800',
      icon: null,
    };
    return (
      <span
        className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${s.color}`}
      >
        {s.icon}
        {s.label}
      </span>
    );
  };

  const isOverdue = (deadline: string) => {
    return new Date(deadline) < new Date();
  };

  const toggleRedFlag = (flag: string) => {
    if (newReport.redFlags.includes(flag)) {
      setNewReport({ ...newReport, redFlags: newReport.redFlags.filter((f) => f !== flag) });
    } else {
      setNewReport({ ...newReport, redFlags: [...newReport.redFlags, flag] });
    }
  };

  const filteredReports = reports.filter(
    (r) =>
      r.reportNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      r.suspiciousActivityType?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="flex justify-between items-center mb-8">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                Reportes de Transacciones Sospechosas
              </h1>
              <p className="text-gray-600 mt-1">
                Gestión de STR/ROS según Ley 155-17 - Reportes a UAF
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => {}}
                className="flex items-center gap-2 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100"
              >
                <FiDownload size={18} />
                Exportar
              </button>
              <button
                onClick={() => setShowCreateModal(true)}
                className="flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                <FiPlus size={18} />
                Nuevo Reporte
              </button>
            </div>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-6 gap-4 mb-8">
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Total</p>
                  <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                </div>
                <FiFileText className="text-blue-500" size={28} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Borradores</p>
                  <p className="text-2xl font-bold text-gray-600">{stats.draft}</p>
                </div>
                <FiFileText className="text-gray-400" size={28} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Pendientes</p>
                  <p className="text-2xl font-bold text-yellow-600">{stats.pending}</p>
                </div>
                <FiClock className="text-yellow-500" size={28} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Aprobados</p>
                  <p className="text-2xl font-bold text-green-600">{stats.approved}</p>
                </div>
                <FiCheckCircle className="text-green-500" size={28} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Enviados UAF</p>
                  <p className="text-2xl font-bold text-blue-600">{stats.sent}</p>
                </div>
                <FiSend className="text-blue-500" size={28} />
              </div>
            </div>
            <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-500">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">Vencidos</p>
                  <p className="text-2xl font-bold text-red-600">{stats.overdue}</p>
                </div>
                <FiAlertTriangle className="text-red-500" size={28} />
              </div>
            </div>
          </div>

          {/* Warning Banner */}
          {stats.overdue > 0 && (
            <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-6 rounded-r-lg">
              <div className="flex items-center">
                <FiAlertTriangle className="text-red-500 mr-3" size={24} />
                <div>
                  <p className="font-medium text-red-800">
                    ¡Atención! Hay {stats.overdue} reporte(s) vencido(s)
                  </p>
                  <p className="text-sm text-red-700">
                    La Ley 155-17 establece un plazo máximo de 15 días hábiles para reportar a la
                    UAF.
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Filters */}
          <div className="bg-white rounded-lg shadow p-4 mb-6">
            <div className="flex flex-col md:flex-row gap-4">
              <div className="flex-1 relative">
                <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar por número de reporte o tipo de actividad..."
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
                    setSelectedStatus(
                      e.target.value === '' ? '' : (Number(e.target.value) as STRStatus)
                    )
                  }
                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Todos los estados</option>
                  <option value={STRStatus.Draft}>Borrador</option>
                  <option value={STRStatus.PendingReview}>Pendiente Revisión</option>
                  <option value={STRStatus.Approved}>Aprobado</option>
                  <option value={STRStatus.SentToUAF}>Enviado a UAF</option>
                </select>
              </div>
            </div>
          </div>

          {/* Reports Table */}
          <div className="bg-white rounded-lg shadow overflow-hidden">
            {loading ? (
              <div className="flex items-center justify-center py-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                <span className="ml-3 text-gray-600">Cargando reportes...</span>
              </div>
            ) : filteredReports.length === 0 ? (
              <div className="text-center py-12">
                <FiFileText className="mx-auto text-gray-400 mb-4" size={48} />
                <p className="text-gray-500">No hay reportes de transacciones sospechosas</p>
              </div>
            ) : (
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      # Reporte
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tipo Actividad
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Monto
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estado
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Deadline
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      # UAF
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredReports.map((report) => (
                    <tr
                      key={report.id}
                      className={`hover:bg-gray-50 ${
                        report.reportingDeadline &&
                        isOverdue(report.reportingDeadline) &&
                        report.status !== STRStatus.SentToUAF
                          ? 'bg-red-50'
                          : ''
                      }`}
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">
                          {report.reportNumber}
                        </div>
                        <div className="text-sm text-gray-500">
                          {report.createdAt
                            ? new Date(report.createdAt).toLocaleDateString('es-DO')
                            : '-'}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-900 max-w-xs truncate">
                          {report.suspiciousActivityType}
                        </div>
                        {report.redFlags && report.redFlags.length > 0 && (
                          <div className="flex gap-1 mt-1 flex-wrap">
                            {report.redFlags.slice(0, 2).map((flag, idx) => (
                              <span
                                key={idx}
                                className="px-1.5 py-0.5 bg-red-100 text-red-600 rounded text-xs"
                              >
                                {flag}
                              </span>
                            ))}
                            {report.redFlags.length > 2 && (
                              <span className="px-1.5 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">
                                +{report.redFlags.length - 2}
                              </span>
                            )}
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900">
                          {report.currency} {report.amount?.toLocaleString()}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getStatusBadge(report.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {report.reportingDeadline ? (
                          <div
                            className={`text-sm ${
                              isOverdue(report.reportingDeadline) &&
                              report.status !== STRStatus.SentToUAF
                                ? 'text-red-600 font-medium'
                                : 'text-gray-500'
                            }`}
                          >
                            {new Date(report.reportingDeadline).toLocaleDateString('es-DO')}
                            {isOverdue(report.reportingDeadline) &&
                              report.status !== STRStatus.SentToUAF && (
                                <span className="ml-2 text-red-600">⚠️ Vencido</span>
                              )}
                          </div>
                        ) : (
                          '-'
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {report.uafReportNumber || '-'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => setSelectedReport(report)}
                            className="text-blue-600 hover:text-blue-900 p-2"
                            title="Ver detalles"
                          >
                            <FiEye size={18} />
                          </button>
                          {report.status === STRStatus.PendingReview && (
                            <button
                              onClick={() => {
                                setSelectedReport(report);
                                setShowApproveModal(true);
                              }}
                              className="text-green-600 hover:text-green-900 p-2"
                              title="Aprobar"
                            >
                              <FiCheckCircle size={18} />
                            </button>
                          )}
                          {report.status === STRStatus.Approved && (
                            <button
                              onClick={() => {
                                setSelectedReport(report);
                                setShowSendModal(true);
                              }}
                              className="text-blue-600 hover:text-blue-900 p-2"
                              title="Enviar a UAF"
                            >
                              <FiSend size={18} />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          {/* Create Report Modal */}
          {showCreateModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                <div className="p-6 border-b">
                  <h2 className="text-xl font-bold text-gray-900">
                    Crear Reporte de Transacción Sospechosa
                  </h2>
                  <p className="text-sm text-gray-500 mt-1">
                    Según Ley 155-17 - Tiene 15 días hábiles para reportar a la UAF
                  </p>
                </div>
                <div className="p-6 space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      ID de Usuario Involucrado *
                    </label>
                    <input
                      type="text"
                      value={newReport.userId}
                      onChange={(e) => setNewReport({ ...newReport, userId: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                      placeholder="GUID del usuario"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Tipo de Actividad Sospechosa *
                    </label>
                    <select
                      value={newReport.suspiciousActivityType}
                      onChange={(e) =>
                        setNewReport({ ...newReport, suspiciousActivityType: e.target.value })
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                    >
                      <option value="">Seleccione un tipo</option>
                      {activityTypes.map((type) => (
                        <option key={type} value={type}>
                          {type}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Monto *
                      </label>
                      <input
                        type="number"
                        value={newReport.amount}
                        onChange={(e) => setNewReport({ ...newReport, amount: e.target.value })}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                        placeholder="0.00"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Moneda</label>
                      <select
                        value={newReport.currency}
                        onChange={(e) => setNewReport({ ...newReport, currency: e.target.value })}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                      >
                        <option value="DOP">DOP (Peso Dominicano)</option>
                        <option value="USD">USD (Dólar Americano)</option>
                        <option value="EUR">EUR (Euro)</option>
                      </select>
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Descripción Detallada *
                    </label>
                    <textarea
                      value={newReport.description}
                      onChange={(e) => setNewReport({ ...newReport, description: e.target.value })}
                      rows={4}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500"
                      placeholder="Describa detalladamente la actividad sospechosa, incluyendo patrones observados, fechas, montos involucrados..."
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Red Flags Identificadas
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {commonRedFlags.map((flag) => (
                        <button
                          key={flag}
                          type="button"
                          onClick={() => toggleRedFlag(flag)}
                          className={`px-3 py-1 rounded-full text-sm ${
                            newReport.redFlags.includes(flag)
                              ? 'bg-red-600 text-white'
                              : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                          }`}
                        >
                          {flag}
                        </button>
                      ))}
                    </div>
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => setShowCreateModal(false)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleCreateReport}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50"
                    disabled={
                      !newReport.userId ||
                      !newReport.suspiciousActivityType ||
                      !newReport.description ||
                      !newReport.amount ||
                      processing
                    }
                  >
                    {processing ? 'Creando...' : 'Crear Reporte'}
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Report Detail Modal */}
          {selectedReport && !showApproveModal && !showSendModal && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                <div className="p-6 border-b">
                  <div className="flex justify-between items-start">
                    <div>
                      <h2 className="text-xl font-bold text-gray-900">
                        Reporte {selectedReport.reportNumber}
                      </h2>
                      <p className="text-gray-500">{selectedReport.suspiciousActivityType}</p>
                    </div>
                    <button
                      onClick={() => setSelectedReport(null)}
                      className="text-gray-400 hover:text-gray-600"
                    >
                      ✕
                    </button>
                  </div>
                </div>
                <div className="p-6">
                  <div className="grid grid-cols-2 gap-4 mb-6">
                    <div>
                      <label className="text-sm font-medium text-gray-500">Estado</label>
                      <p>{getStatusBadge(selectedReport.status)}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Monto</label>
                      <p className="text-gray-900 font-medium">
                        {selectedReport.currency} {selectedReport.amount?.toLocaleString()}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Fecha Detección</label>
                      <p className="text-gray-900">
                        {selectedReport.detectedAt
                          ? new Date(selectedReport.detectedAt).toLocaleDateString('es-DO')
                          : '-'}
                      </p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-gray-500">Fecha Límite UAF</label>
                      <p
                        className={`font-medium ${
                          selectedReport.reportingDeadline &&
                          isOverdue(selectedReport.reportingDeadline) &&
                          selectedReport.status !== STRStatus.SentToUAF
                            ? 'text-red-600'
                            : 'text-gray-900'
                        }`}
                      >
                        {selectedReport.reportingDeadline
                          ? new Date(selectedReport.reportingDeadline).toLocaleDateString('es-DO')
                          : '-'}
                      </p>
                    </div>
                    {selectedReport.uafReportNumber && (
                      <div>
                        <label className="text-sm font-medium text-gray-500"># Reporte UAF</label>
                        <p className="text-gray-900">{selectedReport.uafReportNumber}</p>
                      </div>
                    )}
                    {selectedReport.sentToUafAt && (
                      <div>
                        <label className="text-sm font-medium text-gray-500">Enviado a UAF</label>
                        <p className="text-gray-900">
                          {new Date(selectedReport.sentToUafAt).toLocaleDateString('es-DO')}
                        </p>
                      </div>
                    )}
                  </div>

                  <div className="mb-6">
                    <label className="text-sm font-medium text-gray-500 block mb-2">
                      Descripción
                    </label>
                    <p className="text-gray-900 bg-gray-50 p-4 rounded-lg">
                      {selectedReport.description}
                    </p>
                  </div>

                  {selectedReport.redFlags && selectedReport.redFlags.length > 0 && (
                    <div>
                      <label className="text-sm font-medium text-gray-500 block mb-2">
                        Red Flags
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {selectedReport.redFlags.map((flag, idx) => (
                          <span
                            key={idx}
                            className="px-3 py-1 bg-red-100 text-red-700 rounded-full text-sm"
                          >
                            {flag}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => setSelectedReport(null)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cerrar
                  </button>
                  {selectedReport.status === STRStatus.PendingReview && (
                    <button
                      onClick={() => setShowApproveModal(true)}
                      className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                    >
                      Aprobar
                    </button>
                  )}
                  {selectedReport.status === STRStatus.Approved && (
                    <button
                      onClick={() => setShowSendModal(true)}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                      Enviar a UAF
                    </button>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Approve Modal */}
          {showApproveModal && selectedReport && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
                <div className="p-6 border-b">
                  <h2 className="text-xl font-bold text-gray-900">Aprobar Reporte</h2>
                </div>
                <div className="p-6">
                  <p className="text-gray-700">
                    ¿Está seguro de aprobar el reporte{' '}
                    <strong>{selectedReport.reportNumber}</strong> para su envío a la UAF?
                  </p>
                  <div className="mt-4 p-4 bg-yellow-50 rounded-lg">
                    <p className="text-sm text-yellow-800">
                      <FiAlertTriangle className="inline mr-2" />
                      Una vez aprobado, el reporte deberá ser enviado a la UAF dentro del plazo
                      establecido.
                    </p>
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => setShowApproveModal(false)}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleApprove}
                    className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50"
                    disabled={processing}
                  >
                    {processing ? 'Aprobando...' : 'Confirmar Aprobación'}
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Send to UAF Modal */}
          {showSendModal && selectedReport && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
                <div className="p-6 border-b">
                  <h2 className="text-xl font-bold text-gray-900">Enviar a UAF</h2>
                </div>
                <div className="p-6">
                  <p className="text-gray-700 mb-4">
                    Registre el número de reporte asignado por la UAF para el reporte{' '}
                    <strong>{selectedReport.reportNumber}</strong>.
                  </p>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Número de Reporte UAF *
                    </label>
                    <input
                      type="text"
                      value={uafNumber}
                      onChange={(e) => setUafNumber(e.target.value)}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="UAF-2026-XXXXX"
                    />
                  </div>
                </div>
                <div className="p-6 border-t bg-gray-50 flex justify-end gap-3">
                  <button
                    onClick={() => {
                      setShowSendModal(false);
                      setUafNumber('');
                    }}
                    className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleSendToUAF}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
                    disabled={!uafNumber || processing}
                  >
                    {processing ? 'Enviando...' : 'Confirmar Envío'}
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

export default STRReportsPage;
