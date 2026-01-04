/**
 * Dealer Profile Page
 * Complete dealer management page with tabs
 */

import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Building2,
  Edit,
  Settings,
  Package,
  CreditCard,
  Star,
  MapPin,
  Phone,
  Mail,
  Globe,
  Shield,
  Clock,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Car,
  Truck,
  ChevronLeft,
  Plus,
  ExternalLink,
} from 'lucide-react';
import { useDealerByOwner, useDealerSubscription, useDealerModules } from '@/hooks/useDealer';
import { useAuth } from '@/hooks/useAuth';
import { DealerForm } from '@/components/dealer/DealerForm';
import { VerificationStatusLabels, VerificationStatusColors, DealerTypeLabels } from '@/types/dealer';
import { useUpdateDealer } from '@/hooks/useDealer';
import toast from 'react-hot-toast';

type TabType = 'overview' | 'edit' | 'subscription' | 'modules';

export const DealerProfilePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('overview');
  
  // Use owner ID from params or current user
  const ownerId = id || user?.id || '';
  
  const { data: dealer, isLoading, error, refetch } = useDealerByOwner(ownerId);
  const { data: subscription } = useDealerSubscription(dealer?.id || '');
  const { data: modules } = useDealerModules(dealer?.id || '');
  const updateDealerMutation = useUpdateDealer();

  const handleUpdateDealer = async (data: any) => {
    if (!dealer?.id) return;
    
    try {
      await updateDealerMutation.mutateAsync({ dealerId: dealer.id, data });
      toast.success('Dealer profile updated successfully');
      refetch();
      setActiveTab('overview');
    } catch (error) {
      toast.error('Failed to update dealer profile');
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
      </div>
    );
  }

  if (error || !dealer) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Building2 className="h-16 w-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Dealer Not Found</h2>
          <p className="text-gray-500 mb-4">You don't have a dealer profile yet.</p>
          <button
            onClick={() => navigate('/dealer/onboarding')}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Create Dealer Profile
          </button>
        </div>
      </div>
    );
  }

  const statusColor = VerificationStatusColors[dealer.verificationStatus];

  const tabs = [
    { id: 'overview' as TabType, label: 'Overview', icon: Building2 },
    { id: 'edit' as TabType, label: 'Edit Profile', icon: Edit },
    { id: 'subscription' as TabType, label: 'Subscription', icon: CreditCard },
    { id: 'modules' as TabType, label: 'Modules', icon: Package },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-blue-800 text-white">
        <div className="max-w-7xl mx-auto px-4 py-8">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-white/80 hover:text-white mb-4 transition-colors"
          >
            <ChevronLeft className="h-5 w-5" />
            Back
          </button>
          
          <div className="flex items-start gap-6">
            {/* Logo */}
            <div className="w-24 h-24 bg-white rounded-xl shadow-lg flex items-center justify-center overflow-hidden">
              {dealer.logoUrl ? (
                <img src={dealer.logoUrl} alt={dealer.businessName} className="w-full h-full object-cover" />
              ) : (
                <Building2 className="h-12 w-12 text-gray-400" />
              )}
            </div>
            
            <div className="flex-1">
              <div className="flex items-center gap-3 mb-2">
                <h1 className="text-3xl font-bold">{dealer.businessName}</h1>
                {dealer.verificationStatus === 2 && (
                  <Shield className="h-6 w-6 text-green-400" />
                )}
              </div>
              
              <p className="text-white/80 mb-3">
                {DealerTypeLabels[dealer.dealerType]} • Member since {new Date(dealer.createdAt).getFullYear()}
              </p>
              
              <div className="flex items-center gap-4">
                <span className={`px-3 py-1 rounded-full text-sm font-medium ${statusColor}`}>
                  {VerificationStatusLabels[dealer.verificationStatus]}
                </span>
                {dealer.averageRating > 0 && (
                  <span className="flex items-center gap-1 text-white/90">
                    <Star className="h-5 w-5 fill-yellow-400 text-yellow-400" />
                    {dealer.averageRating.toFixed(1)} ({dealer.totalReviews} reviews)
                  </span>
                )}
              </div>
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setActiveTab('edit')}
                className="px-4 py-2 bg-white/10 hover:bg-white/20 rounded-lg transition-colors flex items-center gap-2"
              >
                <Settings className="h-5 w-5" />
                Settings
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
              {/* Description */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">About</h3>
                <p className="text-gray-600">
                  {dealer.description || 'No description provided.'}
                </p>
              </div>

              {/* Services */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Services Offered</h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className={`flex items-center gap-3 p-3 rounded-lg ${dealer.acceptsFinancing ? 'bg-green-50 text-green-700' : 'bg-gray-50 text-gray-400'}`}>
                    <CreditCard className="h-5 w-5" />
                    <span className="font-medium">Financing</span>
                    {dealer.acceptsFinancing ? <CheckCircle2 className="h-5 w-5 ml-auto" /> : <XCircle className="h-5 w-5 ml-auto" />}
                  </div>
                  <div className={`flex items-center gap-3 p-3 rounded-lg ${dealer.acceptsTradeIn ? 'bg-green-50 text-green-700' : 'bg-gray-50 text-gray-400'}`}>
                    <Car className="h-5 w-5" />
                    <span className="font-medium">Trade-In</span>
                    {dealer.acceptsTradeIn ? <CheckCircle2 className="h-5 w-5 ml-auto" /> : <XCircle className="h-5 w-5 ml-auto" />}
                  </div>
                  <div className={`flex items-center gap-3 p-3 rounded-lg ${dealer.offersWarranty ? 'bg-green-50 text-green-700' : 'bg-gray-50 text-gray-400'}`}>
                    <Shield className="h-5 w-5" />
                    <span className="font-medium">Warranty</span>
                    {dealer.offersWarranty ? <CheckCircle2 className="h-5 w-5 ml-auto" /> : <XCircle className="h-5 w-5 ml-auto" />}
                  </div>
                  <div className={`flex items-center gap-3 p-3 rounded-lg ${dealer.homeDelivery ? 'bg-green-50 text-green-700' : 'bg-gray-50 text-gray-400'}`}>
                    <Truck className="h-5 w-5" />
                    <span className="font-medium">Delivery</span>
                    {dealer.homeDelivery ? <CheckCircle2 className="h-5 w-5 ml-auto" /> : <XCircle className="h-5 w-5 ml-auto" />}
                  </div>
                </div>
              </div>

              {/* Stats */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Statistics</h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="text-center p-4 bg-blue-50 rounded-lg">
                    <p className="text-3xl font-bold text-blue-600">{dealer.totalListings}</p>
                    <p className="text-sm text-gray-600">Active Listings</p>
                  </div>
                  <div className="text-center p-4 bg-green-50 rounded-lg">
                    <p className="text-3xl font-bold text-green-600">{dealer.totalSales}</p>
                    <p className="text-sm text-gray-600">Total Sales</p>
                  </div>
                  <div className="text-center p-4 bg-yellow-50 rounded-lg">
                    <p className="text-3xl font-bold text-yellow-600">{dealer.reviewCount}</p>
                    <p className="text-sm text-gray-600">Reviews</p>
                  </div>
                  <div className="text-center p-4 bg-purple-50 rounded-lg">
                    <div className="flex items-center justify-center gap-1">
                      <Star className="h-6 w-6 text-yellow-400 fill-yellow-400" />
                      <span className="text-3xl font-bold text-purple-600">{dealer.rating?.toFixed(1) || 'N/A'}</span>
                    </div>
                    <p className="text-sm text-gray-600">Rating</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Contact */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Contact</h3>
                <div className="space-y-3">
                  <a href={`mailto:${dealer.email}`} className="flex items-center gap-3 text-gray-600 hover:text-blue-600 transition-colors">
                    <Mail className="h-5 w-5 text-gray-400" />
                    {dealer.email}
                  </a>
                  <a href={`tel:${dealer.phone}`} className="flex items-center gap-3 text-gray-600 hover:text-blue-600 transition-colors">
                    <Phone className="h-5 w-5 text-gray-400" />
                    {dealer.phone}
                  </a>
                  {dealer.website && (
                    <a href={dealer.website} target="_blank" rel="noopener noreferrer" className="flex items-center gap-3 text-gray-600 hover:text-blue-600 transition-colors">
                      <Globe className="h-5 w-5 text-gray-400" />
                      Website
                      <ExternalLink className="h-4 w-4" />
                    </a>
                  )}
                </div>
              </div>

              {/* Location */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Location</h3>
                <div className="flex items-start gap-3 text-gray-600">
                  <MapPin className="h-5 w-5 text-gray-400 mt-0.5" />
                  <div>
                    <p>{dealer.address}</p>
                    <p>{dealer.city}, {dealer.state} {dealer.zipCode}</p>
                    <p>{dealer.country}</p>
                  </div>
                </div>
              </div>

              {/* Working Hours */}
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
                  <Clock className="h-5 w-5 text-gray-400" />
                  Working Hours
                </h3>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Monday - Friday</span>
                    <span className="font-medium">9:00 AM - 7:00 PM</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Saturday</span>
                    <span className="font-medium">9:00 AM - 5:00 PM</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Sunday</span>
                    <span className="font-medium text-red-500">Closed</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'edit' && (
          <DealerForm
            dealer={dealer}
            onSubmit={handleUpdateDealer}
            isLoading={updateDealerMutation.isPending}
            mode="edit"
          />
        )}

        {activeTab === 'subscription' && (
          <div className="max-w-3xl mx-auto">
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
              <h3 className="text-xl font-semibold text-gray-900 mb-6">Your Subscription</h3>
              
              {subscription ? (
                <div className="space-y-6">
                  <div className="flex items-center justify-between p-6 bg-gradient-to-r from-blue-500 to-blue-600 rounded-xl text-white">
                    <div>
                      <p className="text-sm text-blue-100">Current Plan</p>
                      <p className="text-2xl font-bold">{subscription.planName}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm text-blue-100">Status</p>
                      <span className={`inline-flex items-center gap-1 px-3 py-1 rounded-full text-sm font-medium ${
                        subscription.isActive ? 'bg-green-400/20 text-green-100' : 'bg-red-400/20 text-red-100'
                      }`}>
                        {subscription.isActive ? <CheckCircle2 className="h-4 w-4" /> : <AlertCircle className="h-4 w-4" />}
                        {subscription.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="p-4 bg-gray-50 rounded-lg">
                      <p className="text-sm text-gray-500">Billing Period</p>
                      <p className="font-semibold">{subscription.billingPeriod}</p>
                    </div>
                    <div className="p-4 bg-gray-50 rounded-lg">
                      <p className="text-sm text-gray-500">Next Billing Date</p>
                      <p className="font-semibold">{new Date(subscription.currentPeriodEnd).toLocaleDateString()}</p>
                    </div>
                  </div>

                  <button className="w-full py-3 bg-gray-100 hover:bg-gray-200 rounded-lg font-medium transition-colors">
                    Manage Subscription
                  </button>
                </div>
              ) : (
                <div className="text-center py-8">
                  <CreditCard className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500 mb-4">No active subscription</p>
                  <button
                    onClick={() => navigate('/pricing')}
                    className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    View Plans
                  </button>
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === 'modules' && (
          <div className="max-w-4xl mx-auto">
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-xl font-semibold text-gray-900">Active Modules</h3>
                <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
                  <Plus className="h-5 w-5" />
                  Add Module
                </button>
              </div>
              
              {modules && modules.length > 0 ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {modules.map((module) => (
                    <div
                      key={module.moduleId}
                      className="flex items-center justify-between p-4 border border-gray-200 rounded-lg"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                          <Package className="h-5 w-5 text-blue-600" />
                        </div>
                        <div>
                          <p className="font-medium">{module.moduleName}</p>
                          <p className="text-sm text-gray-500">
                            {module.isActive ? 'Active' : 'Inactive'}
                          </p>
                        </div>
                      </div>
                      <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                        module.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-700'
                      }`}>
                        {module.isActive ? 'Enabled' : 'Disabled'}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-8">
                  <Package className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500">No modules activated yet</p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default DealerProfilePage;
