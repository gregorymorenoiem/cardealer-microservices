import { useEffect } from 'react';
import { useParams, Link, Navigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import MainLayout from '@/layouts/MainLayout';
import { LocalizedContent } from '@/components/common';
import ImageGallery from '@/components/organisms/ImageGallery';
import VehicleSpecs from '@/components/organisms/VehicleSpecs';
import ContactSellerForm from '@/components/organisms/ContactSellerForm';
import ReviewsSection from '@/components/organisms/ReviewsSection';
import SimilarVehicles from '@/components/organisms/SimilarVehicles';
import ShareButton from '@/components/molecules/ShareButton';
import PrintButton from '@/components/atoms/PrintButton';
import { getVehicleById, type Vehicle } from '@/services/vehicleService';
import { mockVehicles } from '@/data/mockVehicles';
import { getReviewStats, getVehicleReviews } from '@/data/mockReviews';
import { formatPrice } from '@/utils/formatters';
import { FiHome, FiChevronRight, FiStar, FiMapPin, FiPhone, FiUser, FiHeart, FiLoader, FiAlertCircle, FiWifi, FiWifiOff } from 'react-icons/fi';
import { useFavorites } from '@/hooks/useFavorites';

// Extract the ID from SEO-friendly URL
// Format: /vehicles/{year}-{make}-{model}-{uuid}
// Example: /vehicles/2024-mercedes-benz-clase-c-amg-a1111111-1111-1111-1111-111111111111
const extractIdFromSlug = (slugWithId: string): string => {
  // UUID pattern: 8-4-4-4-12 hexadecimal digits separated by hyphens
  const uuidRegex = /([a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})$/i;
  const match = slugWithId.match(uuidRegex);
  return match ? match[1] : slugWithId;
};

export default function VehicleDetailPage() {
  const { t } = useTranslation('vehicles');
  const { slug } = useParams<{ slug: string }>();
  const id = slug ? extractIdFromSlug(slug) : undefined;
  const { isFavorite, toggleFavorite } = useFavorites();

  // Scroll to top when page loads
  useEffect(() => {
    window.scrollTo(0, 0);
  }, [id]);

  // Fetch vehicle from ProductService
  const {
    data: apiVehicle,
    isLoading, 
    isError,
    error,
  } = useQuery({
    queryKey: ['vehicle', id],
    queryFn: () => getVehicleById(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 1,
  });

  // Fallback to mock data if API fails
  const mockVehicle = mockVehicles.find((v) => v.id === id);
  const isUsingMockData = isError || !apiVehicle;
  const vehicle = apiVehicle || mockVehicle;

  // Show loading state
  if (isLoading) {
    return (
      <MainLayout>
        <div className="bg-gray-50 min-h-screen py-8">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex flex-col items-center justify-center py-20">
              <FiLoader className="animate-spin text-primary mb-4" size={48} />
              <p className="text-gray-600">Loading vehicle details...</p>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!vehicle) {
    return <Navigate to="/vehicles" replace />;
  }

  const vehicleTitle = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;
  const isLiked = isFavorite(vehicle.id);
  
  // Get reviews data
  const reviewStats = getReviewStats(vehicle.id);
  const reviews = getVehicleReviews(vehicle.id);

  return (
    <MainLayout>
      <div className="bg-gray-50 min-h-screen py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Breadcrumbs */}
          <nav className="flex items-center gap-2 text-sm text-gray-600 mb-6">
            <Link to="/" className="hover:text-primary transition-colors duration-200">
              <FiHome size={16} />
            </Link>
            <FiChevronRight size={16} />
            <Link to="/browse" className="hover:text-primary transition-colors duration-200">
              {t('browse.title')}
            </Link>
            <FiChevronRight size={16} />
            <span className="text-gray-900 font-medium">{vehicleTitle}</span>
          </nav>

          {/* Title & Price */}
          <div className="mb-8">
            <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 mb-4">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <h1 className="text-3xl sm:text-4xl font-bold font-heading text-gray-900">
                    {vehicleTitle}
                  </h1>
                  {/* Data source indicator */}
                  <span 
                    className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${
                      isUsingMockData 
                        ? 'bg-amber-100 text-amber-700' 
                        : 'bg-green-100 text-green-700'
                    }`}
                    title={isUsingMockData ? 'Using demo data' : 'Live data from ProductService'}
                  >
                    {isUsingMockData ? <FiWifiOff size={12} /> : <FiWifi size={12} />}
                    {isUsingMockData ? 'Demo' : 'Live'}
                  </span>
                </div>
                <div className="flex items-center gap-4 text-gray-600">
                  <span className="flex items-center gap-1">
                    <FiMapPin size={16} />
                    {vehicle.location || 'Location not specified'}
                  </span>
                  {vehicle.condition && (
                    <span className="px-3 py-1 bg-blue-100 text-blue-800 text-sm font-medium rounded-full">
                      {vehicle.condition}
                    </span>
                  )}
                </div>
              </div>
              <div className="flex flex-col sm:flex-row items-end gap-3">
                <div className="flex items-center gap-3 print:hidden">
                  <button
                    onClick={() => toggleFavorite(vehicle.id)}
                    className={`
                      flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-all
                      ${
                        isLiked
                          ? 'bg-red-500 text-white hover:bg-red-600'
                          : 'border-2 border-gray-300 text-gray-700 hover:border-red-500 hover:text-red-500'
                      }
                    `}
                  >
                    <FiHeart size={20} className={isLiked ? 'fill-white' : ''} />
                    {isLiked ? t('detail.saved') : t('detail.save')}
                  </button>
                  <ShareButton
                    title={vehicleTitle}
                    description={`${vehicleTitle} - ${formatPrice(vehicle.price)} | ${vehicle.location}`}
                  />
                  <PrintButton />
                </div>
                <div className="text-right">
                  <p className="text-4xl font-bold text-primary">
                    {formatPrice(vehicle.price)}
                  </p>
                </div>
              </div>
            </div>

            {/* Badges */}
            <div className="flex flex-wrap gap-2">
              {vehicle.isFeatured && (
                <span className="px-3 py-1 bg-accent text-white text-sm font-medium rounded-full">
                  {t('detail.featured')}
                </span>
              )}
              {vehicle.isNew && (
                <span className="px-3 py-1 bg-green-500 text-white text-sm font-medium rounded-full">
                  {t('detail.new')}
                </span>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left Column - Images & Details */}
            <div className="lg:col-span-2 space-y-8">
              {/* Image Gallery */}
              <ImageGallery images={vehicle.images} alt={vehicleTitle} />

              {/* Description */}
              <div className="bg-white rounded-xl shadow-card p-6">
                <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                  {t('detail.description')}
                </h2>
                <p className="text-gray-700 leading-relaxed whitespace-pre-line">
                  <LocalizedContent content={vehicle.description} showBadge={false} />
                </p>
              </div>

              {/* Specifications */}
              <VehicleSpecs vehicle={vehicle} />

              {/* Features */}
              {vehicle.features && vehicle.features.length > 0 && (
                <div className="bg-white rounded-xl shadow-card p-6">
                  <h2 className="text-2xl font-bold font-heading text-gray-900 mb-4">
                    {t('detail.featuresAndOptions')}
                  </h2>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    {vehicle.features.map((feature, index) => (
                      <div
                        key={index}
                        className="flex items-center gap-2 text-gray-700"
                      >
                        <div className="w-2 h-2 bg-primary rounded-full flex-shrink-0" />
                        <span>{feature}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Right Column - Seller Info & Contact */}
            <div className="space-y-6">
              {/* Seller Info */}
              <div className="bg-white rounded-xl shadow-card p-6">
                <h3 className="text-xl font-bold font-heading text-gray-900 mb-4">
                  {t('detail.sellerInfo')}
                </h3>
                
                <div className="space-y-4 mb-6">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                      <FiUser size={24} className="text-primary" />
                    </div>
                    <div>
                      <p className="font-semibold text-gray-900">{vehicle.seller.name}</p>
                      <p className="text-sm text-gray-600">{vehicle.seller.type}</p>
                    </div>
                  </div>

                  {vehicle.seller.rating && (
                    <div className="flex items-center gap-2">
                      <FiStar size={20} className="text-yellow-400 fill-yellow-400" />
                      <span className="font-semibold text-gray-900">{vehicle.seller.rating}</span>
                      <span className="text-sm text-gray-600">{t('detail.rating')}</span>
                    </div>
                  )}

                  <div className="flex items-center gap-2 text-gray-700">
                    <FiPhone size={18} />
                    <a
                      href={`tel:${vehicle.seller.phone}`}
                      className="hover:text-primary transition-colors duration-200"
                    >
                      {vehicle.seller.phone}
                    </a>
                  </div>
                </div>

                <Link to={`tel:${vehicle.seller.phone}`}>
                  <button className="w-full py-3 px-4 bg-secondary text-white rounded-lg hover:bg-secondary-600 transition-colors duration-200 font-medium mb-3">
                    {t('detail.callSeller')}
                  </button>
                </Link>

                <Link to="/browse">
                  <button className="w-full py-3 px-4 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors duration-200 font-medium">
                    {t('detail.backToBrowse')}
                  </button>
                </Link>
              </div>

              {/* Contact Form */}
              <ContactSellerForm
                vehicleName={vehicleTitle}
                sellerName={vehicle.seller.name}
                sellerPhone={vehicle.seller.phone}
              />
            </div>
          </div>

          {/* Reviews Section */}
          <div className="mt-8">
            <ReviewsSection
              vehicleId={vehicle.id}
              stats={reviewStats}
              reviews={reviews}
            />
          </div>

          {/* Similar Vehicles */}
          <div className="mt-8 print:hidden">
            <SimilarVehicles currentVehicle={vehicle} maxItems={4} />
          </div>
        </div>
      </div>

      {/* Print Styles */}
      <style>{`
        @media print {
          /* Hide navigation, buttons, and interactive elements */
          nav, .print\\:hidden, button:not(.print\\:block) {
            display: none !important;
          }

          /* Optimize layout for print */
          body {
            background: white !important;
          }

          .bg-gray-50 {
            background: white !important;
          }

          /* Remove shadows and borders for cleaner print */
          .shadow-card, .shadow-sm, .shadow-md, .shadow-lg {
            box-shadow: none !important;
            border: 1px solid #e5e7eb;
          }

          /* Ensure content fits on page */
          .max-w-7xl {
            max-width: 100% !important;
          }

          /* Adjust grid for print */
          .grid-cols-1.lg\\:grid-cols-3 {
            grid-template-columns: 1fr !important;
          }

          /* Show essential info */
          .lg\\:col-span-2 {
            grid-column: span 1 !important;
          }

          /* Page breaks */
          .break-before-page {
            page-break-before: always;
          }

          /* Ensure images fit */
          img {
            max-width: 100%;
            page-break-inside: avoid;
          }
        }
      `}</style>
    </MainLayout>
  );
}

