/**
 * Seller Public Profile Page
 * Página de perfil público de vendedor (SELLER-001)
 */

import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import MainLayout from '@/layouts/MainLayout';
import {
  FiMapPin,
  FiCalendar,
  FiPhone,
  FiMail,
  FiGlobe,
  FiStar,
  FiClock,
  FiCheckCircle,
  FiChevronRight,
  FiHeart,
  FiShare2,
  FiMessageCircle,
} from 'react-icons/fi';
import { FaCar, FaWhatsapp } from 'react-icons/fa';
import sellerProfileService, { BADGE_INFO } from '@/services/sellerProfileService';
import type {
  SellerPublicProfile,
  SellerListingsResponse,
  SellerReviewsResponse,
  ContactPreferences,
} from '@/services/sellerProfileService';

export default function SellerPublicProfilePage() {
  const { sellerId } = useParams<{ sellerId: string }>();
  const [profile, setProfile] = useState<SellerPublicProfile | null>(null);
  const [listings, setListings] = useState<SellerListingsResponse | null>(null);
  const [reviews, setReviews] = useState<SellerReviewsResponse | null>(null);
  const [contactPrefs, setContactPrefs] = useState<ContactPreferences | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'listings' | 'reviews'>('listings');

  const loadProfile = useCallback(async () => {
    if (!sellerId) return;

    setLoading(true);
    setError(null);

    try {
      const [profileData, listingsData, reviewsData, contactData] = await Promise.all([
        sellerProfileService.getPublicProfile(sellerId),
        sellerProfileService.getSellerListings(sellerId, 1, 6),
        sellerProfileService.getSellerReviews(sellerId, 1, 5),
        sellerProfileService.getSellerContactPreferences(sellerId),
      ]);

      setProfile(profileData);
      setListings(listingsData);
      setReviews(reviewsData);
      setContactPrefs(contactData);
    } catch (err) {
      setError('Error al cargar el perfil del vendedor');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [sellerId]);

  useEffect(() => {
    if (sellerId) {
      loadProfile();
    }
  }, [sellerId, loadProfile]);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      year: 'numeric',
      month: 'long',
    });
  };

  const renderStars = (rating: number) => {
    return (
      <div className="flex items-center gap-0.5">
        {[1, 2, 3, 4, 5].map((star) => (
          <FiStar
            key={star}
            className={`w-4 h-4 ${
              star <= rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'
            }`}
          />
        ))}
      </div>
    );
  };

  if (loading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (error || !profile) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Vendedor no encontrado</h2>
            <p className="text-gray-600 mb-4">
              {error || 'Este perfil no existe o no está disponible'}
            </p>
            <Link to="/vehicles" className="text-blue-600 hover:underline">
              Ver vehículos disponibles
            </Link>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Cover Photo */}
        <div className="relative h-48 md:h-64 bg-gradient-to-r from-blue-600 to-blue-800">
          {profile.coverPhotoUrl && (
            <img
              src={profile.coverPhotoUrl}
              alt="Cover"
              className="absolute inset-0 w-full h-full object-cover"
            />
          )}
          <div className="absolute inset-0 bg-black/30"></div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Profile Header */}
          <div className="relative -mt-20 mb-8">
            <div className="bg-white rounded-xl shadow-lg p-6">
              <div className="flex flex-col md:flex-row gap-6">
                {/* Avatar */}
                <div className="flex-shrink-0">
                  <div className="w-32 h-32 rounded-full border-4 border-white shadow-lg overflow-hidden bg-gray-200">
                    {profile.profilePhotoUrl ? (
                      <img
                        src={profile.profilePhotoUrl}
                        alt={profile.displayName}
                        className="w-full h-full object-cover"
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-gray-400 text-4xl font-bold">
                        {profile.displayName.charAt(0)}
                      </div>
                    )}
                  </div>
                </div>

                {/* Info */}
                <div className="flex-1">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div>
                      <div className="flex items-center gap-3 mb-2">
                        <h1 className="text-2xl md:text-3xl font-bold text-gray-900">
                          {profile.displayName}
                        </h1>
                        {profile.isVerified && (
                          <span className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-700 text-sm font-medium rounded-full">
                            <FiCheckCircle className="w-4 h-4" />
                            Verificado
                          </span>
                        )}
                      </div>

                      {/* Badges */}
                      {profile.badges.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-3">
                          {profile.badges.map((badge) => {
                            const badgeInfo = BADGE_INFO[badge] || { icon: '🏷️', description: '' };
                            return (
                              <span
                                key={badge}
                                className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-700 text-sm rounded-full"
                                title={badgeInfo.description}
                              >
                                <span>{badgeInfo.icon}</span>
                                {badge.replace(/([A-Z])/g, ' $1').trim()}
                              </span>
                            );
                          })}
                        </div>
                      )}

                      <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                        <span className="flex items-center gap-1">
                          <FiMapPin className="w-4 h-4" />
                          {profile.city}, {profile.province}
                        </span>
                        <span className="flex items-center gap-1">
                          <FiCalendar className="w-4 h-4" />
                          Miembro desde {formatDate(profile.memberSince)}
                        </span>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex gap-2">
                      <button className="p-2 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors">
                        <FiHeart className="w-5 h-5" />
                      </button>
                      <button className="p-2 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors">
                        <FiShare2 className="w-5 h-5" />
                      </button>
                    </div>
                  </div>

                  {/* Stats */}
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
                    <div className="text-center p-3 bg-gray-50 rounded-lg">
                      <div className="text-2xl font-bold text-gray-900">
                        {profile.stats.activeListings}
                      </div>
                      <div className="text-sm text-gray-600">Activos</div>
                    </div>
                    <div className="text-center p-3 bg-gray-50 rounded-lg">
                      <div className="text-2xl font-bold text-gray-900">
                        {profile.stats.soldCount}
                      </div>
                      <div className="text-sm text-gray-600">Vendidos</div>
                    </div>
                    <div className="text-center p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center justify-center gap-1">
                        <FiStar className="w-5 h-5 text-yellow-500 fill-yellow-500" />
                        <span className="text-2xl font-bold text-gray-900">
                          {profile.stats.averageRating.toFixed(1)}
                        </span>
                      </div>
                      <div className="text-sm text-gray-600">
                        ({profile.stats.reviewCount} reseñas)
                      </div>
                    </div>
                    <div className="text-center p-3 bg-gray-50 rounded-lg">
                      <div className="flex items-center justify-center gap-1">
                        <FiClock className="w-5 h-5 text-green-500" />
                        <span className="text-2xl font-bold text-gray-900">
                          {profile.stats.responseRate}%
                        </span>
                      </div>
                      <div className="text-sm text-gray-600">Respuesta</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 pb-12">
            {/* Main Content */}
            <div className="lg:col-span-2 space-y-6">
              {/* Bio */}
              {profile.bio && (
                <div className="bg-white rounded-xl shadow-sm p-6">
                  <h2 className="text-lg font-semibold text-gray-900 mb-3">Sobre nosotros</h2>
                  <p className="text-gray-600">{profile.bio}</p>
                </div>
              )}

              {/* Tabs */}
              <div className="bg-white rounded-xl shadow-sm overflow-hidden">
                <div className="border-b border-gray-200">
                  <nav className="flex -mb-px">
                    <button
                      onClick={() => setActiveTab('listings')}
                      className={`flex-1 py-4 px-4 text-center font-medium text-sm border-b-2 transition-colors ${
                        activeTab === 'listings'
                          ? 'border-blue-600 text-blue-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700'
                      }`}
                    >
                      <FaCar className="inline-block mr-2" />
                      Vehículos ({profile.stats.activeListings})
                    </button>
                    <button
                      onClick={() => setActiveTab('reviews')}
                      className={`flex-1 py-4 px-4 text-center font-medium text-sm border-b-2 transition-colors ${
                        activeTab === 'reviews'
                          ? 'border-blue-600 text-blue-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700'
                      }`}
                    >
                      <FiStar className="inline-block mr-2" />
                      Reseñas ({profile.stats.reviewCount})
                    </button>
                  </nav>
                </div>

                <div className="p-6">
                  {activeTab === 'listings' && (
                    <div>
                      {listings && listings.listings.length > 0 ? (
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                          {listings.listings.map((listing) => (
                            <Link
                              key={listing.id}
                              to={`/vehicles/${listing.slug}`}
                              className="flex gap-4 p-4 border border-gray-200 rounded-lg hover:shadow-md transition-shadow"
                            >
                              <div className="w-24 h-24 rounded-lg overflow-hidden bg-gray-100 flex-shrink-0">
                                {listing.mainImageUrl ? (
                                  <img
                                    src={listing.mainImageUrl}
                                    alt={listing.title}
                                    className="w-full h-full object-cover"
                                  />
                                ) : (
                                  <div className="w-full h-full flex items-center justify-center">
                                    <FaCar className="w-8 h-8 text-gray-400" />
                                  </div>
                                )}
                              </div>
                              <div className="flex-1 min-w-0">
                                <h3 className="font-semibold text-gray-900 truncate">
                                  {listing.title}
                                </h3>
                                <p className="text-lg font-bold text-blue-600">
                                  {formatPrice(listing.price)}
                                </p>
                                <p className="text-sm text-gray-500">
                                  {listing.year} • {listing.mileage.toLocaleString()} km
                                </p>
                              </div>
                            </Link>
                          ))}
                        </div>
                      ) : (
                        <div className="text-center py-8">
                          <FaCar className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                          <p className="text-gray-500">No hay vehículos disponibles</p>
                        </div>
                      )}

                      {listings && listings.totalCount > 6 && (
                        <div className="mt-6 text-center">
                          <Link
                            to={`/sellers/${sellerId}/listings`}
                            className="inline-flex items-center gap-1 text-blue-600 hover:underline font-medium"
                          >
                            Ver todos los vehículos
                            <FiChevronRight className="w-4 h-4" />
                          </Link>
                        </div>
                      )}
                    </div>
                  )}

                  {activeTab === 'reviews' && (
                    <div>
                      {reviews && reviews.reviews.length > 0 ? (
                        <div className="space-y-4">
                          {reviews.reviews.map((review) => (
                            <div
                              key={review.id}
                              className="border-b border-gray-100 pb-4 last:border-0"
                            >
                              <div className="flex items-start gap-3">
                                <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center overflow-hidden">
                                  {review.reviewerPhotoUrl ? (
                                    <img
                                      src={review.reviewerPhotoUrl}
                                      alt={review.reviewerName}
                                      className="w-full h-full object-cover"
                                    />
                                  ) : (
                                    <span className="text-gray-500 font-medium">
                                      {review.reviewerName.charAt(0)}
                                    </span>
                                  )}
                                </div>
                                <div className="flex-1">
                                  <div className="flex items-center gap-2 mb-1">
                                    <span className="font-medium text-gray-900">
                                      {review.reviewerName}
                                    </span>
                                    {review.isVerifiedPurchase && (
                                      <span className="text-xs text-green-600 bg-green-50 px-2 py-0.5 rounded">
                                        Compra verificada
                                      </span>
                                    )}
                                  </div>
                                  <div className="flex items-center gap-2 mb-2">
                                    {renderStars(review.rating)}
                                    <span className="text-sm text-gray-500">
                                      {new Date(review.createdAt).toLocaleDateString()}
                                    </span>
                                  </div>
                                  {review.comment && (
                                    <p className="text-gray-600">{review.comment}</p>
                                  )}
                                  {review.vehicleTitle && (
                                    <p className="text-sm text-gray-500 mt-1">
                                      Vehículo: {review.vehicleTitle}
                                    </p>
                                  )}
                                  {review.reply && (
                                    <div className="mt-3 ml-4 pl-3 border-l-2 border-blue-200">
                                      <p className="text-sm text-gray-600">
                                        <span className="font-medium">
                                          Respuesta del vendedor:{' '}
                                        </span>
                                        {review.reply.content}
                                      </p>
                                    </div>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <div className="text-center py-8">
                          <FiStar className="w-12 h-12 text-gray-300 mx-auto mb-3" />
                          <p className="text-gray-500">Aún no hay reseñas</p>
                        </div>
                      )}

                      {reviews && reviews.totalCount > 5 && (
                        <div className="mt-6 text-center">
                          <Link
                            to={`/sellers/${sellerId}/reviews`}
                            className="inline-flex items-center gap-1 text-blue-600 hover:underline font-medium"
                          >
                            Ver todas las reseñas
                            <FiChevronRight className="w-4 h-4" />
                          </Link>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Sidebar - Contact */}
            <div className="space-y-6">
              <div className="bg-white rounded-xl shadow-sm p-6 sticky top-6">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Contactar</h2>

                {contactPrefs && (
                  <div className="space-y-3">
                    {contactPrefs.allowWhatsApp && contactPrefs.showWhatsAppNumber && (
                      <a
                        href={`https://wa.me/1849XXXXXXX`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-3 w-full p-3 bg-green-500 text-white rounded-lg hover:bg-green-600 transition-colors"
                      >
                        <FaWhatsapp className="w-5 h-5" />
                        <span className="font-medium">WhatsApp</span>
                      </a>
                    )}

                    {contactPrefs.allowPhoneCalls && contactPrefs.showPhoneNumber && (
                      <a
                        href="tel:+18091234567"
                        className="flex items-center gap-3 w-full p-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                      >
                        <FiPhone className="w-5 h-5 text-gray-600" />
                        <span className="font-medium text-gray-700">Llamar</span>
                      </a>
                    )}

                    {contactPrefs.allowInAppChat && (
                      <button className="flex items-center gap-3 w-full p-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
                        <FiMessageCircle className="w-5 h-5 text-gray-600" />
                        <span className="font-medium text-gray-700">Enviar mensaje</span>
                      </button>
                    )}

                    {contactPrefs.allowEmail && contactPrefs.showEmail && (
                      <a
                        href="mailto:vendedor@email.com"
                        className="flex items-center gap-3 w-full p-3 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                      >
                        <FiMail className="w-5 h-5 text-gray-600" />
                        <span className="font-medium text-gray-700">Email</span>
                      </a>
                    )}
                  </div>
                )}

                {/* Horario */}
                {contactPrefs && (
                  <div className="mt-6 pt-6 border-t border-gray-200">
                    <h3 className="text-sm font-medium text-gray-700 mb-2">Horario de atención</h3>
                    <p className="text-sm text-gray-600">
                      <FiClock className="inline-block mr-1" />
                      {contactPrefs.contactHoursStart} - {contactPrefs.contactHoursEnd}
                    </p>
                    <p className="text-sm text-gray-500 mt-1">
                      {contactPrefs.contactDays.join(', ')}
                    </p>
                  </div>
                )}

                {/* Tiempo de respuesta */}
                <div className="mt-6 pt-6 border-t border-gray-200">
                  <div className="flex items-center gap-2 text-sm">
                    <FiClock className="w-4 h-4 text-green-500" />
                    <span className="text-gray-600">
                      Responde generalmente en {profile.stats.responseTime}
                    </span>
                  </div>
                </div>

                {/* Dealer Info */}
                {profile.dealer && (
                  <div className="mt-6 pt-6 border-t border-gray-200">
                    <h3 className="text-sm font-medium text-gray-700 mb-2">
                      Información del dealer
                    </h3>
                    <p className="font-medium text-gray-900">{profile.dealer.businessName}</p>
                    {profile.dealer.website && (
                      <a
                        href={profile.dealer.website}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="flex items-center gap-1 text-sm text-blue-600 hover:underline mt-1"
                      >
                        <FiGlobe className="w-4 h-4" />
                        Visitar sitio web
                      </a>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
