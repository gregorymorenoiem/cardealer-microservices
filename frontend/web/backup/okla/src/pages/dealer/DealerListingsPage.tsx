/**
 * Dealer Listings Page
 * 
 * CRUD for dealer's vehicle/property listings
 * Features plan-based limits on active listings
 */

import { useState, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { LocalizedContent } from '@/components/common';
import {
  Plus,
  Search,
  Filter,
  Eye,
  Edit2,
  Trash2,
  Star,
  StarOff,
  AlertCircle,
  CheckCircle,
  Clock,
  XCircle,
  Car,
  Building2,
  ImageIcon,
  TrendingUp,
  Calendar,
  DollarSign,
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';

// Mock Listing type
interface Listing {
  id: string;
  type: 'vehicle' | 'property';
  title: string;
  price: number;
  status: 'active' | 'pending' | 'sold' | 'draft' | 'rejected';
  isFeatured: boolean;
  views: number;
  inquiries: number;
  image: string;
  createdAt: string;
  updatedAt: string;
  // Vehicle specific
  make?: string;
  model?: string;
  year?: number;
  mileage?: number;
  // Property specific
  propertyType?: string;
  bedrooms?: number;
  bathrooms?: number;
  area?: number;
}

// Mock listings data
const mockListings: Listing[] = [
  {
    id: 'listing-001',
    type: 'vehicle',
    title: 'Toyota Camry 2023',
    make: 'Toyota',
    model: 'Camry',
    year: 2023,
    mileage: 15000,
    price: 485000,
    status: 'active',
    isFeatured: true,
    views: 234,
    inquiries: 12,
    image: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
    createdAt: '2025-01-01T10:00:00Z',
    updatedAt: '2025-01-15T14:30:00Z',
  },
  {
    id: 'listing-002',
    type: 'vehicle',
    title: 'Honda CR-V 2024',
    make: 'Honda',
    model: 'CR-V',
    year: 2024,
    mileage: 5000,
    price: 620000,
    status: 'active',
    isFeatured: false,
    views: 189,
    inquiries: 8,
    image: 'https://images.unsplash.com/photo-1568844293986-8c2bfd4b01e5?w=400',
    createdAt: '2025-01-05T09:00:00Z',
    updatedAt: '2025-01-14T11:00:00Z',
  },
  {
    id: 'listing-003',
    type: 'vehicle',
    title: 'BMW X5 2022',
    make: 'BMW',
    model: 'X5',
    year: 2022,
    mileage: 28000,
    price: 1150000,
    status: 'pending',
    isFeatured: false,
    views: 0,
    inquiries: 0,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400',
    createdAt: '2025-01-15T08:00:00Z',
    updatedAt: '2025-01-15T08:00:00Z',
  },
  {
    id: 'listing-004',
    type: 'property',
    title: 'Casa Moderna en Polanco',
    propertyType: 'Casa',
    bedrooms: 4,
    bathrooms: 3,
    area: 320,
    price: 12500000,
    status: 'active',
    isFeatured: true,
    views: 456,
    inquiries: 23,
    image: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=400',
    createdAt: '2024-12-20T10:00:00Z',
    updatedAt: '2025-01-12T16:00:00Z',
  },
  {
    id: 'listing-005',
    type: 'vehicle',
    title: 'Mercedes-Benz C-Class 2023',
    make: 'Mercedes-Benz',
    model: 'C-Class',
    year: 2023,
    mileage: 12000,
    price: 890000,
    status: 'sold',
    isFeatured: false,
    views: 567,
    inquiries: 34,
    image: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400',
    createdAt: '2024-11-15T10:00:00Z',
    updatedAt: '2025-01-10T09:00:00Z',
  },
  {
    id: 'listing-006',
    type: 'property',
    title: 'Departamento en Santa Fe',
    propertyType: 'Departamento',
    bedrooms: 2,
    bathrooms: 2,
    area: 95,
    price: 4500000,
    status: 'draft',
    isFeatured: false,
    views: 0,
    inquiries: 0,
    image: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=400',
    createdAt: '2025-01-14T10:00:00Z',
    updatedAt: '2025-01-14T10:00:00Z',
  },
  {
    id: 'listing-007',
    type: 'vehicle',
    title: 'Ford Mustang 2024',
    make: 'Ford',
    model: 'Mustang',
    year: 2024,
    mileage: 2000,
    price: 980000,
    status: 'rejected',
    isFeatured: false,
    views: 0,
    inquiries: 0,
    image: 'https://images.unsplash.com/photo-1584345604476-8ec5e12e42dd?w=400',
    createdAt: '2025-01-13T10:00:00Z',
    updatedAt: '2025-01-14T08:00:00Z',
  },
];

const DealerListingsPage = () => {
  const { usage, limits, hasReachedLimit, getUsagePercentage } = usePermissions();
  
  const [listings, setListings] = useState<Listing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [listingToDelete, setListingToDelete] = useState<string | null>(null);

  // Load mock data
  useEffect(() => {
    const loadListings = async () => {
      setIsLoading(true);
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 500));
      setListings(mockListings);
      setIsLoading(false);
    };
    loadListings();
  }, []);

  // Filter listings
  const filteredListings = useMemo(() => {
    return listings.filter(listing => {
      const matchesSearch = listing.title.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesStatus = statusFilter === 'all' || listing.status === statusFilter;
      const matchesType = typeFilter === 'all' || listing.type === typeFilter;
      return matchesSearch && matchesStatus && matchesType;
    });
  }, [listings, searchQuery, statusFilter, typeFilter]);

  // Stats
  const stats = useMemo(() => {
    const active = listings.filter(l => l.status === 'active').length;
    const pending = listings.filter(l => l.status === 'pending').length;
    const featured = listings.filter(l => l.isFeatured).length;
    const totalViews = listings.reduce((sum, l) => sum + l.views, 0);
    return { active, pending, featured, totalViews };
  }, [listings]);

  // Toggle featured
  const toggleFeatured = (id: string) => {
    const currentFeatured = listings.filter(l => l.isFeatured).length;
    const listing = listings.find(l => l.id === id);
    if (!listing?.isFeatured && currentFeatured >= limits.maxFeaturedListings && limits.maxFeaturedListings !== -1) {
      alert('Has alcanzado el límite de publicaciones destacadas. Actualiza tu plan.');
      return;
    }
    setListings(prev => 
      prev.map(l => l.id === id ? { ...l, isFeatured: !l.isFeatured } : l)
    );
  };

  // Delete listing
  const handleDelete = (id: string) => {
    setListingToDelete(id);
    setShowDeleteModal(true);
  };

  const confirmDelete = () => {
    if (listingToDelete) {
      setListings(prev => prev.filter(l => l.id !== listingToDelete));
      setListingToDelete(null);
      setShowDeleteModal(false);
    }
  };

  // Status badge component
  const StatusBadge = ({ status }: { status: Listing['status'] }) => {
    const config = {
      active: { icon: CheckCircle, color: 'bg-green-100 text-green-700', label: 'Activo' },
      pending: { icon: Clock, color: 'bg-yellow-100 text-yellow-700', label: 'Pendiente' },
      sold: { icon: DollarSign, color: 'bg-blue-100 text-blue-700', label: 'Vendido' },
      draft: { icon: Edit2, color: 'bg-gray-100 text-gray-600', label: 'Borrador' },
      rejected: { icon: XCircle, color: 'bg-red-100 text-red-700', label: 'Rechazado' },
    };
    const { icon: Icon, color, label } = config[status];
    return (
      <span className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${color}`}>
        <Icon className="h-3 w-3" />
        {label}
      </span>
    );
  };

  // Format price
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);
  };

  // Format date
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-MX', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        {/* Header skeleton */}
        <div className="flex justify-between items-center">
          <div className="h-8 w-48 bg-gray-200 rounded animate-pulse" />
          <div className="h-10 w-40 bg-gray-200 rounded animate-pulse" />
        </div>
        {/* Cards skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="h-24 bg-gray-200 rounded-lg animate-pulse" />
          ))}
        </div>
        {/* Table skeleton */}
        <div className="bg-white rounded-lg border border-gray-200 p-4 space-y-4">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="h-20 bg-gray-100 rounded animate-pulse" />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mis Publicaciones</h1>
          <p className="text-gray-600 mt-1">
            Gestiona tus listados de vehículos y propiedades
          </p>
        </div>
        <Link
          to="/dealer/listings/new"
          className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors ${
            hasReachedLimit('listings')
              ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
              : 'bg-blue-600 text-white hover:bg-blue-700'
          }`}
          onClick={e => hasReachedLimit('listings') && e.preventDefault()}
        >
          <Plus className="h-5 w-5" />
          Nueva Publicación
        </Link>
      </div>

      {/* Usage Warning */}
      {getUsagePercentage('listings') >= 80 && (
        <div className={`flex items-start gap-3 p-4 rounded-lg ${
          hasReachedLimit('listings') 
            ? 'bg-red-50 border border-red-200' 
            : 'bg-yellow-50 border border-yellow-200'
        }`}>
          <AlertCircle className={`h-5 w-5 flex-shrink-0 ${
            hasReachedLimit('listings') ? 'text-red-500' : 'text-yellow-500'
          }`} />
          <div>
            <p className={`font-medium ${
              hasReachedLimit('listings') ? 'text-red-800' : 'text-yellow-800'
            }`}>
              {hasReachedLimit('listings') 
                ? 'Has alcanzado el límite de publicaciones'
                : 'Te estás acercando al límite de publicaciones'
              }
            </p>
            <p className={`text-sm mt-0.5 ${
              hasReachedLimit('listings') ? 'text-red-600' : 'text-yellow-600'
            }`}>
              {usage.listings} de {limits.maxListings === -1 ? '∞' : limits.maxListings} publicaciones utilizadas.{' '}
              <Link to="/dealer/plans" className="underline font-medium">
                Actualizar plan
              </Link>
            </p>
          </div>
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between">
            <span className="text-gray-500 text-sm">Activas</span>
            <CheckCircle className="h-5 w-5 text-green-500" />
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats.active}</p>
          <p className="text-xs text-gray-500">
            de {limits.maxListings === -1 ? '∞' : limits.maxListings} permitidas
          </p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between">
            <span className="text-gray-500 text-sm">Pendientes</span>
            <Clock className="h-5 w-5 text-yellow-500" />
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats.pending}</p>
          <p className="text-xs text-gray-500">en revisión</p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between">
            <span className="text-gray-500 text-sm">Destacadas</span>
            <Star className="h-5 w-5 text-yellow-400" />
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats.featured}</p>
          <p className="text-xs text-gray-500">
            de {limits.maxFeaturedListings === -1 ? '∞' : limits.maxFeaturedListings} permitidas
          </p>
        </div>

        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between">
            <span className="text-gray-500 text-sm">Vistas Totales</span>
            <Eye className="h-5 w-5 text-blue-500" />
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats.totalViews.toLocaleString()}</p>
          <p className="text-xs text-gray-500">este mes</p>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg border border-gray-200 p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          {/* Search */}
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar publicaciones..."
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {/* Status Filter */}
          <div className="flex items-center gap-2">
            <Filter className="h-5 w-5 text-gray-400" />
            <select
              value={statusFilter}
              onChange={e => setStatusFilter(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="all">Todos los estados</option>
              <option value="active">Activos</option>
              <option value="pending">Pendientes</option>
              <option value="draft">Borradores</option>
              <option value="sold">Vendidos</option>
              <option value="rejected">Rechazados</option>
            </select>
          </div>

          {/* Type Filter */}
          <select
            value={typeFilter}
            onChange={e => setTypeFilter(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="all">Todos los tipos</option>
            <option value="vehicle">Vehículos</option>
            <option value="property">Propiedades</option>
          </select>
        </div>
      </div>

      {/* Listings Table */}
      <div className="bg-white rounded-lg border border-gray-200 overflow-hidden">
        {filteredListings.length === 0 ? (
          <div className="text-center py-12">
            <ImageIcon className="h-12 w-12 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900">No se encontraron publicaciones</h3>
            <p className="text-gray-500 mt-1">
              {searchQuery || statusFilter !== 'all' || typeFilter !== 'all'
                ? 'Intenta con otros filtros'
                : 'Comienza creando tu primera publicación'}
            </p>
            {!searchQuery && statusFilter === 'all' && typeFilter === 'all' && (
              <Link
                to="/dealer/listings/new"
                className="inline-flex items-center gap-2 mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                <Plus className="h-5 w-5" />
                Crear Publicación
              </Link>
            )}
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Publicación
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Precio
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Estado
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rendimiento
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Fecha
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredListings.map(listing => (
                  <tr key={listing.id} className="hover:bg-gray-50">
                    {/* Listing Info */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-4">
                        <div className="relative w-16 h-12 rounded-lg overflow-hidden bg-gray-100 flex-shrink-0">
                          <img
                            src={listing.image}
                            alt={listing.title}
                            className="w-full h-full object-cover"
                          />
                          {listing.isFeatured && (
                            <div className="absolute top-0 right-0 bg-yellow-400 p-0.5 rounded-bl">
                              <Star className="h-3 w-3 text-white fill-current" />
                            </div>
                          )}
                        </div>
                        <div>
                          <div className="flex items-center gap-2">
                            {listing.type === 'vehicle' ? (
                              <Car className="h-4 w-4 text-blue-500" />
                            ) : (
                              <Building2 className="h-4 w-4 text-green-500" />
                            )}
                            <span className="font-medium text-gray-900">
                              <LocalizedContent content={listing.title} showBadge={false} />
                            </span>
                          </div>
                          <p className="text-sm text-gray-500">
                            {listing.type === 'vehicle' 
                              ? `${listing.year} • ${listing.mileage?.toLocaleString()} km`
                              : `${listing.bedrooms} rec • ${listing.bathrooms} baños • ${listing.area} m²`
                            }
                          </p>
                        </div>
                      </div>
                    </td>

                    {/* Price */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="font-semibold text-gray-900">{formatPrice(listing.price)}</span>
                    </td>

                    {/* Status */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <StatusBadge status={listing.status} />
                    </td>

                    {/* Performance */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-4 text-sm">
                        <span className="flex items-center gap-1 text-gray-600">
                          <Eye className="h-4 w-4" />
                          {listing.views}
                        </span>
                        <span className="flex items-center gap-1 text-gray-600">
                          <TrendingUp className="h-4 w-4" />
                          {listing.inquiries}
                        </span>
                      </div>
                    </td>

                    {/* Date */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-1 text-sm text-gray-500">
                        <Calendar className="h-4 w-4" />
                        {formatDate(listing.createdAt)}
                      </div>
                    </td>

                    {/* Actions */}
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => toggleFeatured(listing.id)}
                          className={`p-1.5 rounded-lg transition-colors ${
                            listing.isFeatured 
                              ? 'text-yellow-500 bg-yellow-50 hover:bg-yellow-100'
                              : 'text-gray-400 hover:text-yellow-500 hover:bg-yellow-50'
                          }`}
                          title={listing.isFeatured ? 'Quitar destacado' : 'Destacar'}
                        >
                          {listing.isFeatured ? (
                            <Star className="h-5 w-5 fill-current" />
                          ) : (
                            <StarOff className="h-5 w-5" />
                          )}
                        </button>
                        <Link
                          to={`/dealer/listings/${listing.id}`}
                          className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                          title="Ver"
                        >
                          <Eye className="h-5 w-5" />
                        </Link>
                        <Link
                          to={`/dealer/listings/${listing.id}/edit`}
                          className="p-1.5 text-gray-400 hover:text-green-600 hover:bg-green-50 rounded-lg transition-colors"
                          title="Editar"
                        >
                          <Edit2 className="h-5 w-5" />
                        </Link>
                        <button
                          onClick={() => handleDelete(listing.id)}
                          className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Eliminar"
                        >
                          <Trash2 className="h-5 w-5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteModal && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4">
            <div className="fixed inset-0 bg-black/50" onClick={() => setShowDeleteModal(false)} />
            <div className="relative bg-white rounded-xl shadow-xl max-w-md w-full p-6">
              <div className="text-center">
                <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
                  <Trash2 className="h-6 w-6 text-red-600" />
                </div>
                <h3 className="text-lg font-medium text-gray-900 mb-2">
                  Eliminar Publicación
                </h3>
                <p className="text-gray-500">
                  ¿Estás seguro de que deseas eliminar esta publicación? Esta acción no se puede deshacer.
                </p>
              </div>
              <div className="mt-6 flex gap-3">
                <button
                  onClick={() => setShowDeleteModal(false)}
                  className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  onClick={confirmDelete}
                  className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                >
                  Eliminar
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default DealerListingsPage;
