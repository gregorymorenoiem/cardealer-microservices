import { useState, useEffect, useCallback } from 'react';
import { FiSearch, FiX, FiSliders, FiHeart, FiChevronLeft, FiChevronRight } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';

interface Vehicle {
  id: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  imageUrl: string;
  isFavorite?: boolean;
}

interface FilterState {
  make: string;
  model: string;
  yearFrom: number;
  yearTo: number;
  priceMin: number;
  priceMax: number;
  mileageMax: number;
}

export function SearchPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [showFilters, setShowFilters] = useState(false);
  const [filters, setFilters] = useState<FilterState>({
    make: '',
    model: '',
    yearFrom: 2000,
    yearTo: new Date().getFullYear(),
    priceMin: 0,
    priceMax: 10000000,
    mileageMax: 300000,
  });

  const [makes, setMakes] = useState<string[]>([]);
  const [models, setModels] = useState<string[]>([]);

  useEffect(() => {
    // Load makes from catalog
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
    fetch(`${API_URL}/api/catalog/makes`)
      .then((res) => res.json())
      .then((data) => setMakes(data))
      .catch(console.error);
  }, []);

  useEffect(() => {
    // Load models when make changes
    if (filters.make) {
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
      fetch(`${API_URL}/api/catalog/models/${filters.make}`)
        .then((res) => res.json())
        .then((data) => setModels(data))
        .catch(console.error);
    } else {
      setModels([]);
    }
  }, [filters.make]);

  const handleSearch = useCallback(async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        query: searchQuery,
        page: currentPage.toString(),
        pageSize: '12',
        ...(filters.make && { make: filters.make }),
        ...(filters.model && { model: filters.model }),
        yearFrom: filters.yearFrom.toString(),
        yearTo: filters.yearTo.toString(),
        priceMin: filters.priceMin.toString(),
        priceMax: filters.priceMax.toString(),
        mileageMax: filters.mileageMax.toString(),
      });

      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
      const response = await fetch(
        `${API_URL}/api/vehicles?search=${searchQuery}&page=${currentPage}&pageSize=12`
      );
      const data = await response.json();

      setVehicles(data.items || []);
      setTotalPages(data.totalPages || 1);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  }, [searchQuery, currentPage, filters]);

  useEffect(() => {
    handleSearch();
  }, [handleSearch]);

  const toggleFavorite = async (vehicleId: string) => {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      // ProtectedRoute ya se encarga de la redireción si es necesaria
      return;
    }

    try {
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';
      const vehicle = vehicles.find((v) => v.id === vehicleId);
      const url = vehicle?.isFavorite
        ? `${API_URL}/api/favorites/${vehicleId}`
        : `${API_URL}/api/favorites`;

      const response = await fetch(url, {
        method: vehicle?.isFavorite ? 'DELETE' : 'POST',
        headers: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        ...(vehicle?.isFavorite ? {} : { body: JSON.stringify({ vehicleId }) }),
      });

      if (response.ok) {
        setVehicles(
          vehicles.map((v) => (v.id === vehicleId ? { ...v, isFavorite: !v.isFavorite } : v))
        );
      }
    } catch (error) {
      console.error('Failed to toggle favorite:', error);
    }
  };

  const clearFilters = () => {
    setFilters({
      make: '',
      model: '',
      yearFrom: 2000,
      yearTo: new Date().getFullYear(),
      priceMin: 0,
      priceMax: 10000000,
      mileageMax: 300000,
    });
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const hasActiveFilters =
    filters.make || filters.model || filters.yearFrom !== 2000 || filters.priceMin > 0;

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {/* Search Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-4">Buscar Vehículos</h1>

          <div className="flex gap-2">
            <div className="flex-1 relative">
              <FiSearch className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
              <Input
                placeholder="Buscar por marca, modelo, año..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="pl-10"
              />
            </div>
            <Button onClick={handleSearch} disabled={loading}>
              {loading ? 'Buscando...' : 'Buscar'}
            </Button>

            <Button variant="outline" onClick={() => setShowFilters(!showFilters)}>
              <FiSliders className="h-4 w-4 mr-2" />
              Filtros
            </Button>
          </div>

          {/* Filters Panel */}
          {showFilters && (
            <div className="mt-4 p-6 bg-white border rounded-lg shadow-md">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold">Filtros de Búsqueda</h3>
                <button
                  onClick={() => setShowFilters(false)}
                  className="text-gray-500 hover:text-gray-700"
                >
                  <FiX className="h-5 w-5" />
                </button>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {/* Make */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Marca</label>
                  <select
                    value={filters.make}
                    onChange={(e) => setFilters({ ...filters, make: e.target.value, model: '' })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Todas las marcas</option>
                    {makes.map((make) => (
                      <option key={make} value={make}>
                        {make}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Model */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Modelo</label>
                  <select
                    value={filters.model}
                    onChange={(e) => setFilters({ ...filters, model: e.target.value })}
                    disabled={!filters.make}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                  >
                    <option value="">Todos los modelos</option>
                    {models.map((model) => (
                      <option key={model} value={model}>
                        {model}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Year From */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Año Desde</label>
                  <select
                    value={filters.yearFrom}
                    onChange={(e) => setFilters({ ...filters, yearFrom: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    {Array.from(
                      { length: new Date().getFullYear() - 1999 },
                      (_, i) => 2000 + i
                    ).map((year) => (
                      <option key={year} value={year}>
                        {year}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Year To */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Año Hasta</label>
                  <select
                    value={filters.yearTo}
                    onChange={(e) => setFilters({ ...filters, yearTo: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    {Array.from(
                      { length: new Date().getFullYear() - 1999 },
                      (_, i) => 2000 + i
                    ).map((year) => (
                      <option key={year} value={year}>
                        {year}
                      </option>
                    ))}
                  </select>
                </div>

                {/* Price Min */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Precio Mínimo</label>
                  <Input
                    type="number"
                    value={filters.priceMin}
                    onChange={(e) =>
                      setFilters({ ...filters, priceMin: parseInt(e.target.value) || 0 })
                    }
                    placeholder="0"
                  />
                </div>

                {/* Price Max */}
                <div>
                  <label className="text-sm font-medium mb-2 block">Precio Máximo</label>
                  <Input
                    type="number"
                    value={filters.priceMax}
                    onChange={(e) =>
                      setFilters({ ...filters, priceMax: parseInt(e.target.value) || 10000000 })
                    }
                    placeholder="10,000,000"
                  />
                </div>
              </div>

              <div className="mt-4 flex gap-2">
                <Button onClick={handleSearch}>Aplicar Filtros</Button>
                <Button variant="outline" onClick={clearFilters}>
                  <FiX className="h-4 w-4 mr-2" />
                  Limpiar Filtros
                </Button>
              </div>
            </div>
          )}

          {/* Active Filters Tags */}
          {hasActiveFilters && (
            <div className="flex flex-wrap gap-2 mt-4">
              {filters.make && (
                <span className="inline-flex items-center px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                  Marca: {filters.make}
                  <button
                    className="ml-2 hover:text-blue-600"
                    onClick={() => setFilters({ ...filters, make: '', model: '' })}
                  >
                    <FiX className="h-3 w-3" />
                  </button>
                </span>
              )}
              {filters.model && (
                <span className="inline-flex items-center px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                  Modelo: {filters.model}
                  <button
                    className="ml-2 hover:text-blue-600"
                    onClick={() => setFilters({ ...filters, model: '' })}
                  >
                    <FiX className="h-3 w-3" />
                  </button>
                </span>
              )}
              {filters.yearFrom !== 2000 && (
                <span className="inline-flex items-center px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                  Desde: {filters.yearFrom}
                </span>
              )}
              {filters.priceMin > 0 && (
                <span className="inline-flex items-center px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
                  Precio mín: {formatPrice(filters.priceMin)}
                </span>
              )}
            </div>
          )}
        </div>

        {/* Results Grid */}
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin h-12 w-12 border-4 border-blue-500 border-t-transparent rounded-full mx-auto"></div>
            <p className="mt-4 text-gray-600">Buscando vehículos...</p>
          </div>
        ) : vehicles.length === 0 ? (
          <div className="text-center py-12">
            <FiSearch className="mx-auto h-16 w-16 text-gray-300 mb-4" />
            <h3 className="text-xl font-semibold text-gray-600 mb-2">
              No se encontraron vehículos
            </h3>
            <p className="text-gray-500">Intenta ajustar los filtros o buscar otro término</p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {vehicles.map((vehicle) => (
                <div
                  key={vehicle.id}
                  className="bg-white border rounded-lg overflow-hidden hover:shadow-lg transition-shadow"
                >
                  <div className="relative">
                    <img
                      src={vehicle.imageUrl || '/placeholder-vehicle.jpg'}
                      alt={vehicle.title}
                      className="w-full h-48 object-cover"
                    />
                    <button
                      onClick={() => toggleFavorite(vehicle.id)}
                      className="absolute top-2 right-2 p-2 bg-white rounded-full shadow-md hover:bg-gray-100"
                    >
                      <FiHeart
                        className={`h-5 w-5 ${vehicle.isFavorite ? 'fill-red-500 text-red-500' : 'text-gray-600'}`}
                      />
                    </button>
                  </div>
                  <div className="p-4">
                    <h3 className="font-bold text-lg mb-2">{vehicle.title}</h3>
                    <p className="text-gray-600 text-sm mb-2">
                      {vehicle.year} • {vehicle.mileage?.toLocaleString() || 0} km
                    </p>
                    <p className="text-2xl font-bold text-blue-600">{formatPrice(vehicle.price)}</p>
                    <Button
                      className="w-full mt-4"
                      onClick={() => (window.location.href = `/vehicles/${vehicle.id}`)}
                    >
                      Ver Detalles
                    </Button>
                  </div>
                </div>
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex justify-center items-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  <FiChevronLeft className="h-4 w-4 mr-1" />
                  Anterior
                </Button>
                <div className="flex items-center px-4 py-2 bg-gray-100 rounded-lg">
                  Página {currentPage} de {totalPages}
                </div>
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                >
                  Siguiente
                  <FiChevronRight className="h-4 w-4 ml-1" />
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </MainLayout>
  );
}

export default SearchPage;
