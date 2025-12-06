import { motion } from 'framer-motion';
import { Search, ChevronDown, Star, Shield, Award } from 'lucide-react';
import { useState } from 'react';
import { FadeIn } from '../animations/FadeIn';
import { TextReveal } from '../animations/TextAnimations';
import { OklaButton } from '../../atoms/okla/OklaButton';

interface OklaHeroProps {
  title?: string;
  subtitle?: string;
  backgroundImage?: string;
  showSearch?: boolean;
  showTrustBadges?: boolean;
  onSearch?: (query: string, category: string) => void;
}

const categories = [
  { id: 'all', label: 'Todas las categorías' },
  { id: 'vehicles', label: 'Vehículos' },
  { id: 'properties', label: 'Propiedades' },
  { id: 'services', label: 'Servicios' },
];

export const OklaHero = ({
  title = 'Descubre lo Extraordinario',
  subtitle = 'El marketplace premium donde la calidad se encuentra con la confianza',
  backgroundImage = 'https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?q=80&w=2070',
  showSearch = true,
  showTrustBadges = true,
  onSearch,
}: OklaHeroProps) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('all');
  const [isCategoryOpen, setIsCategoryOpen] = useState(false);

  const handleSearch = () => {
    onSearch?.(searchQuery, selectedCategory);
  };

  return (
    <section className="relative min-h-[90vh] flex items-center justify-center overflow-hidden">
      {/* Background with Parallax Effect */}
      <motion.div
        className="absolute inset-0 w-full h-full"
        initial={{ scale: 1.1 }}
        animate={{ scale: 1 }}
        transition={{ duration: 1.5, ease: 'easeOut' }}
      >
        <div
          className="absolute inset-0 bg-cover bg-center bg-no-repeat"
          style={{ backgroundImage: `url(${backgroundImage})` }}
        />
        {/* Gradient Overlays */}
        <div className="absolute inset-0 bg-gradient-to-b from-okla-navy/80 via-okla-navy/60 to-okla-navy/90" />
        <div className="absolute inset-0 bg-gradient-to-r from-okla-navy/50 via-transparent to-okla-navy/50" />
      </motion.div>

      {/* Decorative Elements */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <motion.div
          className="absolute top-20 left-10 w-72 h-72 bg-okla-gold/10 rounded-full blur-3xl"
          animate={{
            scale: [1, 1.2, 1],
            opacity: [0.3, 0.5, 0.3],
          }}
          transition={{ duration: 8, repeat: Infinity }}
        />
        <motion.div
          className="absolute bottom-20 right-10 w-96 h-96 bg-okla-gold/10 rounded-full blur-3xl"
          animate={{
            scale: [1.2, 1, 1.2],
            opacity: [0.3, 0.5, 0.3],
          }}
          transition={{ duration: 10, repeat: Infinity }}
        />
      </div>

      {/* Content */}
      <div className="relative z-10 w-full max-w-6xl mx-auto px-6 text-center">
        {/* Pre-title */}
        <FadeIn delay={0.2}>
          <div className="inline-flex items-center gap-2 px-4 py-2 bg-okla-gold/10 backdrop-blur-sm rounded-full border border-okla-gold/20 mb-8">
            <Star className="w-4 h-4 text-okla-gold" fill="currentColor" />
            <span className="text-okla-gold text-sm font-medium tracking-wide">
              Marketplace Premium
            </span>
          </div>
        </FadeIn>

        {/* Main Title */}
        <FadeIn delay={0.4}>
          <h1 className="text-5xl md:text-7xl font-playfair font-bold text-white mb-6 leading-tight">
            <TextReveal delay={0.5}>{title}</TextReveal>
          </h1>
        </FadeIn>

        {/* Subtitle */}
        <FadeIn delay={0.6}>
          <p className="text-xl md:text-2xl text-okla-cream/80 max-w-2xl mx-auto mb-12 font-light">
            {subtitle}
          </p>
        </FadeIn>

        {/* Search Box */}
        {showSearch && (
          <FadeIn delay={0.8}>
            <div className="bg-white/10 backdrop-blur-md rounded-2xl p-2 max-w-3xl mx-auto border border-white/10">
              <div className="flex flex-col md:flex-row gap-2">
                {/* Category Selector */}
                <div className="relative">
                  <button
                    onClick={() => setIsCategoryOpen(!isCategoryOpen)}
                    className="w-full md:w-48 px-4 py-4 bg-white/10 rounded-xl text-white text-left flex items-center justify-between hover:bg-white/20 transition-colors"
                  >
                    <span>{categories.find((c) => c.id === selectedCategory)?.label}</span>
                    <ChevronDown
                      className={`w-5 h-5 transition-transform ${isCategoryOpen ? 'rotate-180' : ''}`}
                    />
                  </button>
                  {isCategoryOpen && (
                    <motion.div
                      initial={{ opacity: 0, y: -10 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-xl overflow-hidden z-20"
                    >
                      {categories.map((category) => (
                        <button
                          key={category.id}
                          onClick={() => {
                            setSelectedCategory(category.id);
                            setIsCategoryOpen(false);
                          }}
                          className={`w-full px-4 py-3 text-left hover:bg-okla-cream transition-colors ${
                            selectedCategory === category.id
                              ? 'bg-okla-gold/10 text-okla-navy font-medium'
                              : 'text-okla-charcoal'
                          }`}
                        >
                          {category.label}
                        </button>
                      ))}
                    </motion.div>
                  )}
                </div>

                {/* Search Input */}
                <div className="flex-1 relative">
                  <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-white/50" />
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="¿Qué estás buscando?"
                    className="w-full pl-12 pr-4 py-4 bg-white/10 rounded-xl text-white placeholder-white/50 focus:outline-none focus:ring-2 focus:ring-okla-gold/50"
                    onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  />
                </div>

                {/* Search Button */}
                <OklaButton
                  variant="primary"
                  size="lg"
                  onClick={handleSearch}
                  className="px-8"
                >
                  Buscar
                </OklaButton>
              </div>
            </div>
          </FadeIn>
        )}

        {/* Trust Badges */}
        {showTrustBadges && (
          <FadeIn delay={1}>
            <div className="flex flex-wrap justify-center gap-8 mt-12">
              <div className="flex items-center gap-2 text-okla-cream/70">
                <Shield className="w-5 h-5 text-okla-gold" />
                <span className="text-sm">Transacciones Seguras</span>
              </div>
              <div className="flex items-center gap-2 text-okla-cream/70">
                <Award className="w-5 h-5 text-okla-gold" />
                <span className="text-sm">Vendedores Verificados</span>
              </div>
              <div className="flex items-center gap-2 text-okla-cream/70">
                <Star className="w-5 h-5 text-okla-gold" fill="currentColor" />
                <span className="text-sm">+10,000 Clientes Satisfechos</span>
              </div>
            </div>
          </FadeIn>
        )}
      </div>

      {/* Scroll Indicator */}
      <motion.div
        className="absolute bottom-8 left-1/2 -translate-x-1/2"
        animate={{ y: [0, 10, 0] }}
        transition={{ duration: 2, repeat: Infinity }}
      >
        <div className="w-6 h-10 rounded-full border-2 border-white/30 flex items-start justify-center p-2">
          <motion.div
            className="w-1 h-2 bg-okla-gold rounded-full"
            animate={{ y: [0, 12, 0] }}
            transition={{ duration: 2, repeat: Infinity }}
          />
        </div>
      </motion.div>
    </section>
  );
};

export default OklaHero;
