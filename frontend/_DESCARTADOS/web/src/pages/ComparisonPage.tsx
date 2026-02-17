import React, { useState, useEffect } from 'react';
import {
  FiX,
  FiShare2,
  FiPlus,
  FiGrid,
  FiCalendar,
  FiDollarSign,
  FiActivity,
  FiDroplet,
  FiCopy,
  FiCheck,
  FiSearch,
  FiEdit3,
  FiTrash2,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import EmptyState from '@/components/organisms/EmptyState';

interface Vehicle {
  id: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage?: number;
  transmission?: string;
  fuelType?: string;
  condition?: string;
  engineSize?: string;
  horsepower?: number;
  imageUrl: string;
  location?: string;
}

interface VehicleComparison {
  id: string;
  name: string;
  vehicleCount: number;
  createdAt: string;
  isShared: boolean;
  vehicleIds: string[];
  vehicles?: Vehicle[];
}

// Mock data for development
const mockVehicles: Vehicle[] = [
  {
    id: '1',
    title: 'Toyota Camry 2022 Hybrid',
    make: 'Toyota',
    model: 'Camry',
    year: 2022,
    price: 1800000,
    mileage: 25000,
    fuelType: 'Hybrid',
    transmission: 'Automatic',
    condition: 'Excellent',
    engineSize: '2.5L',
    horsepower: 208,
    imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
    location: 'Santo Domingo',
  },
  {
    id: '2',
    title: 'Honda Accord 2023 Sport',
    make: 'Honda',
    model: 'Accord',
    year: 2023,
    price: 1950000,
    mileage: 15000,
    fuelType: 'Gasoline',
    transmission: 'CVT',
    condition: 'Like New',
    engineSize: '1.5L Turbo',
    horsepower: 192,
    imageUrl: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400',
    location: 'Santiago',
  },
  {
    id: '3',
    title: 'Nissan Altima 2021 SL',
    make: 'Nissan',
    model: 'Altima',
    year: 2021,
    price: 1650000,
    mileage: 35000,
    fuelType: 'Gasoline',
    transmission: 'CVT',
    condition: 'Good',
    engineSize: '2.5L',
    horsepower: 188,
    imageUrl: 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=400',
    location: 'La Romana',
  },
];

export function ComparisonPage() {
  const [comparisons, setComparisons] = useState<VehicleComparison[]>([]);
  const [selectedComparison, setSelectedComparison] = useState<VehicleComparison | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showVehicleModal, setShowVehicleModal] = useState(false);
  const [newComparisonName, setNewComparisonName] = useState('');
  const [selectedVehicles, setSelectedVehicles] = useState<string[]>([]);
  const [copiedShareUrl, setCopiedShareUrl] = useState(false);

  useEffect(() => {
    loadComparisons();
  }, []);

  const loadComparisons = async () => {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      // ProtectedRoute ya se encarga de la redirección, no necesitamos hacerlo aquí
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      // Mock data for now - replace with actual API call
      const mockComparisons: VehicleComparison[] = [
        {
          id: 'comp-1',
          name: 'Family Sedans',
          vehicleCount: 3,
          createdAt: new Date().toISOString(),
          isShared: true,
          vehicleIds: ['1', '2', '3'],
          vehicles: mockVehicles.filter((v) => ['1', '2', '3'].includes(v.id)),
        },
        {
          id: 'comp-2',
          name: 'Compact Cars',
          vehicleCount: 2,
          createdAt: new Date(Date.now() - 86400000).toISOString(),
          isShared: false,
          vehicleIds: ['2', '3'],
          vehicles: mockVehicles.filter((v) => ['2', '3'].includes(v.id)),
        },
      ];

      setComparisons(mockComparisons);
      if (mockComparisons.length > 0) {
        setSelectedComparison(mockComparisons[0]);
      }
    } catch (error) {
      console.error('Failed to load comparisons:', error);
    } finally {
      setLoading(false);
    }
  };

  const createComparison = async () => {
    if (!newComparisonName.trim() || selectedVehicles.length === 0) {
      return;
    }

    try {
      const newComparison: VehicleComparison = {
        id: `comp-${Date.now()}`,
        name: newComparisonName,
        vehicleCount: selectedVehicles.length,
        createdAt: new Date().toISOString(),
        isShared: false,
        vehicleIds: selectedVehicles,
        vehicles: mockVehicles.filter((v) => selectedVehicles.includes(v.id)),
      };

      setComparisons((prev) => [newComparison, ...prev]);
      setSelectedComparison(newComparison);
      setShowCreateModal(false);
      setNewComparisonName('');
      setSelectedVehicles([]);
    } catch (error) {
      console.error('Failed to create comparison:', error);
    }
  };

  const deleteComparison = async (comparisonId: string) => {
    if (!confirm('Are you sure you want to delete this comparison?')) return;

    try {
      setComparisons((prev) => prev.filter((c) => c.id !== comparisonId));

      if (selectedComparison?.id === comparisonId) {
        const remaining = comparisons.filter((c) => c.id !== comparisonId);
        setSelectedComparison(remaining.length > 0 ? remaining[0] : null);
      }
    } catch (error) {
      console.error('Failed to delete comparison:', error);
    }
  };

  const shareComparison = async (comparisonId: string) => {
    try {
      const shareToken = Math.random().toString(36).substring(2, 18);
      const shareUrl = `${window.location.origin}/comparison/shared/${shareToken}`;

      await navigator.clipboard.writeText(shareUrl);
      setCopiedShareUrl(true);
      setTimeout(() => setCopiedShareUrl(false), 2000);

      // Update comparison as shared
      setComparisons((prev) =>
        prev.map((c) => (c.id === comparisonId ? { ...c, isShared: true } : c))
      );
    } catch (error) {
      console.error('Failed to share comparison:', error);
    }
  };

  const removeVehicle = (vehicleId: string) => {
    if (!selectedComparison) return;

    const updatedVehicles = selectedComparison.vehicles?.filter((v) => v.id !== vehicleId) || [];

    if (updatedVehicles.length === 0) {
      alert('Cannot remove the last vehicle from comparison');
      return;
    }

    const updatedComparison = {
      ...selectedComparison,
      vehicles: updatedVehicles,
      vehicleIds: updatedVehicles.map((v) => v.id),
      vehicleCount: updatedVehicles.length,
    };

    setSelectedComparison(updatedComparison);
    setComparisons((prev) =>
      prev.map((c) => (c.id === selectedComparison.id ? updatedComparison : c))
    );
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const formatMileage = (mileage?: number) => {
    if (!mileage) return 'N/A';
    return new Intl.NumberFormat('es-DO').format(mileage) + ' km';
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 py-8">
          <div className="animate-pulse">
            <div className="h-8 bg-gray-200 rounded w-1/3 mb-6"></div>
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
              <div className="lg:col-span-1 space-y-4">
                {Array(3)
                  .fill(0)
                  .map((_, i) => (
                    <div key={i} className="h-24 bg-gray-200 rounded"></div>
                  ))}
              </div>
              <div className="lg:col-span-3">
                <div className="h-96 bg-gray-200 rounded"></div>
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
              <FiGrid className="text-blue-600" />
              Vehicle Comparisons
            </h1>
            <p className="text-gray-600 mt-1">
              Compare up to 3 vehicles side by side to make the best choice
            </p>
          </div>

          <Button
            onClick={() => setShowCreateModal(true)}
            className="bg-blue-600 hover:bg-blue-700 text-white flex items-center gap-2"
          >
            <FiPlus />
            New Comparison
          </Button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Comparisons Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow-sm border">
              <div className="p-4 border-b">
                <h2 className="text-lg font-semibold">My Comparisons</h2>
              </div>
              <div className="p-0">
                {comparisons.length === 0 ? (
                  <div className="p-6 text-center">
                    <FiGrid className="mx-auto h-12 w-12 text-gray-300 mb-3" />
                    <p className="text-gray-500 text-sm">No comparisons yet</p>
                    <p className="text-xs text-gray-400 mt-1">
                      Create your first comparison to get started
                    </p>
                  </div>
                ) : (
                  <div className="space-y-1">
                    {comparisons.map((comparison) => (
                      <div
                        key={comparison.id}
                        className={`p-4 cursor-pointer transition-colors border-l-4 ${
                          selectedComparison?.id === comparison.id
                            ? 'bg-blue-50 border-l-blue-500'
                            : 'border-l-transparent hover:bg-gray-50'
                        }`}
                        onClick={() => setSelectedComparison(comparison)}
                      >
                        <div className="flex justify-between items-start mb-2">
                          <h3 className="font-medium text-sm line-clamp-2">{comparison.name}</h3>
                          <div className="flex gap-1 ml-2">
                            {comparison.isShared && (
                              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-green-100 text-green-800">
                                <FiShare2 className="h-3 w-3 mr-1" />
                                Shared
                              </span>
                            )}
                          </div>
                        </div>
                        <div className="flex items-center justify-between">
                          <p className="text-xs text-gray-500">
                            <FaCar className="inline h-3 w-3 mr-1" />
                            {comparison.vehicleCount} vehicles
                          </p>
                          <div className="flex gap-1">
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                shareComparison(comparison.id);
                              }}
                              className="p-1 text-gray-400 hover:text-blue-600 rounded"
                            >
                              <FiShare2 className="h-3 w-3" />
                            </button>
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                deleteComparison(comparison.id);
                              }}
                              className="p-1 text-gray-400 hover:text-red-600 rounded"
                            >
                              <FiTrash2 className="h-3 w-3" />
                            </button>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Comparison Details */}
          <div className="lg:col-span-3">
            {selectedComparison ? (
              <div className="bg-white rounded-lg shadow-sm border">
                <div className="p-6 border-b">
                  <div className="flex justify-between items-start">
                    <div>
                      <h2 className="text-xl font-semibold flex items-center gap-3">
                        {selectedComparison.name}
                        {selectedComparison.isShared && (
                          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs bg-green-100 text-green-800">
                            <FiShare2 className="h-3 w-3 mr-1" />
                            Shared
                          </span>
                        )}
                      </h2>
                      <p className="text-sm text-gray-500 mt-1">
                        <FiCalendar className="inline h-3 w-3 mr-1" />
                        Created {new Date(selectedComparison.createdAt).toLocaleDateString()}
                      </p>
                    </div>
                    <div className="flex gap-2">
                      <Button
                        onClick={() => shareComparison(selectedComparison.id)}
                        variant="outline"
                        size="sm"
                      >
                        {copiedShareUrl ? (
                          <>
                            <FiCheck className="h-4 w-4 mr-2" />
                            Copied!
                          </>
                        ) : (
                          <>
                            <FiShare2 className="h-4 w-4 mr-2" />
                            Share
                          </>
                        )}
                      </Button>
                    </div>
                  </div>
                </div>

                <div className="p-6">
                  {!selectedComparison.vehicles || selectedComparison.vehicles.length === 0 ? (
                    <div className="text-center py-12">
                      <FaCar className="mx-auto h-16 w-16 text-gray-300 mb-4" />
                      <h3 className="text-lg font-medium text-gray-900 mb-2">
                        No Vehicles in Comparison
                      </h3>
                      <p className="text-gray-500">
                        Add vehicles to start comparing their specifications
                      </p>
                    </div>
                  ) : (
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b">
                            <th className="text-left p-4 font-medium text-gray-600 w-32">
                              Specification
                            </th>
                            {selectedComparison.vehicles.map((vehicle) => (
                              <th key={vehicle.id} className="text-center p-4 min-w-64">
                                <div className="space-y-3">
                                  <div className="relative">
                                    <img
                                      src={vehicle.imageUrl}
                                      alt={vehicle.title}
                                      className="w-full h-40 object-cover rounded-lg"
                                    />
                                    <button
                                      onClick={() => removeVehicle(vehicle.id)}
                                      className="absolute top-2 right-2 p-2 bg-white/90 hover:bg-white rounded-full shadow-sm"
                                    >
                                      <FiX className="h-4 w-4" />
                                    </button>
                                  </div>
                                  <div>
                                    <h3 className="font-semibold text-sm">{vehicle.title}</h3>
                                    <p className="text-lg font-bold text-blue-600">
                                      {formatPrice(vehicle.price)}
                                    </p>
                                  </div>
                                </div>
                              </th>
                            ))}
                          </tr>
                        </thead>
                        <tbody>
                          {[
                            { label: 'Make', key: 'make', icon: FaCar },
                            { label: 'Model', key: 'model', icon: FaCar },
                            { label: 'Year', key: 'year', icon: FiCalendar },
                            {
                              label: 'Price',
                              key: 'price',
                              icon: FiDollarSign,
                              formatter: formatPrice,
                            },
                            {
                              label: 'Mileage',
                              key: 'mileage',
                              icon: FiActivity,
                              formatter: formatMileage,
                            },
                            { label: 'Fuel Type', key: 'fuelType', icon: FiDroplet },
                            { label: 'Transmission', key: 'transmission', icon: FaCar },
                            { label: 'Condition', key: 'condition', icon: FaCar },
                            { label: 'Engine Size', key: 'engineSize', icon: FaCar },
                            { label: 'Horsepower', key: 'horsepower', icon: FiActivity },
                            { label: 'Location', key: 'location', icon: FaCar },
                          ].map((spec) => (
                            <tr key={spec.key} className="border-b hover:bg-gray-50">
                              <td className="p-4 font-medium text-gray-700">
                                <spec.icon className="inline h-4 w-4 mr-2" />
                                {spec.label}
                              </td>
                              {selectedComparison.vehicles!.map((vehicle) => (
                                <td key={vehicle.id} className="p-4 text-center">
                                  {spec.formatter
                                    ? spec.formatter(vehicle[spec.key as keyof Vehicle] as any)
                                    : (vehicle[spec.key as keyof Vehicle] as string) || 'N/A'}
                                </td>
                              ))}
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="bg-white rounded-lg shadow-sm border">
                <div className="flex flex-col items-center justify-center py-16">
                  <FiGrid className="h-16 w-16 text-gray-300 mb-4" />
                  <h3 className="text-xl font-medium text-gray-900 mb-2">Select a Comparison</h3>
                  <p className="text-gray-500 text-center">
                    Choose a comparison from the sidebar to view detailed specifications,
                    <br />
                    or create a new one to get started.
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Create Comparison Modal */}
        {showCreateModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
              <div className="p-6 border-b">
                <h3 className="text-lg font-semibold">Create New Comparison</h3>
              </div>
              <div className="p-6 space-y-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Comparison Name
                  </label>
                  <input
                    type="text"
                    value={newComparisonName}
                    onChange={(e) => setNewComparisonName(e.target.value)}
                    placeholder="e.g., Family Sedans, Compact Cars"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <div className="flex justify-between items-center mb-3">
                    <label className="block text-sm font-medium text-gray-700">
                      Select Vehicles (max 3)
                    </label>
                    <Button onClick={() => setShowVehicleModal(true)} variant="outline" size="sm">
                      <FiSearch className="mr-2 h-4 w-4" />
                      Browse Vehicles
                    </Button>
                  </div>

                  <div className="space-y-2">
                    {selectedVehicles.map((vehicleId) => {
                      const vehicle = mockVehicles.find((v) => v.id === vehicleId);
                      if (!vehicle) return null;

                      return (
                        <div
                          key={vehicleId}
                          className="flex items-center justify-between p-3 bg-gray-50 rounded"
                        >
                          <div className="flex items-center gap-3">
                            <img
                              src={vehicle.imageUrl}
                              alt={vehicle.title}
                              className="w-10 h-10 object-cover rounded"
                            />
                            <div>
                              <p className="font-medium text-sm">{vehicle.title}</p>
                              <p className="text-xs text-gray-500">{formatPrice(vehicle.price)}</p>
                            </div>
                          </div>
                          <button
                            onClick={() =>
                              setSelectedVehicles((prev) => prev.filter((id) => id !== vehicleId))
                            }
                            className="p-1 text-gray-400 hover:text-red-600 rounded"
                          >
                            <FiX className="h-4 w-4" />
                          </button>
                        </div>
                      );
                    })}

                    {selectedVehicles.length === 0 && (
                      <p className="text-sm text-gray-500 text-center py-4">
                        No vehicles selected. Click "Browse Vehicles" to add some.
                      </p>
                    )}
                  </div>
                </div>

                <div className="flex justify-end gap-3">
                  <Button
                    onClick={() => {
                      setShowCreateModal(false);
                      setNewComparisonName('');
                      setSelectedVehicles([]);
                    }}
                    variant="outline"
                  >
                    Cancel
                  </Button>
                  <Button
                    onClick={createComparison}
                    disabled={!newComparisonName.trim() || selectedVehicles.length === 0}
                    className="bg-blue-600 hover:bg-blue-700 text-white"
                  >
                    Create Comparison
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Vehicle Selection Modal */}
        {showVehicleModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg max-w-4xl w-full mx-4 max-h-[80vh] overflow-y-auto">
              <div className="p-6 border-b">
                <h3 className="text-lg font-semibold">Select Vehicles to Compare</h3>
              </div>
              <div className="p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {mockVehicles.map((vehicle) => (
                    <div
                      key={vehicle.id}
                      className={`border rounded-lg p-4 cursor-pointer transition-all ${
                        selectedVehicles.includes(vehicle.id)
                          ? 'ring-2 ring-blue-500 bg-blue-50'
                          : 'hover:shadow-md'
                      }`}
                      onClick={() => {
                        if (selectedVehicles.includes(vehicle.id)) {
                          setSelectedVehicles((prev) => prev.filter((id) => id !== vehicle.id));
                        } else if (selectedVehicles.length < 3) {
                          setSelectedVehicles((prev) => [...prev, vehicle.id]);
                        } else {
                          alert('Maximum 3 vehicles allowed');
                        }
                      }}
                    >
                      <div className="flex gap-3">
                        <img
                          src={vehicle.imageUrl}
                          alt={vehicle.title}
                          className="w-16 h-16 object-cover rounded"
                        />
                        <div className="flex-1">
                          <h3 className="font-semibold text-sm">{vehicle.title}</h3>
                          <p className="text-blue-600 font-bold">{formatPrice(vehicle.price)}</p>
                          <p className="text-sm text-gray-500">
                            {vehicle.year} • {formatMileage(vehicle.mileage)}
                          </p>
                        </div>
                        {selectedVehicles.includes(vehicle.id) && (
                          <FiCheck className="text-blue-600 flex-shrink-0" />
                        )}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="flex justify-between items-center mt-6 pt-4 border-t">
                  <p className="text-sm text-gray-600">
                    {selectedVehicles.length}/3 vehicles selected
                  </p>
                  <Button onClick={() => setShowVehicleModal(false)}>
                    Done ({selectedVehicles.length})
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
