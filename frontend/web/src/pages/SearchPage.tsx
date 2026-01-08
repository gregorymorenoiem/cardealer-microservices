import { useState, useEffect } from 'react';
import { FiSearch, FiSliders, FiHeart, FiX } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import Input from '@/components/atoms/Input';
import VehicleCard from '@/components/organisms/VehicleCard';
import VehicleCardSkeleton from '@/components/organisms/VehicleCardSkeleton';
import Pagination from '@/components/molecules/Pagination';

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
    fetch('https://api.okla.com.do/api/catalog/makes')
      .then((res) => res.json())
      .then((data) => setMakes(data))
      .catch(console.error);
  }, []);

  useEffect(() => {
    // Load models when make changes
    if (filters.make) {
      fetch(`https://api.okla.com.do/api/catalog/models/${filters.make}`)
        .then((res) => res.json())
        .then((data) => setModels(data))
        .catch(console.error);
    } else {
      setModels([]);
    }
  }, [filters.make]);

  const handleSearch = async () => {
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

      const response = await fetch(`https://api.okla.com.do/api/vehicles/search?${params}`);
      const data = await response.json();

      setVehicles(data.items || []);
      setTotalPages(data.totalPages || 1);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    handleSearch();
  }, [currentPage, filters]);

  const toggleFavorite = async (vehicleId: string) => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      window.location.href = '/login?redirect=/search';
      return;
    }

    try {
      const vehicle = vehicles.find((v) => v.id === vehicleId);
      const url = vehicle?.isFavorite
        ? `https://api.okla.com.do/api/favorites/${vehicleId}`
        : 'https://api.okla.com.do/api/favorites';

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

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {/* Search Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-4">Buscar Vehículos</h1>

          <div className="flex gap-2">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
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

            <Sheet>
              <SheetTrigger asChild>
                <Button variant="outline">
                  <SlidersHorizontal className="h-4 w-4 mr-2" />
                  Filtros
                </Button>
              </SheetTrigger>
              <SheetContent>
                <SheetHeader>
                  <SheetTitle>Filtros de Búsqueda</SheetTitle>
                  <SheetDescription>Refina tu búsqueda con estos filtros</SheetDescription>
                </SheetHeader>

                <div className="space-y-6 mt-6">
                  {/* Make */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">Marca</label>
                    <Select
                      value={filters.make}
                      onValueChange={(value) => setFilters({ ...filters, make: value, model: '' })}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Todas las marcas" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="">Todas las marcas</SelectItem>
                        {makes.map((make) => (
                          <SelectItem key={make} value={make}>
                            {make}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  {/* Model */}
                  {filters.make && (
                    <div>
                      <label className="text-sm font-medium mb-2 block">Modelo</label>
                      <Select
                        value={filters.model}
                        onValueChange={(value) => setFilters({ ...filters, model: value })}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Todos los modelos" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="">Todos los modelos</SelectItem>
                          {models.map((model) => (
                            <SelectItem key={model} value={model}>
                              {model}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  )}

                  {/* Year Range */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Año: {filters.yearFrom} - {filters.yearTo}
                    </label>
                    <Slider
                      min={2000}
                      max={new Date().getFullYear()}
                      step={1}
                      value={[filters.yearFrom, filters.yearTo]}
                      onValueChange={([from, to]) =>
                        setFilters({ ...filters, yearFrom: from, yearTo: to })
                      }
                    />
                  </div>

                  {/* Price Range */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Precio: {formatPrice(filters.priceMin)} - {formatPrice(filters.priceMax)}
                    </label>
                    <Slider
                      min={0}
                      max={10000000}
                      step={100000}
                      value={[filters.priceMin, filters.priceMax]}
                      onValueChange={([min, max]) =>
                        setFilters({ ...filters, priceMin: min, priceMax: max })
                      }
                    />
                  </div>

                  {/* Mileage */}
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Kilometraje máximo: {filters.mileageMax.toLocaleString()} km
                    </label>
                    <Slider
                      min={0}
                      max={300000}
                      step={10000}
                      value={[filters.mileageMax]}
                      onValueChange={([max]) => setFilters({ ...filters, mileageMax: max })}
                    />
                  </div>

                  <Button variant="outline" onClick={clearFilters} className="w-full">
                    <X className="h-4 w-4 mr-2" />
                    Limpiar Filtros
                  </Button>
                </div>
              </SheetContent>
            </Sheet>
          </div>

          {/* Active Filters */}
          {(filters.make || filters.model || filters.yearFrom !== 2000 || filters.priceMin > 0) && (
            <div className="flex flex-wrap gap-2 mt-4">
              {filters.make && (
                <Badge variant="secondary">
                  Marca: {filters.make}
                  <X
                    className="h-3 w-3 ml-1 cursor-pointer"
                    onClick={() => setFilters({ ...filters, make: '', model: '' })}
                  />
                </Badge>
              )}
              {filters.model && (
                <Badge variant="secondary">
                  Modelo: {filters.model}
                  <X
                    className="h-3 w-3 ml-1 cursor-pointer"
                    onClick={() => setFilters({ ...filters, model: '' })}
                  />
                </Badge>
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
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {vehicles.map((vehicle) => (
                <div
                  key={vehicle.id}
                  className="border rounded-lg overflow-hidden hover:shadow-lg transition-shadow"
                >
                  <div className="relative">
                    <img
                      src={vehicle.imageUrl}
                      alt={vehicle.title}
                      className="w-full h-48 object-cover"
                    />
                    <button
                      onClick={() => toggleFavorite(vehicle.id)}
                      className="absolute top-2 right-2 p-2 bg-white rounded-full shadow-md hover:bg-gray-100"
                    >
                      <Heart
                        className={`h-5 w-5 ${vehicle.isFavorite ? 'fill-red-500 text-red-500' : 'text-gray-600'}`}
                      />
                    </button>
                  </div>
                  <div className="p-4">
                    <h3 className="font-bold text-lg mb-2">{vehicle.title}</h3>
                    <p className="text-gray-600 text-sm mb-2">
                      {vehicle.year} • {vehicle.mileage.toLocaleString()} km
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
              <div className="flex justify-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  Anterior
                </Button>
                <div className="flex items-center px-4">
                  Página {currentPage} de {totalPages}
                </div>
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                >
                  Siguiente
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </MainLayout>
  );
}
