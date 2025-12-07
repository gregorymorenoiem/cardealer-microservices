import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { LocalizedContent } from '@/components/common';
import { FiEye, FiTrash2, FiCheck, FiX, FiSearch, FiFilter } from 'react-icons/fi';
import { getAllVehicles, approveVehicle, rejectVehicle, deleteVehicle, type Vehicle, type VehicleFilters } from '@/services/vehicleService';

type StatusFilter = 'all' | 'pending' | 'approved' | 'rejected' | 'sold';

export default function AdminListingsPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    loadVehicles();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage, statusFilter]);

  const loadVehicles = async () => {
    try {
      setIsLoading(true);
      const filters: VehicleFilters = {};
      if (searchQuery) filters.search = searchQuery;
      
      const response = await getAllVehicles(filters, currentPage, 20);
      
      // Filter by status on client side if needed
      let filteredVehicles = response.vehicles;
      if (statusFilter !== 'all') {
        filteredVehicles = response.vehicles.filter(v => v.status === statusFilter);
      }
      
      setVehicles(filteredVehicles);
      setTotalPages(response.totalPages);
    } catch (error) {
      console.error('Error loading vehicles:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleApprove = async (id: string) => {
    if (!confirm('Approve this listing?')) return;
    
    try {
      await approveVehicle(id);
      loadVehicles();
    } catch {
      alert('Failed to approve listing');
    }
  };

  const handleReject = async (id: string) => {
    const reason = prompt('Reason for rejection (optional):');
    
    try {
      await rejectVehicle(id, reason || undefined);
      loadVehicles();
    } catch {
      alert('Failed to reject listing');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this listing permanently? This action cannot be undone.')) return;
    
    try {
      await deleteVehicle(id);
      loadVehicles();
    } catch {
      alert('Failed to delete listing');
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setCurrentPage(1);
    loadVehicles();
  };

  const getStatusBadge = (status: Vehicle['status']) => {
    const styles = {
      pending: 'bg-yellow-100 text-yellow-800',
      approved: 'bg-green-100 text-green-800',
      rejected: 'bg-red-100 text-red-800',
      sold: 'bg-blue-100 text-blue-800',
    };

    return (
      <span className={`px-2 py-1 text-xs font-medium rounded-full ${styles[status]}`}>
        {status.charAt(0).toUpperCase() + status.slice(1)}
      </span>
    );
  };

  return (
    <div className="p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold font-heading text-gray-900 mb-2">
          All Listings
        </h1>
        <p className="text-gray-600">Manage all vehicle listings</p>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-card p-4 mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          {/* Search */}
          <form onSubmit={handleSearch} className="flex-1">
            <div className="relative">
              <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search listings..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
              />
            </div>
          </form>

          {/* Status Filter */}
          <select
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value as StatusFilter);
              setCurrentPage(1);
            }}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
          >
            <option value="all">All Status</option>
            <option value="pending">Pending</option>
            <option value="approved">Approved</option>
            <option value="rejected">Rejected</option>
            <option value="sold">Sold</option>
          </select>

          {/* Advanced Filters Toggle */}
          <button
            onClick={() => setShowFilters(!showFilters)}
            className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors flex items-center gap-2"
          >
            <FiFilter size={18} />
            Filters
          </button>
        </div>

        {showFilters && (
          <div className="mt-4 pt-4 border-t border-gray-200 grid md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Make</label>
              <input type="text" className="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Model</label>
              <input type="text" className="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Year</label>
              <input type="number" className="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
          </div>
        )}
      </div>

      {/* Stats Cards */}
      <div className="grid md:grid-cols-4 gap-4 mb-6">
        {[
          { label: 'Total Listings', value: vehicles.length, color: 'blue' },
          { label: 'Pending', value: vehicles.filter(v => v.status === 'pending').length, color: 'yellow' },
          { label: 'Approved', value: vehicles.filter(v => v.status === 'approved').length, color: 'green' },
          { label: 'Sold', value: vehicles.filter(v => v.status === 'sold').length, color: 'purple' },
        ].map((stat) => (
          <div key={stat.label} className="bg-white rounded-xl shadow-card p-4">
            <p className="text-sm text-gray-600 mb-1">{stat.label}</p>
            <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow-card overflow-hidden">
        {isLoading ? (
          <div className="p-12 text-center">
            <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading listings...</p>
          </div>
        ) : vehicles.length === 0 ? (
          <div className="p-12 text-center">
            <p className="text-gray-600">No listings found</p>
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Vehicle
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Seller
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Price
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Created
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {vehicles.map((vehicle) => (
                    <tr key={vehicle.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <img
                            src={vehicle.images[0] || '/placeholder-car.jpg'}
                            alt={vehicle.title}
                            className="w-16 h-16 rounded-lg object-cover"
                          />
                          <div>
                            <p className="font-medium text-gray-900">
                              <LocalizedContent content={vehicle.title} showBadge={false} />
                            </p>
                            <p className="text-sm text-gray-600">
                              {vehicle.year} • {vehicle.mileage.toLocaleString()} mi
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <p className="text-sm text-gray-900">{vehicle.sellerName}</p>
                        <p className="text-sm text-gray-600">{vehicle.sellerEmail}</p>
                      </td>
                      <td className="px-6 py-4">
                        <p className="font-medium text-gray-900">${vehicle.price.toLocaleString()}</p>
                      </td>
                      <td className="px-6 py-4">{getStatusBadge(vehicle.status)}</td>
                      <td className="px-6 py-4">
                        <p className="text-sm text-gray-900">
                          {new Date(vehicle.createdAt).toLocaleDateString()}
                        </p>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-2">
                          <Link
                            to={`/vehicles/${vehicle.id}`}
                            className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                            title="View"
                          >
                            <FiEye size={18} />
                          </Link>
                          {vehicle.status === 'pending' && (
                            <>
                              <button
                                onClick={() => handleApprove(vehicle.id)}
                                className="p-2 text-green-600 hover:bg-green-50 rounded-lg transition-colors"
                                title="Approve"
                              >
                                <FiCheck size={18} />
                              </button>
                              <button
                                onClick={() => handleReject(vehicle.id)}
                                className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                title="Reject"
                              >
                                <FiX size={18} />
                              </button>
                            </>
                          )}
                          <button
                            onClick={() => handleDelete(vehicle.id)}
                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                            title="Delete"
                          >
                            <FiTrash2 size={18} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                <p className="text-sm text-gray-600">
                  Page {currentPage} of {totalPages}
                </p>
                <div className="flex gap-2">
                  <button
                    onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                    className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Next
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
