import { useNavigate } from 'react-router-dom';
import { 
  Car,
  KeyRound,
  Building2,
  BedDouble, 
  Search, 
  UserCheck, 
  Shield, 
  Headphones,
  Target,
  Zap,
  Camera,
  MessageCircle,
} from 'lucide-react';
import { OklaLayout } from '../layouts/OklaLayout';
import {
  OklaHero,
  OklaFeaturedGrid,
  OklaCta,
  OklaCategories,
  OklaHowItWorks,
} from '../components/okla/sections';

/**
 * OKLA Premium Landing Page
 * 
 * Página de inicio con diseño elegante que transmite
 * profesionalidad, confianza y sofisticación.
 */

// Mock data - Vehículos en Venta
const vehiculosListings = [
  {
    id: 'v1',
    title: 'Mercedes-Benz Clase C AMG 2024',
    price: 75000,
    image: 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 47,
    featured: true,
    views: 1250,
  },
  {
    id: 'v2',
    title: 'BMW Serie 7 Executive Package',
    price: 95000,
    image: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 62,
    views: 2100,
  },
  {
    id: 'v3',
    title: 'Porsche 911 Carrera S',
    price: 135000,
    image: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'New York, NY',
    rating: 4.9,
    reviews: 89,
    featured: true,
    views: 3200,
  },
  {
    id: 'v4',
    title: 'Audi RS7 Sportback 2024',
    price: 128000,
    image: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Dallas, TX',
    rating: 4.7,
    reviews: 31,
    isNew: true,
    views: 980,
  },
  {
    id: 'v5',
    title: 'Tesla Model S Plaid',
    price: 108000,
    image: 'https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'San Francisco, CA',
    rating: 4.8,
    reviews: 56,
    views: 1890,
  },
  {
    id: 'v6',
    title: 'Range Rover Sport HSE',
    price: 89000,
    image: 'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800&h=600&fit=crop',
    category: 'Vehículos',
    location: 'Phoenix, AZ',
    rating: 4.6,
    reviews: 28,
    views: 720,
  },
];

// Mock data - Renta de Vehículos
const rentaVehiculosListings = [
  {
    id: 'rv1',
    title: 'BMW X5 - Renta por Día',
    price: 150,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Miami, FL',
    rating: 4.9,
    reviews: 124,
    featured: true,
    views: 2340,
  },
  {
    id: 'rv2',
    title: 'Mercedes GLE Coupe - Renta Semanal',
    price: 850,
    priceLabel: '/semana',
    image: 'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Los Angeles, CA',
    rating: 4.8,
    reviews: 89,
    views: 1560,
  },
  {
    id: 'rv3',
    title: 'Porsche Cayenne - Renta Premium',
    price: 200,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Las Vegas, NV',
    rating: 5.0,
    reviews: 67,
    featured: true,
    isNew: true,
    views: 1890,
  },
  {
    id: 'rv4',
    title: 'Cadillac Escalade - Eventos Especiales',
    price: 280,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Atlanta, GA',
    rating: 4.7,
    reviews: 45,
    views: 890,
  },
  {
    id: 'rv5',
    title: 'Tesla Model X - Renta Ecológica',
    price: 175,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'San Diego, CA',
    rating: 4.9,
    reviews: 98,
    views: 2100,
  },
  {
    id: 'rv6',
    title: 'Range Rover Velar - Renta de Lujo',
    price: 190,
    priceLabel: '/día',
    image: 'https://images.unsplash.com/photo-1551830820-330a71b99659?w=800&h=600&fit=crop',
    category: 'Renta de Vehículos',
    location: 'Orlando, FL',
    rating: 4.6,
    reviews: 52,
    views: 760,
  },
];

// Mock data - Propiedades en Venta
const propiedadesListings = [
  {
    id: 'p1',
    title: 'Penthouse de Lujo con Vista al Mar',
    price: 1250000,
    image: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Cancún, MX',
    rating: 5.0,
    reviews: 23,
    featured: true,
    isNew: true,
    views: 890,
  },
  {
    id: 'p2',
    title: 'Villa Contemporánea con Piscina',
    price: 875000,
    image: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Houston, TX',
    rating: 4.7,
    reviews: 35,
    views: 650,
  },
  {
    id: 'p3',
    title: 'Apartamento Moderno Centro',
    price: 450000,
    image: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Chicago, IL',
    rating: 4.6,
    reviews: 41,
    views: 780,
  },
  {
    id: 'p4',
    title: 'Casa Colonial con Jardín Amplio',
    price: 680000,
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'San Antonio, TX',
    rating: 4.8,
    reviews: 29,
    featured: true,
    views: 540,
  },
  {
    id: 'p5',
    title: 'Loft Industrial Renovado',
    price: 395000,
    image: 'https://images.unsplash.com/photo-1600607688969-a5bfcd646154?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Brooklyn, NY',
    rating: 4.5,
    reviews: 18,
    isNew: true,
    views: 420,
  },
  {
    id: 'p6',
    title: 'Mansión con Vista Panorámica',
    price: 2150000,
    image: 'https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Beverly Hills, CA',
    rating: 4.9,
    reviews: 12,
    views: 1200,
  },
];

// Mock data - Hospedaje
const hospedajeListings = [
  {
    id: 'h1',
    title: 'Suite Premium Frente al Mar',
    price: 250,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Playa del Carmen, MX',
    rating: 4.9,
    reviews: 156,
    featured: true,
    views: 3400,
  },
  {
    id: 'h2',
    title: 'Apartamento Ejecutivo Centro',
    price: 120,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Miami Beach, FL',
    rating: 4.7,
    reviews: 89,
    views: 1890,
  },
  {
    id: 'h3',
    title: 'Villa Privada con Alberca',
    price: 380,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1602002418082-a4443e081dd1?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Tulum, MX',
    rating: 5.0,
    reviews: 67,
    featured: true,
    isNew: true,
    views: 2100,
  },
  {
    id: 'h4',
    title: 'Cabaña Rústica en la Montaña',
    price: 95,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Aspen, CO',
    rating: 4.8,
    reviews: 134,
    views: 2340,
  },
  {
    id: 'h5',
    title: 'Penthouse con Terraza Privada',
    price: 320,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'New York, NY',
    rating: 4.6,
    reviews: 78,
    views: 1560,
  },
  {
    id: 'h6',
    title: 'Casa de Playa con Jacuzzi',
    price: 275,
    priceLabel: '/noche',
    image: 'https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=800&h=600&fit=crop',
    category: 'Hospedaje',
    location: 'Malibu, CA',
    rating: 4.9,
    reviews: 92,
    views: 1890,
  },
];

// Categories data
const categories = [
  {
    id: 'vehicles',
    name: 'Vehículos',
    icon: <Car className="w-8 h-8" />,
    count: 0,
    image: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=400&h=400&fit=crop',
  },
  {
    id: 'vehicle-rental',
    name: 'Renta de Vehículos',
    icon: <KeyRound className="w-8 h-8" />,
    count: 0,
    image: 'https://images.unsplash.com/photo-1449965408869-eaa3f722e40d?w=400&h=400&fit=crop',
  },
  {
    id: 'properties',
    name: 'Propiedades',
    icon: <Building2 className="w-8 h-8" />,
    count: 0,
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=400&h=400&fit=crop',
  },
  {
    id: 'lodging',
    name: 'Hospedaje',
    icon: <BedDouble className="w-8 h-8" />,
    count: 0,
    image: 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400&h=400&fit=crop',
  },
];

// Beneficios para compradores y vendedores
const marketplaceBenefits = [
  {
    id: '1',
    title: 'Encuentra lo que Buscas',
    icon: Target,
    description: 'Filtros avanzados y búsqueda inteligente para encontrar exactamente el producto o servicio que necesitas.',
  },
  {
    id: '2',
    title: 'Vende Más Rápido',
    icon: Zap,
    description: 'Publica tus anuncios en minutos y llega a compradores interesados que buscan lo que ofreces.',
  },
  {
    id: '3',
    title: 'Fotos que Venden',
    icon: Camera,
    description: 'Sube múltiples imágenes en alta calidad para mostrar cada detalle de tus productos.',
  },
  {
    id: '4',
    title: 'Contacto Directo',
    icon: MessageCircle,
    description: 'Comunícate directamente con compradores o vendedores sin intermediarios.',
  },
];

// How it works steps
const howItWorksSteps = [
  {
    icon: <Search className="w-8 h-8" />,
    title: 'Busca',
    description: 'Explora anuncios con filtros avanzados para encontrar exactamente lo que necesitas.',
  },
  {
    icon: <UserCheck className="w-8 h-8" />,
    title: 'Conecta',
    description: 'Contacta directamente con los anunciantes y resuelve todas tus dudas antes de decidir.',
  },
  {
    icon: <Shield className="w-8 h-8" />,
    title: 'Transacciona',
    description: 'Realiza tus operaciones en nuestra plataforma segura con encriptación SSL.',
  },
];

export const OklaPremiumPage = () => {
  const navigate = useNavigate();

  const handleSearch = (query: string, category: string) => {
    navigate(`/marketplace/browse?q=${encodeURIComponent(query)}&category=${category}`);
  };

  const handleListingClick = (id: string) => {
    navigate(`/vehicles/${id}`);
  };

  const handleCategoryClick = (id: string) => {
    navigate(`/marketplace/browse?category=${id}`);
  };

  return (
    <OklaLayout>
      {/* Hero Section */}
      <OklaHero
        title="Descubre lo Extraordinario"
        subtitle="El marketplace premium donde la calidad se encuentra con la confianza"
        backgroundImage="https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?q=80&w=2070"
        showSearch={true}
        showTrustBadges={true}
        onSearch={handleSearch}
      />

      {/* Categories Section */}
      <OklaCategories
        title="Explora por Categoría"
        subtitle="Encuentra exactamente lo que buscas en nuestras categorías curadas"
        categories={categories}
        onCategoryClick={handleCategoryClick}
        variant="tiles"
      />

      {/* Featured Listings - General */}
      <OklaFeaturedGrid
        title="Destacados de la Semana"
        subtitle="Selección exclusiva de nuestros mejores anuncios"
        listings={[...vehiculosListings.slice(0, 2), ...propiedadesListings.slice(0, 2), ...rentaVehiculosListings.slice(0, 1), ...hospedajeListings.slice(0, 1)]}
        onViewAll={() => navigate('/marketplace/browse')}
        onListingClick={handleListingClick}
      />

      {/* Vehículos Destacados */}
      <OklaFeaturedGrid
        title="Vehículos Destacados"
        subtitle="Los mejores vehículos del mercado"
        listings={vehiculosListings}
        onViewAll={() => navigate('/marketplace/browse?category=vehicles')}
        onListingClick={handleListingClick}
      />

      {/* Renta de Vehículos */}
      <OklaFeaturedGrid
        title="Renta de Vehículos"
        subtitle="Alquila el vehículo perfecto para cualquier ocasión"
        listings={rentaVehiculosListings}
        onViewAll={() => navigate('/marketplace/browse?category=vehicle-rental')}
        onListingClick={handleListingClick}
      />

      {/* Propiedades Destacadas */}
      <OklaFeaturedGrid
        title="Propiedades Destacadas"
        subtitle="Encuentra tu próximo hogar o inversión inmobiliaria"
        listings={propiedadesListings}
        onViewAll={() => navigate('/marketplace/browse?category=properties')}
        onListingClick={handleListingClick}
      />

      {/* Hospedaje Destacado */}
      <OklaFeaturedGrid
        title="Hospedaje Destacado"
        subtitle="Apartamentos, hoteles y alojamientos para cada viaje"
        listings={hospedajeListings}
        onViewAll={() => navigate('/marketplace/browse?category=lodging')}
        onListingClick={handleListingClick}
      />

      {/* How It Works */}
      <OklaHowItWorks
        title="Cómo Funciona"
        subtitle="Tres simples pasos para encontrar lo que buscas"
        features={howItWorksSteps}
        variant="steps"
      />

      {/* Stats Section */}
      {/* Benefits Section */}
      <section className="py-12 bg-okla-cream">
        <div className="container mx-auto px-4">
          <div className="text-center mb-8">
            <h2 className="text-3xl md:text-4xl font-display font-bold text-okla-navy mb-4">
              Todo lo que Necesitas
            </h2>
            <p className="text-lg text-okla-slate max-w-2xl mx-auto">
              Compra y vende de manera fácil, rápida y segura
            </p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {marketplaceBenefits.map((benefit) => (
              <div key={benefit.id} className="bg-white p-6 rounded-2xl shadow-sm hover:shadow-lg transition-shadow">
                <div className="w-14 h-14 bg-gradient-to-br from-gold-100 to-gold-200 rounded-2xl flex items-center justify-center mb-4">
                  <benefit.icon className="w-7 h-7 text-gold-600" />
                </div>
                <h3 className="text-xl font-semibold text-okla-navy mb-2">{benefit.title}</h3>
                <p className="text-okla-slate">{benefit.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <OklaCta
        title="¿Listo para Comenzar?"
        subtitle="Descubre una plataforma segura y profesional para tus operaciones"
        primaryAction={{
          label: 'Publicar Anuncio',
          onClick: () => navigate('/marketplace/listings/new'),
        }}
        secondaryAction={{
          label: 'Explorar Marketplace',
          onClick: () => navigate('/marketplace/browse'),
        }}
        variant="gradient"
      />

      {/* Support Section */}
      <section className="py-16 px-6 bg-okla-cream">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-okla-gold/20 rounded-2xl mb-6">
            <Headphones className="w-8 h-8 text-okla-gold" />
          </div>
          <h2 className="text-3xl font-playfair font-bold text-okla-navy mb-4">
            Soporte Premium 24/7
          </h2>
          <p className="text-okla-slate mb-8 max-w-xl mx-auto">
            Nuestro equipo de expertos está disponible las 24 horas para ayudarte con cualquier consulta o necesidad.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <a
              href="mailto:soporte@okla.com"
              className="px-6 py-3 bg-okla-navy text-white rounded-xl font-medium hover:bg-okla-charcoal transition-colors"
            >
              Contactar Soporte
            </a>
            <a
              href="/help"
              className="px-6 py-3 border border-okla-navy text-okla-navy rounded-xl font-medium hover:bg-okla-navy hover:text-white transition-colors"
            >
              Centro de Ayuda
            </a>
          </div>
        </div>
      </section>
    </OklaLayout>
  );
};

export default OklaPremiumPage;
