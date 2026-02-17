import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  FiPhone,
  FiMail,
  FiMapPin,
  FiExternalLink,
  FiClock,
  FiFacebook,
  FiInstagram,
  FiTwitter,
} from 'react-icons/fi';
import { FaWhatsapp, FaYoutube } from 'react-icons/fa';
import { dealerPublicService } from '@/services/dealerPublicService';
import type { PublicDealerProfile, PublicLocation } from '@/services/dealerPublicService';
import MainLayout from '@/layouts/MainLayout';

export default function PublicDealerProfilePage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const [dealer, setDealer] = useState<PublicDealerProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!slug) {
      setError('Dealer no encontrado');
      setLoading(false);
      return;
    }

    fetchDealer();
  }, [slug]);

  const fetchDealer = async () => {
    try {
      setLoading(true);
      const data = await dealerPublicService.getPublicProfile(slug!);
      setDealer(data);

      // Set SEO meta tags
      if (data.seo?.metaTitle) {
        document.title = data.seo.metaTitle;
      } else {
        document.title = `${data.businessName} - OKLA`;
      }

      if (data.seo?.metaDescription) {
        const metaDesc = document.querySelector('meta[name="description"]');
        if (metaDesc) {
          metaDesc.setAttribute('content', data.seo.metaDescription);
        }
      }
    } catch (err: any) {
      console.error('Error fetching dealer:', err);
      setError(err.response?.data?.message || 'Error al cargar el perfil del dealer');
    } finally {
      setLoading(false);
    }
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

  if (error || !dealer) {
    return (
      <MainLayout>
        <div className="min-h-screen flex flex-col items-center justify-center px-4">
          <div className="text-red-600 text-6xl mb-4">⚠️</div>
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            {error || 'Dealer no encontrado'}
          </h1>
          <p className="text-gray-600 mb-6">El dealer que buscas no existe o no está disponible.</p>
          <button
            onClick={() => navigate('/vehicles')}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Ver Todos los Vehículos
          </button>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Banner */}
        {dealer.bannerUrl && (
          <div className="w-full h-64 bg-gradient-to-r from-blue-600 to-blue-800 relative">
            <img
              src={dealer.bannerUrl}
              alt={dealer.businessName}
              className="w-full h-full object-cover opacity-80"
            />
          </div>
        )}

        {/* Header */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 -mt-16 relative z-10">
          <div className="bg-white rounded-lg shadow-lg p-6 mb-8">
            <div className="flex flex-col md:flex-row gap-6">
              {/* Logo */}
              {dealer.logoUrl && (
                <div className="flex-shrink-0">
                  <img
                    src={dealer.logoUrl}
                    alt={dealer.businessName}
                    className="w-32 h-32 rounded-lg object-contain bg-gray-100 border-4 border-white shadow-md"
                  />
                </div>
              )}

              {/* Info */}
              <div className="flex-1">
                <div className="flex flex-wrap items-start gap-3 mb-3">
                  <h1 className="text-3xl font-bold text-gray-900">{dealer.businessName}</h1>

                  {/* Badges */}
                  {dealer.isTrustedDealer && (
                    <span className="px-3 py-1 bg-blue-600 text-white text-sm rounded-full flex items-center gap-1">
                      <span>✓</span> Dealer Verificado
                    </span>
                  )}

                  {dealer.isFoundingMember && (
                    <span className="px-3 py-1 bg-amber-600 text-white text-sm rounded-full flex items-center gap-1">
                      <span>🏆</span> Miembro Fundador
                    </span>
                  )}
                </div>

                {dealer.slogan && (
                  <p className="text-lg text-gray-600 mb-3 italic">"{dealer.slogan}"</p>
                )}

                {/* Rating & Stats */}
                <div className="flex flex-wrap gap-6 mb-4">
                  <div className="flex items-center gap-2">
                    <span className="text-yellow-500 text-xl">
                      {dealerPublicService.getRatingStars(dealer.averageRating)}
                    </span>
                    <span className="text-gray-700 font-semibold">
                      {dealerPublicService.formatRating(dealer.averageRating)}
                    </span>
                    <span className="text-gray-500">({dealer.totalReviews} reviews)</span>
                  </div>

                  <div className="text-gray-700">
                    <span className="font-semibold">{dealer.activeListings}</span> vehículos activos
                  </div>

                  <div className="text-gray-700">
                    <span className="font-semibold">{dealer.totalSales}</span> ventas realizadas
                  </div>
                </div>

                {/* Location */}
                <div className="flex items-center gap-2 text-gray-600 mb-4">
                  <FiMapPin className="w-4 h-4" />
                  <span>
                    {dealer.city}, {dealer.province}
                  </span>
                </div>

                {/* Contact Buttons */}
                <div className="flex flex-wrap gap-3">
                  {dealer.contactInfo.showPhone && dealer.contactInfo.phone && (
                    <a
                      href={`tel:${dealer.contactInfo.phone}`}
                      className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 flex items-center gap-2"
                    >
                      <FiPhone /> Llamar
                    </a>
                  )}

                  {dealer.contactInfo.whatsAppNumber && (
                    <a
                      href={dealerPublicService.getWhatsAppLink(
                        dealer.contactInfo.whatsAppNumber,
                        `Hola, estoy interesado en sus vehículos en OKLA`
                      )}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 flex items-center gap-2"
                    >
                      <FaWhatsapp /> WhatsApp
                    </a>
                  )}

                  {dealer.contactInfo.showEmail && dealer.contactInfo.email && (
                    <a
                      href={`mailto:${dealer.contactInfo.email}`}
                      className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 flex items-center gap-2"
                    >
                      <FiMail /> Email
                    </a>
                  )}

                  {dealer.contactInfo.website && (
                    <a
                      href={dealer.contactInfo.website}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="px-4 py-2 bg-gray-700 text-white rounded-lg hover:bg-gray-800 flex items-center gap-2"
                    >
                      <FiExternalLink /> Sitio Web
                    </a>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pb-12">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Main Content */}
            <div className="lg:col-span-2 space-y-8">
              {/* About */}
              {dealer.aboutUs && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h2 className="text-2xl font-bold text-gray-900 mb-4">
                    Acerca de {dealer.businessName}
                  </h2>
                  <p className="text-gray-700 whitespace-pre-line leading-relaxed">
                    {dealer.aboutUs}
                  </p>
                </div>
              )}

              {/* Features */}
              {dealer.features.length > 0 && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h2 className="text-2xl font-bold text-gray-900 mb-4">
                    Servicios y Características
                  </h2>
                  <div className="grid grid-cols-2 gap-4">
                    {dealer.features.map((feature, index) => (
                      <div
                        key={index}
                        className={`flex items-center gap-3 p-3 rounded-lg ${
                          feature.isAvailable
                            ? 'bg-green-50 text-green-700'
                            : 'bg-gray-50 text-gray-400'
                        }`}
                      >
                        <span className="text-2xl">{feature.icon}</span>
                        <span className="font-medium">{feature.name}</span>
                        {feature.isAvailable && <span className="ml-auto">✓</span>}
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Locations */}
              {dealer.locations.length > 0 && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h2 className="text-2xl font-bold text-gray-900 mb-4">Sucursales</h2>
                  <div className="space-y-4">
                    {dealer.locations.map((location) => (
                      <LocationCard key={location.id} location={location} />
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Sidebar */}
            <div className="space-y-6">
              {/* Specialties */}
              {dealer.specialties.length > 0 && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h3 className="text-lg font-bold text-gray-900 mb-3">Especialidades</h3>
                  <div className="flex flex-wrap gap-2">
                    {dealer.specialties.map((specialty, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-blue-100 text-blue-700 text-sm rounded-full"
                      >
                        {specialty}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Supported Brands */}
              {dealer.supportedBrands.length > 0 && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h3 className="text-lg font-bold text-gray-900 mb-3">Marcas que Manejamos</h3>
                  <div className="flex flex-wrap gap-2">
                    {dealer.supportedBrands.map((brand, index) => (
                      <span
                        key={index}
                        className="px-3 py-1 bg-gray-100 text-gray-700 text-sm rounded-full"
                      >
                        {brand}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Social Media */}
              {dealer.socialMedia && (
                <div className="bg-white rounded-lg shadow p-6">
                  <h3 className="text-lg font-bold text-gray-900 mb-3">Síguenos</h3>
                  <div className="space-y-2">
                    {dealer.socialMedia.facebookUrl && (
                      <a
                        href={dealer.socialMedia.facebookUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-2 text-blue-600 hover:underline"
                      >
                        <FiFacebook /> Facebook
                      </a>
                    )}
                    {dealer.socialMedia.instagramUrl && (
                      <a
                        href={dealer.socialMedia.instagramUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-2 text-pink-600 hover:underline"
                      >
                        <FiInstagram /> Instagram
                      </a>
                    )}
                    {dealer.socialMedia.twitterUrl && (
                      <a
                        href={dealer.socialMedia.twitterUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-2 text-blue-400 hover:underline"
                      >
                        <FiTwitter /> Twitter
                      </a>
                    )}
                    {dealer.socialMedia.youTubeUrl && (
                      <a
                        href={dealer.socialMedia.youTubeUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-2 text-red-600 hover:underline"
                      >
                        <FaYoutube /> YouTube
                      </a>
                    )}
                  </div>
                </div>
              )}

              {/* Member Since */}
              {dealer.trustedDealerSince && (
                <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg shadow p-6 text-center">
                  <p className="text-blue-900 font-semibold">
                    {dealerPublicService.formatMemberSince(dealer.trustedDealerSince)}
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}

// ============================================
// Location Card Component
// ============================================

function LocationCard({ location }: { location: PublicLocation }) {
  const openStatus = dealerPublicService.getOpenStatus(location);
  const todayHours = dealerPublicService.getTodayHours(location);

  return (
    <div className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-3">
        <div>
          <h4 className="font-bold text-gray-900">{location.name}</h4>
          {location.isPrimary && (
            <span className="text-xs text-blue-600 font-medium">Sucursal Principal</span>
          )}
        </div>

        <span
          className={`px-2 py-1 text-xs rounded-full ${
            openStatus === 'open' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
          }`}
        >
          {openStatus === 'open' ? 'Abierto' : 'Cerrado'}
        </span>
      </div>

      <div className="space-y-2 text-sm text-gray-600 mb-3">
        <div className="flex items-start gap-2">
          <FiMapPin className="w-4 h-4 mt-0.5 flex-shrink-0" />
          <span>
            {location.address}, {location.city}
          </span>
        </div>

        <div className="flex items-center gap-2">
          <FiClock className="w-4 h-4 flex-shrink-0" />
          <span>Hoy: {todayHours}</span>
        </div>

        <div className="flex items-center gap-2">
          <FiPhone className="w-4 h-4 flex-shrink-0" />
          <a href={`tel:${location.phone}`} className="hover:underline">
            {location.phone}
          </a>
        </div>
      </div>

      {/* Features */}
      <div className="flex flex-wrap gap-2 mb-3">
        {location.hasShowroom && (
          <span className="text-xs px-2 py-1 bg-blue-50 text-blue-700 rounded">Showroom</span>
        )}
        {location.hasServiceCenter && (
          <span className="text-xs px-2 py-1 bg-green-50 text-green-700 rounded">
            Centro de Servicio
          </span>
        )}
        {location.hasParking && (
          <span className="text-xs px-2 py-1 bg-gray-50 text-gray-700 rounded">
            Estacionamiento
          </span>
        )}
      </div>

      {/* Map Link */}
      <a
        href={dealerPublicService.getMapUrl(location)}
        target="_blank"
        rel="noopener noreferrer"
        className="text-blue-600 hover:underline text-sm flex items-center gap-1"
      >
        <FiExternalLink className="w-3 h-3" />
        Ver en Google Maps
      </a>
    </div>
  );
}
