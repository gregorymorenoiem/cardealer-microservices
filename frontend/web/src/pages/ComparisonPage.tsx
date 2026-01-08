import { useState, useEffect } from 'react';
import { FiX, FiShare2, FiPlus } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';
import Button from '@/components/atoms/Button';
import EmptyState from '@/components/organisms/EmptyState';

interface Vehicle {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  transmission: string;
  fuelType: string;
  engineSize: string;
  horsepower: number;
  imageUrl: string;
}

interface Comparison {
  id: string;
  vehicles: Vehicle[];
  createdAt: string;
}

export function ComparisonPage() {
  const [comparison, setComparison] = useState<Comparison | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadComparison();
  }, []);

  const loadComparison = async () => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      window.location.href = '/login?redirect=/comparison';
      return;
    }

    try {
      const response = await fetch('https://api.okla.com.do/api/comparisons', {
        headers: { Authorization: `Bearer ${token}` },
      });
      const data = await response.json();
      setComparison(data[0] || null);
    } catch (error) {
      console.error('Failed to load comparison:', error);
    } finally {
      setLoading(false);
    }
  };

  const removeVehicle = async (vehicleId: string) => {
    if (!comparison) return;

    const token = localStorage.getItem('authToken');
    try {
      const response = await fetch(
        `https://api.okla.com.do/api/comparisons/${comparison.id}/vehicles/${vehicleId}`,
        {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${token}` },
        }
      );

      if (response.ok) {
        const updatedVehicles = comparison.vehicles.filter((v) => v.id !== vehicleId);
        setComparison({ ...comparison, vehicles: updatedVehicles });
      }
    } catch (error) {
      console.error('Failed to remove vehicle:', error);
    }
  };

  const shareComparison = async () => {
    if (!comparison) return;

    const token = localStorage.getItem('authToken');
    try {
      const response = await fetch(
        `https://api.okla.com.do/api/comparisons/${comparison.id}/share`,
        {
          method: 'POST',
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      const data = await response.json();

      navigator.clipboard.writeText(data.shareUrl);
      alert('Link copiado al portapapeles');
    } catch (error) {
      console.error('Failed to share:', error);
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="text-center py-12">
            <div className="animate-spin h-12 w-12 border-4 border-blue-500 border-t-transparent rounded-full mx-auto"></div>
            <p className="mt-4 text-gray-600">Cargando comparación...</p>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!comparison || comparison.vehicles.length === 0) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <EmptyState
            icon={FiPlus}
            title="No hay vehículos para comparar"
            description="Agrega hasta 3 vehículos para comparar sus especificaciones lado a lado"
            actionLabel="Buscar Vehículos"
            onAction={() => (window.location.href = '/search')}
          />
        </div>
      </MainLayout>
    );
  }

  const specs = [
    { label: 'Precio', key: 'price', format: formatPrice },
    { label: 'Kilometraje', key: 'mileage', format: (v: number) => `${v.toLocaleString()} km` },
    { label: 'Transmisión', key: 'transmission' },
    { label: 'Combustible', key: 'fuelType' },
    { label: 'Motor', key: 'engineSize' },
    { label: 'Caballos de Fuerza', key: 'horsepower', format: (v: number) => `${v} HP` },
  ];

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold">Comparar Vehículos</h1>
            <p className="text-gray-600 mt-2">{comparison.vehicles.length} de 3 vehículos</p>
          </div>
          <Button onClick={shareComparison} leftIcon={<FiShare2 />}>
            Compartir
          </Button>
        </div>

        {/* Comparison Table */}
        <div className="overflow-x-auto">
          <div className="inline-flex min-w-full">
            {/* Spec Labels Column */}
            <div className="w-48 flex-shrink-0">
              <div className="h-64 border-b border-gray-200"></div>
              {specs.map((spec) => (
                <div
                  key={spec.key}
                  className="h-16 border-b border-gray-200 flex items-center px-4 font-semibold"
                >
                  {spec.label}
                </div>
              ))}
            </div>

            {/* Vehicle Columns */}
            {comparison.vehicles.map((vehicle) => (
              <div key={vehicle.id} className="w-80 flex-shrink-0 border-l border-gray-200">
                {/* Vehicle Header */}
                <div className="h-64 p-4 relative">
                  <button
                    onClick={() => removeVehicle(vehicle.id)}
                    className="absolute top-2 right-2 p-2 bg-white rounded-full shadow hover:bg-red-50"
                  >
                    <FiX className="w-4 h-4 text-red-600" />
                  </button>
                  <img
                    src={vehicle.imageUrl}
                    alt={`${vehicle.make} ${vehicle.model}`}
                    className="w-full h-32 object-cover rounded-lg mb-2"
                  />
                  <h3 className="font-bold text-lg">
                    {vehicle.make} {vehicle.model}
                  </h3>
                  <p className="text-gray-600">{vehicle.year}</p>
                </div>

                {/* Specs */}
                {specs.map((spec) => (
                  <div
                    key={spec.key}
                    className="h-16 border-t border-gray-200 flex items-center px-4"
                  >
                    {spec.format
                      ? spec.format(vehicle[spec.key as keyof Vehicle] as any)
                      : vehicle[spec.key as keyof Vehicle]}
                  </div>
                ))}
              </div>
            ))}

            {/* Add Vehicle Placeholder */}
            {comparison.vehicles.length < 3 && (
              <div className="w-80 flex-shrink-0 border-l border-gray-200">
                <div className="h-64 flex items-center justify-center bg-gray-50">
                  <Button onClick={() => (window.location.href = '/search')} leftIcon={<FiPlus />}>
                    Agregar Vehículo
                  </Button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
