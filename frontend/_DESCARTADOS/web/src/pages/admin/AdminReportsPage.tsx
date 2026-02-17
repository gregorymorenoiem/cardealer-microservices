import { useState } from 'react';
import { FiAlertCircle, FiEye, FiCheck, FiX, FiRefreshCw } from 'react-icons/fi';
import { useModerationPage } from '@/hooks';
import type { ReportedContent } from '@/services/adminService';

type ReportStatusFilter = 'all' | 'pending' | 'reviewed' | 'resolved' | 'dismissed';

export default function AdminReportsPage() {
  const [currentPage, setCurrentPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<ReportStatusFilter>('pending');
  const [selectedReport, setSelectedReport] = useState<ReportedContent | null>(null);
  
  const filters = statusFilter !== 'all' ? { status: statusFilter } : {};
  const { reports, total, isLoading, refetch, reviewReport } = useModerationPage(currentPage, 20, filters);

  const handleReview = async (id: string, action: 'resolve' | 'dismiss') => {
    const notes = prompt(`Add notes for ${action}ing this report (optional):`);
    
    try {
      await reviewReport.mutateAsync({ reportId: id, action, notes: notes || undefined });
      refetch();
      setSelectedReport(null);
    } catch {
      alert(`Failed to ${action} report`);
    }
  };

  const getStatusBadge = (status: ReportedContent['status']) => {
    const styles = {
      pending: 'bg-yellow-100 text-yellow-800',
      reviewed: 'bg-blue-100 text-blue-800',
      resolved: 'bg-green-100 text-green-800',
      dismissed: 'bg-gray-100 text-gray-800',
    };

    return (
      <span className={`px-2 py-1 text-xs font-medium rounded-full ${styles[status]}`}>
        {status.charAt(0).toUpperCase() + status.slice(1)}
      </span>
    );
  };

  const getContentTypeBadge = (type: ReportedContent['contentType']) => {
    const styles = {
      vehicle: 'bg-purple-100 text-purple-800',
      message: 'bg-blue-100 text-blue-800',
      user: 'bg-red-100 text-red-800',
    };

    return (
      <span className={`px-2 py-1 text-xs font-medium rounded-full ${styles[type]}`}>
        {type.charAt(0).toUpperCase() + type.slice(1)}
      </span>
    );
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
            Reports
          </h1>
          <p className="text-gray-600">Review and manage reported content ({total} total)</p>
        </div>
        <button
          onClick={() => refetch()}
          disabled={isLoading}
          className="flex items-center gap-2 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
        >
          <FiRefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          Refresh
        </button>
      </div>

      {/* Stats */}
      <div className="grid md:grid-cols-4 gap-4 mb-6">
        {[
          { label: 'Pending', value: reports.filter(r => r.status === 'pending').length, color: 'yellow' },
          { label: 'Reviewed', value: reports.filter(r => r.status === 'reviewed').length, color: 'blue' },
          { label: 'Resolved', value: reports.filter(r => r.status === 'resolved').length, color: 'green' },
          { label: 'Dismissed', value: reports.filter(r => r.status === 'dismissed').length, color: 'gray' },
        ].map((stat) => (
          <div key={stat.label} className="bg-white rounded-xl shadow-card p-4">
            <p className="text-sm text-gray-600 mb-1">{stat.label}</p>
            <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-card p-4 mb-6">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-gray-700">Filter by status:</span>
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as ReportStatusFilter);
              setCurrentPage(1);
            }}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
          >
            <option value="all">All Reports</option>
            <option value="pending">Pending</option>
            <option value="reviewed">Reviewed</option>
            <option value="resolved">Resolved</option>
            <option value="dismissed">Dismissed</option>
          </select>
        </div>
      </div>

      {/* Reports List */}
      <div className="bg-white rounded-xl shadow-card overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading reports...</p>
          </div>
        ) : reports.length === 0 ? (
          <div className="p-12 text-center">
            <FiAlertCircle size={48} className="text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600">No reports found</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Report Details
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Type
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Date
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {reports.map((report) => (
                    <tr key={report.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4">
                        <div>
                          <p className="font-medium text-gray-900">{report.reason}</p>
                          <p className="text-sm text-gray-600 mt-1">
                            Reported by: {report.reporterName}
                          </p>
                          <p className="text-sm text-gray-600">
                            Reported user: {report.reportedUserName}
                          </p>
                          {report.details && (
                            <p className="text-sm text-gray-500 mt-1 italic">"{report.details}"</p>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        {getContentTypeBadge(report.contentType)}
                      </td>
                      <td className="px-6 py-4">
                        {getStatusBadge(report.status)}
                      </td>
                      <td className="px-6 py-4">
                        <p className="text-sm text-gray-900">
                          {new Date(report.createdAt).toLocaleDateString()}
                        </p>
                        <p className="text-xs text-gray-500">
                          {new Date(report.createdAt).toLocaleTimeString()}
                        </p>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => setSelectedReport(report)}
                            className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            title="View Details"
                          >
                            <FiEye size={18} />
                          </button>
                          {report.status === 'pending' && (
                            <>
                              <button
                                onClick={() => handleReview(report.id, 'resolve')}
                                className="p-2 text-green-600 hover:bg-green-50 rounded-lg transition-colors"
                                title="Resolve"
                              >
                                <FiCheck size={18} />
                              </button>
                              <button
                                onClick={() => handleReview(report.id, 'dismiss')}
                                className="p-2 text-gray-600 hover:bg-gray-50 rounded-lg transition-colors"
                                title="Dismiss"
                              >
                                <FiX size={18} />
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                <p className="text-sm text-gray-600">
                  Page {currentPage} of {totalPages}
                </p>
                <div className="flex gap-2">
                  <button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                    className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Next
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      {/* Report Detail Modal */}
      {selectedReport && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl max-w-2xl w-full max-h-[80vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">Report Details</h2>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-700">Reason</label>
                <p className="text-gray-900 mt-1">{selectedReport.reason}</p>
              </div>
              {selectedReport.details && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Details</label>
                  <p className="text-gray-900 mt-1">{selectedReport.details}</p>
                </div>
              )}
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-700">Content Type</label>
                  <p className="text-gray-900 mt-1">{selectedReport.contentType}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Status</label>
                  <div className="mt-1">{getStatusBadge(selectedReport.status)}</div>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-700">Reported By</label>
                  <p className="text-gray-900 mt-1">{selectedReport.reporterName}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700">Reported User</label>
                  <p className="text-gray-900 mt-1">{selectedReport.reportedUserName}</p>
                </div>
              </div>
              {selectedReport.resolutionNotes && (
                <div>
                  <label className="text-sm font-medium text-gray-700">Resolution Notes</label>
                  <p className="text-gray-900 mt-1">{selectedReport.resolutionNotes}</p>
                </div>
              )}
            </div>
            <div className="p-6 border-t border-gray-200 flex justify-end gap-2">
              <button
                onClick={() => setSelectedReport(null)}
                className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
              >
                Close
              </button>
              {selectedReport.status === 'pending' && (
                <>
                  <button
                    onClick={() => handleReview(selectedReport.id, 'resolve')}
                    className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                  >
                    Resolve
                  </button>
                  <button
                    onClick={() => handleReview(selectedReport.id, 'dismiss')}
                    className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
                  >
                    Dismiss
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
