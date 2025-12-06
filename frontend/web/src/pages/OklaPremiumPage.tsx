import { useNavigate } from 'react-router-dom';
import { 
  Car, 
  Home, 
  Briefcase, 
  Smartphone, 
  Search, 
  UserCheck, 
  Shield, 
  Headphones 
} from 'lucide-react';
import { OklaLayout } from '../layouts/OklaLayout';
import {
  OklaHero,
  OklaFeaturedGrid,
  OklaStats,
  OklaTestimonials,
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

// Mock data for featured listings
const featuredListings = [
  {
    id: '1',
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
    id: '2',
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
    id: '3',
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
    id: '4',
    title: 'Villa Contemporánea con Piscina',
    price: 875000,
    image: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Houston, TX',
    rating: 4.7,
    reviews: 35,
    isNew: true,
    views: 650,
  },
  {
    id: '5',
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
    id: '6',
    title: 'Apartamento Moderno Centro',
    price: 450000,
    image: 'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800&h=600&fit=crop',
    category: 'Propiedades',
    location: 'Chicago, IL',
    rating: 4.6,
    reviews: 41,
    views: 780,
  },
];

// Categories data
const categories = [
  {
    id: 'vehicles',
    name: 'Vehículos',
    icon: <Car className="w-8 h-8" />,
    count: 12500,
    image: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=400&h=400&fit=crop',
  },
  {
    id: 'properties',
    name: 'Propiedades',
    icon: <Home className="w-8 h-8" />,
    count: 8750,
    image: 'https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=400&h=400&fit=crop',
  },
  {
    id: 'services',
    name: 'Servicios',
    icon: <Briefcase className="w-8 h-8" />,
    count: 3200,
    image: 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=400&h=400&fit=crop',
  },
  {
    id: 'electronics',
    name: 'Electrónicos',
    icon: <Smartphone className="w-8 h-8" />,
    count: 5600,
    image: 'https://images.unsplash.com/photo-1468495244123-6c6c332eeece?w=400&h=400&fit=crop',
  },
];

// Stats data
const stats = [
  { value: 50000, label: 'Anuncios Activos', suffix: '+' },
  { value: 25000, label: 'Clientes Felices', suffix: '+' },
  { value: 98, label: 'Satisfacción', suffix: '%' },
  { value: 15, label: 'Países', suffix: '+' },
];

// Testimonials data
const testimonials = [
  {
    id: '1',
    name: 'Carlos Mendoza',
    role: 'Empresario',
    avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=face',
    content: 'Okla transformó la forma en que vendo mis vehículos. La plataforma es elegante, profesional y mis ventas aumentaron un 40% desde que empecé a usarla.',
    rating: 5,
  },
  {
    id: '2',
    name: 'María González',
    role: 'Agente Inmobiliaria',
    avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&h=150&fit=crop&crop=face',
    content: 'La mejor plataforma para profesionales del sector inmobiliario. Mis clientes quedan impresionados con la presentación de las propiedades.',
    rating: 5,
  },
  {
    id: '3',
    name: 'Roberto Silva',
    role: 'Coleccionista',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&h=150&fit=crop&crop=face',
    content: 'Encontré piezas únicas que no pude encontrar en ningún otro lugar. El nivel de detalle y la seguridad en las transacciones es excepcional.',
    rating: 5,
  },
  {
    id: '4',
    name: 'Ana Martínez',
    role: 'Inversora',
    avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&h=150&fit=crop&crop=face',
    content: 'La transparencia y profesionalismo de Okla me dan la confianza para hacer inversiones importantes. Altamente recomendado.',
    rating: 5,
  },
];

// How it works steps
const howItWorksSteps = [
  {
    icon: <Search className="w-8 h-8" />,
    title: 'Busca',
    description: 'Explora miles de anuncios verificados con filtros avanzados para encontrar exactamente lo que necesitas.',
  },
  {
    icon: <UserCheck className="w-8 h-8" />,
    title: 'Conecta',
    description: 'Contacta directamente con vendedores verificados y resuelve todas tus dudas antes de decidir.',
  },
  {
    icon: <Shield className="w-8 h-8" />,
    title: 'Transacciona',
    description: 'Realiza transacciones seguras con nuestra protección al comprador y garantía de satisfacción.',
  },
];

export const OklaPremiumPage = () => {
  const navigate = useNavigate();

  const handleSearch = (query: string, category: string) => {
    navigate(`/marketplace/browse?q=${encodeURIComponent(query)}&category=${category}`);
  };

  const handleListingClick = (id: string) => {
    navigate(`/marketplace/vehicles/${id}`);
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

      {/* Featured Listings */}
      <OklaFeaturedGrid
        title="Destacados de la Semana"
        subtitle="Selección exclusiva de nuestros mejores anuncios"
        listings={featuredListings}
        onViewAll={() => navigate('/marketplace/browse')}
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
      <OklaStats
        title="Números que Hablan"
        subtitle="La confianza de miles de usuarios nos respalda"
        stats={stats}
        darkMode={true}
      />

      {/* Testimonials */}
      <OklaTestimonials
        title="Lo que Dicen Nuestros Clientes"
        subtitle="Historias de éxito que nos inspiran a mejorar cada día"
        testimonials={testimonials}
      />

      {/* CTA Section */}
      <OklaCta
        title="¿Listo para Comenzar?"
        subtitle="Únete a miles de usuarios que ya confían en Okla para sus transacciones"
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
