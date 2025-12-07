/**
 * SellerDashboardPage - Dashboard for sellers to manage their listings
 */

import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import type { Listing, MarketplaceCategorySelection } from '@/types/marketplace';
import {
  PlusIcon,
  ChartBarIcon,
  EyeIcon,
  HeartIcon,
  ChatBubbleLeftRightIcon,
  CurrencyDollarIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  PencilIcon,
  TrashIcon,
  ArchiveBoxIcon,
  SparklesIcon,
} from '@heroicons/react/24/outline';

// Mock seller data
const mockSellerStats = {
  totalListings: 12,
  activeListings: 8,
  pendingListings: 2,
  soldListings: 15,
  totalViews: 4523,
  totalFavorites: 234,
  totalInquiries: 89,
  viewsChange: 12.5,
  favoritesChange: -3.2,
  inquiriesChange: 8.7,
};

const mockSellerListings: Listing[] = [
  {
    id: 'seller-v1',
    vertical: 'vehicles',
    title: '2022 Toyota RAV4 XLE Premium',
    description: 'Excelente SUV familiar con bajo kilometraje.',
    price: 425000,
    currency: 'MXN',
    status: 'active',
    isFeatured: true,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/3b82f6/white?text=RAV4', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/3b82f6/white?text=RAV4',
    location: { address: '', city: 'CDMX', state: 'CDMX', zipCode: '', country: 'México' },
    seller: { id: '1', name: 'Mi Dealer', isVerified: true, isDealership: true },
    viewCount: 342,
    favoriteCount: 28,
    inquiryCount: 12,
    createdAt: '2024-01-15T00:00:00Z',
    updatedAt: '2024-01-20T00:00:00Z',
  } as Listing,
  {
    id: 'seller-p1',
    vertical: 'real-estate',
    title: 'Casa moderna en Polanco',
    description: 'Espectacular casa moderna con acabados de lujo.',
    price: 12500000,
    currency: 'MXN',
    status: 'active',
    isFeatured: false,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/10b981/white?text=Casa', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/10b981/white?text=Casa',
    location: { address: '', city: 'CDMX', state: 'CDMX', zipCode: '', country: 'México' },
    seller: { id: '1', name: 'Mi Dealer', isVerified: true, isDealership: true },
    viewCount: 567,
    favoriteCount: 45,
    inquiryCount: 8,
    createdAt: '2024-01-10T00:00:00Z',
    updatedAt: '2024-01-18T00:00:00Z',
  } as Listing,
  {
    id: 'seller-v2',
    vertical: 'vehicles',
    title: '2021 Honda Civic Sport',
    description: 'Sedán deportivo en perfectas condiciones.',
    price: 385000,
    currency: 'MXN',
    status: 'pending',
    isFeatured: false,
    images: [
      { id: '1', url: 'https://placehold.co/600x400/6366f1/white?text=Civic', isPrimary: true, sortOrder: 0 },
    ],
    primaryImageUrl: 'https://placehold.co/600x400/6366f1/white?text=Civic',
    location: { address: '', city: 'Monterrey', state: 'NL', zipCode: '', country: 'México' },
    seller: { id: '1', name: 'Mi Dealer', isVerified: true, isDealership: true },
    viewCount: 0,
    favoriteCount: 0,
    inquiryCount: 0,
    createdAt: '2024-01-20T00:00:00Z',
    updatedAt: '2024-01-20T00:00:00Z',
  } as Listing,
];

const SellerDashboardPage: React.FC = () => {
  const { t } = useTranslation(['dealer', 'common']);
  const [selectedTab, setSelectedTab] = useState<'active' | 'pending' | 'sold' | 'draft'>('active');
  const [selectedCategory, setSelectedCategory] = useState<MarketplaceCategorySelection>('all');
  const stats = mockSellerStats;
  const listings = mockSellerListings;

  const filteredListings = listings.filter((listing) => {
    // Filter by status
    if (selectedTab === 'active' && listing.status !== 'active') return false;
    if (selectedTab === 'pending' && listing.status !== 'pending') return false;
    if (selectedTab === 'sold' && listing.status !== 'sold') return false;
    if (selectedTab === 'draft' && listing.status !== 'draft') return false;
    
    // Filter by category
    if (selectedCategory !== 'all' && listing.vertical !== selectedCategory) return false;
    
    return true;
  });

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              {t('dealer:sellerDashboard.title')}
            </h1>
            <p className="text-gray-500 mt-1">
              {t('dealer:sellerDashboard.subtitle')}
            </p>
          </div>
          <Link
            to="/marketplace/listings/new"
            className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            <PlusIcon className="h-5 w-5" />
            {t('dealer:sellerDashboard.newListing')}
          </Link>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <StatCard
            title={t('dealer:sellerDashboard.stats.activeListings')}
            value={stats.activeListings}
            icon={<ChartBarIcon className="h-5 w-5" />}
            color="blue"
          />
          <StatCard
            title={t('dealer:sellerDashboard.stats.totalViews')}
            value={stats.totalViews}
            icon={<EyeIcon className="h-5 w-5" />}
            change={stats.viewsChange}
            color="purple"
          />
          <StatCard
            title={t('dealer:sellerDashboard.stats.favorites')}
            value={stats.totalFavorites}
            icon={<HeartIcon className="h-5 w-5" />}
            change={stats.favoritesChange}
            color="pink"
          />
          <StatCard
            title={t('dealer:sellerDashboard.stats.inquiries')}
            value={stats.totalInquiries}
            icon={<ChatBubbleLeftRightIcon className="h-5 w-5" />}
            change={stats.inquiriesChange}
            color="green"
          />
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
          <QuickActionCard
            title={t('dealer:sellerDashboard.quickActions.featureListing')}
            description={t('dealer:sellerDashboard.quickActions.increaseVisibility')}
            icon={<SparklesIcon className="h-6 w-6" />}
            color="amber"
          />
          <QuickActionCard
            title={t('dealer:sellerDashboard.quickActions.viewMessages')}
            description={t('dealer:sellerDashboard.quickActions.unread', { count: stats.totalInquiries })}
            icon={<ChatBubbleLeftRightIcon className="h-6 w-6" />}
            color="blue"
          />
          <QuickActionCard
            title={t('dealer:sellerDashboard.quickActions.viewStats')}
            description={t('dealer:sellerDashboard.quickActions.detailedAnalysis')}
            icon={<ChartBarIcon className="h-6 w-6" />}
            color="purple"
          />
          <QuickActionCard
            title={t('dealer:sellerDashboard.quickActions.salesHistory')}
            description={t('dealer:sellerDashboard.quickActions.soldCount', { count: stats.soldListings })}
            icon={<CurrencyDollarIcon className="h-6 w-6" />}
            color="green"
          />
        </div>

        {/* Listings Section */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200">
          {/* Tabs */}
          <div className="flex flex-wrap items-center gap-2 p-4 border-b border-gray-200">
            <TabButton
              label={t('dealer:sellerDashboard.tabs.active')}
              count={stats.activeListings}
              isActive={selectedTab === 'active'}
              onClick={() => setSelectedTab('active')}
            />
            <TabButton
              label={t('dealer:sellerDashboard.tabs.pending')}
              count={stats.pendingListings}
              isActive={selectedTab === 'pending'}
              onClick={() => setSelectedTab('pending')}
            />
            <TabButton
              label={t('dealer:sellerDashboard.tabs.sold')}
              count={stats.soldListings}
              isActive={selectedTab === 'sold'}
              onClick={() => setSelectedTab('sold')}
            />
            <TabButton
              label={t('dealer:sellerDashboard.tabs.draft')}
              count={0}
              isActive={selectedTab === 'draft'}
              onClick={() => setSelectedTab('draft')}
            />
            
            <div className="ml-auto flex items-center gap-2">
              <span className="text-sm text-gray-500">{t('dealer:sellerDashboard.category')}</span>
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value as MarketplaceCategorySelection)}
                className="text-sm border border-gray-300 rounded-lg px-3 py-1.5 bg-white"
              >
                <option value="all">{t('dealer:sellerDashboard.categories.all')}</option>
                <option value="vehicles">{t('dealer:sellerDashboard.categories.vehicles')}</option>
                <option value="real-estate">{t('dealer:sellerDashboard.categories.realEstate')}</option>
              </select>
            </div>
          </div>

          {/* Listings Grid */}
          <div className="p-4">
            {filteredListings.length === 0 ? (
              <EmptyState tab={selectedTab} />
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {filteredListings.map((listing) => (
                  <SellerListingCard key={listing.id} listing={listing} />
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Recent Activity */}
        <div className="mt-8">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            {t('dealer:sellerDashboard.recentActivity')}
          </h2>
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 divide-y divide-gray-200">
            <ActivityItem
              type="view"
              message="Tu publicación 'Toyota RAV4' ha recibido 25 nuevas vistas"
              time="Hace 2 horas"
            />
            <ActivityItem
              type="inquiry"
              message="Nueva consulta sobre 'Casa moderna en Polanco'"
              time="Hace 5 horas"
            />
            <ActivityItem
              type="favorite"
              message="'Honda Civic' fue añadido a favoritos 3 veces"
              time="Ayer"
            />
            <ActivityItem
              type="sold"
              message="¡Felicidades! Tu 'Ford Focus' ha sido marcado como vendido"
              time="Hace 2 días"
            />
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

// Stat Card Component
interface StatCardProps {
  title: string;
  value: number;
  icon: React.ReactNode;
  change?: number;
  color: 'blue' | 'purple' | 'pink' | 'green';
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, change, color }) => {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-600',
    purple: 'bg-purple-50 text-purple-600',
    pink: 'bg-pink-50 text-pink-600',
    green: 'bg-green-50 text-green-600',
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="bg-white rounded-xl p-4 shadow-sm border border-gray-200"
    >
      <div className="flex items-center justify-between mb-3">
        <div className={`p-2 rounded-lg ${colorClasses[color]}`}>
          {icon}
        </div>
        {change !== undefined && (
          <div className={`flex items-center text-sm ${change >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {change >= 0 ? (
              <ArrowTrendingUpIcon className="h-4 w-4 mr-1" />
            ) : (
              <ArrowTrendingDownIcon className="h-4 w-4 mr-1" />
            )}
            {Math.abs(change).toFixed(1)}%
          </div>
        )}
      </div>
      <div className="text-2xl font-bold text-gray-900">
        {value.toLocaleString()}
      </div>
      <div className="text-sm text-gray-500">{title}</div>
    </motion.div>
  );
};

// Quick Action Card
interface QuickActionCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  color: 'amber' | 'blue' | 'purple' | 'green';
}

const QuickActionCard: React.FC<QuickActionCardProps> = ({ title, description, icon, color }) => {
  const colorClasses = {
    amber: 'from-amber-400 to-orange-500',
    blue: 'from-blue-400 to-cyan-500',
    purple: 'from-purple-400 to-pink-500',
    green: 'from-green-400 to-emerald-500',
  };

  return (
    <motion.button
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
      className={`relative overflow-hidden p-4 rounded-xl bg-gradient-to-br ${colorClasses[color]} text-white text-left`}
    >
      <div className="relative z-10">
        <div className="mb-2 opacity-90">{icon}</div>
        <div className="font-semibold">{title}</div>
        <div className="text-sm opacity-90">{description}</div>
      </div>
      <div className="absolute -right-4 -bottom-4 w-24 h-24 bg-white/10 rounded-full" />
    </motion.button>
  );
};

// Tab Button
interface TabButtonProps {
  label: string;
  count: number;
  isActive: boolean;
  onClick: () => void;
}

const TabButton: React.FC<TabButtonProps> = ({ label, count, isActive, onClick }) => (
  <button
    onClick={onClick}
    className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
      isActive
        ? 'bg-blue-600 text-white'
        : 'text-gray-600 hover:bg-gray-100'
    }`}
  >
    {label}
    <span className={`ml-2 px-2 py-0.5 rounded-full text-xs ${
      isActive
        ? 'bg-blue-500 text-white'
        : 'bg-gray-200 text-gray-600'
    }`}>
      {count}
    </span>
  </button>
);

// Seller Listing Card (with actions)
const SellerListingCard: React.FC<{ listing: Listing }> = ({ listing }) => {
  const { t } = useTranslation('dealer');
  
  const statusColors = {
    active: 'bg-green-100 text-green-800',
    pending: 'bg-yellow-100 text-yellow-800',
    sold: 'bg-blue-100 text-blue-800',
    rented: 'bg-purple-100 text-purple-800',
    draft: 'bg-gray-100 text-gray-800',
    archived: 'bg-gray-100 text-gray-800',
    reserved: 'bg-orange-100 text-orange-800',
  };

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: 1, scale: 1 }}
      className="bg-gray-50 rounded-xl overflow-hidden border border-gray-200"
    >
      {/* Image */}
      <div className="relative aspect-video">
        <img
          src={listing.primaryImageUrl || 'https://placehold.co/400x300/gray/white?text=Sin+imagen'}
          alt={listing.title}
          className="w-full h-full object-cover"
        />
        <div className="absolute top-2 left-2">
          <span className={`px-2 py-1 rounded-full text-xs font-medium ${statusColors[listing.status]}`}>
            {t(`sellerDashboard.listingStatus.${listing.status}`)}
          </span>
        </div>
        {listing.isFeatured && (
          <div className="absolute top-2 right-2">
            <span className="px-2 py-1 bg-amber-500 text-white rounded-full text-xs font-medium flex items-center gap-1">
              <SparklesIcon className="h-3 w-3" />
              {t('sellerDashboard.featured')}
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div className="p-4">
        <h3 className="font-semibold text-gray-900 line-clamp-1 mb-1">
          <LocalizedContent content={listing.title} showBadge={false} />
        </h3>
        <p className="text-lg font-bold text-blue-600 mb-3">
          {new Intl.NumberFormat('es-MX', {
            style: 'currency',
            currency: listing.currency,
            maximumFractionDigits: 0,
          }).format(listing.price)}
        </p>

        {/* Stats */}
        <div className="flex items-center gap-4 text-sm text-gray-500 mb-4">
          <span className="flex items-center gap-1">
            <EyeIcon className="h-4 w-4" />
            {listing.viewCount}
          </span>
          <span className="flex items-center gap-1">
            <HeartIcon className="h-4 w-4" />
            {listing.favoriteCount}
          </span>
          <span className="flex items-center gap-1">
            <ChatBubbleLeftRightIcon className="h-4 w-4" />
            {listing.inquiryCount}
          </span>
        </div>

        {/* Actions */}
        <div className="flex items-center gap-2">
          <Link
            to={`/marketplace/listings/${listing.id}/edit`}
            className="flex-1 flex items-center justify-center gap-1 px-3 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 transition-colors"
          >
            <PencilIcon className="h-4 w-4" />
            {t('sellerDashboard.edit')}
          </Link>
          <button className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors">
            <ArchiveBoxIcon className="h-5 w-5" />
          </button>
          <button className="p-2 text-red-500 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors">
            <TrashIcon className="h-5 w-5" />
          </button>
        </div>
      </div>
    </motion.div>
  );
};

// Empty State
const EmptyState: React.FC<{ tab: string }> = ({ tab }) => {
  const { t } = useTranslation('dealer');

  return (
    <div className="text-center py-12">
      <ArchiveBoxIcon className="h-12 w-12 text-gray-400 mx-auto mb-4" />
      <p className="text-gray-500 mb-4">
        {t(`sellerDashboard.emptyStates.${tab}`)}
      </p>
      <Link
        to="/marketplace/listings/new"
        className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
      >
        <PlusIcon className="h-5 w-5" />
        {t('sellerDashboard.createNewListing')}
      </Link>
    </div>
  );
};

// Activity Item
interface ActivityItemProps {
  type: 'view' | 'inquiry' | 'favorite' | 'sold';
  message: string;
  time: string;
}

const ActivityItem: React.FC<ActivityItemProps> = ({ type, message, time }) => {
  const icons = {
    view: <EyeIcon className="h-5 w-5 text-blue-500" />,
    inquiry: <ChatBubbleLeftRightIcon className="h-5 w-5 text-green-500" />,
    favorite: <HeartIcon className="h-5 w-5 text-pink-500" />,
    sold: <CurrencyDollarIcon className="h-5 w-5 text-amber-500" />,
  };

  return (
    <div className="flex items-start gap-3 p-4">
      <div className="p-2 bg-gray-100 rounded-lg">
        {icons[type]}
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-sm text-gray-900">{message}</p>
        <p className="text-xs text-gray-500 mt-1">{time}</p>
      </div>
    </div>
  );
};

export default SellerDashboardPage;
