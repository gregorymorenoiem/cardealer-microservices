import { useState, useEffect } from 'react';
import {
  FiMapPin,
  FiPlus,
  FiEdit2,
  FiTrash2,
  FiHome,
  FiCheck,
  FiPhone,
  FiClock,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

interface DealerLocation {
  id: string;
  name: string;
  type: 'Headquarters' | 'Branch' | 'Showroom' | 'ServiceCenter' | 'Warehouse';
  address: string;
  city: string;
  province: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  isPrimary: boolean;
  isActive: boolean;
  openingHours?: string;
  vehicleCount: number;
  latitude?: number;
  longitude?: number;
  createdAt: string;
}

interface LocationsPageProps {
  dealerId: string;
}

export default function LocationsPage({ dealerId }: LocationsPageProps) {
  const [locations, setLocations] = useState<DealerLocation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editingLocation, setEditingLocation] = useState<DealerLocation | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    type: 'Showroom' as DealerLocation['type'],
    address: '',
    city: '',
    province: '',
    postalCode: '',
    phone: '',
    email: '',
    openingHours: '',
    isPrimary: false,
    isActive: true,
  });

  useEffect(() => {
    loadLocations();
  }, [dealerId]);

  const loadLocations = async () => {
    try {
      setLoading(true);
      // TODO: Call API to get locations
      // const data = await dealerService.getLocations(dealerId);
      // setLocations(data);

      // Mock data for now
      setLocations([
        {
          id: '1',
          name: 'Sede Principal',
          type: 'Headquarters',
          address: 'Av. Winston Churchill #45',
          city: 'Santo Domingo',
          province: 'Distrito Nacional',
          postalCode: '10147',
          phone: '809-555-0100',
          email: 'principal@dealer.com',
          isPrimary: true,
          isActive: true,
          openingHours: 'Lun-Vie: 8:00 AM - 6:00 PM, Sáb: 9:00 AM - 3:00 PM',
          vehicleCount: 45,
          latitude: 18.4664,
          longitude: -69.9325,
          createdAt: '2024-01-15',
        },
        {
          id: '2',
          name: 'Sucursal Santiago',
          type: 'Branch',
          address: 'Av. 27 de Febrero #123',
          city: 'Santiago',
          province: 'Santiago',
          phone: '809-555-0200',
          isPrimary: false,
          isActive: true,
          openingHours: 'Lun-Vie: 8:00 AM - 5:00 PM',
          vehicleCount: 28,
          createdAt: '2024-03-20',
        },
        {
          id: '3',
          name: 'Showroom Mall',
          type: 'Showroom',
          address: 'Blue Mall, Local 105',
          city: 'Santo Domingo',
          province: 'Distrito Nacional',
          phone: '809-555-0300',
          isPrimary: false,
          isActive: true,
          vehicleCount: 12,
          createdAt: '2024-06-10',
        },
      ]);
    } catch (err: any) {
      setError(err.message || 'Error al cargar las ubicaciones');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingLocation) {
        // TODO: Call API to update location
        // await dealerService.updateLocation(editingLocation.id, formData);
        console.log('Updating location:', editingLocation.id, formData);
      } else {
        // TODO: Call API to create location
        // await dealerService.createLocation(dealerId, formData);
        console.log('Creating location:', formData);
      }

      setShowModal(false);
      resetForm();
      loadLocations();
    } catch (err: any) {
      setError(err.message || 'Error al guardar la ubicación');
    }
  };

  const handleEdit = (location: DealerLocation) => {
    setEditingLocation(location);
    setFormData({
      name: location.name,
      type: location.type,
      address: location.address,
      city: location.city,
      province: location.province,
      postalCode: location.postalCode || '',
      phone: location.phone || '',
      email: location.email || '',
      openingHours: location.openingHours || '',
      isPrimary: location.isPrimary,
      isActive: location.isActive,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Está seguro de eliminar esta ubicación?')) return;

    try {
      // TODO: Call API to delete location
      // await dealerService.deleteLocation(id);
      console.log('Deleting location:', id);
      loadLocations();
    } catch (err: any) {
      setError(err.message || 'Error al eliminar la ubicación');
    }
  };

  const handleSetPrimary = async (id: string) => {
    try {
      // TODO: Call API to set primary location
      // await dealerService.setPrimaryLocation(id);
      console.log('Setting primary location:', id);
      loadLocations();
    } catch (err: any) {
      setError(err.message || 'Error al establecer ubicación principal');
    }
  };

  const resetForm = () => {
    setEditingLocation(null);
    setFormData({
      name: '',
      type: 'Showroom',
      address: '',
      city: '',
      province: '',
      postalCode: '',
      phone: '',
      email: '',
      openingHours: '',
      isPrimary: false,
      isActive: true,
    });
  };

  const openNewModal = () => {
    resetForm();
    setShowModal(true);
  };

  const getTypeLabel = (type: DealerLocation['type']) => {
    const labels = {
      Headquarters: 'Sede Principal',
      Branch: 'Sucursal',
      Showroom: 'Showroom',
      ServiceCenter: 'Centro de Servicio',
      Warehouse: 'Almacén',
    };
    return labels[type] || type;
  };

  const getTypeColor = (type: DealerLocation['type']) => {
    const colors = {
      Headquarters: 'bg-purple-100 text-purple-800',
      Branch: 'bg-blue-100 text-blue-800',
      Showroom: 'bg-green-100 text-green-800',
      ServiceCenter: 'bg-yellow-100 text-yellow-800',
      Warehouse: 'bg-gray-100 text-gray-800',
    };
    return colors[type] || 'bg-gray-100 text-gray-800';
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="container mx-auto px-4 py-8">
          <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="flex justify-between items-center mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">Ubicaciones</h1>
            <p className="text-gray-600 mt-1">
              Gestiona las sucursales y puntos de venta de tu dealer
            </p>
          </div>
          <button
            onClick={openNewModal}
            className="flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition"
          >
            <FiPlus size={20} />
            Nueva Ubicación
          </button>
        </div>

        {/* Stats Summary */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
          <div className="bg-white rounded-lg shadow-sm p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center">
                <FiMapPin className="text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{locations.length}</p>
                <p className="text-sm text-gray-500">Ubicaciones</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-lg shadow-sm p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center">
                <FiCheck className="text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">
                  {locations.filter((l) => l.isActive).length}
                </p>
                <p className="text-sm text-gray-500">Activas</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-lg shadow-sm p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-purple-100 flex items-center justify-center">
                <FiHome className="text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">
                  {locations.reduce((acc, l) => acc + l.vehicleCount, 0)}
                </p>
                <p className="text-sm text-gray-500">Vehículos Total</p>
              </div>
            </div>
          </div>
          <div className="bg-white rounded-lg shadow-sm p-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-yellow-100 flex items-center justify-center">
                <FiMapPin className="text-yellow-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">
                  {new Set(locations.map((l) => l.province)).size}
                </p>
                <p className="text-sm text-gray-500">Provincias</p>
              </div>
            </div>
          </div>
        </div>

        {/* Error */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-800 rounded-lg p-4 mb-6">
            {error}
          </div>
        )}

        {/* Locations Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {locations.map((location) => (
            <div
              key={location.id}
              className={`bg-white rounded-lg shadow-sm overflow-hidden ${
                !location.isActive ? 'opacity-60' : ''
              }`}
            >
              {/* Header */}
              <div className="p-4 border-b border-gray-100">
                <div className="flex items-start justify-between">
                  <div>
                    <div className="flex items-center gap-2">
                      <h3 className="font-semibold text-gray-900">{location.name}</h3>
                      {location.isPrimary && (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                          ⭐ Principal
                        </span>
                      )}
                    </div>
                    <span
                      className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium mt-1 ${getTypeColor(location.type)}`}
                    >
                      {getTypeLabel(location.type)}
                    </span>
                  </div>
                  <div className="flex gap-1">
                    <button
                      onClick={() => handleEdit(location)}
                      className="p-2 text-gray-400 hover:text-blue-600 transition"
                      title="Editar"
                    >
                      <FiEdit2 size={16} />
                    </button>
                    {!location.isPrimary && (
                      <button
                        onClick={() => handleDelete(location.id)}
                        className="p-2 text-gray-400 hover:text-red-600 transition"
                        title="Eliminar"
                      >
                        <FiTrash2 size={16} />
                      </button>
                    )}
                  </div>
                </div>
              </div>

              {/* Content */}
              <div className="p-4 space-y-3">
                <div className="flex items-start gap-2 text-sm">
                  <FiMapPin className="text-gray-400 mt-0.5 flex-shrink-0" />
                  <div>
                    <p className="text-gray-900">{location.address}</p>
                    <p className="text-gray-500">
                      {location.city}, {location.province}
                    </p>
                  </div>
                </div>

                {location.phone && (
                  <div className="flex items-center gap-2 text-sm">
                    <FiPhone className="text-gray-400" />
                    <a href={`tel:${location.phone}`} className="text-blue-600 hover:underline">
                      {location.phone}
                    </a>
                  </div>
                )}

                {location.openingHours && (
                  <div className="flex items-start gap-2 text-sm">
                    <FiClock className="text-gray-400 mt-0.5 flex-shrink-0" />
                    <p className="text-gray-600">{location.openingHours}</p>
                  </div>
                )}

                <div className="pt-2 border-t border-gray-100">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-500">Vehículos</span>
                    <span className="font-semibold text-gray-900">{location.vehicleCount}</span>
                  </div>
                </div>
              </div>

              {/* Footer Actions */}
              {!location.isPrimary && (
                <div className="px-4 py-3 bg-gray-50 border-t border-gray-100">
                  <button
                    onClick={() => handleSetPrimary(location.id)}
                    className="w-full text-center text-sm text-blue-600 hover:text-blue-800"
                  >
                    Establecer como Principal
                  </button>
                </div>
              )}
            </div>
          ))}

          {/* Add New Card */}
          <div
            onClick={openNewModal}
            className="bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg p-8 flex flex-col items-center justify-center cursor-pointer hover:border-blue-500 hover:bg-blue-50 transition"
          >
            <FiPlus size={32} className="text-gray-400" />
            <p className="mt-2 font-medium text-gray-600">Nueva Ubicación</p>
          </div>
        </div>

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
              <div className="p-6 border-b border-gray-200">
                <h2 className="text-xl font-semibold text-gray-900">
                  {editingLocation ? 'Editar Ubicación' : 'Nueva Ubicación'}
                </h2>
              </div>

              <form onSubmit={handleSubmit} className="p-6 space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Nombre *</label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="Ej: Sucursal Santiago"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Tipo *</label>
                    <select
                      required
                      value={formData.type}
                      onChange={(e) =>
                        setFormData({ ...formData, type: e.target.value as DealerLocation['type'] })
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="Headquarters">Sede Principal</option>
                      <option value="Branch">Sucursal</option>
                      <option value="Showroom">Showroom</option>
                      <option value="ServiceCenter">Centro de Servicio</option>
                      <option value="Warehouse">Almacén</option>
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Dirección *
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.address}
                    onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    placeholder="Av. Principal #123"
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Ciudad *</label>
                    <input
                      type="text"
                      required
                      value={formData.city}
                      onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="Santo Domingo"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Provincia *
                    </label>
                    <select
                      required
                      value={formData.province}
                      onChange={(e) => setFormData({ ...formData, province: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Seleccionar...</option>
                      <option value="Distrito Nacional">Distrito Nacional</option>
                      <option value="Santiago">Santiago</option>
                      <option value="La Altagracia">La Altagracia</option>
                      <option value="Puerto Plata">Puerto Plata</option>
                      <option value="San Cristóbal">San Cristóbal</option>
                      <option value="La Romana">La Romana</option>
                      <option value="San Pedro de Macorís">San Pedro de Macorís</option>
                      <option value="Duarte">Duarte</option>
                      <option value="La Vega">La Vega</option>
                      <option value="Espaillat">Espaillat</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Código Postal
                    </label>
                    <input
                      type="text"
                      value={formData.postalCode}
                      onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="10147"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
                    <input
                      type="tel"
                      value={formData.phone}
                      onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="809-555-0100"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                    <input
                      type="email"
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="sucursal@dealer.com"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Horario de Atención
                  </label>
                  <input
                    type="text"
                    value={formData.openingHours}
                    onChange={(e) => setFormData({ ...formData, openingHours: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    placeholder="Lun-Vie: 8:00 AM - 6:00 PM, Sáb: 9:00 AM - 3:00 PM"
                  />
                </div>

                <div className="flex items-center gap-4">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                      className="rounded border-gray-300"
                    />
                    <span className="text-sm text-gray-700">Ubicación activa</span>
                  </label>

                  {!editingLocation && (
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.isPrimary}
                        onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                        className="rounded border-gray-300"
                      />
                      <span className="text-sm text-gray-700">Establecer como principal</span>
                    </label>
                  )}
                </div>

                <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={() => {
                      setShowModal(false);
                      resetForm();
                    }}
                    className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                  >
                    {editingLocation ? 'Guardar Cambios' : 'Crear Ubicación'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
