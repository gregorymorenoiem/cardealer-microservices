import { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  FiArrowLeft, 
  FiSearch,
  FiFilter,
  FiCheckCircle,
  FiXCircle,
  FiRefreshCw,
  FiChevronLeft,
  FiChevronRight,
  FiExternalLink
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import { usePayments } from '@/hooks/useBilling';
import { useAuthStore } from '@/store/authStore';
import { mockPayments, formatCurrency, getStatusColor } from '@/mocks/billingData';
import type { PaymentStatus } from '@/types/billing';

export default function PaymentsPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<PaymentStatus | 'all'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 10;

  // Get user info
  const { user } = useAuthStore();
  const dealerId = user?.dealerId || user?.id || '';
  
  // Use TanStack Query hook
  const { data: fetchedPayments } = usePayments(dealerId);
  
  // Use fetched data or fallback to mocks
  const payments = fetchedPayments?.length ? fetchedPayments : mockPayments;

  const filteredPayments = payments.filter((payment) => {
    const matchesSearch = 
      payment.description?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      payment.cardLast4?.includes(searchQuery) ||
      formatCurrency(payment.amount).includes(searchQuery);
    
    const matchesStatus = statusFilter === 'all' || payment.status === statusFilter;
    
    return matchesSearch && matchesStatus;
  });

  const totalPages = Math.ceil(filteredPayments.length / itemsPerPage);
  const paginatedPayments = filteredPayments.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const statusOptions: { value: PaymentStatus | 'all'; label: string }[] = [
    { value: 'all', label: 'All Statuses' },
    { value: 'succeeded', label: 'Succeeded' },
    { value: 'pending', label: 'Pending' },
    { value: 'processing', label: 'Processing' },
    { value: 'failed', label: 'Failed' },
    { value: 'refunded', label: 'Refunded' },
    { value: 'partially_refunded', label: 'Partially Refunded' },
    { value: 'disputed', label: 'Disputed' },
  ];

  const getStatusIcon = (status: PaymentStatus) => {
    switch (status) {
      case 'succeeded':
        return <FiCheckCircle className="w-5 h-5 text-green-500" />;
      case 'failed':
        return <FiXCircle className="w-5 h-5 text-red-500" />;
      case 'pending':
      case 'processing':
        return <FiRefreshCw className="w-5 h-5 text-blue-500 animate-spin" />;
      case 'refunded':
      case 'partially_refunded':
        return <FiRefreshCw className="w-5 h-5 text-purple-500" />;
      default:
        return <FiCheckCircle className="w-5 h-5 text-gray-400" />;
    }
  };

  const totalSuccessful = filteredPayments
    .filter(p => p.status === 'succeeded')
    .reduce((sum, p) => sum + p.amount, 0);
  
  const totalRefunded = filteredPayments
    .reduce((sum, p) => sum + p.refundedAmount, 0);

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <Link to="/billing" className="inline-flex items-center text-gray-600 hover:text-gray-900 mb-4">
              <FiArrowLeft className="w-4 h-4 mr-2" />
              Back to Billing
            </Link>
            <h1 className="text-3xl font-bold text-gray-900">Payment History</h1>
            <p className="text-gray-600 mt-1">
              View all your payment transactions
            </p>
          </div>

          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <p className="text-sm text-gray-500">Total Payments</p>
              <p className="text-2xl font-bold text-gray-900">{filteredPayments.length}</p>
              <p className="text-sm text-gray-500 mt-1">All time transactions</p>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <p className="text-sm text-gray-500">Successful Payments</p>
              <p className="text-2xl font-bold text-green-600">{formatCurrency(totalSuccessful)}</p>
              <p className="text-sm text-gray-500 mt-1">
                {filteredPayments.filter(p => p.status === 'succeeded').length} transactions
              </p>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <p className="text-sm text-gray-500">Total Refunded</p>
              <p className="text-2xl font-bold text-purple-600">{formatCurrency(totalRefunded)}</p>
              <p className="text-sm text-gray-500 mt-1">
                {filteredPayments.filter(p => p.refundedAmount > 0).length} refunds
              </p>
            </motion.div>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-xl shadow-sm p-4 mb-6">
            <div className="flex flex-col sm:flex-row gap-4">
              <div className="relative flex-1">
                <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search payments..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
              <div className="relative">
                <FiFilter className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <select
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value as PaymentStatus | 'all')}
                  className="pl-10 pr-10 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent appearance-none bg-white"
                >
                  {statusOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Payments List */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="bg-white rounded-xl shadow-sm overflow-hidden"
          >
            <div className="divide-y divide-gray-100">
              {paginatedPayments.map((payment) => (
                <div key={payment.id} className="p-6 hover:bg-gray-50">
                  <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center">
                        {getStatusIcon(payment.status)}
                      </div>
                      <div>
                        <div className="flex items-center gap-2">
                          <p className="font-medium text-gray-900">
                            {payment.cardBrand} •••• {payment.cardLast4}
                          </p>
                          <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full capitalize ${getStatusColor(payment.status)}`}>
                            {payment.status.replace('_', ' ')}
                          </span>
                        </div>
                        <p className="text-sm text-gray-500 mt-0.5">{payment.description}</p>
                        <p className="text-xs text-gray-400 mt-1">{formatDate(payment.createdAt)}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-6 sm:ml-auto">
                      <div className="text-right">
                        <p className="text-lg font-semibold text-gray-900">
                          {formatCurrency(payment.amount)}
                        </p>
                        {payment.refundedAmount > 0 && (
                          <p className="text-sm text-purple-600">
                            -{formatCurrency(payment.refundedAmount)} refunded
                          </p>
                        )}
                      </div>
                      
                      {payment.receiptUrl && (
                        <a
                          href={payment.receiptUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg"
                          title="View Receipt"
                        >
                          <FiExternalLink className="w-5 h-5" />
                        </a>
                      )}
                    </div>
                  </div>

                  {/* Failure reason */}
                  {payment.status === 'failed' && payment.failureReason && (
                    <div className="mt-3 p-3 bg-red-50 rounded-lg">
                      <p className="text-sm text-red-800">
                        <strong>Failure Reason:</strong> {payment.failureReason}
                      </p>
                    </div>
                  )}

                  {/* Refund reason */}
                  {(payment.status === 'refunded' || payment.status === 'partially_refunded') && payment.refundReason && (
                    <div className="mt-3 p-3 bg-purple-50 rounded-lg">
                      <p className="text-sm text-purple-800">
                        <strong>Refund Reason:</strong> {payment.refundReason}
                      </p>
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100">
                <p className="text-sm text-gray-500">
                  Showing {(currentPage - 1) * itemsPerPage + 1} to{' '}
                  {Math.min(currentPage * itemsPerPage, filteredPayments.length)} of{' '}
                  {filteredPayments.length} payments
                </p>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="p-2 text-gray-400 hover:text-gray-600 disabled:opacity-50"
                  >
                    <FiChevronLeft className="w-5 h-5" />
                  </button>
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                    <button
                      key={page}
                      onClick={() => setCurrentPage(page)}
                      className={`w-8 h-8 rounded-lg text-sm font-medium ${
                        currentPage === page
                          ? 'bg-primary-500 text-white'
                          : 'text-gray-600 hover:bg-gray-100'
                      }`}
                    >
                      {page}
                    </button>
                  ))}
                  <button
                    onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                    className="p-2 text-gray-400 hover:text-gray-600 disabled:opacity-50"
                  >
                    <FiChevronRight className="w-5 h-5" />
                  </button>
                </div>
              </div>
            )}
          </motion.div>

          {/* Empty State */}
          {paginatedPayments.length === 0 && (
            <div className="text-center py-12 bg-white rounded-xl shadow-sm">
              <FiCheckCircle className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900">No payments found</h3>
              <p className="text-gray-500 mt-1">
                {searchQuery || statusFilter !== 'all'
                  ? 'Try adjusting your filters'
                  : 'Your payment history will appear here'}
              </p>
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
}
