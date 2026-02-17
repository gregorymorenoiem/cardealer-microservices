/**
 * Seller Profile Card Component
 * Displays seller information in a card format
 */

import {
  User,
  MapPin,
  Phone,
  Mail,
  Star,
  CheckCircle2,
  Clock,
  Calendar,
  BadgeCheck,
  MessageCircle,
  ShoppingBag,
} from 'lucide-react';
import type { SellerProfile, SellerStats } from '@/types/dealer';
import { VerificationStatusLabels, VerificationStatusColors } from '@/types/dealer';

interface SellerCardProps {
  seller: SellerProfile;
  stats?: SellerStats;
  showActions?: boolean;
  onEdit?: () => void;
  onView?: () => void;
  onMessage?: () => void;
}

export const SellerCard: React.FC<SellerCardProps> = ({
  seller,
  stats,
  showActions = true,
  onEdit,
  onView,
  onMessage,
}) => {
  const memberDays = stats?.memberSinceDays || 
    Math.floor((new Date().getTime() - new Date(seller.createdAt).getTime()) / (1000 * 60 * 60 * 24));

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-shadow">
      {/* Header */}
      <div className="bg-gradient-to-r from-emerald-500 to-teal-600 p-6 text-white">
        <div className="flex items-start gap-4">
          {/* Avatar */}
          <div className="w-20 h-20 rounded-full bg-white/20 flex items-center justify-center overflow-hidden border-2 border-white/50">
            {seller.avatarUrl ? (
              <img src={seller.avatarUrl} alt={seller.fullName} className="w-full h-full object-cover" />
            ) : (
              <User className="h-10 w-10 text-white/80" />
            )}
          </div>

          <div className="flex-1">
            <div className="flex items-center gap-2">
              <h3 className="text-xl font-semibold">{seller.fullName}</h3>
              {seller.verificationStatus === 3 && (
                <BadgeCheck className="h-5 w-5 text-white" />
              )}
            </div>
            <span className={`inline-block mt-1 text-xs px-2 py-1 rounded-full bg-white/20`}>
              {VerificationStatusLabels[seller.verificationStatus]}
            </span>
            <div className="flex items-center gap-1 mt-2">
              <Calendar className="h-4 w-4 opacity-80" />
              <span className="text-sm opacity-90">Member for {memberDays} days</span>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Bio */}
        {seller.bio && (
          <p className="text-sm text-gray-600 mb-4 line-clamp-2">{seller.bio}</p>
        )}

        {/* Contact Info */}
        <div className="space-y-2 mb-4">
          {seller.showLocation && seller.city && (
            <div className="flex items-center text-sm text-gray-600">
              <MapPin className="h-4 w-4 mr-2 text-gray-400" />
              {seller.city}, {seller.state}
            </div>
          )}
          {seller.showPhone && seller.phone && (
            <div className="flex items-center text-sm text-gray-600">
              <Phone className="h-4 w-4 mr-2 text-gray-400" />
              {seller.phone}
            </div>
          )}
          <div className="flex items-center text-sm text-gray-600">
            <Mail className="h-4 w-4 mr-2 text-gray-400" />
            {seller.email}
          </div>
          {seller.preferredContactMethod && (
            <div className="flex items-center text-sm text-gray-500">
              <MessageCircle className="h-4 w-4 mr-2 text-gray-400" />
              Prefers: {seller.preferredContactMethod}
            </div>
          )}
        </div>

        {/* Stats */}
        <div className="grid grid-cols-4 gap-2 py-3 border-t border-gray-100">
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{seller.activeListings}</p>
            <p className="text-xs text-gray-500">Active</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{seller.totalSales}</p>
            <p className="text-xs text-gray-500">Sales</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900 flex items-center justify-center gap-1">
              <Star className="h-4 w-4 text-yellow-400 fill-yellow-400" />
              {seller.averageRating > 0 ? seller.averageRating.toFixed(1) : '-'}
            </p>
            <p className="text-xs text-gray-500">Rating</p>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold text-gray-900">{seller.responseTimeMinutes}m</p>
            <p className="text-xs text-gray-500">Response</p>
          </div>
        </div>

        {/* Features */}
        <div className="flex flex-wrap gap-2 py-3 border-t border-gray-100">
          {seller.acceptsOffers && (
            <span className="flex items-center gap-1 text-xs bg-green-50 text-green-700 px-2 py-1 rounded">
              <ShoppingBag className="h-3 w-3" />
              Accepts Offers
            </span>
          )}
          {seller.canSellHighValue && (
            <span className="flex items-center gap-1 text-xs bg-purple-50 text-purple-700 px-2 py-1 rounded">
              <CheckCircle2 className="h-3 w-3" />
              High Value Verified
            </span>
          )}
          <span className="flex items-center gap-1 text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
            Max {seller.maxActiveListings} listings
          </span>
        </div>

        {/* Actions */}
        {showActions && (
          <div className="flex gap-2 pt-3 border-t border-gray-100">
            {onMessage && (
              <button
                onClick={onMessage}
                className="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white font-medium py-2 px-4 rounded-lg transition-colors flex items-center justify-center gap-2"
              >
                <MessageCircle className="h-4 w-4" />
                Message
              </button>
            )}
            {onView && (
              <button
                onClick={onView}
                className="flex-1 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium py-2 px-4 rounded-lg transition-colors"
              >
                View Profile
              </button>
            )}
            {onEdit && (
              <button
                onClick={onEdit}
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors"
              >
                Edit
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default SellerCard;
