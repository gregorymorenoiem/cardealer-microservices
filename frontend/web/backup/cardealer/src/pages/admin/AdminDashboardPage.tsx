import { useTranslation } from 'react-i18next';
import { mockAdminStats, mockActivityLogs } from '../../data/mockAdmin';
import type { ActivityLog } from '../../types/admin';
import { FiPackage, FiUsers, FiDollarSign, FiTrendingUp, FiEye, FiMessageSquare } from 'react-icons/fi';

const AdminDashboardPage = () => {
  const { t } = useTranslation(['admin', 'common']);
  const stats = mockAdminStats;
  const recentActivity = mockActivityLogs.slice(0, 5);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(amount);
  };

  const formatNumber = (num: number) => {
    return new Intl.NumberFormat('en-US').format(num);
  };

  const formatTimeAgo = (timestamp: string) => {
    const now = new Date();
    const date = new Date(timestamp);
    const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (seconds < 60) return 'just now';
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'error':
        return 'text-red-600 bg-red-50';
      case 'warning':
        return 'text-yellow-600 bg-yellow-50';
      default:
        return 'text-blue-600 bg-blue-50';
    }
  };

  return (
    <div>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">{t('admin:dashboard.title')}</h1>
        <p className="text-gray-600 mt-1">{t('admin:dashboard.subtitle')}</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        {/* Pending Approvals */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Pending Approvals</p>
              <p className="text-3xl font-bold text-gray-900">{stats.pendingApprovals}</p>
              <p className="text-sm text-orange-600 mt-2">Requires attention</p>
            </div>
            <div className="w-12 h-12 bg-orange-100 rounded-lg flex items-center justify-center">
              <FiPackage className="w-6 h-6 text-orange-600" />
            </div>
          </div>
        </div>

        {/* Active Listings */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Active Listings</p>
              <p className="text-3xl font-bold text-gray-900">{formatNumber(stats.activeListings)}</p>
              <p className="text-sm text-green-600 mt-2">+12% this month</p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <FiTrendingUp className="w-6 h-6 text-green-600" />
            </div>
          </div>
        </div>

        {/* Total Users */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Users</p>
              <p className="text-3xl font-bold text-gray-900">{formatNumber(stats.totalUsers)}</p>
              <p className="text-sm text-blue-600 mt-2">+8% this month</p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <FiUsers className="w-6 h-6 text-blue-600" />
            </div>
          </div>
        </div>

        {/* Total Revenue */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Revenue</p>
              <p className="text-3xl font-bold text-gray-900">{formatCurrency(stats.totalRevenue)}</p>
              <p className="text-sm text-green-600 mt-2">+15% this month</p>
            </div>
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <FiDollarSign className="w-6 h-6 text-purple-600" />
            </div>
          </div>
        </div>

        {/* Today's Views */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Today's Views</p>
              <p className="text-3xl font-bold text-gray-900">{formatNumber(stats.todayViews)}</p>
              <p className="text-sm text-gray-500 mt-2">Last 24 hours</p>
            </div>
            <div className="w-12 h-12 bg-indigo-100 rounded-lg flex items-center justify-center">
              <FiEye className="w-6 h-6 text-indigo-600" />
            </div>
          </div>
        </div>

        {/* Today's Inquiries */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Today's Inquiries</p>
              <p className="text-3xl font-bold text-gray-900">{formatNumber(stats.todayInquiries)}</p>
              <p className="text-sm text-gray-500 mt-2">Last 24 hours</p>
            </div>
            <div className="w-12 h-12 bg-pink-100 rounded-lg flex items-center justify-center">
              <FiMessageSquare className="w-6 h-6 text-pink-600" />
            </div>
          </div>
        </div>
      </div>

        {/* Recent Activity */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">{t('admin:dashboard.recentActivity')}</h2>
        </div>
        <div className="divide-y divide-gray-200">
          {recentActivity.length === 0 ? (
            <div className="px-6 py-8 text-center text-gray-500">
              <p>{t('admin:dashboard.noActivity')}</p>
            </div>
          ) : (
            recentActivity.map((activity: ActivityLog) => (
              <div key={activity.id} className="px-6 py-4 hover:bg-gray-50 transition-colors">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <span
                        className={`px-2 py-1 rounded text-xs font-medium ${getSeverityColor(
                          activity.severity
                        )}`}
                      >
                        {activity.type.toUpperCase()}
                      </span>
                      <span className="font-medium text-gray-900">{activity.action}</span>
                    </div>
                    <p className="text-sm text-gray-600">{activity.description}</p>
                    {activity.userName && (
                      <p className="text-xs text-gray-500 mt-1">by {activity.userName}</p>
                    )}
                  </div>
                  <span className="text-sm text-gray-400 whitespace-nowrap ml-4">
                    {formatTimeAgo(activity.timestamp)}
                  </span>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* Quick Actions */}
      <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl shadow-lg p-6 text-white">
          <h3 className="text-lg font-semibold mb-2">Pending Approvals</h3>
          <p className="text-blue-100 mb-4">
            {stats.pendingApprovals} listings waiting for review
          </p>
          <a
            href="/admin/pending"
            className="inline-block bg-white text-blue-600 px-4 py-2 rounded-lg font-medium hover:bg-blue-50 transition-colors"
          >
            Review Now
          </a>
        </div>

        <div className="bg-gradient-to-br from-purple-500 to-purple-600 rounded-xl shadow-lg p-6 text-white">
          <h3 className="text-lg font-semibold mb-2">User Management</h3>
          <p className="text-purple-100 mb-4">
            Manage {stats.totalUsers} registered users
          </p>
          <a
            href="/admin/users"
            className="inline-block bg-white text-purple-600 px-4 py-2 rounded-lg font-medium hover:bg-purple-50 transition-colors"
          >
            Manage Users
          </a>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboardPage;
