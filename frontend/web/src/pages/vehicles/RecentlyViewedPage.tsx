/**
 * RecentlyViewedPage - Vehículos vistos recientemente
 *
 * Muestra el historial de vehículos que el usuario ha visto recientemente,
 * permitiendo retomar su búsqueda fácilmente.
 *
 * @module pages/vehicles/RecentlyViewedPage
 * @version 1.0.0
 * @since Enero 25, 2026
 */

import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  FiClock,
  FiHeart,
  FiTrash2,
  FiX,
  FiEye,
  FiCalendar,
  FiGrid,
  FiList,
  FiAlertCircle,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

interface ViewedVehicle {
  id: string;
  slug: string;
  title: string;
  price: number;
  year: number;
  mileage: number;
  location: string;
  imageUrl: string;
  viewedAt: string;
  priceChange?: {
    type: 'up' | 'down';
    amount: number;
    percentage: number;
  };
  isFavorite: boolean;
  isSold: boolean;
}

const RecentlyViewedPage = () => {
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [vehicles, setVehicles] = useState<ViewedVehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Mock data - in production this would come from UserBehaviorService
  useEffect(() => {
    const mockVehicles: ViewedVehicle[] = [
      {
        id: '1',
        slug: 'toyota-camry-2023',
        title: 'Toyota Camry 2023 XSE',
        price: 1850000,
        year: 2023,
        mileage: 15000,
        location: 'Santo Domingo',
        imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 30).toISOString(), // 30 min ago
        isFavorite: true,
        isSold: false,
      },
      {
        id: '2',
        slug: 'honda-accord-2022',
        title: 'Honda Accord 2022 Sport',
        price: 1650000,
        year: 2022,
        mileage: 22000,
        location: 'Santiago',
        imageUrl: 'https://images.unsplash.com/photo-1606611013016-969c19ba27bb?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(), // 2 hours ago
        priceChange: { type: 'down', amount: 50000, percentage: 3 },
        isFavorite: false,
        isSold: false,
      },
      {
        id: '3',
        slug: 'bmw-x5-2021',
        title: 'BMW X5 2021 xDrive40i',
        price: 3200000,
        year: 2021,
        mileage: 35000,
        location: 'Punta Cana',
        imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(), // 1 day ago
        isFavorite: true,
        isSold: false,
      },
      {
        id: '4',
        slug: 'mercedes-c300-2020',
        title: 'Mercedes-Benz C300 2020',
        price: 2450000,
        year: 2020,
        mileage: 42000,
        location: 'Santo Domingo',
        imageUrl: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2).toISOString(), // 2 days ago
        isFavorite: false,
        isSold: true,
      },
      {
        id: '5',
        slug: 'audi-a4-2022',
        title: 'Audi A4 2022 Premium Plus',
        price: 2100000,
        year: 2022,
        mileage: 18000,
        location: 'La Romana',
        imageUrl: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3).toISOString(), // 3 days ago
        priceChange: { type: 'up', amount: 30000, percentage: 1.5 },
        isFavorite: false,
        isSold: false,
      },
      {
        id: '6',
        slug: 'lexus-rx350-2023',
        title: 'Lexus RX 350 2023 F Sport',
        price: 3800000,
        year: 2023,
        mileage: 8000,
        location: 'Santo Domingo',
        imageUrl: 'https://images.unsplash.com/photo-1619682817481-e994891cd1f5?w=400',
        viewedAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 5).toISOString(), // 5 days ago
        isFavorite: true,
        isSold: false,
      },
    ];

    setTimeout(() => {
      setVehicles(mockVehicles);
      setIsLoading(false);
    }, 500);
  }, []);

  const formatTimeAgo = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffMins < 60) return `Hace ${diffMins} minutos`;
    if (diffHours < 24) return `Hace ${diffHours} horas`;
    if (diffDays === 1) return 'Ayer';
    return `Hace ${diffDays} días`;
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      maximumFractionDigits: 0,
    }).format(price);
  };

  const formatMileage = (km: number) => {
    return new Intl.NumberFormat('es-DO').format(km) + ' km';
  };

  const handleToggleFavorite = (id: string) => {
    setVehicles((prev) => prev.map((v) => (v.id === id ? { ...v, isFavorite: !v.isFavorite } : v)));
  };

  const handleRemove = (id: string) => {
    setVehicles((prev) => prev.filter((v) => v.id !== id));
  };

  const handleClearAll = () => {
    if (confirm('¿Estás seguro de que deseas borrar todo tu historial de vehículos vistos?')) {
      setVehicles([]);
    }
  };

  const groupVehiclesByDate = () => {
    const today: ViewedVehicle[] = [];
    const yesterday: ViewedVehicle[] = [];
    const thisWeek: ViewedVehicle[] = [];
    const older: ViewedVehicle[] = [];

    const now = new Date();
    const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const yesterdayStart = new Date(todayStart.getTime() - 24 * 60 * 60 * 1000);
    const weekStart = new Date(todayStart.getTime() - 7 * 24 * 60 * 60 * 1000);

    vehicles.forEach((v) => {
      const viewedDate = new Date(v.viewedAt);
      if (viewedDate >= todayStart) {
        today.push(v);
      } else if (viewedDate >= yesterdayStart) {
        yesterday.push(v);
      } else if (viewedDate >= weekStart) {
        thisWeek.push(v);
      } else {
        older.push(v);
      }
    });

    return { today, yesterday, thisWeek, older };
  };

  const { today, yesterday, thisWeek, older } = groupVehiclesByDate();

  const VehicleCard = ({ vehicle }: { vehicle: ViewedVehicle }) => (
    <div
      className={`bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden group hover:shadow-md transition-shadow ${
        vehicle.isSold ? 'opacity-60' : ''
      }`}
    >
      <div className="relative">
        <Link to={`/vehicles/${vehicle.slug}`}>
          <img src={vehicle.imageUrl} alt={vehicle.title} className="w-full h-48 object-cover" />
        </Link>
        {vehicle.isSold && (
          <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center">
            <span className="bg-red-600 text-white px-4 py-2 rounded-md font-semibold">
              VENDIDO
            </span>
          </div>
        )}
        {vehicle.priceChange && !vehicle.isSold && (
          <div
            className={`absolute top-2 left-2 px-2 py-1 rounded text-xs font-medium ${
              vehicle.priceChange.type === 'down'
                ? 'bg-green-500 text-white'
                : 'bg-red-500 text-white'
            }`}
          >
            {vehicle.priceChange.type === 'down' ? '↓' : '↑'}{' '}
            {formatPrice(vehicle.priceChange.amount)}
          </div>
        )}
        <div className="absolute top-2 right-2 flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            onClick={() => handleToggleFavorite(vehicle.id)}
            className={`p-2 rounded-full ${
              vehicle.isFavorite
                ? 'bg-red-500 text-white'
                : 'bg-white text-gray-600 hover:bg-gray-100'
            }`}
          >
            <FiHeart className={vehicle.isFavorite ? 'fill-current' : ''} />
          </button>
          <button
            onClick={() => handleRemove(vehicle.id)}
            className="p-2 rounded-full bg-white text-gray-600 hover:bg-gray-100"
          >
            <FiX />
          </button>
        </div>
      </div>
      <div className="p-4">
        <Link to={`/vehicles/${vehicle.slug}`} className="hover:text-blue-600">
          <h3 className="font-semibold text-gray-900 mb-1 line-clamp-1">{vehicle.title}</h3>
        </Link>
        <p className="text-lg font-bold text-blue-600 mb-2">{formatPrice(vehicle.price)}</p>
        <div className="flex items-center text-sm text-gray-500 mb-2">
          <span>{vehicle.year}</span>
          <span className="mx-2">•</span>
          <span>{formatMileage(vehicle.mileage)}</span>
          <span className="mx-2">•</span>
          <span>{vehicle.location}</span>
        </div>
        <div className="flex items-center text-xs text-gray-400">
          <FiClock className="mr-1" />
          {formatTimeAgo(vehicle.viewedAt)}
        </div>
      </div>
    </div>
  );

  const VehicleListItem = ({ vehicle }: { vehicle: ViewedVehicle }) => (
    <div
      className={`bg-white rounded-lg shadow-sm border border-gray-200 p-4 flex gap-4 group hover:shadow-md transition-shadow ${
        vehicle.isSold ? 'opacity-60' : ''
      }`}
    >
      <div className="relative w-32 h-24 flex-shrink-0">
        <Link to={`/vehicles/${vehicle.slug}`}>
          <img
            src={vehicle.imageUrl}
            alt={vehicle.title}
            className="w-full h-full object-cover rounded-md"
          />
        </Link>
        {vehicle.isSold && (
          <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center rounded-md">
            <span className="bg-red-600 text-white px-2 py-1 rounded text-xs font-semibold">
              VENDIDO
            </span>
          </div>
        )}
      </div>
      <div className="flex-1 min-w-0">
        <div className="flex items-start justify-between">
          <div>
            <Link to={`/vehicles/${vehicle.slug}`} className="hover:text-blue-600">
              <h3 className="font-semibold text-gray-900 line-clamp-1">{vehicle.title}</h3>
            </Link>
            <div className="flex items-center gap-2 mt-1">
              <p className="text-lg font-bold text-blue-600">{formatPrice(vehicle.price)}</p>
              {vehicle.priceChange && (
                <span
                  className={`text-xs px-1.5 py-0.5 rounded ${
                    vehicle.priceChange.type === 'down'
                      ? 'bg-green-100 text-green-700'
                      : 'bg-red-100 text-red-700'
                  }`}
                >
                  {vehicle.priceChange.type === 'down' ? '↓' : '↑'} {vehicle.priceChange.percentage}
                  %
                </span>
              )}
            </div>
          </div>
          <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
            <button
              onClick={() => handleToggleFavorite(vehicle.id)}
              className={`p-2 rounded-full ${
                vehicle.isFavorite
                  ? 'bg-red-500 text-white'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
              }`}
            >
              <FiHeart className={vehicle.isFavorite ? 'fill-current' : ''} size={16} />
            </button>
            <button
              onClick={() => handleRemove(vehicle.id)}
              className="p-2 rounded-full bg-gray-100 text-gray-600 hover:bg-gray-200"
            >
              <FiX size={16} />
            </button>
          </div>
        </div>
        <div className="flex items-center text-sm text-gray-500 mt-1">
          <span>{vehicle.year}</span>
          <span className="mx-2">•</span>
          <span>{formatMileage(vehicle.mileage)}</span>
          <span className="mx-2">•</span>
          <span>{vehicle.location}</span>
        </div>
        <div className="flex items-center text-xs text-gray-400 mt-2">
          <FiClock className="mr-1" />
          {formatTimeAgo(vehicle.viewedAt)}
        </div>
      </div>
    </div>
  );

  const VehicleSection = ({
    title,
    vehicles: sectionVehicles,
  }: {
    title: string;
    vehicles: ViewedVehicle[];
  }) => {
    if (sectionVehicles.length === 0) return null;

    return (
      <div className="mb-8">
        <h2 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
          <FiCalendar className="mr-2 text-gray-500" />
          {title}
          <span className="ml-2 text-sm font-normal text-gray-500">({sectionVehicles.length})</span>
        </h2>
        {viewMode === 'grid' ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {sectionVehicles.map((vehicle) => (
              <VehicleCard key={vehicle.id} vehicle={vehicle} />
            ))}
          </div>
        ) : (
          <div className="space-y-3">
            {sectionVehicles.map((vehicle) => (
              <VehicleListItem key={vehicle.id} vehicle={vehicle} />
            ))}
          </div>
        )}
      </div>
    );
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8">
            <div className="flex items-center mb-4 sm:mb-0">
              <div className="p-3 bg-blue-100 rounded-lg mr-4">
                <FiClock className="w-6 h-6 text-blue-600" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">Vistos recientemente</h1>
                <p className="text-gray-600">
                  {vehicles.length > 0
                    ? `${vehicles.length} vehículos en tu historial`
                    : 'Tu historial está vacío'}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-4">
              {/* View Toggle */}
              <div className="flex bg-gray-100 rounded-lg p-1">
                <button
                  onClick={() => setViewMode('grid')}
                  className={`p-2 rounded-md ${
                    viewMode === 'grid' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-500'
                  }`}
                >
                  <FiGrid size={18} />
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`p-2 rounded-md ${
                    viewMode === 'list' ? 'bg-white shadow-sm text-blue-600' : 'text-gray-500'
                  }`}
                >
                  <FiList size={18} />
                </button>
              </div>
              {vehicles.length > 0 && (
                <button
                  onClick={handleClearAll}
                  className="inline-flex items-center px-3 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <FiTrash2 className="mr-2" />
                  Borrar historial
                </button>
              )}
            </div>
          </div>

          {isLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4" />
              <p className="text-gray-500">Cargando historial...</p>
            </div>
          ) : vehicles.length === 0 ? (
            <div className="text-center py-12 bg-white rounded-lg shadow-sm border border-gray-200">
              <FiEye className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h2 className="text-xl font-semibold text-gray-900 mb-2">
                No has visto ningún vehículo aún
              </h2>
              <p className="text-gray-500 mb-6">
                Explora nuestro catálogo y los vehículos que visites aparecerán aquí
              </p>
              <Link
                to="/vehicles"
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
              >
                Explorar vehículos
              </Link>
            </div>
          ) : (
            <>
              {/* Price Changes Alert */}
              {vehicles.some((v) => v.priceChange && !v.isSold) && (
                <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
                  <div className="flex">
                    <FiAlertCircle className="w-5 h-5 text-yellow-600 mr-3 flex-shrink-0" />
                    <div className="text-sm text-yellow-800">
                      <p className="font-medium">Cambios de precio detectados</p>
                      <p>Algunos vehículos que has visto han cambiado de precio.</p>
                    </div>
                  </div>
                </div>
              )}

              <VehicleSection title="Hoy" vehicles={today} />
              <VehicleSection title="Ayer" vehicles={yesterday} />
              <VehicleSection title="Esta semana" vehicles={thisWeek} />
              <VehicleSection title="Anteriores" vehicles={older} />
            </>
          )}

          {/* Privacy Notice */}
          <div className="mt-8 text-center text-sm text-gray-500">
            <p>
              Tu historial de navegación se guarda localmente y en tu cuenta para mejorar tu
              experiencia.
              <Link to="/privacy-center" className="text-blue-600 hover:text-blue-800 ml-1">
                Gestionar privacidad
              </Link>
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default RecentlyViewedPage;
