import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Building2, FileText, Loader2 } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';
import { dealerManagementService } from '@/services/dealerManagementService';
import type { CreateDealerRequest } from '@/services/dealerManagementService';

export default function DealerRegistrationPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const selectedPlan = searchParams.get('plan') || 'Pro';

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    businessName: '',
    rnc: '',
    legalName: '',
    type: 'Independent',
    email: '',
    phone: '',
    mobilePhone: '',
    website: '',
    address: '',
    city: '',
    province: '',
    description: '',
    establishedDate: '',
    employeeCount: '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Get current user ID from local storage or auth context
      const userId = localStorage.getItem('userId') || '';

      if (!userId) {
        throw new Error('Debes iniciar sesión para registrar un dealer');
      }

      const request: CreateDealerRequest = {
        userId,
        businessName: formData.businessName,
        rnc: formData.rnc,
        legalName: formData.legalName || undefined,
        type: formData.type,
        email: formData.email,
        phone: formData.phone,
        mobilePhone: formData.mobilePhone || undefined,
        website: formData.website || undefined,
        address: formData.address,
        city: formData.city,
        province: formData.province,
        description: formData.description || undefined,
        establishedDate: formData.establishedDate || undefined,
        employeeCount: formData.employeeCount ? parseInt(formData.employeeCount) : undefined,
      };

      const dealer = await dealerManagementService.createDealer(request);

      // Redirect to subscription checkout
      navigate(`/dealer/subscribe?dealerId=${dealer.id}&plan=${selectedPlan}`);
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Error al registrar dealer');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-12">
        <div className="container mx-auto px-4">
          <div className="max-w-3xl mx-auto">
            {/* Header */}
            <div className="text-center mb-8">
              <Building2 className="w-16 h-16 mx-auto mb-4 text-blue-600" />
              <h1 className="text-4xl font-bold mb-2">Registro de Dealer</h1>
              <p className="text-gray-600">Complete el formulario para crear su cuenta de dealer</p>
              <div className="mt-4 inline-block bg-blue-100 text-blue-800 px-4 py-2 rounded-full">
                Plan seleccionado: <strong>{selectedPlan}</strong>
              </div>
            </div>

            {/* Form */}
            <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow-lg p-8">
              {error && (
                <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                  {error}
                </div>
              )}

              {/* Business Information */}
              <div className="mb-8">
                <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                  <Building2 className="w-5 h-5" />
                  Información del Negocio
                </h2>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Nombre Comercial <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="businessName"
                      value={formData.businessName}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="Ej: AutoExcel Motors"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      RNC <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="rnc"
                      value={formData.rnc}
                      onChange={handleChange}
                      required
                      pattern="[0-9]{9,11}"
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="000000000"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">Razón Social</label>
                    <input
                      type="text"
                      name="legalName"
                      value={formData.legalName}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="Nombre legal de la empresa"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">Tipo de Dealer</label>
                    <select
                      name="type"
                      value={formData.type}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    >
                      <option value="Independent">Independiente</option>
                      <option value="Chain">Cadena</option>
                      <option value="MultipleStore">Multi-tienda</option>
                      <option value="Franchise">Franquicia</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Contact Information */}
              <div className="mb-8">
                <h2 className="text-xl font-bold mb-4">Información de Contacto</h2>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Email <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="email"
                      name="email"
                      value={formData.email}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Teléfono <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="tel"
                      name="phone"
                      value={formData.phone}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="809-000-0000"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">Celular</label>
                    <input
                      type="tel"
                      name="mobilePhone"
                      value={formData.mobilePhone}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="829-000-0000"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">Website</label>
                    <input
                      type="url"
                      name="website"
                      value={formData.website}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="https://www.ejemplo.com"
                    />
                  </div>
                </div>
              </div>

              {/* Location */}
              <div className="mb-8">
                <h2 className="text-xl font-bold mb-4">Ubicación</h2>
                <div className="grid md:grid-cols-2 gap-6">
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-2">
                      Dirección <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="address"
                      value={formData.address}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Ciudad <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="city"
                      value={formData.city}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Provincia <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="province"
                      value={formData.province}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>
                </div>
              </div>

              {/* Additional Info */}
              <div className="mb-8">
                <h2 className="text-xl font-bold mb-4">Información Adicional</h2>
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium mb-2">Año de Fundación</label>
                    <input
                      type="date"
                      name="establishedDate"
                      value={formData.establishedDate}
                      onChange={handleChange}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2">Número de Empleados</label>
                    <input
                      type="number"
                      name="employeeCount"
                      value={formData.employeeCount}
                      onChange={handleChange}
                      min="1"
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium mb-2">Descripción</label>
                    <textarea
                      name="description"
                      value={formData.description}
                      onChange={handleChange}
                      rows={4}
                      className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      placeholder="Describe tu negocio, especialidades, años de experiencia..."
                    />
                  </div>
                </div>
              </div>

              {/* Note */}
              <div className="bg-blue-50 border border-blue-200 text-blue-800 px-4 py-3 rounded mb-6 flex gap-3">
                <FileText className="w-5 h-5 flex-shrink-0 mt-0.5" />
                <div>
                  <p className="font-semibold mb-1">Verificación de Documentos</p>
                  <p className="text-sm">
                    Después del registro, necesitarás subir: RNC, Licencia Comercial y Cédula del
                    propietario. La verificación toma 1-2 días hábiles.
                  </p>
                </div>
              </div>

              {/* Submit */}
              <div className="flex gap-4">
                <button
                  type="button"
                  onClick={() => navigate('/dealer/pricing')}
                  className="flex-1 px-6 py-3 border border-gray-300 rounded-lg font-semibold hover:bg-gray-50 transition-colors"
                >
                  Volver
                </button>
                <button
                  type="submit"
                  disabled={loading}
                  className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                >
                  {loading ? (
                    <>
                      <Loader2 className="w-5 h-5 animate-spin" />
                      Registrando...
                    </>
                  ) : (
                    'Continuar a Suscripción'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
