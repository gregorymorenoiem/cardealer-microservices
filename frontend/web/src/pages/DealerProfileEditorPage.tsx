import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FiSave, FiAlertCircle, FiCheck, FiExternalLink } from 'react-icons/fi';
import { dealerPublicService } from '@/services/dealerPublicService';
import type {
  PublicDealerProfile,
  ProfileCompletion,
  UpdateProfileRequest,
} from '@/services/dealerPublicService';
import { dealerManagementService } from '@/services/dealerManagementService';
import MainLayout from '@/layouts/MainLayout';

export default function DealerProfileEditorPage() {
  const navigate = useNavigate();

  const [dealer, setDealer] = useState<PublicDealerProfile | null>(null);
  const [completion, setCompletion] = useState<ProfileCompletion | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Form state
  const [formData, setFormData] = useState<UpdateProfileRequest>({});

  useEffect(() => {
    fetchDealerProfile();
  }, []);

  const fetchDealerProfile = async () => {
    try {
      setLoading(true);

      // Get dealer by current user
      const dealerId = localStorage.getItem('dealerId'); // Assuming this is set after login

      if (!dealerId) {
        setError('No se encontró información del dealer. Por favor contacte soporte.');
        return;
      }

      const dealerData = await dealerManagementService.getDealerById(dealerId);

      // Get completion status
      const completionData = await dealerPublicService.getProfileCompletion(dealerId);
      setCompletion(completionData);

      // Map to PublicDealerProfile (simplified)
      const profile: PublicDealerProfile = {
        ...dealerData,
        contactInfo: {
          phone: dealerData.phone,
          email: dealerData.email,
          website: dealerData.website,
          whatsAppNumber: dealerData.whatsAppNumber,
          showPhone: dealerData.showPhoneOnProfile ?? false,
          showEmail: dealerData.showEmailOnProfile ?? false,
        },
        features: [],
        locations: [],
        specialties: dealerData.specialties || [],
        supportedBrands: dealerData.supportedBrands || [],
        slug: dealerData.slug || '',
        averageRating: dealerData.averageRating || 0,
        totalReviews: dealerData.totalReviews || 0,
        totalSales: dealerData.totalSales || 0,
        activeListings: 0,
      };

      setDealer(profile);

      // Initialize form
      setFormData({
        slogan: dealerData.slogan,
        aboutUs: dealerData.aboutUs,
        specialties: dealerData.specialties || [],
        supportedBrands: dealerData.supportedBrands || [],
        logoUrl: dealerData.logoUrl,
        bannerUrl: dealerData.bannerUrl,
        facebookUrl: dealerData.facebookUrl,
        instagramUrl: dealerData.instagramUrl,
        twitterUrl: dealerData.twitterUrl,
        youTubeUrl: dealerData.youTubeUrl,
        whatsAppNumber: dealerData.whatsAppNumber,
        showPhoneOnProfile: dealerData.showPhoneOnProfile ?? false,
        showEmailOnProfile: dealerData.showEmailOnProfile ?? false,
        acceptsTradeIns: dealerData.acceptsTradeIns ?? false,
        offersFinancing: dealerData.offersFinancing ?? false,
        offersWarranty: dealerData.offersWarranty ?? false,
        offersHomeDelivery: dealerData.offersHomeDelivery ?? false,
        metaTitle: dealerData.metaTitle,
        metaDescription: dealerData.metaDescription,
        metaKeywords: dealerData.metaKeywords,
      });
    } catch (err: any) {
      console.error('Error fetching dealer:', err);
      setError(err.response?.data?.message || 'Error al cargar el perfil');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!dealer) return;

    try {
      setSaving(true);
      setError(null);
      setSuccess(false);

      await dealerPublicService.updateProfile(dealer.id, formData);

      setSuccess(true);

      // Refresh data
      await fetchDealerProfile();

      // Auto-hide success message
      setTimeout(() => setSuccess(false), 3000);
    } catch (err: any) {
      console.error('Error saving profile:', err);
      setError(err.response?.data?.message || 'Error al guardar el perfil');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (field: keyof UpdateProfileRequest, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleArrayChange = (field: 'specialties' | 'supportedBrands', value: string) => {
    const items = value
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);
    handleChange(field, items);
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (error && !dealer) {
    return (
      <MainLayout>
        <div className="min-h-screen flex flex-col items-center justify-center px-4">
          <div className="text-red-600 text-6xl mb-4">
            <FiAlertCircle />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">{error}</h1>
          <button
            onClick={() => navigate('/dealer/dashboard')}
            className="mt-4 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Volver al Dashboard
          </button>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Editar Perfil Público</h1>
            <p className="text-gray-600">Optimiza tu perfil para atraer más clientes</p>
          </div>

          {/* Profile Completion */}
          {completion && (
            <div className="bg-white rounded-lg shadow-md p-6 mb-6">
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-lg font-semibold text-gray-900">Completitud del Perfil</h3>
                <span className="text-2xl font-bold text-blue-600">
                  {completion.completionPercentage}%
                </span>
              </div>

              <div className="w-full bg-gray-200 rounded-full h-3 mb-4">
                <div
                  className="bg-blue-600 h-3 rounded-full transition-all duration-500"
                  style={{ width: `${completion.completionPercentage}%` }}
                />
              </div>

              {completion.missingFields.length > 0 && (
                <div className="mt-4">
                  <p className="text-sm text-gray-600 mb-2">
                    Campos faltantes ({completion.missingFields.length}):
                  </p>
                  <div className="flex flex-wrap gap-2">
                    {completion.missingFields.map((field, index) => (
                      <span
                        key={index}
                        className="text-xs px-2 py-1 bg-amber-100 text-amber-700 rounded"
                      >
                        {field}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Success Message */}
          {success && (
            <div className="bg-green-50 border-l-4 border-green-500 p-4 mb-6 flex items-center gap-3">
              <FiCheck className="text-green-600 text-xl" />
              <p className="text-green-700 font-medium">¡Perfil actualizado exitosamente!</p>
            </div>
          )}

          {/* Error Message */}
          {error && (
            <div className="bg-red-50 border-l-4 border-red-500 p-4 mb-6 flex items-center gap-3">
              <FiAlertCircle className="text-red-600 text-xl" />
              <p className="text-red-700">{error}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Basic Info */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Información Básica</h3>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Slogan</label>
                  <input
                    type="text"
                    value={formData.slogan || ''}
                    onChange={(e) => handleChange('slogan', e.target.value)}
                    placeholder="Tu mejor opción en vehículos premium"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    maxLength={100}
                  />
                  <p className="text-xs text-gray-500 mt-1">Máximo 100 caracteres</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Acerca de Nosotros
                  </label>
                  <textarea
                    value={formData.aboutUs || ''}
                    onChange={(e) => handleChange('aboutUs', e.target.value)}
                    rows={5}
                    placeholder="Describe tu negocio, historia, valores, etc."
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Especialidades
                  </label>
                  <input
                    type="text"
                    value={formData.specialties?.join(', ') || ''}
                    onChange={(e) => handleArrayChange('specialties', e.target.value)}
                    placeholder="SUVs de lujo, Vehículos eléctricos, Sedanes deportivos"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                  <p className="text-xs text-gray-500 mt-1">Separar con comas</p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Marcas que Manejamos
                  </label>
                  <input
                    type="text"
                    value={formData.supportedBrands?.join(', ') || ''}
                    onChange={(e) => handleArrayChange('supportedBrands', e.target.value)}
                    placeholder="Toyota, Honda, BMW, Mercedes-Benz"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                  <p className="text-xs text-gray-500 mt-1">Separar con comas</p>
                </div>
              </div>
            </div>

            {/* Images */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Imágenes</h3>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Logo URL</label>
                  <input
                    type="url"
                    value={formData.logoUrl || ''}
                    onChange={(e) => handleChange('logoUrl', e.target.value)}
                    placeholder="https://ejemplo.com/logo.png"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Banner URL</label>
                  <input
                    type="url"
                    value={formData.bannerUrl || ''}
                    onChange={(e) => handleChange('bannerUrl', e.target.value)}
                    placeholder="https://ejemplo.com/banner.jpg"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                  <p className="text-xs text-gray-500 mt-1">Recomendado: 1920x400px</p>
                </div>
              </div>
            </div>

            {/* Social Media */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Redes Sociales</h3>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Facebook</label>
                  <input
                    type="url"
                    value={formData.facebookUrl || ''}
                    onChange={(e) => handleChange('facebookUrl', e.target.value)}
                    placeholder="https://facebook.com/tudealer"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Instagram</label>
                  <input
                    type="url"
                    value={formData.instagramUrl || ''}
                    onChange={(e) => handleChange('instagramUrl', e.target.value)}
                    placeholder="https://instagram.com/tudealer"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">WhatsApp</label>
                  <input
                    type="tel"
                    value={formData.whatsAppNumber || ''}
                    onChange={(e) => handleChange('whatsAppNumber', e.target.value)}
                    placeholder="+18095551234"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
            </div>

            {/* Features */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Servicios Ofrecidos</h3>

              <div className="space-y-3">
                {[
                  { field: 'acceptsTradeIns', label: 'Aceptamos Trade-ins' },
                  { field: 'offersFinancing', label: 'Ofrecemos Financiamiento' },
                  { field: 'offersWarranty', label: 'Garantía Extendida' },
                  { field: 'offersHomeDelivery', label: 'Entrega a Domicilio' },
                ].map(({ field, label }) => (
                  <label key={field} className="flex items-center gap-3 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={(formData[field as keyof UpdateProfileRequest] as boolean) || false}
                      onChange={(e) =>
                        handleChange(field as keyof UpdateProfileRequest, e.target.checked)
                      }
                      className="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                    />
                    <span className="text-gray-700">{label}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Privacy */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Configuración de Privacidad
              </h3>

              <div className="space-y-3">
                <label className="flex items-center gap-3 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={formData.showPhoneOnProfile || false}
                    onChange={(e) => handleChange('showPhoneOnProfile', e.target.checked)}
                    className="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                  />
                  <span className="text-gray-700">Mostrar teléfono en perfil público</span>
                </label>

                <label className="flex items-center gap-3 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={formData.showEmailOnProfile || false}
                    onChange={(e) => handleChange('showEmailOnProfile', e.target.checked)}
                    className="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500"
                  />
                  <span className="text-gray-700">Mostrar email en perfil público</span>
                </label>
              </div>
            </div>

            {/* Actions */}
            <div className="flex flex-col sm:flex-row gap-4">
              <button
                type="submit"
                disabled={saving}
                className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 flex items-center justify-center gap-2"
              >
                {saving ? (
                  <>
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white"></div>
                    Guardando...
                  </>
                ) : (
                  <>
                    <FiSave /> Guardar Cambios
                  </>
                )}
              </button>

              {dealer && (
                <a
                  href={`/dealers/${dealer.slug}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="px-6 py-3 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 flex items-center justify-center gap-2"
                >
                  <FiExternalLink /> Ver Perfil Público
                </a>
              )}
            </div>
          </form>
        </div>
      </div>
    </MainLayout>
  );
}
