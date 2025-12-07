import { motion } from 'framer-motion';
import type { ReactNode } from 'react';
import { ArrowRight } from 'lucide-react';
import { FadeIn } from '../animations/FadeIn';
import { StaggerContainer, StaggerItem } from '../animations/StaggerContainer';
import { HoverLift } from '../animations/ScaleEffects';

interface Category {
  id: string;
  name: string;
  icon: ReactNode;
  count: number;
  image?: string;
  color?: string;
}

interface OklaCategoriesProps {
  title?: string;
  subtitle?: string;
  categories: Category[];
  onCategoryClick?: (id: string) => void;
  variant?: 'cards' | 'icons' | 'tiles';
}

export const OklaCategories = ({
  title = 'Explora por Categoría',
  subtitle = 'Encuentra exactamente lo que buscas',
  categories,
  onCategoryClick,
  variant = 'cards',
}: OklaCategoriesProps) => {
  if (variant === 'icons') {
    return (
      <section className="py-16 px-6 bg-white">
        <div className="max-w-7xl mx-auto">
          <FadeIn>
            <div className="text-center mb-12">
              <h2 className="text-3xl md:text-4xl font-playfair font-bold text-okla-navy">
                {title}
              </h2>
              <p className="text-okla-slate mt-3">{subtitle}</p>
            </div>
          </FadeIn>

          <StaggerContainer className="flex flex-wrap justify-center gap-8">
            {categories.map((category) => (
              <StaggerItem key={category.id}>
                <motion.button
                  onClick={() => onCategoryClick?.(category.id)}
                  className="group flex flex-col items-center"
                  whileHover={{ y: -5 }}
                  whileTap={{ scale: 0.95 }}
                >
                  <div className="w-20 h-20 rounded-2xl bg-okla-cream flex items-center justify-center text-okla-navy group-hover:bg-okla-gold group-hover:text-white transition-all duration-300 shadow-sm group-hover:shadow-lg">
                    {category.icon}
                  </div>
                  <span className="mt-3 font-medium text-okla-navy">{category.name}</span>
                  <span className="text-sm text-okla-slate">{category.count.toLocaleString()}</span>
                </motion.button>
              </StaggerItem>
            ))}
          </StaggerContainer>
        </div>
      </section>
    );
  }

  if (variant === 'tiles') {
    return (
      <section className="py-12 px-6 bg-okla-cream">
        <div className="max-w-7xl mx-auto">
          <FadeIn>
            <div className="text-center mb-12">
              <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
                Categorías
              </span>
              <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
                {title}
              </h2>
              <p className="text-okla-slate mt-4 max-w-xl mx-auto">{subtitle}</p>
            </div>
          </FadeIn>

          <StaggerContainer className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {categories.map((category) => (
              <StaggerItem key={category.id}>
                <HoverLift>
                  <motion.button
                    onClick={() => onCategoryClick?.(category.id)}
                    className="relative w-full aspect-square rounded-2xl overflow-hidden group"
                    whileTap={{ scale: 0.98 }}
                  >
                    {category.image ? (
                      <motion.img
                        src={category.image}
                        alt={category.name}
                        className="absolute inset-0 w-full h-full object-cover"
                        whileHover={{ scale: 1.1 }}
                        transition={{ duration: 0.6 }}
                      />
                    ) : (
                      <div
                        className="absolute inset-0"
                        style={{ backgroundColor: category.color || '#0A192F' }}
                      />
                    )}
                    <div className="absolute inset-0 bg-gradient-to-t from-okla-navy/90 via-okla-navy/40 to-transparent" />
                    <div className="absolute inset-0 flex flex-col items-center justify-center text-white p-4">
                      <div className="w-14 h-14 rounded-xl bg-white/10 backdrop-blur-sm flex items-center justify-center mb-3 group-hover:bg-okla-gold transition-colors">
                        {category.icon}
                      </div>
                      <h3 className="text-lg font-semibold">{category.name}</h3>
                      <span className="text-sm text-okla-cream/70">
                        {category.count.toLocaleString()} listings
                      </span>
                    </div>
                  </motion.button>
                </HoverLift>
              </StaggerItem>
            ))}
          </StaggerContainer>
        </div>
      </section>
    );
  }

  // Default: cards variant
  return (
    <section className="py-12 px-6 bg-white">
      <div className="max-w-7xl mx-auto">
        <FadeIn>
          <div className="text-center mb-12">
            <span className="text-okla-gold font-medium tracking-wider text-sm uppercase">
              Categorías
            </span>
            <h2 className="text-4xl md:text-5xl font-playfair font-bold text-okla-navy mt-2">
              {title}
            </h2>
            <p className="text-okla-slate mt-4 max-w-xl mx-auto">{subtitle}</p>
          </div>
        </FadeIn>

        <StaggerContainer className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {categories.map((category) => (
            <StaggerItem key={category.id}>
              <HoverLift>
                <motion.button
                  onClick={() => onCategoryClick?.(category.id)}
                  className="w-full bg-okla-cream rounded-2xl p-6 text-left group flex items-center gap-6 hover:bg-okla-navy transition-colors duration-300"
                  whileTap={{ scale: 0.98 }}
                >
                  <div className="w-16 h-16 rounded-xl bg-white flex items-center justify-center text-okla-navy group-hover:bg-okla-gold group-hover:text-white transition-colors shadow-sm flex-shrink-0">
                    {category.icon}
                  </div>
                  <div className="flex-1 min-w-0">
                    <h3 className="text-xl font-semibold text-okla-navy group-hover:text-white transition-colors">
                      {category.name}
                    </h3>
                    <p className="text-okla-slate group-hover:text-okla-cream/70 transition-colors mt-1">
                      {category.count.toLocaleString()} disponibles
                    </p>
                  </div>
                  <ArrowRight className="w-5 h-5 text-okla-slate group-hover:text-okla-gold transition-colors flex-shrink-0" />
                </motion.button>
              </HoverLift>
            </StaggerItem>
          ))}
        </StaggerContainer>
      </div>
    </section>
  );
};

export default OklaCategories;
