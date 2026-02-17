/**
 * Dealer Profile Card Component
 * Displays dealer information in a card format
 */


import {
  Building2,
  MapPin,
  Phone,
  Mail,
  Globe,
  Star,
  CheckCircle2,
  Clock,
  Car,
  BadgeCheck,
  Shield,
  CreditCard,
  Truck,
  Wrench,
} from 'lucide-react';
import type { Dealer } from '@/types/dealer';
import { DealerTypeLabels, VerificationStatusLabels, VerificationStatusColors } from '@/types/dealer';

interface DealerCardProps {
  dealer: Dealer;
  showActions?: boolean;
  onEdit?: () => void;
  onView?: () => void;
}

export const DealerCard: React.FC<DealerCardProps> = ({
  dealer,
  showActions = true,
  onEdit,
  onView,
}) => {
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow">
      {/* Banner */}
      <div className="h-24 bg-gradient-to-r from-blue-600 to-indigo-600 relative">
        {dealer.bannerUrl && (
          <img
            src={dealer.bannerUrl}
            alt="Banner"
            className="w-full h-full object-cover"
          />
        )}
        {dealer.isFeatured && (
          <span className="absolute top-2 right-2 bg-yellow-400 text-yellow-900 text-xs font-semibold px-2 py-1 rounded-full flex items-center gap-1">
            <Star className="h-3 w-3" />
            Featured
          </span>
        )}
      </div>

      {/* Logo */}
      <div className="relative px-4 -mt-10">
        <div className="w-20 h-20 rounded-xl bg-white shadow-md flex items-center justify-center border-2 border-white overflow-hidden">
          {dealer.logoUrl ? (
            <img src={dealer.logoUrl} alt={dealer.businessName} className="w-full h-full object-cover" />
          ) : (
            <Building2 className="h-10 w-10 text-gray-400" />
          )}
        </div>
      </div>

      {/* Content */}
      <div className="p-4 pt-2">
        {/* Header */}
        <div className="flex items-start justify-between mb-3">
          <div>
            <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
              {dealer.businessName}
              {dealer.verificationStatus === 2 && (
                <BadgeCheck className="h-5 w-5 text-blue-500" />
              )}
            </h3>
            {dealer.tradeName && (
              <p className="text-sm text-gray-500">{dealer.tradeName}</p>
            )}
          </div>
          <span className={`text-xs px-2 py-1 rounded-full ${VerificationStatusColors[dealer.verificationStatus]}`}>
            {VerificationStatusLabels[dealer.verificationStatus]}
          </span>
        </div>

        {/* Type & Rating */}
        <div className="flex items-center gap-4 mb-3">
          <span className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
            {DealerTypeLabels[dealer.dealerType]}
          </span>
          {dealer.averageRating > 0 && (
            <div className="flex items-center gap-1">
              <Star className="h-4 w-4 text-yellow-400 fill-yellow-400" />
              <span className="text-sm font-medium">{dealer.averageRating.toFixed(1)}</span>
              <span className="text-xs text-gray-500">({dealer.totalReviews})</span>
            </div>
          )}
        </div>

        {/* Location */}
        <div className="flex items-center text-sm text-gray-600 mb-2">
          <MapPin className="h-4 w-4 mr-2 text-gray-400" />
          {dealer.city}, {dealer.state}
        </div>

        {/* Contact */}
        <div className="space-y-1 mb-3">
          <div className="flex items-center text-sm text-gray-600">
            <Phone className="h-4 w-4 mr-2 text-gray-400" />
            {dealer.phone}
          </div>
          <div className="flex items-center text-sm text-gray-600">
            <Mail className="h-4 w-4 mr-2 text-gray-400" />
            {dealer.email}
          </div>
          {dealer.website && (
            <div className="flex items-center text-sm text-blue-600">
              <Globe className="h-4 w-4 mr-2 text-gray-400" />
              <a href={dealer.website} target="_blank" rel="noopener noreferrer" className="hover:underline">
                {dealer.website.replace(/^https?:\/\//, '')}
              </a>
            </div>
          )}
        </div>

        {/* Stats */}
        <div className="grid grid-cols-3 gap-2 py-3 border-t border-gray-100">
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{dealer.activeListings}</p>
            <p className="text-xs text-gray-500">Listings</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{dealer.totalSales}</p>
            <p className="text-xs text-gray-500">Sales</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{dealer.responseTimeMinutes}m</p>
            <p className="text-xs text-gray-500">Response</p>
          </div>
        </div>

        {/* Features */}
        <div className="flex flex-wrap gap-2 py-3 border-t border-gray-100">
          {dealer.acceptsFinancing && (
            <span className="flex items-center gap-1 text-xs bg-green-50 text-green-700 px-2 py-1 rounded">
              <CreditCard className="h-3 w-3" />
              Financing
            </span>
          )}
          {dealer.acceptsTradeIn && (
            <span className="flex items-center gap-1 text-xs bg-blue-50 text-blue-700 px-2 py-1 rounded">
              <Car className="h-3 w-3" />
              Trade-In
            </span>
          )}
          {dealer.offersWarranty && (
            <span className="flex items-center gap-1 text-xs bg-purple-50 text-purple-700 px-2 py-1 rounded">
              <Shield className="h-3 w-3" />
              Warranty
            </span>
          )}
          {dealer.homeDelivery && (
            <span className="flex items-center gap-1 text-xs bg-amber-50 text-amber-700 px-2 py-1 rounded">
              <Truck className="h-3 w-3" />
              Delivery
            </span>
          )}
        </div>

        {/* Actions */}
        {showActions && (
          <div className="flex gap-2 pt-3 border-t border-gray-100">
            <button
              onClick={onView}
              className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium py-2 px-4 rounded-lg transition-colors"
            >
              View Details
            </button>
            <button
              onClick={onEdit}
              className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors"
            >
              Edit
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default DealerCard;
