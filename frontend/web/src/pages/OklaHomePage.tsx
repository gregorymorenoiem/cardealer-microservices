import React from 'react';
import { Link } from 'react-router-dom';
import { clsx } from 'clsx';
import { motion } from 'framer-motion';
import {
  ArrowRight,
  Search,
  Shield,
  Star,
  TrendingUp,
  Users,
  CheckCircle2,
  Play,
  ChevronRight,
} from 'lucide-react';
import { OklaButton } from '../components/atoms/okla/OklaButton';
import { OklaProductCard } from '../components/atoms/okla/OklaCard';
import { OklaTrustBadge, OklaRatingBadge } from '../components/atoms/okla/OklaBadge';
import { OklaLayout } from '../layouts/OklaLayout';

/**
 * OKLA Home Page
 * 
 * Página de inicio premium con diseño elegante que transmite
 * profesionalidad, confianza y sofisticación.
 */

// Animation variants
const fadeInUp = {
  initial: { opacity: 0, y: 30 },
  animate: { opacity: 1, y: 0 },
  transition: { duration: 0.6, ease: [0.4, 0, 0.2, 1] }
};

const staggerContainer = {
  animate: {
    transition: {
      staggerChildren: 0.1
    }
  }
};

// Mock data for featured products
const featuredProducts = [
  {
    id: '1',
    image: 'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=600&h=400&fit=crop',
    title: 'Mercedes-Benz Clase C 2024',
    price: 45000,
    rating: 4.8,
    reviews: 124,
    badge: 'Premium',
    badgeColor: 'gold' as const,
  },
  {
    id: '2',
    image: 'https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=600&h=400&fit=crop',
    title: 'Villa Moderna con Vista al Mar',
    price: 850000,
    rating: 4.9,
    reviews: 45,
    badge: 'Destacado',
    badgeColor: 'gold' as const,
  },
  {
    id: '3',
    image: 'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=600&h=400&fit=crop',
    title: 'BMW Serie 5 Executive',
    price: 52000,
    rating: 4.7,
    reviews: 89,
    badge: 'Nuevo',
    badgeColor: 'green' as const,
  },
  {
    id: '4',
    image: 'https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600&h=400&fit=crop',
    title: 'Penthouse de Lujo Centro',
    price: 1200000,
    rating: 5.0,
    reviews: 28,
    badge: 'Exclusivo',
    badgeColor: 'gold' as const,
  },
];

const categories = [
  {
    id: 'vehicles',
    name: 'Vehículos',
    count: '2,450+',
    icon: '🚗',
    image: 'https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=400&h=300&fit=crop',
  },
  {
    id: 'properties',
    name: 'Propiedades',
    count: '1,230+',
    icon: '🏠',
    image: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=400&h=300&fit=crop',
  },
  {
    id: 'electronics',
    name: 'Electrónica',
    count: '3,100+',
    icon: '📱',
    image: 'https://images.unsplash.com/photo-1468495244123-6c6c332eeece?w=400&h=300&fit=crop',
  },
  {
    id: 'fashion',
    name: 'Moda',
    count: '5,670+',
    icon: '👔',
    image: 'https://images.unsplash.com/photo-1445205170230-053b83016050?w=400&h=300&fit=crop',
  },
];

const benefits = [
  {
    icon: Shield,
    title: 'Transacciones Seguras',
    description: 'Pagos protegidos y verificación de vendedores para tu tranquilidad.',
  },
  {
    icon: Star,
    title: 'Calidad Premium',
    description: 'Productos verificados y vendedores con reputación comprobada.',
  },
  {
    icon: TrendingUp,
    title: 'Mejores Precios',
    description: 'Compara y encuentra las mejores ofertas del mercado.',
  },
  {
    icon: Users,
    title: 'Comunidad Confiable',
    description: 'Miles de compradores y vendedores satisfechos nos respaldan.',
  },
];

const testimonials = [
  {
    id: 1,
    name: 'Carlos Mendoza',
    role: 'Comprador Frecuente',
    avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop',
    content: 'OKLA transformó mi experiencia de compra. La calidad de los productos y la seguridad de las transacciones son incomparables.',
    rating: 5,
  },
  {
    id: 2,
    name: 'María García',
    role: 'Vendedora Verificada',
    avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&h=100&fit=crop',
    content: 'Como vendedora, OKLA me ha dado las herramientas para alcanzar más clientes y crecer mi negocio de manera profesional.',
    rating: 5,
  },
  {
    id: 3,
    name: 'Roberto Silva',
    role: 'Inversor Inmobiliario',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=100&h=100&fit=crop',
    content: 'Encontré la propiedad perfecta gracias a los filtros avanzados y la calidad de los listados. 100% recomendado.',
    rating: 5,
  },
];

const stats = [
  { value: '50K+', label: 'Usuarios Activos' },
  { value: '100K+', label: 'Productos' },
  { value: '98%', label: 'Satisfacción' },
  { value: '24/7', label: 'Soporte' },
];

export const OklaHomePage: React.FC = () => {
  return (
    <OklaLayout>
      {/* Hero Section */}
      <section className="relative min-h-[90vh] flex items-center overflow-hidden">
        {/* Background */}
        <div className="absolute inset-0 z-0">
          <div 
            className={clsx(
              'absolute inset-0',
              'bg-gradient-to-br from-gray-50 via-white to-gold-50',
              'dark:from-gray-950 dark:via-gray-900 dark:to-gray-950'
            )}
          />
          {/* Decorative elements */}
          <div className="absolute top-20 right-[10%] w-72 h-72 bg-gold-200/30 dark:bg-gold-500/10 rounded-full blur-3xl" />
          <div className="absolute bottom-20 left-[5%] w-96 h-96 bg-gold-100/40 dark:bg-gold-500/5 rounded-full blur-3xl" />
        </div>

        <div className="container mx-auto px-4 lg:px-8 relative z-10">
          <div className="grid lg:grid-cols-2 gap-12 lg:gap-20 items-center">
            {/* Left Content */}
            <motion.div
              initial={{ opacity: 0, x: -30 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.8, ease: [0.4, 0, 0.2, 1] }}
            >
              {/* Badge */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gold-100 dark:bg-gold-900/30 text-gold-700 dark:text-gold-400 text-sm font-medium mb-6"
              >
                <Star className="w-4 h-4 fill-current" />
                El marketplace #1 en elegancia
              </motion.div>

              {/* Headline */}
              <h1 className="font-display text-4xl md:text-5xl lg:text-6xl xl:text-7xl font-bold text-primary-500 dark:text-white leading-tight mb-6">
                Descubre lo
                <span className="block text-transparent bg-clip-text bg-gradient-to-r from-gold-500 via-gold-400 to-gold-600">
                  Extraordinario
                </span>
              </h1>

              <p className="text-lg md:text-xl text-gray-600 dark:text-gray-300 mb-8 max-w-xl leading-relaxed">
                El marketplace premium donde la calidad se encuentra con la confianza. 
                Compra y vende con la elegancia que mereces.
              </p>

              {/* Search Bar */}
              <div className="relative max-w-lg mb-8">
                <input
                  type="text"
                  placeholder="¿Qué estás buscando?"
                  className={clsx(
                    'w-full h-14 pl-14 pr-36',
                    'bg-white dark:bg-gray-900',
                    'border border-gray-200 dark:border-gray-700',
                    'rounded-2xl',
                    'text-gray-700 dark:text-white',
                    'placeholder:text-gray-400',
                    'focus:outline-none focus:ring-2 focus:ring-gold-500/30 focus:border-gold-500',
                    'shadow-elegant',
                    'transition-all duration-200'
                  )}
                />
                <Search className="absolute left-5 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <OklaButton
                  variant="gold"
                  size="lg"
                  className="absolute right-2 top-1/2 -translate-y-1/2"
                  rightIcon={<ArrowRight className="w-4 h-4" />}
                >
                  Buscar
                </OklaButton>
              </div>

              {/* Trust Indicators */}
              <div className="flex flex-wrap items-center gap-6">
                <OklaTrustBadge type="secure" />
                <OklaTrustBadge type="verified-seller" />
                <OklaTrustBadge type="guarantee" />
              </div>
            </motion.div>

            {/* Right Content - Featured Image/Cards */}
            <motion.div
              initial={{ opacity: 0, x: 30 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.8, delay: 0.2, ease: [0.4, 0, 0.2, 1] }}
              className="relative hidden lg:block"
            >
              {/* Main Image */}
              <div className="relative">
                <div className="relative z-10 rounded-3xl overflow-hidden shadow-2xl">
                  <img
                    src="https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?w=800&h=600&fit=crop"
                    alt="Luxury Living"
                    className="w-full h-auto"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent" />
                </div>
                
                {/* Floating Card 1 */}
                <motion.div
                  animate={{ y: [0, -10, 0] }}
                  transition={{ duration: 4, repeat: Infinity, ease: "easeInOut" }}
                  className={clsx(
                    'absolute -top-6 -left-6 z-20',
                    'bg-white dark:bg-gray-900',
                    'rounded-2xl shadow-elegant-xl',
                    'p-4'
                  )}
                >
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-xl bg-gold-100 dark:bg-gold-900/30 flex items-center justify-center">
                      <TrendingUp className="w-6 h-6 text-gold-600" />
                    </div>
                    <div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">Ventas hoy</p>
                      <p className="text-lg font-bold text-gray-900 dark:text-white">+2,450</p>
                    </div>
                  </div>
                </motion.div>

                {/* Floating Card 2 */}
                <motion.div
                  animate={{ y: [0, 10, 0] }}
                  transition={{ duration: 5, repeat: Infinity, ease: "easeInOut", delay: 0.5 }}
                  className={clsx(
                    'absolute -bottom-8 -right-8 z-20',
                    'bg-white dark:bg-gray-900',
                    'rounded-2xl shadow-elegant-xl',
                    'p-4'
                  )}
                >
                  <div className="flex items-center gap-3">
                    <div className="flex -space-x-2">
                      {[1, 2, 3].map((i) => (
                        <img
                          key={i}
                          src={`https://images.unsplash.com/photo-150700321116${i}-0a1dd7228f2d?w=40&h=40&fit=crop`}
                          alt=""
                          className="w-8 h-8 rounded-full border-2 border-white dark:border-gray-900"
                        />
                      ))}
                    </div>
                    <div>
                      <p className="text-sm font-semibold text-gray-900 dark:text-white">+50K usuarios</p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">nos confían</p>
                    </div>
                  </div>
                </motion.div>

                {/* Decorative rings */}
                <div className="absolute -z-10 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[120%] h-[120%] border border-gold-200/50 dark:border-gold-700/30 rounded-full" />
                <div className="absolute -z-10 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[140%] h-[140%] border border-gold-100/50 dark:border-gold-700/20 rounded-full" />
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Stats Section */}
      <section className="py-12 border-y border-gray-100 dark:border-gray-800 bg-white/50 dark:bg-gray-900/50">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            variants={staggerContainer}
            initial="initial"
            whileInView="animate"
            viewport={{ once: true }}
            className="grid grid-cols-2 md:grid-cols-4 gap-8"
          >
            {stats.map((stat, index) => (
              <motion.div
                key={stat.label}
                variants={fadeInUp}
                className="text-center"
              >
                <p className="text-3xl md:text-4xl font-bold text-gold-600 dark:text-gold-400 mb-1">
                  {stat.value}
                </p>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  {stat.label}
                </p>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Categories Section */}
      <section className="py-20 lg:py-28">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-12"
          >
            <h2 className="font-display text-3xl md:text-4xl font-bold text-primary-500 dark:text-white mb-4">
              Explora por Categorías
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              Encuentra exactamente lo que buscas en nuestras categorías cuidadosamente curadas.
            </p>
          </motion.div>

          <motion.div
            variants={staggerContainer}
            initial="initial"
            whileInView="animate"
            viewport={{ once: true }}
            className="grid grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6"
          >
            {categories.map((category) => (
              <motion.div
                key={category.id}
                variants={fadeInUp}
              >
                <Link
                  to={`/${category.id}`}
                  className={clsx(
                    'group relative block',
                    'rounded-2xl overflow-hidden',
                    'aspect-[4/3]',
                    'shadow-elegant hover:shadow-elegant-xl',
                    'transition-all duration-300'
                  )}
                >
                  <img
                    src={category.image}
                    alt={category.name}
                    className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                  />
                  <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/30 to-transparent" />
                  <div className="absolute bottom-0 left-0 right-0 p-5">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-2xl">{category.icon}</span>
                      <h3 className="text-lg font-semibold text-white">
                        {category.name}
                      </h3>
                    </div>
                    <p className="text-sm text-gray-300">
                      {category.count} anuncios
                    </p>
                  </div>
                  <div className={clsx(
                    'absolute top-4 right-4',
                    'w-10 h-10 rounded-full',
                    'bg-white/20 backdrop-blur-sm',
                    'flex items-center justify-center',
                    'opacity-0 group-hover:opacity-100',
                    'transform translate-x-4 group-hover:translate-x-0',
                    'transition-all duration-300'
                  )}>
                    <ArrowRight className="w-5 h-5 text-white" />
                  </div>
                </Link>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Featured Products Section */}
      <section className="py-20 lg:py-28 bg-gray-50 dark:bg-gray-900/50">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="flex flex-col sm:flex-row items-start sm:items-end justify-between mb-12"
          >
            <div>
              <h2 className="font-display text-3xl md:text-4xl font-bold text-primary-500 dark:text-white mb-4">
                Destacados para Ti
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-400 max-w-xl">
                Productos premium seleccionados por nuestro equipo de curadores.
              </p>
            </div>
            <Link
              to="/browse"
              className={clsx(
                'mt-4 sm:mt-0',
                'inline-flex items-center gap-2',
                'text-gold-600 dark:text-gold-400',
                'font-medium',
                'hover:gap-3 transition-all duration-200'
              )}
            >
              Ver todos
              <ChevronRight className="w-5 h-5" />
            </Link>
          </motion.div>

          <motion.div
            variants={staggerContainer}
            initial="initial"
            whileInView="animate"
            viewport={{ once: true }}
            className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6"
          >
            {featuredProducts.map((product) => (
              <motion.div key={product.id} variants={fadeInUp}>
                <OklaProductCard
                  image={product.image}
                  title={product.title}
                  price={product.price}
                  rating={product.rating}
                  reviews={product.reviews}
                  badge={product.badge}
                  badgeColor={product.badgeColor}
                  onClick={() => console.log('Product clicked:', product.id)}
                  onFavoriteClick={() => console.log('Favorite clicked:', product.id)}
                />
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="py-20 lg:py-28">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="font-display text-3xl md:text-4xl font-bold text-primary-500 dark:text-white mb-4">
              ¿Por qué elegir OKLA?
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              Nos diferenciamos por nuestra dedicación a la excelencia en cada detalle.
            </p>
          </motion.div>

          <motion.div
            variants={staggerContainer}
            initial="initial"
            whileInView="animate"
            viewport={{ once: true }}
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8"
          >
            {benefits.map((benefit, index) => (
              <motion.div
                key={benefit.title}
                variants={fadeInUp}
                className={clsx(
                  'text-center p-6',
                  'rounded-2xl',
                  'bg-white dark:bg-gray-900',
                  'border border-gray-100 dark:border-gray-800',
                  'hover:border-gold-200 dark:hover:border-gold-700/50',
                  'hover:shadow-gold',
                  'transition-all duration-300'
                )}
              >
                <div className={clsx(
                  'w-14 h-14 mx-auto mb-5',
                  'rounded-2xl',
                  'bg-gold-100 dark:bg-gold-900/30',
                  'flex items-center justify-center'
                )}>
                  <benefit.icon className="w-7 h-7 text-gold-600 dark:text-gold-400" />
                </div>
                <h3 className="text-lg font-semibold text-primary-500 dark:text-white mb-2">
                  {benefit.title}
                </h3>
                <p className="text-gray-600 dark:text-gray-400">
                  {benefit.description}
                </p>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Testimonials Section */}
      <section className="py-20 lg:py-28 bg-primary-500 dark:bg-gray-900">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="font-display text-3xl md:text-4xl font-bold text-white mb-4">
              Lo que dicen nuestros usuarios
            </h2>
            <p className="text-lg text-gray-300 max-w-2xl mx-auto">
              Miles de usuarios confían en OKLA para sus transacciones diarias.
            </p>
          </motion.div>

          <motion.div
            variants={staggerContainer}
            initial="initial"
            whileInView="animate"
            viewport={{ once: true }}
            className="grid grid-cols-1 md:grid-cols-3 gap-8"
          >
            {testimonials.map((testimonial) => (
              <motion.div
                key={testimonial.id}
                variants={fadeInUp}
                className={clsx(
                  'p-6 md:p-8',
                  'rounded-2xl',
                  'bg-white/5 backdrop-blur-sm',
                  'border border-white/10'
                )}
              >
                {/* Rating */}
                <div className="flex items-center gap-1 mb-4">
                  {[...Array(5)].map((_, i) => (
                    <Star
                      key={i}
                      className={clsx(
                        'w-5 h-5',
                        i < testimonial.rating ? 'text-gold-400 fill-gold-400' : 'text-gray-600'
                      )}
                    />
                  ))}
                </div>

                {/* Content */}
                <p className="text-white/90 mb-6 leading-relaxed">
                  "{testimonial.content}"
                </p>

                {/* Author */}
                <div className="flex items-center gap-3">
                  <img
                    src={testimonial.avatar}
                    alt={testimonial.name}
                    className="w-12 h-12 rounded-full object-cover"
                  />
                  <div>
                    <p className="font-semibold text-white">
                      {testimonial.name}
                    </p>
                    <p className="text-sm text-gray-400">
                      {testimonial.role}
                    </p>
                  </div>
                </div>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 lg:py-28">
        <div className="container mx-auto px-4 lg:px-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className={clsx(
              'relative overflow-hidden',
              'rounded-3xl',
              'bg-gradient-to-br from-gold-500 via-gold-400 to-gold-600',
              'px-8 py-16 md:px-16 md:py-20',
              'text-center'
            )}
          >
            {/* Decorative elements */}
            <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl" />
            <div className="absolute bottom-0 left-0 w-48 h-48 bg-white/10 rounded-full blur-2xl" />

            <div className="relative z-10">
              <h2 className="font-display text-3xl md:text-4xl lg:text-5xl font-bold text-primary-900 mb-4">
                Comienza a vender hoy
              </h2>
              <p className="text-lg md:text-xl text-primary-800 mb-8 max-w-2xl mx-auto">
                Únete a miles de vendedores exitosos y alcanza a millones de compradores potenciales.
              </p>
              <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                <OklaButton
                  variant="primary"
                  size="xl"
                  rightIcon={<ArrowRight className="w-5 h-5" />}
                >
                  Publicar Gratis
                </OklaButton>
                <OklaButton
                  variant="outline"
                  size="xl"
                  className="bg-white/20 border-white/30 text-primary-900 hover:bg-white/30"
                >
                  Ver Planes
                </OklaButton>
              </div>
            </div>
          </motion.div>
        </div>
      </section>
    </OklaLayout>
  );
};

export default OklaHomePage;
