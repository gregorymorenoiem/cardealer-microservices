/**
 * Seller Profile Page
 * Complete seller profile management page
 */

import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  User,
  Edit,
  MessageCircle,
  Star,
  MapPin,
  Phone,
  Mail,
  Shield,
  Clock,
  CheckCircle2,
  ChevronLeft,
  Car,
  Eye,
  Heart,
  TrendingUp,
  Facebook,
  Instagram,
  Twitter,
  AlertCircle,
  Share2,
} from 'lucide-react';
import { useSellerByUser, useSellerStats, useUpdateSeller } from '@/hooks/useSeller';
import { useAuth } from '@/hooks/useAuth';
import { SellerForm } from '@/components/dealer/SellerForm';
import { VerificationStatusLabels, VerificationStatusColors } from '@/types/dealer';
import toast from 'react-hot-toast';

type TabType = 'overview' | 'edit' | 'listings' | 'stats';

export const SellerProfilePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('overview');
  
  const userId = id || user?.id || '';
  
  const { data: seller, isLoading, error, refetch } = useSellerByUser(userId);
  const { data: stats } = useSellerStats(seller?.id || '');
  const updateSellerMutation = useUpdateSeller();

  const handleUpdateSeller = async (data: any) => {
    if (!seller?.id) return;
    
    try {
      await updateSellerMutation.mutateAsync({ sellerId: seller.id, data });
      toast.success('Profile updated successfully');
      refetch();
      setActiveTab('overview');
    } catch (error) {
      toast.error('Failed to update profile');
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
      </div>
    );
  }

  if (error || !seller) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <User className="h-16 w-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Seller Profile Not Found</h2>
          <p className="text-gray-500 mb-4">You don't have a seller profile yet.</p>
          <button
            onClick={() => navigate('/seller/create')}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Create Seller Profile
          </button>
        </div>
      </div>
    );
  }

  const statusColor = VerificationStatusColors[seller.verificationStatus];
  const isOwner = user?.id === seller.userId;

  const tabs = [
    { id: 'overview' as TabType, label: 'Overview', icon: User },
    { id: 'listings' as TabType, label: 'My Listings', icon: Car },
    { id: 'stats' as TabType, label: 'Statistics', icon: TrendingUp },
    ...(isOwner ? [{ id: 'edit' as TabType, label: 'Edit Profile', icon: Edit }] : []),
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-purple-600 to-indigo-700 text-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-white/80 hover:text-white mb-4 transition-colors"
          >
            <ChevronLeft className="h-5 w-5" />
            Back
          </button>
          
          <div className="flex items-start gap-6">
            {/* Avatar */}
            <div className="w-24 h-24 bg-white rounded-full shadow-lg flex items-center justify-center overflow-hidden">
              {seller.avatarUrl ? (
                <img src={seller.avatarUrl} alt={seller.displayName || seller.fullName} className="w-full h-full object-cover" />
              ) : (
                <User className="h-12 w-12 text-gray-400" />
              )}
            </div>
            
            <div className="flex-1">
              <div className="flex items-center gap-3 mb-2">
                <h1 className="text-3xl font-bold">{seller.displayName || seller.fullName}</h1>
                {seller.verificationStatus === 2 && (
                  <Shield className="h-6 w-6 text-green-400" />
                )}
              </div>
              
              <p className="text-white/80 mb-3 flex items-center gap-2">
                <MapPin className="h-4 w-4" />
                {seller.city}, {seller.state}
                <span className="mx-2">•</span>
                <Clock className="h-4 w-4" />
                Member since {new Date(seller.createdAt).toLocaleDateString()}
              </p>
              
              <div className="flex items-center gap-4">
                <span className={`px-3 py-1 rounded-full text-sm font-medium ${statusColor}`}>
                  {VerificationStatusLabels[seller.verificationStatus]}
                </span>
                {seller.averageRating && (
                  <span className="flex items-center gap-1 text-white/90">
                    <Star className="h-5 w-5 fill-yellow-400 text-yellow-400" />
                    {seller.averageRating.toFixed(1)} ({seller.totalReviews} reviews)
                  </span>
                )}
              </div>
            </div>

            <div className="flex gap-3">
              {!isOwner && (
                <button className="px-4 py-2 bg-white text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors flex items-center gap-2 font-medium">
                  <MessageCircle className="h-5 w-5" />
                  Message
                </button>
              )}
              <button className="px-4 py-2 bg-white/10 hover:bg-white/20 rounded-lg transition-colors flex items-center gap-2">
                <Share2 className="h-5 w-5" />
                Share
              </button>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="max-w-7xl mx-auto px-4">
          <div className="flex gap-1 overflow-x-auto">
            {tabs.map(({ id, label, icon: Icon }) => (
              <button
                key={id}
                onClick={() => setActiveTab(id)}
                className={`flex items-center gap-2 px-4 py-3 font-medium transition-colors whitespace-nowrap ${
                  activeTab === id
                    ? 'text-white border-b-2 border-white'
                    : 'text-white/60 hover:text-white/80'
                }`}
              >
                <Icon className="h-5 w-5" />
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 py-8">
        {activeTab === 'overview' && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Main Info */}
            <div className="lg:col-span-2 space-y-6">
              {/* Bio */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">About</h3>
                <p className="text-gray-600">
                  {seller.bio || 'No bio provided.'}
                </p>
              </div>

              {/* Stats Cards */}
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
                  <Car className="h-8 w-8 text-blue-600 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{stats?.activeListings || seller.activeListings}</p>
                  <p className="text-sm text-gray-500">Active Listings</p>
                </div>
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
                  <CheckCircle2 className="h-8 w-8 text-green-600 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{stats?.totalSold || seller.totalSales}</p>
                  <p className="text-sm text-gray-500">Vehicles Sold</p>
                </div>
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
                  <Clock className="h-8 w-8 text-purple-600 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{stats?.avgResponseTimeMinutes || seller.responseTimeMinutes || 'N/A'}</p>
                  <p className="text-sm text-gray-500">Avg. Response (min)</p>
                </div>
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 text-center">
                  <TrendingUp className="h-8 w-8 text-amber-600 mx-auto mb-2" />
                  <p className="text-2xl font-bold text-gray-900">{stats?.responseRate ? `${(stats.responseRate * 100).toFixed(0)}%` : 'N/A'}</p>
                  <p className="text-sm text-gray-500">Response Rate</p>
                </div>
              </div>

              {/* Trust Badges */}
              {seller.trustBadges && seller.trustBadges.length > 0 && (
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">Trust Badges</h3>
                  <div className="flex flex-wrap gap-3">
                    {seller.trustBadges.map((badge, index) => (
                      <span
                        key={index}
                        className="flex items-center gap-2 px-4 py-2 bg-green-50 text-green-700 rounded-full text-sm font-medium"
                      >
                        <Shield className="h-4 w-4" />
                        {badge}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Languages */}
              {seller.spokenLanguages && seller.spokenLanguages.length > 0 && (
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">Languages Spoken</h3>
                  <div className="flex flex-wrap gap-2">
                    {seller.spokenLanguages.map((lang, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-gray-100 text-gray-700 rounded-lg text-sm"
                      >
                        {lang}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Contact */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Contact</h3>
                <div className="space-y-3">
                  <a href={`mailto:${seller.email}`} className="flex items-center gap-3 text-gray-600 hover:text-blue-600 transition-colors">
                    <Mail className="h-5 w-5 text-gray-400" />
                    {seller.email}
                  </a>
                  {seller.showPhone && seller.phone && (
                    <a href={`tel:${seller.phone}`} className="flex items-center gap-3 text-gray-600 hover:text-blue-600 transition-colors">
                      <Phone className="h-5 w-5 text-gray-400" />
                      {seller.phone}
                    </a>
                  )}
                  {seller.whatsApp && (
                    <a href={`https://wa.me/${seller.whatsApp}`} target="_blank" rel="noopener noreferrer" className="flex items-center gap-3 text-gray-600 hover:text-green-600 transition-colors">
                      <MessageCircle className="h-5 w-5 text-gray-400" />
                      WhatsApp
                    </a>
                  )}
                </div>
                
                <div className="mt-4 pt-4 border-t border-gray-100">
                  <p className="text-sm text-gray-500">Preferred contact method</p>
                  <p className="font-medium capitalize">{seller.preferredContactMethod}</p>
                </div>
              </div>

              {/* Social Media */}
              {(seller.facebookUrl || seller.instagramUrl || seller.twitterUrl) && (
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">Social Media</h3>
                  <div className="flex gap-3">
                    {seller.facebookUrl && (
                      <a
                        href={seller.facebookUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-3 bg-blue-50 text-blue-600 rounded-lg hover:bg-blue-100 transition-colors"
                      >
                        <Facebook className="h-6 w-6" />
                      </a>
                    )}
                    {seller.instagramUrl && (
                      <a
                        href={seller.instagramUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-3 bg-pink-50 text-pink-600 rounded-lg hover:bg-pink-100 transition-colors"
                      >
                        <Instagram className="h-6 w-6" />
                      </a>
                    )}
                    {seller.twitterUrl && (
                      <a
                        href={seller.twitterUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="p-3 bg-sky-50 text-sky-600 rounded-lg hover:bg-sky-100 transition-colors"
                      >
                        <Twitter className="h-6 w-6" />
                      </a>
                    )}
                  </div>
                </div>
              )}

              {/* Quick Stats for Buyers */}
              <div className="bg-gradient-to-br from-purple-500 to-indigo-600 rounded-xl p-6 text-white">
                <h3 className="text-lg font-semibold mb-4">Why Buy From This Seller?</h3>
                <div className="space-y-3">
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-green-300" />
                    <span>{seller.totalSales}+ successful sales</span>
                  </div>
                  {seller.verificationStatus === 2 && (
                    <div className="flex items-center gap-3">
                      <Shield className="h-5 w-5 text-green-300" />
                      <span>Verified seller</span>
                    </div>
                  )}
                  {seller.responseTimeMinutes && seller.responseTimeMinutes < 60 && (
                    <div className="flex items-center gap-3">
                      <Clock className="h-5 w-5 text-green-300" />
                      <span>Fast response time</span>
                    </div>
                  )}
                  {seller.averageRating && seller.averageRating >= 4 && (
                    <div className="flex items-center gap-3">
                      <Star className="h-5 w-5 text-yellow-300 fill-yellow-300" />
                      <span>Highly rated</span>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'listings' && (
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
            <div className="text-center py-12">
              <Car className="h-16 w-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">Active Listings</h3>
              <p className="text-gray-500 mb-6">View all active listings from this seller</p>
              <button
                onClick={() => navigate(`/browse?sellerId=${seller.id}`)}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Browse Listings
              </button>
            </div>
          </div>
        )}

        {activeTab === 'stats' && stats && (
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <Eye className="h-8 w-8 text-blue-600" />
                  <span className="text-xs text-gray-500">30 days</span>
                </div>
                <p className="text-3xl font-bold text-gray-900">{stats.totalViews30Days}</p>
                <p className="text-sm text-gray-500">Profile Views</p>
              </div>
              
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <Heart className="h-8 w-8 text-red-500" />
                  <span className="text-xs text-gray-500">30 days</span>
                </div>
                <p className="text-3xl font-bold text-gray-900">{stats.totalFavorites30Days}</p>
                <p className="text-sm text-gray-500">Favorites Received</p>
              </div>
              
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <MessageCircle className="h-8 w-8 text-green-600" />
                  <span className="text-xs text-gray-500">30 days</span>
                </div>
                <p className="text-3xl font-bold text-gray-900">{stats.totalInquiries30Days}</p>
                <p className="text-sm text-gray-500">Inquiries</p>
              </div>
              
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <div className="flex items-center justify-between mb-4">
                  <TrendingUp className="h-8 w-8 text-purple-600" />
                  <span className="text-xs text-gray-500">All time</span>
                </div>
                <p className="text-3xl font-bold text-gray-900">
                  {stats.conversionRate ? `${(stats.conversionRate * 100).toFixed(1)}%` : 'N/A'}
                </p>
                <p className="text-sm text-gray-500">Conversion Rate</p>
              </div>
            </div>

            {/* Pending Stats */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                <AlertCircle className="h-5 w-5 text-amber-500" />
                Pending Activity
              </h3>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="p-4 bg-amber-50 rounded-lg">
                  <p className="text-2xl font-bold text-amber-600">{stats.pendingMessages}</p>
                  <p className="text-sm text-gray-600">Pending Messages</p>
                </div>
                <div className="p-4 bg-blue-50 rounded-lg">
                  <p className="text-2xl font-bold text-blue-600">{stats.pendingReviews}</p>
                  <p className="text-sm text-gray-600">Pending Reviews</p>
                </div>
                <div className="p-4 bg-green-50 rounded-lg">
                  <p className="text-2xl font-bold text-green-600">{stats.newFollowers7Days}</p>
                  <p className="text-sm text-gray-600">New Followers (7d)</p>
                </div>
                <div className="p-4 bg-purple-50 rounded-lg">
                  <p className="text-2xl font-bold text-purple-600">
                    ${stats.estimatedMonthlyRevenue?.toLocaleString() || '0'}
                  </p>
                  <p className="text-sm text-gray-600">Est. Monthly Revenue</p>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'edit' && isOwner && (
          <SellerForm
            seller={seller}
            onSubmit={handleUpdateSeller}
            isLoading={updateSellerMutation.isPending}
            mode="edit"
          />
        )}
      </div>
    </div>
  );
};

export default SellerProfilePage;
