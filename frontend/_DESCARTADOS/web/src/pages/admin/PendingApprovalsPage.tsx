import { useState, useCallback } from 'react';
import { usePendingApprovalsPage } from '../../hooks';
import { LocalizedContent } from '@/components/common';
import type { PendingVehicle } from '../../types/admin';
import { FiCheck, FiX, FiEye, FiUser, FiCalendar, FiRefreshCw } from 'react-icons/fi';

const PendingApprovalsPage = () => {
  const { 
    vehicles, 
    total, 
    isLoading, 
    refetch, 
    approveVehicle, 
    rejectVehicle 
  } = usePendingApprovalsPage();
  
  const [selectedVehicle, setSelectedVehicle] = useState<PendingVehicle | null>(null);
  const [rejectReason, setRejectReason] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [detailsVehicle, setDetailsVehicle] = useState<PendingVehicle | null>(null);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatMileage = (mileage: number) => {
    return new Intl.NumberFormat('en-US').format(mileage) + ' miles';
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

  const handleViewDetails = (vehicle: PendingVehicle) => {
    setDetailsVehicle(vehicle);
    setShowDetailsModal(true);
    setCurrentImageIndex(0);
  };

  const handleCloseModal = () => {
    setShowDetailsModal(false);
    setDetailsVehicle(null);
    setRejectReason('');
    setCurrentImageIndex(0);
  };

  const handleApprove = useCallback(async (vehicleId: string) => {
    setIsProcessing(true);
    
    try {
      await approveVehicle.mutateAsync(vehicleId);
      refetch();
    } catch (error) {
      console.error('Error approving vehicle:', error);
    }
    
    setSelectedVehicle(null);
    setShowDetailsModal(false);
    setIsProcessing(false);
    
    console.log('Approved vehicle:', vehicleId);
  }, [approveVehicle, refetch]);

  const handleReject = useCallback(async (vehicleId: string) => {
    if (!rejectReason.trim()) {
      alert('Please provide a reason for rejection');
      return;
    }
    
    setIsProcessing(true);
    
    try {
      await rejectVehicle.mutateAsync({ vehicleId, reason: rejectReason });
      refetch();
    } catch (error) {
      console.error('Error rejecting vehicle:', error);
    }
    
    setSelectedVehicle(null);
    setRejectReason('');
    setShowDetailsModal(false);
    setIsProcessing(false);
    
    console.log('Rejected vehicle:', vehicleId, 'Reason:', rejectReason);
  }, [rejectVehicle, rejectReason, refetch]);

  return (
    <div>
      {/* Header */}
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Pending Approvals</h1>
          <p className="text-gray-600 mt-1">
            {total} listing{total !== 1 ? 's' : ''} waiting for review
          </p>
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

      {/* Loading State */}
      {isLoading && (
        <div className="text-center py-8 text-gray-500">Loading pending approvals...</div>
      )}

      {!isLoading && vehicles.length === 0 ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <FiCheck className="w-8 h-8 text-green-600" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">All caught up!</h3>
          <p className="text-gray-600">There are no pending listings to review.</p>
        </div>
      ) : (
        <div className="space-y-6">
          {vehicles.map((vehicle) => (
            <div
              key={vehicle.id}
              className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden"
            >
              <div className="p-6">
                <div className="flex gap-6">
                  {/* Vehicle Image */}
                  <img
                    src={vehicle.images[0]}
                    alt={vehicle.title}
                    className="w-48 h-36 rounded-lg object-cover flex-shrink-0"
                  />

                  {/* Vehicle Info */}
                  <div className="flex-1">
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h3 className="text-xl font-semibold text-gray-900 mb-1">
                          <LocalizedContent content={vehicle.title} showBadge={false} />
                        </h3>
                        <p className="text-2xl font-bold text-primary">{formatPrice(vehicle.price)}</p>
                      </div>
                      <span className="px-3 py-1 bg-orange-100 text-orange-800 text-sm font-medium rounded-full">
                        Pending Review
                      </span>
                    </div>

                    <div className="grid grid-cols-2 gap-4 mb-4">
                      <div>
                        <p className="text-sm text-gray-600">Mileage</p>
                        <p className="font-medium text-gray-900">{formatMileage(vehicle.mileage)}</p>
                      </div>
                      <div>
                        <p className="text-sm text-gray-600">Year</p>
                        <p className="font-medium text-gray-900">{vehicle.year}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-4 text-sm text-gray-600 mb-4">
                      <div className="flex items-center gap-2">
                        <FiUser className="w-4 h-4" />
                        <span>{vehicle.sellerName}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <FiCalendar className="w-4 h-4" />
                        <span>Submitted {formatTimeAgo(vehicle.submittedAt)}</span>
                      </div>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex gap-3">
                      <button
                        onClick={() => handleViewDetails(vehicle)}
                        className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
                      >
                        <FiEye className="w-4 h-4" />
                        View Details
                      </button>
                      <button
                        onClick={() => handleApprove(vehicle.id)}
                        disabled={isProcessing}
                        className="flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <FiCheck className="w-4 h-4" />
                        Quick Approve
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              {/* Reject Form (if selected) */}
              {selectedVehicle?.id === vehicle.id && (
                <div className="border-t border-gray-200 bg-gray-50 p-6">
                  <h4 className="font-semibold text-gray-900 mb-3">Rejection Reason</h4>
                  <textarea
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                    placeholder="Please provide a detailed reason for rejection..."
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent resize-none"
                    rows={4}
                  />
                  <div className="flex gap-3 mt-4">
                    <button
                      onClick={() => {
                        setSelectedVehicle(null);
                        setRejectReason('');
                      }}
                      className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100 transition-colors"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={() => handleReject(vehicle.id)}
                      disabled={!rejectReason.trim() || isProcessing}
                      className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      {isProcessing ? 'Processing...' : 'Confirm Rejection'}
                    </button>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Details Modal */}
      {showDetailsModal && detailsVehicle && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl max-w-5xl w-full max-h-[90vh] overflow-y-auto">
            {/* Modal Header */}
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
              <h2 className="text-2xl font-bold text-gray-900">Vehicle Details</h2>
              <button
                onClick={handleCloseModal}
                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
              >
                <FiX className="w-6 h-6 text-gray-600" />
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6">
              {/* Image Gallery */}
              <div className="mb-6">
                <div className="relative aspect-video rounded-lg overflow-hidden bg-gray-100">
                  <img
                    src={detailsVehicle.images[currentImageIndex]}
                    alt={detailsVehicle.title}
                    className="w-full h-full object-cover"
                  />
                  
                  {/* Image Navigation */}
                  {detailsVehicle.images.length > 1 && (
                    <>
                      <button
                        onClick={() => setCurrentImageIndex((prev) => 
                          prev === 0 ? detailsVehicle.images.length - 1 : prev - 1
                        )}
                        className="absolute left-4 top-1/2 -translate-y-1/2 p-2 bg-white rounded-full shadow-lg hover:bg-gray-100"
                      >
                        ‹
                      </button>
                      <button
                        onClick={() => setCurrentImageIndex((prev) => 
                          prev === detailsVehicle.images.length - 1 ? 0 : prev + 1
                        )}
                        className="absolute right-4 top-1/2 -translate-y-1/2 p-2 bg-white rounded-full shadow-lg hover:bg-gray-100"
                      >
                        ›
                      </button>
                      <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
                        {detailsVehicle.images.map((_, index) => (
                          <button
                            key={index}
                            onClick={() => setCurrentImageIndex(index)}
                            className={`w-2 h-2 rounded-full ${
                              index === currentImageIndex ? 'bg-white' : 'bg-white/50'
                            }`}
                          />
                        ))}
                      </div>
                    </>
                  )}
                </div>

                {/* Thumbnails */}
                {detailsVehicle.images.length > 1 && (
                  <div className="flex gap-2 mt-4 overflow-x-auto">
                    {detailsVehicle.images.map((image, index) => (
                      <button
                        key={index}
                        onClick={() => setCurrentImageIndex(index)}
                        className={`flex-shrink-0 w-20 h-20 rounded-lg overflow-hidden border-2 ${
                          index === currentImageIndex ? 'border-primary' : 'border-gray-200'
                        }`}
                      >
                        <img src={image} alt={`Thumbnail ${index + 1}`} className="w-full h-full object-cover" />
                      </button>
                    ))}
                  </div>
                )}
              </div>

              {/* Vehicle Information */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
                {/* Left Column */}
                <div>
                  <h3 className="text-2xl font-bold text-gray-900 mb-2">{detailsVehicle.title}</h3>
                  <p className="text-3xl font-bold text-primary mb-4">{formatPrice(detailsVehicle.price)}</p>

                  <div className="space-y-3">
                    <div className="flex items-center justify-between py-2 border-b border-gray-200">
                      <span className="text-gray-600">Brand</span>
                      <span className="font-semibold text-gray-900">{detailsVehicle.brand}</span>
                    </div>
                    <div className="flex items-center justify-between py-2 border-b border-gray-200">
                      <span className="text-gray-600">Model</span>
                      <span className="font-semibold text-gray-900">{detailsVehicle.model}</span>
                    </div>
                    <div className="flex items-center justify-between py-2 border-b border-gray-200">
                      <span className="text-gray-600">Year</span>
                      <span className="font-semibold text-gray-900">{detailsVehicle.year}</span>
                    </div>
                    <div className="flex items-center justify-between py-2 border-b border-gray-200">
                      <span className="text-gray-600">Mileage</span>
                      <span className="font-semibold text-gray-900">{formatMileage(detailsVehicle.mileage)}</span>
                    </div>
                    <div className="flex items-center justify-between py-2 border-b border-gray-200">
                      <span className="text-gray-600">Status</span>
                      <span className="px-3 py-1 bg-orange-100 text-orange-800 text-sm font-medium rounded-full">
                        Pending Review
                      </span>
                    </div>
                  </div>
                </div>

                {/* Right Column - Seller Info */}
                <div>
                  <h4 className="text-lg font-bold text-gray-900 mb-4">Seller Information</h4>
                  <div className="bg-gray-50 rounded-lg p-4 space-y-3">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 bg-primary-100 rounded-full flex items-center justify-center">
                        <FiUser className="w-5 h-5 text-primary" />
                      </div>
                      <div>
                        <p className="font-semibold text-gray-900">{detailsVehicle.sellerName}</p>
                        <p className="text-sm text-gray-600">Seller ID: {detailsVehicle.sellerId}</p>
                      </div>
                    </div>
                    
                    <div className="flex items-center gap-2 text-gray-700">
                      <FiMail className="w-4 h-4" />
                      <span className="text-sm">{detailsVehicle.sellerEmail}</span>
                    </div>
                    
                    <div className="flex items-center gap-2 text-gray-700">
                      <FiCalendar className="w-4 h-4" />
                      <span className="text-sm">Submitted {formatTimeAgo(detailsVehicle.submittedAt)}</span>
                    </div>
                  </div>

                  {/* Additional Info */}
                  <div className="mt-6 bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <p className="text-sm text-blue-900 font-medium mb-2">Review Checklist</p>
                    <ul className="space-y-1 text-sm text-blue-700">
                      <li>✓ All images provided ({detailsVehicle.images.length} photos)</li>
                      <li>✓ Pricing is within market range</li>
                      <li>✓ Vehicle specifications complete</li>
                      <li>✓ Seller information verified</li>
                    </ul>
                  </div>
                </div>
              </div>

              {/* Action Section */}
              <div className="border-t border-gray-200 pt-6">
                <h4 className="text-lg font-bold text-gray-900 mb-4">Approval Decision</h4>
                
                {/* Rejection Reason Input */}
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Rejection Reason (Optional - only if rejecting)
                  </label>
                  <textarea
                    value={rejectReason}
                    onChange={(e) => setRejectReason(e.target.value)}
                    placeholder="Provide a detailed reason if you're rejecting this listing..."
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent resize-none"
                    rows={4}
                  />
                </div>

                {/* Action Buttons */}
                <div className="flex gap-4">
                  <button
                    onClick={() => handleApprove(detailsVehicle.id)}
                    disabled={isProcessing}
                    className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
                  >
                    <FiCheck className="w-5 h-5" />
                    {isProcessing ? 'Processing...' : 'Approve Listing'}
                  </button>
                  
                  <button
                    onClick={() => handleReject(detailsVehicle.id)}
                    disabled={!rejectReason.trim() || isProcessing}
                    className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
                  >
                    <FiX className="w-5 h-5" />
                    {isProcessing ? 'Processing...' : 'Reject Listing'}
                  </button>
                  
                  <button
                    onClick={handleCloseModal}
                    disabled={isProcessing}
                    className="px-6 py-3 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Cancel
                  </button>
                </div>

                <p className="text-sm text-gray-500 mt-4">
                  * Approved listings will be published immediately. Rejected listings will notify the seller via email.
                </p>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PendingApprovalsPage;
