import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { 
  FiCreditCard, 
  FiFileText, 
  FiTrendingUp, 
  FiSettings,
  FiArrowUpRight,
  FiAlertCircle,
  FiCheckCircle,
  FiClock,
  FiDownload,
  FiExternalLink
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import {
  mockSubscription,
  mockInvoices,
  mockPayments,
  mockUsageMetrics,
  mockBillingStats,
  formatCurrency,
  getStatusColor,
  getPlanById,
} from '@/mocks/billingData';

export default function BillingDashboardPage() {
  const { t, i18n } = useTranslation('billing');
  const currentPlan = getPlanById(mockSubscription.plan);
  const recentInvoices = mockInvoices.slice(0, 3);
  const recentPayments = mockPayments.slice(0, 3);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(i18n.language === 'es' ? 'es-MX' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const getDaysUntilBilling = () => {
    const next = new Date(mockSubscription.nextBillingDate || '');
    const now = new Date();
    const diff = Math.ceil((next.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    return diff;
  };

  const usagePercentage = (current: number, max: number | 'unlimited') => {
    if (max === 'unlimited') return 0;
    return Math.min((current / max) * 100, 100);
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">{t('dashboard.title')}</h1>
            <p className="text-gray-600 mt-2">
              {t('dashboard.subtitle')}
            </p>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{t('dashboard.currentPlan')}</p>
                  <p className="text-2xl font-bold text-gray-900 capitalize">
                    {mockSubscription.plan}
                  </p>
                </div>
                <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center">
                  <FiCreditCard className="w-6 h-6 text-primary-600" />
                </div>
              </div>
              <div className="mt-3">
                <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(mockSubscription.status)}`}>
                  {mockSubscription.status}
                </span>
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{t('dashboard.nextBilling')}</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(mockBillingStats.nextBillingAmount)}
                  </p>
                </div>
                <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                  <FiClock className="w-6 h-6 text-blue-600" />
                </div>
              </div>
              <p className="mt-3 text-sm text-gray-500">
                {t('dashboard.inDays', { days: getDaysUntilBilling() })}
              </p>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{t('dashboard.totalPaidYTD')}</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(mockBillingStats.totalPaid)}
                  </p>
                </div>
                <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                  <FiTrendingUp className="w-6 h-6 text-green-600" />
                </div>
              </div>
              <p className="mt-3 text-sm text-gray-500">
                {t('dashboard.invoicesCount', { count: mockBillingStats.invoiceCount })}
              </p>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{t('dashboard.outstanding')}</p>
                  <p className="text-2xl font-bold text-gray-900">
                    {formatCurrency(mockBillingStats.outstandingBalance)}
                  </p>
                </div>
                <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
                  <FiAlertCircle className="w-6 h-6 text-yellow-600" />
                </div>
              </div>
              <Button variant="ghost" size="sm" className="mt-2 text-primary-600">
                {t('dashboard.payNow')}
              </Button>
            </motion.div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Main Content */}
            <div className="lg:col-span-2 space-y-8">
              {/* Current Plan Card */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.4 }}
                className="bg-white rounded-xl shadow-sm overflow-hidden"
              >
                <div className="p-6 bg-gradient-to-r from-primary-600 to-primary-700 text-white">
                  <div className="flex items-center justify-between">
                    <div>
                      <h2 className="text-lg font-semibold">
                        {t('dashboard.plan', { name: currentPlan?.name })}
                      </h2>
                      <p className="text-primary-100 text-sm">
                        {formatCurrency(mockSubscription.pricePerCycle)}/{mockSubscription.cycle}
                      </p>
                    </div>
                    <Link to="/billing/plans">
                      <Button variant="secondary" size="sm">
                        <FiArrowUpRight className="w-4 h-4 mr-1" />
                        {t('dashboard.upgrade')}
                      </Button>
                    </Link>
                  </div>
                </div>
                
                <div className="p-6">
                  <h3 className="font-medium text-gray-900 mb-4">{t('dashboard.usageThisPeriod')}</h3>
                  
                  {/* Listings Usage */}
                  <div className="mb-4">
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-gray-600">{t('dashboard.activeListings')}</span>
                      <span className="font-medium">
                        {mockUsageMetrics.currentListings} / {mockUsageMetrics.maxListings}
                      </span>
                    </div>
                    <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-primary-500 rounded-full transition-all"
                        style={{ 
                          width: `${usagePercentage(mockUsageMetrics.currentListings, mockUsageMetrics.maxListings)}%` 
                        }}
                      />
                    </div>
                  </div>

                  {/* Users Usage */}
                  <div className="mb-4">
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-gray-600">{t('dashboard.teamMembers')}</span>
                      <span className="font-medium">
                        {mockUsageMetrics.currentUsers} / {mockUsageMetrics.maxUsers}
                      </span>
                    </div>
                    <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-blue-500 rounded-full transition-all"
                        style={{ 
                          width: `${usagePercentage(mockUsageMetrics.currentUsers, mockUsageMetrics.maxUsers)}%` 
                        }}
                      />
                    </div>
                  </div>

                  {/* Storage Usage */}
                  <div className="mb-4">
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-gray-600">{t('dashboard.storage')}</span>
                      <span className="font-medium">
                        {mockUsageMetrics.storageUsed} / {mockUsageMetrics.storageLimit}
                      </span>
                    </div>
                    <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-green-500 rounded-full transition-all"
                        style={{ width: '32%' }}
                      />
                    </div>
                  </div>

                  {/* API Calls */}
                  <div>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-gray-600">{t('dashboard.apiCalls')}</span>
                      <span className="font-medium">
                        {mockUsageMetrics.apiCalls.toLocaleString()} / {
                          mockUsageMetrics.apiLimit === 'unlimited' 
                            ? '∞' 
                            : mockUsageMetrics.apiLimit.toLocaleString()
                        }
                      </span>
                    </div>
                    <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-purple-500 rounded-full transition-all"
                        style={{ 
                          width: `${usagePercentage(mockUsageMetrics.apiCalls, mockUsageMetrics.apiLimit)}%` 
                        }}
                      />
                    </div>
                  </div>
                </div>
              </motion.div>

              {/* Recent Invoices */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.5 }}
                className="bg-white rounded-xl shadow-sm"
              >
                <div className="p-6 border-b border-gray-100">
                  <div className="flex items-center justify-between">
                    <h2 className="text-lg font-semibold text-gray-900">{t('dashboard.recentInvoices')}</h2>
                    <Link to="/billing/invoices" className="text-sm text-primary-600 hover:text-primary-700">
                      {t('dashboard.viewAll')}
                    </Link>
                  </div>
                </div>
                <div className="divide-y divide-gray-100">
                  {recentInvoices.map((invoice) => (
                    <div key={invoice.id} className="p-4 flex items-center justify-between hover:bg-gray-50">
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 bg-gray-100 rounded-lg flex items-center justify-center">
                          <FiFileText className="w-5 h-5 text-gray-600" />
                        </div>
                        <div>
                          <p className="font-medium text-gray-900">{invoice.invoiceNumber}</p>
                          <p className="text-sm text-gray-500">{formatDate(invoice.issueDate)}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <div className="text-right">
                          <p className="font-medium text-gray-900">
                            {formatCurrency(invoice.totalAmount)}
                          </p>
                          <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${getStatusColor(invoice.status)}`}>
                            {invoice.status.replace('_', ' ')}
                          </span>
                        </div>
                        {invoice.pdfUrl && (
                          <button className="p-2 text-gray-400 hover:text-gray-600">
                            <FiDownload className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </motion.div>

              {/* Recent Payments */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.6 }}
                className="bg-white rounded-xl shadow-sm"
              >
                <div className="p-6 border-b border-gray-100">
                  <div className="flex items-center justify-between">
                    <h2 className="text-lg font-semibold text-gray-900">{t('dashboard.recentPayments')}</h2>
                    <Link to="/billing/payments" className="text-sm text-primary-600 hover:text-primary-700">
                      {t('dashboard.viewAll')}
                    </Link>
                  </div>
                </div>
                <div className="divide-y divide-gray-100">
                  {recentPayments.map((payment) => (
                    <div key={payment.id} className="p-4 flex items-center justify-between hover:bg-gray-50">
                      <div className="flex items-center gap-4">
                        <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${
                          payment.status === 'succeeded' ? 'bg-green-100' : 'bg-gray-100'
                        }`}>
                          <FiCheckCircle className={`w-5 h-5 ${
                            payment.status === 'succeeded' ? 'text-green-600' : 'text-gray-600'
                          }`} />
                        </div>
                        <div>
                          <p className="font-medium text-gray-900">
                            {payment.cardBrand} •••• {payment.cardLast4}
                          </p>
                          <p className="text-sm text-gray-500">{formatDate(payment.createdAt)}</p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-medium text-gray-900">
                          {formatCurrency(payment.amount)}
                        </p>
                        <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${getStatusColor(payment.status)}`}>
                          {payment.status}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </motion.div>
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Quick Actions */}
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.4 }}
                className="bg-white rounded-xl shadow-sm p-6"
              >
                <h3 className="font-semibold text-gray-900 mb-4">{t('dashboard.quickActions')}</h3>
                <div className="space-y-3">
                  <Link to="/billing/plans">
                    <Button variant="outline" className="w-full justify-start">
                      <FiArrowUpRight className="w-4 h-4 mr-2" />
                      {t('dashboard.upgradePlan')}
                    </Button>
                  </Link>
                  <Link to="/billing/payment-methods">
                    <Button variant="outline" className="w-full justify-start">
                      <FiCreditCard className="w-4 h-4 mr-2" />
                      {t('dashboard.paymentMethods')}
                    </Button>
                  </Link>
                  <Link to="/billing/invoices">
                    <Button variant="outline" className="w-full justify-start">
                      <FiFileText className="w-4 h-4 mr-2" />
                      {t('dashboard.viewInvoices')}
                    </Button>
                  </Link>
                  <Link to="/billing/settings">
                    <Button variant="outline" className="w-full justify-start">
                      <FiSettings className="w-4 h-4 mr-2" />
                      {t('dashboard.billingSettings')}
                    </Button>
                  </Link>
                </div>
              </motion.div>

              {/* Billing Info */}
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.5 }}
                className="bg-white rounded-xl shadow-sm p-6"
              >
                <h3 className="font-semibold text-gray-900 mb-4">{t('dashboard.billingInfo')}</h3>
                <div className="space-y-3 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('dashboard.billingCycle')}</span>
                    <span className="font-medium capitalize">{mockSubscription.cycle}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('dashboard.startDate')}</span>
                    <span className="font-medium">{formatDate(mockSubscription.startDate)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('dashboard.nextBilling')}</span>
                    <span className="font-medium">{formatDate(mockSubscription.nextBillingDate || '')}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('dashboard.currency')}</span>
                    <span className="font-medium">{mockSubscription.currency}</span>
                  </div>
                </div>
              </motion.div>

              {/* Need Help */}
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.6 }}
                className="bg-gradient-to-br from-gray-800 to-gray-900 rounded-xl shadow-sm p-6 text-white"
              >
                <h3 className="font-semibold mb-2">{t('dashboard.needHelp')}</h3>
                <p className="text-gray-300 text-sm mb-4">
                  {t('dashboard.supportAvailable')}
                </p>
                <Button variant="secondary" size="sm" className="w-full">
                  <FiExternalLink className="w-4 h-4 mr-2" />
                  {t('dashboard.contactSupport')}
                </Button>
              </motion.div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
