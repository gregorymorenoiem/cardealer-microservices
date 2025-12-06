import { useState } from 'react';
import { motion } from 'framer-motion';
import { useParams, Link } from 'react-router-dom';
import {
  ChevronLeft,
  MapPin,
  Calendar,
  Shield,
  Clock,
  Eye,
  Gauge,
  Fuel,
  Settings,
  Car,
  Palette,
} from 'lucide-react';
import { OklaLayout } from '../layouts/OklaLayout';
import { OklaImageGallery } from '../components/okla/detail/OklaImageGallery';
import { OklaSpecsTable } from '../components/okla/detail/OklaSpecsTable';
import { OklaSellerInfo } from '../components/okla/detail/OklaSellerInfo';
import { OklaContactForm } from '../components/okla/detail/OklaContactForm';
import { OklaListingActions } from '../components/okla/detail/OklaListingActions';
import { OklaRelatedListings } from '../components/okla/detail/OklaRelatedListings';
import { OklaBadge } from '../components/atoms/okla/OklaBadge';
import { OklaButton } from '../components/atoms/okla/OklaButton';
import { FadeIn } from '../components/okla/animations/FadeIn';

// Mock data for the listing
const mockListing = {
  id: 'listing-1',
  title: 'Mercedes-Benz Clase C 300 Sport Edition',
  subtitle: 'Sedan ejecutivo con paquete deportivo AMG Line',
  price: 65000,
  originalPrice: 72000,
  location: 'San Juan, PR',
  year: 2023,
  mileage: 12500,
  views: 342,
  daysOnMarket: 5,
  condition: 'Certificado',
  isNew: false,
  isFeatured: true,
  isVerified: true,
  images: [
    'https://images.unsplash.com/photo-1549399542-7e8ee8c6e7a0?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=800&h=600&fit=crop',
    'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&h=600&fit=crop',
  ],
  description: `Este Mercedes-Benz Clase C 300 2023 representa la perfecta combinación de lujo, rendimiento y tecnología. Con apenas 12,500 millas, este vehículo se encuentra en condiciones excepcionales y ha sido mantenido bajo los más altos estándares del fabricante.

El paquete AMG Line incluye fascia deportiva, llantas de aleación de 19 pulgadas, asientos deportivos en cuero Nappa y acabados en aluminio cepillado negro. El motor turboalimentado de 2.0L produce 255 caballos de fuerza, ofreciendo una aceleración impresionante con un consumo eficiente de combustible.

Este vehículo ha sido certificado por nuestro programa de pre-owned, lo que garantiza que cumple con los más estrictos estándares de calidad. Incluye garantía extendida y servicio de mantenimiento por 2 años adicionales.`,
  specs: [
    { label: 'Año', value: '2023', icon: <Calendar className="w-5 h-5" /> },
    { label: 'Millaje', value: '12,500 mi', icon: <Gauge className="w-5 h-5" /> },
    { label: 'Motor', value: '2.0L Turbo', icon: <Settings className="w-5 h-5" /> },
    { label: 'Potencia', value: '255 HP', icon: <Fuel className="w-5 h-5" /> },
    { label: 'Transmisión', value: 'Automática 9G', icon: <Car className="w-5 h-5" /> },
    { label: 'Tracción', value: 'RWD', icon: <Settings className="w-5 h-5" /> },
    { label: 'Color Exterior', value: 'Plata Iridio', icon: <Palette className="w-5 h-5" /> },
    { label: 'Color Interior', value: 'Negro/Marrón', icon: <Palette className="w-5 h-5" /> },
  ],
  features: [
    { label: 'Techo Solar Panorámico', available: true },
    { label: 'Asientos de Cuero', available: true },
    { label: 'Navegación GPS', available: true },
    { label: 'Apple CarPlay / Android Auto', available: true },
    { label: 'Cámara de Reversa 360°', available: true },
    { label: 'Asientos Calefaccionados', available: true },
    { label: 'Asientos Ventilados', available: true },
    { label: 'Sensores de Estacionamiento', available: true },
    { label: 'Sistema de Audio Burmester', available: true },
    { label: 'Control de Crucero Adaptativo', available: true },
    { label: 'Asistente de Mantenimiento de Carril', available: true },
    { label: 'Faros LED Inteligentes', available: true },
  ],
  seller: {
    id: 'seller-1',
    name: 'Premium Motors PR',
    type: 'certified' as const,
    avatar: 'https://images.unsplash.com/photo-1560179707-f14e90ef3623?w=100&h=100&fit=crop',
    verified: true,
    rating: 4.9,
    reviewCount: 156,
    memberSince: 'Enero 2019',
    location: 'San Juan, PR',
    responseRate: 98,
    responseTime: '< 1 hora',
    listingsCount: 45,
  },
};

// Mock related listings
const mockRelatedListings = [
  {
    id: 'rel-1',
    title: 'BMW Serie 5 530i M Sport',
    price: 62000,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400&h=300&fit=crop',
    location: 'Bayamón',
    isNew: false,
    views: 234,
    daysOnMarket: 3,
  },
  {
    id: 'rel-2',
    title: 'Audi A6 Premium Plus',
    price: 58000,
    image: 'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=400&h=300&fit=crop',
    location: 'Caguas',
    isNew: true,
    views: 189,
    daysOnMarket: 1,
  },
  {
    id: 'rel-3',
    title: 'Lexus ES 350 F Sport',
    price: 52000,
    image: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400&h=300&fit=crop',
    location: 'Ponce',
    views: 145,
    daysOnMarket: 7,
  },
  {
    id: 'rel-4',
    title: 'Volvo S60 T6 Inscription',
    price: 48000,
    image: 'https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?w=400&h=300&fit=crop',
    location: 'Mayagüez',
    views: 98,
    daysOnMarket: 14,
  },
];

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(price);
};

export const OklaDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const [isFavorite, setIsFavorite] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [activeTab, setActiveTab] = useState<'specs' | 'description'>('specs');

  // In real app, fetch listing by id
  const listing = mockListing;

  return (
    <OklaLayout>
      {/* Breadcrumb */}
      <div className="bg-okla-cream/50 py-4">
        <div className="container mx-auto px-4">
          <div className="flex items-center gap-2 text-sm">
            <Link
              to="/okla/browse"
              className="flex items-center gap-1 text-okla-slate hover:text-okla-navy transition-colors"
            >
              <ChevronLeft className="w-4 h-4" />
              Volver a resultados
            </Link>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="container mx-auto px-4 py-8">
        <div className="grid lg:grid-cols-3 gap-8">
          {/* Left Column - Gallery & Details */}
          <div className="lg:col-span-2 space-y-8">
            {/* Gallery */}
            <FadeIn>
              <OklaImageGallery images={listing.images} title={listing.title} />
            </FadeIn>

            {/* Title & Price (Mobile) */}
            <div className="lg:hidden">
              <FadeIn delay={0.1}>
                <div className="flex flex-wrap gap-2 mb-3">
                  {listing.isVerified && (
                    <OklaBadge variant="outline">
                      <Shield className="w-3 h-3 mr-1" />
                      Verificado
                    </OklaBadge>
                  )}
                  {listing.isFeatured && (
                    <OklaBadge variant="solid">Destacado</OklaBadge>
                  )}
                  {listing.condition && (
                    <OklaBadge variant="subtle">{listing.condition}</OklaBadge>
                  )}
                </div>

                <h1 className="text-2xl font-display font-bold text-okla-navy mb-2">
                  {listing.title}
                </h1>
                {listing.subtitle && (
                  <p className="text-okla-slate mb-4">{listing.subtitle}</p>
                )}

                <div className="flex items-baseline gap-3 mb-4">
                  <span className="text-3xl font-display font-bold text-okla-navy">
                    {formatPrice(listing.price)}
                  </span>
                  {listing.originalPrice && listing.originalPrice > listing.price && (
                    <span className="text-lg text-okla-slate line-through">
                      {formatPrice(listing.originalPrice)}
                    </span>
                  )}
                </div>

                <div className="flex flex-wrap gap-4 text-sm text-okla-slate mb-6">
                  <span className="flex items-center gap-1">
                    <MapPin className="w-4 h-4 text-okla-gold" />
                    {listing.location}
                  </span>
                  <span className="flex items-center gap-1">
                    <Eye className="w-4 h-4" />
                    {listing.views} vistas
                  </span>
                  <span className="flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    {listing.daysOnMarket} días
                  </span>
                </div>

                <OklaListingActions
                  listingId={listing.id}
                  isFavorite={isFavorite}
                  isSaved={isSaved}
                  onFavorite={() => setIsFavorite(!isFavorite)}
                  onSave={() => setIsSaved(!isSaved)}
                  onPrint={() => window.print()}
                  onReport={() => console.log('Report')}
                />
              </FadeIn>
            </div>

            {/* Tabs */}
            <FadeIn delay={0.2}>
              <div className="border-b border-okla-cream">
                <div className="flex gap-8">
                  {[
                    { id: 'specs', label: 'Especificaciones' },
                    { id: 'description', label: 'Descripción' },
                  ].map((tab) => (
                    <button
                      key={tab.id}
                      onClick={() => setActiveTab(tab.id as 'specs' | 'description')}
                      className={`pb-4 font-semibold transition-colors relative ${
                        activeTab === tab.id
                          ? 'text-okla-navy'
                          : 'text-okla-slate hover:text-okla-navy'
                      }`}
                    >
                      {tab.label}
                      {activeTab === tab.id && (
                        <motion.div
                          className="absolute bottom-0 left-0 right-0 h-0.5 bg-okla-gold"
                          layoutId="activeTab"
                        />
                      )}
                    </button>
                  ))}
                </div>
              </div>

              {/* Tab Content */}
              <div className="pt-6">
                {activeTab === 'specs' && (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                  >
                    <OklaSpecsTable
                      specs={listing.specs}
                      features={listing.features}
                      variant="list"
                    />
                  </motion.div>
                )}

                {activeTab === 'description' && (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="prose prose-slate max-w-none"
                  >
                    {listing.description.split('\n\n').map((paragraph, index) => (
                      <p key={index} className="text-okla-charcoal leading-relaxed mb-4">
                        {paragraph}
                      </p>
                    ))}
                  </motion.div>
                )}
              </div>
            </FadeIn>

            {/* Seller Info (Mobile) */}
            <div className="lg:hidden">
              <FadeIn delay={0.3}>
                <OklaSellerInfo
                  seller={listing.seller}
                  onContact={() => console.log('Contact')}
                  onCall={() => console.log('Call')}
                  onMessage={() => console.log('Message')}
                  onViewProfile={() => console.log('View profile')}
                />
              </FadeIn>
            </div>

            {/* Contact Form (Mobile) */}
            <div className="lg:hidden">
              <FadeIn delay={0.4}>
                <OklaContactForm
                  listingTitle={listing.title}
                  sellerName={listing.seller.name}
                  onSubmit={(data) => console.log('Submit:', data)}
                />
              </FadeIn>
            </div>
          </div>

          {/* Right Column - Sticky Sidebar */}
          <div className="hidden lg:block">
            <div className="sticky top-24 space-y-6">
              {/* Title & Price Card */}
              <FadeIn delay={0.1}>
                <div className="bg-white rounded-2xl border border-okla-cream p-6 shadow-sm">
                  {/* Badges */}
                  <div className="flex flex-wrap gap-2 mb-4">
                    {listing.isVerified && (
                      <OklaBadge variant="outline">
                        <Shield className="w-3 h-3 mr-1" />
                        Verificado
                      </OklaBadge>
                    )}
                    {listing.isFeatured && (
                      <OklaBadge variant="solid">Destacado</OklaBadge>
                    )}
                    {listing.condition && (
                      <OklaBadge variant="subtle">{listing.condition}</OklaBadge>
                    )}
                  </div>

                  {/* Title */}
                  <h1 className="text-xl font-display font-bold text-okla-navy mb-2">
                    {listing.title}
                  </h1>
                  {listing.subtitle && (
                    <p className="text-sm text-okla-slate mb-4">{listing.subtitle}</p>
                  )}

                  {/* Price */}
                  <div className="flex items-baseline gap-3 mb-4">
                    <span className="text-3xl font-display font-bold text-okla-navy">
                      {formatPrice(listing.price)}
                    </span>
                    {listing.originalPrice && listing.originalPrice > listing.price && (
                      <span className="text-lg text-okla-slate line-through">
                        {formatPrice(listing.originalPrice)}
                      </span>
                    )}
                  </div>

                  {/* Meta */}
                  <div className="flex flex-wrap gap-4 text-sm text-okla-slate mb-6">
                    <span className="flex items-center gap-1">
                      <MapPin className="w-4 h-4 text-okla-gold" />
                      {listing.location}
                    </span>
                    <span className="flex items-center gap-1">
                      <Eye className="w-4 h-4" />
                      {listing.views} vistas
                    </span>
                    <span className="flex items-center gap-1">
                      <Clock className="w-4 h-4" />
                      {listing.daysOnMarket} días
                    </span>
                  </div>

                  {/* Quick Specs */}
                  <div className="grid grid-cols-2 gap-3 mb-6">
                    {listing.specs.slice(0, 4).map((spec, index) => (
                      <div
                        key={index}
                        className="bg-okla-cream/50 rounded-lg p-3 text-center"
                      >
                        <p className="text-xs text-okla-slate">{spec.label}</p>
                        <p className="font-semibold text-okla-navy">{spec.value}</p>
                      </div>
                    ))}
                  </div>

                  {/* Actions */}
                  <OklaListingActions
                    listingId={listing.id}
                    isFavorite={isFavorite}
                    isSaved={isSaved}
                    onFavorite={() => setIsFavorite(!isFavorite)}
                    onSave={() => setIsSaved(!isSaved)}
                    onPrint={() => window.print()}
                    onReport={() => console.log('Report')}
                    variant="icons-only"
                  />
                </div>
              </FadeIn>

              {/* Seller Info */}
              <FadeIn delay={0.2}>
                <OklaSellerInfo
                  seller={listing.seller}
                  onContact={() => console.log('Contact')}
                  onCall={() => console.log('Call')}
                  onMessage={() => console.log('Message')}
                  onViewProfile={() => console.log('View profile')}
                />
              </FadeIn>

              {/* Contact Form */}
              <FadeIn delay={0.3}>
                <OklaContactForm
                  listingTitle={listing.title}
                  sellerName={listing.seller.name}
                  onSubmit={(data) => console.log('Submit:', data)}
                />
              </FadeIn>
            </div>
          </div>
        </div>

        {/* Related Listings */}
        <FadeIn delay={0.4}>
          <OklaRelatedListings
            listings={mockRelatedListings}
            title="Vehículos similares"
            subtitle="Basado en tu búsqueda"
            onListingClick={(id) => console.log('Navigate to:', id)}
            onFavorite={(id) => console.log('Favorite:', id)}
          />
        </FadeIn>
      </div>
    </OklaLayout>
  );
};

export default OklaDetailPage;
