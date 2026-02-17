import { useState } from 'react';
import { Link } from 'react-router-dom';
import { FiEdit2, FiTrash2, FiEye, FiPlus, FiPackage } from 'react-icons/fi';
import { formatPrice, formatMileage } from '@/utils/formatters';
import { generateListingUrl } from '@/utils/seoSlug';
import { getVehicleSaleImageUrl } from '@/utils/s3ImageUrl';

interface Listing {
  id: string;
  title: string;
  price: number;
  mileage: number;
  image: string;
  status: 'active' | 'pending' | 'sold';
  views: number;
  inquiries: number;
  createdAt: string;
}

// Mock data - replace with real data from API
const mockListings: Listing[] = [
  {
    id: '1',
    title: '2023 Tesla Model 3 Long Range',
    price: 45999,
    mileage: 12000,
    image: getVehicleSaleImageUrl('photo-1560958089-b8a1929cea89'),
    status: 'active',
    views: 245,
    inquiries: 12,
    createdAt: '2024-11-15',
  },
  {
    id: '2',
    title: '2022 BMW 3 Series 330i',
    price: 38500,
    mileage: 18000,
    image: getVehicleSaleImageUrl('photo-1555215695-3004980ad54e'),
    status: 'pending',
    views: 89,
    inquiries: 4,
    createdAt: '2024-11-20',
  },
];

export default function MyListingsTab() {
  const [listings] = useState<Listing[]>(mockListings);

  const getStatusBadge = (status: Listing['status']) => {
    const styles = {
      active: 'bg-green-100 text-green-800',
      pending: 'bg-yellow-100 text-yellow-800',
      sold: 'bg-gray-100 text-gray-800',
    };

    return (
      <span className={`px-3 py-1 rounded-full text-xs font-medium ${styles[status]}`}>
        {status.charAt(0).toUpperCase() + status.slice(1)}
      </span>
    );
  };

  if (listings.length === 0) {
    return (
      <div className="bg-white rounded-xl shadow-card p-12 text-center">
        <div className="flex justify-center mb-4">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
            <FiPackage size={32} className="text-gray-400" />
          </div>
        </div>
        <h3 className="text-xl font-bold font-heading text-gray-900 mb-2">
          No listings yet
        </h3>
        <p className="text-gray-600 mb-6 max-w-md mx-auto">
          Start selling your vehicle by creating your first listing.
        </p>
        <Link
          to="/sell"
          className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium"
        >
          <FiPlus size={20} />
          Create Listing
        </Link>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-card p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-2xl font-bold font-heading text-gray-900">
            My Listings
          </h2>
          <p className="text-gray-600 mt-1">
            {listings.length} {listings.length === 1 ? 'listing' : 'listings'}
          </p>
        </div>
        <Link
          to="/sell"
          className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg hover:bg-primary-600 transition-colors duration-200 font-medium"
        >
          <FiPlus size={18} />
          New Listing
        </Link>
      </div>

      <div className="space-y-4">
        {listings.map((listing) => (
          <div
            key={listing.id}
            className="border border-gray-200 rounded-lg p-4 hover:border-primary transition-colors duration-200"
          >
            <div className="flex gap-4">
              {/* Image */}
              <div className="flex-shrink-0">
                <img
                  src={listing.image}
                  alt={listing.title}
                  className="w-32 h-24 object-cover rounded-lg"
                />
              </div>

              {/* Content */}
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-4 mb-2">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900 mb-1">
                      {listing.title}
                    </h3>
                    <div className="flex items-center gap-4 text-sm text-gray-600">
                      <span>{formatPrice(listing.price)}</span>
                      <span>•</span>
                      <span>{formatMileage(listing.mileage)}</span>
                      <span>•</span>
                      <span>Listed {new Date(listing.createdAt).toLocaleDateString()}</span>
                    </div>
                  </div>
                  {getStatusBadge(listing.status)}
                </div>

                {/* Stats */}
                <div className="flex items-center gap-6 text-sm text-gray-600 mb-3">
                  <div className="flex items-center gap-1">
                    <FiEye size={16} />
                    <span>{listing.views} views</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <span className="font-medium">{listing.inquiries}</span>
                    <span>inquiries</span>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2">
                  <Link
                    to={generateListingUrl(listing.id, listing.title)}
                    className="px-3 py-1.5 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 font-medium"
                  >
                    View
                  </Link>
                  <button className="px-3 py-1.5 text-sm border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 font-medium inline-flex items-center gap-1">
                    <FiEdit2 size={14} />
                    Edit
                  </button>
                  <button className="px-3 py-1.5 text-sm border border-red-300 text-red-600 rounded-lg hover:bg-red-50 transition-colors duration-200 font-medium inline-flex items-center gap-1">
                    <FiTrash2 size={14} />
                    Delete
                  </button>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
